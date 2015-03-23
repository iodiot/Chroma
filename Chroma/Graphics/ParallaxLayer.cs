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
    private int zIndex;

    private string[] tape;
    private int tapePos;

    private List<Sprite> onScreen;
    private int onScreenWidth = 0;
    private Vector2 screenPos;
    private int dX = 0;

    private Color extendColor = Color.Black;
    private int extendHeight = 0;

    public ParallaxLayer(Core core, int y,  float parallax, int zIndex = 0, Color extendColor = default(Color), int extendHeight = 0)
    {
      this.core = core;
      this.parallax = parallax;
      this.y = y;
      this.zIndex = zIndex;
      this.extendColor = extendColor;
      this.extendHeight = extendHeight;

      onScreen = new List<Sprite>();
      screenPos = new Vector2(0, y);
    }

    public void LoadTape(string[] newTape)
    {
      tape = newTape;
      tapePos = 0;

      Update(); // Prevent flickering in the first tick
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
      while (tape.Length > 0 && onScreenWidth + screenPos.X < core.Renderer.ScreenWidth + 100)
      {
        var newSprite = core.SpriteManager.GetSprite(tape[tapePos]);
        onScreen.Add(newSprite);
        onScreenWidth += newSprite.Width;

        tapePos = SciHelper.GetRandom(0, tape.Length - 1);
      }
    }

    public void Draw()
    {
      var x = screenPos.X; // - (core.deviceTilt + 1.5f) * 20 * parallax;
      foreach (var s in onScreen)
      {
        core.Renderer["bg", zIndex].DrawSpriteS(s, new Vector2(x, y));
        if (extendHeight > 0)
        {
          core.Renderer["bg", zIndex].DrawRectangleS(new Rectangle().FromFloats(x, y - extendHeight, s.Width, extendHeight), extendColor);
          core.Renderer["bg", zIndex].DrawRectangleS(new Rectangle().FromFloats(x, y + s.Height, s.Width, extendHeight), extendColor);
        }
        x += s.Width;
      }
    }

  }
}

