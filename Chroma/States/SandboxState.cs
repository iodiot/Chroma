using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;

namespace Chroma.States
{
  public class SandboxState : State
  {
    PlatformActor platform;

    public SandboxState(Core core) : base(core)
    {
      platform = new PlatformActor(core, new Vector2(50, 50), 100);
    }

    public override void Update(int ticks)
    {
      platform.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      platform.Draw();

      //var box = platform.GetWorldBoundingBox();
      //core.Renderer.DrawRectangleW(new Vector2(box.X, box.Y), box.Width, box.Height, Color.Red * 0.1f);

      base.Draw();
    }
  }
}

