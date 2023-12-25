using System.Collections.Generic;
using UnityEngine;

namespace UnitsAndBuildings
{
    public class Smithery : UnitsAndBuildings.Building
    {
        //The dictionary stores the cost of the building
        private static System.Collections.Generic.Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new System.Collections.Generic.Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 80 }
            };

        private Vector3 m_forge;
        // Start is called before the first frame update

        protected override void Awake()
        {
            m_workersNeeded = 1;
            m_forge = transform.Find("AnvilWorkingPosition").gameObject.transform.position;
        }
        protected override void Start()
        {
            base.Start();
            m_health = 200;
            if (m_workersNeeded > 0)
            {
                PeasantsManager.Instance.AddToBuildingsThatNeedWorkers(gameObject);
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public override void AssignWorker(GameObject newSmith)
        {
            m_numberOfWorkers++;
            newSmith.GetComponent<Peasants.Smith>().DefineForgePosition(m_forge);
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