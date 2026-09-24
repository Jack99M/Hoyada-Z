using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de Mateo (Episodio I): movimiento WASD relativo a la camara, salto con
/// Espacio y linterna spot toggleable con F. Mateo gira hacia donde camina y la linterna
/// apunta hacia donde mira la camara. Usa el nuevo Input System (Keyboard.current).
/// Se congela mientras hay una decision o el final en curso.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MateoController : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento en m/s.")]
    public float speed = 5f;

    [Tooltip("Rapidez con la que Mateo gira hacia la direccion de movimiento.")]
    public float velocidadGiro = 12f;

    [Tooltip("Altura del salto en metros.")]
    public float jumpHeight = 1.5f;

    [Tooltip("Gravedad aplicada (valor negativo).")]
    public float gravity = -9.81f;

    [Header("Linterna")]
    [Tooltip("Luz spot que actua como linterna. Si se deja vacia, se crea automaticamente al iniciar.")]
    public Light flashlight;

    [Tooltip("Si esta activo, la linterna apunta hacia donde mira la camara.")]
    public bool linternaSigueCamara = true;

    private CharacterController controller;
    private Vector3 velocity;
    private Animator animator;
    private Transform camara;

    /// <summary>True si la linterna esta encendida (lo usa el HUD).</summary>
    public bool LinternaEncendida { get { return flashlight != null && flashlight.enabled; } }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (flashlight == null)
        {
            GameObject lampara = new GameObject("Linterna");
            lampara.transform.SetParent(transform);
            lampara.transform.localPosition = new Vector3(0f, 0.5f, 0.2f);
            lampara.transform.localRotation = Quaternion.identity;

            flashlight = lampara.AddComponent<Light>();
            flashlight.type = LightType.Spot;
            flashlight.range = 15f;
            flashlight.spotAngle = 45f;
            flashlight.intensity = 4f;
            flashlight.color = new Color(1f, 0.95f, 0.8f);
        }
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (!FlujoJuego.EnJuego)
        {
            return;
        }

        HandleMovement();
        HandleFlashlight();
    }

    private void LateUpdate()
    {
        if (linternaSigueCamara && flashlight != null && camara != null)
        {
            Vector3 dir = camara.forward;
            dir.y = Mathf.Min(dir.y, 0.05f) - 0.08f;
            flashlight.transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }

    private void HandleMovement()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        float h = 0f;
        float v = 0f;
        if (kb.aKey.isPressed) { h -= 1f; }
        if (kb.dKey.isPressed) { h += 1f; }
        if (kb.sKey.isPressed) { v -= 1f; }
        if (kb.wKey.isPressed) { v += 1f; }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            Vector2 stick = pad.leftStick.ReadValue();
            h += stick.x;
            v += stick.y;
        }

        // Adelante/derecha segun hacia donde mira la camara (en el plano del suelo).
        Vector3 adelante = transform.forward;
        Vector3 derecha = transform.right;
        if (camara != null)
        {
            adelante = Vector3.ProjectOnPlane(camara.forward, Vector3.up).normalized;
            derecha = Vector3.ProjectOnPlane(camara.right, Vector3.up).normalized;
        }

        Vector3 move = derecha * h + adelante * v;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * speed * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion objetivoRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, objetivoRot, 1f - Mathf.Exp(-velocidadGiro * Time.deltaTime));
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", move.magnitude * speed);
            animator.SetBool("Grounded", controller.isGrounded);
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            bool saltar = kb.spaceKey.wasPressedThisFrame || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            if (saltar)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleFlashlight()
    {
        Keyboard kb = Keyboard.current;
        if (flashlight != null && kb != null && kb.fKey.wasPressedThisFrame)
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}
