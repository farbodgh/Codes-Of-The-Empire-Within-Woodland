using System.Collections.Generic;
using UnityEngine;

public class PeasantsManager
    : MonoBehaviour, IUIPopulationUpdater
{


    [SerializeField]
    private List<GameObject> m_buildings = new List<GameObject>(32);
    [SerializeField]
    private Queue<GameObject> m_idlePeasants = new Queue<GameObject>(10);
    [SerializeField]
    private Queue<GameObject> m_buildingsThatNeedWorkers = new Queue<GameObject>(10);

    public static PeasantsManager Instance;
    [SerializeField]
    private GameObject[] m_gatheringSpot; 
    private int m_gatheringSpotIndex = 0;
    [SerializeField]
    private GameObject m_farmer;
    [SerializeField]
    private GameObject m_baker;
    [SerializeField]
    private GameObject m_lumberJack;
    [SerializeField]
    private GameObject m_smith;
    [SerializeField]
    private GameObject m_idlePeasant;
    [SerializeField]
    private Transform m_peasantSpawnPoint;
    private int m_idlePeasantCount;
    private int m_houseCount;

    private int m_availablePopulation;

    private float m_timeSinceLastPeasantSpawn;

    private float PEASANTSPAWNINTERVAL = 3;

    private IUIPopulationObserver m_UIobser;
    private void Awake()
    {
        if(Instance == null)
        {
            ((IUIPopulationUpdater)this).AddObserver();
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Spawn a peasant every 3 seconds, if the conditions are met
        SpawnIdlePeasant(PEASANTSPAWNINTERVAL);

        //Assign a peasant to a building that needs workers
        AssignPeasantToWork();


    }


    public void SoldierCreated()
    {
        m_availablePopulation--;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(1, 0);
    }

    public void SoldierKilled()
    {
        m_availablePopulation++;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(-1, 0);
    }

    public void HouseCreated()
    {
        m_houseCount++;
        m_availablePopulation += 4;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(0, 4);

    }
    public void HouseDestroyed()
    {
        m_houseCount--;
        m_availablePopulation -= 4;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(0, -4);
    }

    public void IdlePeasantCreated()
    {
        m_idlePeasantCount++;
        m_availablePopulation--;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(1, 0);
    }

    public void IdlePeasantKilled()
    {
        m_idlePeasantCount--;
        m_availablePopulation++;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(-1, 0);
    }

    //Thess methods are called to hanlde population related calculation when an idle peasant get a job
    public void WorkerPeasantCreated()
    {
        m_availablePopulation--;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(1,0);
    }
    
    public void WorkerPeasantKilled()
    {
        m_availablePopulation++;
        ((IUIPopulationUpdater)this).UpdateUIPopulation(-1,0);
    }

    //The following method spawns an idle peasant, idle peasants gather somewhere near the door of the keep.
    private void SpawnIdlePeasant(float time)
    {
        if (Time.time - m_timeSinceLastPeasantSpawn >= time && m_availablePopulation > 0 && m_idlePeasantCount < m_gatheringSpot.Length)
        {

            m_timeSinceLastPeasantSpawn = Time.time;
            var peasant = Instantiate(m_idlePeasant, m_peasantSpawnPoint.position, Quaternion.identity);
            m_idlePeasants.Enqueue(peasant);
            peasant.GetComponent<Peasants.Peasant>().Move(m_gatheringSpot[m_gatheringSpotIndex].transform);

            m_gatheringSpotIndex = (m_gatheringSpotIndex + 1) % m_gatheringSpot.Length;
            //Debug.Log($"Available pop {m_availablePopulation}");
            //Debug.Log($"Idle peasant count {m_idlePeasantCount}");
            //Debug.Log($"Length {m_gatheringSpot.Length}");
        }

        return;

    }

    public void AddToBuildingsThatNeedWorkers(GameObject building)
    {
        m_buildingsThatNeedWorkers.Enqueue(building);
    }

    private void AssignPeasantToWork()
    {
        if(m_buildingsThatNeedWorkers.Count > 0 && m_idlePeasantCount > 0 && m_idlePeasants.Count != 0)
        {
            //First, get a reference to the building that needs workers, and a reference to an idle peasant
            var building = m_buildingsThatNeedWorkers.Peek();
            var idlePeasant = m_idlePeasants.Peek();
            if(idlePeasant == null)
            {
                m_idlePeasants.Dequeue();
                return;
            }

            //if the building is null, then remove it from the queue
            if (building == null)
            {
                m_buildingsThatNeedWorkers.Dequeue();
                return;
            }

            // If the building have the required number of workers, remove it from the queue
            if (building.GetComponent<UnitsAndBuildings.Building>().IsBuildingHaveEnoughWorkers())
            {
                m_buildingsThatNeedWorkers.Dequeue();
                return;
            }

            //Depending on the type of building, convert the idle peasant to a specific type of peasant
            if (building.GetComponent<UnitsAndBuildings.Building>().GetType() == typeof(UnitsAndBuildings.Farm))
            {
                //Assigning the next peasant occupation to the static variable in the peasant class
                Peasants.Peasant.nextPeasantOccupation = building;
                ConvertPeasant(idlePeasant.gameObject, m_farmer).GetComponent<Peasants.Peasant>().DefineOccupation(building.gameObject);
                m_idlePeasants.Dequeue();
                return;
            }

            if (building.GetComponent<UnitsAndBuildings.Building>().GetType() == typeof(UnitsAndBuildings.Bakery))
            {
                //Assigning the next peasant occupation to the static variable in the peasant class
                Peasants.Peasant.nextPeasantOccupation = building;
                ConvertPeasant(idlePeasant.gameObject, m_baker).GetComponent<Peasants.Baker>().DefineOccupation(building.gameObject);
                m_idlePeasants.Dequeue();
                return;
            }

            if (building.GetComponent<UnitsAndBuildings.Building>().GetType() == typeof(UnitsAndBuildings.Smithery))
            {
                //Assigning the next peasant occupation to the static variable in the peasant class
                Peasants.Peasant.nextPeasantOccupation = building;
                ConvertPeasant(idlePeasant.gameObject, m_smith).GetComponent<Peasants.Smith>().DefineOccupation(building.gameObject);
                m_idlePeasants.Dequeue();
                return;
            }

            if (building.GetComponent<UnitsAndBuildings.Building>().GetType() == typeof(UnitsAndBuildings.WoodCuttingCamp))
            {
                //Assigning the next peasant occupation to the static variable in the peasant class
                Peasants.Peasant.nextPeasantOccupation = building;
                ConvertPeasant(idlePeasant.gameObject, m_lumberJack).GetComponent<Peasants.LumberJack>().DefineOccupation(building.gameObject);
                m_idlePeasants.Dequeue();
                return;
            }


        }
    }
    //The following method is used to convert a peasant to a specific type of peasant
    private GameObject ConvertPeasant(GameObject oldOne, GameObject prefab)
    {
        var peasant = Instantiate(prefab,oldOne.transform.position, oldOne.transform.rotation);
        Destroy(oldOne);
        return peasant;
    }

    //The following method is used to convert any peasant to Idle peasant.
    //This method is used when a building is destroyed and the peasants working at that building must be converted to idle peasants
    public void ConvertToIdlePeasant(GameObject current)
    {
        //Debug.Log("Converting to idle peasant");
        var peasant = Instantiate(m_idlePeasant, current.transform.position, current.transform.rotation);
        Destroy(current);
        IdlePeasantCreated();
        //Now the peasant is converted to idle peasant, so we must assign the peasant to a gathering spot
        m_idlePeasants.Enqueue(peasant);
        peasant.GetComponent<Peasants.Peasant>().Move(m_gatheringSpot[m_gatheringSpotIndex].transform);
        m_gatheringSpotIndex = (m_gatheringSpotIndex + 1) % m_gatheringSpot.Length;
        
    }

    //IUIPopulationUpdater implementation

     void IUIPopulationUpdater.AddObserver()
    {
        m_UIobser = InGameUIManager.Instance.GetComponent<IUIPopulationObserver>();
    }

    void IUIPopulationUpdater.UpdateUIPopulation(int populationChange, int maxPopulationChange)
    {
       if(populationChange != 0)
        {
            m_UIobser.OnPopulationChanged(populationChange); 
        }

        if (maxPopulationChange != 0)
        {
            m_UIobser.OnHouseCountChanged(maxPopulationChange);
        }

    }

    public bool IsIdlePeasantAvailable()
    {
        return m_idlePeasants.Count > 0;
    }

    public void RemoveOneIdlePeasant()
    {
        if(m_idlePeasants.Count > 0)
        {
            Destroy(m_idlePeasants.Dequeue());
        }
    }
}
