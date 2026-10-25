using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Gamekit2D;
using DavidGame;
using Object=UnityEngine.Object;
public static class VerifyRenderingAndPause
{
 static Texture2D Frame(Camera camera)
 {
  var target=RenderTexture.GetTemporary(1280,720,24);var previous=camera.targetTexture;var active=RenderTexture.active;
  try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();return texture;}
  finally{camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);}
 }
 public static async Task<string> Run()
 {
  Application.runInBackground=true;
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play mode required.");
  if(SceneManager.GetActiveScene().name!=DavidStory.Scenes[0])
  {
   foreach(var c in Object.FindObjectsByType<SceneController>())Object.DestroyImmediate(c.gameObject);
   PersistentDataManager.ClearPersisters();await SceneManager.LoadSceneAsync(DavidStory.Scenes[0]);await Task.Delay(2800);
  }
  var hero=PlayerCharacter.PlayerInstance;Gamekit2D.PlayerInput.Instance.ReleaseControl();
  Vector2 pos=new Vector2(26,-1);hero.GetComponent<Rigidbody2D>().position=pos;hero.GetComponent<CharacterController2D>().Teleport(pos);
  await Task.Delay(1500);
  var camera=Object.FindFirstObjectByType<Camera>();
  var spriteMaterials=Resources.FindObjectsOfTypeAll<Material>().Where(m=>m.shader!=null&&m.shader.name.StartsWith("Universal Render Pipeline/2D/")&&m.renderQueue>=2500&&m.HasProperty("_ZWrite")).ToArray();
  if(spriteMaterials.Any(m=>m.GetFloat("_ZWrite")!=0))throw new Exception("Transparent material still writes depth.");
  // Reintroduce only the known converted kit material, not Unity's built-in defaults.
  var regressionMaterial=spriteMaterials.First(m=>m.name=="LitBasic");
  regressionMaterial.SetFloat("_ZWrite",1);
  var broken=Frame(camera);
  DavidRenderingCompatibility.RepairLoadedContent();
  var fixedFrame=Frame(camera);
  int changed=0;var a=broken.GetPixels32();var b=fixedFrame.GetPixels32();
  for(int y=20;y<260;y++)for(int x=850;x<1260;x++){int i=y*1280+x;if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>60)changed++;}
  File.WriteAllBytes(Path.GetFullPath("../outputs/Terrain-rendering-fixed.png"),fixedFrame.EncodeToPNG());
  File.WriteAllBytes(Path.GetFullPath("../work/Terrain-depth-regression.png"),broken.EncodeToPNG());
  Object.DestroyImmediate(broken);Object.DestroyImmediate(fixedFrame);
  if(changed<15000)throw new Exception("Expected terrain restoration was not visible: "+changed);
  var story=hero.GetComponent<DavidNativeChapter>();story.Say("David: I will search the hidden paths.",6);
  await Task.Delay(1000);
  if(story.dialogue.textMeshProUGUI.textInfo.characterCount<10||!story.dialogue.textMeshProUGUI.isActiveAndEnabled)throw new Exception("Native dialogue text did not render.");
  if(Object.FindObjectsByType<Image>(FindObjectsInactive.Include).Any(g=>g.enabled&&g.material!=null&&g.material.shader.name=="Unlit/FrostedGlass"))throw new Exception("Unsupported blur still enabled.");
  var oldBackground=InputSystem.settings.backgroundBehavior;var oldEditor=InputSystem.settings.editorInputBehaviorInPlayMode;
  var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Keyboard||d is Mouse)).ToArray();foreach(var device in physical)InputSystem.DisableDevice(device);
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(keyboard);
  try
  {
   Gamekit2D.PlayerInput.Instance.GainControl();
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));await Task.Delay(150);
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(500);
   if(Time.timeScale!=0||!SceneManager.GetSceneByName("UIMenus").isLoaded)throw new Exception("Escape did not open native pause menu.");
   if(Object.FindObjectsByType<Image>(FindObjectsInactive.Include).Any(g=>g.enabled&&g.material!=null&&g.material.shader.name=="Unlit/FrostedGlass"))throw new Exception("Pause loaded unsupported blur.");
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));await Task.Delay(150);
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(500);
   if(Time.timeScale!=1||!Gamekit2D.PlayerInput.Instance.HaveControl)throw new Exception("Resume did not restore gameplay.");
  }
  finally{InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditor;foreach(var device in physical)InputSystem.EnableDevice(device);}
  return "PASS: terrain restored in "+changed+" regression pixels; transparent materials corrected; native dialogue renders; unsupported blur disabled; Escape pauses and resumes.";
 }
}
