using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;

namespace Chroma.Gui
{
  public class JoystickGui : Gui
  {
    private readonly Sprite jumpButton, jumpButtonPressed;

    public JoystickGui(Core core) : base(core)
    {
      jumpButton = core.SpriteManager.GetSprite("btn_jump");
      jumpButtonPressed = core.SpriteManager.GetSprite("btn_jump_pressed");
    }

    public override void Update(int ticks)
    {
      HandleInput();

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteS(jumpButton, new Vector2(10, 68), Color.White * 0.5f);

      base.Draw();
    }

    private void HandleInput()
    {
    }
  }
}

