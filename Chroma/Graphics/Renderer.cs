using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Chroma.Graphics
{
  sealed class Renderer
  {
    public Vector2 ScreenCenter { get { return new Vector2(ScreenWidth/2, ScreenHeight/2); } }

    public Vector2 World;

    public readonly int ScreenWidth;
    public readonly int ScreenHeight;

    private readonly Core core;
    private readonly SpriteBatch spriteBatch;

    public Renderer(Core core, SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
      this.core = core;
      this.spriteBatch = spriteBatch;

      ScreenWidth = screenWidth;
      ScreenHeight = screenHeight;

      World = Vector2.Zero;
    }

    public void Update(int ticks)
    {
      
    }

    public void Begin(BlendState blendState)
    {
      var scale = Matrix.CreateScale(new Vector3(Settings.ScaleFactor, Settings.ScaleFactor, 1));
      spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, scale);
    }

    public void End()
    {
      spriteBatch.End();
    }

    #region World draw

    public void DrawSpriteW(Sprite sprite, Vector2 position, Color tint, float scale = 1.0f)
    {
      DrawSpriteS(sprite, position + World, tint, scale);
    }

    public void DrawRectangleW(Vector2 position, float width, float height, Color color)
    {
      DrawRectangleS(position + World, width, height, color);
    }

    public void DrawLineW(Vector2 from, Vector2 to, Color color)
    {
      DrawLineS(from + World, to + World, color);
    }

    public void DrawSpriteW(string spriteName, Vector2 position, Color tint, float scale = 1.0f)
    {
      DrawSpriteS(core.SpriteManager.GetSprite(spriteName), position, tint, scale);
    }

    #endregion

    #region Screen draw

    public void DrawSpriteS(Sprite sprite, Vector2 position, Color tint, float scale = 1.0f)
    {
      var d = new Vector2(sprite.AnchorPoint.X * sprite.Width, sprite.AnchorPoint.Y * sprite.Height);

      spriteBatch.Draw(
        core.SpriteManager.GetTexture(sprite.TextureName),
        position - d,
        new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height),
        tint,
        0,
        Vector2.Zero,
        scale,
        SpriteEffects.None,
        0
       );
    }

    public void DrawLineS(Vector2 from, Vector2 to, Color color)
    {
      var v = to - from;
      var length = v.Length();
      var rect = new Rectangle(0, 0, (int)length, 1);

      spriteBatch.Draw(
        core.SpriteManager.OnePixel, 
        from, 
        rect, 
        color, 
        (float)Math.Atan(v.Y / v.X), 
        Vector2.Zero, 
        1.0f, 
        SpriteEffects.None, 
        0
      );
    }

    public void DrawRectangleS(Vector2 position, float width, float height, Color color)
    {
      spriteBatch.Draw(
        core.SpriteManager.OnePixel, 
        position, 
        new Rectangle(0, 0, (int)width, (int)height), 
        color, 
        0, 
        Vector2.Zero, 
        1.0f, 
        SpriteEffects.None, 
        0
      );
    }

    #endregion
  }
}
