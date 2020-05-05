using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorMain { RED, GREEN, BLUE };

public class Rainbow : MonoBehaviour
{
    // How fast the colors change
    [SerializeField] private float _speed = 3f;

    // The particle system to spawn
    private GameObject _partSysObj = null;
    // Reference to the particle system spawned
    private ParticleSystem _myPartSys;
    // If we have spawnd the particle system yet
    private bool _hasSpawned;

    // The current color of the rainbow effect
    private Color _curCol;
    // The current color we are increasing
    private ColorMain _colToChange;

    // Spriteredender attached to this gameobject
    private SpriteRenderer _sprRendRef;

    // Called before start
    // Set references to itself
    private void Awake()
    {
        _sprRendRef = this.GetComponent<SpriteRenderer>();

        // Load the prefab that holds the particle system
        _partSysObj = Resources.Load<GameObject>("RainbowPrefab");

        if (_partSysObj == null)
            Debug.Log("Could not load Rainbow Particles");
    }

    // Start is called before the first frame update
    private void Start()
    {
        _hasSpawned = false;
        _sprRendRef.color = new Color(0, 1, 1);
        _curCol = _sprRendRef.color;
        _colToChange = ColorMain.RED;

        StartFlashing();
    }

    /// <summary>
    /// Begin the rainbow effect
    /// </summary>
    public void StartFlashing()
    {
        // Set the default values
        _curCol = _sprRendRef.color;
        _colToChange = ColorMain.RED;
        // If we haven't spawend the particle system yet, spawn it
        if (!_hasSpawned)
        {
            //Debug.Log("Hello from " + this.name);
            // Instantiate it
            GameObject partSysObj = Instantiate(_partSysObj);
            // Have it be centered on this character
            partSysObj.transform.SetParent(this.transform);
            partSysObj.transform.localPosition = Vector3.zero;
            // Get the ParticleSystem off it, so we can change its colors
            _myPartSys = partSysObj.GetComponent<ParticleSystem>();

            _hasSpawned = true;
        }
        StopAllCoroutines();
        // Start the rainbow effect
        StartCoroutine(StarPowerUpdate());
    }

    /// <summary>
    /// Makes the Rainbow effect change colors
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator StarPowerUpdate()
    {
        // Continue flashing the rainbow until this is destroyed
        while (true)
        {
            // If we are to increase red
            if (_colToChange == ColorMain.RED)
            {
                // Increase red, decrement green
                _curCol.r += Time.deltaTime * _speed;
                _curCol.g -= Time.deltaTime * _speed;
                // If we reached max red, swap _colToChange
                if (_curCol.r >= 1)
                {
                    _colToChange += 1;
                }
                yield return null;
            }
            // If we are to increase green
            else if (_colToChange == ColorMain.GREEN)
            {
                // Increase green, decrement blue
                _curCol.g += Time.deltaTime * _speed;
                _curCol.b -= Time.deltaTime * _speed;
                // If we reached max green, swap _colToChange
                if (_curCol.g >= 1)
                {
                    _colToChange += 1;
                }
                yield return null;
            }
            // If we are to increase blue
            else
            {
                // Increase blue, decrement red
                _curCol.b += Time.deltaTime * _speed;
                _curCol.r -= Time.deltaTime * _speed;
                // If we reached max blue, swap _colToChange
                if (_curCol.b >= 1)
                {
                    _colToChange = ColorMain.RED;
                }
                yield return null;
            }
            // Set the color of this sprite renderer
            _sprRendRef.color = _curCol;
            // Change the particle system's color
            ParticleSystem.MainModule tempMain = _myPartSys.main;
            tempMain.startColor = _curCol;
            
            yield return null;
        }
    }
}
