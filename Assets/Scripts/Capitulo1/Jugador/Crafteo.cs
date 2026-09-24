using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fabricacion en tiempo real estilo The Last of Us: mantener [Tab] abre la mochila y con
/// [1]-[4] se fabrica. El mundo NO se pausa: fabricar toma tiempo y te pueden interrumpir.
/// Los materiales compiten entre recetas (alcohol + trapo sirven para vendas O molotovs).
/// </summary>
public class Crafteo : MonoBehaviour
{
    public class Receta
    {
        public string nombre;
        public string descripcion;
        public string[] costo;      // "recurso:n"
        public string resultado;    // recurso o "clavos"
        public float tiempo;
        public Receta(string n, string d, string res, float t, params string[] c) { nombre = n; descripcion = d; resultado = res; tiempo = t; costo = c; }
    }

    public static readonly Receta[] Recetas =
    {
        new Receta("Venda", "Cura 45 de salud. [H] para usarla.", "vendas", 1.6f, "alcohol:1", "trapo:1"),
        new Receta("Molotov", "Quema a los infectados en un área. [G] para lanzarlo.", "molotovs", 1.8f, "alcohol:1", "trapo:1", "botellas:1"),
        new Receta("Punta", "Te zafa de un agarre, mata en sigilo a un fúngico y fuerza candados.", "puntas", 1.4f, "cinta:1", "cuchilla:1"),
        new Receta("Clavos al arma", "El arma cuerpo a cuerpo pega +40% y dura más.", "clavos", 2f, "cinta:1", "cuchilla:1")
    };

    public bool Abierto { get; private set; }
    public bool Fabricando { get; private set; }
    public float Progreso { get; private set; }
    public int RecetaActual { get; private set; } = -1;

    private Jugador j;
    private Inventario inv;
    private float inicio;

    private void Awake()
    {
        j = GetComponent<Jugador>();
        inv = GetComponent<Inventario>();
    }

    private void Update()
    {
        bool control = Juego.Control && j != null && !j.Muerto && !j.Agarrado && !j.Bloqueado;
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;

        Abierto = control && ((kb != null && kb.tabKey.isPressed) || (pad != null && pad.selectButton.isPressed));

        if (Fabricando)
        {
            Receta r = Recetas[RecetaActual];
            Progreso = Mathf.Clamp01((Time.time - inicio) / r.tiempo);
            if (!control) { Interrumpir(); return; }
            if (Progreso >= 1f) { Terminar(); }
            return;
        }

        if (!Abierto || kb == null)
        {
            return;
        }
        int elegido = -1;
        if (kb.digit1Key.wasPressedThisFrame) { elegido = 0; }
        else if (kb.digit2Key.wasPressedThisFrame) { elegido = 1; }
        else if (kb.digit3Key.wasPressedThisFrame) { elegido = 2; }
        else if (kb.digit4Key.wasPressedThisFrame) { elegido = 3; }
        if (elegido >= 0) { Empezar(elegido); }
    }

    /// <summary>Devuelve null si se puede fabricar, o el motivo si no.</summary>
    public string Motivo(int i)
    {
        if (inv == null) { return "Sin mochila"; }
        Receta r = Recetas[i];
        foreach (string c in r.costo)
        {
            string[] p = c.Split(':');
            if (inv.Cantidad(p[0]) < int.Parse(p[1])) { return "Falta " + Inventario.NombreRecurso(p[0], 1); }
        }
        if (r.resultado == "clavos")
        {
            if (inv.arma == CombateJugador.Arma.Punos) { return "Necesitas un arma cuerpo a cuerpo"; }
            if (inv.clavos) { return "Tu arma ya tiene clavos"; }
        }
        else if (inv.Cantidad(r.resultado) >= inv.Maximo(r.resultado))
        {
            return "No puedes cargar más";
        }
        return null;
    }

    private void Empezar(int i)
    {
        string m = Motivo(i);
        if (m != null)
        {
            if (Juego.I != null) { Juego.I.Mensaje(m); }
            AudioCap1.Play2D("sfx_click", 0.4f);
            return;
        }
        RecetaActual = i;
        Fabricando = true;
        inicio = Time.time;
        Progreso = 0f;
        AudioCap1.Play3D("sfx_craftear", transform.position, 0.7f);
        SistemaRuido.Emitir(transform.position, 1.5f, true);
    }

    private void Terminar()
    {
        Fabricando = false;
        Receta r = Recetas[RecetaActual];
        if (Motivo(RecetaActual) != null)
        {
            RecetaActual = -1;
            return;
        }
        foreach (string c in r.costo)
        {
            string[] p = c.Split(':');
            inv.Sumar(p[0], -int.Parse(p[1]));
        }
        if (r.resultado == "clavos") { inv.PonerClavos(); }
        else { inv.Sumar(r.resultado, 1); }
        Stats.D.crafteados++;
        Logros.Revisar();
        AudioCap1.Play3D("sfx_recoger", transform.position, 0.8f);
        if (Juego.I != null) { Juego.I.Mensaje("Fabricaste: " + r.nombre); }
        RecetaActual = -1;
    }

    public void Interrumpir()
    {
        if (Fabricando && Juego.I != null) { Juego.I.Mensaje("Se interrumpió la fabricación"); }
        Fabricando = false;
        RecetaActual = -1;
        Progreso = 0f;
    }
}
