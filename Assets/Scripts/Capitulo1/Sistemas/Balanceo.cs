using UnityEngine;

/// <summary>
/// Balanceo suave (cabinas del teleferico detenidas, banderas, carteles colgados).
/// </summary>
public class Balanceo : MonoBehaviour
{
    public Vector3 eje = Vector3.forward;
    public float amplitud = 3f;
    public float velocidad = 0.6f;
    private Quaternion inicial;
    private float fase;

    private void Start()
    {
        inicial = transform.localRotation;
        fase = Random.value * 10f;
    }

    private void Update()
    {
        float a = Mathf.Sin(Time.time * velocidad + fase) * amplitud + Mathf.Sin(Time.time * velocidad * 2.3f + fase * 0.5f) * amplitud * 0.3f;
        transform.localRotation = inicial * Quaternion.AngleAxis(a, eje);
    }
}
