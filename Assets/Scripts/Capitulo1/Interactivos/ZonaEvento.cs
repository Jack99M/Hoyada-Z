using UnityEngine;

/// <summary>
/// Zona invisible (caja) que dispara acciones cuando Mateo entra: subtitulos, objetivos,
/// sustos, puntos de control, despertar infectados, cinematicas, etc.
/// </summary>
public class ZonaEvento : MonoBehaviour
{
    public Vector3 tamano = new Vector3(4f, 3f, 4f);
    public bool soloUnaVez = true;
    [Tooltip("Solo se dispara si el jugador tiene este objeto clave.")]
    public string requiereObjeto;
    [Tooltip("Solo se dispara si el jugador NO tiene este objeto clave.")]
    public string requiereSinObjeto;
    [Tooltip("Solo se dispara si existe este flag de la partida.")]
    public string requiereFlag;
    [Tooltip("Solo se dispara si NO existe este flag.")]
    public string requiereSinFlag;
    [Tooltip("Solo se dispara si la compañera esta con Mateo.")]
    public bool requiereCompanera;
    public float retraso;
    public Acciones acciones = new Acciones();

    public bool Disparada { get { return disparado; } }

    private bool disparado;
    private bool dentro;
    private float disparoEn = -1f;

    public void MarcarDisparada()
    {
        disparado = true;
    }

    private void Update()
    {
        if (disparoEn > 0f && Time.time >= disparoEn)
        {
            disparoEn = -1f;
            acciones.Ejecutar(transform.position);
        }

        if ((disparado && soloUnaVez) || !Juego.Control)
        {
            return;
        }

        Jugador j = Jugador.I;
        if (j == null || j.Muerto)
        {
            return;
        }

        Vector3 local = transform.InverseTransformPoint(j.transform.position + Vector3.up * 0.5f);
        bool ahora = Mathf.Abs(local.x) <= tamano.x * 0.5f && Mathf.Abs(local.y) <= tamano.y * 0.5f && Mathf.Abs(local.z) <= tamano.z * 0.5f;

        if (ahora && !dentro)
        {
            Juego g = Juego.I;
            bool cumple = (string.IsNullOrEmpty(requiereObjeto) || j.Inventario.Tiene(requiereObjeto))
                          && (string.IsNullOrEmpty(requiereSinObjeto) || !j.Inventario.Tiene(requiereSinObjeto))
                          && (string.IsNullOrEmpty(requiereFlag) || g.Flags.Contains(requiereFlag))
                          && (string.IsNullOrEmpty(requiereSinFlag) || !g.Flags.Contains(requiereSinFlag))
                          && (!requiereCompanera || (Companera.I != null && Companera.I.Siguiendo));
            if (cumple)
            {
                disparado = true;
                if (retraso > 0f) { disparoEn = Time.time + retraso; }
                else { acciones.Ejecutar(transform.position); }
            }
        }
        dentro = ahora;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, tamano);
    }
}
