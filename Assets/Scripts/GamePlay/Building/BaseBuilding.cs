#region

using System;
using System.Collections;
using System.Collections.Generic;
using GamePlay.AI;
using GamePlay.Level;
using UnityEngine;

#endregion

namespace GamePlay.Building
{
    [Serializable]
    public class RuntimeBuildingData
    {
        public int ID;

        public float HappinessBenefit;
        public float MoneyBenefit;

        //Maximum number of concurrent guests
        public int ReceptionCount;

        //The duration of tourists' stay in this building (in seconds)
        public float StayTime;

        public bool NeedToEnterTheBuilding;

        public RuntimeBuildingData(BuildingData buildingData)
        {
            ID = buildingData.ID;
            HappinessBenefit = buildingData.HappinessBenefit;
            MoneyBenefit = buildingData.MoneyBenefit;
            ReceptionCount = buildingData.ReceptionCount;
            StayTime = buildingData.StayTime;
            NeedToEnterTheBuilding = buildingData.NeedToEnterTheBuilding;
        }
    }

    public class BaseBuilding : MonoBehaviour
    {
        public RuntimeBuildingData Data;

        public Vector3 VisitPosition { get; private set; }


        [SerializeField] private List<VisitorController> m_CurrentVisitors = new();

        public void Init(BuildingData buildingData)
        {
            Data = new RuntimeBuildingData(buildingData);
        }

        protected virtual void Awake()
        {
            var visitPoint = transform.Find("VisitPoint");

            VisitPosition = visitPoint ? visitPoint.position : transform.position;
            //Debug.Log("VisitPosition:" + VisitPosition);
        }

        private void OnEnable()
        {
            BuildingManager.Instance.AllBuildings.Add(this);
        }

        private void OnDisable()
        {
            if (BuildingManager.Instance)
            {
                BuildingManager.Instance.AllBuildings.Remove(this);
            }
        }

        public bool OnVisitorArrived(VisitorController visitor)
        {
            /*if (Data.ID == 2)
            {
                Debug.Log("1");
            }*/

            if (m_CurrentVisitors.Count >= Data.ReceptionCount)
            {
                return false;
            }

            /*if (Data.ID == 2)
            {
                Debug.Log("2");
            }*/


            m_CurrentVisitors.Add(visitor);
            StartCoroutine(InteractionCoroutine(visitor));
            return true;
        }

        private IEnumerator InteractionCoroutine(VisitorController visitor)
        {
            yield return new WaitForSeconds(Data.StayTime);

            if (visitor != null && m_CurrentVisitors.Contains(visitor))
            {
                ValueManager.Instance.AddMoney(Data.MoneyBenefit);
                visitor.OnInteractionComplete();
                visitor.FeelHappy(Data.HappinessBenefit);

                m_CurrentVisitors.Remove(visitor);
            }
        }
    }
}