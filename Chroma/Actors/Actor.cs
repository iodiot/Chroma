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

    public bool CanMove { get; protected set; }
    public bool CanFall { get; protected set; }
    public bool CanLick { get; protected set; }

    private readonly List<Collider> colliders;
    protected Rectangle boundingBox;

    public int Ttl { get; protected set; }
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
    }

    public virtual void Draw()
    {
      if (Settings.DrawBoundingBoxes)
      {
        core.Renderer.DrawRectangleW(GetBoundingBoxW(), boundingBoxColor * 0.25f);
      }

      if (Settings.DrawColliders)
      {
        for (var i = 0; i < GetCollidersCount(); ++i)
        {
          core.Renderer.DrawRectangleW(GetColliderW(i).BoundingBox, Color.Yellow * 0.25f);
        }
      }
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

    public void AddCollider(Collider collider)
    {
      colliders.Add(collider);
    }

    #endregion

    protected void DropCoin(float chance = 1f, Vector2? from = null) 
    {
      if (ScienceHelper.ChanceRoll(chance))
      {
        core.MessageManager.Send(new AddActorMessage(new CoinActor(core, from ?? Position, true)), this);
      } 
    }
  }
}

