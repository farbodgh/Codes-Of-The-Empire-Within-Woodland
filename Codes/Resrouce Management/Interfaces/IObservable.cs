using UnityEngine;

interface IObservable
{
    public void AddObserver(GameObject observer);
    //public void RemoveObserver();
    public void NotifyObserver();
}