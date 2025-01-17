using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum itemType
{
    Bandage,
    Key,
    Rune,
    Gem,
    flashlight,
    Default
}

public abstract class pickupObject : ScriptableObject
{
    public GameObject prefab; //item to display
    public itemType type; //what type of object
    public int amount;
    public bool destroyAfterUse;
    public bool triggerType;
    [TextArea(10, 10)]
    public string description;
}
