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
      bgColor = new Color(17, 22, 42);

      BG = new List<ParallaxLayer>();
      BG.Add(new ParallaxLayer(core, 31, 0.1f));
      BG.Add(new ParallaxLayer(core, 31, 0.2f));
      BG.Add(new ParallaxLayer(core, 17, 0.5f));
      BG.Add(new ParallaxLayer(core, 0, 0.7f));

      LoadBGTape(1, new string[] { "trees_l1" });
      LoadBGTape(2, new string[] { "trees_l2" });
      LoadBGTape(3, new string[] { "trees_l3_1", "trees_l3_2", "trees_l3_3" });
      LoadBGTape(4, new string[] { "trees_l4" });
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

          SetRatioOf(LevelModule.Flat, 200);
          SetRatioOf(LevelModule.Gap, 50);
          SetRatioOf(LevelModule.Pond, 20);

          SetRatioOf(Encounter.None, 100);
          SetRatioOf(Encounter.Golem, 20);
          SetRatioOf(Encounter.Zapper, 50);
          SetRatioOf(Encounter.HealthItem, 1);
          break;
      }
    }
  }
}

