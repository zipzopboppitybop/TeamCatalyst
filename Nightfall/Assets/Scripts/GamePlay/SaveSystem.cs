using System.IO;
using Catalyst.Player;
using UnityEngine;



namespace Catalyst.GamePlay
{
    public class SaveSystem
    {
        private static SaveData _saveData = new SaveData();
        //[SerializeField] Loading load;

        public struct SaveData
        {
            public PlayerSaveData playerData;

        }

        public static string SaveFileName()
        {
            return Path.Combine(Application.persistentDataPath, "savefile.json");
        }

        public static void Save()
        {
            HandleSaveData();
            File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
        }

        public static void HandleSaveData()
        {
            GameManager.instance.playerController.Save(ref _saveData.playerData);
        }

        public static void Load()
        {
            if (File.Exists(SaveFileName()))
            {
                _saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SaveFileName()));
                HandleLoadData(GetLoad());
            }
            else
            {
                Debug.LogWarning("Save file not found!");
            }
        }

        private static Loading GetLoad()
        {
            Loading load = GameObject.FindFirstObjectByType<Loading>();
            return load;
        }

        private static void HandleLoadData(Loading load)
        {
            load.LoadScene(_saveData.playerData.playerData.SceneIndex);
            GameManager.instance.playerController.Load(ref _saveData.playerData);


        }

    }
}
