using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Chroma.Actors;

namespace Chroma.Actors
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
      }

      RemoveDeadActors();

      foreach (var actor in actorsToRemove)
      {
        actor.Unload();
        actors.Remove(actor);
      }
      actorsToRemove.Clear();

      foreach (var actor in actorsToAdd)
      {
        actor.Load();
        actors.Add(actor);
      }
      actorsToAdd.Clear();

      CheckCollisions();

      foreach (var actor in actors)
      {
        actor.Update(ticks);
      }
    }

    public void Draw()
    {
      foreach (var actor in actors)
      {
        actor.Draw();
      }

      core.Renderer.DrawTextS(
        String.Format("actors: {0}", actors.Count),
        new Vector2(core.Renderer.ScreenWidth - 75, 12),
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

    public void CheckCollisions()
    {
      var what = actors
        .Where(actor => (actor is PlayerActor) || (actor is ProjectileActor))
        .Select(actor => actor as CollidableActor);

      var with = actors
        .Where(actor => (actor is CollidableActor) && !(actor is PlayerActor) && !(actor is ProjectileActor))
        .Select(actor => actor as CollidableActor);

      foreach (var a in what)
      {
        var rectA = a.GetBoundingBox();

        foreach (var b in with)
        {
          var rectB = b.GetBoundingBox();

          if (rectA.Intersects(rectB))
          {
            a.OnCollide(b);
            b.OnCollide(a);
          }
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

    public void RemoveOffScreenActors()
    {
      const float criticalDistance = 25.0f;

      var x = player.Position.X;
      var n = 0;

      foreach (var actor in actors)
      {
        if (x - actor.X > criticalDistance)
        {
          Remove(actor);
          ++n;
        }
      }

      //if (n > 0)
      //{
      //  Log.Print(String.Format("{0} offscreen actors were removed", n));
      //}
    }
  }
}

