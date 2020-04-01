using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    
    [SerializeField] Image image;

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

    protected virtual void OnValidate()
    {
        if(image == null)
        {
            image = GetComponent<Image>();
        }
    }
}
