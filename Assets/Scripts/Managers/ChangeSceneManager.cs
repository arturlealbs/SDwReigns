using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : MonoBehaviour
{
    // Esta fun��o ser� chamada pelo bot�o
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
}
