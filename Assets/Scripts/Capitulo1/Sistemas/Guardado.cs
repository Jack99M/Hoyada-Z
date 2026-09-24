using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>Todo lo necesario para continuar una partida del modo historia.</summary>
[System.Serializable]
public class DatosPartida
{
    public int version = 2;
    public int dificultad;
    public string fecha;
    public string lugar;
    public float px, py, pz, ry;
    public int salud;
    public int moral;
    public float minuto;
    public float cielo;
    public string objetivo;
    public bool hayMarcador;
    public float mx, my, mz;
    public DatosInventario inv;
    public DatosStats stats;
    public int companera;
    public List<string> recogidos = new List<string>();
    public List<string> leidos = new List<string>();
    public List<string> puertas = new List<string>();
    public List<string> zonas = new List<string>();
    public List<string> muertos = new List<string>();
    public List<string> charlas = new List<string>();
    public List<string> asedios = new List<string>();
    public List<string> flags = new List<string>();
    public List<string> mapa = new List<string>();
}

/// <summary>
/// Guardado persistente (PlayerPrefs, JSON). Cada objeto del nivel se identifica por su
/// ruta en la jerarquia, asi que la escena construida por el editor siempre coincide.
/// Al cargar se restauran: posicion, salud, inventario, metricas, objetos recogidos,
/// archivos leidos, puertas, eventos, infectados muertos, conversaciones y la compañera.
/// </summary>
public static class Guardado
{
    private const string Clave = "hz_guardado_historia";

    public static bool Existe { get { return PlayerPrefs.HasKey(Clave); } }

    public static string Resumen
    {
        get
        {
            DatosPartida d = Leer();
            if (d == null) { return ""; }
            return d.lugar + "  ·  " + Dificultad.Nombre((NivelDificultad)d.dificultad) + "  ·  " + Stats.TiempoTexto(d.stats != null ? d.stats.tiempo : 0f) + "  ·  " + d.fecha;
        }
    }

    public static string Id(Component c)
    {
        var sb = new StringBuilder();
        Transform t = c.transform;
        while (t != null)
        {
            int k = 0;
            Transform p = t.parent;
            if (p != null)
            {
                for (int i = 0; i < p.childCount; i++)
                {
                    Transform h = p.GetChild(i);
                    if (h == t) { break; }
                    if (h.name == t.name) { k++; }
                }
            }
            sb.Insert(0, "/" + t.name + (k > 0 ? "[" + k + "]" : ""));
            t = p;
        }
        return sb.ToString();
    }

    public static void Escribir(Juego g, string lugar)
    {
        Jugador j = g.jugador;
        var d = new DatosPartida
        {
            dificultad = (int)Dificultad.Nivel,
            fecha = System.DateTime.Now.ToString("dd/MM HH:mm"),
            lugar = lugar,
            px = j.transform.position.x, py = j.transform.position.y, pz = j.transform.position.z,
            ry = j.transform.eulerAngles.y,
            salud = j.Salud,
            moral = g.moral,
            minuto = g.minutoReloj,
            cielo = g.ProgresoCielo,
            objetivo = g.Objetivo,
            hayMarcador = g.HayMarcador,
            mx = g.Marcador.x, my = g.Marcador.y, mz = g.Marcador.z,
            inv = j.Inventario != null ? j.Inventario.Exportar() : null,
            stats = JsonUtility.FromJson<DatosStats>(JsonUtility.ToJson(Stats.D)),
            companera = Companera.I != null ? Companera.I.EstadoGuardado : 0
        };

        foreach (Recogible r in Object.FindObjectsByType<Recogible>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (r.Recogido) { d.recogidos.Add(Id(r)); }
        }
        foreach (Coleccionable c in Object.FindObjectsByType<Coleccionable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.Recogido) { d.recogidos.Add(Id(c)); }
        }
        foreach (Documento doc in Object.FindObjectsByType<Documento>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (doc.Leido) { d.leidos.Add(Id(doc)); }
        }
        foreach (PuertaCap p in Object.FindObjectsByType<PuertaCap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            d.puertas.Add(Id(p) + "|" + (p.abierta ? 1 : 0) + "|" + (p.trabada ? 1 : 0));
        }
        foreach (ZonaEvento z in Object.FindObjectsByType<ZonaEvento>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (z.Disparada) { d.zonas.Add(Id(z)); }
        }
        foreach (Infectado inf in Object.FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (inf.Muerto) { d.muertos.Add(Id(inf)); }
        }
        foreach (EleccionMoral em in Object.FindObjectsByType<EleccionMoral>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (em.Usado) { d.charlas.Add(Id(em)); }
        }
        foreach (Conversacion cv in Object.FindObjectsByType<Conversacion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            d.charlas.Add(Id(cv) + "|" + cv.Exportar());
        }
        foreach (EventoAsedio ev in Object.FindObjectsByType<EventoAsedio>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (ev.Terminado) { d.asedios.Add(Id(ev)); }
        }
        d.flags.AddRange(g.Flags);
        d.mapa.AddRange(g.ZonasVisitadas);

        PlayerPrefs.SetString(Clave, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
    }

    public static DatosPartida Leer()
    {
        if (!Existe) { return null; }
        try
        {
            return JsonUtility.FromJson<DatosPartida>(PlayerPrefs.GetString(Clave));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se pudo leer la partida guardada: " + e.Message);
            return null;
        }
    }

    public static void Borrar()
    {
        PlayerPrefs.DeleteKey(Clave);
        PlayerPrefs.Save();
    }

    public static void Aplicar(DatosPartida d, Juego g)
    {
        Dificultad.Nivel = (NivelDificultad)d.dificultad;
        Stats.D = d.stats ?? new DatosStats();

        var muertos = new HashSet<string>(d.muertos);
        foreach (Infectado inf in Object.FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!muertos.Contains(Id(inf))) { continue; }
            if (inf.soltarAlMorir != null)
            {
                inf.soltarAlMorir.transform.position = inf.transform.position + inf.transform.right * 0.5f + Vector3.up * 0.05f;
                inf.soltarAlMorir.SetActive(true);
            }
            if (inf.tipo == Infectado.Tipo.Carnicero) { inf.MarcarJefeDerrotado(); }
            else { inf.MarcarMuerto(); }
        }

        var recogidos = new HashSet<string>(d.recogidos);
        foreach (Recogible r in Object.FindObjectsByType<Recogible>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (recogidos.Contains(Id(r))) { r.MarcarRecogido(); }
        }
        foreach (Coleccionable c in Object.FindObjectsByType<Coleccionable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (recogidos.Contains(Id(c))) { c.MarcarRecogido(); }
        }
        var leidos = new HashSet<string>(d.leidos);
        foreach (Documento doc in Object.FindObjectsByType<Documento>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (leidos.Contains(Id(doc))) { doc.MarcarLeido(); }
        }
        var puertas = new Dictionary<string, string[]>();
        foreach (string s in d.puertas)
        {
            string[] p = s.Split('|');
            if (p.Length >= 3) { puertas[p[0]] = p; }
        }
        foreach (PuertaCap p in Object.FindObjectsByType<PuertaCap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string[] e;
            if (!puertas.TryGetValue(Id(p), out e)) { continue; }
            p.trabada = e[2] == "1";
            if (e[1] == "1") { p.AbrirSinEfectos(); }
        }
        var zonas = new HashSet<string>(d.zonas);
        foreach (ZonaEvento z in Object.FindObjectsByType<ZonaEvento>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (zonas.Contains(Id(z))) { z.MarcarDisparada(); }
        }
        var charlas = new Dictionary<string, string>();
        foreach (string s in d.charlas)
        {
            int i = s.IndexOf('|');
            if (i < 0) { charlas[s] = ""; }
            else { charlas[s.Substring(0, i)] = s.Substring(i + 1); }
        }
        foreach (EleccionMoral em in Object.FindObjectsByType<EleccionMoral>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (charlas.ContainsKey(Id(em))) { em.MarcarUsado(); }
        }
        foreach (Conversacion cv in Object.FindObjectsByType<Conversacion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string e;
            if (charlas.TryGetValue(Id(cv), out e)) { cv.Importar(e); }
        }
        var asedios = new HashSet<string>(d.asedios);
        foreach (EventoAsedio ev in Object.FindObjectsByType<EventoAsedio>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (asedios.Contains(Id(ev))) { ev.MarcarTerminado(); }
        }

        Jugador j = g.jugador;
        if (j != null)
        {
            j.Teletransportar(new Vector3(d.px, d.py + 0.05f, d.pz), Quaternion.Euler(0f, d.ry, 0f));
            j.FijarSalud(d.salud);
            if (j.Inventario != null) { j.Inventario.Importar(d.inv); }
        }
        g.AplicarGuardado(d.moral, d.minuto, d.cielo, d.objetivo, d.hayMarcador, new Vector3(d.mx, d.my, d.mz), d.flags, d.mapa);
        if (Companera.I != null) { Companera.I.RestaurarEstado(d.companera); }
    }
}

/// <summary>
/// Zonas del mapa (rectangulos en el plano XZ, en metros) para el mapa del HUD y para
/// registrar por donde paso Mateo.
/// </summary>
public static class MapaZonas
{
    public struct Zona
    {
        public string nombre;
        public Rect r;       // x = min X, y = min Z
        public float altura; // y del suelo aproximada
        public Color color;
        public Zona(string n, float x0, float x1, float z0, float z1, float y, Color c) { nombre = n; r = Rect.MinMaxRect(x0, z0, x1, z1); altura = y; color = c; }
    }

    public static readonly Zona[] Zonas =
    {
        new Zona("Discoteca Altura", -30f, -8f, -18f, 6f, 0f, new Color(0.55f, 0.2f, 0.5f)),
        new Zona("Callejón", -36f, -30f, -18f, 28f, 0f, new Color(0.35f, 0.35f, 0.38f)),
        new Zona("Pasaje", -30f, -8f, 22f, 28f, 0f, new Color(0.35f, 0.35f, 0.38f)),
        new Zona("Av. 20 de Octubre", -8f, 8f, -26f, 60f, 0f, new Color(0.3f, 0.3f, 0.33f)),
        new Zona("Plaza Abaroa", 8f, 40f, 16f, 46f, 0.2f, new Color(0.25f, 0.4f, 0.25f)),
        new Zona("Calle Belisario Salinas", 8f, 40f, 10f, 16f, 0f, new Color(0.3f, 0.3f, 0.33f)),
        new Zona("Farmacia Chuquiago", 12f, 26f, -6f, 10f, 0f, new Color(0.2f, 0.5f, 0.35f)),
        new Zona("Calle del mercado", 5f, 40f, 46f, 52f, 0f, new Color(0.3f, 0.3f, 0.33f)),
        new Zona("Mercado Sopocachi", 10f, 40f, 52f, 74f, 0f, new Color(0.6f, 0.3f, 0.2f)),
        new Zona("Patio del mercado", 26f, 40f, 74f, 80f, 0f, new Color(0.4f, 0.35f, 0.3f)),
        new Zona("Gradas", 30f, 34f, 80f, 100f, 3f, new Color(0.45f, 0.42f, 0.38f)),
        new Zona("Calle de doña Bety", 8f, 56f, 100f, 112f, 7.5f, new Color(0.5f, 0.4f, 0.3f)),
    };

    public static string ZonaEn(Vector3 p)
    {
        foreach (Zona z in Zonas)
        {
            if (z.r.Contains(new Vector2(p.x, p.z)) && Mathf.Abs(p.y - z.altura) < 6f) { return z.nombre; }
        }
        return null;
    }

    /// <summary>Limites totales para escalar el mapa.</summary>
    public static Rect Limites { get { return Rect.MinMaxRect(-38f, -28f, 58f, 116f); } }
}
