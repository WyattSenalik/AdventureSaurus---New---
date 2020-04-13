using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuToGame : MonoBehaviour
{
    /// <summary>
    /// Loads the specified level
    /// </summary>
    /// <param name="level">Name of the scene to load</param>
    public void MenuToScene(string level)
    {
        SceneManager.LoadScene(level);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitApp()
    {
        Application.Quit();
    }
}
