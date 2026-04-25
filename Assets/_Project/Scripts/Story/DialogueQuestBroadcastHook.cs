using System;
using UnityEngine;

namespace ExtractionShooter.Story
{
    /// <summary>
    /// Receives Unity <c>SendMessage</c> quest notifications from the Dialogue Manager GameObject
    /// (see Pixel Crushers <c>QuestLog</c> / <c>DialogueSystemMessages.OnQuestStateChange</c>).
    /// </summary>
    public class DialogueQuestBroadcastHook : MonoBehaviour
    {
        public event Action<string> QuestStateChanged;

        /// <summary>Called by Unity SendMessage from Dialogue System.</summary>
        public void OnQuestStateChange(string questName)
        {
            QuestStateChanged?.Invoke(questName);
        }
    }
}
