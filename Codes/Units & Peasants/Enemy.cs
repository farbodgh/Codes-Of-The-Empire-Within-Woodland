using UnityEngine;
using UnityEngine.AI;



namespace UnitsAndBuildings
{
    public class Enemy : UnitsAndBuildings.Soldier
    {

        protected enum m_enemyStates
        {
            roaming,
            targetKeep,
            targetSolider,
            targetBuilding,
            targetPeasants,
        }
        protected m_enemyStates m_currentEnemyState;


        private Vector3 keepPosition;
        private float distance;

        //Right now keep is in the following location of the map
        private readonly static Vector3 KeepPosition = new Vector3(19, 0, 0);



        protected override void Start()
        {
            m_animator = GetComponent<Animator>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            m_audioSource = GetComponent<AudioSource>();

            m_hitPoint = 50f;
            m_damage = 10;

            m_lastAttackedAt = Time.time;

            this.keepPosition = KeepPosition;
            m_currentEnemyState = m_enemyStates.targetKeep;
            m_rayCastModificationSpeed = 360;
            m_rayCastModification = 45;
        }

        protected override void Targeting()
        {

            RaycastHit hitResult;
            Vector3 rayPosition = transform.position + new Vector3(0, 1, 0);
            m_rayCastModification += m_rayCastModificationSpeed * Time.deltaTime;


            m_rayRight = new Ray(transform.position, Quaternion.AngleAxis(-45 + m_rayCastModification, Vector3.up) * transform.forward);

            m_rayLeft = new Ray(transform.position, Quaternion.AngleAxis(45 + m_rayCastModification, Vector3.up) * transform.forward);

            m_rayBehind = new Ray(transform.position, Quaternion.AngleAxis(180 + m_rayCastModification, Vector3.up) * transform.forward);

            //The vision system range of the enemy is different for different targets.
            //In this way they prioritize the soldiers, but not in a way they seems smart, since they are undead army they are not supposed to be that smart.


            // Soldier;
            if (Physics.Raycast(m_rayRight, out hitResult, Mathf.Infinity, 1 << 8) || Physics.Raycast(m_rayLeft, out hitResult, Mathf.Infinity, 1 << 8)
                 || Physics.Raycast(m_rayBehind, out hitResult, 15f, 1 << 8))
            {
                m_currentTarget = hitResult.collider.GetComponent<UnitsAndBuildings.Unit>();

                if (m_currentTarget && m_currentTarget.m_numOfAttackingSelf < 6)
                {
                    m_currentTarget.m_numOfAttackingSelf += 1;
                    m_currentEnemyState = m_enemyStates.targetSolider;
                }
            }


            // Peasants
            if (Physics.Raycast(m_rayRight, out hitResult, 60f, 1 << 10) || Physics.Raycast(m_rayLeft, out hitResult, 60f, 1 << 10)
                || Physics.Raycast(m_rayBehind, out hitResult, 15f, 1 << 10))
            {
                m_currentPeasantTarget = hitResult.collider.GetComponent<Peasants.Peasant>();
                if (m_currentPeasantTarget && m_currentPeasantTarget.m_numOfAttackingSelf < 1)
                {
                    m_currentPeasantTarget.m_numOfAttackingSelf += 1;
                    m_currentEnemyState = m_enemyStates.targetPeasants;
                }
            }

            // Buildings
            if (Physics.Raycast(m_rayRight, out hitResult, 30f, 1 << 7) || Physics.Raycast(m_rayLeft, out hitResult, 30f, 1 << 7)
            || Physics.Raycast(m_rayBehind, out hitResult, 15f, 1 << 7))
            {
                Debug.Log("Sees building");
                m_currentBuildingTarget = hitResult.collider.GetComponent<UnitsAndBuildings.Building>();

                if (m_currentBuildingTarget && m_currentBuildingTarget.m_numOfAttackingSelf < 3)
                {
                    m_currentBuildingTarget.m_numOfAttackingSelf += 1;
                    m_currentEnemyState = m_enemyStates.targetBuilding;
                }
            }
        }


        protected override void Update()
        {

            //if the agent is not alive but not destroyed yet non of the following code should be executed.
            if (!m_isAlive)
            {
                if(m_navMeshAgent.enabled)
                {
                    m_navMeshAgent.enabled = false;
                }    

                return;
            }
            //float  = GetComponent<NavMeshAgent>().velocity.magnitude;

            if (m_navMeshAgent.velocity.magnitude > 0)
            {
                m_animator.SetBool("moving", true);
            }
            else
            {
                m_animator.SetBool("moving", false);
            }

            switch (m_currentEnemyState)
            {
                case m_enemyStates.targetKeep:
                    // king is at keep an enemy is going to keep range
                    moveToKeep();
                    Targeting();
                    break;
                case m_enemyStates.roaming:
                    // if at keep and king is within keep range
                    distance = (transform.position - keepPosition).sqrMagnitude;
                    if (m_navMeshAgent.velocity.magnitude == 0f)
                    {
                        roaming();
                    }
                    if (distance > 3600)
                    {
                        m_currentEnemyState = m_enemyStates.targetKeep;
                    }
                    Targeting();
                    break;
                case m_enemyStates.targetSolider:
                    FightingTarget();
                    break;
                case m_enemyStates.targetBuilding:
                    BreakingBuilding();
                    break;
                case m_enemyStates.targetPeasants:
                    FightingPeasants();
                    break;
            }
        }

        protected virtual void FightingPeasants()
        {
            if (m_currentPeasantTarget)
            {
                if ((transform.position - m_currentPeasantTarget.transform.position).sqrMagnitude > 25f)
                {
                    if (m_navMeshAgent.destination != m_currentPeasantTarget.transform.position)
                        Move(m_currentPeasantTarget.transform.position);
                }
                else
                {
                    m_navMeshAgent.ResetPath();
                    if (m_currentPeasantTarget.TryGetComponent(out IDamageable damageableObject))
                    {
                        if (Time.time > (m_lastAttackedAt + m_attackCooldown))
                        {
                            rotate();
                            m_animator.SetTrigger("slashing");
                            damageableObject.TakeDamage(m_damage);
                            m_lastAttackedAt = Time.time;
                        }
                    }
                }
            }
            else { m_currentEnemyState = m_enemyStates.targetKeep; }

        }
        protected virtual void rotate()
        {
            if (!m_currentTarget) { return; }
            Vector3 newDirection = m_currentTarget.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        protected virtual void BreakingBuilding()
        {

            if (m_currentBuildingTarget)
            {
                Vector3 m_currentBuildingTargetPos = m_currentBuildingTarget.transform.Find("attackPoint").position;

                if ((transform.position - m_currentBuildingTargetPos).sqrMagnitude > 25f)
                {

                    if (m_navMeshAgent.destination != m_currentBuildingTargetPos)
                        Move(m_currentBuildingTargetPos);
                }
                else
                {
                    m_navMeshAgent.ResetPath();
                    if (m_currentBuildingTarget.TryGetComponent(out IDamageable damageableObject))
                    {
                        if (Time.time > (m_lastAttackedAt + m_attackCooldown))
                        {
                            m_animator.SetTrigger("slashing");
                            damageableObject.TakeDamage(m_damage);
                            m_lastAttackedAt = Time.time;
                        }
                    }
                }
            }
            else { m_currentEnemyState = m_enemyStates.targetKeep; }
        }


        protected override void FightingTarget()
        {
            if (m_currentTarget)
            {
                if ((transform.position - m_currentTarget.transform.position).sqrMagnitude > 25f)
                {
                    if (m_navMeshAgent.destination != m_currentTarget.transform.position)
                        Move(m_currentTarget.transform.position);
                }
                else
                {
                    m_navMeshAgent.ResetPath();
                    if (m_currentTarget.TryGetComponent(out IDamageable damageableObject))
                    {
                        if (Time.time > (m_lastAttackedAt + m_attackCooldown))
                        {
                            if (damageableObject.TakeDamage(m_damage))
                            {
                                m_currentTarget = null;
                                m_currentEnemyState = m_enemyStates.targetKeep;
                                return;
                            }
                            rotate();
                            m_animator.SetTrigger("slashing");
                            m_lastAttackedAt = Time.time;
                        }
                    }
                }
            }
            else { m_currentEnemyState = m_enemyStates.targetKeep; }
        }

        protected virtual Vector3 RandomNavmeshLocation(float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        protected virtual void roaming()
        {
            Move(RandomNavmeshLocation(30f));
        }

        //public virtual void Move(Vector3 destination)
        //{
        //    m_navMeshAgent.SetDestination(destination);  
        //}

        protected virtual void moveToKeep()
        {
            distance = (transform.position - keepPosition).sqrMagnitude;

            if (distance < 2500)
            {
                m_navMeshAgent.ResetPath();
                Debug.Log("Close to keep");
                m_currentEnemyState = m_enemyStates.roaming;
                return;
            }
            if (m_navMeshAgent.destination != keepPosition)
                Move(keepPosition);

        }
    }
}
