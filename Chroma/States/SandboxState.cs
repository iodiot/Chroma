using System;
using Microsoft.Xna.Framework;
using Chroma.Actors;

namespace Chroma.States
{
  public class SandboxState : State
  {
    private readonly ActorManager actorManager;
    private readonly PlayerActor player;

    public SandboxState(Core core) : base(core)
    {
      actorManager = new ActorManager(core);

      player = actorManager.Add(new PlayerActor(core, new Vector2(50, 50))) as PlayerActor;
      actorManager.Add(new FlatPlatformActor(core, new Vector2(0, 100), 100500));
    }

    public override void Update(int ticks)
    {
      actorManager.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      actorManager.Draw();

      base.Draw();
    }
  }
}

