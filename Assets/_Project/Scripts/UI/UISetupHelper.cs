using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ExtractionShooter.UI
{
    /// <summary>
    /// Helper script to programmatically create UI elements for the game
    /// This is useful for rapid prototyping without needing to manually create prefabs
    /// Phase 1: Core HUD setup
    /// </summary>
    public class UISetupHelper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool createHUDOnStart = true;
        [SerializeField] private bool createPauseMenuOnStart = true;
        
        [Header("References")]
        [SerializeField] private UIManager uiManager;
        
        private void Start()
        {
            if (createHUDOnStart)
            {
                CreateHUD();
            }
            
            if (createPauseMenuOnStart)
            {
                CreatePauseMenu();
            }
        }
        
        /// <summary>
        /// Create the HUD programmatically
        /// </summary>
        [ContextMenu("Create HUD")]
        public void CreateHUD()
        {
            // Find or create canvas
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create HUD container
            GameObject hudContainer = new GameObject("HUD_Container");
            hudContainer.transform.SetParent(canvas.transform, false);
            RectTransform hudRect = hudContainer.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.sizeDelta = Vector2.zero;
            
            // Create HUD Manager component
            HUDManager hudManager = hudContainer.AddComponent<HUDManager>();
            
            // === TOP LEFT: Health & Armor ===
            GameObject healthPanel = CreatePanel("HealthPanel", hudContainer.transform, new Vector2(10, -10), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 80));
            
            // Health bar background
            GameObject healthBG = CreateImage("HealthBar_BG", healthPanel.transform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(280, 30), new Color(0.2f, 0.2f, 0.2f, 0.8f));
            
            // Health bar fill
            GameObject healthFill = CreateImage("HealthBar_Fill", healthBG.transform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(280, 30), new Color(0, 1, 0, 0.8f));
            Image healthImage = healthFill.GetComponent<Image>();
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;
            
            // Armor bar background
            GameObject armorBG = CreateImage("ArmorBar_BG", healthPanel.transform, new Vector2(0, -40), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(280, 25), new Color(0.2f, 0.2f, 0.2f, 0.8f));
            
            // Armor bar fill
            GameObject armorFill = CreateImage("ArmorBar_Fill", armorBG.transform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(280, 25), new Color(0.5f, 0.5f, 1f, 0.8f));
            Image armorImage = armorFill.GetComponent<Image>();
            armorImage.type = Image.Type.Filled;
            armorImage.fillMethod = Image.FillMethod.Horizontal;
            
            // === TOP RIGHT: Timer ===
            GameObject timerText = CreateText("TimerText", hudContainer.transform, new Vector2(-10, -10), new Vector2(1, 1), new Vector2(1, 1), new Vector2(150, 60), "15:00", 36, TextAlignmentOptions.TopRight);
            
            // === BOTTOM RIGHT: Ammo ===
            GameObject ammoPanel = CreatePanel("AmmoPanel", hudContainer.transform, new Vector2(-10, 10), new Vector2(1, 0), new Vector2(1, 0), new Vector2(200, 80));
            GameObject ammoText = CreateText("AmmoText", ammoPanel.transform, new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), "30/30", 48, TextAlignmentOptions.Center);
            
            // === BOTTOM LEFT: Weapon Name ===
            GameObject weaponText = CreateText("WeaponNameText", hudContainer.transform, new Vector2(10, 10), new Vector2(0, 0), new Vector2(0, 0), new Vector2(300, 60), "M4 Rifle", 24, TextAlignmentOptions.BottomLeft);
            
            // === CENTER: Crosshair ===
            GameObject crosshair = CreateCrosshair("Crosshair", hudContainer.transform);
            
            // === DAMAGE INDICATOR ===
            GameObject damageIndicator = CreateImage("DamageIndicator", hudContainer.transform, Vector2.zero, Vector2.zero, Vector2.one, Vector2.zero, new Color(1, 0, 0, 0.3f));
            damageIndicator.GetComponent<Image>().enabled = false;
            
            // === INTERACTION PROMPT ===
            GameObject promptText = CreateText("InteractionPrompt", hudContainer.transform, new Vector2(0, 100), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(600, 50), "Press [E] to interact", 24, TextAlignmentOptions.Center);
            promptText.SetActive(false);
            
            // Wire up HUD Manager references using reflection or manual assignment
            // For now, we'll log that manual assignment is needed
            Debug.Log("HUD created! Please manually assign references in HUDManager component.");
            Debug.Log("References needed: healthBar, armorBar, ammoText, timerText, weaponNameText, crosshair, damageIndicator, interactionPromptText");
            
            // Try to auto-assign what we can
            var healthBarField = typeof(HUDManager).GetField("healthBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var armorBarField = typeof(HUDManager).GetField("armorBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var ammoTextField = typeof(HUDManager).GetField("ammoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timerTextField = typeof(HUDManager).GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weaponNameTextField = typeof(HUDManager).GetField("weaponNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var crosshairField = typeof(HUDManager).GetField("crosshair", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var damageIndicatorField = typeof(HUDManager).GetField("damageIndicator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var interactionPromptTextField = typeof(HUDManager).GetField("interactionPromptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hudContainerField = typeof(HUDManager).GetField("hudContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (healthBarField != null) healthBarField.SetValue(hudManager, healthImage);
            if (armorBarField != null) armorBarField.SetValue(hudManager, armorImage);
            if (ammoTextField != null) ammoTextField.SetValue(hudManager, ammoText.GetComponent<TextMeshProUGUI>());
            if (timerTextField != null) timerTextField.SetValue(hudManager, timerText.GetComponent<TextMeshProUGUI>());
            if (weaponNameTextField != null) weaponNameTextField.SetValue(hudManager, weaponText.GetComponent<TextMeshProUGUI>());
            if (crosshairField != null) crosshairField.SetValue(hudManager, crosshair.GetComponent<RectTransform>());
            if (damageIndicatorField != null) damageIndicatorField.SetValue(hudManager, damageIndicator.GetComponent<Image>());
            if (interactionPromptTextField != null) interactionPromptTextField.SetValue(hudManager, promptText.GetComponent<TextMeshProUGUI>());
            if (hudContainerField != null) hudContainerField.SetValue(hudManager, hudContainer);
            
            Debug.Log("HUD references auto-assigned via reflection!");
            
            // Wire up UIManager if available
            if (uiManager != null)
            {
                var hudManagerField = typeof(UIManager).GetField("hudManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (hudManagerField != null)
                {
                    hudManagerField.SetValue(uiManager, hudManager);
                    Debug.Log("HUDManager linked to UIManager!");
                }
            }
        }
        
        /// <summary>
        /// Create pause menu programmatically
        /// </summary>
        [ContextMenu("Create Pause Menu")]
        public void CreatePauseMenu()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No canvas found! Create HUD first.");
                return;
            }
            
            // Create pause menu panel
            GameObject pausePanel = CreatePanel("PauseMenu", canvas.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 500));
            pausePanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            pausePanel.SetActive(false);
            
            // Add PauseMenuController
            PauseMenuController pauseController = pausePanel.AddComponent<PauseMenuController>();
            
            // Title
            CreateText("Title", pausePanel.transform, new Vector2(0, 180), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350, 60), "PAUSED", 48, TextAlignmentOptions.Center);
            
            // Resume button
            GameObject resumeBtn = CreateButton("ResumeButton", pausePanel.transform, new Vector2(0, 60), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 60), "Resume");
            resumeBtn.GetComponent<Button>().onClick.AddListener(pauseController.OnResumeClicked);
            
            // Settings button
            GameObject settingsBtn = CreateButton("SettingsButton", pausePanel.transform, new Vector2(0, -20), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 60), "Settings");
            settingsBtn.GetComponent<Button>().onClick.AddListener(pauseController.OnSettingsClicked);
            
            // Quit button
            GameObject quitBtn = CreateButton("QuitButton", pausePanel.transform, new Vector2(0, -100), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 60), "Quit to Menu");
            quitBtn.GetComponent<Button>().onClick.AddListener(pauseController.OnQuitToMenuClicked);
            
            Debug.Log("Pause menu created!");
            
            // Wire up UIManager
            if (uiManager != null)
            {
                var pauseMenuField = typeof(UIManager).GetField("pauseMenuPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (pauseMenuField != null)
                {
                    pauseMenuField.SetValue(uiManager, pausePanel);
                    Debug.Log("Pause menu linked to UIManager!");
                }
            }
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
            return panel;
        }
        
        private GameObject CreateImage(string name, Transform parent, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Color color)
        {
            GameObject obj = CreatePanel(name, parent, anchoredPos, anchorMin, anchorMax, sizeDelta);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }
        
        private GameObject CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject obj = CreatePanel(name, parent, anchoredPos, anchorMin, anchorMax, sizeDelta);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return obj;
        }
        
        private GameObject CreateButton(string name, Transform parent, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, string text)
        {
            GameObject obj = CreatePanel(name, parent, anchoredPos, anchorMin, anchorMax, sizeDelta);
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
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
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return obj;
        }
        
        private GameObject CreateCrosshair(string name, Transform parent)
        {
            GameObject crosshair = CreatePanel(name, parent, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 40));
            
            // Center dot
            GameObject dot = CreateImage("Dot", crosshair.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(4, 4), Color.white);
            
            // Top line
            CreateImage("LineTop", crosshair.transform, new Vector2(0, 10), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(2, 8), Color.white);
            
            // Bottom line
            CreateImage("LineBottom", crosshair.transform, new Vector2(0, -10), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(2, 8), Color.white);
            
            // Left line
            CreateImage("LineLeft", crosshair.transform, new Vector2(-10, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8, 2), Color.white);
            
            // Right line
            CreateImage("LineRight", crosshair.transform, new Vector2(10, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8, 2), Color.white);
            
            return crosshair;
        }
    }
}


