using UnityEngine;

/// <summary>
/// Beat narrativo "El primer infectado" (Acto III). Al acercarse Mateo, aplica el
/// impacto del primer ataque (salud/moral) y lanza una narracion scriptada.
/// El marcador visual es temporal: se reemplazara por el personaje char_infectado.
/// </summary>
public class PrimerInfectado : MonoBehaviour
{
    [Tooltip("Radio de activacion en metros.")]
    [Min(0.5f)] public float radio = 5f;

    private Transform jugador;
    private bool disparado;

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
        if (disparado || jugador == null)
        {
            return;
        }

        if (Vector3.Distance(jugador.position, transform.position) <= radio)
        {
            Disparar();
        }
    }

private void Disparar()
    {
        disparado = true;

        if (GrupoEstado.Instancia != null)
        {
            GrupoEstado.Instancia.AplicarDecision(new DecisionEpisodio
            {
                titulo = "El primer infectado",
                salud = -10,
                moral = -8
            });
        }

        if (DialogoController.Instancia != null)
        {
            string[] lineas =
            {
                "Alguien del grupo susurra: 'Esta enfermo.' El hombre tose, tiembla, desorientado.",
                "Un ruido. Gira de golpe hacia ustedes. Silencio. Y ataca.",
                "Esto no es un simple disturbio. Corran hacia el refugio."
            };
            DialogoController.Instancia.Iniciar("...", lineas, null);
        }

        InfectadoPerseguidor perseguidor = GetComponent<InfectadoPerseguidor>();
        if (perseguidor != null)
        {
            perseguidor.Activar();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.1f, 0.1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
