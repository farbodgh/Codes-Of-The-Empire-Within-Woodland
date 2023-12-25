using System.Collections.Generic;
using UnityEngine;

namespace UnitsAndBuildings
{

    public class Farm : UnitsAndBuildings.Building
    {
        //The following dictionary define the cost of the building.
        private static Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 200 }
            };

        protected override void Awake()
        {
            m_workersNeeded = 6;
        }

        //private GameObject[] m_peasent = new GameObject[10];


        protected override void Update()
        {
            base.Update();
        }
        protected override void Start()
        {
            m_health = 350;
            //Each building that needs workers must register itself into Peasant manager, thus allowing the game manager to assign peasents to work at the building
            if (m_workersNeeded > 0)
                PeasantsManager.Instance.AddToBuildingsThatNeedWorkers(gameObject);
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