using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void GameOverToMenu(string menu)
    {
        // Destroy the persistant controller
        GameObject persistObj = GameObject.FindWithTag("PersistantController");
        PersistantController persistRef = persistObj.GetComponent<PersistantController>();
        persistRef.PrepareForQuit();
        SceneManager.LoadScene(menu);
    }
    public void GameOverToCheckpoint(string Checkpoint)
    {
        SceneManager.LoadScene(Checkpoint);
    }
}
