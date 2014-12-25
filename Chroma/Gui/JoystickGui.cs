﻿using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Gui
{
  public class JoystickGui : Gui
  {
    public JoystickGui(Core core) : base(core)
    {
    }

    public override void Update(int ticks)
    {
      HandleInput();

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteS("btn_jump", new Vector2(10, 68), Color.White * 0.5f);
      core.Renderer.DrawSpriteS("pad_base", new Vector2(145, 60), Color.White * 0.5f);
      core.Renderer.DrawSpriteS("gem_red", new Vector2(145, 65), Color.White * 0.5f);
      core.Renderer.DrawSpriteS("gem_yellow", new Vector2(190, 65), Color.White * 0.5f);
      core.Renderer.DrawSpriteS("gem_blue", new Vector2(170, 100), Color.White * 0.5f);

      base.Draw();
    }

    private void HandleInput()
    {
    }
  }
}

