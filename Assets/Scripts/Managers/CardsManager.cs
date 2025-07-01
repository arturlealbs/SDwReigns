using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq; // Usaremos para embaralhar

public class CardsManager : MonoBehaviour
{
    // Padrão Singleton para acesso fácil e global
    public static CardsManager Instance { get; private set; }

    [Tooltip("Arraste todos os ScriptableObjects de cartas aqui. Este é o deck mestre.")]
    [SerializeField] private List<CardData> allCards;
    [SerializeField] private TextMeshProUGUI contextLabel;
    [SerializeField] private TextMeshPro rightTextLabel;
    [SerializeField] private TextMeshPro leftTextLabel;


    // Este baralho será usado e modificado durante o jogo
    private List<CardData> availableCards;

    // A CARTA ATUAL! A única fonte da verdade.
    public CardData CurrentCard { get; private set; }

    private void Awake()
    {
        // Configuração do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Opcional, se o manager precisar persistir entre cenas
        }

        InitializeDeck();
    }

    private void InitializeDeck()
    {
        // Copia todas as cartas para o baralho disponível
        availableCards = new List<CardData>(allCards);

        // Embaralha o baralho para aleatoriedade (usando System.Linq)
        System.Random rng = new();
        availableCards = availableCards.OrderBy(a => rng.Next()).ToList();
    }

    /// <summary>
    /// Pega a próxima carta do baralho, a define como a carta atual e a retorna.
    /// </summary>
    /// <returns>O ScriptableObject da próxima carta, ou null se o baralho acabou.</returns>
    public void GetNextCard()
    {
        if (availableCards.Count == 0)
        {
            Debug.LogWarning("O baralho de cartas acabou! Reiniciando o baralho.");
            // Aqui você pode decidir o que fazer: terminar o jogo, recarregar o baralho, etc.
            InitializeDeck();
        }

        // Pega a carta do topo do baralho
        CurrentCard = availableCards[0];
        availableCards.RemoveAt(0);

        contextLabel.text = CurrentCard.decisao;
        rightTextLabel.text = CurrentCard.textoEscolhaDireita;
        leftTextLabel.text = CurrentCard.textoEscolhaEsquerda;
    }

    // Métodos de conveniência para o GameManager
    public int[] GetCurrentCardLeftValues()
    {
        return CurrentCard != null ? CurrentCard.valoresEscolhaEsquerda : null;
    }

    public int[] GetCurrentCardRightValues()
    {
        return CurrentCard != null ? CurrentCard.valoresEscolhaDireita : null;
    }

    public string GetCurrentCardRightResult()
    {
        return CurrentCard.textoResultadoDireita;
    }

    public string GetCurrentCardLeftResult()
    {
        return CurrentCard.textoResultadoEsquerda;
    }
}