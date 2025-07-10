using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 5) The TimeController

public class TimeController : MonoBehaviour
{
    public static TimeController Instance { get; private set; }
    private readonly List<IRewindable> objects = new List<IRewindable>();

    public bool IsRewinding { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // toggle rewind mode
        IsRewinding = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // for each object, either record or rewind
        foreach (var obj in objects)
        {
            if (IsRewinding)
                obj.ApplyRewind();
            else
                obj.RecordState();
        }
    }

    public void Register(IRewindable obj)
    {
        if (!objects.Contains(obj))
            objects.Add(obj);
    }
    public void Unregister(IRewindable obj)
    {
        objects.Remove(obj);
    }
}

