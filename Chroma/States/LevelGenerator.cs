using System;
using Chroma.Actors;
using Microsoft.Xna.Framework;

namespace Chroma.States
{
  public class LevelGenerator
  {
  
    private readonly ActorManager actorManager;
    private readonly Core core;

    public PlatformActor lastPlatform { get; private set; }
    public int currentX { get; private set; }
    public int currentY { get; private set; }
    public float distance;

    public LevelGenerator(Core core, ActorManager actorManager)
    {
      this.core = core;
      this.actorManager = actorManager;
      lastPlatform = null;
    }

    public void StartLevel() 
    {
      currentX = currentY = 0;
      distance = 0;
      AddFlat((int)core.Renderer.ScreenWidth + 10);
    }

    public void Update(float distance)
    {
      this.distance = distance;
      if (currentX < distance + core.Renderer.ScreenWidth + 100)
      {
        ExtendLevel();
      }
    }

    public void ExtendLevel() 
    {
      AddModule((LevelModule)core.GetRandom(0, Enum.GetValues(typeof(LevelModule)).Length));
    }
      
    //--------------------------------------------------

    #region Basic secions
    private void AttachToLast(PlatformActor newPlatform)
    {
      if (lastPlatform != null)
      {
        newPlatform.PreviousPlatform = lastPlatform;
        lastPlatform.NextPlatform = newPlatform;
      }
      lastPlatform = newPlatform;
    }

    private void AddFlat(int length = 30, int elevation = 0)
    {
      currentY += elevation;

      var newPlatform = new FlatPlatformActor(core, new Vector2(currentX, currentY), length);
      actorManager.Add(newPlatform);
      AttachToLast(newPlatform);

      currentX += length;
    }

    private void AddSlope(SlopeDirection direction, int sections = 1, int elevation = 0)
    {
      currentY += elevation;

      var newSlope = new SlopedPlatformActor(core, new Vector2(currentX, currentY), direction, sections);
      actorManager.Add(newSlope);
      AttachToLast(newSlope);

      currentY = (int)newSlope.RightY;
      currentX += newSlope.Width;
    }

    private void AddGap(int length)
    {
      currentX += length;
      lastPlatform = null;
    }
    #endregion

    //--------------------------------------------------

    #region Modules
    enum LevelModule {
      Flat = 0,

      Raise,
      Descent,

      CliffRight,
      CliffLeft,
      Gap
    }

    private void AddModule(LevelModule module)
    {
      switch (module)
      {
        case LevelModule.Flat:
          AddFlat(100);
          break;

        case LevelModule.Raise:
          AddSlope(SlopeDirection.Up);
          break;
        case LevelModule.Descent:
          AddSlope(SlopeDirection.Down);
          break;

        case LevelModule.CliffRight:
          AddSlope(SlopeDirection.Up, 2);
          AddFlat(40, 70);
          break;

        case LevelModule.CliffLeft:
          AddFlat(40);
          AddSlope(SlopeDirection.Down, 2, -70);
          break;

        case LevelModule.Gap:
          AddGap(50);
          break;
      }
    }
    #endregion

    //--------------------------------------------------
  }
}

