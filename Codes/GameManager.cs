using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour,IJob
{

    public static GameManager Instance { get; private set; }

    private GameObject m_king;

    //As ther should be only one barraacks in the game this variable take care of it
    private bool m_isBarracksBuilt = false;
    private UnitsAndBuildings.Barracks m_barracks;

    private AudioSource m_audiSource;
    //these variable is used in order to instantiate the enemy
    [SerializeField]
    private GameObject m_enemyPrefab;
    [SerializeField]
    private GameObject[] m_enemySpawnPoints;
    int numberOfSpawnedEnemies = 0;
    private const int IncreaseInNumberOfEnemies = 25;
    private float m_timeSinceLastAttack = 0;
    //300 seconds = 5 minutes
    private const int TimeBetweenAttacks = 300;
    //an array of 300 trees which are closest to the player's keep(0,0,0);
    public GameObject[] m_closestTrees = new GameObject[300];

    //Constants for the initial resources amount in the game
    public static readonly int InitialWood = 600;
    public static readonly int InitialFood = 100;
    private void Awake()
    {
        if (Instance == null)
        {

            #if !UNITY_EDITOR
            Debug.unityLogger.logEnabled = false;
            #endif
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        m_audiSource = GetComponent<AudioSource>(); 
    }
    
    void Start()
    {
        m_king = GameObject.Find("King");
        Application.targetFrameRate = 120;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > m_timeSinceLastAttack + TimeBetweenAttacks)
        {
            m_timeSinceLastAttack = Time.time;
            numberOfSpawnedEnemies += IncreaseInNumberOfEnemies;
            ((IJob)this).Execute();
            
        }
        if (!m_king)
        {
            Debug.Log("Game over");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            SceneManager.LoadScene (sceneName:"GameOver");
        }
    }

    public GameObject FindClosestTreeToPoint(Vector3 point)
    {
        GameObject closestTree = null;
        float minDistance = float.MaxValue;

        foreach (GameObject tree in m_closestTrees)
        {
            float distance = Vector3.Distance(tree.transform.position, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTree = tree;
            }
        }

        return closestTree;
    }

    private void EnemyAttack()
    {
        m_audiSource.Play();
        int spawnPointIndex = 0;
        for (int i = 0; i < numberOfSpawnedEnemies; i++)
        {

            Instantiate(m_enemyPrefab, m_enemySpawnPoints[spawnPointIndex].transform.position, Quaternion.identity);
            spawnPointIndex = (spawnPointIndex + 1) % m_enemySpawnPoints.Length;

        }
    }

    public void BarracksBuilt(GameObject barracks)
    {
        m_barracks = barracks.GetComponent<UnitsAndBuildings.Barracks>();
        m_isBarracksBuilt = true;
    }

    public void BarracksDestroyed()
    {
        m_isBarracksBuilt = false;
    }

    public bool IsBarracksBuilt()
    {
        return m_isBarracksBuilt;
    }

    public void TrainSoldier()
    {
        if(m_isBarracksBuilt)
        {
            m_barracks.TrainSoldier();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void IJob.Execute()
    {
        EnemyAttack();
    }
}
