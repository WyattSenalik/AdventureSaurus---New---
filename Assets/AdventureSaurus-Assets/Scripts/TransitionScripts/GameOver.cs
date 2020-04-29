using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void GameOverToMenu(string menu)
    {
        // Destroy the persistant controller
        try
        {
            GameObject persistObj = GameObject.FindWithTag("PersistantController");
            PersistantController persistRef = persistObj.GetComponent<PersistantController>();
            persistRef.PrepareForQuit();
        }
        catch
        {
            Debug.LogError("No Persistant Controller found");
        }
        // Load the menu scene
        SceneManager.LoadScene(menu);
    }
    public void GameOverToCheckpoint(string Checkpoint)
    {
        SceneManager.LoadScene(Checkpoint);
    }
}
