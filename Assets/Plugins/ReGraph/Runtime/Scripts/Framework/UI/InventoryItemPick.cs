using Reshape.Unity;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class InventoryItemPick : BaseBehaviour
    {
        public Canvas parentCanvas;
        public Image itemIcon;
        
        public void ShowPickInfo (ItemData itemData, InventoryItem slotData)
        {
            itemIcon.sprite = itemData.icon;
            SetPositionToCursor();
            itemIcon.gameObject.SetActiveOpt(true);
            gameObject.SetActiveOpt(true);
        }

        public void Hide ()
        {
            gameObject.SetActiveOpt(false);
        }
        
        private void SetPositionToCursor ()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, ReInput.mousePosition, parentCanvas.worldCamera, out var movePos);
            transform.position = parentCanvas.transform.TransformPoint(movePos);
        }

        protected void Update ()
        {
            SetPositionToCursor();
        }
    }
}