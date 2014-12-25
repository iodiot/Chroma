﻿using System;
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

      groundLevel = core.SpriteManager.GetSprite("trees_l1").Height;

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
      var speed = 0.1f;

      for (var n = 4; n >=1; --n)
      {
        var trees = core.SpriteManager.GetSprite(String.Format("trees_l{0}", n));

        for (var i = 0; i < 5; ++i)
        {
          core.Renderer.DrawSpriteS(trees, new Vector2(trees.Width * i - (groundScroll * speed) % trees.Width, 0), Color.White);   
        }

        speed += 0.1f;
      }
    }

    private void DrawGround()
    {
      var ground = core.SpriteManager.GetSprite("ground");
      var earth = core.SpriteManager.GetSprite("earth");
      var floor = core.SpriteManager.GetSprite("floor");

      for (var i = 0; i < 10; ++i)
      {
        core.Renderer.DrawSpriteS(earth, new Vector2(earth.Width * i - groundScroll % earth.Width, groundLevel + 8), Color.White);   
      }

      for (var i = 0; i < 15; ++i)
      {
        core.Renderer.DrawSpriteS(floor, new Vector2(floor.Width * i - groundScroll % floor.Width, groundLevel - 9), Color.White);        
      }

      for (var i = 0; i < 15; ++i)
      {
        core.Renderer.DrawSpriteS(ground, new Vector2(ground.Width * i - groundScroll % ground.Width, groundLevel + 3), Color.White);        
      }
    }

    private void DrawGrass()
    {      
      var grass = core.SpriteManager.GetSprite("grass");

      for (var i = 0; i < 10; ++i)
      {
        core.Renderer.DrawSpriteS(grass, new Vector2(grass.Width * i - (groundScroll * 0.75f) % grass.Width, groundLevel - grass.Height - 3), Color.White);   
      }
    }

    private void DrawFgGrass()
    {
      var grass = core.SpriteManager.GetSprite("floor_grass");

      for (var i = 0; i < 20; ++i)
      {
        core.Renderer.DrawSpriteS(grass, new Vector2(grass.Width * i - groundScroll % grass.Width + 7, groundLevel - 3), Color.White);        
      }
    }

    public override void Update(int ticks)
    {
      if (ticks % core.GetRandom(75, 125) == 0)
      {
        ActorManager.Add(new GolemActor(core, new Vector2(player.Position.X + 250, 27)));
      }

      if (ticks % core.GetRandom(100, 150) == 0)
      {
        ActorManager.Add(new SpikeActor(core, new Vector2(player.Position.X + 250, 50), core.GetRandom(1, 4)));
      }

      groundScroll += 1.0f;

      ActorManager.Update(ticks);

      core.Renderer.World = new Vector2(-player.Position.X + 10, 0);

      health.Update(ticks);
      joystick.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      // fill bottom space of screen
      core.Renderer.DrawRectangleS(
        new Vector2(0, core.Renderer.ScreenHeight * 0.8f),
        core.Renderer.ScreenWidth,
        core.Renderer.ScreenHeight * 0.2f,
        new Color(29, 33, 30)
      );

      DrawTrees();
      DrawGrass();
      DrawGround();

      ActorManager.Draw();

      DrawFgGrass();

      health.Draw();
      joystick.Draw();

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
