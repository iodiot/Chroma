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
    public enum Preset
    {
      Custom,

      Remains
    }

    private Sprite sprite;
    private Animation animation;

    public bool hurtPlayer = false;

    public string Layer = "";
    public int zIndex = 0;
    //
    private float rotation = 0;
    public float RotationSpeed = 0;
    public float RotationFriction = 0.99f;
    //
    public float BouncesTillDrop = -1;
    //
    public Color Tint = Color.White;
    public float Opacity = 1f;
    public float OpacityStep = 0f;
    //
    public float Scale = 1.0f;
    public float ScaleStep = 0f;

    public FragmentActor(Core core, Vector2 position, Sprite sprite, Preset preset = Preset.Custom) : base(core, position)
    {
      animation = null;

      this.sprite = sprite;
      boundingBox = new Rectangle(0, 0, sprite.Width, sprite.Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      Initialize(preset);
    }

    public FragmentActor(Core core, Vector2 position, Animation animation, Preset preset = Preset.Custom) : base(core, position)
    {
      sprite = null;

      this.animation = animation;
      animation.Play("live");
      var frame = animation.GetCurrentFrame();
      boundingBox = new Rectangle(0, 0, frame.Width, frame.Height);
      AddCollider(new Collider() { BoundingBox = boundingBox });

      Initialize(preset);
    }

    private void Initialize(Preset preset) 
    {
      // Defaults
      CanMove = true;
      CanFall = true;
      CanBounce = true;

      IsSolid = false;

      Velocity.X = (float)ScienceHelper.GetRandom(-20, 20) / 10f;
      Velocity.Y = (float)ScienceHelper.GetRandom(-30, -10) / 10f;

      // Presets
      switch (preset)
      {
        case Preset.Remains:
          Velocity.X = ScienceHelper.GetRandom(3, 5);
          Velocity.Y = ScienceHelper.GetRandom(-3, -1);
          hurtPlayer = false;
          zIndex = 51;
          BouncesTillDrop = 1;
          RotationSpeed = ScienceHelper.GetRandom(0.1f, 0.4f);
          break;
      }
    }

    public override void Update(int ticks)
    {
      if (animation != null)
        animation.Update(ticks);

      rotation += RotationSpeed;
      RotationSpeed *= RotationFriction;
      //
      Opacity += OpacityStep;
      //
      Scale += ScaleStep;

      base.Update(ticks);
    }

    public override void Draw()
    {
      var sprite = (this.sprite == null) ? animation.GetCurrentFrame() : this.sprite;
      core.Renderer[Layer, zIndex].DrawSpriteW(
        sprite, 
        Position, 
        rotation: rotation, 
        origin: SpriteOrigin.Center,
        tint: Tint * Opacity,
        scale: new Vector2(Scale)
      );
      base.Draw();
    }

    public override void OnBounce()
    {
      if (BouncesTillDrop > 0)
        BouncesTillDrop--;
      if (BouncesTillDrop == 0)
        CanPassPlatforms = true;

      base.OnBounce();
    }

    public override void OnFall()
    {
      CanPassPlatforms = false;
      base.OnFall();
    }

    public override void OnDrown()
    {
      CanPassPlatforms = false;
      base.OnDrown();
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

