using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    public class BaseScriptable : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector]
        public bool showHints;

        [MenuItem("CONTEXT/BaseScriptable/Hints Display/Show", false)]
        public static void ShowHints (MenuCommand command)
        {
            var comp = (BaseScriptable) command.context;
            comp.showHints = true;
        }

        [MenuItem("CONTEXT/BaseScriptable/Hints Display/Show", true)]
        public static bool IsShowHints (MenuCommand command)
        {
            var comp = (BaseScriptable) command.context;
            if (comp.showHints)
                return false;
            return true;
        }

        [MenuItem("CONTEXT/BaseScriptable/Hints Display/Hide", false)]
        public static void HideHints (MenuCommand command)
        {
            var comp = (BaseScriptable) command.context;
            comp.showHints = false;
        }

        [MenuItem("CONTEXT/BaseScriptable/Hints Display/Hide", true)]
        public static bool IsHideHints (MenuCommand command)
        {
            var comp = (BaseScriptable) command.context;
            if (!comp.showHints)
                return false;
            return true;
        }
#endif
    }
}