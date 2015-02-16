using System;
using System.Collections.Generic;

namespace Chroma.Actors
{
  public sealed class ActorFeatures
  {
    public enum Feature
    {
      CanMove,
      CanLick,
      CanFall,

      OneWayObstacle
    }

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

    #region Handy getters
    //public IsObstacle { get { return !Contains(CanMove) && !Contains(CanFall); } }
    #endregion

    public bool this[Feature feature] 
    {
      get { return Contains(feature); }
    }

    private readonly HashSet<Feature> features;

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

