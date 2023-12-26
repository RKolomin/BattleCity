using BattleCity.Common;
using SlimDX.Direct3D9;
using Texture2D = SlimDX.Direct3D9.Texture;

namespace BattleCity.Video
{
    public class ScanlinesPostProcessEffect : IPostProcessEffect
    {
        private IGameApplication gameApplication;
        private IDeviceContext deviceContext;
        private AppSettings appSettings;
        private Effect effect;
        private Texture2D postTx;

        public ScanlinesPostProcessEffect(IGameApplication gameApplication, IDeviceContext deviceContext, AppSettings appSettings)
        {
            this.gameApplication = gameApplication;
            this.deviceContext = deviceContext;
            this.appSettings = appSettings;
            deviceContext.DeviceLost += DeviceContext_DeviceLost;
            deviceContext.DeviceRestored += DeviceContext_DeviceRestored;
            
            effect = Effect.FromString(deviceContext.Device, Shaders.Scanlines, ShaderFlags.None);
            CreateRenderTarget();
        }

        private void DeviceContext_DeviceRestored()
        {
            effect?.OnResetDevice();
            CreateRenderTarget();
        }

        private void DeviceContext_DeviceLost()
        {
            effect?.OnLostDevice();
            DisposeRenderTarget();
        }

        private void DisposeRenderTarget()
        {
            if (postTx != null)
            {
                postTx.Dispose();
                postTx = null;
            }
        }

        private void CreateRenderTarget()
        {
            using (var swapChain = deviceContext.Device.GetSwapChain(0))
            {
                var pp = swapChain.PresentParameters;
                postTx = new Texture2D(
                    deviceContext.Device,
                    pp.BackBufferWidth,
                    pp.BackBufferHeight,
                    1,
                    Usage.RenderTarget,
                    pp.BackBufferFormat,
                    Pool.Default);
            }
        }

        /// <inheritdoc/>
        public void Draw()
        {
            if (postTx == null)
                return;

            float x = 0;
            float y = 0;
            float w = deviceContext.DeviceWidth;
            float h = deviceContext.DeviceHeight;

            TransformedTextured[] rect =
            {
                new TransformedTextured(x - 0.5f, y - 0.5f, 0, 1, 0, 0),
                new TransformedTextured(x + w - 0.5f, y - 0.5f, 0, 1, 1, 0),
                new TransformedTextured(x - 0.5f, y + h - 0.5f, 0, 1, 0, 1),
                new TransformedTextured(x + w - 0.5f, y + h - 0.5f, 0, 1, 1, 1)
            };

            using (var surface = deviceContext.Device.GetRenderTarget(0))
            {
                using (var dstSurf = postTx.GetSurfaceLevel(0))
                {
                    deviceContext.Device.StretchRectangle(surface, dstSurf, TextureFilter.Point);
                }
            }

            deviceContext.Device.VertexFormat = TransformedTextured.Format;
            deviceContext.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            deviceContext.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            deviceContext.Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            deviceContext.Device.SetTexture(0, postTx);

            effect.SetValue("aspectY", (float)deviceContext.DeviceHeight / gameApplication.ClientSizeHeight);
            effect.SetValue("amount", appSettings.ScanlinesFxLevel * 0.01f);
            effect.Begin();
            effect.BeginPass(0);
            deviceContext.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);
            effect.EndPass();
            effect.End();

            deviceContext.Device.SetTexture(0, null);
        }

        public void Dispose()
        {
            if (deviceContext != null)
            {
                deviceContext.DeviceLost -= DeviceContext_DeviceLost;
                deviceContext.DeviceRestored -= DeviceContext_DeviceRestored;
                deviceContext = null;
            }

            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }

            DisposeRenderTarget();
            appSettings = null;
            gameApplication = null;
        }
    }
}
