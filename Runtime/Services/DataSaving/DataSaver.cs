#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using System;
using Core.Services.DataSaving.SerializedValueTypes;
using UnityEngine;

namespace Core.Services.DataSaving
{
    public abstract class DataSaver : ServiceScriptableObject
    {
        public static bool IsReady { get; private set; }

        private static DataSaver _instance;
        private DataSaverData _data;
        
        public override async void InitializeService()
        {
            if (_instance != null)
            {
                throw new Exception($"Trying to initialize second instance of DataSaver!\n " +
                                    $"Already active: {_instance.name}\n " +
                                    $"Trying to init: {name}");
            }

            _instance = this;
            await Init();

            _data = LoadData();
            IsServiceInitialized = true;
            IsReady = true;
        }

        public override void DisposeService()
        {
            base.DisposeService();
            _instance = null;
            IsReady = false;
        }

        protected virtual UnifiedTask Init() => UnifiedTask.CompletedTask;
        protected abstract DataSaverData LoadData();
        protected abstract void SaveData(DataSaverData data);
        protected abstract void ClearData();

        #region Static methods

        public static void SetValue<T>(T key, SerializedValueBase val) where T : Enum
        {
            Validate();
            _instance._data.Set(key.GetType().Name, key.ToString(), val);
            _instance.SaveData(_instance._data);
        }

        public static SerializedValueBase GetValue<T>(T key) where T : Enum
        {
            Validate();
            return _instance._data.Get(key.GetType().Name, key.ToString());
        }

        public static void SetValueCustomKey<T>(T key, SerializedValueBase val) where T : IDataSaveKey
        {
            if (key == null)
                throw new NullReferenceException("Key can't be null!");
            
            Validate();
            _instance._data.Set(key.GetDataSaveID(), key.GetDataSaveKey(), val);
            _instance.SaveData(_instance._data);
        }

        public static SerializedValueBase GetValueCustomKey<T>(T key) where T : IDataSaveKey
        {
            if (key == null)
                throw new NullReferenceException("Key can't be null!");

            Validate();
            return _instance._data.Get(key.GetDataSaveID(), key.GetDataSaveKey());
        }
        
        public static async UnifiedTask WaitForInitialization()
        {
            while (_instance == false || _instance.IsServiceInitialized == false)
            {
                await UnifiedTask.Delay(100);
                if (Application.isPlaying == false)
                    return;
            }
        }

        public static async void ExecuteOnInit(Action action)
        {
            while (_instance == false || _instance.IsServiceInitialized == false)
            {
                await UnifiedTask.Delay(100);
                if (Application.isPlaying == false)
                    return;
            }
            action?.Invoke();
        }

        protected static void Validate()
        {
            if (_instance == false)
                throw new Exception("No DataSaver existing! Please ensure to have DataSaver prefab on Init scene!");
            
            if (_instance.IsServiceInitialized == false)
                throw new Exception("DataSaver is not initialized yet! Please, use await function WaitForInitialization\n" +
                                    "OR use method ExecuteOnInit");
        }

        #endregion
        
#if UNITY_EDITOR
        [ContextMenu("Clear all data")]
        private void EDITOR_ClearAllData()
        {
            ClearData();
        }
#endif
    }
}