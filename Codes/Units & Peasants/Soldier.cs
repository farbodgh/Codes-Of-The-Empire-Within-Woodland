using UnityEngine;
using UnityEngine.AI;

namespace UnitsAndBuildings
{
    public abstract class Soldier : UnitsAndBuildings.Unit
    {
        public GameObject sheathedSword;
        public GameObject fightingSword;

        protected float m_lastAttackedAt;
        protected bool m_commanded = false;
        //protected Vector3 currentMove;

        protected float m_armor;
        protected SpriteRenderer m_selectedCircle;

        protected Ray m_rayStraight;
        protected Ray m_rayRight;
        protected Ray m_rayLeft;
        protected Ray m_rayBehind;
        protected Ray m_rayBehind1;
        protected Ray m_rayBehind2;
        protected Ray m_rayRight1;
        protected Ray m_rayLeft1;

        protected m_soldierStates m_currentState;

        protected float m_rayCastModification = 45;
        protected float m_rayCastModificationSpeed = 360;

        protected bool m_isSheathed = true;
        protected float m_timeOfBeingUnsheated = 5;
        protected float m_timeSinceLastFight;

        private bool m_isMoving = false;
        protected enum m_soldierStates
        {
            noCommand,
            commandedMovement,
            fighting,
            readyToFight,
        }
        protected bool EnemySetByPlayer = false;


        protected virtual void Start()
        {
            m_animator = GetComponent<Animator>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();

            m_selectedCircle = GetComponentInChildren<SpriteRenderer>();
            m_selectedCircle.enabled = false;

            m_currentState = m_soldierStates.noCommand;
            m_lastAttackedAt = Time.time;

            sheathedSword.SetActive(true);
            fightingSword.SetActive(false);
            UnitSelection.Instance.allUnits.Add(this);
        }
        public virtual void PassCommand(Vector3 hitPoint)
        {
            //currentMove = hitPoint;
            m_currentState = m_soldierStates.commandedMovement;
            m_currentTarget = null;
        }
        public virtual void PassTarget(UnitsAndBuildings.Unit enemy)
        {
            m_currentTarget = enemy;
            m_currentState = m_soldierStates.readyToFight;
        }

        protected virtual void Update()
        {
            Debug.Log($"Soldier:: status is: {m_currentState}");
            if (m_navMeshAgent.velocity.magnitude > 0)
            {
                m_animator.SetBool("moving", true);
            }
            else
            {
                m_animator.SetBool("moving", false);
            }

            switch (m_currentState)
            {
                case m_soldierStates.noCommand:
                    //Targetting and attack on sight;
                    if (Time.time > m_timeSinceLastFight + m_timeOfBeingUnsheated && !m_isSheathed)
                    {
                        m_isSheathed = true;
                        m_animator.SetTrigger("sheath");
                        Invoke("switchWeapon", 1f);
                    }
                    m_isMoving = false;
                    m_navMeshAgent.ResetPath();
                    Targeting();
                    break;
                case m_soldierStates.readyToFight:
                    //if the sowrd is unsheathed unsheath sword and attack the target
                    if (m_isSheathed)
                    {
                        m_isSheathed = false;
                        m_animator.SetTrigger("unsheath");
                        Invoke("switchWeapon", 1f);
                    }
                    m_currentState = m_soldierStates.fighting;
                    break;

                case m_soldierStates.fighting:
                    //Given target and attack that target
                    FightingTarget();
                    if (!m_currentTarget)
                    {
                        m_currentState = m_soldierStates.noCommand;
                    }
                    break;
                case m_soldierStates.commandedMovement:

                    if (Vector3.Distance(transform.position, m_navMeshAgent.destination) <= 3f || (m_navMeshAgent.velocity.magnitude <= .1f && m_isMoving))
                    {
                        if (m_navMeshAgent.velocity.magnitude >= .3f)
                        {
                            m_isMoving = true;
                        }
                        m_currentState = m_soldierStates.noCommand;
                    }
                    break;

            }
        }


        protected virtual void Targeting()
        {
            RaycastHit hitResult;
            Vector3 rayPosition = transform.position + new Vector3(0, 1, 0);
            m_rayCastModification += m_rayCastModificationSpeed * Time.deltaTime;
            m_rayRight = new Ray(rayPosition, Quaternion.AngleAxis(-45 + m_rayCastModification, Vector3.up) * transform.forward);

            m_rayLeft = new Ray(rayPosition, Quaternion.AngleAxis(45 + m_rayCastModification, Vector3.up) * transform.forward);

            m_rayBehind = new Ray(rayPosition, Quaternion.AngleAxis(180 + m_rayCastModification, Vector3.up) * transform.forward);



            if (Physics.Raycast(m_rayRight, out hitResult, 50, 1 << 12) || Physics.Raycast(m_rayLeft, out hitResult, 50, 1 << 12)
            || Physics.Raycast(m_rayBehind, out hitResult, 15f, 1 << 12))
            {

                m_currentTarget = hitResult.collider.GetComponent<UnitsAndBuildings.Enemy>();

                if (m_currentTarget && m_currentTarget.m_numOfAttackingSelf < 6)
                {
                    m_currentTarget.m_numOfAttackingSelf += 1;
                    m_currentState = m_soldierStates.readyToFight;
                }
            }
            else
            {

                Debug.DrawRay(rayPosition, Quaternion.AngleAxis(-45 + m_rayCastModification, Vector3.up) * transform.forward * 50, Color.yellow);
                Debug.DrawRay(rayPosition, Quaternion.AngleAxis(45 + m_rayCastModification, Vector3.up) * transform.forward * 50, Color.yellow);
                Debug.DrawRay(rayPosition, Quaternion.AngleAxis(180 + m_rayCastModification, Vector3.up) * transform.forward * 15f, Color.yellow);

            }
        }

        protected virtual void switchWeapon()
        {
            sheathedSword.SetActive(!sheathedSword.activeSelf);
            fightingSword.SetActive(!fightingSword.activeSelf);

        }

        protected virtual void FightingTarget()
        {

            if (m_currentTarget)
            {
                m_timeSinceLastFight = Time.time;
                if (Vector3.Distance(transform.position, m_currentTarget.transform.position) > 5f)
                {
                    //Vector3 moveTo = m_currentTarget.transform.position;
                    Move(m_currentTarget.transform.position);
                }
                else
                {
                    m_navMeshAgent.ResetPath();
                    if (m_currentTarget.TryGetComponent(out IDamageable damageableObject))
                    {
                        //this line defines whether the enemy is alive before attacking it or not;
                        if (damageableObject.TakeDamage(0))
                        {
                            m_currentTarget = null;
                            m_numOfAttackingSelf -= 1;
                            //m_animator.SetTrigger("sheath");
                            //Invoke("switchWeapon", 1f);
                            m_currentState = m_soldierStates.noCommand;
                            return;

                        }
                        if (Time.time > (m_lastAttackedAt + m_attackCooldown))
                        {
                            Vector3 newDirection = m_currentTarget.transform.position - transform.position;
                            transform.rotation = Quaternion.LookRotation(newDirection);

                            m_animator.SetTrigger("slashing");
                            //if the unit is dead, then the current target should be null, since the destroy method of the unit is called after 3 seconds of its death.
                            damageableObject.TakeDamage(m_damage);

                            m_lastAttackedAt = Time.time;
                        }
                    }
                }
            }
            else
            {
                m_timeSinceLastFight = Time.time;
                m_numOfAttackingSelf -= 1;
                //m_animator.SetTrigger("sheath");
                //Invoke("switchWeapon", 1f); 
                m_currentState = m_soldierStates.noCommand;
            }
        }

        protected virtual void OnDestroy()
        {
            UnitSelection.Instance.allUnits.Remove(this);
        }

        public void SetSelectedSpriteAvailable()
        {
            m_selectedCircle.enabled = true;
        }

        public void SetSelectedSpriteUnavailable()
        {
            m_selectedCircle.enabled = false;
        }

        public void Move(Vector3 destination)
        {
            m_navMeshAgent.SetDestination(destination);
        }

    }
}