using UnityEngine;

/// <summary>
/// Sonido ambiental 3D en bucle (parlante de la discoteca, fuego, radio, sirena lejana).
/// Toma el clip por nombre desde AudioCap1.
/// </summary>
public class FuenteSonido : MonoBehaviour
{
    public string clip = "amb_fuego";
    [Range(0f, 1f)] public float volumen = 0.6f;
    public float distanciaMin = 2f;
    public float distanciaMax = 25f;
    [Tooltip("0 = 2D, 1 = totalmente 3D.")]
    [Range(0f, 1f)] public float espacial = 1f;

    private AudioSource fuente;

    private void Start()
    {
        fuente = gameObject.AddComponent<AudioSource>();
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.spatialBlend = espacial;
        fuente.rolloffMode = AudioRolloffMode.Linear;
        fuente.minDistance = distanciaMin;
        fuente.maxDistance = distanciaMax;
        fuente.dopplerLevel = 0f;
        fuente.volume = volumen;
        AudioClip c = AudioCap1.I != null ? AudioCap1.I.Clip(clip) : null;
        if (c != null)
        {
            fuente.clip = c;
            fuente.time = Random.Range(0f, c.length);
            fuente.Play();
        }
    }
}
