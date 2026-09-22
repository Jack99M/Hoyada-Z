using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de Mateo (Episodio I): movimiento WASD, salto con Espacio y linterna
/// spot toggleable con F. Usa el nuevo Input System (Keyboard.current), acorde a la
/// configuracion del proyecto. Se congela mientras hay una decision o el final en curso.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MateoController : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento en m/s.")]
    public float speed = 5f;

    [Tooltip("Altura del salto en metros.")]
    public float jumpHeight = 1.5f;

    [Tooltip("Gravedad aplicada (valor negativo).")]
    public float gravity = -9.81f;

    [Header("Linterna")]
    [Tooltip("Luz spot que actua como linterna. Si se deja vacia, se crea automaticamente al iniciar.")]
    public Light flashlight;

    private CharacterController controller;
    private Vector3 velocity;
    private Animator animator;

private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (flashlight == null)
        {
            GameObject lampara = new GameObject("Linterna");
            lampara.transform.SetParent(transform);
            lampara.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            lampara.transform.localRotation = Quaternion.identity;

            flashlight = lampara.AddComponent<Light>();
            flashlight.type = LightType.Spot;
            flashlight.range = 15f;
            flashlight.spotAngle = 45f;
            flashlight.intensity = 4f;
            flashlight.color = new Color(1f, 0.95f, 0.8f);
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

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * speed * Time.deltaTime);

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

            if (kb.spaceKey.wasPressedThisFrame)
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
