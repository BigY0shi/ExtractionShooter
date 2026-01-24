using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ExtractionShooter.UI
{
    /// <summary>
    /// Helper script to create main menu UI programmatically
    /// Phase 1: Basic main menu
    /// </summary>
    public class MainMenuSetupHelper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool createMenuOnStart = true;
        
        private void Start()
        {
            if (createMenuOnStart)
            {
                CreateMainMenu();
            }
        }
        
        /// <summary>
        /// Create main menu programmatically
        /// </summary>
        [ContextMenu("Create Main Menu")]
        public void CreateMainMenu()
        {
            // Find or create canvas
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("MenuCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create menu panel
            GameObject menuPanel = CreatePanel("MainMenuPanel", canvas.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 600));
            menuPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            // Add MainMenuController
            MainMenuController menuController = menuPanel.AddComponent<MainMenuController>();
            
            // Title
            CreateText("Title", menuPanel.transform, new Vector2(0, 220), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(450, 80), "EXTRACTION SHOOTER", 56, TextAlignmentOptions.Center);
            
            // Subtitle
            CreateText("Subtitle", menuPanel.transform, new Vector2(0, 160), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(450, 40), "Prototype", 24, TextAlignmentOptions.Center);
            
            // Start Raid button
            GameObject startBtn = CreateButton("StartButton", menuPanel.transform, new Vector2(0, 40), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350, 70), "START RAID");
            startBtn.GetComponent<Button>().onClick.AddListener(menuController.OnStartRaidClicked);
            
            // Settings button
            GameObject settingsBtn = CreateButton("SettingsButton", menuPanel.transform, new Vector2(0, -50), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350, 70), "SETTINGS");
            settingsBtn.GetComponent<Button>().onClick.AddListener(menuController.OnSettingsClicked);
            
            // Quit button
            GameObject quitBtn = CreateButton("QuitButton", menuPanel.transform, new Vector2(0, -140), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350, 70), "QUIT");
            quitBtn.GetComponent<Button>().onClick.AddListener(menuController.OnQuitClicked);
            
            Debug.Log("Main menu created!");
        }
        
        // === HELPER METHODS ===
        
        private GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = sizeDelta;
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            return panel;
        }
        
        private GameObject CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = sizeDelta;
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return obj;
        }
        
        private GameObject CreateButton(string name, Transform parent, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, string text)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = sizeDelta;
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button btn = obj.AddComponent<Button>();
            
            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return obj;
        }
    }
}


