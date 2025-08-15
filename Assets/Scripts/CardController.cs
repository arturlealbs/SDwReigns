using UnityEngine;
using TMPro; // Namespace para TextMeshPro
using UnityEngine.UI; // Namespace para UI.Image
using System.Collections; // Namespace para Coroutines
using UnityEngine.InputSystem; // NOVO: Namespace para o novo Input System
using UnityEngine.EventSystems; // NOVO: Namespace para verificar a UI

public class CardController : MonoBehaviour
{
    #region Variáveis SerializeField
    // --- Todas as suas variáveis públicas permanecem exatamente as mesmas ---

    [Header("Object References")]
    [Tooltip("O Transform do objeto visual do card que deve rotacionar.")]
    [SerializeField] private Transform cardSprite;
    [SerializeField] private VotingManager votingManager;
    [SerializeField] private CardsManager cardsManager;

    [Header("Card Movement Settings")]
    [SerializeField] private float maxDistance = 3.0f;
    [SerializeField] private float maxRotationAngle = 15.0f;
    [SerializeField] private float returnSpeed = 10.0f;

    [Header("Decision Logic")]
    [Tooltip("A distância que o card precisa ser arrastado para que uma escolha seja confirmada.")]
    [SerializeField] private float decisionDistance = 2.0f;

    [Header("Fall Animation")]
    [Tooltip("A aceleração da queda (como a gravidade).")]
    [SerializeField] private float fallAcceleration = 9.8f;
    [Tooltip("A posição Y em que o card deve ser considerado 'fora da tela' para resetar.")]
    [SerializeField] private float verticalResetLimit = -12f;
    [Tooltip("Multiplicador da rotação durante a queda.")]
    [SerializeField] private float fallRotationMultiplier = 30.0f;
    [SerializeField] private float horizontalSpeedMultiplier = 0.5f;

    [Header("Stats Indicators Feedback")]
    [Tooltip("O objeto pai (com RectTransform) que contém todos os indicadores de stats.")]
    [SerializeField] private RectTransform statsContainer;
    [Tooltip("O deslocamento vertical máximo do container de stats.")]
    [SerializeField] private float statsMaxVerticalOffset = -30f;
    [Tooltip("O valor de stat que serve como limiar para mudar o tamanho do indicador.")]
    [SerializeField] private int statValueThreshold = 50;
    [Tooltip("O tamanho (Largura, Altura) para o indicador quando o valor do stat está abaixo do limiar.")]
    [SerializeField] private Vector2 smallStatSize = new(25f, 25f);
    [Tooltip("O tamanho (Largura, Altura) para o indicador quando o valor do stat está acima do limiar.")]
    [SerializeField] private Vector2 largeStatSize = new(40f, 40f);

    [Header("Text Elements Feedback")]
    [SerializeField] private TextMeshPro rightText;
    [SerializeField] private TextMeshPro leftText;
    [SerializeField] private float textFadeStartDistance = 0.5f;
    [SerializeField] private float textMaxVerticalOffset = -0.2f;
    #endregion

    #region Variáveis Privadas
    // --- Variáveis privadas existentes ---
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDragging = false;
    private Coroutine activeCoroutine;
    private Vector3 rightTextOriginalLocalPos;
    private Vector3 leftTextOriginalLocalPos;
    private Collider2D cardCollider;
    private Vector2 statsOriginalLocalPos;
    private Image[] statsImages;
    private enum StatDirection { Neutral, Left, Right }
    private StatDirection currentStatDirection;

    // --- NOVAS variáveis para o Input System ---
    private PlayerControls playerControls;
    private Camera mainCamera;
    private Vector3 initialPointerWorldPosition; // Substitui initialMousePosition
    #endregion

    #region Métodos do Ciclo de Vida do Unity (Awake, Start, etc.)

    // NOVO: Awake é usado para inicializar objetos antes de Start
    private void Awake()
    {
        mainCamera = Camera.main;
        playerControls = new PlayerControls();
    }

    // NOVO: OnEnable/OnDisable gerenciam a ativação dos controles
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    // Start agora também configura as inscrições nos eventos de input
    void Start()
    {
        // Pega o collider
        cardCollider = GetComponent<Collider2D>();

        // --- Sua lógica de Start original permanece intacta ---
        originalPosition = transform.position;
        originalRotation = cardSprite.rotation;

        if (rightText != null)
        {
            rightTextOriginalLocalPos = rightText.transform.localPosition;
            SetTextAlpha(rightText, 0);
        }
        if (leftText != null)
        {
            leftTextOriginalLocalPos = leftText.transform.localPosition;
            SetTextAlpha(leftText, 0);
        }
        if (statsContainer != null)
        {
            statsOriginalLocalPos = statsContainer.anchoredPosition;
            statsImages = statsContainer.GetComponentsInChildren<Image>();
            foreach (Image img in statsImages)
            {
                SetImageAlpha(img, 0);
            }
        }

        // --- NOVO: Inscrição nos eventos de input ---
        // Quando o botão/toque é pressionado, chama OnPointerPressStarted
        playerControls.Gameplay.PointerPress.started += context => OnPointerPressStarted();

        // Quando o botão/toque é solto, chama OnPointerPressCanceled
        playerControls.Gameplay.PointerPress.canceled += context => OnPointerPressCanceled();
    }

    // NOVO: O Update agora lida com a lógica de arrastar (o antigo OnMouseDrag)
    void Update()
    {
        // Se não estamos arrastando, não faz nada.
        if (!isDragging) return;

        // Pega a posição do ponteiro na tela
        Vector2 pointerScreenPosition = playerControls.Gameplay.PointerPosition.ReadValue<Vector2>();

        // Converte para a posição no mundo do jogo
        Vector3 pointerWorldPosition = mainCamera.ScreenToWorldPoint(pointerScreenPosition);

        // --- A lógica de OnMouseDrag é copiada para cá ---
        float distanceX = pointerWorldPosition.x - initialPointerWorldPosition.x;
        float clampedDistance = Mathf.Clamp(distanceX, -maxDistance, maxDistance);

        transform.position = new Vector3(originalPosition.x + clampedDistance, originalPosition.y, originalPosition.z);

        float movePercentage = clampedDistance / maxDistance;
        float currentAngle = -movePercentage * maxRotationAngle;

        if (cardSprite != null)
        {
            cardSprite.rotation = Quaternion.Euler(0, 0, currentAngle);
        }

        StatDirection targetDirection;
        if (clampedDistance > 0.1f)
            targetDirection = StatDirection.Right;
        else if (clampedDistance < -0.1f)
            targetDirection = StatDirection.Left;
        else
            targetDirection = StatDirection.Neutral;

        if (targetDirection != currentStatDirection)
        {
            UpdateStatsForDirection(targetDirection);
            currentStatDirection = targetDirection;
        }

        UpdateTextFeedback(clampedDistance);
        UpdateStatsIndicators(clampedDistance);
    }

    #endregion

    #region Novos Handlers de Input

    // NOVO: Chamado quando o toque/clique começa. Substitui OnMouseDown.
    private void OnPointerPressStarted()
    {
        // Pega a posição do ponteiro na tela
        Vector2 pointerScreenPosition = playerControls.Gameplay.PointerPosition.ReadValue<Vector2>();

        // Se o toque foi em um botão da UI, ignora completamente.
        if (IsPointerOverUI(pointerScreenPosition)) return;

        // Verifica se o toque atingiu o collider deste card
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(pointerScreenPosition), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            // Começa o arraste (lógica do antigo OnMouseDown)
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);

            initialPointerWorldPosition = mainCamera.ScreenToWorldPoint(pointerScreenPosition);
            isDragging = true;
            currentStatDirection = StatDirection.Neutral;
        }
    }

    // NOVO: Chamado quando o toque/clique termina. Substitui OnMouseUp.
    private void OnPointerPressCanceled()
    {
        // Se não estávamos arrastando, não faz nada.
        if (!isDragging) return;

        // Lógica do antigo OnMouseUp
        isDragging = false;
        float horizontalDistance = transform.position.x - originalPosition.x;

        if (Mathf.Abs(horizontalDistance) > decisionDistance)
        {
            activeCoroutine = StartCoroutine(AnimateChoice(horizontalDistance));
        }
        else
        {
            activeCoroutine = StartCoroutine(ReturnToOrigin());
        }
    }

    // NOVO: Função auxiliar para verificar se o ponteiro está sobre a UI
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    #endregion

    // =============================================================================================
    // TODAS AS SUAS OUTRAS FUNÇÕES (COROUTINES, HELPERS) PERMANECEM EXATAMENTE IGUAIS
    // =============================================================================================

    #region Coroutines e Funções de Feedback

    private IEnumerator AnimateChoice(float direction)
    {
        if (cardCollider != null) cardCollider.enabled = false;
        float currentVerticalSpeed = 0f;
        float horizontalSpeed = Mathf.Abs(direction) * horizontalSpeedMultiplier;

        while (transform.position.y > verticalResetLimit)
        {
            currentVerticalSpeed += fallAcceleration * Time.deltaTime;
            float moveY = -currentVerticalSpeed * Time.deltaTime;
            float moveX = Mathf.Sign(direction) * horizontalSpeed * Time.deltaTime;
            transform.position += new Vector3(moveX, moveY, 0);
            transform.Rotate(0, 0, -Mathf.Sign(direction) * fallRotationMultiplier * Time.deltaTime);
            yield return null;
        }

        if (direction > 0)
        {
            Debug.Log("ESCOLHA DA DIREITA TOMADA!");
            votingManager.OnVoteRight();
        }
        else
        {
            Debug.Log("ESCOLHA DA ESQUERDA TOMADA!");
            votingManager.OnVoteLeft();
        }

        transform.SetPositionAndRotation(originalPosition, Quaternion.identity);
        if (cardSprite != null) cardSprite.rotation = originalRotation;

        if (rightText != null)
        {
            SetTextAlpha(rightText, 0);
            rightText.transform.localPosition = rightTextOriginalLocalPos;
            rightText.transform.rotation = Quaternion.identity;
        }
        if (leftText != null)
        {
            SetTextAlpha(leftText, 0);
            leftText.transform.localPosition = leftTextOriginalLocalPos;
            leftText.transform.rotation = Quaternion.identity;
        }

        UpdateStatsValues(new int[] { 0, 0, 0, 0 });
        currentStatDirection = StatDirection.Neutral;
        if (cardCollider != null) cardCollider.enabled = true;
    }

    private IEnumerator ReturnToOrigin()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = cardSprite.rotation;
        Color startRightColor = rightText != null ? rightText.color : Color.clear;
        Color startLeftColor = leftText != null ? leftText.color : Color.clear;
        Vector3 startRightLocalPos = rightText != null ? rightText.transform.localPosition : Vector3.zero;
        Vector3 startLeftLocalPos = leftText != null ? leftText.transform.localPosition : Vector3.zero;

        float elapsedTime = 0f;
        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            float t = elapsedTime * returnSpeed;
            transform.position = Vector3.Lerp(startPosition, originalPosition, t);
            if (cardSprite != null)
            {
                cardSprite.rotation = Quaternion.Lerp(startRotation, originalRotation, t);
            }
            if (rightText != null)
            {
                rightText.color = Color.Lerp(startRightColor, new Color(startRightColor.r, startRightColor.g, startRightColor.b, 0), t);
                rightText.transform.localPosition = Vector3.Lerp(startRightLocalPos, rightTextOriginalLocalPos, t);
            }
            if (leftText != null)
            {
                leftText.color = Color.Lerp(startLeftColor, new Color(startLeftColor.r, startLeftColor.g, startLeftColor.b, 0), t);
                leftText.transform.localPosition = Vector3.Lerp(startLeftLocalPos, leftTextOriginalLocalPos, t);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        if (cardSprite != null) cardSprite.rotation = originalRotation;
        if (rightText != null)
        {
            SetTextAlpha(rightText, 0);
            rightText.transform.localPosition = rightTextOriginalLocalPos;
        }
        if (leftText != null)
        {
            SetTextAlpha(leftText, 0);
            leftText.transform.localPosition = leftTextOriginalLocalPos;
        }
        activeCoroutine = null;
    }

    private void UpdateTextFeedback(float horizontalDistance)
    {
        if (rightText == null || leftText == null) return;

        float rightAlpha = Mathf.InverseLerp(textFadeStartDistance, maxDistance, horizontalDistance);
        SetTextAlpha(rightText, rightAlpha);
        float rightOffsetY = Mathf.Lerp(0, textMaxVerticalOffset, rightAlpha);
        rightText.transform.localPosition = new Vector3(rightTextOriginalLocalPos.x, rightTextOriginalLocalPos.y + rightOffsetY, rightTextOriginalLocalPos.z);

        float leftAlpha = Mathf.InverseLerp(-textFadeStartDistance, -maxDistance, horizontalDistance);
        SetTextAlpha(leftText, leftAlpha);
        float leftOffsetY = Mathf.Lerp(0, textMaxVerticalOffset, leftAlpha);
        leftText.transform.localPosition = new Vector3(leftTextOriginalLocalPos.x, leftTextOriginalLocalPos.y + leftOffsetY, leftTextOriginalLocalPos.z);
    }

    private void UpdateStatsIndicators(float horizontalDistance)
    {
        if (statsContainer == null) return;

        float absoluteDistance = Mathf.Abs(horizontalDistance);
        float alpha = Mathf.InverseLerp(textFadeStartDistance, maxDistance, absoluteDistance);

        foreach (Image img in statsImages)
        {
            SetImageAlpha(img, alpha);
        }

        float offsetY = Mathf.Lerp(0, statsMaxVerticalOffset, alpha);
        statsContainer.anchoredPosition = new Vector2(statsOriginalLocalPos.x, statsOriginalLocalPos.y + offsetY);
    }

    private void UpdateStatsForDirection(StatDirection direction)
    {
        if (cardsManager == null) return;

        int[] newValues;
        switch (direction)
        {
            case StatDirection.Right:
                newValues = cardsManager.GetCurrentCardRightValues();
                break;
            case StatDirection.Left:
                newValues = cardsManager.GetCurrentCardLeftValues();
                break;
            case StatDirection.Neutral:
            default:
                newValues = new int[] { 0, 0, 0, 0 };
                break;
        }
        UpdateStatsValues(newValues);
    }

    private void UpdateStatsValues(int[] statValues)
    {
        if (statsImages == null || statsImages.Length == 0) return;
        if (statValues == null || statValues.Length != statsImages.Length) return;

        for (int i = 0; i < statsImages.Length; i++)
        {
            Image image = statsImages[i];
            int value = statValues[i];
            RectTransform rect = image.rectTransform;

            if (value == 0)
            {
                rect.localScale = Vector3.zero;
            }
            else
            {
                rect.localScale = Vector3.one;
                rect.sizeDelta = (Mathf.Abs(value) > statValueThreshold) ? largeStatSize : smallStatSize;
            }
        }
    }

    private void SetTextAlpha(TextMeshPro textElement, float alpha)
    {
        Color newColor = textElement.color;
        newColor.a = alpha;
        textElement.color = newColor;
    }

    private void SetImageAlpha(Image imageElement, float alpha)
    {
        Color newColor = imageElement.color;
        newColor.a = Mathf.Clamp01(alpha);
        imageElement.color = newColor;
    }
    #endregion
}