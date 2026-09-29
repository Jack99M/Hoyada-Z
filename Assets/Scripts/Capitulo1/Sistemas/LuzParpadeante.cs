using UnityEngine;

/// <summary>
/// Luces con vida: foco que falla, estroboscopio de discoteca, fuego o colores que rotan.
/// </summary>
[RequireComponent(typeof(Light))]
public class LuzParpadeante : MonoBehaviour
{
    public enum Modo { Falla, Estrobo, Fuego, Colores }

    public Modo modo = Modo.Falla;
    public float velocidad = 1f;
    public Color[] colores;

    private Light luz;
    private float baseIntensidad;
    private Color baseColor;
    private float semilla;

    private void Awake()
    {
        luz = GetComponent<Light>();
        baseIntensidad = luz.intensity;
        baseColor = luz.color;
        semilla = Random.value * 100f;
    }

    private void Update()
    {
        float t = Time.time * velocidad + semilla;
        switch (modo)
        {
            case Modo.Falla:
                float n = Mathf.PerlinNoise(t * 3f, 0.5f);
                float corte = Mathf.PerlinNoise(t * 0.7f, 3.3f);
                luz.intensity = corte > 0.62f ? (n > 0.5f ? baseIntensidad * 0.08f : baseIntensidad) : baseIntensidad * Mathf.Lerp(0.85f, 1f, n);
                break;
            case Modo.Estrobo:
                bool pausa = Mathf.PerlinNoise(t * 0.4f, 7f) > 0.6f;
                luz.intensity = !pausa && Mathf.Repeat(t * 9f, 1f) < 0.3f ? baseIntensidad : 0f;
                break;
            case Modo.Fuego:
                float f = Mathf.PerlinNoise(t * 6f, 1.7f);
                luz.intensity = baseIntensidad * Mathf.Lerp(0.55f, 1.25f, f);
                luz.color = Color.Lerp(baseColor, new Color(1f, 0.45f, 0.1f), Mathf.PerlinNoise(t * 3f, 9f) * 0.5f);
                break;
            case Modo.Colores:
                if (colores != null && colores.Length > 0)
                {
                    float k = Mathf.Repeat(t * 0.5f, colores.Length);
                    int a = Mathf.FloorToInt(k);
                    luz.color = Color.Lerp(colores[a], colores[(a + 1) % colores.Length], k - a);
                }
                luz.intensity = baseIntensidad * Mathf.Lerp(0.6f, 1f, Mathf.PerlinNoise(t * 2f, 0f));
                break;
        }
    }
}
