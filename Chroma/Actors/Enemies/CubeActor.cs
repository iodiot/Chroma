using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Messages;
using Chroma.Gameplay;
using Chroma.StateMachines;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public class CubeActor : Actor
  {
    private enum CubeFace {
      Top = 0,
      Right = 5,
      Bottom = 10,
      Left = 15
    }
    private Dictionary<CubeFace, MagicColor?> faceColor;
    private Dictionary<CubeFace, Sprite> faceSprite;
    private CubeFace DirOfTop;

    //-------------------

    const int width = 32;
    const int rollX = 23;
    const float movementSpeed = 0.7f;

    private List<Sprite> frames;
    private int frameN;
    private float anchor;

    private readonly bool goRight;

    public CubeActor(Core core, Vector2 position) : base(core, position)
    {
      boundingBox = new Rectangle(0, -32, 32, 32);

      frames = new List<Sprite>();
      frames.Add(core.SpriteManager.GetSprite(SpriteName.cube_1));
      frames.Add(core.SpriteManager.GetSprite(SpriteName.cube_2));
      frames.Add(core.SpriteManager.GetSprite(SpriteName.cube_3));
      frames.Add(core.SpriteManager.GetSprite(SpriteName.cube_4));
      frames.Add(core.SpriteManager.GetSprite(SpriteName.cube_5));

      CanMove = true;
      CanFall = true;
      CanLick = true;

      AddCollider(new Collider() { Name = "heart", BoundingBox = boundingBox });

      // Type and colors initialization

      faceSprite = new Dictionary<CubeFace, Sprite>();
      for (var i = 0; i <= 15; i += 5)
      {
        faceSprite.Add((CubeFace)i, core.SpriteManager.GetSprite("cube_face_scared_" + i.ToString()));
      }

      faceColor = new Dictionary<CubeFace, MagicColor?>();
      for (var i = 0; i <= 15; i += 5)
      {
        MagicColor? color = null;
        if (ScienceHelper.ChanceRoll(0.8f)) {
          color = MagicManager.GetRandomColor();
        }
        faceColor[(CubeFace)i] = color;
      }

      goRight = true;//ScienceHelper.ChanceRoll();
      anchor = (goRight) ? X : X - 32;

      DirOfTop = CubeFace.Top;
    }

    #region Cube sides
    private int Angle() {
      var angle = (int)DirOfTop + frameN;
      if (!goRight && frameN > 0)
      {
        angle -= 5;
        if (angle < 0)
          angle = 20 + angle;
      }
      return angle;
    }

    private CubeFace NextCubeFace(CubeFace face)
    {
      var newFace = (int)face + 5;
      return newFace <= 15 ? (CubeFace)newFace : CubeFace.Top;
    }

    private CubeFace PrevCubeFace(CubeFace face)
    {
      var newFace = (int)face - 5;
      return newFace >= 0 ? (CubeFace)newFace : CubeFace.Left;
    }
    #endregion

    public override void Update(int ticks)
    {
      var dir = (goRight) ? 1 : -1;
      var speed = movementSpeed * dir;

      if (IsOnPlatform)
      {
        Velocity.X = speed;
      }
      else
      {
        // Complete rotation when falling
        anchor += Velocity.X;
        if (X - anchor >= rollX)
          anchor -= speed;
      }
        
      if (X < anchor)
      {
        anchor = X - width;
      }

      var dx = X - anchor;

      if (dx <= rollX) {
        frameN = 0;
      } else {
        dx -= rollX;
        var newFrameN = (int)Math.Round(dx * 5 / (width - rollX));

        if (frameN != 0 && newFrameN == 0 && !goRight)
          DirOfTop = PrevCubeFace(DirOfTop);

        frameN = newFrameN;
      }

      if (frameN == 5)
      {
        if (goRight)
        {
          frameN = 0;
          anchor = X;
          DirOfTop = NextCubeFace(DirOfTop);
        }
        else
        {
          frameN = 4;
        }
      }
        
      //core.DebugWatch(this, "", (goRight ? ">>" : "<<") + ", Top: " + DirOfTop.ToString() + ", angle = " + Angle().ToString());

      base.Update(ticks);
    }

    public override void Draw()
    {

      // Body
      var frame = frames[frameN];
      core.Renderer[9].DrawSpriteW(frame, new Vector2(anchor, Y - frame.LinkY));
      if (Settings.DrawBoundingBoxes)
      {
        core.Renderer["fg"].DrawDotW(anchor + 32, Y, Color.Red * 0.5f);
        core.Renderer["fg"].DrawDotW(anchor + rollX, Y, Color.Blue * 0.5f);
        core.Renderer["fg"].DrawDotW(anchor - 2, Y, Color.Red);
      }

      // Face
      var dfx = 0;
      var dfy = 0;
      switch (frameN)
      {
        case 0:
          dfx = 5;
          dfy = 7;
          break;
        case 1:
          dfx = 15;
          dfy = 1;
          break;
        case 2:
          dfx = 26;
          dfy = -2;
          break;
        case 3:
          dfx = 37;
          dfy = -3;
          break;
        case 4:
          dfx = 49;
          dfy = 0;
          break;
      }
      var rotationAngle = 0.1f * (float)Math.PI * frameN;
      var faceDir = DirOfTop;

      if (!goRight && frameN > 0)
      {
        faceDir = PrevCubeFace(faceDir);
      }

      core.Renderer[9].DrawSpriteW(faceSprite[faceDir], 
        new Vector2(anchor + dfx, Y - width + dfy), 
        rotation: rotationAngle
      );

      // Marks
      for (var i = 0; i <= 15; i += 5)
      { 
        var face = (CubeFace)i;
        var color = faceColor[face];
        if (color != null)
        {
          var angle = Angle() + i;
          if (angle > 19)
            angle -= 20;
          var mark = core.SpriteManager.GetSprite("cube_marks_" + angle.ToString(), color);
          core.Renderer[9].DrawSpriteW(mark, new Vector2(anchor, Y - frame.LinkY - 1));
        }
      }

      base.Draw();
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
//      if (other is ProjectileActor && ((ProjectileActor)other).color == this.color)
//      {
//        core.MessageManager.Send(new RemoveActorMessage(this), this);
//        core.MessageManager.Send(new AddActorMessage(new SpriteDestroyerActor(core, Position, animation.GetCurrentFrame())), this);
//
//        DropCoin();
//      }

      if (other is PlayerActor)
      {
        (other as PlayerActor).Hurt();
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return actor.CanMove;
    }
  }
}

