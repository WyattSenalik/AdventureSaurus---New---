﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Inventory inventoryREF = null;
    [SerializeField] private EquipmentPanel equipmentPanelREF = null;
    [SerializeField] CharDetailedMenuController CharDetailedMenuControllerREF = null;

    [SerializeField] AllyStats AllyStatsREF;

    public void setAllyStatsREF(AllyStats reference)
    {
        AllyStatsREF = reference;
    }
    
    private void Awake()
    {
        inventoryREF.OnItemRightClickedEvent += EquipFromInv;
        equipmentPanelREF.OnItemRightClickedEvent += UnequipFromEquipPanel;
    }

    private void EquipFromInv(Item item)
    {
        Equip(item);
    }

    private void UnequipFromEquipPanel(Item item)
    {
        Unequip(item);
    }

    public void Equip(Item equippingItem)
    {
        
        if (inventoryREF.removeItem(equippingItem))
        {
            Item previousItem;
            if (equipmentPanelREF.addItem(equippingItem, out previousItem))
            {
                if(previousItem != null)
                {
                    inventoryREF.addItem(previousItem);
                    previousItem.Unequip(AllyStatsREF);
                }
                equippingItem.Equip(AllyStatsREF);
                CharDetailedMenuControllerREF.RefreshStats(AllyStatsREF);
                
            }
            else
            {
                
                inventoryREF.addItem(equippingItem);
            }
        }
    }

    public void Unequip(Item unequipItem)
    {
        if (!inventoryREF.IsFull() && equipmentPanelREF.removeItem(unequipItem))
        {
            unequipItem.Unequip(AllyStatsREF);
            CharDetailedMenuControllerREF.RefreshStats(AllyStatsREF);
            inventoryREF.addItem(unequipItem);
        }
    }

}
