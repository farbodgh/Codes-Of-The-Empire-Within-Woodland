using System.Collections.Generic;
using UnityEngine;


namespace UnitsAndBuildings
{
    public class Stockpile : UnitsAndBuildings.Building, IObserver, IUIResourceUpdater
    {


        //The dictionary stores the cost of the building
        private static Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 30 }
            };

        private static Dictionary<ResourceManagement.Items, int> m_goods
            = new   Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, GameManager.InitialWood }
            };  

        private const int m_initialWood = 600;

        //A shared that holds the reference to the UI observer
        private static IUIResourcesObserver m_UIobser;

        protected override void Awake()
        {
            m_workersNeeded = 0;
        }

        protected override void Start()
        {
            base.Start();
            ResourceManagement.InventoryCMS.Instance.RegisterStorage(gameObject);
            //As the reference to the observer is static it should be assigned only once
            if (m_UIobser == null)
            {
                ((IUIResourceUpdater)this).AddObserver();
            }
        }

        void IObserver.OnStorageChanged(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_goods.ContainsKey(resourceType))
            {
                //Update the UI(Observer) about the change in the amount of the resource
                ((IUIResourceUpdater)this).UpdateUIResources(quantity, 0, 0);
                m_goods[resourceType] += quantity;
                Debug.Log("Stockpile: we have " + resourceType + ": " + m_goods[resourceType]);
            }
        }

        //This method deeducts the resource from the stockpile
        ////As the deduction is done by the InventoryCMS and this method is static, the observer should be notified at CMS about deduction
        public static void DeductResourceFromStockpile(ResourceManagement.Items resourceType, int quantity)
        {
            if(m_goods.ContainsKey(resourceType))
            {
                if (m_goods[resourceType] >= quantity)
                {
                    m_goods[resourceType] -= quantity;
                    Debug.Log("Stockpile: we have " + resourceType + ": " + m_goods[resourceType]);
                }
            }
        }

        //This method checks if the stockpile has enough of a resource type or not
        public static bool CheckIfSufficient(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_goods.ContainsKey(resourceType))
            {
                if (m_goods[resourceType] >= quantity)
                {
                    return true;
                }
            }
            return false;
        }
        
        public override bool IsBuldingFreeToBuild()
        {
            return false;
        }

        public override Dictionary<ResourceManagement.Items, int> GetBuildingResourceRequirements()
        {
            return m_constructionCost;
        }

        //This method when there is a change in the amount of a resource in the inventory, it notifies the UI to update the amount of the resource
        public void UpdateUIResources(int woodChange, int FoodChange, int weaponChange)
        {
            if (woodChange != 0)
            {
                m_UIobser.OnWoodChanged(woodChange);
            }
        }

        public void AddObserver()
        {
            m_UIobser = InGameUIManager.Instance.GetComponent<IUIResourcesObserver>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ResourceManagement.InventoryCMS.Instance.RemoveDestroyedStorageFromList(gameObject);
        }
    }
}
