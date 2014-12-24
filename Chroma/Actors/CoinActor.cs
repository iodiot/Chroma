using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Chroma.Actors;
using Chroma.States;
using Chroma.Messages;
using Chroma.Graphics;

namespace Chroma.Actors
{
  public enum CoinType
  {
    Vertical,
    Horizontal
  };

  public class CoinActor : CollidableActor
  {
    private readonly Animation animation;

    public CoinActor(Core core, Vector2 position, CoinType type = CoinType.Vertical) : base(core, position)
    {
      boundingBox = new Rectangle(0, 0, 5, 5);

      animation = new Animation();
      var prefix = String.Format("coin_{0}", type == CoinType.Vertical ? "v" : "h");
      animation.Add("live", core.SpriteManager.GetFrames(prefix, new List<int>{ 1, 2, 3 }));

      animation.Play("live");
    }
      
    public override void Update(int ticks)
    {
      animation.Update(ticks);

      base.Update(ticks);
    }

    public override void Draw()
    {
      core.Renderer.DrawSpriteW(animation.GetCurrentFrame(), Position, Color.White);

      base.Draw();
    }

    public override void OnCollide(CollidableActor other)
    {
      core.SoundManager.Play("click");

      core.MessageManager.Send(new RemoveActorMessage(this), this);

      base.OnCollide(other);
    }
  }
}

