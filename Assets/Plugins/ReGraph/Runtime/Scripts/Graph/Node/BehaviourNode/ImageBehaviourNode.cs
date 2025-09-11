using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ImageBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            FillAmount = 10,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(image)")]
        [InlineButton("@image.SetObjectValue(AssignComponent<UnityEngine.UI.Image>())", "♺", ShowIf = "@image.IsObjectValueType()")]
        [InfoBox("@image.GetMismatchWarningMessage()", InfoMessageType.Error, "@image.IsShowMismatchWarning()")]
        private SceneObjectProperty image = new SceneObjectProperty(SceneObject.ObjectType.Image);

        [LabelText("Value")]
        [ShowIf("@executionType == ExecutionType.FillAmount")]
        [OnInspectorGUI("@MarkPropertyDirty(number)")]
        [InlineProperty]
        public FloatProperty number;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (image.IsEmpty || !image.IsMatchType() || executionType is ExecutionType.None)
            {
                LogWarning("Found an empty Image Behaviour node in " + context.objectName);
            }
            else
            {
                if (executionType is ExecutionType.FillAmount)
                {
                    ((Image) image).fillAmount = number;
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Fill Amount", ExecutionType.FillAmount},
        };

        public static string displayName = "Image Behaviour Node";
        public static string nodeName = "Image";

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
            return $"Audio & Visual/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (image.IsEmpty || !image.IsMatchType() || executionType is ExecutionType.None)
                return string.Empty;
            string message = "";
            if (executionType is ExecutionType.FillAmount)
                message = "Set " + number + " Fill Amount to " + image.name;
            return message;
        }
        
        public override string GetNodeViewTooltip ()
        {
            return "This will provide several controls to a specific Image.\n\n" + base.GetNodeViewTooltip();
        }
#endif
    }
}