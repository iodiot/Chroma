using System;
using Chroma.Gameplay;
using System.Collections.Generic;

namespace Chroma.States
{

  public enum Resource {
    Coin
  }

  public class ProfileData
  {
    public int MaxDistance;
    public Area CurrentArea;
    public Dictionary<Resource, int> Resources;

    public ProfileData()
    {
      // TODO Load local data

      // Debug
      CurrentArea = Area.Jungle;
      MaxDistance = 0;

      Resources = new Dictionary<Resource, int>();
      foreach (Resource resource in Enum.GetValues(typeof(Resource)))
      {
        Resources.Add(resource, 0);
      }
    }
  }
}

