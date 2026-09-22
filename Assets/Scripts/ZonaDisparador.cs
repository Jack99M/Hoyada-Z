using UnityEngine;

/// <summary>
/// Zona que dispara un evento del episodio cuando Mateo se acerca lo suficiente.
/// Usa deteccion por distancia (no requiere Rigidbody ni colliders trigger).
/// Coloca este componente en un GameObject vacio y ajusta 'tipo' y 'radio' en el inspector.
/// </summary>
public class ZonaDisparador : MonoBehaviour
{
    public enum Tipo
    {
        Provisiones,
        FinEpisodio
    }

    [Tooltip("Que evento dispara esta zona al entrar Mateo.")]
    public Tipo tipo = Tipo.Provisiones;

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

        if (DecisionController.Instancia == null)
        {
            return;
        }

        if (tipo == Tipo.Provisiones)
        {
            DecisionController.Instancia.PresentarProvisiones();
        }
        else
        {
            DecisionController.Instancia.FinEpisodio();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
