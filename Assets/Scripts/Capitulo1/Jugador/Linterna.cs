using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Linterna de Mateo con bateria: F encender/apagar, R cambiar pilas.
/// Apunta hacia donde mira la camara. Encendida hace que los infectados te vean de mas lejos.
/// </summary>
[DefaultExecutionOrder(90)]
public class Linterna : MonoBehaviour
{
    public Light luz;
    [Range(0f, 100f)] public float bateria = 100f;
    [Tooltip("Porcentaje de bateria por segundo encendida.")]
    public float consumo = 0.75f;
    [Tooltip("Porcentaje que recarga un par de pilas.")]
    public float recargaPorPila = 65f;
    public float intensidad = 7f;
    public Vector3 offsetPecho = new Vector3(0.18f, 1.42f, 0.25f);

    public bool Encendida { get { return luz != null && luz.enabled; } }

    private Jugador j;
    private Inventario inv;
    private Transform cam;
    private bool avisoBateria;

    private void Awake()
    {
        j = GetComponent<Jugador>();
        inv = GetComponent<Inventario>();
        if (luz != null) { luz.enabled = false; }
    }

    private void Start()
    {
        if (Camera.main != null) { cam = Camera.main.transform; }
    }

    private void Update()
    {
        if (luz == null)
        {
            return;
        }

        Keyboard kb = Keyboard.current;
        bool control = Juego.Control && (j == null || !j.Muerto);

        if (control && kb != null && kb.fKey.wasPressedThisFrame)
        {
            if (inv != null && !inv.tieneLinterna)
            {
                Juego.I.Mensaje("No tengo linterna");
            }
            else if (!luz.enabled && bateria <= 0f)
            {
                if (!CambiarPilas()) { Juego.I.Mensaje("Sin batería. Necesito pilas"); }
            }
            else
            {
                luz.enabled = !luz.enabled;
                AudioCap1.Play3D("sfx_click", transform.position, 0.5f);
            }
        }

        if (control && kb != null && kb.rKey.wasPressedThisFrame && (j == null || j.Combate == null || !j.Combate.UsaTeclaR))
        {
            if (inv != null && inv.tieneLinterna)
            {
                if (bateria > 95f) { Juego.I.Mensaje("La batería está llena"); }
                else if (!CambiarPilas()) { Juego.I.Mensaje("No tengo pilas"); }
            }
        }

        if (luz.enabled)
        {
            bateria -= consumo * Time.deltaTime;
            if (bateria <= 15f && !avisoBateria)
            {
                avisoBateria = true;
                Juego.I.Mensaje("La linterna se está quedando sin batería  [R] cambiar pilas");
            }
            if (bateria <= 0f)
            {
                bateria = 0f;
                luz.enabled = false;
                AudioCap1.Play3D("sfx_click", transform.position, 0.5f);
                Juego.I.Mensaje("Se acabó la batería");
            }

            float parpadeo = 1f;
            if (bateria < 15f)
            {
                float n = Mathf.PerlinNoise(Time.time * 9f, 0f);
                parpadeo = n < 0.3f ? 0.15f : Mathf.Lerp(0.6f, 1f, n);
            }
            luz.intensity = intensidad * parpadeo;
        }
    }

    private bool CambiarPilas()
    {
        if (inv == null || inv.pilas <= 0)
        {
            return false;
        }
        inv.pilas--;
        bateria = Mathf.Min(100f, bateria + recargaPorPila);
        avisoBateria = false;
        AudioCap1.Play3D("sfx_recoger", transform.position, 0.6f);
        Juego.I.Mensaje("Cambias las pilas de la linterna");
        return true;
    }

    private void LateUpdate()
    {
        if (luz == null)
        {
            return;
        }
        if (cam == null && Camera.main != null) { cam = Camera.main.transform; }

        Transform t = luz.transform;
        t.position = transform.TransformPoint(j != null && j.Agachado ? new Vector3(offsetPecho.x, offsetPecho.y - 0.5f, offsetPecho.z) : offsetPecho);
        Vector3 dir = cam != null ? cam.forward : transform.forward;
        dir.y -= 0.06f;
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(dir.normalized), 1f - Mathf.Exp(-18f * Time.deltaTime));
    }
}
