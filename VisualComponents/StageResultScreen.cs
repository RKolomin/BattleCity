using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX.Direct3D9;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    public class StageResultScreen : IDisposable
    {
        class SubTotals
        {
            /// <summary>
            /// Бонусные очки (домножаются на фактическое количество уничтоженных юнитов по каждому игроку)
            /// </summary>
            public int BonusPoints { get; set; }

            /// <summary>
            /// Идентификатор текстуры-иконки
            /// </summary>
            public int TextureId { get; set; }

            /// <summary>
            /// Признак окончания подсчётов
            /// </summary>
            public bool IsCalculated { get; set; }

            /// <summary>
            /// Значение всего, определяется как максимальное количество уничтоженных юнитов среди игроков
            /// </summary>
            public int Total { get; set; }

            /// <summary>
            /// Подсчитанное (анимированное) значение
            /// </summary>
            public int PreCalculated { get; set; }
        }

        #region events

        public event Action<StageResult> Exit;

        #endregion

        #region members

        private StageResult stageResult;
        private IGameFont font;
        private IDeviceContext deviceContext;
        private ISoundEngine soundEngine;
        private GameContent content;
        private GameAchievements gameRecord;
        private IGameGraphics graphics;
        private List<SubTotals> enemyObjects;
        private bool isBonusCalculated = false;
        private int frameNumber = 0;
        private const int ShowTotalsDuration = 2 * 60;
        private const int IntroShowDuration = 30;
        private const int DelayBetweenSubTotals = 30;
        private int showTotalsRemainFrames;
        private int remainDelayBetweenSubTotals;
        private readonly int textHeight;
        private readonly int textLineHeight;
        private readonly int screenWidth, screenHeight;
        private readonly int HiScoreLabelColor = Colors.Tomato;
        private readonly int HiScoreValueColor = Colors.Orange;
        private readonly int TextColor;

        #endregion

        #region Constructor

        public StageResultScreen(
            IDeviceContext deviceContext,
            ISoundEngine soundEngine,
            IGameGraphics graphics,
            GameContent content,
            GameAchievements gameRecord)
        {
            this.deviceContext = deviceContext;
            this.soundEngine = soundEngine;
            this.graphics = graphics;
            this.gameRecord = gameRecord;
            this.content = content;
            screenWidth = deviceContext.DeviceWidth;
            screenHeight = deviceContext.DeviceHeight;
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            textHeight = Convert.ToInt32(font.MeasureString("L").Height * 1d);
            textLineHeight = Convert.ToInt32(font.MeasureString("L").Height * 1.4d);
            TextColor = content.GameConfig.TextColor;
        }

        #endregion

        #region methods

        /// <summary>
        /// Задать результат
        /// </summary>
        /// <param name="stageResult"></param>
        public void SetResult(StageResult stageResult)
        {
            this.stageResult = stageResult;

            // формируем список иконок вражеских юнитов
            enemyObjects = content.GameObjects
                // получаем все объекты типа Enemy
                .GetAll(p => p.Type.HasFlag(GameObjectType.Enemy))
                // группируем по идентфикатору текстуры, выбираем с мин уровнем прокачки
                .GroupBy(p => p.TextureIdList.FirstOrDefault(), (key, g) => g.OrderBy(o => o.UpgradeLevel).First())
                // сортируем по бонусным очкам
                .OrderBy(o => o.BonusPoints)
                // создаем выборку
                .Select(s => new SubTotals()
                {
                    BonusPoints = s.BonusPoints,
                    TextureId = s.TextureIdList.FirstOrDefault(),
                })
                .ToList();

            foreach (var enemyObject in enemyObjects)
            {
                foreach (var player in stageResult.PlayersResults)
                {
                    enemyObject.Total += player.DestroyedEnemies.Count(p => p.TextureId == enemyObject.TextureId);
                }
            }
        }

        /// <summary>
        /// Сбросить в начальное состояние
        /// </summary>
        public void Reset()
        {
            frameNumber = 0;
            showTotalsRemainFrames = -1;
            remainDelayBetweenSubTotals = 0;
            showTotalsRemainFrames = 0;
            isBonusCalculated = false;
        }

        /// <summary>
        /// Отрисовка
        /// </summary>
        public void Render()
        {
            int screenHalfWidth = screenWidth / 2;
            int screenThirdWidth = screenWidth / 3;
            int screenQuarterWidth = screenWidth / 4;
            int y = textLineHeight + textLineHeight / 2;

            int iconSize = textHeight * 2;
            int iconColor = Colors.White;

            var rect = new Rectangle() { X = 0, Y = y, Width = screenHalfWidth, Height = screenHeight };
            font.DrawString($"HI-SCORE", rect, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, HiScoreLabelColor);

            rect = new Rectangle() { X = screenHalfWidth + textHeight * 2, Y = y, Width = screenHalfWidth, Height = screenHeight };
            font.DrawString($"{gameRecord.HiScoreValue ?? 0}", rect, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, HiScoreValueColor);

            y += textLineHeight;
            rect = new Rectangle() { X = 0, Y = y, Width = screenHalfWidth, Height = screenHeight };
            font.DrawString($"STAGE", rect, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, TextColor);
            rect = new Rectangle() { X = screenHalfWidth + textHeight * 2, Y = y, Width = screenHalfWidth, Height = screenHeight };
            font.DrawString($"{stageResult.StageNumber}", rect, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, TextColor);

            y += textLineHeight;

            var player1 = stageResult.PlayersResults.FirstOrDefault(p => p.Id == 1);
            var player2 = stageResult.PlayersResults.FirstOrDefault(p => p.Id == 2);

            if (player1 != null)
            {
                rect = new Rectangle() { X = 0, Y = y, Width = screenThirdWidth, Height = screenHeight };
                font.DrawString($"I-PLAYER", rect, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, HiScoreLabelColor);

                rect.Y = y + textLineHeight;
                font.DrawString($"{stageResult.PlayersResults[0].Score}", rect, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, HiScoreValueColor);
            }

            if (player2 != null)
            {
                rect = new Rectangle() { X = screenThirdWidth * 2, Y = y, Width = screenThirdWidth, Height = screenHeight };
                font.DrawString($"II-PLAYER", rect, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, HiScoreLabelColor);

                rect.Y = y + textLineHeight;
                font.DrawString($"{stageResult.PlayersResults[1].Score}", rect, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, HiScoreValueColor);
            }

            y += textLineHeight * 2;

            SetObjectIconRenderStates();

            int x = (screenWidth - iconSize) / 2;
            int iconY = y;
            int tableRowIndex = 0;

            bool isCalculationsComplete = enemyObjects.All(p => p.IsCalculated);
            if (isCalculationsComplete && showTotalsRemainFrames == -1)
                showTotalsRemainFrames = ShowTotalsDuration;

            bool skipPreCalculation = frameNumber < IntroShowDuration || remainDelayBetweenSubTotals > 0;
            int iconLineNumber = 0;
            foreach (var enemy in enemyObjects)
            {
                iconLineNumber++;
                DrawObjectIcon(x, iconY, iconSize, iconSize, iconColor, enemy.TextureId);

                if (!enemy.IsCalculated && !skipPreCalculation)
                {
                    if (frameNumber % 10 == 0)
                    {
                        if (enemy.Total > 0 && enemy.PreCalculated < enemy.Total)
                            soundEngine.PlaySound("count");
                        PreCalculate(enemy);
                        skipPreCalculation = true;
                        if (enemy.IsCalculated)
                            remainDelayBetweenSubTotals = DelayBetweenSubTotals;
                    }
                }

                if (player1 != null)
                {
                    int n = Math.Min(enemy.PreCalculated, player1.DestroyedEnemies.Count(p => p.TextureId == enemy.TextureId));
                    string count = !enemy.IsCalculated && enemy.PreCalculated == 0 ? " " : n.ToString();
                    string points = !enemy.IsCalculated && enemy.PreCalculated == 0
                        ? ""
                        : (n * enemy.BonusPoints).ToString();
                    DrawText($"{points} PTS   {count} X", 0, iconY + textHeight / 2, x - iconSize / 2,
                        screenHeight, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, TextColor);
                }
                if (player2 != null)
                {
                    int n = Math.Min(enemy.PreCalculated, player2.DestroyedEnemies.Count(p => p.TextureId == enemy.TextureId));
                    string count = !enemy.IsCalculated && enemy.PreCalculated == 0 ? " " : n.ToString();
                    string points = !enemy.IsCalculated && enemy.PreCalculated == 0
                        ? ""
                        : (n * enemy.BonusPoints).ToString();
                    DrawText($"X {count}   {points} PTS", x + iconSize + iconSize / 2, iconY + textHeight / 2,
                            screenThirdWidth, screenHeight, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, TextColor);

                }

                if (iconLineNumber == enemyObjects.Count)
                    iconY += iconSize;
                else
                    iconY += iconSize + textHeight;
                tableRowIndex++;
            }

            x = (screenWidth - screenQuarterWidth) / 2;
            graphics.FillRect(x, iconY, screenQuarterWidth, 3, iconColor);

            x = (screenWidth - iconSize) / 2;
            y = iconY + textLineHeight / 2;

            // Draw Total

            DrawText($"TOTAL", 0, y, screenThirdWidth,
                   screenHeight, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, iconColor);

            if (isCalculationsComplete && remainDelayBetweenSubTotals <= 0)
            {
                if (player1 != null)
                {
                    DrawText($"{player1.DestroyedEnemies.Count}", 0, y, x - (iconSize / 2) - 2 * textHeight,
                        screenHeight, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, TextColor);
                }
                if (player2 != null)
                {
                    DrawText($"{player2.DestroyedEnemies.Count}", x + (iconSize + iconSize / 2) + 2 * textHeight, y, x - iconSize / 2,
                         screenHeight, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, TextColor);
                }
            }

            y += textLineHeight + textLineHeight / 2;

            if (!stageResult.IsGameOver && isCalculationsComplete && remainDelayBetweenSubTotals <= 0)
            {
                if (player1 != null && player2 != null)
                {
                    int player1BonusPoints = 0;
                    int player2BonusPoints = 0;

                    if (content.GameConfig.ChallengeBonusPoints > 0 || content.GameConfig.StageBonusPoints > 0)
                    {
                        player1BonusPoints = Math.Max(0, content.GameConfig.StageBonusPoints);
                        player2BonusPoints = Math.Max(0, content.GameConfig.StageBonusPoints);

                        if (player1.DestroyedEnemies.Count > player2.DestroyedEnemies.Count)
                        {
                            DrawText($"BONUS!", 0, y, screenThirdWidth,
                                screenHeight, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, HiScoreLabelColor);
                            DrawText($"{content.GameConfig.ChallengeBonusPoints} PTS", 0, y + textLineHeight, screenThirdWidth,
                                screenHeight, DrawStringFormat.Top | DrawStringFormat.Right | DrawStringFormat.NoClip, TextColor);
                            player1BonusPoints += content.GameConfig.ChallengeBonusPoints;
                        }
                        else if (player1.DestroyedEnemies.Count < player2.DestroyedEnemies.Count)
                        {
                            DrawText($"BONUS!", x + (iconSize + iconSize / 2) + 2 * textHeight, y, screenThirdWidth,
                                screenHeight, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, HiScoreLabelColor);
                            DrawText($"{content.GameConfig.ChallengeBonusPoints} PTS", x + (iconSize + iconSize / 2) + 2 * textHeight, y + textLineHeight, screenThirdWidth,
                                screenHeight, DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip, TextColor);
                            player2BonusPoints += content.GameConfig.ChallengeBonusPoints;
                        }
                    }

                    if (!isBonusCalculated)
                    {
                        isBonusCalculated = true;
                        if (player1BonusPoints > 0)
                            AddBonusPoints(player1, player1BonusPoints);
                        if (player2BonusPoints > 0)
                            AddBonusPoints(player2, player2BonusPoints);
                    }
                }
            }

            if (isCalculationsComplete && showTotalsRemainFrames == 0)
            {
                if (player1 != null)
                    gameRecord.Player1Record.UpdateHiScoreValue(player1.Score);
                if (player2 != null)
                    gameRecord.Player2Record.UpdateHiScoreValue(player2.Score);
                Exit?.Invoke(stageResult);
            }

            if (remainDelayBetweenSubTotals <= 0 && showTotalsRemainFrames != -1)
                showTotalsRemainFrames--;
            frameNumber++;
            remainDelayBetweenSubTotals--;
        }

        private void AddBonusPoints(Player player, int points)
        {
            int n = content.GameConfig.RewardsExtraLifeAnEvery <= 0 ? 0 : player.Score / content.GameConfig.RewardsExtraLifeAnEvery;
            player.Score += points;
            int m = content.GameConfig.RewardsExtraLifeAnEvery <= 0 ? 0 : player.Score / content.GameConfig.RewardsExtraLifeAnEvery;

            if (m > n)
            {
                player.Lifes++;
                soundEngine.PlaySound("extra_life");
            }
            else
            {
                soundEngine.PlaySound("bonus_points");
            }
        }

        private void PreCalculate(SubTotals enemy)
        {
            if (enemy.PreCalculated < enemy.Total)
            {
                enemy.PreCalculated++;
            }
            else
            {
                enemy.IsCalculated = true;
            }
        }

        /// <summary>
        /// Отрисовать текст
        /// </summary>
        private void DrawText(string text, int x, int y, int w, int h, DrawStringFormat format, int color)
        {
            font.DrawString(text, new Rectangle(x, y, w, h), format, color);
        }

        /// <summary>
        /// Отрисовка иконки
        /// </summary>
        private void DrawObjectIcon(int x, int y, int w, int h, int color, int textureId)
        {
            int tu0 = 0;
            int tu1 = 1;
            int tv0 = 0;
            int tv1 = 1;

            TransformedColoredTextured[] verts =
            {
                new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv0 },
                new TransformedColoredTextured(x + w - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu1, Tv = tv0 },
                new TransformedColoredTextured(x + w - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tu1, Tv = tv1 },
                new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv0 },
                new TransformedColoredTextured(x + w - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tu1, Tv = tv1 },
                new TransformedColoredTextured(x - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv1 }
            };

            var texture = content.Textures.GetOrCreateTexture(textureId);
            if (texture != null)
            {
                deviceContext.Device.SetTexture(0, texture);
                deviceContext.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, verts);
            }
        }

        /// <summary>
        /// Установка состояний отрисовки иконок
        /// </summary>
        private void SetObjectIconRenderStates()
        {
            deviceContext.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            deviceContext.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            deviceContext.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            deviceContext.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Mirror);
            deviceContext.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Mirror);
            deviceContext.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
            deviceContext.Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.None);
            deviceContext.Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);
            deviceContext.Device.VertexFormat = TransformedColoredTextured.Format;
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            deviceContext = null;
            graphics = null;
            soundEngine = null;
            content = null;
            gameRecord = null;
        }

        #endregion

    }
}
