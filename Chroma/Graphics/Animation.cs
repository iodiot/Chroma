using System;
using System.Collections.Generic; 

namespace Chroma.Graphics
{
  public sealed class Animation
  {
    public string CurrentPlay { get; private set; }
    public float Speed { get; private set; }

    private readonly Dictionary<string, List<Sprite>> animations;

    private float timeLine;

    public Animation(float speed = 0.2f)
    {
      Speed = speed;
      CurrentPlay = "";

      animations = new Dictionary<string, List<Sprite>>();

      Reset();
    }

    public void Reset()
    {
      timeLine = 0;
    }

    public void Add(string name, IEnumerable<Sprite> frames)
    {
      animations[name] = new List<Sprite>();
      animations[name].AddRange(frames);
    }

    public void AddAndPlay(string name, IEnumerable<Sprite> frames)
    {
      Add(name, frames);
      Play(name);
    }

    public void Play(string name)
    {
      CurrentPlay = name;
      Reset();
    }

    public void Update(int ticks)
    {
      Debug.Assert(animations[CurrentPlay] != null, "Animation.Update() : Current play is missing");

      timeLine += Speed;

      if (timeLine >= animations[CurrentPlay].Count)
      {
        Reset();
      }
    }

    public Sprite GetCurrentFrame()
    {
      Debug.Assert(animations[CurrentPlay] != null, "Animation.GetCurrentFrame() : Current play is missing");

      return animations[CurrentPlay][(int)timeLine];
    }
  }
}

