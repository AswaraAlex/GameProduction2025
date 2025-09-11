using System.Collections;
using Reshape.ReFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class UiBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            RefreshLayout = 100,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        private LayoutGroup layout;

        private LayoutController layoutController;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.RefreshLayout)
            {
                if (layout == null)
                {
                    LogWarning("Found an empty UI Behaviour node in " + context.objectName);
                }
                else
                {
                    if (layoutController == null)
                    {
                        layout.TryGetComponent(out layoutController);
                        if (layoutController == null)
                            layoutController = layout.gameObject.AddComponent<LayoutController>();
                    }

                    layoutController.RefreshLayout(layout);
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Refresh Layout", ExecutionType.RefreshLayout},
        };

        public static string displayName = "UI Behaviour Node";
        public static string nodeName = "UI";

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
            if (executionType is ExecutionType.RefreshLayout)
                if (layout != null)
                    return $"Refresh {layout.gameObject.name}'s layout";
            return string.Empty;
        }
        
        public override string GetNodeViewTooltip ()
        {
            return "This will provide several controls to UI components.\n\n" + base.GetNodeViewTooltip();
        }
#endif
    }
}