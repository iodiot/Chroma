using System;
using Microsoft.Xna.Framework;

namespace Chroma.Helpers
{
  public static class Extensions
  {
    public static Vector2 XY (this Vector3 v) {
      return new Vector2 (v.X, v.Y);
    }
  }
}

