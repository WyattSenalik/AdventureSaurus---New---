using UnityEngine;

public class FolderDesignation : MonoBehaviour
{
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
