using System;
using UnityEngine;
using System.Collections.Generic;
using ExtractionShooter.Core;

namespace ExtractionShooter.Story
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        /// <summary>Fired once when a mission reaches its <see cref="MissionObjective.TargetCount"/>.</summary>
        public event Action<MissionObjective> OnMissionObjectiveCompleted;

        [SerializeField] private List<MissionObjective> activeMissions = new List<MissionObjective>();
        private readonly Dictionary<string, int> progressMap = new Dictionary<string, int>();
        private readonly HashSet<string> completedMissionIds = new HashSet<string>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            MatchEvents.OnItemPickedUp += OnItemLooted;
            MatchEvents.OnExtractionSuccess += OnExtracted;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            MatchEvents.OnItemPickedUp -= OnItemLooted;
            MatchEvents.OnExtractionSuccess -= OnExtracted;
        }

        public void AddMission(MissionObjective mission)
        {
            if (mission == null) return;
            if (!activeMissions.Contains(mission))
            {
                activeMissions.Add(mission);
                if (!progressMap.ContainsKey(mission.ID))
                    progressMap[mission.ID] = 0;
            }
        }

        public bool IsMissionComplete(MissionObjective mission)
        {
            if (mission == null) return false;
            return completedMissionIds.Contains(mission.ID);
        }

        public int GetProgress(MissionObjective mission)
        {
            if (mission == null) return 0;
            return progressMap.TryGetValue(mission.ID, out var p) ? p : 0;
        }

        public IReadOnlyList<MissionObjective> ActiveMissions => activeMissions;

        /// <summary>
        /// Dialogue System may request completion; gameplay remains authoritative:
        /// returns true only if the mission was already marked complete here.
        /// </summary>
        public bool TryApplyDialogueCompletion(MissionObjective mission)
        {
            return mission != null && IsMissionComplete(mission);
        }

        /// <summary>Forces progress to target (e.g. after verified dialogue sync).</summary>
        public void ForceCompleteMission(MissionObjective mission)
        {
            if (mission == null) return;
            if (!activeMissions.Contains(mission))
                AddMission(mission);
            progressMap[mission.ID] = mission.TargetCount;
            MarkComplete(mission, "forced");
        }

        private void OnItemLooted(string itemName)
        {
            CheckProgress(MissionObjective.MissionType.LootItem, itemName);
        }

        private void OnExtracted()
        {
            CheckProgress(MissionObjective.MissionType.Extract, "Any");
        }

        private void CheckProgress(MissionObjective.MissionType type, string targetPayload)
        {
            foreach (var mission in activeMissions)
            {
                if (mission.Type != type || !(mission.TargetID == targetPayload || mission.TargetID == "Any"))
                    continue;
                if (!progressMap.ContainsKey(mission.ID))
                    progressMap[mission.ID] = 0;
                progressMap[mission.ID]++;
                if (progressMap[mission.ID] >= mission.TargetCount)
                    MarkComplete(mission, "gameplay");
            }
        }

        private void MarkComplete(MissionObjective mission, string source)
        {
            if (!completedMissionIds.Add(mission.ID)) return;
            Debug.Log($"MISSION COMPLETE ({source}): {mission.Title}");
            OnMissionObjectiveCompleted?.Invoke(mission);
        }
    }
}
