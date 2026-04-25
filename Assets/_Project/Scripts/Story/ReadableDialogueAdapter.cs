using System.Collections;
using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace ExtractionShooter.Story
{
    /// <summary>
    /// Shows <see cref="ReadableDocument"/> text through the Dialogue System using the
    /// <c>Readable_Document_Template</c> conversation (Lua variables <c>readable_title</c> / <c>readable_body</c>).
    /// </summary>
    public class ReadableDialogueAdapter : MonoBehaviour
    {
        public static ReadableDialogueAdapter Instance { get; private set; }

        [SerializeField] private DialogueSystemBridge bridge;

        [SerializeField] private string readableConversationTitle = "Readable_Document_Template";

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Reset()
        {
            bridge = FindFirstObjectByType<DialogueSystemBridge>();
        }

        /// <summary>Opens the Dialogue UI with the document content.</summary>
        public void ShowReadable(ReadableDocument document, Transform actorOverride = null)
        {
            if (document == null) return;
            if (bridge == null)
                bridge = FindFirstObjectByType<DialogueSystemBridge>();
            StartCoroutine(ShowRoutine(document, actorOverride));
        }

        private IEnumerator ShowRoutine(ReadableDocument document, Transform actorOverride)
        {
            if (!DialogueSystemBridge.IsReady)
            {
                Debug.LogWarning("[ReadableDialogueAdapter] Dialogue System not ready; falling back to log.");
                Debug.Log($"[Document] {document.Title}\n{document.PayloadText}");
                yield break;
            }

            DialogueLua.SetVariable("readable_title", document.Title ?? string.Empty);
            DialogueLua.SetVariable("readable_body", document.PayloadText ?? string.Empty);

            var actor = actorOverride != null ? actorOverride : bridge.PlayerTransform;
            var conversant = bridge.RadioSpeakerTransform;
            PixelCrushers.DialogueSystem.DialogueManager.StartConversation(readableConversationTitle, actor, conversant);

            while (PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
                yield return null;
        }
    }
}
