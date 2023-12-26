using System;
using System.Runtime.InteropServices;

namespace BattleCity.Audio
{
    /// <summary>
    /// Структура данных, предоставляющая доступ к элементам массива типа
    /// <see cref="Byte"/>, <see cref="Int16"/>, <see cref="Single"/> без выполенения преобразования
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct AudioSamplesMulticast
    {
        [FieldOffset(0)]
        public byte[] Bytes;
        [FieldOffset(0)]
        public float[] Floats;
        [FieldOffset(0)]
        public short[] Shorts;
    }
}
