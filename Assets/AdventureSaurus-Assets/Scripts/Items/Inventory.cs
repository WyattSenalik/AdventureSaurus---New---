﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<Item> items;
    [SerializeField] Transform itemParent;
    [SerializeField] ItemSlot[] itemSlots;

    private void OnValidate()
    {
        if(itemParent != null)
        {
            itemSlots = itemParent.GetComponentsInChildren<ItemSlot>();
        }

        refreshUI();
    }

    /// <summary>
    /// Refreshes the UI to match current data.
    /// </summary>
    private void refreshUI()
    {
        int i = 0;
        for(; i<items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].item = items[i];
        }

        for (; i < itemSlots.Length; i++)
        {
            itemSlots[i].item = null;
        }
    }

    /// <summary>
    /// Used to add an item to inventory
    /// </summary>
    /// <param name="item">The item being added.</param>
    /// <returns></returns>
    public bool addItem(Item item)
    {
        if (IsFull())
            return false;

        items.Add(item);
        refreshUI();
        return true;
    }

    public bool removeItem(Item item)
    {
        if (items.Remove(item))
        {
            refreshUI();
            return true;
        }
        return false;
    }

    public bool IsFull()
    {
        return items.Count >= itemSlots.Length;
    }
}
