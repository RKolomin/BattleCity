using SlimDX.Direct3D9;
using System.Text;

namespace BattleCity.Video
{
    public class D3DState
    {
        private bool Lighting;
        private bool ZEnable;
        private bool ZWriteEnable;
        private bool FogEnable;
        private bool AlphaTestEnable;
        private bool AlphaBlendEnable;
        private bool MultisampleAntialias;
        private Blend SourceBlend;
        private Blend SourceBlendAlpha;
        private Blend DestinationBlend;
        private Blend DestinationBlendAlpha;
        private BlendOperation BlendOperation;
        private int BorderColor;
        private int TextureCoordIndex;

        private FillMode FillMode;
        private Cull CullMode;

        private TextureAddress AddressU;
        private TextureAddress AddressV;

        private Compare AlphaFunc;
        private TextureArgument ColorArg1;
        private TextureArgument ColorArg2;
        private TextureArgument AlphaArg0;
        private TextureArgument AlphaArg1;
        private TextureArgument AlphaArg2;
        private TextureOperation AlphaOperation;
        private TextureOperation ColorOperation;

        private TextureFilter MipFilter;
        private TextureFilter MinFilter;
        private TextureFilter MagFilter;

        private int MipLevel = 0;

        private static int MAX_AA = -1;

        public void Save(Device device)
        {
            Lighting = device.GetRenderState<bool>(RenderState.Lighting);
            ZEnable = device.GetRenderState<bool>(RenderState.ZEnable);
            ZWriteEnable = device.GetRenderState<bool>(RenderState.ZWriteEnable);
            FogEnable = device.GetRenderState<bool>(RenderState.FogEnable);
            AlphaTestEnable = device.GetRenderState<bool>(RenderState.AlphaTestEnable);
            AlphaBlendEnable = device.GetRenderState<bool>(RenderState.AlphaBlendEnable);
            SourceBlend = device.GetRenderState<Blend>(RenderState.SourceBlend);
            SourceBlendAlpha = device.GetRenderState<Blend>(RenderState.SourceBlendAlpha);
            DestinationBlend = device.GetRenderState<Blend>(RenderState.DestinationBlend);
            DestinationBlendAlpha = device.GetRenderState<Blend>(RenderState.DestinationBlendAlpha);
            MultisampleAntialias = device.GetRenderState<bool>(RenderState.MultisampleAntialias);

            FillMode = device.GetRenderState<FillMode>(RenderState.FillMode);
            CullMode = device.GetRenderState<Cull>(RenderState.CullMode);

            AlphaFunc = device.GetRenderState<Compare>(RenderState.AlphaFunc);

            ColorArg1 = device.GetTextureStageState<TextureArgument>(0, TextureStage.ColorArg1);
            ColorArg2 = device.GetTextureStageState<TextureArgument>(0, TextureStage.ColorArg2);
            AlphaArg0 = device.GetTextureStageState<TextureArgument>(0, TextureStage.AlphaArg0);
            AlphaArg1 = device.GetTextureStageState<TextureArgument>(0, TextureStage.AlphaArg1);
            AlphaArg2 = device.GetTextureStageState<TextureArgument>(0, TextureStage.AlphaArg2);
            AlphaOperation = device.GetTextureStageState<TextureOperation>(0, TextureStage.AlphaOperation);
            ColorOperation = device.GetTextureStageState<TextureOperation>(0, TextureStage.ColorOperation);

            MipLevel = device.GetSamplerState(0, SamplerState.MaxMipLevel);
            AddressU = device.GetSamplerState<TextureAddress>(0, SamplerState.AddressU);
            AddressV = device.GetSamplerState<TextureAddress>(0, SamplerState.AddressV);
            MagFilter = device.GetSamplerState<TextureFilter>(0, SamplerState.MagFilter);
            MinFilter = device.GetSamplerState<TextureFilter>(0, SamplerState.MinFilter);
            MipFilter = device.GetSamplerState<TextureFilter>(0, SamplerState.MipFilter);

            BlendOperation = device.GetRenderState<BlendOperation>(RenderState.BlendOperation);
            BorderColor = device.GetSamplerState(0, SamplerState.BorderColor);
            TextureCoordIndex = device.GetTextureStageState(0, TextureStage.TexCoordIndex);
        }

        public void Restore(Device device)
        {
            device.SetRenderState(RenderState.Lighting, Lighting);
            device.SetRenderState(RenderState.ZEnable, ZEnable);
            device.SetRenderState(RenderState.FillMode, FillMode);
            device.SetRenderState(RenderState.ZWriteEnable, ZWriteEnable);
            device.SetRenderState(RenderState.FogEnable, FogEnable);
            device.SetRenderState(RenderState.AlphaTestEnable, AlphaTestEnable);
            device.SetRenderState(RenderState.AlphaBlendEnable, AlphaBlendEnable);
            device.SetRenderState(RenderState.SourceBlend, SourceBlend);
            device.SetRenderState(RenderState.SourceBlendAlpha, SourceBlendAlpha);
            device.SetRenderState(RenderState.DestinationBlend, DestinationBlend);
            device.SetRenderState(RenderState.DestinationBlendAlpha, DestinationBlendAlpha);
            device.SetRenderState(RenderState.CullMode, CullMode);
            device.SetRenderState(RenderState.AlphaFunc, AlphaFunc);
            device.SetRenderState(RenderState.MultisampleAntialias, MultisampleAntialias);
            device.SetRenderState(RenderState.BlendOperation, BlendOperation);
            // Blend Texture and Vertex alphas 
            device.SetTextureStageState(0, TextureStage.ColorArg1, ColorArg1);
            device.SetTextureStageState(0, TextureStage.ColorArg2, ColorArg2);
            device.SetTextureStageState(0, TextureStage.AlphaArg0, AlphaArg0);
            device.SetTextureStageState(0, TextureStage.AlphaArg1, AlphaArg1);
            device.SetTextureStageState(0, TextureStage.AlphaArg2, AlphaArg2);
            device.SetTextureStageState(0, TextureStage.AlphaOperation, AlphaOperation);
            device.SetTextureStageState(0, TextureStage.ColorOperation, ColorOperation);
            device.SetTextureStageState(0, TextureStage.TexCoordIndex, TextureCoordIndex);

            device.SetSamplerState(0, SamplerState.MaxMipLevel, MipLevel);
            device.SetSamplerState(0, SamplerState.AddressU, AddressU);
            device.SetSamplerState(0, SamplerState.AddressV, AddressV);
            device.SetSamplerState(0, SamplerState.MipFilter, MipFilter);
            device.SetSamplerState(0, SamplerState.MinFilter, MinFilter);
            device.SetSamplerState(0, SamplerState.MagFilter, MagFilter);
            device.SetSamplerState(0, SamplerState.BorderColor, BorderColor);


            device.VertexDeclaration = null;
            device.VertexFormat = VertexFormat.None;
        }

        public static void EnableMSAA(Device device)
        {
            EnableMSAA(0, device);
            EnableMSAA(1, device);
        }

        public static void EnableMSAA(int samplerIndex, Device device)
        {
            if (MAX_AA == -1)
                MAX_AA = device.Capabilities.MaxAnisotropy;
            device.SetSamplerState(samplerIndex, SamplerState.MaxAnisotropy, MAX_AA);
            device.SetRenderState(RenderState.MultisampleAntialias, true);
            device.SetSamplerState(samplerIndex, SamplerState.MaxMipLevel, 0);
            device.SetSamplerState(samplerIndex, SamplerState.MinFilter, TextureFilter.Anisotropic);
            device.SetSamplerState(samplerIndex, SamplerState.MagFilter, TextureFilter.Anisotropic);
            device.SetSamplerState(samplerIndex, SamplerState.MipFilter, TextureFilter.Anisotropic);
        }

        public static void EnableLinearFilter(Device device, int anisotropyLevel = 4)
        {
            EnableLinearFilter(0, device, anisotropyLevel);
            EnableLinearFilter(1, device, anisotropyLevel);
        }

        public static void EnableLinearFilter(int samplerIndex, Device device, int anisotropyLevel = 4)
        {
            device.SetSamplerState(samplerIndex, SamplerState.MaxAnisotropy, anisotropyLevel);
            device.SetRenderState(RenderState.MultisampleAntialias, false);
            device.SetSamplerState(samplerIndex, SamplerState.MaxMipLevel, 0);
            device.SetSamplerState(samplerIndex, SamplerState.MinFilter, TextureFilter.Linear);
            device.SetSamplerState(samplerIndex, SamplerState.MagFilter, TextureFilter.Linear);
            device.SetSamplerState(samplerIndex, SamplerState.MipFilter, TextureFilter.Linear);
        }

        public static void DisableMSAA(Device device)
        {
            DisableMSAA(0, device);
            DisableMSAA(1, device);
        }

        public static void DisableMSAA(int samplerIndex, Device device)
        {
            device.SetRenderState(RenderState.MultisampleAntialias, false);
            device.SetSamplerState(samplerIndex, SamplerState.MaxMipLevel, 1);
            device.SetSamplerState(samplerIndex, SamplerState.MinFilter, TextureFilter.Point);
            device.SetSamplerState(samplerIndex, SamplerState.MagFilter, TextureFilter.Point);
            device.SetSamplerState(samplerIndex, SamplerState.MipFilter, TextureFilter.Point);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ZWriteEnable=" + ZWriteEnable);
            sb.AppendLine("AlphaTestEnable=" + AlphaTestEnable);
            sb.AppendLine("AlphaBlendEnable=" + AlphaBlendEnable);
            sb.AppendLine("SourceBlend=" + SourceBlend);
            sb.AppendLine("SourceBlendAlpha=" + SourceBlendAlpha);
            sb.AppendLine("DestinationBlend=" + DestinationBlend);
            sb.AppendLine("AlphaFunc=" + AlphaFunc);
            sb.AppendLine("ColorArg1=" + ColorArg1);
            sb.AppendLine("ColorArg2=" + ColorArg2);
            sb.AppendLine("AlphaArg0=" + AlphaArg0);
            sb.AppendLine("AlphaArg1=" + AlphaArg1);
            sb.AppendLine("AlphaArg2=" + AlphaArg2);
            sb.AppendLine("AlphaOperation=" + AlphaOperation);
            sb.AppendLine("ColorOperation=" + ColorOperation);

            return sb.ToString();
        }
    }
}
