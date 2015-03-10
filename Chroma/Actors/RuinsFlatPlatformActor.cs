using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Helpers;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public class RuinsFlatPlatformActor : FlatPlatformActor
  {
    protected override void GetSprites()
    {
      floorSprite = core.SpriteManager.GetSprite("ruins_floor_" + ScienceHelper.GetRandom(1, 2).ToString());

      base.GetSprites();
    }

    public RuinsFlatPlatformActor(Core core, Vector2 position, int width, Area area) : base(core, position, width, area)
    {
      // Vines
      if (ScienceHelper.ChanceRoll(0.5f))
      {
        var x = Position.X + ScienceHelper.GetRandom(4, width - 12);
        var vine = ScienceHelper.GetRandom(1, 4);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
              new Vector2(x, Position.Y),
              "vine_" + vine.ToString(),
              flip: ScienceHelper.ChanceRoll(0.5f),
              depth: 10
            )), this);
        if (ScienceHelper.ChanceRoll(0.3f))
        {
          x += 8;
          var vine2 = ScienceHelper.GetRandom(1, 4);
          while (vine == vine2)
          {
            vine2 = ScienceHelper.GetRandom(1, 4);
          }
          core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
                new Vector2(x, Position.Y),
                "vine_" + vine2.ToString(),
                flip: ScienceHelper.ChanceRoll(0.5f),
                depth: 10
              )), this);
        }
      }
      else
      {

        var x = Position.X + ScienceHelper.GetRandom(4, width - 12);
        var leaves = ScienceHelper.GetRandom(1, 3);
        core.MessageManager.Send(new AddActorMessage(new DecalActor(core, 
          new Vector2(x, Position.Y),
          "leaves_" + leaves.ToString(),
          flip: ScienceHelper.ChanceRoll(0.5f),
          depth: 10
        )), this);
      }
    }

    protected override void DrawGround()
    {
      var n = width / floorSprite.Width;
      for (var i = 0; i <= n; ++i)
      {
        core.Renderer.DrawSpriteW(
          i == n ? floorSprite.ClampWidth(width % floorSprite.Width) : floorSprite, 
          new Vector2(Position.X + floorSprite.Width * i, Position.Y - 13)
        );
      }

    }
  }
}

