using UnityEngine;

namespace UnitsAndBuildings
{
    public class King : UnitsAndBuildings.Soldier
    {
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            m_hitPoint = 400;
            m_damage = 30;
            m_navMeshAgent.speed = 8;
            m_audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        protected override void Update()
        {
            float speed = GetComponent<UnityEngine.AI.NavMeshAgent>().velocity.magnitude;
                if (speed >= 1) {
                m_animator.SetBool("moving", true);
            }
            else 
            {
                m_animator.SetBool("moving", false);
            }

            switch (m_currentState)
            {
                case m_soldierStates.noCommand:
                    // Waiting for command - either to attack or move
                    m_navMeshAgent.ResetPath();
                    Waiting();
                    break;
                case m_soldierStates.readyToFight:
                    //m_animator.SetTrigger("unsheath");
                    Invoke("switchWeapon", 1f);
                    m_currentState = m_soldierStates.fighting;
                    break;

                case m_soldierStates.fighting:
                    FightingTarget();
                    if (!m_currentTarget)
                    {
                        m_currentState = m_soldierStates.noCommand;
                    }
                    break;
                case m_soldierStates.commandedMovement:
                    
                    if (Vector3.Distance(transform.position, m_navMeshAgent.destination) > 3f)
                    {
                        m_currentTarget = null;
                    }
                    else
                    {
                        m_currentState =  m_soldierStates.noCommand;
                    }
                    break;
            }
        }

        protected virtual void Waiting()
        {
            if (!m_currentTarget)
            {
                Debug.Log("");
            }
            else
            {
                m_currentState = m_soldierStates.readyToFight;
            }
        }

         protected override void FightingTarget()
        {
            if (m_currentTarget)
            {

                if(Vector3.Distance(transform.position, m_currentTarget.transform.position) > 5f)
                {
                    Vector3 moveTo = m_currentTarget.transform.position;
                    Move(m_currentTarget.transform.position);
                }
                else
                {
                    m_navMeshAgent.ResetPath();
                    if (m_currentTarget.TryGetComponent(out IDamageable damageableObject))
                        { 
                            if (Time.time > (m_lastAttackedAt + m_attackCooldown))
                            {
                                if(damageableObject.TakeDamage(0))
                            {
                                m_currentTarget = null;
                                m_numOfAttackingSelf--;
                                m_currentState = m_soldierStates.noCommand;
                                return;
                            }
                                Vector3 newDirection = m_currentTarget.transform.position - transform.position;
                                transform.rotation = Quaternion.LookRotation(newDirection);

                                m_animator.SetTrigger("slashing");
                                damageableObject.TakeDamage(m_damage);
                                
                                m_lastAttackedAt = Time.time;
                            }
                        }
                }
            }
            else { m_numOfAttackingSelf-- ; m_currentState = m_soldierStates.noCommand;  }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        } 
        
        
    }
}