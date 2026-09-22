using UnityEngine;

/// <summary>
/// Objetivo actual del jugador, mostrado en el HUD para guiar la partida.
/// Avanza al conseguir provisiones y se limpia al llegar al refugio.
/// </summary>
public class Misiones : MonoBehaviour
{
    public static Misiones Instancia { get; private set; }

    public string ObjetivoActual { get; private set; }

    private bool provisionesConseguidas;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    private void Start()
    {
        ObjetivoActual = "Explora Sopocachi y consigue provisiones (mercado, farmacia o casa).";
        provisionesConseguidas = false;
    }

    public void ProvisionConseguida()
    {
        if (provisionesConseguidas)
        {
            return;
        }

        provisionesConseguidas = true;
        ObjetivoActual = "Reune al grupo y llega al refugio, al final de la calle.";
    }

    public void LlegasteAlRefugio()
    {
        ObjetivoActual = string.Empty;
    }
}
