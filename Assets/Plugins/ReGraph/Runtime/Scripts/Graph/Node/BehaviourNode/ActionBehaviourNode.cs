using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ActionBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            Graph = 10,
            Character = 20,
            TargetAim = 30,
            Stamina = 40,
            AttackSkill = 50
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Graph")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(graph)")]
        [InlineButton("@graph.SetObjectValue(AssignComponent<GraphRunner>())", "♺", ShowIf = "@graph.IsObjectValueType()")]
        [InfoBox("@graph.GetMismatchWarningMessage()", InfoMessageType.Error, "@graph.IsShowMismatchWarning()")]
        private SceneObjectProperty graph = new SceneObjectProperty(SceneObject.ObjectType.GraphRunner);

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Character")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(character)")]
        [InlineButton("@character.SetObjectValue(AssignComponent<CharacterOperator>())", "♺", ShowIf = "@character.IsObjectValueType()")]
        [InfoBox("@character.GetMismatchWarningMessage()", InfoMessageType.Error, "@character.IsShowMismatchWarning()")]
        private SceneObjectProperty character = new SceneObjectProperty(SceneObject.ObjectType.CharacterOperator);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.TargetAim")]
        private TargetAimPack targetAim;
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.Stamina")]
        private StaminaPack stamina;
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.AttackSkill")]
        private AttackSkillPack attackSkill;

        [SerializeField]
        [ValueDropdown("DrawActionNameListDropdown", ExpandAllMenuItems = true)]
        [OnValueChanged("MarkDirty")]
        private ActionNameChoice actionName;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (!actionName)
            {
                LogWarning("Found an empty Action Behaviour node in " + context.objectName);
            }
            else
            {
                if (executionType == ExecutionType.Graph)
                {
                    if (graph.IsEmpty || !graph.IsMatchType())
                        LogWarning("Found an empty Action Behaviour node in " + context.objectName);
                    else
                    {
                        var runner = (GraphRunner) graph;
                        if (runner)
                            runner.CacheExecute(runner.TriggerAction(actionName));
                    }
                }
                else if (executionType == ExecutionType.Character)
                {
                    if (character.IsEmpty || !graph.IsMatchType())
                        LogWarning("Found an empty Action Behaviour node in " + context.objectName);
                    else
                        ((CharacterOperator) character)?.FeedbackGraphTrigger(TriggerNode.Type.ActionTrigger, actionName: actionName);
                }
                else if (executionType == ExecutionType.TargetAim)
                {
                    if (!targetAim)
                        LogWarning("Found an empty Action Behaviour node in " + context.objectName);
                    else
                        targetAim.TriggerAction(actionName, execution);
                }
                else if (executionType == ExecutionType.Stamina)
                {
                    if (!stamina)
                        LogWarning("Found an empty Action Behaviour node in " + context.objectName);
                    else
                        stamina.TriggerAction(actionName, execution);
                }
                else if (executionType == ExecutionType.AttackSkill)
                {
                    if (!attackSkill)
                        LogWarning("Found an empty Action Behaviour node in " + context.objectName);
                    else
                        attackSkill.TriggerAction(actionName, execution);
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        public string ActionName => actionName == null ? string.Empty : actionName;
        public GraphRunner Runner => executionType == ExecutionType.Graph && !graph.IsEmpty && graph.IsMatchType() ? (GraphRunner) graph : null;

        public GraphScriptable Scriptable
        {
            get
            {
                if (executionType == ExecutionType.TargetAim && targetAim != null)
                    return targetAim;
                if (executionType == ExecutionType.Stamina && stamina != null)
                    return stamina;
                if (executionType == ExecutionType.AttackSkill && attackSkill != null)
                    return attackSkill;
                return null;
            }
        }

        private ValueDropdownList<ExecutionType> TypeChoice ()
        {
            var listDropdown = new ValueDropdownList<ExecutionType>();
            var curGraph = GetGraph();
            if (curGraph is {isTargetAimPack: true})
            {
                listDropdown.Add("TargetAim", ExecutionType.TargetAim);
            }
            else if (curGraph is {isStaminaPack: true})
            {
                listDropdown.Add("Stamina", ExecutionType.Stamina);
            }
            else if (curGraph is {isAttackSkillPack: true})
            {
                listDropdown.Add("Attack Skill", ExecutionType.AttackSkill);
                listDropdown.Add("Character", ExecutionType.Character);
            }
            else
            {
                listDropdown.Add("Graph", ExecutionType.Graph);
                listDropdown.Add("Character", ExecutionType.Character);
            }

            return listDropdown;
        }

        private static IEnumerable DrawActionNameListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }

        public static string displayName = "Action Behaviour Node";
        public static string nodeName = "Action";

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
            return $"Logic/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (executionType == ExecutionType.Graph)
                if (!graph.IsNull && graph.IsMatchType() && actionName != null)
                    return "Execute " + actionName + " in graph of " + graph.objectName;
            if (executionType == ExecutionType.Character)
                if (!character.IsNull && graph.IsMatchType() && actionName != null)
                    return "Execute " + actionName + " in graph of " + character.objectName;
            if (executionType == ExecutionType.TargetAim)
                if (targetAim != null && actionName != null)
                    return "Execute " + actionName + " in graph of " + targetAim.name;
            if (executionType == ExecutionType.Stamina)
                if (stamina != null && actionName != null)
                    return "Execute " + actionName + " in graph of " + stamina.name;
            if (executionType == ExecutionType.AttackSkill)
                if (attackSkill != null && actionName != null)
                    return "Execute " + actionName + " in graph of " + attackSkill.name;
            return string.Empty;
        }

        public override string GetNodeViewTooltip ()
        {
            return "This will execute another Action Trigger node at specific graph.\n\n" + base.GetNodeViewTooltip();
        }
#endif
    }
}