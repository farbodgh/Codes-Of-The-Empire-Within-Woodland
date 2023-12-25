using UnityEngine;
using UnityEngine.AI;

namespace UnitsAndBuildings
{
    /// <summary>
    /// This class represents a base unit in the game and serves as an abstract base class for specific unit types like Soldier and Peasant.
    /// It defines common attributes and behaviors shared by all units.
    /// </summary>
    public abstract class Unit : MonoBehaviour, IDamageable
    {
        // Common attributes for all units
        //As hitpoint should be used directly in other child classes, it must defines as protected;
        protected float m_hitPoint;
        protected int m_damage;
        protected float m_attackRate;
        protected float m_range;
        protected Animator m_animator;
        protected NavMeshAgent m_navMeshAgent;
        public int m_numOfAttackingSelf;
        protected float m_attackCooldown = 2f;

        protected AudioSource m_audioSource;
        [SerializeField]
        protected AudioClip m_getHittedSound;
        [SerializeField]
        protected AudioClip m_dyingSound;
        protected UnitsAndBuildings.Unit m_currentTarget;
        protected UnitsAndBuildings.Building m_currentBuildingTarget;
        protected Peasants.Peasant m_currentPeasantTarget;

        protected bool m_isAlive = true;


        /// <summary>
        /// Called when the unit attacks another enemy unit.
        /// </summary>


        //This method is used to handle the damaging process of all units.
        bool IDamageable.TakeDamage(int damage, int piercingDamage = 0)
        {
            if (m_hitPoint <= 0)
            {
                return true;
            }
            if (damage <= 0)
            {
                return false;
            }

                m_hitPoint -= damage;

            if(m_hitPoint <= 0)
            {
                m_navMeshAgent.isStopped = true;
                ((IDamageable)this).Die();
                return true;
            }
            else
            {
                
                m_audioSource.PlayOneShot(m_getHittedSound);
                return false;
            }
        }

        //This method handles all the behaviors that should be done when a unit is died.
        void IDamageable.Die()
        {
            m_isAlive = false;
            m_navMeshAgent.ResetPath();
            m_audioSource.PlayOneShot(m_dyingSound);
            m_animator.SetBool("death", true);
            Destroy(gameObject, 3f);
        }


    }
}