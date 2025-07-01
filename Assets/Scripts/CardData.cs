using UnityEngine;

[CreateAssetMenu(fileName = "NovaCarta", menuName = "Jogo/Carta", order = 1)]
public class CardData : ScriptableObject
{
    [Header("Textos da Carta")]
    [Tooltip("O texto principal que descreve a situação.")]
    [TextArea(3, 5)] // Faz o campo de texto ser maior no inspetor
    public string decisao;

    [Tooltip("Texto para a escolha da ESQUERDA.")]
    public string textoEscolhaEsquerda;

    [Tooltip("O texto do resultado caso ESQUERDA vença")]
    [TextArea(3, 5)] // Faz o campo de texto ser maior no inspetor
    public string textoResultadoEsquerda;

    [Tooltip("Texto para a escolha da DIREITA.")]
    public string textoEscolhaDireita;

    [Tooltip("O texto do resultado caso DIREITA vença")]
    [TextArea(3, 5)] // Faz o campo de texto ser maior no inspetor
    public string textoResultadoDireita;

    [Header("Valores da Escolha da ESQUERDA")]
    [Tooltip("Mudanças nos status se o jogador escolher a opção da esquerda. Ordem: Comida, Armas, Popularidade, Energia")]
    public int[] valoresEscolhaEsquerda = new int[4]; // Ex: [Comida, Armas, Popularidade, Energia]

    [Header("Valores da Escolha da DIREITA")]
    [Tooltip("Mudanças nos status se o jogador escolher a opção da direita. Ordem: Comida, Armas, Popularidade, Energia")]
    public int[] valoresEscolhaDireita = new int[4]; // Ex: [Comida, Armas, Popularidade, Energia]

    // Você pode adicionar mais campos aqui, como:
    // public Sprite arteDaCarta;
    // public AudioClip somDaCarta;
    // public string personagem;
}