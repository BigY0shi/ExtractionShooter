using System;
using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace ExtractionShooter.Story
{
    /// <summary>
    /// Bidirectional sync between <see cref="MissionManager"/> and Dialogue System quests.
    /// <see cref="MissionManager"/> is authoritative for completion: premature quest success from dialogue is reverted.
    /// </summary>
    public class MissionDialogueSync : MonoBehaviour
    {
        [Serializable]
        public class QuestMissionLink
        {
            [Tooltip("Quest title in the Dialogue Database (Item / quest name).")]
            public string dialogueQuestTitle;

            public MissionObjective mission;
        }

        [SerializeField] private QuestMissionLink[] links = Array.Empty<QuestMissionLink>();

        private bool _suppressQuestCallbacks;

        private void OnEnable() => BindMissionEvents();

        private void Start() => BindMissionEvents();

        private void OnDisable() => UnbindMissionEvents();

        private void BindMissionEvents()
        {
            if (MissionManager.Instance == null) return;
            MissionManager.Instance.OnMissionObjectiveCompleted -= OnMissionGameplayComplete;
            MissionManager.Instance.OnMissionObjectiveCompleted += OnMissionGameplayComplete;
        }

        private void UnbindMissionEvents()
        {
            if (MissionManager.Instance == null) return;
            MissionManager.Instance.OnMissionObjectiveCompleted -= OnMissionGameplayComplete;
        }

        /// <summary>Forwarded from <see cref="DialogueSystemBridge"/> / SendMessage hook.</summary>
        public static void NotifyQuestStateChanged(string questName)
        {
            var sync = FindFirstObjectByType<MissionDialogueSync>();
            sync?.HandleQuestStateChanged(questName);
        }

        private void OnMissionGameplayComplete(MissionObjective mission)
        {
            if (!DialogueSystemBridge.IsReady) return;
            var questTitle = FindQuestTitleForMission(mission);
            if (string.IsNullOrEmpty(questTitle)) return;

            _suppressQuestCallbacks = true;
            try
            {
                QuestLog.SetQuestState(questTitle, QuestState.Success);
            }
            finally
            {
                _suppressQuestCallbacks = false;
            }
        }

        private void HandleQuestStateChanged(string questTitle)
        {
            if (_suppressQuestCallbacks) return;
            if (!DialogueSystemBridge.IsReady) return;
            var mission = FindMissionForQuest(questTitle);
            if (mission == null) return;

            if (QuestLog.IsQuestSuccess(questTitle))
            {
                if (!MissionManager.Instance || !MissionManager.Instance.IsMissionComplete(mission))
                {
                    _suppressQuestCallbacks = true;
                    try
                    {
                        QuestLog.SetQuestState(questTitle, QuestState.Active);
                    }
                    finally
                    {
                        _suppressQuestCallbacks = false;
                    }

                    Debug.LogWarning(
                        $"[MissionDialogueSync] Quest '{questTitle}' was set to success in dialogue before gameplay completed mission '{mission.Title}'. Reverted quest to Active (MissionManager authoritative).");
                }
            }
            else if (QuestLog.IsQuestActive(questTitle))
            {
                MissionManager.Instance?.AddMission(mission);
            }
        }

        private string FindQuestTitleForMission(MissionObjective mission)
        {
            if (mission == null || links == null) return null;
            for (var i = 0; i < links.Length; i++)
            {
                if (links[i].mission == mission && !string.IsNullOrEmpty(links[i].dialogueQuestTitle))
                    return links[i].dialogueQuestTitle;
            }

            return string.IsNullOrEmpty(mission.ID) ? null : mission.ID;
        }

        private MissionObjective FindMissionForQuest(string questTitle)
        {
            if (links != null)
            {
                for (var i = 0; i < links.Length; i++)
                {
                    if (string.Equals(links[i].dialogueQuestTitle, questTitle, StringComparison.Ordinal))
                        return links[i].mission;
                }
            }

            if (MissionManager.Instance == null) return null;
            foreach (var m in MissionManager.Instance.ActiveMissions)
            {
                if (m != null && m.ID == questTitle) return m;
            }

            return null;
        }
    }
}
