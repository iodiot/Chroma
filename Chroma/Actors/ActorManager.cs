using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
      foreach (var actor in actors)
      {
        if ((actor !=player) && (actor is BodyActor))
        {
          var rect = (actor as BodyActor).GetBoundingRect();

          if (rect.Intersects(player.GetBoundingRect()))
          {
            player.OnCollide(actor);
            (actor as BodyActor).OnCollide(player);
          }
        }
      }
    }

    public void SpawnCoin()
    {
      Add(new CoinActor(core, new Vector2(100, 25)));
    }
  }
}

