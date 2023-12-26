using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.InputControllers;
using BattleCity.Logging;
using BattleCity.Video;
using BattleCity.VisualComponents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace BattleCity
{
    /// <summary>
    /// Форма отображения игры
    /// </summary>
    public partial class GameForm : Form, IGameApplication
    {
        #region members

        // настройки приложения
        readonly AppSettings appSettings;

        // игровые рекорды и достижения
        readonly GameAchievements gameRecord = GameAchievements.Load();

        // сервсис логирования
        readonly ILogger logger;

        // счётчик измерения времени между отрисовкой кадров
        readonly Stopwatch frameTime = new Stopwatch();

        // объект синхронизации
        readonly object SyncObject = new object();

        // хаб игровых устройств
        IControllerHub controllerHub;

        // контекст графического устройства вывода графики
        D3D9DeviceContext deviceContext;

        // редактор уровней
        LevelEditor levelEditor;

        // игровой контент
        GameContent content;

        // титульный экран
        MainScreen mainScreen;

        // экран подведения итогов уровня (stage)
        StageResultScreen stageResultScreen;

        // игровая графика, методы отрисовки
        IGameGraphics graphics;

        // экран перехода
        ScreenTransition screenTransition;

        // экран выбора уровня (stage)
        StageSelectorScreenTransition stageStartScreen;

        // экран достижений
        HiScoreScreen hiScoreScreen;

        // экран конца игры
        GameOverScreen gameOverScreen;

        // экран настроек
        SettingsScreen settingsScreen;

        // игра. логика, механика
        BattleGround game;

        // звуковой движок
        ISoundEngine soundEngine;

        // эффект пост обработки
        IPostProcessEffect postProcessEffect;

        // текущий экран
        GameScreenEnum screen = GameScreenEnum.Title;

        // размер клиентской области
        int clientSizeWidth, clientSizeHeight;

        // признак завершения работы приложения
        bool mTerminate;

        // признак того, требуется закрыть форму (приложение)
        bool closeFormRequired;

        // признак того, требуется сбросить устройство вывода графики
        bool deviceResetRequired;

        // ограничение количества кадров в секунду: 60
        const int FpsLimit = 60;

        // размер клиентской области до пользовательского измение размера формы
        Size clientSizeBeforeResize;

        // текущая контент директория
        string currentContentDirectory = GameContent.DefaultContentDirectory;

        // признак перезагрузки игры (при смене контент директории)
        bool reloadGame;

        #endregion


        #region Properties

        /// <inheritdoc/>
        public bool IsActive { get; private set; }

        /// <inheritdoc/>
        public bool ContinuousFire { get => appSettings.ContinuousFire; set => appSettings.ContinuousFire = value; }

        /// <inheritdoc/>
        public bool IsFullScreen { get; private set; }

        /// <inheritdoc/>
        public bool SaveAspectRatio { get; private set; }

        /// <inheritdoc/>
        public int ScanlinesFxLevel { get => appSettings.ScanlinesFxLevel; set => appSettings.ScanlinesFxLevel = value; }

        /// <inheritdoc/>
        public int ClientSizeWidth => clientSizeWidth;

        /// <inheritdoc/>
        public int ClientSizeHeight => clientSizeHeight;

        #endregion


        #region Constructor

        /// <summary>
        /// Конструктор формы
        /// </summary>
        public GameForm(ILogger logger = null, bool resetContent = false)
        {
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            this.logger = logger;
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            if (resetContent)
            {
                AppSettings.Delete();
                GameContentGenerator.CreateDefaultContent(logger: logger);
            }

            appSettings = AppSettings.Load();
        }

        #endregion


        #region methods

        /// <inheritdoc/>
        public void SwitchFullScreenMode()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SwitchFullScreenMode));
                return;
            }

            if (IsFullScreen)
            {
                IsFullScreen = false;
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                IsFullScreen = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
        }

        /// <inheritdoc/>
        public void SwitchAspectRatio()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SwitchAspectRatio));
                return;
            }

            int width = content.CommonConfig.WindowWidth;
            int height = content.CommonConfig.WindowHeight;

            if (SaveAspectRatio)
            {
                SaveAspectRatio = false;
            }
            else
            {
                SaveAspectRatio = true;
                var screen = Screen.FromHandle(Handle);
                var aspectRatio = screen.Bounds.Width / (double)screen.Bounds.Height;
                width = Convert.ToInt32(height * aspectRatio);
            }

            ClientSize = new Size(width, height);
            deviceResetRequired = true;
        }

        /// <inheritdoc/>
        public void SetContentDirectory(string contentDirectoryName)
        {
            reloadGame = true;
            currentContentDirectory = contentDirectoryName;

            if (string.IsNullOrWhiteSpace(contentDirectoryName))
            {
                currentContentDirectory = GameContent.DefaultContentDirectory;
            }
            else if (string.Compare(contentDirectoryName, GameContent.DefaultContentDirectory, true) != 0)
            {
                bool directoryExists = false;
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(contentDirectoryName);
                    directoryExists = directory.Exists;
                }
                catch
                {
                    logger?.WriteLine($"{nameof(SetContentDirectory)} wrong directory \"{contentDirectoryName}\".", LogLevel.Warning);
                    logger?.WriteLine($"{nameof(SetContentDirectory)} wrong directory \"{contentDirectoryName}\". Default content directory will be used", LogLevel.Info);
                }

                if (!directoryExists)
                {
                    currentContentDirectory = GameContent.DefaultContentDirectory;
                }
            }
        }

        private void InitializeGame()
        {
            UnloadGame();

            content = new GameContent(currentContentDirectory, logger);

            SetOptimizedWindowSize();
            CreateDeviceContext();

            content.Load(deviceContext.Device);

            if (appSettings.FullScreen)
                SwitchFullScreenMode();

            if (gameRecord.HiScoreValue == null)
                gameRecord.HiScoreValue = gameRecord.GetHiScoreValue(content.GameConfig.HiScoreValue);

            postProcessEffect = new ScanlinesPostProcessEffect(this, deviceContext, appSettings);

            soundEngine = new SoundEngine(logger, content.Sounds, content.CommonConfig.MaxAudioSlots, content.CommonConfig.SoundEngineLatency)
            {
                SfxLevel = appSettings.SoundLevel,
                MusicLevel = appSettings.MusicLevel
            };

            CreateGameGraphics();
            CreateScreenTransition();
            CreateStageStartScreen();
            CreateTitleScreen();
            CreateStageResultScreen();
            CreateHiScoreScreen();
            CreateGameOverScreen();
            CreateSettingsScreen();

            frameTime.Restart();
            screen = GameScreenEnum.Title;

            // Запуск потока отрисовки игры
            new Thread(RenderLoop).Start();
        }

        private void UnloadGame()
        {
            postProcessEffect?.Dispose();
            game?.Dispose();
            soundEngine?.Dispose();
            content?.Dispose();
            DisposeLevelEditor();
            DisposeHiScoreScreen();
            DisposeScreenTransition();
            DisposeGameOverScreen();
            DisposeSettingsScreen();
            DisposeStageResultScreen();
            DisposeStageStartScreen();
            DisposeTitleScreen();
            DisposeGameGrphics();
            DisposeDeviceContext();
        }

        /// <summary>
        /// Инициализация контроллеров управления
        /// </summary>
        private void InitializeControllers()
        {
            controllerHub = new ControllerHub(this);

            controllerHub.AddButtonsMap(new ButtonsMap(1, InputDeviceType.Keyboard, new List<ControllerButton>()
            {
                new ControllerButton(ButtonNames.Up, KeyboardKey.W),
                new ControllerButton(ButtonNames.Left, KeyboardKey.A),
                new ControllerButton(ButtonNames.Down, KeyboardKey.S),
                new ControllerButton(ButtonNames.Right, KeyboardKey.D),
                new ControllerButton(ButtonNames.Pause, KeyboardKey.Escape),
                new ControllerButton(ButtonNames.Cancel, KeyboardKey.Escape),
                new ControllerButton(ButtonNames.Start, KeyboardKey.Return),
                new ControllerButton(ButtonNames.Exit, KeyboardKey.F12),
                new ControllerButton(ButtonNames.Attack, KeyboardKey.Space),
                new ControllerButton(ButtonNames.LoadPrevLevel, KeyboardKey.F1),
                new ControllerButton(ButtonNames.LoadNextLevel, KeyboardKey.F2),
                new ControllerButton(ButtonNames.PrevObjectType, KeyboardKey.F6),
                new ControllerButton(ButtonNames.NextObjectType, KeyboardKey.F7),
                new ControllerButton(ButtonNames.CreateObject, KeyboardKey.Space),
                new ControllerButton(ButtonNames.DeleteObject, KeyboardKey.Delete),
                new ControllerButton(ButtonNames.SaveLevel, KeyboardKey.F4),
                new ControllerButton(ButtonNames.PickObject, KeyboardKey.LeftControl)
            }));

            controllerHub.AddButtonsMap(new ButtonsMap(2, InputDeviceType.Keyboard, new List<ControllerButton>()
            {
                new ControllerButton(ButtonNames.Up, KeyboardKey.UpArrow),
                new ControllerButton(ButtonNames.Left, KeyboardKey.LeftArrow),
                new ControllerButton(ButtonNames.Down, KeyboardKey.DownArrow),
                new ControllerButton(ButtonNames.Right, KeyboardKey.RightArrow),
                new ControllerButton(ButtonNames.Pause, KeyboardKey.Escape),
                new ControllerButton(ButtonNames.Cancel, KeyboardKey.Backspace),
                new ControllerButton(ButtonNames.Start, KeyboardKey.PageDown),
                new ControllerButton(ButtonNames.Attack, KeyboardKey.RightControl)
            }));

            #region DirectInput

            //gamepadService.Update();

            //if (gamepadService.Gamepads.Count > 0)
            //{
            //    var gamepad = gamepadService.Gamepads.OrderByDescending(x => x.Name).First();
            //    controllerHub.AddController(new DirectInputController(gamepad, Handle));

            //    controllerHub.AddButtonsMap(new ButtonsMap(1, InputDeviceType.Joystick, new List<ControllerButton>()
            //    {
            //        new ControllerButton(ButtonNames.Up, XInputKeys.Up),
            //        new ControllerButton(ButtonNames.Left, XInputKeys.Left),
            //        new ControllerButton(ButtonNames.Down, XInputKeys.Down),
            //        new ControllerButton(ButtonNames.Right, XInputKeys.Right),
            //        new ControllerButton(ButtonNames.Pause, XInputKeys.Start),
            //        new ControllerButton(ButtonNames.Confirm, XInputKeys.Start),
            //        new ControllerButton(ButtonNames.Attack, XInputKeys.X)
            //    }));
            //}

            #endregion

            #region XInput

            controllerHub.AddController(new XInputController(1, this));
            controllerHub.AddController(new XInputController(2, this));

            controllerHub.AddButtonsMap(new ButtonsMap(1, InputDeviceType.Joystick, new List<ControllerButton>()
            {
                new ControllerButton(ButtonNames.Up, XInputKeys.Up),
                new ControllerButton(ButtonNames.Left, XInputKeys.Left),
                new ControllerButton(ButtonNames.Down, XInputKeys.Down),
                new ControllerButton(ButtonNames.Right, XInputKeys.Right),
                new ControllerButton(ButtonNames.Pause, XInputKeys.Start),
                new ControllerButton(ButtonNames.Start, XInputKeys.Start),
                new ControllerButton(ButtonNames.Cancel, XInputKeys.Y),
                new ControllerButton(ButtonNames.Exit, XInputKeys.Back),
                new ControllerButton(ButtonNames.PickObject, XInputKeys.X),
                new ControllerButton(ButtonNames.CreateObject, XInputKeys.A),
                new ControllerButton(ButtonNames.LoadNextLevel, XInputKeys.R1),
                new ControllerButton(ButtonNames.LoadPrevLevel, XInputKeys.L1),
                new ControllerButton(ButtonNames.PrevObjectType, XInputKeys.L2),
                new ControllerButton(ButtonNames.NextObjectType, XInputKeys.R2),
                new ControllerButton(ButtonNames.Attack, XInputKeys.A, XInputKeys.B)
            }, "1"));

            controllerHub.AddButtonsMap(new ButtonsMap(2, InputDeviceType.Joystick, new List<ControllerButton>()
            {
                new ControllerButton(ButtonNames.Up, XInputKeys.Up),
                new ControllerButton(ButtonNames.Left, XInputKeys.Left),
                new ControllerButton(ButtonNames.Down, XInputKeys.Down),
                new ControllerButton(ButtonNames.Right, XInputKeys.Right),
                new ControllerButton(ButtonNames.Pause, XInputKeys.Start),
                new ControllerButton(ButtonNames.Start, XInputKeys.Start),
                new ControllerButton(ButtonNames.Cancel, XInputKeys.Y),
                new ControllerButton(ButtonNames.Exit, XInputKeys.Back),
                new ControllerButton(ButtonNames.PickObject, XInputKeys.X),
                new ControllerButton(ButtonNames.CreateObject, XInputKeys.A),
                new ControllerButton(ButtonNames.Attack, XInputKeys.A, XInputKeys.B)
            }, "2"));

            #endregion
        }

        /// <summary>
        /// Установка оптимального размера формы
        /// </summary>
        private void SetOptimizedWindowSize()
        {
            int width = content.CommonConfig.WindowWidth;
            int height = content.CommonConfig.WindowHeight;
            var screen = Screen.FromHandle(Handle);

            if (appSettings.SaveAspectRatio)
            {
                SaveAspectRatio = true;

                var aspectRatio = screen.Bounds.Width / (double)screen.Bounds.Height;
                width = Convert.ToInt32(height * aspectRatio);
                Location = new Point(
                    screen.WorkingArea.X + (screen.WorkingArea.Width - width) / 2,
                    screen.WorkingArea.Y + (screen.WorkingArea.Height - height) / 2);
            }
            else
            {
                SaveAspectRatio = false;
            }
            ClientSize = new Size(width, height);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F10)
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeControllers();
            InitializeGame();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            clientSizeWidth = ClientSize.Width;
            clientSizeHeight = ClientSize.Height;
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            clientSizeBeforeResize = ClientSize;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            if (clientSizeBeforeResize != ClientSize && appSettings.SaveAspectRatio)
            {
                var screen = Screen.FromHandle(Handle);

                int width = ClientSize.Width;
                int height = ClientSize.Height;

                if (ClientSize.Width == clientSizeBeforeResize.Width)
                {
                    var aspectRatio = screen.Bounds.Width / (double)screen.Bounds.Height;
                    width = Convert.ToInt32(height * aspectRatio);
                }
                else if (ClientSize.Height == clientSizeBeforeResize.Height)
                {
                    var aspectRatio = (double)screen.Bounds.Height / screen.Bounds.Width;
                    height = Convert.ToInt32(width * aspectRatio);
                }
                else
                {
                    var aspectRatio = screen.Bounds.Width / (double)screen.Bounds.Height;
                    width = Convert.ToInt32(height * aspectRatio);
                }

                //Location = new Point(
                //    screen.WorkingArea.X + (screen.WorkingArea.Width - width) / 2,
                //    screen.WorkingArea.Y + (screen.WorkingArea.Height - height) / 2);
                ClientSize = new Size(width, height);
            }

            clientSizeBeforeResize = ClientSize;
        }

        private void SettingsScreen_Exit()
        {
            appSettings.SoundLevel = soundEngine.SfxLevel;
            appSettings.MusicLevel = soundEngine.MusicLevel;
            appSettings.SaveAspectRatio = SaveAspectRatio;
            appSettings.FullScreen = IsFullScreen;
            appSettings.Save();
            ShowTitleScreen();
        }

        private void HiScoreScreen_Exited()
        {
            ShowTitleScreen();
        }

        private void GameOverScreen_Exited()
        {
            int hiScoreValue = gameRecord.HiScoreValue ?? content.GameConfig.HiScoreValue;
            int currentHiScore = gameRecord.GetHiScoreValue(hiScoreValue);
            bool isNewRecord = currentHiScore > hiScoreValue;
            gameRecord.HiScoreValue = currentHiScore;
            gameRecord.Save();

            if (content.GameConfig.ShowHiScoreScreen && isNewRecord)
            {
                screen = GameScreenEnum.HiScores;
                hiScoreScreen.Reset();
            }
            else
            {
                HiScoreScreen_Exited();
            }
        }

        private void StageResultScreen_Exited(StageResult result)
        {
            gameRecord.Save();

            if (result.IsGameOver)
            {
                if (content.GameConfig.ShowGameOverScreen)
                {
                    screen = GameScreenEnum.GameOver;
                    gameOverScreen.Show();
                }
                else
                {
                    GameOverScreen_Exited();
                }
            }
            else
            {
                StartNextStage(result.StageNumber);
            }
        }

        /// <summary>
        /// Начать следующий уровень (stage)
        /// </summary>
        /// <param name="stageNumber"></param>
        private void StartNextStage(int stageNumber)
        {
            game.Reset(false);
            screen = GameScreenEnum.StartLevel;
            stageStartScreen.Reset();
            stageStartScreen.CurrentScreen = GameScreenEnum.PlayGame;
            stageStartScreen.NextScreen = GameScreenEnum.PlayGame;
            stageStartScreen.StartNextStage(stageNumber + 1);
        }

        /// <summary>
        /// Показать титульный экран
        /// </summary>
        private void ShowTitleScreen()
        {
            mainScreen.Reset();

            if (game != null)
            {
                game.EndGame -= Game_EndGame;
                game.Dispose();
                game = null;
            }

            screen = GameScreenEnum.Title;
            stageStartScreen.Reset();
            stageStartScreen.CurrentScreen = GameScreenEnum.None;
            stageStartScreen.NextScreen = GameScreenEnum.None;
            screenTransition.Reset();
            screenTransition.CurrentScreen = GameScreenEnum.None;
            screenTransition.NextScreen = GameScreenEnum.None;
        }

        /// <summary>
        /// Метод обработки закрытия игровой формы
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            mTerminate = true;
        }

        /// <summary>
        /// Метод обратки после выбора уровня (stage)
        /// </summary>
        /// <param name="stage"></param>
        private void StageStartScreen_StageSelected(int stage)
        {
            game.InitStage(stage);

            if (stageStartScreen.NextScreen == GameScreenEnum.PlayGame)
            {
                game.Start();
            }
        }

        /// <summary>
        /// Метод обратки после выхода из редактора уровней
        /// </summary>
        private void Editor_Exit()
        {
            screen = GameScreenEnum.Title;
            stageStartScreen.Reset();
            stageStartScreen.NextScreen = GameScreenEnum.None;
            DisposeLevelEditor();
        }

        private void StageStartScreen_Opened()
        {
            screen = stageStartScreen.NextScreen;
        }

        private void ScreenSwitcher_Opened()
        {
            screen = screenTransition.NextScreen;
        }

        /// <summary>
        /// Метод обратки нажатия кнопок мыши
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            switch (e.Button)
            {
                case MouseButtons.Middle:
                    SwitchFullScreenMode();
                    break;
            }
        }

        /// <summary>
        /// Инициализировать игру
        /// </summary>
        /// <param name="numPlayers"></param>
        private void InitGame(int numPlayers)
        {
            game = new BattleGround(
                this,
                logger,
                soundEngine,
                deviceContext,
                controllerHub,
                graphics,
                content,
                numPlayers);

            game.Reset(true);
            game.Exit += ShowTitleScreen;
            game.EndGame += Game_EndGame;
        }

        private void Game_EndGame(StageResult result)
        {
            screen = GameScreenEnum.StageResult;
            stageResultScreen.SetResult(result);
            stageResultScreen.Reset();
        }

        /// <summary>
        /// Метод обработки выбора опции главного экрана
        /// </summary>
        /// <param name="option"></param>
        private void MainScreen_OptionSelected(MainMenuOption option)
        {
            switch (option.NextScreen)
            {
                case GameScreenEnum.StartSinglePlayer:
                case GameScreenEnum.StartMultiplayer:

                    if (stageStartScreen.NextScreen == GameScreenEnum.PlayGame)
                        return;

                    if (game == null)
                    {
                        // если запуск новой игры
                        int numPlayers = option.NextScreen == GameScreenEnum.StartSinglePlayer ? 1 : 2;
                        InitGame(numPlayers);
                    }
                    else
                    {
                        // сбрасываем состояние игры
                        game.Reset(true);
                    }

                    stageStartScreen.CurrentScreen = screen;
                    stageStartScreen.NextScreen = GameScreenEnum.PlayGame;
                    stageStartScreen.Reset();
                    screen = GameScreenEnum.StartLevel;
                    return;
                case GameScreenEnum.Settings:
                    screenTransition.CurrentScreen = screen;
                    screenTransition.NextScreen = GameScreenEnum.Settings;
                    screen = GameScreenEnum.Settings;
                    settingsScreen.Reset();
                    return;
                case GameScreenEnum.LevelEditor:
                    CreateLevelEditor();
                    screenTransition.CurrentScreen = screen;
                    screenTransition.NextScreen = GameScreenEnum.LevelEditor;
                    levelEditor.Initialize();
                    screenTransition.Reset();
                    screen = GameScreenEnum.ScreenTransition;
                    return;
                case GameScreenEnum.ExitGame:
                    mTerminate = true;
                    Application.Exit();
                    return;
            }
        }



        /// <summary>
        /// Создать конткест графического устройства
        /// </summary>
        private void CreateDeviceContext()
        {
            deviceContext = D3D9DeviceContext.Create(Handle, ClientSize.Width, ClientSize.Height);
        }

        private void DisposeDeviceContext()
        {
            if (deviceContext != null)
            {
                deviceContext.Dispose();
                deviceContext = null;
            }
        }

        /// <summary>
        /// Метод обработки потери фокуса формы (форма стала неактивна)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            IsActive = false;
        }

        /// <summary>
        /// Метод обработки получения фокуса формы (форма стала активной)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            IsActive = true;
        }

        /// <summary>
        /// Выполнить безопасный вызов метода относительно STA потока
        /// </summary>
        /// <param name="action"></param>
        private void SafeInvoke(Action action)
        {
            try
            {
                if (InvokeRequired)
                    //new Action(() => Invoke(action)).BeginInvoke(null, null);
                    Invoke(action);
                else
                    action();
            }
            catch (ObjectDisposedException) { /*форма закрылась*/ }
        }

        /// <summary>
        /// Метод обработки потери графического устройста
        /// </summary>
        private void OnLostDevice()
        {
            // логируем событие потери графического устройства
            logger?.WriteLine(nameof(OnLostDevice), LogLevel.Info);
            // сообщаем графическому контексту о потери устройства
            deviceContext.OnDeviceLost();
        }

        /// <summary>
        /// Метод обработки восстановления графического устройста
        /// </summary>
        private void OnRestoreDevice()
        {
            // логируем событие восстановления графического устройства
            logger?.WriteLine(nameof(OnRestoreDevice), LogLevel.Info);
            // сообщаем графическому контексту о восстановлении устройства
            deviceContext.OnDeviceRestore();
            // сообщаем графическому контексту о изменении параметров ширины / высода графического устройста
            deviceContext.OnDeviceResize();
            // перезапускаем счётчик времени кадра
            frameTime.Restart();
        }

        /// <summary>
        /// Сброс графического устройста
        /// </summary>
        private void ResetDevice()
        {
            // Проверка условий, при котором возможно выполнить сброс графического устройста
            if (IsDisposed || WindowState == FormWindowState.Minimized)
                return;

            Monitor.Enter(SyncObject);
            try
            {
                OnLostDevice();
                try
                {
                    deviceContext.Reset(ClientSize.Width, ClientSize.Height);
                }
                catch (Exception ex)
                {
                    // логируем ошибку сброса графического устройста
                    logger?.WriteLine($"D3D Reset - FAILED:{Environment.NewLine}{ex}", LogLevel.Error);
                    mTerminate = true;
                    Close();
                    return;
                }

                //device.Viewport = new Viewport(0, 0, currentRenderFormSize.Width, currentRenderFormSize.Height);
                OnRestoreDevice();
            }
            finally
            {
                Monitor.Exit(SyncObject);
            }
        }

        /// <summary>
        /// Цикл отрисовки игры
        /// </summary>
        private void RenderLoop()
        {
            // выполнения цикла пока нет признака заврешения работы приложения
            while (!mTerminate && !reloadGame)
            {
                #region FpsLimit: контролируем количество кадров в секунду

                double FrameTimeLimit = 1000d / FpsLimit;
                double frameTimeMsec = frameTime.Elapsed.TotalMilliseconds;
                var delta = (FrameTimeLimit - frameTimeMsec);
                if (delta > 0)
                {
                    Thread.Sleep(0);
                    continue;
                }

                #endregion

                frameTime.Restart();

                if (deviceResetRequired)
                {
                    SafeInvoke(ResetDevice);
                    deviceResetRequired = false;
                    continue;
                }

                if (deviceContext.IsLost())
                {
                    //Log?.WriteLine($"Device is lost. Device Reset scheduled.", LogLevel.Info);
                    deviceResetRequired = true;
                    continue;
                }

                var result = deviceContext.Device.TestCooperativeLevel();
                if (result.IsFailure)
                {
                    // логируем ошибку
                    logger?.WriteLine($"Device no ready.", LogLevel.Error);
                    mTerminate = true;
                    break;
                }

                //gamepadService.Update();
                controllerHub.Update();

                deviceContext.Clear(true, true, 0);

                if (deviceContext.Device.BeginScene().IsSuccess)
                {
                    switch (screen)
                    {
                        case GameScreenEnum.ScreenTransition:
                            {
                                if (screenTransition.State == GameScreenShowState.Closing)
                                    DrawScreen(screenTransition.CurrentScreen, false);
                                else if (screenTransition.State == GameScreenShowState.Opening)
                                    DrawScreen(screenTransition.NextScreen, false);
                                DrawScreen(screen, false);
                                break;
                            }
                        case GameScreenEnum.StartLevel:
                            {
                                if (stageStartScreen.State == GameScreenShowState.Closing)
                                    DrawScreen(stageStartScreen.CurrentScreen, false);
                                else if (stageStartScreen.State == GameScreenShowState.Opening)
                                    DrawScreen(stageStartScreen.NextScreen, false);
                                DrawScreen(screen, false);
                                break;
                            }
                        default:
                            {
                                DrawScreen(screen, true);
                                break;
                            }
                    }

                    postProcessEffect?.Draw();

                    deviceContext.Device.EndScene();
                }

                try
                {
                    deviceContext.Device.Present();
                }
                catch { }
            }

            if (mTerminate)
            {
                CleanUp();
            }
            else if (reloadGame)
            {
                reloadGame = false;
                Invoke(new Action(InitializeGame));
            }
        }

        private void CreateGameGraphics()
        {
            graphics = new GameGraphics(deviceContext, content.Textures);
        }

        private void DisposeGameGrphics()
        {
            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }
        }

        private void CreateLevelEditor()
        {
            levelEditor = new LevelEditor(logger, deviceContext, content, graphics, controllerHub);
            levelEditor.Exit += Editor_Exit;
        }

        private void DisposeLevelEditor()
        {
            if (levelEditor != null)
            {
                levelEditor.Exit -= Editor_Exit;
                levelEditor.Dispose();
                levelEditor = null;
            }
        }

        private void CreateTitleScreen()
        {
            mainScreen = new MainScreen(deviceContext, controllerHub, content, graphics, gameRecord);
            mainScreen.OptionSelected += MainScreen_OptionSelected;
        }

        private void DisposeTitleScreen()
        {
            if (mainScreen != null)
            {
                mainScreen.OptionSelected -= MainScreen_OptionSelected;
                mainScreen.Dispose();
            }
        }

        private void CreateScreenTransition()
        {
            screenTransition = new ScreenTransition(graphics, deviceContext, content.GameConfig);
            screenTransition.Opened += ScreenSwitcher_Opened;
        }

        private void DisposeScreenTransition()
        {
            if (screenTransition != null)
            {
                screenTransition.Opened -= ScreenSwitcher_Opened;
                screenTransition.Dispose();
            }
        }

        private void CreateStageStartScreen()
        {
            stageStartScreen = new StageSelectorScreenTransition(deviceContext, controllerHub, graphics, content);
            stageStartScreen.Opened += StageStartScreen_Opened;
            stageStartScreen.StageSelected += StageStartScreen_StageSelected;
            stageStartScreen.Exit += ShowTitleScreen;
        }

        private void DisposeStageStartScreen()
        {
            if (stageStartScreen != null)
            {
                stageStartScreen.Opened -= StageStartScreen_Opened;
                stageStartScreen.StageSelected -= StageStartScreen_StageSelected;
                stageStartScreen.Exit -= ShowTitleScreen;
                stageStartScreen.Dispose();
            }
        }

        private void CreateStageResultScreen()
        {
            stageResultScreen = new StageResultScreen(deviceContext, soundEngine, graphics, content, gameRecord);
            stageResultScreen.Exit += StageResultScreen_Exited;
        }

        private void DisposeStageResultScreen()
        {
            if (stageResultScreen != null)
            {
                stageResultScreen.Exit -= StageResultScreen_Exited;
                stageResultScreen.Dispose();
            }
        }

        private void CreateSettingsScreen()
        {
            settingsScreen = new SettingsScreen(this, deviceContext, controllerHub, soundEngine, logger, content, graphics);
            settingsScreen.Exit += SettingsScreen_Exit;
        }

        private void DisposeSettingsScreen()
        {
            if (settingsScreen != null)
            {
                settingsScreen.Exit -= SettingsScreen_Exit;
                settingsScreen.Dispose();
            }
        }

        private void CreateGameOverScreen()
        {
            gameOverScreen = new GameOverScreen(deviceContext, soundEngine, graphics, controllerHub, content);
            gameOverScreen.Exit += GameOverScreen_Exited;
        }

        private void DisposeGameOverScreen()
        {
            if (gameOverScreen != null)
            {
                gameOverScreen.Exit -= GameOverScreen_Exited;
                gameOverScreen.Dispose();
            }
        }

        private void CreateHiScoreScreen()
        {
            hiScoreScreen = new HiScoreScreen(deviceContext, controllerHub, soundEngine, graphics, content, gameRecord);
            hiScoreScreen.Exit += HiScoreScreen_Exited;
        }

        private void DisposeHiScoreScreen()
        {
            if (hiScoreScreen != null)
            {
                hiScoreScreen.Exit -= HiScoreScreen_Exited;
                hiScoreScreen.Dispose();
            }
        }

        /// <summary>
        /// Очистка ресурсов, освобождения памяти
        /// </summary>
        private void CleanUp()
        {
            UnloadGame();
            controllerHub?.Dispose();

            if (closeFormRequired)
            {
                // закрываем форму
                SafeInvoke(Close);
            }

            mTerminate = false;
        }

        /// <summary>
        /// Острисовка экрана
        /// </summary>
        private void DrawScreen(GameScreenEnum screen, bool isForeground = false)
        {
            try
            {
                switch (screen)
                {
                    case GameScreenEnum.GameOver:
                        gameOverScreen?.Render();
                        break;
                    case GameScreenEnum.HiScores:
                        hiScoreScreen?.Render();
                        break;
                    case GameScreenEnum.StageResult:
                        stageResultScreen.Render();
                        break;
                    case GameScreenEnum.Title:
                        mainScreen?.Render(isForeground);
                        break;
                    case GameScreenEnum.LevelEditor:
                        levelEditor?.Render();
                        break;
                    case GameScreenEnum.ScreenTransition:
                        screenTransition.Render();
                        break;
                    case GameScreenEnum.StartLevel:
                        stageStartScreen.Render();
                        break;
                    case GameScreenEnum.PlayGame:
                        game?.Render();
                        break;
                    case GameScreenEnum.Settings:
                        settingsScreen?.Render();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger?.WriteLine($"{nameof(DrawScreen)} {screen} error: " + ex, LogLevel.Error);
                if (Debugger.IsAttached)
                {
                    throw;
                }
                else
                {
                    closeFormRequired = true;
                    mTerminate = true;
                }
            }
        }

        #endregion

    }
}