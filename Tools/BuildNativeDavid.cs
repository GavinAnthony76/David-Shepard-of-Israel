using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildNativeDavid
{
 public static object Run()
 {
  if(EditorApplication.isPlaying)throw new Exception("Stop play mode first.");
  string folder=Path.GetFullPath("../outputs/David-Template-Edition");
  Directory.CreateDirectory(folder);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
   scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
   locationPathName=Path.Combine(folder,"David-Shepherd-of-Israel.exe"),
   target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
  });
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build "+report.summary.result+", errors "+report.summary.totalErrors);
  return new{result=report.summary.result.ToString(),report.summary.totalErrors,report.summary.totalWarnings,report.summary.totalSize,path=folder};
 }
}
