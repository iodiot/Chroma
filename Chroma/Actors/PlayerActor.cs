using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.StateMachines;
using Chroma.Gameplay;

namespace Chroma.Actors
{

  public class PlayerActor : CollidableActor
  {
    private readonly Animation animation, armAnimation; 

    public MagicColor chargeColor;
    public bool charging = false;

    private int jumpTtl;
    private int hurtTtl;

    private float groundLevel;

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
      Handle = "player";
          
      boundingBox = new Rectangle(5, 0, 12, 21);

      animation = new Animation();
      animation.Add("run", core.SpriteManager.GetFrames("druid_run_", new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8 }));
      animation.Add("fall", core.SpriteManager.GetFrames("druid_fall_", new List<int>{ 1, 2 }));
      animation.Add("jump", core.SpriteManager.GetFrames("druid_jump_", new List<int>{ 1, 2 }));
      animation.Add("land", new List<Sprite>{core.SpriteManager.GetSprite("druid_land")});

      armAnimation = new Animation();
      armAnimation.Add("run", core.SpriteManager.GetFrames("arm_run_", new List<int>{ 1, 2, 3, 4, 5, 4, 3, 2 }));

      animation.Play("run");
      armAnimation.Play("run");

      jumpTtl = 0;
      hurtTtl = 0;
      groundLevel = position.Y;

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
    }

    public override void Update(int ticks)
    {
      sm.Update(ticks);

      if (sm.currentState == DruidState.Running && sm.justEnteredState)
      {
        animation.Play("run");
      }

      if (jumpTtl > 0)
      {
        --jumpTtl;
        float oldY = Y;
        Y = groundLevel - 50.0f * (float)Math.Sin((double)jumpTtl / 50.0 * Math.PI);

        if (oldY < Y)
        {
          animation.Play("fall");
          sm.Trigger(DruidEvent.Fall);
        }

        if (jumpTtl == 0)
        {
          animation.Play("land");
          sm.Trigger(DruidEvent.Land);
        }
      }

      if (hurtTtl > 0)
      {
        --hurtTtl;
      }

      animation.Update(ticks);
      armAnimation.Update(ticks);

      X += 1.0f;

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
        core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(22, 15),
          MagicManager.MagicColors[chargeColor] * 0.8f);

      pos.X += animation.GetCurrentFrame().LinkX - armAnimation.GetCurrentFrame().LinkX;
      pos.Y += animation.GetCurrentFrame().LinkY - armAnimation.GetCurrentFrame().LinkY;
      core.Renderer.DrawSpriteW(armAnimation.GetCurrentFrame(), pos, tint);

      //core.Renderer.DrawTextW(sm.currentState.ToString() + animation.GetCurrentFrame().LinkX.ToString(), 
      //  Position + new Vector2(10, -10), Color.White);

      base.Draw();
    }

    public void TryToJump()
    {
      if (sm.currentState == DruidState.Running)
      {
        jumpTtl = 50;
        animation.Play("jump");
        sm.Trigger(DruidEvent.Jump);
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

    public override void OnCollide(CollidableActor other)
    {
      if ((other is CollidableActor) && (hurtTtl == 0))
      {
        hurtTtl = 25;
      }

      base.OnCollide(other);
    }
  }
}

