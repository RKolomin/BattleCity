namespace BattleCity.GameObjects
{
    public class AnimationObject : GameFieldObject
    {
        public bool AutoRepeat { get; set; }
        public int? Duration { get; set; }
        public int ElapsedFrames => Duration.HasValue
            ? Duration.Value - frameNumber
            : (TextureAnimationTime * TextureIdList.Count) - frameNumber;
        int frameNumber;
        int textureIndex;

        public GameFieldObject AttachedObject { get; set; }

        public AnimationObject()
        {
            Type = Enums.GameObjectType.None;
        }

        public void Update()
        {
            if (AttachedObject != null)
            {
                X = AttachedObject.X;
                Y = AttachedObject.Y;
                SubPixelX = AttachedObject.SubPixelX;
                SubPixelY = AttachedObject.SubPixelY;
                Width = AttachedObject.Width;
                Height = AttachedObject.Height;
            }

            if (TextureAnimationTime > 0 && ElapsedFrames == 0 && AutoRepeat)
            {
                frameNumber %= TextureAnimationTime;
            }

            if (ElapsedFrames > 0)
            {
                frameNumber++;
                if (frameNumber % TextureAnimationTime == 0)
                    textureIndex++;
                if (textureIndex >= TextureIdList.Count)
                    textureIndex = 0;
            }
        }

        public override int NextTextureId(int gameTime)
        {
            if (TextureIdList.Count == 0)
                return 0;

            return textureIndex == -1 ? -1 : TextureIdList[textureIndex];
        }
    }
}
