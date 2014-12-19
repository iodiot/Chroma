﻿using System;

namespace Chroma
{
  sealed class Debug
  {
    public static void Assert(bool condition, string message)
    {
      #if DEBUG
      if (!condition)
      {
        throw new Exception(message);
      }
      #endif
    }
  }
}

