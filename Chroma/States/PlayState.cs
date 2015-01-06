using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Actors;
using Chroma.Messages;
using Chroma.Graphics;
using Chroma.Gui;

namespace Chroma.States
{
  public class PlayState : State
  {
    public ActorManager ActorManager { get; private set; }
    private PlayerActor player;

    private float groundScroll;
    private int groundLevel;

    private HealthGui health;
    private JoystickGui joystick;

    public PlayState(Core core) : base(core)
    {
      core.MessageManager.Subscribe(MessageType.AddActor, this);
      core.MessageManager.Subscribe(MessageType.RemoveActor, this);

      groundScroll = 0;

      groundLevel = 85;

      player = new PlayerActor(core, new Vector2(25, groundLevel - 21));

      ActorManager = new ActorManager(core);
      ActorManager.Add(player);

      //for (var i = 0; i < 100; ++i)
      //{
      //  var y = (float)(10 * Math.Sin(i * 0.5));
      //  ActorManager.Add(new CoinActor(core, new Vector2(50 + i * 10, 25 + y)));
      // }

      //ActorManager.Add(new GolemActor(core, new Vector2(300, 27)));

      health = new HealthGui(core);
      joystick = new JoystickGui(core); 
    }

    public override void Load()
    {
      ActorManager.Load();
    }

    public override void Unload()
    {
      ActorManager.Unload();
    }

    private void DrawTrees()
    {
      var trees = core.SpriteManager.GetSprite("trees_l1");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.1f) % trees.Width, 31), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l2");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.2f) % trees.Width, 31), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l3");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.5f) % trees.Width, 17), Color.White);   
      }
      trees = core.SpriteManager.GetSprite("trees_l4");
      for (var i = 0; i <= 2; i++)
      {
        core.Renderer.DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * 0.7f) % trees.Width, 0), Color.White);   
      }

      var offset = 120f; 
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

    private void DrawGround()
    {
      var ground = core.SpriteManager.GetSprite("ground");
      var earth = core.SpriteManager.GetSprite("earth");
      var floor = core.SpriteManager.GetSprite("floor");

      for (var i = 0; i <= (core.Renderer.ScreenWidth / earth.Width) + 1; ++i)
      {
        core.Renderer.DrawSpriteS(earth, new Vector2(earth.Width * i - groundScroll % earth.Width, groundLevel + 8), Color.White);   
      }

      for (var i = 0; i <= (core.Renderer.ScreenWidth / floor.Width) + 1; ++i)
      {
        core.Renderer.DrawSpriteS(floor, new Vector2(floor.Width * i - groundScroll % floor.Width, groundLevel - 9), Color.White);        
      }

      for (var i = 0; i <= (core.Renderer.ScreenWidth / ground.Width) + 1; ++i)
      {
        core.Renderer.DrawSpriteS(ground, new Vector2(ground.Width * i - groundScroll % ground.Width, groundLevel + 3), Color.White);        
      }
    }

    private void DrawFgGrass()
    {
      var grass = core.SpriteManager.GetSprite("floor_grass");

      for (var i = 0; i <= (core.Renderer.ScreenWidth / grass.Width) + 1; ++i)
      {
        core.Renderer.DrawSpriteS(grass, new Vector2(grass.Width * i - groundScroll % grass.Width + 7, groundLevel - 3), Color.White);        
      }
    }

    public override void Update(int ticks)
    {
      if (core.GetRandom(0, 150) == 0)
      {
        ActorManager.Add(new SlimeWalkActor(core, new Vector2(player.Position.X + core.Renderer.ScreenWidth, groundLevel - 53)));
      }

//      if (core.GetRandom(0, 50) == 0)
//      {
//        ActorManager.Add(new SpikeActor(core, new Vector2(player.Position.X + 250, groundLevel - 28), core.GetRandom(1, 4)));
//      }

      groundScroll += 1.0f;

      ActorManager.Update(ticks);

      core.Renderer.World = new Vector2(-player.Position.X + 10, 0);

      health.Update(ticks);
      joystick.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawRectangleS(
        new Vector2(0, 0),
        core.Renderer.ScreenWidth + 1,
        groundLevel,
        new Color(17, 22, 42)
      );
      core.Renderer.DrawRectangleS(
        new Vector2(0, groundLevel),
        core.Renderer.ScreenWidth + 1,
        core.Renderer.ScreenHeight - groundLevel + 1,
        new Color(16, 19, 17)
      );

      DrawTrees();
      DrawGround();

      ActorManager.Draw();

      DrawFgGrass();

      //health.Draw();
      //joystick.Draw();

      base.Draw();
    }

    private void HandleInput()
    {
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
