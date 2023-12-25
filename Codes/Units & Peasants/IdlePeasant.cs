using UnityEngine;

namespace Peasants
{

    public class IdlePeasant : Peasants.Peasant

    {
        //The reference to the campfire so peasants can face
        static private GameObject m_campfire;
        protected override void Awake()
        {
            base.Awake();
            PeasantsManager.Instance.IdlePeasantCreated();
            m_campfire = GameObject.FindGameObjectWithTag("Campfire");
        }
        protected override void Start()
        {
            base.Start();
            m_peasantState = PeasantState.Idle;
        }
        private void OnDestroy()
        {
            PeasantsManager.Instance.IdlePeasantKilled();
        }
        protected override void DecisionMaker()
        {
            //An idle peasent will do nothing but wait for a new task
            if(m_navMeshAgent.velocity.magnitude < 0.1f)
            {
                //now the idle peasants should face the campfire
                gameObject.transform.LookAt(m_campfire.transform);

            }
            return;
        }

    }
}
