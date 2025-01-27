using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();
    private Dictionary<string, Action<object>> eventDictionaryWithArgs = new Dictionary<string, Action<object>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("EventManager initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Add a listener for events without arguments
    public void StartListening(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent += listener;
            eventDictionary[eventName] = thisEvent;
        }
        else
        {
            eventDictionary.Add(eventName, listener);
        }
    }

    // Add a listener for events with arguments
    public void StartListening(string eventName, Action<object> listener)
    {
        if (eventDictionaryWithArgs.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent += listener;
            eventDictionaryWithArgs[eventName] = thisEvent;
        }
        else
        {
            eventDictionaryWithArgs.Add(eventName, listener);
        }
    }

    // Stop listening for events without arguments
    public void StopListening(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
                eventDictionary.Remove(eventName);
            else
                eventDictionary[eventName] = thisEvent;
        }
    }

    // Stop listening for events with arguments
    public void StopListening(string eventName, Action<object> listener)
    {
        if (eventDictionaryWithArgs.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
                eventDictionaryWithArgs.Remove(eventName);
            else
                eventDictionaryWithArgs[eventName] = thisEvent;
        }
    }

    // Trigger an event without arguments
    public void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke();
        }
    }

    // Trigger an event with arguments
    public void TriggerEvent(string eventName, object argument)
    {
        if (eventDictionaryWithArgs.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(argument);
        }
    }
}
