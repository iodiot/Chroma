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
      const float G = 5.0f;
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
      var obstacles = GetObstacles(actor, actor.Velocity.X, actor.Velocity.Y, true);

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
      if (Math.Abs(actor.Velocity.Y) > Settings.Eps)
      {
        var obstaclesY = GetObstacles(actor, 0, actor.Velocity.Y, true);
        if (obstaclesY.Count > 0)
        {
          var minY = (int)Math.Abs(actor.Velocity.Y);
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
            
          v.Y = minY * Math.Sign(actor.Velocity.Y);
        }
      }

      var licking = false;

      // x-axis
      if (Math.Abs(actor.Velocity.X) > Settings.Eps)
      {
        var obstaclesX = GetObstacles(actor, actor.Velocity.X, 0, true);
        if (obstaclesX.Count > 0)
        {
          var minX = (int)Math.Abs(actor.Velocity.X);
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

          v.X = minX * Math.Sign(actor.Velocity.X);
            
          // try to lick
          if (actor.CanLick && minX == 0 && GetObstacles(actor, actor.Velocity.X, -LickStep, false).Count == 0)
          {
            v.X = actor.Velocity.X;
            v.Y = -LickStep;
            licking = true;
          }
        }
      }

      // final check
      if (!licking && v.Length() > Settings.Eps && GetObstacles(actor, v.X, v.Y, true).Count > 0)
      {
        actor.Velocity *= 0.75f;
        MoveActor(actor);
        return;
      }

      actor.Velocity = v;
      actor.Position += v;
    }

    public List<Actor> GetObstacles(Actor actor, float dx, float dy, bool continuous)
    {
      var result = new List<Actor>();

      var box = actor.GetBoundingBox();

     
      if (continuous)
      {
        if (dx < 0)
        {
          box.X = (int)Math.Round(box.X + dx + actor.Position.X);
          box.Width = (int)Math.Round(box.Width - dx);
        }
        else
        {
          box.X = (int)Math.Round(box.X + actor.Position.X);
          box.Width = (int)Math.Round(box.Width + dx);
        }

        if (dy < 0)
        {
          box.Y = (int)Math.Round(box.Y + dy + actor.Position.Y);
          box.Height = (int)Math.Round(box.Height - dy);
        }
        else
        {
          box.Y = (int)Math.Round(box.Y + actor.Position.Y);
          box.Height = (int)Math.Round(box.Height + dy);
        }
      }
      else
      {
        box.X = (int)Math.Round(box.X + dx + actor.Position.X);
        box.Y = (int)Math.Round(box.Y + dy + actor.Position.Y);
      }

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
          //other.TintTtl = 5;
        }
      }

      return result;
    }

    #endregion
  }
}

