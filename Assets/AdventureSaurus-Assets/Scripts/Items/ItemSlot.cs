using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    
    [SerializeField] Image image;

    private Item item;
    public Item _item {
        get { return item; }
        set {
            item = value;

            if(item == null)
            {
                image.enabled = false;
            }
            else
            {
                image.sprite = item.Icon;
                image.enabled = true;
            }
        }
    }

    private void OnValidate()
    {
        if(image == null)
        {
            image = GetComponent<Image>();
        }
    }
}
