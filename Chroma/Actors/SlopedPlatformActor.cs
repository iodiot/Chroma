using System;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Graphics;

namespace Chroma.Actors
{

  public enum SlopeDirection {
    Up,
    Down
  }

  public class SlopedPlatformActor: Actor
  {
    public readonly int sections;
    public readonly int width;
    public readonly SlopeDirection direction;

    private readonly int dir;

    public SlopedPlatformActor(Core core, Vector2 position, SlopeDirection direction, int sections = 1) : base(core, position)
    {
      this.sections = sections;
      this.width = sections * 33;

      this.direction = direction;

      CanMove = false;
      CanFall = true;

      boundingBox = new Rectangle(0, (direction == SlopeDirection.Up) ? 0 : 16 * sections, width, 300);

      int steps = 8;
      int stepHeight = 16 / steps;
      int stepWidth = 33 / steps;
      dir = (direction == SlopeDirection.Up) ? -1 : 1;

      for (int i = 1; i < steps * sections; i++)
      {
        int thisStepStart = 0;
        int thisStepWidth = 0;

        switch (direction) {
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
          stepHeight
        )), this);
      }
    }
      
    public override void Draw()
    {
      DrawGround();

      base.Draw();
    }

    private void DrawGround() {
      for (int i = 0; i < sections; i++) {

        var yPos = dir * i * 16;
        if (direction == SlopeDirection.Down)
          yPos += 16;

        core.Renderer.DrawSpriteW(
          "slope",
          new Vector2(Position.X + i * 33, Position.Y + yPos - 24),
          flip: direction == SlopeDirection.Up ? SpriteFlip.None : SpriteFlip.Horizontal
        );
        core.Renderer.DrawSpriteW(
          "slope_grass",
          new Vector2(Position.X + i * 33, Position.Y + yPos - 18),
          flip: direction == SlopeDirection.Up ? SpriteFlip.None : SpriteFlip.Horizontal
        );
      }
    }
  }
}

