using System;
using Microsoft.Xna.Framework;

namespace Chroma.Helpers
{
  public class SciHelper
  {
    public static float Eps = 0.001f;
    public static float BigFloat = 100500.0f;

    private static Random random;

    static SciHelper()
    {
      random = new Random();
    }

    public static int GetRandom(int from, int to, int? except = null)
    {
      int result;
      Debug.Assert(!(from == except && to == except), "Range contains no integers but the exception.");

      do
      {
        result = (random.Next() % (to - from + 1)) + from;
      } while (result == except);

      return result;
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

    public static Vector2 GetRandomVectorInCircle(float radius)
    {
      return new Vector2(SciHelper.GetRandom(-radius, radius), SciHelper.GetRandom(-radius, radius));
    }

    public static float GetNormalRandom(float mean, float deviation)
    {
      var u1 = GetRandom(0f, 1f);
      var u2 = GetRandom(0f, 1f);
      var randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2));

      return mean + deviation * randStdNormal; 
    }
  }
}

