using UnityEngine;

[System.Serializable] // Isso faz com que apareça no Inspetor da Unity
public class Goal
{
    public string id; // Um ID único, ex: "AJUDAR_NECESSITADO"
    public int points; // Pontos que o objetivo concede
    public bool isCompleted;
}
