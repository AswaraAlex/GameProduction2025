using System;
using System.Collections.Generic;
using Reshape.ReFramework;
using UnityEngine;

namespace Reshape.ReGraph
{
    [Serializable]
    public class GraphVariables
    {
        public struct NodeStateInfo
        {
            private readonly string id;
            private Node.State state;

            public NodeStateInfo (string i, Node.State s)
            {
                id = i;
                state = s;
            }

            private void SetState (Node.State s)
            {
                state = s;
            }

            public static bool TryGetValue (List<NodeStateInfo> list, string i, out Node.State outState)
            {
                outState = Node.State.None;
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].id.Equals(i))
                    {
                        outState = list[j].state;
                        return true;
                    }
                }

                return false;
            }

            public static void TryAddOrEdit (List<NodeStateInfo> list, string i, Node.State s)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].id.Equals(i))
                    {
                        var temp = list[j];
                        temp.SetState(s);
                        list[j] = temp;
                        return;
                    }
                }

                list.Add(new NodeStateInfo(i, s));
            }
        }

        public struct StartedInfo
        {
            private readonly string id;
            private bool started;

            public StartedInfo (string i, bool s)
            {
                id = i;
                started = s;
            }

            private void SetStarted (bool s)
            {
                started = s;
            }

            public static bool TryGetValue (List<StartedInfo> list, string i, out bool outStarted, bool d = default)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].id.Equals(i))
                    {
                        outStarted = list[j].started;
                        return true;
                    }
                }

                outStarted = d;
                return false;
            }

            public static void TryAddOrEdit (List<StartedInfo> list, string i, bool s)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].id.Equals(i))
                    {
                        var temp = list[j];
                        temp.SetStarted(s);
                        list[j] = temp;
                        return;
                    }
                }

                list.Add(new StartedInfo(i, s));
            }
        }

        public struct IntInfo
        {
            private readonly string id;
            private readonly int prefix;
            private int value;

            public IntInfo (string i, int v, int p)
            {
                id = i;
                value = v;
                prefix = p;
            }

            private void SetValue (int v)
            {
                value = v;
            }

            public static bool TryGetValue (List<IntInfo> list, string i, int p, out int outValue, int d = default)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].prefix == p && list[j].id.Equals(i))
                    {
                        outValue = list[j].value;
                        return true;
                    }
                }

                outValue = d;
                return false;
            }

            public static void TryAddOrEdit (List<IntInfo> list, string i, int p, int v)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].prefix == p && list[j].id.Equals(i))
                    {
                        var temp = list[j];
                        temp.SetValue(v);
                        list[j] = temp;
                        return;
                    }
                }

                list.Add(new IntInfo(i, v, p));
            }
        }

        public struct FloatInfo
        {
            private readonly string id;
            private readonly int prefix;
            private float value;

            public FloatInfo (string i, float v, int p)
            {
                id = i;
                value = v;
                prefix = p;
            }

            private void SetValue (float v)
            {
                value = v;
            }

            public static bool TryGetValue (List<FloatInfo> list, string i, int p, out float outValue, float d = default)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].prefix == p && list[j].id.Equals(i))
                    {
                        outValue = list[j].value;
                        return true;
                    }
                }

                outValue = d;
                return false;
            }

            public static void TryAddOrEdit (List<FloatInfo> list, string i, int p, float v)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].prefix == p && list[j].id.Equals(i))
                    {
                        var temp = list[j];
                        temp.SetValue(v);
                        list[j] = temp;
                        return;
                    }
                }

                list.Add(new FloatInfo(i, v, p));
            }
        }

        public struct CharacterInfo
        {
            private readonly string id;
            private readonly int prefix;
            private CharacterOperator character;

            public CharacterInfo (string i, CharacterOperator c, int p)
            {
                id = i;
                character = c;
                prefix = p;
            }

            private void SetValue (CharacterOperator c)
            {
                character = c;
            }

            public static bool TryGetValue (List<CharacterInfo> list, string i, int p, out CharacterOperator outValue, CharacterOperator d = default)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].prefix == p && list[j].id.Equals(i))
                    {
                        outValue = list[j].character;
                        return true;
                    }
                }

                outValue = d;
                return false;
            }

            public static void TryAddOrEdit (List<CharacterInfo> list, string i, int p, CharacterOperator v)
            {
                var count = list.Count;
                for (var j = 0; j < count; j++)
                {
                    if (list[j].prefix == p && list[j].id.Equals(i))
                    {
                        var temp = list[j];
                        temp.SetValue(v);
                        list[j] = temp;
                        return;
                    }
                }

                list.Add(new CharacterInfo(i, v, p));
            }
        }

        public const int PREFIX_RETURN = 1;

        public List<NodeStateInfo> states;
        public List<StartedInfo> started;
        public List<IntInfo> intList;
        public List<FloatInfo> floatList;
        public List<CharacterInfo> characterList;

        public GraphVariables ()
        {
            states = new List<NodeStateInfo>();
        }

        public void Reset ()
        {
            states.Clear();
            started?.Clear();
            intList?.Clear();
            floatList?.Clear();
            characterList?.Clear();
        }

        public void CollectReturnData (GraphVariables inputVars, string inputId, string collectorId)
        {
            if (inputVars != null)
            {
                if (inputVars.intList != null && IntInfo.TryGetValue(inputVars.intList, inputId, PREFIX_RETURN, out var intValue))
                    SetInt(collectorId, intValue, 1);
                if (inputVars.floatList != null && FloatInfo.TryGetValue(inputVars.floatList, inputId, PREFIX_RETURN, out var floatValue))
                    SetFloat(collectorId, floatValue, 1);
                if (inputVars.characterList != null && CharacterInfo.TryGetValue(inputVars.characterList, inputId, PREFIX_RETURN, out var charOpValue))
                    SetCharacter(collectorId, charOpValue, 1);
            }
        }

        public Node.State GetState (string nodeId, Node.State defaultValue)
        {
            if (NodeStateInfo.TryGetValue(states, nodeId, out var outState))
                return outState;
            return defaultValue;
        }

        public void SetState (string nodeId, Node.State value)
        {
            NodeStateInfo.TryAddOrEdit(states, nodeId, value);
        }

        public bool GetStarted (string nodeId, bool defaultValue)
        {
            if (started != null)
            {
                StartedInfo.TryGetValue(started, nodeId, out var outStarted, defaultValue);
                return outStarted;
            }

            return defaultValue;
        }

        public void SetStarted (string nodeId, bool value)
        {
            started ??= new List<StartedInfo>();
            StartedInfo.TryAddOrEdit(started, nodeId, value);
        }

        public int GetInt (string varId, int defaultValue = 0, int prefix = 0)
        {
            if (intList != null)
                if (!string.IsNullOrEmpty(varId) && IntInfo.TryGetValue(intList, varId, prefix, out var outInt))
                    return outInt;
            return defaultValue;
        }

        public void SetInt (string varId, int value, int prefix = 0)
        {
            if (!string.IsNullOrEmpty(varId))
            {
                intList ??= new List<IntInfo>();
                IntInfo.TryAddOrEdit(intList, varId, prefix, value);
            }
        }

        public float GetFloat (string varId, float defaultValue = 0f, int prefix = 0)
        {
            if (floatList != null)
                if (FloatInfo.TryGetValue(floatList, varId, prefix, out var outFloat))
                    return outFloat;
            return defaultValue;
        }

        public void SetFloat (string varId, float value, int prefix = 0)
        {
            if (!string.IsNullOrEmpty(varId))
            {
                floatList ??= new List<FloatInfo>();
                FloatInfo.TryAddOrEdit(floatList, varId, prefix, value);
            }
        }

        public CharacterOperator GetCharacter (string varId, CharacterOperator defaultValue = null, int prefix = 0)
        {
            if (characterList != null)
                if (CharacterInfo.TryGetValue(characterList, varId, prefix, out var outCharacter))
                    return outCharacter;
            return defaultValue;
        }

        public void SetCharacter (string varId, CharacterOperator value, int prefix = 0)
        {
            if (!string.IsNullOrEmpty(varId))
            {
                characterList ??= new List<CharacterInfo>();
                CharacterInfo.TryAddOrEdit(characterList, varId, prefix, value);
            }
        }

        public float GetNumber (string varId, float defaultFloatValue = 0f, int defaultIntValue = 0, int prefix = 0)
        {
            var value = GetFloat(varId, defaultFloatValue, prefix);
            if (value.Equals(defaultFloatValue))
            {
                value = GetInt(varId, defaultIntValue, prefix);
                if (value.Equals(defaultIntValue))
                    value = defaultFloatValue;
            }

            return value;
        }
    }
}