using System;
using System.Collections.Generic;
using Core.Services.DataSaving.SerializedValueTypes;
using UnityEngine;

namespace Core.Services.DataSaving
{
    public class DataSaverData
    {
        public List<DataSaverRecord> Data => _data;

        private List<DataSaverRecord> _data = new();

        public SerializedValueBase Get(string keyType, string keyValue)
        {
            if (_data == null || string.IsNullOrEmpty(keyType) || string.IsNullOrEmpty(keyValue))
                return null;

            for (int i = 0; i < _data.Count; i++)
            {
                if (keyType.Equals(_data[i].KeyType) && keyValue.Equals(_data[i].KeyValue))
                {
                    return _data[i].SerializedObject;
                }
            }

            return null;
        }

        public void Set(string keyType, string keyValue, SerializedValueBase obj)
        {
            if (_data == null || string.IsNullOrEmpty(keyType) || string.IsNullOrEmpty(keyValue))
                throw new NullReferenceException("Input values can't be null");
            
            for (int i = 0; i < _data.Count; i++)
            {
                if (keyType.Equals(_data[i].KeyType) && keyValue.Equals(_data[i].KeyValue))
                {
                    var buf = _data[i];
                    buf.SerializedObject = obj;
                    _data[i] = buf;
                    return;
                }
            }
            
            _data.Add(new DataSaverRecord()
            {
                KeyType = keyType,
                KeyValue = keyValue,
                SerializedObject = obj
            });
        }

        public string Serialize()
        {
            List<SerializedData.SerializedDataEntry> data = new();
            for (int i = 0; i < _data.Count; i++)
            {
                data.Add(new SerializedData.SerializedDataEntry()
                {
                    KeyType = _data[i].KeyType,
                    KeyValue = _data[i].KeyValue,
                    Type = _data[i].SerializedObject.GetType().FullName,
                    Data = _data[i].SerializedObject.Serialize()
                });
            }

            return JsonUtility.ToJson(new SerializedData()
            {
                Data = data.ToArray()
            });
        }

        public static DataSaverData Deserialize(string s)
        {
            SerializedData saved;
            try
            {
                saved = JsonUtility.FromJson<SerializedData>(s);
            }
            catch
            {
                saved = new SerializedData()
                {
                    Data = Array.Empty<SerializedData.SerializedDataEntry>()
                };
            }

            DataSaverData res = new();
            for (int i = 0; i < saved.Data.Length; i++)
            {
                var obj = GetSerializedObject(saved.Data[i]);
                if (obj != null)
                {
                    res._data.Add(new DataSaverRecord()
                    {
                        KeyType = saved.Data[i].KeyType,
                        KeyValue = saved.Data[i].KeyValue,
                        SerializedObject = obj
                    });
                }
            }

            return res;
        }

        private static SerializedValueBase GetSerializedObject(SerializedData.SerializedDataEntry entry)
        {
            Type type = Type.GetType(entry.Type);
            if (type != null)
            {
                var res = Activator.CreateInstance(type) as SerializedValueBase;
                try
                {
                    res.DeserializeFromString(entry.Data);
                }
                catch{/**/}
                return res;
            }

            return null;
        }
        
        public struct DataSaverRecord
        {
            public string KeyType;
            public string KeyValue;
            public SerializedValueBase SerializedObject;
        }
        
        [Serializable]
        public struct SerializedData
        {
            public SerializedDataEntry[] Data;
            
            [Serializable]
            public struct SerializedDataEntry
            {
                public string KeyType;
                public string KeyValue;
                public string Type;
                public string Data;
            }
        }
    }
}