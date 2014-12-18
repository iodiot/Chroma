using Microsoft.Xna.Framework;

namespace Chroma.Graphics
{
  sealed class Sprite
  {
    public string TextureName;
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public Vector2 AnchorPoint;

    public Sprite(int x, int y, int width, int height, string textureName)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
      TextureName = textureName;
      AnchorPoint = Vector2.Zero;
    }
  }
}
