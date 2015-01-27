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

  public sealed class Renderer
  {
    private const string DefaultLayerName = "default";

    #region Class definitions

    private class Layer
    {
      public int Z;
      public BlendState Blend;
      public List<DrawDesc> DrawsDesc;
    }

    private class DrawDesc
    {
      public Texture2D Texture;
      public Vector2 Position;
      public Rectangle SourceRect;
      public Color Tint;
      public float Rotation;
      public float Scale;
    }

    #endregion
      
    // blending
    public static readonly BlendState AlphaBlend;
    public static readonly BlendState AdditiveBlend;

    public Renderer this[string key]
    {
      get {
        SetCurrentLayer(key);
        return this;
      }
    }

    public Vector2 ScreenCenter { get { return new Vector2(ScreenWidth * 0.5f, ScreenHeight * 0.5f); } }
    public Vector2 World;

    // layers
    private string currentLayerName;
    private Dictionary<string, Layer> layers;

    public readonly float ScreenWidth;
    public readonly float ScreenHeight;

    private readonly Core core;
    private readonly SpriteBatch spriteBatch;

    private float shakeAmplitude;
    private int shakeTtl;

    private Random random;

    static Renderer()
    {
      AlphaBlend = BlendState.AlphaBlend;

      AdditiveBlend = BlendState.Additive;
      AdditiveBlend.ColorSourceBlend = Blend.One;
    }

    public Renderer(Core core, SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
      this.core = core;
      this.spriteBatch = spriteBatch;

      layers = new Dictionary<string, Layer>();

      ScreenWidth = screenWidth;
      ScreenHeight = screenHeight;

      World = Vector2.Zero;

      random = new Random();
    }

    public void Load()
    {
      AddLayer("bg", -2, Renderer.AlphaBlend);
      AddLayer("bg_add", -1, Renderer.AdditiveBlend);
      AddLayer(DefaultLayerName, 0, Renderer.AlphaBlend);
      AddLayer("fg_add", 1, Renderer.AdditiveBlend);
      AddLayer("fg", 2, Renderer.AlphaBlend);

      SetCurrentLayer(DefaultLayerName);
    }

    public void Unload()
    {
    }

    #region Layers

    public void AddLayer(string name, int z, BlendState blend)
    {
      layers.Add(name, new Layer() { Z = z, Blend = blend, DrawsDesc = new List<DrawDesc>() });

      // sort by z index
      layers = layers.OrderBy(x => x.Value.Z).ToDictionary(x => x.Key, x => x.Value);
    }

    public void RemoveLayer(string name)
    {
      Debug.Assert(layers.ContainsKey(name), String.Format("Renderer.RemoveLayer() : Layer {0} is missing", name));

      layers.Remove(name);
    }

    public void ClearLayers()
    {
      layers.Clear();
    }
      
    private void SetCurrentLayer(string name)
    {
      Debug.Assert(layers.ContainsKey(name), String.Format("Renderer.SetCurrentLayer() : Layer {0} is missing", name));

      currentLayerName = name;
    }

    #endregion

    #region Effects

    public void ShakeScreen(float amplitude, int duration)
    {
      shakeAmplitude = amplitude;
      shakeTtl = duration;
    }

    #endregion

    public void Update(int ticks)
    {
      if (shakeTtl > 0)
      {
        --shakeTtl;
      }
    }

    public void Draw()
    {
      // statistics
      var spritesCounter = 0;
      var drawCallsCounter = 0;

      foreach (var layer in layers.Values)
      {
        if (layer.DrawsDesc.Count == 0)
        {
          continue;
        }

        Begin(layer.Blend);

        foreach (var dd in layer.DrawsDesc)
        {
          spriteBatch.Draw(
            dd.Texture, 
            dd.Position,
            dd.SourceRect,
            dd.Tint,
            dd.Rotation,
            Vector2.Zero,
            new Vector2(dd.Scale, dd.Scale),
            SpriteEffects.None,
            0
          );

          ++spritesCounter;
        }

        End();

        ++drawCallsCounter;

        layer.DrawsDesc.Clear();
      }

      //Debug.Print(drawCallsCounter.ToString());
    }

    public void Begin(BlendState blendState)
    {
      var shake = shakeTtl > 0 ? 
        Matrix.CreateTranslation((float)random.NextDouble() * shakeAmplitude - shakeAmplitude * 0.5f, (float)random.NextDouble() * shakeAmplitude - shakeAmplitude * 0.5f, 0) : Matrix.Identity;

      var scale = Matrix.CreateScale(new Vector3(Settings.ScreenScale, Settings.ScreenScale, 1));
      spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, scale * shake);
    }

    public void End()
    {
      spriteBatch.End();
    }

    #region World draw

    public void DrawTextW(string text, Vector2 position, Color tint, float scale = 1.0f)
    {
      DrawTextS(text, position + World, tint, scale);
    }

    public void DrawSpriteW(Sprite sprite, Vector2 position, Color tint, float scale = 1.0f, float rotation = 0.0f)
    {
      DrawSpriteS(sprite, position + World, tint, scale, rotation);
    }

    public void DrawRectangleW(Vector2 position, float width, float height, Color color)
    {
      DrawRectangleS(position + World, width, height, color);
    }

    public void DrawRectangleW(Rectangle rect, Color color)
    {
      DrawRectangleS(new Rectangle((int)(rect.X + World.X), (int)(rect.Y + World.Y), rect.Width, rect.Height), color);
    }

    public void DrawLineW(Vector2 from, Vector2 to, Color color)
    {
      DrawLineS(from + World, to + World, color);
    }

    public void DrawSpriteW(string spriteName, Vector2 position, Color tint, float scale = 1.0f, float rotation = 0.0f)
    {
      DrawSpriteS(core.SpriteManager.GetSprite(spriteName), position + World, tint, scale, rotation);
    }

    #endregion

    #region Screen draw

    public void DrawSpriteS(string spriteName, Vector2 position, Color tint, float scale = 1.0f, float rotation = 0.0f)
    {
      DrawSpriteS(core.SpriteManager.GetSprite(spriteName), position, tint, scale, rotation);
    }

    public void DrawSpriteS(Sprite sprite, Vector2 position, Color tint, float scale = 1.0f, float rotation = 0.0f)
    {
      var d = new Vector2(sprite.AnchorPoint.X * sprite.Width, sprite.AnchorPoint.Y * sprite.Height);

      InternalDrawSprite(
        core.SpriteManager.GetTexture(sprite.TextureName),
        position - d,
        new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height),
        tint,
        rotation,
        scale
      );
    }

    public void DrawLineS(Vector2 from, Vector2 to, Color color)
    {
      var v = to - from;
      var length = v.Length();
      var rect = new Rectangle(0, 0, (int)length, 1);

      InternalDrawSprite(
        core.SpriteManager.OnePixel,
        from,
        rect,
        color,
        (float)Math.Atan(v.Y / v.X), 
        1.0f
      );
    }

    public void DrawRectangleS(Vector2 position, float width, float height, Color color)
    {
      InternalDrawSprite(
        core.SpriteManager.OnePixel,
        position,
        new Rectangle(0, 0, (int)width, (int)height), 
        color, 
        0,
        1.0f
      );
    }

    public void DrawRectangleS(Rectangle rect, Color color)
    {
      InternalDrawSprite(
        core.SpriteManager.OnePixel,
        new Vector2(rect.X, rect.Y),
        new Rectangle(0, 0, rect.Width, rect.Height), 
        color, 
        0,
        1.0f
      );
    }

    public void DrawTextS(string text, Vector2 position, Color tint, float scale = 1.0f)
    {
      const int CharWidth = 6;

      for (var i = 0; i < text.Length; ++i)
      {
        DrawSpriteS(core.SpriteManager.GetFontSprite(text[i]), new Vector2(position.X + i * scale * CharWidth, position.Y), tint, scale);
      }
    }

    public void FillScreen(Color color)
    {
      core.Renderer.DrawRectangleS(Vector2.Zero, ScreenWidth, ScreenHeight, color);
    }

    #endregion
  
    private void InternalDrawSprite(Texture2D texture, Vector2 position, Rectangle sourceRect, Color tint, float rotation, float scale)
    {
      layers[currentLayerName].DrawsDesc.Add(new DrawDesc() {
        Texture = texture,
        Position = position,
        SourceRect = sourceRect,
        Tint = tint,
        Rotation = rotation,
        Scale = scale
      });

      // reset layer
      SetCurrentLayer(DefaultLayerName);
    }
  }
}
