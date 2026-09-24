using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio del capitulo: efectos 2D/3D con un pool de fuentes, musica con fundidos,
/// capa de tension cuando un infectado persigue y latido con salud baja.
/// Los clips se generan por codigo (GeneradorAudio) y se asignan en el inspector.
/// </summary>
public class AudioCap1 : MonoBehaviour
{
    public static AudioCap1 I { get; private set; }

    public AudioClip[] clips;
    [Range(0f, 1f)] public float volumenEfectos = 0.9f;
    [Range(0f, 1f)] public float volumenMusica = 0.45f;
    public string ambiente = "amb_viento";
    public string musicaTension = "mus_tension";

    private readonly Dictionary<string, AudioClip> mapa = new Dictionary<string, AudioClip>();
    private readonly List<AudioSource> pool = new List<AudioSource>();
    private AudioSource fuente2D;
    private AudioSource musica;
    private AudioSource tension;
    private AudioSource amb;
    private AudioSource latido;
    private float volMusicaObjetivo;
    private float velFundido = 1f;

    private void Awake()
    {
        I = this;
        foreach (AudioClip c in clips)
        {
            if (c != null) { mapa[c.name] = c; }
        }

        fuente2D = gameObject.AddComponent<AudioSource>();
        fuente2D.playOnAwake = false;
        fuente2D.spatialBlend = 0f;

        musica = CrearBucle(0f);
        tension = CrearBucle(0f);
        amb = CrearBucle(0f);
        latido = CrearBucle(0f);

        for (int i = 0; i < 24; i++)
        {
            var go = new GameObject("Sfx3D_" + i);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 1f;
            s.rolloffMode = AudioRolloffMode.Logarithmic;
            s.minDistance = 2f;
            s.maxDistance = 40f;
            s.dopplerLevel = 0f;
            pool.Add(s);
        }
    }

    private AudioSource CrearBucle(float vol)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = true;
        s.spatialBlend = 0f;
        s.volume = vol;
        return s;
    }

    private void Start()
    {
        AudioClip a = Clip(ambiente);
        if (a != null) { amb.clip = a; amb.volume = 0.35f; amb.Play(); }
        AudioClip t = Clip(musicaTension);
        if (t != null) { tension.clip = t; tension.volume = 0f; tension.Play(); }
        AudioClip l = Clip("sfx_latido");
        if (l != null) { latido.clip = l; latido.volume = 0f; latido.Play(); }
        Musica("mus_menu", 0.5f);
    }

    private void OnDestroy()
    {
        if (I == this) { I = null; }
    }

    public AudioClip Clip(string nombre)
    {
        AudioClip c;
        return !string.IsNullOrEmpty(nombre) && mapa.TryGetValue(nombre, out c) ? c : null;
    }

    public static void Play2D(string nombre, float volumen = 1f)
    {
        if (I == null) { return; }
        AudioClip c = I.Clip(nombre);
        if (c != null) { I.fuente2D.PlayOneShot(c, volumen * I.volumenEfectos); }
    }

    public static void Play3D(string nombre, Vector3 pos, float volumen = 1f, float tono = 1f)
    {
        if (I == null) { return; }
        AudioClip c = I.Clip(nombre);
        if (c == null) { return; }
        AudioSource libre = null;
        foreach (AudioSource s in I.pool)
        {
            if (!s.isPlaying) { libre = s; break; }
        }
        if (libre == null) { libre = I.pool[Random.Range(0, I.pool.Count)]; }
        libre.transform.position = pos;
        libre.clip = c;
        libre.pitch = tono;
        libre.volume = volumen * I.volumenEfectos;
        libre.Play();
    }

    /// <summary>Cambia la musica de fondo con fundido (null = silencio).</summary>
    public static void Musica(string nombre, float fundido)
    {
        if (I == null) { return; }
        AudioClip c = I.Clip(nombre);
        I.velFundido = fundido <= 0.01f ? 100f : 1f / fundido;
        if (c == null)
        {
            I.volMusicaObjetivo = 0f;
            return;
        }
        if (I.musica.clip != c)
        {
            I.musica.clip = c;
            I.musica.volume = 0f;
            I.musica.Play();
        }
        I.volMusicaObjetivo = 1f;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        musica.volume = Mathf.MoveTowards(musica.volume, volMusicaObjetivo * volumenMusica, dt * velFundido * volumenMusica);

        bool jugando = Juego.I != null && (Juego.I.estado == Juego.Estado.Jugando || Juego.I.estado == Juego.Estado.Cinematica);
        bool peligro = jugando && Infectado.AlgunoPersiguiendo;
        tension.volume = Mathf.MoveTowards(tension.volume, peligro ? volumenMusica * 1.1f : 0f, dt * (peligro ? 0.8f : 0.2f));

        Jugador j = Jugador.I;
        float objetivoLatido = 0f;
        if (j != null && !j.Muerto && jugando)
        {
            float bajo = 1f - Mathf.Clamp01(j.Salud / 45f);
            objetivoLatido = Mathf.Max(bajo, peligro ? 0.35f : 0f);
            latido.pitch = 1f + bajo * 0.4f;
        }
        latido.volume = Mathf.MoveTowards(latido.volume, objetivoLatido * 0.8f, dt * 0.8f);

        bool pausa = Juego.I != null && (Juego.I.estado == Juego.Estado.Pausa || Juego.I.estado == Juego.Estado.Lectura || Juego.I.estado == Juego.Estado.Eleccion);
        amb.volume = Mathf.MoveTowards(amb.volume, pausa ? 0.12f : 0.35f, dt);
    }
}
