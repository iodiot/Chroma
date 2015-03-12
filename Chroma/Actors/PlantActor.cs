using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;
using Chroma.Helpers;
using Chroma.StateMachines;

namespace Chroma.Actors
{
  public class PlantActor : Actor
  {
    private MagicColor color;
    private bool isSecondHead;
    private Vector2 headOffset, stemPos, headPos;

    private Sprite bush, leaf;
    private Animation stem, colar, head;
    private bool movedCollider = false;

    private int animOffset;

    enum PlantState {
      Idle,
      Aiming,
      Shooting,
      Dying
    }
    enum PlantEvent {
      Die
    }
    private StateMachine<PlantState, PlantEvent> sm;

    public PlantActor(Core core, Vector2 position, bool isSecondHead = false) : base(core, position)
    {
      this.isSecondHead = isSecondHead;
      this.color = MagicManager.GetRandomColor(except: MagicColor.Green);
      boundingBox = new Rectangle(0, 0, 20, 20);
      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });

      animOffset = ScienceHelper.GetRandom(0, 10000);

      CanMove = false;

      bush = core.SpriteManager.GetSprite(SpriteName.plant_base);
      leaf = core.SpriteManager.GetSprite(SpriteName.plant_front_leaf);
      stemPos = Vector2.Zero;

      stem = new Animation(loop: false);

      if (isSecondHead || ScienceHelper.ChanceRoll())
      {
        stem.Add("idle", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_2)
          });
        stem.Add("aim", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_3)
          });
        stem.Add("shoot", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_2),
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_1)
          });

        stem.Add("die", new List<Sprite> //temp
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_1),
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_2),
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_3),
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_die)
          });
        headOffset = new Vector2(-16, -13);
      }
      else
      {
        stem.Add("idle", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem_2)
          });
        stem.Add("aim", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem_3)
          });
        stem.Add("shoot", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem_2),
            core.SpriteManager.GetSprite(SpriteName.plant_stem_1)
          });
        stem.Add("die", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem_die_1),
            core.SpriteManager.GetSprite(SpriteName.plant_stem_die_2),
            core.SpriteManager.GetSprite(SpriteName.plant_stem_die_3)
          });
        headOffset = new Vector2(-11, -11);

        // Second head
        if (!isSecondHead && ScienceHelper.ChanceRoll())
        {
          core.MessageManager.Send(new AddActorMessage(new PlantActor(core, 
            new Vector2(
              Position.X - 4, 
              Position.Y), 
            true
          )), this);
        }
      }


      colar = new Animation(0.1f);
      colar.Add("idle", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_colar_1),
        core.SpriteManager.GetSprite(SpriteName.plant_colar_2),
        core.SpriteManager.GetSprite(SpriteName.plant_colar_3),
      });
      colar.Add("die", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_colar_2),
        core.SpriteManager.GetSprite(SpriteName.plant_colar_die_1),
        core.SpriteManager.GetSprite(SpriteName.plant_colar_die_2),
      });
      colar.Play("idle");

      head = new Animation(loop: false);
      head.Add("aim", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_head_1, color)
      });
      head.Add("shoot", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_head_2, color),
        core.SpriteManager.GetSprite(SpriteName.plant_head_3, color)
      });
      head.Add("idle", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_head_2, color)
      });

      var delay = isSecondHead ? 50 : 0; // To prevent two heads shooting simultaneously

      sm = new StateMachine<PlantState, PlantEvent>();
      sm.State(PlantState.Idle).IsInitial()
        .AutoTransitionTo(PlantState.Aiming).After(ScienceHelper.GetRandom(50 + delay, 100 + delay));
      sm.State(PlantState.Aiming).AutoTransitionTo(PlantState.Shooting).After(50);
      sm.State(PlantState.Shooting).AutoTransitionTo(PlantState.Idle).After(25);
      sm.State(PlantState.Dying).ForcedOn(PlantEvent.Die);
      sm.Start();

    }

    public override void Update(int ticks)
    {

      //==================================

      sm.Update(ticks);

      if (sm.justEnteredState) {
        switch (sm.currentState)
        {
          case PlantState.Idle:
            head.Play("idle");
            stem.Play("idle");
            break;
          case PlantState.Aiming:
            head.Play("aim");
            stem.Play("aim");
            break;
          case PlantState.Shooting:
            head.Play("shoot");
            stem.Play("shoot");

            // Suspending second shot
            sm.State(PlantState.Idle).autoDelay = 200;
            break;
          case PlantState.Dying:
            stem.Play("die");
            colar.Play("die");
            colar.Loop = false;
            break;
        }
      }

      //==================================

      stem.Update(ticks);
      colar.Update(ticks);
      head.Update(ticks);

      //==================================

      if (stemPos == Vector2.Zero)
        stemPos = Position + new Vector2(18, -stem.GetCurrentFrame().Height);
      var animTicks = ticks + animOffset;

      Vector2 wave;
      if (!sm.IsIn(PlantState.Dying))
        wave = new Vector2((float)(Math.Sin(animTicks / 20) * 2), (float)(Math.Cos(animTicks / 17) * 2));
      else
        wave = new Vector2(stem.GetCurrentFrameNumber() * 10, 0);

      headPos = stemPos + stem.GetCurrentFrame().GetLink() + headOffset + wave 
        + new Vector2(sm.IsIn(PlantState.Shooting) ? -5 : 0, 0);

      if (!sm.IsIn(PlantState.Dying))
      {
        GetCollider(0).BoundingBox.X = (int)(headPos.X - Position.X);
        GetCollider(0).BoundingBox.Y = (int)(headPos.Y - Position.Y);
      }
      boundingBox.X = (int)(headPos.X - Position.X);
      boundingBox.Y = (int)(headPos.Y - Position.Y);

      base.Update(ticks);
    }

    public override void Draw()
    {
      if (!isSecondHead)
      {
        // Base
        core.Renderer[1].DrawSpriteW(bush, Position + new Vector2(0, -bush.Height));
        core.Renderer[6].DrawSpriteW(leaf, Position + new Vector2(11, -6));
      }

      // Stem
      core.Renderer[2].DrawSpriteW(
        stem.GetCurrentFrame(), 
        stemPos
      );

      // Colar and head
      core.Renderer[3].DrawSpriteW(
        colar.GetCurrentFrame(), 
        headPos
      );
      if (!sm.IsIn(PlantState.Dying))
      {
        core.Renderer[4].DrawSpriteW(
          head.GetCurrentFrame(), 
          headPos + new Vector2(-6, 2) + new Vector2(sm.currentState == PlantState.Shooting ? -4 : 0, 0)
        );
      }

      base.Draw();
    }

    private void Shoot() 
    {
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (sm.IsIn(PlantState.Dying))
      {
        if (!movedCollider)
        {
          GetCollider(0).BoundingBox.Y += 10000;
          movedCollider = true;
        }
        return;
      }

      var projectile = other as ProjectileActor;
      if (projectile != null && projectile.color == this.color)
      {
        sm.Trigger(PlantEvent.Die);

        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(
          core, headPos + new Vector2(-6, 2), head.GetCurrentFrame())), this);
          
        DropCoin(from: headPos);
      }

      var player = other as PlayerActor;
      if (player != null)
      {
        player.Hurt();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

