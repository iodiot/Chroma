using System;
using Chroma.Helpers;
using Chroma.Actors;
using Microsoft.Xna.Framework;
using Chroma.States;
using System.Collections.Generic;
using System.Linq;

namespace Chroma.Gameplay
{
  public partial class LevelGenerator
  {

    enum Encounter {

      None = 0,

      // Obstacles
      Boulder,

      // Items
      HealthItem,

      //Structures
      Bridge,

      // Enemies
      Slime,
      SlimeRoll,
      SlimeWalk,

      Golem
    }

    private void SpawnEncounter(Encounter encounter)
    {
      var width = 30;

      var position = new Vector2(CurrentX + 15, CurrentY);
      switch (encounter)
      {
        case Encounter.None:
          break;

          #region Items
        case Encounter.HealthItem:
          position.Y -= 50;
          var newItem = new ItemActor(core, position);
          actorManager.Add(newItem);
          break;
          #endregion

          #region Objects
        case Encounter.Boulder:
          var newBoulder = new BoulderActor(core, position);
          width += newBoulder.GetBoundingBoxW().Width;
          actorManager.Add(newBoulder);
          break;
          #endregion

          #region Structures
        case Encounter.Bridge:
          SpawnBridge();
          break;
          #endregion

          #region Enemies
        case Encounter.Golem:
          var newGolem = new GolemActor(core, position, MagicManager.GetRandomColor(core, 0.2f));
          width += newGolem.GetBoundingBoxW().Width;
          actorManager.Add(newGolem);
          break;

        case Encounter.Slime:
          var newSlime = new SlimeActor(core, position, MagicManager.GetRandomColor(core, 0.2f), false);
          width += newSlime.GetBoundingBoxW().Width;
          actorManager.Add(newSlime);
          break;

        case Encounter.SlimeRoll:
          var newSlimeRoller = new SlimeActor(core, position, MagicManager.GetRandomColor(core, 0.2f), true);
          width += newSlimeRoller.GetBoundingBoxW().Width;
          actorManager.Add(newSlimeRoller);
          break;

        case Encounter.SlimeWalk:
          var newSlimeWalker = new SlimeWalkActor(core, position, MagicManager.GetRandomColor(core, 0.2f));
          width += newSlimeWalker.GetBoundingBoxW().Width;
          actorManager.Add(newSlimeWalker);
          break;
          #endregion
      }

      SpawnFlat(width);
    }
      
    private void SpawnBridge()
    {
      var n = ScienceHelper.GetRandom(10, 30);
      var x = CurrentX;
      var y = CurrentY - ScienceHelper.GetRandom(30, 50);

      for (var i = 1; i <= n; i++)
      {
        PlankActor newPlank = new PlankActor(core, new Vector2(x, y), 
          (i == 1) ? PlankActor.PlankOrigin.Left :
          (i == n) ? PlankActor.PlankOrigin.Right:
          PlankActor.PlankOrigin.Middle
        );
        actorManager.Add(newPlank);
        x += newPlank.Width;
        //y += core.GetRandom(-2, 2);
      }
    }

  }
}
