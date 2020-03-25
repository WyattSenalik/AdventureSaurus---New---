using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    
    [SerializeField] Image image;

    private Item Item;
    public Item item {
        get { return Item; }
        set {
            Item = value;

            if(Item == null)
            {
                image.enabled = false;
            }
            else
            {
                image.sprite = Item.Icon;
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
