using System;
using Chroma.Helpers;
using Chroma.Actors;
using Microsoft.Xna.Framework;
using Chroma.States;
using System.Collections.Generic;
using System.Linq;

namespace Chroma.Gameplay
{
  public class LevelGenerator
  {
    private readonly ActorManager actorManager;
    private readonly Core core;

    public PlatformActor LastPlatform { get; private set; }
    public int CurrentX { get; private set; }
    public int CurrentY { get; private set; }
    public float distance;
    public int distanceMeters;

    private int coinGrid = 10;
    private List<string> coinPatterns;

    enum LevelModule {
      Flat = 0,

      Raise,
      Descent,

      CliffRight,
      CliffLeft,
      Gap,
      Pond,

      CoinPattern,
      CoinGap
    }

    enum Encounter {
      // Obstacles
      None = 0,
      Boulder,

      // Enemies
      Slime,
      SlimeRoll,
      SlimeWalk,

      Golem
    }

    private int milestone;
    private List<Pair<LevelModule, int>> ModuleRatios;
    private List<Pair<Encounter, int>> EncounterRatios;

    public LevelGenerator(Core core, ActorManager actorManager)
    {
      this.core = core;
      this.actorManager = actorManager;
      LastPlatform = null;
      milestone = -1;

      coinPatterns = new List<string> { "arrows", "ring", "checkers" };

      ModuleRatios = new List<Pair<LevelModule, int>>();
      EncounterRatios = new List<Pair<Encounter, int>>();
    }

    private void ProgressLevel()
    {
      switch (milestone)
      {
        case 0:
          core.DebugMessage("Level started!");
          StartLevel();
          ResetAllRatios();
          SetRatioOf(LevelModule.Flat, 200);
          SetRatioOf(LevelModule.Pond, 200);
          SetRatioOf(LevelModule.Raise, 2);
          SetRatioOf(LevelModule.Descent, 2);
          SetRatioOf(LevelModule.Gap, 1);
          SetRatioOf(LevelModule.CoinGap, 1);
          SetRatioOf(Encounter.None, 100);
          SetRatioOf(Encounter.Golem, 50);
          SetRatioOf(Encounter.Boulder, 20);
          break;
        case 200:
          SetRatioOf(LevelModule.CliffRight, 1);
          SetRatioOf(Encounter.SlimeWalk, 10);
          break;
        case 400:
          SetRatioOf(LevelModule.CoinPattern, 1);
          SetRatioOf(Encounter.Golem, 20);
          SetRatioOf(Encounter.SlimeWalk, 50);
          break;
        case 600:
          SetRatioOf(LevelModule.CoinPattern, 1);
          SetRatioOf(Encounter.Golem, 0);
          SetRatioOf(Encounter.SlimeWalk, 50);
          break;
      }
    }

    public void Update(float distance)
    {
      this.distance = distance;
      this.distanceMeters = (int)(distance / 10);

      var latestMilestone = (int)(distance / 100) * 10;
      if (milestone < latestMilestone)
      {
        milestone = latestMilestone;
        ProgressLevel();
        SortRatios();
      }

      if (CurrentX < distance + core.Renderer.ScreenWidth + 100)
      {
        ExtendLevel();
      }
    }

    public void StartLevel() 
    {
      CurrentX = CurrentY = 0;

      SpawnFlat((int)core.Renderer.ScreenWidth + 10);
    }

    private void ResetAllRatios()
    {
      ResetRatios<LevelModule>(ModuleRatios);
      ResetRatios<Encounter>(EncounterRatios);
    }

    private void ResetRatios<T>(List<Pair<T, int>> Ratios)
    {
      Ratios.Clear();
      foreach (T item in Enum.GetValues(typeof(T))) {
        Ratios.Add(new Pair<T, int>(item, 0));
      }
    }

    private void SortRatios() 
    {
      ModuleRatios.Sort((a, b) => a.B.CompareTo(b.B));
      EncounterRatios.Sort((a, b) => a.B.CompareTo(b.B));
    }

    private void SetRatioOf(LevelModule module, int ratio)
    {
      ModuleRatios.Find(x => x.A == module).B = ratio;
    }

    private void SetRatioOf(Encounter encounter, int ratio)
    {
      EncounterRatios.Find(x => x.A == encounter).B = ratio;

      core.DebugMessage(String.Format("{0} -> {1}", encounter, ratio));
    }

    private T GetRandom<T>(List<Pair<T, int>> Ratios)
    {
      var total = Ratios.Sum(x => x.B);
      var roll = core.GetRandom(1, total);
      var i = -1;
      var sum = 0;
      do
      {
        i++;
        sum += Ratios[i].B;
      } while (sum < roll && i < Ratios.Count - 1);
      return Ratios[i].A;
    }

    private void ExtendLevel() 
    {
      SpawnModule(GetRandom<LevelModule>(ModuleRatios));
    }

    //--------------------------------------------------

    #region Basic secions
    private void AttachToLast(PlatformActor newPlatform)
    {
      if (LastPlatform != null)
      {
        newPlatform.PreviousPlatform = LastPlatform;
        LastPlatform.NextPlatform = newPlatform;
      }
      LastPlatform = newPlatform;
    }

    private void SpawnFlat(int length = 30, int elevation = 0)
    {
      CurrentY += elevation;

      var newPlatform = new FlatPlatformActor(core, new Vector2(CurrentX, CurrentY), length);
      actorManager.Add(newPlatform);
      AttachToLast(newPlatform);

      CurrentX += length;
    }

    private void SpawnSlope(SlopeDirection direction, int sections = 1, int elevation = 0)
    {
      CurrentY += elevation;

      var newSlope = new SlopedPlatformActor(core, new Vector2(CurrentX, CurrentY), direction, sections);
      actorManager.Add(newSlope);
      AttachToLast(newSlope);

      CurrentY = (int)newSlope.RightY;
      CurrentX += newSlope.Width;
    }

    private void SpawnGap(int length)
    {
      CurrentX += length;
      LastPlatform = null;
    }

    private void SpawnWater(int width)
    {
      WaterBodyActor newWater = new WaterBodyActor(core, new Vector2(CurrentX, CurrentY + 30), width);
      actorManager.Add(newWater);
      LastPlatform = null;

      CurrentX += width;
    }
    #endregion

    //--------------------------------------------------

    #region Encounters
    private void SpawnEncounter(Encounter encounter)
    {
      var width = 30;

      var position = new Vector2(CurrentX + 15, CurrentY);
      switch (encounter)
      {
        case Encounter.None:
          break;

        case Encounter.Boulder:
          var newBoulder = new BoulderActor(core, position);
          width += newBoulder.GetBoundingBoxW().Width;
          actorManager.Add(newBoulder);
          break;

        case Encounter.Golem:
          var newGolem = new GolemActor(core, position, MagicManager.GetRandomColor(core, 0.2f));
          width += newGolem.GetBoundingBoxW().Width;
          actorManager.Add(newGolem);
          break;

        case Encounter.SlimeWalk:
          var newSlimeWalker = new SlimeWalkActor(core, position, MagicManager.GetRandomColor(core, 0.2f));
          width += newSlimeWalker.GetBoundingBoxW().Width;
          actorManager.Add(newSlimeWalker);
          break;
      }

      SpawnFlat(width);
    }
    #endregion

    //--------------------------------------------------

    #region Modules
    private void SpawnModule(LevelModule module)
    {
      switch (module)
      {
        case LevelModule.Flat:
          var encounter = GetRandom<Encounter>(EncounterRatios);
          SpawnEncounter(encounter);
          break;

        case LevelModule.Raise:
          SpawnSlope(SlopeDirection.Up, core.GetRandom(1,2));
          SpawnFlat(30);
          break;
        case LevelModule.Descent:
          SpawnSlope(SlopeDirection.Down, core.GetRandom(1,2));
          SpawnFlat(30);
          break;

        case LevelModule.CliffRight:
          SpawnSlope(SlopeDirection.Up, 2);
          SpawnFlat(200, core.GetRandom(60, 100));
          break;

        case LevelModule.CliffLeft:
          SpawnSlope(SlopeDirection.Down, 2, -30);
          SpawnFlat(30);
          break;

        case LevelModule.Gap:
          SpawnGap(50);
          break;

        case LevelModule.Pond:
          SpawnFlat(30);
          SpawnWater(100);
          SpawnFlat(30);
          break;

        case LevelModule.CoinPattern:
          var pattern = "cp_" + coinPatterns[core.GetRandom(0, coinPatterns.Count - 1)];
          var sprite = core.SpriteManager.GetSprite(pattern);
          SpawnCoinPattern(CurrentX, CurrentY - coinGrid, pattern);
          SpawnFlat(sprite.Width * coinGrid);
          break;



        case LevelModule.CoinGap:
          SpawnFlat(40);
          SpawnCoinArc(CurrentX - 30, CurrentY - 15);
          SpawnGap(90);
          SpawnFlat(50);
          break;
      }
    }
    #endregion

    //--------------------------------------------------

    #region Coin clusters
    private void SpawnCoin(int x, int y)
    {
      var newCoin = new CoinActor(core, new Vector2(x, y));
      actorManager.Add(newCoin);
    }
      
    private void SpawnCoinRect(int x, int y, int width, int height)
    {
      for (var i = 0; i < width / 15; i++)
      {
        for (var j = 0; j < height / 15; j++)
        {
          SpawnCoin(x + i * 15, y + j * 15);
        }
      }
    }

    private void SpawnCoinArc(int x, int y)
    {
      float cx = x;
      float cy = y;
      var vx = 2.2f;
      var vy = -3.0f;
      var coins = 0;

      do
      {
        SpawnCoin((int)cx, (int)cy);
        coins++;

        for (var i = 0; i <= 5; i++)
        {
          cx += vx;
          cy += vy;
          vy += 0.08f;
          vy *= 0.99f;
        }
          
      } while (coins < 12);
    }

    private void SpawnCoinPattern(int x, int y, string pattern)
    {
      var sprite = core.SpriteManager.GetSprite(pattern);
      var texture = core.SpriteManager.GetTexture(sprite.TextureName);
      var textureData = core.SpriteManager.GetTextureData(sprite.TextureName);

      for (var j = sprite.Y; j < sprite.Y + sprite.Height; j++)
      {
        for (var i = sprite.X; i < sprite.X + sprite.Width; i++)
        {
          var color = textureData[i + j * texture.Width];
          if (color == Color.Black)
          {
            SpawnCoin(x + (i - sprite.X) * coinGrid, y - (coinGrid * sprite.Height) + (j - sprite.Y) * coinGrid);
          }
        }
      }
    }

    #endregion

    //--------------------------------------------------
  }
}

