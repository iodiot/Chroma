using System;
using Chroma.Helpers;
using Chroma.Actors;
using Microsoft.Xna.Framework;
using Chroma.States;
using Chroma.Graphics;
using System.Collections.Generic;
using System.Linq;
using Chroma.Messages;

namespace Chroma.Gameplay
{
  public enum Area 
  {
    Jungle = 0,
    Ruins
  }

  public abstract class LevelGenerator
  {
    protected readonly ActorManager actorManager;
    protected readonly Core core;

    protected Area area;

    // Background
    protected Color bgColor;
    protected List<ParallaxLayer> BG;
    protected List<ParallaxDecal> BGDecals;

    // Platforms
    public PlatformActor LastPlatform { get; private set; }
    public int CurrentX { get; private set; }
    public int CurrentY { get; private set; }
    public float distance;
    public int distanceMeters;

    protected bool builtStartingScene = false;
    protected bool startedLevel = false;

    protected enum LevelModule {
      Flat = 0,

      Raise,
      Descent,

      CliffRight,
      CliffLeft,
      Gap,
      Pond,

      CoinGap
    }

    protected int milestone;
    protected List<Pair<LevelModule, int>> ModuleRatios;
    protected List<Pair<Encounter, int>> EncounterRatios;

    protected LevelGenerator(Core core, ActorManager actorManager, Area area)
    {
      this.core = core;
      this.actorManager = actorManager;
      this.area = area;
      LastPlatform = null;
      milestone = -1;

      ModuleRatios = new List<Pair<LevelModule, int>>();
      EncounterRatios = new List<Pair<Encounter, int>>();

      bgColor = Color.Black;

      BG = new List<ParallaxLayer>();
      BGDecals = new List<ParallaxDecal>();
    }

    public static LevelGenerator Create(Core core, ActorManager actorManager, Area area) 
    {
      switch (area)
      {
        default:
        case Area.Jungle:
          return new JungleLevelGenerator(core, actorManager, area);
        case Area.Ruins:
          return new RuinsLevelGenerator(core, actorManager, area);
      }
    }
      
    //--------------------------------------------------

    protected abstract void ProgressLevel();
          
    //--------------------------------------------------
    protected enum Encounter {

      None = 0,

      // Obstacles ------
      Boulder,
      Spikes,

      // Items ----------
      HealthItem,

      //Structures ------
      Bridge,

      // Enemies --------

      // Jungle
      Slime,
      SlimeRoll,
      SlimeWalk,
      Plant,
      // Ruins,
      Golem,
      Zapper
    }
      
    protected void SpawnEncounter(Encounter encounter)
    {
      var width = 30;

      var position = new Vector2(CurrentX + 15, CurrentY);
      switch (encounter)
      {
        case Encounter.None:
          break;

          #region Items
        case Encounter.HealthItem:
          position.Y -= 50;
          var newItem = new ItemActor(core, position);
          actorManager.Add(newItem);
          break;
          #endregion

          #region Objects
        case Encounter.Boulder:
          var newBoulder = new BoulderActor(core, position);
          width += newBoulder.GetBoundingBoxW().Width;
          actorManager.Add(newBoulder);
          break;
        case Encounter.Spikes:
          var newSpikes = new SpikesActor(core, position, ScienceHelper.GetRandom(50, 80));
          width += newSpikes.GetBoundingBoxW().Width;
          actorManager.Add(newSpikes);
          break;
          #endregion

          #region Structures
        case Encounter.Bridge:
          //SpawnBridge();
          break;
          #endregion

          #region Enemies
        case Encounter.Slime:
          var newSlime = new SlimeActor(core, position, MagicManager.GetRandomColor(0.2f), false);
          width += newSlime.GetBoundingBoxW().Width;
          actorManager.Add(newSlime);
          break;

        case Encounter.SlimeRoll:
          var newSlimeRoller = new SlimeActor(core, position, MagicManager.GetRandomColor(0.2f), true);
          width += newSlimeRoller.GetBoundingBoxW().Width;
          actorManager.Add(newSlimeRoller);
          break;

        case Encounter.SlimeWalk:
          var newSlimeWalker = new SlimeWalkActor(core, position, MagicManager.GetRandomColor(0.2f));
          width += newSlimeWalker.GetBoundingBoxW().Width;
          actorManager.Add(newSlimeWalker);
          break;

        case Encounter.Plant:
          var newPlant = new PlantActor(core, position);
          width += 70;
          actorManager.Add(newPlant);
          break;

        case Encounter.Golem:
          var newGolem = new GolemActor(core, position, MagicManager.GetRandomColor(0.2f));
          width += newGolem.GetBoundingBoxW().Width;
          actorManager.Add(newGolem);
          break;

        case Encounter.Zapper:
          var newZapper = new ZapperActor(core, position, MagicManager.GetRandomColor(0.2f));
          actorManager.Add(newZapper);
          break;
          #endregion
      }

      SpawnFlat(width);
    }

    protected void SpawnBridge(int length)
    {
//      var n = ScienceHelper.GetRandom(10, 30);
//      var x = CurrentX;
//      var y = CurrentY - ScienceHelper.GetRandom(30, 50);
//
//      for (var i = 1; i <= n; i++)
//      {
//        PlankActor newPlank = new PlankActor(core, new Vector2(x, y), 
//          area,
//          (i == 1) ? PlankActor.PlankOrigin.Left :
//          (i == n) ? PlankActor.PlankOrigin.Right:
//          PlankActor.PlankOrigin.Middle
//        );
//        actorManager.Add(newPlank);
//        x += newPlank.Width;
//        //y += core.GetRandom(-2, 2);
//      }
    }

    //--------------------------------------------------
    #region Backgrounds

    protected void LoadBGTape(int layerId, string[] tape) 
    {
      var layer = BG[layerId - 1];
      layer.LoadTape(tape);
    }

    protected void PlaceDecal(ParallaxDecal decal)
    {
      BGDecals.Add(decal);
    }

    public virtual void DrawBackground()
    {
      core.GraphicsDevice.Clear(bgColor);

      foreach (var layer in BG)
      {
        layer.Draw();
      }

      foreach (var decal in BGDecals)
      {
        decal.Draw();
      }
    }

    public abstract Sprite GetGroundSprite();

    #endregion
    //--------------------------------------------------

    public void Go()
    {
      startedLevel = true;
    }

    public virtual void Update(float distance)
    {
      if (!builtStartingScene)
      {
        CurrentX = -10;
        CurrentY = 0;

        StartLevel();
        builtStartingScene = true;
      }
        
      foreach (var layer in BG)
      {
        layer.Update();
      }
        
      foreach (var decal in BGDecals)
      {
        decal.Update();
      }

      for (var i = BGDecals.Count - 1; i >= 0; i--)
      {
        if (BGDecals[i].IsDead())
          BGDecals.RemoveAt(i);
      }

      if (!startedLevel)
        return;

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

    protected virtual void StartLevel() 
    {
      SpawnFlat((int)core.Renderer.ScreenWidth + 20);
    }

    protected void ResetAllRatios()
    {
      ResetRatios<LevelModule>(ModuleRatios);
      ResetRatios<Encounter>(EncounterRatios);
    }

    protected void ResetRatios<T>(List<Pair<T, int>> Ratios)
    {
      Ratios.Clear();
      foreach (T item in Enum.GetValues(typeof(T))) 
      {
        Ratios.Add(new Pair<T, int>(item, 0));
      }

      if (Ratios.Count > 0)
      {
        Ratios[0].B = 1;
      }
    }

    protected void SortRatios() 
    {
      ModuleRatios.Sort((a, b) => a.B.CompareTo(b.B));
      EncounterRatios.Sort((a, b) => a.B.CompareTo(b.B));
    }

    protected void SetRatioOf(LevelModule module, int ratio)
    {
      ModuleRatios.Find(x => x.A == module).B = ratio;
    }

    protected void SetRatioOf(Encounter encounter, int ratio)
    {
      EncounterRatios.Find(x => x.A == encounter).B = ratio;

      core.DebugMessage(String.Format("{0} -> {1}", encounter, ratio));
    }

    protected T GetRandom<T>(List<Pair<T, int>> Ratios)
    {
      var total = Ratios.Sum(x => x.B);
      var roll = ScienceHelper.GetRandom(1, total);
      var i = -1;
      var sum = 0;
      do
      {
        i++;
        sum += Ratios[i].B;
      } while (sum < roll && i < Ratios.Count - 1);
      return Ratios[i].A;
    }

    protected void ExtendLevel() 
    {
      SpawnModule(GetRandom<LevelModule>(ModuleRatios));
    }

    //--------------------------------------------------

    #region Basic sections
    protected void AttachToLast(PlatformActor newPlatform)
    {
      if (LastPlatform != null)
      {
        newPlatform.PreviousPlatform = LastPlatform;
        LastPlatform.NextPlatform = newPlatform;
      }
      LastPlatform = newPlatform;
    }

    protected void SpawnFlat(int length = 30, int elevation = 0)
    {
      CurrentY += elevation;

      var newPlatform = FlatPlatformActor.Create(core, new Vector2(CurrentX, CurrentY), length, area);
      actorManager.Add(newPlatform);
      AttachToLast(newPlatform);

      CurrentX += newPlatform.width;
    }

    protected void SpawnSlope(SlopeDirection direction, int sections = 1, int elevation = 0)
    {
      CurrentY += elevation;

      var newSlope = SlopedPlatformActor.Create(core, new Vector2(CurrentX, CurrentY), direction, area, sections);
      actorManager.Add(newSlope);
      AttachToLast(newSlope);

      CurrentY = (int)newSlope.RightY;
      CurrentX += newSlope.Width;
    }

    protected void SpawnGap(int length)
    {

      var newGap = new GapActor(core, new Vector2(CurrentX, CurrentY), length, area);
      actorManager.Add(newGap);

      CurrentX += length;
      LastPlatform = null;
    }

    protected void SpawnWater(int width)
    {
      WaterBodyActor newWater = new WaterBodyActor(core, new Vector2(CurrentX, CurrentY), width, area);
      actorManager.Add(newWater);
      LastPlatform = null;

      CurrentX += width;
    }
    #endregion

    //--------------------------------------------------

    #region Modules
    protected void SpawnModule(LevelModule module)
    {
      switch (module)
      {
        case LevelModule.Flat:
          var encounter = GetRandom<Encounter>(EncounterRatios);
          SpawnEncounter(encounter);
          break;

        case LevelModule.Raise:
          SpawnSlope(SlopeDirection.Up, ScienceHelper.GetRandom(1,2));
          SpawnFlat(30);
          break;
        case LevelModule.Descent:
          SpawnSlope(SlopeDirection.Down, ScienceHelper.GetRandom(1,2));
          SpawnFlat(30);
          break;

        case LevelModule.CliffRight:
          SpawnSlope(SlopeDirection.Up, 2);
          SpawnFlat(200, ScienceHelper.GetRandom(60, 100));
          break;

        case LevelModule.CliffLeft:
          SpawnSlope(SlopeDirection.Down, 2, -30);
          SpawnFlat(30);
          break;

        case LevelModule.Gap:
          SpawnGap(ScienceHelper.GetRandom(50, 100));
          SpawnFlat(50);
          break;

        case LevelModule.Pond:
          SpawnFlat(30);
          SpawnWater(100);
          SpawnFlat(30);
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
    protected void SpawnCoin(int x, int y)
    {
      var newCoin = new CoinActor(core, new Vector2(x, y));
      actorManager.Add(newCoin);
    }

    protected void SpawnCoinArc(int x, int y)
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

    #endregion

    //--------------------------------------------------
  }
}

