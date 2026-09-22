using UnityEngine;

/// <summary>
/// Dispara el dilema del hombre atrapado (Acto IV) cuando Mateo se acerca.
/// Deteccion por distancia; colocar en un GameObject vacio en la avenida del Acto IV.
/// </summary>
public class DisparadorHombreAtrapado : MonoBehaviour
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
            disparado = true;
            if (DecisionController.Instancia != null)
            {
                DecisionController.Instancia.PresentarHombreAtrapado();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.3f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
