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
  public enum Area {
    Jungle,
    Ruins
  }

  public partial class LevelGenerator
  {
    private readonly ActorManager actorManager;
    private readonly Core core;

    private Area area;

    // Background
    private List<ParallaxLayer> BG;

    // Platforms
    public PlatformActor LastPlatform { get; private set; }
    public int CurrentX { get; private set; }
    public int CurrentY { get; private set; }
    public float distance;
    public int distanceMeters;

    private bool builtStartingScene = false;
    private bool startedLevel = false;

    enum LevelModule {
      Flat = 0,

      Raise,
      Descent,

      CliffRight,
      CliffLeft,
      Gap,
      Pond,

      CoinGap
    }

    private int milestone;
    private List<Pair<LevelModule, int>> ModuleRatios;
    private List<Pair<Encounter, int>> EncounterRatios;

    public LevelGenerator(Core core, ActorManager actorManager, Area area)
    {
      this.core = core;
      this.actorManager = actorManager;
      this.area = area;
      LastPlatform = null;
      milestone = -1;

      ModuleRatios = new List<Pair<LevelModule, int>>();
      EncounterRatios = new List<Pair<Encounter, int>>();

      // Background
      BG = new List<ParallaxLayer>();
      BG.Add(new ParallaxLayer(core, 31, 0.1f));
      BG.Add(new ParallaxLayer(core, 31, 0.2f));
      BG.Add(new ParallaxLayer(core, 17, 0.5f));
      BG.Add(new ParallaxLayer(core, 0, 0.7f));

      LoadBGTape(1, new string[] { "trees_l1" });
      LoadBGTape(2, new string[] { "trees_l2" });
      LoadBGTape(3, new string[] { "trees_l3_1", "trees_l3_2", "trees_l3_3" });
      LoadBGTape(4, new string[] { "trees_l4" });
    }

    //--------------------------------------------------
    #region Backgrounds

    private void SpawnParallaxDecal()
    {
      var lvl = ScienceHelper.GetRandom(7, 9);
      var newDecal = new ParallaxDecalActor(core, new Vector2(CurrentX, CurrentY - 90), new Vector2(lvl * 0.1f, 1), 
        "trees_l5_" + ScienceHelper.GetRandom(1, 4).ToString(), "bg", lvl, scale: 1.0f);
      core.MessageManager.Send(new AddActorMessage(newDecal), this);
    }

    private void LoadBGTape(int layerId, string[] tape) 
    {
      var layer = BG[layerId - 1];
      layer.LoadTape(tape);
    }

    public void DrawBackground()
    {
      foreach (var layer in BG)
      {
        layer.Draw();
      }
    }

    #endregion
    //--------------------------------------------------

    public void Go()
    {
      startedLevel = true;
    }

    public void Update(float distance)
    {
      if (!builtStartingScene)
      {
        StartLevel();
        builtStartingScene = true;
      }
        
      foreach (var layer in BG)
      {
        layer.Update();
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

    public void StartLevel() 
    {
      CurrentX = -10;
      CurrentY = 0;

      // TODO: Spawn start scene
      SpawnFlat((int)core.Renderer.ScreenWidth + 20);
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

    private void ExtendLevel() 
    {
      SpawnModule(GetRandom<LevelModule>(ModuleRatios));
    }

    //--------------------------------------------------

    #region Basic sections
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

      if (ScienceHelper.ChanceRoll(0.2f))
        SpawnParallaxDecal();

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
          SpawnGap(50);
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
    private void SpawnCoin(int x, int y)
    {
      var newCoin = new CoinActor(core, new Vector2(x, y));
      actorManager.Add(newCoin);
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

    #endregion

    //--------------------------------------------------
  }
}

