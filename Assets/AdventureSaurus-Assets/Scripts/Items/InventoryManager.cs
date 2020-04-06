using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] Inventory inventoryREF;
    [SerializeField] EquipmentPanel equipmentPanelREF;
    private void Awake()
    {
        inventoryREF.OnItemRightClickedEvent += Equip;
    }

    private void EquipFromInv(Item item)
    {
        Equip(item);
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
                }
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
            inventoryREF.addItem(unequipItem);
        }
    }

}
