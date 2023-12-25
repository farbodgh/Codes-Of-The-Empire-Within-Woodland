using System.Collections.Generic;
using UnityEngine;

public class House : UnitsAndBuildings.Building
{
    //The dictionary stores the cost of the building
    private static Dictionary<ResourceManagement.Items, int> m_constructionCost
        = new Dictionary<ResourceManagement.Items, int>()
        {
            { ResourceManagement.Items.Wood, 40 }
        };

    protected override void Awake()
    {
        m_numberOfWorkers = 0;
    }
    protected override void Start()
    {
        m_health = 200;
        PeasantsManager.Instance.HouseCreated();
    }
    public override void OnDestroy()
    {
        PeasantsManager.Instance.HouseDestroyed();
        base.OnDestroy();
    }

    public override bool IsBuldingFreeToBuild()
    {
        return false;
    }

    public override Dictionary<ResourceManagement.Items, int> GetBuildingResourceRequirements()
    {
        return m_constructionCost;
    }
}
