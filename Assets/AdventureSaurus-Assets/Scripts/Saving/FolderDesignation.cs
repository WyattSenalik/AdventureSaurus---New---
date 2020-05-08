using UnityEngine;

public class FolderDesignation : MonoBehaviour
{
    // Get rid of the existing folders on start
    private void Start()
    {
        SaveSystem.RefreshMainSaveFolder("Infinite");
        SaveSystem.RefreshMainSaveFolder("Campaign");
    }

    /// <summary>
    /// Sets the main save folder to Infinite
    /// </summary>
    public void CreateInfiniteDir()
    {
        SaveSystem.RefreshMainSaveFolder("Infinite");
    }

    /// <summary>
    /// Sets the main save folder to Campaign
    /// </summary>
    public void CreateCampaignDir()
    {
        SaveSystem.RefreshMainSaveFolder("Campaign");
    }
}
