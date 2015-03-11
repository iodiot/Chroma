using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Helpers;

namespace Chroma.Gameplay
{
  public enum MagicColor
  {
    Red = 1,
    Yellow = 2,
    Blue = 3,

    Orange = 4,
    Green = 5,
    Purple = 6
  }

  public static class MagicManager
  {
    public static Dictionary<MagicColor, Color> MagicColors = new Dictionary<MagicColor, Color>()
    {
      { MagicColor.Red, Color.Red },
      { MagicColor.Yellow, Color.Yellow },
      { MagicColor.Blue, Color.DodgerBlue },
      { MagicColor.Green, Color.GreenYellow },
      { MagicColor.Orange, Color.Orange },
      { MagicColor.Purple, Color.Magenta }
    };

    public static MagicColor GetRandomColor(float chanceOfComplexColor = 0.5f, MagicColor? except = null)
    {
      MagicColor result;

      do
      {
        result = (MagicColor)(ScienceHelper.GetRandom(1, 3) + (ScienceHelper.ChanceRoll(chanceOfComplexColor) ? 3 : 0));
      } while (!(except == null || result != except));

      return result;
    }

    public static MagicColor Mix(MagicColor first, MagicColor second) 
    {
      var result = first;

      if (second == 0) 
      {
        result = first;
      } 
      else 
      {
        if (second < first)
        {
          var temp = second;
          second = first;
          first = temp;
        }

        if (first == MagicColor.Red && second == MagicColor.Yellow)
          result = MagicColor.Orange;
        if (first == MagicColor.Red && second == MagicColor.Blue)
          result = MagicColor.Purple;
        if (first == MagicColor.Yellow && second == MagicColor.Blue)
          result = MagicColor.Green;
      }

      return result;
    }
  }
}

