using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Actors;

namespace Chroma.Gui
{
  public class HealthGui : Gui
  {
    private Animation animation;
    private PlayerActor player;

    public HealthGui(Core core, PlayerActor player) : base(core)
    {
      this.player = player;

      animation = new Animation(0.2f);
      animation.AddAndPlay("live", core.SpriteManager.GetFrames("heart_", new List<int> { 1, 1, 2, 3, 4 }));
    }

    public override void Update(int ticks)
    {
      animation.Update(ticks);

      animation.Speed = (player.Hearts > 1) ? 0.2f : 0.4f;

      base.Update(ticks);
    }

    public override void Draw()
    {
      var heartsScale = 1.5f;
      var heartWidth = animation.GetCurrentFrame().Width * heartsScale;
      var heartsPadding = 0.1f * heartWidth;
      var heartsX = core.Renderer.ScreenWidth;
      heartsX -= player.MaxHearts * heartWidth + (player.MaxHearts - 1) * heartsPadding;
      heartsX /= 2;

      for (var i = 0; i < player.MaxHearts; ++i)
      {
        var alive = i < player.Hearts;
        var sprite = alive ? animation.GetCurrentFrame() : core.SpriteManager.GetSprite("heart_1");
        core.Renderer.DrawSpriteS(
          sprite,
          new Vector2(heartsX + i * (heartWidth + heartsPadding), 5),
          Color.White * (alive ? 1.0f : 0.2f),
          scale: new Vector2(heartsScale)
        );
      }

      base.Draw();
    }
  }
}

