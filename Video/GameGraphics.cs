using BattleCity.Repositories;
using BattleCity.Enums;
using BattleCity.GameObjects;
using SlimDX;
using SlimDX.Direct3D9;
using GdiFont = System.Drawing.Font;

namespace BattleCity.Video
{
    /// <summary>
    /// Игровая графика, методы отрисовки
    /// </summary>
    public class GameGraphics : IGameGraphics
    {
        private IDeviceContext deviceContext;
        private TextureRepository textureRepo;
        private Effect shader;
        private Effect chessBoardShader;
        private Effect brickWallShader;
        private Device Device => deviceContext.Device;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="deviceContext">Контекст графического устройства</param>
        /// <param name="textureRepo">Репозиторий текстур</param>
        public GameGraphics(IDeviceContext deviceContext, TextureRepository textureRepo)
        {
            this.deviceContext = deviceContext;
            deviceContext.DeviceLost += DeviceContext_DeviceLost;
            deviceContext.DeviceRestored += DeviceContext_DeviceRestored;
            this.textureRepo = textureRepo;
            shader = Effect.FromString(Device, Shaders.GameObject, ShaderFlags.None);
            chessBoardShader = Effect.FromString(Device, Shaders.ChessBoard, ShaderFlags.None);
            brickWallShader = Effect.FromString(Device, Shaders.BrickWall, ShaderFlags.None);
        }

        /// <summary>
        /// Обработка восстановления устройства
        /// </summary>
        private void DeviceContext_DeviceRestored()
        {
            shader?.OnResetDevice();
            chessBoardShader?.OnResetDevice();
            brickWallShader?.OnResetDevice();
        }

        /// <summary>
        /// Обработка потери устройства
        /// </summary>
        private void DeviceContext_DeviceLost()
        {
            shader?.OnLostDevice();
            chessBoardShader?.OnLostDevice();
            brickWallShader?.OnLostDevice();
        }

        /// <inheritdoc/>
        public void DrawChessboard(int x, int y, int w, int h, 
            int cellSize, int cellColor1, int cellColor2)
        {
            TransformedTextured[] rect =
            {
                new TransformedTextured(x - 0.5f, y - 0.5f, 0, 1, 0, 0),
                new TransformedTextured(x + w - 0.5f, y - 0.5f, 0, 1, 1, 0),
                new TransformedTextured(x - 0.5f, y + h - 0.5f, 0, 1, 0, 1),
                new TransformedTextured(x + w - 0.5f, y + h - 0.5f, 0, 1, 1, 1)
            };

            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            Device.SetTexture(0, null);
            Device.VertexFormat = TransformedTextured.Format;

            chessBoardShader.SetValue("color1", new Color4(cellColor1).ToColor3());
            chessBoardShader.SetValue("color2", new Color4(cellColor2).ToColor3());
            chessBoardShader.SetValue("cols", w / cellSize);
            chessBoardShader.SetValue("rows", h / cellSize);
            chessBoardShader.SetValue("iVPSize", new Vector2(w, h));
            chessBoardShader.Begin();
            chessBoardShader.BeginPass(0);
            Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);
            chessBoardShader.EndPass();
            chessBoardShader.End();
        }

        /// <inheritdoc/>
        public void DrawBrickWallOverlay(float x, float y, float width, float height, float zoomX, float zoomY, int color)
        {
            float tu0 = 0;
            float tv0 = 0;
            float tu1 = 1;
            float tv1 = 1;
            brickWallShader.SetValue("zoom", new Vector2(zoomX, zoomY));
            brickWallShader.SetValue("brickColor", new Color4(color));

            TransformedTextured[] verts =
            {
                new TransformedTextured(x - 0.5f, y - 0.5f, 0, 1, tu0, tv0 ),
                new TransformedTextured(x + width - 0.5f, y - 0.5f, 0, 1, tu1, tv0),
                new TransformedTextured(x + width - 0.5f, y + height - 0.5f, 0, 1, tu1, tv1),
                new TransformedTextured(x - 0.5f, y - 0.5f, 0, 1, tu0, tv0),
                new TransformedTextured(x + width - 0.5f, y + height - 0.5f, 0, 1, tu1, tv1),
                new TransformedTextured(x - 0.5f, y + height - 0.5f, 0, 1, tu0, tv1)
            };

            // Установка состояний отрисовки

            deviceContext.Device.VertexFormat = TransformedTextured.Format;
            deviceContext.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Mirror);
            deviceContext.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Mirror);

            deviceContext.Device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
            deviceContext.Device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
            deviceContext.Device.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
            deviceContext.Device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
            deviceContext.Device.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
            deviceContext.Device.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Diffuse);

            deviceContext.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            deviceContext.Device.SetRenderState(RenderState.SourceBlend, Blend.DestinationColor);
            deviceContext.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseDestinationColor);

            deviceContext.Device.SetTexture(0, null);

            // Отрисовка

            brickWallShader.Begin();
            brickWallShader.BeginPass(0);
            deviceContext.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, verts);
            brickWallShader.EndPass();
            brickWallShader.End();

            // Установка обычных состояний отрисовки
            deviceContext.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            deviceContext.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
            deviceContext.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            deviceContext.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            deviceContext.Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            deviceContext.Device.SetTexture(0, null);
        }

        /// <inheritdoc/>
        public void FillRect(int x, int y, int w, int h, int fillColor)
        {
            TransformedColored[] rect =
            {
                new TransformedColored(x - 0.5f, y - 0.5f, 0, fillColor),
                new TransformedColored(x + w - 0.5f, y - 0.5f, 0, fillColor),
                new TransformedColored(x - 0.5f, y + h - 0.5f, 0, fillColor),
                new TransformedColored(x + w - 0.5f, y + h - 0.5f, 0, fillColor)
            };
            deviceContext.Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            deviceContext.Device.SetTexture(0, null);
            deviceContext.Device.VertexFormat = TransformedColored.Format;
            deviceContext.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);
        }

        /// <inheritdoc/>
        public void DrawBorderRect(int x, int y, int w, int h, int borderColor)
        {
            var lineRect = CreateBorderRect(x, y, w, h, borderColor);
            Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            Device.SetTexture(0, null);
            Device.VertexFormat = VertexFormat.Diffuse | VertexFormat.PositionRhw;
            Device.DrawUserPrimitives(PrimitiveType.LineList, lineRect.Length / 2, lineRect);
        }

        /// <summary>
        /// Создать вершины прямоугольника с заданным цветом граней
        /// </summary>
        private TransformedColored[] CreateBorderRect(int x, int y, int w, int h, int borderColor)
        {
            return new TransformedColored[]
            {
                new TransformedColored(x, y, 0, borderColor),
                new TransformedColored(x + w - 0.5f, y, 0, borderColor),
                new TransformedColored(x + w - 0.5f, y, 0, borderColor),
                new TransformedColored(x + w - 0.5f, y + h - 0.5f, 0, borderColor),
                new TransformedColored(x + w - 0.5f, y + h - 0.5f, 0, borderColor),
                new TransformedColored(x, y + h - 0.5f, 0, borderColor),
                new TransformedColored(x, y, 0, borderColor),
                new TransformedColored(x, y + h, 0, borderColor),
            };
        }

        /// <inheritdoc/>
        public void BeginDrawGameObjects()
        {
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Mirror);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Mirror);

            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.None);
            Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);

            Device.VertexFormat = TransformedColoredTextured.Format;

            shader.Begin();
            shader.BeginPass(0);
        }

        /// <inheritdoc/>
        public void EndDrawGameObjects()
        {
            shader.EndPass();
            shader.End();
        }

        /// <inheritdoc/>
        public void DrawGameObject(int left, int top, GameFieldObject gameObject, int gameTime, int cellSize)
        {
            if (!gameObject.IsVisible) return;

            var verts = Create6Vertex(gameObject, cellSize, left, top);
            var texture = textureRepo.GetOrCreateTexture(gameObject.NextTextureId(gameTime));

            if (texture != null)
            {
                Device.SetTexture(0, texture);
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, verts);
            }
        }

        /// <inheritdoc/>
        public void SetDefaultRenderStates()
        {
            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.None);
            Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
            Device.SetRenderState(RenderState.CullMode, Cull.None);
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            Device.SetTexture(0, null);
        }

        /// <inheritdoc/>
        public void Clear(int fillColor)
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, fillColor, 1, 0);
        }

        /// <summary>
        /// Построение вершин из двух треугольников для указанного объекта
        /// </summary>
        /// <param name="gameObject">Объект, для которого создаются вершины</param>
        /// <param name="cellSize">Размер условной единицы клетки</param>
        /// <param name="left">Абсолютное смещение по X-координате</param>
        /// <param name="top">Абсолютное смещение по Y-координате</param>
        /// <returns></returns>
        private TransformedColoredTextured[] Create6Vertex(GameFieldObject gameObject, int cellSize, int left, int top)
        {
            TransformedColoredTextured[] verts = new TransformedColoredTextured[6];
            int color = gameObject.Color;
            float x = left + (gameObject.X + gameObject.SubPixelX * 0.1f) * cellSize;
            float y = top + (gameObject.Y + gameObject.SubPixelY * 0.1f) * cellSize;
            float w = gameObject.Width * cellSize;
            float h = gameObject.Height * cellSize;

            float tu0 = gameObject.UVTileX == 0 ? 0 : (gameObject.X * gameObject.UVTileX) % gameObject.Width;
            float tv0 = gameObject.UVTileY == 0 ? 0 : (gameObject.Y * gameObject.UVTileY) % gameObject.Height;
            float tu1 = gameObject.UVTileX == 0 ? 1 : 1 - ((gameObject.X + gameObject.Width) * gameObject.UVTileX) % gameObject.Width;
            float tv1 = gameObject.UVTileY == 0 ? 1 : 1 - ((gameObject.Y + gameObject.Height) * gameObject.UVTileY) % gameObject.Height;

            switch (gameObject.Direction)
            {
                case MoveDirection.Up:
                    verts[0] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv0 };
                    verts[1] = new TransformedColoredTextured(x + w - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu1, Tv = tv0 };
                    verts[2] = new TransformedColoredTextured(x + w - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tu1, Tv = tv1 };
                    verts[3] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv0 };
                    verts[4] = new TransformedColoredTextured(x + w - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tu1, Tv = tv1 };
                    verts[5] = new TransformedColoredTextured(x - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv1 };

                    break;
                case MoveDirection.Down:
                    verts[0] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = tu0, Tv = tv1 };
                    verts[1] = new TransformedColoredTextured(x + w - 0.5f, y - 0.5f, 0) { Color = color, Tu = tv1, Tv = tv1 };
                    verts[2] = new TransformedColoredTextured(x + w - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tv1, Tv = tv0 };
                    verts[3] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = tv0, Tv = tv1 };
                    verts[4] = new TransformedColoredTextured(x + w - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tv1, Tv = tv0 };
                    verts[5] = new TransformedColoredTextured(x - 0.5f, y + h - 0.5f, 0) { Color = color, Tu = tv0, Tv = tv0 };

                    break;
                case MoveDirection.Right:
                    verts[0] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = 1, Tv = 1 };
                    verts[1] = new TransformedColoredTextured(x + h - 0.5f, y - 0.5f, 0) { Color = color, Tu = 1, Tv = 0 };
                    verts[2] = new TransformedColoredTextured(x + h - 0.5f, y + w - 0.5f, 0) { Color = color, Tu = 0, Tv = 0 };
                    verts[3] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = 1, Tv = 1 };
                    verts[4] = new TransformedColoredTextured(x + h - 0.5f, y + w - 0.5f, 0) { Color = color, Tu = 0, Tv = 0 };
                    verts[5] = new TransformedColoredTextured(x - 0.5f, y + w - 0.5f, 0) { Color = color, Tu = 0, Tv = 1 };

                    break;
                case MoveDirection.Left:
                    verts[0] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = 1, Tv = 0 };
                    verts[1] = new TransformedColoredTextured(x + h - 0.5f, y - 0.5f, 0) { Color = color, Tu = 1, Tv = 1 };
                    verts[2] = new TransformedColoredTextured(x + h - 0.5f, y + w - 0.5f, 0) { Color = color, Tu = 0, Tv = 1 };
                    verts[3] = new TransformedColoredTextured(x - 0.5f, y - 0.5f, 0) { Color = color, Tu = 1, Tv = 0 };
                    verts[4] = new TransformedColoredTextured(x + h - 0.5f, y + w - 0.5f, 0) { Color = color, Tu = 0, Tv = 1 };
                    verts[5] = new TransformedColoredTextured(x - 0.5f, y + w - 0.5f, 0) { Color = color, Tu = 0, Tv = 0 };

                    break;
            }

            return verts;
        }

        /// <inheritdoc/>
        public IGameFont CreateFont(GdiFont gdiFont)
        {
            return new GameFont(deviceContext, gdiFont);
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            if (deviceContext != null)
            {
                deviceContext.DeviceLost -= DeviceContext_DeviceLost;
                deviceContext.DeviceRestored -= DeviceContext_DeviceRestored;
                deviceContext = null;
            }

            textureRepo = null;

            if (shader != null)
            {
                shader.Dispose();
                shader = null;
            }

            if (brickWallShader != null)
            {
                brickWallShader.Dispose();
                brickWallShader = null;
            }

            if (chessBoardShader != null)
            {
                chessBoardShader.Dispose();
                chessBoardShader = null;
            }
        }
    }
}