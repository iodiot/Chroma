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
    private List<Actor> actors, actorsToAdd, actorsToRemove;

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
        actorsToRemove.AddRange(actors.FindAll(actor => CanRemoveActor(actor)));
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
      var tileWidth = tile.Width;
      var tileHeight = tile.Height;
      foreach (var actor in actors)
      {
        if (actor is PlatformActor)
        {
          var box = actor.GetBoundingBoxW();

          var offx = Math.Abs(box.Left) % tileWidth;
          if (box.Left < 0)
            offx = tileWidth - offx;

          var offy = Math.Abs(box.Top) % tileHeight;
          if (box.Top < 0)
            offy = tileHeight - offy;
            
          var x = box.Left - offx;
          do
          {
            var y = box.Top - offy;
            do {
              var reducedTile = tile.Reduce(
                Math.Max(box.Left - x, 0),
                Math.Max(box.Top - y, 0),
                Math.Max(x + tileWidth - box.Right, 0),
                Math.Max(y + tileHeight - box.Bottom, 0)
              );

              var pos = new Vector2(
                Math.Max(x, box.Left), 
                Math.Max(y, box.Top)
              );

              core.Renderer.DrawSpriteW(reducedTile, pos);

              y += tileHeight;
            } while (y < box.Bottom);

            x += tileWidth;
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
      const float G = 0.08f;
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

          if (ScienceHelper.IsZero(a.Velocity))
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
      if (ScienceHelper.IsZero(actor.Velocity))
      {
        return;
      }

      const float LickStep = 2.0f;

      var v = actor.Velocity;

      // y-axis
      if (Math.Abs(v.Y) > ScienceHelper.Eps)
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
      if (Math.Abs(v.X) > ScienceHelper.Eps)
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
                        
          // Try to lick
          if (actor.CanLick && minX == 0 && GetObstacles(actor, actor.Velocity.X, -LickStep).Count == 0)
          {
            actor.Position += new Vector2(v.X, -LickStep);
            return;
          }

          v.X = minX * Math.Sign(v.X);
        }
      }

      // Final check
      if (!ScienceHelper.IsZero(v) && GetObstacles(actor, v.X, v.Y).Count > 0)
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

    public Actor FindPlatformUnder(Vector2 position)
    {
      Actor closestActor = null;
      var minY = ScienceHelper.BigFloat;

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

