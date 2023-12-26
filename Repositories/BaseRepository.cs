using BattleCity.Common;
using BattleCity.Extensions;
using BattleCity.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BattleCity.Repositories
{
    /// <summary>
    /// Базовый репозиторий
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public abstract class BaseRepository<T> : IDisposable where T : class, IResxId
    {
        protected string directory;
        protected string filename;
        protected T[] array;
        protected ILogger logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger">Сервис логирования</param>
        /// <param name="directory">Путь к директории</param>
        /// <param name="filename">Имя файла</param>
        /// <param name="capacity">Максимальный размер массива данных</param>
        public BaseRepository(ILogger logger, string directory, string filename, int capacity)
        {
            this.logger = logger;
            this.directory = directory;
            this.filename = filename;
            array = new T[capacity];
        }

        public virtual void Save()
        {
            var data = array.ToJson();
            File.WriteAllText(Fullpath, data, Encoding.UTF8);
        }

        protected virtual void Deserialize()
        {
            if (!File.Exists(Fullpath))
                return;

            try
            {
                var data = File.ReadAllText(Fullpath, Encoding.UTF8);
                var list = JsonConvert.DeserializeObject<List<T>>(data);
                int index = 0;
                foreach (var t in list)
                {
                    if (t != null)
                        OnDeserializeItem(index, t);
                    index++;
                }
            }
            catch { }
        }

        protected virtual void OnDeserializeItem(int index, T t) { array[index] = t; }

        /// <summary>
        /// Очистить массив данных
        /// </summary>
        public virtual void Clear()
        {
            if (array == null) return;
            for (int i = 0; i < array.Length; i++)
            {
                var x = array[i];
                if (x == null) continue;
                if (x is IDisposable)
                    (x as IDisposable).Dispose();
                array[i] = null;
            }
        }

        public virtual void Dispose()
        {
            Clear();
            array = null;
        }

        /// <summary>
        /// Максимальный размер массива данных
        /// </summary>
        public int Capacity => array.Length;

        /// <summary>
        /// Получить элемент массива по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual T this[int index]
        {
            get { return array[index]; }
            protected set { array[index] = value; }
        }

        /// <summary>
        /// Полный путь к файлу данных
        /// </summary>
        protected string Fullpath => Path.Combine(directory, filename);
    }
}