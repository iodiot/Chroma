using System;
using Microsoft.Xna.Framework;

namespace Chroma.Helpers
{
  public class ScienceHelper
  {
    public static float Eps = 0.001f;
    public static float BigFloat = 100500.0f;

    private static Random random;

    static ScienceHelper()
    {
      random = new Random();
    }

    public static int GetRandom(int from, int to)
    {
      return (random.Next() % (to - from + 1)) + from;
    }

    public static bool ChanceRoll(float chance = 0.5f)
    {
      return GetRandom(0, 100) <= chance * 100;
    }

    public static bool IsZero(Vector2 v)
    {
      return v.Length() < Eps;
    }

    public static float GetRandom(float from, float to)
    {
      return (float)random.NextDouble() * (to - from) + from;
    }
  }
}

