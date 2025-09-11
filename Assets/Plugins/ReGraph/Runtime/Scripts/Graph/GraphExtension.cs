using Reshape.Unity;
using UnityEngine;

namespace Reshape.ReGraph
{
    public static class GraphExtension
    {
        public static void Deactivate (this GameObject go)
        {
            if (go.activeSelf)
            {
                if (go.TryGetComponent(out GraphRunner graph))
                {
                    var result = graph.TriggerDeactivate();
                    if (result == null || result.isFailed)
                        go.SetActiveOpt(false);
                }
                else
                    go.SetActiveOpt(false);
            }
        }
        
        public static void Activate (this GameObject go)
        {
            if (go.activeSelf)
            {
                if (go.TryGetComponent(out GraphRunner graph))
                {
                    var result = graph.TriggerActivate();
                    if (result == null || result.isFailed)
                        go.SetActiveOpt(true);
                }
                else
                    go.SetActiveOpt(true);
            }
        }
    }
}