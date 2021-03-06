﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(IDTag))]
public class Item : MonoBehaviour
{
    //The prefab that this object is an instance of
    [HideInInspector]
    public GameObject itemPrefabRoot;

    //The ID that lets the inventory know when to stack items
    public string itemNameID = "Item";

    //The icon that is displayed for this item in inventory slots
    public Sprite icon;

    //The maximum amount of this item that can fit in one inventory slot
    public uint maxStackSize = 1;

    //The current amount in this item stack
    public uint currentStackSize = 1;

    //The weight of each individual item in this stack
    public float kilogramPerUnit = 1;

    //The default value of this item
    public int value = 0;
}
