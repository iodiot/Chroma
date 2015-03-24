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

    private bool holdingJump = false;
    private int jumpReserve;
    private int maxJumpReserve;
    private float jumpSpeed;
    private float jumpHeightTolerance;
    private bool jumpAfterLanding = false;

    #region Player stats
    public int MaxHearts { get; private set; }
    public int Hearts { get; private set; }

    public bool HasLost { get; private set; }
    #endregion

    #region Effects
    private int hurtTimeout;
    #endregion

    enum PlayerState {
      Running,
      Jumping,
      Falling,
      Landing
    }
    enum PlayerEvent {
      Jump,
      Fall,
      Land
    }
    //private StateMachine<PlayerState, PlayerEvent> sm;

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

//      sm = new StateMachine<PlayerState, PlayerEvent>();
//      sm.State(PlayerState.Running)
//        .IsInitial()
//        .On(PlayerEvent.Jump, PlayerState.Jumping)
//        .On(PlayerEvent.Fall, PlayerState.Falling);
//      sm.State(PlayerState.Jumping)
//        .On(PlayerEvent.Fall, PlayerState.Falling);
//      sm.State(PlayerState.Falling)
//        .On(PlayerEvent.Land, PlayerState.Landing);
//      sm.State(PlayerState.Landing)
//        .After(10).AutoTransitionTo(PlayerState.Running);
//      sm.Start();

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
//      if (touchingFloor)
//        sm.Trigger(PlayerEvent.Land);
//      else if (Velocity.Y >= 0)
//        sm.Trigger(PlayerEvent.Fall);
//
//      sm.Update(ticks);
//
//      // Initializing body states
//      if (sm.justEnteredState)
//        switch (sm.currentState)
//        {
//          case PlayerState.Running:
//            animation.Play("run");
//            break;
//          case PlayerState.Jumping:
//            animation.Play("jump");
//            break;
//          case PlayerState.Falling:
//            animation.Play("fall");
//            break;
//          case PlayerState.Landing:
//            animation.Play("land");
//            break;
//        }
//
//      // Processing body states
//      switch (sm.currentState)
//      {
//        case PlayerState.Jumping:
//          if (holdingJump)
//            Velocity.Y = -4f;
//          break;
//        case PlayerState.Falling:
//          break;
//      }

      //DEBUG
      core.DebugWatch("Hearts", Hearts.ToString());

      // Jump -------------------------------------------------

      maxJumpReserve = 15;
      jumpSpeed = 3.0f;
      jumpHeightTolerance = 40.0f;

      //DEBUG
      //core.DebugWatch("Jump reserve", jumpReserve.ToString());
      //jumpSpeed = core.LiveTune("Jump speed", jumpSpeed, 0.05f);
      //maxJumpReserve = core.LiveTune("Max jump reserve", maxJumpReserve);
      //jumpHeightTolerance = core.LiveTune("Height tolerance", jumpHeightTolerance, 1f);
      //core.DebugWatch("Jump after landing", jumpAfterLanding.ToString());

      if (IsOnPlatform && (!holdingJump || jumpAfterLanding))
      {
        jumpReserve = maxJumpReserve;
        jumpAfterLanding = false;
      }

      if (holdingJump)
      {
        if (jumpReserve > 0)
        {
          Velocity.Y = -jumpSpeed;
          jumpReserve--;
        }
      }

      // ------------------------------------------------------

      if (hurtTimeout > 0)
      {
        --hurtTimeout;
      }

      animation.Update(ticks);
      armAnimation.Update(ticks);

      if (!Fell && !Drowned)
        Velocity.X = 2.0f;

      base.Update(ticks);

      //core.DebugWatch("player", sm.currentState.ToString());
    }

    public void TryToJump()
    {
      holdingJump = true;

      var platformBelow = core.GetPlayState().ActorManager.FindPlatformUnder(Position);
      if (platformBelow != null) {
        if (platformBelow.GetBoundingBoxW().Top - GetBoundingBoxW().Bottom <= jumpHeightTolerance)
        {
          jumpAfterLanding = true;
        }
      }
    }

    public void StopJump()
    {
      holdingJump = false;
      jumpAfterLanding = false;
      if (!IsOnPlatform)
      {
        jumpReserve = 0;
      }
    }

    public override void Draw()
    {
      var tint = hurtTimeout == 0 ? Color.White : Color.White * (0.5f + 0.5f * (float)Math.Sin(core.Ticks / 2));

      var pos = new Vector2(Position.X, Position.Y);
//      if (sm.currentState == PlayerState.Landing)
//        pos.Y += 5;

      core.Renderer[10].DrawSpriteW(animation.GetCurrentFrame(), pos, tint);

      pos.X += animation.GetCurrentFrame().LinkX - armAnimation.GetCurrentFrame().LinkX;
      pos.Y += animation.GetCurrentFrame().LinkY - armAnimation.GetCurrentFrame().LinkY;
      core.Renderer[10].DrawSpriteW(armAnimation.GetCurrentFrame(), pos, tint);

      if (charging)
      {
//        core.Renderer["fg_add"].DrawSpriteW(core.SpriteManager.GetSprite("glow"), Position - new Vector2(22, 15),
//          MagicManager.MagicColors[chargeColor] * 0.8f);
        core.Renderer["add"].DrawGlowW(pos + new Vector2(6, 8), MagicManager.MagicColors[chargeColor] * 0.8f, 20);
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
      if (chargeColor == 0)
        return;

      core.MessageManager.Send(
        new AddActorMessage(
          new ProjectileActor(core, Position + new Vector2(10, 10), chargeColor, this)
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
        //core.DebugWatch("platform Y", PlatformY.ToString(), 1000000);
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
        //Die();
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

