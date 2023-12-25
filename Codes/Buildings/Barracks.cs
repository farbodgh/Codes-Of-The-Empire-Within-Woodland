using ResourceManagement;
using System.Collections.Generic;
using UnityEngine;

namespace UnitsAndBuildings
{
    public class Barracks : UnitsAndBuildings.Building
    {
        private static Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 100 },
                { ResourceManagement.Items.Bread, 100 }
            };



        [SerializeField]
        private GameObject m_SwordsmanPrefab;
        [SerializeField]
        private GameObject m_SoldierSpawnPoint;

        private static Dictionary<ResourceManagement.Items, int> m_soldierCost = new Dictionary<ResourceManagement.Items, int>()
        {
                { ResourceManagement.Items.Weapon, 1 },
                { ResourceManagement.Items.Bread, 50 }
         };

        protected override void Awake()
        {
            m_workersNeeded = 0;
            GameManager.Instance.BarracksBuilt(gameObject);
        }

        public override void OnDestroy()
        {
            GameManager.Instance.BarracksDestroyed();
            base.OnDestroy();
        }

        protected override void Start()
        {
            m_health = 500;
        }

        public override bool IsBuldingFreeToBuild()
        {
            return false;
        }
        public override Dictionary<ResourceManagement.Items, int> GetBuildingResourceRequirements()
        {
            return m_constructionCost;
        }

        public void TrainSoldier()
        {

            if (PeasantsManager.Instance.IsIdlePeasantAvailable())
            {
                bool isEnoughResources = true;
                foreach (var resource in m_soldierCost)
                {
                    isEnoughResources = isEnoughResources & InventoryCMS.Instance.IsResourceAmountSufficient(resource.Key, resource.Value);
                }

                if (isEnoughResources)
                {
                    foreach (var resource in m_soldierCost)
                    {
                        InventoryCMS.Instance.DeductResource(resource.Key, resource.Value);
                    }
                    PeasantsManager.Instance.RemoveOneIdlePeasant();
                    Instantiate(m_SwordsmanPrefab, m_SoldierSpawnPoint.transform.position, m_SoldierSpawnPoint.transform.rotation);
                }
            }
        }

    }
}
