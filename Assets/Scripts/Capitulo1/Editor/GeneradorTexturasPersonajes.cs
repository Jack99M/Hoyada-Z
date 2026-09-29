using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Texturas pintadas para los cuerpos de los personajes.
/// Antes todos los cuerpos usaban un material de color plano (se veían grises y sin textura).
/// Este generador usa los huesos del rig para saber qué parte del cuerpo es cada vértice
/// (cabeza, torso, brazos, piernas, pies) y pinta piel, pelo, ropa, sangre y suciedad en el espacio UV.
/// Menú: Hoyada Z/Texturas/Pintar cuerpos de personajes
/// </summary>
public static class GeneradorTexturasPersonajes
{
    private const string CarpetaTex = "Assets/Arte/Texturas/Personajes";
    private const string CarpetaMat = "Assets/Arte/Materiales/Personajes";
    private const int N = 1024;

    private enum Parte { Cabeza, Cuello, Torso, Cadera, BrazoSup, Antebrazo, Mano, Muslo, Pierna, Pie }
    private enum Patron { Liso, Rayas, Cuadros, Reflectivo, Chaleco, Capucha, Aguayo, Mandil }

    private class Atuendo
    {
        public string id;
        public string rig = "Rig_Infectado";
        public Color piel = C(0.60f, 0.42f, 0.31f);
        public Color pelo = C(0.05f, 0.045f, 0.04f);
        public Color torso = C(0.5f, 0.5f, 0.5f);
        public Color torso2 = C(0.3f, 0.3f, 0.3f);
        public Color manga = C(-1f, 0f, 0f);          // r negativo = igual que el torso
        public Color pantalon = C(0.2f, 0.22f, 0.3f);
        public Color zapato = C(0.07f, 0.065f, 0.06f);
        public Color suela = C(-1f, 0f, 0f);
        public Color medias = C(0.62f, 0.52f, 0.42f);
        public Color sombreroColor = C(0.3f, 0.2f, 0.13f);
        public Patron patron = Patron.Liso;
        public bool mangaCorta, jean, falda, sombrero, pasamontanas, botas;
        public float infeccion;                       // 0 sano, 1 infectado
        public float sangre, suciedad = 0.2f, rotura, hongos;
        public float suavidad = 0.18f;
    }

    // Material plano original -> { atuendo si es infectado o cadáver, atuendo si está sano }
    private static readonly Dictionary<string, string[]> Mapa = new Dictionary<string, string[]>
    {
        { "mat_char_infectado", new[] { "infectado", "infectado" } },
        { "mat_char_corredor", new[] { "corredor", "corredor" } },
        { "mat_char_fungico", new[] { "fungico", "fungico" } },
        { "mat_char_griton", new[] { "griton", "griton" } },
        { "mat_char_carnicero", new[] { "carnicero", "carnicero" } },
        { "mat_char_policia", new[] { "policia", "policia" } },
        { "mat_verde_policia", new[] { "paco", "paco" } },
        { "mat_char_civil", new[] { "civil", "civil_sano" } },
        { "mat_char_tito", new[] { "tecnico", "tito" } },
        { "mat_char_chofer", new[] { "chofer", "chofer" } },
        { "mat_verde_arbol", new[] { "wilmer", "wilmer" } },
        { "mat_char_lustra", new[] { "lustra", "lustra" } },
        { "mat_char_mateo", new[] { "mateo", "mateo" } },
        { "mat_char_bety", new[] { "bety", "bety" } },
        { "mat_bata", new[] { "doctora", "doctora" } },
    };

    private static Color C(float r, float g, float b) { return new Color(r, g, b, 1f); }

    private static Dictionary<string, Atuendo> Atuendos()
    {
        var d = new Dictionary<string, Atuendo>();
        void Add(Atuendo a) { d[a.id] = a; }

        Add(new Atuendo { id = "infectado", torso = C(0.66f, 0.63f, 0.56f), pantalon = C(0.33f, 0.27f, 0.2f), zapato = C(0.16f, 0.11f, 0.08f), mangaCorta = true, infeccion = 1f, sangre = 0.8f, suciedad = 0.6f, rotura = 0.5f });
        Add(new Atuendo { id = "corredor", torso = C(0.62f, 0.13f, 0.11f), patron = Patron.Capucha, pantalon = C(0.1f, 0.12f, 0.2f), zapato = C(0.78f, 0.78f, 0.75f), suela = C(0.9f, 0.9f, 0.88f), infeccion = 1f, sangre = 0.9f, suciedad = 0.4f, rotura = 0.3f, suavidad = 0.25f });
        Add(new Atuendo { id = "fungico", torso = C(0.33f, 0.3f, 0.2f), pantalon = C(0.22f, 0.19f, 0.14f), infeccion = 1f, hongos = 1f, sangre = 0.3f, suciedad = 0.8f, rotura = 0.6f });
        Add(new Atuendo { id = "griton", piel = C(0.78f, 0.75f, 0.7f), torso = C(0.52f, 0.66f, 0.64f), pantalon = C(0.5f, 0.63f, 0.61f), zapato = C(0.3f, 0.3f, 0.3f), infeccion = 1f, sangre = 1f, suciedad = 0.4f, rotura = 0.4f });
        Add(new Atuendo { id = "carnicero", torso = C(0.82f, 0.8f, 0.74f), patron = Patron.Mandil, pantalon = C(0.18f, 0.17f, 0.18f), zapato = C(0.05f, 0.05f, 0.06f), botas = true, infeccion = 1f, sangre = 1.3f, suciedad = 0.5f, rotura = 0.2f });
        Add(new Atuendo { id = "doctora", torso = C(0.86f, 0.86f, 0.84f), pantalon = C(0.45f, 0.6f, 0.65f), zapato = C(0.8f, 0.8f, 0.78f), infeccion = 1f, sangre = 0.9f, suciedad = 0.35f, rotura = 0.3f });
        Add(new Atuendo { id = "policia", torso = C(0.22f, 0.31f, 0.2f), torso2 = C(0.82f, 0.86f, 0.25f), patron = Patron.Reflectivo, pantalon = C(0.2f, 0.28f, 0.18f), botas = true, infeccion = 1f, sangre = 0.7f, suciedad = 0.4f, rotura = 0.25f });
        Add(new Atuendo { id = "paco", torso = C(0.2f, 0.29f, 0.19f), patron = Patron.Chaleco, pantalon = C(0.2f, 0.28f, 0.18f), botas = true, infeccion = 1f, sangre = 0.8f, suciedad = 0.4f, rotura = 0.2f, suavidad = 0.25f });
        Add(new Atuendo { id = "civil", torso = C(0.55f, 0.2f, 0.15f), torso2 = C(0.18f, 0.14f, 0.13f), patron = Patron.Cuadros, jean = true, pantalon = C(0.2f, 0.27f, 0.42f), zapato = C(0.2f, 0.13f, 0.08f), infeccion = 1f, sangre = 0.7f, suciedad = 0.5f, rotura = 0.35f });
        Add(new Atuendo { id = "civil_sano", torso = C(0.55f, 0.2f, 0.15f), torso2 = C(0.18f, 0.14f, 0.13f), patron = Patron.Cuadros, jean = true, pantalon = C(0.2f, 0.27f, 0.42f), zapato = C(0.2f, 0.13f, 0.08f), suciedad = 0.35f, rotura = 0.05f });
        Add(new Atuendo { id = "tito", piel = C(0.58f, 0.4f, 0.29f), torso = C(0.17f, 0.26f, 0.46f), torso2 = C(0.9f, 0.78f, 0.2f), patron = Patron.Reflectivo, pantalon = C(0.14f, 0.16f, 0.22f), botas = true, suciedad = 0.35f });
        Add(new Atuendo { id = "tecnico", torso = C(0.17f, 0.26f, 0.46f), torso2 = C(0.9f, 0.78f, 0.2f), patron = Patron.Reflectivo, pantalon = C(0.14f, 0.16f, 0.22f), botas = true, infeccion = 1f, sangre = 0.7f, suciedad = 0.45f, rotura = 0.3f });
        Add(new Atuendo { id = "chofer", piel = C(0.55f, 0.38f, 0.28f), torso = C(0.36f, 0.25f, 0.17f), pantalon = C(0.3f, 0.3f, 0.32f), suciedad = 0.3f, suavidad = 0.35f });
        Add(new Atuendo { id = "wilmer", torso = C(0.16f, 0.3f, 0.16f), jean = true, pantalon = C(0.2f, 0.25f, 0.38f), zapato = C(0.25f, 0.25f, 0.27f), suela = C(0.88f, 0.88f, 0.86f), suciedad = 0.3f });
        Add(new Atuendo { id = "lustra", pasamontanas = true, torso = C(0.13f, 0.13f, 0.15f), pantalon = C(0.14f, 0.14f, 0.16f), zapato = C(0.1f, 0.08f, 0.07f), suciedad = 0.5f });
        Add(new Atuendo { id = "mateo", rig = "Rig_Mateo", piel = C(0.6f, 0.42f, 0.3f), torso = C(0.15f, 0.23f, 0.4f), patron = Patron.Capucha, jean = true, pantalon = C(0.17f, 0.21f, 0.32f), zapato = C(0.12f, 0.12f, 0.13f), suela = C(0.85f, 0.85f, 0.83f), suciedad = 0.25f });
        Add(new Atuendo { id = "bety", rig = "Rig_Bety", piel = C(0.58f, 0.4f, 0.3f), patron = Patron.Aguayo, manga = C(0.26f, 0.14f, 0.1f), falda = true, pantalon = C(0.45f, 0.13f, 0.12f), sombrero = true, zapato = C(0.08f, 0.07f, 0.07f), suciedad = 0.2f });
        return d;
    }

    // ------------------------------------------------------------------ Menú / API

    [MenuItem("Hoyada Z/Texturas/Pintar cuerpos de personajes")]
    public static void Menu()
    {
        Debug.Log(Generar());
        Debug.Log(AplicarEnEscenas());
    }

    /// <summary>Pinta las texturas y crea los materiales mat_cuerpo_*. soloId = null pinta todos.</summary>
    public static string Generar(string soloId = null)
    {
        Carpeta("Assets/Arte/Texturas", "Personajes");
        Carpeta("Assets/Arte/Materiales", "Personajes");
        var sb = new StringBuilder();
        var cache = new Dictionary<string, InfoMalla>();
        foreach (var a in Atuendos().Values)
        {
            if (soloId != null && a.id != soloId) { continue; }
            if (!cache.TryGetValue(a.rig, out InfoMalla im))
            {
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/" + a.rig + ".prefab");
                var smr = pf != null ? pf.GetComponentInChildren<SkinnedMeshRenderer>(true) : null;
                if (smr == null || smr.sharedMesh == null) { sb.AppendLine("Sin rig: " + a.rig); continue; }
                im = Analizar(smr);
                cache[a.rig] = im;
            }
            var tex = Hornear(a, im);
            CrearMaterial(a, tex);
            sb.AppendLine("Pintado: " + a.id);
        }
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    /// <summary>Cambia los materiales planos de los cuerpos (SkinnedMeshRenderer) de las escenas abiertas por los texturizados.</summary>
    public static string AplicarEnEscena()
    {
        int n = 0;
        var faltan = new HashSet<string>();
        foreach (var smr in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var mats = smr.sharedMaterials;
            bool cambio = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) { continue; }
                var nuevo = Cuerpo(mats[i], smr.sharedMesh, EsInfectado(smr.transform));
                if (nuevo != null && nuevo != mats[i]) { mats[i] = nuevo; cambio = true; }
                else if (nuevo == null && mats[i].GetTexture("_BaseMap") == null) { faltan.Add(mats[i].name); }
            }
            if (cambio)
            {
                Undo.RecordObject(smr, "Cuerpos texturizados");
                smr.sharedMaterials = mats;
                EditorUtility.SetDirty(smr);
                n++;
            }
        }
        if (n > 0) { EditorSceneManager.MarkAllScenesDirty(); }
        return "Cuerpos texturizados: " + n + (faltan.Count > 0 ? " | siguen sin textura: " + string.Join(", ", faltan) : "");
    }

    /// <summary>Abre el Capítulo 1 y 2, aplica los cuerpos texturizados y guarda.</summary>
    public static string AplicarEnEscenas()
    {
        string previa = EditorSceneManager.GetActiveScene().path;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) { return "Cancelado."; }
        var sb = new StringBuilder();
        foreach (var ruta in new[] { "Assets/Scenes/Capitulo1_Resaca.unity", "Assets/Scenes/Capitulo2_CorteDePaso.unity" })
        {
            if (!File.Exists(ruta)) { continue; }
            var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            sb.AppendLine(Path.GetFileNameWithoutExtension(ruta) + ": " + AplicarEnEscena());
            EditorSceneManager.SaveScene(escena);
        }
        if (!string.IsNullOrEmpty(previa)) { EditorSceneManager.OpenScene(previa, OpenSceneMode.Single); }
        return sb.ToString();
    }

    /// <summary>Devuelve el material texturizado que corresponde a un material plano (o null si no hay).</summary>
    public static Material Cuerpo(Material plano, Mesh malla, bool infectado)
    {
        if (plano == null || malla == null) { return null; }
        string origen = plano.name;
        if (origen.StartsWith("mat_cuerpo_"))
        {
            string id = origen.Substring("mat_cuerpo_".Length);
            origen = null;
            foreach (var kv in Mapa) { if (kv.Value[0] == id || kv.Value[1] == id) { origen = kv.Key; break; } }
            if (origen == null) { return null; }
        }
        if (!Mapa.TryGetValue(origen, out string[] ids)) { return null; }
        var atuendos = Atuendos();
        string elegido = infectado ? ids[0] : ids[1];
        if (!atuendos.TryGetValue(elegido, out Atuendo a)) { return null; }
        if (malla.name != a.rig.Substring(4) + "_rig") { return null; }
        return AssetDatabase.LoadAssetAtPath<Material>(CarpetaMat + "/mat_cuerpo_" + a.id + ".mat");
    }

    private static bool EsInfectado(Transform t)
    {
        if (t.GetComponentInParent<Infectado>(true) != null) { return true; }
        var pose = t.GetComponentInParent<PoseNPC>(true);
        if (pose != null && pose.pose == PoseNPC.Pose.Cadaver) { return true; }
        for (var p = t; p != null; p = p.parent)
        {
            if (p.name.Contains("Cadaver") || p.name.Contains("Muerto")) { return true; }
        }
        return false;
    }

    // ------------------------------------------------------------------ Análisis de la malla

    private class InfoMalla
    {
        public Vector3[] v;
        public Vector2[] uv;
        public int[] tri;
        public Parte[] parte;
        public float minY, altura, cinturaY;
        public Vector3 centro, adelante, derecha, cabezaCentro;
        public float cabezaMinY, cabezaMaxY, cabezaRadio, cuelloMinY, cuelloMaxY;
    }

    private static Parte ParteDeHueso(string n)
    {
        if (n.StartsWith("Head")) { return Parte.Cabeza; }
        if (n.StartsWith("Neck")) { return Parte.Cuello; }
        if (n.StartsWith("Chest") || n.StartsWith("Spine")) { return Parte.Torso; }
        if (n.StartsWith("Hips")) { return Parte.Cadera; }
        if (n.StartsWith("UpperArm")) { return Parte.BrazoSup; }
        if (n.StartsWith("LowerArm")) { return Parte.Antebrazo; }
        if (n.StartsWith("Hand")) { return Parte.Mano; }
        if (n.StartsWith("UpperLeg")) { return Parte.Muslo; }
        if (n.StartsWith("LowerLeg")) { return Parte.Pierna; }
        return Parte.Pie;
    }

    private static InfoMalla Analizar(SkinnedMeshRenderer smr)
    {
        var m = smr.sharedMesh;
        var im = new InfoMalla { v = m.vertices, uv = m.uv, tri = m.triangles };
        var bw = m.boneWeights;
        var huesos = smr.bones;
        var partesHueso = new Parte[huesos.Length];
        for (int i = 0; i < huesos.Length; i++) { partesHueso[i] = ParteDeHueso(huesos[i] != null ? huesos[i].name : ""); }
        im.parte = new Parte[im.v.Length];
        for (int i = 0; i < im.v.Length; i++)
        {
            var w = bw[i];
            int idx = w.boneIndex0; float mx = w.weight0;
            if (w.weight1 > mx) { mx = w.weight1; idx = w.boneIndex1; }
            if (w.weight2 > mx) { mx = w.weight2; idx = w.boneIndex2; }
            if (w.weight3 > mx) { idx = w.boneIndex3; }
            im.parte[i] = partesHueso[Mathf.Clamp(idx, 0, huesos.Length - 1)];
        }
        var b = m.bounds;
        im.minY = b.min.y; im.altura = Mathf.Max(0.001f, b.size.y); im.centro = b.center;

        Vector3 ad = Centro(im, Parte.Pie) - Centro(im, Parte.Pierna); ad.y = 0f;
        im.adelante = ad.sqrMagnitude > 1e-8f ? ad.normalized : Vector3.forward;
        im.derecha = Vector3.Cross(Vector3.up, im.adelante);

        im.cabezaCentro = Centro(im, Parte.Cabeza);
        float mn = float.MaxValue, mxY = float.MinValue, rad = 0f, cMin = float.MaxValue, cMax = float.MinValue; int n = 0;
        var alturasCadera = new List<float>();
        for (int i = 0; i < im.v.Length; i++)
        {
            var p = im.v[i];
            if (im.parte[i] == Parte.Cabeza)
            {
                mn = Mathf.Min(mn, p.y); mxY = Mathf.Max(mxY, p.y);
                var d = p - im.cabezaCentro; d.y = 0f; rad += d.magnitude; n++;
            }
            else if (im.parte[i] == Parte.Cadera) { alturasCadera.Add((p.y - im.minY) / im.altura); }
            else if (im.parte[i] == Parte.Cuello) { cMin = Mathf.Min(cMin, p.y); cMax = Mathf.Max(cMax, p.y); }
        }
        im.cabezaMinY = n > 0 ? mn : im.minY + im.altura * 0.85f;
        im.cabezaMaxY = n > 0 ? mxY : im.minY + im.altura;
        im.cabezaRadio = n > 0 ? rad / n * 1.25f : im.altura * 0.06f;
        im.cuelloMinY = cMin < cMax ? cMin : im.cabezaMinY - im.altura * 0.05f;
        im.cuelloMaxY = cMin < cMax ? cMax : im.cabezaMinY;
        alturasCadera.Sort();
        im.cinturaY = alturasCadera.Count > 0 ? alturasCadera[(int)((alturasCadera.Count - 1) * 0.93f)] : 0.52f;
        return im;
    }

    private static Vector3 Centro(InfoMalla im, Parte parte)
    {
        Vector3 s = Vector3.zero; int n = 0;
        for (int i = 0; i < im.v.Length; i++) { if (im.parte[i] == parte) { s += im.v[i]; n++; } }
        return n > 0 ? s / n : im.centro;
    }

    // ------------------------------------------------------------------ Horneado en UV

    private static Texture2D Hornear(Atuendo a, InfoMalla im)
    {
        var col = new Color[N * N];
        var lleno = new bool[N * N];
        var v = im.v; var uv = im.uv; var tri = im.tri;
        for (int t = 0; t < tri.Length; t += 3)
        {
            int i0 = tri[t], i1 = tri[t + 1], i2 = tri[t + 2];
            Vector2 A = uv[i0] * N, B = uv[i1] * N, Cc = uv[i2] * N;
            float area = (B.x - A.x) * (Cc.y - A.y) - (Cc.x - A.x) * (B.y - A.y);
            if (Mathf.Abs(area) < 1e-7f) { continue; }
            int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(A.x, Mathf.Min(B.x, Cc.x))));
            int x1 = Mathf.Min(N - 1, Mathf.CeilToInt(Mathf.Max(A.x, Mathf.Max(B.x, Cc.x))));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(A.y, Mathf.Min(B.y, Cc.y))));
            int y1 = Mathf.Min(N - 1, Mathf.CeilToInt(Mathf.Max(A.y, Mathf.Max(B.y, Cc.y))));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float w0 = ((B.x - px) * (Cc.y - py) - (Cc.x - px) * (B.y - py)) / area;
                    float w1 = ((px - A.x) * (Cc.y - A.y) - (Cc.x - A.x) * (py - A.y)) / area;
                    float w2 = 1f - w0 - w1;
                    if (w0 < -0.01f || w1 < -0.01f || w2 < -0.01f) { continue; }
                    int k = y * N + x;
                    if (lleno[k]) { continue; }
                    Vector3 p = v[i0] * w0 + v[i1] * w1 + v[i2] * w2;
                    Parte pa = (w0 >= w1 && w0 >= w2) ? im.parte[i0] : (w1 >= w2 ? im.parte[i1] : im.parte[i2]);
                    col[k] = Pintar(a, im, pa, p);
                    lleno[k] = true;
                }
            }
        }

        // Dilatar los bordes de cada isla para que no se vean costuras
        var tmp = new Color[N * N];
        for (int pasada = 0; pasada < 8; pasada++)
        {
            System.Array.Copy(col, tmp, col.Length);
            var nuevos = new List<int>();
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    int k = y * N + x;
                    if (lleno[k]) { continue; }
                    Color s = Color.clear; int c = 0;
                    if (x > 0 && lleno[k - 1]) { s += tmp[k - 1]; c++; }
                    if (x < N - 1 && lleno[k + 1]) { s += tmp[k + 1]; c++; }
                    if (y > 0 && lleno[k - N]) { s += tmp[k - N]; c++; }
                    if (y < N - 1 && lleno[k + N]) { s += tmp[k + N]; c++; }
                    if (c > 0) { col[k] = s / c; nuevos.Add(k); }
                }
            }
            foreach (int k in nuevos) { lleno[k] = true; }
        }
        Color fondo = a.torso * 0.6f; fondo.a = 1f;
        for (int k = 0; k < col.Length; k++) { if (!lleno[k]) { col[k] = fondo; } }

        var tex = new Texture2D(N, N, TextureFormat.RGB24, false);
        tex.SetPixels(col);
        tex.Apply();
        string ruta = CarpetaTex + "/t_cuerpo_" + a.id + ".jpg";
        File.WriteAllBytes(ruta, tex.EncodeToJPG(92));
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
        var ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.mipmapEnabled = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.maxTextureSize = N;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
    }

    private static Material CrearMaterial(Atuendo a, Texture2D tex)
    {
        string ruta = CarpetaMat + "/mat_cuerpo_" + a.id + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetTexture("_BaseMap", tex);
        m.mainTexture = tex;
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Smoothness", a.suavidad);
        m.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(m);
        return m;
    }

    private static void Carpeta(string padre, string nombre)
    {
        if (!AssetDatabase.IsValidFolder(padre + "/" + nombre)) { AssetDatabase.CreateFolder(padre, nombre); }
    }

    // ------------------------------------------------------------------ Pintura

    private static Color Pintar(Atuendo a, InfoMalla im, Parte parte, Vector3 p)
    {
        Vector3 q = new Vector3(p.x - im.centro.x, p.y - im.minY, p.z - im.centro.z) / im.altura;
        float u = Vector3.Dot(q, im.derecha), f = Vector3.Dot(q, im.adelante), h = q.y;
        Color c;
        bool tela = true;

        switch (parte)
        {
            case Parte.Cabeza:
                c = Cabeza(a, im, p, q, out tela);
                break;
            case Parte.Cuello:
            {
                // El hueso del cuello también mueve la mandíbula: la parte alta es cara
                float hc = Mathf.InverseLerp(im.cuelloMinY, im.cuelloMaxY, p.y);
                if (a.pasamontanas) { c = Lana(q); }
                else if (a.patron == Patron.Capucha && hc < 0.45f) { c = a.torso * 0.72f; }
                else if (a.patron == Patron.Aguayo && hc < 0.35f) { c = Aguayo(u, h); }
                else
                {
                    c = Piel(a, q); tela = false;
                    if (a.infeccion > 0f && a.sangre > 0f && hc > 0.45f && f > 0f)
                    {
                        float g = Ruido(new Vector3(q.x * 70f, q.y * 12f, q.z * 70f) + new Vector3(1f, 2f, 3f), 2);
                        c = Color.Lerp(c, C(0.38f, 0.04f, 0.04f), Mathf.Clamp01((g - (0.5f - 0.08f * a.sangre)) * 8f) * 0.85f);
                    }
                }
                break;
            }
            case Parte.Torso:
                c = Torso(a, u, f, h, q);
                break;
            case Parte.BrazoSup:
                c = Manga(a, u, f, h, q);
                break;
            case Parte.Antebrazo:
                if (a.mangaCorta) { c = Piel(a, q); tela = false; }
                else { c = Manga(a, u, f, h, q); }
                break;
            case Parte.Mano:
                c = Piel(a, q); tela = false;
                break;
            case Parte.Cadera:
                if (a.falda) { c = Pollera(a, im, u, f, h); }
                else if (!(f > 0f && Mathf.Abs(u) < 0.045f)) { c = Torso(a, u, f, h, q); }   // la chamarra cubre la cadera; solo se ve el pantalón por delante
                else { c = Pantalon(a, im, u, f, h, q, true); }
                break;
            case Parte.Muslo:
                c = a.falda ? Pollera(a, im, u, f, h) : Pantalon(a, im, u, f, h, q, false);
                break;
            case Parte.Pierna:
                if (a.falda) { c = h > 0.17f ? Pollera(a, im, u, f, h) : a.medias * (0.92f + 0.12f * Ruido(q * 90f, 2)); }
                else if (a.botas && h < 0.13f) { c = Zapato(a, h, q); }
                else { c = Pantalon(a, im, u, f, h, q, false); }
                break;
            default:
                c = Zapato(a, h, q);
                break;
        }

        if (tela)
        {
            c *= 0.9f + 0.2f * Ruido(q * 140f, 2);
            if (a.infeccion > 0f) { float g = c.grayscale; c = Color.Lerp(c, C(g, g, g), 0.18f * a.infeccion); }
            if (a.rotura > 0f && parte != Parte.Pie && parte != Parte.Cabeza)
            {
                float n = Ruido(q * 21f + new Vector3(5.1f, 1.3f, 7.7f));
                float umbral = 0.73f - 0.17f * a.rotura * (parte == Parte.Muslo || parte == Parte.Pierna ? 0.45f : 1f);
                if (n > umbral + 0.018f) { c = Piel(a, q); }
                else if (n > umbral) { c *= 0.35f; }
            }
        }

        if (a.hongos > 0f)
        {
            float s = Ruido(q * 30f + new Vector3(21f, 3f, 9f));
            float um = 0.72f - 0.08f * a.hongos;
            if (s > um + 0.04f) { c = Color.Lerp(C(0.62f, 0.48f, 0.27f), C(0.8f, 0.68f, 0.45f), Ruido(q * 110f, 2)); }
            else if (s > um) { c = Color.Lerp(c, C(0.3f, 0.2f, 0.1f), 0.7f); }
        }

        if (a.suciedad > 0f)
        {
            float d = Ruido(q * 5f + new Vector3(7f, 2f, 3f));
            float cant = Mathf.Clamp01((d - 0.42f) * 3f) * a.suciedad * (0.35f + (1f - Mathf.Clamp01(h)) * 0.8f);
            c = Color.Lerp(c, C(0.3f, 0.24f, 0.17f), Mathf.Clamp01(cant) * 0.8f);
        }

        if (a.sangre > 0f)
        {
            float w = a.sangre;
            if (parte == Parte.Torso && f > 0f) { w *= 1.35f; }
            if (parte == Parte.Mano || parte == Parte.Antebrazo) { w *= 0.8f; }
            if (parte == Parte.Pie || parte == Parte.Pierna || parte == Parte.Muslo) { w *= 0.45f; }
            float b = Ruido(q * 12f + new Vector3(13f, 5f, 1f)) * 0.75f + Ruido(q * 34f + new Vector3(3f, 8f, 2f), 2) * 0.25f;
            float chorro = Ruido(new Vector3(q.x * 55f, q.y * 4f, q.z * 55f) + new Vector3(2f, 9f, 4f), 2);
            bool enChorro = parte == Parte.Torso && f > 0f && chorro > 0.8f - 0.08f * w;
            if (b > 0.72f - 0.1f * w || enChorro)
            {
                Color sangre = Color.Lerp(C(0.45f, 0.03f, 0.03f), C(0.2f, 0.05f, 0.035f), Ruido(q * 22f, 2));
                c = Color.Lerp(c, sangre, 0.85f);
            }
        }

        c.a = 1f;
        return c;
    }

    private static Color Cabeza(Atuendo a, InfoMalla im, Vector3 p, Vector3 q, out bool tela)
    {
        tela = false;
        float hn = Mathf.InverseLerp(im.cabezaMinY, im.cabezaMaxY, p.y);
        Vector3 d = p - im.cabezaCentro; d.y = 0f;
        float r = Mathf.Max(0.0001f, im.cabezaRadio);
        float fr = Vector3.Dot(d, im.adelante) / r;
        float lat = Vector3.Dot(d, im.derecha) / r;

        if (a.pasamontanas)
        {
            bool ojos = fr > 0.35f && hn > 0.3f && hn < 0.45f;
            if (!ojos) { tela = true; return Lana(q); }
            return Piel(a, q);
        }
        if (a.sombrero && hn > 0.8f)
        {
            tela = true;
            return hn < 0.84f ? a.sombreroColor * 0.55f : a.sombreroColor * (0.9f + 0.2f * Ruido(q * 60f, 2));
        }
        float borde = (Ruido(q * 45f, 2) - 0.5f) * 0.08f;
        bool pelo = hn > 0.66f + borde || (fr < -0.3f && hn > 0.2f + borde) || (fr < 0.1f && hn > 0.52f + borde);
        if (pelo)
        {
            float hebra = Ruido(new Vector3(q.x * 300f, q.y * 25f, q.z * 300f), 2);
            return a.pelo * (0.7f + 0.6f * hebra) + C(0.02f, 0.02f, 0.02f);
        }
        Color c = Piel(a, q);
        if (a.infeccion > 0f && fr > 0.3f)
        {
            // Ojeras hundidas (máscara suave, sin bordes duros)
            float me = 1f - Mathf.Pow((hn - 0.37f) / 0.07f, 2f) - Mathf.Pow((Mathf.Abs(lat) - 0.28f) / 0.17f, 2f);
            if (me > 0f) { c = Color.Lerp(c, C(0.2f, 0.15f, 0.15f), Mathf.Clamp01(me * 1.5f) * 0.45f * a.infeccion); }
            // Sangre alrededor de la boca
            if (a.sangre > 0f && hn < 0.4f && Mathf.Abs(lat) < 0.45f)
            {
                float g = Ruido(new Vector3(q.x * 70f, q.y * 12f, q.z * 70f), 2);
                float m = Mathf.Clamp01((g - (0.52f - 0.08f * a.sangre)) * 8f) * Mathf.Clamp01((0.4f - hn) * 6f);
                c = Color.Lerp(c, C(0.38f, 0.04f, 0.04f), m * 0.85f);
            }
        }
        return c;
    }

    private static Color Piel(Atuendo a, Vector3 q)
    {
        Color c = a.piel * (0.93f + 0.14f * Ruido(q * 45f, 2));
        if (a.infeccion > 0f)
        {
            c = Color.Lerp(c, C(0.55f, 0.58f, 0.49f) * (0.9f + 0.2f * Ruido(q * 30f, 2)), 0.75f * a.infeccion);
            float vena = Mathf.Abs(Ruido(q * 26f + new Vector3(4f, 4f, 4f)) - 0.5f);
            if (vena < 0.014f) { c = Color.Lerp(c, C(0.3f, 0.2f, 0.3f), 0.7f * a.infeccion); }
            float moreton = Ruido(q * 7f + new Vector3(9f, 1f, 6f));
            if (moreton > 0.64f) { c = Color.Lerp(c, C(0.35f, 0.28f, 0.3f), Mathf.Clamp01((moreton - 0.64f) * 8f) * 0.6f * a.infeccion); }
        }
        return c;
    }

    private static Color Lana(Vector3 q)
    {
        return C(0.1f, 0.1f, 0.12f) * (0.8f + 0.4f * Ruido(q * 200f, 1));
    }

    private static bool Banda(float h, float centro) { return Mathf.Abs(h - centro) < 0.014f; }

    private static Color PatronBase(Atuendo a, float u, float f, float h, Color baseC)
    {
        switch (a.patron)
        {
            case Patron.Rayas:
                return Mathf.Repeat(h * 30f, 1f) < 0.5f ? baseC : a.torso2;
            case Patron.Cuadros:
            {
                float su = Mathf.Repeat((u + f) * 16f, 1f), sh = Mathf.Repeat(h * 16f, 1f);
                bool bu = su < 0.34f, bh = sh < 0.34f;
                Color c = baseC;
                if (bu && bh) { c = a.torso2; }
                else if (bu || bh) { c = Color.Lerp(baseC, a.torso2, 0.5f); }
                if (Mathf.Abs(su - 0.67f) < 0.02f || Mathf.Abs(sh - 0.67f) < 0.02f) { c = Color.Lerp(c, C(0.85f, 0.8f, 0.6f), 0.5f); }
                return c;
            }
            default:
                return baseC;
        }
    }

    private static Color Torso(Atuendo a, float u, float f, float h, Vector3 q)
    {
        Color c = PatronBase(a, u, f, h, a.torso);
        switch (a.patron)
        {
            case Patron.Reflectivo:
                if (Banda(h, 0.66f) || Banda(h, 0.575f)) { c = a.torso2 * (0.95f + 0.1f * Ruido(q * 80f, 2)); }
                break;
            case Patron.Chaleco:
                c = C(0.1f, 0.115f, 0.1f) * (0.9f + 0.2f * Ruido(q * 60f, 2));
                if (Mathf.Abs(Mathf.Repeat(h * 22f, 1f) - 0.5f) < 0.04f) { c *= 0.6f; }
                if (f < 0f && Banda(h, 0.68f)) { c = C(0.78f, 0.76f, 0.3f); }
                break;
            case Patron.Capucha:
                if (f > 0f && Mathf.Abs(u) < 0.006f) { c = C(0.6f, 0.6f, 0.62f); }
                if (f > 0f && Mathf.Abs(u) < 0.075f && h > 0.5f && h < 0.575f)
                {
                    c *= 0.85f;
                    if (h > 0.565f || Mathf.Abs(Mathf.Abs(u) - 0.072f) < 0.003f) { c *= 0.7f; }
                }
                if (f < 0f && h > 0.73f) { c *= 0.75f; }
                if (f > 0f && h > 0.72f && h < 0.78f && Mathf.Abs(Mathf.Abs(u) - 0.024f) < 0.003f) { c = C(0.85f, 0.85f, 0.82f); }
                break;
            case Patron.Aguayo:
                c = Aguayo(u, h);
                break;
            case Patron.Mandil:
                if (f > 0f && Mathf.Abs(u) < 0.008f && Mathf.Repeat(h * 18f, 1f) < 0.15f) { c = C(0.25f, 0.25f, 0.27f); }
                break;
        }
        return c;
    }

    private static Color Manga(Atuendo a, float u, float f, float h, Vector3 q)
    {
        if (a.manga.r >= 0f) { return a.manga * (0.85f + 0.3f * Ruido(q * 70f, 2)); }
        return PatronBase(a, u, f, h, a.torso);
    }

    private static Color Pantalon(Atuendo a, InfoMalla im, float u, float f, float h, Vector3 q, bool cadera)
    {
        Color c = a.pantalon;
        if (a.jean)
        {
            float sarga = Mathf.Repeat((u + f + h) * 260f, 1f) < 0.5f ? 0.93f : 1.05f;
            c *= sarga * (0.9f + 0.25f * Ruido(q * 18f, 2));
            if (h > 0.24f && h < 0.32f) { c = Color.Lerp(c, c * 1.3f, 0.35f); }
        }
        if (cadera && Mathf.Abs(h - im.cinturaY) < 0.013f)
        {
            c = C(0.1f, 0.07f, 0.05f);
            if (f > 0f && Mathf.Abs(u) < 0.014f) { c = C(0.55f, 0.55f, 0.52f); }
        }
        return c;
    }

    private static Color Pollera(Atuendo a, InfoMalla im, float u, float f, float h)
    {
        float ang = Mathf.Atan2(f, u);
        Color c = a.pantalon * (0.82f + 0.18f * (0.5f + 0.5f * Mathf.Sin(ang * 64f)));
        if (h > 0.18f && h < 0.21f) { c = Mathf.Repeat(h * 180f, 1f) < 0.5f ? C(0.72f, 0.56f, 0.2f) : C(0.2f, 0.38f, 0.24f); }
        if (Mathf.Abs(h - im.cinturaY) < 0.012f) { c *= 0.6f; }
        return c;
    }

    private static Color Zapato(Atuendo a, float h, Vector3 q)
    {
        Color c = a.zapato * (0.9f + 0.2f * Ruido(q * 60f, 2));
        if (h < 0.012f) { c = a.suela.r >= 0f ? a.suela : a.zapato * 0.6f; }
        return c;
    }

    private static readonly Color[] ColoresAguayo =
    {
        C(0.62f, 0.3f, 0.15f), C(0.86f, 0.76f, 0.55f), C(0.36f, 0.2f, 0.12f),
        C(0.82f, 0.46f, 0.18f), C(0.47f, 0.12f, 0.1f), C(0.86f, 0.76f, 0.55f)
    };

    private static Color Aguayo(float u, float h)
    {
        int banda = Mathf.FloorToInt(h * 38f);
        Color c = ColoresAguayo[((banda % 6) + 6) % 6];
        if (((banda % 3) + 3) % 3 == 1)
        {
            float du = Mathf.Abs(Mathf.Repeat(u * 70f, 1f) - 0.5f), dh = Mathf.Abs(Mathf.Repeat(h * 38f, 1f) - 0.5f);
            if (du + dh < 0.32f) { c = ColoresAguayo[(((banda + 2) % 6) + 6) % 6]; }
        }
        return c;
    }

    // ------------------------------------------------------------------ Ruido 3D

    private static float Hash(int x, int y, int z)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + z * 1274126177;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xffffff) / 16777215f;
        }
    }

    private static float Valor(Vector3 p)
    {
        int x = Mathf.FloorToInt(p.x), y = Mathf.FloorToInt(p.y), z = Mathf.FloorToInt(p.z);
        float fx = p.x - x, fy = p.y - y, fz = p.z - z;
        fx = fx * fx * (3f - 2f * fx); fy = fy * fy * (3f - 2f * fy); fz = fz * fz * (3f - 2f * fz);
        float a = Mathf.Lerp(Hash(x, y, z), Hash(x + 1, y, z), fx);
        float b = Mathf.Lerp(Hash(x, y + 1, z), Hash(x + 1, y + 1, z), fx);
        float c = Mathf.Lerp(Hash(x, y, z + 1), Hash(x + 1, y, z + 1), fx);
        float d = Mathf.Lerp(Hash(x, y + 1, z + 1), Hash(x + 1, y + 1, z + 1), fx);
        return Mathf.Lerp(Mathf.Lerp(a, b, fy), Mathf.Lerp(c, d, fy), fz);
    }

    private static float Ruido(Vector3 p, int octavas = 3)
    {
        float s = 0f, amp = 0.5f, total = 0f;
        for (int i = 0; i < octavas; i++)
        {
            s += Valor(p) * amp;
            total += amp;
            p = p * 2.03f + new Vector3(17.1f, 3.7f, 9.3f);
            amp *= 0.5f;
        }
        return s / total;
    }
}
