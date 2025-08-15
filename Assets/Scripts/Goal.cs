using UnityEngine;

[System.Serializable] // Isso faz com que apare�a no Inspetor da Unity
public class Goal
{
    public string id; // Um ID �nico, ex: "AJUDAR_NECESSITADO"
    public int points; // Pontos que o objetivo concede
    public bool isCompleted;
}
