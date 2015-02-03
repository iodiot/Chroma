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

    private PlayerActor player;

    private float groundScroll;

    private int toNextGolem = -1;
    private int toNextSlime = -1;

    private int currentX, currentY;

    public PlayState(Core core) : base(core)
    {
      core.MessageManager.Subscribe(MessageType.AddActor, this);
      core.MessageManager.Subscribe(MessageType.RemoveActor, this);

      groundScroll = 0;

      ActorManager = new ActorManager(core);

      player = new PlayerActor(core, new Vector2(25, 0));
      ActorManager.Add(player);

      AddPlatform(0, 100, 500);

      gameControls = new GameHudGui(core, this, player);
    }

    private void AddPlatform(int dx, int dy, int width)
    {
      currentX += dx;
      currentY += dy;

      var newPlatform = new PlatformActor(core, new Vector2(currentX, currentY), width);
      ActorManager.Add(newPlatform);

      currentX += width;
    }

    private void AddSlope(int sections){
      bool down = core.ChanceRoll(0.5f);

      var newSlope = new SlopedPlatformActor(
        core, new Vector2(currentX, currentY), 
        down ? SlopeDirection.Down : SlopeDirection.Up, sections
      );
      ActorManager.Add(newSlope);

      currentX += newSlope.width;
      currentY += (down ? 1 : -1) * sections * 16;
    }

    private void UpdatePlatform()
    {
      if (currentX - player.X < 500)
      {

        if (core.ChanceRoll(0.5f))
        {
          AddSlope(3);
          return;
        }

        AddPlatform(0, 0, 100);
      }
    }

    public override void Load()
    {
      ActorManager.Initialize();
    }

    public override void Unload()
    {
      ActorManager.Uninitialize();
    }

    private void DrawTrees()
    {
      var trees = core.SpriteManager.GetSprite("trees_l1");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.1f) % trees.Width, 31), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l2");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.2f) % trees.Width, 31), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l3");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.5f) % trees.Width, 17), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l4");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer["bg"].DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.7f) % trees.Width, 0), Color.White);   
      }

     
      var offset = 60f; 
      trees = core.SpriteManager.GetSprite("sun_ray_3");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer["bg_add"].DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 0.80f) % (offset + trees.Width), 0), Color.White); 
      }
      offset = 90; 
      trees = core.SpriteManager.GetSprite("sun_ray_2");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer["bg_add"].DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 0.70f) % (offset + trees.Width), 0), Color.White); 
      }
      offset = 75; 
      trees = core.SpriteManager.GetSprite("sun_ray_1");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer["bg_add"].DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 0.90f) % (offset + trees.Width), 0), Color.White); 
      }


      offset = 120; 
      trees = core.SpriteManager.GetSprite("trees_l5_1");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 0.85f) % (offset + trees.Width), 0), Color.White); 
      }

      offset = 130f; 
      trees = core.SpriteManager.GetSprite("trees_l5_1");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 0.9f) % (offset + trees.Width), 0), Color.White); 
      }

      offset = 90;
      trees = core.SpriteManager.GetSprite("trees_l6");
      for (var i = 0; i <= 5; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2((trees.Width + offset) * i - (groundScroll * .925f) % (trees.Width + offset), -5), Color.White); 
      }

      offset = 140; 
      trees = core.SpriteManager.GetSprite("trees_l5_3");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 0.95f) % (offset + trees.Width), 0), Color.White); 
      }

      offset = 160; 
      trees = core.SpriteManager.GetSprite("trees_l5_4");
      for (var i = 0; i <= 3; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(
          (offset + trees.Width) * i - (groundScroll * 1.0f) % (offset + trees.Width), 0), Color.White); 
      } 

      offset = 90;
      trees = core.SpriteManager.GetSprite("trees_l6");
      for (var i = 0; i <= 5; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2((trees.Width + offset) * i - (groundScroll * 1.1f) % (trees.Width + offset), 0), Color.White); 
      }
    }

    public override void Update(int ticks)
    {

      #region Enemy spawning
      if (toNextGolem < 0)
        toNextGolem = core.GetRandom(80, 200);
      if (toNextSlime < 0)
        toNextSlime = core.GetRandom(200, 600);

      if (toNextSlime == 0)
      {
        ActorManager.Add(new SlimeWalkActor(core, new Vector2(player.Position.X + core.Renderer.ScreenWidth, player.Position.Y - 100),
          MagicManager.GetRandomColor(core, 0.2f)));
      }
      if (toNextGolem == 0)
      {
        ActorManager.Add(new GolemActor(core, new Vector2(player.Position.X + core.Renderer.ScreenWidth, player.Position.Y - 100),
          (MagicManager.GetRandomColor(core, 0.2f))));
       
      }

      toNextGolem--;
      toNextSlime--;
      #endregion

      groundScroll += 1.0f;

      ActorManager.Update(ticks);
      UpdatePlatform();

//      if (core.GetRandom(0, 150) == 0)
//      {
//        ActorManager.Add(new SlimeWalkActor(core, new Vector2(player.Position.X + core.Renderer.ScreenWidth, player.Position.Y - 100), MagicColor.Red));
//      }
//
      groundScroll = player.Position.X;

      ActorManager.Update(ticks);

      float targetWorldY = (core.Renderer.ScreenHeight - 120) * 0.9f - (player.platformY + player.Position.Y) / 2;
      float currentWorldY = core.Renderer.World.Y;
      core.Renderer.World = new Vector2(
        25 - player.Position.X, 
        currentWorldY + (targetWorldY - currentWorldY) * 0.05f
       );
      core.Renderer.World.Y = Math.Max(core.Renderer.World.Y, 10 - player.Position.Y);
      core.Renderer.World.Y = Math.Min(core.Renderer.World.Y, core.Renderer.ScreenHeight - 120 - player.Position.Y);

      gameControls.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer["bg"].DrawRectangleS(
        new Rectangle(0, 0, (int)core.Renderer.ScreenWidth + 1, 85),
        new Color(17, 22, 42)
      );
      core.Renderer["bg"].DrawRectangleS(
        new Rectangle(0, 85, (int)core.Renderer.ScreenWidth + 1, (int)core.Renderer.ScreenHeight - 85 + 1),
        new Color(16, 19, 17)
      );

      DrawTrees();

      ActorManager.Draw();

      gameControls.Draw();

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
