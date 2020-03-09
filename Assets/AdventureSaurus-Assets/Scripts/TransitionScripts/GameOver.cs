using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    public void GameOverToMenu(string menu)
    {
        SceneManager.LoadScene(menu);
    }
    public void GameOverToCheckpoint(string Checkpoint)
    {
        SceneManager.LoadScene(Checkpoint);
    }
}
