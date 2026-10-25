using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using Gamekit2D;
using DavidGame;
using TMPro;
using Object = UnityEngine.Object;

public static class DavidNativeCampaignBuilder
{
    const string Root = "Assets/David";
    static readonly int[] Sources = {1,2,3,4,2,1,5};
    static readonly string[] Openings = {
        "David: Father's flock has wandered toward the old paths. I must search carefully. A/D to walk, SPACE to jump. The way onward lies to the right.",
        "David: Three hidden trail keys open the road ahead. One is here; the other paths lead into the high hollow and deep cistern. I can return here after each search.",
        "David: I once followed a lion and a bear for the sake of a lamb. Courage begins with faithfulness. Search the upper hollow for the second trail key, then return to the ravine.",
        "",
        "David: Father has sent food to my brothers. Three glowing supply markers show where the provisions were left. Search the ledges and lower paths, then return to the central passage.",
        "David: I will take the tools I know. Five glowing markers lead to smooth stones for my pouch. Search above and below the main path before leaving to the right.",
        "David: Goliath's challenge has frightened the army. I remember the Lord's deliverance in the wilderness. Watch the guardian's patterns and use the openings it gives you."
    };
    public static object Build()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop play mode before adapting scenes.");
        Directory.CreateDirectory(Root + "/NativePhrases");
        var report = new List<object>();
        for (int i=0;i<7;i++)
        {
            string target=Root+"/Scenes/"+DavidStory.Scenes[i]+".unity";
            File.Copy("Assets/2DGamekit/Scenes/Zone"+Sources[i]+".unity",target,true);
            AssetDatabase.ImportAsset(target,ImportAssetOptions.ForceUpdate);
            var scene=EditorSceneManager.OpenScene(target);
            var hero=Object.FindFirstObjectByType<PlayerCharacter>();
            hero.name="David";
            var story=hero.gameObject.AddComponent<DavidNativeChapter>();
            story.chapter=i;
            story.dialogue=Object.FindFirstObjectByType<DialogueCanvasController>(FindObjectsInactive.Include);
            story.opening=Openings[i];
            story.trackedKeys=i==4?new[]{"Provision1","Provision2","Provision3"}:i==5?new[]{"Stone1","Stone2","Stone3","Stone4","Stone5"}:new[]{"Key1","Key2","Key3"};
            story.itemName=i==4?"supply markers":i==5?"stone markers":"trail keys";
            story.phrases=PreparePhrases(i);
            var inventory=hero.GetComponent<InventoryController>();
            // Reused terrain in later chapters has its own puzzle state. Player inventory stays shared.
            if(i==4||i==5)
                foreach(var p in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).OfType<IDataPersister>())
                    if((p as Component).gameObject!=hero.gameObject){var ds=p.GetDataSettings();p.SetDataSettings("David"+i+"."+ds.dataTag,ds.persistenceType);}
            if(i==4)
            {
                var item=Object.FindObjectsByType<InventoryItem>(FindObjectsInactive.Include).First(p=>p.inventoryKey=="Key1");
                item.inventoryKey="Provision2";
                AddKey("Provision1",new Vector2(3,4),hero);
                AddKey("Provision3",new Vector2(31,3),hero);
            }
            if(i==5)
            {
                // Positions follow the template's authored terrain, with alternate upper/lower routes.
                var hints=new[]{new Vector2(2,4),new Vector2(6,4),new Vector2(10,4),new Vector2(15,4),new Vector2(20,4)};
                for(int n=0;n<hints.Length;n++)AddKey("Stone"+(n+1),hints[n],hero);
            }
            var keyUI=Object.FindFirstObjectByType<KeyUI>(FindObjectsInactive.Include);
            if(keyUI!=null)keyUI.keyNames=story.trackedKeys;
            foreach(var t in Object.FindObjectsByType<TransitionPoint>(FindObjectsInactive.Include))
            {
                if(t.transitionType==TransitionPoint.TransitionType.SameScene)continue;
                string old=t.newSceneName;
                if(i==0)t.newSceneName=DavidStory.Scenes[1];
                else if(i==1)t.newSceneName=old=="Zone1"?DavidStory.Scenes[0]:old=="Zone3"?DavidStory.Scenes[2]:old=="Zone4"?DavidStory.Scenes[3]:DavidStory.Scenes[4];
                else if(i==2||i==3)t.newSceneName=DavidStory.Scenes[1];
                else if(i==4)
                {
                    if(old=="Zone5")
                    {
                        t.newSceneName=DavidStory.Scenes[5];
                        t.transitionDestinationTag=SceneTransitionDestination.DestinationTag.A;
                        t.requiresInventoryCheck=true;
                        t.inventoryCheck.inventoryItems=story.trackedKeys;
                        t.inventoryController=inventory;
                        t.inventoryCheck.OnDoesNotHaveItem=new UnityEvent();
                        UnityEventTools.AddPersistentListener(t.inventoryCheck.OnDoesNotHaveItem,story.GateHint);
                    }
                    else
                    {
                        // Preserve optional exploration branches through the original connected world.
                        t.newSceneName=old=="Zone3"?DavidStory.Scenes[2]:old=="Zone4"?DavidStory.Scenes[3]:DavidStory.Scenes[0];
                    }
                }
                else if(i==5)
                {
                    t.newSceneName=DavidStory.Scenes[6];
                    t.requiresInventoryCheck=true;t.inventoryController=inventory;
                    t.inventoryCheck=new InventoryController.InventoryChecker {inventoryItems=story.trackedKeys,OnHasItem=new UnityEvent(),OnDoesNotHaveItem=new UnityEvent()};
                    UnityEventTools.AddPersistentListener(t.inventoryCheck.OnDoesNotHaveItem,story.GateHint);
                }
                else {t.newSceneName="Start";t.transitionType=TransitionPoint.TransitionType.DifferentNonGameplayScene;}
            }
            RewriteDialogueCalls(i);
            RecordOverrides();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,target);
            report.Add(new{chapter=i+1,scene=target,nativeItems=Object.FindObjectsByType<InventoryItem>(FindObjectsInactive.Include).Select(p=>p.inventoryKey).ToArray(),nativeDialogue=story.dialogue!=null});
        }
        BuildTitle();
        var scenes=new List<EditorBuildSettingsScene>{new EditorBuildSettingsScene(Root+"/Scenes/Start.unity",true)};
        scenes.AddRange(DavidStory.Scenes.Select(s=>new EditorBuildSettingsScene(Root+"/Scenes/"+s+".unity",true)));
        scenes.Add(new EditorBuildSettingsScene("Assets/2DGamekit/Scenes/UI/UIMenus.unity",true));
        scenes.Add(new EditorBuildSettingsScene("Assets/2DGamekit/Scenes/UI/Loading.unity",true));
        EditorBuildSettings.scenes=scenes.ToArray();
        PlayerSettings.productName="David: Shepherd of Israel";PlayerSettings.bundleVersion="0.4.0";
        AssetDatabase.SaveAssets();
        return report;
    }

    static OriginalPhrases PreparePhrases(int chapter)
    {
        string path=Root+"/NativePhrases/Chapter"+(chapter+1)+".asset";
        var p=AssetDatabase.LoadAssetAtPath<OriginalPhrases>(path);
        if(p==null){p=ScriptableObject.CreateInstance<OriginalPhrases>();AssetDatabase.CreateAsset(p,path);}
        p.language="English";
        p.phrases=new List<Phrase>{
            new Phrase{key="INFOPOST1",value=chapter==5?"David: Five smooth stones. I must search every ledge. The glowing markers show my progress in the pouch.":"David: Father trusts me with the flock. I will search the old paths. A and D to walk, SPACE to jump, S to crouch. Look carefully beyond the main trail."},
            new Phrase{key="INFOPOST2",value=chapter==5?"David: The lower path may hide another stone marker. Hold S and press SPACE to drop through this platform.":"David: The flock could have passed beneath this ledge. Hold S and press SPACE to drop through the platform. Hidden paths reward a careful search."},
            new Phrase{key="INFOPOST3",value="David: A shepherd does not abandon the flock. K swings the staff; O uses the ranged weapon. Try the cracked wall, then search the passage beyond."}
        };
        EditorUtility.SetDirty(p);return p;
    }

    static void AddKey(string key,Vector2 hint,PlayerCharacter hero)
    {
        var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/2DGamekit/Prefabs/Interactables/Key.prefab"));
        go.name=key;
        Physics2D.SyncTransforms();
        var hit=Physics2D.RaycastAll(hint,Vector2.down,20,hero.GetComponent<CharacterController2D>().groundedLayerMask).Where(h=>h.collider!=null&&!h.collider.isTrigger&&h.normal.y>.6f).OrderBy(h=>h.distance).FirstOrDefault();
        go.transform.position=hit.collider!=null?hit.point+Vector2.up*.9f:hint;
        var item=go.AddComponent<InventoryItem>();
        item.inventoryKey=key;item.layers=1<<hero.gameObject.layer;item.disableOnEnter=true;
        item.collider=go.GetComponent<CircleCollider2D>();item.collider.isTrigger=true;item.collider.radius=.7f;
        item.SetDataSettings("David.Item."+key,DataSettings.PersistenceType.ReadWrite);
    }

    static void RewriteDialogueCalls(int chapter)
    {
        foreach(var component in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
        {
            if(component==null)continue;
            var so=new SerializedObject(component);var it=so.GetIterator();bool changed=false;
            while(it.Next(true))
            {
                if(it.name!="m_MethodName"||it.propertyType!=SerializedPropertyType.String||it.stringValue!="ActivateCanvasWithText")continue;
                string prefix=it.propertyPath.Substring(0,it.propertyPath.LastIndexOf('.'));
                var argument=so.FindProperty(prefix+".m_Arguments.m_StringArgument");
                if(argument!=null){argument.stringValue=chapter==4?"David: The central passage leads to the brook. Find all three supply markers first, then press E here.":"David: This is the road onward. The three hidden trail keys will open it. Press E when all three symbols are lit.";changed=true;}
            }
            if(changed)so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void BuildTitle()
    {
        string target=Root+"/Scenes/Start.unity";
        File.Copy("Assets/2DGamekit/Scenes/Start.unity",target,true);
        AssetDatabase.ImportAsset(target,ImportAssetOptions.ForceUpdate);
        var scene=EditorSceneManager.OpenScene(target);
        foreach(var t in Object.FindObjectsByType<TransitionPoint>(FindObjectsInactive.Include))t.newSceneName=DavidStory.Scenes[0];
        var logo=Object.FindObjectsByType<Image>(FindObjectsInactive.Include).FirstOrDefault(x=>x.name=="StartLogo");
        foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            if(text.text.StartsWith("This is a free project"))
            {
                text.text="DAVID\n<size=55%>SHEPHERD OF ISRAEL</size>";
                text.raycastTarget=false;
                text.fontSize=88;text.enableAutoSizing=true;text.fontSizeMin=40;text.fontSizeMax=88;
                text.alignment=TextAlignmentOptions.Center;
                var link=text.GetComponentInParent<UnityEngine.UI.Button>();if(link!=null)link.interactable=false;
                if(logo!=null)
                {
                    var r=text.rectTransform;var source=logo.rectTransform;
                    r.SetParent(source.parent,false);r.anchorMin=source.anchorMin;r.anchorMax=source.anchorMax;
                    r.pivot=source.pivot;r.anchoredPosition=source.anchoredPosition;r.sizeDelta=source.sizeDelta;
                    text.color=new Color(1,.9f,.67f);logo.enabled=false;
                }
            }
            if(text.text=="START")text.text="BEGIN";
        }
        RecordOverrides();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,target);
    }

    static void RecordOverrides()
    {
        foreach(var c in Object.FindObjectsByType<Component>(FindObjectsInactive.Include))
            if(c!=null&&PrefabUtility.IsPartOfPrefabInstance(c))
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        foreach(var g in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            if(PrefabUtility.IsPartOfPrefabInstance(g))PrefabUtility.RecordPrefabInstancePropertyModifications(g);
    }
}
