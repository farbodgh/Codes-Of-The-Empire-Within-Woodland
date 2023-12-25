using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    public List<UnitsAndBuildings.Soldier> allUnits;
    public List<UnitsAndBuildings.Soldier> selectedUnits;

    public static UnitSelection Instance;

    //Singleton pattern
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
    }

    public void ClickSelect(GameObject unitToAdd)
    {
        UnitsAndBuildings.Soldier soldierComponent = unitToAdd.GetComponent<UnitsAndBuildings.Soldier>();
        DeselectAll();
        if (unitToAdd != null)
        {
            selectedUnits.Add(soldierComponent);
            soldierComponent.SetSelectedSpriteAvailable();
        }
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        UnitsAndBuildings.Soldier soldierComponent = unitToAdd.GetComponent<UnitsAndBuildings.Soldier>();
        //if the unit is not slected add it to the list
        if (!selectedUnits.Contains(soldierComponent))
        {
            Debug.Log("Add unit to list");
            if (unitToAdd != null)
                selectedUnits.Add(soldierComponent);
            soldierComponent.SetSelectedSpriteAvailable();
        }
        //if the unit is selected remove it from the list
        else
        {
            Debug.Log("After else Add unit to list");
            selectedUnits.Remove(soldierComponent);
            if (soldierComponent != null)
                soldierComponent.SetSelectedSpriteUnavailable();
        }
    }

    public void DragSelect(UnitsAndBuildings.Soldier unitToAdd)
    {
        if (!selectedUnits.Contains(unitToAdd))
        {
            if (unitToAdd != null)
                selectedUnits.Add(unitToAdd);
            unitToAdd.SetSelectedSpriteAvailable();
        }
    }

    public void DeselectAll()
    {
        foreach (UnitsAndBuildings.Soldier unit in selectedUnits)
        {
            if (unit != null)
                unit.GetComponent<UnitsAndBuildings.Soldier>().SetSelectedSpriteUnavailable();
        }
        selectedUnits.Clear();
    }

    public void Deselect(GameObject unitToDeselect)
    {
        throw new System.NotImplementedException();
    }

    //public void RemoveNullUnits()
    //{
    //    for (int i = 0; i < allUnits.Count; i++)
    //    {
    //        if (allUnits[i] == null)
    //        {
    //            allUnits.RemoveAt(i);
    //        }
    //    }
    //}

}
