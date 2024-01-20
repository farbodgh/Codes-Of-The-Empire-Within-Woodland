using ResourceManagement;
using System.Collections.Generic;
using UnityEngine;

namespace UnitsAndBuildings
{
    public class Armory : UnitsAndBuildings.Building, IObserver, IUIResourceUpdater
    {

        //The dictionary stores the cost of the building
        private static Dictionary<ResourceManagement.Items, int> m_constructionCost
            = new Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Wood, 80 }
            };

        private static Dictionary<ResourceManagement.Items, int> m_weapons
            = new Dictionary<ResourceManagement.Items, int>()
            {
                { ResourceManagement.Items.Weapon, 0 }
            };

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
            InventoryCMS.Instance.RecheckNearestArmories();
        }

        void IObserver.OnStorageChanged(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_weapons.ContainsKey(resourceType))
            {
                m_weapons[resourceType] += quantity;
                //Update the UI(Observer) about the change in the amount of the resource
                ((IUIResourceUpdater)this).UpdateUIResources(0, 0, quantity);
                Debug.Log("Armory: we have " + resourceType + ": " + m_weapons[resourceType]);
            }
        }

        //This method deeducts the weapons from the Armory
        //As the deduction is done by the InventoryCMS and this method is static, the observer should be notified at CMS about deduction
        public static void DeductAWeaponFromArmory(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_weapons.ContainsKey(resourceType))
            {
                if (m_weapons[resourceType] >= quantity)
                {
                    m_weapons[resourceType] -= quantity;
                    Debug.Log("Armory: we have " + resourceType + ": " + m_weapons[resourceType]);
                }
            }
        }

        //This method checks if the stockpile has enough of a resource type or not
        public static bool CheckIfSufficient(ResourceManagement.Items resourceType, int quantity)
        {
            if (m_weapons.ContainsKey(resourceType))
            {
                if (m_weapons[resourceType] >= quantity)
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
            if (weaponChange != 0)
            {
                m_UIobser.OnWeaponChanged(weaponChange);
            }
        }

        public void AddObserver()
        {
            m_UIobser = InGameUIManager.Instance.GetComponent<IUIResourcesObserver>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            InventoryCMS.Instance.RemoveDestroyedStorageFromList(gameObject);
        }
    }
}
