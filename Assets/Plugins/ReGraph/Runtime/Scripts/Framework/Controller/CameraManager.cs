using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class CameraManager : BaseBehaviour
    {
        protected static CameraManager instance;
        
        [Hint("showHints", "Define the camera that render display to player. \n\n" +
                           "Position of Transform component at this gameobject in Prefab always uses default value.\nRotation Y & Z of Transform component at this gameobject in Prefab always uses default value. \n\n" +
                           "This gameobject is always be a child of its confiner.")]
        [PropertyOrder(-9999)]
        public Camera viewCamera;
        
        //-----------------------------------------------------------------
        //-- static methods
        //-----------------------------------------------------------------

        public static Camera GetViewCamera ()
        {
            return instance.viewCamera;
        }
        
        //-----------------------------------------------------------------
        //-- public methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- protected methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- mono methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- BaseBehaviour methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- private methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- editor methods
        //-----------------------------------------------------------------
    }
}