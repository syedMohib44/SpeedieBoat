using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class PListProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
#if UNITY_IPHONE
        // Replace with your iOS AdMob app ID. Your AdMob App ID will look
        // similar to this sample ID: ca-app-pub-3940256099942544~1458002511
        //string appId = "ca-app-pub-1507685666870088~1231588092";

        //string plistPath = Path.Combine(path, "Info.plist");
        //PlistDocument plist = new PlistDocument();

        //plist.ReadFromFile(plistPath);
        //plist.root.SetBoolean("GADIsAdManagerApp", true);
        //File.WriteAllText(plistPath, plist.WriteToString());

        // Replace with your iOS AdMob app ID. Your AdMob App ID will look
        // similar to this sample ID: ca-app-pub-3940256099942544~1458002511
        
        //Edited: 5/30/2019
        string appId = "ca-app-pub-8954387591640940~2103755403";

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();

        plist.ReadFromFile(plistPath);
        plist.root.SetString("GADApplicationIdentifier", appId);
        File.WriteAllText(plistPath, plist.WriteToString());
#endif




    }
}
