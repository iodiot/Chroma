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
    private int nextTreeIn = 0;

    public JungleLevelGenerator(Core core, ActorManager actorManager, Area area) : base(core, actorManager, area)
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
      return core.SpriteManager.GetSprite("earth");
    }

    public override void DrawBackground()
    {
      // EXPERIMENT: sun rays
//      var rays = core.SpriteManager.GetSprite(SpriteName.rays);
//
//      var swing1 = new Vector2(
//        (float)Math.Cos((float)core.Ticks / 83) * 15,
//        -10 + (float)Math.Sin((float)core.Ticks / 70) * 10
//      );
//
//      var swing2 = new Vector2(
//        (float)Math.Sin((float)core.Ticks / 67) * 10,
//        -5 + (float)Math.Cos((float)core.Ticks / 57) * 5
//      );
//
//      core.Renderer["bg", 5].DrawSpriteS(rays, new Vector2(25, 0) + swing1, Color.LightSkyBlue);
//      core.Renderer["bg", 5].DrawSpriteS(rays, new Vector2(20, 0) + swing2, Color.MediumTurquoise);

      base.DrawBackground();
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
          SetRatioOf(LevelModule.CliffRight, 2);
          SetRatioOf(LevelModule.CliffLeft, 10);
          SetRatioOf(LevelModule.Gap, 0);
          SetRatioOf(LevelModule.Pond, 0);
          SetRatioOf(LevelModule.CoinGap, 50);


          SetRatioOf(Encounter.None, 100);
          SetRatioOf(Encounter.Plant, 10);
          SetRatioOf(Encounter.Slime, 20);
          SetRatioOf(Encounter.Bridge, 2);
          SetRatioOf(Encounter.Boulder, 2);
          SetRatioOf(Encounter.HealthItem, 100);
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

//      if (this.distanceMeters - lastGrassDistance > 3)
//      {
//        float lvl = ScienceHelper.GetRandom(10, 18);
//        var scale = lvl / 10f;
//        var newDecal = new ParallaxDecalActor(core, new Vector2(CurrentX, - core.Renderer.World.Y + core.Renderer.ScreenHeight - 82 * scale + ScienceHelper.GetRandom(0, 50)), 1.4f, 
//          "fg_grass_1", "", 1000 + (int)lvl, ScienceHelper.ChanceRoll(0.5f), scale);
//        //core.MessageManager.Send(new AddActorMessage(newDecal), this);
//        lastGrassDistance = this.distanceMeters;
//      }

      if (nextTreeIn <= 0)
      {
        var lvl = (float)SciHelper.GetRandom(0, 20);

        var sprite = core.SpriteManager.GetSprite("trees_l5_" + SciHelper.GetRandom(1, 4).ToString());
        var newDecal = new ParallaxDecal(core, 
                         sprite, 
                         distance + core.Renderer.ScreenWidth + 40, 95 + lvl - sprite.Height, 
                         .7f + lvl / 100, "bg", 5 + (int)lvl
                       );
        PlaceDecal(newDecal);

        sprite = core.SpriteManager.GetSprite(SpriteName.tree_leaves);
        newDecal = new ParallaxDecal(core, 
          sprite, 
          distance + core.Renderer.ScreenWidth + 20, -25 + lvl, 
          .7f + lvl / 100, "bg", 5 + (int)lvl
        );
        PlaceDecal(newDecal);

        nextTreeIn = SciHelper.GetRandom(5, 20);
      }
      else
      {
        nextTreeIn -= dDistance;
      }
    }
  }
}

