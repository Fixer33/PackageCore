#if PLUGIN_YG_2 && Storage_yg
using System;
using Cysharp.Threading.Tasks;
using YG;

namespace Core.Services.DataSaving.Concrete
{
    public class YandexDataSaver : DataSaver
    {
        private bool _sdkInitialized;
        
        protected override async UniTask Init()
        {
            YG2.onGetSDKData += OnGetSDKData;

            while (_sdkInitialized == false)
            {
                await UniTask.Delay(100);
            }
        }
        
        private void OnGetSDKData()
        {
            YG2.onGetSDKData -= OnGetSDKData;

            _sdkInitialized = true;
        }

        protected override DataSaverData LoadData()
        {
            string raw = YG2.saves.SavedData;
            try
            {
                var data = DataSaverData.Deserialize(raw);
                if (data == null)
                    throw new NullReferenceException();
                return data;
            }
            catch{/**/}

            return new DataSaverData();
        }

        protected override void SaveData(DataSaverData data)
        {
            YG2.saves.SavedData = data.Serialize();
            YG2.SaveProgress();
        }

        protected override void ClearData()
        {
            YG2.SetDefaultSaves();
        }
    }
}
#else
namespace Core.Services.DataSaving.Concrete
{
    public class YandexDataSaver : ServiceScriptableObject
    {
        public override void InitializeService()
        {
            throw new System.NotImplementedException();
        }
    }
}
#endif