using UnityEngine;

namespace Peasants
{
    public class Baker : Peasants.Peasant, IObservable
    {
        private const int RESOURCECARRYAMOUNT = 10;
        private IObserver m_observer;
        private Vector3 m_ovenPosition;
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
            m_workingTime = 3;
            PeasantsManager.Instance.WorkerPeasantCreated();
            //Assing the baker to an oven in a bakery
            occupation.GetComponent<UnitsAndBuildings.Building>().AssignWorker(gameObject);
            //Assign the closet storage at the time of build to the baker to store its good there,
            ResourceManagement.InventoryCMS.Instance.RegisterPeasantInAppropriateStorage(this);
            ResourceManagement.InventoryCMS.Instance.Bakers.Add(this);
        }


        protected override void Update()
        {
            base.Update();
        }

        protected void OnDestroy()
        {
            //Informing the PeasantsManager 
            PeasantsManager.Instance.WorkerPeasantKilled();
            //Informing the occupation that it losts one of its workers
            if (occupation != null)
            {
                occupation.GetComponent<UnitsAndBuildings.Building>().RemoveWorker();
                PeasantsManager.Instance.IdlePeasantKilled();
            }
        }
        //As this type of peasents have more complex behaviour,  this method is acting like a state machine
        protected override void DecisionMaker()
        {
            if (occupation == null)
            {
                //if occupation of the peasent is null, then the peasent must switch to idle state.
                PeasantsManager.Instance.ConvertToIdlePeasant(gameObject);
            }

            if(m_storage == null)
            {
                //First we should reset the animation status to idle

                m_isStatusSet = false;
                if ((transform.position - m_ovenPosition).magnitude<= 9)
                {
                    m_navMeshAgent.ResetPath();
                }
                else if(m_navMeshAgent.destination != m_ovenPosition)
                {
                    m_animator.SetBool("working", false);
                    Move(m_ovenPosition);
                }
                ResourceManagement.InventoryCMS.Instance.RegisterPeasantInAppropriateStorage(this);
                return;
            }

            if (m_peasantState == PeasantState.ReturnToWork)
            {
                if (!m_isStatusSet)
                {           
                   // Debug.Log("Return to work");
                    Move(m_ovenPosition);
                    m_isStatusSet = true;
                }

                if ((transform.position - m_ovenPosition).sqrMagnitude <= 9)
                {
                    m_peasantState = PeasantState.Working;
                    m_animator.SetBool("working", true);
                    m_timeOfLastStateChange = Time.time;
                    m_isStatusSet = false;
                }
            }

            else if (m_peasantState == PeasantState.Working)
            {
                //Do the work
                if (Time.time - m_timeOfLastStateChange >= m_workingTime)
                {

                    m_peasantState = PeasantState.StoringResources;
                    m_timeOfLastStateChange = Time.time;
                }
            }

            else if (m_peasantState == PeasantState.StoringResources)
            {
                //Move to the storage after gathering or producing resources
                if(!m_isStatusSet)
                {
                    m_animator.SetBool("working", false);
                    Move(m_resourceCollectionPoint);
                    m_isStatusSet = true;
                }

                if (Vector3.Distance(transform.position, m_resourceCollectionPoint) <= 3)
                {
                    //Updating the amount of bread in the granary
                    ((IObservable)this).NotifyObserver();
                    m_peasantState = PeasantState.ReturnToWork;
                    m_isStatusSet = false;
                }
            }
        }


        //This method defines the bakingSpot Transform object that should be use for guiding the baker to that point
        public void DefineOvenPosition(Vector3 ovenPosition)
        {
            m_ovenPosition = ovenPosition;
        }
        //Implementation of the Interface methods.
        void IObservable.AddObserver(GameObject storage)
        {
            m_storage = storage;
            m_resourceCollectionPoint = storage.transform.Find("resourceCollectionPoint").transform.position;
            m_observer = storage.GetComponent<IObserver>();
        }
        //void IObservable.RemoveObserver()
        //{

        //}
        void IObservable.NotifyObserver()
        {
            //The following code is used in order to add the resource to the inventory
            m_observer.OnStorageChanged(ResourceManagement.Items.Bread, RESOURCECARRYAMOUNT);
        }

    }
}