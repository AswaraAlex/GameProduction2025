using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Reshape.Unity;
using UnityEngine.Events;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class LootController : BaseBehaviour
    {
        private static List<LootController> list;

        public enum WhenEmptyBehaviour
        {
            None,
            Remove = 10,
            Event = 50
        }

        [SerializeField]
        [BoxGroup("Loot Info")]
        [InlineProperty]
        private StringProperty lootName;
        
        [SerializeField]
        [BoxGroup("Loot Info")]
        private WordVariable lootInvVariable;

        [ShowInInspector, HideInEditorMode]
        [BoxGroup("Loot Info")]
        public string lootId => lootData == null ? string.Empty : lootData.id;

        [ShowInInspector, HideInEditorMode]
        [BoxGroup("Loot Info")]
        public LootPack lootPack => lootData == null ? null : lootData.loot;

        [SerializeField]
        [BoxGroup("Behaviour")]
        [LabelText("Remove At Empty")]
        private WhenEmptyBehaviour emptyAction;
        
        [SerializeField]
        [BoxGroup("Spawn at Start")]
        [LabelText("Loot Pack")]
        private LootPack startLootPack;

        [BoxGroup("Behaviour")]
        [LabelText("Event")]
        [ShowIf("@emptyAction == WhenEmptyBehaviour.Event")]
        public UnityEvent emptyEvent;

        private LootData lootData;
        
        public static LootController Generate (GameObject lootGo, LootPack lootPack)
        {
            if (lootGo != null)
            {
                if (!lootGo.TryGetComponent(out LootController controller))
                    controller = lootGo.AddComponent<LootController>();
                controller.Init(new LootData(lootPack));
                return controller;
            }

            return null;
        }
        
        public static void Generate (GameObject lootGo, List<InventoryItem> loots)
        {
            if (lootGo != null)
            {
                if (!lootGo.TryGetComponent(out LootController controller))
                    controller = lootGo.AddComponent<LootController>();
                controller.Init(new LootData(loots));
            }
        }

        public static void CleanAll ()
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    list[i].Clear();
                list.Clear();
            }
        }
        
        public static bool Contains (string id)
        {
            if (list != null)
                for (var i = 0; i < list.Count; i++)
                    if (list[i].lootId == id)
                        return true;
            return false;
        }

        public void GetLootName (WordVariable word)
        {
            word.SetValue(lootName);
        }

        public void Init (LootData data)
        {
            lootData = data;
            TriggerGenerateUsage();
        }

        public void PickUp (string inv)
        {
            if (lootData.PutItemsIntoInventory(inv))
                OnInventoryEmpty();
        }

        public void Show ()
        {
            lootInvVariable.SetValue(lootId);
            InventoryManager.TriggerUpdate(lootId);
            InventoryCanvas.ShowCanvas("", lootId, true);
            InventoryCanvas.OnInvClosed -= OnInvCanvasClosed;
            InventoryCanvas.OnInvClosed += OnInvCanvasClosed;
        }

        private void OnInventoryEmpty ()
        {
            if (emptyAction == WhenEmptyBehaviour.Remove)
                FinishUsage();
            else if (emptyAction == WhenEmptyBehaviour.Event)
                emptyEvent?.Invoke();
        }

        private void OnInvCanvasClosed (string panelName)
        {
            if (panelName == lootId)
            {
                InventoryCanvas.OnInvClosed -= OnInvCanvasClosed;
                if (lootData != null && !lootData.HaveItemsInInventory())
                    OnInventoryEmpty();
            }
        }

        protected virtual void FinishUsage ()
        {
            InventoryCanvas.OnInvClosed -= OnInvCanvasClosed;
            BackToPool();
            lootData?.TriggerClear();
            lootData = null;
        }

        public override void PostBegin ()
        {
            Init(new LootData(startLootPack));
            DonePostBegin();
        }

        protected void Awake ()
        {
            list ??= new List<LootController>();
            list.Add(this);
            if (startLootPack != null)
                PlanPostBegin();
        }

        protected void OnDestroy ()
        {
            InventoryCanvas.OnInvClosed -= OnInvCanvasClosed;
            list?.Remove(this);
            lootData?.Terminate();
            lootData = null;
        }

        private void TriggerGenerateUsage ()
        {
            lootData.TriggerGenerate();
        }

        private void BackToPool ()
        {
            var me = gameObject;
            me.SetActiveOpt(false);
            InsertIntoPool(me, true);
        }

        private void Clear ()
        {
            lootData?.TriggerClear();
            ClearPool(gameObject.name);
            Destroy(gameObject);
        }
    }
}