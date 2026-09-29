using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sonidos nuevos: esquiva, forcejeo, fabricar, honda, piedra, revolver (disparo, vacio,
/// recarga), molotov, logro, guardar en el altar, rugido y pisoton del Carnicero, grito del
/// griton, silbato de la cebra y dos musicas (jefe y supervivencia con bombo legüero).
/// Menu: Hoyada Z / Audio / Generar sonidos extra.
/// </summary>
public static partial class GeneradorAudio
{
    [MenuItem("Hoyada Z/Audio/Generar sonidos extra")]
    public static void GenerarExtraMenu()
    {
        Debug.Log(GenerarExtra());
    }

    public static string GenerarExtra()
    {
        Directory.CreateDirectory(Carpeta);
        var hechos = new List<string>();
        rnd = new System.Random(11);

        Guardar("sfx_esquiva", Esquiva(), 44100, hechos);
        Guardar("sfx_agarre", Agarre(), 44100, hechos);
        Guardar("sfx_craftear", Craftear(), 44100, hechos);
        Guardar("sfx_honda", Honda(), 44100, hechos);
        Guardar("sfx_piedra", Piedra(), 44100, hechos);
        Guardar("sfx_disparo", Disparo(), 44100, hechos);
        Guardar("sfx_vacio", Vacio(), 44100, hechos);
        Guardar("sfx_recarga", Recarga(), 44100, hechos);
        Guardar("sfx_molotov", Molotov(), 44100, hechos);
        Guardar("sfx_logro", LogroSonido(), 44100, hechos);
        Guardar("sfx_guardar", GuardarSonido(), 44100, hechos);
        Guardar("sfx_rugido", Rugido(), 44100, hechos);
        Guardar("sfx_golpe_suelo", GolpeSuelo(), 44100, hechos);
        Guardar("sfx_grito_griton", GritoGriton(), 44100, hechos);
        Guardar("sfx_silbato", Silbato(), 44100, hechos);
        Guardar("mus_jefe", Bucle(MusicaJefe(22050, 26f), 22050, 1.5f), 22050, hechos);
        Guardar("mus_supervivencia", Bucle(MusicaSupervivencia(22050, 26f), 22050, 1.5f), 22050, hechos);

        AssetDatabase.Refresh();
        foreach (string n in hechos)
        {
            string ruta = Carpeta + "/" + n + ".wav";
            var imp = AssetImporter.GetAtPath(ruta) as AudioImporter;
            if (imp == null) { continue; }
            var s = imp.defaultSampleSettings;
            bool largo = n.StartsWith("amb_") || n.StartsWith("mus_");
            s.compressionFormat = largo ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
            s.quality = 0.6f;
            s.loadType = largo ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            imp.defaultSampleSettings = s;
            imp.forceToMono = true;
            imp.SaveAndReimport();
        }
        return "Audio extra generado: " + hechos.Count + " clips";
    }

    private static float[] Esquiva()
    {
        int sr = 44100; float dur = 0.35f; var s = Nuevo(dur, sr); var bp = new Biquad(sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float k = t / dur;
            if (i % 64 == 0) { bp.PasaBanda(Mathf.Lerp(300f, 1100f, Mathf.Sin(k * Mathf.PI)), 0.9f); }
            float roce = (U() > 0.97f ? R() * 0.4f : 0f) * Env(t, 0.05f, 0.1f);
            s[i] = bp.P(R()) * Mathf.Sin(k * Mathf.PI) + roce;
        }
        return s;
    }

    private static float[] Agarre()
    {
        int sr = 44100; float dur = 0.9f; var s = Nuevo(dur, sr);
        float[] g = Grunido(0.8f, 110f);
        var lp = new PasaBajos(700f, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float golpes = 0f;
            for (int k = 0; k < 4; k++) { float tk = t - k * 0.18f; if (tk > 0f) { golpes += lp.P(R()) * Env(tk, 0.002f, 0.04f); } }
            s[i] = (i < g.Length ? g[i] * 0.8f : 0f) + golpes * 1.2f;
        }
        return s;
    }

    private static float[] Craftear()
    {
        int sr = 44100; float dur = 1.3f; var s = Nuevo(dur, sr); var hp = new Biquad(sr).PasaAltos(1500f, 0.7f);
        float siguiente = 0f; float inicio = -1f; float largo = 0.1f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            if (t >= siguiente) { inicio = t; largo = 0.05f + U() * 0.18f; siguiente = t + largo + U() * 0.12f; }
            float e = t - inicio < largo ? Mathf.Sin((t - inicio) / largo * Mathf.PI) : 0f;
            float cinta = t > 0.7f && t < 1.05f ? Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.1f * R() : 0f;
            s[i] = hp.P(R()) * e * 0.8f + cinta;
        }
        return s;
    }

    private static float[] Honda()
    {
        int sr = 44100; float dur = 0.55f; var s = Nuevo(dur, sr); var bp = new Biquad(sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float giro = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(5f, 11f, t / 0.4f) * t);
            if (i % 64 == 0) { bp.PasaBanda(Mathf.Lerp(400f, 900f, giro), 2f); }
            float remolino = t < 0.4f ? bp.P(R()) * giro * (t / 0.4f) : 0f;
            float tc = t - 0.4f;
            float chasquido = tc > 0f ? R() * Env(tc, 0.0005f, 0.012f) * 1.5f : 0f;
            s[i] = remolino + chasquido;
        }
        return s;
    }

    private static float[] Piedra()
    {
        int sr = 44100; var s = Nuevo(0.25f, sr); var lp = new PasaBajos(2500f, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            s[i] = Mathf.Sin(2f * Mathf.PI * 180f * t) * Env(t, 0.001f, 0.03f) + lp.P(R()) * Env(t, 0.0005f, 0.02f) * 1.2f + (U() > 0.99f ? R() * Env(t, 0.02f, 0.05f) * 0.3f : 0f);
        }
        return s;
    }

    private static float[] Disparo()
    {
        int sr = 44100; float dur = 1.6f; var s = Nuevo(dur, sr); var lp = new PasaBajos(3000f, sr); var lp2 = new PasaBajos(200f, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float crack = R() * Env(t, 0.0003f, 0.006f) * 1.6f;
            float cuerpo = lp.P(R()) * Env(t, 0.001f, 0.06f) * 1.3f;
            float boom = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 45f, Mathf.Clamp01(t * 6f)) * t) * Env(t, 0.001f, 0.18f) * 1.1f;
            float cola = lp2.P(R()) * Env(t, 0.05f, 0.5f) * 1.2f;
            s[i] = crack + cuerpo + boom + cola;
        }
        return Reverb(s, sr, 0.35f, 3f);
    }

    private static float[] Vacio()
    {
        int sr = 44100; var s = Nuevo(0.12f, sr); var hp = new Biquad(sr).PasaAltos(3000f, 0.7f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            s[i] = hp.P(R()) * Env(t, 0.0003f, 0.006f) + Mathf.Sin(2f * Mathf.PI * 3200f * t) * Env(t, 0.0005f, 0.01f) * 0.4f;
        }
        return s;
    }

    private static float[] Recarga()
    {
        int sr = 44100; float dur = 1.8f; var s = Nuevo(dur, sr); var hp = new Biquad(sr).PasaAltos(2000f, 0.7f);
        float[] marcas = { 0.05f, 0.35f, 0.55f, 0.72f, 0.88f, 1.03f, 1.18f, 1.33f, 1.6f };
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float v = 0f;
            for (int k = 0; k < marcas.Length; k++)
            {
                float tk = t - marcas[k];
                if (tk < 0f || tk > 0.08f) { continue; }
                float f = k == 0 || k == marcas.Length - 1 ? 1800f : 4200f;
                v += (Mathf.Sin(2f * Mathf.PI * f * tk) * 0.5f + R() * 0.5f) * Env(tk, 0.0003f, k == 0 || k == marcas.Length - 1 ? 0.02f : 0.008f);
            }
            s[i] = hp.P(v);
        }
        return s;
    }

    private static float[] Molotov()
    {
        int sr = 44100; float dur = 2.2f; var s = Nuevo(dur, sr); var lp = new PasaBajos(900f, sr);
        float[] vidrio = Botella();
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float fuego = lp.P(R()) * Mathf.Clamp01(t * 6f) * Env(Mathf.Max(0f, t - 0.3f), 0.001f, 1.2f) * (0.8f + 0.2f * Mathf.PerlinNoise(t * 20f, 1f));
            float whoosh = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(60f, 120f, t) * t) * Env(t, 0.05f, 0.3f) * 0.5f;
            s[i] = (i < vidrio.Length ? vidrio[i] * 0.7f : 0f) + fuego * 1.4f + whoosh;
        }
        return s;
    }

    private static float Tono(float f, float t, float caida)
    {
        return (Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * f * 2f * t) + 0.12f * Mathf.Sin(2f * Mathf.PI * f * 3f * t)) * Env(t, 0.004f, caida);
    }

    private static float[] LogroSonido()
    {
        int sr = 44100; float dur = 1.6f; var s = Nuevo(dur, sr);
        float[] notas = { 440f, 523.25f, 659.25f, 783.99f, 880f };
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float v = 0f;
            for (int k = 0; k < notas.Length; k++)
            {
                float tk = t - k * 0.09f;
                if (tk > 0f) { v += Tono(notas[k], tk, k == notas.Length - 1 ? 0.7f : 0.25f) * 0.35f; }
            }
            s[i] = v;
        }
        return Reverb(s, sr, 0.3f, 1.5f);
    }

    private static float[] GuardarSonido()
    {
        int sr = 44100; float dur = 2.4f; var s = Nuevo(dur, sr);
        float[] parciales = { 1f, 2.76f, 5.4f, 8.93f };
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float v = 0f;
            for (int k = 0; k < parciales.Length; k++) { v += Mathf.Sin(2f * Mathf.PI * 392f * parciales[k] * t) * Env(t, 0.002f, 1.4f / (1f + k)) / (1f + k); }
            v += Mathf.Sin(2f * Mathf.PI * 98f * t) * Env(t, 0.3f, 0.8f) * 0.2f;
            s[i] = v;
        }
        return Reverb(s, sr, 0.35f, 2f);
    }

    private static float[] Rugido()
    {
        int sr = 44100; float dur = 2.1f; var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(420f, 1.2f); var lp = new PasaBajos(1200f, sr);
        float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float f = 58f + 20f * Mathf.Sin(t * 1.6f) + 6f * Mathf.Sin(t * 37f) + R() * 4f;
            fase += f / sr;
            float voz = Saw(fase) * 0.8f + bp.P(R()) * 1.4f;
            float env = Mathf.Clamp01(t * 5f) * Mathf.Clamp01((dur - t) * 2.5f);
            s[i] = lp.P(voz) * env * (0.8f + 0.2f * Mathf.PerlinNoise(t * 9f, 3f));
        }
        return Reverb(s, sr, 0.3f, 2.5f);
    }

    private static float[] GolpeSuelo()
    {
        int sr = 44100; float dur = 1.4f; var s = Nuevo(dur, sr); var lp = new PasaBajos(400f, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float boom = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(70f, 32f, Mathf.Clamp01(t * 3f)) * t) * Env(t, 0.002f, 0.35f) * 1.4f;
            float escombros = lp.P(R()) * Env(t, 0.005f, 0.4f) * 1.3f + (U() > 0.992f ? R() * Env(t, 0.05f, 0.5f) : 0f);
            s[i] = boom + escombros;
        }
        return Reverb(s, sr, 0.3f, 2.5f);
    }

    private static float[] GritoGriton()
    {
        int sr = 44100; float dur = 1.7f; var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(2400f, 3f); var bp2 = new Biquad(sr).PasaBanda(1200f, 2f);
        float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float f = Mathf.Lerp(900f, 1500f, Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI)) + 60f * Mathf.Sin(t * 42f) + R() * 30f;
            fase += f / sr;
            float voz = Saw(fase) * 0.6f + Mathf.Sin(2f * Mathf.PI * fase * 2.01f) * 0.3f;
            float aspero = bp.P(R()) * 0.9f + bp2.P(voz);
            float env = Mathf.Clamp01(t * 12f) * Mathf.Clamp01((dur - t) * 3f);
            s[i] = (voz * 0.6f + aspero) * env;
        }
        return Reverb(s, sr, 0.4f, 3f);
    }

    private static float[] Silbato()
    {
        int sr = 44100; float dur = 0.7f; var s = Nuevo(dur, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float trino = 1f + 0.02f * Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 28f * t));
            float env = (t < 0.25f || t > 0.33f) ? Mathf.Clamp01(t * 30f) * Mathf.Clamp01((dur - t) * 12f) : 0.1f;
            s[i] = (Mathf.Sin(2f * Mathf.PI * 2900f * trino * t) + R() * 0.12f) * env;
        }
        return s;
    }

    /// <summary>Bombo legüero, bajo insistente y quenas disonantes (jefe y asedio).</summary>
    private static float[] MusicaJefe(int sr, float dur)
    {
        var s = Nuevo(dur, sr); var lp = new PasaBajos(900f, sr);
        float bpm = 132f; float beat = 60f / bpm;
        float[] bajo = { 55f, 55f, 58.27f, 55f, 65.41f, 61.74f, 58.27f, 55f };
        float faseQuena = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float b = Mathf.Repeat(t, beat);
            int n = Mathf.FloorToInt(t / beat);
            bool fuerte = n % 4 == 0 || n % 4 == 3;
            float bombo = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 45f, Mathf.Clamp01(b * 12f)) * b) * Env(b, 0.002f, fuerte ? 0.16f : 0.08f) * (fuerte ? 1f : 0.55f);
            float parche = R() * Env(b, 0.001f, 0.02f) * 0.35f;
            float semi = Mathf.Repeat(t, beat * 0.5f);
            float aro = (n % 2 == 1) ? R() * Env(semi, 0.001f, 0.015f) * 0.25f : 0f;
            float fb = bajo[(n / 2) % bajo.Length];
            float bj = Saw(fb * t) * 0.35f * (0.6f + 0.4f * Env(b, 0.005f, 0.2f));
            int compas = Mathf.FloorToInt(t / (beat * 8f));
            float fq = compas % 2 == 0 ? 440f : 466.16f;
            faseQuena += (fq + Mathf.Sin(t * 30f) * 4f) / sr;
            float quena = (Mathf.Sin(2f * Mathf.PI * faseQuena) + R() * 0.15f) * 0.12f * Mathf.Clamp01(Mathf.Sin(2f * Mathf.PI * t / (beat * 8f)));
            s[i] = lp.P(bj + bombo * 1.2f) + parche + aro + quena;
        }
        return Reverb(s, sr, 0.25f, 2f);
    }

    /// <summary>Pulso grave y tic-tac para el modo Supervivencia.</summary>
    private static float[] MusicaSupervivencia(int sr, float dur)
    {
        var s = Nuevo(dur, sr); var lp = new PasaBajos(600f, sr);
        float beat = 60f / 100f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float b = Mathf.Repeat(t, beat);
            float latido = Mathf.Sin(2f * Mathf.PI * 50f * b) * Env(b, 0.003f, 0.1f) + Mathf.Sin(2f * Mathf.PI * 45f * Mathf.Max(0f, b - 0.18f)) * Env(Mathf.Max(0f, b - 0.18f), 0.003f, 0.08f) * 0.6f;
            float tic = Mathf.Repeat(t, beat / 2f) < 0.01f ? R() * 0.2f : 0f;
            float dron = (Mathf.Sin(2f * Mathf.PI * 73.42f * t) + Mathf.Sin(2f * Mathf.PI * 77.78f * t) * 0.6f) * 0.25f * (0.6f + 0.4f * Mathf.Sin(t * 0.8f));
            s[i] = lp.P(latido + dron) + tic;
        }
        return Reverb(s, sr, 0.3f, 2f);
    }
}
