using BattleCity.Video;
using System;
using System.Linq;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Опция меню в разделе Extras
    /// </summary>
    public class ExtrasMenuOption : MenuOption
    {
        private readonly string[] values;
        private string currentValue;
        private readonly string originalValue;
        private readonly string zeroIndexAlterText;
        private readonly Action<string> valueChangeHandler;
        private int index;

        /// <summary>
        /// Признак установленного значения, отличного от оригинального
        /// </summary>
        public bool IsChanged => originalValue != currentValue;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public ExtrasMenuOption() : base() { }

        public ExtrasMenuOption(
            string text,
            string currentValue,
            string originalValue,
            string[] values,
            string zeroIndexAlterText,
            Action<string> valueChangeHandler)
            : base()
        {
            this.valueChangeHandler = valueChangeHandler;
            this.originalValue = originalValue;
            this.values = values;
            this.currentValue = currentValue;
            this.zeroIndexAlterText = zeroIndexAlterText ?? values.FirstOrDefault();
            index = Array.FindIndex(values, p => string.Compare(p, currentValue, true) == 0);
            Text = text;
        }

        /// <summary>
        /// Отрисовать
        /// </summary>
        /// <param name="font"></param>
        public override void Draw(IGameFont font)
        {
            if (values == null)
            {
                base.Draw(font);
            }
            else if (index == -1)
            {
                //font.DrawString(null, $"{Text}: <wrong value>", X, Y, Color);
                font.DrawString($"{Text}: {currentValue}", X, Y, Color);
            }
            else
            {
                if (index == 0)
                    font.DrawString($"{Text}: {zeroIndexAlterText}", X, Y, Color);
                else
                    font.DrawString($"{Text}: {currentValue}", X, Y, Color);
            }
        }

        /// <summary>
        /// Уставновить следующиее возможное значение
        /// </summary>
        public void NextValue()
        {
            if (values == null || values.Length == 0)
                return;

            index = (index + 1) % values.Length;
            currentValue = values[index];
            valueChangeHandler?.Invoke(currentValue);
        }

        /// <summary>
        /// Уставновить предыдущее возможное значение
        /// </summary>
        public void PreviousValue()
        {
            if (values == null || values.Length == 0)
                return;

            index = index == 0 ? values.Length - 1 : index - 1;
            currentValue = values[index];
            valueChangeHandler?.Invoke(currentValue);
        }

        /// <summary>
        /// Сброс к значению по умолчанию
        /// </summary>
        public void Reset()
        {
            if (values == null || currentValue == originalValue)
                return;

            currentValue = originalValue;
            index = Array.FindIndex(values, p => string.Compare(p, currentValue, true) == 0);
            valueChangeHandler?.Invoke(currentValue);
        }
    }
}
