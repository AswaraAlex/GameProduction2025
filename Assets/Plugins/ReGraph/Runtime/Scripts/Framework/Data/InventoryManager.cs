using System.Collections.Generic;
using Reshape.ReGraph;
using Reshape.Unity;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class InventoryManager
    {
        private static List<string> runtimeUsageOnlyInvName;

        public static InventoryData GetInventory (string name)
        {
            return ReInventoryController.GetInventory(name);
        }

        public static List<string> GetInventoriesName ()
        {
            return ReInventoryController.GetInventoriesName();
        }

        public static bool CreateInventory (string name, int size, int stack, int noPerRow = 0, int tags = 0, Vector2Int sizeLimit = default, int countLimit = 0, bool runtimeUsage = false)
        {
            if (runtimeUsage)
            {
                runtimeUsageOnlyInvName ??= new List<string>();
                runtimeUsageOnlyInvName.Add(name);
            }

            return ReInventoryController.CreateInventory(name, size, stack, noPerRow, tags, sizeLimit, countLimit);
        }
        
        public static void CreateSubInventory (ItemData item, string inv, int index)
        {
            if (item.invBehaviour != null)
            {
                var fromInv = GetInventory(inv);
                var tradeItem = fromInv?.GetItem(index);
                if (tradeItem != null)
                    if (CreateInventory(tradeItem.InstanceId, item.invBehaviour.Size, item.invBehaviour.Stack, item.invBehaviour.Rows, item.invBehaviour.tags, item.invBehaviour.SizeLimit, item.invBehaviour.CountLimit))
                        TriggerUpdate(tradeItem.InstanceId);
            }
        }

        public static bool IsInventoryRuntimeUsageOnly (string name)
        {
            return runtimeUsageOnlyInvName != null && runtimeUsageOnlyInvName.Contains(name);
        }

        public static void RemoveInventoryRuntimeUsageOnly (string name)
        {
            runtimeUsageOnlyInvName?.Remove(name);
        }

        public static void ClearInventoryRuntimeUsageOnly ()
        {
            if (runtimeUsageOnlyInvName != null)
            {
                for (var i = 0; i < runtimeUsageOnlyInvName.Count; i++)
                    ReInventoryController.DestroyInventory(runtimeUsageOnlyInvName[i]);
                runtimeUsageOnlyInvName?.Clear();
            }
        }

        public static bool CreateInventory (InventoryData rawData)
        {
            return ReInventoryController.CreateInventory(rawData);
        }

        public static void DestroyInventory (string name)
        {
            RemoveInventoryRuntimeUsageOnly(name);
            ReInventoryController.DestroyInventory(name);
        }

        public static void DestroyAllInventory ()
        {
            ClearInventoryRuntimeUsageOnly();
            ReInventoryController.DestroyAllInventory();
        }

        public static bool Relocate (string from, int fromIndex, int toIndex)
        {
            var fromInv = GetInventory(from);
            if (fromInv != null)
            {
                var tradeItem = fromInv.GetItem(fromIndex);
                var toSlot = fromInv.GetItem(toIndex);
                if (tradeItem is {Quantity: > 0} && toSlot is not {Quantity: > 0})
                    return fromInv.SwitchItem(fromIndex, toIndex);
            }

            return false;
        }

        public static bool Give (string from, int index, string to, bool rearrange)
        {
            var fromInv = GetInventory(from);
            var toInv = GetInventory(to);
            if (fromInv != null && toInv != null)
            {
                var tradeItem = fromInv.GetItem(index);
                if (tradeItem is {Quantity: > 0})
                {
                    if (toInv.Name != tradeItem.InstanceId)
                    {
                        var manager = GraphManager.instance.runtimeSettings.itemManager;
                        var itemData = manager.GetItemData(tradeItem.ItemId);
                        if (itemData != null)
                        {
                            if (toInv.HasSpace(tradeItem.ItemId, tradeItem.Quantity, itemData.Stackable))
                            {
                                if (toInv.AddItem(itemData, tradeItem.Quantity, tradeItem.Decay))
                                {
                                    fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out _, index);
                                    if (rearrange)
                                        fromInv.ArrangeItem();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool Give (string from, int fromIndex, string to, int toIndex)
        {
            var fromInv = GetInventory(from);
            var toInv = GetInventory(to);
            if (fromInv != null && toInv != null)
            {
                var tradeItem = fromInv.GetItem(fromIndex);
                if (tradeItem is {Quantity: > 0})
                {
                    if (toInv.Name != tradeItem.InstanceId)
                    {
                        var manager = GraphManager.instance.runtimeSettings.itemManager;
                        var receiveSlot = toInv.GetItem(toIndex);
                        if (receiveSlot is not {Quantity: > 0})
                        {
                            var itemData = manager.GetItemData(tradeItem.ItemId);
                            if (itemData != null)
                            {
                                if (tradeItem.Quantity <= toInv.Stack)
                                {
                                    if (toInv.SetItem(itemData, tradeItem.InstanceId, tradeItem.Quantity, tradeItem.Decay, toIndex))
                                    {
                                        fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out _, fromIndex);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static List<InventoryItem> Discard (string inv, int index)
        {
            var fromInv = GetInventory(inv);
            var tradeItem = fromInv?.GetItem(index);
            if (tradeItem != null)
            {
                fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out var removed, index);
                return removed;
            }

            return null;
        }

        public static bool Use (string inv, int index, MultiTag consume)
        {
            var fromInv = GetInventory(inv);
            var fromItem = fromInv?.GetItem(index);
            if (fromItem != null)
            {
                var manager = GraphManager.instance.runtimeSettings.itemManager;
                var itemData = manager.GetItemData(fromItem.ItemId);
                if (!itemData.isEmptyStatus || !itemData.isEmptySkill)
                {
                    if (consume.ContainAny(itemData.tags, false))
                    {
                        for (var i = 0; i < fromItem.Quantity; i++)
                        {
                            if (!itemData.isEmptyStatus)
                                AddCharacterAttackStatus(inv, itemData);
                            if (!itemData.isEmptySkill)
                                AddCharacterAttackSkill(inv, itemData);
                        }

                        Discard(inv, index);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Trade (string from, int index, string to, string wallet, string coinItemId, bool buy, bool rearrange)
        {
            var fromInv = GetInventory(from);
            var toInv = GetInventory(to);
            var coinInv = GetInventory(wallet);
            if (fromInv != null && toInv != null && coinInv != null)
            {
                var tradeItem = fromInv.GetItem(index);
                if (tradeItem is {Quantity: > 0})
                {
                    var manager = GraphManager.instance.runtimeSettings.itemManager;
                    var itemData = manager.GetItemData(tradeItem.ItemId);
                    var coinData = manager.GetItemData(coinItemId);
                    if (itemData != null && coinData != null)
                    {
                        if (buy)
                        {
                            if (toInv.HasSpace(tradeItem.ItemId, tradeItem.Quantity, itemData.Stackable))
                            {
                                var totalCost = itemData.cost * tradeItem.Quantity;
                                if (coinInv.HasItem(coinItemId, totalCost))
                                {
                                    if (toInv.AddItem(itemData, tradeItem.Quantity))
                                    {
                                        coinInv.RemoveItem(coinItemId, totalCost, out _);
                                        fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out _, index);
                                        if (rearrange)
                                            fromInv.ArrangeItem();
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var totalCost = itemData.cost * tradeItem.Quantity;
                            fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out _, index);
                            if (rearrange)
                                fromInv.ArrangeItem();
                            coinInv.AddItem(coinData, totalCost);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool Trade (string from, int fromIndex, string to, int toIndex, string wallet, string coinItemId, bool buy)
        {
            var fromInv = GetInventory(from);
            var toInv = GetInventory(to);
            var coinInv = GetInventory(wallet);
            if (fromInv != null && toInv != null && coinInv != null)
            {
                var tradeItem = fromInv.GetItem(fromIndex);
                if (tradeItem is {Quantity: > 0})
                {
                    var manager = GraphManager.instance.runtimeSettings.itemManager;
                    var itemData = manager.GetItemData(tradeItem.ItemId);
                    var coinData = manager.GetItemData(coinItemId);
                    if (itemData != null && coinData != null)
                    {
                        if (buy)
                        {
                            var receiveSlot = toInv.GetItem(toIndex);
                            if (receiveSlot is not {Quantity: > 0})
                            {
                                if (tradeItem.Quantity <= toInv.Stack)
                                {
                                    var totalCost = itemData.cost * tradeItem.Quantity;
                                    if (coinInv.HasItem(coinItemId, totalCost))
                                    {
                                        if (toInv.SetItem(itemData, tradeItem.InstanceId, tradeItem.Quantity, toIndex))
                                        {
                                            coinInv.RemoveItem(coinItemId, totalCost, out _);
                                            fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out _, fromIndex);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var totalCost = itemData.cost * tradeItem.Quantity;
                            fromInv.RemoveItem(tradeItem.ItemId, tradeItem.Quantity, out _, fromIndex);
                            coinInv.AddItem(coinData, totalCost);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool Stack (string from, int fromIndex, int toIndex)
        {
            var fromInv = GetInventory(from);
            var fromItem = fromInv?.GetItem(fromIndex);
            if (fromItem is {Quantity: > 0})
            {
                var manager = GraphManager.instance.runtimeSettings.itemManager;
                var itemData = manager.GetItemData(fromItem.ItemId);
                if (itemData != null)
                {
                    var receiveSlot = fromInv?.GetItem(toIndex);
                    if (receiveSlot is {Quantity: >= 0})
                    {
                        if (receiveSlot.ItemId == fromItem.ItemId)
                        {
                            if (itemData.Stackable && receiveSlot.Quantity < fromInv.Stack)
                            {
                                var available = fromInv.Stack - receiveSlot.Quantity;
                                var quantity = fromItem.Quantity <= available ? fromItem.Quantity : available;
                                if (fromInv.AddItem(itemData, quantity, toIndex, false))
                                {
                                    fromInv.RemoveItem(fromItem.ItemId, quantity, out _, fromIndex);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static void RemoveInventoryItem (string inventoryName, string itemID, int quality, bool includeSubInv)
        {
            var remaining = quality;
            var inv = InventoryManager.GetInventory(inventoryName);
            if (inv != null)
            {
                if (!inv.RemoveItem(itemID, quality, out var removed))
                {
                    for (var j = 0; j < removed.Count; j++)
                        remaining -= removed[j].Quantity;
                    if (includeSubInv)
                    {
                        var manager = GraphManager.instance.runtimeSettings.itemManager;
                        if (manager != null)
                        {
                            for (var i = 0; i < inv.Count; i++)
                            {
                                var item = inv.GetItem(i);
                                if (item is {isSolid: true})
                                {
                                    var subItemData = manager.GetItemData(item.ItemId);
                                    if (subItemData != null && subItemData.invBehaviour != null)
                                    {
                                        var subInv = InventoryManager.GetInventory(item.InstanceId);
                                        if (!subInv.RemoveItem(itemID, remaining, out removed))
                                        {
                                            for (var j = 0; j < removed.Count; j++)
                                                remaining -= removed[j].Quantity;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool HaveInventoryItem (string inventoryName, string itemID, bool includeSubInv)
        {
            var inv = GetInventory(inventoryName);
            if (inv != null)
            {
                if (inv.HasItem(itemID, 1))
                    return true;
                if (includeSubInv)
                {
                    var manager = GraphManager.instance.runtimeSettings.itemManager;
                    if (manager != null)
                    {
                        for (var i = 0; i < inv.Count; i++)
                        {
                            var item = inv.GetItem(i);
                            if (item is {isSolid: true})
                            {
                                var subItemData = manager.GetItemData(item.ItemId);
                                if (subItemData != null && subItemData.invBehaviour != null)
                                {
                                    var subInv = GetInventory(item.InstanceId);
                                    if (subInv.HasItem(itemID, 1))
                                        return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }


        public static int GetInventoryItemTotalQuantity (string inventoryName, string itemID, bool includeSubInv)
        {
            var totalQuantity = 0;
            var inv = GetInventory(inventoryName);
            if (inv != null)
            {
                totalQuantity = inv.GetItemTotalQuantity(itemID);
                if (includeSubInv)
                {
                    var manager = GraphManager.instance.runtimeSettings.itemManager;
                    if (manager != null)
                    {
                        for (var i = 0; i < inv.Count; i++)
                        {
                            var item = inv.GetItem(i);
                            if (item is {isSolid: true})
                            {
                                var subItemData = manager.GetItemData(item.ItemId);
                                if (subItemData != null && subItemData.invBehaviour != null)
                                {
                                    var subInv = GetInventory(item.InstanceId);
                                    totalQuantity += subInv.GetItemTotalQuantity(itemID);
                                }
                            }
                        }
                    }
                }
            }

            return totalQuantity;
        }

        public static void TriggerUpdate (string invName)
        {
            ReInventoryController.UpdateInventory(invName);
        }

        public static void AddCharacterAttackStatus (string inv, ItemData itemData)
        {
            for (var i = 0; i < itemData.attackStatusPack.Length; i++)
            {
                var status = itemData.attackStatusPack[i];
                if (status != null)
                    CharacterOperator.AddItemAttackStatus(inv, status);
            }
        }

        public static void RemoveCharacterAttackStatus (string inv, ItemData itemData)
        {
            for (var i = 0; i < itemData.attackStatusPack.Length; i++)
            {
                var status = itemData.attackStatusPack[i];
                if (status != null)
                    CharacterOperator.RemoveItemAttackStatus(inv, status);
            }
        }

        public static void AddCharacterAttackSkill (string inv, ItemData itemData)
        {
            for (var i = 0; i < itemData.attackSkillPack.Length; i++)
            {
                var skill = itemData.attackSkillPack[i];
                if (skill != null)
                    CharacterOperator.AddItemAttackSkill(inv, skill);
            }
        }

        public static void RemoveCharacterAttackSkill (string inv, ItemData itemData)
        {
            for (var i = 0; i < itemData.attackSkillPack.Length; i++)
            {
                var skill = itemData.attackSkillPack[i];
                if (skill != null)
                    CharacterOperator.RemoveItemAttackSkill(inv, skill);
            }
        }

        public static void ApplyCharacterAttackStatus (CharacterOperator charOp, string invName)
        {
            var inv = GetInventory(invName);
            if (inv != null)
            {
                for (var i = 0; i < inv.Count; i++)
                {
                    var item = inv.GetItem(i);
                    if (item is {isSolid: true})
                    {
                        var manager = GraphManager.instance.runtimeSettings.itemManager;
                        var itemData = manager.GetItemData(item.ItemId);
                        if (itemData != null && item.isUsable)
                        {
                            if (!itemData.isEmptyStatus)
                            {
                                for (var j = 0; j < itemData.attackStatusPack.Length; j++)
                                {
                                    var status = itemData.attackStatusPack[j];
                                    if (status != null)
                                        charOp.AddAttackStatus(status, charOp);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ApplyCharacterAttackSkill (CharacterOperator charOp, string invName)
        {
            var inv = GetInventory(invName);
            if (inv != null)
            {
                for (var i = 0; i < inv.Count; i++)
                {
                    var item = inv.GetItem(i);
                    if (item is {isSolid: true})
                    {
                        var manager = GraphManager.instance.runtimeSettings.itemManager;
                        var itemData = manager.GetItemData(item.ItemId);
                        if (itemData != null && item.isUsable)
                        {
                            if (!itemData.isEmptySkill)
                            {
                                for (var j = 0; j < itemData.attackSkillPack.Length; j++)
                                {
                                    var skill = itemData.attackSkillPack[j];
                                    if (skill != null)
                                        charOp.SetAttackSkill(skill, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static InventoryBehaviour.ApplyStatusTrigger GetInventoryApplyStatusType (string inv)
        {
            return CharacterOperator.GetInvApplyStatusType(inv);
        }

        public static InventoryBehaviour.ApplySkillTrigger GetInventoryApplySkillType (string inv)
        {
            return CharacterOperator.GetInvApplySkillType(inv);
        }

        public static void ApplyInventoryAttackStatus (string inv)
        {
            CharacterOperator.ApplyInventoryStatus(inv);
        }

        public static void ApplyInventoryAttackSkill (string inv)
        {
            CharacterOperator.ApplyInventorySkill(inv);
        }

        public static void ApplyAllInventoryAttackStatus ()
        {
            CharacterOperator.ApplyAllInventoryStatus();
        }

        public static void ApplyAllInventoryAttackSkill ()
        {
            CharacterOperator.ApplyAllInventorySkill();
        }
    }
}