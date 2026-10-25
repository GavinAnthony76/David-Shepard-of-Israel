using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class VerifyNativeMenuClick
{
 public static async Task<string> Run()
 {
  Application.runInBackground=true;await Task.Delay(100);Canvas.ForceUpdateCanvases();
  if(SceneManager.GetActiveScene().name!="Start")throw new Exception("Open native title first.");
  var button=Object.FindObjectsByType<Button>().First(b=>b.name=="StartButton");
  if(!button.IsInteractable())throw new Exception("Begin is disabled.");
  var r=button.GetComponent<RectTransform>();var canvas=button.GetComponentInParent<Canvas>();
  var position=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,r.TransformPoint(r.rect.center));
  var data=new PointerEventData(EventSystem.current){position=position};
  var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
  if(hits.Count==0||hits[0].gameObject.GetComponentInParent<Button>()!=button)
   throw new Exception("Begin hit area blocked by "+(hits.Count==0?"no hit":hits[0].gameObject.name));
  var oldBackground=InputSystem.settings.backgroundBehavior;var oldEditor=InputSystem.settings.editorInputBehaviorInPlayMode;
  var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Keyboard||d is Mouse)).ToArray();
  foreach(var device in physical)InputSystem.DisableDevice(device);
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  var mouse=InputSystem.AddDevice<Mouse>();InputSystem.EnableDevice(mouse);
  try
  {
   InputSystem.QueueStateEvent(mouse,new MouseState{position=position});await Task.Delay(100);
   InputSystem.QueueStateEvent(mouse,new MouseState{position=position,buttons=1});await Task.Delay(100);
   InputSystem.QueueStateEvent(mouse,new MouseState{position=position,buttons=0});
   for(int i=0;i<80;i++){await Task.Delay(100);if(SceneManager.GetActiveScene().name=="David_01_Pastures")return "PASS: Begin is interactable, its center raycast resolves to the native button, and mouse down/up through the Input System opens David's pastures.";}
   throw new Exception("Mouse click did not open pastures.");
  }
  finally{InputSystem.RemoveDevice(mouse);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditor;foreach(var device in physical)InputSystem.EnableDevice(device);}
 }
}
