using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // key = resource name, value = how many the player has
    private Dictionary<string, int> resourceCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // add amount (default 1) to a named resource
    public void AddResource(string resourceName, int amount = 1)
    {
        if (resourceCounts.ContainsKey(resourceName))
            resourceCounts[resourceName] += amount;
        else
            resourceCounts[resourceName] = amount;

        // fire events so anyone listening can update
        EventManager.Instance.TriggerEvent($"resourceAdded_{resourceName}");
        EventManager.Instance.TriggerEvent("resourceChanged");
    }

    // remove amount (default 1), return false if not enough
    public bool RemoveResource(string resourceName, int amount = 1)
    {
        int current = GetResourceCount(resourceName);
        if (current < amount)
            return false;

        resourceCounts[resourceName] = current - amount;
        EventManager.Instance.TriggerEvent($"resourceRemoved_{resourceName}");
        EventManager.Instance.TriggerEvent("resourceChanged");
        return true;
    }

    // get current count (0 if none)
    public int GetResourceCount(string resourceName)
    {
        if (resourceCounts.TryGetValue(resourceName, out int count))
            return count;
        return 0;
    }

    // returns a copy of all counts
    public Dictionary<string, int> GetAllResources()
    {
        return new Dictionary<string, int>(resourceCounts);
    }
}
