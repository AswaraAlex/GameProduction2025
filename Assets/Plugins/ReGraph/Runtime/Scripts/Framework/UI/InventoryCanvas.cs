using System;
using System.Collections;
using System.Collections.Generic;
using Reshape.ReGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Reshape.Unity;
using TMPro;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class InventoryCanvas : ReSingletonBehaviour<InventoryCanvas>
    {
        private const string WALLET = "Wallet";

        public Canvas canvas;
        public InventoryPanel[] panels;
        public RectTransform leftPanel;
        public RectTransform rightPanel;
        public GameObject currencyPanel;
        public InventoryItemTooltip itemTooltip;
        public InventoryItemPick itemPick;
        public InventoryItemContext itemContext;

        private string currentInv1;
        private string currentInv2;
        private InventoryPanel buySellPanel;
        private string pickInv;
        private int pickSlotIndex;
        private bool pickApplyStatus;
        private bool pickApplySkill;
        private Vector3 leftPanelPos;
        private Vector3 rightPanelPos;
        private int showCanvasFrameNo;

        public delegate void TypeDelegate (int type);

        public const int DROP_NOT_SPACE = 101;

        public static event ReInventoryController.InvNameDelegate OnInvClosed;
        public static event ReInventoryController.InvNameDelegate OnInvUnlockRequested;
        public static event TypeDelegate OnWarning;

        public static bool IsUnderBuySellPanel ()
        {
            if (instance == null)
                return false;
            return instance.buySellPanel != null;
        }

        public static bool IsUnderPickUp ()
        {
            if (instance == null)
                return false;
            return !string.IsNullOrEmpty(instance.pickInv);
        }

        public static void HideCanvas ()
        {
            if (instance == null)
                return;
            instance.Hide();
        }

        public static void NotifyClosePanel (string invName)
        {
            OnInvClosed?.Invoke(invName);
        }

        public static void ShowCanvas (string invName, string tradeInvName = "", bool autoInit = false)
        {
            if (instance == null)
            {
                if (autoInit)
                {
                    var go = Instantiate(GraphManager.instance.runtimeSettings.inventoryCanvas);
                    go.name = GraphManager.instance.runtimeSettings.inventoryCanvas.name;
                }
                else
                {
                    return;
                }
            }

            instance.Show(invName, tradeInvName);
        }

        public static bool isShowingCanvas => instance != null && instance.isShowing;
        
        public static bool isShowingLeftOnly => instance != null && instance.isShowing && !string.IsNullOrEmpty(instance.currentInv1) && string.IsNullOrEmpty(instance.currentInv2); 

        public static void ShowToolTip (string inv, int index)
        {
            if (instance == null)
                return;
            instance.DisplayToolTip(inv, index);
        }

        public static void ShowContext (InventoryBehaviour invBehave, int index)
        {
            if (instance == null)
                return;
            instance.DisplayContext(invBehave, index);
        }

        public static void HideContext (InventoryBehaviour invBehave, int index)
        {
            if (instance == null)
                return;
            instance.CloseContext(invBehave, index);
        }

        public static void HideToolTip (string inv, int index)
        {
            if (instance == null)
                return;
            instance.CloseToolTip(inv, index);
        }

        public static bool PickUp (string inv, int index, bool applyStatus, bool applySkill)
        {
            if (instance == null)
                return false;
            return instance.PerformPickUp(inv, index, applyStatus, applySkill);
        }

        public static void Trade (string inv, int index, bool applyStatus, bool applySkill)
        {
            if (instance == null)
                return;
            instance.TradeBetweenCurrent(inv, index, applyStatus, applySkill);
        }

        public static void Discard (string inv, int index)
        {
            if (instance == null)
                return;
            instance.PerformDiscard(inv, index);
        }

        public static void Drop (string inv, int index)
        {
            if (instance == null)
                return;
            instance.PerformDrop(inv, index);
        }

        public static void Use (string inv, int index, MultiTag consume)
        {
            if (instance == null)
                return;
            instance.PerformUse(inv, index, consume);
        }

        public static void Unlock (string inv)
        {
            if (instance == null)
                return;
            instance.PerformUnlock(inv);
        }

        public bool isShowing => canvas.enabled;

        public void TradeBetweenCurrent (string inv, int index, bool applyStatus, bool applySkill)
        {
            if (currentInv1 != inv && currentInv2 != inv)
                return;
            var from = currentInv1;
            var to = currentInv2;
            if (currentInv2 == inv)
            {
                from = currentInv2;
                to = currentInv1;
            }

            var tradeInv = InventoryManager.GetInventory(from);
            var tradeItem = tradeInv.GetItem(index);
            var manager = GraphManager.instance.runtimeSettings.itemManager;
            var tradeItemData = manager.GetItemData(tradeItem.ItemId);
            bool slotInStatus, slotOutStatus, slotInSkill, slotOutSkill;
            slotInStatus = slotOutStatus = slotInSkill = slotOutSkill = false;
            if (tradeItemData.invBehaviour != null)
                InventoryManager.CreateSubInventory(tradeItemData, from, index);

            if (IsUnderBuySellPanel())
            {
                if (IsBuyItem(from, to))
                {
                    if (InventoryManager.Trade(from, index, to, WALLET, buySellPanel.invBehave.Currency.id, true, true))
                    {
                        if (InventoryManager.GetInventoryApplyStatusType(to) == InventoryBehaviour.ApplyStatusTrigger.SlotIn)
                            slotInStatus = true;
                        if (InventoryManager.GetInventoryApplySkillType(to) == InventoryBehaviour.ApplySkillTrigger.SlotIn)
                            slotInSkill = true;
                    }
                }
                else
                {
                    if (InventoryManager.Trade(from, index, to, WALLET, buySellPanel.invBehave.Currency.id, false, true))
                    {
                        if (applyStatus)
                            slotOutStatus = true;
                        if (applySkill)
                            slotOutSkill = true;
                    }
                }
            }
            else
            {
                var panel = GetPanel(to);
                if (!panel.invBehave.RestrictAdd)
                {
                    if (InventoryManager.Give(from, index, to, true))
                    {
                        if (InventoryManager.GetInventoryApplyStatusType(to) == InventoryBehaviour.ApplyStatusTrigger.SlotIn)
                            slotInStatus = true;
                        if (InventoryManager.GetInventoryApplySkillType(to) == InventoryBehaviour.ApplySkillTrigger.SlotIn)
                            slotInSkill = true;
                        if (applyStatus)
                            slotOutStatus = true;
                        if (applySkill)
                            slotOutSkill = true;
                    }
                }
            }

            if (tradeItem.isUsable)
            {
                if (!tradeItemData.isEmptyStatus)
                {
                    if (slotInStatus)
                        InventoryManager.AddCharacterAttackStatus(to, tradeItemData);
                    if (slotOutStatus)
                        InventoryManager.RemoveCharacterAttackStatus(from, tradeItemData);
                }

                if (!tradeItemData.isEmptySkill)
                {
                    if (slotInSkill)
                        InventoryManager.AddCharacterAttackSkill(from, tradeItemData);
                    if (slotOutSkill)
                        InventoryManager.RemoveCharacterAttackSkill(to, tradeItemData);
                }
            }
        }

        public bool PerformPickUp (string inv, int index, bool applyStatus, bool applySkill)
        {
            if (!IsUnderPickUp())
            {
                var fromInv = InventoryManager.GetInventory(inv);
                if (fromInv != null)
                {
                    var pickItem = fromInv.GetItem(index);
                    if (pickItem is {isSolid: true})
                    {
                        pickInv = inv;
                        pickSlotIndex = index;
                        pickApplyStatus = applyStatus;
                        pickApplySkill = applySkill;
                        var manager = GraphManager.instance.runtimeSettings.itemManager;
                        var itemData = manager.GetItemData(pickItem.ItemId);
                        itemPick.ShowPickInfo(itemData, pickItem);
                        return true;
                    }
                }
            }
            else
            {
                var fromInv = InventoryManager.GetInventory(pickInv);
                var toInv = InventoryManager.GetInventory(inv);
                var putItem = toInv.GetItem(index);
                var pickItem = fromInv.GetItem(pickSlotIndex);
                var manager = GraphManager.instance.runtimeSettings.itemManager;
                var pickItemData = manager.GetItemData(pickItem.ItemId);
                var success = false;
                bool slotInStatus, slotOutStatus, slotInSkill, slotOutSkill;
                slotInStatus = slotOutStatus = slotInSkill = slotOutSkill = false;
                if (putItem == null || putItem.isEmpty)
                {
                    if (pickInv == inv)
                    {
                        var relocate = false;
                        if (IsUnderBuySellPanel())
                        {
                            if (pickInv != buySellPanel.invName)
                                relocate = true;
                        }
                        else
                            relocate = true;

                        if (relocate)
                            success = InventoryManager.Relocate(pickInv, pickSlotIndex, index);
                    }
                    else
                    {
                        if (pickItemData.invBehaviour != null)
                            InventoryManager.CreateSubInventory(pickItemData, pickInv, pickSlotIndex);

                        var give = false;
                        if (!IsUnderBuySellPanel())
                        {
                            give = true;
                        }
                        else
                        {
                            if (IsBuyItem(pickInv, inv))
                            {
                                if (InventoryManager.Trade(pickInv, pickSlotIndex, inv, index, WALLET, buySellPanel.invBehave.Currency.id, true))
                                {
                                    success = true;
                                    if (applyStatus)
                                        slotInStatus = true;
                                    if (applySkill)
                                        slotInSkill = true;
                                }
                            }
                            else if (IsSellItem(pickInv, inv))
                            {
                                if (InventoryManager.Trade(pickInv, pickSlotIndex, inv, index, WALLET, buySellPanel.invBehave.Currency.id, false))
                                {
                                    success = true;
                                    if (pickApplyStatus)
                                        slotOutStatus = true;
                                    if (pickApplySkill)
                                        slotOutSkill = true;
                                }
                            }
                            else
                            {
                                give = true;
                            }
                        }

                        if (give)
                        {
                            var panel = GetPanel(inv);
                            if (panel == null || !panel.invBehave.RestrictAdd)
                            {
                                if (InventoryManager.Give(pickInv, pickSlotIndex, inv, index))
                                {
                                    success = true;
                                    if (pickApplyStatus)
                                        slotOutStatus = true;
                                    if (pickApplySkill)
                                        slotOutSkill = true;
                                    if (applyStatus)
                                        slotInStatus = true;
                                    if (applySkill)
                                        slotInSkill = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (pickInv == inv)
                    {
                        if (pickSlotIndex == index)
                        {
                            success = true;
                        }
                        else if (!IsUnderBuySellPanel() || pickInv != buySellPanel.invName)
                        {
                            if (InventoryManager.Stack(pickInv, pickSlotIndex, index))
                                success = true;
                        }
                    }
                }

                if (pickItem.isUsable)
                {
                    if (!pickItemData.isEmptyStatus)
                    {
                        if (slotInStatus)
                            InventoryManager.AddCharacterAttackStatus(inv, pickItemData);
                        if (slotOutStatus)
                            InventoryManager.RemoveCharacterAttackStatus(pickInv, pickItemData);
                    }

                    if (!pickItemData.isEmptySkill)
                    {
                        if (slotInSkill)
                            InventoryManager.AddCharacterAttackSkill(inv, pickItemData);
                        if (slotOutSkill)
                            InventoryManager.RemoveCharacterAttackSkill(pickInv, pickItemData);
                    }
                }

                if (success)
                {
                    InventorySlotItem.UnhighlightHighlighted();
                    pickInv = string.Empty;
                    itemTooltip.Hide();
                    itemPick.Hide();
                }
            }

            return false;
        }

        public void CloseToolTip (string inv, int index)
        {
            itemTooltip.Hide();
        }

        public void DisplayToolTip (string inv, int index)
        {
            if (IsUnderPickUp())
            {
                inv = pickInv;
                index = pickSlotIndex;
            }

            var fromInv = InventoryManager.GetInventory(inv);
            if (fromInv != null)
            {
                var manager = GraphManager.instance.runtimeSettings.itemManager;
                var tradeItem = fromInv.GetItem(index);
                if (tradeItem is {isSolid: true})
                {
                    var itemData = manager.GetItemData(tradeItem.ItemId);
                    itemTooltip.ShowItemInfo(itemData, tradeItem);
                    if (IsUnderBuySellPanel())
                    {
                        if (inv == buySellPanel.invName)
                            itemTooltip.ShowBuyInfo(itemData, tradeItem);
                        else
                            itemTooltip.ShowSellInfo(itemData, tradeItem);
                    }
                }
            }
        }

        public void DisplayContext (InventoryBehaviour invBehave, int index)
        {
            if (IsUnderPickUp())
                return;
            var fromInv = InventoryManager.GetInventory(invBehave.Name);
            if (fromInv != null)
            {
                var manager = GraphManager.instance.runtimeSettings.itemManager;
                var tradeItem = fromInv.GetItem(index);
                if (tradeItem is {isSolid: true})
                {
                    var itemData = manager.GetItemData(tradeItem.ItemId);
                    var use = invBehave.Consume.ContainAny(itemData.tags, false);
                    itemContext.Show(invBehave, index, use, invBehave.Discard);
                }
            }
        }

        public void CloseContext (InventoryBehaviour invBehave, int index)
        {
            var fromInv = InventoryManager.GetInventory(invBehave.Name);
            var tradeItem = fromInv?.GetItem(index);
            if (tradeItem is {isSolid: true})
                itemContext.Hide();
        }

        public void Show (string invName1, string invName2)
        {
            if (!string.IsNullOrWhiteSpace(invName1) && isShowing)
                return;
            InventoryPanel panel1 = null;
            InventoryPanel panel2 = null;
            for (var i = 0; i < panels.Length; i++)
            {
                if (!string.IsNullOrEmpty(panels[i].invName))
                {
                    if (panels[i].invName == invName1 && !panel1)
                    {
                        panel1 = panels[i];
                    }
                    else if (panels[i].invName == invName2 && !panel2)
                    {
                        panel2 = panels[i];
                    }
                }
            }

            var isCurrencyDisplay = false;
            if (panel1)
            {
                currentInv1 = invName1;
                if (panel1.invBehave.BuySell)
                    buySellPanel = panel1;
                panel1.transform.position = leftPanelPos;
                panel1.Show(IsUnderBuySellPanel());
                if (panel1.invBehave.CurrencyDisplay)
                    isCurrencyDisplay = true;
            }

            if (panel2)
            {
                currentInv2 = invName2;
                if (panel2.invBehave.BuySell)
                    buySellPanel = panel2;
                panel2.transform.position = rightPanelPos;
                panel2.Show(IsUnderBuySellPanel());
                if (panel2.invBehave.CurrencyDisplay)
                    isCurrencyDisplay = true;
            }

            if (currencyPanel)
            {
                if (IsUnderBuySellPanel())
                    currencyPanel.gameObject.SetActiveOpt(true);
                else if (isCurrencyDisplay)
                    currencyPanel.gameObject.SetActiveOpt(true);
                else
                    currencyPanel.gameObject.SetActiveOpt(false);
            }

            canvas.enabled = true;
            showCanvasFrameNo = ReTime.frameCount;
        }

        public void Hide ()
        {
            if (!isShowing)
                return;
            if (showCanvasFrameNo >= ReTime.frameCount)
                return;
            for (var i = 0; i < panels.Length; i++)
                if (panels[i].Hide())
                    OnInvClosed?.Invoke(panels[i].invName);
            buySellPanel = null;
            currentInv1 = string.Empty;
            currentInv2 = string.Empty;
            currencyPanel.gameObject.SetActiveOpt(false);
            pickInv = string.Empty;
            itemTooltip.Hide();
            itemPick.Hide();
            canvas.enabled = false;
        }

        protected override void Awake ()
        {
            base.Awake();
            leftPanelPos = leftPanel.position;
            rightPanelPos = rightPanel.position;
            Hide();
        }

        protected void OnDestroy ()
        {
            ClearInstance();
        }

        private void PerformDrop (string inv, int index)
        {
            if (IsUnderPickUp())
            {
                InventorySlotItem.UnhighlightHighlighted();
                pickInv = string.Empty;
                itemTooltip.Hide();
                itemPick.Hide();
            }

            var unit = CharacterOperator.GetWithInventory(inv);
            if (unit != null)
            {
                var loc = unit.GetDropPoint();
                if (loc != null)
                {
                    var dropped = InventoryManager.Discard(inv, index);
                    if (dropped != null || dropped.Count > 0)
                    {
                        var go = TakeFromPool(GraphManager.instance.runtimeSettings.dropInvGo, loc.position, loc.rotation, true);
                        LootController.Generate(go, dropped);
                    }
                }
                else
                {
                    OnWarning?.Invoke(DROP_NOT_SPACE);
                }
            }
        }

        private void PerformDiscard (string inv, int index)
        {
            if (IsUnderPickUp())
            {
                InventorySlotItem.UnhighlightHighlighted();
                pickInv = string.Empty;
                itemTooltip.Hide();
                itemPick.Hide();
            }

            InventoryManager.Discard(inv, index);
        }

        private void PerformUse (string inv, int index, MultiTag consume)
        {
            if (IsUnderPickUp())
            {
                InventorySlotItem.UnhighlightHighlighted();
                pickInv = string.Empty;
                itemTooltip.Hide();
                itemPick.Hide();
            }

            InventoryManager.Use(inv, index, consume);
        }

        private void PerformUnlock (string invName)
        {
            OnInvUnlockRequested?.Invoke(invName);
        }

        private InventoryPanel GetPanel (string invName)
        {
            for (var i = 0; i < panels.Length; i++)
                if (panels[i].invName == invName)
                    return panels[i];
            return null;
        }

        private bool IsBuyItem (string fromInvName, string toInvName)
        {
            InventoryPanel panel1 = null;
            for (var i = 0; i < panels.Length; i++)
            {
                if (!string.IsNullOrEmpty(panels[i].invName))
                {
                    if (panels[i].invName == fromInvName)
                    {
                        panel1 = panels[i];
                        break;
                    }
                }
            }

            if (panel1 != null)
                return panel1.invBehave.BuySell;
            return false;
        }

        private bool IsSellItem (string fromInvName, string toInvName)
        {
            InventoryPanel panel1 = null;
            for (var i = 0; i < panels.Length; i++)
            {
                if (!string.IsNullOrEmpty(panels[i].invName))
                {
                    if (panels[i].invName == toInvName)
                    {
                        panel1 = panels[i];
                        break;
                    }
                }
            }

            if (panel1 != null)
                return panel1.invBehave.BuySell;
            return false;
        }
    }
}