using UnityEngine;

namespace ExtractionShooter.Story
{
    [CreateAssetMenu(fileName = "NewReadable", menuName = "ExtractionShooter/Story/ReadableDocument")]
    public class ReadableDocument : ScriptableObject
    {
        [Header("Document Info")]
        public string Title;
        public string Author; // Optional
        [TextArea(10, 20)] public string PayloadText;

        [Header("Context")]
        public string ZoneID; // Which zone this belongs to
        public bool IsEncrypted; 
        
        [Header("UI")]
        public Sprite BackgroundImage; // Optional paper texture
    }
}
