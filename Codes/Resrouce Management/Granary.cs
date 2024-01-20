using ResourceManagement;
using System.Collections.Generic;
using UnityEngine;


namespace UnitsAndBuildings
{
    public class Granary : Building, IObserver, IUIResourceUpdater
    {

        //The dictionary stores the cost of the building
        private static Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 30 }
            };

        private static Dictionary<ResourceManagement.Items, int> m_Foods
            = new Dictionary<ResourceManagement.Items, int>() { { ResourceManagement.Items.Bread, GameManager.InitialWood } };



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
            InventoryCMS.Instance.RecheckNearestGranaries();
        }

        void IObserver.OnStorageChanged(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_Foods.ContainsKey(resourceType))
            {
                //Update the UI(Observer) about the change in the amount of the resource
                ((IUIResourceUpdater)this).UpdateUIResources(0, quantity, 0);
                m_Foods[resourceType] += quantity;
                Debug.Log("Granary: we have Food: " + m_Foods[resourceType]);
            }
        }

        //This method deeducts the foods from the Granary
        //As the deduction is done by the InventoryCMS and this method is static, the observer should be notified at CMS about deduction
        public static void DeductResourceFromGranary(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_Foods.ContainsKey(resourceType))
            {
                if (m_Foods[resourceType] >= quantity)
                {
                    m_Foods[resourceType] -= quantity;
                    Debug.Log("Granary: we have Food: " + m_Foods[resourceType]);

                }
            }
        }
        
        //This method checks if the stockpile has enough of a resource type or not
        public static bool CheckIfSufficient(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_Foods.ContainsKey(resourceType))
            {
                if (m_Foods[resourceType] >= quantity)
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
        public override void OnDestroy()
        {
            ResourceManagement.InventoryCMS.Instance.RemoveDestroyedStorageFromList(gameObject);
            base.OnDestroy();
        }
        //This method when there is a change in the amount of a resource in the inventory, it notifies the UI to update the amount of the resource
        public void UpdateUIResources(int woodChange, int FoodChange, int weaponChange)
        {
            if (FoodChange != 0)
            {
                m_UIobser.OnFoodChanged(FoodChange);
            }
        }

        public void AddObserver()
        {
            m_UIobser = InGameUIManager.Instance.GetComponent<IUIResourcesObserver>();
        }
    }
}







