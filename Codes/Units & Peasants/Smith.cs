using UnityEngine;

namespace Peasants
{
    public class Smith

        : Peasants.Peasant, IObservable 
    {
        private const int RESOURCECARRYAMOUNT = 1; 
        private IObserver m_observer;
        private Vector3 m_anvilPosition;

        public GameObject sword;
        public GameObject hammer;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            m_workingTime = 10;

            hammer.SetActive(false);
            sword.SetActive(false);

            PeasantsManager.Instance.WorkerPeasantCreated();
            // Assign the smith to an anvil in a smithery workshop
            occupation.GetComponent<UnitsAndBuildings.Building>().AssignWorker(gameObject);
            // Assign the closest storage at the time of build to the smith to store goods there
            ResourceManagement.InventoryCMS.Instance.RegisterPeasantInAppropriateStorage(this);
            ResourceManagement.InventoryCMS.Instance.Smiths.Add(this);
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

        protected virtual void setHammer()
        {
            hammer.SetActive(!hammer.activeSelf);
        }

        protected override void DecisionMaker()
        {
            if (occupation == null)
            {
                //if occupation of the peasant is null, then the peasant must switch to idle state.
                PeasantsManager.Instance.ConvertToIdlePeasant(gameObject);
            }
           
            if(m_storage == null)
            {
                m_isStatusSet = false;
                if (m_navMeshAgent.destination != m_anvilPosition)
                {
                    m_animator.SetBool("working", false);
                    Move(m_anvilPosition);
                }
                ResourceManagement.InventoryCMS.Instance.RegisterPeasantInAppropriateStorage(this);
                return;
            }

            if(m_peasantState == PeasantState.ReturnToWork)
            {
                
                if (!m_isStatusSet)
                {
                    Debug.Log("Return to work");
                    Debug.Log($"m_anvilPosition : {m_anvilPosition}");
                    Move(m_anvilPosition);
                    m_isStatusSet = true;
                }

                //if the peasant is returning to work, then it must check if it is close enough to the forge
                if (Vector3.Distance(transform.position, m_anvilPosition) <= 3)
                {
                    //if the peasant is close enough to the forge, then it must start working
                    m_peasantState = PeasantState.Working;
                    m_timeOfLastStateChange = Time.time;
                    Invoke("setHammer", 2f);
                    m_animator.SetBool("hammering", true);
                    m_isStatusSet = false;
                }
            }

            else if(m_peasantState == PeasantState.Working)
            {
                //if the peasant is working, then it must check if it has finished working
                if (Time.time - m_timeOfLastStateChange >= m_workingTime)
                {
                    //if the peasant has finished working, then it must return to work
                    m_peasantState = PeasantState.StoringResources;
                    hammer.SetActive(false);
                    sword.SetActive(true);
                    m_timeOfLastStateChange = Time.time;

                }
                m_animator.SetBool("hammering", true);
            }
            
            else if(m_peasantState == PeasantState.StoringResources)
            {

                m_animator.SetBool("hammering", false);
                if (!m_isStatusSet)
                {
                    //Debug.Log("Storing resources");
                    Move(m_resourceCollectionPoint);
                    m_isStatusSet = true;
                }

                //if the peasant is storing resources, then it must check if it is close enough to the storage
                if (Vector3.Distance(transform.position, m_resourceCollectionPoint) <= 3)
                {
                    //if the peasant is close enough to the storage, then it must store the resources
                    //Notify the storage that the resource is ready to be stored
                    ((IObservable)this).NotifyObserver();
                    sword.SetActive(false);
                    m_peasantState = PeasantState.ReturnToWork;
                    m_isStatusSet = false;
                }
            }

        }

        public void DefineForgePosition(Vector3 anvilPosition)
        {
            Debug.Log("DefineForgePosition: anvilPosition is " + anvilPosition);
            m_anvilPosition = anvilPosition;
        }

        void IObservable.AddObserver(GameObject storage)
        {
            m_storage = storage;
            m_resourceCollectionPoint = storage.transform.Find("resourceCollectionPoint").transform.position;
            m_observer = storage.GetComponent<IObserver>();
        }

        void IObservable.NotifyObserver()
        {
            m_observer.OnStorageChanged(ResourceManagement.Items.Weapon, RESOURCECARRYAMOUNT);
        }
    }
}