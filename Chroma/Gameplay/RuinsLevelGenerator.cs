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
    public RuinsLevelGenerator(Core core, ActorManager actorManager, Area area) : base(core, actorManager, area)
    {
      // Background
      bgColor = new Color(9, 16, 25);

      BG.Add(new ParallaxLayer(core, 21, 0.1f, 1));
      BG.Add(new ParallaxLayer(core, 21, 0.2f, 2));
      BG.Add(new ParallaxLayer(core, 7, 0.5f, 3));
      BG.Add(new ParallaxLayer(core, -10, 0.65f, 4));
      BG.Add(new ParallaxLayer(core, 45, 0.7f, 5));

      BG.Add(new ParallaxLayer(core, 91, 0.75f, 5));
      BG.Add(new ParallaxLayer(core, 99, 0.8f, 5));
      BG.Add(new ParallaxLayer(core, 107, 0.85f, 5));
      BG.Add(new ParallaxLayer(core, 115, 0.9f, 5));

      LoadBGTape(1, new string[] { "trees_l1" });
      LoadBGTape(2, new string[] { "trees_l2" });
      LoadBGTape(3, new string[] { "trees_l3_1", "trees_l3_2", "trees_l3_3" });
      LoadBGTape(4, new string[] { "trees_l4" });
      LoadBGTape(5, new string[] { "forest_floor_0" });

      LoadBGTape(6, new string[] { "forest_floor_1", "forest_floor_2", "forest_floor_3" });
      LoadBGTape(7, new string[] { "forest_floor_1", "forest_floor_2", "forest_floor_3" });
      LoadBGTape(8, new string[] { "forest_floor_1", "forest_floor_2", "forest_floor_3" });
      LoadBGTape(9, new string[] { "forest_floor_1", "forest_floor_2", "forest_floor_3" });
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
      if (milestone > 0)
        return;

      ResetAllRatios();

      SetRatioOf(LevelModule.Flat, 300);
      SetRatioOf(LevelModule.Raise, 00);
      SetRatioOf(LevelModule.Descent, 00);
      SetRatioOf(LevelModule.Gap, 00);
      SetRatioOf(LevelModule.Pond, 00);
      SetRatioOf(LevelModule.CliffRight, 00);
      SetRatioOf(LevelModule.CliffLeft, 00);

      SetRatioOf(Encounter.None, 100);
      SetRatioOf(Encounter.Cube, 30);

      return;

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

