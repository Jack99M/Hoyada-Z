using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cinematica en el motor: una serie de tomas de camara (posicion y punto al que mira,
/// con movimiento de inicio a fin), franjas negras y subtitulos. Se salta con [Espacio].
/// Al terminar devuelve el control (y puede iniciar un jefe).
/// </summary>
public class CinematicaCamara : MonoBehaviour
{
    [System.Serializable]
    public class Toma
    {
        public Vector3 desde;
        public Vector3 hasta;
        public Vector3 mirarDesde;
        public Vector3 mirarHasta;
        public float duracion = 3f;
        public float fov = 50f;
        [Tooltip("Subtitulo 'Quien|Texto' al empezar la toma.")]
        public string subtitulo;
        public string sonido;
    }

    public static CinematicaCamara Activa { get; private set; }

    public Toma[] tomas;
    public bool unaVez = true;
    public bool Vista { get; private set; }

    public void Reproducir(Infectado jefe = null)
    {
        if ((Vista && unaVez) || Juego.I == null || Activa != null)
        {
            if (jefe != null) { jefe.IniciarJefe(); }
            return;
        }
        StartCoroutine(Rutina(jefe));
    }

    private IEnumerator Rutina(Infectado jefe)
    {
        Vista = true;
        Activa = this;
        Juego.I.EmpezarCinematica();
        Camera cam = Camera.main;
        float fovAntes = cam != null ? cam.fieldOfView : 58f;
        float total = 0f;
        bool saltar = false;

        foreach (Toma t in tomas)
        {
            if (!string.IsNullOrEmpty(t.subtitulo))
            {
                string[] p = t.subtitulo.Split('|');
                if (p.Length >= 2) { Juego.I.Decir(p[0], p[1], Mathf.Max(2.4f, t.duracion)); }
            }
            if (!string.IsNullOrEmpty(t.sonido)) { AudioCap1.Play2D(t.sonido, 0.9f); }
            for (float k = 0f; k < t.duracion; k += Time.deltaTime)
            {
                float u = Mathf.SmoothStep(0f, 1f, k / t.duracion);
                if (cam != null)
                {
                    Vector3 pos = Vector3.Lerp(t.desde, t.hasta, u);
                    Vector3 mira = Vector3.Lerp(t.mirarDesde, t.mirarHasta, u);
                    cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(mira - pos));
                    cam.fieldOfView = t.fov;
                }
                total += Time.deltaTime;
                Keyboard kb = Keyboard.current;
                if (total > 0.8f && kb != null && (kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                {
                    saltar = true;
                    break;
                }
                yield return null;
            }
            if (saltar) { break; }
        }

        if (cam != null) { cam.fieldOfView = fovAntes; }
        Activa = null;
        Juego.I.TerminarCinematica();
        if (jefe != null) { jefe.IniciarJefe(); }
    }

    private void OnDisable()
    {
        if (Activa == this) { Activa = null; }
    }
}
