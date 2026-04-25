using UnityEngine;

namespace ExtractionShooter.Story
{
    [CreateAssetMenu(fileName = "NewMission", menuName = "ExtractionShooter/Story/MissionObjective")]
    public class MissionObjective : ScriptableObject
    {
        [Header("Mission Data")]
        public string ID;
        public string Title;
        [TextArea] public string Description;
        
        [Header("Completion Logic")]
        public MissionType Type;
        public string TargetID; // ItemName, ZoneID, AIArgument, etc.
        public int TargetCount = 1;

        public enum MissionType
        {
            KillAI,
            LootItem,
            VisitZone,
            Extract
        }
    }
}
