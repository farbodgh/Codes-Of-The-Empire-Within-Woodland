using UnitsAndBuildings;
using UnityEngine;

public class UnitClick : MonoBehaviour
{
    private Camera m_myCam;

    public LayerMask m_clickableLayer;
    public LayerMask m_groundLayer;
    public LayerMask m_enemyLayer;

    [SerializeField]
    private Texture2D m_cursorAttackPic;

    public static bool m_isDestructionModeActive = false;
    void Start()
    {
        m_myCam = Camera.main;
    }

    private void Update()
    {
        if(m_isDestructionModeActive)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = m_myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_clickableLayer))
            {
                //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
                //if we click on a unit while holding the shift key it adds the unit to the selected units
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (hit.collider.gameObject != null)
                        UnitSelection.Instance.ShiftClickSelect(hit.collider.gameObject);
                }
                //if we click on a unit without holding the shift key it selects the unit
                //In simpler words, normal click
                else
                {
                    if (hit.collider.gameObject != null)
                        UnitSelection.Instance.ClickSelect(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    UnitSelection.Instance.DeselectAll();
                }
            }
        }

        //if the user hover over enemy units the curosr changes to attack cursor
        if (Physics.Raycast(m_myCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, Mathf.Infinity, m_enemyLayer) && UnitSelection.Instance.selectedUnits.Count > 0)
        {
            Cursor.SetCursor(m_cursorAttackPic, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = m_myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_groundLayer))
            {
                if (UnitSelection.Instance.selectedUnits.Count == 1)
                {
                    if (UnitSelection.Instance.selectedUnits[0] != null)
                    {
                        UnitSelection.Instance.selectedUnits[0].PassCommand(hit.point);
                        UnitSelection.Instance.selectedUnits[0].Move(hit.point);
                    }
                    else
                    {
                        UnitSelection.Instance.selectedUnits.RemoveAt(0);
                    }
                }
                else
                {
                    for (int i = 0; i < UnitSelection.Instance.selectedUnits.Count; i++)
                    {
                        if (UnitSelection.Instance.selectedUnits[i] != null)
                            UnitSelection.Instance.selectedUnits[i].PassCommand(hit.point);
                        else
                            UnitSelection.Instance.selectedUnits.RemoveAt(i);
                    }
                    FormationMove(hit.point);
                }
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_enemyLayer))
            {
                for (int i = 0; i < UnitSelection.Instance.selectedUnits.Count; i++)
                {
                    if (UnitSelection.Instance.selectedUnits[i] != null)
                        UnitSelection.Instance.selectedUnits[i].PassTarget(hit.collider.GetComponent<UnitsAndBuildings.Unit>());
                    else
                        UnitSelection.Instance.selectedUnits.RemoveAt(i);
                }
            }
        }

    }

    private void FormationMove(Vector3 destination)
    {
        float soldierSpacing = 3.5f; // Adjust this value for a larger distance between soldiers

        if (UnitSelection.Instance.selectedUnits.Count < 10)
        {
            for (int i = 0; i < UnitSelection.Instance.selectedUnits.Count; i++)
            {
                float row = i / 3; // 3 soldiers per row
                float col = i % 3; // 3 soldiers per column
                Vector3 offset = new Vector3(col * soldierSpacing, 0, row * soldierSpacing);
                if (UnitSelection.Instance.selectedUnits[i] != null)
                    UnitSelection.Instance.selectedUnits[i].Move(destination + offset);
                else
                    UnitSelection.Instance.selectedUnits.RemoveAt(i);
            }
        }
        else if (UnitSelection.Instance.selectedUnits.Count >= 10)
        {
            for (int i = 0; i < UnitSelection.Instance.selectedUnits.Count; i++)
            {
                float row = i / 10; // 10 soldiers per row
                float col = i % 10; // 10 soldiers per column
                Vector3 offset = new Vector3(col * soldierSpacing, 0, row * soldierSpacing);
                if (UnitSelection.Instance.selectedUnits[i] != null)
                    UnitSelection.Instance.selectedUnits[i].Move(destination + offset);
                else
                    UnitSelection.Instance.selectedUnits.RemoveAt(i);
            }
        }
    }
}