using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// is event manager 
/// </summary>
public class EventManager : MonoBehaviour
{
    #region Singleton Instance

    /// <summary>
    /// Singleton instance of the EventManager.
    /// Ensures there is only one instance managing game events.
    /// </summary>
    public static EventManager Instance { get; private set; }

    #endregion

    #region Event Dictionaries

    /// <summary>
    /// Dictionary storing events without parameters.
    /// The key is the event name (string), and the value is an action (delegate method).
    /// </summary>
    private Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    /// <summary>
    /// Dictionary storing events that require parameters.
    /// The key is the event name (string), and the value is an action that takes an object as a parameter.
    /// </summary>
    private Dictionary<string, Action<object>> eventDictionaryWithArgs = new Dictionary<string, Action<object>>();

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Implements the Singleton pattern to ensure only one EventManager exists.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure EventManager persists across scenes
            Debug.Log("EventManager initialized.");
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate EventManagers to enforce singleton behavior
        }
    }

    #endregion

    #region StartListeningMethods

    /// <summary>
    /// Adds a listener to an event without arguments.
    /// If the event already exists, the listener is added to it; otherwise, a new event is created.
    /// </summary>
    /// <param name="eventName">The name of the event to listen for.</param>
    /// <param name="listener">The method to execute when the event is triggered.</param>
    public void StartListening(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent += listener; // Add listener to existing event
            eventDictionary[eventName] = thisEvent;
        }
        else
        {
            eventDictionary.Add(eventName, listener); // Create new event entry
        }
    }

    /// <summary>
    /// Adds a listener to an event with arguments.
    /// If the event already exists, the listener is added to it; otherwise, a new event is created.
    /// </summary>
    /// <param name="eventName">The name of the event to listen for.</param>
    /// <param name="listener">The method to execute when the event is triggered, with an object argument.</param>
    public void StartListening(string eventName, Action<object> listener)
    {
        if (eventDictionaryWithArgs.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent += listener; // Add listener to existing event
            eventDictionaryWithArgs[eventName] = thisEvent;
        }
        else
        {
            eventDictionaryWithArgs.Add(eventName, listener); // Create new event entry
        }
    }

    #endregion

    #region StopListeningMethods

    /// <summary>
    /// Removes a listener from an event without arguments.
    /// If the event has no more listeners, it is removed from the dictionary.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The listener method to remove.</param>
    public void StopListening(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent -= listener; // Remove listener from event

            // If no more listeners remain, remove the event from the dictionary
            if (thisEvent == null)
                eventDictionary.Remove(eventName);
            else
                eventDictionary[eventName] = thisEvent;
        }
    }

    /// <summary>
    /// Removes a listener from an event with arguments.
    /// If the event has no more listeners, it is removed from the dictionary.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The listener method to remove.</param>
    public void StopListening(string eventName, Action<object> listener)
    {
        if (eventDictionaryWithArgs.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent -= listener; // Remove listener from event

            // If no more listeners remain, remove the event from the dictionary
            if (thisEvent == null)
                eventDictionaryWithArgs.Remove(eventName);
            else
                eventDictionaryWithArgs[eventName] = thisEvent;
        }
    }

    #endregion

    #region TriggerEventMethods

    /// <summary>
    /// Triggers an event without arguments.
    /// Executes all listeners associated with the event.
    /// </summary>
    /// <param name="eventName">The name of the event to trigger.</param>
    public void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(); // Invoke all subscribed listeners
        }
    }

    /// <summary>
    /// Triggers an event with an argument.
    /// Executes all listeners associated with the event, passing in the provided argument.
    /// </summary>
    /// <param name="eventName">The name of the event to trigger.</param>
    /// <param name="argument">The argument to pass to the event listeners.</param>
    public void TriggerEvent(string eventName, object argument)
    {
        if (eventDictionaryWithArgs.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(argument); // Invoke all subscribed listeners with argument
        }
    }

    #endregion

}
