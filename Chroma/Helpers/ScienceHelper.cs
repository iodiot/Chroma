using System;

namespace Chroma.Helpers
{
  public class ScienceHelper
  {
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
      return GetRandom(1, 100) <= chance * 100;
    }
  }
}

