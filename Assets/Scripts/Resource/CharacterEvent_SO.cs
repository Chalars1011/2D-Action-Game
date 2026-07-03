using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/CharacterEvent_SO")]
public class CharacterEvent_SO : ScriptableObject
{
    public UnityAction<Character> OnEventRaised;

    public void RaiseEvent( Character character) 
    {
        OnEventRaised?.Invoke(character);
    }
}
