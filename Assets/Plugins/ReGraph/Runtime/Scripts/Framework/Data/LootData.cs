using System;
using System.Collections.Generic;
using Reshape.ReGraph;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class LootData
    {
        public enum Type
        {
            Pack = 1,
            Items = 2
        }

        public GraphExecution lastExecuteResult;
        public Type type;

        private LootPack lootPack;
        private List<InventoryItem> lootItems;

        public string id { get; private set; }
        public string lootPackName => lootPack == null ? string.Empty : lootPack.name;
        public LootPack loot => lootPack;

        public LootData (LootPack pack)
        {
            type = Type.Pack;
            id = ReUniqueId.GenerateId(false);
            lootPack = pack;
        }

        public LootData (List<InventoryItem> loots)
        {
            type = Type.Items;
            id = ReUniqueId.GenerateId(false);
            lootItems = loots;
        }

        public void Terminate ()
        {
            ReUniqueId.ReturnId(id);
            lastExecuteResult = null;
            lootPack = null;
        }

        public bool PutItemsIntoInventory (string inv)
        {
            if (string.IsNullOrEmpty(inv))
                return false;
            var fromInv = InventoryManager.GetInventory(id);
            var slotCount = fromInv.Count;
            for (var i = 0; i < slotCount; i++)
            {
                var tradeItem = fromInv.GetItem(i);
                if (tradeItem is {Quantity: > 0})
                    if (!InventoryManager.Give(id, i, inv, false))
                        return false;
            }

            return true;
        }

        public bool HaveItemsInInventory ()
        {
            var fromInv = InventoryManager.GetInventory(id);
            if (fromInv != null)
                return !fromInv.IsEmpty();
            return false;
        }

        public void TriggerGenerate ()
        {
            if (type == Type.Pack)
            {
                if (lootPack == null)
                {
                    ReDebug.LogWarning("Loot Data Warning", "TriggerGenerate activation being ignored due to missing loot pack");
                }
                else if (InventoryManager.CreateInventory(id, lootPack.size, lootPack.stack, lootPack.rows))
                {
                    lootPack.TriggerGenerate(this);
                }
            }
            else if (type == Type.Items)
            {
                if (lootItems == null)
                {
                    ReDebug.LogWarning("Loot Data Warning", "TriggerGenerate activation being ignored due to missing loot items");
                }
                else
                {
                    var itemsCount = lootItems.Count;
                    var slotCount = 0;
                    var stack = 1;
                    var noRow = 0;
                    var tag = 0;
                    var sizeLimit = default(Vector2Int);
                    var countLimit = 0;
                    var lootInvBeh = GraphManager.instance.runtimeSettings.dropInvBehaviour;
                    if (lootInvBeh == null)
                    {
                        for (var i = 0; i < itemsCount; i++)
                        {
                            if (lootItems[i].isMultiSlot)
                                slotCount += lootItems[i].Size.x * lootItems[i].Size.y;
                            else
                                slotCount += 1;
                        }
                    }
                    else
                    {
                        slotCount = lootInvBeh.Size;
                        stack = lootInvBeh.Stack;
                        noRow = lootInvBeh.Rows;
                        tag = lootInvBeh.Tags;
                        sizeLimit = lootInvBeh.SizeLimit;
                        countLimit = lootInvBeh.CountLimit;
                    }
                    
                    if (InventoryManager.CreateInventory(id, slotCount, stack, noRow, tag, sizeLimit, countLimit))
                    {
                        if (itemsCount > 0)
                        {
                            var inv = InventoryManager.GetInventory(id);
                            if (inv != null)
                            {
                                var manager = GraphManager.instance.runtimeSettings.itemManager;
                                for (var i = 0; i < itemsCount; i++)
                                {
                                    var invItem = lootItems[i];
                                    var itemData = manager.GetItemData(invItem.ItemId);
                                    if (itemData != null)
                                        inv.AddItem(itemData, invItem.InstanceId, invItem.Quantity, invItem.Decay, 0, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void TriggerClear ()
        {
            if (type == Type.Pack)
            {
                if (lootPack != null)
                {
                    InventoryManager.DestroyInventory(id);
                }
            }
            else if (type == Type.Items)
            {
                if (lootItems != null)
                {
                    InventoryManager.DestroyInventory(id);
                }
            }
        }
    }
}