using System.Collections.Generic;
using UnityEngine;

namespace UnitsAndBuildings
{
    public class Bakery : UnitsAndBuildings.Building
    {



        //We should assign one baker to each baking spot
        //The followign array stores the location of the spots
        private Vector3[] m_ovenPositions = new Vector3[6];
        //The following array stores information of the bakers, if any of them die this array helps to define
        //A new baker to an oven which does not have a baker.
        private GameObject[] m_bakers = new GameObject[6];


        protected override void Awake()
        {
            m_workersNeeded = 6;
            for (int i = 0; i < m_ovenPositions.Length; i++)
            {
                Debug.Log("BakerySpot" + i);
                m_ovenPositions[i] = transform.Find("BakerySpot" + i).gameObject.transform.position;
            }
        }


        protected override void Start()
        {
            base.Start();
            m_health = 1;
            if (m_workersNeeded > 0)
            {
                Debug.Log("Bakery is added to the list of buildings that need workers");
                PeasantsManager.Instance.AddToBuildingsThatNeedWorkers(gameObject);
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        //This method assigns a baker to the first empty oven in the bakery
        public override void AssignWorker(GameObject newBaker)
        {

            for (int i = 0; i < m_bakers.Length; i++)
            {
                if (m_bakers[i] == null)
                {
                    m_bakers[i] = newBaker;
                    newBaker.GetComponent<Peasants.Baker>().DefineOvenPosition(m_ovenPositions[i]);
                    m_numberOfWorkers++;
                    break;
                }
            }
        }

        public override bool IsBuldingFreeToBuild()
        {
            return true;
        }

        public override Dictionary<ResourceManagement.Items, int> GetBuildingResourceRequirements()
        {
            return null;
        }
    }
}