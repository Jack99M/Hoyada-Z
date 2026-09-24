using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sintetiza todos los sonidos del capitulo (sin assets externos) y los guarda como WAV en
/// Assets/Arte/Audio: pasos, golpes, gritos, chasquidos del fungico, vidrio, puertas,
/// latido, ambiente de viento, sirena lejana, discoteca apagada, fuego, radio y musica
/// (drones de tension y un tema andino con cuerdas pulsadas tipo charango).
/// Menu: Hoyada Z / Audio / Generar sonidos. Son provisionales hasta tener el audio del equipo.
/// </summary>
public static partial class GeneradorAudio
{
    private const string Carpeta = "Assets/Arte/Audio";
    private static System.Random rnd = new System.Random(7);

    [MenuItem("Hoyada Z/Audio/Generar sonidos")]
    public static void GenerarMenu()
    {
        Debug.Log(Generar());
    }

    public static string Generar()
    {
        Directory.CreateDirectory(Carpeta);
        var hechos = new List<string>();
        rnd = new System.Random(7);

        Guardar("sfx_paso_1", Paso(0.9f), 44100, hechos);
        Guardar("sfx_paso_2", Paso(1.1f), 44100, hechos);
        Guardar("sfx_swing", Swing(), 44100, hechos);
        Guardar("sfx_golpe_carne", GolpeCarne(), 44100, hechos);
        Guardar("sfx_golpe_arma", GolpeArma(), 44100, hechos);
        Guardar("sfx_golpe_recibido", GolpeRecibido(), 44100, hechos);
        Guardar("sfx_rompe_palo", RompePalo(), 44100, hechos);
        Guardar("sfx_grunido", Grunido(1.3f, 85f), 44100, hechos);
        Guardar("sfx_grito", Grito(), 44100, hechos);
        Guardar("sfx_chasquido", Chasquidos(), 44100, hechos);
        Guardar("sfx_muerte_infectado", MuerteInfectado(), 44100, hechos);
        Guardar("sfx_estrangular", Estrangular(), 44100, hechos);
        Guardar("sfx_botella", Botella(), 44100, hechos);
        Guardar("sfx_puerta", Puerta(), 44100, hechos);
        Guardar("sfx_puerta_trabada", PuertaTrabada(), 44100, hechos);
        Guardar("sfx_cortina", Cortina(), 44100, hechos);
        Guardar("sfx_cadena", Cadena(), 44100, hechos);
        Guardar("sfx_latido", Latido(), 44100, hechos);
        Guardar("sfx_recoger", Recoger(), 44100, hechos);
        Guardar("sfx_arma", Arma(), 44100, hechos);
        Guardar("sfx_click", Click(), 44100, hechos);
        Guardar("sfx_objetivo", Objetivo(), 44100, hechos);
        Guardar("sfx_papel", Papel(), 44100, hechos);
        Guardar("sfx_celular", Celular(), 44100, hechos);
        Guardar("sfx_vendar", Vendar(), 44100, hechos);
        Guardar("sfx_jadeo", Jadeo(), 44100, hechos);
        Guardar("sfx_muerte", MuerteJugador(), 44100, hechos);
        Guardar("sfx_susto", Susto(), 44100, hechos);
        Guardar("amb_viento", Bucle(Viento(22050, 18f), 22050, 1.5f), 22050, hechos);
        Guardar("amb_sirena", Bucle(Sirena(22050, 14f), 22050, 1.5f), 22050, hechos);
        Guardar("amb_disco", Bucle(Disco(22050, 9.6f), 22050, 0.05f), 22050, hechos);
        Guardar("amb_fuego", Bucle(Fuego(22050, 6f), 22050, 0.8f), 22050, hechos);
        Guardar("amb_radio", Bucle(Radio(22050, 6f), 22050, 0.5f), 22050, hechos);
        Guardar("mus_tension", Bucle(Tension(22050, 24f), 22050, 2f), 22050, hechos);
        Guardar("mus_menu", Bucle(TemaAndino(22050, 32f), 22050, 2f), 22050, hechos);

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
        return "Audio generado: " + hechos.Count + " clips en " + Carpeta;
    }

    // ---------- Utilidades ----------

    private static float R() { return (float)rnd.NextDouble() * 2f - 1f; }
    private static float U() { return (float)rnd.NextDouble(); }

    private static float[] Nuevo(float seg, int sr = 44100) { return new float[Mathf.Max(1, (int)(seg * sr))]; }

    private static float Env(float t, float ataque, float caida)
    {
        if (t < ataque) { return t / Mathf.Max(ataque, 1e-5f); }
        return Mathf.Exp(-(t - ataque) / Mathf.Max(caida, 1e-5f));
    }

    private class PasaBajos
    {
        private float y; private readonly float a;
        public PasaBajos(float fc, int sr) { a = 1f - Mathf.Exp(-2f * Mathf.PI * fc / sr); }
        public float P(float x) { y += a * (x - y); return y; }
    }

    private class Biquad
    {
        private float b0, b1, b2, a1, a2, x1, x2, y1, y2;
        private readonly int sr;
        public Biquad(int sr) { this.sr = sr; }
        public Biquad PasaBanda(float f, float q)
        {
            float w = 2f * Mathf.PI * f / sr, al = Mathf.Sin(w) / (2f * q), c = Mathf.Cos(w), a0 = 1f + al;
            b0 = al / a0; b1 = 0f; b2 = -al / a0; a1 = -2f * c / a0; a2 = (1f - al) / a0;
            return this;
        }
        public Biquad PasaAltos(float f, float q)
        {
            float w = 2f * Mathf.PI * f / sr, al = Mathf.Sin(w) / (2f * q), c = Mathf.Cos(w), a0 = 1f + al;
            b0 = (1f + c) / 2f / a0; b1 = -(1f + c) / a0; b2 = (1f + c) / 2f / a0; a1 = -2f * c / a0; a2 = (1f - al) / a0;
            return this;
        }
        public float P(float x)
        {
            float y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = x; y2 = y1; y1 = y;
            return y;
        }
    }

    private static float Saw(float fase) { return 2f * (fase - Mathf.Floor(fase + 0.5f)); }

    private static void Normalizar(float[] s, float pico = 0.9f)
    {
        float m = 1e-6f;
        foreach (float v in s) { m = Mathf.Max(m, Mathf.Abs(v)); }
        for (int i = 0; i < s.Length; i++) { s[i] = s[i] / m * pico; }
    }

    private static void Fundidos(float[] s, int sr, float entrada, float salida)
    {
        int a = (int)(entrada * sr), b = (int)(salida * sr);
        for (int i = 0; i < a && i < s.Length; i++) { s[i] *= i / (float)a; }
        for (int i = 0; i < b && i < s.Length; i++) { s[s.Length - 1 - i] *= i / (float)b; }
    }

    /// <summary>Hace un bucle sin cortes mezclando el final con el principio.</summary>
    private static float[] Bucle(float[] s, int sr, float cruce)
    {
        int n = Mathf.Min((int)(cruce * sr), s.Length / 3);
        if (n < 2) { return s; }
        var r = new float[s.Length - n];
        Array.Copy(s, r, r.Length);
        for (int i = 0; i < n; i++)
        {
            float k = i / (float)n;
            r[i] = s[i] * k + s[r.Length + i] * (1f - k);
        }
        return r;
    }

    private static float[] Reverb(float[] s, int sr, float mezcla, float tam)
    {
        int[] d = { (int)(0.0297f * sr * tam), (int)(0.0371f * sr * tam), (int)(0.0411f * sr * tam), (int)(0.0437f * sr * tam) };
        var outp = new float[s.Length];
        foreach (int delay in d)
        {
            var buf = new float[Mathf.Max(1, delay)];
            int idx = 0;
            for (int i = 0; i < s.Length; i++)
            {
                float y = buf[idx];
                buf[idx] = s[i] + y * 0.8f;
                idx = (idx + 1) % buf.Length;
                outp[i] += y * 0.25f;
            }
        }
        for (int i = 0; i < s.Length; i++) { outp[i] = s[i] * (1f - mezcla) + outp[i] * mezcla; }
        return outp;
    }

    private static void Guardar(string nombre, float[] s, int sr, List<string> hechos)
    {
        Normalizar(s, nombre.StartsWith("amb_") || nombre.StartsWith("mus_") ? 0.7f : 0.92f);
        string ruta = Path.Combine(Carpeta, nombre + ".wav");
        using (var fs = new FileStream(ruta, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            int datos = s.Length * 2;
            bw.Write(new[] { 'R', 'I', 'F', 'F' }); bw.Write(36 + datos);
            bw.Write(new[] { 'W', 'A', 'V', 'E' }); bw.Write(new[] { 'f', 'm', 't', ' ' });
            bw.Write(16); bw.Write((short)1); bw.Write((short)1); bw.Write(sr); bw.Write(sr * 2); bw.Write((short)2); bw.Write((short)16);
            bw.Write(new[] { 'd', 'a', 't', 'a' }); bw.Write(datos);
            foreach (float v in s) { bw.Write((short)Mathf.Clamp(v * 32767f, -32768f, 32767f)); }
        }
        hechos.Add(nombre);
    }

    // ---------- Efectos ----------

    private static float[] Paso(float tono)
    {
        int sr = 44100; var s = Nuevo(0.16f, sr); var lp = new PasaBajos(1400f * tono, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float n = lp.P(R()) * Env(t, 0.002f, 0.025f);
            float grava = (U() > 0.985f ? R() : 0f) * Env(t, 0.01f, 0.06f) * 0.6f;
            float golpe = Mathf.Sin(2f * Mathf.PI * 75f * tono * t) * Env(t, 0.001f, 0.02f) * 0.7f;
            s[i] = n * 1.2f + grava + golpe;
        }
        return s;
    }

    private static float[] Swing()
    {
        int sr = 44100; var s = Nuevo(0.28f, sr); var bp = new Biquad(sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float k = t / 0.28f;
            if (i % 64 == 0) { bp.PasaBanda(Mathf.Lerp(500f, 1800f, Mathf.Sin(k * Mathf.PI)), 1.2f); }
            s[i] = bp.P(R()) * Mathf.Sin(k * Mathf.PI);
        }
        return s;
    }

    private static float[] GolpeCarne()
    {
        int sr = 44100; var s = Nuevo(0.3f, sr); var lp = new PasaBajos(900f, sr); float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            fase += Mathf.Lerp(90f, 40f, t / 0.3f) / sr;
            s[i] = Mathf.Sin(2f * Mathf.PI * fase) * Env(t, 0.001f, 0.07f) + lp.P(R()) * Env(t, 0.001f, 0.04f) * 1.5f;
        }
        return s;
    }

    private static float[] GolpeArma()
    {
        int sr = 44100; var s = GolpeCarne(); var hp = new Biquad(sr).PasaAltos(2500f, 0.8f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            s[i] += hp.P(R()) * Env(t, 0.0005f, 0.012f) * 0.9f + Mathf.Sin(2f * Mathf.PI * 420f * t) * Env(t, 0.001f, 0.03f) * 0.5f;
        }
        return s;
    }

    private static float[] GolpeRecibido()
    {
        int sr = 44100; var s = Nuevo(0.45f, sr); var g = GolpeCarne(); var lp = new PasaBajos(600f, sr); float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float b = i < g.Length ? g[i] : 0f;
            fase += (120f + Mathf.Sin(t * 40f) * 8f) / sr;
            float voz = lp.P(Saw(fase)) * Env(t, 0.03f, 0.12f) * 0.8f;
            s[i] = b + voz;
        }
        return s;
    }

    private static float[] RompePalo()
    {
        int sr = 44100; var s = Nuevo(0.45f, sr); var bp = new Biquad(sr).PasaBanda(1500f, 0.9f);
        float[] chasquidos = { 0f, 0.03f, 0.07f, 0.12f, 0.2f };
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float v = 0f;
            foreach (float c in chasquidos) { if (t >= c) { v += Env(t - c, 0.0005f, 0.01f + c * 0.05f); } }
            s[i] = bp.P(R()) * v;
        }
        return s;
    }

    private static float[] Grunido(float dur, float f0)
    {
        int sr = 44100; var s = Nuevo(dur, sr);
        var f1 = new Biquad(sr).PasaBanda(480f, 3f); var f2 = new Biquad(sr).PasaBanda(1150f, 4f); var lp = new PasaBajos(2500f, sr);
        float fase = 0f, jit = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            if (i % 200 == 0) { jit = R() * 12f; }
            fase += (f0 + jit + Mathf.Sin(t * 5f) * 10f) / sr;
            float fuente = Saw(fase) * (0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 27f * t)) + R() * 0.35f;
            float v = f1.P(fuente) * 1.5f + f2.P(fuente) * 0.8f + lp.P(fuente) * 0.3f;
            float env = Mathf.Clamp01(t / 0.12f) * Mathf.Clamp01((dur - t) / 0.35f);
            s[i] = (float)Math.Tanh(v * 2.5f) * env;
        }
        return s;
    }

    private static float[] Grito()
    {
        int sr = 44100; float dur = 1.1f; var s = Nuevo(dur, sr);
        var f1 = new Biquad(sr).PasaBanda(850f, 2.5f); var f2 = new Biquad(sr).PasaBanda(1700f, 3f); float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float k = t / dur;
            float f = Mathf.Lerp(280f, 620f, Mathf.Sin(Mathf.Min(1f, k * 1.6f) * Mathf.PI * 0.5f)) - k * 120f + R() * 20f;
            fase += f / sr;
            float fuente = Saw(fase) + R() * 0.6f;
            float v = f1.P(fuente) * 1.4f + f2.P(fuente);
            s[i] = (float)Math.Tanh(v * 3f) * Mathf.Clamp01(t / 0.05f) * Mathf.Clamp01((dur - t) / 0.4f);
        }
        return Reverb(s, sr, 0.25f, 1f);
    }

    private static float[] Chasquidos()
    {
        int sr = 44100; float dur = 1.2f; var s = Nuevo(dur, sr);
        var res = new Biquad(sr).PasaBanda(2600f, 8f); var res2 = new Biquad(sr).PasaBanda(1300f, 6f); var gorgoteo = new PasaBajos(300f, sr);
        var tiempos = new List<float>(); float tc = 0.02f;
        while (tc < dur - 0.1f) { tiempos.Add(tc); tc += 0.045f + U() * 0.11f; }
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float imp = 0f;
            foreach (float c in tiempos) { float d = t - c; if (d >= 0f && d < 0.004f) { imp += R() * (1f - d / 0.004f); } }
            float g = gorgoteo.P(R()) * (0.3f + 0.3f * Mathf.Sin(t * 31f)) * 0.5f;
            s[i] = res.P(imp) * 3f + res2.P(imp) * 2f + g;
        }
        return Reverb(s, sr, 0.2f, 0.8f);
    }

    private static float[] MuerteInfectado()
    {
        int sr = 44100; float dur = 1.1f; var s = Nuevo(dur, sr); var gr = Grunido(0.8f, 70f); var golpe = GolpeCarne();
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float a = i < gr.Length ? gr[i] * (1f - t / 0.8f) : 0f;
            int j = i - (int)(0.75f * sr);
            float b = j >= 0 && j < golpe.Length ? golpe[j] * 1.3f : 0f;
            s[i] = a + b;
        }
        return s;
    }

    private static float[] Estrangular()
    {
        int sr = 44100; float dur = 1.2f; var s = Nuevo(dur, sr); var lp = new PasaBajos(400f, sr); var gr = Grunido(1.0f, 60f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float g = i < gr.Length ? gr[i] * 0.5f : 0f;
            s[i] = lp.P(R()) * (0.5f + 0.5f * Mathf.Sin(t * 22f)) * Env(t, 0.1f, 0.6f) * 2f + g * Mathf.Clamp01(1f - t);
        }
        return s;
    }

    private static float[] Botella()
    {
        int sr = 44100; float dur = 0.7f; var s = Nuevo(dur, sr); var hp = new Biquad(sr).PasaAltos(3000f, 0.7f);
        var pings = new List<float[]>();
        for (int k = 0; k < 18; k++) { pings.Add(new[] { 2500f + U() * 5000f, U() * 0.25f, 0.02f + U() * 0.08f }); }
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float v = hp.P(R()) * Env(t, 0.001f, 0.06f) * 1.4f;
            foreach (var p in pings) { if (t >= p[1]) { v += Mathf.Sin(2f * Mathf.PI * p[0] * (t - p[1])) * Env(t - p[1], 0.0005f, p[2]) * 0.3f; } }
            s[i] = v;
        }
        return Reverb(s, sr, 0.2f, 0.7f);
    }

    private static float[] Puerta()
    {
        int sr = 44100; float dur = 1.1f; var s = Nuevo(dur, sr); var bp = new Biquad(sr); float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float k = t / dur;
            if (i % 128 == 0) { bp.PasaBanda(700f + Mathf.Sin(k * 9f) * 250f, 5f); }
            fase += (60f + Mathf.Sin(t * 13f) * 40f + U() * 25f) / sr;
            float creak = bp.P(Saw(fase) + R() * 0.2f) * Mathf.Sin(k * Mathf.PI) * 1.6f;
            float cierre = t < 0.05f ? Mathf.Sin(2f * Mathf.PI * 900f * t) * Env(t, 0.0005f, 0.01f) : 0f;
            s[i] = creak + cierre;
        }
        return s;
    }

    private static float[] PuertaTrabada()
    {
        int sr = 44100; float dur = 0.5f; var s = Nuevo(dur, sr); float[] golpes = { 0f, 0.09f, 0.16f, 0.27f };
        float[] parciales = { 180f, 263f, 410f, 590f };
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float v = 0f;
            foreach (float g in golpes)
            {
                if (t < g) { continue; }
                float d = t - g;
                foreach (float p in parciales) { v += Mathf.Sin(2f * Mathf.PI * p * d) * Env(d, 0.001f, 0.06f) * 0.25f; }
                v += R() * Env(d, 0.0005f, 0.01f) * 0.4f;
            }
            s[i] = v;
        }
        return s;
    }

    private static float[] Cortina()
    {
        int sr = 44100; float dur = 1.6f; var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(900f, 1.5f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float traqueteo = (Mathf.Repeat(t * 22f, 1f) < 0.2f ? 1f : 0.3f) * R();
            s[i] = bp.P(traqueteo) * Mathf.Clamp01(t / 0.1f) * Mathf.Clamp01((dur - t) / 0.3f) * 2f;
        }
        return Reverb(s, sr, 0.25f, 1f);
    }

    private static float[] Cadena()
    {
        int sr = 44100; float dur = 0.8f; var s = Nuevo(dur, sr); var hp = new Biquad(sr).PasaAltos(1800f, 1f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float snap = hp.P(R()) * Env(t, 0.0005f, 0.02f) * 1.5f;
            float jingle = 0f;
            for (int k = 1; k < 7; k++) { float tk = 0.05f + k * 0.07f + (k % 2) * 0.02f; if (t > tk) { jingle += Mathf.Sin(2f * Mathf.PI * (3000f + k * 370f) * (t - tk)) * Env(t - tk, 0.0005f, 0.03f) * 0.2f; } }
            s[i] = snap + jingle;
        }
        return s;
    }

    private static float[] Latido()
    {
        int sr = 44100; float dur = 0.95f; var s = Nuevo(dur, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float a = Mathf.Sin(2f * Mathf.PI * 52f * t) * Env(t, 0.005f, 0.06f);
            float t2 = t - 0.2f;
            float b = t2 > 0f ? Mathf.Sin(2f * Mathf.PI * 46f * t2) * Env(t2, 0.005f, 0.07f) * 0.75f : 0f;
            s[i] = a + b;
        }
        return s;
    }

    private static float[] Recoger()
    {
        int sr = 44100; float dur = 0.25f; var s = Nuevo(dur, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            s[i] = Mathf.Sin(2f * Mathf.PI * 660f * t) * Env(t, 0.002f, 0.05f) * 0.4f + Mathf.Sin(2f * Mathf.PI * 990f * t) * Env(t, 0.03f, 0.06f) * 0.3f + R() * Env(t, 0.001f, 0.01f) * 0.3f;
        }
        return s;
    }

    private static float[] Arma()
    {
        int sr = 44100; float dur = 0.3f; var s = Nuevo(dur, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            s[i] = Mathf.Sin(2f * Mathf.PI * 330f * t) * Env(t, 0.001f, 0.03f) + Mathf.Sin(2f * Mathf.PI * 180f * t) * Env(t, 0.001f, 0.05f) + R() * Env(t, 0.001f, 0.015f) * 0.5f;
        }
        return s;
    }

    private static float[] Click()
    {
        int sr = 44100; var s = Nuevo(0.05f, sr);
        for (int i = 0; i < s.Length; i++) { float t = i / (float)sr; s[i] = (R() * 0.5f + Mathf.Sin(2f * Mathf.PI * 2000f * t)) * Env(t, 0.0003f, 0.006f); }
        return s;
    }

    private static float[] Objetivo()
    {
        int sr = 44100; float dur = 1.4f; var s = Nuevo(dur, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            s[i] = Mathf.Sin(2f * Mathf.PI * 220f * t) * Env(t, 0.01f, 0.5f) * 0.5f + Mathf.Sin(2f * Mathf.PI * 311f * t) * Env(t, 0.15f, 0.5f) * 0.35f
                 + Mathf.Sin(2f * Mathf.PI * 110f * t) * Env(t, 0.02f, 0.7f) * 0.4f;
        }
        return Reverb(s, sr, 0.35f, 1.4f);
    }

    private static float[] Papel()
    {
        int sr = 44100; float dur = 0.5f; var s = Nuevo(dur, sr); var hp = new Biquad(sr).PasaAltos(2000f, 0.7f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float crinkle = U() > 0.97f ? 1f : 0.25f;
            s[i] = hp.P(R()) * crinkle * Mathf.Sin(t / dur * Mathf.PI);
        }
        return s;
    }

    private static float[] Celular()
    {
        int sr = 44100; float dur = 0.7f; var s = Nuevo(dur, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            bool on = (t < 0.25f) || (t > 0.4f && t < 0.65f);
            s[i] = on ? Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 150f * t)) * 0.4f * (0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 30f * t)) : 0f;
        }
        var lp = new PasaBajos(900f, sr);
        for (int i = 0; i < s.Length; i++) { s[i] = lp.P(s[i]); }
        return s;
    }

    private static float[] Vendar()
    {
        int sr = 44100; float dur = 1.6f; var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(3000f, 0.8f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float m = Mathf.Abs(Mathf.Sin(t * 7f)) * (U() > 0.5f ? 1f : 0.6f);
            s[i] = bp.P(R()) * m * Mathf.Clamp01((dur - t) / 0.3f);
        }
        return s;
    }

    private static float[] Jadeo()
    {
        int sr = 44100; float dur = 1.3f; var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(1200f, 1.2f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float e = Mathf.Max(0f, Mathf.Sin(t / 0.65f * Mathf.PI));
            s[i] = bp.P(R()) * e;
        }
        return s;
    }

    private static float[] MuerteJugador()
    {
        int sr = 44100; float dur = 2.5f; var s = Nuevo(dur, sr); var g = GolpeRecibido();
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float a = i < g.Length ? g[i] : 0f;
            float dron = (Mathf.Sin(2f * Mathf.PI * 55f * t) + Mathf.Sin(2f * Mathf.PI * 58.3f * t) * 0.8f + Mathf.Sin(2f * Mathf.PI * 82.4f * t) * 0.5f) * Mathf.Clamp01(t / 0.8f) * Mathf.Clamp01((dur - t) / 0.8f) * 0.4f;
            s[i] = a + dron;
        }
        return Reverb(s, sr, 0.3f, 1.5f);
    }

    private static float[] Susto()
    {
        int sr = 44100; float dur = 1.3f; var s = Nuevo(dur, sr);
        float[] fr = { 1480f, 1520f, 1570f, 2090f, 740f };
        float[] fases = new float[fr.Length];
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr; float v = 0f;
            for (int k = 0; k < fr.Length; k++) { fases[k] += (fr[k] + Mathf.Sin(t * 30f + k) * 12f) / sr; v += Saw(fases[k]) * 0.25f; }
            s[i] = v * Env(t, 0.01f, 0.45f);
        }
        return Reverb(s, sr, 0.3f, 1.2f);
    }

    // ---------- Ambientes ----------

    private static float[] Viento(int sr, float dur)
    {
        var s = Nuevo(dur, sr); float marron = 0f; var bp = new Biquad(sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            marron = marron * 0.995f + R() * 0.05f;
            if (i % 256 == 0) { bp.PasaBanda(300f + 250f * Mathf.PerlinNoise(t * 0.2f, 0.3f), 0.8f); }
            float rafaga = 0.4f + 0.6f * Mathf.PerlinNoise(t * 0.15f, 5f);
            s[i] = (marron * 2f + bp.P(R()) * 0.5f) * rafaga;
        }
        return s;
    }

    private static float[] Sirena(int sr, float dur)
    {
        var s = Nuevo(dur, sr); float fase = 0f; var lp = new PasaBajos(1400f, sr); float marron = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float f = 750f + 280f * Mathf.Sin(2f * Mathf.PI * t / 3.5f);
            fase += f / sr;
            float ciclo = Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);
            marron = marron * 0.995f + R() * 0.02f;
            s[i] = lp.P(Mathf.Sin(2f * Mathf.PI * fase) * 0.25f) * ciclo + marron * 0.3f;
        }
        return Reverb(s, sr, 0.55f, 2.2f);
    }

    private static float[] Disco(int sr, float dur)
    {
        var s = Nuevo(dur, sr); float bpm = 100f; float negra = 60f / bpm; var lp = new PasaBajos(220f, sr);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float b = Mathf.Repeat(t, negra);
            float bombo = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(110f, 45f, Mathf.Clamp01(b / 0.12f)) * b) * Env(b, 0.002f, 0.12f);
            float c = Mathf.Repeat(t + negra * 0.5f, negra);
            float contratiempo = Mathf.Sin(2f * Mathf.PI * 70f * c) * Env(c, 0.002f, 0.06f) * 0.4f;
            float bajo = Mathf.Sin(2f * Mathf.PI * (Mathf.Repeat(t, negra * 8f) < negra * 4f ? 55f : 49f) * t) * 0.35f;
            s[i] = lp.P(bombo + contratiempo + bajo);
        }
        return s;
    }

    private static float[] Fuego(int sr, float dur)
    {
        var s = Nuevo(dur, sr); float marron = 0f; var hp = new Biquad(sr).PasaAltos(1500f, 0.7f);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            marron = marron * 0.99f + R() * 0.05f;
            float pop = U() > 0.9994f ? 1f : 0f;
            s[i] = marron * (0.7f + 0.3f * Mathf.PerlinNoise(t * 2f, 1f)) + hp.P(pop * R() * 8f);
        }
        return s;
    }

    private static float[] Radio(int sr, float dur)
    {
        var s = Nuevo(dur, sr); var bp = new Biquad(sr).PasaBanda(1500f, 0.6f); float fase = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            fase += (180f + Mathf.Sin(t * 3f) * 40f + Mathf.Sin(t * 11f) * 25f) / sr;
            float voz = Saw(fase) * Mathf.Max(0f, Mathf.Sin(t * 7f + Mathf.Sin(t * 2f) * 2f)) * 0.3f;
            s[i] = bp.P(R() * 0.8f + voz) * (0.6f + 0.4f * Mathf.PerlinNoise(t * 4f, 2f));
        }
        return s;
    }

    // ---------- Musica ----------

    private static float[] Tension(int sr, float dur)
    {
        var s = Nuevo(dur, sr); var lp = new PasaBajos(500f, sr); float marron = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float trem = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
            float dron = (Mathf.Sin(2f * Mathf.PI * 55f * t) + Mathf.Sin(2f * Mathf.PI * 58.27f * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * 82.4f * t) * 0.4f) * trem;
            float pulso = Mathf.Repeat(t, 0.857f);
            float golpe = Mathf.Sin(2f * Mathf.PI * 40f * pulso) * Env(pulso, 0.005f, 0.15f) * 0.9f;
            marron = marron * 0.997f + R() * 0.02f;
            float chirrido = Mathf.Sin(2f * Mathf.PI * (1760f + Mathf.Sin(t * 6f) * 20f) * t) * 0.04f * Mathf.PerlinNoise(t * 0.3f, 4f);
            s[i] = lp.P(dron * 0.5f + golpe) + marron * 1.5f + chirrido;
        }
        return Reverb(s, sr, 0.35f, 2f);
    }

    private static float[] TemaAndino(int sr, float dur)
    {
        var s = Nuevo(dur, sr);
        // Dron grave (La) y quena lejana
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)sr;
            float dron = (Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.25f) * (0.8f + 0.2f * Mathf.Sin(t * 0.5f));
            s[i] = dron * 0.35f;
        }

        // Charango: cuerdas pulsadas (Karplus-Strong) en pentatonica de La menor
        float[] escala = { 220f, 261.63f, 293.66f, 329.63f, 392f, 440f, 523.25f };
        int[] melodia = { 5, 4, 3, 1, 0, 1, 3, 2, 3, 4, 5, 6, 5, 3, 1, 0 };
        float paso = dur / (melodia.Length * 2f);
        for (int n = 0; n < melodia.Length * 2; n++)
        {
            int nota = melodia[n % melodia.Length];
            if (n % 4 == 3 && U() < 0.5f) { continue; }
            Pulsar(s, sr, n * paso + U() * 0.02f, escala[nota], 0.35f);
            if (n % 2 == 0) { Pulsar(s, sr, n * paso + 0.015f, escala[nota] * 1.5f, 0.15f); }
        }

        // Quena: notas largas con vibrato y aire
        float[] quena = { 440f, 392f, 329.63f, 293.66f };
        var bp = new Biquad(sr).PasaBanda(1500f, 0.8f);
        for (int q = 0; q < quena.Length; q++)
        {
            float ini = q * dur / quena.Length + 1f;
            float largo = dur / quena.Length * 0.7f;
            float fase = 0f;
            for (int i = (int)(ini * sr); i < Mathf.Min(s.Length, (int)((ini + largo) * sr)); i++)
            {
                float t = i / (float)sr - ini;
                fase += (quena[q] * 2f * (1f + 0.006f * Mathf.Sin(2f * Mathf.PI * 5.5f * t))) / sr;
                float env = Mathf.Clamp01(t / 0.6f) * Mathf.Clamp01((largo - t) / 1f);
                s[i] += (Mathf.Sin(2f * Mathf.PI * fase) * 0.12f + bp.P(R()) * 0.03f) * env;
            }
        }
        return Reverb(s, sr, 0.4f, 2.5f);
    }

    private static void Pulsar(float[] s, int sr, float inicio, float f, float vol)
    {
        int largo = Mathf.Max(2, (int)(sr / f));
        var buf = new float[largo];
        for (int i = 0; i < largo; i++) { buf[i] = R(); }
        int ini = (int)(inicio * sr);
        int idx = 0;
        int total = (int)(2.2f * sr);
        for (int i = 0; i < total && ini + i < s.Length; i++)
        {
            int sig = (idx + 1) % largo;
            float v = buf[idx];
            buf[idx] = (buf[idx] + buf[sig]) * 0.5f * 0.996f;
            idx = sig;
            s[ini + i] += v * vol;
        }
    }
}
