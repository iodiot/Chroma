using System;
using System.Linq;
using System.Collections.Generic;

namespace Chroma.Graphics
{
  public sealed class FrameCounter
  {
    public long TotalFrames { get; private set; }
    public float TotalSeconds { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    public const int MaximumSamples = 100;

    private Queue<float> sampleBuffer = new Queue<float>();

    public void Update(float deltaTime)
    {
      CurrentFramesPerSecond = 1.0f / deltaTime;

      sampleBuffer.Enqueue(CurrentFramesPerSecond);

      if (sampleBuffer.Count > MaximumSamples)
      {
        sampleBuffer.Dequeue();
        AverageFramesPerSecond = sampleBuffer.Average(i => i);
      } 
      else
      {
        AverageFramesPerSecond = CurrentFramesPerSecond;
      }

      ++TotalFrames;
      TotalSeconds += deltaTime;
    }
  }
}

