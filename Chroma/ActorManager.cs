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

    public void Load()
    {

    }

    public void Unload()
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
      var earth = core.SpriteManager.GetSprite("earth");
      foreach (var actor in actors)
      {
        if (actor is PlatformActor)
        {
          var box = actor.GetBoundingBoxW();

          var offx = Math.Abs(box.Left) % earth.Width;
          if (box.Left < 0)
            offx = earth.Width - offx;

          var offy = Math.Abs(box.Top) % earth.Height;
          if (box.Top < 0)
            offy = earth.Height - offy;
            
          var x = box.Left - offx;
          do
          {
            var y = box.Top - offy;
            do {
              var reducedEarth = earth.Reduce(
                Math.Max(box.Left - x, 0),
                Math.Max(box.Top - y, 0),
                Math.Max(x + earth.Width - box.Right, 0),
                Math.Max(y + earth.Height - box.Bottom, 0)
              );

              var pos = new Vector2(
                Math.Max(x, box.Left), 
                Math.Max(y, box.Top)
              );

              core.Renderer.DrawSpriteW(reducedEarth, pos);

              y += earth.Height;
            } while (y < box.Bottom);

            x += earth.Width;
          } while (x < box.Right);
        }
      }

      foreach (var actor in actors)
      {
        actor.Draw();
      }

      if (Settings.DrawActorsCount)
      {
        core.Renderer.DrawTextS(
          String.Format("actors: {0}", actors.Count),
          new Vector2(10, 15),
          Color.White * 0.25f
        );
      }
    }

    public Actor Add(Actor actor)
    {
      actorsToAdd.Add(actor);

      if (actor is PlayerActor)
      {
        player = actor as PlayerActor;
      }

      return actor;
    }

    public void Remove(Actor actor)
    {
      actorsToRemove.Add(actor);
    }

    public void RemoveOffScreenActors()
    {
      const float criticalDistance = 30.0f;

      var x = player.Position.X;

      foreach (var actor in actors)
      {
        var otherX = actor.GetBoundingBoxW().X + actor.GetBoundingBoxW().Width;
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

      const float G = 0.08f;
      const float DragFactor = 0.99f;

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
        var collider = actor.GetColliderW(i);

        //var actorsInRadius = actorMap.FetchActors(collider.BoundingBox);

        foreach (var other in actors)
        {
          if (actor == other)
          {
            continue;
          }

          for (var j = 0; j < other.GetCollidersCount(); ++j)
          {
            var otherCollider = other.GetColliderW(j);

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

      const float LickStep = 2.0f;

      var v = actor.Velocity;

      // y-axis
      if (Math.Abs(v.Y) > Settings.Eps)
      {
        var obstaclesY = GetObstacles(actor, 0, v.Y);
        if (obstaclesY.Count > 0)
        {
          var minY = (int)Math.Abs(v.Y);
          var box = actor.GetBoundingBoxW();
          foreach (var o in obstaclesY)
          {
            var otherBox = o.GetBoundingBoxW();

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
          var box = actor.GetBoundingBoxW();
          foreach (var o in obstaclesX)
          {
            var otherBox = o.GetBoundingBoxW();

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
          //  actor.Velocity = Vector2.Zero;
            return;
          }

          v.X = minX * Math.Sign(v.X);
        }
      }

      // final check
      if (v.Length() > Settings.Eps && GetObstacles(actor, v.X, v.Y).Count > 0)
      {
        actor.Velocity = v * 0.75f;
        MoveActor(actor);
        return;
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

        if (box.Intersects(other.GetBoundingBoxW()))
        {
          result.Add(other); 
        }
      }

      return result;
    }

    public Actor FindPlatformUnder(Vector2 position)
    {
      Actor closestActor = null;
      var minY = 100500.0f;

      foreach (var a in actors)
      {
        if ((a is PlatformActor) || (a is InvisiblePlatformActor))
        {
          var box = a.GetBoundingBoxW();

          if ((position.X < box.X) || (position.X > box.X + box.Width) || (position.Y > box.Y))
          {
            continue;
          }

          var y = box.Y - position.Y;

          if (y < minY)
          {
            minY = y;
            closestActor = a;
          }
        }
      }

      return closestActor;
    }

    #endregion
  }
}

