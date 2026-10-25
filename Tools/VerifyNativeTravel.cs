using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Gamekit2D;
using DavidGame;
using Object=UnityEngine.Object;
public static class VerifyNativeTravel
{
 static async Task Travel(TransitionPoint point)
 {
  string expected=point.newSceneName;SceneController.TransitionToScene(point);
  for(int n=0;n<150;n++){await Task.Delay(100);if(SceneManager.GetActiveScene().name==expected&&!SceneController.Transitioning)return;}
  throw new Exception("Native transition failed to settle: "+expected);
 }
 static async Task Collect(string key)
 {
  var hero=PlayerCharacter.PlayerInstance;var item=Object.FindObjectsByType<InventoryItem>(FindObjectsInactive.Include).First(p=>p.inventoryKey==key);
  item.gameObject.SetActive(true);Vector2 pos=item.transform.position-Vector3.up*.4f;
  hero.GetComponent<Rigidbody2D>().position=pos;hero.GetComponent<CharacterController2D>().Teleport(pos);Physics2D.SyncTransforms();
  await Task.Delay(500);if(!hero.GetComponent<InventoryController>().HasItem(key))throw new Exception("Pickup failed: "+key);
 }
 public static async Task<string> Run()
 {
  Application.runInBackground=true;
  foreach(var c in Object.FindObjectsByType<SceneController>())Object.DestroyImmediate(c.gameObject);
  PersistentDataManager.ClearPersisters();
  await SceneManager.LoadSceneAsync(DavidStory.Scenes[1]);await Task.Delay(2800);
  var hero=PlayerCharacter.PlayerInstance;var inv=hero.GetComponent<InventoryController>();
  foreach(var k in new[]{"Key1","Key2","Key3"})inv.RemoveItem(k);
  await Collect("Key1");
  await Travel(Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[2]&&t.transitionDestinationTag==SceneTransitionDestination.DestinationTag.A));
  await Collect("Key2");
  await Travel(Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[1]&&t.transitionDestinationTag==SceneTransitionDestination.DestinationTag.C));
  inv=PlayerCharacter.PlayerInstance.GetComponent<InventoryController>();
  if(!inv.HasItem("Key1")||!inv.HasItem("Key2"))throw new Exception("Keys lost during native return travel.");
  await Travel(Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[3]));
  await Collect("Key3");
  await Travel(Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[1]));
  inv=PlayerCharacter.PlayerInstance.GetComponent<InventoryController>();
  var gate=Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[4]);
  if(!gate.inventoryCheck.CheckInventory(inv))throw new Exception("Three-key gate did not unlock.");
  await Travel(gate);
  inv=PlayerCharacter.PlayerInstance.GetComponent<InventoryController>();
  foreach(var k in new[]{"Provision1","Provision2","Provision3"})inv.RemoveItem(k);
  gate=Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[5]);
  if(gate.inventoryCheck.CheckInventory(inv))throw new Exception("Supply gate opens without items.");
  foreach(var k in new[]{"Provision1","Provision2","Provision3"})await Collect(k);
  if(!gate.inventoryCheck.CheckInventory(inv))throw new Exception("Supply gate remained locked.");
  await Travel(gate);
  inv=PlayerCharacter.PlayerInstance.GetComponent<InventoryController>();
  foreach(var k in new[]{"Stone1","Stone2","Stone3","Stone4","Stone5"})inv.RemoveItem(k);
  gate=Object.FindObjectsByType<TransitionPoint>().First(t=>t.newSceneName==DavidStory.Scenes[6]);
  if(gate.inventoryCheck.CheckInventory(inv))throw new Exception("Stone gate opens without items.");
  foreach(var k in new[]{"Stone1","Stone2","Stone3","Stone4","Stone5"})await Collect(k);
  if(!gate.inventoryCheck.CheckInventory(inv))throw new Exception("Stone gate remained locked.");
  if(KeyUI.Instance==null||KeyUI.Instance.keyNames.Length!=5)throw new Exception("Native five-item HUD missing.");
  await Travel(gate);
  return "PASS: physical key pickups, native hollow/cistern return routes, persistent inventory, three-key gate, three-supply gate, five-stone gate and native five-item HUD, final arena reached.";
 }
}
