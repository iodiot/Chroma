﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Chroma.Graphics
{
  public enum SpriteFlip
  {
    None =        SpriteEffects.None,
    Horizontal =  SpriteEffects.FlipHorizontally,
    Vertical =    SpriteEffects.FlipVertically
  }

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
      public Vector2 Scale;
      public SpriteEffects Flip;
      public int Depth;
    }
    #endregion
      
    #region Blending
    public static readonly BlendState AlphaBlend;
    public static readonly BlendState AdditiveBlend;

    static Renderer()
    {
      AlphaBlend = BlendState.AlphaBlend;

      AdditiveBlend = BlendState.Additive;
      AdditiveBlend.ColorSourceBlend = Blend.One;
    }
    #endregion

    #region Special getters
    public Renderer this[string layer, int depth]
    {
      get {
        SetCurrentLayer(layer);
        SetCurrentDepth(depth);
        return this;
      }
    }

    public Renderer this[string layer]
    {
      get {
        SetCurrentLayer(layer);
        return this;
      }
    }

    public Renderer this[int depth]
    {
      get {
        SetCurrentDepth(depth);
        return this;
      }
    }
    #endregion

    #region Fields
    public Vector2 ScreenCenter { get { return new Vector2(ScreenWidth * 0.5f, ScreenHeight * 0.5f); } }
    public Vector2 World;
    public float WorldYOffset { get; set; }

    public readonly float ScreenWidth;
    public readonly float ScreenHeight;

    private readonly Core core;
    private readonly SpriteBatch spriteBatch;

    private int currentDepth;
    private string currentLayerName;
    private Dictionary<string, Layer> layers;

    private float shakeAmplitude;
    private int shakeTtl;

    private Random random;
    #endregion

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
      AddLayer("add", 1, Renderer.AdditiveBlend);
      AddLayer("fg", 2, Renderer.AlphaBlend);
      AddLayer("fg_add", 3, Renderer.AdditiveBlend);

      SetCurrentLayer(DefaultLayerName);

      SetCurrentDepth(0);
    }

    public void Unload()
    {
    }

    public void SetCurrentDepth(int depth)
    {
      currentDepth = depth;
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
      if (name == "") 
      {
        name = DefaultLayerName;
      }

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

        foreach (var dd in layer.DrawsDesc.OrderBy(dd => dd.Depth))
        {
          spriteBatch.Draw(
            dd.Texture, 
            dd.Position,
            dd.SourceRect,
            dd.Tint,
            dd.Rotation,
            Vector2.Zero,
            dd.Scale,
            dd.Flip,
            0
          );

          ++spritesCounter;
        }

        End();

        ++drawCallsCounter;

        layer.DrawsDesc.Clear();
      }

      core.DebugWatch("sprites/drawcalls", spritesCounter.ToString() + "/" + drawCallsCounter.ToString());
    }

    public void Begin(BlendState blendState)
    {
      var shake = 
        shakeTtl > 0 
          ? Matrix.CreateTranslation(
              (float)random.NextDouble() * shakeAmplitude - shakeAmplitude * 0.5f, 
              (float)random.NextDouble() * shakeAmplitude - shakeAmplitude * 0.5f, 
              0) 
          : Matrix.Identity;
      var scale = Matrix.CreateScale(new Vector3(Settings.ScreenScale, Settings.ScreenScale, 1));

      spriteBatch.Begin(
        SpriteSortMode.Immediate, 
        blendState, 
        SamplerState.PointClamp, 
        DepthStencilState.Default, 
        RasterizerState.CullCounterClockwise, 
        null, 
        scale * shake
      );
    }

    public void End()
    {
      spriteBatch.End();
    }

    #region World draw

    public float DrawTextW(string text, Vector2 position, Color tint, float scale = 1.0f)
    {
      return DrawTextS(text, position + World, tint, scale);
    }

    public void DrawSpriteW(Sprite sprite, Vector2 position, Color? tint = null, 
      Vector2? scale = null, float rotation = 0.0f,
      SpriteFlip flip = SpriteFlip.None)
    {
      DrawSpriteS(sprite, position + World, tint, scale, rotation, flip);
    }

    public void DrawSpriteTiledW(Sprite tile, 
      Rectangle box, 
      bool tileHorizontally = true,
      bool tileVertically = true,
      Vector2? tileOffset = null
    )
    {
      var tileWidth = tile.Width;
      var tileHeight = tile.Height;
      var offset = (Vector2)(tileOffset ?? Vector2.Zero);

      var layer = currentLayerName;
      var depth = currentDepth;

      var offx = Math.Abs(offset.X) % tileWidth;
      if (offset.X < 0)
        offx = tileWidth - offx;

      var offy = Math.Abs(offset.Y) % tileHeight;
      if (offset.Y < 0)
        offy = tileHeight - offy;

      var x = box.Left - offx;
      do
      {
        var y = box.Top - offy;
        do {
          var reducedTile = tile.Reduce(
            (int)Math.Max(box.Left - x, 0),
            (int)Math.Max(box.Top - y, 0),
            (int)Math.Max(x + tileWidth - box.Right, 0),
            (int)Math.Max(y + tileHeight - box.Bottom, 0)
          );

          var pos = new Vector2(
            Math.Max(x, box.Left), 
            Math.Max(y, box.Top)
          );

          core.Renderer[layer, depth].DrawSpriteW(reducedTile, pos);

          y += tileHeight;
        } while (tileVertically && y < box.Bottom);

        x += tileWidth;
      } while (tileHorizontally && x < box.Right);
    }

    public void DrawRectangleW(Vector2 position, float width, float height, Color color)
    {
      DrawRectangleS(position + World, width, height, color);
    }

    public void DrawRectangleW(Rectangle rect, Color color)
    {
      DrawRectangleS(new Vector2(rect.X + World.X, rect.Y + World.Y), rect.Width, rect.Height, color);
    }

    public void DrawLineW(Vector2 from, Vector2 to, Color color)
    {
      DrawLineS(from + World, to + World, color);
    }

    public void DrawSpriteW(string spriteName, Vector2 position, Color? tint = null, 
      Vector2? scale = null, float rotation = 0.0f,
      SpriteFlip flip = SpriteFlip.None)
    {
      DrawSpriteS(core.SpriteManager.GetSprite(spriteName), position + World, tint, scale, rotation, flip);
    }

    #endregion

    #region Screen draw

    public void DrawSpriteS(string spriteName, Vector2 position, Color? tint = null, 
      Vector2? scale = null, float rotation = 0.0f, 
      SpriteFlip flip = SpriteFlip.None)
    {
      var sprite = core.SpriteManager.GetSprite(spriteName);
      DrawSpriteS(sprite, position, tint ?? Color.White, scale, rotation, flip);
    }

    public void DrawSpriteS(Sprite sprite, Vector2 position, Color? tint = null,
      Vector2? scale = null, float rotation = 0.0f, 
      SpriteFlip flip = SpriteFlip.None)
    {
      var offset = sprite.GetOffset();
      if (scale != null)
      {
        offset.X *= ((Vector2)scale).X;
        offset.Y *= ((Vector2)scale).Y;
      }
      InternalDrawSprite(
        core.SpriteManager.GetTexture(sprite.TextureName),
        position + offset,
        new Rectangle(sprite.X, sprite.Y, sprite.SrcWidth, sprite.SrcHeight),
        tint ?? Color.White,
        rotation,
        scale ?? new Vector2(1.0f, 1.0f),
        (SpriteEffects)flip
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
        new Vector2(1.0f, 1.0f),
        SpriteEffects.None
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
        new Vector2(1.0f, 1.0f),
        SpriteEffects.None
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
        new Vector2(1.0f, 1.0f),
        SpriteEffects.None
      );
    }

    public float DrawTextS(string text, Vector2 position, Color tint, float scale = 1.0f)
    {
      const int CharWidth = 6;
      var layer = currentLayerName;
      var depth = currentDepth;

      for (var i = 0; i < text.Length; ++i)
      {
        SetCurrentLayer(layer);
        SetCurrentDepth(depth);
        DrawSpriteS(core.SpriteManager.MakeCharSprite(text[i]), new Vector2(position.X + i * scale * CharWidth, position.Y), tint, new Vector2(scale, scale));
      }

      return text.Length * CharWidth * scale;
    }

    public void FillScreen(Color color)
    {
      core.Renderer.DrawRectangleS(new Vector2(-1), ScreenWidth + 1, ScreenHeight + 1, color);
    }

    #endregion
  
    private void InternalDrawSprite(Texture2D texture, Vector2 position, Rectangle sourceRect, 
      Color tint, float rotation, Vector2 scale, SpriteEffects flip)
    {
      const float D = 1.0f; // delta

      // Draw only sprites that are on screen
      if (position.X < ScreenWidth + D && 
          position.Y < ScreenHeight + D && 
          position.X + sourceRect.Width * scale.X >= -D && 
          position.Y + sourceRect.Height * scale.Y >= -D)
      {    
        layers[currentLayerName].DrawsDesc.Add(new DrawDesc()
          {
            Texture = texture,
            Position = position,
            SourceRect = sourceRect,
            Tint = tint,
            Rotation = rotation,
            Scale = scale,
            Flip = flip,
            Depth = currentDepth
          });
      }

      // Reset layer and depth
      SetCurrentLayer(DefaultLayerName);
      SetCurrentDepth(0);
    }
  }
}
