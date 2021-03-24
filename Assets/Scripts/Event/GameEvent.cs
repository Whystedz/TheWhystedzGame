using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class would be the Subject of an Event System.
/// (Look up "Observer design pattern" for more information)
/// 
/// @Special thanks to Ryan Hipple (https://unity.com/how-to/architect-game-code-scriptable-objects)
/// </summary>
[CreateAssetMenu(menuName = "Event System/GameEvent")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> listeners = new List<GameEventListener>();

    /// <summary>
    /// Signal all the listeners (observers)
    /// </summary>
    public void Raise()
    {
        int listenersAmount = this.listeners.Count;
        for (int i = listenersAmount - 1; i >= 0; i--)
            this.listeners[i].OnEventRaised();
    }

    public void RegisterListener(GameEventListener listener) => this.listeners.Add(listener);

    public void UnregisterListener(GameEventListener listener)  => this.listeners.Remove(listener);
}
