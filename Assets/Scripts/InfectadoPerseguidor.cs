using UnityEngine;

/// <summary>
/// Infectado que persigue a Mateo una vez activado (tras el susto del Acto III).
/// Se mueve hacia el jugador en el plano y le hace dano por contacto.
/// Es mas lento que Mateo: se puede escapar corriendo al refugio.
/// </summary>
public class InfectadoPerseguidor : MonoBehaviour
{
    [Tooltip("Velocidad de persecucion (menor que la de Mateo).")]
    public float velocidad = 2.2f;

    [Tooltip("Distancia a la que empieza a hacer dano.")]
    public float distanciaContacto = 1.7f;

    [Tooltip("Dano por mordida.")]
    public int danoContacto = 6;

    [Tooltip("Segundos entre mordidas.")]
    public float intervaloDano = 1.2f;

    private bool activo;
    private Transform jugador;
    private float t;

    public void Activar()
    {
        activo = true;
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
        if (!activo || jugador == null || !FlujoJuego.EnJuego)
        {
            return;
        }

        DecisionController d = DecisionController.Instancia;
        if (d != null && (d.FinActivo || d.GameOverActivo))
        {
            return;
        }

        Vector3 destino = new Vector3(jugador.position.x, transform.position.y, jugador.position.z);
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

        Vector3 dir = destino - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
        }

        float dist = Vector3.Distance(transform.position, jugador.position);
        if (dist <= distanciaContacto)
        {
            t += Time.deltaTime;
            if (t >= intervaloDano)
            {
                t = 0f;
                if (GrupoEstado.Instancia != null)
                {
                    GrupoEstado.Instancia.salud = Mathf.Max(0, GrupoEstado.Instancia.salud - danoContacto);
                }
            }
        }
        else
        {
            t = 0f;
        }
    }
}
