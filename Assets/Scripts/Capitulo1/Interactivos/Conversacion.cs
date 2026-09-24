using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arbol de dialogo con un personaje: nodos con texto y opciones. Las opciones pueden
/// costar recursos (trueque), dar recursos, cambiar la moral, poner flags de la partida,
/// ejecutar acciones y llevar a otro nodo. Tambien soporta "charlas" opcionales que se
/// habilitan por flags (como las conversaciones opcionales de Ellie en The Last of Us).
/// </summary>
public class Conversacion : Interactivo
{
    [System.Serializable]
    public class Opcion
    {
        public string texto;
        [Tooltip("Id del siguiente nodo. Vacio = termina la conversacion.")]
        public string siguiente;
        [Tooltip("Lo que paga Mateo: 'botellas:2,vendas:1'")]
        public string costo;
        [Tooltip("Lo que recibe Mateo: 'piedras:6,trapo:1' (tambien 'honda:8' o 'revolver:6')")]
        public string dar;
        public int moral;
        public string requiereFlag;
        public string requiereSinFlag;
        public string ponerFlag;
        public bool unaVez;
        [TextArea] public string respuesta;
        public Acciones acciones = new Acciones();
    }

    [System.Serializable]
    public class Nodo
    {
        public string id;
        public string quien;
        public string retrato;
        [TextArea(3, 10)] public string texto;
        public Opcion[] opciones;
    }

    [System.Serializable]
    public class Charla
    {
        public string flag;
        public string nodo;
    }

    public string nombreNPC = "Sobreviviente";
    public string nodoInicio = "inicio";
    [Tooltip("Nodo al volver a hablar despues de la primera vez (vacio = inicio).")]
    public string nodoRegreso;
    [Tooltip("Despues de la primera conversacion solo se puede hablar si hay charlas pendientes.")]
    public bool soloCharlasDespues;
    public Nodo[] nodos;
    public Charla[] charlas;

    private bool hablado;
    private readonly HashSet<string> usadas = new HashSet<string>();
    private readonly HashSet<string> charlasHechas = new HashSet<string>();

    private void Reset()
    {
        radio = 2.3f;
        altura = 1f;
    }

    private Charla CharlaPendiente()
    {
        if (charlas == null || Juego.I == null) { return null; }
        foreach (Charla c in charlas)
        {
            if (!charlasHechas.Contains(c.nodo) && Juego.I.Flags.Contains(c.flag)) { return c; }
        }
        return null;
    }

    public bool TieneCharlaPendiente { get { return CharlaPendiente() != null; } }

    public override bool Disponible
    {
        get
        {
            if (!base.Disponible) { return false; }
            if (hablado && soloCharlasDespues) { return CharlaPendiente() != null; }
            return true;
        }
    }

    public override string Prompt { get { return (CharlaPendiente() != null && hablado ? "Conversar con " : "Hablar con ") + nombreNPC; } }

    public override void Usar(Jugador j)
    {
        Charla ch = hablado ? CharlaPendiente() : null;
        if (ch != null)
        {
            charlasHechas.Add(ch.nodo);
            Mostrar(ch.nodo);
            return;
        }
        Mostrar(hablado && !string.IsNullOrEmpty(nodoRegreso) ? nodoRegreso : nodoInicio);
    }

    private Nodo Buscar(string id)
    {
        foreach (Nodo n in nodos) { if (n.id == id) { return n; } }
        return null;
    }

    public void Mostrar(string id)
    {
        Nodo n = Buscar(id);
        if (n == null)
        {
            hablado = true;
            return;
        }
        Jugador j = Jugador.I;
        Inventario inv = j != null ? j.Inventario : null;
        Juego g = Juego.I;
        var lista = new List<Juego.Opcion>();
        if (n.opciones == null || n.opciones.Length == 0)
        {
            lista.Add(new Juego.Opcion("[Continuar]", () => { hablado = true; }));
        }
        else
        {
            for (int i = 0; i < n.opciones.Length; i++)
            {
                Opcion o = n.opciones[i];
                string clave = n.id + ":" + i;
                if (o.unaVez && usadas.Contains(clave)) { continue; }
                if (!string.IsNullOrEmpty(o.requiereFlag) && !g.Flags.Contains(o.requiereFlag)) { continue; }
                if (!string.IsNullOrEmpty(o.requiereSinFlag) && g.Flags.Contains(o.requiereSinFlag)) { continue; }
                string etiqueta = o.texto;
                bool puede = PuedePagar(inv, o.costo);
                if (!string.IsNullOrEmpty(o.costo)) { etiqueta += "   <color=#E8B46A>[das: " + Describir(o.costo) + "]</color>"; }
                if (!string.IsNullOrEmpty(o.dar)) { etiqueta += "   <color=#8FD18F>[recibes: " + Describir(o.dar) + "]</color>"; }
                Nodo nodo = n;
                lista.Add(new Juego.Opcion(etiqueta, () => Resolver(nodo, o, clave), !puede));
            }
        }
        g.MostrarEleccion(string.IsNullOrEmpty(n.quien) ? nombreNPC : n.quien, n.texto, n.retrato, lista);
    }

    private void Resolver(Nodo n, Opcion o, string clave)
    {
        Jugador j = Jugador.I;
        Inventario inv = j != null ? j.Inventario : null;
        Juego g = Juego.I;
        usadas.Add(clave);
        if (inv != null)
        {
            Pagar(inv, o.costo);
            Entregar(inv, o.dar);
        }
        if (o.moral != 0) { g.CambiarMoral(o.moral); }
        if (!string.IsNullOrEmpty(o.ponerFlag)) { g.Flags.Add(o.ponerFlag); }
        if (!string.IsNullOrEmpty(o.respuesta))
        {
            foreach (string linea in o.respuesta.Split('\n'))
            {
                string l = linea.Trim();
                if (string.IsNullOrEmpty(l)) { continue; }
                int k = l.IndexOf('|');
                if (k > 0) { g.Decir(l.Substring(0, k), l.Substring(k + 1)); }
                else { g.Decir(nombreNPC, l); }
            }
        }
        if (o.acciones != null) { o.acciones.Ejecutar(transform.position); }
        if (!string.IsNullOrEmpty(o.siguiente)) { Mostrar(o.siguiente); }
        else { hablado = true; }
    }

    // ---------- Recursos por texto ----------

    public static bool PuedePagar(Inventario inv, string spec)
    {
        if (string.IsNullOrEmpty(spec)) { return true; }
        if (inv == null) { return false; }
        foreach (string parte in spec.Split(','))
        {
            string[] p = parte.Trim().Split(':');
            if (p.Length < 2) { continue; }
            if (inv.Cantidad(p[0]) < int.Parse(p[1])) { return false; }
        }
        return true;
    }

    public static void Pagar(Inventario inv, string spec)
    {
        if (string.IsNullOrEmpty(spec)) { return; }
        foreach (string parte in spec.Split(','))
        {
            string[] p = parte.Trim().Split(':');
            if (p.Length < 2) { continue; }
            int n = int.Parse(p[1]);
            if (inv.Tiene(p[0])) { inv.Quitar(p[0]); }
            else { inv.Sumar(p[0], -n); }
        }
    }

    public static void Entregar(Inventario inv, string spec)
    {
        if (string.IsNullOrEmpty(spec) || inv == null) { return; }
        foreach (string parte in spec.Split(','))
        {
            string[] p = parte.Trim().Split(':');
            if (p.Length < 2) { continue; }
            int n = int.Parse(p[1]);
            switch (p[0])
            {
                case "honda":
                    inv.tieneHonda = true;
                    inv.Sumar("piedras", n);
                    inv.ranura = Ranura.Honda;
                    inv.ActualizarVisual();
                    if (Juego.I != null) { Juego.I.Mensaje("Recibiste: Honda (warak'a) + " + n + " piedras   ·   [2] equipar  ·  clic der. apuntar"); }
                    break;
                case "revolver":
                    inv.tieneRevolver = true;
                    inv.balasCargadas = Mathf.Min(inv.capacidadTambor, n);
                    if (Juego.I != null) { Juego.I.Mensaje("Recibiste: Revólver .38   ·   [3] equipar"); }
                    break;
                default:
                    int d = inv.Sumar(p[0], n);
                    if (d > 0 && Juego.I != null) { Juego.I.Mensaje("Recibiste: " + d + " " + Inventario.NombreRecurso(p[0], d)); }
                    else if (d <= 0 && Juego.I != null) { Juego.I.Mensaje("No puedes cargar más " + Inventario.NombreRecurso(p[0], 2)); }
                    break;
            }
        }
    }

    public static string Describir(string spec)
    {
        var partes = new List<string>();
        foreach (string parte in spec.Split(','))
        {
            string[] p = parte.Trim().Split(':');
            if (p.Length < 2) { continue; }
            int n = int.Parse(p[1]);
            if (p[0] == "honda") { partes.Add("honda"); }
            else if (p[0] == "revolver") { partes.Add("revólver"); }
            else { partes.Add(n + " " + Inventario.NombreRecurso(p[0], n)); }
        }
        return string.Join(", ", partes.ToArray());
    }

    // ---------- Guardado ----------

    public string Exportar()
    {
        return (hablado ? "1" : "0") + ";" + string.Join(",", new List<string>(usadas).ToArray()) + ";" + string.Join(",", new List<string>(charlasHechas).ToArray());
    }

    public void Importar(string s)
    {
        if (string.IsNullOrEmpty(s)) { return; }
        string[] p = s.Split(';');
        hablado = p.Length > 0 && p[0] == "1";
        usadas.Clear();
        charlasHechas.Clear();
        if (p.Length > 1) { foreach (string u in p[1].Split(',')) { if (u.Length > 0) { usadas.Add(u); } } }
        if (p.Length > 2) { foreach (string c in p[2].Split(',')) { if (c.Length > 0) { charlasHechas.Add(c); } } }
    }
}
