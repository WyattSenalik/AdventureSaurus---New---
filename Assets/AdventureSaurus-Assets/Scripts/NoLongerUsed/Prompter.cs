using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prompter : MonoBehaviour
{
    private Transform charParent;   // Parent of all the characters
    [SerializeField] private GameObject[] uIElementsToTurnOff = null;   // The UI elements that will be turned off by the pause
    private bool[] uIElementsActiveStatus;  // Holds if each element of uIElementsToTurnOff was on or off before the game was paused
    [SerializeField] private GameObject[] pauseMenuElements = null;     // The things we want to turn on when the game is paused
    private InputController inpContRef; // Reference to the Input controller
    // References to the various scripts that will be turned off by the pause
    private List<MoveAttack> allyMARefs;
    private List<MoveAttack> enemyMARefs;
    private MoveAttackController mAContRef;
    private MoveAttackGUIController mAGUIContRef;
    private EnemyMoveAttackAI enemyMAAIRef;
    private TurnSystem turnSysRef;

    private List<MonoBehaviour> scriptsToTurnOff;   // The scripts that will be turned off by the pause

    private float prevTime; // The timeScale before the pause
    private bool isPaused;  // If the game is paused or not

    // Set references
    private void Awake()
    {
        scriptsToTurnOff = new List<MonoBehaviour>();

        // Get the scripts on the GameController
        GameObject gameController = GameObject.FindWithTag("GameController");
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            inpContRef = gameController.GetComponent<InputController>();
            if (inpContRef == null)
                Debug.Log("Could not find InputController attached to " + gameController.name);

            mAContRef = gameController.GetComponent<MoveAttackController>();
            if (mAContRef == null)
                Debug.Log("Could not find MoveAttackController attached to " + gameController.name);
            scriptsToTurnOff.Add(mAContRef);

            mAGUIContRef = gameController.GetComponent<MoveAttackGUIController>();
            if (mAGUIContRef == null)
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameController.name);
            scriptsToTurnOff.Add(mAGUIContRef);

            enemyMAAIRef = gameController.GetComponent<EnemyMoveAttackAI>();
            if (enemyMAAIRef == null)
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameController.name);
            scriptsToTurnOff.Add(enemyMAAIRef);

            turnSysRef = gameController.GetComponent<TurnSystem>();
            if (turnSysRef == null)
                Debug.Log("Could not find TurnSystem attached to " + gameController.name);
            scriptsToTurnOff.Add(turnSysRef);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        prevTime = Time.timeScale;
        isPaused = false;

        // Initialize whether the uIElementsToTurnOff are active or not
        uIElementsActiveStatus = new bool[uIElementsToTurnOff.Length];
        for (int i = 0; i < uIElementsToTurnOff.Length; ++i)
        {
            uIElementsActiveStatus[i] = uIElementsToTurnOff[i].activeSelf;
        }
    }

    /// <summary>
    /// Called from Procedural Generation after everything is created.
    /// Gets the allies and enemies from the character parent
    /// </summary>
    /// <param name="charPar">Parent of all characters</param>
    public void Initiasdalize(Transform charPar)
    {
        // Set the transform of the character parent
        charParent = charPar;

        // Get the scripts on the characters
        allyMARefs = new List<MoveAttack>();
        enemyMARefs = new List<MoveAttack>();
        foreach (Transform child in charParent)
        {
            MoveAttack mARef = child.GetComponent<MoveAttack>();
            if (mARef == null)
            {
                Debug.Log("Could not find MoveAttack attached to " + child.name);
                continue;
            }
            if (mARef.WhatAmI == CharacterType.Ally)
                allyMARefs.Add(mARef);
            else if (mARef.WhatAmI == CharacterType.Enemy)
                enemyMARefs.Add(mARef);
            else
                Debug.Log(child.name + " is having an identity crisis");
            scriptsToTurnOff.Add(mARef);
        }
    }

    /// <summary>
    /// Pauses or unpauses the game. Called from a button
    /// </summary>
    public void ProasdpasdtGame()
    {
        if (!isPaused)
        {
            // Change the time scale
            prevTime = Time.timeScale;
            Time.timeScale = 0;
            //if (Time.fixedDeltaTime - prevTime < 0)
            //Time.fixedDeltaTime = 0;
            //else
            //Time.fixedDeltaTime -= prevTime;
        }
        else
        {
            // Revert the time
            Time.timeScale = prevTime;
            //Time.fixedDeltaTime += prevTime;
        }
        // Turn on/off scripts
        ToggleScriptActivity(isPaused);

        isPaused = !isPaused;

        // Turn on/off the pause menu
        TogglePauseMenu(isPaused);
    }

    /// <summary>
    /// Turns scripts either on or off
    /// </summary>
    /// <param name="onOff">Whether the scripts should be turned on or off</param>
    private void ToggleScriptActivity(bool onOff)
    {
        // We have to deal with the UI elements that need to be turned off when we pause
        for (int i = 0; i < uIElementsToTurnOff.Length; ++i)
        {
            if (uIElementsToTurnOff[i] != null)
            {
                // If we are to be pausing
                if (!onOff)
                {
                    // Update the uIElementsActiveStatus for when we unpause and turn everything off
                    uIElementsActiveStatus[i] = uIElementsToTurnOff[i].activeSelf;
                    uIElementsToTurnOff[i].SetActive(onOff);
                }
                // If we are to be unpausing, revert the on status back to what it was before the pause
                else
                {
                    uIElementsToTurnOff[i].SetActive(uIElementsActiveStatus[i]);
                }
            }
        }

        // In case any scripts get nullified, like when a character dies, we need to be able to remove those scripts
        // This list holds those scripts that must be removed
        List<MonoBehaviour> scriptsToRemove = new List<MonoBehaviour>();
        // Turn on/off the scripts
        foreach (MonoBehaviour script in scriptsToTurnOff)
        {
            // If the script does not exist anymore
            if (script == null)
            {
                scriptsToRemove.Add(script);
                continue;
            }
            script.enabled = onOff;
        }
        // Remove the missing scripts
        foreach (MonoBehaviour script in scriptsToRemove)
        {
            scriptsToTurnOff.Remove(script);
        }
    }

    /// <summary>
    /// Enables/Disables the pause menu
    /// </summary>
    /// <param name="onOff">Whether to turn on or off the pause menu</param>
    private void TogglePauseMenu(bool onOff)
    {
        foreach (GameObject element in pauseMenuElements)
        {
            if (element == null)
            {
                Debug.Log("An element in pauseMenuElements is null. Check the predefined elements in the editor.");
                continue;
            }
            element.SetActive(onOff);
        }
    }
}
