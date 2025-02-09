﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using UnityEngine;

public class EquipmentPanel : MonoBehaviour
{
    [SerializeField] private Transform equipmentSlotsParent = null;
    [SerializeField] private EquipSlot[] equipmentSlots = null;

    public event Action<Item> OnItemRightClickedEvent;

    private void Start()
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].OnRightClickEvent += OnItemRightClickedEvent;
        }
    }

    private void OnValidate()
    {
        equipmentSlots = equipmentSlotsParent.GetComponentsInChildren<EquipSlot>();
    }

    /// <summary>
    /// FUnction used to add item to equip slot. Returns true if successful. Also returns previous item in the slot if any.
    /// </summary> 
    /// <param name="equip">The item to be equipped.</param>
    /// <param name="previousItem">The item in the equip slot before equipping.</param>
    /// <returns></returns>
    public bool addItem(Item equip, out Item previousItem)
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {

            if (equipmentSlots[i].ItemType == equip.ItemType)//Checks if type of item matches the type of slot.
            {
                previousItem = equipmentSlots[i].Item;//Gets previous item before equiping over it.
                equipmentSlots[i].Item = equip;//assign new item to slot
                return true;
            }
        }
        previousItem = null;//returns nothing if no swap occurs
        return false;
    }


    /// <summary>
    /// Finds and removes Item from equipped items.
    /// </summary>
    /// <param name="equip"></param>
    /// <returns></returns>
    public bool removeItem(Item equip)
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {

            if (equipmentSlots[i].Item == equip)//Checks if type of item matches the type of slot.
            {
                
                equipmentSlots[i].Item = null;//assign new item to slot
                return true;
            }
        }
//returns nothing if no swap occurs
        return false;
    }

    }
