using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TestTransition : MonoBehaviour
{
    public void MenuToScene(string level)
    {
        SceneManager.LoadScene(level);
    }
}
