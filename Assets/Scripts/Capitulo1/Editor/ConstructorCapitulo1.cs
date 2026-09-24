using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Construye la escena completa del Capitulo 1 "Resaca" por codigo: geometria, luces,
/// personajes, infectados, objetos, puertas, eventos, audio, post-proceso y NavMesh.
/// Menu: Hoyada Z / Construir Capitulo 1. Se puede volver a ejecutar para regenerar.
/// </summary>
public static partial class ConstructorCapitulo1
{
    public const string RutaEscena = "Assets/Scenes/Capitulo1_Resaca.unity";
    private const string CarpetaMat = "Assets/Arte/Materiales/Nivel/";
    public const int CapaJugador = 8, CapaInfectado = 9, CapaPuertas = 10;

    private static Transform padre;
    private static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Mesh> mallas = new Dictionary<string, Mesh>();
    private static System.Random azar;

    private static Jugador jugador;
    private static Juego juego;

    public struct Hueco
    {
        public float centro, ancho, abajo, arriba;
        public Hueco(float c, float a, float ab, float ar) { centro = c; ancho = a; abajo = ab; arriba = ar; }
        public static Hueco Puerta(float c, float ancho = 1.4f, float alto = 2.4f) { return new Hueco(c, ancho, 0f, alto); }
        public static Hueco Ventana(float c, float ancho = 1.4f) { return new Hueco(c, ancho, 1.0f, 2.2f); }
    }

    [MenuItem("Hoyada Z/Construir Capitulo 1")]
    public static void ConstruirMenu()
    {
        Debug.Log(Construir());
    }

    public static string Construir()
    {
        var log = new System.Text.StringBuilder();
        azar = new System.Random(2026);
        mats.Clear();
        mallas.Clear();

        ConfigurarCapas();
        ConfigurarAgente();

        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var raizNivel = new GameObject("_Nivel").transform;
        var raizPersonajes = new GameObject("_Personajes").transform;
        var raizEventos = new GameObject("_Eventos").transform;
        var raizLuces = new GameObject("_Luces").transform;

        // Sistemas
        CrearSistemas(log);

        // Zonas
        padre = raizNivel;
        Discoteca(raizNivel, raizPersonajes, raizEventos, raizLuces);
        Callejon(raizNivel, raizPersonajes, raizEventos, raizLuces);
        AvenidaYPlaza(raizNivel, raizPersonajes, raizEventos, raizLuces);
        Farmacia(raizNivel, raizPersonajes, raizEventos, raizLuces);
        Mercado(raizNivel, raizPersonajes, raizEventos, raizLuces);
        GradasYCalleSuperior(raizNivel, raizPersonajes, raizEventos, raizLuces);
        Extras(raizNivel, raizPersonajes, raizEventos, raizLuces);
        Supervivencia(raizNivel, raizPersonajes, raizEventos);

        // NavMesh
        var nav = new GameObject("NavMesh");
        var surf = nav.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.All;
        surf.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surf.layerMask = ~((1 << 2) | (1 << CapaJugador) | (1 << CapaInfectado) | (1 << CapaPuertas));
        surf.BuildNavMesh();
        Directory.CreateDirectory("Assets/Scenes/Capitulo1_Resaca");
        string rutaNav = "Assets/Scenes/Capitulo1_Resaca/NavMesh.asset";
        AssetDatabase.DeleteAsset(rutaNav);
        if (surf.navMeshData != null)
        {
            AssetDatabase.CreateAsset(surf.navMeshData, rutaNav);
            log.AppendLine("NavMesh horneado.");
        }

        // Posicionar infectados sobre el NavMesh
        int fuera = 0;
        foreach (var inf in Object.FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(inf.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                inf.transform.position = hit.position;
            }
            else
            {
                fuera++;
                log.AppendLine("Infectado fuera del NavMesh: " + inf.name + " " + inf.transform.position);
            }
        }

        foreach (var ag in Object.FindObjectsByType<Companera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            NavMeshHit hitW;
            if (NavMesh.SamplePosition(ag.transform.position, out hitW, 2f, NavMesh.AllAreas)) { ag.transform.position = hitW.position; }
        }

        EditorSceneManager.SaveScene(escena, RutaEscena);
        var escenas = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(RutaEscena, true) };
        foreach (var e in EditorBuildSettings.scenes)
        {
            if (e.path != RutaEscena) { escenas.Add(new EditorBuildSettingsScene(e.path, false)); }
        }
        EditorBuildSettings.scenes = escenas.ToArray();

        int nInf = Object.FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int nDoc = Object.FindObjectsByType<Documento>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int nRec = Object.FindObjectsByType<Recogible>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        log.AppendLine("Escena guardada: " + RutaEscena + " | infectados=" + nInf + " (fuera de navmesh=" + fuera + ") documentos=" + nDoc + " objetos=" + nRec);
        return log.ToString();
    }

    // ---------- Configuracion del proyecto ----------

    private static void ConfigurarCapas()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var capas = tm.FindProperty("layers");
        capas.GetArrayElementAtIndex(CapaJugador).stringValue = "Jugador";
        capas.GetArrayElementAtIndex(CapaInfectado).stringValue = "Infectado";
        capas.GetArrayElementAtIndex(CapaPuertas).stringValue = "Puertas";
        tm.ApplyModifiedProperties();
    }

    private static void ConfigurarAgente()
    {
        var so = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
        var ajustes = so.FindProperty("m_Settings");
        if (ajustes != null && ajustes.arraySize > 0)
        {
            var a = ajustes.GetArrayElementAtIndex(0);
            a.FindPropertyRelative("agentRadius").floatValue = 0.35f;
            a.FindPropertyRelative("agentHeight").floatValue = 1.8f;
            a.FindPropertyRelative("agentClimb").floatValue = 0.45f;
            a.FindPropertyRelative("agentSlope").floatValue = 46f;
            so.ApplyModifiedProperties();
        }
    }

    // ---------- Sistemas: juego, jugador, camara, luces globales ----------

    private static void CrearSistemas(System.Text.StringBuilder log)
    {
        // Ambiente: sin cielo brillante que se refleje en superficies lisas
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.26f, 0.27f, 0.34f);
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture = null;
        RenderSettings.reflectionIntensity = 0.25f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.028f;

        // Sol / luna
        var solGo = new GameObject("Sol");
        var sol = solGo.AddComponent<Light>();
        sol.type = LightType.Directional;
        sol.shadows = LightShadows.Soft;
        sol.shadowStrength = 0.85f;
        solGo.transform.rotation = Quaternion.Euler(10f, -60f, 0f);

        // Post-proceso
        var vol = new GameObject("PostProceso").AddComponent<Volume>();
        vol.isGlobal = true;
        vol.sharedProfile = PerfilPostProceso();

        // Juego + HUD + Audio
        var jg = new GameObject("Juego");
        juego = jg.AddComponent<Juego>();
        juego.sol = sol;
        juego.ambienteInicio = new Color(0.26f, 0.27f, 0.34f);
        juego.ambienteFinal = new Color(0.5f, 0.48f, 0.5f);
        juego.intensidadInicio = 0.35f;
        juego.intensidadFinal = 1.4f;
        juego.densidadInicio = 0.028f;
        juego.densidadFinal = 0.012f;
        juego.nieblaInicio = new Color(0.07f, 0.08f, 0.11f);
        juego.nieblaFinal = new Color(0.46f, 0.44f, 0.47f);
        GuionIntro(juego);
        var hud = jg.AddComponent<HUDCap1>();
        AsignarHUD(hud);
        var au = jg.AddComponent<AudioCap1>();
        var clips = new List<AudioClip>();
        foreach (string g in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Arte/Audio" }))
        {
            clips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g)));
        }
        au.clips = clips.ToArray();
        log.AppendLine("Clips de audio: " + clips.Count);

        // Jugador
        jugador = CrearJugador(new Vector3(-10.6f, 0.05f, 2.2f), 220f);
        juego.jugador = jugador;

        // Camara
        var camGo = new GameObject("Camara");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.08f;
        cam.farClipPlane = 220f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 58f;
        camGo.AddComponent<AudioListener>();
        var ucd = camGo.AddComponent<UniversalAdditionalCameraData>();
        ucd.renderPostProcessing = true;
        ucd.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        var tps = camGo.AddComponent<CamaraTPS>();
        tps.objetivo = jugador.transform;
        camGo.transform.position = jugador.transform.position + new Vector3(1f, 1.8f, 3f);
        juego.camara = tps;
    }

    private static VolumeProfile PerfilPostProceso()
    {
        Directory.CreateDirectory("Assets/Arte/Ajustes");
        string ruta = "Assets/Arte/Ajustes/PerfilCapitulo1.asset";
        AssetDatabase.DeleteAsset(ruta);
        var p = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(p, ruta);

        var tone = p.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.Neutral);
        var bloom = p.Add<Bloom>(true);
        bloom.intensity.Override(0.9f);
        bloom.threshold.Override(1.0f);
        bloom.scatter.Override(0.65f);
        var vig = p.Add<Vignette>(true);
        vig.intensity.Override(0.38f);
        vig.smoothness.Override(0.45f);
        var col = p.Add<ColorAdjustments>(true);
        col.saturation.Override(-22f);
        col.contrast.Override(18f);
        col.postExposure.Override(1.1f);
        col.colorFilter.Override(new Color(0.93f, 0.97f, 1f));
        var grano = p.Add<FilmGrain>(true);
        grano.type.Override(FilmGrainLookup.Medium3);
        grano.intensity.Override(0.35f);
        var ca = p.Add<ChromaticAberration>(true);
        ca.intensity.Override(0.12f);
        var lgg = p.Add<LiftGammaGain>(true);
        lgg.lift.Override(new Vector4(0.98f, 1f, 1.03f, 0.02f));

        foreach (var c in p.components) { AssetDatabase.AddObjectToAsset(c, p); }
        EditorUtility.SetDirty(p);
        AssetDatabase.SaveAssets();
        return p;
    }

    private static void AsignarHUD(HUDCap1 h)
    {
        System.Func<string, Texture2D> T = r => AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Arte/UI/" + r);
        h.iconoMoral = T("Iconos/ui_icono_moral.jpg");
        h.iconoAgua = T("Iconos/ui_icono_agua.jpg");
        h.iconoMedicina = T("Iconos/ui_icono_medicina.jpg");
        h.iconoBateria = T("Iconos/ui_icono_bateria.jpg");
        h.iconoTiempo = T("Iconos/ui_icono_tiempo.jpg");
        h.mateoNeutro = T("Retratos/ui_retrato_mateo_neutro.jpg");
        h.mateoAlerta = T("Retratos/ui_retrato_mateo_alerta.jpg");
        h.mateoTension = T("Retratos/ui_retrato_mateo_tension.jpg");
        h.betyNeutro = T("Retratos/ui_retrato_bety_neutro.jpg");
        h.betyAlerta = T("Retratos/ui_retrato_bety_alerta.jpg");
        h.betyTension = T("Retratos/ui_retrato_bety_tension.jpg");
        h.infectado = T("Retratos/ui_retrato_infectado.jpg");
        h.fondoMenu = T("Menu/ui_fondo_menu.jpg");
        h.logo = T("Menu/ui_logo.png");
    }

    private static Jugador CrearJugador(Vector3 pos, float rotY)
    {
        var go = new GameObject("Mateo");
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        var cc = go.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.33f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.stepOffset = 0.4f;
        cc.slopeLimit = 50f;
        cc.skinWidth = 0.04f;
        var j = go.AddComponent<Jugador>();
        go.AddComponent<Inventario>();
        go.AddComponent<CombateJugador>();
        var lin = go.AddComponent<Linterna>();

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/Rig_Mateo.prefab"));
        rig.transform.SetParent(go.transform, false);
        var anim = rig.GetComponent<AnimProcedural>();
        anim.velocidadCaminar = 2.6f;
        anim.velocidadCorrer = 5.8f;
        j.anim = anim;

        var esq = rig.GetComponent<EsqueletoHumanoide>();
        var inv = go.GetComponent<Inventario>();
        if (esq != null && esq.puntoArma != null)
        {
            inv.visualPalo = Arma("Arma_Palo", esq.puntoArma, new Vector3(0.045f, 0.045f, 0.95f), "mat_mateo_arma");
            inv.visualFierro = Arma("Arma_Fierro", esq.puntoArma, new Vector3(0.028f, 0.028f, 1.05f), "mat_fierro");
            inv.visualMachete = Arma("Arma_Machete", esq.puntoArma, new Vector3(0.018f, 0.08f, 0.66f), "mat_fierro");
            inv.visualHonda = Arma("Arma_Honda", esq.puntoArma, new Vector3(0.03f, 0.03f, 0.32f), "mat_honda");
            inv.visualRevolver = Arma("Arma_Revolver", esq.puntoArma, new Vector3(0.04f, 0.1f, 0.24f), "mat_revolver");
        }

        var luzGo = new GameObject("LuzLinterna");
        luzGo.transform.SetParent(go.transform, false);
        luzGo.transform.localPosition = new Vector3(0.22f, 1.45f, 0.45f);
        lin.intensidad = 45f;
        lin.offsetPecho = new Vector3(0.22f, 1.45f, 0.45f);
        var luz = luzGo.AddComponent<Light>();
        luz.type = LightType.Spot;
        luz.range = 24f;
        luz.spotAngle = 58f;
        luz.innerSpotAngle = 26f;
        luz.intensity = 45f;
        luz.shadowNearPlane = 0.7f;
        luz.color = new Color(1f, 0.95f, 0.85f);
        luz.shadows = LightShadows.Soft;
        luz.enabled = false;
        lin.luz = luz;

        CambiarCapa(go, CapaJugador);
        return j;
    }

    private static GameObject Arma(string n, Transform punto, Vector3 tam, string mat)
    {
        var go = new GameObject(n);
        go.transform.SetParent(punto, false);
        var malla = new GameObject("Malla");
        malla.transform.SetParent(go.transform, false);
        malla.transform.localPosition = new Vector3(0f, 0f, tam.z * 0.38f);
        malla.AddComponent<MeshFilter>().sharedMesh = MallaCaja(tam);
        malla.AddComponent<MeshRenderer>().sharedMaterial = Mat(mat);
        go.SetActive(false);
        return go;
    }

    private static void CambiarCapa(GameObject go, int capa)
    {
        go.layer = capa;
        foreach (Transform t in go.transform) { CambiarCapa(t.gameObject, capa); }
    }

    // ---------- Utilidades de geometria ----------

    public static Material Mat(string n)
    {
        Material m;
        if (mats.TryGetValue(n, out m)) { return m; }
        m = AssetDatabase.LoadAssetAtPath<Material>(CarpetaMat + n + ".mat");
        if (m == null) { m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Arte/Materiales/" + n + ".mat"); }
        if (m == null) { Debug.LogWarning("Material no encontrado: " + n); }
        mats[n] = m;
        return m;
    }

    /// <summary>Malla de caja con UV en metros (la textura no se estira con el tamano).</summary>
    public static Mesh MallaCaja(Vector3 s)
    {
        string clave = s.x.ToString("F3") + "_" + s.y.ToString("F3") + "_" + s.z.ToString("F3");
        Mesh m;
        if (mallas.TryGetValue(clave, out m)) { return m; }

        Vector3 h = s * 0.5f;
        var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
        System.Action<Vector3, Vector3, Vector3, float, float> cara = (normal, ejeU, ejeV, largoU, largoV) =>
        {
            int i0 = v.Count;
            Vector3 c = Vector3.Scale(normal, h);
            Vector3 du = ejeU * largoU * 0.5f, dv = ejeV * largoV * 0.5f;
            v.Add(c - du - dv); v.Add(c + du - dv); v.Add(c + du + dv); v.Add(c - du + dv);
            for (int k = 0; k < 4; k++) { n.Add(normal); }
            uv.Add(new Vector2(0f, 0f)); uv.Add(new Vector2(largoU, 0f)); uv.Add(new Vector2(largoU, largoV)); uv.Add(new Vector2(0f, largoV));
            tri.Add(i0); tri.Add(i0 + 2); tri.Add(i0 + 1); tri.Add(i0); tri.Add(i0 + 3); tri.Add(i0 + 2);
        };
        cara(Vector3.forward, Vector3.left, Vector3.up, s.x, s.y);
        cara(Vector3.back, Vector3.right, Vector3.up, s.x, s.y);
        cara(Vector3.right, Vector3.forward, Vector3.up, s.z, s.y);
        cara(Vector3.left, Vector3.back, Vector3.up, s.z, s.y);
        cara(Vector3.up, Vector3.right, Vector3.forward, s.x, s.z);
        cara(Vector3.down, Vector3.right, Vector3.back, s.x, s.z);

        m = new Mesh { name = "Caja_" + clave };
        m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
        m.RecalculateBounds();
        m.RecalculateTangents();
        mallas[clave] = m;
        return m;
    }

    public static GameObject Caja(string nombre, Vector3 centro, Vector3 tam, string mat, bool colision = true, float rotY = 0f, Transform p = null)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(p != null ? p : padre, false);
        go.transform.SetPositionAndRotation(centro, Quaternion.Euler(0f, rotY, 0f));
        go.AddComponent<MeshFilter>().sharedMesh = MallaCaja(tam);
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Mat(mat);
        if (colision)
        {
            var bc = go.AddComponent<BoxCollider>();
            bc.size = tam;
        }
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        return go;
    }

    /// <summary>Caja definida por sus esquinas (min / max).</summary>
    public static GameObject Bloque(string nombre, Vector3 min, Vector3 max, string mat, bool colision = true, Transform p = null)
    {
        return Caja(nombre, (min + max) * 0.5f, max - min, mat, colision, 0f, p);
    }

    /// <summary>Pared recta de a a b (plano XZ) con huecos para puertas y ventanas.</summary>
    public static void Pared(string nombre, Vector3 a, Vector3 b, float alto, float grosor, string mat, params Hueco[] huecos)
    {
        Vector3 dir = b - a; dir.y = 0f;
        float largo = dir.magnitude;
        dir /= largo;
        float rot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        var lista = new List<Hueco>(huecos);
        lista.Sort((x, y) => x.centro.CompareTo(y.centro));

        float cursor = 0f;
        int k = 0;
        System.Action<float, float, float, float> tramo = (d0, d1, y0, y1) =>
        {
            if (d1 - d0 < 0.01f || y1 - y0 < 0.01f) { return; }
            Vector3 c = a + dir * ((d0 + d1) * 0.5f);
            c.y = a.y + (y0 + y1) * 0.5f;
            Caja(nombre + "_" + (k++), c, new Vector3(grosor, y1 - y0, d1 - d0), mat, true, rot);
        };
        foreach (Hueco h in lista)
        {
            float h0 = Mathf.Clamp(h.centro - h.ancho * 0.5f, 0f, largo);
            float h1 = Mathf.Clamp(h.centro + h.ancho * 0.5f, 0f, largo);
            tramo(cursor, h0, 0f, alto);
            tramo(h0, h1, 0f, h.abajo);
            tramo(h0, h1, h.arriba, alto);
            cursor = h1;
        }
        tramo(cursor, largo, 0f, alto);
    }

    /// <summary>Cuarto cerrado: piso, techo y 4 paredes con huecos (coordenada del mundo a lo largo de cada pared).</summary>
    public static void Cuarto(string n, float x0, float x1, float z0, float z1, float y0, float alto, string matPared, string matPiso, string matTecho,
        Hueco[] norte, Hueco[] sur, Hueco[] este, Hueco[] oeste, bool techo = true, float grosor = 0.3f)
    {
        Bloque(n + "_Piso", new Vector3(x0, y0 - 0.2f, z0), new Vector3(x1, y0, z1), matPiso);
        if (techo) { Bloque(n + "_Techo", new Vector3(x0 - grosor, y0 + alto, z0 - grosor), new Vector3(x1 + grosor, y0 + alto + 0.3f, z1 + grosor), matTecho); }
        if (norte != null) { Pared(n + "_ParedN", new Vector3(x0 - grosor * 0.5f, y0, z1), new Vector3(x1 + grosor * 0.5f, y0, z1), alto, grosor, matPared, Relativos(norte, x0 - grosor * 0.5f)); }
        if (sur != null) { Pared(n + "_ParedS", new Vector3(x0 - grosor * 0.5f, y0, z0), new Vector3(x1 + grosor * 0.5f, y0, z0), alto, grosor, matPared, Relativos(sur, x0 - grosor * 0.5f)); }
        if (este != null) { Pared(n + "_ParedE", new Vector3(x1, y0, z0), new Vector3(x1, y0, z1), alto, grosor, matPared, Relativos(este, z0)); }
        if (oeste != null) { Pared(n + "_ParedO", new Vector3(x0, y0, z0), new Vector3(x0, y0, z1), alto, grosor, matPared, Relativos(oeste, z0)); }
    }

    private static Hueco[] Relativos(Hueco[] hs, float origen)
    {
        var r = new Hueco[hs.Length];
        for (int i = 0; i < hs.Length; i++) { r[i] = hs[i]; r[i].centro = hs[i].centro - origen; }
        return r;
    }

    public static readonly Hueco[] SinHuecos = new Hueco[0];

    /// <summary>Edificio exterior macizo con ventanas, cortinas metalicas y detalles de azotea (autoconstruccion paceña).</summary>
    public static void Edificio(string n, Vector3 min, Vector3 max, string mat, bool fN, bool fS, bool fE, bool fW, bool tiendas = true, bool azotea = true)
    {
        Bloque(n, min, max, mat);
        float alto = max.y - min.y;
        int pisos = Mathf.Max(1, Mathf.FloorToInt(alto / 3f));
        if (fN) { Fachada(n + "_N", new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), Vector3.forward, pisos, tiendas); }
        if (fS) { Fachada(n + "_S", new Vector3(max.x, min.y, min.z), new Vector3(min.x, min.y, min.z), Vector3.back, pisos, tiendas); }
        if (fE) { Fachada(n + "_E", new Vector3(max.x, min.y, max.z), new Vector3(max.x, min.y, min.z), Vector3.right, pisos, tiendas); }
        if (fW) { Fachada(n + "_W", new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z), Vector3.left, pisos, tiendas); }
        if (azotea) { Azotea(n, min, max); }
    }

    private static void Fachada(string n, Vector3 a, Vector3 b, Vector3 normal, int pisos, bool tiendas)
    {
        Vector3 dir = (b - a); float largo = dir.magnitude; dir /= largo;
        float rot = Mathf.Atan2(normal.x, normal.z) * Mathf.Rad2Deg;
        int columnas = Mathf.Max(1, Mathf.FloorToInt(largo / 3.2f));
        float paso = largo / columnas;
        for (int p = 0; p < pisos; p++)
        {
            for (int c = 0; c < columnas; c++)
            {
                Vector3 centro = a + dir * (paso * (c + 0.5f)) + normal * 0.06f;
                if (p == 0 && tiendas)
                {
                    if (azar.NextDouble() < 0.7)
                    {
                        centro.y = a.y + 1.35f;
                        Caja(n + "_Cortina_" + c, centro, new Vector3(Mathf.Min(2.8f, paso - 0.4f), 2.7f, 0.1f), "mat_cortina", false, rot);
                    }
                    continue;
                }
                if (azar.NextDouble() < 0.15) { continue; }
                centro.y = a.y + p * 3f + 1.7f;
                double r = azar.NextDouble();
                string m = r < 0.08 ? "mat_luz_calida" : (r < 0.2 ? "mat_vidrio" : "mat_negro");
                Caja(n + "_Ventana_" + p + "_" + c, centro, new Vector3(1.2f, 1.3f, 0.08f), m, false, rot);
                Caja(n + "_Marco_" + p + "_" + c, centro - Vector3.up * 0.72f + normal * 0.04f, new Vector3(1.4f, 0.1f, 0.18f), "mat_concreto", false, rot);
                if (azar.NextDouble() < 0.12 && p > 0)
                {
                    Caja(n + "_Balcon_" + p + "_" + c, centro - Vector3.up * 0.9f + normal * 0.5f, new Vector3(2f, 0.12f, 1f), "mat_concreto", false, rot);
                    Caja(n + "_Baranda_" + p + "_" + c, centro - Vector3.up * 0.4f + normal * 0.95f, new Vector3(2f, 0.9f, 0.05f), "mat_metal_oscuro", false, rot);
                }
            }
        }
    }

    private static void Azotea(string n, Vector3 min, Vector3 max)
    {
        float y = max.y;
        int tanques = 1 + azar.Next(2);
        for (int i = 0; i < tanques; i++)
        {
            float x = Mathf.Lerp(min.x + 1.5f, max.x - 1.5f, (float)azar.NextDouble());
            float z = Mathf.Lerp(min.z + 1.5f, max.z - 1.5f, (float)azar.NextDouble());
            var t = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t.name = n + "_Tanque";
            t.transform.SetParent(padre, false);
            t.transform.position = new Vector3(x, y + 0.7f, z);
            t.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
            t.GetComponent<Renderer>().sharedMaterial = Mat(azar.NextDouble() < 0.5 ? "mat_negro" : "mat_plastico_azul");
            Object.DestroyImmediate(t.GetComponent<Collider>());
        }
        int fierros = 4 + azar.Next(8);
        for (int i = 0; i < fierros; i++)
        {
            bool bordeX = azar.NextDouble() < 0.5;
            float x = bordeX ? (azar.NextDouble() < 0.5 ? min.x + 0.2f : max.x - 0.2f) : Mathf.Lerp(min.x, max.x, (float)azar.NextDouble());
            float z = !bordeX ? (azar.NextDouble() < 0.5 ? min.z + 0.2f : max.z - 0.2f) : Mathf.Lerp(min.z, max.z, (float)azar.NextDouble());
            float h = 0.8f + (float)azar.NextDouble() * 1.4f;
            Caja(n + "_Fierro_" + i, new Vector3(x, y + h * 0.5f, z), new Vector3(0.04f, h, 0.04f), "mat_fierro", false);
        }
        if (azar.NextDouble() < 0.5)
        {
            Caja(n + "_Parapeto", new Vector3((min.x + max.x) * 0.5f, y + 0.5f, min.z + 0.1f), new Vector3(max.x - min.x, 1f, 0.2f), "mat_ladrillo", false);
        }
    }

    public static Light Luz(string n, Vector3 pos, Color c, float intensidad, float rango, Transform p, LuzParpadeante.Modo? modo = null, bool sombras = false)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = c;
        l.intensity = intensidad * 3.2f;
        l.range = rango * 1.25f;
        l.shadows = sombras ? LightShadows.Soft : LightShadows.None;
        if (modo.HasValue) { go.AddComponent<LuzParpadeante>().modo = modo.Value; }
        return l;
    }

    public static Light Foco(string n, Vector3 pos, Vector3 mirar, Color c, float intensidad, float rango, float angulo, Transform p, LuzParpadeante.Modo? modo = null, bool sombras = true)
    {
        var l = Luz(n, pos, c, intensidad, rango, p, modo, sombras);
        l.type = LightType.Spot;
        l.spotAngle = angulo;
        l.innerSpotAngle = angulo * 0.5f;
        l.transform.rotation = Quaternion.LookRotation(mirar - pos);
        return l;
    }

    public static GameObject Modelo(string archivo, Vector3 pos, float rotY, string mat, Transform p, bool colision = true, float escala = 1f)
    {
        string ruta = "Assets/Arte/Modelos/" + archivo;
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        go.transform.localScale = Vector3.one * escala;
        foreach (var r in go.GetComponentsInChildren<Renderer>()) { r.sharedMaterial = Mat(mat); }
        if (colision)
        {
            var mf = go.GetComponentInChildren<MeshFilter>();
            var bc = go.AddComponent<BoxCollider>();
            Bounds b = mf.sharedMesh.bounds;
            bc.center = b.center;
            bc.size = b.size * 0.92f;
        }
        return go;
    }

    public static GameObject Poste(Vector3 pos, float rotY, Transform p, Transform luces, bool luz)
    {
        var go = Modelo("Props/prop_poste_luz_cables.obj", pos, rotY, "mat_concreto", p, false);
        var cap = go.AddComponent<CapsuleCollider>();
        cap.radius = 0.25f; cap.height = 7f; cap.center = new Vector3(0f, 3.5f, 0f);
        if (luz)
        {
            Foco("LuzPoste", pos + Vector3.up * 6.2f + go.transform.forward * 1.2f, pos + go.transform.forward * 1.5f, new Color(1f, 0.72f, 0.42f), 14f, 16f, 95f, luces,
                azar.NextDouble() < 0.4 ? LuzParpadeante.Modo.Falla : (LuzParpadeante.Modo?)null, false);
        }
        return go;
    }

    public static void Arbol(Vector3 pos, Transform p)
    {
        Caja("Tronco", pos + Vector3.up * 1.6f, new Vector3(0.3f, 3.2f, 0.3f), "mat_tronco", true, (float)azar.NextDouble() * 90f, p);
        for (int i = 0; i < 3; i++)
        {
            var copa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            copa.name = "Copa";
            copa.transform.SetParent(p, false);
            copa.transform.position = pos + new Vector3((float)azar.NextDouble() - 0.5f, 3.4f + i * 0.5f, (float)azar.NextDouble() - 0.5f);
            copa.transform.localScale = Vector3.one * (2.6f - i * 0.5f);
            copa.GetComponent<Renderer>().sharedMaterial = Mat("mat_verde_arbol");
            Object.DestroyImmediate(copa.GetComponent<Collider>());
        }
    }

    public static void Banca(Vector3 pos, float rotY, Transform p)
    {
        var go = new GameObject("Banca");
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        Caja("Asiento", go.transform.TransformPoint(0f, 0.45f, 0f), new Vector3(1.8f, 0.08f, 0.5f), "mat_madera", true, rotY, go.transform);
        Caja("Respaldo", go.transform.TransformPoint(0f, 0.8f, -0.24f), new Vector3(1.8f, 0.5f, 0.06f), "mat_madera", false, rotY, go.transform);
        Caja("PataA", go.transform.TransformPoint(-0.8f, 0.22f, 0f), new Vector3(0.08f, 0.45f, 0.45f), "mat_metal_oscuro", false, rotY, go.transform);
        Caja("PataB", go.transform.TransformPoint(0.8f, 0.22f, 0f), new Vector3(0.08f, 0.45f, 0.45f), "mat_metal_oscuro", false, rotY, go.transform);
    }

    /// <summary>Mancha de sangre (plano muy delgado sin colision).</summary>
    public static void Sangre(Vector3 pos, float tam, Transform p)
    {
        Caja("Sangre", new Vector3(pos.x, pos.y + 0.012f, pos.z), new Vector3(tam, 0.01f, tam * (0.6f + (float)azar.NextDouble() * 0.8f)), "mat_sangre", false, (float)azar.NextDouble() * 180f, p);
    }

    public static void Basura(Vector3 pos, Transform p)
    {
        for (int i = 0; i < 3; i++)
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.name = "BolsaBasura";
            b.transform.SetParent(p, false);
            b.transform.position = pos + new Vector3((float)azar.NextDouble() * 0.8f - 0.4f, 0.25f, (float)azar.NextDouble() * 0.8f - 0.4f);
            b.transform.localScale = new Vector3(0.6f, 0.5f, 0.55f);
            b.GetComponent<Renderer>().sharedMaterial = Mat("mat_basura");
            Object.DestroyImmediate(b.GetComponent<Collider>());
        }
    }

    // ---------- Personajes ----------

    public static Infectado CrearInfectado(string n, Vector3 pos, float rotY, Infectado.Tipo tipo, Infectado.Estado estado, Transform p, float radio = 6f, string mat = null, bool activo = true)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        var agente = go.AddComponent<NavMeshAgent>();
        agente.radius = 0.32f;
        agente.height = 1.75f;
        var col = go.AddComponent<CapsuleCollider>();
        col.radius = 0.35f; col.height = 1.8f; col.center = new Vector3(0f, 0.9f, 0f);

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/Rig_Infectado.prefab"));
        rig.transform.SetParent(go.transform, false);
        string m = mat ?? (tipo == Infectado.Tipo.Corredor ? "mat_char_corredor" : (tipo == Infectado.Tipo.Fungico ? "mat_char_fungico" : "mat_char_infectado"));
        foreach (var smr in rig.GetComponentsInChildren<SkinnedMeshRenderer>()) { smr.sharedMaterial = Mat(m); }
        if (tipo == Infectado.Tipo.Fungico)
        {
            rig.transform.localScale = Vector3.one * 1.07f;
            PlacasFungicas(rig.GetComponent<EsqueletoHumanoide>());
        }

        var inf = go.AddComponent<Infectado>();
        inf.tipo = tipo;
        inf.estadoInicial = estado;
        inf.radioDeambular = radio;
        CambiarCapa(go, CapaInfectado);
        go.SetActive(activo);
        return inf;
    }

    private static void PlacasFungicas(EsqueletoHumanoide e)
    {
        if (e == null) { return; }
        var sitios = new[]
        {
            new KeyValuePair<int, Vector3>(EsqueletoHumanoide.Head, new Vector3(0.05f, 0.14f, 0.02f)),
            new KeyValuePair<int, Vector3>(EsqueletoHumanoide.Head, new Vector3(-0.07f, 0.08f, 0.08f)),
            new KeyValuePair<int, Vector3>(EsqueletoHumanoide.Head, new Vector3(0.02f, 0.06f, 0.1f)),
            new KeyValuePair<int, Vector3>(EsqueletoHumanoide.Chest, new Vector3(0.12f, 0.18f, 0.08f)),
            new KeyValuePair<int, Vector3>(EsqueletoHumanoide.UpperArmL, new Vector3(-0.08f, 0f, 0.02f)),
            new KeyValuePair<int, Vector3>(EsqueletoHumanoide.Neck, new Vector3(0.05f, 0.02f, 0.06f)),
        };
        foreach (var s in sitios)
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.name = "PlacaFungica";
            Object.DestroyImmediate(b.GetComponent<Collider>());
            b.transform.SetParent(e[s.Key], false);
            b.transform.localPosition = s.Value;
            b.transform.localScale = new Vector3(0.14f, 0.07f, 0.12f) * (0.8f + (float)azar.NextDouble() * 0.6f);
            b.transform.localRotation = Quaternion.Euler((float)azar.NextDouble() * 60f, (float)azar.NextDouble() * 360f, 0f);
            b.GetComponent<Renderer>().sharedMaterial = Mat("mat_char_fungico");
        }
    }

    public static GameObject CrearNPC(string prefab, string n, Vector3 pos, float rotY, string mat, PoseNPC.Pose pose, bool mirar, Transform p, AnimProcedural.Estilo estilo)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/" + prefab + ".prefab"));
        rig.transform.SetParent(go.transform, false);
        if (mat != null) { foreach (var smr in rig.GetComponentsInChildren<SkinnedMeshRenderer>()) { smr.sharedMaterial = Mat(mat); } }
        rig.GetComponent<AnimProcedural>().estilo = estilo;
        var pn = go.AddComponent<PoseNPC>();
        pn.pose = pose;
        pn.mirarAlJugador = mirar;
        if (pose != PoseNPC.Pose.Cadaver)
        {
            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.35f; col.height = pose == PoseNPC.Pose.Sentado ? 1.1f : 1.7f; col.center = new Vector3(0f, col.height * 0.5f, 0f);
        }
        return go;
    }

    // ---------- Interactivos ----------

    public static Recogible Objeto(string n, Vector3 pos, Recogible.TipoObjeto tipo, int cantidad, Transform p, string id = null, string nombre = null)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var r = go.AddComponent<Recogible>();
        r.tipo = tipo; r.cantidad = cantidad; r.idObjeto = id; r.nombre = nombre;
        r.destello = true; r.radio = 1.7f; r.altura = 0.3f;
        switch (tipo)
        {
            case Recogible.TipoObjeto.Venda: Visual(go, new Vector3(0.18f, 0.08f, 0.12f), "mat_baldosa", 0.04f); Visual(go, new Vector3(0.19f, 0.03f, 0.04f), "mat_toldo_rojo", 0.085f); break;
            case Recogible.TipoObjeto.Botella: Visual(go, new Vector3(0.08f, 0.28f, 0.08f), "mat_vidrio", 0.14f); Visual(go, new Vector3(0.07f, 0.26f, 0.07f), "mat_farmacia_verde", 0.13f); break;
            case Recogible.TipoObjeto.Pilas: Visual(go, new Vector3(0.05f, 0.1f, 0.05f), "mat_negro", 0.05f); Visual(go, new Vector3(0.05f, 0.03f, 0.05f), "mat_barrera", 0.1f); break;
            case Recogible.TipoObjeto.Palo: VisualRot(go, new Vector3(0.06f, 0.06f, 0.95f), "mat_mateo_arma", 0.04f, 30f); break;
            case Recogible.TipoObjeto.Fierro: VisualRot(go, new Vector3(0.035f, 0.035f, 1.05f), "mat_fierro", 0.03f, 60f); break;
            case Recogible.TipoObjeto.Linterna: VisualRot(go, new Vector3(0.06f, 0.06f, 0.24f), "mat_metal_oscuro", 0.04f, 20f); break;
            default: Visual(go, new Vector3(0.12f, 0.03f, 0.08f), "mat_barrera", 0.02f); break;
        }
        return r;
    }

    private static void Visual(GameObject go, Vector3 tam, string mat, float y)
    {
        var v = new GameObject("Visual");
        v.transform.SetParent(go.transform, false);
        v.transform.localPosition = new Vector3(0f, y, 0f);
        v.AddComponent<MeshFilter>().sharedMesh = MallaCaja(tam);
        v.AddComponent<MeshRenderer>().sharedMaterial = Mat(mat);
    }

    private static void VisualRot(GameObject go, Vector3 tam, string mat, float y, float rot)
    {
        Visual(go, tam, mat, y);
        go.transform.GetChild(go.transform.childCount - 1).localRotation = Quaternion.Euler(0f, rot, 0f);
    }

    public static Documento Doc(string n, Vector3 pos, string titulo, string texto, Transform p, bool celular = false, bool guardar = false)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var d = go.AddComponent<Documento>();
        d.titulo = titulo; d.texto = texto; d.celular = celular; d.guardarAlLeer = guardar;
        d.destello = true; d.radio = 1.8f; d.altura = 0.25f;
        if (celular) { Visual(go, new Vector3(0.08f, 0.012f, 0.16f), "mat_pantalla", 0.01f); }
        else { Visual(go, new Vector3(0.22f, 0.005f, 0.3f), "mat_baldosa", 0.005f); }
        return d;
    }

    public static Documento Cartel(string n, Vector3 pos, float rotY, string titulo, string texto, Transform p)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        var d = go.AddComponent<Documento>();
        d.titulo = titulo; d.texto = texto;
        d.destello = true; d.radio = 2f; d.altura = 0f;
        var v = new GameObject("Papel");
        v.transform.SetParent(go.transform, false);
        v.AddComponent<MeshFilter>().sharedMesh = MallaCaja(new Vector3(0.5f, 0.7f, 0.01f));
        v.AddComponent<MeshRenderer>().sharedMaterial = Mat("mat_baldosa");
        return d;
    }

    /// <summary>Puerta en un hueco: 'pos' es el centro del hueco al nivel del piso, 'rotY' la orientacion de la pared.</summary>
    public static PuertaCap Puerta(string n, Vector3 pos, float rotY, float ancho, float alto, PuertaCap.Tipo tipo, string mat, Transform p)
    {
        var raiz = new GameObject(n);
        raiz.transform.SetParent(p, false);
        raiz.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));

        var pivote = new GameObject("Hoja").transform;
        pivote.SetParent(raiz.transform, false);
        Vector3 tamHoja = tipo == PuertaCap.Tipo.Reja ? new Vector3(ancho, alto, 0.06f) : new Vector3(ancho - 0.04f, alto - 0.02f, 0.08f);
        if (tipo == PuertaCap.Tipo.Bisagra)
        {
            pivote.localPosition = new Vector3(-ancho * 0.5f, 0f, 0f);
        }
        var hoja = new GameObject("Malla");
        hoja.transform.SetParent(pivote, false);
        hoja.transform.localPosition = tipo == PuertaCap.Tipo.Bisagra ? new Vector3(ancho * 0.5f, alto * 0.5f, 0f) : new Vector3(0f, alto * 0.5f, 0f);
        hoja.AddComponent<MeshFilter>().sharedMesh = MallaCaja(tamHoja);
        hoja.AddComponent<MeshRenderer>().sharedMaterial = Mat(mat);
        var bc = hoja.AddComponent<BoxCollider>();
        bc.size = tamHoja;
        var obs = hoja.AddComponent<NavMeshObstacle>();
        obs.shape = NavMeshObstacleShape.Box;
        obs.size = tamHoja + new Vector3(0.1f, 0f, 0.3f);
        obs.carving = true;
        if (tipo == PuertaCap.Tipo.Bisagra)
        {
            var manija = new GameObject("Manija");
            manija.transform.SetParent(hoja.transform, false);
            manija.transform.localPosition = new Vector3(ancho * 0.38f, -0.1f, 0.07f);
            manija.AddComponent<MeshFilter>().sharedMesh = MallaCaja(new Vector3(0.12f, 0.03f, 0.05f));
            manija.AddComponent<MeshRenderer>().sharedMaterial = Mat("mat_metal_oscuro");
        }
        CambiarCapa(pivote.gameObject, CapaPuertas);

        var pc = raiz.AddComponent<PuertaCap>();
        pc.tipo = tipo;
        pc.hoja = pivote;
        pc.radio = 2.2f;
        pc.altura = 1.1f;
        pc.desplazamiento = tipo == PuertaCap.Tipo.Cortina ? alto - 0.3f : ancho;
        pc.sonido = tipo == PuertaCap.Tipo.Cortina ? "sfx_cortina" : (tipo == PuertaCap.Tipo.Reja ? "sfx_cadena" : "sfx_puerta");
        return pc;
    }

    public static ZonaEvento Zona(string n, Vector3 centro, Vector3 tam, Transform p, Acciones a, bool unaVez = true)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = centro;
        var z = go.AddComponent<ZonaEvento>();
        z.tamano = tam;
        z.soloUnaVez = unaVez;
        z.acciones = a;
        return z;
    }

    public static FuenteSonido Sonido(string n, Vector3 pos, string clip, float vol, float max, Transform p, float espacial = 1f)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var f = go.AddComponent<FuenteSonido>();
        f.clip = clip; f.volumen = vol; f.distanciaMax = max; f.espacial = espacial;
        return f;
    }

    public static void Incendio(Vector3 pos, float tam, Transform p)
    {
        var go = new GameObject("Incendio");
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        go.AddComponent<Fuego>().tamano = tam;
        var f = go.AddComponent<FuenteSonido>();
        f.clip = "amb_fuego"; f.volumen = 0.5f * tam; f.distanciaMax = 18f;
    }

    public static Acciones A(string objetivo = null, params string[] subtitulos)
    {
        return new Acciones { objetivo = objetivo, subtitulos = subtitulos };
    }
}
