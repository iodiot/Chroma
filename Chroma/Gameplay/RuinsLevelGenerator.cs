using System;
using Chroma.Helpers;
using Chroma.Actors;
using Microsoft.Xna.Framework;
using Chroma.States;
using Chroma.Graphics;
using System.Collections.Generic;
using System.Linq;
using Chroma.Messages;

namespace Chroma.Gameplay
{
  public class RuinsLevelGenerator : LevelGenerator
  {
    private int nextBgObjectIn = 0;
    private int lastObjectIndex = -1;

    public RuinsLevelGenerator(Core core, ActorManager actorManager, Area area) : base(core, actorManager, area)
    {
      // Background
      bgColor = new Color(8, 20, 14);

      var dy = 8;
      BG.Add(new ParallaxLayer(core, 4 + dy, 0.05f, 1));
      BG.Add(new ParallaxLayer(core, 9 + dy, 0.1f, 2));
      BG.Add(new ParallaxLayer(core, 0 + dy, 0.2f, 3, new Color(12, 29, 21), 10));
      BG.Add(new ParallaxLayer(core, 59 + dy, 0.65f, 4));
      BG.Add(new ParallaxLayer(core, 94 + dy, 0.65f, 5));

      LoadBGTape(1, new string[] { "ruins_sky" });
      LoadBGTape(2, new string[] { "ruins_trees_l1" });
      LoadBGTape(3, new string[] { "ruins_trees_l2" });
      LoadBGTape(4, new string[] { "ruins_bg_platform_1", "ruins_bg_platform_2" });
      LoadBGTape(5, new string[] { "ruins_bg_bottom" });

      var sun = core.SpriteManager.GetSprite(SpriteName.ruins_sun);
      var newDecal = new ParallaxDecal(core, 
        sun, 
        core.Renderer.ScreenWidth * 0.8f, 12, 
        0f, "bg", 1
      );
      PlaceDecal(newDecal);
    }

    public override void Update(float distance)
    {
      base.Update(distance);

      if (nextBgObjectIn <= 0)
      {
        // Trees
        int index = SciHelper.GetRandom(1, 4, except: lastObjectIndex);
        lastObjectIndex = index;
        var sprite = core.SpriteManager.GetSprite("ruins_bg_tree_" + index.ToString());
        var newDecal = new ParallaxDecal(core, 
          sprite, 
          distance + core.Renderer.ScreenWidth + 40, 8, 
          .65f, "bg", 6
        );
        PlaceDecal(newDecal);

        // Leaves
        var x = distance + core.Renderer.ScreenWidth + 10;
        var maxX = x + sprite.Width + 30 + 20;
        var i = 0;
        do {
          sprite = core.SpriteManager.GetSprite(SpriteName.ruins_bg_leaves_1);
          newDecal = new ParallaxDecal(core, 
            sprite, 
            x, 0, 
            .65f, "bg", 6 + ((i % 2 == 0) ? 0 : 1)
          );
          PlaceDecal(newDecal);
          x += 30;
          i++;
        } while (x + sprite.Width < maxX);

        // Smaller leaves
        for (i = 0; i < SciHelper.GetRandom(0, 3); i++)
        {
          sprite = core.SpriteManager.GetSprite(SpriteName.ruins_bg_leaves_2);
          newDecal = new ParallaxDecal(core, 
            sprite, 
            x - SciHelper.GetRandom(0, 35), SciHelper.GetRandom(15, 25), 
            .65f, "bg", 6
          );
          PlaceDecal(newDecal);
        }

        nextBgObjectIn = SciHelper.GetRandom(8, 30);
      }
      else
      {
        nextBgObjectIn -= dDistance;
      }
    }

    public override Sprite GetGroundSprite()
    {
      return core.SpriteManager.GetSprite("bricks");
    }

    protected override void StartLevel()
    {
      base.StartLevel();

      // TODO: spawn starting scene
    }

    protected override void ProgressLevel()
    {
      switch (milestone)
      {
        case 0:
          ResetAllRatios();

          SetRatioOf(LevelModule.Flat, 100);
          SetRatioOf(LevelModule.Raise, 2);
          SetRatioOf(LevelModule.Descent, 2);
          SetRatioOf(LevelModule.CliffRight, 1);
          SetRatioOf(LevelModule.Gap, 1);
          SetRatioOf(LevelModule.Pond, 1);

          SetRatioOf(Encounter.None, 100);
          SetRatioOf(Encounter.Spikes, 20);
          SetRatioOf(Encounter.Golem, 10);
          SetRatioOf(Encounter.Zapper, 30);
          SetRatioOf(Encounter.HealthItem, 1);
          SetRatioOf(Encounter.Cube, 30);

          SetRatioOf(Encounter.Plant, 30);
          break;
      }
    }
  }
}

