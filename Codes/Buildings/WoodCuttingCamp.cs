using System.Collections.Generic;
using UnityEngine;


namespace UnitsAndBuildings
{

    public class WoodCuttingCamp : UnitsAndBuildings.Building
    {
        //The dictionary stores the cost of the building
        private static System.Collections.Generic.Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new System.Collections.Generic.Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 20 }
            };

        private Vector3 m_woodChopingSite;

        protected override void Awake()
        {
            m_workersNeeded = 1;
            m_woodChopingSite = transform.Find("WoodChopingSite").gameObject.transform.position;
        }

        protected override void Start()
        {
            base.Start();
            m_health = 100;
            if (m_workersNeeded > 0)
            {
                Debug.Log("WoodCuttingCamp is added to the list of buildings that need workers");
                PeasantsManager.Instance.AddToBuildingsThatNeedWorkers(gameObject);
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void AssignWorker(GameObject newLumberJack)
        {
            newLumberJack.GetComponent<Peasants.LumberJack>().DefineWoodChoppingSite(m_woodChopingSite);
            m_numberOfWorkers++;
        }

        public override bool IsBuldingFreeToBuild()
        {
            return false;
        }
        public override Dictionary<ResourceManagement.Items, int> GetBuildingResourceRequirements()
        {
            return m_constructionCost;
        }
    }
}