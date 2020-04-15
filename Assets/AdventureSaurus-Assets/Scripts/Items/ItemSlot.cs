using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class ItemSlot : MonoBehaviour, IPointerClickHandler 
{
     
    [SerializeField] Image image;

    public event Action<Item> OnRightClickEvent;

    private Item _Item;
    public Item Item {
        get { return _Item; }
        set {
            _Item = value;

            if(_Item == null)
            {
                image.enabled = false;
            }
            else
            {
                image.sprite = _Item.Icon;
                image.enabled = true;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
        {
            
            if (Item != null && OnRightClickEvent != null)
            {
                
                OnRightClickEvent(Item);
            }
        }
    }

    protected virtual void OnValidate()
    {
        if(image == null)
        {
            image = GetComponent<Image>();
        }
    }
}
