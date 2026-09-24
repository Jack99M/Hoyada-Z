using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

/// <summary>
/// Contenido ampliado del Capitulo 1 (se construye despues de las zonas base):
/// graffitis y letreros paceños, banderas, teleferico detenido, altares para guardar,
/// illas de Alasitas, archivos nuevos, materiales de fabricacion, casetas con candado,
/// revolver del sargento, Wara (la cebra) y el Chino (lustrabotas), gritones, el jefe
/// El Carnicero con su camara frigorifica, cinematicas, marcadores de objetivo y el
/// modo Supervivencia en la Plaza Abaroa.
/// </summary>
public static partial class ConstructorCapitulo1
{
    /// <summary>
    /// Copia de la malla del rig con UV cilindricas para que el traje de cebra de Wara
    /// muestre franjas que rodean el cuerpo (la malla original tiene UV de atlas).
    /// </summary>
    private static Mesh MallaCebra(Mesh fuente)
    {
        const string ruta = "Assets/Arte/Modelos/Rig/Wara_Cebra_mesh.asset";
        if (fuente == null) { return null; }
        Mesh m = Object.Instantiate(fuente);
        m.name = "Wara_Cebra";
        Vector3[] v = m.vertices;
        Bounds b = m.bounds;
        float alturaBrazos = b.min.y + b.size.y * 0.8f;
        var uv = new Vector2[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            Vector3 d = v[i] - b.center;
            bool brazo = Mathf.Abs(d.x) > 0.21f && v[i].y > b.min.y + b.size.y * 0.6f;
            float eje, ang;
            if (brazo)
            {
                eje = 1.4f + Mathf.Abs(d.x);
                ang = Mathf.Atan2(v[i].y - alturaBrazos, d.z);
            }
            else
            {
                eje = v[i].y - b.min.y;
                float cx = Mathf.Abs(d.x) > 0.06f && v[i].y < b.min.y + b.size.y * 0.5f ? Mathf.Sign(d.x) * 0.1f : 0f;
                ang = Mathf.Atan2(d.x - cx, d.z);
            }
            uv[i] = new Vector2(eje * 2.3f, (ang / (2f * Mathf.PI) + 0.5f) * 2f);
        }
        m.uv = uv;
        if (AssetDatabase.LoadAssetAtPath<Mesh>(ruta) != null) { AssetDatabase.DeleteAsset(ruta); }
        AssetDatabase.CreateAsset(m, ruta);
        return m;
    }

    private static T Buscar<T>(string nombre) where T : Component
    {
        foreach (T c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.name == nombre) { return c; }
        }
        Debug.LogWarning("Extras: no se encontro " + typeof(T).Name + " '" + nombre + "'");
        return null;
    }

    private static float Aspecto(string mat)
    {
        Material m = Mat(mat);
        return m != null && m.mainTexture != null ? m.mainTexture.width / (float)m.mainTexture.height : 4f;
    }

    /// <summary>Plano con textura pegado a una pared. 'normal' apunta hacia donde se ve.</summary>
    public static GameObject Plano(string n, Vector3 centro, Vector3 normal, float ancho, string mat, Transform p, float giro = 0f, float alto = -1f)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = n;
        Object.DestroyImmediate(q.GetComponent<Collider>());
        q.transform.SetParent(p, false);
        if (alto <= 0f) { alto = ancho / Aspecto(mat); }
        q.transform.position = centro + normal.normalized * 0.03f;
        q.transform.rotation = Quaternion.LookRotation(-normal.normalized) * Quaternion.Euler(0f, 0f, giro);
        q.transform.localScale = new Vector3(ancho, alto, 1f);
        var r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = Mat(mat);
        r.shadowCastingMode = ShadowCastingMode.Off;
        return q;
    }

    private static void Graf(string id, Vector3 centro, Vector3 normal, float ancho, Transform p, float giro = 0f)
    {
        LimpiarFachada(centro, normal, ancho, ancho / Aspecto("mat_g_" + id));
        Plano("Graffiti_" + id, centro, normal, ancho, "mat_g_" + id, p, giro);
    }

    /// <summary>Quita ventanas y marcos de la fachada que taparian un graffiti o letrero.</summary>
    private static void LimpiarFachada(Vector3 centro, Vector3 normal, float ancho, float alto)
    {
        Vector3 n = normal.normalized;
        Vector3 der = Vector3.Cross(Vector3.up, n).normalized;
        foreach (MeshRenderer mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            string nm = mr.name;
            if (!(nm.Contains("_Ventana_") || nm.Contains("_Marco_") || nm.Contains("_Balcon_") || nm.Contains("_Baranda_"))) { continue; }
            Vector3 d = mr.transform.position - centro;
            if (Mathf.Abs(Vector3.Dot(d, n)) > 1.2f) { continue; }
            if (Mathf.Abs(Vector3.Dot(d, der)) > ancho * 0.5f + 0.8f || Mathf.Abs(d.y) > alto * 0.5f + 0.8f) { continue; }
            Object.DestroyImmediate(mr.gameObject);
        }
    }

    private static void Letrero(string id, Vector3 centro, Vector3 normal, float ancho, Transform p, bool marco = true)
    {
        LimpiarFachada(centro, normal, ancho, ancho / Aspecto("mat_l_" + id));
        var l = Plano("Letrero_" + id, centro, normal, ancho, "mat_l_" + id, p);
        if (marco)
        {
            float alto = ancho / Aspecto("mat_l_" + id);
            Vector3 n = normal.normalized;
            var fondo = Caja("Letrero_" + id + "_Placa", centro - n * 0.01f, new Vector3(ancho + 0.08f, alto + 0.08f, 0.04f), "mat_metal_oscuro", false, 0f, p);
            fondo.transform.rotation = Quaternion.LookRotation(-n);
        }
    }

    private static GameObject Accesorio(EsqueletoHumanoide e, int hueso, PrimitiveType forma, Vector3 offset, Vector3 escala, string mat, Vector3 rot = default(Vector3))
    {
        var go = GameObject.CreatePrimitive(forma);
        go.name = "Accesorio_" + mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Transform b = e[hueso];
        Transform raiz = e.transform;
        float s = raiz.lossyScale.y;
        go.transform.position = b.position + raiz.rotation * (offset * s);
        go.transform.rotation = raiz.rotation * Quaternion.Euler(rot);
        go.transform.localScale = escala * s;
        go.transform.SetParent(b, true);
        go.GetComponent<Renderer>().sharedMaterial = Mat(mat);
        go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
        return go;
    }

    // ---------- Objetos ----------

    private static Recogible Suministro(string n, Vector3 pos, Recogible.TipoObjeto t, int cant, Transform p, string nombre = null)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var r = go.AddComponent<Recogible>();
        r.tipo = t; r.cantidad = cant; r.nombre = nombre;
        r.destello = true; r.radio = 1.7f; r.altura = 0.3f;
        switch (t)
        {
            case Recogible.TipoObjeto.Alcohol:
                Visual(go, new Vector3(0.07f, 0.24f, 0.07f), "mat_vidrio", 0.12f);
                Visual(go, new Vector3(0.075f, 0.08f, 0.075f), "mat_baldosa", 0.1f);
                break;
            case Recogible.TipoObjeto.Trapo: Visual(go, new Vector3(0.28f, 0.03f, 0.22f), "mat_aguayo", 0.015f); break;
            case Recogible.TipoObjeto.Cinta: Visual(go, new Vector3(0.1f, 0.05f, 0.1f), "mat_baldosa", 0.025f); break;
            case Recogible.TipoObjeto.Cuchilla: Visual(go, new Vector3(0.025f, 0.015f, 0.16f), "mat_fierro", 0.008f); Visual(go, new Vector3(0.03f, 0.02f, 0.07f), "mat_negro", 0.01f); break;
            case Recogible.TipoObjeto.Piedras:
                Visual(go, new Vector3(0.07f, 0.06f, 0.07f), "mat_concreto", 0.03f);
                Visual(go, new Vector3(0.06f, 0.05f, 0.05f), "mat_concreto", 0.03f);
                go.transform.GetChild(1).localPosition = new Vector3(0.08f, 0.025f, 0.03f);
                break;
            case Recogible.TipoObjeto.Balas: Visual(go, new Vector3(0.1f, 0.05f, 0.07f), "mat_barrera", 0.025f); break;
            case Recogible.TipoObjeto.Revolver: Visual(go, new Vector3(0.05f, 0.03f, 0.26f), "mat_revolver", 0.02f); Visual(go, new Vector3(0.04f, 0.03f, 0.1f), "mat_madera", 0.02f); go.transform.GetChild(1).localPosition = new Vector3(0f, 0.02f, -0.14f); break;
            case Recogible.TipoObjeto.Machete: Visual(go, new Vector3(0.09f, 0.015f, 0.62f), "mat_fierro", 0.01f); break;
            case Recogible.TipoObjeto.Molotov: Visual(go, new Vector3(0.08f, 0.26f, 0.08f), "mat_vidrio", 0.13f); Visual(go, new Vector3(0.05f, 0.06f, 0.05f), "mat_aguayo", 0.28f); break;
            default: Visual(go, new Vector3(0.12f, 0.04f, 0.1f), "mat_barrera", 0.02f); break;
        }
        return r;
    }

    private static PuntoGuardado Altar(string n, Vector3 pos, float rotY, string lugar, Transform p, Transform luces)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        Transform t = go.transform;
        Caja(n + "_Mesa", t.TransformPoint(0f, 0.35f, 0f), new Vector3(0.9f, 0.7f, 0.5f), "mat_madera", true, rotY, t);
        Caja(n + "_Aguayo", t.TransformPoint(0f, 0.71f, 0f), new Vector3(0.96f, 0.02f, 0.56f), "mat_aguayo", false, rotY, t);
        Caja(n + "_Coca", t.TransformPoint(0.18f, 0.73f, 0.04f), new Vector3(0.22f, 0.015f, 0.16f), "mat_coca", false, rotY + 25f, t);
        Caja(n + "_Estampa", t.TransformPoint(0f, 0.86f, 0.17f), new Vector3(0.18f, 0.28f, 0.03f), "mat_yeso", false, rotY, t);
        for (int i = 0; i < 3; i++)
        {
            var v = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            v.name = n + "_Vela";
            Object.DestroyImmediate(v.GetComponent<Collider>());
            v.transform.SetParent(t, false);
            v.transform.position = t.TransformPoint(-0.3f + i * 0.13f, 0.78f + (i % 2) * 0.02f, -0.1f);
            v.transform.localScale = new Vector3(0.045f, 0.06f + (i % 2) * 0.02f, 0.045f);
            v.GetComponent<Renderer>().sharedMaterial = Mat("mat_vela");
        }
        Light l = Luz(n + "_Llama", pos + Vector3.up * 1.05f, new Color(1f, 0.7f, 0.35f), 0.9f, 4.5f, luces, LuzParpadeante.Modo.Fuego);
        var pg = go.AddComponent<PuntoGuardado>();
        pg.lugar = lugar;
        pg.llama = l;
        pg.radio = 2.1f;
        pg.altura = 0.9f;
        pg.destello = true;
        return pg;
    }

    private static void Illa(int i, Vector3 pos, Transform p, Transform luces)
    {
        var go = new GameObject("Illa_" + (i + 1));
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var c = go.AddComponent<Coleccionable>();
        c.titulo = Illas[i, 0];
        c.descripcion = Illas[i, 1];
        c.destello = true;
        c.radio = 1.7f;
        c.altura = 0.15f;
        Visual(go, new Vector3(0.09f, 0.12f, 0.07f), "mat_illa", 0.06f);
        Visual(go, new Vector3(0.05f, 0.05f, 0.05f), "mat_yeso", 0.15f);
        var l = new GameObject("Brillo").AddComponent<Light>();
        l.transform.SetParent(go.transform, false);
        l.transform.localPosition = Vector3.up * 0.35f;
        l.type = LightType.Point;
        l.color = new Color(1f, 0.8f, 0.4f);
        l.intensity = 0.8f;
        l.range = 1.6f;
    }

    private static void Bandera(string mat, Vector3 pos, Vector3 normal, float ancho, float alto, Transform p)
    {
        var raiz = new GameObject("Bandera_" + mat);
        raiz.transform.SetParent(p, false);
        raiz.transform.position = pos;
        raiz.transform.rotation = Quaternion.LookRotation(-normal);
        var b = raiz.AddComponent<Balanceo>();
        b.eje = Vector3.up;
        b.amplitud = 6f;
        b.velocidad = 1.1f;
        var a = Plano("Frente", pos, normal, ancho, mat, raiz.transform, 0f, alto);
        var d = Plano("Dorso", pos, -normal, ancho, mat, raiz.transform, 0f, alto);
        a.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        d.transform.localPosition = new Vector3(0f, 0f, 0.01f);
    }

    private static void Cable(string n, Vector3 a, Vector3 b, float grosor, Transform p)
    {
        var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        c.name = n;
        Object.DestroyImmediate(c.GetComponent<Collider>());
        c.transform.SetParent(p, false);
        c.transform.position = (a + b) * 0.5f;
        c.transform.rotation = Quaternion.FromToRotation(Vector3.up, b - a);
        c.transform.localScale = new Vector3(grosor, (b - a).magnitude * 0.5f, grosor);
        c.GetComponent<Renderer>().sharedMaterial = Mat("mat_cable");
        c.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
    }

    private static PuertaCap Caseta(string n, float x0, float x1, float z0, float z1, float y0, char ladoPuerta, float centroPuerta, float rotPuerta, Transform p)
    {
        Hueco[] puerta = { Hueco.Puerta(centroPuerta, 1.0f, 2.1f) };
        Cuarto(n, x0, x1, z0, z1, y0, 2.5f, "mat_metal_pintado", "mat_concreto", "mat_calamina",
            ladoPuerta == 'N' ? puerta : SinHuecos, ladoPuerta == 'S' ? puerta : SinHuecos,
            ladoPuerta == 'E' ? puerta : SinHuecos, ladoPuerta == 'O' ? puerta : SinHuecos, true, 0.12f);
        Vector3 pos;
        switch (ladoPuerta)
        {
            case 'N': pos = new Vector3(centroPuerta, y0, z1); break;
            case 'S': pos = new Vector3(centroPuerta, y0, z0); break;
            case 'E': pos = new Vector3(x1, y0, centroPuerta); break;
            default: pos = new Vector3(x0, y0, centroPuerta); break;
        }
        var pc = Puerta(n + "_Puerta", pos, rotPuerta, 1.0f, 2.1f, PuertaCap.Tipo.Bisagra, "mat_metal_oscuro", p);
        pc.llaveRequerida = PuertaCap.CandadoPunta;
        pc.nombreLlave = "Punta";
        pc.mensajeCerrada = "Tiene un candado viejo. Con una punta lo podría forzar.";
        pc.ruidoAlAbrir = 2f;
        return pc;
    }

    // =====================================================================
    // CONTENIDO EXTRA
    // =====================================================================
    private static void Extras(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z7_Extras").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        Graffitis(raiz);
        Letreros(raiz);
        Teleferico(raiz, luces);
        Altares(raiz, luces);
        IllasYArchivos(raiz, luces);
        Materiales(raiz);
        Casetas(raiz);
        Personajes(raiz, pers, ev, luces);
        Jefe(raiz, pers, ev, luces);
        Cinematicas(ev);
        Marcadores();
        Tutoriales(ev);
        LimitesSalto(raiz);
    }

    /// <summary>
    /// Con el salto, las barreras bajas (policiales, parapetos de la calle alta) se podrian
    /// saltar y salir del mapa: se les pone encima una pared invisible alta.
    /// Capa Ignore Raycast para no molestar a la camara ni a los disparos.
    /// </summary>
    private static void LimitesSalto(Transform r)
    {
        var raiz = new GameObject("Limites_Invisibles").transform;
        raiz.SetParent(r, false);
        int n = 0;
        foreach (Collider c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (c.isTrigger) { continue; }
            string nom = c.name;
            if (!nom.StartsWith("Barrera") && !nom.StartsWith("Calle_Parapeto")) { continue; }
            Bounds b = c.bounds;
            var go = new GameObject("Limite_" + nom);
            go.transform.SetParent(raiz, false);
            go.layer = 2;
            go.transform.position = new Vector3(b.center.x, b.max.y + 1.75f, b.center.z);
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(Mathf.Max(b.size.x, 0.3f), 3.5f, Mathf.Max(b.size.z, 0.3f));
            n++;
        }
        Debug.Log("Limites invisibles sobre barreras: " + n);
    }

    private static void Graffitis(Transform r)
    {
        Vector3 E = Vector3.right, O = Vector3.left, N = Vector3.forward, S = Vector3.back;
        // Callejon: dialogo politico entre dos paredes
        Graf("agua", new Vector3(-36f, 1.9f, -4f), E, 3.2f, r);
        Graf("mordidos", new Vector3(-30.15f, 1.9f, -12f), O, 3.4f, r, 2f);
        Graf("evo", new Vector3(-36f, 2.3f, 10f), E, 4.2f, r, -3f);
        Graf("abajo_mas", new Vector3(-30f, 2.05f, 11.4f), O, 5.1f, r, 2f);
        Graf("cuarentena", new Vector3(-36f, 1.7f, 20.5f), E, 3f, r);
        Graf("x", new Vector3(-30.15f, 1.5f, 3.4f), O, 1f, r);
        // Pasaje
        Graf("21f", new Vector3(-22f, 2.1f, 28f), S, 5f, r, -2f);
        Graf("amor", new Vector3(-24f, 1.4f, 22f), N, 2.3f, r, 4f);
        Graf("dios", new Vector3(-13.5f, 2.6f, 28f), S, 3f, r);
        // Avenida 20 de Octubre
        Graf("alto", new Vector3(-8f, 2.3f, 14f), E, 4.6f, r);
        Graf("jallalla", new Vector3(-8f, 2.5f, 44f), E, 4.5f, r, -2f);
        Graf("choqueyapu", new Vector3(-8f, 2.1f, 52.5f), E, 3.8f, r, 1f);
        Graf("academia", new Vector3(8f, 1.8f, 2f), O, 3.4f, r, 2f);
        Graf("fuera", new Vector3(8f, 2.4f, -3.5f), O, 3.2f, r);
        // Calle Belisario Salinas (frente a la plaza)
        Graf("resiste", new Vector3(33f, 2.2f, 10f), N, 5f, r);
        Graf("tigre", new Vector3(42.5f, 1.8f, 10f), N, 4.4f, r, -2f);
        Graf("refugio", new Vector3(48f, 2.4f, 36f), O, 4.4f, r);
        // Mercado
        Graf("vivos", new Vector3(20.5f, 1.6f, 51.84f), S, 2.8f, r, 3f);
        Graf("clic", new Vector3(10.16f, 2f, 69f), E, 3f, r);
        Graf("bety", new Vector3(37f, 1.8f, 79.84f), S, 3f, r, -2f);
        // Gradas
        Graf("x", new Vector3(30f, 5.5f, 94f), E, 1.2f, r);
        Graf("cuarentena", new Vector3(34f, 3.8f, 85f), O, 2.6f, r);
    }

    private static void Letreros(Transform r)
    {
        Vector3 E = Vector3.right, O = Vector3.left, N = Vector3.forward, S = Vector3.back;
        Letrero("disco", new Vector3(-7.6f, 4.1f, -6f), E, 3.8f, r, false);
        Letrero("farmacia", new Vector3(19f, 3.25f, 10.3f), N, 4.8f, r);
        Letrero("mercado", new Vector3(25f, 4.8f, 51.7f), S, 8.4f, r);
        Letrero("saltenas", new Vector3(-7.85f, 3.2f, 40.5f), E, 4.4f, r);
        Letrero("pension", new Vector3(-7.85f, 3.1f, 32f), E, 3.2f, r);
        Letrero("api", new Vector3(9.85f, 3.2f, 10.05f), N, 3f, r);
        Letrero("ferreteria", new Vector3(37f, 2.4f, 72.25f), S, 3f, r);
        Letrero("carniceria", new Vector3(39.83f, 3.3f, 62f), O, 3f, r);
        Letrero("calle20", new Vector3(-7.85f, 3.5f, 21.6f), E, 1.7f, r);
        Letrero("plaza", new Vector3(8.02f, 3.3f, 9.5f), O, 1.6f, r);
        Letrero("belisario", new Vector3(11.68f, 3.4f, 10.05f), N, 1.9f, r);
        Letrero("teleferico", new Vector3(56f, 10f, 106f), O, 4f, r);
        Letrero("kiosco", new Vector3(16f, 2.55f, 43.96f), S, 2.6f, r, false);
        Letrero("cebras", new Vector3(14.48f, 1.7f, 45f), O, 1.6f, r, false);
        Letrero("cuarentena", new Vector3(0.8f, 1.9f, 58.2f), S, 2.2f, r);
        Letrero("karaoke", new Vector3(14f, 10.6f, 112f), S, 4.2f, r, false);
        Letrero("sedes", new Vector3(13.1f, 1.7f, 10.03f), N, 0.9f, r, false);
        Letrero("sedes", new Vector3(-29.8f, 1.7f, 22.03f), N, 0.9f, r, false);
        Letrero("sedes", new Vector3(24.5f, 1.8f, 51.84f), S, 0.9f, r, false);
        Letrero("minibus", new Vector3(-3.1f, 1.95f, 9.2f), new Vector3(0.37f, 0f, 0.93f), 1.6f, r, false);
        Luz("Karaoke_Luz", new Vector3(14f, 10.4f, 111f), new Color(0.3f, 0.9f, 1f), 1.2f, 6f, r, LuzParpadeante.Modo.Falla);

        // Banderas: tricolor y wiphala en los balcones y en la plaza
        Bandera("mat_bandera_bolivia", new Vector3(-7.4f, 6.6f, 48f), Vector3.right, 1.5f, 1f, r);
        Bandera("mat_wiphala", new Vector3(-7.4f, 6.6f, 54.5f), Vector3.right, 1.3f, 1.3f, r);
        Bandera("mat_wiphala", new Vector3(20f, 6.2f, 111.4f), Vector3.back, 1.2f, 1.2f, r);
        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? 22.6f : 29.4f;
            Caja("Asta_" + i, new Vector3(x, 3.2f, 34.2f), new Vector3(0.08f, 6f, 0.08f), "mat_metal_oscuro", true);
            Bandera(i == 0 ? "mat_bandera_bolivia" : "mat_wiphala", new Vector3(x + 0.8f, 5.6f, 34.2f), Vector3.back, 1.5f, i == 0 ? 1f : 1.4f, r);
        }
    }

    private static void Teleferico(Transform r, Transform luces)
    {
        var raiz = new GameObject("Teleferico_LineaAmarilla").transform;
        raiz.SetParent(r, false);
        Vector3 a = new Vector3(-40f, 48f, 150f), b = new Vector3(120f, 38f, 40f);
        Vector3 lado = Vector3.Cross(Vector3.up, (b - a).normalized) * 1.3f;
        Cable("Cable_1", a + lado, b + lado, 0.07f, raiz);
        Cable("Cable_2", a - lado, b - lado, 0.07f, raiz);
        float[] ts = { 0.12f, 0.3f, 0.45f, 0.62f, 0.8f };
        for (int i = 0; i < ts.Length; i++)
        {
            Vector3 c = Vector3.Lerp(a, b, ts[i]) + (i % 2 == 0 ? lado : -lado);
            var cab = new GameObject("Cabina_" + i).transform;
            cab.SetParent(raiz, false);
            cab.position = c;
            cab.rotation = Quaternion.LookRotation(new Vector3((b - a).x, 0f, (b - a).z));
            var bal = cab.gameObject.AddComponent<Balanceo>();
            bal.eje = Vector3.right;
            bal.amplitud = i == 2 ? 4f : 1.2f;
            bal.velocidad = i == 2 ? 0.7f : 0.4f;
            Caja("Brazo", cab.TransformPoint(0f, -1.1f, 0f), new Vector3(0.12f, 2.2f, 0.12f), "mat_metal_oscuro", false, 0f, cab).transform.rotation = cab.rotation;
            var cuerpo = Caja("Cuerpo", cab.TransformPoint(0f, -3.2f, 0f), new Vector3(2.2f, 2.3f, 2.6f), "mat_teleferico", false, 0f, cab);
            cuerpo.transform.rotation = cab.rotation;
            var vent = Caja("Ventanas", cab.TransformPoint(0f, -2.9f, 0f), new Vector3(2.24f, 0.8f, 2.3f), i == 2 ? "mat_ventana_cabina" : "mat_vidrio", false, 0f, cab);
            vent.transform.rotation = cab.rotation;
        }
        Luz("Cabina_Luz", Vector3.Lerp(a, b, 0.45f) + lado - Vector3.up * 3f, new Color(1f, 0.85f, 0.6f), 2.5f, 8f, luces, LuzParpadeante.Modo.Falla);
        // Torre
        float tt = 0.6875f;
        Vector3 top = Vector3.Lerp(a, b, tt);
        Caja("Torre", new Vector3(top.x, top.y * 0.5f, top.z), new Vector3(1.6f, top.y, 1.6f), "mat_metal_pintado", false, 30f, raiz);
        Caja("Torre_Cruceta", top + Vector3.up * 0.3f, new Vector3(4f, 0.5f, 0.8f), "mat_metal_oscuro", false, 30f, raiz);
    }

    private static void Altares(Transform r, Transform luces)
    {
        Altar("Altar_Deposito", new Vector3(-28.4f, 0f, -2.2f), 90f, "Depósito de la discoteca", r, luces);
        Altar("Altar_Farmacia", new Vector3(17.2f, 0f, -4.6f), 0f, "Farmacia Chuquiago", r, luces);
        Altar("Altar_Mercado", new Vector3(6.2f, 0f, 64.6f), 90f, "Pasillo del mercado", r, luces);
        Altar("Altar_Patio", new Vector3(26.7f, 0f, 76.6f), 90f, "Patio del mercado", r, luces);
    }

    private static void IllasYArchivos(Transform r, Transform luces)
    {
        Illa(0, new Vector3(-12.6f, 0.52f, 2.4f), r, luces);
        Illa(1, new Vector3(-34.8f, 1.42f, -15.5f), r, luces);
        Illa(2, new Vector3(23f, 0.66f, 36f), r, luces);
        Illa(3, new Vector3(22.4f, 1.82f, 7f), r, luces);
        Illa(4, new Vector3(-5.8f, 0.17f, 30f), r, luces);
        Illa(5, new Vector3(12f, 0.92f, 71.8f), r, luces);
        Illa(6, new Vector3(38.5f, 1.62f, 77.5f), r, luces);
        Illa(7, new Vector3(30.6f, 3.42f, 90.5f), r, luces);

        Doc("Celular_Vecinos", new Vector3(-32.4f, 0.02f, 18f), "Chat \"Sopocachi Unidos\"", TextoChatVecinos, r, true);
        Doc("Periodico", new Vector3(29f, 0.65f, 26f), "El Chasqui Paceño", TextoPeriodico, r);
        Doc("Orden_Militar", new Vector3(-2.6f, 0.02f, 55.2f), "Orden del Comando Departamental", TextoOrdenMilitar, r);
        Doc("Receta_Bety", new Vector3(36.5f, 0.02f, 57.8f), "Recetario de Mama Bety", TextoRecetaBety, r);
        Doc("Nota_Cirilo", new Vector3(39.3f, 0.02f, 60.3f), "Nota en la cámara frigorífica", TextoNotaCirilo, r);
        var nota = Doc("Nota_Estacion", new Vector3(52f, 7.52f, 103.2f), "Nota pegada en un poste", TextoNotaEstacion, r);
        nota.alLeer = new Acciones { subtitulos = new[] { "Mateo|Rosario... la mamá de Wara. Ojalá la encuentre." } };
    }

    private static void Materiales(Transform r)
    {
        var alc1 = Suministro("Singani_Barra", new Vector3(-20.6f, 1.15f, 2.05f), Recogible.TipoObjeto.Alcohol, 1, r, "Botella de singani");
        alc1.alRecoger = new Acciones { mensaje = "Alcohol + trapo = venda o molotov.  Mantén [Tab] para fabricar" };
        Suministro("Trapo_VIP", new Vector3(-9.3f, 0.72f, 0.4f), Recogible.TipoObjeto.Trapo, 1, r, "Trapo de aguayo");
        var cinta = Suministro("Cinta_Callejon", new Vector3(-31f, 0.62f, -6f), Recogible.TipoObjeto.Cinta, 1, r);
        cinta.alRecoger = new Acciones { mensaje = "Cinta + cuchilla = punta: te salva de un agarre y abre candados" };
        Suministro("Cuchilla_Callejon", new Vector3(-35.2f, 0.52f, 19f), Recogible.TipoObjeto.Cuchilla, 1, r, "Hoja de afeitar");
        Suministro("Trapo_Avenida", new Vector3(-4.6f, 0.17f, 26f), Recogible.TipoObjeto.Trapo, 1, r, "Aguayo roto");
        Suministro("Alcohol_Farmacia", new Vector3(18.4f, 1.07f, 2.3f), Recogible.TipoObjeto.Alcohol, 1, r, "Alcohol medicinal");
        Suministro("Bisturi_Farmacia", new Vector3(13.8f, 0.78f, -2.5f), Recogible.TipoObjeto.Cuchilla, 1, r, "Bisturí");
        Suministro("Cinta_Farmacia", new Vector3(22.4f, 1.82f, 5.2f), Recogible.TipoObjeto.Cinta, 1, r, "Esparadrapo");
        Suministro("Piedras_Jardin_1", new Vector3(13.5f, 0.32f, 20f), Recogible.TipoObjeto.Piedras, 5, r);
        Suministro("Piedras_Jardin_2", new Vector3(36.2f, 0.32f, 38.5f), Recogible.TipoObjeto.Piedras, 5, r);
        Suministro("Piedras_Avenida", new Vector3(-1f, 0.02f, 33f), Recogible.TipoObjeto.Piedras, 4, r);
        Suministro("Alcohol_Mercado", new Vector3(20.4f, 0.02f, 64.9f), Recogible.TipoObjeto.Alcohol, 1, r, "Botella de alcohol");
        Suministro("Trapo_Mercado", new Vector3(30.8f, 0.02f, 69.4f), Recogible.TipoObjeto.Trapo, 1, r);
        Suministro("Cinta_Ferreteria", new Vector3(36.3f, 1.03f, 71.4f), Recogible.TipoObjeto.Cinta, 1, r, "Cinta aislante");
        Suministro("Cuchilla_Ferreteria", new Vector3(35.6f, 1.03f, 71.9f), Recogible.TipoObjeto.Cuchilla, 1, r, "Cúter");
        Suministro("Piedras_Mercado", new Vector3(14.5f, 0.02f, 65f), Recogible.TipoObjeto.Piedras, 5, r);
        Suministro("Venda_Patio", new Vector3(30f, 0.02f, 75f), Recogible.TipoObjeto.Venda, 1, r);
        Suministro("Molotov_Patio", new Vector3(34.6f, 0.02f, 78.8f), Recogible.TipoObjeto.Molotov, 1, r, "Molotov a medio hacer");
        Suministro("Piedras_Calle", new Vector3(34f, 7.52f, 103f), Recogible.TipoObjeto.Piedras, 5, r);
        Suministro("Venda_Calle", new Vector3(26f, 7.52f, 110.5f), Recogible.TipoObjeto.Venda, 1, r);
        Suministro("Alcohol_Calle", new Vector3(40f, 7.52f, 110.8f), Recogible.TipoObjeto.Alcohol, 1, r);
        Suministro("Trapo_Calle", new Vector3(18f, 7.52f, 110.8f), Recogible.TipoObjeto.Trapo, 1, r);
    }

    private static void Casetas(Transform r)
    {
        // Caseta del guardia municipal (calle del mercado)
        Caseta("Caseta_Municipal", 34f, 36.4f, 46.4f, 48.8f, 0f, 'S', 35.2f, 180f, r);
        Suministro("Caseta_Municipal_Molotov", new Vector3(34.6f, 0.02f, 48.2f), Recogible.TipoObjeto.Molotov, 1, r);
        Suministro("Caseta_Municipal_Balas", new Vector3(35.8f, 0.02f, 48.2f), Recogible.TipoObjeto.Balas, 3, r);
        Suministro("Caseta_Municipal_Venda", new Vector3(35.2f, 0.02f, 47.6f), Recogible.TipoObjeto.Venda, 1, r);
        Graf("vivos", new Vector3(36.46f, 1.5f, 47.6f), Vector3.right, 1.6f, r);
        // Caseta policial (junto a la barricada)
        Caseta("Caseta_Policial", 5.2f, 7.6f, 53f, 55.4f, 0.15f, 'O', 54.2f, 90f, r);
        Suministro("Caseta_Policial_Balas", new Vector3(7f, 0.17f, 54.8f), Recogible.TipoObjeto.Balas, 4, r, "Caja de balas .38");
        Suministro("Caseta_Policial_Molotov", new Vector3(7f, 0.17f, 53.6f), Recogible.TipoObjeto.Molotov, 1, r);
    }

    private static void Personajes(Transform r, Transform pers, Transform ev, Transform luces)
    {
        // Revolver del sargento Choque (custodiado por los policias infectados)
        CrearNPC("Rig_Infectado", "Sargento_Choque", new Vector3(1.8f, 0.02f, 51.5f), 30f, "mat_char_policia", PoseNPC.Pose.Cadaver, false, pers, AnimProcedural.Estilo.Humano);
        Sangre(new Vector3(1.8f, -0.02f, 51.3f), 1.5f, r);
        var rev = Suministro("Revolver_Sargento", new Vector3(2.5f, 0.02f, 50.9f), Recogible.TipoObjeto.Revolver, 5, r, "Revólver .38 del sargento");
        rev.alRecoger = new Acciones
        {
            subtitulos = new[] { "Mateo|El revólver del sargento Choque... cinco balas.", "Mateo|Cada disparo va a atraer a todo el barrio. Solo si no queda otra." },
            mensaje = "[3] revólver  ·  clic derecho apuntar  ·  clic izquierdo disparar  ·  [R] recargar"
        };

        // Gritones
        var g1 = CrearInfectado("Griton_Plaza", new Vector3(30f, 0.18f, 43.2f), 200f, Infectado.Tipo.Griton, Infectado.Estado.Deambular, pers, 4f, "mat_char_griton");
        var g2 = CrearInfectado("Griton_Mercado", new Vector3(20.5f, 0f, 68f), 90f, Infectado.Tipo.Griton, Infectado.Estado.Deambular, pers, 3f, "mat_char_griton");
        foreach (var g in new[] { g1, g2 })
        {
            var e = g.GetComponentInChildren<EsqueletoHumanoide>();
            if (e != null) { Accesorio(e, EsqueletoHumanoide.Neck, PrimitiveType.Sphere, new Vector3(0f, 0f, 0.07f), new Vector3(0.17f, 0.14f, 0.14f), "mat_garganta"); }
        }
        Zona("Z_Aviso_Griton", new Vector3(26f, 1.5f, 39f), new Vector3(12f, 3f, 3f), ev, new Acciones
        {
            subtitulos = new[] { "Mateo|Ese tiene la garganta hinchada... Si grita, se me vienen todos encima." },
            mensaje = "Gritón: si te ve, alerta a todos. Mátalo primero, en silencio (sigilo o honda a la cabeza)"
        });

        // ---- El Chino, lustrabotas (trueque) ----
        var chino = CrearNPC("Rig_Infectado", "Lustrabotas_Chino", new Vector3(-6.3f, 0.15f, 33.6f), 90f, "mat_char_lustra", PoseNPC.Pose.Sentado, true, pers, AnimProcedural.Estilo.Humano);
        var ech = chino.GetComponentInChildren<EsqueletoHumanoide>();
        if (ech != null)
        {
            Accesorio(ech, EsqueletoHumanoide.Head, PrimitiveType.Sphere, new Vector3(0f, 0.09f, 0.01f), new Vector3(0.24f, 0.27f, 0.26f), "mat_pasamontanas");
            Accesorio(ech, EsqueletoHumanoide.Head, PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0.02f), new Vector3(0.26f, 0.03f, 0.26f), "mat_toldo_rojo");
        }
        Caja("Cajon_Lustrar", new Vector3(-5.8f, 0.33f, 33.6f), new Vector3(0.35f, 0.36f, 0.28f), "mat_madera", true, 90f, r);
        var cv = chino.AddComponent<Conversacion>();
        cv.nombreNPC = "El Chino";
        cv.radio = 2.4f;
        cv.altura = 0.8f;
        cv.nodos = new[]
        {
            new Conversacion.Nodo
            {
                id = "inicio", quien = "El Chino", retrato = "lustra",
                texto = "Un lustrabotas con pasamontañas está sentado en un zaguán, con su cajón entre las piernas. Tiene un fierro en la mano y no parece asustado.\n\n\"Tranquilo, joven. Aquí nadie muerde... todavía. Me dicen el Chino. ¿Hacemos trueque? La plata ya no vale nada.\"",
                opciones = new[]
                {
                    new Conversacion.Opcion { texto = "Trueque: 2 botellas por trapo y alcohol", costo = "botellas:2", dar = "trapo:1,alcohol:1", unaVez = true, siguiente = "inicio" },
                    new Conversacion.Opcion { texto = "Trueque: 1 pila por cinta y cuchilla", costo = "pilas:1", dar = "cinta:1,cuchilla:1", unaVez = true, siguiente = "inicio" },
                    new Conversacion.Opcion { texto = "Trueque: 1 venda por 8 piedras", costo = "vendas:1", dar = "piedras:8", unaVez = true, siguiente = "inicio" },
                    new Conversacion.Opcion { texto = "\"Estás herido. Toma una venda.\"", costo = "vendas:1", dar = "cuchilla:1", moral = 8, unaVez = true, ponerFlag = "ayuda_chino", siguiente = "gracias", acciones = new Acciones { flag = "ayuda_chino" } },
                    new Conversacion.Opcion { texto = "\"¿Qué sabes de lo que está pasando?\"", siguiente = "info" },
                    new Conversacion.Opcion { texto = "\"Me voy. Cuídate, Chino.\"" }
                }
            },
            new Conversacion.Nodo
            {
                id = "gracias", quien = "El Chino", retrato = "lustra",
                texto = "El Chino se venda la mano despacio, sin decir nada. Luego te pasa un cúter.\n\n\"Dios le pague, joven. En esta ciudad todavía hay gente. Tome, para que se defienda. Y ojo con los que hacen clic: esos no ven, pero oyen hasta tu corazón.\"",
                opciones = new[] { new Conversacion.Opcion { texto = "[Continuar]", siguiente = "inicio" } }
            },
            new Conversacion.Nodo
            {
                id = "info", quien = "El Chino", retrato = "lustra",
                texto = "\"Unos dicen que es el agua del Choqueyapu. Otros que es castigo de la Pachamama. Yo lustro zapatos en esta esquina hace veinte años y nunca vi a la policía correr así.\n\nLo que sé seguro: a los que tienen la garganta hinchada, mátalos primero y calladito. Gritan y se viene todo el barrio. Y en la barricada quedó el sargento Choque con su revólver... pero sus propios muchachos lo cuidan.\"",
                opciones = new[] { new Conversacion.Opcion { texto = "\"Gracias, Chino.\"", siguiente = "inicio" } }
            }
        };

        // ---- Wara, la cebra ----
        var wara = new GameObject("Wara");
        wara.transform.SetParent(pers, false);
        wara.transform.SetPositionAndRotation(new Vector3(17.2f, 0.18f, 46.7f), Quaternion.Euler(0f, 200f, 0f));
        var ag = wara.AddComponent<NavMeshAgent>();
        ag.radius = 0.25f;
        ag.height = 1.6f;
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arte/Prefabs/Rig_Infectado.prefab"));
        rig.transform.SetParent(wara.transform, false);
        rig.transform.localScale = Vector3.one * 0.93f;
        foreach (var smr in rig.GetComponentsInChildren<SkinnedMeshRenderer>()) { smr.sharedMesh = MallaCebra(smr.sharedMesh); smr.sharedMaterial = Mat("mat_cebra"); }
        rig.GetComponent<AnimProcedural>().estilo = AnimProcedural.Estilo.Humano;
        var ew = rig.GetComponent<EsqueletoHumanoide>();
        if (ew != null)
        {
            Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Sphere, new Vector3(0f, 0.1f, -0.01f), new Vector3(0.25f, 0.26f, 0.27f), "mat_cebra");
            Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Cube, new Vector3(0f, 0.16f, 0.14f), new Vector3(0.12f, 0.09f, 0.17f), "mat_cebra", new Vector3(12f, 0f, 0f));
            Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Capsule, new Vector3(0.08f, 0.27f, -0.02f), new Vector3(0.05f, 0.07f, 0.03f), "mat_cebra", new Vector3(0f, 0f, -15f));
            Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Capsule, new Vector3(-0.08f, 0.27f, -0.02f), new Vector3(0.05f, 0.07f, 0.03f), "mat_cebra", new Vector3(0f, 0f, 15f));
            Accesorio(ew, EsqueletoHumanoide.Head, PrimitiveType.Cube, new Vector3(0f, 0.19f, -0.13f), new Vector3(0.04f, 0.16f, 0.05f), "mat_negro");
        }
        var comp = wara.AddComponent<Companera>();
        comp.escondite = new Vector3(12.6f, 0f, 54.6f);
        comp.destinoSeparacion = new Vector3(54f, 7.5f, 106f);
        var cw = wara.AddComponent<Conversacion>();
        cw.nombreNPC = "Wara";
        cw.radio = 2.6f;
        cw.altura = 0.9f;
        cw.soloCharlasDespues = true;
        var unirse = new Acciones
        {
            companera = 1,
            flag = "wara_unida",
            objetivo = "Consigue la llave del mercado en la farmacia (cruzando la plaza, al sur)",
            usarMarcador = true, marcador = new Vector3(19f, 0f, 9f)
        };
        cw.nodos = new[]
        {
            new Conversacion.Nodo
            {
                id = "inicio", quien = "Wara", retrato = "wara",
                texto = "Detrás del kiosco hay una chica con traje de cebra —de esas que ayudan a cruzar la calle en La Paz—, agachada, con una honda de lana tensa en la mano. Te apunta a la cara.\n\n\"¡Quieto! ¿Estás mordido? ¡Muéstrame los brazos!\"",
                opciones = new[]
                {
                    new Conversacion.Opcion { texto = "\"Tranquila, estoy bien. Me llamo Mateo.\"", siguiente = "presentacion" },
                    new Conversacion.Opcion { texto = "\"Baja eso. No tengo tiempo para esto.\"", moral = -2, siguiente = "presentacion" }
                }
            },
            new Conversacion.Nodo
            {
                id = "presentacion", quien = "Wara", retrato = "wara",
                texto = "Wara baja la honda. Le tiemblan las manos.\n\n\"Wara. Soy cebra, trabajo en el cruce de la 20 de Octubre... trabajaba. Estaba de turno en la noche cuando empezó todo. La radio dice que nos quedemos adentro, pero mi mamá está en la Ceja y yo aquí sola. ¿Tú a dónde vas?\"",
                opciones = new[]
                {
                    new Conversacion.Opcion
                    {
                        texto = "\"Donde doña Bety, arriba de las gradas del mercado. Vente conmigo.\"",
                        dar = "honda:8", moral = 3,
                        respuesta = "Wara|Ya. Toma, tengo otra honda. Mi abuelo me enseñó en Achacachi: apunta a la cabeza.\nWara|El mercado está cerrado. La doctora Julia de la farmacia tenía la llave, ella lo abría temprano.",
                        acciones = unirse
                    },
                    new Conversacion.Opcion { texto = "\"Es peligroso. Mejor escóndete aquí.\"", siguiente = "insiste" }
                }
            },
            new Conversacion.Nodo
            {
                id = "insiste", quien = "Wara", retrato = "wara",
                texto = "\"¿Y quedarme sola con esas cosas? Ni loca. Te acompaño hasta las gradas, yo sé por dónde se puede pasar.\"",
                opciones = new[]
                {
                    new Conversacion.Opcion
                    {
                        texto = "\"Está bien. Pero haces lo que te digo.\"", dar = "honda:8",
                        respuesta = "Wara|Toma, tengo otra honda. Apunta a la cabeza.\nWara|La llave del mercado la tenía la doctora de la farmacia.",
                        acciones = unirse
                    }
                }
            },
            new Conversacion.Nodo
            {
                id = "monumento", quien = "Wara", retrato = "wara",
                texto = "Wara se queda mirando la estatua de Eduardo Abaroa, negra de hollín.\n\n\"¿Sabías que cuando le pidieron rendirse dijo '¿rendirme yo? ¡que se rinda su abuela!'? Mi profe de historia lo repetía siempre en el colegio.\"",
                opciones = new[]
                {
                    new Conversacion.Opcion { texto = "\"Nosotros tampoco nos vamos a rendir.\"", moral = 2, respuesta = "Wara|Jallalla, pues." },
                    new Conversacion.Opcion { texto = "\"No es momento para historia, Wara.\"", respuesta = "Wara|Ya, ya... perdón." }
                }
            },
            new Conversacion.Nodo
            {
                id = "farmacia", quien = "Wara", retrato = "wara",
                texto = "\"La doctora Julia me vendía fiado cuando me enfermaba en el turno... Mateo, ¿tú crees que haya cura para esto?\"",
                opciones = new[]
                {
                    new Conversacion.Opcion { texto = "\"Tiene que haber. Doña Bety sabe de medicina kallawaya.\"", moral = 2, respuesta = "Wara|Mi abuela decía que los kallawayas curan hasta el susto." },
                    new Conversacion.Opcion { texto = "\"No lo sé.\"", respuesta = "Wara|...Yo tampoco." }
                }
            },
            new Conversacion.Nodo
            {
                id = "mercado", quien = "Wara", retrato = "wara",
                texto = "\"Aquí mi mamá compraba el chuño y la papa... Mateo, si me pasa algo, busca a mi mamá en la Ceja. Se llama Rosario. Rosario Condori.\"",
                opciones = new[]
                {
                    new Conversacion.Opcion { texto = "\"No te va a pasar nada.\"" },
                    new Conversacion.Opcion { texto = "\"Te lo prometo.\"", moral = 3, ponerFlag = "promesa_wara" }
                }
            },
            new Conversacion.Nodo
            {
                id = "despedida", quien = "Wara", retrato = "wara",
                texto = "Wara se detiene en el descanso de las gradas y mira hacia arriba: la Línea Amarilla del teleférico, detenida, con las cabinas amarillas colgando en la neblina.\n\n\"Mateo... la estación está ahí nomás. Si sigo los cables a pie, llego a la Ceja antes del mediodía. Mi mamá me está esperando.\"",
                opciones = new[]
                {
                    new Conversacion.Opcion
                    {
                        texto = "\"Anda. Toma, llévate esto.\" (le das una venda)", costo = "vendas:1", moral = 8, ponerFlag = "wara_regalo",
                        respuesta = "Wara|Gracias, Mateo. Jallalla, ¿ya? Nos vemos arriba, en El Alto.",
                        acciones = new Acciones { companera = 2 }
                    },
                    new Conversacion.Opcion { texto = "\"Ven conmigo donde Bety. Es más seguro.\"", siguiente = "despedida_no" },
                    new Conversacion.Opcion { texto = "\"Anda nomás. Suerte.\"", respuesta = "Wara|Suerte a ti también, Mateo.", acciones = new Acciones { companera = 2 } }
                }
            },
            new Conversacion.Nodo
            {
                id = "despedida_no", quien = "Wara", retrato = "wara",
                texto = "\"No puedo. Es mi mamá.\"\n\nWara te abraza fuerte y rápido, como quien se despide en la terminal de buses. Después sube corriendo, silbando como silban las cebras en el cruce.",
                opciones = new[] { new Conversacion.Opcion { texto = "\"Cuídate, cebra.\"", moral = 2, acciones = new Acciones { companera = 2 } } }
            }
        };
        cw.charlas = new[]
        {
            new Conversacion.Charla { flag = "charla_monumento", nodo = "monumento" },
            new Conversacion.Charla { flag = "charla_farmacia", nodo = "farmacia" },
            new Conversacion.Charla { flag = "charla_mercado", nodo = "mercado" }
        };

        Zona("Z_Wara_Llama", new Vector3(16f, 1.5f, 42.5f), new Vector3(9f, 3f, 5f), ev, new Acciones
        {
            subtitulos = new[] { "Wara|¡Psst! ¡Oye, tú! ¡Aquí, detrás del kiosco! ¡Agáchate!" },
            sonido = "sfx_silbato",
            mensaje = "Alguien te llama desde detrás del kiosco"
        });
        var zm = Zona("Z_Charla_Monumento", new Vector3(26f, 1.5f, 27f), new Vector3(8f, 3f, 3f), ev, new Acciones { flag = "charla_monumento", mensaje = "Wara quiere decirte algo  ·  [E] conversar" });
        zm.requiereCompanera = true;
        var zf = Zona("Z_Charla_Farmacia", new Vector3(19f, 1.5f, 12f), new Vector3(6f, 3f, 3f), ev, new Acciones { flag = "charla_farmacia", mensaje = "Wara quiere decirte algo  ·  [E] conversar" });
        zf.requiereCompanera = true;
        zf.requiereObjeto = "llave_mercado";
        var zme = Zona("Z_Charla_Mercado", new Vector3(7.5f, 1.5f, 58.5f), new Vector3(5f, 3f, 3f), ev, new Acciones { flag = "charla_mercado", mensaje = "Wara quiere decirte algo  ·  [E] conversar" });
        zme.requiereCompanera = true;
        var zfar = Zona("Z_Wara_Farmacia", new Vector3(19f, 1.5f, 7f), new Vector3(4f, 3f, 3f), ev, new Acciones { subtitulos = new[] { "Wara|Yo te cuido la puerta. Si viene alguno, te silbo." } });
        zfar.requiereCompanera = true;
        var zmer = Zona("Z_Wara_Mercado", new Vector3(13f, 1.5f, 62f), new Vector3(3f, 3f, 4f), ev, new Acciones { subtitulos = new[] { "Wara|Ese olor... viene de la carnicería de don Cirilo. Tenía una cámara frigorífica al fondo." } });
        zmer.requiereCompanera = true;
        var zdes = Zona("Z_Wara_Despedida", new Vector3(32f, 4.9f, 90f), new Vector3(4f, 3f, 2f), ev, new Acciones { conversacion = cw, nodoConversacion = "despedida" });
        zdes.requiereCompanera = true;

        // Freddy: flags para el logro Ayni / ladron
        EleccionMoral freddy = Buscar<EleccionMoral>("Sobreviviente_Freddy");
        if (freddy != null && freddy.opciones != null && freddy.opciones.Length >= 3)
        {
            freddy.opciones[0].acciones = new Acciones { flag = "ayuda_freddy" };
            freddy.opciones[2].acciones = new Acciones { logro = "ladron" };
            freddy.opciones[2].dar = "alcohol:1";
        }

        // Radio del mercado
        Sonido("Radio_Mercado", new Vector3(31f, 1f, 63.5f), "amb_radio", 0.45f, 10f, r);
        Zona("Z_Radio_Mercado", new Vector3(31f, 1.5f, 63.5f), new Vector3(6f, 3f, 5f), ev, new Acciones
        {
            subtitulos = new[]
            {
                "Radio|...Radio Altiplano FM, seguimos informando: el SEDES confirma que la fiebre se contagia por mordeduras y rasguños...",
                "Radio|...se pide a los vecinos de Sopocachi y la zona Sur dirigirse al estadio Hernando Siles, donde el Ejército instaló un refugio...",
                "Radio|...hermano, si me escuchas: NO vayas al estadio. No vayas al est... [estática]"
            }
        });
    }

    private static void Jefe(Transform r, Transform pers, Transform ev, Transform luces)
    {
        // Camara frigorifica (detras de la pared este del mercado)
        Cuarto("Camara_Frigorifica", 40f, 45.5f, 58f, 66f, 0f, 4f, "mat_baldosa", "mat_baldosa", "mat_concreto", SinHuecos, SinHuecos, SinHuecos, null, true, 0.3f);
        for (int i = 0; i < 4; i++)
        {
            Caja("Res_Colgada_" + i, new Vector3(42f + (i % 2) * 2f, 2.3f, 59.5f + i * 1.6f), new Vector3(0.45f, 1.3f, 0.3f), "mat_mandil_sangre", false, i * 20f);
            Caja("Gancho_" + i, new Vector3(42f + (i % 2) * 2f, 3.4f, 59.5f + i * 1.6f), new Vector3(0.03f, 0.9f, 0.03f), "mat_fierro", false);
        }
        Sangre(new Vector3(42.5f, 0f, 62f), 2.2f, r);
        Sangre(new Vector3(40.8f, 0f, 61.5f), 1f, r);
        Luz("Camara_Luz", new Vector3(43f, 3.6f, 62f), new Color(0.5f, 0.7f, 1f), 1.4f, 7f, luces, LuzParpadeante.Modo.Falla);

        var puerta = Puerta("Puerta_Camara", new Vector3(40f, 0f, 62f), 90f, 2.2f, 2.8f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", r);
        puerta.trabada = true;
        puerta.angulo = -110f; // abre hacia adentro de la camara: no tapa el pasillo del mercado
        puerta.mensajeTrabada = "La cámara frigorífica de la carnicería. Algo golpea desde adentro... No voy a abrir eso.";
        puerta.ruidoAlAbrir = 12f;
        puerta.sonido = "sfx_golpe_suelo";
        Zona("Z_Camara_Golpes", new Vector3(36.5f, 1.5f, 62f), new Vector3(4f, 3f, 6f), ev, new Acciones { sonido = "sfx_golpe_suelo", subtitulos = new[] { "Mateo|Algo está golpeando la puerta de la cámara... desde adentro." } });

        var jefe = CrearInfectado("El_Carnicero", new Vector3(43.2f, 0f, 62f), 270f, Infectado.Tipo.Carnicero, Infectado.Estado.Quieto, pers, 1f, "mat_char_carnicero");
        var e = jefe.GetComponentInChildren<EsqueletoHumanoide>();
        if (e != null)
        {
            e.transform.localScale = Vector3.one * 1.45f;
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Cube, new Vector3(0f, -0.2f, 0.14f), new Vector3(0.42f, 0.75f, 0.05f), "mat_mandil");
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Cube, new Vector3(0.05f, -0.1f, 0.17f), new Vector3(0.25f, 0.3f, 0.02f), "mat_mandil_sangre");
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Sphere, new Vector3(0f, 0.02f, -0.2f), new Vector3(0.42f, 0.46f, 0.3f), "mat_char_fungico");
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, -0.26f), new Vector3(0.18f, 0.16f, 0.12f), "mat_char_fungico");
            Accesorio(e, EsqueletoHumanoide.Head, PrimitiveType.Sphere, new Vector3(0.06f, 0.14f, 0.02f), new Vector3(0.12f, 0.06f, 0.1f), "mat_char_fungico");
            if (e.puntoArma != null)
            {
                var cuchillo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(cuchillo.GetComponent<Collider>());
                cuchillo.name = "Cuchillo_Carnicero";
                cuchillo.transform.SetParent(e.puntoArma, false);
                cuchillo.transform.localPosition = new Vector3(0f, 0f, 0.22f);
                cuchillo.transform.localScale = new Vector3(0.02f, 0.14f, 0.4f);
                cuchillo.GetComponent<Renderer>().sharedMaterial = Mat("mat_fierro");
            }
        }
        var col = jefe.GetComponent<CapsuleCollider>();
        if (col != null) { col.radius = 0.6f; col.height = 2.6f; col.center = new Vector3(0f, 1.3f, 0f); }
        jefe.nombreJefe = "EL CARNICERO";
        jefe.subtituloJefe = "Don Cirilo, puesto 7 del Mercado Sopocachi";

        var machete = Suministro("Machete_Carnicero", jefe.transform.position, Recogible.TipoObjeto.Machete, 1, r, "Machete de don Cirilo");
        machete.gameObject.SetActive(false);
        machete.alRecoger = new Acciones { mensaje = "Machete: el arma cuerpo a cuerpo más fuerte" };
        jefe.soltarAlMorir = machete.gameObject;

        Infectado emb1 = Buscar<Infectado>("Corredor_Mercado");
        Infectado emb2 = Buscar<Infectado>("Infectado_Mercado_Emboscada");
        jefe.refuerzos = new[] { emb1, emb2 };
        jefe.alFase2 = new Acciones { subtitulos = new[] { "Wara|¡Está llamando a los otros! ¡Cuidado, Mateo!" } };

        PuertaCap lateral = Buscar<PuertaCap>("Puerta_Lateral_Mercado");
        PuertaCap trasera = Buscar<PuertaCap>("Puerta_Trasera_Mercado");
        Recogible corta = Buscar<Recogible>("Cortafierro");

        var cine = new GameObject("Cine_Carnicero");
        cine.transform.SetParent(ev, false);
        var cc = cine.AddComponent<CinematicaCamara>();
        cc.tomas = new[]
        {
            new CinematicaCamara.Toma { desde = new Vector3(30f, 2.3f, 67f), hasta = new Vector3(33f, 2f, 65f), mirarDesde = new Vector3(40f, 1.4f, 62f), mirarHasta = new Vector3(40f, 1.6f, 62f), duracion = 2.4f, fov = 50f, subtitulo = "Mateo|¿Qué fue eso...?", sonido = "sfx_golpe_suelo" },
            new CinematicaCamara.Toma { desde = new Vector3(36.5f, 1.7f, 60.2f), hasta = new Vector3(36f, 1.5f, 60.6f), mirarDesde = new Vector3(42f, 2.2f, 62f), mirarHasta = new Vector3(41f, 2.8f, 62f), duracion = 2.6f, fov = 45f, subtitulo = "Wara|¡Es don Cirilo, el carnicero! ¡Yo me escondo, tú corre!", sonido = "sfx_rugido" },
            new CinematicaCamara.Toma { desde = new Vector3(19f, 5.2f, 56f), hasta = new Vector3(21f, 4.6f, 57f), mirarDesde = new Vector3(38f, 1.5f, 62f), mirarHasta = new Vector3(36f, 1.5f, 63f), duracion = 2f, fov = 55f }
        };

        if (corta != null)
        {
            corta.alRecoger = new Acciones
            {
                abrirPuerta = puerta,
                trabarPuertas = new[] { lateral, trasera },
                companera = 3,
                puntoControl = true,
                cinematica = cc,
                iniciarJefe = jefe,
                objetivo = "¡Sobrevive a El Carnicero!",
                mensaje = "Hazlo chocar contra los puestos cuando embista y pégale mientras está aturdido"
            };
        }
        if (lateral != null) { lateral.mensajeTrabada = "¡Los puestos caídos bloquean la puerta! No puedo huir."; }
        if (trasera != null) { trasera.mensajeTrabada = "¡Está trabada! Algo se cayó del otro lado."; }

        jefe.alMorir = new Acciones
        {
            destrabarPuertas = new[] { lateral, trasera },
            companera = 4,
            puntoControl = true,
            objetivo = "Sal por la puerta trasera del mercado y corta la cadena de la reja",
            usarMarcador = true, marcador = new Vector3(32f, 0f, 79.5f),
            subtitulos = new[] { "Mateo|...Se acabó. Perdón, don Cirilo.", "Wara|¡Mateo! ¡Lo hiciste! ¡Vamos, antes de que vengan más!" }
        };
    }

    private static void Cinematicas(Transform ev)
    {
        // Llegada a la avenida en llamas
        var z = Buscar<ZonaEvento>("Z_Llegada_Avenida");
        if (z != null)
        {
            var go = new GameObject("Cine_Avenida");
            go.transform.SetParent(ev, false);
            var c = go.AddComponent<CinematicaCamara>();
            c.tomas = new[]
            {
                new CinematicaCamara.Toma { desde = new Vector3(-7f, 3.5f, 22f), hasta = new Vector3(-3f, 5.5f, 23f), mirarDesde = new Vector3(0f, 1.5f, 42f), mirarHasta = new Vector3(4f, 1.5f, 36f), duracion = 4f, fov = 55f },
                new CinematicaCamara.Toma { desde = new Vector3(3f, 7f, 27f), hasta = new Vector3(6f, 7.5f, 30f), mirarDesde = new Vector3(26f, 4f, 31f), mirarHasta = new Vector3(26f, 3.5f, 33f), duracion = 3.2f, fov = 50f, subtitulo = "Mateo|La Plaza Abaroa... y el mercado está justo detrás." }
            };
            z.acciones.cinematica = c;
        }
        // Tope de las gradas: teleferico detenido y Bety en el balcon
        var zt = Buscar<ZonaEvento>("Z_Tope_Gradas");
        if (zt != null)
        {
            var go = new GameObject("Cine_Gradas");
            go.transform.SetParent(ev, false);
            var c = go.AddComponent<CinematicaCamara>();
            c.tomas = new[]
            {
                new CinematicaCamara.Toma { desde = new Vector3(32f, 9f, 100f), hasta = new Vector3(31.5f, 9.6f, 98.5f), mirarDesde = new Vector3(33f, 30f, 101f), mirarHasta = new Vector3(30f, 40f, 101f), duracion = 2.6f, fov = 55f, subtitulo = "Mateo|Hasta el teleférico está parado..." },
                new CinematicaCamara.Toma { desde = new Vector3(28.5f, 9.4f, 104f), hasta = new Vector3(27.5f, 9.8f, 105f), mirarDesde = new Vector3(24f, 11.4f, 111f), mirarHasta = new Vector3(24f, 11.8f, 111f), duracion = 2.4f, fov = 45f }
            };
            zt.acciones.cinematica = c;
        }
        var asedio = Object.FindFirstObjectByType<EventoAsedio>(FindObjectsInactive.Include);
        if (asedio != null && asedio.oleadas != null && asedio.oleadas.Length > 0)
        {
            asedio.oleadas[0].tiempo = 6f;
            if (asedio.oleadas.Length > 2) { asedio.oleadas[2].aviso = "Bety|¡Ya casi, hijito! ¡La cadena está dura!"; }
            if (asedio.oleadas.Length > 4) { asedio.oleadas[4].aviso = "Bety|¡Un ratito más! ¡Ya la estoy cortando!"; }
        }
    }

    private static void MarcarZona(string nombre, Vector3 m)
    {
        var z = Buscar<ZonaEvento>(nombre);
        if (z == null || z.acciones == null) { return; }
        z.acciones.usarMarcador = true;
        z.acciones.marcador = m;
    }

    private static void Marcadores()
    {
        if (juego != null && juego.alEmpezar != null) { juego.alEmpezar.usarMarcador = true; juego.alEmpezar.marcador = new Vector3(-19.6f, 1.2f, 2f); }
        var cel = Buscar<Documento>("Celular_Mateo");
        if (cel != null) { cel.alLeer.usarMarcador = true; cel.alLeer.marcador = new Vector3(-26f, 0f, 0f); }
        MarcarZona("Z_DepositoCerrado", new Vector3(-28.4f, 0f, -14.4f));
        var llaveDep = Buscar<Recogible>("Llave_Deposito");
        if (llaveDep != null) { llaveDep.alRecoger.usarMarcador = true; llaveDep.alRecoger.marcador = new Vector3(-30f, 0f, 2f); }
        MarcarZona("Z_Callejon_Inicio", new Vector3(-10f, 0f, 25f));
        MarcarZona("Z_Llegada_Avenida", new Vector3(25f, 0f, 50f));
        MarcarZona("Z_Mercado_Frente", new Vector3(8.5f, 0f, 62f));
        MarcarZona("Z_Mercado_Lateral", new Vector3(19f, 0f, 9f));
        MarcarZona("Z_Mercado_Entrada", new Vector3(32f, 0f, 73.5f));
        MarcarZona("Z_Patio_SinCortafierro", new Vector3(37.6f, 1f, 71.5f));
        var llaveMer = Buscar<Recogible>("Llave_Mercado");
        if (llaveMer != null) { llaveMer.alRecoger.usarMarcador = true; llaveMer.alRecoger.marcador = new Vector3(8.5f, 0f, 62f); }
        var reja = Buscar<PuertaCap>("Reja_Gradas");
        if (reja != null) { reja.alAbrir.usarMarcador = true; reja.alAbrir.marcador = new Vector3(32f, 7.5f, 104f); }
        var asedio = Object.FindFirstObjectByType<EventoAsedio>(FindObjectsInactive.Include);
        if (asedio != null) { asedio.alTerminar.usarMarcador = true; asedio.alTerminar.marcador = new Vector3(28f, 7.5f, 114f); }
    }

    private static void Tutoriales(Transform ev)
    {
        Zona("Z_Tutorial_Salto", new Vector3(-18.5f, 1.5f, -8f), new Vector3(5f, 3f, 7f), ev, new Acciones
        {
            mensaje = "[Espacio] saltar: sube a la tarima, mostradores y cajas bajas  ·  [Alt] esquivar"
        });
        Zona("Z_Tutorial_Escucha", new Vector3(-33f, 1.5f, 5.5f), new Vector3(6f, 3f, 2f), ev, new Acciones
        {
            mensaje = "Mantén [Z] para escuchar: verás a los infectados a través de las paredes"
        });
        var ataque = Buscar<ZonaEvento>("Z_GuardiaAtaca");
        if (ataque != null) { ataque.acciones.mensaje = "Clic izq: golpear  ·  Clic der: empujar  ·  Alt: esquivar  ·  H: curarte"; }
        Zona("Z_Tutorial_Mapa", new Vector3(-10f, 1.5f, 20f), new Vector3(4f, 3f, 4f), ev, new Acciones
        {
            mensaje = "[M] mapa  ·  Los altares con velas guardan la partida"
        });
    }

    // =====================================================================
    // MODO SUPERVIVENCIA
    // =====================================================================
    private static void Supervivencia(Transform nivel, Transform pers, Transform ev)
    {
        var go = new GameObject("ModoSupervivencia");
        var sup = go.AddComponent<ModoSupervivencia>();
        sup.inicioJugador = new Vector3(26f, 0.25f, 24f);
        sup.rotJugador = 0f;

        var muros = new GameObject("Arena_Muros");
        muros.transform.SetParent(go.transform, false);
        System.Action<string, Vector3, Vector3> muro = (n, c, t) =>
        {
            var m = new GameObject(n);
            m.transform.SetParent(muros.transform, false);
            m.transform.position = c;
            m.AddComponent<BoxCollider>().size = t;
        };
        muro("Muro_S", new Vector3(17.25f, 2f, 11f), new Vector3(45.5f, 4f, 0.4f));
        muro("Muro_N", new Vector3(17.25f, 2f, 49f), new Vector3(45.5f, 4f, 0.4f));
        muro("Muro_O", new Vector3(-5.5f, 2f, 30f), new Vector3(0.4f, 4f, 38.4f));
        muro("Muro_E", new Vector3(40f, 2f, 30f), new Vector3(0.4f, 4f, 38.4f));
        muros.SetActive(false);
        sup.muros = muros;

        Vector3[] sp = { new Vector3(-3f, 0f, 12.5f), new Vector3(-3f, 0f, 47.5f), new Vector3(38.5f, 0f, 12.5f), new Vector3(38.5f, 0f, 47.5f), new Vector3(24f, 0f, 47.8f), new Vector3(24f, 0f, 12.3f), new Vector3(4f, 0f, 30f) };
        sup.spawns = new Transform[sp.Length];
        for (int i = 0; i < sp.Length; i++)
        {
            var s = new GameObject("Spawn_" + i).transform;
            s.SetParent(go.transform, false);
            s.position = sp[i];
            sup.spawns[i] = s;
        }
        Vector3[] su = { new Vector3(22.4f, 1.02f, 29.2f), new Vector3(29.6f, 1.02f, 29.2f), new Vector3(22.4f, 1.02f, 32.8f), new Vector3(29.6f, 1.02f, 32.8f), new Vector3(16f, 0.32f, 23f), new Vector3(34f, 0.32f, 23f), new Vector3(16f, 0.32f, 40.5f), new Vector3(34f, 0.32f, 40.5f) };
        sup.suministros = new Transform[su.Length];
        for (int i = 0; i < su.Length; i++)
        {
            var s = new GameObject("Suministro_" + i).transform;
            s.SetParent(go.transform, false);
            s.position = su[i];
            sup.suministros[i] = s;
        }

        var plant = new GameObject("_Plantillas").transform;
        plant.SetParent(go.transform, false);
        sup.plantillas = new[]
        {
            CrearInfectado("Plantilla_Comun", new Vector3(26f, 0.18f, 20f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, plant, 5f, null, false),
            CrearInfectado("Plantilla_Corredor", new Vector3(26f, 0.18f, 20f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Deambular, plant, 5f, null, false),
            CrearInfectado("Plantilla_Fungico", new Vector3(26f, 0.18f, 20f), 0f, Infectado.Tipo.Fungico, Infectado.Estado.Deambular, plant, 5f, null, false),
            CrearInfectado("Plantilla_Griton", new Vector3(26f, 0.18f, 20f), 0f, Infectado.Tipo.Griton, Infectado.Estado.Deambular, plant, 5f, "mat_char_griton", false)
        };
        var eg = sup.plantillas[3].GetComponentInChildren<EsqueletoHumanoide>(true);
        if (eg != null) { Accesorio(eg, EsqueletoHumanoide.Neck, PrimitiveType.Sphere, new Vector3(0f, 0f, 0.07f), new Vector3(0.17f, 0.14f, 0.14f), "mat_garganta"); }
        sup.desactivar = new[] { ev.gameObject };
        if (juego != null) { juego.supervivencia = sup; }
    }
}
