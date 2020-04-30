using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistCharges: MonoBehaviour
{
    public int _potCharges;
    HealthPotion _holdCharge;


    // Start is called before the first frame update
    private void Awake()
    {
        _holdCharge = GameObject.FindGameObjectWithTag("PotionHolder").GetComponent<HealthPotion>();
    }
    private void LateUpdate()
    {
        if(_holdCharge._update == true)
        {
            _potCharges = _holdCharge.Charges;
            _holdCharge._update = false;
        }
    }
}
