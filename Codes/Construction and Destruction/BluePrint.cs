using ResourceManagement;
using Unity.Jobs;
using UnityEngine;

public class BluePrint : MonoBehaviour, IJob
{
    static Unity.AI.Navigation.NavMeshSurface m_navMeshSurface;
    static bool m_isNavMeshSurfaceFound = false;
    //It is used to store the reference to the building that is going to be built
    static UnitsAndBuildings.Building m_building;

    RaycastHit hit;

    [SerializeField]
    private GameObject m_prefab;
    private GameObject m_ground;
    private int m_numberOfCollisions = 0;

    private bool m_isResourceAmountSufficient = true;
    private bool m_isOutOfBunds = false;
    //bool isPositionOccupied = false;

    

    //this variable holds a reference to the currently selected building blueprint.
    private static GameObject m_currentlySelectedBuildingBlueprint;

    private void Awake()
    {
        //This line ensures that he newluy instantiated building blueprint destroy the previously instantiated building blueprint
        if (m_currentlySelectedBuildingBlueprint != null)
        {
            Destroy(m_currentlySelectedBuildingBlueprint);
        }
        m_currentlySelectedBuildingBlueprint = gameObject;
        Cursor.visible = false;
        //Caching a reference to the nav mesh surface
        if (!m_isNavMeshSurfaceFound)
        {
            Debug.Log("Line 1 blueprint");
            Debug.Log("Ground is not null");
            m_navMeshSurface = GameObject.Find("Ground").GetComponentInChildren<Unity.AI.Navigation.NavMeshSurface>();

            Debug.Log("Line 2 blueprint");
            if (m_navMeshSurface)
            {
                Debug.Log("NavMeshSurface is not null");
                m_isNavMeshSurfaceFound = true;
            }
        }
    }
    private void Start()
    {
        if (m_prefab.GetComponent<UnitsAndBuildings.Barracks>() != null)
        {
            if (GameManager.Instance.IsBarracksBuilt())
            {
                Destroy(gameObject);
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 6)))
        {
            transform.position = hit.point;
        }
        m_ground = GameObject.Find("Ground");
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Destroy(gameObject);
        }

        // Check collisions with other buildings
        if (!IsOccupied())
        {

            if (Input.GetMouseButton(0))
            {
                if (!IsOccupied() && !m_isOutOfBunds)
                {
                    m_building = m_prefab.GetComponent<UnitsAndBuildings.Building>();
                    if (m_building.IsBuldingFreeToBuild())
                    {
                        Instantiate(m_prefab, hit.point, transform.rotation);
                        if (m_ground != null)
                        {
                            //((IJob)this).Execute();

                        }
                        else
                        {
                            Debug.LogError("The reference to the ground is null in the construction system script (blueprint.cs)");
                        }
                        Destroy(gameObject);
                    }
                    else
                    {
                        var resourceRequirements = m_building.GetBuildingResourceRequirements();
                        foreach (var resource in resourceRequirements)
                        {
                            //if all required resources are available
                            //then we should deduct the resources from the inventory
                            m_isResourceAmountSufficient = m_isResourceAmountSufficient & InventoryCMS.Instance.IsResourceAmountSufficient(resource.Key, resource.Value);
                        }
                        if (m_isResourceAmountSufficient)
                        {
                            foreach (var resource in resourceRequirements)
                            {
                                InventoryCMS.Instance.DeductResource(resource.Key, resource.Value);
                            }
                            Instantiate(m_prefab, hit.point, transform.rotation);
                            if (m_ground != null)
                            {
                                Debug.Log("buldign NavMesh");
                                //((IJob)this).Execute();
                            }
                            else
                            {
                                Debug.LogError("The reference to the ground is null in the construction system script (blueprint.cs)");
                            }
                            Destroy(gameObject);
                        }
                        else
                        {
                            Debug.Log("Not enough resources to build the building");
                        }
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 6)))
        {
            transform.position = hit.point;
            m_isOutOfBunds = false;
        }
        else
        {
            m_isOutOfBunds = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //When the blue print of the building collides with something else the variable m_numberOfCollisions is incremented
        m_numberOfCollisions++;
    }
    private void OnTriggerExit(Collider other)
    {
        m_numberOfCollisions--;
    }
    private bool IsOccupied()
    {
        //Debug.Log($"m_numberOfCollisions {m_numberOfCollisions}");
        if (m_numberOfCollisions > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Based on the prefab we should check whether there is enough resources to build the building
    private bool CheckResources()
    {

        return false;
    }

    void IJob.Execute()
    {
        m_navMeshSurface.BuildNavMesh();
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}




