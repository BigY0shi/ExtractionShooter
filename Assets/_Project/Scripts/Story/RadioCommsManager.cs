using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtractionShooter.Core;

namespace ExtractionShooter.Story
{
    /// <summary>
    /// Queues radio bark lines (Dialogue System conversations) driven by <see cref="MatchEvents"/>.
    /// </summary>
    public class RadioCommsManager : MonoBehaviour
    {
        [SerializeField] private DialogueSystemBridge bridge;

        [Tooltip("Conversation titled like this in the prototype database (bark lines).")]
        [SerializeField] private string radioBarkConversation = "Radio_Barks";

        [Tooltip("Conversation used for non-story patrol/chill NPC chatter.")]
        [SerializeField] private string ambientBarkConversation = "NPC_Ambient_Chatter";

        [Header("Ambient Chatter")]
        [SerializeField] private bool enableAmbientChatter = true;
        [SerializeField] private float ambientMinIntervalSeconds = 28f;
        [SerializeField] private float ambientMaxIntervalSeconds = 52f;
        [SerializeField] private float ambientCombatCooldownSeconds = 24f;

        [Header("Combat Chatter")]
        [SerializeField] private float enemyContactMinIntervalSeconds = 7f;

        [SerializeField] private float postBarkPauseSeconds = 4f;
        [SerializeField] private int maxQueuedLines = 12;

        private readonly Queue<string> _pendingReasons = new Queue<string>();
        private Coroutine _runner;
        private bool _linePlaying;
        private bool _matchActive;
        private float _nextAmbientAt;
        private float _lastCombatSignalAt = float.NegativeInfinity;
        private float _lastEnemyContactAt = float.NegativeInfinity;
        private string _lastQueuedConversation;

        private void Reset()
        {
            bridge = FindFirstObjectByType<DialogueSystemBridge>();
        }

        private void OnEnable()
        {
            MatchEvents.OnMatchStart += OnMatchStart;
            MatchEvents.OnExtractionStart += OnExtractionStart;
            MatchEvents.OnPlayerSpotted += OnPlayerSpotted;
            MatchEvents.OnAIDeath += OnAIDeath;
        }

        private void OnDisable()
        {
            MatchEvents.OnMatchStart -= OnMatchStart;
            MatchEvents.OnExtractionStart -= OnExtractionStart;
            MatchEvents.OnPlayerSpotted -= OnPlayerSpotted;
            MatchEvents.OnAIDeath -= OnAIDeath;
            if (_runner != null)
            {
                StopCoroutine(_runner);
                _runner = null;
            }
        }

        private void Update()
        {
            if (!enableAmbientChatter || !_matchActive) return;
            if (Time.time < _nextAmbientAt) return;
            if (Time.time - _lastCombatSignalAt < ambientCombatCooldownSeconds) return;
            if (_pendingReasons.Count > 0 || _linePlaying) return;
            if (string.IsNullOrWhiteSpace(ambientBarkConversation)) return;

            Enqueue(ambientBarkConversation);
            ScheduleNextAmbient();
        }

        private void OnMatchStart()
        {
            _matchActive = true;
            Enqueue(radioBarkConversation);
            ScheduleNextAmbient();
        }

        private void OnExtractionStart()
        {
            _matchActive = false;
            Enqueue(radioBarkConversation);
        }

        private void OnPlayerSpotted(Vector3 _)
        {
            _lastCombatSignalAt = Time.time;
            if (Time.time - _lastEnemyContactAt < Mathf.Max(0.1f, enemyContactMinIntervalSeconds)) return;
            _lastEnemyContactAt = Time.time;
            Enqueue(radioBarkConversation);
        }

        private void OnAIDeath(Vector3 _)
        {
            _lastCombatSignalAt = Time.time;
        }

        private void ScheduleNextAmbient()
        {
            var minInterval = Mathf.Max(1f, ambientMinIntervalSeconds);
            var maxInterval = Mathf.Max(minInterval, ambientMaxIntervalSeconds);
            _nextAmbientAt = Time.time + Random.Range(minInterval, maxInterval);
        }

        private void Enqueue(string conversationTitle)
        {
            if (string.IsNullOrWhiteSpace(conversationTitle)) return;
            if (maxQueuedLines > 0 && _pendingReasons.Count >= maxQueuedLines) return;
            if (_pendingReasons.Count > 0 && _lastQueuedConversation == conversationTitle) return;
            _pendingReasons.Enqueue(conversationTitle);
            _lastQueuedConversation = conversationTitle;
            if (_runner == null && isActiveAndEnabled)
                _runner = StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            while (_pendingReasons.Count > 0 || _linePlaying)
            {
                if (_pendingReasons.Count == 0)
                {
                    yield return null;
                    continue;
                }

                var conversationTitle = _pendingReasons.Peek();
                if (bridge == null)
                    bridge = FindFirstObjectByType<DialogueSystemBridge>();

                if (bridge == null || !DialogueSystemBridge.IsReady)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                // Avoid stomping over other conversations (story/readable/etc.).
                if (PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
                {
                    yield return new WaitForSeconds(0.2f);
                    continue;
                }

                _linePlaying = true;
                var speaker = bridge.RadioSpeakerTransform;
                var listener = bridge.PlayerTransform;
                bridge.Bark(conversationTitle, speaker, listener);
                _pendingReasons.Dequeue();
                _lastQueuedConversation = null;

                // Barks do not always toggle isConversationActive; use a short pause so lines do not stomp each other.
                yield return new WaitForSeconds(Mathf.Max(0.1f, postBarkPauseSeconds));

                _linePlaying = false;
            }

            _runner = null;
        }
    }
}
