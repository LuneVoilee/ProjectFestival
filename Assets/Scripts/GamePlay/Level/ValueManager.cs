#region

using System;
using System.Collections.Generic;
using Core;
using Core.Reactive;
using Tool;
using UnityEngine;

#endregion

namespace GamePlay.Level
{
    [Serializable]
    public class LevelConfig
    {
        public float NeedHappiness;
        [Header("Spawn rate (people per second)")] public int VisitorGenerateRate;
        [Header("Random offset")] public int VisitorGenerateRandomOffset;
    }

    [Serializable]
    public class HappinessConfig
    {
        public int RandomHappinessMinGet;
        public int RandomHappinessMaxGet;
        public int RandomUnhappinessMinGet;
        public int RandomUnhappinessMaxGet;
    }

    public class ValueManager : SingletonMono<ValueManager>
    {
        public List<LevelConfig> LevelConfigs;

        public HappinessConfig HappinessGetRange;

        public float InitialMoney;

        public ReactiveValue<int> CurrentLevel = new();

        public ReactiveValue<float> Money { get; } = new();
        public ReactiveValue<float> Happiness { get; } = new();

        public ReactiveValue<float> Difficulty { get; } = new(-1f);

        public float TotalMoney
        {
            get => Money.Value;
            set => Money.Value = value;
        }

        public float TotalHappiness
        {
            get => Happiness.Value;
            set => Happiness.Value = value;
        }

        private bool IsFirstFrameOfUpdate = true;

        protected override void Awake()
        {
            base.Awake();
            GPEvents.GetEconomyData = () => (Money, Happiness);
            GPEvents.GetDifficulty = () => Difficulty;
        }

        private void OnEnable()
        {
            UIEvents.OnHopeDifficultyChangeAction += OnDifficultyChange;
        }


        private void OnDisable()
        {
            UIEvents.OnHopeDifficultyChangeAction -= OnDifficultyChange;
        }

        private void Update()
        {
            if (IsFirstFrameOfUpdate)
            {
                //NOTICE: To update the UI properly, values must be set after Start
                TotalMoney = InitialMoney;
                TotalHappiness = 0f;

                Difficulty.Value = 1f;

                IsFirstFrameOfUpdate = false;
            }

            UpdateLevel();
        }

        private void UpdateLevel()
        {
            if (CurrentLevel >= LevelConfigs.Count)
            {
                return;
            }

            var nextLevelConfig = LevelConfigs[CurrentLevel];
            if (TotalHappiness >= nextLevelConfig.NeedHappiness)
            {
                CurrentLevel.Value += 1;
                GPEvents.OnLevelChangeAction?.Invoke(
                    CurrentLevel,
                    nextLevelConfig.VisitorGenerateRate,
                    nextLevelConfig.VisitorGenerateRandomOffset);
            }
        }

        private void OnDifficultyChange(float value)
        {
            Difficulty.Value = value;
        }

        public bool CanAfford(float cost)
        {
            return TotalMoney >= cost;
        }

        public bool TrySpendMoney(float cost)
        {
            if (CanAfford(cost))
            {
                TotalMoney -= cost;

                return true;
            }

            return false;
        }

        public void AddValue(EValueType targetVar, float value)
        {
            switch (targetVar)
            {
                case EValueType.Money:
                    AddMoney(value);
                    break;
                case EValueType.Happiness:
                    AddHappiness(value);
                    break;
                default:
                    Debug.LogWarning($"{targetVar} is invalid");
                    break;
            }
        }

        public void SetValue(EValueType effectTargetVar, float effectValue)
        {
            switch (effectTargetVar)
            {
                case EValueType.Money:
                    TotalMoney = effectValue;
                    break;
                case EValueType.Happiness:
                    TotalHappiness = effectValue;
                    break;
                default:
                    Debug.LogWarning($"{effectTargetVar} is invalid");
                    break;
            }
        }

        public void AddMoney(float amount)
        {
            if (amount >= 0)
            {
                TotalMoney += amount / Difficulty;
            }
            else
            {
                TotalMoney += amount * Difficulty;
            }
        }

        public void AddHappiness(float amount)
        {
            if (amount >= 0)
            {
                TotalHappiness += amount / Difficulty;
            }
            else
            {
                TotalHappiness += amount * Difficulty;
            }
        }
    }
}