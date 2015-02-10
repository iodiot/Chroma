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
  public class PlayState : State
  {
    public ActorManager ActorManager { get; private set; }

    public GameHudGui gameControls { get; private set; }

    private LevelGenerator LevelGenerator;
    private PlayerActor player;
    private float levelDistance = 0;

    public PlayState(Core core) : base(core)
    {
      core.MessageManager.Subscribe(MessageType.AddActor, this);
      core.MessageManager.Subscribe(MessageType.RemoveActor, this);

      ActorManager = new ActorManager(core);
      LevelGenerator = new LevelGenerator(core, ActorManager);

      player = new PlayerActor(core, new Vector2(25, -50));
      ActorManager.Add(player);

      gameControls = new GameHudGui(core, this, player);
    }

    public override void Load()
    {
      ActorManager.Load();
    }

    public override void Unload()
    {
      ActorManager.Unload();
    }

    private void DrawBackground()
    {
      core.Renderer["bg"].DrawRectangleS(
        new Rectangle(0, 0, (int)core.Renderer.ScreenWidth + 1, (int)core.Renderer.ScreenHeight + 1),
        new Color(17, 22, 42)
      );

      #region Trees, temporary
      var trees = core.SpriteManager.GetSprite("trees_l1");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (levelDistance * 0.1f) % trees.Width, 31), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l2");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (levelDistance * 0.2f) % trees.Width, 31), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l3");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (levelDistance * 0.5f) % trees.Width, 17), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l4");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (levelDistance * 0.7f) % trees.Width, 0), Color.White);   
      }

     
//      var offset = 60f; 
//      trees = core.SpriteManager.GetSprite("sun_ray_3");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer["bg_add"].DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 0.80f) % (offset + trees.Width), 0), Color.White); 
//      }
//      offset = 90; 
//      trees = core.SpriteManager.GetSprite("sun_ray_2");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer["bg_add"].DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 0.70f) % (offset + trees.Width), 0), Color.White); 
//      }
//      offset = 75; 
//      trees = core.SpriteManager.GetSprite("sun_ray_1");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer["bg_add"].DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 0.90f) % (offset + trees.Width), 0), Color.White); 
//      }
//
//
//      offset = 120; 
//      trees = core.SpriteManager.GetSprite("trees_l5_1");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer.DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 0.85f) % (offset + trees.Width), 0), Color.White); 
//      }
//
//      offset = 130f; 
//      trees = core.SpriteManager.GetSprite("trees_l5_1");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer.DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 0.9f) % (offset + trees.Width), 0), Color.White); 
//      }
//
//      offset = 90;
//      trees = core.SpriteManager.GetSprite("trees_l6");
//      for (var i = 0; i <= 5; i++)
//      {
//        core.Renderer.DrawSpriteS(trees, new Vector2((trees.Width + offset) * i - (levelDistance * .925f) % (trees.Width + offset), -5), Color.White); 
//      }
//
//      offset = 140; 
//      trees = core.SpriteManager.GetSprite("trees_l5_3");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer.DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 0.95f) % (offset + trees.Width), 0), Color.White); 
//      }
//
//      offset = 160; 
//      trees = core.SpriteManager.GetSprite("trees_l5_4");
//      for (var i = 0; i <= 3; i++)
//      {
//        core.Renderer.DrawSpriteS(trees, new Vector2(
//          (offset + trees.Width) * i - (levelDistance * 1.0f) % (offset + trees.Width), 0), Color.White); 
//      } 
//
//      offset = 90;
//      trees = core.SpriteManager.GetSprite("trees_l6");
//      for (var i = 0; i <= 5; i++)
//      {
//        core.Renderer.DrawSpriteS(trees, new Vector2((trees.Width + offset) * i - (levelDistance * 1.1f) % (trees.Width + offset), 0), Color.White); 
//      }
      #endregion
    }

    public override void Update(int ticks)
    {
      ActorManager.Update(ticks);
      levelDistance = player.Position.X;
      LevelGenerator.Update(levelDistance);

      #region Positioning camera
      var currentWorldY = core.Renderer.World.Y;
      var targetWorldY = (core.Renderer.ScreenHeight - 130) * 0.9f - (player.platformY + player.Position.Y) / 2;
      core.Renderer.World = new Vector2(
        25 - player.Position.X, 
        currentWorldY + (targetWorldY - currentWorldY) * 0.05f
       );
      core.Renderer.World.Y = Math.Max(core.Renderer.World.Y, 10 - player.Position.Y);
      core.Renderer.World.Y = Math.Min(core.Renderer.World.Y, core.Renderer.ScreenHeight - 120 - player.Position.Y);
      #endregion

      gameControls.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      DrawBackground();
      ActorManager.Draw();
      gameControls.Draw();

      core.Renderer.DrawTextS(
        String.Format("distance: {0} m", LevelGenerator.distanceMeters),
        new Vector2(10, 12),
        Color.White * 0.25f
      );

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
  }
}
