using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Gamekit2D;
using DavidGame;
using Object=UnityEngine.Object;
public static class VerifyNativeCampaign
{
 public static async Task<object> Run()
 {
  Application.runInBackground=true;
  var results=new System.Collections.Generic.List<object>();
  var oldBackground=InputSystem.settings.backgroundBehavior;
  var oldEditor=InputSystem.settings.editorInputBehaviorInPlayMode;
  var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Keyboard||d is Mouse)).ToArray();
  foreach(var device in physical)InputSystem.DisableDevice(device);
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(keyboard);
  try
  {
   if(SceneManager.GetActiveScene().name!="Start"&&SceneManager.GetActiveScene().name!=DavidStory.Scenes[0])
   {
    foreach(var c in Object.FindObjectsByType<SceneController>())Object.DestroyImmediate(c.gameObject);
    PersistentDataManager.ClearPersisters();await SceneManager.LoadSceneAsync(DavidStory.Scenes[0]);await Task.Delay(2800);
   }
   if(SceneManager.GetActiveScene().name=="Start")
   {
    var button=Object.FindObjectsByType<UnityEngine.UI.Button>().First(b=>b.name=="StartButton");
    button.onClick.Invoke();
    await Task.Delay(3000);
   }
   if(SceneManager.GetActiveScene().name!=DavidStory.Scenes[0])throw new Exception("Native Start button did not load David's pastures.");
   var hero=PlayerCharacter.PlayerInstance;
   if(hero==null||!Gamekit2D.PlayerInput.Instance.HaveControl)throw new Exception("Native player has no control.");
   Vector3 start=hero.transform.position;
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));await Task.Delay(300);
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(100);
   if(hero.transform.position.x<start.x+.2f)throw new Exception("Move right failed.");
   float right=hero.transform.position.x;
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));await Task.Delay(300);
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(100);
   if(hero.transform.position.x>right-.2f)throw new Exception("Move left failed.");
   float y=hero.transform.position.y;
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));await Task.Delay(170);
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());
   if(hero.transform.position.y<y+.2f)throw new Exception("Jump failed.");
   results.Add(new{check="Native Start, left/right and jump",pass=true});
   for(int i=0;i<7;i++)
   {
    if(i>0){foreach(var c in Object.FindObjectsByType<SceneController>())Object.DestroyImmediate(c.gameObject);PersistentDataManager.ClearPersisters();await SceneManager.LoadSceneAsync(DavidStory.Scenes[i]);await Task.Delay(2800);}
    hero=PlayerCharacter.PlayerInstance;
    var story=hero.GetComponent<DavidNativeChapter>();
    if(story==null||story.chapter!=i)throw new Exception("Missing native story chapter "+i);
    if(Resources.FindObjectsOfTypeAll<Material>().Any(m=>m.shader!=null&&m.shader.name.StartsWith("Universal Render Pipeline/2D/")&&m.renderQueue>=2500&&m.HasProperty("_ZWrite")&&m.GetFloat("_ZWrite")!=0))throw new Exception("Transparent material depth regression in chapter "+i);
    if(Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).Any(c=>c!=null&&new[]{"DavidChapter","DavidPickup","DavidMenu","DavidCostume","DavidWildlife"}.Contains(c.GetType().Name)))throw new Exception("Retired addition in scene "+i);
    foreach(var t in Object.FindObjectsByType<TransitionPoint>(FindObjectsInactive.Include))
     if(t.transitionType!=TransitionPoint.TransitionType.SameScene&&t.newSceneName!="Start"&&!DavidStory.Scenes.Contains(t.newSceneName))throw new Exception("Unmapped native exit "+t.newSceneName);
    if(story.dialogue!=null){story.Say("David: I will search the hidden paths.",8);if(story.dialogue.textMeshProUGUI.text!="David: I will search the hidden paths.")throw new Exception("Native dialogue rewrite failed.");}
    var inv=hero.GetComponent<InventoryController>();
    var items=Object.FindObjectsByType<InventoryItem>(FindObjectsInactive.Include).Where(p=>p.inventoryKey.StartsWith("Key")||p.inventoryKey.StartsWith("Provision")||p.inventoryKey.StartsWith("Stone")).ToArray();
    foreach(var item in items)
    {
     inv.RemoveItem(item.inventoryKey);item.gameObject.SetActive(true);
     Vector2 destination=item.transform.position-Vector3.up*.4f;
     hero.GetComponent<Rigidbody2D>().position=destination;
     hero.GetComponent<CharacterController2D>().Teleport(destination);
     Physics2D.SyncTransforms();
     await Task.Delay(500);
     if(!inv.HasItem(item.inventoryKey))throw new Exception("Physical pickup failed "+item.inventoryKey+" in "+i);
    }
    results.Add(new{chapter=i+1,pass=true,pickups=items.Select(p=>p.inventoryKey).ToArray(),dialogue=story.dialogue!=null});
   }
   return results;
  }
  finally{InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditor;foreach(var device in physical)InputSystem.EnableDevice(device);}
 }
}
