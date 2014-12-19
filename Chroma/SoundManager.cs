using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Microsoft.Xna.Framework.Audio;
using Chroma.Messages;

namespace Chroma
{
  sealed class SoundManager
  {
    private readonly Core core; 
    private readonly Dictionary<string, SoundEffect> sounds;

    public SoundManager(Core core)
    {
      this.core = core;

      sounds = new Dictionary<string, SoundEffect>();
    }

    public void Load()
    {
      LoadFromFile("scheme.json");
    }

    public void Unload()
    {
    }

    public void Play(string name)
    {
      Debug.Assert(sounds.ContainsKey(name), String.Format("SoundManager.Play() : Sound {0} is missing", name));

      sounds[name].Play();
    }

    private void LoadFromFile(string fileName)
    {
      JsonValue results;
      using (var streamReader = new StreamReader(String.Format(@"Content/Sounds/{0}", fileName)))
      {
        results = JsonObject.Load(streamReader);
      }

      var array = (JsonArray)results["sounds"];
      foreach (var sound in array)
      {
        var soundName = sound["name"].ToString().Replace("\"", "");
        var soundFileName = sound["fileName"].ToString().Replace("\"", "");

        var soundEffect = core.Content.Load<SoundEffect>(String.Format(@"Sounds/{0}", soundFileName));

        sounds.Add(soundName, soundEffect);
      }
    }
  }
}

