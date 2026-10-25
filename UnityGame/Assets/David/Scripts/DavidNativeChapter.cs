using System.Collections;
using System.Linq;
using UnityEngine;
using Gamekit2D;

namespace DavidGame
{
    // Story and inventory feedback share the template's existing dialogue canvas.
    // Movement, combat, checkpoints, puzzles, HUD and transitions stay in Gamekit2D.
    public class DavidNativeChapter : MonoBehaviour
    {
        public int chapter;
        public OriginalPhrases phrases;
        public DialogueCanvasController dialogue;
        public string opening;
        public string[] trackedKeys;
        public string itemName = "trail key";
        InventoryController inventory;
        int previousCount;

        void Awake()
        {
            if (phrases != null)
            {
                // Some kit scenes rely on a Translator in another scene; support direct scene play too.
                var translator = FindFirstObjectByType<Translator>();
                if (translator == null) translator = gameObject.AddComponent<Translator>();
                translator.phrases = new System.Collections.Generic.List<OriginalPhrases> { phrases };
                Translator.SetLanguage(0);
            }
            inventory = GetComponent<InventoryController>();
            var keyUI = FindFirstObjectByType<KeyUI>(FindObjectsInactive.Include);
            if (keyUI != null && trackedKeys != null && trackedKeys.Length > 0)
                keyUI.keyNames = trackedKeys;
        }

        IEnumerator Start()
        {
            yield return null;
            previousCount = Count;
            RefreshInventory();
            yield return new WaitForSeconds(2);
            if (dialogue != null && !string.IsNullOrEmpty(opening))
            {
                string message = opening;
                if (chapter == 1 && inventory != null && inventory.HasItem("Key3"))
                    message = "David: Samuel called me from the fields and anointed me. For now I serve my father. With all three trail keys, I can take his provisions toward my brothers.";
                Say(message, 9);
            }
        }

        int Count => inventory == null || trackedKeys == null ? 0 : trackedKeys.Count(inventory.HasItem);

        void Update()
        {
            int count = Count;
            if (count == previousCount) return;
            if (count > previousCount)
            {
                string end = count == trackedKeys.Length ? " All found. Return to the marked passage." : " Search the side paths for the rest.";
                Say("David: " + count + "/" + trackedKeys.Length + " " + itemName + " found." + end, 6);
            }
            previousCount = count;
            RefreshInventory();
        }

        void RefreshInventory()
        {
            if (inventory != null && KeyUI.Instance != null)
                KeyUI.Instance.ChangeKeyUI(inventory);
        }

        public void Say(string text, float seconds = 6)
        {
            if (dialogue == null) return;
            dialogue.ActivateCanvasWithText(text);
            dialogue.DeactivateCanvasWithDelay(seconds);
        }

        public void GateHint()
        {
            Say("David: This passage needs all " + trackedKeys.Length + " " + itemName + ". Look beyond the main path, and return here when the symbols are lit.");
        }
    }
}
