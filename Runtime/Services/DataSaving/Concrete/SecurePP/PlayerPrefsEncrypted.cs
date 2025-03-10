#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using Core.Security;
using UnityEngine;

namespace Core.Services.DataSaving.Concrete.SecurePP
{
    [CreateAssetMenu(fileName = "Encrypted Player Prefs saver", menuName = "Services/DataSaving/Player Prefs Encrypted", order = 0)]
    public class PlayerPrefsEncrypted : DataSaver
    {
        private const string KEY = "StoredData";

        private string _encryptionKey;
        private string _encryptionIv;
        private StringEncryptor _encryptor;

        protected override UnifiedTask Init()
        {
            _encryptionKey = (Application.identifier+nameof(PlayerPrefsEncrypted)+new string('+', 32))[..32];
            _encryptionIv = (Application.identifier+nameof(PlayerPrefsEncrypted)+new string('+', 16))[..16];
            
            _encryptor = new StringEncryptor(_encryptionKey, _encryptionIv);
            return UnifiedTask.CompletedTask;
        }

        protected override DataSaverData LoadData()
        {
            DataSaverData result = null;
            try
            {
                string savedValue = PlayerPrefs.GetString(KEY, "");
                result = DataSaverData.Deserialize(_encryptor.Decrypt(savedValue));
                Debug.Log("Loaded PP data collection of size: " + result.Data.Count);
            }
            catch
            {
                try
                {
                    result = DataSaverData.Deserialize(PlayerPrefs.GetString(KEY, ""));
                    Debug.Log("Loaded PP unencrypted data collection of size: " + result.Data.Count);
                }
                catch
                {
                    result = new DataSaverData();
                }
            }

            return result;
        }

        protected override void SaveData(DataSaverData data)
        {
            PlayerPrefs.SetString(KEY, _encryptor.Encrypt(data.Serialize()));
        }

        protected override void ClearData()
        {
            PlayerPrefs.DeleteKey(KEY);
        }
    }
}
