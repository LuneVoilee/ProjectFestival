#region

using GamePlay.AI;
using UnityEngine;

#endregion

public class AntiStuckBehaviour : ContextBehavior
{
    public override void Evaluate(ContextMap map, Transform agentTransform)
    {
        for (var i = 0; i < ContextMap.GetDirectionCount(); i++)
        {
            map.Interest[i] += 0.2f;
        }
    }
}