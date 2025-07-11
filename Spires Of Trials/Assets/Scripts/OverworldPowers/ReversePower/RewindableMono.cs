using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2) Base class with an extraInt slot and virtual hooks
public class RewindableMono : MonoBehaviour, IRewindable
{
    protected struct State
    {
        public Vector3 pos;
        public Quaternion rot;
        public float timestamp;
        public int extraInt;
    }

    [Tooltip("How many seconds to buffer for rewind")]
    private float bufferDuration = 1000f;
    protected List<State> buffer = new List<State>();

    public virtual bool CanInstantRewind => false;

    protected virtual void OnEnable()
    {
        //TimeController.Instance.Register(this);
    }


    protected virtual void Start()
    {
        if (TimeController.Instance != null)
            TimeController.Instance.Register(this);
        else
            Debug.LogError("No TimeController found in scene!", this);
    }

    protected virtual void OnDisable()
    {
        if (TimeController.Instance != null)
            TimeController.Instance.Unregister(this);
    }



    public virtual void RecordState()
    {
        float now = Time.time;
        buffer.Insert(0, new State
        {
            pos = transform.position,
            rot = transform.rotation,
            timestamp = now,
            extraInt = GetExtraInt()
        });

        // purge anything older than bufferDuration
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            if (now - buffer[i].timestamp > bufferDuration)
                buffer.RemoveAt(i);
            else
                break;
        }
    }

    public virtual void ApplyRewind()
    {
        if (CanInstantRewind)
            return;

        if (buffer.Count > 0)
        {
            State s = buffer[0];
            transform.position = s.pos;
            transform.rotation = s.rot;
            buffer.RemoveAt(0);
            ApplyExtraInt(s.extraInt);
        }
    }

    // override this in subclasses to record a custom integer
    protected virtual int GetExtraInt() => 0;

    // override this in subclasses to apply the custom integer when rewinding
    protected virtual void ApplyExtraInt(int extraInt) { }
}
