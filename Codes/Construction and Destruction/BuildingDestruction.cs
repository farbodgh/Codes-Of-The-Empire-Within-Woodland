using UnityEngine;

public class BuildingDestruction : MonoBehaviour
{
    [SerializeField]
    private Texture2D m_destructionModCursor;

    private bool m_isDestructionModeActive = false;
    [SerializeField]
    private LayerMask m_buildingLayer;

    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_destructionSound;

    public static BuildingDestruction Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        m_audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        

        if (!m_isDestructionModeActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButton(1))
        {
            DeactivateDestructionMode();
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_buildingLayer))
        {
            if (hit.collider.gameObject != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if(hit.collider.gameObject.tag != "Keep")
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }
    public void ActivateDestructionMode()
    {
        UnitClick.m_isDestructionModeActive = true;
        m_isDestructionModeActive = true;
        Debug.Log("Destruction Mode Activated");
        Cursor.SetCursor(m_destructionModCursor, new Vector2(m_destructionModCursor.width / 2, m_destructionModCursor.width / 2), CursorMode.Auto);
    }

    public void DeactivateDestructionMode()
    {
        UnitClick.m_isDestructionModeActive = false;
        m_isDestructionModeActive = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void PlayDestructionSoundAtPosition(Vector3 buildingPosition)
    {
        Debug.Log("Playing destruction sound"); 
        AudioSource.PlayClipAtPoint(m_destructionSound, buildingPosition);
    }

}
