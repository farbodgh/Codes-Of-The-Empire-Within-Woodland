using UnityEngine;

namespace UnitsAndBuildings
{
    public class Swordsman : UnitsAndBuildings.Soldier
    {


        // Start is called before the first frame update
        protected override void Start()
        {
            m_audioSource = GetComponent<AudioSource>();
            base.Start();
            m_hitPoint = 100;
            m_damage = 10;
            m_navMeshAgent.speed = 10;
            PeasantsManager.Instance.SoldierCreated();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

 

        protected override void OnDestroy()
        {
            PeasantsManager.Instance.SoldierKilled();
            base.OnDestroy();
        }   
    }
}