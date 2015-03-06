using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Helpers;

namespace Chroma.Graphics
{
  public class ParallaxLayer
  {
    private Core core;
    private float parallax;
    private int y;

    private string[] tape;
    private int tapePos;

    private List<Sprite> onScreen;
    private int onScreenWidth = 0;
    private Vector2 screenPos;
    private int dX = 0;

    public ParallaxLayer(Core core, int y,  float parallax)
    {
      this.core = core;
      this.parallax = parallax;
      this.y = y;

      onScreen = new List<Sprite>();
      screenPos = new Vector2(0, y);
    }

    public void LoadTape(string[] newTape)
    {
      tape = newTape;
      tapePos = 0;
    }

    public void Update()
    {
      // Remove from the left
      if (onScreen.Count > 0 && screenPos.X + onScreen[0].Width < -10)
      {
        dX += onScreen[0].Width;
        onScreenWidth -= onScreen[0].Width;
        onScreen.RemoveAt(0);
      }

      screenPos.X = dX + core.Renderer.World.X * parallax;

      // Add to the right
      if (onScreenWidth + screenPos.X < core.Renderer.ScreenWidth + 100)
      {
        var newSprite = core.SpriteManager.GetSprite(tape[tapePos]);
        onScreen.Add(newSprite);
        onScreenWidth += newSprite.Width;

        tapePos = ScienceHelper.GetRandom(0, tape.Length - 1);
      }
    }

    public void Draw()
    {
      var x = screenPos.X - (core.deviceTilt + 1.5f) * 20 * parallax;
      foreach (var s in onScreen)
      {
        core.Renderer["bg"].DrawSpriteS(s, new Vector2(x, y));
        x += s.Width;
      }
    }

  }
}

