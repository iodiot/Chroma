using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Actors;
using Chroma.Messages;
using Chroma.Graphics;
using Chroma.Gui;
using Chroma.Gameplay;

namespace Chroma.States
{
  public class GameResult
  {
    public readonly int Distance;

    public GameResult(int distance)
    {
      this.Distance = distance;
    }
  }

  public class PlayState : State
  {
    private enum SubState
    {
      Beginning,
      Playing,
      GameOver
    }

    public ActorManager ActorManager { get; private set; }
    public GameHudGui GameControls { get; private set; }
    public HealthGui HealthGui { get; private set; }
    public LevelGenerator LevelGenerator { get; private set; }
    public PlayerActor Player { get; private set; }

    private SubState substate;
    private Area area;
    private float levelDistance = 0;

    public PlayState(Core core, Area area) : base(core)
    {
      this.area = area;

      core.MessageManager.Subscribe(MessageType.AddActor, this);
      core.MessageManager.Subscribe(MessageType.RemoveActor, this);
    }

    public override void Load()
    {
      core.Renderer.World = new Vector2(0, (core.Renderer.ScreenHeight - 130) * 0.9f + 50/2);

      ActorManager = new ActorManager(core, this);
      ActorManager.Load();

      LevelGenerator = LevelGenerator.Create(core, ActorManager, this.area);

      substate = SubState.Beginning;
    }

    public override void Unload()
    {
      ActorManager.Unload();
    }

    public override void Update(int ticks)
    {
      ActorManager.Update(ticks);
      LevelGenerator.Update(levelDistance);

      if (substate == SubState.Playing)
      {
        levelDistance = Player.Position.X;

        core.DebugWatch("platform Y", Player.PlatformY.ToString());

        #region Positioning camera
        if (!Player.HasLost) {
          var targetPlatformScreenPos = (core.Renderer.ScreenHeight - 120) * 0.9f;
          var targetWorldY = targetPlatformScreenPos - (Player.PlatformY + Player.Position.Y) / 2;
          var currentWorldY = core.Renderer.World.Y;
            
          //var targetWorldYOffset = player.PlatformY - (targetPlatformScreenPos - currentWorldY);
          core.Renderer.WorldYOffset = 0;// += (targetWorldYOffset - core.Renderer.WorldYOffset) * 0.02f;

          core.Renderer.World = new Vector2(
            25 - Player.Position.X, 
            currentWorldY + (targetWorldY - currentWorldY) * 0.05f
          );
          core.Renderer.World.Y = Math.Max(core.Renderer.World.Y, 10 - Player.Position.Y);
          core.Renderer.World.Y = Math.Min(core.Renderer.World.Y, core.Renderer.ScreenHeight - 120 - Player.Position.Y);
        }
        #endregion

        GameControls.Update(ticks);
        HealthGui.Update(ticks);

        if (Player.HasLost)
        {
          GameOver();
        }
      }
        
      base.Update(ticks);
    }

    public override void HandleInput()
    {

      GameControls.HandleInput();
      base.HandleInput();
    }

    public override void Draw()
    {
      LevelGenerator.DrawBackground();
      ActorManager.Draw();

      if (substate == SubState.Playing)
      {
        GameControls.Draw();
        HealthGui.Draw();
      }

      core.DebugWatch("distance", LevelGenerator.distanceMeters.ToString() + " m");

      base.Draw();
    }

    public override void OnMessage(Message message, object sender)
    {
      switch (message.Type)
      {
        case MessageType.AddActor:
          ActorManager.Add((message as AddActorMessage).Actor);
          break;
        case MessageType.RemoveActor:
          ActorManager.Remove((message as RemoveActorMessage).Actor);
          break;
      }

      base.OnMessage(message, sender);
    }

    // ---------------------------------------

    public void Start()
    {
      substate = SubState.Playing;
      LevelGenerator.Go();

      Player = new PlayerActor(core, new Vector2(25, -27));
      ActorManager.Add(Player);

      GameControls = new GameHudGui(core, this, Player);
      HealthGui = new HealthGui(core, Player);
    }

    public void GameOver()
    {
      substate = SubState.GameOver;
      core.GameResult = new GameResult(LevelGenerator.distanceMeters); 
      core.MessageManager.Send(new CoreEventMessage(CoreEvent.GameOver), this);
    }
  }
}
