using System.Collections;
using System.Collections.Generic;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class SpeechOwnerController : BaseBehaviour
    {
        private static List<SpeechOwnerController> list;
        private static int globalUpdateType;
        private static float globalShowDuration = -1f;
        
        private const string GO_NAME = "SpeechBubble";

        public Transform spawnLocation;
        public Transform spawnParent;
        public float showDuration = -1f;
        
        [ValueDropdown("UpdateTypeChoice")]
        public int updateType;
        
        private SpeechBubbleController currentBubble;
        private Camera followCamera;
        private float fontSize = -1f;
        private Color fontColor;
        private List<string> pendingMessages;
        private float showTime;
        private bool hiding;

        //-----------------------------------------------------------------
        //-- static methods
        //-----------------------------------------------------------------

        public static void SetGlobalUpdateType (int t)
        {
            globalUpdateType = t;
        }
        
        public static void SetGlobalShowDuration (float d)
        {
            globalShowDuration = d;
        }
        
        //-----------------------------------------------------------------
        //-- protected methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- public methods
        //-----------------------------------------------------------------

        public void SetFontColor (string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out fontColor);
        }

        public void SetFontSize (float s)
        {
            fontSize = s;
        }
        
        public void SetUpdateType (int t)
        {
            updateType = t;
        }

        public void ClearPending ()
        {
            pendingMessages.Clear();
        }
        
#if REGRAPH_DEV_DEBUG
        [Button]
#endif
        public void Show (string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            if (!currentBubble)
            {
                if (followCamera == null)
                    followCamera = CameraManager.GetViewCamera();
                var type = updateType;
                if (globalUpdateType != 0 && updateType == 0)
                    type = globalUpdateType;
                currentBubble = SpeechBubbleController.Spawn(message, spawnLocation, GO_NAME, spawnParent, fontSize, fontColor, type, followCamera, spawnLocation);
                currentBubble.Show();
                showTime = 0;
            }
            else
            {
                pendingMessages.Add(message);
            }
        }
        
#if REGRAPH_DEV_DEBUG
        [Button]
#endif
        public void Hide ()
        {
            if (currentBubble && !hiding)
            {
                hiding = true;
                currentBubble.Hide();
                currentBubble.OnFinishUsage += OnBubbleFinishUsage;
            }
        }
        
        //-----------------------------------------------------------------
        //-- mono methods
        //-----------------------------------------------------------------

        protected void Awake ()
        {
            pendingMessages ??= new List<string>();
            list ??= new List<SpeechOwnerController>();
            list.Add(this);
            PlanTick();
        }

        protected void OnDestroy ()
        {
            OmitTick();
            list.Remove(this);
        }

        //-----------------------------------------------------------------
        //-- BaseBehaviour methods
        //-----------------------------------------------------------------

        public override void Tick ()
        {
            if (currentBubble == null)
            {
                if (pendingMessages is {Count: > 0})
                {
                    Show(pendingMessages[0]);
                    pendingMessages.RemoveAt(0);
                }
            }
            else if (!hiding)
            {
                showTime += ReTime.deltaTime;
                var duration = showDuration;
                if (duration < 0)
                    duration = globalShowDuration;
                if (duration > 0 && showTime >= duration)
                {
                    showTime = 0;
                    Hide();
                }
            }
        }
        
        //-----------------------------------------------------------------
        //-- private methods
        //-----------------------------------------------------------------

        private void OnBubbleFinishUsage ()
        {
            currentBubble = null;
            hiding = false;
        }
        
        //-----------------------------------------------------------------
        //-- editor methods
        //-----------------------------------------------------------------
        
#if UNITY_EDITOR
        private IEnumerable UpdateTypeChoice = new ValueDropdownList<int>()
        {
            {"None", 0},
            {"Face Camera", SpeechBubbleController.TYPE_FACE_CAMERA},
            {"Follow Camera", SpeechBubbleController.TYPE_FOLLOW_CAMERA},
        };
#endif
    }
}