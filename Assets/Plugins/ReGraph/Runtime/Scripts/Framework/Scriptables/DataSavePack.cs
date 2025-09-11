using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Reshape.ReFramework
{
    [CreateAssetMenu(menuName = "Reshape/Data Save Pack", fileName = "DataSavePack", order = 15)]
    [Serializable]
    [HideMonoScript]
    public class DataSavePack : SavePack
    {
        [Serializable]
        public struct DataInfo
        {
            public enum Type
            {
                Number = 0,
                Word = 1,
            }

            [HorizontalGroup(Width = 0.7f)]
            [HideLabel]
            [InlineProperty]
            public StringProperty name;

            [HorizontalGroup(Width = 0.3f)]
            [HideLabel, HideInInlineEditors]
            [ValueDropdown("TypeChoice")]
            public Type type;

            [HideInInspector]
            public string strValue;

            [HideInInspector]
            public float floatValue;

            private bool inited;

            [HideIf("HideValue")]
            [ShowInInspector]
            [HorizontalGroup(Width = 0.3f)]
            [HideLabel, HideReferenceObjectPicker]
            public object value
            {
                get
                {
                    if (type == Type.Number)
                        return floatValue;
                    if (type == Type.Word)
                        return strValue;
                    return null;
                }
            }

            public void Init ()
            {
                inited = true;
            }

            public new string ToString ()
            {
                if (type == Type.Number)
                    return floatValue.ToString(CultureInfo.InvariantCulture);
                if (type == Type.Word)
                    return strValue;
                return string.Empty;
            }

            public void Reset ()
            {
                floatValue = 0f;
                strValue = string.Empty;
            }

#if UNITY_EDITOR
            private bool HideValue ()
            {
                return !inited;
            }

            private IEnumerable TypeChoice ()
            {
                var listDropdown = new ValueDropdownList<Type> {{"Number", Type.Number}, {"Word", Type.Word}};
                return listDropdown;
            }
#endif
        }

        [Space(4)]
        [DisableInPlayMode]
        public List<DataInfo> datas;

        //-----------------------------------------------------------------
        //-- static methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- public methods
        //-----------------------------------------------------------------

        public VariableScriptableObject GetValue (string dataName, VariableScriptableObject variable)
        {
            for (var j = 0; j < datas.Count; j++)
            {
                var data = datas[j];
                if (data.name.Equals(dataName))
                {
                    if (data.type == DataInfo.Type.Number && variable is NumberVariable num)
                    {
                        num.SetValue(data.floatValue);
                        return num;
                    }
                     
                    if (data.type == DataInfo.Type.Word && variable is WordVariable word)
                    {
                        word.SetValue(data.strValue);
                        return word;
                    }

                    break;
                }
            }

            return variable;
        }
        
        public void SetValue (string dataName, string value)
        {
            for (var j = 0; j < datas.Count; j++)
            {
                var data = datas[j];
                if (data.name.Equals(dataName))
                {
                    if (data.type == DataInfo.Type.Number)
                    {
                        if (float.TryParse(value, out var result))
                        {
                            data.floatValue = result;
                            datas[j] = data;
                        }
                    }
                     
                    if (data.type == DataInfo.Type.Word)
                    {
                        data.strValue = value;
                        datas[j] = data;
                    }

                    break;
                }
            }
        }

        public bool Match (string value)
        {
            return string.Equals(fileName, value);
        }

        [SpecialName]
        public void SetId (string value)
        {
            fileNamePostfix = value;
        }

        [SpecialName]
        public void Init ()
        {
            printLog = false;
            for (var i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                data.Init();
                datas[i] = data;
            }
        }

        [SpecialName]
        public void Save ()
        {
            if (string.IsNullOrEmpty(fileName) || datas is not {Count: > 0})
                return;
            var dict = new Dictionary<string, object>();
            for (var i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if (!string.IsNullOrEmpty(data.name))
                    dict.Add(data.name, data.value);
            }

            SaveFile(dict, TYPE_DATA);
        }

        [SpecialName]
        public bool Load ()
        {
            if (string.IsNullOrEmpty(fileName) || datas is not {Count: > 0})
                return false;
            var dict = LoadFile(TYPE_DATA);
            if (dict != null)
            {
                foreach (var save in dict)
                {
                    for (var i = 0; i < datas.Count; i++)
                    {
                        var data = datas[i];
                        if (data.name == save.Key)
                        {
                            if (data.type == DataInfo.Type.Number)
                                if (float.TryParse(save.Value.ToString(), out var result))
                                    data.floatValue = result;
                            if (data.type == DataInfo.Type.Word)
                                data.strValue = save.Value.ToString();
                            datas[i] = data;
                            break;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        [SpecialName]
        public void Delete ()
        {
            if (string.IsNullOrEmpty(fileName) || datas is not {Count: > 0})
                return;
            DeleteFile(TYPE_DATA);
        }
        
        [SpecialName]
        public void Clear ()
        {
            if (string.IsNullOrEmpty(fileName) || datas is not {Count: > 0})
                return;
            for (var i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                data.Reset();
                datas[i] = data;
            }
        }

        public void DeleteSave (string value)
        {
            if (string.IsNullOrEmpty(fileName) || datas is not {Count: > 0})
                return;
            var previousName = fileNamePostfix;
            fileNamePostfix = value;
            DeleteFile(TYPE_DATA);
            fileNamePostfix = previousName;
        }


        //-----------------------------------------------------------------
        //-- protected methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- mono methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- BaseScriptable methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- private methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- editor methods
        //-----------------------------------------------------------------
    }
}