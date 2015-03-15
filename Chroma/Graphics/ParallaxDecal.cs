using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Helpers;

namespace Chroma.Graphics
{
  public class ParallaxDecal
  {
    private Core core;
    private Sprite sprite;
    public Vector2 screenPos, oldWorld, oldPos;
    private string layer;
    private int zIndex;
    private float parallax;

    public ParallaxDecal(Core core, Sprite sprite, float worldX, float screenY, float parallax, string layer = "bg", int zIndex = 0)
    {
      this.core = core;
      this.sprite = sprite;
      this.layer = layer;
      this.zIndex = zIndex;
      this.parallax = parallax;

      screenPos = new Vector2(worldX + core.Renderer.World.X, screenY);
      oldPos = screenPos;
      oldWorld = core.Renderer.World;
    }

    public virtual void Update()
    {
      var dx = oldWorld.X - core.Renderer.World.X;
      screenPos.X = oldPos.X - dx * parallax;
    }

    public virtual bool IsDead()
    {
      return screenPos.X + sprite.Width < -10;
    }

    public virtual void Draw()
    {
      core.Renderer[layer, zIndex].DrawSpriteS(sprite, screenPos);
    }

  }
}

