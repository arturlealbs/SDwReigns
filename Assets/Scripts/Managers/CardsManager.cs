using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq; // Usaremos para embaralhar

public class CardsManager : MonoBehaviour
{
    // Padr�o Singleton para acesso f�cil e global
    public static CardsManager Instance { get; private set; }

    [Tooltip("Arraste todos os ScriptableObjects de cartas aqui. Este � o deck mestre.")]
    [SerializeField] private CardData tutorialCard;
    [SerializeField] private List<CardData> allCards;
    [SerializeField] private TextMeshProUGUI contextLabel;
    [SerializeField] private TextMeshProUGUI rightTextLabel;
    [SerializeField] private TextMeshProUGUI leftTextLabel;


    // Este baralho ser� usado e modificado durante o jogo
    private List<CardData> availableCards;

    // A CARTA ATUAL! A �nica fonte da verdade.
    public CardData CurrentCard { get; private set; }

    private void Awake()
    {
        // Configura��o do Singleton
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
        // Copia todas as cartas para o baralho dispon�vel
        availableCards = new List<CardData>(allCards);

        // Embaralha o baralho para aleatoriedade (usando System.Linq)
        System.Random rng = new();
        availableCards = availableCards.OrderBy(a => rng.Next()).ToList();
        CurrentCard = tutorialCard;
        contextLabel.text = CurrentCard.decisao;
        rightTextLabel.text = CurrentCard.textoEscolhaDireita;
        leftTextLabel.text = CurrentCard.textoEscolhaEsquerda;
    }

    /// <summary>
    /// Pega a pr�xima carta do baralho, a define como a carta atual e a retorna.
    /// </summary>
    /// <returns>O ScriptableObject da pr�xima carta, ou null se o baralho acabou.</returns>
    public void GetNextCard()
    {
        if (availableCards.Count == 0)
        {
            Debug.LogWarning("O baralho de cartas acabou! Reiniciando o baralho.");
            // Aqui voc� pode decidir o que fazer: terminar o jogo, recarregar o baralho, etc.
            InitializeDeck();
        }

        // Pega a carta do topo do baralho
        CurrentCard = availableCards[0];
        availableCards.RemoveAt(0);

        contextLabel.text = CurrentCard.decisao;
        rightTextLabel.text = CurrentCard.textoEscolhaDireita;
        leftTextLabel.text = CurrentCard.textoEscolhaEsquerda;
    }

    // M�todos de conveni�ncia para o GameManager
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

    /// <summary>
    /// Dispara o evento UnityEvent configurado para a escolha da ESQUERDA na carta atual.
    /// </summary>
    public void TriggerLeftChoiceEvent()
    {
        if (CurrentCard != null && CurrentCard.eventoEscolhaEsquerda != null)
        {
            CurrentCard.eventoEscolhaEsquerda.Raise(); // MUDAN�A AQUI
        }
    }

    public void TriggerRightChoiceEvent()
    {
        if (CurrentCard != null && CurrentCard.eventoEscolhaDireita != null)
        {
            CurrentCard.eventoEscolhaDireita.Raise(); // MUDAN�A AQUI
        }
    }
}