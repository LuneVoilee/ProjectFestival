#region

using System.Collections.Generic;
using Core;
using UnityEngine;

#endregion

namespace GamePlay.Level
{
    public class ConversationController : MonoBehaviour
    {
        public List<ConversationData> ConversationData;
        private bool m_FirstBankrupt = true;

        private void OnEnable()
        {
            UIEvents.OnConversationEndAction += ApplyEffects;
        }

        private void OnDisable()
        {
            UIEvents.OnConversationEndAction -= ApplyEffects;
        }

        private void Start()
        {
            foreach (var data in ConversationData)
            {
                if (data.TriggerType == ETriggerType.StartGame)
                {
                    GPEvents.OnBeginConversationAction?.Invoke(data.ID, data.Conversations);
                    break;
                }
            }
        }

        private void Update()
        {
            if (ValueManager.Instance.CurrentLevel == 3)
            {
                foreach (var data in ConversationData)
                {
                    if (data.TriggerType != ETriggerType.EndGame)
                    {
                        continue;
                    }

                    GPEvents.OnBeginConversationAction?.Invoke(data.ID, data.Conversations);
                    break;
                }
            }

            CheckBankrupt();
        }

        private void CheckBankrupt()
        {
            if (ValueManager.Instance.CanAfford(-0.1f))
            {
                return;
            }

            if (m_FirstBankrupt)
            {
                m_FirstBankrupt = false;

                ValueManager.Instance.Difficulty.Value = 0.8f;

                foreach (var data in ConversationData)
                {
                    if (data.TriggerType != ETriggerType.CheckMoney)
                    {
                        continue;
                    }

                    GPEvents.OnBeginConversationAction?.Invoke(data.ID, data.Conversations);
                }
            }
        }

        private void ApplyEffects(int id)
        {
            foreach (var data in ConversationData)
            {
                if (data.ID == id)
                {
                    var effects = data.Effects;
                    foreach (var effect in effects)
                    {
                        ApplyEffect(effect);
                    }

                    break;
                }
            }
        }

        private void ApplyEffect(ConversationEffect effect)
        {
            switch (effect.EffectType)
            {
                case EEffectType.AddValue:
                    ValueManager.Instance.AddValue(effect.TargetVar, effect.Value);
                    break;
                case EEffectType.SetValue:
                    ValueManager.Instance.SetValue(effect.TargetVar, effect.Value);
                    break;
            }
        }
    }
}