using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class PlantActor : Actor
  {
    private MagicColor color;
    private bool isSecondHead;
    private Vector2 headOffset, stemPos, headPos;

    private Sprite bush, leaf;
    private Animation stem, colar, head;
    private int nextMoveIn = 0;
    private bool noHead = false;
    private bool movedCollider = false;

    private int animOffset;

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

      stem = new Animation(loop: false);

      if (isSecondHead || ScienceHelper.ChanceRoll())
      {
        stem.Add("live", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_1),
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_2),
            core.SpriteManager.GetSprite(SpriteName.plant_stem1_3),
          });
        headOffset = new Vector2(-16, -15);
      }
      else
      {
        stem.Add("live", new List<Sprite>
          {
            core.SpriteManager.GetSprite(SpriteName.plant_stem_1),
            core.SpriteManager.GetSprite(SpriteName.plant_stem_2),
            core.SpriteManager.GetSprite(SpriteName.plant_stem_3),
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

      stem.Pong = true;
      stem.Play("live");

      stemPos = Position + new Vector2(18, -stem.GetCurrentFrame().Height);

      colar = new Animation(0.1f);
      colar.Add("live", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_colar_1),
        core.SpriteManager.GetSprite(SpriteName.plant_colar_2),
        core.SpriteManager.GetSprite(SpriteName.plant_colar_3),
      });
      colar.Play("live");

      head = new Animation(loop: false);
      head.Add("shoot", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_head_1, color),
        core.SpriteManager.GetSprite(SpriteName.plant_head_2, color),
        core.SpriteManager.GetSprite(SpriteName.plant_head_3, color),
      });
      head.Add("live", new List<Sprite> {
        core.SpriteManager.GetSprite(SpriteName.plant_head_2, color)
      });
      head.Play("live");

    }

    public override void Update(int ticks)
    {

      stem.Update(ticks);
      colar.Update(ticks);
      head.Update(ticks);

      if (stem.Paused)
      {
        if (stem.JustStopped)
          nextMoveIn = 50;
        else
        {
          nextMoveIn--;
          if (nextMoveIn == 0)
            stem.Paused = false;
        }
      }

      var animTicks = ticks + animOffset;
      var wave = new Vector2((float)(Math.Sin(animTicks / 20) * 2), (float)(Math.Cos(animTicks / 17) * 2));
      headPos = stemPos + stem.GetCurrentFrame().GetLink() + headOffset + wave;

      if (!noHead)
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
      if (!noHead)
      {
        core.Renderer[4].DrawSpriteW(
          head.GetCurrentFrame(), 
          headPos + new Vector2(-6, 2)
        );
      }

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (noHead)
      {
        if (!movedCollider)
        {
          GetCollider(0).BoundingBox.Y += 10000;
          movedCollider = true;
        }
        return;
      }

      if (other is ProjectileActor && ((ProjectileActor)other).color == this.color)
      {
        noHead = true;
        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(
          core, headPos + new Vector2(-6, 2), head.GetCurrentFrame())), this);
          
        DropCoin(from: headPos);
      }

      if (other is PlayerActor)
      {
        (other as PlayerActor).Hurt();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

