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

  public class GameStats
  {
    public readonly int distance;

    public GameStats(int distance) {
      this.distance = distance;
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
    private SubState substate;

    public ActorManager ActorManager { get; private set; }
    private Area area;

    public GameHudGui GameControls { get; private set; }
    public HealthGui HealthGui { get; set; }

    private LevelGenerator LevelGenerator;
    private PlayerActor player;
    private float levelDistance = 0;

    public PlayState(Core core, Area area) : base(core)
    {
      this.area = area;

      core.MessageManager.Subscribe(MessageType.AddActor, this);
      core.MessageManager.Subscribe(MessageType.RemoveActor, this);

      ActorManager = new ActorManager(core);
      LevelGenerator = new LevelGenerator(core, ActorManager, this.area);
    }

    public override void Load()
    {
      substate = SubState.Beginning;
      ActorManager.Load();
      core.Renderer.World = new Vector2(0, (core.Renderer.ScreenHeight - 130));
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
        levelDistance = player.Position.X;

        #region Positioning camera
        core.Renderer.WorldYOffset = (player.PlatformY + player.Position.Y) / 2;
        var targetWorldY = (core.Renderer.ScreenHeight - 130) * 0.9f - core.Renderer.WorldYOffset;
        var currentWorldY = core.Renderer.World.Y;

        core.Renderer.World = new Vector2(
          25 - player.Position.X, 
          currentWorldY + (targetWorldY - currentWorldY) * 0.05f
        );
        core.Renderer.World.Y = Math.Max(core.Renderer.World.Y, 10 - player.Position.Y);
        core.Renderer.World.Y = Math.Min(core.Renderer.World.Y, core.Renderer.ScreenHeight - 120 - player.Position.Y);
        #endregion

        GameControls.Update(ticks);
        HealthGui.Update(ticks);

        if (player.HasLost)
          GameOver();
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
      core.GraphicsDevice.Clear(new Color(17, 22, 42));

      LevelGenerator.DrawBackground();
      ActorManager.Draw();

      if (substate == SubState.Playing)
      {
        GameControls.Draw();
        HealthGui.Draw();
      }

      if (Settings.DrawDebugMessages)
      {
        core.Renderer.DrawTextS(
          String.Format("{0} m", LevelGenerator.distanceMeters),
          new Vector2(60, 5),
          Color.White * 0.25f
        );
      }

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

      player = new PlayerActor(core, new Vector2(25, -50));
      ActorManager.Add(player);

      GameControls = new GameHudGui(core, this, player);
      HealthGui = new HealthGui(core, player);
    }

    public void GameOver()
    {
      substate = SubState.GameOver;
      core.gameResult = new GameStats(LevelGenerator.distanceMeters); 
      core.MessageManager.Send(new CoreEventMessage(CoreEvent.GameOver), this);
    }
  }
}
