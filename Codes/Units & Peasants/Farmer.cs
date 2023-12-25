using UnityEngine;

namespace Peasants
{
    public class Farmer : Peasants.Peasant
    {
        // A reference to the farm that the farmer is working at
        private float m_minX;
        private float m_maxX;
        private float m_minZ;
        private float m_maxZ;

        private Vector3 m_navmeshDestination;

        protected override void Awake()
        {
            base.Awake();
            //PeasantsManager.Instance.IdlePeasantCreated();
        }
        protected override void Start()
        {
            base.Start();
            m_animator.SetBool("farming", false);
            m_workingTime = 12f;
            PeasantsManager.Instance.WorkerPeasantCreated();
            //Assign the farmer to the farm
            occupation.GetComponent<UnitsAndBuildings.Building>().AssignWorker();
            DefineWorkArea();
        }

        protected override void Update()
        {
            base.Update();

            if (Vector3.Distance(transform.position, m_navmeshDestination) < 4f)
                {
                    m_animator.SetBool("farming", true);
                }
                else
                {
                    m_animator.SetBool("farming", false);
                }

        }

        protected override void DecisionMaker()
        {
            if (occupation == null)
            {
                //if occupation of the peasent is null, then the peasent must switch to idle state.
                PeasantsManager.Instance.ConvertToIdlePeasant(gameObject);
            }
            if (Time.time - m_timeOfLastStateChange >= m_workingTime)
            {
                m_navmeshDestination = new Vector3(Random.Range(m_minX, m_maxX), 0, Random.Range(m_minZ, m_maxZ));
                Move(m_navmeshDestination);
                Debug.Log(Vector3.Distance(transform.position, m_navmeshDestination) < 10f);
                m_timeOfLastStateChange = Time.time;
            }
        }

        private void OnDestroy()
        {
            if (occupation != null)
            {
                occupation.GetComponent<UnitsAndBuildings.Building>().RemoveWorker();

            }
            PeasantsManager.Instance.IdlePeasantKilled();
        }

        //This method defines the area of the farm that the farmer will work at
        private void DefineWorkArea()
        {
            Vector3[] farmCorners = new Vector3[4];
            if (occupation != null)
            {
                //Debug.Log("Occupation is not null");
                for (int i = 0; i < 4; i++)
                {
                    //Debug.Log("WalkingPoint" + i);
                    farmCorners[i] = occupation.transform.Find("WalkingPoint" + i).gameObject.transform.position;
                }
            }
            //Find the minimum and maximum x and z values
            m_minX = farmCorners[0].x;
            m_maxX = farmCorners[0].x;
            m_minZ = farmCorners[0].z;
            m_maxZ = farmCorners[0].z;
            for (int i = 1; i < 4; i++)
            {
                if (farmCorners[i].x < m_minX)
                {
                    m_minX = farmCorners[i].x;
                }
                if (farmCorners[i].x > m_maxX)
                {
                    m_maxX = farmCorners[i].x;
                }
                if (farmCorners[i].z < m_minZ)
                {
                    m_minZ = farmCorners[i].z;
                }
                if (farmCorners[i].z > m_maxZ)
                {
                    m_maxZ = farmCorners[i].z;
                }
            }
        }
    }
}