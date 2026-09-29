using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera texturas procedurales (color + normal) para el nivel: ladrillo visto paceño,
/// revoque desteñido (ocre, gris, verde agua), asfalto parchado, adoquin, baldosa,
/// madera, calamina / cortina metalica, concreto y piso de discoteca.
/// Crea tambien los materiales URP del nivel en Assets/Arte/Materiales/Nivel.
/// </summary>
public static class GeneradorTexturas
{
    private const string CarpetaTex = "Assets/Arte/Texturas";
    private const string CarpetaMat = "Assets/Arte/Materiales/Nivel";
    private const int N = 512;
    private static System.Random rnd = new System.Random(11);

    [MenuItem("Hoyada Z/Texturas/Generar texturas y materiales")]
    public static void GenerarMenu() { Debug.Log(Generar()); }

    public static string Generar()
    {
        Directory.CreateDirectory(CarpetaTex);
        Directory.CreateDirectory(CarpetaMat);
        rnd = new System.Random(11);

        Par("tex_ladrillo_visto", Ladrillo);
        Par("tex_revoque_ocre", (x, y) => Revoque(x, y, new Color(0.72f, 0.58f, 0.36f)));
        Par("tex_revoque_gris", (x, y) => Revoque(x, y, new Color(0.56f, 0.56f, 0.55f)));
        Par("tex_revoque_verde", (x, y) => Revoque(x, y, new Color(0.45f, 0.6f, 0.55f)));
        Par("tex_asfalto", Asfalto);
        Par("tex_adoquin", Adoquin);
        Par("tex_baldosa", Baldosa);
        Par("tex_madera", Madera);
        Par("tex_calamina", Calamina);
        Par("tex_concreto", Concreto);
        Par("tex_pista", Pista);
        AssetDatabase.Refresh();

        foreach (string f in Directory.GetFiles(CarpetaTex, "*.png"))
        {
            string ruta = f.Replace("\\", "/");
            var ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (ti == null) { continue; }
            bool normal = ruta.EndsWith("_normal.png");
            ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.sRGBTexture = !normal;
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.maxTextureSize = 512;
            ti.mipmapEnabled = true;
            ti.anisoLevel = 4;
            ti.SaveAndReimport();
        }

        Mat("mat_ladrillo", "tex_ladrillo_visto", 1.2f, 0.12f, Color.white, 1f);
        Mat("mat_revoque_ocre", "tex_revoque_ocre", 2f, 0.1f, Color.white, 0.6f);
        Mat("mat_revoque_gris", "tex_revoque_gris", 2f, 0.1f, Color.white, 0.6f);
        Mat("mat_revoque_verde", "tex_revoque_verde", 2f, 0.1f, Color.white, 0.6f);
        Mat("mat_asfalto", "tex_asfalto", 4f, 0.25f, Color.white, 0.8f);
        Mat("mat_adoquin", "tex_adoquin", 1.5f, 0.2f, Color.white, 1f);
        Mat("mat_baldosa", "tex_baldosa", 1.2f, 0.55f, Color.white, 0.5f);
        Mat("mat_madera", "tex_madera", 1.5f, 0.2f, Color.white, 0.7f);
        Mat("mat_calamina", "tex_calamina", 1.5f, 0.45f, Color.white, 1f, 0.5f);
        Mat("mat_cortina", "tex_calamina", 1.2f, 0.4f, new Color(0.55f, 0.6f, 0.62f), 1f, 0.6f);
        Mat("mat_concreto", "tex_concreto", 2.5f, 0.1f, Color.white, 0.6f);
        Mat("mat_pista", "tex_pista", 2f, 0.85f, Color.white, 0.4f);
        Solido("mat_negro", new Color(0.03f, 0.03f, 0.035f), 0.3f);
        Solido("mat_metal_oscuro", new Color(0.12f, 0.13f, 0.14f), 0.55f, 0.7f);
        Solido("mat_metal_pintado", new Color(0.22f, 0.3f, 0.26f), 0.4f, 0.4f);
        Solido("mat_tela_sofa", new Color(0.35f, 0.08f, 0.12f), 0.2f);
        Solido("mat_vidrio", new Color(0.55f, 0.65f, 0.7f, 0.35f), 0.95f, 0f, true);
        Solido("mat_sangre", new Color(0.22f, 0.02f, 0.02f), 0.7f);
        Solido("mat_basura", new Color(0.05f, 0.05f, 0.06f), 0.5f);
        Solido("mat_carton", new Color(0.55f, 0.42f, 0.28f), 0.1f);
        Solido("mat_plastico_azul", new Color(0.1f, 0.25f, 0.6f), 0.4f);
        Solido("mat_toldo_rojo", new Color(0.6f, 0.12f, 0.1f), 0.15f);
        Solido("mat_verde_arbol", new Color(0.12f, 0.22f, 0.1f), 0.1f);
        Solido("mat_tronco", new Color(0.25f, 0.18f, 0.12f), 0.1f);
        Solido("mat_policia", new Color(0.12f, 0.3f, 0.18f), 0.5f, 0.3f);
        Solido("mat_barrera", new Color(0.8f, 0.7f, 0.1f), 0.3f);
        Solido("mat_farmacia_verde", new Color(0.05f, 0.55f, 0.25f), 0.3f);
        Emisivo("mat_neon_magenta", new Color(1f, 0.1f, 0.7f), 4f);
        Emisivo("mat_neon_cian", new Color(0.1f, 0.8f, 1f), 4f);
        Emisivo("mat_neon_verde", new Color(0.1f, 1f, 0.4f), 3f);
        Emisivo("mat_luz_calida", new Color(1f, 0.8f, 0.55f), 3f);
        Emisivo("mat_pantalla", new Color(0.5f, 0.7f, 1f), 1.5f);
        Emisivo("mat_fuego", new Color(1f, 0.45f, 0.1f), 6f);
        Solido("mat_mateo_arma", new Color(0.35f, 0.24f, 0.15f), 0.2f);
        Solido("mat_fierro", new Color(0.3f, 0.2f, 0.16f), 0.35f, 0.8f);
        Solido("mat_char_civil", new Color(0.35f, 0.33f, 0.3f), 0.2f);
        Solido("mat_char_fungico", new Color(0.3f, 0.28f, 0.16f), 0.35f);
        Solido("mat_char_corredor", new Color(0.46f, 0.4f, 0.38f), 0.25f);
        Solido("mat_char_policia", new Color(0.12f, 0.24f, 0.16f), 0.3f);
        Solido("mat_bata", new Color(0.8f, 0.8f, 0.78f), 0.2f);
        AssetDatabase.SaveAssets();
        return "Texturas y materiales generados en " + CarpetaTex + " y " + CarpetaMat;
    }

    // ---------- Utilidades ----------

    private static float U() { return (float)rnd.NextDouble(); }

    private static float Ruido(float x, float y, float escala, int octavas = 4)
    {
        float v = 0f, a = 0.5f, f = escala;
        for (int o = 0; o < octavas; o++)
        {
            v += a * Mathf.PerlinNoise(x * f + 17.3f * o, y * f + 9.1f * o);
            a *= 0.5f; f *= 2f;
        }
        return v;
    }

    /// <summary>Muestra periodica (para que la textura se repita sin costuras).</summary>
    private static float RuidoCiclico(float u, float v, float escala, int octavas = 4)
    {
        float a = Ruido(u, v, escala, octavas), b = Ruido(u - 1f, v, escala, octavas);
        float c = Ruido(u, v - 1f, escala, octavas), d = Ruido(u - 1f, v - 1f, escala, octavas);
        return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
    }

    // Par: genera color + mapa de normales a partir de una funcion de muestra.

    private static void Par(string nombre, System.Func<float, float, (Color, float)> f)
    {
        var col = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var alt = new float[N * N];
        var px = new Color[N * N];
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                var r = f((x + 0.5f) / N, (y + 0.5f) / N);
                px[y * N + x] = r.Item1;
                alt[y * N + x] = r.Item2;
            }
        }
        col.SetPixels(px);
        col.Apply();
        File.WriteAllBytes(Path.Combine(CarpetaTex, nombre + ".png"), col.EncodeToPNG());

        var nor = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var np = new Color[N * N];
        float fuerza = 3.5f;
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                float l = alt[y * N + (x - 1 + N) % N], rr = alt[y * N + (x + 1) % N];
                float d = alt[((y - 1 + N) % N) * N + x], u = alt[((y + 1) % N) * N + x];
                Vector3 n = new Vector3((l - rr) * fuerza, (d - u) * fuerza, 1f).normalized;
                np[y * N + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
            }
        }
        nor.SetPixels(np);
        nor.Apply();
        File.WriteAllBytes(Path.Combine(CarpetaTex, nombre + "_normal.png"), nor.EncodeToPNG());
        Object.DestroyImmediate(col);
        Object.DestroyImmediate(nor);
    }

    private static Material Crear(string nombre)
    {
        string ruta = CarpetaMat + "/" + nombre + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        return m;
    }

    private static void Mat(string nombre, string tex, float metrosPorRepeticion, float suavidad, Color tinte, float fuerzaNormal, float metalico = 0f)
    {
        Material m = Crear(nombre);
        var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(CarpetaTex + "/" + tex + ".png");
        var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(CarpetaTex + "/" + tex + "_normal.png");
        m.SetTexture("_BaseMap", albedo);
        m.SetTextureScale("_BaseMap", Vector2.one / metrosPorRepeticion);
        m.SetColor("_BaseColor", tinte);
        m.SetFloat("_Smoothness", suavidad);
        m.SetFloat("_Metallic", metalico);
        if (normal != null)
        {
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", fuerzaNormal);
            m.EnableKeyword("_NORMALMAP");
        }
        EditorUtility.SetDirty(m);
    }

    private static void Solido(string nombre, Color c, float suavidad, float metalico = 0f, bool transparente = false)
    {
        Material m = Crear(nombre);
        m.SetTexture("_BaseMap", null);
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", suavidad);
        m.SetFloat("_Metallic", metalico);
        if (transparente)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3000;
        }
        EditorUtility.SetDirty(m);
    }

    private static void Emisivo(string nombre, Color c, float intensidad)
    {
        Material m = Crear(nombre);
        m.SetColor("_BaseColor", c * 0.3f);
        m.SetColor("_EmissionColor", c * intensidad);
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        EditorUtility.SetDirty(m);
    }

    // ---------- Patrones (u, v en 0..1, textura de ~1 m) ----------

    private static (Color, float) Ladrillo(float u, float v)
    {
        const int filas = 12;
        float fy = v * filas;
        int fila = Mathf.FloorToInt(fy);
        float desplaz = (fila % 2) * 0.5f;
        float fx = u * 4f + desplaz;
        int col = Mathf.FloorToInt(fx);
        float lx = fx - col, ly = fy - fila;
        float mx = 0.045f + 0.02f * Mathf.PerlinNoise(u * 30f, v * 30f);
        float my = 0.13f + 0.05f * Mathf.PerlinNoise(u * 25f + 3f, v * 25f);
        bool mortero = lx < mx || lx > 1f - mx || ly < my || ly > 1f - my;

        int semilla = (fila * 31 + ((col % 4) + 4) % 4 * 7) % 97;
        float var = (semilla % 13) / 13f;
        Color baseL = Color.Lerp(new Color(0.55f, 0.27f, 0.17f), new Color(0.68f, 0.38f, 0.24f), var);
        baseL = Color.Lerp(baseL, new Color(0.35f, 0.18f, 0.12f), (semilla % 5 == 0) ? 0.35f : 0f);
        float n = RuidoCiclico(u, v, 22f, 3);
        float mancha = RuidoCiclico(u, v, 3f, 3);
        if (mortero)
        {
            Color m = Color.Lerp(new Color(0.5f, 0.48f, 0.44f), new Color(0.62f, 0.6f, 0.55f), n);
            return (m * Mathf.Lerp(0.8f, 1f, mancha), 0.1f + n * 0.1f);
        }
        Color c = baseL * Mathf.Lerp(0.78f, 1.1f, n) * Mathf.Lerp(0.75f, 1f, mancha);
        float borde = Mathf.Min(Mathf.Min(lx - mx, 1f - mx - lx) * 8f, Mathf.Min(ly - my, 1f - my - ly) * 3f);
        return (c, 0.6f + Mathf.Clamp01(borde) * 0.3f + n * 0.1f);
    }

    private static (Color, float) Revoque(float u, float v, Color baseC)
    {
        float n = RuidoCiclico(u, v, 6f, 5);
        float grande = RuidoCiclico(u, v, 1.5f, 3);
        float grano = RuidoCiclico(u, v, 60f, 2);
        Color c = baseC * Mathf.Lerp(0.72f, 1.08f, n) * Mathf.Lerp(0.8f, 1f, grande);
        c = Color.Lerp(c, new Color(0.3f, 0.28f, 0.25f), Mathf.Clamp01((0.35f - grande) * 2f) * (1f - v) * 0.6f);
        bool descascarado = grande > 0.62f && n > 0.55f;
        if (descascarado)
        {
            var l = Ladrillo(u, v);
            return (l.Item1 * 0.85f, 0.2f + l.Item2 * 0.3f);
        }
        return (c * Mathf.Lerp(0.95f, 1.03f, grano), 0.5f + grano * 0.1f + n * 0.05f);
    }

    private static (Color, float) Asfalto(float u, float v)
    {
        float n = RuidoCiclico(u, v, 40f, 3);
        float g = RuidoCiclico(u, v, 4f, 4);
        Color c = Color.Lerp(new Color(0.12f, 0.12f, 0.13f), new Color(0.22f, 0.22f, 0.23f), n);
        if (g > 0.6f) { c = Color.Lerp(c, new Color(0.08f, 0.08f, 0.09f), 0.6f); }
        float grieta = Mathf.Abs(RuidoCiclico(u, v, 5f, 4) - 0.5f);
        if (grieta < 0.008f) { return (c * 0.4f, 0.1f); }
        float aceite = RuidoCiclico(u + 0.3f, v, 2.5f, 3);
        if (aceite > 0.66f) { c = Color.Lerp(c, new Color(0.04f, 0.04f, 0.05f), 0.5f); }
        return (c, 0.5f + n * 0.15f);
    }

    private static (Color, float) Adoquin(float u, float v)
    {
        const int n = 6;
        float fy = v * n; int fila = Mathf.FloorToInt(fy);
        float fx = u * n + (fila % 2) * 0.5f; int col = Mathf.FloorToInt(fx);
        float lx = fx - col - 0.5f, ly = fy - fila - 0.5f;
        float d = Mathf.Max(Mathf.Abs(lx), Mathf.Abs(ly)) + Mathf.PerlinNoise(u * 40f, v * 40f) * 0.06f;
        float ruido = RuidoCiclico(u, v, 30f, 2);
        if (d > 0.42f) { return (new Color(0.18f, 0.17f, 0.15f) * Mathf.Lerp(0.8f, 1.1f, ruido), 0.1f); }
        float var = ((fila * 13 + ((col % n) + n) % n * 7) % 11) / 11f;
        Color c = Color.Lerp(new Color(0.38f, 0.37f, 0.35f), new Color(0.52f, 0.5f, 0.46f), var) * Mathf.Lerp(0.8f, 1.1f, ruido);
        return (c, 0.7f - d * 0.6f + ruido * 0.1f);
    }

    private static (Color, float) Baldosa(float u, float v)
    {
        float fx = u * 4f, fy = v * 4f;
        float lx = fx - Mathf.Floor(fx), ly = fy - Mathf.Floor(fy);
        bool junta = lx < 0.03f || ly < 0.03f;
        float n = RuidoCiclico(u, v, 20f, 2);
        float suciedad = RuidoCiclico(u, v, 3f, 3);
        if (junta) { return (new Color(0.35f, 0.33f, 0.3f), 0.2f); }
        Color c = Color.Lerp(new Color(0.85f, 0.85f, 0.82f), new Color(0.62f, 0.6f, 0.55f), suciedad * 0.8f) * Mathf.Lerp(0.95f, 1.02f, n);
        return (c, 0.6f);
    }

    private static (Color, float) Madera(float u, float v)
    {
        float tabla = Mathf.Floor(u * 6f);
        float lx = u * 6f - tabla;
        float veta = Mathf.Sin((v * 40f + Mathf.PerlinNoise(tabla * 3.1f, v * 4f) * 8f) + tabla * 5f) * 0.5f + 0.5f;
        float n = RuidoCiclico(u, v, 25f, 2);
        Color c = Color.Lerp(new Color(0.32f, 0.2f, 0.12f), new Color(0.48f, 0.32f, 0.18f), veta * 0.6f + n * 0.4f);
        c *= Mathf.Lerp(0.85f, 1.05f, Mathf.PerlinNoise(tabla * 7.7f, 1.3f));
        if (lx < 0.03f) { return (c * 0.4f, 0.1f); }
        return (c, 0.5f + veta * 0.1f);
    }

    private static (Color, float) Calamina(float u, float v)
    {
        float onda = Mathf.Sin(u * Mathf.PI * 2f * 10f) * 0.5f + 0.5f;
        float n = RuidoCiclico(u, v, 8f, 4);
        float oxido = Mathf.Clamp01((RuidoCiclico(u, v, 3f, 4) - 0.45f) * 3f);
        Color metal = Color.Lerp(new Color(0.5f, 0.52f, 0.54f), new Color(0.7f, 0.72f, 0.74f), onda * 0.5f + n * 0.3f);
        Color c = Color.Lerp(metal, new Color(0.45f, 0.22f, 0.1f) * Mathf.Lerp(0.7f, 1.1f, n), oxido);
        return (c * Mathf.Lerp(0.75f, 1f, onda), onda * 0.8f);
    }

    private static (Color, float) Concreto(float u, float v)
    {
        float n = RuidoCiclico(u, v, 12f, 5);
        float p = RuidoCiclico(u, v, 50f, 2);
        Color c = Color.Lerp(new Color(0.4f, 0.4f, 0.39f), new Color(0.58f, 0.57f, 0.55f), n) * Mathf.Lerp(0.95f, 1.05f, p);
        if (p > 0.72f) { c *= 0.7f; }
        return (c, 0.5f + n * 0.2f - (p > 0.72f ? 0.2f : 0f));
    }

    private static (Color, float) Pista(float u, float v)
    {
        float fx = u * 4f, fy = v * 4f;
        int cx = Mathf.FloorToInt(fx), cy = Mathf.FloorToInt(fy);
        float lx = fx - cx, ly = fy - cy;
        bool junta = lx < 0.02f || ly < 0.02f;
        float n = RuidoCiclico(u, v, 20f, 2);
        Color c = ((cx + cy) % 2 == 0) ? new Color(0.06f, 0.06f, 0.08f) : new Color(0.12f, 0.1f, 0.14f);
        if (junta) { c = new Color(0.02f, 0.02f, 0.02f); }
        return (c * Mathf.Lerp(0.9f, 1.1f, n), junta ? 0.3f : 0.6f);
    }
}
