using System;
using Chroma.Graphics;
using Chroma.Gameplay;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Chroma.Messages;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class FragmentActor : Actor
  {
    private Sprite sprite;
    private Animation animation;

    public bool hurtPlayer = false;

    public FragmentActor(Core core, Vector2 position, Sprite sprite) : base(core, position)
    {
      animation = null;

      this.sprite = sprite;
      boundingBox = new Rectangle(0, 0, sprite.Width, sprite.Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      Initialize();
    }

    public FragmentActor(Core core, Vector2 position, Animation animation) : base(core, position)
    {
      sprite = null;

      this.animation = animation;
      animation.Play("live");
      var frame = animation.GetCurrentFrame();
      boundingBox = new Rectangle(0, 0, frame.Width, frame.Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      Initialize();
    }

    private void Initialize() 
    {
      CanMove = true;
      CanFall = true;
      CanBounce = true;

      Velocity.X = (float)ScienceHelper.GetRandom(-20, 20) / 10f;
      Velocity.Y = (float)ScienceHelper.GetRandom(-30, -10) / 10f;
    }

    public override void Update(int ticks)
    {
      if (animation != null)
        animation.Update(ticks);
      base.Update(ticks);
    }

    public override void Draw()
    {
      var sprite = (this.sprite == null) ? animation.GetCurrentFrame() : this.sprite;
      core.Renderer.DrawSpriteW(sprite, Position);
      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (hurtPlayer)
      {
        var player = other as PlayerActor;
        if (player != null)
          player.Hurt();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return true;
    }
  }
}

