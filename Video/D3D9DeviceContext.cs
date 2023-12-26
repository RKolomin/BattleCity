using SlimDX;
using SlimDX.Direct3D9;
using System;

namespace BattleCity.Video
{
    public class D3D9DeviceContext : IDeviceContext, IDisposable
    {
        /// <summary>
        /// Создать контекст графического устройства
        /// </summary>
        /// <param name="handle">Декскиптор элемента отображения (Form, Control)</param>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        /// <returns></returns>
        public static D3D9DeviceContext Create(IntPtr handle, int width, int height)
        {
            using (var d3d = new Direct3D())
            {
                var pp = CreatePresentParameters(handle, width, height);
                var device = new Device(d3d, d3d.Adapters[0].Adapter, DeviceType.Hardware, handle,
                    CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded, pp);

                return new D3D9DeviceContext(device, pp);
            }
        }

        /// <summary>
        /// Создать параметры графического устройства
        /// </summary>
        private static PresentParameters CreatePresentParameters(IntPtr handle, int width, int height)
        {
            return new PresentParameters
            {
                Windowed = true,
                PresentationInterval = PresentInterval.One,
                SwapEffect = SwapEffect.Discard,
                AutoDepthStencilFormat = Format.D16,
                BackBufferCount = 1,
                BackBufferFormat = Format.Unknown,
                //BackBufferFormat = Format.X1R5G5B5,
                BackBufferWidth = width,
                BackBufferHeight = height,
                //presentParameters.BackBufferWidth = 1920;
                //presentParameters.BackBufferHeight = 1080;
                DeviceWindowHandle = handle,
                EnableAutoDepthStencil = true,
                Multisample = MultisampleType.None,
                FullScreenRefreshRateInHertz = 0
            };
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        private D3D9DeviceContext(Device device, PresentParameters pp)
        {
            Device = device;
            PresentParameters = pp;
            DeviceWidth = pp.BackBufferWidth;
            DeviceHeight = pp.BackBufferHeight;
        }

        /// <summary>
        /// Параметры графического устройства
        /// </summary>
        public PresentParameters PresentParameters { get; private set; }

        /// <summary>
        /// Признак отсутствия ограничения количества кадров в секунду
        /// </summary>
        public bool HasFreeFps => PresentParameters.PresentationInterval == PresentInterval.Immediate;

        /// <inheritdoc/>
        public Device Device { get; private set; }

        /// <inheritdoc/>
        public int DeviceWidth { get; private set; }

        /// <inheritdoc/>
        public int DeviceHeight { get; private set; }

        /// <inheritdoc/>
        public bool IsLost()
        {
            try
            {
                Result d3dRes = Device.TestCooperativeLevel();
                if (d3dRes == ResultCode.DeviceLost
                    || d3dRes == ResultCode.DeviceNotReset)
                {
                    return true;
                }
            }
            catch { return true; }
            return false;
        }

        /// <summary>
        /// Очистить поверхность
        /// </summary>
        /// <param name="zbuffer">Очистить буффер глубины</param>
        /// <param name="target">Очистить поверхность отрисовки</param>
        /// <param name="color">Цвет заливки поверхности отрисовки</param>
        public void Clear(bool zbuffer, bool target, int color)
        {
            ClearFlags flags = ClearFlags.None;
            if (zbuffer) flags|= ClearFlags.ZBuffer;
            if (target) flags |= ClearFlags.Target;
            Device.Clear(flags, color, 0, 1);
        }

        /// <summary>
        /// Сброс графического устройства
        /// </summary>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        public void Reset(int width, int height)
        {
            PresentParameters.BackBufferWidth = width;
            PresentParameters.BackBufferHeight = height;
            Device.Reset(PresentParameters);
        }

        /// <summary>
        /// Сообщить о петери устройства
        /// </summary>
        public void OnDeviceLost() => DeviceLost?.Invoke();

        /// <summary>
        /// Сообщить о восстановлении устройства
        /// </summary>
        public void OnDeviceRestore() => DeviceRestored?.Invoke();

        /// <summary>
        /// Сообщить об изменении размера ширины / высоты в параметрах устройства
        /// </summary>
        public void OnDeviceResize()
        {
            DeviceWidth = PresentParameters.BackBufferWidth;
            DeviceHeight = PresentParameters.BackBufferHeight;
            DeviceResize?.Invoke();
        }

        /// <summary>
        /// Очистить используемые ресурсы, освободить память
        /// </summary>
        public void Dispose()
        {
            Device?.Dispose();
            Device = null;
        }

        /// <inheritdoc/>
        public event Action DeviceLost;
        /// <inheritdoc/>
        public event Action DeviceRestored;
        /// <inheritdoc/>
        public event Action DeviceResize;
    }

}
