using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Modo Supervivencia "Noche en la Plaza Abaroa": oleadas cada vez mas grandes de infectados
/// (comunes, corredores, gritones y fungicos). Entre oleadas llegan suministros. Puntaje con
/// combos (sigilo y tiros a la cabeza valen mas) y tabla de records local.
/// </summary>
public class ModoSupervivencia : MonoBehaviour
{
    public Vector3 inicioJugador = new Vector3(26f, 0.2f, 24f);
    public float rotJugador;
    public Transform[] spawns;
    public Transform[] suministros;
    [Tooltip("Plantillas desactivadas: 0 comun, 1 corredor, 2 fungico, 3 griton")]
    public Infectado[] plantillas;
    public GameObject muros;
    public GameObject[] desactivar;

    public int Oleada { get; private set; }
    public int Puntaje { get; private set; }
    public int Bajas { get; private set; }
    public int Combo { get; private set; }
    public bool EnDescanso { get; private set; }
    public float FinDescanso { get; private set; }
    public bool Activo { get; private set; }
    public bool Terminada { get; private set; }
    public bool NuevoRecord { get; private set; }
    public int Restantes { get { return pendientes.Count + vivos.Count; } }
    public float UltimosPuntosT { get; private set; } = -10f;
    public int UltimosPuntos { get; private set; }

    private readonly List<Infectado> vivos = new List<Infectado>();
    private readonly Queue<Infectado.Tipo> pendientes = new Queue<Infectado.Tipo>();
    private float siguienteSpawn;
    private float comboHasta;
    private readonly List<GameObject> cajas = new List<GameObject>();
    private Juego juego;

    public void Preparar(Juego g)
    {
        juego = g;
        if (desactivar != null) { foreach (GameObject go in desactivar) { if (go != null) { go.SetActive(false); } } }
        foreach (Infectado inf in FindObjectsByType<Infectado>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            bool plantilla = false;
            if (plantillas != null) { foreach (Infectado p in plantillas) { if (p == inf) { plantilla = true; } } }
            if (!plantilla) { inf.gameObject.SetActive(false); }
        }
        foreach (Interactivo it in FindObjectsByType<Interactivo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (it.GetComponent<Companera>() != null) { it.gameObject.SetActive(false); continue; }
            if (it is Recogible || it is Documento || it is Coleccionable || it is PuntoGuardado || it is Conversacion || it is EleccionMoral)
            {
                it.gameObject.SetActive(false);
            }
        }
        if (Companera.I != null) { Companera.I.gameObject.SetActive(false); }
        if (muros != null) { muros.SetActive(true); }

        Jugador j = g.jugador;
        if (j != null)
        {
            j.Teletransportar(inicioJugador, Quaternion.Euler(0f, rotJugador, 0f));
            Inventario inv = j.Inventario;
            inv.tieneLinterna = true;
            inv.Equipar(CombateJugador.Arma.Palo);
            inv.tieneHonda = true;
            inv.piedras = Dificultad.Nivel == NivelDificultad.Superviviente ? 6 : 10;
            inv.vendas = Dificultad.Nivel == NivelDificultad.Facil ? 3 : 2;
            inv.botellas = 1;
            inv.alcohol = 1;
            inv.trapo = 1;
            inv.pilas = 1;
            inv.ranura = Ranura.CuerpoACuerpo;
            inv.ActualizarVisual();
        }
        g.SetObjetivo("Sobrevive. Entre oleadas busca suministros alrededor del monumento.");
    }

    public void Iniciar()
    {
        Activo = true;
        Puntaje = 0;
        Bajas = 0;
        EmpezarOleada(1);
    }

    private void EmpezarOleada(int n)
    {
        Oleada = n;
        EnDescanso = false;
        int total = 4 + n * 2;
        int fungicos = n >= 4 ? n / 4 : 0;
        int gritones = n >= 3 ? 1 + (n >= 8 ? 1 : 0) : 0;
        float probCorredor = Mathf.Min(0.4f, 0.08f * n);
        var lista = new List<Infectado.Tipo>();
        for (int i = 0; i < fungicos; i++) { lista.Add(Infectado.Tipo.Fungico); }
        for (int i = 0; i < gritones; i++) { lista.Add(Infectado.Tipo.Griton); }
        while (lista.Count < total) { lista.Add(Random.value < probCorredor ? Infectado.Tipo.Corredor : Infectado.Tipo.Comun); }
        for (int i = 0; i < lista.Count; i++)
        {
            int k = Random.Range(i, lista.Count);
            Infectado.Tipo t = lista[i]; lista[i] = lista[k]; lista[k] = t;
        }
        pendientes.Clear();
        foreach (Infectado.Tipo t in lista) { pendientes.Enqueue(t); }
        siguienteSpawn = Time.time + 1.5f;
        AudioCap1.Play2D("sfx_grito", 0.6f);
        AudioCap1.Musica(n % 5 == 0 ? "mus_jefe" : "mus_supervivencia", 1.5f);
        if (juego != null) { juego.Mensaje("OLEADA " + n + "  ·  " + total + " infectados"); }
    }

    private void Update()
    {
        if (!Activo || Terminada || !Juego.Control)
        {
            return;
        }

        vivos.RemoveAll(v => v == null || v.Muerto);

        if (EnDescanso)
        {
            if (Time.time >= FinDescanso) { EmpezarOleada(Oleada + 1); }
            return;
        }

        int maxSimultaneos = 6 + Oleada;
        if (pendientes.Count > 0 && Time.time >= siguienteSpawn && vivos.Count < maxSimultaneos)
        {
            siguienteSpawn = Time.time + Mathf.Max(0.35f, 1.1f - Oleada * 0.05f);
            Spawn(pendientes.Dequeue());
        }

        if (pendientes.Count == 0 && vivos.Count == 0)
        {
            OleadaSuperada();
        }
    }

    private void Spawn(Infectado.Tipo tipo)
    {
        if (plantillas == null || plantillas.Length == 0 || spawns == null || spawns.Length == 0) { return; }
        int idx = tipo == Infectado.Tipo.Corredor ? 1 : (tipo == Infectado.Tipo.Fungico ? 2 : (tipo == Infectado.Tipo.Griton ? 3 : 0));
        Infectado plantilla = plantillas[Mathf.Min(idx, plantillas.Length - 1)];
        if (plantilla == null) { return; }

        Vector3 pj = Jugador.I != null ? Jugador.I.transform.position : Vector3.zero;
        Transform mejor = spawns[Random.Range(0, spawns.Length)];
        float mejorD = 0f;
        for (int i = 0; i < 4; i++)
        {
            Transform s = spawns[Random.Range(0, spawns.Length)];
            float d = Vector3.Distance(s.position, pj);
            if (d > mejorD) { mejorD = d; mejor = s; }
        }
        Vector3 pos = mejor.position + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 3f, NavMesh.AllAreas)) { pos = hit.position; }

        Infectado inf = Instantiate(plantilla, pos, Quaternion.LookRotation(pj - pos == Vector3.zero ? Vector3.forward : new Vector3(pj.x - pos.x, 0f, pj.z - pos.z)), transform);
        inf.name = plantilla.name + "_O" + Oleada;
        inf.estadoInicial = Infectado.Estado.Deambular;
        inf.radioDeambular = 5f;
        inf.gameObject.SetActive(true);
        inf.Alertar();
        vivos.Add(inf);
    }

    private void OleadaSuperada()
    {
        int bono = 100 * Oleada;
        Sumar(bono);
        EnDescanso = true;
        FinDescanso = Time.time + 15f;
        AudioCap1.Musica(null, 2f);
        AudioCap1.Play2D("sfx_objetivo", 0.7f);
        if (juego != null) { juego.Mensaje("¡Oleada " + Oleada + " superada! +" + bono + " pts  ·  Llegaron suministros"); }
        if (Oleada >= 10) { Logros.Desbloquear("oleada10"); }
        Suministros();
    }

    private void Suministros()
    {
        foreach (GameObject c in cajas) { if (c != null) { Destroy(c); } }
        cajas.Clear();
        if (suministros == null || suministros.Length == 0) { return; }
        var tipos = new List<Recogible.TipoObjeto>
        {
            Recogible.TipoObjeto.Venda, Recogible.TipoObjeto.Piedras, Recogible.TipoObjeto.Botella,
            Recogible.TipoObjeto.Alcohol, Recogible.TipoObjeto.Trapo, Recogible.TipoObjeto.Cinta, Recogible.TipoObjeto.Cuchilla
        };
        if (Oleada % 2 == 0) { tipos.Add(Recogible.TipoObjeto.Fierro); }
        if (Oleada % 3 == 0) { tipos.Add(Recogible.TipoObjeto.Molotov); tipos.Add(Recogible.TipoObjeto.Venda); }
        if (Oleada >= 3 && Oleada % 3 == 0) { tipos.Add(Recogible.TipoObjeto.Revolver); }
        if (Oleada >= 4) { tipos.Add(Recogible.TipoObjeto.Balas); }
        int n = Mathf.Min(suministros.Length, 3 + Oleada / 3);
        var puntos = new List<Transform>(suministros);
        for (int i = 0; i < n && puntos.Count > 0; i++)
        {
            int k = Random.Range(0, puntos.Count);
            Transform p = puntos[k];
            puntos.RemoveAt(k);
            Recogible.TipoObjeto t = tipos[Random.Range(0, tipos.Count)];
            int cant = t == Recogible.TipoObjeto.Piedras ? 5 : (t == Recogible.TipoObjeto.Balas ? 3 : (t == Recogible.TipoObjeto.Revolver ? 6 : 1));
            cajas.Add(CrearSuministro(p.position, t, cant));
        }
    }

    public static GameObject CrearSuministro(Vector3 pos, Recogible.TipoObjeto t, int cantidad)
    {
        var go = new GameObject("Suministro_" + t);
        go.transform.position = pos;
        var r = go.AddComponent<Recogible>();
        r.tipo = t;
        r.cantidad = cantidad;
        r.destello = true;
        r.radio = 1.8f;
        r.altura = 0.3f;
        var caja = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(caja.GetComponent<Collider>());
        caja.transform.SetParent(go.transform, false);
        caja.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        caja.transform.localScale = new Vector3(0.35f, 0.24f, 0.28f);
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s != null)
        {
            var m = new Material(s);
            m.SetColor("_BaseColor", new Color(0.55f, 0.42f, 0.28f));
            caja.GetComponent<Renderer>().sharedMaterial = m;
        }
        var luz = new GameObject("Luz").AddComponent<Light>();
        luz.transform.SetParent(go.transform, false);
        luz.transform.localPosition = Vector3.up * 0.8f;
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.85f, 0.5f);
        luz.intensity = 3f;
        luz.range = 4f;
        return go;
    }

    public void RegistrarBaja(Infectado inf)
    {
        if (!Activo || Terminada || inf == null) { return; }
        Bajas++;
        int pts;
        switch (inf.tipo)
        {
            case Infectado.Tipo.Corredor: pts = 15; break;
            case Infectado.Tipo.Fungico: pts = 45; break;
            case Infectado.Tipo.Griton: pts = 30; break;
            default: pts = 10; break;
        }
        if (inf.UltimaFuente == FuenteDano.Sigilo) { pts *= 2; }
        if (inf.UltimaFuente == FuenteDano.Fuego) { pts += 10; }
        Combo = Time.time < comboHasta ? Combo + 1 : 1;
        comboHasta = Time.time + 4f;
        pts = Mathf.RoundToInt(pts * Mathf.Min(3f, 1f + 0.25f * (Combo - 1)));
        Sumar(pts);
    }

    private void Sumar(int pts)
    {
        Puntaje += pts;
        UltimosPuntos = pts;
        UltimosPuntosT = Time.unscaledTime;
    }

    public void Terminar()
    {
        if (Terminada) { return; }
        Terminada = true;
        Activo = false;
        NuevoRecord = Records.GuardarSupervivencia(Puntaje, Oleada);
        AudioCap1.Musica(null, 2f);
    }
}
