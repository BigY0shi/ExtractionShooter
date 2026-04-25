using UnityEngine;

namespace ExtractionShooter.Story
{
    public class ReadableObject : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private ReadableDocument document;
        [SerializeField] private float interactionRange = 1.5f;

        public ReadableDocument Document => document;
        public float InteractionRange => interactionRange;

        public void Interact()
        {
            if (document == null) return;
            if (ReadableDialogueAdapter.Instance != null)
                ReadableDialogueAdapter.Instance.ShowReadable(document, transform);
            else
                Debug.Log($"Reading: {document.Title}\n{document.PayloadText}");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            Gizmos.DrawIcon(transform.position, "Book", true);
        }
    }
}
