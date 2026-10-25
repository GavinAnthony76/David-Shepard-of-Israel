using UnityEditor;
public static class DavidCampaignMenu
{
    [MenuItem("David/Rebuild Template Campaign")]
    public static void Rebuild()
    {
        System.IO.Directory.CreateDirectory("Assets/David/Scenes");
        DavidNativeCampaignBuilder.Build();
    }
}
