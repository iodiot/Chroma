using System;
using System.Collections.Generic;

namespace Chroma.Actors
{
  public enum Feature
  {
    CanMove,
    CanLick,
    CanFall,

    OneWayObstacle
  }

  public sealed class ActorFeatures
  {
    #region Factory
    public static ActorFeatures MakeTypicalBeing()
    {
      return new ActorFeatures().Add(Feature.CanMove).Add(Feature.CanLick).Add(Feature.CanLick);
    }

    public static ActorFeatures MakeTypicalObstacle(bool oneWay = false)
    {
      return oneWay ? new ActorFeatures().Add(Feature.OneWayObstacle) : new ActorFeatures();
    }
    #endregion

    private readonly HashSet<Feature> features;

    public bool IsObstacle { get { return !Contains(Feature.CanMove) && !Contains(Feature.CanFall); } }

    public bool this[Feature feature] 
    {
      get { return Contains(feature); }
    }

    public ActorFeatures()
    {
      features = new HashSet<Feature>();
    }

    public ActorFeatures(HashSet<Feature> features)
    {
      this.features = features;
    }

    public ActorFeatures Add(Feature feature)
    {
      features.Add(feature);
      return this;
    }

    public bool Contains(Feature feature)
    {
      return features.Contains(feature);
    }
  }

}

