using System;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Graphics;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public enum SlopeDirection {
    Up,
    Down
  }

  public abstract class SlopedPlatformActor: PlatformActor
  {
    public readonly int Sections;
    public readonly int Width;
    public readonly SlopeDirection Direction;

    protected readonly int dir;

    public SlopedPlatformActor(Core core, Vector2 position, SlopeDirection direction, Area area, int sections = 1) : 
    base(core, position, area)
    {
      Sections = sections;
      Width = sections * 33;
      Direction = direction;

      CanMove = false;
      CanFall = true;

      dir = (direction == SlopeDirection.Up) ? -1 : 1;

      boundingBox = new Rectangle(0, (direction == SlopeDirection.Up) ? 0 : 16 * sections, Width, 300);
      LeftY = (int)position.Y;
      RightY = (int)position.Y + dir * 16 * sections;

      var steps = 8;
      var stepHeight = 16 / steps;
      var stepWidth = 33 / steps;

      for (var i = 1; i < steps * sections; i++)
      {
        var thisStepStart = 0;
        var thisStepWidth = 0;

        switch (direction) 
        {
          case SlopeDirection.Up:
            thisStepStart = i * stepWidth;
            thisStepWidth = 33 * sections - stepWidth * i;
            break;
          case SlopeDirection.Down:
            thisStepStart = 0;
            thisStepWidth = i * stepWidth;
            break;
        }

        core.MessageManager.Send(new AddActorMessage(new InvisiblePlatformActor(core, 
          new Vector2(
            Position.X + thisStepStart, 
            Position.Y + dir * i * stepHeight), 
          thisStepWidth, 
          stepHeight,
          area
        )), this);
      }
    }

    public static SlopedPlatformActor Create(Core core, Vector2 position, SlopeDirection direction, Area area, int sections = 1)
    {
      switch (area)
      {
        default:
        case Area.Jungle:
          return new JungleSlopedPlatformActor(core, position, direction, area, sections);
        case Area.Ruins:
          return new RuinsSlopedPlatformActor(core, position, direction, area, sections);
      }
    }
      
    public override void Draw()
    {
      DrawEdges();
      DrawGround();

      base.Draw();
    }
      
    protected abstract void DrawGround();
  }
}

