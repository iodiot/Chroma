using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Chroma.Actors;

namespace Chroma
{
  public sealed class ActorManager
  {
    private readonly Core core;
    private readonly List<Actor> actors, actorsToAdd, actorsToRemove;

    private PlayerActor player;

    public ActorManager(Core core)
    {
      this.core = core;

      actors = new List<Actor>();
      actorsToAdd = new List<Actor>();
      actorsToRemove = new List<Actor>();
    }

    public void Initialize()
    {

    }

    public void Uninitialize()
    {
      foreach (var actor in actors)
      {
        actor.Unload();
      }

      actors.Clear();
    }

    public void Update(int ticks)
    {
      if (ticks % 100 == 0)
      {
        RemoveOffScreenActors();
        RemoveDeadActors();
      }

      // remove actors
      foreach (var actor in actorsToRemove)
      {
        actor.Unload();
        actors.Remove(actor);
      }
      actorsToRemove.Clear();

      // add actors
      foreach (var actor in actorsToAdd)
      {
        actor.Load();
        actors.Add(actor);
      }
      actorsToAdd.Clear();

      // update actors
      foreach (var actor in actors)
      {
        actor.Update(ticks);
      }

      Step();
    }

    public void Draw()
    {
      foreach (var actor in actors)
      {
        actor.Draw();
      }

      core.Renderer.DrawTextS(
        String.Format("actors: {0}", actors.Count),
        new Vector2(core.Renderer.ScreenWidth - 70, 12),
        Color.White * 0.25f
      );
    }

    public void Add(Actor actor)
    {
      actorsToAdd.Add(actor);

      if (actor is PlayerActor)
      {
        player = actor as PlayerActor;
      }
    }

    public void Remove(Actor actor)
    {
      actorsToRemove.Add(actor);
    }

    public void RemoveOffScreenActors()
    {
      const float criticalDistance = 25.0f;

      var x = player.Position.X;

      foreach (var actor in actors)
      {
        var otherX = actor.GetWorldBoundingBox().X + actor.GetWorldBoundingBox().Width;
        if (x - otherX > criticalDistance)
        {
          Remove(actor);
        }
      }
    }

    public void RemoveDeadActors()
    {
      foreach (var actor in actors)
      {
        if (actor.IsDead)
        {
          Remove(actor);
        }
      }
    }

    #region Physics

    private void Step()
    {
      const float G = 6.15f;
      const float DragFactor = 0.5f;

      // apply gravity
      foreach (var a in actors)
      {
        if (a.CanMove && a.CanFall)
        {
          a.Velocity.Y += G;
        }
      }

      foreach (var a in actors)
      {
        if (a.CanMove)
        {
          ResolveBoundingBoxes(a);
        }
      }

      foreach (var a in actors)
      {
        if (a.CanMove)
        {
          MoveActor(a);
        }
      }

      foreach (var a in actors)
      {
        if (a.CanMove)
        {
          ResolveColliders(a);
        }
      }

      // drag velocity
      foreach (var a in actors)
      {
        if (a.CanMove)
        {
          a.Velocity *= DragFactor;

          if (a.Velocity.Length() < Settings.Eps)
          {
            a.Velocity = Vector2.Zero;
            continue;
          }
        }
      }
    }

    private void ResolveBoundingBoxes(Actor actor)
    {
      var obstacles = GetObstacles(actor, actor.Velocity.X, actor.Velocity.Y);

      foreach (var obstacle in obstacles)
      {
        actor.OnBoundingBoxTrigger(obstacle);
        obstacle.OnBoundingBoxTrigger(actor);
      }
    }

    private void ResolveColliders(Actor actor)
    {
      for (var i = 0; i < actor.GetCollidersCount(); ++i)
      {
        var collider = actor.GetWorldCollider(i);

        //var actorsInRadius = actorMap.FetchActors(collider.BoundingBox);

        foreach (var other in actors)
        {
          if (actor == other)
          {
            continue;
          }

          for (var j = 0; j < other.GetCollidersCount(); ++j)
          {
            var otherCollider = other.GetWorldCollider(j);

            if (collider.BoundingBox.Intersects(otherCollider.BoundingBox))
            {
              actor.OnColliderTrigger(other, j, i);
              other.OnColliderTrigger(actor, i, j);
            }
          }
        }
      }
    }

    private void MoveActor(Actor actor)
    {
      if (actor.Velocity.Length() < Settings.Eps)
      {
        return;
      }

      const float LickStep = 10.0f;

      var v = actor.Velocity;

      // y-axis
      if (Math.Abs(v.Y) > Settings.Eps)
      {
        var obstaclesY = GetObstacles(actor, 0, v.Y);
        if (obstaclesY.Count > 0)
        {
          var minY = (int)Math.Abs(v.Y);
          var box = actor.GetWorldBoundingBox();
          foreach (var o in obstaclesY)
          {
            var otherBox = o.GetWorldBoundingBox();

            var topBox = box.Y < otherBox.Y ? box : otherBox;
            var bottomBox = box.Y < otherBox.Y ? otherBox : box;

            var y = Math.Abs(topBox.Y + topBox.Height - bottomBox.Y);
            if (y < minY)
            {
              minY = y;
            }
          }
            
          v.Y = minY * Math.Sign(v.Y);
        }
      }

      // x-axis
      if (Math.Abs(v.X) > Settings.Eps)
      {
        var obstaclesX = GetObstacles(actor, v.X, 0);
        if (obstaclesX.Count > 0)
        {
          var minX = (int)Math.Abs(v.X);
          var box = actor.GetWorldBoundingBox();
          foreach (var o in obstaclesX)
          {
            var otherBox = o.GetWorldBoundingBox();

            var leftBox = box.X < otherBox.X ? box : otherBox;
            var rightBox = box.X < otherBox.X ? otherBox : box;

            var x = Math.Abs(rightBox.X - leftBox.X - leftBox.Width);
            if (x < minX)
            {
              minX = x;
            }
          }
                        
          // try to lick
          if (actor.CanLick && minX == 0 && GetObstacles(actor, actor.Velocity.X, -LickStep).Count == 0)
          {
            actor.Position += new Vector2(v.X, -LickStep);
            actor.Velocity = Vector2.Zero;
            return;
          }

          v.X = minX * Math.Sign(v.X);
        }
      }

      // final check
      if (v.Length() > Settings.Eps && GetObstacles(actor, v.X, v.Y).Count > 0)
      {
        Debug.Print("deeper");
        v = Vector2.Zero;
     
        /*actor.Velocity = v * 0.75f;
        MoveActor(actor);
        return;*/
      }

      actor.Velocity = v;
      actor.Position += v;
    }

    public List<Actor> GetObstacles(Actor actor, float dx, float dy)
    {
      var result = new List<Actor>();

      var box = actor.GetBoundingBox();
     
      box.X = (int)Math.Round(box.X + dx + actor.Position.X);
      box.Y = (int)Math.Round(box.Y + dy + actor.Position.Y);

      //var actorsInRadius = actorMap.FetchActors(box);

      foreach (var other in actors)
      {
        if (actor == other || other.IsPassableFor(actor))
        {
          continue;
        }

        if (box.Intersects(other.GetWorldBoundingBox()))
        {
          result.Add(other); 
        }
      }

      return result;
    }

    #endregion
  }
}

