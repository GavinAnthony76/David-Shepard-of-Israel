using UnityEngine;
using UnityEngine.SceneManagement;
using Gamekit2D;

namespace DavidGame
{
    public static class DavidStory
    {
        public const string TitleScene="Start";
        public static readonly string[] Scenes={"David_01_Pastures","David_02_LionRavine","David_03_BearHollow","David_04_TheCalling","David_05_TheErrand","David_06_FiveStones","David_07_Elah"};
        public static readonly string[] Titles={"The Shepherd’s Watch","The Lion’s Ravine","The Bear’s Hollow","The Unexpected Calling","Bread for the Brothers","Five Smooth Stones","The Valley of Elah"};
        public static readonly string[] Places={"BETHLEHEM PASTURES","THE JUDEAN RIDGES","A SHADOWED WILDERNESS","JESSE’S HOUSE","THE ROAD TO THE ARMY","THE BROOK OF ELAH","BEFORE THE PHILISTINE CHAMPION"};
        public static readonly string[] Intros={
            "While others sought great deeds, David watched over his father’s flock. Three sheep have wandered into the terraced hills. A faithful shepherd goes after each one.",
            "A lion has taken a lamb into the ravine. David remembers the flock entrusted to him and follows. Watch the beast’s warning crouch, then move before its charge.",
            "The wilderness brings another test. A bear guards the high path through the hollow. Patience and a steady hand matter more than rushing into danger.",
            "Samuel has come to Jesse’s house. David is still in the fields when he is called. The youngest son is not forgotten. Reach Samuel and hear his message.",
            "Jesse sends David to carry food to his brothers in the army. Gather the three provision baskets along the road. News of a giant’s challenge waits at the camp.",
            "David will not fight in armor he has never tested. At the brook he chooses five smooth stones, places them in his shepherd’s pouch, and takes up his sling.",
            "Two armies watch the valley. Goliath trusts his armor, spear, and strength. David steps forward with a shepherd’s staff and sling. Evade the spear, then strike when the giant lowers his guard."};
        public static readonly string[] Objectives={"Rescue 3 lost sheep","Rescue the lamb from the lion","Defeat the bear and cross the hollow","Speak with Samuel","Collect 3 provision baskets","Choose 5 smooth stones","Defeat Goliath"};
        public static readonly string[] Endings={
            "The flock is safe. Courage begins with caring for what has been entrusted to you.",
            "The lamb is rescued. David will later remember the Lord’s deliverance when he stands before Saul.",
            "The path is clear. David has learned to face danger without abandoning the helpless.",
            "Samuel anoints David. His calling begins here, long before he wears a crown. For now, he returns to faithful service.",
            "The provisions reach the camp. David hears the challenge that has frightened Israel and asks why no one will stand against it.",
            "Five stones rest in the pouch. David approaches the valley carrying the tools he knows, trusting the Lord who delivered him before.",
            "The giant falls. Israel finds courage again. David’s journey from shepherd to king is only beginning."};
        public static readonly string[] References={"1 Samuel 16:11; 17:34–35","1 Samuel 17:34–37","1 Samuel 17:34–37","1 Samuel 16:1–13","1 Samuel 17:17–30","1 Samuel 17:38–40","1 Samuel 17:41–51"};
        public static readonly int[] Required={3,0,0,1,3,5,0};
        public static int Unlocked=>Mathf.Clamp(PlayerPrefs.GetInt("David.Campaign.Unlocked",0),0,6);
        public static int Last=>Mathf.Clamp(PlayerPrefs.GetInt("David.Campaign.Last",0),0,Unlocked);
        public static int Completed=>PlayerPrefs.GetInt("David.Campaign.Completed",0);
        public static void MarkComplete(int chapter)
        {
            PlayerPrefs.SetInt("David.Campaign.Completed",Completed|(1<<chapter));
            PlayerPrefs.SetInt("David.Campaign.Unlocked",Mathf.Max(Unlocked,Mathf.Min(6,chapter+1)));
            PlayerPrefs.SetInt("David.Campaign.Last",Mathf.Min(6,chapter+1));PlayerPrefs.Save();
        }
        public static void Load(int chapter)
        {
            Time.timeScale=1;PlayerPrefs.SetInt("David.Campaign.Last",chapter);PlayerPrefs.Save();
            ResetSceneController();SceneManager.LoadScene(Scenes[chapter]);
        }
        public static void Menu(){Time.timeScale=1;ResetSceneController();SceneManager.LoadScene(TitleScene);}
        static void ResetSceneController()
        {
            PersistentDataManager.ClearPersisters();
            foreach(var controller in Object.FindObjectsByType<SceneController>())Object.DestroyImmediate(controller.gameObject);
        }
    }
}
