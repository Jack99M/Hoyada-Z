using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Prueba automatica del recorrido (QA): hace caminar a Mateo con su movimiento real
/// (CharacterController, escalones y salto) por una lista de puntos siguiendo el navmesh
/// y reporta en que tramo se queda atascado. Sirve para encontrar puertas tapadas,
/// escalones demasiado altos o pasillos cerrados sin tener que jugar todo el capitulo.
/// Uso: PruebaRecorrido.Iniciar(puntos, nombres, 3f) y luego leer PruebaRecorrido.Reporte.
/// No se usa en el juego normal.
/// </summary>
public class PruebaRecorrido : MonoBehaviour
{
    public static PruebaRecorrido I { get; private set; }
    public static string Reporte = "";
    public static bool Terminada = true;
    public static int Fallas;

    private Vector3[] puntos;
    private string[] nombres;
    private float escala = 1f;

    public static void Iniciar(Vector3[] puntos, string[] nombres, float escalaTiempo)
    {
        if (I != null) { Destroy(I.gameObject); }
        var go = new GameObject("_PruebaRecorrido");
        I = go.AddComponent<PruebaRecorrido>();
        I.puntos = puntos;
        I.nombres = nombres;
        I.escala = Mathf.Clamp(escalaTiempo, 0.5f, 4f);
        Reporte = "";
        Terminada = false;
        Fallas = 0;
        I.StartCoroutine(I.Correr());
    }

    private void OnDestroy()
    {
        if (I == this) { I = null; }
        if (Jugador.I != null) { Jugador.I.moverBot = Vector3.zero; }
    }

    private IEnumerator Correr()
    {
        var sb = new StringBuilder();
        Jugador j = Jugador.I;
        var path = new NavMeshPath();
        for (int i = 0; i < puntos.Length && j != null; i++)
        {
            string nombre = nombres != null && i < nombres.Length ? nombres[i] : ("P" + i);
            Vector3 destino = puntos[i];
            NavMeshHit h;
            if (NavMesh.SamplePosition(destino, out h, 2f, NavMesh.AllAreas)) { destino = h.position; }

            float t0 = Time.time;
            int saltos = 0;
            bool llego = false;
            Vector3 ultimaPos = j.transform.position;
            float ultimoAvance = Time.time;
            int esquina = 1;
            float siguienteRuta = 0f;
            float maxAltura = j.transform.position.y;

            while (Time.time - t0 < 100f)
            {
                Time.timeScale = escala;
                Vector3 pos = j.transform.position;
                maxAltura = Mathf.Max(maxAltura, pos.y);
                if (Plano(destino - pos).magnitude < 0.7f && Mathf.Abs(destino.y - pos.y) < 1.2f)
                {
                    llego = true;
                    break;
                }
                if (Time.time >= siguienteRuta)
                {
                    siguienteRuta = Time.time + 0.5f;
                    NavMeshHit hp;
                    Vector3 origen = NavMesh.SamplePosition(pos, out hp, 1.5f, NavMesh.AllAreas) ? hp.position : pos;
                    NavMesh.CalculatePath(origen, destino, NavMesh.AllAreas, path);
                    esquina = 1;
                }
                Vector3 meta = destino;
                if (path.corners != null && path.corners.Length > 1)
                {
                    while (esquina < path.corners.Length - 1 && Plano(path.corners[esquina] - pos).magnitude < 0.45f) { esquina++; }
                    meta = path.corners[Mathf.Min(esquina, path.corners.Length - 1)];
                }
                Vector3 d = Plano(meta - pos);
                j.moverBot = d.sqrMagnitude > 0.0025f ? d.normalized : Plano(destino - pos).normalized;

                if (Plano(pos - ultimaPos).magnitude > 0.35f)
                {
                    ultimaPos = pos;
                    ultimoAvance = Time.time;
                }
                else if (Time.time - ultimoAvance > 1.2f)
                {
                    if (saltos >= 4) { break; }
                    j.saltarBot = true;
                    saltos++;
                    ultimoAvance = Time.time;
                }
                yield return null;
            }
            j.moverBot = Vector3.zero;
            float dt = Time.time - t0;
            if (!llego) { Fallas++; }
            sb.AppendLine((llego ? "OK    " : "FALLA ") + nombre + "   " + dt.ToString("F1") + " s   saltos " + saltos + "   ruta " + path.status
                + (llego ? "" : "   quedo en " + j.transform.position.ToString("F1") + " a " + Vector3.Distance(j.transform.position, destino).ToString("F1") + " m"));
            Reporte = sb.ToString();
            if (!llego)
            {
                j.Teletransportar(destino + Vector3.up * 0.1f, j.transform.rotation);
                yield return null;
            }
        }
        Time.timeScale = 1f;
        Terminada = true;
        Reporte = sb.ToString();
        Debug.Log("Prueba de recorrido (" + Fallas + " fallas):\n" + Reporte);
    }

    private static Vector3 Plano(Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }
}
