using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("O Transform do objeto visual do card que deve rotacionar.")]
    [SerializeField] private Transform cardSprite;
    [SerializeField] private VotingManager votingManager;
    [SerializeField] private CardsManager cardsManager;

    [Header("Card Movement Settings")]
    [SerializeField] private float maxDistance = 3.0f;
    [SerializeField] private float maxRotationAngle = 15.0f;
    [SerializeField] private float returnSpeed = 10.0f;

    [Header("Decision Logic")] // <<< NOVA SE��O
    [Tooltip("A dist�ncia que o card precisa ser arrastado para que uma escolha seja confirmada.")]
    [SerializeField] private float decisionDistance = 2.0f;

    [Header("Fall Animation")]
    [Tooltip("A acelera��o da queda (como a gravidade).")]
    [SerializeField] private float fallAcceleration = 9.8f;
    [Tooltip("A posi��o Y em que o card deve ser considerado 'fora da tela' para resetar.")]
    [SerializeField] private float verticalResetLimit = -12f;
    [Tooltip("Multiplicador da rota��o durante a queda.")]
    [SerializeField] private float fallRotationMultiplier = 30.0f;
    [SerializeField] private float horizontalSpeedMultiplier = 0.5f;

    [Header("Stats Indicators Feedback")]
    [Tooltip("O objeto pai (com RectTransform) que cont�m todos os indicadores de stats.")]
    [SerializeField] private RectTransform statsContainer;
    [Tooltip("O deslocamento vertical m�ximo do container de stats.")]
    [SerializeField] private float statsMaxVerticalOffset = -30f;
    [Tooltip("O valor de stat que serve como limiar para mudar o tamanho do indicador.")]
    [SerializeField] private int statValueThreshold = 50;
    [Tooltip("O tamanho (Largura, Altura) para o indicador quando o valor do stat est� abaixo do limiar.")]
    [SerializeField] private Vector2 smallStatSize = new(25f, 25f);
    [Tooltip("O tamanho (Largura, Altura) para o indicador quando o valor do stat est� acima do limiar.")]
    [SerializeField] private Vector2 largeStatSize = new(40f, 40f);

    [Header("Text Elements Feedback")]
    [SerializeField] private TextMeshPro rightText;
    [SerializeField] private TextMeshPro leftText;
    [SerializeField] private float textFadeStartDistance = 0.5f;
    [SerializeField] private float textMaxVerticalOffset = -0.2f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 initialMousePosition;
    private bool isDragging = false;
    private Coroutine activeCoroutine; // Renomeado para ser gen�rico
    private Vector3 rightTextOriginalLocalPos;
    private Vector3 leftTextOriginalLocalPos;
    private Collider2D cardCollider; // Refer�ncia para o collider
    private Vector2 statsOriginalLocalPos;
    private Image[] statsImages;
    private enum StatDirection { Neutral, Left, Right }
    private StatDirection currentStatDirection;

    void Start()
    {
        cardCollider = GetComponent<Collider2D>(); // Pega o collider
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

        // Inicializa os indicadores de stats
        if (statsContainer != null)
        {
            // Guarda a posi��o inicial do container
            statsOriginalLocalPos = statsContainer.anchoredPosition;

            // Pega TODAS as imagens que s�o filhas do container
            statsImages = statsContainer.GetComponentsInChildren<Image>();

            // Garante que todos os indicadores comecem invis�veis
            foreach (Image img in statsImages)
            {
                SetImageAlpha(img, 0);
            }
        }
    }

    private void OnMouseDown()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        initialMousePosition = GetMouseWorldPosition();
        isDragging = true;

        // Reseta o estado da dire��o no in�cio de cada arraste
        currentStatDirection = StatDirection.Neutral;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        float distanceX = GetMouseWorldPosition().x - initialMousePosition.x;
        float clampedDistance = Mathf.Clamp(distanceX, -maxDistance, maxDistance);

        transform.position = new Vector3(originalPosition.x + clampedDistance, originalPosition.y, originalPosition.z);

        float movePercentage = clampedDistance / maxDistance;
        float currentAngle = -movePercentage * maxRotationAngle;

        if (cardSprite != null)
        {
            cardSprite.rotation = Quaternion.Euler(0, 0, currentAngle);
        }

        // --- L�GICA DE ATUALIZA��O DE STATS POR DIRE��O ---

        // 1. Determina a dire��o alvo com base na posi��o atual
        StatDirection targetDirection;
        if (clampedDistance > 0.1f) // Usamos um pequeno limiar para evitar oscila��es no centro
        {
            targetDirection = StatDirection.Right;
        }
        else if (clampedDistance < -0.1f)
        {
            targetDirection = StatDirection.Left;
        }
        else
        {
            targetDirection = StatDirection.Neutral;
        }

        // 2. Compara a dire��o alvo com a dire��o atual e atualiza APENAS se houver mudan�a
        if (targetDirection != currentStatDirection)
        {
            UpdateStatsForDirection(targetDirection);
            currentStatDirection = targetDirection; // Atualiza o estado atual
        }

        UpdateTextFeedback(clampedDistance);
        UpdateStatsIndicators(clampedDistance);
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        float horizontalDistance = transform.position.x - originalPosition.x;

        // Verifica se a dist�ncia absoluta (para qualquer um dos lados) ultrapassou o limite de decis�o
        if (Mathf.Abs(horizontalDistance) > decisionDistance)
        {
            // ESCOLHA TOMADA
            activeCoroutine = StartCoroutine(AnimateChoice(horizontalDistance));
        }
        else
        {
            // VOLTAR AO CENTRO
            activeCoroutine = StartCoroutine(ReturnToOrigin());
        }
    }

    // NOVA COROUTINE para animar a queda do card
    private IEnumerator AnimateChoice(float direction)
    {
        // Desativa o collider para impedir novos cliques durante a anima��o
        if (cardCollider != null) cardCollider.enabled = false;

        float currentVerticalSpeed = 0f;
        float horizontalSpeed = Mathf.Abs(direction) * horizontalSpeedMultiplier;

        // O loop de anima��o de queda (exatamente como antes)
        while (transform.position.y > verticalResetLimit)
        {
            currentVerticalSpeed += fallAcceleration * Time.deltaTime;
            float moveY = -currentVerticalSpeed * Time.deltaTime;
            float moveX = Mathf.Sign(direction) * horizontalSpeed * Time.deltaTime;
            transform.position += new Vector3(moveX, moveY, 0);
            transform.Rotate(0, 0, -Mathf.Sign(direction) * fallRotationMultiplier * Time.deltaTime);
            yield return null;
        }

        // --- FIM DA ANIMA��O: RESET E DESATIVA��O ---

        // 1. Registra a escolha no console
        if (direction > 0)
        {
            Debug.Log("ESCOLHA DA DIREITA TOMADA!");
            votingManager.OnVoteRight(); // Chama o m�todo de voto da direita
        }
        else
        {
            Debug.Log("ESCOLHA DA ESQUERDA TOMADA!");
            votingManager.OnVoteLeft(); // Chama o m�todo de voto da esquerda
        }

        // 2. Reseta a posi��o do controlador e a rota��o do sprite para os valores originais.
        transform.SetPositionAndRotation(originalPosition, Quaternion.identity);
        if (cardSprite != null) cardSprite.rotation = originalRotation;

        // 3. Reseta os textos (opacidade e posi��o).
        if (rightText != null)
        {
            SetTextAlpha(rightText, 0);
            rightText.transform.localPosition = rightTextOriginalLocalPos;
            rightText.transform.rotation = Quaternion.identity; // Reseta a rota��o do texto
        }
        if (leftText != null)
        {
            SetTextAlpha(leftText, 0);
            leftText.transform.localPosition = leftTextOriginalLocalPos;
            leftText.transform.rotation = Quaternion.identity; // Reseta a rota��o do texto
        }
        
        UpdateStatsValues(new int[] { 0, 0, 0, 0 });
        currentStatDirection = StatDirection.Neutral;
        // 4. REATIVA o collider para que o card possa ser clicado novamente quando for reutilizado.
        // Este passo � MUITO importante para object pooling.
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

    // As fun��es abaixo permanecem inalteradas
    private void UpdateTextFeedback(float horizontalDistance)
    {
        if (rightText != null)
        {
            float rightAlpha = Mathf.InverseLerp(textFadeStartDistance, maxDistance, horizontalDistance);
            SetTextAlpha(rightText, rightAlpha);
            float rightOffsetY = Mathf.Lerp(0, textMaxVerticalOffset, rightAlpha);
            rightText.transform.localPosition = new Vector3(rightTextOriginalLocalPos.x, rightTextOriginalLocalPos.y + rightOffsetY, rightTextOriginalLocalPos.z);
        }
        if (leftText != null)
        {
            float leftAlpha = Mathf.InverseLerp(-textFadeStartDistance, -maxDistance, horizontalDistance);
            SetTextAlpha(leftText, leftAlpha);
            float leftOffsetY = Mathf.Lerp(0, textMaxVerticalOffset, leftAlpha);
            leftText.transform.localPosition = new Vector3(leftTextOriginalLocalPos.x, leftTextOriginalLocalPos.y + leftOffsetY, leftTextOriginalLocalPos.z);
        }
    }

    private void UpdateStatsIndicators(float horizontalDistance)
    {
        // Se o container n�o foi definido, n�o faz nada.
        if (statsContainer == null) return;

        // Pega a dist�ncia absoluta, pois o efeito � o mesmo para a esquerda ou direita.
        float absoluteDistance = Mathf.Abs(horizontalDistance);

        // Calcula o alfa usando InverseLerp, come�ando do limiar (textFadeStartDistance) at� a dist�ncia m�xima.
        // O alfa ser� um valor entre 0 e 1.
        float alpha = Mathf.InverseLerp(textFadeStartDistance, maxDistance, absoluteDistance);

        // Itera por todos os indicadores (imagens) e aplica o mesmo alfa.
        foreach (Image img in statsImages)
        {
            SetImageAlpha(img, alpha);
        }

        // Calcula o deslocamento vertical para o container pai.
        float offsetY = Mathf.Lerp(0, statsMaxVerticalOffset, alpha);

        // Aplica o deslocamento ao RectTransform usando anchoredPosition (correto para UI).
        statsContainer.anchoredPosition = new Vector2(statsOriginalLocalPos.x, statsOriginalLocalPos.y + offsetY);
    }

    /// <summary>
    /// Pega os valores do CardsManager com base na dire��o e chama a fun��o de atualiza��o da UI.
    /// </summary>
    private void UpdateStatsForDirection(StatDirection direction)
    {
        // Se o cardsManager n�o existir, n�o faz nada.
        if (cardsManager == null) return;

        int[] newValues;

        switch (direction)
        {
            case StatDirection.Right:
                Debug.Log("Mudou para stats da DIREITA");
                newValues = cardsManager.GetCurrentCardRightValues();
                break;

            case StatDirection.Left:
                Debug.Log("Mudou para stats da ESQUERDA");
                newValues = cardsManager.GetCurrentCardLeftValues();
                break;

            case StatDirection.Neutral:
            default:
                // Quando volta para o centro, esconde os indicadores de valor
                newValues = new int[] { 0, 0, 0, 0 };
                break;
        }

        // Chama a fun��o que atualiza a UI com os novos valores
        UpdateStatsValues(newValues);
    }

    /// <summary>
    /// Atualiza o tamanho visual dos indicadores de stats com base em um array de valores.
    /// </summary>
    /// <param name="statValues">Um array de inteiros (int[]) com os valores de cada stat.</param>
    private void UpdateStatsValues(int[] statValues)
    {
        // --- Verifica��es de Seguran�a (evita erros) ---
        if (statsImages == null || statsImages.Length == 0)
        {
            Debug.LogWarning("O array de imagens de stats n�o foi configurado.");
            return;
        }
        if (statValues == null || statValues.Length != statsImages.Length)
        {
            Debug.LogError("O array de valores de stats � nulo ou tem um tamanho diferente do n�mero de imagens de stats!");
            return;
        }

        // --- L�gica Principal ---
        // Itera pelos arrays usando um �ndice
        for (int i = 0; i < statsImages.Length; i++)
        {
            // Pega a imagem e o valor correspondente
            Image image = statsImages[i];
            int value = statValues[i];

            // Pega o RectTransform da imagem para manipular seu tamanho
            RectTransform rect = image.rectTransform;

            // Condi��o 1: Valor � 0, indicador fica invis�vel.
            if (value == 0)
            {
                rect.localScale = Vector3.zero;
            }
            else
            {
                // Garante que o indicador esteja vis�vel antes de ajustar o tamanho.
                // Isso � importante caso ele estivesse com scale 0 anteriormente.
                rect.localScale = Vector3.one;

                // Condi��o 2: Valor acima do limiar
                if (Mathf.Abs(value) > statValueThreshold)
                {
                    rect.sizeDelta = largeStatSize;
                }
                // Condi��o 3: Valor entre 0 e o limiar
                else
                {
                    rect.sizeDelta = smallStatSize;
                }
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
        newColor.a = Mathf.Clamp01(alpha); // Garante que o alfa esteja sempre entre 0 e 1
        imageElement.color = newColor;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}