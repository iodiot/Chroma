using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chroma.Messages;
using Chroma.Helpers;

namespace Chroma.Actors
{
  public sealed class Collider
  {
    public string Name;
    public Rectangle BoundingBox;
  }
    
  public abstract class Actor : ISubscriber
  {
    #region Fields 
    public Vector2 Position;
    public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
    public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }

    public Vector2 Velocity;

    public string Handle { get; protected set; }

    public bool CanMove { get; set; }
    public bool CanFall { get; protected set; }
    public bool CanLick { get; protected set; }
    public bool CanPassPlatforms { get; protected set; }
    public bool CanBounce { get; protected set; }

    public bool IsSolid { get; protected set; }

    public bool Fell { get; protected set; }
    public bool Drowned { get; protected set; }
    public bool IsOnPlatform;

    private readonly List<Collider> colliders;
    protected Rectangle boundingBox;

    public int Ttl { get; set; }
    public bool IsDead { get { return Ttl == 0; } }

    protected Color boundingBoxColor = Color.Red;

    protected readonly Core core;
    #endregion

    public Actor(Core core, Vector2 position)
    {
      this.core = core;

      Position = position;

      Handle = String.Empty;

      boundingBox = Rectangle.Empty;

      colliders = new List<Collider>();

      Ttl = -1;

      IsSolid = true;
    }
      
    public virtual void Load()
    {

    }

    public virtual void Unload()
    {

    }

    public virtual void Update(int ticks)
    {
      if (Ttl > 0)
      {
        --Ttl;
      }

//      if (Drowned)
//      {
//        Velocity.Y /= 1.2f;
//      }
    }

    public virtual void Draw()
    {
      #if DEBUG
      if (Settings.DrawBoundingBoxes)
      {
        core.Renderer["fg", 1001].DrawRectangleW(GetBoundingBoxW(), boundingBoxColor * 0.25f);
        core.Renderer["fg", 1001].DrawTextW(
          String.Format("{0:X}", GetHashCode() % 1000), 
          new Vector2(GetBoundingBoxW().Left, GetBoundingBoxW().Top),
          Color.White,
          0.33f
        );
      }

      if (Settings.DrawColliders)
      {
        for (var i = 0; i < GetCollidersCount(); ++i)
        {
          core.Renderer["fg", 1000].DrawRectangleW(GetColliderW(i).BoundingBox, Color.Yellow * 0.25f);
        }
      }
      #endif
    }

    public virtual void OnMessage(Message message, object sender)
    {
    }

    #region Physics

    public void SetBoundingBox(float x, float y, float width, float height)
    {
      boundingBox = new Rectangle((int)x, (int)y, (int)width, (int)height);
    }

    public Rectangle GetBoundingBoxW()
    {
      return new Rectangle(
        (int)Math.Round(Position.X + boundingBox.X),
        (int)Math.Round(Position.Y + boundingBox.Y),
        boundingBox.Width,
        boundingBox.Height
      );
    }

    public Rectangle GetBoundingBox()
    {
      return boundingBox;
    }

    public int GetCollidersCount()
    {
      return colliders.Count;
    }

    public Collider GetColliderW(int n)
    {
      var box = colliders[n].BoundingBox;

      var collider = new Collider()
        {
          Name = colliders[n].Name,
          BoundingBox = new Rectangle((int)Position.X + box.X, (int)Position.Y + box.Y, box.Width, box.Height)
        };

      return collider;
    }

    public Collider GetCollider(int n)
    {
      return colliders[n];
    }

    public virtual bool IsPassableFor(Actor actor)
    {
      return false;
    }

    public virtual void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
    }

    public virtual void OnBoundingBoxTrigger(Actor other)
    {
    } 

    public virtual void OnBounce()
    {
    }

    public virtual void OnFall()
    {
      Fell = true;
      CanBounce = true;
    }

    public virtual void OnDrown()
    {
      Drowned = true;
      CanBounce = true;
    }

    public void AddCollider(Collider collider)
    {
      colliders.Add(collider);
    }

    #endregion

    protected void DropCoin(float chance = 1f, Vector2? from = null, int number = 1) 
    {
      if (SciHelper.ChanceRoll(chance))
      {
        for (var i = 0; i < number; ++i)
        {
          core.MessageManager.Send(new AddActorMessage(new CoinActor(core, from ?? Position, true)), this);
        }
      } 
    }
  }
}

