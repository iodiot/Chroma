using System;
using System.Collections.Generic; 

namespace Chroma.Graphics
{
  public sealed class Animation
  {
    public string CurrentSequence { get; private set; }
    public float Speed { get; set; }

    private readonly Dictionary<string, List<Sprite>> animations;

    private float timeLine;

    public Animation(float speed = 0.2f)
    {
      Speed = speed;
      CurrentSequence = "";

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
      Debug.Assert(animations[name] != null, String.Format("Animation.Play() : Sequence {0} is missing", name));

      CurrentSequence = name;
      Reset();
    }

    private void InternalUpdate(bool forward = true)
    {
      Debug.Assert(animations[CurrentSequence] != null, "Animation.InternalUpdate() : Current sequence is missing");

      if (forward)
      {
        timeLine += Speed;

        if (timeLine >= animations[CurrentSequence].Count)
        {
          Reset();
        }
      }
      else
      {
        timeLine -= Speed;

        if (timeLine < 0f)
        {
          timeLine = animations[CurrentSequence].Count - 1;
        }
      }
    }

    public void Update(int ticks)
    {
      InternalUpdate();
    }

    public void StepBackward(int steps = 1)
    {
      for (var i = 0; i < steps; i++)
      {
        InternalUpdate(false);
      }
    }

    public void StepForward(int steps = 1)
    {
      for (var i = 0; i < steps; i++)
      {
        InternalUpdate();
      }
    }

    public Sprite GetCurrentFrame()
    {
      Debug.Assert(animations[CurrentSequence] != null, "Animation.GetCurrentFrame() : Current sequence is missing");

      return animations[CurrentSequence][(int)timeLine];
    }

    public int GetCurrentFrameNumber()
    {
      return (int)timeLine;
    }
  }
}

