using UnityEngine;

namespace Core.Services.DataSaving.Concrete.PP
{
    [CreateAssetMenu(fileName = "Player Prefs saver", menuName = "Services/DataSaving/Player Prefs", order = 0)]
    public class PlayerPrefsSaver : DataSaver
    {
        private const string KEY = "StoredData";
        
        protected override DataSaverData LoadData()
        {
            DataSaverData result = null;
            try
            {
                result = DataSaverData.Deserialize(PlayerPrefs.GetString(KEY, ""));
                Debug.Log("Loaded PP data collection of size: " + result.Data.Count);
            }
            catch
            {
                result = new DataSaverData();
            }

            return result;
        }

        protected override void SaveData(DataSaverData data)
        {
            PlayerPrefs.SetString(KEY, data.Serialize());
        }

        protected override void ClearData()
        {
            PlayerPrefs.DeleteKey(KEY);
        }
    }
}