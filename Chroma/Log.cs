using System;

namespace Chroma
{
  sealed class Log
  {
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
