using System;
using Microsoft.Xna.Framework;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Helpers;
using Chroma.Gameplay;

namespace Chroma.Actors
{
  public abstract class FlatPlatformActor : PlatformActor
  {
    public int width { get; protected set; }
    protected Sprite floorSprite;

    public FlatPlatformActor(Core core, Vector2 position, int width, Area area) : base(core, position, area)
    {
      this.width = width;
      GetSprites();

      if (this.width % floorSprite.Width > 0)
      {
        this.width -= this.width % floorSprite.Width;
        this.width += floorSprite.Width;
      }

      boundingBox = new Rectangle(0, 0, this.width, 300);
      LeftY = RightY = (int)position.Y;

      CanMove = false;
      CanFall = true;
    }

    protected virtual void GetSprites() {
    }

    public override void Draw()
    {
      DrawEdges();
      DrawGround();

      base.Draw();
    }   

    public static FlatPlatformActor Create(Core core, Vector2 position, int width, Area area)
    {
      switch (area)
      {
        default:
        case Area.Jungle:
          return new JungleFlatPlatformActor(core, position, width, area);
        case Area.Ruins:
          return new RuinsFlatPlatformActor(core, position, width, area);

      }
    }

    protected abstract void DrawGround();
  }
}

