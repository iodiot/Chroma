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

    public float PlatformY { get; private set; }

    #region Player stats
    public int MaxHearts { get; private set; }
    public int Hearts { get; private set; }
    public bool HasLost { get; private set; }
    #endregion

    #region Effects
    private int hurtTimeout;
    #endregion

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

      AddCollider(new Collider(){ Name = "body", BoundingBox = boundingBox });

      MaxHearts = 3;
      Hearts = 3;

      hurtTimeout = 0;
    }

    public override void Update(int ticks)
    {
      core.DebugWatch("On the platform", IsOnPlatform.ToString());

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

      if (hurtTimeout > 0)
      {
        --hurtTimeout;
      }

      animation.Update(ticks);
      armAnimation.Update(ticks);

      if (!Fell && !Drowned)
        Velocity.X = 2.0f;

      base.Update(ticks);
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

    public override void Draw()
    {
      var tint = hurtTimeout == 0 ? Color.White : Color.White * (0.5f + 0.5f * (float)Math.Sin(core.Ticks / 2));

      var pos = new Vector2(Position.X, Position.Y);
      if (sm.currentState == DruidState.Landing)
        pos.Y += 5;

      core.Renderer[10].DrawSpriteW(animation.GetCurrentFrame(), pos, tint);

      pos.X += animation.GetCurrentFrame().LinkX - armAnimation.GetCurrentFrame().LinkX;
      pos.Y += animation.GetCurrentFrame().LinkY - armAnimation.GetCurrentFrame().LinkY;
      core.Renderer[10].DrawSpriteW(armAnimation.GetCurrentFrame(), pos, tint);

      if (charging)
      {
        core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(22, 15),
          MagicManager.MagicColors[chargeColor] * 0.8f);
      }

      base.Draw();
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
      if (other.CanMove && (hurtTimeout == 0))
      {
        hurtTimeout = 25;
      }

      if (other is PlatformActor)
      {
        PlatformY = other.GetBoundingBoxW().Top;
      }

      base.OnBoundingBoxTrigger(other);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }

    public void PickItem()
    {
      if (Hearts < MaxHearts)
      {
        Hearts++;
      }
    }

    public void Hurt(int strength = 1) 
    {
      // After-shock invincibility
      if (hurtTimeout > 0)
        return;

      hurtTimeout = 30;

      Hearts--;

      if (Hearts == 0)
      {
        Die();
      }
    }

    public override void OnFall()
    {
      HasLost = true;

      base.OnFall();
    }

    public override void OnDrown()
    {
      HasLost = true;

      base.OnDrown();
    }

    private void Die() {
      HasLost = true;
      core.MessageManager.Send(new RemoveActorMessage(this), this);
      core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, animation.GetCurrentFrame())), this);
    }
  }
}

