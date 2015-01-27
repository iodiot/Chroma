using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;

namespace Chroma.States
{
  public class SandboxState : State
  {
    SpriteDestroyerActor actor;

    public SandboxState(Core core) : base(core)
    {
      actor = new SpriteDestroyerActor(core, new Vector2(50, 50), core.SpriteManager.GetSprite("druid_fall_1"));
    }

    public override void Update(int ticks)
    {
      actor.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      actor.Draw();

      base.Draw();
    }
  }
}

