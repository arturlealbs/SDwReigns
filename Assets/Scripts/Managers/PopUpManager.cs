using UnityEngine;
using UnityEngine.EventSystems; // Para IPointerClickHandler

public class PopUpManager : MonoBehaviour, IPointerClickHandler
{
    // A referência ao alerta que está atualmente ativo e deve ser fechado
    private GameObject currentActiveAlert;

    // Método público para outros scripts chamarem e mostrarem um alerta
    public void ShowAlert(GameObject alertToShow)
    {
        // Garante que o BackgroundBlocker esteja ativo
        gameObject.SetActive(true);

        // Ativa o alerta fornecido
        if (alertToShow != null)
        {
            alertToShow.SetActive(true);
            currentActiveAlert = alertToShow; // Define qual alerta deve ser fechado
        }
    }

    // Método chamado quando o BackgroundBlocker é clicado
    public void OnPointerClick(PointerEventData eventData)
    {
        CloseCurrentAlert();
    }

    // Método para fechar o alerta atual e desativar o BackgroundBlocker
    public void CloseCurrentAlert()
    {
        if (currentActiveAlert != null)
        {
            currentActiveAlert.SetActive(false); // Desativa o alerta
            currentActiveAlert = null; // Limpa a referência
        }
        gameObject.SetActive(false); // Desativa o BackgroundBlocker
    }
}