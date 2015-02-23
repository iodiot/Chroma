using System;

namespace Chroma.Helpers
{

  // A simple mutable pair
  public class Pair<T1, T2>
  {
    public T1 A { get; set; }
    public T2 B { get; set; }

    public Pair(T1 A, T2 B) 
    {
      this.A = A;
      this.B = B;
    }
  }

}

