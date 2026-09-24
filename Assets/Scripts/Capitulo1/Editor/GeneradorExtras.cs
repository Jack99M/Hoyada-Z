using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera el arte paceño que no venia en los assets del equipo:
/// - Graffitis con efecto de aerosol (bordes difuminados y chorreados): "EVO PUEBLO",
///   "ABAJO EL MAS", "BOLIVIA DIJO NO", "EL ALTO DE PIE", avisos del brote, futbol, amor...
/// - Letreros de tiendas y calles (Farmacia Chuquiago, Mercado Sopocachi, salteñeria,
///   pension, api con pastel, carniceria, placas de calle, ruta de minibus, teleferico).
/// - Texturas: traje de cebra, aguayo, bandera de Bolivia y wiphala.
/// - Materiales de personajes nuevos (cebra, lustrabotas, griton, carnicero) y utileria.
/// Menu: Hoyada Z / Arte / Generar graffitis y letreros. Para cambiar un texto, editar las
/// listas Graffitis / Letreros y volver a generar.
/// </summary>
public static class GeneradorExtras
{
    private const string CarpetaTex = "Assets/Arte/Texturas/Extras";
    private const string CarpetaMat = "Assets/Arte/Materiales/Nivel";

    public class Graffiti
    {
        public string id, texto, fuente;
        public Color color;
        public float spray;
        public Graffiti(string i, string t, string f, Color c, float s = 1f) { id = i; texto = t; fuente = f; color = c; spray = s; }
    }

    public class Letrero
    {
        public string id, texto, fuente;
        public Color fondo, letra;
        public bool emisivo;
        public float aspecto;
        public Letrero(string i, string t, string f, Color fo, Color le, float asp, bool em = false) { id = i; texto = t; fuente = f; fondo = fo; letra = le; aspecto = asp; emisivo = em; }
    }

    public static readonly Graffiti[] Graffitis =
    {
        new Graffiti("evo", "EVO PUEBLO", "Impact", new Color(0.78f, 0.08f, 0.08f)),
        new Graffiti("abajo_mas", "ABAJO EL MAS", "Impact", new Color(0.03f, 0.03f, 0.035f)),
        new Graffiti("21f", "BOLIVIA DIJO NO  21F", "Impact", new Color(0.12f, 0.3f, 0.7f)),
        new Graffiti("alto", "EL ALTO DE PIE\nNUNCA DE RODILLAS", "Impact", new Color(0.05f, 0.05f, 0.05f)),
        new Graffiti("fuera", "FUERA CORRUPTOS", "Impact", new Color(0.7f, 0.1f, 0.1f)),
        new Graffiti("agua", "NO TOMEN AGUA\nDEL GRIFO!!", "Ink Free", new Color(0.92f, 0.92f, 0.9f), 0.6f),
        new Graffiti("mordidos", "LOS MORDIDOS\nYA NO SON GENTE", "Ink Free", new Color(0.75f, 0.05f, 0.05f), 0.6f),
        new Graffiti("dios", "DIOS NOS CASTIGA", "Segoe Print", new Color(0.05f, 0.05f, 0.05f), 0.5f),
        new Graffiti("refugio", "REFUGIO: ESTADIO\nHERNANDO SILES", "Ink Free", new Color(0.95f, 0.8f, 0.1f), 0.6f),
        new Graffiti("jallalla", "JALLALLA LA PAZ", "Impact", new Color(0.95f, 0.75f, 0.1f)),
        new Graffiti("tigre", "THE STRONGEST CAMPEON", "Impact", new Color(0.95f, 0.8f, 0.05f)),
        new Graffiti("academia", "BOLIVAR ACADEMIA", "Impact", new Color(0.35f, 0.65f, 0.95f)),
        new Graffiti("amor", "PAMELA TE AMO <3", "Segoe Print", new Color(0.95f, 0.35f, 0.6f), 0.5f),
        new Graffiti("vivos", "AQUI HAY VIVOS\nNO ENTREN", "Ink Free", new Color(0.95f, 0.95f, 0.95f), 0.6f),
        new Graffiti("clic", "SI HACE CLIC CLIC\nCORRE", "Ink Free", new Color(0.8f, 0.05f, 0.05f), 0.6f),
        new Graffiti("choqueyapu", "EL CHOQUEYAPU\nNOS ENVENENO", "Impact", new Color(0.1f, 0.35f, 0.15f)),
        new Graffiti("resiste", "SOPOCACHI RESISTE", "Stencil", new Color(0.92f, 0.92f, 0.92f)),
        new Graffiti("x", "X", "Impact", new Color(0.8f, 0.05f, 0.05f), 1.4f),
        new Graffiti("cuarentena", "CUARENTENA", "Stencil", new Color(0.9f, 0.1f, 0.1f)),
        new Graffiti("bety", "MATEO: SUBE POR\nLAS GRADAS - BETY", "Segoe Print", new Color(0.95f, 0.95f, 0.95f), 0.4f)
    };

    public static readonly Letrero[] Letreros =
    {
        new Letrero("farmacia", "FARMACIA CHUQUIAGO", "Franklin Gothic Heavy", new Color(0.05f, 0.45f, 0.22f), Color.white, 8f),
        new Letrero("mercado", "MERCADO SOPOCACHI", "Franklin Gothic Heavy", new Color(0.55f, 0.08f, 0.06f), new Color(1f, 0.95f, 0.85f), 8f),
        new Letrero("disco", "ALTURA", "Impact", new Color(0.02f, 0.02f, 0.03f), new Color(1f, 0.2f, 0.75f), 3.2f, true),
        new Letrero("saltenas", "SALTEÑERIA DOÑA ROSITA", "Franklin Gothic Heavy", new Color(0.7f, 0.1f, 0.08f), new Color(1f, 0.85f, 0.2f), 7f),
        new Letrero("pension", "PENSION EL PACEÑITO\nALMUERZO 15 Bs.", "Rockwell Extra Bold", new Color(0.92f, 0.86f, 0.72f), new Color(0.15f, 0.1f, 0.08f), 3.4f),
        new Letrero("api", "API CON PASTEL", "Franklin Gothic Heavy", new Color(0.4f, 0.1f, 0.45f), Color.white, 5f),
        new Letrero("ferreteria", "FERRETERIA DON EFRAIN", "Franklin Gothic Heavy", new Color(0.95f, 0.78f, 0.1f), new Color(0.08f, 0.08f, 0.08f), 6f),
        new Letrero("carniceria", "CARNICERIA DON CIRILO\nPUESTO 7", "Rockwell Extra Bold", new Color(0.65f, 0.08f, 0.08f), Color.white, 3.6f),
        new Letrero("calle20", "AV. 20 DE OCTUBRE", "Bahnschrift", new Color(0.1f, 0.25f, 0.6f), Color.white, 4.5f),
        new Letrero("plaza", "PLAZA ABAROA", "Bahnschrift", new Color(0.1f, 0.25f, 0.6f), Color.white, 4f),
        new Letrero("belisario", "C. BELISARIO SALINAS", "Bahnschrift", new Color(0.1f, 0.25f, 0.6f), Color.white, 5f),
        new Letrero("teleferico", "TELEFERICO  LINEA AMARILLA  >>", "Franklin Gothic Heavy", new Color(0.98f, 0.8f, 0.1f), new Color(0.1f, 0.1f, 0.1f), 7f),
        new Letrero("minibus", "SOPOCACHI - PEREZ - CEMENTERIO - CEJA", "Franklin Gothic Heavy", new Color(0.95f, 0.95f, 0.92f), new Color(0.05f, 0.15f, 0.55f), 7f),
        new Letrero("kiosco", "PERIODICOS  RECARGAS  DULCES", "Franklin Gothic Heavy", new Color(0.12f, 0.45f, 0.75f), Color.white, 6f),
        new Letrero("cuarentena", "ZONA DE CUARENTENA\nPROHIBIDO EL PASO", "Franklin Gothic Heavy", new Color(0.98f, 0.8f, 0.1f), new Color(0.08f, 0.08f, 0.08f), 3f),
        new Letrero("cebras", "CRUZA POR LA CEBRA!", "Franklin Gothic Heavy", Color.white, new Color(0.05f, 0.05f, 0.05f), 5f),
        new Letrero("karaoke", "KARAOKE  LA CHOLITA", "Impact", new Color(0.05f, 0.02f, 0.08f), new Color(0.3f, 0.9f, 1f), 5.5f, true),
        new Letrero("sedes", "SEDES LA PAZ\nALERTA SANITARIA", "Franklin Gothic Heavy", Color.white, new Color(0.7f, 0.05f, 0.05f), 2.6f)
    };

    [MenuItem("Hoyada Z/Arte/Generar graffitis y letreros")]
    public static void GenerarMenu()
    {
        Debug.Log(Generar());
    }

    public static string Generar()
    {
        Directory.CreateDirectory(CarpetaTex);
        Directory.CreateDirectory(CarpetaMat);
        var rnd = new System.Random(21);
        int n = 0;
        var pendientes = new List<KeyValuePair<string, System.Action<Texture2D>>>();

        foreach (Graffiti g in Graffitis)
        {
            Color[] px; int w, h;
            if (!Spray(g, rnd, out px, out w, out h)) { continue; }
            string ruta = GuardarPNG("g_" + g.id, px, w, h);
            Graffiti gg = g;
            pendientes.Add(new KeyValuePair<string, System.Action<Texture2D>>(ruta, t => MatTransparente("mat_g_" + gg.id, t)));
            n++;
        }
        foreach (Letrero l in Letreros)
        {
            Color[] px; int w, h;
            if (!Placa(l, rnd, out px, out w, out h)) { continue; }
            string ruta = GuardarPNG("l_" + l.id, px, w, h);
            Letrero ll = l;
            pendientes.Add(new KeyValuePair<string, System.Action<Texture2D>>(ruta, t => MatOpaco("mat_l_" + ll.id, t, ll.emisivo ? ll.letra : (Color?)null)));
            n++;
        }

        // Texturas de tela y banderas
        pendientes.Add(new KeyValuePair<string, System.Action<Texture2D>>(GuardarPNG("t_cebra", Cebra(rnd, 256), 256, 256), t => MatOpaco("mat_cebra", t, null, 0.15f)));
        pendientes.Add(new KeyValuePair<string, System.Action<Texture2D>>(GuardarPNG("t_aguayo", Aguayo(256), 256, 256), t => MatOpaco("mat_aguayo", t, null, 0.1f)));
        pendientes.Add(new KeyValuePair<string, System.Action<Texture2D>>(GuardarPNG("t_bandera", Bandera(192, 128), 192, 128), t => MatOpaco("mat_bandera_bolivia", t, null, 0.1f)));
        pendientes.Add(new KeyValuePair<string, System.Action<Texture2D>>(GuardarPNG("t_wiphala", Wiphala(210), 210, 210), t => MatOpaco("mat_wiphala", t, null, 0.1f)));

        AssetDatabase.Refresh();
        foreach (var p in pendientes)
        {
            var imp = AssetImporter.GetAtPath(p.Key) as TextureImporter;
            if (imp != null)
            {
                imp.alphaIsTransparency = true;
                imp.wrapMode = p.Key.Contains("/t_cebra") || p.Key.Contains("/t_aguayo") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                imp.mipmapEnabled = true;
                imp.maxTextureSize = 2048;
                imp.SaveAndReimport();
            }
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p.Key);
            p.Value(tex);
        }

        // Materiales solidos de personajes y utileria
        MatSolido("mat_char_griton", new Color(0.62f, 0.6f, 0.58f), 0.3f);
        MatSolido("mat_garganta", new Color(0.5f, 0.12f, 0.12f), 0.6f);
        MatSolido("mat_char_carnicero", new Color(0.36f, 0.3f, 0.28f), 0.25f);
        MatSolido("mat_mandil", new Color(0.78f, 0.74f, 0.7f), 0.2f);
        MatSolido("mat_mandil_sangre", new Color(0.4f, 0.05f, 0.04f), 0.5f);
        MatSolido("mat_char_lustra", new Color(0.12f, 0.13f, 0.16f), 0.2f);
        MatSolido("mat_pasamontanas", new Color(0.03f, 0.03f, 0.035f), 0.15f);
        MatSolido("mat_teleferico", new Color(0.95f, 0.75f, 0.08f), 0.5f);
        MatSolido("mat_cable", new Color(0.05f, 0.05f, 0.05f), 0.6f);
        MatSolido("mat_coca", new Color(0.18f, 0.38f, 0.12f), 0.3f);
        MatSolido("mat_illa", new Color(0.85f, 0.65f, 0.25f), 0.7f, 0.6f);
        MatSolido("mat_yeso", new Color(0.85f, 0.8f, 0.72f), 0.2f);
        MatSolido("mat_revolver", new Color(0.1f, 0.1f, 0.11f), 0.6f, 0.8f);
        MatSolido("mat_honda", new Color(0.55f, 0.3f, 0.2f), 0.1f);
        MatEmisivo("mat_vela", new Color(1f, 0.75f, 0.35f), 3f);
        MatEmisivo("mat_ventana_cabina", new Color(1f, 0.85f, 0.6f), 1.5f);
        AssetDatabase.SaveAssets();
        return "Graffitis, letreros y texturas generados: " + (n + 4);
    }

    // ---------- Texto con fuentes del sistema ----------

    private static Color[] LeerAtlas(Font f)
    {
        Texture atlas = f.material.mainTexture;
        var rt = RenderTexture.GetTemporary(atlas.width, atlas.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(atlas, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        var t = new Texture2D(atlas.width, atlas.height, TextureFormat.RGBA32, false, true);
        t.ReadPixels(new Rect(0, 0, atlas.width, atlas.height), 0, 0);
        t.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        Color[] px = t.GetPixels();
        Object.DestroyImmediate(t);
        return px;
    }

    /// <summary>Rasteriza texto (varias lineas, centrado) a una mascara de alfa. Devuelve false si la fuente no existe.</summary>
    private static bool Rasterizar(string texto, string fuente, int tam, int margen, int extraAbajo, out float[] mascara, out int w, out int h)
    {
        mascara = null; w = h = 0;
        Font f = Font.CreateDynamicFontFromOSFont(fuente, tam);
        if (f == null) { f = Font.CreateDynamicFontFromOSFont("Arial", tam); }
        if (f == null) { return false; }
        string todos = texto.Replace("\n", "");
        f.RequestCharactersInTexture(todos, tam, FontStyle.Normal);
        string[] lineas = texto.Split('\n');
        var anchos = new int[lineas.Length];
        int maxAncho = 1;
        for (int i = 0; i < lineas.Length; i++)
        {
            int a = 0;
            foreach (char c in lineas[i])
            {
                CharacterInfo ci;
                if (f.GetCharacterInfo(c, out ci, tam, FontStyle.Normal)) { a += ci.advance; }
            }
            anchos[i] = a;
            maxAncho = Mathf.Max(maxAncho, a);
        }
        int alto = Mathf.RoundToInt(tam * 1.18f);
        w = maxAncho + margen * 2;
        h = alto * lineas.Length + margen * 2 + extraAbajo;
        mascara = new float[w * h];
        Texture atlas = f.material.mainTexture;
        int aw = atlas.width, ah = atlas.height;
        Color[] apx = LeerAtlas(f);

        for (int li = 0; li < lineas.Length; li++)
        {
            int baseY = h - margen - alto * (li + 1) + Mathf.RoundToInt(tam * 0.22f);
            int pen = margen + (maxAncho - anchos[li]) / 2;
            foreach (char c in lineas[li])
            {
                CharacterInfo ci;
                if (!f.GetCharacterInfo(c, out ci, tam, FontStyle.Normal)) { continue; }
                int gw = ci.maxX - ci.minX, gh = ci.maxY - ci.minY;
                for (int y = 0; y < gh; y++)
                {
                    for (int x = 0; x < gw; x++)
                    {
                        float sx = (x + 0.5f) / gw, sy = (y + 0.5f) / gh;
                        Vector2 uv = ci.uvBottomLeft + (ci.uvBottomRight - ci.uvBottomLeft) * sx + (ci.uvTopLeft - ci.uvBottomLeft) * sy;
                        float u = uv.x * aw - 0.5f, v = uv.y * ah - 0.5f;
                        int x0 = Mathf.Clamp(Mathf.FloorToInt(u), 0, aw - 1), y0 = Mathf.Clamp(Mathf.FloorToInt(v), 0, ah - 1);
                        int x1 = Mathf.Min(x0 + 1, aw - 1), y1 = Mathf.Min(y0 + 1, ah - 1);
                        float fx = u - Mathf.Floor(u), fy = v - Mathf.Floor(v);
                        float a00 = A(apx[y0 * aw + x0]), a10 = A(apx[y0 * aw + x1]), a01 = A(apx[y1 * aw + x0]), a11 = A(apx[y1 * aw + x1]);
                        float a = Mathf.Lerp(Mathf.Lerp(a00, a10, fx), Mathf.Lerp(a01, a11, fx), fy);
                        int ox = pen + ci.minX + x, oy = baseY + ci.minY + y;
                        if (ox < 0 || oy < 0 || ox >= w || oy >= h) { continue; }
                        mascara[oy * w + ox] = Mathf.Max(mascara[oy * w + ox], a);
                    }
                }
                pen += ci.advance;
            }
        }
        return true;
    }

    private static float A(Color c) { return Mathf.Max(c.a, c.r); }

    private static float[] Desenfocar(float[] m, int w, int h, int r)
    {
        var tmp = new float[m.Length];
        var outp = new float[m.Length];
        for (int y = 0; y < h; y++)
        {
            float acc = 0f; int cnt = 0;
            for (int x = -r; x < w + r; x++)
            {
                int xa = x + r, xs = x - r - 1;
                if (xa >= 0 && xa < w) { acc += m[y * w + xa]; cnt++; }
                if (xs >= 0 && xs < w) { acc -= m[y * w + xs]; cnt--; }
                if (x >= 0 && x < w) { tmp[y * w + x] = cnt > 0 ? acc / (2 * r + 1) : 0f; }
            }
        }
        for (int x = 0; x < w; x++)
        {
            float acc = 0f;
            for (int y = -r; y < h + r; y++)
            {
                int ya = y + r, ys = y - r - 1;
                if (ya >= 0 && ya < h) { acc += tmp[ya * w + x]; }
                if (ys >= 0 && ys < h) { acc -= tmp[ys * w + x]; }
                if (y >= 0 && y < h) { outp[y * w + x] = acc / (2 * r + 1); }
            }
        }
        return outp;
    }

    private static bool Spray(Graffiti g, System.Random rnd, out Color[] px, out int w, out int h)
    {
        px = null;
        int tam = g.texto.Length > 16 && !g.texto.Contains("\n") ? 110 : 140;
        float[] m;
        if (!Rasterizar(g.texto, g.fuente, tam, 40, 70, out m, out w, out h)) { return false; }
        if (w > 2048)
        {
            tam = Mathf.RoundToInt(tam * 2000f / w);
            if (!Rasterizar(g.texto, g.fuente, tam, 40, 70, out m, out w, out h)) { return false; }
        }
        float[] halo = Desenfocar(m, w, h, Mathf.RoundToInt(6 * g.spray));
        float[] suave = Desenfocar(m, w, h, 1);
        var goteo = new float[m.Length];
        for (int x = 1; x < w - 1; x++)
        {
            for (int y = 1; y < h; y++)
            {
                if (m[y * w + x] > 0.6f && m[(y - 1) * w + x] < 0.3f && rnd.NextDouble() < 0.035 * g.spray)
                {
                    int largo = 8 + rnd.Next(55);
                    int ancho = 1 + rnd.Next(2);
                    for (int k = 0; k < largo && y - k >= 0; k++)
                    {
                        float a = 0.85f * (1f - k / (float)largo * 0.6f);
                        for (int dx = 0; dx <= ancho; dx++)
                        {
                            int xx = Mathf.Min(w - 1, x + dx);
                            goteo[(y - k) * w + xx] = Mathf.Max(goteo[(y - k) * w + xx], a);
                        }
                    }
                    int cy = Mathf.Max(0, y - largo);
                    for (int dy = -2; dy <= 2; dy++) { for (int dx = -1; dx <= ancho + 1; dx++) { int yy = cy + dy, xx = x + dx; if (yy >= 0 && yy < h && xx >= 0 && xx < w) { goteo[yy * w + xx] = Mathf.Max(goteo[yy * w + xx], 0.7f); } } }
                }
            }
        }
        px = new Color[w * h];
        float semilla = (float)rnd.NextDouble() * 100f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                float ruido = Mathf.PerlinNoise(x * 0.05f + semilla, y * 0.05f);
                float grano = (float)rnd.NextDouble();
                float cuerpo = suave[i] * (0.82f + 0.18f * ruido);
                float niebla = halo[i] * 0.42f * g.spray * (grano > 0.35f ? 1f : 0.3f);
                float a = Mathf.Clamp01(Mathf.Max(cuerpo, Mathf.Max(niebla, goteo[i])));
                Color c = g.color * (0.9f + 0.15f * ruido);
                c.a = a * 0.95f;
                px[i] = c;
            }
        }
        return true;
    }

    private static bool Placa(Letrero l, System.Random rnd, out Color[] px, out int w, out int h)
    {
        px = null;
        float[] m;
        int lineas = l.texto.Split('\n').Length;
        int tam = lineas > 1 ? 90 : 120;
        if (!Rasterizar(l.texto, l.fuente, tam, 30, 0, out m, out w, out h)) { return false; }
        // Ajustar al aspecto pedido (ancho/alto) centrando el texto
        int hw = Mathf.Max(h, Mathf.RoundToInt(w / l.aspecto));
        int ww = Mathf.Max(w, Mathf.RoundToInt(hw * l.aspecto));
        ww = Mathf.Min(ww, 2048);
        var outp = new Color[ww * hw];
        int ox = (ww - w) / 2, oy = (hw - h) / 2;
        float semilla = (float)rnd.NextDouble() * 50f;
        int borde = Mathf.Max(6, hw / 18);
        for (int y = 0; y < hw; y++)
        {
            for (int x = 0; x < ww; x++)
            {
                float mugre = Mathf.PerlinNoise(x * 0.02f + semilla, y * 0.02f) * 0.25f + Mathf.PerlinNoise(x * 0.2f, y * 0.2f + semilla) * 0.08f;
                Color fondo = l.fondo * (1f - mugre * 0.6f);
                bool enBorde = x < borde || y < borde || x >= ww - borde || y >= hw - borde;
                if (enBorde && !l.emisivo) { fondo = Color.Lerp(fondo, l.letra, 0.7f) * (1f - mugre * 0.5f); }
                float a = 0f;
                int mx = x - ox, my = y - oy;
                if (mx >= 0 && my >= 0 && mx < w && my < h) { a = m[my * w + mx]; }
                Color c = Color.Lerp(fondo, l.letra * (l.emisivo ? 1f : (1f - mugre * 0.4f)), a);
                c.a = 1f;
                outp[y * ww + x] = c;
            }
        }
        w = ww; h = hw;
        px = outp;
        return true;
    }

    // ---------- Telas y banderas ----------

    private static Color[] Cebra(System.Random rnd, int n)
    {
        var px = new Color[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float u = x / (float)n, v = y / (float)n;
                float onda = Mathf.Sin((u * 6f + Mathf.Sin(v * Mathf.PI * 2f * 2f) * 0.25f + Mathf.PerlinNoise(u * 4f, v * 4f) * 0.4f) * Mathf.PI * 2f);
                float negro = Mathf.Clamp01(0.5f - onda * 4f);
                Color c = Color.Lerp(new Color(0.93f, 0.93f, 0.9f), new Color(0.05f, 0.05f, 0.06f), negro);
                c *= 0.92f + 0.08f * Mathf.PerlinNoise(u * 40f, v * 40f);
                c.a = 1f;
                px[y * n + x] = c;
            }
        }
        return px;
    }

    private static Color[] Aguayo(int n)
    {
        Color[] bandas = { new Color(0.75f, 0.1f, 0.15f), new Color(0.95f, 0.55f, 0.1f), new Color(0.1f, 0.45f, 0.3f), new Color(0.95f, 0.85f, 0.2f), new Color(0.45f, 0.1f, 0.4f), new Color(0.1f, 0.3f, 0.6f), new Color(0.85f, 0.2f, 0.45f) };
        var px = new Color[n * n];
        for (int y = 0; y < n; y++)
        {
            int banda = (y / 12) % bandas.Length;
            for (int x = 0; x < n; x++)
            {
                Color c = bandas[banda];
                int ly = y % 12;
                if (ly == 0 || ly == 11) { c = new Color(0.1f, 0.08f, 0.08f); }
                else if (banda % 2 == 0)
                {
                    int dx = Mathf.Abs((x % 12) - 6), dy = Mathf.Abs(ly - 6);
                    if (dx + dy == 3) { c = Color.white * 0.9f; }
                }
                c *= 0.88f + 0.12f * ((x + y) % 2);
                c.a = 1f;
                px[y * n + x] = c;
            }
        }
        return px;
    }

    private static Color[] Bandera(int w, int h)
    {
        var px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            Color c = y > h * 2 / 3 ? new Color(0.84f, 0.12f, 0.12f) : (y > h / 3 ? new Color(0.98f, 0.85f, 0.1f) : new Color(0.0f, 0.5f, 0.2f));
            for (int x = 0; x < w; x++) { px[y * w + x] = c; }
        }
        return px;
    }

    private static Color[] Wiphala(int n)
    {
        Color[] col = { new Color(0.85f, 0.1f, 0.1f), new Color(0.95f, 0.5f, 0.1f), new Color(0.98f, 0.9f, 0.15f), Color.white, new Color(0.1f, 0.6f, 0.25f), new Color(0.1f, 0.35f, 0.75f), new Color(0.45f, 0.15f, 0.55f) };
        var px = new Color[n * n];
        int c = n / 7;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                int cx = Mathf.Min(6, x / c), cy = Mathf.Min(6, (n - 1 - y) / c);
                int idx = ((cx - cy) % 7 + 7 + 3) % 7;
                px[y * n + x] = col[idx];
            }
        }
        return px;
    }

    // ---------- Guardado y materiales ----------

    private static string GuardarPNG(string nombre, Color[] px, int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, true);
        t.SetPixels(px);
        t.Apply();
        string ruta = CarpetaTex + "/" + nombre + ".png";
        File.WriteAllBytes(ruta, t.EncodeToPNG());
        Object.DestroyImmediate(t);
        return ruta;
    }

    private static Material Crear(string nombre)
    {
        string ruta = CarpetaMat + "/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.shader = Shader.Find("Universal Render Pipeline/Lit");
        return m;
    }

    private static void MatTransparente(string nombre, Texture2D tex)
    {
        Material m = Crear(nombre);
        m.SetTexture("_BaseMap", tex);
        m.mainTexture = tex;
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Smoothness", 0f);
        m.SetFloat("_Metallic", 0f);
        m.SetFloat("_BlendModePreserveSpecular", 0f);
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.SetFloat("_EnvironmentReflections", 0f);
        m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        m.SetFloat("_SpecularHighlights", 0f);
        m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", 2f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = 3000;
        EditorUtility.SetDirty(m);
    }

    private static void MatOpaco(string nombre, Texture2D tex, Color? emision, float suave = 0.25f)
    {
        Material m = Crear(nombre);
        m.SetTexture("_BaseMap", tex);
        m.mainTexture = tex;
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Smoothness", suave);
        m.SetFloat("_Surface", 0f);
        m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = -1;
        if (emision.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetTexture("_EmissionMap", tex);
            m.SetColor("_EmissionColor", Color.white * 2.2f);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(m);
    }

    private static void MatSolido(string nombre, Color c, float suave, float metal = 0f)
    {
        Material m = Crear(nombre);
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", suave);
        m.SetFloat("_Metallic", metal);
        EditorUtility.SetDirty(m);
    }

    private static void MatEmisivo(string nombre, Color c, float fuerza)
    {
        Material m = Crear(nombre);
        m.SetColor("_BaseColor", c);
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", c * fuerza);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        EditorUtility.SetDirty(m);
    }
}
