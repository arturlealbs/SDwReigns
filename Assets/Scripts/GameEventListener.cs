// GameEventListener.cs
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("O evento (Asset) que este componente deve escutar.")]
    public GameEvent gameEvent;

    [Tooltip("A resposta que será executada quando o evento for ouvido.")]
    public UnityEvent response;

    private void OnEnable()
    {
        if (gameEvent != null) gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (gameEvent != null) gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        response.Invoke();
    }
}