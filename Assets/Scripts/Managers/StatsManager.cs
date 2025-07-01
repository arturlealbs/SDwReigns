using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necess�rio para usar Corrotinas (IEnumerator)

public class StatsManager : MonoBehaviour
{
    [Header("Configura��es de UI")]
    [SerializeField] GameObject statsContainer;
    [SerializeField] GameObject gameOverScreen;

    [Header("Configura��es da Anima��o")]
    [Tooltip("Dura��o da anima��o da barra de status em segundos.")]
    [SerializeField] private float animationDuration = 0.5f;

    // --- Constantes e Vari�veis de Status ---
    private const int MAX_STAT_VALUE = 100;
    private const int MIN_STAT_VALUE = 0;

    private Image[] statsImages;
    private int[] stats = new int[] { 50, 50, 50, 50 };

    // Array para guardar as refer�ncias das corrotinas em execu��o
    private Coroutine[] runningAnimations;

    void Start()
    {
        statsImages = statsContainer.GetComponentsInChildren<Image>();
        // Inicializa o array de corrotinas com o mesmo tamanho do array de imagens
        runningAnimations = new Coroutine[statsImages.Length];
        UpdateStatsUI(true); // O 'true' indica que � uma atualiza��o instant�nea no in�cio
    }

    public void UpdateStats(int[] changes)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i] += changes[i];
            stats[i] = Mathf.Clamp(stats[i], MIN_STAT_VALUE, MAX_STAT_VALUE);
        }

        UpdateStatsUI(); // Agora vai chamar a vers�o animada

        if (CheckForGameOver())
        {
            gameOverScreen.SetActive(true);
        }
    }

    // Modificamos UpdateStatsUI para iniciar as corrotinas
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
                // Antes de iniciar uma nova anima��o, paramos qualquer uma que j� esteja rodando para esta barra
                if (runningAnimations[i] != null)
                {
                    StopCoroutine(runningAnimations[i]);
                }
                // Inicia a nova anima��o e guarda sua refer�ncia
                runningAnimations[i] = StartCoroutine(AnimateStatBar(statsImages[i], targetFill));
            }
        }
    }

    // --- A CORROTINA DE ANIMA��O ---
    private IEnumerator AnimateStatBar(Image statBarImage, float targetFill)
    {
        float startFill = statBarImage.fillAmount; // O valor inicial da barra
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Aumenta o tempo decorrido
            elapsedTime += Time.deltaTime;

            // Calcula o novo valor de preenchimento usando Lerp
            // elapsedTime / animationDuration cria um valor 't' que vai de 0 a 1
            float newFillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime / animationDuration);

            // Aplica o novo valor � barra
            statBarImage.fillAmount = newFillAmount;

            // Pausa a execu��o aqui e continua no pr�ximo frame
            yield return null;
        }

        // Ao final do loop, garante que a barra esteja exatamente no valor alvo
        statBarImage.fillAmount = targetFill;
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