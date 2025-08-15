using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StatsManager : MonoBehaviour
{
    [Header("Configurações de UI")]
    [SerializeField] GameObject statsBackgrounds;
    [SerializeField] GameObject statsBars;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GoalsManager goalsManager;

    [Header("Configurações da Animação de Preenchimento")]
    [Tooltip("Duração da animação da barra de status em segundos.")]
    [SerializeField] private float animationDuration = 1f;

    [Header("Configurações da Animação de Pulsação")]
    [Tooltip("A velocidade base do ciclo de aumentar e diminuir.")]
    [Range(0.1f, 10f)] public float pulseAnimationSpeed = 2f;
    [Tooltip("A escala mínima que os objetos atingirão.")]
    [Range(0.5f, 1f)] public float minScale = 1.0f;
    [Tooltip("A escala máxima que os objetos atingirão no efeito normal.")]
    [Range(1f, 2f)] public float maxScale = 1.15f;

    // --- NOVO: Variáveis para o efeito multiplicado ---
    [Tooltip("Acima deste valor, o efeito de pulsação é intensificado.")]
    [SerializeField] private int strengthThreshold = 50;
    [Tooltip("Multiplicador de velocidade e tamanho para o efeito de pulsação intenso.")]
    [SerializeField] private float highStrengthMultiplier = 1.5f;


    [Header("Configurações de Cor")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color increaseColor = Color.green;
    [SerializeField] private Color decreaseColor = Color.red;

    private const int MAX_STAT_VALUE = 100;
    private const int MIN_STAT_VALUE = 0;

    private Image[] statsImages;
    private int[] stats = new int[] { 30, 70, 40, 60 };
    private int id = 1;

    private Coroutine[] runningFillAnimations;

    private List<Transform> backgroundTransforms = new List<Transform>();
    private List<Transform> barTransforms = new List<Transform>();
    private Coroutine runningPulseAnimation;

    // --- NOVO: Array para guardar as forças atuais do efeito ---
    private int[] currentPulseStrengths;

    void Start()
    {
        statsImages = statsBars.GetComponentsInChildren<Image>();
        runningFillAnimations = new Coroutine[statsImages.Length];
        InitializeBarColors();
        UpdateStatsUI(true);

        foreach (Transform child in statsBackgrounds.transform)
        {
            backgroundTransforms.Add(child);
        }
        foreach (Transform child in statsBars.transform)
        {
            barTransforms.Add(child);
        }
    }

    // --- ALTERADO: Assinatura do método StartPulseEffect ---
    /// <summary>
    /// Inicia o efeito de pulsação contínua com forças individuais para cada barra.
    /// </summary>
    /// <param name="strengths">Um array de 4 inteiros. 0 = sem efeito, >0 = efeito.</param>
    public void StartPulseEffect(int[] strengths)
    {
        if (strengths == null || strengths.Length != 4)
        {
            Debug.LogError("StartPulseEffect requer um array de 4 inteiros.");
            return;
        }

        // Guarda as forças para a corrotina usar
        currentPulseStrengths = strengths;

        if (runningPulseAnimation != null)
        {
            StopCoroutine(runningPulseAnimation);
        }
        runningPulseAnimation = StartCoroutine(PulseEffectCoroutine());
    }

    public void StopPulseEffect()
    {
        if (runningPulseAnimation != null)
        {
            StopCoroutine(runningPulseAnimation);
            runningPulseAnimation = null;
            currentPulseStrengths = null; // Limpa as forças
            ResetPulseAppearance();
        }
    }

    private void ResetPulseAppearance()
    {
        foreach (Transform bgTransform in backgroundTransforms)
        {
            bgTransform.localScale = Vector3.one;
        }
        foreach (Transform barTransform in barTransforms)
        {
            barTransform.localScale = Vector3.one;
        }
        InitializeBarColors();
    }

    // --- ALTERADO: Lógica principal da corrotina de pulsação ---
    private IEnumerator PulseEffectCoroutine()
    {
        while (true)
        {
            // O loop agora itera por cada um dos 4 elementos individualmente
            for (int i = 0; i < 4; i++)
            {
                // Garante que não tentaremos acessar elementos que não existem
                if (i >= backgroundTransforms.Count || i >= barTransforms.Count || i >= statsImages.Length) continue;

                int strength = currentPulseStrengths[i];

                // CASO 1: Força é 0, sem efeito.
                if (strength == 0)
                {
                    backgroundTransforms[i].localScale = Vector3.one;
                    barTransforms[i].localScale = Vector3.one;
                    if (runningFillAnimations[i] == null)
                    {
                        statsImages[i].color = defaultColor;
                    }
                    continue; // Pula para o próximo elemento
                }

                // Define os multiplicadores com base na força
                float speedMultiplier = 1.0f;
                float scaleMultiplier = 1.0f;

                // CASO 3: Força é maior que o threshold, efeito intensificado.
                if (Mathf.Abs(strength) > strengthThreshold)
                {
                    speedMultiplier = highStrengthMultiplier;
                    scaleMultiplier = highStrengthMultiplier / 1.5f;
                }

                // CASO 2 (implícito): Força > 0 e <= threshold, efeito normal (multiplicadores = 1.0f).

                // Calcula a animação para ESTE elemento com seus multiplicadores
                float sinTime = Mathf.Sin(Time.time * pulseAnimationSpeed * speedMultiplier);
                float t = (sinTime + 1) / 2f;

                // A escala máxima é ajustada pelo multiplicador
                // Amplificamos a "amplitude" da animação (diferença entre max e min)
                float currentMaxScale = minScale + (maxScale - minScale) * scaleMultiplier;
                float scale = Mathf.Lerp(minScale, currentMaxScale, t);
                Vector3 targetScale = new Vector3(scale, scale, scale);

                Color targetColor = Color.Lerp(decreaseColor, increaseColor, t);

                // Aplica os valores calculados apenas ao elemento 'i'
                backgroundTransforms[i].localScale = targetScale;
                barTransforms[i].localScale = targetScale;

                if (runningFillAnimations[i] == null)
                {
                    statsImages[i].color = targetColor;
                }
            }

            yield return null;
        }
    }

    // --- O restante do script (lógica de stats, preenchimento, game over) permanece o mesmo ---
    private void InitializeBarColors() { foreach (var image in statsImages) { image.color = defaultColor; } }
    public void resetColors() { InitializeBarColors(); }
   
    public void UpdateStats(int[] changes)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i] += changes[i];
            stats[i] = Mathf.Clamp(stats[i], MIN_STAT_VALUE, MAX_STAT_VALUE);
        }

        UpdateStatsUI();

        if (CheckForGameOver() || id == 15)
        {
            gameOverScreen.SetActive(true);
            goalsManager.HandleGameOverGoals(stats, id);
        }
        id++;
    }

    private void UpdateStatsUI(bool instant = false)
    {
        for (int i = 0; i < statsImages.Length; i++)
        {
            float targetFill = (float)stats[i] / MAX_STAT_VALUE;

            if (instant)
            {
                statsImages[i].fillAmount = targetFill;
            }
            else
            {
                if (runningFillAnimations[i] != null)
                {
                    StopCoroutine(runningFillAnimations[i]);
                }
                runningFillAnimations[i] = StartCoroutine(AnimateStatBar(i, statsImages[i], targetFill));
            }
        }
    }

    private IEnumerator AnimateStatBar(int index, Image statBarImage, float targetFill)
    {
        float startFill = statBarImage.fillAmount;
        float elapsedTime = 0f;

        if (Mathf.Approximately(startFill, targetFill))
        {
            runningFillAnimations[index] = null; // Limpa a referência da corrotina
            yield break;
        }

        Color animationColor = (targetFill > startFill) ? increaseColor : decreaseColor;
        statBarImage.color = animationColor; // A cor da animação de preenchimento tem prioridade

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float newFillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime / animationDuration);
            statBarImage.fillAmount = newFillAmount;
            yield return null;
        }

        statBarImage.fillAmount = targetFill;

        // **IMPORTANTE**: Após a animação, a cor volta para a cor da pulsação,
        // não para a 'defaultColor', para que o efeito de pulsação continue visualmente.
        // Se a pulsação não estiver ativa, ela pegará a cor que a pulsação definiria no próximo frame.
        // statBarImage.color = defaultColor; // Esta linha não é mais necessária aqui.

        runningFillAnimations[index] = null; // Limpa a referência para indicar que a animação terminou
    }

    public bool CheckForGameOver()
    {
        foreach (int statValue in stats)
        {
            if (statValue <= MIN_STAT_VALUE || statValue >= MAX_STAT_VALUE)
            {
                return true;
            }
        }
        return false;
    }
}