using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IntroductionManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image headerBackground;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Introduction Data")]
    [Tooltip("Drag your background images here in order.")]
    [SerializeField] private List<Sprite> backgroundImages = new List<Sprite>();
    [Tooltip("Set the width of the header background for each screen.")]
    [SerializeField] private List<float> headerBackgroundWidths = new List<float>();
    [Tooltip("Set the color of the header text for each screen.")]
    [SerializeField] private List<Color> textColors = new List<Color>();
    [Tooltip("Set the color of the body text for each screen.")]
    [SerializeField] private List<string> headerTexts = new List<string>();
    [Tooltip("The actual text content for each body.")]
    [SerializeField] private List<string> bodyTexts = new List<string>();


    private int currentScreenIndex = 0;

    void Start()
    {
        // Add listeners to the buttons
        continueButton.onClick.AddListener(OnContinueButtonClicked);

        // Ensure the Start Game button is initially hidden
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(false);
            startGameButton.onClick.AddListener(OnStartGameButtonClicked); // Adiciona listener ao novo botão
        }

        // Ensure the Skip button is active at the start (if you plan to use it)
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.AddListener(OnStartGameButtonClicked); 
        }

        // Initialize the first screen
        UpdateIntroductionScreen();
    }

    void OnContinueButtonClicked()
    {
        currentScreenIndex++;
        UpdateIntroductionScreen();
        // Check if there are more screens
        if (currentScreenIndex == backgroundImages.Count - 1)
        {
            // Introduction finished, hide continue/skip buttons and show start game button
            continueButton.gameObject.SetActive(false);
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }
            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(true);
            }
            Debug.Log("Introduction finished! Showing Start Game button.");
        }

    }
    void OnStartGameButtonClicked()
    {
        Debug.Log("Starting Game!");
        // Aqui você pode carregar a próxima cena principal do jogo
        // Exemplo: SceneManager.LoadScene("MainGameScene");
        // Ou desativar a UI da introdução: gameObject.SetActive(false);
    }

    void UpdateIntroductionScreen()
    {
        // Ensure lists have consistent counts to prevent errors
        if (currentScreenIndex >= backgroundImages.Count ||
            currentScreenIndex >= headerBackgroundWidths.Count ||
            currentScreenIndex >= textColors.Count ||
            currentScreenIndex >= headerTexts.Count ||
            currentScreenIndex >= bodyTexts.Count)
        {
            Debug.LogError("Introduction data lists are not consistent in size! Please check all lists in the Inspector.");
            return;
        }

        // Update background image
        backgroundImage.sprite = backgroundImages[currentScreenIndex];

        // Update header background width
        RectTransform headerBgRect = headerBackground.GetComponent<RectTransform>();
        headerBgRect.sizeDelta = new Vector2(headerBackgroundWidths[currentScreenIndex], headerBgRect.sizeDelta.y);

        // Update header text color and content
        headerText.color = textColors[currentScreenIndex];
        headerText.text = headerTexts[currentScreenIndex];

        // Update body text color and content
        bodyText.color = textColors[currentScreenIndex];
        bodyText.text = bodyTexts[currentScreenIndex];

        // Adjust header text width based on header background width
        RectTransform headerTextRect = headerText.GetComponent<RectTransform>();
        // The text width is the header background width minus 30
        headerTextRect.sizeDelta = new Vector2(headerBackgroundWidths[currentScreenIndex] - 30, headerTextRect.sizeDelta.y);
    }
}