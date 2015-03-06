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

    private void ProgressLevel()
    {
      switch (milestone)
      {
        case 0:
          core.DebugMessage("Level started!");
          ResetAllRatios();

          //Testing out
          SetRatioOf(Encounter.HealthItem, 1);

          SetRatioOf(LevelModule.Flat, 200);
          SetRatioOf(LevelModule.Pond, 2);
          SetRatioOf(Encounter.Bridge, 2);
          //SetRatioOf(LevelModule.Raise, 0);
          //SetRatioOf(LevelModule.Descent, 0);
          SetRatioOf(LevelModule.Gap, 1);
          //SetRatioOf(LevelModule.CoinGap, 1);
          //SetRatioOf(LevelModule.CoinPattern, 3);

          SetRatioOf(Encounter.None, 100);
          SetRatioOf(Encounter.Slime, 20);
          SetRatioOf(Encounter.Golem, 0);
          SetRatioOf(Encounter.Boulder, 20);
          break;
        case 200:
          SetRatioOf(Encounter.SlimeWalk, 2);
          SetRatioOf(Encounter.SlimeRoll, 20);
          break;
        case 400:
          SetRatioOf(Encounter.Golem, 20);
          SetRatioOf(Encounter.SlimeWalk, 5);
          break;
        case 600:
          SetRatioOf(Encounter.SlimeWalk, 10);
          break;
      }
    }

  }
}

