using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
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
      var touchState = TouchPanel.GetState();

      if (touchState.Count == 1)
      {
        player.TryToJump();
      }

      actorManager.Update(ticks);

      #region Positioning camera
      var targetWorldY = (core.Renderer.ScreenHeight - 120) * 0.9f - (player.PlatformY + player.Position.Y) / 2;
      var currentWorldY = core.Renderer.World.Y;
      core.Renderer.World = new Vector2(
        25 - player.Position.X, 
        currentWorldY + (targetWorldY - currentWorldY) * 0.05f
      );
      core.Renderer.World.Y = Math.Max(core.Renderer.World.Y, 10 - player.Position.Y);
      core.Renderer.World.Y = Math.Min(core.Renderer.World.Y, core.Renderer.ScreenHeight - 120 - player.Position.Y);
      #endregion

      base.Update(ticks);
    }

    public override void Draw()
    {
      actorManager.Draw();

      base.Draw();
    }
  }
}

