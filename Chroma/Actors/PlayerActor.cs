using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.StateMachines;
using Chroma.Gameplay;
using Chroma.States;

namespace Chroma.Actors
{
  public class PlayerActor : Actor
  {
    private readonly Animation animation, armAnimation; 

    public MagicColor chargeColor;
    public bool charging = false;

    private int hurtTtl;
    public float platformY { get; private set; }

    enum DruidState {
      Running,
      Jumping,
      Falling,
      Landing
    }
    enum DruidEvent {
      Jump,
      Fall,
      Land
    }
    private StateMachine<DruidState, DruidEvent> sm;

    public PlayerActor(Core core, Vector2 position) : base(core, position)
    {
      boundingBox = new Rectangle(3, 2, 12, 21);

      animation = new Animation();
      animation.Add("run", core.SpriteManager.GetFrames("druid_run_", new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8 }));
      animation.Add("fall", core.SpriteManager.GetFrames("druid_fall_", new List<int>{ 1, 2 }));
      animation.Add("jump", core.SpriteManager.GetFrames("druid_jump_", new List<int>{ 1, 2 }));
      animation.Add("land", new List<Sprite>{core.SpriteManager.GetSprite("druid_land")});

      armAnimation = new Animation();
      armAnimation.Add("run", core.SpriteManager.GetFrames("arm_run_", new List<int>{ 1, 2, 3, 4, 5, 4, 3, 2 }));

      animation.Play("run");
      armAnimation.Play("run");

      sm = new StateMachine<DruidState, DruidEvent>();
      sm.State(DruidState.Running)
        .IsInitial()
        .On(DruidEvent.Jump, DruidState.Jumping)
        .On(DruidEvent.Fall, DruidState.Falling);
      sm.State(DruidState.Jumping)
        .On(DruidEvent.Fall, DruidState.Falling);
      sm.State(DruidState.Falling)
        .On(DruidEvent.Land, DruidState.Landing);
      sm.State(DruidState.Landing)
        .After(10).AutoTransitionTo(DruidState.Running);
      sm.Start();

      CanMove = true;
      CanFall = true;
      CanLick = true;

      boundingBoxColor = Color.Yellow;
    }

    public override void Update(int ticks)
    {
      sm.Update(ticks);

      if (sm.currentState == DruidState.Running && sm.justEnteredState)
      {
        animation.Play("run");
      }
        
//      if (jumpTtl > 0)
//      {
//        animation.Play("fall");
//        sm.Trigger(DruidEvent.Fall);
//      }
//      else
//      {
//        animation.Play("land");
//        sm.Trigger(DruidEvent.Land);
//      }

      if (hurtTtl > 0)
      {
        --hurtTtl;
      }

      animation.Update(ticks);
      armAnimation.Update(ticks);

      Velocity.X = 2.2f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      var tint = (hurtTtl / 5) % 2 == 0 ? Color.White : Color.Red;

      var pos = new Vector2(Position.X, Position.Y);
      if (sm.currentState == DruidState.Landing)
        pos.Y += 5;

      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), pos, tint);

      if (charging)
      {
        core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(22, 15),
          MagicManager.MagicColors[chargeColor] * 0.8f);
      }

      pos.X += animation.GetCurrentFrame().LinkX - armAnimation.GetCurrentFrame().LinkX;
      pos.Y += animation.GetCurrentFrame().LinkY - armAnimation.GetCurrentFrame().LinkY;
      core.Renderer.DrawSpriteW(armAnimation.GetCurrentFrame(), pos, tint);

      var box = GetBoundingBoxW();
      var point = new Vector2(box.X + box.Width, box.Y + box.Height);
      var platform = (core.GetCurrentState() as PlayState).ActorManager.FindPlatformUnder(point);
      if (platform != null)
      {
        core.Renderer.DrawTextW((platform.Y - point.Y).ToString(), Position + new Vector2(25.0f, -25.0f), Color.White);
      }

      base.Draw();
    }

    public void TryToJump()
    {
      //if (sm.currentState == DruidState.Running)
      {
        Velocity.Y = -3.0f;
        //Velocity.X = 2f;

        //animation.Play("jump");
        //sm.Trigger(DruidEvent.Jump);
      }
    }

    public void Charge(MagicColor first, MagicColor second)
    {
      chargeColor = MagicManager.Mix(first, second);
      charging = true;
    }

    public void Shoot()
    {
      core.MessageManager.Send(
        new AddActorMessage(
          new ProjectileActor(core, Position + new Vector2(10, 10), chargeColor)
        ),
        this
      );

      charging = false;
      chargeColor = 0;
    }

    public override void OnBoundingBoxTrigger(Actor other)
    {
      if (other.CanMove && (hurtTtl == 0))
      {
        hurtTtl = 25;
      }

      if (!other.CanMove)
      {
        platformY = other.GetBoundingBoxW().Top;
      }

      base.OnBoundingBoxTrigger(other);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

