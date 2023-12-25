using UnityEngine;
using UnityEngine.AI;

namespace Peasants
{
    public enum PeasantState : short
    {
        Idle,
        Working,
        StoringResources,
        ReturnToWork,
        GoToResource,
        ResourceGathering,

    }
    

    public abstract class Peasant : MonoBehaviour, IDamageable
    {
        public int m_numOfAttackingSelf;

        protected float m_hitPoint = 15f;

        //The following static variable is used to assign the occupation to a peasant
        public static GameObject nextPeasantOccupation;

        protected NavMeshAgent m_navMeshAgent;
        protected PeasantState m_peasantState;

        protected float m_idleTime;
        protected float m_workingTime;
        //protected float m_storingTime;

        protected float m_timeOfLastStateChange;

        //this boolean flag is used to check whether the peasant make a decision recently or not
        protected bool m_isStatusSet = false;

        public GameObject occupation { protected set; get; }
        protected GameObject m_storage;
        protected Vector3 m_resourceCollectionPoint;
        //The following variable is used to store the previous state of the peasant, when its storage is destroyed its remembers its previous state and returns to it after the storage is rebuilt
        protected PeasantState m_previousState;
        protected bool m_isAddedToListOfFindingNewStorage = false;
        protected IObserver m_observerReference;
        protected Animator m_animator;

        protected virtual void SwitchWeaponOn(GameObject gameOb)
        {
            gameOb.SetActive(!gameOb.activeSelf);
        }

        protected virtual void Awake()
        {
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            m_animator = GetComponent<Animator>();
            occupation = nextPeasantOccupation;
            m_navMeshAgent.speed = 8;
        }
        protected virtual void Start()
        {
            m_peasantState = PeasantState.ReturnToWork;
        }

        protected virtual void Update()
        {
            DecisionMaker();



            if (m_navMeshAgent.velocity.magnitude >= 1) {
                m_animator.SetBool("moving", true);
            }
            else 
            {
                m_animator.SetBool("moving", false);
            }
        }
        protected abstract void DecisionMaker();
        
        //protected void ChangeState(PeasantState newState)
        //{
        //    m_peasantState = newState;
        //}


            //else if(m_peasantState == PeasantState.StoringResources)
            //{
            //    //Move to the storage after gathering or producing resources
            //    Move(m_storage.transform);
            //    if (Vector3.Distance(transform.position, m_storage.transform.position) <= 1)
            //    {
            //        m_peasantState = PeasantState.ReturnToWork;
            //    }
            //}
        
        //This method is used to assign an occupation to a peasent
        public void DefineOccupation(GameObject occupation)
        {
            this.occupation = occupation;
        }

        //The following methods are used to move the peasent to a destination
        public void Move(Transform destination)
        {
            m_navMeshAgent.SetDestination(destination.position);
        }
        public void Move(Vector3 destination)
        {
            m_navMeshAgent.SetDestination(destination);
        }
        //This method is used to handle the damaging process of all peasants
        bool IDamageable.TakeDamage(int damage, int piercingDamage = 0)
        {

            m_hitPoint -= damage;
            if(m_hitPoint <= 0)
            {
                Debug.Log("Unit dead");
                ((IDamageable)this).Die();
                return true;
            }
            return false;
        }

        //This method handles all the behaviors that should be done when a unit is died.
        void IDamageable.Die()
        {
            Destroy(gameObject);
        }

    }

}
