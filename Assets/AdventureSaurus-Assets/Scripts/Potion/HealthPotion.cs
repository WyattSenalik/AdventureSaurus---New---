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
    private bool _isHolding;

    //Perstant stuff
    //bool destroy
    public bool _update;

    //current ally
    private AllyHealth _allyToHeal;

    private AudioManager _audManRef;


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

        _update = false;

        // Get the audio manager
        try
        {
            _audManRef = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
        }
        catch
        {
            Debug.Log("Could not get AudioManager");
        }
    }
    private void Start()
    {
        //starting charges
        
        //start without potion in hand
        _isHolding = false;
        _defaultPos = _potionImage.transform.position;
    }

    private void LateUpdate()
    {
        //updates the charges whenever it is changed
        _chargesText.text = PersistantController.GetPotCharges().ToString();

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
        if (_isHolding == false && PersistantController.GetPotCharges() != 0)
        {
            _isHolding = true;
        }
        //if you are already holding the potion and reclick the potion icon it puts it back
        else if(_isHolding == true)
        {
            _isHolding = false;
        }
    }

    // Gets character selected using the event and changes allyIndex to match the character selected character
    private void CharacterToHeal(MoveAttack charMA)
    {
        if (charMA.WhatAmI == CharacterType.Ally &&_isHolding==true)
        {
            // reduces the charges
            PersistantController.SetPotCharges(PersistantController.GetPotCharges() - 1);

            _allyToHeal = charMA.GetComponent<AllyHealth>();
            if (_allyToHeal == null)
                Debug.LogError("No AllyHealth attached to " + _allyToHeal.name);

            HealCharacter();
            _isHolding = false;
       }
    }

    //Heals the ally that is listed under _allyIndex
    private void HealCharacter()
    {
        _allyToHeal.ConsumePotion();
        _allyToHeal = null;
        _update = true;
        _audManRef.PlaySound("Heal");
    }

    //refills potion back to three
    public void RefillPotion()
    {
        PersistantController.SetPotCharges(3);
        _update = true;
        Debug.Log("Potions have been refilled");
    }

}
