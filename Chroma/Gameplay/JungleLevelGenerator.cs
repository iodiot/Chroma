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
  public class JungleLevelGenerator : LevelGenerator
  {

    private int lastGrassDistance = 0;

    public JungleLevelGenerator(Core core, ActorManager actorManager, Area area) : base(core, actorManager, area)
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
      return core.SpriteManager.GetSprite("earth");
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
          SetRatioOf(LevelModule.Raise, 2);
          SetRatioOf(LevelModule.Descent, 2);
          SetRatioOf(LevelModule.Gap, 10);
          SetRatioOf(LevelModule.Pond, 10);

          SetRatioOf(Encounter.None, 100);
          SetRatioOf(Encounter.Plant, 3);
          SetRatioOf(Encounter.Slime, 20);
          SetRatioOf(Encounter.Bridge, 2);
          SetRatioOf(Encounter.Boulder, 2);
          SetRatioOf(Encounter.HealthItem, 1);
          break;
        case 200:
          SetRatioOf(Encounter.Plant, 20);
          SetRatioOf(Encounter.SlimeWalk, 2);
          SetRatioOf(Encounter.SlimeRoll, 20);
          break;
        case 400:
          SetRatioOf(Encounter.SlimeWalk, 10);
          break;
        case 600:
          SetRatioOf(Encounter.SlimeWalk, 20);
          break;
      }
    }

    public override void Update(float distance)
    {
      base.Update(distance);

      if (this.distanceMeters - lastGrassDistance > 3)
      {
        float lvl = ScienceHelper.GetRandom(10, 18);
        var scale = lvl / 10f;
        var newDecal = new ParallaxDecalActor(core, new Vector2(CurrentX, - core.Renderer.World.Y + core.Renderer.ScreenHeight - 82 * scale + ScienceHelper.GetRandom(0, 50)), new Vector2(1.4f, 0), 
          "fg_grass_1", "", 1000 + (int)lvl, ScienceHelper.ChanceRoll(0.5f), scale);
        core.MessageManager.Send(new AddActorMessage(newDecal), this);
        lastGrassDistance = this.distanceMeters;
      }

    }
  }
}

