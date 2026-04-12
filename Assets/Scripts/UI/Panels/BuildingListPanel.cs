#region

using Core.Data;
using UnityEngine;

#endregion

namespace UI
{
    public class BuildingListPanel : BasePanel<BuildingListData>
    {
        public Transform Content;

        protected override void OnBind()
        {
            Data.BuildingCards.BindAdd(AddBuilding);

            foreach (Transform child in Content)
            {
                Destroy(child.gameObject);
            }
        }

        protected override void OnUnbind()
        {
            Data.BuildingCards.UnbindAdd(AddBuilding);
        }

        public void AddBuilding(int _, BuildingCardData data)
        {
            var buildingPanel = UIManager.Instance.CreatePanel<BuildingPanel>();
            if (buildingPanel != null)
            {
                buildingPanel.Bind(data);
                buildingPanel.transform.SetParent(Content, false);
            }
        }
    }
}