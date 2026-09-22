using UnityEngine;

/// <summary>
/// Inicia la conversacion con Mama Bety cuando Mateo se acerca lo suficiente.
/// Deteccion por distancia; colocar en el GameObject de Mama Bety.
/// </summary>
public class DisparadorDialogoBety : MonoBehaviour
{
    [Tooltip("Radio de activacion en metros.")]
    [Min(0.5f)] public float radio = 4f;

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
            if (DialogoController.Instancia != null)
            {
                DialogoController.Instancia.IniciarConversacionBety();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.5f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
