using UnityEngine;

/// <summary>
/// Eleccion de provisiones del Acto II "caminando": cada destino (mercado / farmacia /
/// casa) tiene una zona; al entrar Mateo se aplica su efecto y se bloquea el resto
/// (solo se elige uno). Reemplaza al panel de botones por una decision espacial.
/// </summary>
public class EleccionProvisiones : MonoBehaviour
{
    public enum Lugar { Mercado, Farmacia, Casa }

    [Tooltip("Que destino representa esta zona.")]
    public Lugar lugar = Lugar.Mercado;

    [Tooltip("Radio de activacion en metros.")]
    [Min(0.5f)] public float radio = 4f;

    private static bool yaElegido;
    private Transform jugador;
    private bool disparado;

    /// <summary>Reinicia la eleccion (llamar al recargar la escena / reiniciar partida).</summary>
    public static void ReiniciarEleccion()
    {
        yaElegido = false;
    }

    private void Start()
    {
        MateoController mc = Object.FindFirstObjectByType<MateoController>();
        if (mc != null)
        {
            jugador = mc.transform;
        }
    }

    private void Update()
    {
        if (disparado || yaElegido || jugador == null)
        {
            return;
        }

        if (Vector3.Distance(jugador.position, transform.position) <= radio)
        {
            Elegir();
        }
    }

    private void Elegir()
    {
        disparado = true;
        yaElegido = true;

        DecisionEpisodio efecto;
        string linea;

        switch (lugar)
        {
            case Lugar.Farmacia:
                efecto = new DecisionEpisodio { titulo = "Farmacia", medicina = 3, moral = -5, salud = -8 };
                linea = "La farmacia esta medio saqueada; consigues medicinas entre heridos.";
                break;
            case Lugar.Casa:
                efecto = new DecisionEpisodio { titulo = "Casa de un conocido", abrigo = 2, comida = 1, moral = 5 };
                linea = "En casa del conocido hay poco, pero estan a salvo un momento.";
                break;
            default:
                efecto = new DecisionEpisodio { titulo = "Mercado", comida = 3, agua = 2, moral = -8, salud = -5 };
                linea = "En el mercado hay de todo, pero la gente esta al borde del panico.";
                break;
        }

        if (GrupoEstado.Instancia != null)
        {
            GrupoEstado.Instancia.AplicarDecision(efecto);
        }

        if (DialogoController.Instancia != null)
        {
            DialogoController.Instancia.Iniciar("Provisiones", new string[] { linea }, null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 0.9f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
