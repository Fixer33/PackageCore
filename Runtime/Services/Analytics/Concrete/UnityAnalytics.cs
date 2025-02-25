using System;
using Core.Services.Analytics.Base;
using UnityEngine;
#if USE_UNITY_ANALYTICS
using Unity.Services.Analytics;
using System.Collections.Generic;
using Event = Unity.Services.Analytics.Event;
#endif

namespace Core.Services.Analytics.Concrete
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Services/Ads/Unity analytics", order = 0)]
    public class UnityAnalytics : Analytics
    {
#if USE_UNITY_ANALYTICS
        private readonly Dictionary<string, AnalyticsEventWrapper> _eventDictionary = new();
        
        protected override void Init(Action initCallback)
        {
            AnalyticsService.Instance.StartDataCollection();
            initCallback?.Invoke();
        }

        protected override void SendEvent(AnalyticsEvent analyticsEvent)
        {
            if (_eventDictionary.TryGetValue(analyticsEvent.EventId, out var eventWrapper) == false)
            {
                eventWrapper = new AnalyticsEventWrapper(analyticsEvent.EventId);
                _eventDictionary.Add(analyticsEvent.EventId, eventWrapper);
            }

            foreach (var record in analyticsEvent.Parameters)
            {
                switch (record.value)
                {
                    case int val: eventWrapper.SetInt(record.name, val); break;
                    case float val: eventWrapper.SetFloat(record.name, val); break;
                    case bool val: eventWrapper.SetBool(record.name, val); break;
                    case string val: eventWrapper.SetString(record.name, val); break;
                    
                    default: 
                        eventWrapper.SetObject(record.name, record.value);
                        break;
                }
            }
            
            eventWrapper.Send();
        }
        
        private class AnalyticsEventWrapper : Event
        {
            private readonly int _hash;
            
            public AnalyticsEventWrapper(string id) : base(id)
            {
                _hash = id.GetHashCode();
            }

            public override int GetHashCode() => _hash;

            public void Send()
            {
                AnalyticsService.Instance.RecordEvent(this);
            }

            public void SetBool(string name, bool value) => SetParameter(name, value);
            public void SetInt(string name, int value) => SetParameter(name, value);
            public void SetFloat(string name, float value) => SetParameter(name, value);
            public void SetString(string name, string value) => SetParameter(name, value);
            public void SetObject(string name, object value) => SetParameter(name, value.ToString());
        }
#else
        protected override void Init(Action initCallback)
        {
            throw new NotImplementedException();
        }

        protected override void SendEvent(AnalyticsEvent analyticsEvent)
        {
            throw new NotImplementedException();
        }
#endif
    }
}