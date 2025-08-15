using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VotingManager : MonoBehaviour
{
    [Header("Object References")]
    public GameObject cardObject;
    public CardsManager cardsManager;
    public StatsManager statsManager;
    public GameObject goalsPanel;
    public GameObject stats;
    public RectTransform homeButton;
    public GameObject returnButton;
    public Image backgroundImage;
    public Button leftButton;
    public Button rightButton;
    public Button confirmButton;

    [Header("Telas (GameObjects)")]
    public GameObject selectionScreen; // Tela inicial com os botões dos players
    public GameObject votingScreen; // Tela onde o player faz a escolha (vota)
    public GameObject resultsScreen; // Tela que mostra o resultado final

    [Header("Labels Ajustáveis")]
    public TextMeshProUGUI resultsText; // Texto que mostra os resultados na tela de resultados
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI goalsText;
    [SerializeField] private List<Sprite> backgroundImages = new List<Sprite>();

    [Header("Botões dos Players")]
    // Arraste todos os botões dos players para esta lista no Inspector
    public List<Button> playerButtons;

    private int rightVotes = 0;
    private int leftVotes = 0;
    private int playersVoted = 0;
    private int totalPlayers;
    private string cardResultsText = string.Empty;
    private PlayerComponent currentPlayer;
    private Button currentButton;
    private string currentVote;

    void Start()
    {
        // Define o número total de players com base na quantidade de botões
        totalPlayers = playerButtons.Count;

        // Garante que o estado inicial das telas está correto
        selectionScreen.SetActive(true);
        votingScreen.SetActive(false);
        resultsScreen.SetActive(false);

    }

    /// <summary>
    /// Esta função é chamada quando um player clica no seu botão para iniciar a votação.
    /// Ela recebe como parâmetro o botão que foi clicado.
    /// </summary>
    /// <param name="clickedButton">O botão que o player apertou.</param>
    public void OnPlayerSelects(Button clickedButton)
    {
        // 1. Desabilita o botão que foi clicado para que não possa ser usado novamente
        currentButton = clickedButton;
        currentButton.interactable = false;
        currentPlayer = clickedButton.GetComponent<PlayerComponent>();
        playerNameText.text = "Player " + currentPlayer.id; // Atualiza o texto com o ID do jogador
        goalsText.text = string.Join("\n", currentPlayer.goals.Select(g => "• " + g)); // Atualiza o texto com o objetivo do jogador

        // 2. Esconde a tela de seleção de players
        selectionScreen.SetActive(false);

        // 3. Mostra a tela de escolha/votação
        votingScreen.SetActive(true);
        Vector2 currentPosition = homeButton.anchoredPosition;

        // Adiciona 400 à posição X.
        currentPosition.x = 400f;

        // Aplica a nova posição ao botão.
        homeButton.anchoredPosition = currentPosition;

        backgroundImage.sprite = backgroundImages[1]; // Atualiza a imagem de fundo com base no ID do jogador
        stats.SetActive(true); // Mostra o painel de stats do jogador atual
        cardObject.SetActive(true); // Mostra o card para o player votar
    }

    public void OnPlayerReturns()
    {
        votingScreen.SetActive(false);
        selectionScreen.SetActive(true);
        currentButton.interactable = true; // Reabilita o botão do player que retornou

        Vector2 currentPosition = homeButton.anchoredPosition;
        currentPosition.x = 0f;
        homeButton.anchoredPosition = currentPosition;

        backgroundImage.sprite = backgroundImages[0];
        stats.SetActive(false); // Esconde o painel de stats do jogador atual

    }

    public void OnSelectedRight()
    {
        leftButton.interactable = true;
        confirmButton.interactable = true;
        rightButton.interactable = false;
        statsManager.StopPulseEffect();
        statsManager.StartPulseEffect(cardsManager.GetCurrentCardRightValues());

        currentVote = "Right";
    }

    public void OnSelectedLeft()
    {
        leftButton.interactable = false;
        confirmButton.interactable = true;
        rightButton.interactable = true;
        statsManager.StopPulseEffect();
        statsManager.StartPulseEffect(cardsManager.GetCurrentCardLeftValues());

        currentVote = "Left";
    }

    /// <summary>
    /// Função a ser chamada pelo botão de voto da "Esquerda" na tela de seleção.
    /// </summary>
    public void OnVoteLeft()
    {
        leftVotes++;
        PlayerHasVoted();
    }

    /// <summary>
    /// Função a ser chamada pelo botão de voto da "Direita" na tela de seleção.
    /// </summary>
    public void OnVoteRight()
    {
        rightVotes++;
        PlayerHasVoted();
    }

    /// <summary>
    /// Lógica executada após um jogador registrar seu voto.
    /// </summary>
    public void PlayerHasVoted()
    {
        if (currentVote == "Right")
        {
            rightVotes++;
            rightButton.interactable = true;
            confirmButton.interactable = false;
        } else
        {
            leftVotes++;
            leftButton.interactable = true;
            confirmButton.interactable = false;
        }
        
        playersVoted++;
        statsManager.StopPulseEffect();
        votingScreen.SetActive(false);
        cardObject.SetActive(false); // Esconde o card após o voto
        Vector2 currentPosition = homeButton.anchoredPosition;
        currentPosition.x = 0f;
        homeButton.anchoredPosition = currentPosition;
        stats.SetActive(false);

        // Verifica se todos os jogadores já votaram
        if (playersVoted >= totalPlayers)
        {
            // Se sim, mostra os resultados
            ShowResults();
        }
        else
        {
            // Se não, volta para a tela de seleção de player para o próximo
            selectionScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Mostra a tela de resultados.
    /// </summary>
    private void ShowResults()
    {
        
        if(rightVotes > leftVotes)
        {
            statsManager.UpdateStats(cardsManager.GetCurrentCardRightValues());
            cardResultsText = cardsManager.GetCurrentCardRightResult();
            cardsManager.TriggerRightChoiceEvent();

        }
        else
        {
            statsManager.UpdateStats(cardsManager.GetCurrentCardLeftValues());
            cardResultsText = cardsManager.GetCurrentCardLeftResult();
            cardsManager.TriggerLeftChoiceEvent();
        }
        Debug.Log(cardResultsText);
        resultsScreen.SetActive(true);
        backgroundImage.sprite = backgroundImages[2];
        
        if (cardResultsText != string.Empty)
        {
            
            resultsText.text = cardResultsText;
            
        }
        else
        {
            resultsText.text = $"Direita: {rightVotes} vs Esquerda: {leftVotes}";
        }
        
        
    }

    /// <summary>
    /// Reinicia todo o sistema de votação para uma nova rodada.
    /// </summary>
    public void ResetVoting()
    {
        // Reseta os contadores
        leftVotes = 0;
        rightVotes = 0;
        playersVoted = 0;
        cardResultsText = string.Empty;

        // Reabilita todos os botões dos players
        foreach (Button button in playerButtons)
        {
            button.interactable = true;
        }

        // Define o estado inicial das telas
        selectionScreen.SetActive(true);
        votingScreen.SetActive(false);
        resultsScreen.SetActive(false);
        cardObject.SetActive(false);
        stats.SetActive(false); // Esconde o painel de stats
        cardsManager.GetNextCard();
        backgroundImage.sprite = backgroundImages[0];
    }

    public void OnClickGoals()
    {
        goalsPanel.SetActive(!goalsPanel.activeSelf);
    }
}
