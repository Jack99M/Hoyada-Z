using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camara en tercera persona con orbita libre alrededor de Mateo.
/// - Mouse: mirar a izquierda/derecha y arriba/abajo (tambien flechas o stick derecho).
/// - Rueda del mouse: acercar / alejar.
/// - Esc: soltar el mouse; clic izquierdo en el juego: volver a capturarlo.
/// Evita atravesar paredes acercandose al jugador cuando hay un obstaculo detras.
/// El cursor se libera solo cuando hay menu, dialogo, decision o confirmacion en pantalla.
/// </summary>
public class CamaraSeguidor : MonoBehaviour
{
    [Tooltip("Objetivo a seguir. Si se deja vacio, se busca a Mateo automaticamente.")]
    public Transform objetivo;

    [Header("Orbita")]
    [Tooltip("Altura del punto que mira la camara, sobre el centro del objetivo (hombros).")]
    public float alturaPivote = 0.6f;

    [Tooltip("Distancia inicial de la camara al jugador.")]
    public float distancia = 5f;

    [Tooltip("Distancia minima y maxima al hacer zoom con la rueda.")]
    public Vector2 limitesDistancia = new Vector2(2.5f, 8f);

    [Tooltip("Inclinacion inicial (grados, positivo = mirando hacia abajo).")]
    public float inclinacionInicial = 18f;

    [Tooltip("Limites de inclinacion vertical en grados.")]
    public Vector2 limitesInclinacion = new Vector2(-20f, 65f);

    [Header("Sensibilidad")]
    [Tooltip("Grados por pixel de movimiento del mouse.")]
    public float sensibilidadMouse = 0.15f;

    [Tooltip("Grados por segundo al girar con flechas o gamepad.")]
    public float velocidadTeclas = 120f;

    public bool invertirY = false;

    [Header("Suavizado y colision")]
    [Tooltip("Suavizado del seguimiento (mayor = mas pegado).")]
    [Min(0.1f)] public float suavizado = 12f;

    [Tooltip("Radio usado para detectar paredes entre la camara y el jugador.")]
    public float radioColision = 0.25f;

    public LayerMask capasColision = ~0;

    private float yaw;
    private float pitch;
    private float distanciaActual;
    private Vector3 pivoteSuave;
    private bool mouseLiberadoManual;

    /// <summary>Angulo horizontal actual de la camara (lo usa el controlador de Mateo).</summary>
    public float Yaw { get { return yaw; } }

    private void Start()
    {
        if (objetivo == null)
        {
            MateoController mc = Object.FindFirstObjectByType<MateoController>();
            if (mc != null)
            {
                objetivo = mc.transform;
            }
        }

        yaw = objetivo != null ? objetivo.eulerAngles.y : transform.eulerAngles.y;
        pitch = inclinacionInicial;
        distanciaActual = distancia;

        if (objetivo != null)
        {
            pivoteSuave = objetivo.position + Vector3.up * alturaPivote;
            ColocarCamara(true);
        }
    }

    private void Update()
    {
        bool jugando = FlujoJuego.EnJuego;
        ActualizarCursor(jugando);

        if (!jugando || mouseLiberadoManual)
        {
            return;
        }

        Vector2 giro = Vector2.zero;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            giro += mouse.delta.ReadValue() * sensibilidadMouse;

            float rueda = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(rueda) > 0.01f)
            {
                distancia = Mathf.Clamp(distancia - Mathf.Sign(rueda) * 0.5f, limitesDistancia.x, limitesDistancia.y);
            }
        }

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            float teclas = 0f;
            if (kb.leftArrowKey.isPressed) { teclas -= 1f; }
            if (kb.rightArrowKey.isPressed) { teclas += 1f; }
            giro.x += teclas * velocidadTeclas * Time.deltaTime;

            float vertical = 0f;
            if (kb.upArrowKey.isPressed) { vertical += 1f; }
            if (kb.downArrowKey.isPressed) { vertical -= 1f; }
            giro.y += vertical * velocidadTeclas * 0.6f * Time.deltaTime;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            giro += pad.rightStick.ReadValue() * velocidadTeclas * Time.deltaTime;
        }

        yaw += giro.x;
        pitch += invertirY ? giro.y : -giro.y;
        pitch = Mathf.Clamp(pitch, limitesInclinacion.x, limitesInclinacion.y);
    }

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            return;
        }

        ColocarCamara(false);
    }

    private void ColocarCamara(bool inmediato)
    {
        Vector3 pivoteDeseado = objetivo.position + Vector3.up * alturaPivote;
        float t = inmediato ? 1f : 1f - Mathf.Exp(-suavizado * Time.deltaTime);
        pivoteSuave = Vector3.Lerp(pivoteSuave, pivoteDeseado, t);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 atras = rot * Vector3.back;

        // Si hay una pared entre el jugador y la camara, acercarla.
        float libre = distancia;
        RaycastHit[] hits = Physics.SphereCastAll(pivoteSuave, radioColision, atras, distancia, capasColision, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == objetivo || hits[i].transform.IsChildOf(objetivo))
            {
                continue;
            }

            if (hits[i].distance > 0f && hits[i].distance < libre)
            {
                libre = hits[i].distance;
            }
        }
        libre = Mathf.Max(libre - 0.1f, 0.6f);

        // Acercarse rapido al chocar, alejarse suave al liberar.
        float vel = libre < distanciaActual ? 25f : 4f;
        distanciaActual = inmediato ? libre : Mathf.Lerp(distanciaActual, libre, 1f - Mathf.Exp(-vel * Time.deltaTime));

        transform.position = pivoteSuave + atras * distanciaActual;
        transform.rotation = rot;
    }

    private void ActualizarCursor(bool jugando)
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            mouseLiberadoManual = true;
        }
        else if (mouseLiberadoManual && jugando && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            mouseLiberadoManual = false;
        }

        bool capturar = jugando && !mouseLiberadoManual && Application.isFocused;
        Cursor.lockState = capturar ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !capturar;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
