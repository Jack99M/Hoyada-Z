using UnityEngine;

/// <summary>
/// Camara en tercera persona que sigue a un objetivo (Mateo) con un offset y suavizado.
/// Si no se asigna objetivo, busca automaticamente al MateoController en escena.
/// </summary>
public class CamaraSeguidor : MonoBehaviour
{
    [Tooltip("Objetivo a seguir. Si se deja vacio, se busca a Mateo automaticamente.")]
    public Transform objetivo;

    [Tooltip("Desplazamiento de la camara respecto al objetivo (detras y arriba).")]
    public Vector3 offset = new Vector3(0f, 3f, -5f);

    [Tooltip("Punto al que mira la camara, relativo al objetivo.")]
    public Vector3 miradaOffset = new Vector3(0f, 1.5f, 0f);

    [Tooltip("Suavizado del seguimiento (mayor = mas pegado).")]
    [Min(0.1f)] public float suavizado = 8f;

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

        if (objetivo != null)
        {
            transform.position = objetivo.position + offset;
            transform.LookAt(objetivo.position + miradaOffset);
        }
    }

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            return;
        }

        Vector3 posDeseada = objetivo.position + offset;
        float t = 1f - Mathf.Exp(-suavizado * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, posDeseada, t);
        transform.LookAt(objetivo.position + miradaOffset);
    }
}
