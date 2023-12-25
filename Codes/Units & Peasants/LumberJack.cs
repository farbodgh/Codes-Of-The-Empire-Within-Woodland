using UnityEngine;


namespace Peasants
{
    public class LumberJack : Peasants.Peasant, IObservable
    {
        [SerializeField]
        private GameObject m_NearestTree;
        private Vector3 m_nearestTree;
        private Vector3 m_woodChoppingSite;
        private const int RESOURCECARRYAMOUNT = 10;
        private IObserver m_observer;

        public GameObject axe;

        protected override void Awake()
        {
            base.Awake();

        }

        protected override void Start()
        {
            base.Start();
            m_peasantState = Peasants.PeasantState.GoToResource;
            m_workingTime = 7;

            PeasantsManager.Instance.WorkerPeasantCreated();
            // Assign the lumberjack to a tree in a lumberjack hut
            occupation.GetComponent<UnitsAndBuildings.Building>().AssignWorker(gameObject);
            // Assign the closest storage at the time of build to the lumberjack to store goods there
            ResourceManagement.InventoryCMS.Instance.RegisterPeasantInAppropriateStorage(this);
        }

        protected override void Update()
        {
            base.Update();
            // Additional update logic if needed
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

        protected override void DecisionMaker()
        {
            //if occupation of the peasent is null, then the peasent must switch to idle state.
            if (occupation == null)
            {
                PeasantsManager.Instance.ConvertToIdlePeasant(gameObject);
            }

            if(m_storage == null)
            {
                m_isStatusSet = false;
                if (m_navMeshAgent.destination != m_woodChoppingSite)
                {
                    m_animator.SetBool("chopping", false);
                    Move(m_woodChoppingSite);
                }
                ResourceManagement.InventoryCMS.Instance.RegisterPeasantInAppropriateStorage(this);
                return;
            }

            //If the nearest tree is not set, then the peasant must find the nearest tree
            if (m_nearestTree == Vector3.zero)
            {
                FindTheNearestTree();
            }

            //First the peasant must go to the nearest tree to chop it
            if(m_peasantState == Peasants.PeasantState.GoToResource)
            {
                if (!m_isStatusSet)
                {
                    Move(m_nearestTree);
                    m_isStatusSet = true;
                }

                if (Vector3.Distance(transform.position, m_nearestTree) <= 7f)
                {
                    m_peasantState = Peasants.PeasantState.ResourceGathering;
                    axe.SetActive(true);
                    m_animator.SetBool("chopping", true);
                    m_timeOfLastStateChange = Time.time;
                    m_isStatusSet = false;
                }
                
            }
            //The peasant work on the tree in order to bring some logs of it to the wood chopping site
            else if (m_peasantState == Peasants.PeasantState.ResourceGathering)
            { 
                
                if(Time.time - m_timeOfLastStateChange >= m_workingTime)
                {
                    m_peasantState = Peasants.PeasantState.ReturnToWork;
                    m_timeOfLastStateChange = Time.time;
                }
            }

            //Afterwards, the peasant must return to the wood chopping site
             if (m_peasantState == Peasants.PeasantState.ReturnToWork)
            {
                m_animator.SetBool("chopping", false);
                axe.SetActive(false);
                if (!m_isStatusSet)
                {
                    Move(m_woodChoppingSite);
                    m_isStatusSet = true;
                }

                if (Vector3.Distance(transform.position, m_woodChoppingSite) <= 2f)
                {
                    axe.SetActive(true);
                    m_peasantState = Peasants.PeasantState.Working;
                    m_animator.SetBool("chopping", true);
                    m_timeOfLastStateChange = Time.time;
                    m_isStatusSet = false;
                }
            }

            //Then, the peasant should process the logs into processed woods
            else if (m_peasantState == Peasants.PeasantState.Working)
            {
                
                if (Time.time - m_timeOfLastStateChange >= m_workingTime)
                {
                    axe.SetActive(false);
                    m_peasantState = Peasants.PeasantState.StoringResources;
                    m_timeOfLastStateChange = Time.time;
                }
            }

             //Finally, the peasant should store the processed woods in the storage
            else if (m_peasantState == Peasants.PeasantState.StoringResources)
            {
                if (!m_isStatusSet)
                {
                    m_animator.SetBool("chopping", false);
                    Move(m_resourceCollectionPoint);
                    m_isStatusSet = true;
                }

                if (Vector3.Distance(transform.position, m_resourceCollectionPoint) <= 3)
                {
                    ((IObservable)this).NotifyObserver();
                    m_peasantState = Peasants.PeasantState.GoToResource;
                    m_isStatusSet = false;
                }
            }
        }


        private void FindTheNearestTree()
        {
            //Find the nearest tree to the lumberjack
            //based on the 300 closest trees to the player's keep.
            //the closest one to the lumberjack is chosen.
            m_NearestTree = GameManager.Instance.FindClosestTreeToPoint(m_woodChoppingSite);  
            m_nearestTree = m_NearestTree.transform.position;
        }

        public void DefineWoodChoppingSite(Vector3 woodChoppingSite)
        {       
            m_woodChoppingSite = woodChoppingSite;
        }

        void IObservable.AddObserver(GameObject storage)
        {
            m_storage = storage;
            m_resourceCollectionPoint = storage.transform.Find("resourceCollectionPoint").transform.position;
            m_observer = storage.GetComponent<IObserver>();
        }

        void IObservable.NotifyObserver()
        {
            m_observer.OnStorageChanged(ResourceManagement.Items.Wood, RESOURCECARRYAMOUNT);
        }


    }
}