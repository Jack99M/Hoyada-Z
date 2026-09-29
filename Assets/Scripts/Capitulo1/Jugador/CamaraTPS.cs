using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camara al hombro estilo The Last of Us / Resident Evil 2 Remake.
/// Mouse (o flechas / stick derecho) para mirar en todas direcciones, rueda para acercar.
/// Se acerca sola al chocar con paredes, baja al agacharse, abre el FOV al correr, se
/// cierra al apuntar, tiene retroceso al disparar y tiembla con los golpes.
/// Durante una cinematica la controla CinematicaCamara.
/// </summary>
[DefaultExecutionOrder(200)]
[RequireComponent(typeof(Camera))]
public class CamaraTPS : MonoBehaviour
{
    public Transform objetivo;

    [Header("Encuadre")]
    public float distancia = 3.1f;
    public Vector2 limitesDistancia = new Vector2(1.8f, 5f);
    public float alturaDePie = 1.6f;
    public float alturaAgachado = 1.05f;
    [Tooltip("Desplazamiento lateral (hombro derecho).")]
    public float hombro = 0.55f;
    public float fovNormal = 58f;
    public float fovCorriendo = 66f;

    [Header("Apuntar")]
    public float distanciaApuntando = 1.55f;
    public float hombroApuntando = 0.7f;
    public float fovApuntando = 42f;

    [Header("Control")]
    public float sensibilidadMouse = 0.12f;
    public float velocidadTeclas = 120f;
    public Vector2 limitesInclinacion = new Vector2(-35f, 70f);
    public bool invertirY;

    [Header("Colision")]
    public float radioColision = 0.22f;
    public LayerMask capasColision = ~((1 << 8) | (1 << 9) | (1 << 2));

    private static float sacudida;
    private static float sacudidaFin;
    private static float retroceso;

    private Camera cam;
    private Jugador jugador;
    private float yaw;
    private float pitch = 12f;
    private float distActual;
    private float alturaActual;
    private float hombroActual;
    private float apuntar;
    private Vector3 pivoteSuave;

    public float Yaw { get { return yaw; } }
    public float Pitch { get { return pitch; } }

    public static void Sacudir(float intensidad, float duracion)
    {
        sacudida = Mathf.Max(sacudida, intensidad);
        sacudidaFin = Mathf.Max(sacudidaFin, Time.time + duracion);
    }

    /// <summary>Patada del arma: sube la mira unos grados.</summary>
    public static void Retroceso(float grados)
    {
        retroceso += grados;
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (objetivo == null)
        {
            Jugador jj = FindFirstObjectByType<Jugador>();
            if (jj != null) { objetivo = jj.transform; }
        }
        if (objetivo != null) { jugador = objetivo.GetComponent<Jugador>(); }
        distActual = distancia;
        alturaActual = alturaDePie;
        hombroActual = hombro;
        sensibilidadMouse = Opciones.Sensibilidad;
        invertirY = Opciones.InvertirY;
        ColocarDetras();
    }

    public void ColocarDetras()
    {
        if (objetivo == null)
        {
            return;
        }
        yaw = objetivo.eulerAngles.y;
        pitch = 12f;
        pivoteSuave = objetivo.position + Vector3.up * alturaActual;
        Colocar(true);
    }

    public void MirarHacia(float nuevoYaw, float nuevoPitch)
    {
        yaw = nuevoYaw;
        pitch = Mathf.Clamp(nuevoPitch, limitesInclinacion.x, limitesInclinacion.y);
    }

    private void Update()
    {
        bool jugando = Juego.Control;
        bool capturar = jugando && Application.isFocused;
        Cursor.lockState = capturar ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !capturar;

        bool mirar = jugando || (Juego.I != null && Juego.I.estado == Juego.Estado.Muerto);
        if (!mirar || CinematicaCamara.Activa != null)
        {
            return;
        }

        Vector2 giro = Vector2.zero;
        Mouse m = Mouse.current;
        bool apuntando = jugador != null && jugador.Combate != null && jugador.Combate.Apuntando;
        float sens = sensibilidadMouse * (apuntando ? 0.6f : 1f);
        if (m != null && capturar)
        {
            giro += m.delta.ReadValue() * sens;
            float rueda = m.scroll.ReadValue().y;
            if (Mathf.Abs(rueda) > 0.01f)
            {
                distancia = Mathf.Clamp(distancia - Mathf.Sign(rueda) * 0.35f, limitesDistancia.x, limitesDistancia.y);
            }
        }

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            float h = (kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.leftArrowKey.isPressed ? 1f : 0f);
            float v = (kb.upArrowKey.isPressed ? 1f : 0f) - (kb.downArrowKey.isPressed ? 1f : 0f);
            giro.x += h * velocidadTeclas * Time.deltaTime;
            giro.y += v * velocidadTeclas * 0.6f * Time.deltaTime;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            giro += pad.rightStick.ReadValue() * velocidadTeclas * Time.deltaTime * (apuntando ? 0.6f : 1f);
        }

        yaw += giro.x;
        pitch += invertirY ? giro.y : -giro.y;
        if (retroceso > 0f)
        {
            float r = Mathf.Min(retroceso, Time.deltaTime * 40f);
            pitch -= r;
            retroceso -= r;
        }
        pitch = Mathf.Clamp(pitch, limitesInclinacion.x, limitesInclinacion.y);
    }

    private void LateUpdate()
    {
        if (objetivo == null || CinematicaCamara.Activa != null)
        {
            return;
        }
        Colocar(false);
    }

    private void Colocar(bool inmediato)
    {
        float dt = Time.deltaTime;
        bool agachado = jugador != null && jugador.Agachado;
        bool corriendo = jugador != null && jugador.Corriendo;
        bool muerto = jugador != null && jugador.Muerto;
        bool apuntando = jugador != null && jugador.Combate != null && jugador.Combate.Apuntando;
        apuntar = inmediato ? (apuntando ? 1f : 0f) : Mathf.MoveTowards(apuntar, apuntando ? 1f : 0f, dt * 6f);

        alturaActual = inmediato ? (agachado ? alturaAgachado : alturaDePie)
            : Mathf.Lerp(alturaActual, muerto ? 0.6f : (agachado ? alturaAgachado : alturaDePie), 1f - Mathf.Exp(-6f * dt));
        hombroActual = Mathf.Lerp(hombro, hombroApuntando, apuntar);

        Vector3 pivoteDeseado = objetivo.position + Vector3.up * alturaActual;
        pivoteSuave = inmediato ? pivoteDeseado : Vector3.Lerp(pivoteSuave, pivoteDeseado, 1f - Mathf.Exp(-16f * dt));

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 derecha = rot * Vector3.right;
        Vector3 atras = rot * Vector3.back;

        // Desplazamiento de hombro, recortado si hay pared al costado
        float lado = hombroActual;
        RaycastHit hit;
        if (Physics.SphereCast(pivoteSuave, radioColision, derecha, out hit, hombroActual, capasColision, QueryTriggerInteraction.Ignore))
        {
            lado = Mathf.Max(0f, hit.distance - 0.05f);
        }
        Vector3 origen = pivoteSuave + derecha * lado;

        float deseada = muerto ? distancia + 1.5f : Mathf.Lerp(distancia, distanciaApuntando, apuntar);
        float libre = deseada;
        if (Physics.SphereCast(origen, radioColision, atras, out hit, deseada, capasColision, QueryTriggerInteraction.Ignore))
        {
            libre = Mathf.Max(0.4f, hit.distance - 0.05f);
        }
        float vel = libre < distActual ? 30f : 5f;
        distActual = inmediato ? libre : Mathf.Lerp(distActual, libre, 1f - Mathf.Exp(-vel * dt));

        Vector3 pos = origen + atras * distActual;

        if (Time.time < sacudidaFin)
        {
            float k = sacudida * Mathf.Clamp01((sacudidaFin - Time.time) * 4f);
            pos += (Random.insideUnitSphere * 0.08f) * k;
            rot *= Quaternion.Euler(Random.Range(-2f, 2f) * k, Random.Range(-2f, 2f) * k, 0f);
        }
        else
        {
            sacudida = 0f;
        }

        transform.SetPositionAndRotation(pos, rot);

        if (cam != null)
        {
            float fov = Mathf.Lerp(corriendo ? fovCorriendo : fovNormal, fovApuntando, apuntar);
            cam.fieldOfView = inmediato ? fov : Mathf.Lerp(cam.fieldOfView, fov, 1f - Mathf.Exp(-8f * dt));
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
