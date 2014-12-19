using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Chroma.Actors;

namespace Chroma.Actors
{
  sealed class ActorManager
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
      if (ticks % 1000 == 0)
      {
        RemoveOffScreenActors();
      }

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
      var bodies = actors
        .Where(actor => (actor is BodyActor) && !(actor is PlayerActor))
        .Select(actor => actor as BodyActor);

      foreach (var body in bodies)
      {
        var rect = body.GetBoundingRect();

        if (rect.Intersects(player.GetBoundingRect()))
        {
          player.OnCollide(body);
          body.OnCollide(player);
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
        if (actor is BodyActor)
        {
          var otherX = (actor as BodyActor).X;
          if (x - otherX > criticalDistance)
          {
            Remove(actor);
            ++n;
          }
        }
      }

      if (n > 0)
      {
        Log.Print(String.Format("{0} offscreen actors were removed", n));
      }
    }
  }
}

