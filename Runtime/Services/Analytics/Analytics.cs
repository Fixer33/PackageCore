#if UNITY_WEBGL
using UnifiedTask = Cysharp.Threading.Tasks.UniTask;
#else
using UnifiedTask = System.Threading.Tasks.Task;
#endif
using System;
using Core.Services.Analytics.Base;
using UnityEngine;

namespace Core.Services.Analytics
{
    public abstract class Analytics : ServiceScriptableObject
    {
        private static Analytics _instance;
        
        public override async void InitializeService()
        {
            bool isInitializedLocally = false;
            Init(() => isInitializedLocally = true);

            while (isInitializedLocally == false)
            {
                await UnifiedTask.Delay(100);
            }

            _instance = this;
            IsServiceInitialized = true;
        }

        public override void DisposeService()
        {
            _instance = null;
            base.DisposeService();
        }

        protected abstract void Init(Action initCallback);

        protected abstract void SendEvent(AnalyticsEvent analyticsEvent);

        public static void CallEvent(AnalyticsEvent analyticsEvent)
        {
            if (_instance == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Failed to log analytics event: No analytics instance found!");
#endif
                return;
            }

            _instance.SendEvent(analyticsEvent);
        }
    }
}