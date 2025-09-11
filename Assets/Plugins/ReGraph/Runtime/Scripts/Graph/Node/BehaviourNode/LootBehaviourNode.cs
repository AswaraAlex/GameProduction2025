using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class LootBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            Create = 10,
            GetId = 20,
            PickUp = 50,
            Show = 100,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.Create")]
        private LootPack lootPack;

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Create")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(gameObject)")]
        [InlineButton("@gameObject.SetObjectValue(AssignGameObject())", "♺", ShowIf = "@gameObject.IsObjectValueType()")]
        [InfoBox("@gameObject.GetMismatchWarningMessage()", InfoMessageType.Error, "@gameObject.IsShowMismatchWarning()")]
        private SceneObjectProperty gameObject = new SceneObjectProperty(SceneObject.ObjectType.GameObject, "Loot Prefab");

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Create")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(transformObject)")]
        [InlineButton("@transformObject.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@transformObject.IsObjectValueType()")]
        [InfoBox("@transformObject.GetMismatchWarningMessage()", InfoMessageType.Error, "@transformObject.IsShowMismatchWarning()")]
        private SceneObjectProperty transformObject = new SceneObjectProperty(SceneObject.ObjectType.Transform, "Spawn Transform");
        
        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(paramString)")]
        [InlineProperty]
        [ShowIf("@executionType == ExecutionType.PickUp")]
        [LabelText("Inv Name")]
        private StringProperty paramString;
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.PickUp || executionType == ExecutionType.Show || executionType == ExecutionType.GetId")]
        private LootController lootController;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.GetId")]
        [LabelText("Variable")]
        private WordVariable paramWord1;
        
        [ShowIf("@executionType == ExecutionType.Create")]
        [LabelText("Character Store To")]
        [OnValueChanged("MarkDirty")]
        [InfoBox("The assigned variable is not match type!", InfoMessageType.Warning, "ShowObjectVariableWarning", GUIAlwaysEnabled = true)]
        public SceneObjectVariable objectVariable;
        
        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.None)
            {
                LogWarning("Found an empty Loot Behaviour node in " + context.objectName);
            }
            else if (executionType is ExecutionType.Create)
            {
                if (lootPack == null || gameObject.IsEmpty || !gameObject.IsMatchType())
                {
                    LogWarning("Found an empty Loot Behaviour node in " + context.objectName);
                }
                else
                {
                    var loc = context.transform;
                    if (!transformObject.IsEmpty && transformObject.IsMatchType())
                        loc = (Transform) transformObject;
                    var go = context.runner.TakeFromPool(gameObject, loc, true);
                    var controller = LootController.Generate(go, lootPack);
                    if (controller)
                    {
                        if (controller.gameObject.TryGetComponent(out CharacterOperator character))
                        {
                            if (objectVariable)
                            {
                                objectVariable.Reset();
                                objectVariable.SetValue(character);
                            }
                        }
                    }
                }
            }
            else if (executionType is ExecutionType.PickUp)
            {
                if (string.IsNullOrEmpty(paramString) || lootController == null)
                {
                    LogWarning("Found an empty Loot Behaviour node in " + context.objectName);
                }
                else
                {
                    lootController.PickUp(paramString);
                }
            }
            else if (executionType is ExecutionType.Show)
            {
                if (lootController == null)
                {
                    LogWarning("Found an empty Loot Behaviour node in " + context.objectName);
                }
                else
                {
                    lootController.Show();
                }
            }
            else if (executionType is ExecutionType.GetId)
            {
                if (paramWord1 == null || lootController == null)
                {
                    LogWarning("Found an empty Loot Behaviour node in " + context.objectName);
                }
                else
                {
                    paramWord1.SetValue(lootController.lootId);
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private bool ShowObjectVariableWarning ()
        {
            if (objectVariable != null)
                if (objectVariable.sceneObject.type != SceneObject.ObjectType.CharacterOperator)
                    return true;
            return false;
        }
        
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Create", ExecutionType.Create},
            {"Get Id", ExecutionType.GetId},
            {"Pick Up", ExecutionType.PickUp},
            {"Show", ExecutionType.Show},
        };

        public static string displayName = "Loot Behaviour Node";
        public static string nodeName = "Loot";

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
            if (executionType is ExecutionType.None)
                return string.Empty;
            string message = "";
            if (executionType is ExecutionType.Create && lootPack != null && !gameObject.IsNull && gameObject.IsMatchType())
                message = $"Create {gameObject.objectName} to drop {lootPack.name}";
            else if (executionType is ExecutionType.PickUp && paramString.IsAssigned() && lootController != null)
                message = "Pick up loot drop";
            else if (executionType is ExecutionType.Show && lootController != null)
                message = "Show loot drop UI";
            else if (executionType == ExecutionType.GetId && lootController != null && lootController != null && paramWord1 != null)
                message = "Get loot drop Id";
            return message;
        }

        public override string GetNodeViewTooltip ()
        {
            var tip = string.Empty;
            if (executionType == ExecutionType.PickUp)
                tip += "This will put all items in loot into the defined inventory.\n\n";
            else if (executionType == ExecutionType.Create)
                tip += "This will create a loot gameObject and spawn it on the scene.\n\n";
            else if (executionType == ExecutionType.Show)
                tip += "This will open the loot drop UI.\n\n";
            else if (executionType == ExecutionType.GetId)
                tip += "This will store the loot id into defined variable.\n\n";
            else
                tip += "This will provide functionality for Loot Drop.\n\n";
            return tip + base.GetNodeViewTooltip();
        }
#endif
    }
}