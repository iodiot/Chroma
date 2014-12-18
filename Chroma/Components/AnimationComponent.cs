using System;
using System.Collections.Generic;
using Chroma;
using Chroma.Actors;
using Chroma.Graphics;

namespace Chroma.Components
{
  class AnimationComponent : Component
  {
    public string CurrentName { get; private set; }
    public float Speed { get; private set; }

    private readonly Dictionary<string, List<Sprite>> animations;
    private float timeLine;

    public AnimationComponent(Actor actor, float speed = 0.2f) : base(ComponentType.Animation, actor)
    {
      Speed = speed;
      CurrentName = "";

      animations = new Dictionary<string, List<Sprite>>();

      Reset();
    }

    public void Reset()
    {
      timeLine = 0;
    }

    public void Add(string name, List<Sprite> frames)
    {
      animations[name] = new List<Sprite>();
      animations[name].AddRange(frames);
    }

    public void Play(string name)
    {
      CurrentName = name;
      Reset();
    }

    public override void Update(int ticks)
    {
      if (animations[CurrentName] == null)
      {
        return;
      }

      timeLine += Speed;

      if (timeLine >= animations[CurrentName].Count)
      {
        Reset();
      }
    }

    public Sprite GetCurrentFrame()
    {
      if (animations[CurrentName] == null)
      {
        return null ;
      }

      return animations[CurrentName][(int)timeLine];
    }
  }
}

