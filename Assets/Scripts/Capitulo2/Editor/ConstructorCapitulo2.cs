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
/// Construye la escena del Capitulo 2 "Corte de Paso" (la Ceja de El Alto, 16:20) por codigo.
/// Wara (honda) y Tito Huanca (llave inglesa, compañero IA) cruzan de la Feria 16 de Julio a
/// Villa Dolores usando la estacion del teleferico como puente sobre la Av. 6 de Marzo.
/// Menu: Hoyada Z / Construir Capitulo 2. Reutiliza las utilidades del constructor del Capitulo 1.
/// Zonas: ConstructorCapitulo2_Zonas*.cs  ·  Guion: ConstructorCapitulo2_Guion.cs
/// </summary>
public static partial class ConstructorCapitulo1
{
    public const string RutaEscena2 = "Assets/Scenes/Capitulo2_CorteDePaso.unity";

    [MenuItem("Hoyada Z/Construir Capitulo 2")]
    public static void ConstruirCapitulo2Menu()
    {
        Debug.Log(ConstruirCapitulo2());
    }

    public static string ConstruirCapitulo2()
    {
        var log = new System.Text.StringBuilder();
        azar = new System.Random(2027);
        mats.Clear();
        mallas.Clear();

        ConfigurarCapas();
        ConfigurarAgente();
        MaterialesCap2();

        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var raizNivel = new GameObject("_Nivel").transform;
        var raizPersonajes = new GameObject("_Personajes").transform;
        var raizEventos = new GameObject("_Eventos").transform;
        var raizLuces = new GameObject("_Luces").transform;

        CrearSistemasCap2(log);

        padre = raizNivel;
        C2_Feria(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_Pasarela(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_Explanada(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_Estacion(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_Cabinas(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_Patio(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_VillaDolores(raizNivel, raizPersonajes, raizEventos, raizLuces);
        C2_Extras(raizNivel, raizPersonajes, raizEventos, raizLuces);
        padre = raizNivel;
        LimitesSalto(raizNivel);

        // NavMesh
        var nav = new GameObject("NavMesh");
        var surf = nav.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.All;
        surf.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surf.layerMask = ~((1 << 2) | (1 << CapaJugador) | (1 << CapaInfectado) | (1 << CapaPuertas));
        surf.BuildNavMesh();
        Directory.CreateDirectory("Assets/Scenes/Capitulo2_CorteDePaso");
        string rutaNav = "Assets/Scenes/Capitulo2_CorteDePaso/NavMesh.asset";
        AssetDatabase.DeleteAsset(rutaNav);
        if (surf.navMeshData != null)
        {
            AssetDatabase.CreateAsset(surf.navMeshData, rutaNav);
            log.AppendLine("NavMesh horneado.");
        }

        int fuera = 0;
        foreach (var inf in Object.FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(inf.transform.position, out hit, 2f, NavMesh.AllAreas)) { inf.transform.position = hit.position; }
            else { fuera++; log.AppendLine("Infectado fuera del NavMesh: " + inf.name + " " + inf.transform.position); }
        }
        foreach (var ag in Object.FindObjectsByType<Companera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            NavMeshHit hitC;
            if (NavMesh.SamplePosition(ag.transform.position, out hitC, 2f, NavMesh.AllAreas)) { ag.transform.position = hitC.position; }
            else { log.AppendLine("Compañero fuera del NavMesh: " + ag.name); }
        }

        GenerarRetratosCap2();
        var hudCap2 = Object.FindFirstObjectByType<HUDCap1>();
        AsignarRetratosCap2(hudCap2);
        Texture2D fondoCap2 = GenerarFondoMenuCap2();
        if (fondoCap2 != null) { hudCap2.fondoMenu = fondoCap2; }

        EditorSceneManager.SaveScene(escena, RutaEscena2);
        var escenas = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(RutaEscena, true), new EditorBuildSettingsScene(RutaEscena2, true) };
        foreach (var e in EditorBuildSettings.scenes)
        {
            if (e.path != RutaEscena && e.path != RutaEscena2) { escenas.Add(new EditorBuildSettingsScene(e.path, false)); }
        }
        EditorBuildSettings.scenes = escenas.ToArray();

        int nInf = Object.FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int nDoc = Object.FindObjectsByType<Documento>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int nRec = Object.FindObjectsByType<Recogible>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        log.AppendLine("Escena guardada: " + RutaEscena2 + " | infectados=" + nInf + " (fuera de navmesh=" + fuera + ") documentos=" + nDoc + " objetos=" + nRec);
        return log.ToString();
    }

    // ---------- Materiales propios del capitulo ----------

    private static void MaterialesCap2()
    {
        MatColor2("mat_char_tito", new Color(0.17f, 0.26f, 0.46f), 0.2f);
        MatColor2("mat_char_piel", new Color(0.55f, 0.38f, 0.28f), 0.3f);
        MatColor2("mat_char_chofer", new Color(0.36f, 0.26f, 0.18f), 0.2f);
        MatColor2("mat_teleferico_rojo", new Color(0.72f, 0.07f, 0.06f), 0.55f);
        MatColor2("mat_arena", new Color(0.52f, 0.46f, 0.34f), 0.08f);
        MatColor2("mat_puerta_azul", new Color(0.12f, 0.28f, 0.6f), 0.4f);
        MatColor2("mat_chaleco", new Color(0.08f, 0.09f, 0.1f), 0.35f);
        MatColor2("mat_adobe", new Color(0.5f, 0.35f, 0.25f), 0.05f);
        MatColor2("mat_verde_policia", new Color(0.12f, 0.26f, 0.16f), 0.3f);
        string ruta = CarpetaMat + "mat_laser_rojo.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetColor("_BaseColor", new Color(1f, 0.08f, 0.05f, 1f));
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
    }

    private static void MatColor2(string n, Color c, float suave)
    {
        string ruta = CarpetaMat + n + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", suave);
        EditorUtility.SetDirty(m);
    }

    // ---------- Sistemas: juego, Wara, camara, atardecer ----------

    private static void CrearSistemasCap2(System.Text.StringBuilder log)
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.44f, 0.38f, 0.4f);
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture = null;
        RenderSettings.reflectionIntensity = 0.25f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.011f;
        RenderSettings.fogColor = new Color(0.66f, 0.52f, 0.46f);

        var solGo = new GameObject("Sol");
        var sol = solGo.AddComponent<Light>();
        sol.type = LightType.Directional;
        sol.shadows = LightShadows.Soft;
        sol.shadowStrength = 0.8f;
        solGo.transform.rotation = Quaternion.Euler(22f, 78f, 0f);

        var vol = new GameObject("PostProceso").AddComponent<Volume>();
        vol.isGlobal = true;
        vol.sharedProfile = PerfilPostProcesoCap2();

        var jg = new GameObject("Juego");
        juego = jg.AddComponent<Juego>();
        juego.capitulo = 2;
        juego.nombreJugador = "Wara";
        juego.nombreCompleto = "WARA CONDORI";
        juego.escenaSiguiente = "";
        juego.lemaMenu = "El Alto, Bolivia. La tarde en que la fiebre subió a la Ceja.";
        juego.sol = sol;
        juego.solInicio = new Color(1f, 0.72f, 0.46f);
        juego.solFinal = new Color(0.95f, 0.42f, 0.32f);
        juego.intensidadInicio = 1.35f;
        juego.intensidadFinal = 0.45f;
        juego.nieblaInicio = new Color(0.66f, 0.52f, 0.46f);
        juego.nieblaFinal = new Color(0.24f, 0.2f, 0.3f);
        juego.ambienteInicio = new Color(0.44f, 0.38f, 0.4f);
        juego.ambienteFinal = new Color(0.2f, 0.19f, 0.28f);
        juego.densidadInicio = 0.011f;
        juego.densidadFinal = 0.02f;
        juego.solAlturaInicio = 22f;
        juego.solAlturaFinal = 3f;
        juego.solGiroInicio = 78f;
        juego.solGiroFinal = 96f;
        juego.minutoReloj = 980f;
        juego.segundosPorMinuto = 14f;
        GuionIntroCap2(juego);
        var hud = jg.AddComponent<HUDCap1>();
        AsignarHUD(hud);
        AsignarRetratosCap2(hud);
        var au = jg.AddComponent<AudioCap1>();
        var clips = new List<AudioClip>();
        foreach (string g in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Arte/Audio" }))
        {
            clips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g)));
        }
        au.clips = clips.ToArray();
        log.AppendLine("Clips de audio: " + clips.Count);

        jugador = CrearJugadorWara(new Vector3(0f, 0.05f, -5f), 0f);
        juego.jugador = jugador;

        var camGo = new GameObject("Camara");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.08f;
        cam.farClipPlane = 280f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = juego.nieblaInicio;
        cam.fieldOfView = 58f;
        camGo.AddComponent<AudioListener>();
        var ucd = camGo.AddComponent<UniversalAdditionalCameraData>();
        ucd.renderPostProcessing = true;
        ucd.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        var tps = camGo.AddComponent<CamaraTPS>();
        tps.objetivo = jugador.transform;
        camGo.transform.position = jugador.transform.position + new Vector3(1f, 1.8f, -3f);
        juego.camara = tps;
    }

    private static VolumeProfile PerfilPostProcesoCap2()
    {
        Directory.CreateDirectory("Assets/Arte/Ajustes");
        string ruta = "Assets/Arte/Ajustes/PerfilCapitulo2.asset";
        AssetDatabase.DeleteAsset(ruta);
        var p = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(p, ruta);
        var tone = p.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.Neutral);
        var bloom = p.Add<Bloom>(true);
        bloom.intensity.Override(0.7f);
        bloom.threshold.Override(1.05f);
        bloom.scatter.Override(0.6f);
        var vig = p.Add<Vignette>(true);
        vig.intensity.Override(0.34f);
        vig.smoothness.Override(0.45f);
        var col = p.Add<ColorAdjustments>(true);
        col.saturation.Override(-12f);
        col.contrast.Override(16f);
        col.postExposure.Override(0.55f);
        col.colorFilter.Override(new Color(1f, 0.93f, 0.86f));
        var grano = p.Add<FilmGrain>(true);
        grano.type.Override(FilmGrainLookup.Medium3);
        grano.intensity.Override(0.3f);
        var ca = p.Add<ChromaticAberration>(true);
        ca.intensity.Override(0.1f);
        foreach (var c in p.components) { AssetDatabase.AddObjectToAsset(c, p); }
        EditorUtility.SetDirty(p);
        AssetDatabase.SaveAssets();
        return p;
    }

    private static void AsignarRetratosCap2(HUDCap1 h)
    {
        System.Func<string, Texture2D> T = r =>
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Arte/UI/Retratos/" + r + ".png");
            return t != null ? t : AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Arte/UI/Retratos/" + r + ".jpg");
        };
        Texture2D wn = T("ui_retrato_wara_neutro");
        Texture2D wa = T("ui_retrato_wara_alerta");
        Texture2D wt = T("ui_retrato_wara_tension");
        h.mateoNeutro = wn;
        h.mateoAlerta = wa != null ? wa : wn;
        h.mateoTension = wt != null ? wt : wn;
        h.waraRetrato = wn;
        h.titoRetrato = T("ui_retrato_tito");
        h.choferRetrato = T("ui_retrato_chofer");
    }

    /// <summary>
    /// Retratos del HUD y de los dialogos: se fotografian los personajes de la escena con una
    /// camara temporal (Wara en 3 estados de salud, Tito y el chofer).
    /// </summary>
    private static void GenerarRetratosCap2()
    {
        string[] quienes = { "Wara", "Tito", "Chofer_Minibus" };
        string[] archivos = { "ui_retrato_wara_neutro", "ui_retrato_tito", "ui_retrato_chofer" };
        for (int q = 0; q < quienes.Length; q++)
        {
            var raiz = GameObject.Find(quienes[q]);
            if (raiz == null) { continue; }
            var esq = raiz.GetComponentInChildren<EsqueletoHumanoide>();
            if (esq == null) { continue; }
            Transform head = esq[EsqueletoHumanoide.Head];
            var capas = new Dictionary<Transform, int>();
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true)) { capas[t] = t.gameObject.layer; t.gameObject.layer = 31; }
            var camGo = new GameObject("CamRetrato");
            var cam = camGo.AddComponent<Camera>();
            cam.cullingMask = 1 << 31;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.16f, 0.12f, 0.12f, 1f);
            cam.fieldOfView = 26f;
            cam.nearClipPlane = 0.05f;
            Vector3 fw = raiz.transform.forward;
            Vector3 centro = head.position + Vector3.up * 0.06f;
            camGo.transform.position = centro + fw * 0.95f + raiz.transform.right * 0.18f + Vector3.up * 0.04f;
            camGo.transform.LookAt(centro - Vector3.up * 0.04f);
            var luzGo = new GameObject("LuzRetrato");
            var luz = luzGo.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.range = 4f;
            luz.intensity = 3f;
            luz.color = new Color(1f, 0.85f, 0.7f);
            luz.cullingMask = 1 << 31;
            luzGo.transform.position = centro + fw * 0.8f + raiz.transform.right * 0.6f + Vector3.up * 0.4f;
            var rt = new RenderTexture(256, 256, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(256, 256, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            GuardarRetrato(archivos[q], tex, Color.white);
            if (q == 0)
            {
                GuardarRetrato("ui_retrato_wara_alerta", tex, new Color(1f, 0.8f, 0.7f));
                GuardarRetrato("ui_retrato_wara_tension", tex, new Color(1f, 0.55f, 0.5f));
            }
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(luzGo);
            foreach (var kv in capas) { kv.Key.gameObject.layer = kv.Value; }
        }
    }

    /// <summary>Fondo del menu del capitulo: la estacion de la Linea Roja al atardecer, sin personajes.</summary>
    private static Texture2D GenerarFondoMenuCap2()
    {
        var camGo = GameObject.Find("Camara");
        var pers = GameObject.Find("_Personajes");
        if (camGo == null) { return null; }
        var cam = camGo.GetComponent<Camera>();
        Vector3 p0 = camGo.transform.position;
        Quaternion r0 = camGo.transform.rotation;
        Color bg = cam.backgroundColor;
        if (pers != null) { pers.SetActive(false); }
        // Atardecer avanzado para la foto
        var jg = Object.FindFirstObjectByType<Juego>();
        Color niebla0 = RenderSettings.fogColor, amb0 = RenderSettings.ambientLight;
        float dens0 = RenderSettings.fogDensity;
        RenderSettings.fogColor = Color.Lerp(jg.nieblaInicio, jg.nieblaFinal, 0.55f);
        RenderSettings.fogDensity = Mathf.Lerp(jg.densidadInicio, jg.densidadFinal, 0.55f);
        RenderSettings.ambientLight = Color.Lerp(jg.ambienteInicio, jg.ambienteFinal, 0.55f);
        Quaternion solRot0 = jg.sol.transform.rotation;
        Color solCol0 = jg.sol.color;
        float solInt0 = jg.sol.intensity;
        jg.sol.transform.rotation = Quaternion.Euler(Mathf.Lerp(jg.solAlturaInicio, jg.solAlturaFinal, 0.55f), Mathf.Lerp(jg.solGiroInicio, jg.solGiroFinal, 0.55f), 0f);
        jg.sol.color = Color.Lerp(jg.solInicio, jg.solFinal, 0.55f);
        jg.sol.intensity = Mathf.Lerp(jg.intensidadInicio, jg.intensidadFinal, 0.55f);
        camGo.transform.position = new Vector3(-67f, 9.4f, 141f);
        camGo.transform.LookAt(new Vector3(-36f, 9.2f, 148.5f));
        cam.backgroundColor = RenderSettings.fogColor;
        var rt = new RenderTexture(1920, 1080, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        camGo.transform.SetPositionAndRotation(p0, r0);
        cam.backgroundColor = bg;
        RenderSettings.fogColor = niebla0;
        RenderSettings.fogDensity = dens0;
        RenderSettings.ambientLight = amb0;
        jg.sol.transform.rotation = solRot0;
        jg.sol.color = solCol0;
        jg.sol.intensity = solInt0;
        if (pers != null) { pers.SetActive(true); }
        Directory.CreateDirectory("Assets/Arte/UI/Menu");
        string ruta = "Assets/Arte/UI/Menu/ui_fondo_menu_cap2.png";
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
    }

    private static void GuardarRetrato(string nombre, Texture2D fuente, Color tinte)
    {
        var t = new Texture2D(fuente.width, fuente.height, TextureFormat.RGB24, false);
        Color[] px = fuente.GetPixels();
        for (int i = 0; i < px.Length; i++) { px[i] = px[i] * tinte; }
        t.SetPixels(px);
        t.Apply();
        string ruta = "Assets/Arte/UI/Retratos/" + nombre + ".png";
        File.WriteAllBytes(ruta, t.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta);
    }

    /// <summary>Wara como jugadora: el mismo controlador de Mateo con el rig de la cebra.</summary>
    private static Jugador CrearJugadorWara(Vector3 pos, float rotY)
    {
        Jugador j = CrearJugador(pos, rotY);
        j.name = "Wara";
        if (j.anim != null) { Object.DestroyImmediate(j.anim.gameObject); }
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/Rig_Infectado.prefab"));
        rig.name = "Rig_Wara";
        rig.transform.SetParent(j.transform, false);
        rig.transform.localScale = Vector3.one * 0.93f;
        foreach (var smr in rig.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.sharedMesh = MallaCebra(smr.sharedMesh);
            smr.sharedMaterial = Mat("mat_cebra");
        }
        var anim = rig.GetComponent<AnimProcedural>();
        anim.estilo = AnimProcedural.Estilo.Humano;
        anim.velocidadCaminar = 2.6f;
        anim.velocidadCorrer = 5.8f;
        j.anim = anim;
        var esq = rig.GetComponent<EsqueletoHumanoide>();
        DisfrazCebra(esq);
        var inv = j.GetComponent<Inventario>();
        if (esq != null && esq.puntoArma != null)
        {
            inv.visualPalo = Arma("Arma_Palo", esq.puntoArma, new Vector3(0.045f, 0.045f, 0.95f), "mat_mateo_arma");
            inv.visualFierro = Arma("Arma_Fierro", esq.puntoArma, new Vector3(0.028f, 0.028f, 1.05f), "mat_fierro");
            inv.visualMachete = Arma("Arma_Machete", esq.puntoArma, new Vector3(0.018f, 0.08f, 0.66f), "mat_fierro");
            inv.visualHonda = Arma("Arma_Honda", esq.puntoArma, new Vector3(0.03f, 0.03f, 0.32f), "mat_honda");
            inv.visualRevolver = Arma("Arma_Revolver", esq.puntoArma, new Vector3(0.04f, 0.1f, 0.24f), "mat_revolver");
        }
        // Wara sube con su honda, algunas piedras, una linterna de mano y una venda
        inv.tieneHonda = true;
        inv.piedras = 12;
        inv.tieneLinterna = true;
        inv.vendas = 1;
        inv.botellas = 1;
        inv.ranura = Ranura.Honda;
        CambiarCapa(j.gameObject, CapaJugador);
        return j;
    }

    private static void DisfrazCebra(EsqueletoHumanoide ew)
    {
        if (ew == null) { return; }
        Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Sphere, new Vector3(0f, 0.1f, -0.01f), new Vector3(0.25f, 0.26f, 0.27f), "mat_cebra");
        Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Cube, new Vector3(0f, 0.16f, 0.14f), new Vector3(0.12f, 0.09f, 0.17f), "mat_cebra", new Vector3(12f, 0f, 0f));
        Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Capsule, new Vector3(0.08f, 0.27f, -0.02f), new Vector3(0.05f, 0.07f, 0.03f), "mat_cebra", new Vector3(0f, 0f, -15f));
        Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Capsule, new Vector3(-0.08f, 0.27f, -0.02f), new Vector3(0.05f, 0.07f, 0.03f), "mat_cebra", new Vector3(0f, 0f, 15f));
        Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Cube, new Vector3(0f, 0.19f, -0.13f), new Vector3(0.04f, 0.16f, 0.05f), "mat_negro");
    }

    /// <summary>Tito Huanca: tecnico de celulares de la Feria 16 de Julio, overol azul, gorra roja y llave inglesa.</summary>
    private static Companera CrearTito(Vector3 pos, float rotY, Transform pers)
    {
        var tito = new GameObject("Tito");
        tito.transform.SetParent(pers, false);
        tito.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        var ag = tito.AddComponent<NavMeshAgent>();
        ag.radius = 0.28f;
        ag.height = 1.7f;
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/Rig_Infectado.prefab"));
        rig.name = "Rig_Tito";
        rig.transform.SetParent(tito.transform, false);
        foreach (var smr in rig.GetComponentsInChildren<SkinnedMeshRenderer>()) { smr.sharedMaterial = Mat("mat_char_tito"); }
        rig.GetComponent<AnimProcedural>().estilo = AnimProcedural.Estilo.Humano;
        var e = rig.GetComponent<EsqueletoHumanoide>();
        if (e != null)
        {
            Rostro(e, true);
            Accesorio(e, EsqueletoHumanoide.Head, PrimitiveType.Sphere, new Vector3(0f, 0.17f, -0.01f), new Vector3(0.24f, 0.12f, 0.25f), "mat_toldo_rojo");
            Accesorio(e, EsqueletoHumanoide.Head, PrimitiveType.Cube, new Vector3(0f, 0.155f, 0.15f), new Vector3(0.19f, 0.022f, 0.12f), "mat_toldo_rojo");
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Cube, new Vector3(0f, -0.05f, -0.19f), new Vector3(0.3f, 0.38f, 0.15f), "mat_negro");
            Accesorio(e, EsqueletoHumanoide.Hips, PrimitiveType.Cube, new Vector3(0f, 0.06f, 0f), new Vector3(0.38f, 0.07f, 0.28f), "mat_madera");
            if (e.puntoArma != null)
            {
                var llave = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(llave.GetComponent<Collider>());
                llave.name = "Llave_Inglesa";
                llave.transform.SetParent(e.puntoArma, false);
                llave.transform.localPosition = new Vector3(0f, 0f, 0.14f);
                llave.transform.localScale = new Vector3(0.035f, 0.03f, 0.34f);
                llave.GetComponent<Renderer>().sharedMaterial = Mat("mat_fierro");
            }
        }
        var comp = tito.AddComponent<Companera>();
        comp.nombre = "Tito";
        comp.estado = Companera.EstadoC.Siguiendo;
        comp.cuerpoACuerpo = true;
        comp.danoCuerpoACuerpo = 18;
        comp.intervaloAyuda = 6f;
        comp.distraeJefe = true;
        comp.escondite = new Vector3(-71f, 0f, 174.2f);
        comp.destinoSeparacion = pos;
        comp.avisos = new[] { "¡Wara, atrás tuyo!", "¡Cuidado, cebrita, por detrás!", "¡Ahí viene uno por tu espalda!", "¡Wara, date la vuelta!" };
        comp.tiros = new[] { "¡Toma, llave inglesa en la cabeza!", "¡Suéltala, desgraciado!", "¡Eso es por mis compañeros!", "¡Ya está, ya está!" };
        comp.consejosJefe = new[]
        {
            "¡Agáchate, Wara! ¡Si te ve parada, dispara!",
            "¡De frente no le hace nada, tiene casco y chaleco!",
            "¡Yo le hago bulla, tú rodéalo por atrás!",
            "¡Cuando recarga es tu momento!",
            "¡Tírale una botella lejos con la Q, que se voltee!"
        };
        comp.distracciones = new[] { "¡Oye, Paco! ¡Aquí estoy, pues!", "¡Por aquí, mi subteniente!", "¡Acá, acá! ¡Dispárame a mí!", "¡Eh, Mamani! ¡Mírame!" };
        return comp;
    }

    // ---------- Utilidades del capitulo 2 ----------

    /// <summary>Cara simple (piel, ojos, cejas, nariz, boca y bigote opcional) para los NPC vivos.</summary>
    private static void Rostro(EsqueletoHumanoide e, bool bigote)
    {
        if (e == null) { return; }
        int h = EsqueletoHumanoide.Head;
        Accesorio(e, h, PrimitiveType.Sphere, new Vector3(0f, 0.08f, 0.015f), new Vector3(0.21f, 0.24f, 0.23f), "mat_char_piel");
        for (int s = -1; s <= 1; s += 2)
        {
            Accesorio(e, h, PrimitiveType.Sphere, new Vector3(s * 0.042f, 0.1f, 0.118f), new Vector3(0.028f, 0.02f, 0.012f), "mat_negro");
            Accesorio(e, h, PrimitiveType.Cube, new Vector3(s * 0.043f, 0.128f, 0.122f), new Vector3(0.048f, 0.011f, 0.01f), "mat_negro", new Vector3(0f, 0f, s * -8f));
        }
        Accesorio(e, h, PrimitiveType.Sphere, new Vector3(0f, 0.075f, 0.13f), new Vector3(0.03f, 0.045f, 0.03f), "mat_char_piel");
        Accesorio(e, h, PrimitiveType.Cube, new Vector3(0f, 0.032f, 0.121f), new Vector3(0.052f, 0.009f, 0.01f), "mat_sangre");
        if (bigote) { Accesorio(e, h, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.127f), new Vector3(0.075f, 0.016f, 0.012f), "mat_negro"); }
    }

    /// <summary>Escalera maciza de 'abajo' a 'arriba' en cualquier direccion horizontal, con rampa lisa y barandas.</summary>
    private static void Escalera(string n, Vector3 abajo, Vector3 arriba, float ancho, bool barandas, Transform p)
    {
        Vector3 dir = arriba - abajo; dir.y = 0f;
        float largo = dir.magnitude;
        dir /= largo;
        float alto = arriba.y - abajo.y;
        float rotY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        int escalones = Mathf.Max(4, Mathf.RoundToInt(alto / 0.18f));
        float huella = largo / escalones, contra = alto / escalones;
        float suelo = abajo.y - 0.3f;
        for (int i = 0; i < escalones; i++)
        {
            float yTop = abajo.y + contra * (i + 1);
            Vector3 c = abajo + dir * (huella * (i + 0.5f));
            c.y = (suelo + yTop) * 0.5f;
            Caja(n + "_Escalon_" + i, c, new Vector3(ancho, yTop - suelo, huella), "mat_concreto", true, rotY, p);
        }
        float ang = Mathf.Atan2(alto, largo) * Mathf.Rad2Deg;
        float hip = Mathf.Sqrt(largo * largo + alto * alto);
        Quaternion rot = Quaternion.Euler(0f, rotY, 0f) * Quaternion.Euler(-ang, 0f, 0f);
        Vector3 medio = (abajo + arriba) * 0.5f;
        var rampa = new GameObject(n + "_Rampa");
        rampa.transform.SetParent(p, false);
        rampa.transform.SetPositionAndRotation(medio - rot * Vector3.up * 0.15f, rot);
        rampa.AddComponent<BoxCollider>().size = new Vector3(ancho, 0.3f, hip + 0.1f);
        if (barandas)
        {
            Vector3 der = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 lado = der * (s * (ancho * 0.5f + 0.05f));
                var col = new GameObject(n + "_Baranda_" + (s < 0 ? "I" : "D"));
                col.transform.SetParent(p, false);
                col.transform.SetPositionAndRotation(medio + lado + Vector3.up * 0.6f, rot);
                col.AddComponent<BoxCollider>().size = new Vector3(0.1f, 1.2f, hip);
                var riel = Caja(n + "_Pasamanos_" + (s < 0 ? "I" : "D"), medio + lado + Vector3.up * 1.18f, new Vector3(0.07f, 0.07f, hip), "mat_fierro", false, 0f, p);
                riel.transform.rotation = rot;
                int postes = Mathf.Max(2, Mathf.RoundToInt(largo / 1.5f));
                for (int k = 0; k <= postes; k++)
                {
                    Vector3 q = Vector3.Lerp(abajo, arriba, k / (float)postes) + lado;
                    Caja(n + "_Barrote_" + (s < 0 ? "I" : "D") + k, q + Vector3.up * 0.6f, new Vector3(0.05f, 1.2f, 0.05f), "mat_fierro", false, rotY, p);
                }
            }
        }
    }

    /// <summary>Baranda recta (1.2 m, no se puede saltar) de a hasta b.</summary>
    private static void Baranda(string n, Vector3 a, Vector3 b, Transform p)
    {
        Vector3 d = b - a; d.y = 0f;
        float rot = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        Vector3 c = (a + b) * 0.5f;
        float largo = d.magnitude;
        // Colision (invisible) + barrotes, pasamanos y travesaño
        var col = new GameObject(n);
        col.transform.SetParent(p, false);
        col.transform.SetPositionAndRotation(c + Vector3.up * 0.6f, Quaternion.Euler(0f, rot, 0f));
        col.AddComponent<BoxCollider>().size = new Vector3(0.12f, 1.2f, largo);
        Caja(n + "_Pasamanos", c + Vector3.up * 1.18f, new Vector3(0.07f, 0.07f, largo), "mat_fierro", false, rot, p);
        Caja(n + "_Travesano", c + Vector3.up * 0.55f, new Vector3(0.04f, 0.04f, largo), "mat_fierro", false, rot, p);
        Caja(n + "_Zocalo", c + Vector3.up * 0.06f, new Vector3(0.08f, 0.12f, largo), "mat_metal_oscuro", false, rot, p);
        int postes = Mathf.Max(1, Mathf.RoundToInt(largo / 1.4f));
        Vector3 dirN = d.normalized;
        for (int i = 0; i <= postes; i++)
        {
            Vector3 q = a + dirN * (largo * i / postes);
            Caja(n + "_Barrote_" + i, q + Vector3.up * 0.6f, new Vector3(0.05f, 1.2f, 0.05f), "mat_fierro", false, rot, p);
        }
    }

    /// <summary>Pared invisible (capa Ignore Raycast) para que no se caigan del mapa.</summary>
    private static void Invisible(string n, Vector3 min, Vector3 max, Transform p)
    {
        var go = new GameObject("Limite_" + n);
        go.transform.SetParent(p, false);
        go.layer = 2;
        go.transform.position = (min + max) * 0.5f;
        go.AddComponent<BoxCollider>().size = max - min;
    }

    /// <summary>Barricada de sacos de arena (1.15 m) o de calaminas (1.3 m): cobertura contra los tiros.</summary>
    private static void Barricada(string n, Vector3 centro, float largo, float rotY, bool sacos, Transform p)
    {
        if (sacos)
        {
            Caja(n, centro + Vector3.up * 0.55f, new Vector3(largo, 1.1f, 0.8f), "mat_arena", true, rotY, p);
            int k = Mathf.Max(2, Mathf.RoundToInt(largo / 0.7f));
            Quaternion q = Quaternion.Euler(0f, rotY, 0f);
            for (int i = 0; i < k; i++)
            {
                Vector3 o = q * new Vector3(-largo * 0.5f + (i + 0.5f) * largo / k, 1.16f, 0f);
                Caja(n + "_Saco_" + i, centro + o, new Vector3(largo / k - 0.06f, 0.14f, 0.6f), "mat_arena", false, rotY + (i % 2 == 0 ? 3f : -4f), p);
            }
        }
        else
        {
            Caja(n, centro + Vector3.up * 0.65f, new Vector3(largo, 1.3f, 0.12f), "mat_calamina", true, rotY, p);
            Quaternion q = Quaternion.Euler(0f, rotY, 0f);
            for (int i = 0; i <= Mathf.FloorToInt(largo / 1.5f); i++)
            {
                Vector3 o = q * new Vector3(-largo * 0.5f + i * 1.5f, 0.7f, -0.12f);
                Caja(n + "_Poste_" + i, centro + o, new Vector3(0.1f, 1.4f, 0.1f), "mat_madera", false, rotY, p);
            }
        }
    }

    private static void Puesto(string n, Vector3 pos, float rotY, string toldo, Transform p)
    {
        Modelo("Props/prop_puesto_mercado.obj", pos, rotY, "mat_prop_puesto", p);
        Caja(n + "_Toldo", pos + Vector3.up * 2.55f, new Vector3(2.6f, 0.04f, 2.2f), toldo, false, rotY + (float)(azar.NextDouble() * 8 - 4), p);
    }

    private static void Cadaver(string n, Vector3 pos, float rotY, string mat, Transform p)
    {
        CrearNPC("Rig_Infectado", n, pos, rotY, mat, PoseNPC.Pose.Cadaver, false, p, AnimProcedural.Estilo.Humano);
        Sangre(pos + Vector3.up * 0.02f, 1.3f, p);
    }

    private static EventoAsedio Asedio(string n, Vector3 pos, float duracion, Transform p)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var ea = go.AddComponent<EventoAsedio>();
        ea.duracion = duracion;
        return ea;
    }

    private static Interruptor Accion(string n, Vector3 pos, float rotY, string texto, Transform p)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        var it = go.AddComponent<Interruptor>();
        it.accion = texto;
        it.radio = 2.2f;
        it.altura = 1f;
        it.destello = true;
        return it;
    }
}
