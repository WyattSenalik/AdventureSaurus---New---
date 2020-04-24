using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPotion : MonoBehaviour
{
    //Potion charges
    [SerializeField] private Text _chargesText = null;
    //Potion image
    [SerializeField] private Image _potionImage = null;
    //Potion Position
    Vector3 _defaultPos;

    //current charges on the potion
    private int _charges;
    public int Charges
    {
        get { return _charges; }
        set { _charges = value; }
    }
    private bool _isHolding;
    

    //current ally
    private int _allyIndex;

    //list of ally health script
    private List<AllyHealth> _alliesHealth;


    private void OnEnable()
    {
        // Subscribe to when a character is clicked, so that we can move to them
        MoveAttackGUIController.OnCharacterSelect += CharacterToHeal;
        // When the generation finishes, initialize this script
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }

    // Called when the gameobject is toggled active
    // Unsubscribe from events

    private void OnDisable()
    {
        // Subscribe to when a character is clicked, so that we can move to them
        MoveAttackGUIController.OnCharacterSelect -= CharacterToHeal;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events

    private void OnDestroy()
    {
        // Subscribe to when a character is clicked, so that we can move to them
        MoveAttackGUIController.OnCharacterSelect -= CharacterToHeal;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    private void Initialize()
    {
        Transform allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;

        // Initialize ally stats
        _alliesHealth = new List<AllyHealth>();
        // Iterate over the allies to get their health
        foreach (Transform allyTrans in allyParent)
        {
            // Try to ge the ally's health
            AllyHealth allyHealth = allyTrans.GetComponent<AllyHealth>();
            // If the Health aren't null, add it to the list
            if (allyHealth != null)
                _alliesHealth.Add(allyHealth);
        }
    }
    private void Start()
    {
        //starting charges
        _charges = 3;
        //start without potion in hand
        _isHolding = false;
        _defaultPos = _potionImage.transform.position;
    }

    private void LateUpdate()
    {
        //updates the charges whenever it is changed
        _chargesText.text = _charges.ToString();

        //updates potion image to follow mouse
        if(_isHolding==true)
        {
            _potionImage.transform.position = Input.mousePosition;
        }
        //returns potion to default position
        else if(_isHolding==false)
        {
            _potionImage.transform.position = _defaultPos;
        }
    }

    /// <summary>
    /// when you click on the potion you pick it up and whoever you click on next recieves it reduces charges by 1
    /// if you reclick the potion with a potion in hand it will put it back down increasing the charges by 1
    /// </summary>
    public void HoldingPotion()
    {
        //if you aren't holding the potion you will pick it up
        if (_isHolding == false && _charges!=0)
        {
            _isHolding = true;

            
        }
        //if you are already holding the potion and reclick the potion icon it puts it back
        else if(_isHolding == true)
        {
            _isHolding = false;


        }
            
       

    }

    //This is really cumbersome I'm going to fix this later but I want to have it working to some extent
    // Gets character selected using the event and changes allyIndex to match the character selected character
    private void CharacterToHeal(MoveAttack charMA)
    {
        if (charMA.WhatAmI == CharacterType.Ally &&_isHolding==true)
        {
            // reduces the charges
            _charges--;

            if (charMA.name == _alliesHealth[0].name)
            {
                _allyIndex = 0;
            }
            else if (charMA.name == _alliesHealth[1].name)
            {
                _allyIndex = 1;
            }
            else if (charMA.name == _alliesHealth[2].name)
            {
                _allyIndex = 2;
            }

            HealCharacter();
            _isHolding = false;
       }
    }

    //Heals the ally that is listed under _allyIndex
    public void HealCharacter()
    {
        Debug.Log("Someone is being healed");
        AllyHealth allyHealth = null;
        // Check the index is valid and that the value is not null
        if (_alliesHealth[_allyIndex] != null)
            allyHealth = _alliesHealth[_allyIndex];
        else
            Debug.Log("Character you are trying to heal has no stats");
        Debug.Log(allyHealth.name + " has been healed you no longer have a potion in hand");
        allyHealth.ConsumePotion();
    }
}
