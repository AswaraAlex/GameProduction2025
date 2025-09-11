using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Reshape.ReFramework
{
    public partial class ItemAddonInfo
    {
        [LabelText("Visible Body Part")]
        [ShowIf("ShowBodyPartTag")]
        [Hint("ShowHints", "Define visibility of  body part when equipped this item.")]
        public MultiTag visibleBodyPartTag = new MultiTag("Body Part Tags", typeof(MultiTagBodyPart));

#if UNITY_EDITOR
        private bool ShowBodyPartTag ()
        {
            if (data != null)
            {
                if (data.multiTags.Contain("Headgear"))
                    return true;
            }

            return false;
        }

        private bool ShowHints ()
        {
            return data != null && data.showHints;
        }
#endif
    }
}