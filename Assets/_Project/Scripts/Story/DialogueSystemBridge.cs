using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace ExtractionShooter.Story
{
    /// <summary>
    /// Runtime wiring for Pixel Crushers Dialogue System: ensures a database is assigned,
    /// exposes a small API for conversations/barks, and installs quest broadcast hooks for <see cref="MissionDialogueSync"/>.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class DialogueSystemBridge : MonoBehaviour
    {
        public static DialogueSystemBridge Instance { get; private set; }

        [Header("Dialogue System")]
        [Tooltip("Assigned at runtime before DialogueSystemController initializes (execution order -1000).")]
        [SerializeField] private DialogueDatabase dialogueDatabase;

        [Tooltip("Optional: instantiate this prefab if no DialogueSystemController exists in the scene.")]
        [SerializeField] private GameObject dialogueManagerPrefab;

        [Header("Transforms")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform radioSpeakerTransform;

        private DialogueQuestBroadcastHook _questHook;

        public Transform PlayerTransform => playerTransform != null ? playerTransform : Camera.main != null ? Camera.main.transform : transform;
        public Transform RadioSpeakerTransform => radioSpeakerTransform != null ? radioSpeakerTransform : PlayerTransform;

        private void Awake()
        {
            Instance = this;
            EnsureDialogueManager();
            AssignDatabaseIfNeeded();
            InstallQuestHook();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_questHook != null)
            {
                _questHook.QuestStateChanged -= OnQuestBroadcast;
                if (_questHook.gameObject != null) Destroy(_questHook);
                _questHook = null;
            }
        }

        private void EnsureDialogueManager()
        {
            if (DialogueManager.hasInstance) return;
            if (dialogueManagerPrefab == null)
            {
                Debug.LogWarning("[DialogueSystemBridge] No DialogueSystemController in scene and no dialogueManagerPrefab assigned. Add a Dialogue Manager prefab or assign one here.");
                return;
            }

            var go = Instantiate(dialogueManagerPrefab);
            go.name = dialogueManagerPrefab.name;
            go.SetActive(false);
            var controller = go.GetComponent<DialogueSystemController>();
            if (controller == null)
            {
                Debug.LogError("[DialogueSystemBridge] dialogueManagerPrefab is missing DialogueSystemController.");
                Destroy(go);
                return;
            }

            if (dialogueDatabase != null && controller.initialDatabase == null)
                controller.initialDatabase = dialogueDatabase;

            go.SetActive(true);
        }

        private void AssignDatabaseIfNeeded()
        {
            if (!DialogueManager.hasInstance) return;
            var controller = DialogueManager.instance;
            if (dialogueDatabase == null) return;
            if (controller.initialDatabase == null)
                controller.initialDatabase = dialogueDatabase;
        }

        private void InstallQuestHook()
        {
            if (!DialogueManager.hasInstance) return;
            var host = DialogueManager.instance.gameObject;
            _questHook = host.GetComponent<DialogueQuestBroadcastHook>();
            if (_questHook == null)
                _questHook = host.AddComponent<DialogueQuestBroadcastHook>();
            _questHook.QuestStateChanged -= OnQuestBroadcast;
            _questHook.QuestStateChanged += OnQuestBroadcast;
        }

        private void OnQuestBroadcast(string questName)
        {
            MissionDialogueSync.NotifyQuestStateChanged(questName);
        }

        /// <summary>Starts a named conversation from the master database (player as actor).</summary>
        public void StartConversation(string conversationTitle, Transform actor = null, Transform conversant = null)
        {
            if (!DialogueManager.hasInstance)
            {
                Debug.LogWarning("[DialogueSystemBridge] DialogueManager not ready.");
                return;
            }

            var a = actor != null ? actor : PlayerTransform;
            var c = conversant != null ? conversant : RadioSpeakerTransform;
            DialogueManager.StartConversation(conversationTitle, a, c);
        }

        /// <summary>Plays a one-off bark (e.g. radio line). Uses <see cref="DialogueManager.Bark"/>.</summary>
        public void Bark(string conversationTitle, Transform speaker, Transform listener = null)
        {
            if (!DialogueManager.hasInstance)
            {
                Debug.LogWarning("[DialogueSystemBridge] DialogueManager not ready.");
                return;
            }

            if (listener == null) listener = PlayerTransform;
            DialogueManager.Bark(conversationTitle, speaker, listener);
        }

        /// <summary>True when the Dialogue System is ready for calls.</summary>
        public static bool IsReady => DialogueManager.hasInstance && DialogueManager.masterDatabase != null;
    }
}
