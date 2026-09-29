using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sonidos procedurales del Capitulo 2 "Corte de Paso": motor del generador de emergencia,
/// helicoptero militar, escopeta antidisturbios, golpe en chapa, bocina del altavoz, ambiente de
/// la Ceja y la musica de exploracion (charango y quena). Menu: Hoyada Z / Audio / Generar sonidos Cap 2.
/// </summary>
public static partial class GeneradorAudio
{
    [MenuItem("Hoyada Z/Audio/Generar sonidos Cap 2")]
    public static void GenerarCap2Menu()
    {
        Debug.Log(GenerarCap2());
    }

    public static string GenerarCap2()
    {
        Directory.CreateDirectory(Carpeta);
        var hechos = new List<string>();
        rnd = new System.Random(2027);

        Guardar("amb_generador", Bucle(Generador(22050, 9f), 22050, 1f), 22050, hechos);
        Guardar("amb_helicoptero", Bucle(Helicoptero(22050, 8f), 22050, 1f), 22050, hechos);
        Guardar("amb_ceja", Bucle(AmbienteCeja(22050, 18f), 22050, 2f), 22050, hechos);
        Guardar("sfx_escopeta", Escopeta(), 44100, hechos);
        Guardar("sfx_chapa", Chapa(), 44100, hechos);
        Guardar("sfx_megafono", Megafono(), 44100, hechos);
        Guardar("mus_ceja", Bucle(MusicaCeja(22050), 22050, 1.5f), 22050, hechos);

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
        return "Audio del capitulo 2 generado: " + hechos.Count + " clips";
    }

    /// <summary>Diesel en ralenti: golpes de combustion a 12.5 Hz, traqueteo metalico y zumbido.</summary>
    private static float[] Generador(int sr, float dur)
    {
        var s = Nuevo(dur, sr); var lp = new PasaBajos(420f, sr); var bp = new Biquad(sr).PasaBanda(900f, 1.4f);
        const float f = 12.5f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float tt = Mathf.Repeat(t * f, 1f) / f;
            float golpe = Env(tt, 0.002f, 0.028f);
            float grave = Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.22f + Saw(25f * t) * 0.3f;
            float traqueteo = bp.P(R()) * golpe * 1.3f;
            float zumbido = Mathf.Sin(2f * Mathf.PI * 221f * t) * 0.035f;
            s[i] = lp.P(grave * (0.45f + 0.55f * golpe)) + traqueteo + zumbido;
        }
        return s;
    }

    /// <summary>Rotor a 5.5 golpes por segundo, turbina y viento.</summary>
    private static float[] Helicoptero(int sr, float dur)
    {
        var s = Nuevo(dur, sr); var lp = new PasaBajos(700f, sr); var lp2 = new PasaBajos(180f, sr); var hp = new Biquad(sr).PasaAltos(2500f, 0.7f);
        const float f = 5.5f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float tt = Mathf.Repeat(t * f, 1f) / f;
            float palada = lp.P(R()) * Env(tt, 0.004f, 0.05f) * 1.4f + Mathf.Sin(2f * Mathf.PI * 55f * tt) * Env(tt, 0.003f, 0.07f) * 0.9f;
            float turbina = Mathf.Sin(2f * Mathf.PI * (1150f + 8f * Mathf.Sin(t * 2f)) * t) * 0.045f + hp.P(R()) * 0.05f;
            float viento = lp2.P(R()) * 0.5f;
            s[i] = palada + turbina + viento;
        }
        return s;
    }

    /// <summary>Viento de altura con gemidos lejanos de la turba de la feria.</summary>
    private static float[] AmbienteCeja(int sr, float dur)
    {
        float[] s = Viento(sr, dur);
        for (int i = 0; i < s.Length; i++) { s[i] *= 0.55f; }
        int gemidos = 9;
        for (int g = 0; g < gemidos; g++)
        {
            float inicio = U() * (dur - 3f);
            float largo = 1.4f + U() * 1.8f;
            float f0 = 320f + U() * 220f;
            var bp = new Biquad(sr).PasaBanda(f0, 3f);
            float vol = 0.25f + U() * 0.35f;
            int a = (int)(inicio * sr), n = (int)(largo * sr);
            for (int k = 0; k < n && a + k < s.Length; k++)
            {
                float t = k / (float)sr;
                if (k % 128 == 0) { bp.PasaBanda(f0 * (1f - 0.25f * t / largo) + 15f * Mathf.Sin(t * 9f), 3f); }
                float env = Mathf.Sin(Mathf.PI * t / largo);
                s[a + k] += bp.P(R()) * env * vol;
            }
        }
        return Reverb(s, sr, 0.4f, 3f);
    }

    /// <summary>Escopeta antidisturbios: estampido grave, cola larga y la corredera al recargar.</summary>
    private static float[] Escopeta()
    {
        int sr = 44100; float dur = 2f; var s = Nuevo(dur, sr); var lp = new PasaBajos(2200f, sr); var lp2 = new PasaBajos(150f, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float crack = R() * Env(t, 0.0003f, 0.009f) * 1.8f;
            float cuerpo = lp.P(R()) * Env(t, 0.001f, 0.09f) * 1.5f;
            float boom = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(75f, 38f, Mathf.Clamp01(t * 5f)) * t) * Env(t, 0.001f, 0.25f) * 1.4f;
            float cola = lp2.P(R()) * Env(t, 0.06f, 0.7f) * 1.4f;
            float corredera = 0f;
            foreach (float c0 in new[] { 0.62f, 0.78f })
            {
                float tc = t - c0;
                if (tc > 0f) { corredera += R() * Env(tc, 0.0005f, 0.012f) * 0.5f + Mathf.Sin(2f * Mathf.PI * 2400f * tc) * Env(tc, 0.0005f, 0.02f) * 0.2f; }
            }
            s[i] = crack + cuerpo + boom + cola + corredera;
        }
        return Reverb(s, sr, 0.35f, 3.5f);
    }

    /// <summary>Golpe en una calamina: parciales inarmonicos que vibran y un traqueteo.</summary>
    private static float[] Chapa()
    {
        int sr = 44100; float dur = 1.5f; var s = Nuevo(dur, sr); var lp = new PasaBajos(3500f, sr);
        float[] parciales = { 176f, 259f, 407f, 583f, 808f, 1127f };
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float golpe = lp.P(R()) * Env(t, 0.0005f, 0.02f) * 1.3f;
            float resonancia = 0f;
            for (int k = 0; k < parciales.Length; k++)
            {
                float f = parciales[k] * (1f + 0.015f * Mathf.Sin(2f * Mathf.PI * 6f * t + k));
                resonancia += Mathf.Sin(2f * Mathf.PI * f * t) * Env(t, 0.001f, 0.45f / (1f + k * 0.45f)) / (1f + k * 0.5f);
            }
            float traqueteo = R() * Env(t, 0.01f, 0.12f) * 0.25f * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 38f * t));
            s[i] = golpe + resonancia * 0.8f + traqueteo;
        }
        return Reverb(s, sr, 0.3f, 2f);
    }

    /// <summary>Acople del altavoz del helicoptero antes del anuncio.</summary>
    private static float[] Megafono()
    {
        int sr = 44100; float dur = 1.3f; var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(1700f, 2f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float click = R() * Env(t, 0.0005f, 0.01f) * 0.8f;
            float f = 1650f + 180f * Mathf.Clamp01(t / 0.9f);
            float acople = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Clamp01(t / 0.25f) * Mathf.Clamp01((dur - t) / 0.2f) * 0.6f;
            float ruido = bp.P(R()) * 0.25f * Mathf.Clamp01((dur - t) / 0.2f);
            s[i] = click + acople + ruido;
        }
        return s;
    }

    /// <summary>Exploracion en la Ceja: charango arpegiado en La menor, bordon grave y una quena lejana.</summary>
    private static float[] MusicaCeja(int sr)
    {
        float corchea = 60f / 84f / 2f;
        float[][] acordes =
        {
            new[] { 440f, 523.25f, 659.25f, 880f },
            new[] { 349.23f, 440f, 523.25f, 698.46f },
            new[] { 392f, 493.88f, 587.33f, 783.99f },
            new[] { 329.63f, 415.3f, 493.88f, 659.25f }
        };
        float[] raiz = { 110f, 87.31f, 98f, 82.41f };
        float[] quena = { 659.25f, 587.33f, 523.25f, 493.88f };
        float compas = corchea * 8f;
        float dur = compas * 8f;
        var s = Nuevo(dur, sr); var lp = new PasaBajos(2600f, sr);
        int[] patron = { 0, 1, 2, 3, 2, 1, 2, 1 };
        float faseQ = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            int bar = Mathf.Min(7, Mathf.FloorToInt(t / compas));
            int ac = bar % 4;
            float tb = Mathf.Max(0f, t - bar * compas);
            int nota = Mathf.Clamp(Mathf.FloorToInt(tb / corchea), 0, 7);
            float tn = tb - nota * corchea;
            float f = acordes[ac][patron[nota % 8]];
            float pluck = 0f;
            for (int k = 1; k <= 5; k++) { pluck += Mathf.Sin(2f * Mathf.PI * f * k * tn) * Mathf.Exp(-tn * (4f + k * 3f)) / k; }
            pluck *= Mathf.Clamp01(tn / 0.002f) * 0.35f;
            float bordon = (Mathf.Sin(2f * Mathf.PI * raiz[ac] * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * raiz[ac] * 1.5f * t)) * 0.14f * (0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * tb / compas));
            float q = 0f;
            if (bar >= 4)
            {
                float fq = quena[bar - 4];
                faseQ += (fq + Mathf.Sin(t * 33f) * 5f * Mathf.Clamp01(tb / 0.8f)) / sr;
                float envQ = Mathf.Clamp01(tb / 0.4f) * Mathf.Clamp01((compas - tb) / 0.5f);
                q = (Mathf.Sin(2f * Mathf.PI * faseQ) + 0.25f * Mathf.Sin(4f * Mathf.PI * faseQ) + R() * 0.08f) * 0.12f * envQ;
            }
            s[i] = lp.P(pluck + bordon) + q;
        }
        return Reverb(s, sr, 0.35f, 2.5f);
    }
}
