using System.Collections;
using Sirenix.OdinInspector;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class AttackDamageTriggerNode : TriggerNode
    {
        [ValueDropdown("TriggerTypeChoice")]
        [OnValueChanged("MarkDirty")]
        public Type triggerType;

        [LabelText("Attacker Store To")]
        [OnValueChanged("MarkDirty")]
        [InfoBox("The assigned variable is not match type!", InfoMessageType.Warning, "ShowObjectVariableWarning", GUIAlwaysEnabled = true)]
        public SceneObjectVariable objectVariable;

        [LabelText("Defender Store To")]
        [OnValueChanged("MarkDirty")]
        [InfoBox("The assigned variable is not match type!", InfoMessageType.Warning, "ShowObjectVariable2Warning", GUIAlwaysEnabled = true)]
        public SceneObjectVariable objectVariable2;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            var state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                var proceed = false;
                if (execution.type == triggerType)
                {
                    proceed = true;
                }
                else if (execution.type == Type.All)
                {
                    if (execution.parameters.actionName.Equals(TriggerId))
                        proceed = true;
                }

                if (proceed)
                {
                    execution.variables.SetState(guid, State.Success);
                    state = State.Success;
                    if (context.graph.isAttackDamagePack)
                    {
                        if (triggerType == Type.CalculateAttackDamage)
                        {
                            if (objectVariable != null)
                            {
                                objectVariable.Reset();
                                objectVariable.SetValue(execution.parameters.attackDamageData.attacker);
                            }

                            if (objectVariable2 != null)
                            {
                                objectVariable2.Reset();
                                objectVariable2.SetValue(execution.parameters.attackDamageData.defender);
                            }
                        }
                    }
                }

                if (state != State.Success)
                {
                    execution.variables.SetState(guid, State.Failure);
                    state = State.Failure;
                }
                else
                    OnSuccess();
            }

            if (state == State.Success)
                return base.OnUpdate(execution, updateId);
            return State.Failure;
        }
        
        public override bool IsTrigger (TriggerNode.Type type, int paramInt = 0)
        {
            return type == triggerType;
        }

#if UNITY_EDITOR
        private bool ShowObjectVariableWarning ()
        {
            if (objectVariable != null)
                if (objectVariable.sceneObject.type != SceneObject.ObjectType.CharacterOperator)
                    return true;
            return false;
        }

        private bool ShowObjectVariable2Warning ()
        {
            if (objectVariable2 != null)
                if (objectVariable2.sceneObject.type != SceneObject.ObjectType.CharacterOperator)
                    return true;
            return false;
        }
        
        private IEnumerable TriggerTypeChoice ()
        {
            var menu = new ValueDropdownList<Type> {{"Calculate", Type.CalculateAttackDamage}};
            return menu;
        }
        
        public static string displayName = "Attack Damage Trigger Node";
        public static string nodeName = "Attack Damage";

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
            return triggerType.ToString();
        }

        public override string GetNodeViewDescription ()
        {
            if (triggerType == Type.CalculateAttackDamage)
                return "Calculate Attack Damage";
            return string.Empty;
        }
        
        public override string GetNodeViewTooltip ()
        {
            var tip = string.Empty;
            if (triggerType == Type.CalculateAttackDamage)
                tip += "This will get trigger when the Attack Damage pack being ask to calculate the attack damage.\n\n";
            else
                tip += "This will get trigger all Attack Damage related events.\n\n";
            return tip + base.GetNodeViewTooltip();
        }
#endif
    }
}