using System.Collections.Generic;

public interface IObserver
{
    void OnStorageChanged(ResourceManagement.Items resourceType, int quantity);
}