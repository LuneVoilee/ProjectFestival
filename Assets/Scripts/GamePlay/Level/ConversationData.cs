#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GamePlay.Level
{
    [Serializable]
    public enum EEffectType
    {
        AddValue,
        SetValue
    }

    [Serializable]
    public enum EValueType
    {
        Money,
        Happiness
    }

    [Serializable]
    public enum ETriggerType
    {
        StartGame,
        CheckMoney,
        EndGame
    }

    [Serializable]
    public class ConversationEffect
    {
        public EEffectType EffectType;
        public EValueType TargetVar;
        public float Value;
    }

    [CreateAssetMenu(fileName = "ConversationData",
        menuName = "AScriptableObject/Conversation Data")]
    public class ConversationData : ScriptableObject
    {
        public int ID;
        public ETriggerType TriggerType;
        public List<string> Conversations;
        public List<ConversationEffect> Effects;
    }
}