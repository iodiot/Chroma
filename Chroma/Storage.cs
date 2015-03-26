using System;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.IO;

namespace Chroma
{
  [Serializable]
  public class GameData
  {
    public int CoinsNumber;
    public int RacesNumber;
  }

  public class Storage
  {
    private static readonly string fileName = "savegame.xml";

    private static bool IsRunningOnDevice()
    {
      return ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.DEVICE;
    }

    public static void SaveGameData(GameData data)
    {
      if (!IsRunningOnDevice())
      {
        return;
      }

      var storage = IsolatedStorageFile.GetUserStoreForApplication();

      var fs = storage.OpenFile(fileName, System.IO.FileMode.OpenOrCreate);
      if (fs != null)
      {

        var serializer = new XmlSerializer(typeof(GameData));
        serializer.Serialize(fs, data);

        fs.Close();
      }
    }

    public static GameData LoadGameData()
    {
      var result = new GameData();

      if (IsRunningOnDevice())
      {
        var storage = IsolatedStorageFile.GetUserStoreForApplication();

        if (storage.FileExists(fileName))
        {
          var fs = storage.OpenFile(fileName, System.IO.FileMode.Open);

          var serializer = new XmlSerializer(typeof(GameData));
          result = (GameData)serializer.Deserialize(fs);

          fs.Close();
        }
      }

      return result;
    }

    public static GameData ResetGameData()
    {
      var result = new GameData();

      if (IsRunningOnDevice())
      {
        SaveGameData(result);
      }

      return result;
    }
  }
}

