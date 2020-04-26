using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTest : MonoBehaviour
{
    [SerializeField] private string _reloadSceneName = "ReloadScene";

    public void Reload()
    {
        SceneManager.LoadScene(_reloadSceneName);
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame();
    }
}
