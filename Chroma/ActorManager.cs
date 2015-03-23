using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Chroma.Actors;
using Chroma.Graphics;
using Chroma.Helpers;
using System.Diagnostics;
using Chroma.States;

namespace Chroma
{
  public sealed class ActorManager
  {
    private readonly Core core;
    private readonly PlayState playState;
    private readonly List<Actor> actors, actorsToAdd, actorsToRemove;

    public ActorManager(Core core, PlayState playState)
    {
      this.core = core;
      this.playState = playState;

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
      // Remove actors
      if (ticks % 100 == 0)
      {
        actorsToRemove.AddRange(actors.FindAll(CanRemoveActor));
      }

      foreach (var actor in actorsToRemove)
      {
        actor.Unload();
        actors.Remove(actor);
      }
      actorsToRemove.Clear();


      // Add actors
      foreach (var actor in actorsToAdd)
      {
        actor.Load();
        actors.Add(actor);
      }
      actorsToAdd.Clear();

      // Update actors
      foreach (var actor in actors)
      {
        actor.Update(ticks);
      }

      Step();
    }

    public void Draw()
    {
      var tile = playState.LevelGenerator.GetGroundSprite();
      foreach (var actor in actors)
      {
        if (actor is PlatformActor)
        {
          var box = actor.GetBoundingBoxW();
          core.Renderer.DrawSpriteTiledW(tile, box, tileOffset: new Vector2(box.Left, box.Top));
        }
      }

      foreach (var actor in actors)
      {
        actor.Draw();
      }

      core.DebugWatch("actors", actors.Count.ToString());
    }

    public Actor Add(Actor actor)
    {
      actorsToAdd.Add(actor);

      return actor;
    }

    public void Remove(Actor actor)
    {
      actorsToRemove.Add(actor);
    }

    private bool CanRemoveActor(Actor actor)
    {
      if (actor.IsDead)
      {
        return true;
      }

      const float CriticalDistance = 30.0f;

      var otherX = actor.GetBoundingBoxW().X + actor.GetBoundingBoxW().Width;
      return ((-core.Renderer.World.X) - otherX > CriticalDistance);
    }

    #region Physics
    private void Step()
    {
      const float G = 0.12f;
      const float DragFactor = 0.99f;

      // Apply gravity
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
        ResolveColliders(a);
      }

      // Drag velocity
      foreach (var a in actors)
      {
        if (a.CanMove)
        {
          a.Velocity *= DragFactor;

          if (SciHelper.IsZero(a.Velocity))
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
            }
          }
        }
      }
    }

    private void MoveActor(Actor actor)
    {
      if (SciHelper.IsZero(actor.Velocity))
      {
        return;
      }

      const float LickStep = 2.0f;

      var v = actor.Velocity;
      var bounced = false;

      // y-axis
      if (Math.Abs(v.Y) > SciHelper.Eps)
      {
        actor.IsOnPlatform = false;
        var dy = (actor.CanLick && v.Y >= 0f) ? Math.Max(LickStep * 1.5f, v.Y) : v.Y;

        var obstaclesY = GetObstacles(actor, 0, dy);
        if (obstaclesY.Count > 0)
        {
          var minY = (int)Math.Abs(dy);
          var box = actor.GetBoundingBoxW();
          foreach (var o in obstaclesY)
          {
            var otherBox = o.GetBoundingBoxW();

            var topBox = box.Y < otherBox.Y ? box : otherBox;
            var bottomBox = box.Y < otherBox.Y ? otherBox : box;

            minY = Math.Min(minY, Math.Abs(topBox.Y + topBox.Height - bottomBox.Y));
          }

          // Try to down-lick
          if (actor.CanLick && minY > Math.Abs(actor.Velocity.Y))
          {
            actor.IsOnPlatform = true;
            actor.Position += new Vector2(v.X, minY);
            return;
          }

          if (actor.CanBounce)
          {
            v.Y = -v.Y * 0.5f;
            bounced = Math.Abs(v.Y) > 0.2f;
          }
          else
          {
            v.Y = minY * Math.Sign(dy);
            actor.IsOnPlatform = minY == 0;
          }
        }
      }

      // x-axis
      if (Math.Abs(v.X) > SciHelper.Eps)
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

            minX = Math.Min(minX, Math.Abs(rightBox.X - leftBox.X - leftBox.Width));
          }
                        
          // Try to up-lick
          if (actor.CanLick && minX == 0 && GetObstacles(actor, actor.Velocity.X, -LickStep).Count == 0)
          {
            actor.Position += new Vector2(v.X, -LickStep);
            return;
          }

          if (actor.CanBounce)
          {
            v.X = -v.X * 0.5f;
            bounced = Math.Abs(v.X) > 0;
          }
          else
          {
            v.X = minX * Math.Sign(v.X);
          }
        }
      }

      if (bounced)
      {
        actor.OnBounce();
      }

      // Final check
      if (!SciHelper.IsZero(v) && GetObstacles(actor, v.X, v.Y).Count > 0)
      {
        core.DebugMessage("final check");
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
        if (actor == other || other.IsPassableFor(actor) && other.GetBoundingBox() != Rectangle.Empty)
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

    public PlatformActor FindPlatformUnder(Vector2 position)
    {
      PlatformActor closestActor = null;
      var minY = SciHelper.BigFloat;

      foreach (var a in actors)
      {
        if (a is PlatformActor)
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
            closestActor = a as PlatformActor;
          }
        }
      }

      return closestActor;
    }
    #endregion
  }
}

