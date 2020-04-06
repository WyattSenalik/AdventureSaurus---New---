using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSlot : ItemSlot
{
    public ItemType ItemType;

    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.name = ItemType.ToString() + " Slot";
    }

    /// <summary>
    /// FUnction used to add item to equip slot. Returns true if successful. Also returns previous item in the slot if any.
    /// </summary>
    /// <param name="equip">The item to be equipped.</param>
    /// <param name="previousItem">The item in the equip slot before equipping.</param>
    /// <returns></returns>
    public bool addItem(Item equip, out Item previousItem)
    {
        if (equip.ItemType == this.ItemType)//Checks if type of item matches the type of slot.
        {
            previousItem = Item;//Gets previous item before equiping over it.
            Item = equip;//assign new item to slot
            return true;
        }

        previousItem = null;//returns nothing if no swap occurs
        return false;
    }

    /// <summary>
    /// Function used to remove an item from the equipment slot. Returns true if successful.
    /// </summary>
    /// <returns></returns>
    public bool removeItem()
    {
        if(Item != null)
        {
            Item = null;
            return true;
        }

        return false;

    }

}
