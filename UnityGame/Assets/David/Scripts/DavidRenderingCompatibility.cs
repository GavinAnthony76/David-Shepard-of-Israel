using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DavidGame
{
    public static class DavidRenderingCompatibility
    {
        static readonly Dictionary<Material, float> OriginalDepth = new Dictionary<Material, float>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            RestoreMaterials();
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
#endif
        }

        static void SceneLoaded(Scene scene, LoadSceneMode mode) => RepairLoadedContent();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void RepairLoadedContent()
        {
            // The imported kit retains _ZWrite=1 from its old materials after shader conversion.
            // Transparent pixels then occlude later sprites, including rectangular regions of terrain.
            // Fix loaded shared materials so prefab pools inherit the same correct state.
            foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (material.shader == null || !material.shader.name.StartsWith("Universal Render Pipeline/2D/")
                    || material.renderQueue < 2500 || !material.HasProperty("_ZWrite")) continue;
                float depth = material.GetFloat("_ZWrite");
                if (depth == 0) continue;
                if (!OriginalDepth.ContainsKey(material)) OriginalDepth.Add(material, depth);
                material.SetFloat("_ZWrite", 0);
            }

            // GrabPass-based blur is unsupported by URP. Keep the native panel, portrait and text;
            // disable only its old blur image, including in the additively loaded pause UI.
            // Only Images can be the blur panel. Reading TMP_SubMeshUI.material during scene
            // startup can instantiate a font material before its shared material is assigned.
            foreach (var graphic in Object.FindObjectsByType<Image>(FindObjectsInactive.Include))
                if (graphic.material != null && graphic.material.shader != null
                    && graphic.material.shader.name == "Unlit/FrostedGlass")
                    graphic.enabled = false;
        }

        static void RestoreMaterials()
        {
            // Play-mode fixes must not alter the user's imported Store assets in the editor.
            foreach (var pair in OriginalDepth)
                if (pair.Key != null) pair.Key.SetFloat("_ZWrite", pair.Value);
            OriginalDepth.Clear();
        }

#if UNITY_EDITOR
        static void PlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                RestoreMaterials();
        }
#endif
    }
}
