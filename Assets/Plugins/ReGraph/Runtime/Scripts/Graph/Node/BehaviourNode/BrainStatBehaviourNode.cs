using System.Collections;
using Reshape.ReFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class BrainStatBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            ReadBaseValue = 101,
            ReadCalcValue = 102,
            UpdateValue = 201,
        }

        public enum BrainOwner
        {
            None,
            Owner = 101,
            Target = 102,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType = ExecutionType.ReadBaseValue;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Brain")]
        [ValueDropdown("BrainOwnerChoice")]
        [ShowIf("ShowBrainOwnerProperty")]
        private BrainOwner brainOwner = BrainOwner.Owner;

        [SerializeField]
        [ShowIf("ShowCharacterParam")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(paramCharacter)")]
        [InlineButton("@paramCharacter.SetObjectValue(AssignComponent<CharacterOperator>())", "♺", ShowIf = "@paramCharacter.IsObjectValueType()")]
        [InfoBox("@paramCharacter.GetMismatchWarningMessage()", InfoMessageType.Error, "@paramCharacter.IsShowMismatchWarning()")]
        private SceneObjectProperty paramCharacter = new SceneObjectProperty(SceneObject.ObjectType.CharacterOperator, "Character");

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("@StatType.DrawAllStatNameListDropdown()", DropdownWidth = 250, AppendNextDrawer = true)]
        [ShowIf("@executionType == ExecutionType.ReadBaseValue || executionType == ExecutionType.ReadCalcValue || executionType == ExecutionType.UpdateValue")]
        [DisplayAsString]
        private string statType;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.ReadBaseValue || executionType == ExecutionType.ReadCalcValue")]
        [LabelText("Variable")]
        private NumberVariable paramVariable;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.ReadBaseValue)
            {
                if (string.IsNullOrEmpty(statType) || paramVariable == null)
                {
                    LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                }
                else
                {
                    if (!context.isScriptableGraph)
                    {
                        if (context.runner.graph.isStatGraph)
                        {
                            if (execution.parameters.characterBrain == null)
                            {
                                LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                            }
                            else
                            {
                                execution.parameters.characterBrain.GetStatValue(statType, out var value);
                                paramVariable.SetValue(value);
                            }
                        }
                        else
                        {
                            if (paramCharacter.IsEmpty || !paramCharacter.IsMatchType())
                            {
                                LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                            }
                            else
                            {
                                var characterOperator = (CharacterOperator) paramCharacter;
                                characterOperator.brain.GetStatValue(statType, out var value);
                                paramVariable.SetValue(value);
                            }
                        }
                    }
                    else
                    {
                        CharacterBrain brain = null;
                        if (context.graph.isAttackDamagePack)
                        {
                            if (brainOwner == BrainOwner.Owner)
                                brain = execution.parameters.attackDamageData.attackerBrain;
                            else if (brainOwner == BrainOwner.Target)
                                brain = execution.parameters.attackDamageData.defenderBrain;
                        }
                        else if (context.graph.isAttackSkillPack)
                        {
                            if (brainOwner == BrainOwner.Owner)
                                brain = execution.parameters.attackSkillData.owner.brain;
                            else if (brainOwner == BrainOwner.Target)
                                brain = execution.parameters.attackSkillData.damageData.defenderBrain;
                        }
                        else if (context.graph.isTargetAimPack)
                        {
                            if (paramCharacter.IsEmpty || !paramCharacter.IsMatchType())
                            {
                                LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                            }
                            else
                            {
                                var characterOperator = (CharacterOperator) paramCharacter;
                                brain = characterOperator.brain;
                            }
                        }
                        else if (context.graph.isMoralePack)
                        {
                            brain = execution.parameters.moraleData.owner.brain;
                        }
                        else if (context.graph.isStaminaPack)
                        {
                            brain = execution.parameters.staminaData.owner.brain;
                        }

                        if (brain)
                        {
                            brain.GetStatValue(statType, out var value);
                            paramVariable.SetValue(value);
                        }
                    }
                }
            }
            else if (executionType == ExecutionType.ReadCalcValue)
            {
                if (string.IsNullOrEmpty(statType) || paramVariable == null)
                {
                    LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                }
                else if (execution.parameters.actionName == statType)
                {
                    LogWarning("Found stat overflow at Brain Stat Behaviour node in " + context.objectName);
                }
                else
                {
                    if (!context.isScriptableGraph)
                    {
                        if (context.runner.graph.isStatGraph)
                        {
                            var value = 0f;
                            var executionResult = context.runner.TriggerBrainStat(TriggerNode.Type.BrainStatGet, execution.parameters.characterBrain, statType);
                            if (executionResult is {variables: { }})
                            {
                                value = executionResult.variables.GetNumber(statType, float.PositiveInfinity, int.MaxValue, GraphVariables.PREFIX_RETURN);
                                context.runner.CacheExecute(executionResult);
                                if (float.IsPositiveInfinity(value))
                                    execution.parameters.characterBrain.GetStatValue(statType, out value);
                            }

                            paramVariable.SetValue(value);
                        }
                        else
                        {
                            if (paramCharacter.IsEmpty || !paramCharacter.IsMatchType())
                            {
                                LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                            }
                            else
                            {
                                var value = 0f;
                                var characterOperator = (CharacterOperator) paramCharacter;
                                if (characterOperator.statGraph)
                                {
                                    var executionResult = characterOperator.statGraph.TriggerBrainStat(TriggerNode.Type.BrainStatGet, characterOperator.brain, statType);
                                    if (executionResult is {variables: { }})
                                    {
                                        value = executionResult.variables.GetNumber(statType, float.PositiveInfinity, int.MaxValue, GraphVariables.PREFIX_RETURN);
                                        characterOperator.statGraph.CacheExecute(executionResult);
                                        if (float.IsPositiveInfinity(value))
                                            characterOperator.brain.GetStatValue(statType, out value);
                                    }
                                    else
                                    {
                                        characterOperator.brain.GetStatValue(statType, out value);
                                    }
                                }
                                else
                                {
                                    characterOperator.brain.GetStatValue(statType, out value);
                                }

                                paramVariable.SetValue(value);
                            }
                        }
                    }
                    else
                    {
                        CharacterOperator co = null;
                        CharacterBrain brain = null;
                        if (context.graph.isAttackDamagePack)
                        {
                            if (brainOwner == BrainOwner.Owner)
                            {
                                co = execution.parameters.attackDamageData.attacker;
                                if (co)
                                    brain = execution.parameters.attackDamageData.attackerBrain;
                            }
                            else if (brainOwner == BrainOwner.Target)
                            {
                                co = execution.parameters.attackDamageData.defender;
                                if (co)
                                    brain = execution.parameters.attackDamageData.defenderBrain;
                            }
                        }
                        else if (context.graph.isAttackSkillPack)
                        {
                            if (brainOwner == BrainOwner.Owner)
                            {
                                co = execution.parameters.attackSkillData.owner;
                                if (co)
                                    brain = execution.parameters.attackSkillData.owner.brain;
                            }
                            else if (brainOwner == BrainOwner.Target)
                            {
                                co = execution.parameters.attackSkillData.damageData.defender;
                                if (co)
                                    brain = execution.parameters.attackSkillData.damageData.defenderBrain;
                            }
                        }
                        else if (context.graph.isTargetAimPack)
                        {
                            if (paramCharacter.IsEmpty || !paramCharacter.IsMatchType())
                            {
                                LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                            }
                            else
                            {
                                co = (CharacterOperator) paramCharacter;
                                if (co)
                                    brain = co.brain;
                            }
                        }
                        else if (context.graph.isMoralePack)
                        {
                            Debug.Log("execution.parameters.moraleData.owner:"+execution.parameters.moraleData.owner);
                            co = execution.parameters.moraleData.owner;
                            if (co)
                                brain = co.brain;
                        }
                        else if (context.graph.isStaminaPack)
                        {
                            co = execution.parameters.staminaData.owner;
                            if (co)
                                brain = co.brain;
                        }

                        if (co && brain && co.statGraph)
                        {
                            var executionResult = co.statGraph.TriggerBrainStat(TriggerNode.Type.BrainStatGet, brain, statType);
                            var value = 0f;
                            if (executionResult is {variables: { }})
                            {
                                value = executionResult.variables.GetNumber(statType, float.PositiveInfinity, int.MaxValue, GraphVariables.PREFIX_RETURN);
                                co.statGraph.CacheExecute(executionResult);
                                if (float.IsPositiveInfinity(value))
                                    brain.GetStatValue(statType, out value);
                            }

                            paramVariable.SetValue(value);
                        }
                    }
                }
            }
            if (executionType == ExecutionType.UpdateValue)
            {
                if (string.IsNullOrEmpty(statType))
                    LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                else
                {
                    if (!context.isScriptableGraph)
                    {
                        if (context.runner.graph.isStatGraph)
                        {
                            if (execution.parameters.characterBrain == null)
                                LogWarning("Found an empty Brain Stat Behaviour node in " + context.objectName);
                            else
                                execution.parameters.characterBrain.Owner.UpdateStatusEffect(statType);
                        }
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private bool ShowCharacterParam ()
        {
            var graph = GetGraph();
            if (graph != null)
            {
                if (graph.isBehaviourGraph)
                    return true;
                if (graph.isTargetAimPack)
                    return true;
            }

            return false;
        }

        private bool ShowBrainOwnerProperty ()
        {
            var graph = GetGraph();
            if (graph != null)
            {
                if (graph.isAttackDamagePack)
                    return true;
                if (graph.isAttackSkillPack)
                    return true;
            }

            return false;
        }

        private ValueDropdownList<BrainOwner> BrainOwnerChoice ()
        {
            var listDropdown = new ValueDropdownList<BrainOwner>();
            var curGraph = GetGraph();
            if (curGraph is {isAttackDamagePack: true})
            {
                listDropdown.Add("Attacker", BrainOwner.Owner);
                listDropdown.Add("Defender", BrainOwner.Target);
            }
            else if (curGraph is {isAttackSkillPack: true})
            {
                listDropdown.Add("Owner", BrainOwner.Owner);
                listDropdown.Add("Target", BrainOwner.Target);
            }

            return listDropdown;
        }

        private IEnumerable TypeChoice ()
        {
            var listDropdown = new ValueDropdownList<ExecutionType>();
            listDropdown.Add("Get Base Value (BV)", ExecutionType.ReadBaseValue);
            listDropdown.Add("Get Final Value (FV)", ExecutionType.ReadCalcValue);
            var graph = GetGraph();
            if (graph is {isStatGraph: true})
                listDropdown.Add("Update Value", ExecutionType.UpdateValue);
            return listDropdown;
        }

        public static string displayName = "Brain Stat Behaviour Node";
        public static string nodeName = "Brain Stat";

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
            if (executionType is ExecutionType.ReadBaseValue or ExecutionType.ReadCalcValue)
            {
                if (!string.IsNullOrEmpty(statType) && paramVariable)
                {
                    var graph = GetGraph();
                    if (graph != null && (graph.isBehaviourGraph || graph.isTargetAimPack))
                    {
                        if (!paramCharacter.IsNull)
                        {
                            if (executionType == ExecutionType.ReadBaseValue)
                                return $"Get {paramCharacter.objectName}'s {statType} (BV) into {paramVariable.name}";
                            if (executionType == ExecutionType.ReadCalcValue)
                                return $"Get {paramCharacter.objectName}'s {statType} (FV) into {paramVariable.name}";
                        }
                    }
                    else
                    {
                        if (executionType == ExecutionType.ReadBaseValue)
                            return $"Get {statType} (BV) into {paramVariable.name}";
                        if (executionType == ExecutionType.ReadCalcValue)
                            return $"Get {statType} (FV) into {paramVariable.name}";
                    }
                }
            }
            else if (executionType == ExecutionType.UpdateValue)
            {
                if (!string.IsNullOrEmpty(statType))
                {
                    var graph = GetGraph();
                    if (graph is {isStatGraph: true})
                        return $"Update {statType}";
                } 
            }

            return string.Empty;
        }

        public override string GetNodeViewTooltip ()
        {
            var tip = string.Empty;
            if (executionType == ExecutionType.ReadBaseValue)
                tip += "This will get the base value of the stat, base value is direct from the Battle system which include Stat Sheet, Mod & Mtp.\n\n";
            else if (executionType == ExecutionType.ReadCalcValue)
                tip += "This will get the final value of the stat, final value is passing thru Graph which have modify by logic setup in Graph.\n\n";
            else if (executionType == ExecutionType.UpdateValue)
                tip += "This will update value of the stat, usually this is require to force a stat recalculate when there is another stat's value change.\n\n";
            else
                tip += "This will execute all Brain Stat related behaviour.\n\n";
            return tip + base.GetNodeViewTooltip();
        }
#endif
    }
}