using System.Collections.Generic;
using UnityEngine;

//This class is used in order to manage all the distinct types of storages in the game.

namespace ResourceManagement
{
    public enum Items
    {
        //Items are the resources that are stored in the stockpiles
        Wood,

        //m_Foods are the resources that are stored in the granaries
        Bread,

        //Weapons are the resources that are stored in the armories
        Weapon
    }

    class InventoryCMS : MonoBehaviour, IUIResourceUpdater
    {
        public static InventoryCMS Instance { get; private set;}
        //A list of all the stockpiles in the game
        private List<GameObject> m_stockpiles = new List<GameObject>(3);
        private List<GameObject> m_graneries = new List<GameObject>(3);
        private List<GameObject> m_armories = new List<GameObject>(3);

        //A list of types of items in each inventory
        private ResourceManagement.Items[] m_stockpileItems = new ResourceManagement.Items[1] { ResourceManagement.Items.Wood };
        private ResourceManagement.Items[] m_graneryItems = new ResourceManagement.Items[1] { ResourceManagement.Items.Bread };
        private ResourceManagement.Items[] m_armoryItems = new ResourceManagement.Items[1] { ResourceManagement.Items.Weapon };

        private IUIResourcesObserver m_UIobser;

        void Awake()
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

        private void Start()
        {
            //As the reference to the observer is static it should be assigned only once
            if (m_UIobser == null)
            {
                ((IUIResourceUpdater)this).AddObserver();
            }
        }

        //This method is used in order to register an appropriate observer(storage) for each peasant that needs to store resources
        public void RegisterPeasantInAppropriateStorage(Peasants.Peasant peasant)
        {
            Debug.Log($"RegisterPeasantInAppropriateStorage: peasant name : {peasant.name}, and peasant type : {peasant.GetType()}");
            //Depend on the type of the peasant's occupation, we should register him in the appropriate storage
            //peasants are subjects and storages are observers
            if (peasant.GetType() == typeof(Peasants.Baker))
            {

                if(m_graneries.Count <= 0)
                {
                    return;
                }
                var storage = FindClosestStorageForTheOccuption(m_graneries, peasant.GetComponent<Peasants.Peasant>().occupation.transform);

                peasant.GetComponent<IObservable>().AddObserver(storage);
                return;
            }

            if(peasant.GetType() == typeof(Peasants.Smith))
            {
                if(m_armories.Count <= 0)
                {
                    return;
                }
                var storage = FindClosestStorageForTheOccuption(m_armories, peasant.GetComponent<Peasants.Peasant>().occupation.transform);
                peasant.GetComponent<IObservable>().AddObserver(storage);
                return;
            }

            if(peasant.GetType() == typeof(Peasants.LumberJack))
            {
                if(m_stockpiles.Count <= 0)
                {
                    return;
                }
                var storage = FindClosestStorageForTheOccuption(m_stockpiles, peasant.GetComponent<Peasants.Peasant>().occupation.transform);
                peasant.GetComponent<IObservable>().AddObserver(storage);
                return;
            }
        }

        //This method is used in order to find the closest storage to the occupation of the peasant
        private GameObject FindClosestStorageForTheOccuption(List<GameObject> storages, Transform occupationLocation)
        {
            GameObject closestStorage = storages[0];
            for (int i = 1; i < storages.Count; i++)
            {
                if (storages[i] == null)
                {
                    storages.RemoveAt(i);
                    continue;
                }
                //There is no need to know exact distance so the sqrt() operation is redundant, and it is better to use sqrMagnitude instead of Vector3.Distance()
                if ((storages[i].transform.position - occupationLocation.position).sqrMagnitude < (closestStorage.transform.position - occupationLocation.position).sqrMagnitude)
                {
                    closestStorage = storages[i];
                }
            }
            return closestStorage;
        }

        //This method is used in order to register a new storage in the game, and put it in the appropriate list
        public void RegisterStorage(GameObject storage)
        {
            if(storage.GetComponent<UnitsAndBuildings.Stockpile>() != null)
            {
                m_stockpiles.Add(storage);
            }
            else if(storage.GetComponent<UnitsAndBuildings.Granary>() != null)
            {
                Debug.Log("RegisterStorage: granary");  
                m_graneries.Add(storage);
            }
            else if(storage.GetComponent<UnitsAndBuildings.Armory>() != null)
            {
                m_armories.Add(storage);
            }
        }

        //This method deducts the amount of the resource from the player's inventory
        //This method is used where the player wants to build a building, or train a unit to deduct a resource from the player's inventory
        public void DeductResource(Items resourceType, int quantity)
        {
            //check which of the storages is responsible for the resource type
           for(int i = 0; i < m_stockpileItems.Length; i++)
            {
                if (m_stockpileItems[i] == resourceType)
                {
                    //check if the player has enough of the resource

                    {
                        //deduct the amount of the resource from the player's inventory
                        UnitsAndBuildings.Stockpile.DeductResourceFromStockpile(resourceType, quantity);
                        //The observer should be notified that the amount of the resource has changed
                        ((IUIResourceUpdater)this).UpdateUIResources(-quantity, 0, 0);
                        return;
                    }
                }
            }

           for(int i = 0; i < m_graneryItems.Length; i++)
            {
                if (m_graneryItems[i] == resourceType)
                {
                    UnitsAndBuildings.Granary.DeductResourceFromGranary(resourceType, quantity);
                    //The observer should be notified that the amount of the resource has changed
                    ((IUIResourceUpdater)this).UpdateUIResources(0, -quantity, 0);
                    return;
                }
            }

           for (int i = 0; i < m_armoryItems.Length; i++)
            {
                if (m_armoryItems[i] == resourceType)
                {
                    UnitsAndBuildings.Armory.DeductAWeaponFromArmory(resourceType, quantity);
                    //The observer should be notified that the amount of the resource has changed
                    ((IUIResourceUpdater)this).UpdateUIResources(0, 0, -quantity);
                    return;
                }
            }
        }

        //This method checks if the player has enough of a resource type or not
        public bool IsResourceAmountSufficient(Items resourceType, int quantity)
        {
            for (int i = 0; i < m_stockpileItems.Length; i++)
            {
                if (m_stockpileItems[i] == resourceType)
                {
                    return(UnitsAndBuildings.Stockpile.CheckIfSufficient(resourceType, quantity));
                }
            }
            for(int i = 0; i < m_graneryItems.Length; i++)
            {
                   if (m_graneryItems[i] == resourceType)
                {
                    return(UnitsAndBuildings.Granary.CheckIfSufficient(resourceType, quantity));
                }
            }
            for(int i = 0; i < m_armoryItems.Length; i++)
            {
                   if (m_armoryItems[i] == resourceType)
                {
                    return(UnitsAndBuildings.Armory.CheckIfSufficient(resourceType, quantity));
                }
            }
                return false;
        }

        //This method when there is a change in the amount of a resource in the inventory, it notifies the UI to update the amount of the resource
        public void UpdateUIResources(int woodChange, int FoodChange, int weaponChange)
        {
            if(woodChange != 0)
            {
                m_UIobser.OnWoodChanged(woodChange);
            }
            if(FoodChange != 0)
            {
                m_UIobser.OnFoodChanged(FoodChange);
            }
            if(weaponChange != 0)
            {
                m_UIobser.OnWeaponChanged(weaponChange);
            }

        }

        public void AddObserver()
        {
            m_UIobser = InGameUIManager.Instance.GetComponent<IUIResourcesObserver>();
        }

        public void RemoveDestroyedStorageFromList(GameObject destroyedStorage)
        {
            if(destroyedStorage.GetComponent<UnitsAndBuildings.Stockpile>() != null)
            {
                m_stockpiles.Remove(destroyedStorage);
            }
            else if(destroyedStorage.GetComponent<UnitsAndBuildings.Granary>() != null)
            {
                m_graneries.Remove(destroyedStorage);
            }
            else if(destroyedStorage.GetComponent<UnitsAndBuildings.Armory>() != null)
            {
                m_armories.Remove(destroyedStorage);
            }
        }
    }
}