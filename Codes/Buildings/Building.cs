using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Jobs;
using UnityEngine;

namespace UnitsAndBuildings
{

    public abstract class Building : MonoBehaviour, IDamageable,IJob
    {
        static protected NavMeshSurface m_navMeshSurface;
        protected float m_health = 100f;
        protected int m_workersNeeded;
        protected int m_numberOfWorkers;
        public int m_numOfAttackingSelf;
        //Initial value is true because it is already in the queue of buildings that need workers
        protected bool m_isAllWorkersAssigned = true;

        //Any building must define the number of workers it need in its awake method.
        protected abstract void Awake();
   
        protected virtual void Start()
        {
            if(m_navMeshSurface == null)
            {
                Debug.Log("Getting navmesh");
                m_navMeshSurface = GameObject.Find("Ground").GetComponentInChildren<Unity.AI.Navigation.NavMeshSurface>();
            }
        }
        //protected virtual void Destruction()
        //{
        //    Destroy(gameObject);
        //}

        protected virtual void Update()
        {
           if(!m_isAllWorkersAssigned)
            {
                PeasantsManager.Instance.AddToBuildingsThatNeedWorkers(gameObject);
            }
        }
        //This function is essential in managing the workforce allocation of the buildings.
        public void AssignWorker()
        {
            m_numberOfWorkers++;
        }
        //In case the assignWorker should return a reference of the worker besides incrementing number of workers.
        public virtual void AssignWorker(GameObject worker)
        {
            m_numberOfWorkers++;
        }
        //This function handles the changes that are necessary when a worker of a builgins is died.
        public virtual void RemoveWorker()
        {
            m_numberOfWorkers--;
            m_isAllWorkersAssigned = false;
        }

        public bool IsBuildingHaveEnoughWorkers()
        {
            if (m_numberOfWorkers < m_workersNeeded)
            {
                return false;
            }
            else
            {
                m_isAllWorkersAssigned = true;
                return true;
            }
        }

        //This method returns the amount of resources that are needed to build the building.
        public abstract Dictionary<ResourceManagement.Items, int> GetBuildingResourceRequirements();
        //This method returns true if the building is free to build and false if it is not.
        public abstract bool IsBuldingFreeToBuild();

        //This method is used to handle the damaging process of all buildings.
        bool IDamageable.TakeDamage(int damage, int piercingDamage = 0)
        {
            m_health -= damage;
            if (m_health <= 0)
            {
               
                ((IDamageable)this).Die();
                return true;
            }
            return false;
        }
        public virtual void OnDestroy()
        {
            if (m_navMeshSurface)
            {
                //After a building is destoryed, the navmesh should be updated.
                //In order to not stop the main game thread, we use a job system to update the navmesh.
                //Debug.Log("Building is destroyed");
                BuildingDestruction.Instance.PlayDestructionSoundAtPosition(transform.position);
                //((IJob)this).Execute();
            }
        }
        //This method handles all the behaviors that should be done when a building is destroyed.
        void IDamageable.Die()
        {

            Destroy(gameObject);
        }

        void IJob.Execute()
        {
            m_navMeshSurface.BuildNavMesh();
        }
    }
}