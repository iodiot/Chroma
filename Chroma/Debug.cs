using System;

namespace Chroma
{
  public sealed class Debug
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

    public static void Print(string message)
    {
      Console.WriteLine(">>>>> " + message);
    }

    public static void Error(string message)
    {
      Console.WriteLine("##### " + message);
    }
  }
}

