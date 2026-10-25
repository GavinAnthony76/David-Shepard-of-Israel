using System;
using System.Linq;
using UnityEditor;

// Pipeline 0.9.1 can start its listener before Unity 6.5 permits publishing
// the discovery descriptor. Restart on the first settled editor tick.
[InitializeOnLoad]
public static class GameKitEditorConnection
{
    static readonly double readyAfter;
    static GameKitEditorConnection()
    {
        readyAfter=EditorApplication.timeSinceStartup+2;
        EditorApplication.update+=ConnectWhenReady;
    }
    static void ConnectWhenReady()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<readyAfter)return;
        EditorApplication.update-=ConnectWhenReady;
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.EditorPipelineStartup")).FirstOrDefault(t=>t!=null);
        type?.GetMethod("RestartServer",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)?.Invoke(null,null);
        var publisher=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.PipelineDescriptorPublisher")).FirstOrDefault(t=>t!=null);
        publisher?.GetMethod("Start",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)?.Invoke(null,null);
    }
}
