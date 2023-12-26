using BattleCity.Extensions;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace BattleCity.Common
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Наименование файла конфигураций
        /// </summary>
        const string FILENAME = "app_settings.json";

        /// <summary>
        /// Уровень громкости звуков
        /// </summary>
        public float SoundLevel { get; set; } = 1;

        /// <summary>
        /// Уровень громкости музыки
        /// </summary>
        public float MusicLevel { get; set; } = 1;

        /// <summary>
        /// Признак полноэкранного режима
        /// </summary>
        public bool FullScreen { get; set; }

        /// <summary>
        /// Сохранять пропорции экрана
        /// </summary>
        public bool SaveAspectRatio { get; set; } = true;

        /// <summary>
        /// Значение эффекта Scanlines
        /// </summary>
        public int ScanlinesFxLevel { get; set; } = 25;

        /// <summary>
        /// Непрерывные выстрел при зажатой кнопки
        /// </summary>
        public bool ContinuousFire { get; set; }

        /// <summary>
        /// Удалить файл конфигурации
        /// </summary>
        public static void Delete()
        {
            try
            {
                File.Delete(FILENAME);
            }
            catch { }
        }

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        /// <returns></returns>
        public static AppSettings Load()
        {
            try
            {
                var data = File.ReadAllText(FILENAME, Encoding.UTF8);
                return JsonConvert.DeserializeObject<AppSettings>(data);
            }
            catch
            {
                return new AppSettings();
            }
        }

        /// <summary>
        /// Сохранить настройки
        /// </summary>
        public void Save()
        {
            try
            {
                File.WriteAllText(FILENAME, this.ToJson(), Encoding.UTF8);
            }
            catch { }
        }
    }
}
