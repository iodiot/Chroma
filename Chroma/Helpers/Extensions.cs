﻿using System;
using Microsoft.Xna.Framework;

namespace Chroma.Helpers
{
  public static class Extensions
  {
    public static Vector2 XY(this Vector3 v) 
    {
      return new Vector2(v.X, v.Y);
    }

    public static void SetX(this Vector2 v, float x)
    {
      v.X = x;
    }

    public static Rectangle FromFloats(this Rectangle r, float x, float y, float width, float height) 
    {
      r.X = (int)x;
      r.Y = (int)y;
      r.Width = (int)width;
      r.Height = (int)height;
      return r;
    }

    public static bool IsZero(this Vector3 v)
    {
      return v.Length() < SciHelper.Eps;
    }
  }
}

