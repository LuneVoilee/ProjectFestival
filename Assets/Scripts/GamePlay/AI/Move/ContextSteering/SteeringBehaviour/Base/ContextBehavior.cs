#region

using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public abstract class ContextBehavior : MonoBehaviour
    {
        public abstract void Evaluate(ContextMap map, Transform agentTransform);
    }
}