using System.Collections.Generic;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using Reshape.Unity.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor.SceneManagement;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class SaveBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            SaveVariables = 11,
            LoadVariables = 12,
            DeleteVariables = 101,
            GetCharacter = 1001,
            SetCharacter = 1002,
            SaveCharacter = 1003,
            LoadCharacter = 1004,
            DeleteCharacter = 1005,
            ClearCharacter = 1006,
            SaveAllCharacter = 1007,
            LoadAllCharacter = 1008,
            DeleteAllCharacter = 1009,
            ClearAllCharacter = 1010,
        }

        public enum SaveType
        {
            None,
            Overwrite = 11,
            Append = 21
        }

        [SerializeField]
        [OnValueChanged("OnChangeType")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField, PropertySpace(-1)]
        [OnInspectorGUI("@MarkPropertyDirty(saveFile)")]
        [InlineProperty]
        [ShowIf("ShowSaveFile")]
        private StringProperty saveFile;

        [SerializeField, PropertySpace(1)]
        [ShowIf("ShowCharacter")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(character)")]
        [InlineButton("@character.SetObjectValue(AssignComponent<CharacterOperator>())", "♺", ShowIf = "@character.IsObjectValueType()")]
        [InfoBox("@character.GetMismatchWarningMessage()", InfoMessageType.Error, "@character.IsShowMismatchWarning()")]
        private SceneObjectProperty character = new SceneObjectProperty(SceneObject.ObjectType.CharacterOperator, "Character");

        [SerializeField, PropertySpace(1)]
        [OnInspectorGUI("@MarkPropertyDirty(password)")]
        [InlineProperty]
        [ShowIf("ShowPassword")]
        [LabelText("@PasswordLabel()")]
        private StringProperty password;

        [SerializeField, PropertySpace(1)]
        [OnInspectorGUI("@MarkPropertyDirty(saveTag)")]
        [InlineProperty]
        [LabelText("@SaveTagLabel()")]
        [ShowIf("ShowSaveTag")]
        [DisableIf("DisableSaveTag")]
        private StringProperty saveTag;

        [SerializeField, PropertySpace(1)]
        [OnValueChanged("MarkDirty")]
        [LabelText("Save Method")]
        [ValueDropdown("SaveTypeChoice")]
        [ShowIf("@executionType == ExecutionType.SaveVariables")]
        private SaveType saveType = SaveType.Overwrite;

        [OnValueChanged("MarkDirty")]
        [PropertySpace(2)]
        [ShowIf("ShowVariables")]
        [ListDrawerSettings(OnTitleBarGUI = "DrawCreateButton", HideAddButton = true)]
        [OnInspectorGUI("CheckVariablesDirty")]
        [InfoBox("Only first variable is being use.", InfoMessageType.Warning, "@ShowVariablesWarning()")]
        public List<VariableScriptableObject> variables;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None)
            {
                LogWarning("Found an empty Save Behaviour node in " + context.objectName);
            }
            else if (executionType is ExecutionType.SaveVariables or ExecutionType.LoadVariables)
            {
                if (string.IsNullOrEmpty(saveFile) || variables == null || variables.Count <= 0)
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else if (executionType is ExecutionType.SaveVariables)
                {
                    if (saveType == SaveType.Append)
                    {
                        SaveOperation op = ReSave.Load(SavePack.GetSaveFileName(saveFile, saveTag, SavePack.TYPE_VAR), password);
                        if (!op.success)
                        {
                            LogWarning($"{saveFile} variables not success append save in {context.objectName}.");
                        }
                        else
                        {
                            var dict = op.receivedDict;
                            for (int i = 0; i < variables.Count; i++)
                            {
                                if (variables[i].supportSaveLoad)
                                {
                                    bool found = false;
                                    var varId = variables[i].GetInstanceID().ToString();
                                    foreach (KeyValuePair<string, object> pair in dict)
                                    {
                                        if (varId == pair.Key)
                                        {
                                            dict[pair.Key] = variables[i].GetObject();
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        dict.Add(varId, variables[i].GetObject());
                                    }
                                }
                            }

                            op = ReSave.Save(SavePack.GetSaveFileName(saveFile, saveTag, SavePack.TYPE_VAR), dict, password);
                            if (!op.success)
                                LogWarning($"{saveFile} variables not success overwrite save in {context.objectName}.");
                        }
                    }
                    else
                    {
                        var dict = new Dictionary<string, object>();
                        for (int i = 0; i < variables.Count; i++)
                            if (variables[i].supportSaveLoad)
                                dict.Add(variables[i].GetInstanceID().ToString(), variables[i].GetObject());
                        SaveOperation op = ReSave.Save(SavePack.GetSaveFileName(saveFile, saveTag, SavePack.TYPE_VAR), dict, password);
                        if (!op.success)
                            LogWarning($"{saveFile} variables not success overwrite save in {context.objectName}.");
                    }
                }
                else if (executionType is ExecutionType.LoadVariables)
                {
                    SaveOperation op = ReSave.Load(SavePack.GetSaveFileName(saveFile, saveTag, SavePack.TYPE_VAR), password);
                    if (!op.success)
                    {
                        LogWarning($"{saveFile} variables not success load in {context.objectName}.");
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object> save in op.receivedDict)
                        {
                            for (int i = 0; i < variables.Count; i++)
                            {
                                if (variables[i].supportSaveLoad)
                                {
                                    if (variables[i].GetInstanceID().ToString() == save.Key)
                                    {
                                        variables[i].SetObject(save.Value);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (executionType is ExecutionType.DeleteVariables)
            {
                if (string.IsNullOrEmpty(saveFile))
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    SaveOperation op = ReSave.Delete(SavePack.GetSaveFileName(saveFile, saveTag, SavePack.TYPE_VAR), password);
                    if (!op.success)
                    {
                        LogWarning($"{saveFile} not successfully delete.");
                    }
                }
            }
            else if (executionType is ExecutionType.GetCharacter)
            {
                if (string.IsNullOrEmpty(saveFile) || string.IsNullOrEmpty(password) || character.IsEmpty || !character.IsMatchType() || variables == null || variables.Count < 1 || !variables[0])
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    var co = (CharacterOperator) character;
                    variables[0].Reset();
                    variables[0] = co.GetSaveValue(saveFile, password, variables[0]);
                }
            }
            else if (executionType is ExecutionType.SetCharacter)
            {
                if (string.IsNullOrEmpty(saveFile) || string.IsNullOrEmpty(password) || character.IsEmpty || !character.IsMatchType())
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    var value = saveTag.value;
                    if (variables is {Count: > 0} && variables[0])
                    {
                        if (variables[0] is NumberVariable num)
                            value = num.ToString();
                        else if (variables[0] is WordVariable word)
                            value = word.value;
                    }

                    var co = (CharacterOperator) character;
                    co.SetSaveValue(saveFile, password, value);
                }
            }
            else if (executionType is ExecutionType.LoadCharacter)
            {
                if (string.IsNullOrEmpty(saveFile) || character.IsEmpty || !character.IsMatchType())
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    var co = (CharacterOperator) character;
                    co.LoadSaveValue(saveFile);
                }
            }
            else if (executionType is ExecutionType.SaveCharacter)
            {
                if (string.IsNullOrEmpty(saveFile) || character.IsEmpty || !character.IsMatchType())
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    var co = (CharacterOperator) character;
                    co.StoreSaveValue(saveFile);
                }
            }
            else if (executionType is ExecutionType.DeleteCharacter)
            {
                if (string.IsNullOrEmpty(saveFile) || character.IsEmpty || !character.IsMatchType())
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    var co = (CharacterOperator) character;
                    co.DeleteSaveValue(saveFile);
                }
            }
            else if (executionType is ExecutionType.ClearCharacter)
            {
                if (string.IsNullOrEmpty(saveFile) || character.IsEmpty || !character.IsMatchType())
                {
                    LogWarning("Found an empty Save Behaviour node in " + context.objectName);
                }
                else
                {
                    var co = (CharacterOperator) character;
                    co.ClearSaveValue(saveFile);
                }
            }
            else if (executionType is ExecutionType.LoadAllCharacter)
            {
                CharacterOperator.LoadAllSave();
            }
            else if (executionType is ExecutionType.SaveAllCharacter)
            {
                CharacterOperator.StoreAllSave();
            }
            else if (executionType is ExecutionType.DeleteAllCharacter)
            {
                CharacterOperator.DeleteAllSave();
            }
            else if (executionType is ExecutionType.ClearAllCharacter)
            {
                CharacterOperator.ClearAllSave();
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        [Button]
        [ShowIf("showAdvanceSettings"), BoxGroup("Show Debug Info")]
        private void CheckSaveFileContent ()
        {
            if (string.IsNullOrEmpty(saveFile))
                return;
            SaveOperation op = ReSave.Load(SavePack.GetSaveFileName(saveFile, saveTag, SavePack.TYPE_VAR), password);
            if (!op.success)
            {
                ReDebug.LogWarning("Graph Warning", $"{saveFile} variables not success load.");
            }
            else
            {
                ReDebug.Log("Graph Save", $"{saveFile} save content : {op.savedString}");
            }
        }

        private static IEnumerable SaveTypeChoice = new ValueDropdownList<SaveType>()
        {
            {"Overwrite Entire Save", SaveType.Overwrite},
            {"Append The Save", SaveType.Append}
        };

        private void DrawCreateButton ()
        {
            var showPlus = true;
            if (executionType is ExecutionType.SetCharacter or ExecutionType.GetCharacter)
                if (variables.Count > 0)
                    showPlus = false;
            if (showPlus)
            {
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
                {
                    variables.Add(null);
                    MarkDirty();
                }
            }

            if (SirenixEditorGUI.ToolbarButton(EditorIcons.File))
            {
                VariableScriptableObject.OpenCreateVariableMenu(null);
            }

            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Folder))
            {
                ReEditorHelper.OpenPersistentDataPath();
            }
        }

        private void CheckVariablesDirty ()
        {
            var createVarPath = GraphEditorVariable.GetString(GetGraphSelectionInstanceID(), "createVariable");
            if (!string.IsNullOrEmpty(createVarPath))
            {
                GraphEditorVariable.SetString(GetGraphSelectionInstanceID(), "createVariable", string.Empty);
                var createVar = (VariableScriptableObject) AssetDatabase.LoadAssetAtPath(createVarPath, typeof(VariableScriptableObject));
                variables.Add(createVar);
                MarkDirty();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (executionType is ExecutionType.SetCharacter or ExecutionType.GetCharacter)
            {
                if (variables.Count > 0)
                    MarkDirty();
            }
        }

        private bool ShowVariablesWarning ()
        {
            if (executionType is ExecutionType.SetCharacter or ExecutionType.GetCharacter)
                if (variables.Count > 1)
                    return true;
            return false;
        }

        private bool ShowSaveFile ()
        {
            switch (executionType)
            {
                case ExecutionType.None:
                case ExecutionType.SaveAllCharacter:
                case ExecutionType.LoadAllCharacter:
                case ExecutionType.ClearAllCharacter:
                case ExecutionType.DeleteAllCharacter:
                    return false;
                default:
                    return true;
            }
        }

        private bool ShowCharacter ()
        {
            switch (executionType)
            {
                case ExecutionType.SetCharacter:
                case ExecutionType.GetCharacter:
                case ExecutionType.SaveCharacter:
                case ExecutionType.LoadCharacter:
                case ExecutionType.ClearCharacter:
                case ExecutionType.DeleteCharacter:
                    return true;
                default:
                    return false;
            }
        }

        private bool ShowSaveTag ()
        {
            if (executionType is ExecutionType.SaveVariables or ExecutionType.LoadVariables or ExecutionType.DeleteVariables or ExecutionType.SetCharacter)
                return true;
            return false;
        }

        private bool ShowPassword ()
        {
            if (executionType is ExecutionType.SaveVariables or ExecutionType.LoadVariables or ExecutionType.DeleteVariables or ExecutionType.GetCharacter or ExecutionType.SetCharacter)
                return true;
            return false;
        }

        private bool ShowVariables ()
        {
            if (executionType is ExecutionType.SaveVariables or ExecutionType.LoadVariables or ExecutionType.GetCharacter or ExecutionType.SetCharacter)
                return true;
            return false;
        }

        private string PasswordLabel ()
        {
            if (executionType is ExecutionType.DeleteVariables or ExecutionType.LoadVariables or ExecutionType.SaveVariables)
                return "Password";
            if (executionType is ExecutionType.SetCharacter or ExecutionType.GetCharacter)
                return "Data Name";
            return string.Empty;
        }

        private string SaveTagLabel ()
        {
            if (executionType is ExecutionType.DeleteVariables or ExecutionType.LoadVariables or ExecutionType.SaveVariables)
                return "Save Tag";
            if (executionType is ExecutionType.SetCharacter)
                return "Value";
            return string.Empty;
        }

        private bool DisableSaveTag ()
        {
            if (executionType is ExecutionType.SetCharacter)
                if (variables is {Count: > 0} && variables[0])
                    return true;
            return false;
        }

        public void OnChangeType ()
        {
            if (executionType == ExecutionType.SetCharacter)
                saveTag.AllowStringOnly();
            else
                saveTag.AllowAll();
            MarkDirty();
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Save Variables", ExecutionType.SaveVariables},
            {"Load Variables", ExecutionType.LoadVariables},
            {"Delete Variables", ExecutionType.DeleteVariables},
            {"Get Character", ExecutionType.GetCharacter},
            {"Set Character", ExecutionType.SetCharacter},
            {"Save Character", ExecutionType.SaveCharacter},
            {"Load Character", ExecutionType.LoadCharacter},
            {"Delete Character", ExecutionType.DeleteCharacter},
            {"Clear Character", ExecutionType.ClearCharacter},
            {"Save All Character", ExecutionType.SaveAllCharacter},
            {"Load All Character", ExecutionType.LoadAllCharacter},
            {"Delete All Character", ExecutionType.DeleteAllCharacter},
            {"Clear All Character", ExecutionType.ClearAllCharacter},
        };

        public static string displayName = "Save Behaviour Node";
        public static string nodeName = "Save";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeIdentityName ()
        {
            return executionType.ToString();
        }

        public override string GetNodeMenuDisplayName ()
        {
            return $"Gameplay/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (executionType == ExecutionType.SaveVariables && !string.IsNullOrEmpty(saveFile) && variables != null && variables.Count > 0)
                return $"Save {variables.Count.ToString()} variables";
            if (executionType == ExecutionType.LoadVariables && !string.IsNullOrEmpty(saveFile) && variables != null && variables.Count > 0)
                return $"Load {variables.Count.ToString()} variables";
            if (executionType == ExecutionType.DeleteVariables && !string.IsNullOrEmpty(saveFile))
                return $"Delete {saveFile}";
            if (executionType == ExecutionType.GetCharacter && !string.IsNullOrEmpty(saveFile) && !string.IsNullOrEmpty(password) && !character.IsNull && character.IsMatchType())
                if (variables is {Count: > 0} && variables[0])
                    return $"Get {character.objectName}'s {saveFile}";
            if (executionType == ExecutionType.SetCharacter && !string.IsNullOrEmpty(saveFile) && !string.IsNullOrEmpty(password) && !character.IsNull && character.IsMatchType())
                return $"Set {character.objectName}'s {saveFile}";
            if (executionType == ExecutionType.SaveCharacter && !string.IsNullOrEmpty(saveFile) && !character.IsNull && character.IsMatchType())
                return $"Store {character.objectName} save";
            if (executionType == ExecutionType.LoadCharacter && !string.IsNullOrEmpty(saveFile) && !character.IsNull && character.IsMatchType())
                return $"Load {character.objectName} save";
            if (executionType == ExecutionType.DeleteCharacter && !string.IsNullOrEmpty(saveFile) && !character.IsNull && character.IsMatchType())
                return $"Delete {character.objectName} save";
            if (executionType == ExecutionType.ClearCharacter && !string.IsNullOrEmpty(saveFile) && !character.IsNull && character.IsMatchType())
                return $"Clear {character.objectName} save";
            if (executionType == ExecutionType.SaveAllCharacter)
                return $"Store all character save";
            if (executionType == ExecutionType.LoadAllCharacter)
                return $"Load all character save";
            if (executionType == ExecutionType.DeleteAllCharacter)
                return $"Delete all character save";
            if (executionType == ExecutionType.ClearAllCharacter)
                return $"Clear all character save";
            return string.Empty;
        }

        public override string GetNodeViewTooltip ()
        {
            var tip = string.Empty;
            if (executionType is ExecutionType.SaveVariables)
                tip += "This will save all defined variables' value into a save file.\n\n";
            else if (executionType is ExecutionType.LoadVariables)
                tip += "This will load the defined save file and store loaded value into those variables.\n\n";
            else if (executionType is ExecutionType.DeleteVariables)
                tip += "This will delete the defined save file, runtime value is not affected\n\n";
            else if (executionType is ExecutionType.GetCharacter)
                tip += "This will get the specific data's value from character runtime save and store into a variable.\n\n";
            else if (executionType is ExecutionType.SetCharacter)
                tip += "This will set a value to the specific data in character runtime save.\n\n";
            else if (executionType is ExecutionType.SaveCharacter)
                tip += "This will store the specific character save into a save file.\n\n";
            else if (executionType is ExecutionType.LoadCharacter)
                tip += "This will load the specific character save from save file and store into the character runtime save.\n\n";
            else if (executionType is ExecutionType.DeleteCharacter)
                tip += "This will delete the specific character save file, runtime value is not affected.\n\n";
            else if (executionType is ExecutionType.ClearCharacter)
                tip += "This will clear the specific character runtime save to default value.\n\n";
            else if (executionType is ExecutionType.SaveAllCharacter)
                tip += "This will store all character save to corresponding save file.\n\n";
            else if (executionType is ExecutionType.LoadAllCharacter)
                tip += "This will load all character save from save file and store into corresponding character runtime save.\n\n";
            else if (executionType is ExecutionType.DeleteAllCharacter)
                tip += "This will delete all character save file, runtime value is not affected.\n\n";
            else if (executionType is ExecutionType.ClearAllCharacter)
                tip += "This will clear all character runtime save to default value.\n\n";
            else
                tip += "This will provide save/load execution to defined variable/character.\n\n";
            return tip + base.GetNodeViewTooltip();
        }
#endif
    }
}