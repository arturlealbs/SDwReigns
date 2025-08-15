using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necessário para usar o .FirstOrDefault


public class GoalsManager : MonoBehaviour
{
 
    // Singleton para acesso fácil (opcional, mas recomendado)
    public static GoalsManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Use se o GoalsManager precisar persistir entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Lidar com os seguintes objetivos:
    // Proteja pelo menos uma criança (+3)
    // Ajude alguem em necessidade (+3)
    // Evite um confronto (+3)
    // Seja egoista ao menos uma vêz (+3)
    // Cause alguma destruição (+3)
    public void AjudarNecessitado()
    {

    }
    public void ProtegerCrianca()
    {

    }
    public void EvitarConfronto()
    {

    }
    public void SerEgoista()
    {

    }
    public void CausarDestruicao()
    {

    }

    // Lidar com os seguintes eventos:
    // Finalizar o jogo com a barra "Armamentos" vazia (+10)
    // Perca o jogo antes da rodada 10 (+5)
    // Finalizar o jogo com a barra "Energia" vazia (+10)
    // Finalizar o jogo com a barra "Popularidade" cheia (+10)
    // Finalizar o jogo com a barra "Alimentos" vazia (+10)
    // Finalizar o jogo com a barra "Popularidade" vazia (+10)
    public void HandleGameOverGoals(int[] stats, int id)
    {
        if (id < 10)
        {

        }
        if (stats[0] <= 0)
        {
            Debug.Log("Game Over: Alimento failed");
        }
        if (stats[1] <= 0)
        {
            Debug.Log("Game Over: Armamento failed");
        }
        if (stats[2] <= 0)
        {
            Debug.Log("Game Over: Popularidade failed");
        }
        else if (stats[2] >= 100)
        {
            Debug.Log("Game Over: Popularidade achieved");
        }
        if (stats[3] <= 0)
        {
            Debug.Log("Game Over: Energia failed");
        }
    }

    // Lidar com os seguintes objetivos:
    // Não deixar ninguem para trás (+7)
    // Manter-se furtivo (+7)
    // Não deixe a energia ficar abaixo de 20% (+7)
    // Não aceitar nenhum acordo/tregua (+7)
    // Não iniciar novos confrontos (+7)

}
