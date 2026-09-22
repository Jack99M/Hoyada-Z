using UnityEngine;

/// <summary>
/// Estado del grupo de sobrevivientes de Hoyada Z (Episodio I).
/// Centraliza Moral, Recursos y Salud, y aplica los efectos de cada decision del jugador.
/// Es el nucleo sobre el que se conecta el sistema de dialogos y elecciones.
/// </summary>
public class GrupoEstado : MonoBehaviour
{
    public static GrupoEstado Instancia { get; private set; }

    [Header("Moral del grupo (0-100)")]
    [Range(0, 100)] public int moral = 70;

    [Header("Recursos disponibles")]
    [Min(0)] public int comida = 5;
    [Min(0)] public int agua = 5;
    [Min(0)] public int medicina = 3;
    [Min(0)] public int abrigo = 2;
    [Min(0)] public int municion = 0;

    [Header("Salud de Mateo (0-100)")]
    [Range(0, 100)] public int salud = 100;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    /// <summary>Aplica los efectos de una decision y reporta el nuevo estado por consola.</summary>
    public void AplicarDecision(DecisionEpisodio decision)
    {
        if (decision == null)
        {
            return;
        }

        moral = Mathf.Clamp(moral + decision.moral, 0, 100);
        salud = Mathf.Clamp(salud + decision.salud, 0, 100);
        comida = Mathf.Max(0, comida + decision.comida);
        agua = Mathf.Max(0, agua + decision.agua);
        medicina = Mathf.Max(0, medicina + decision.medicina);
        abrigo = Mathf.Max(0, abrigo + decision.abrigo);
        municion = Mathf.Max(0, municion + decision.municion);

        Debug.Log("[Hoyada Z] Decision '" + decision.titulo + "' aplicada -> " +
                  "Moral:" + moral + " Salud:" + salud + " | Comida:" + comida +
                  " Agua:" + agua + " Medicina:" + medicina + " Abrigo:" + abrigo +
                  " Municion:" + municion);
    }

    /// <summary>True si el grupo entra en crisis (moral o salud demasiado bajas).</summary>
    public bool EnCrisis()
    {
        return moral <= 20 || salud <= 20;
    }
}
