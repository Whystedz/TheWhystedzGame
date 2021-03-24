using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class would be the Observer of an Event System.
/// (Look up "Observer design pattern" for more information)
/// 
/// @Special thanks to Ryan Hipple (https://unity.com/how-to/architect-game-code-scriptable-objects)
/// </summary>
public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;
    public UnityEvent Response;

    private void OnEnable() => Event.RegisterListener(this);

    private void OnDisable() => Event.UnregisterListener(this);

    public void OnEventRaised() => Response.Invoke();
}
