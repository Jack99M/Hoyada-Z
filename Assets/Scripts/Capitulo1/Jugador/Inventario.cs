using System.Collections.Generic;
using UnityEngine;

public enum Ranura { CuerpoACuerpo, Honda, Revolver }

/// <summary>Copia serializable del inventario (guardado de partida y puntos de control).</summary>
[System.Serializable]
public class DatosInventario
{
    public int vendas, botellas, pilas, alcohol, trapo, cinta, cuchilla, molotovs, puntas, piedras, balas, balasCargadas;
    public int arma, durabilidad;
    public bool clavos, tieneHonda, tieneRevolver, tieneLinterna;
    public int ranura;
    public float bateria;
    public List<string> llaves = new List<string>();
    public List<string> nombresLlaves = new List<string>();
    public List<string> archivos = new List<string>();
}

/// <summary>
/// Inventario limitado estilo survival horror: consumibles con tope, materiales de fabricacion
/// (alcohol, trapo, cinta, cuchilla), arrojadizos (botellas, molotovs), puntas, municion de
/// honda (piedras) y revolver (balas), arma cuerpo a cuerpo con durabilidad, objetos clave y archivos.
/// </summary>
public class Inventario : MonoBehaviour
{
    [Header("Consumibles")]
    public int vendas;
    public int botellas;
    public int pilas;
    public int maxVendas = 4;
    public int maxBotellas = 3;
    public int maxPilas = 3;

    [Header("Materiales de fabricacion")]
    public int alcohol;
    public int trapo;
    public int cinta;
    public int cuchilla;
    public int maxMaterial = 3;

    [Header("Fabricados")]
    public int molotovs;
    public int puntas;
    public int maxMolotovs = 3;
    public int maxPuntas = 3;

    [Header("Armas a distancia")]
    public bool tieneHonda;
    public int piedras;
    public int maxPiedras = 15;
    public bool tieneRevolver;
    public int balas;
    public int balasCargadas;
    public int maxBalas = 18;
    public int capacidadTambor = 6;

    [Header("Arma cuerpo a cuerpo")]
    public CombateJugador.Arma arma = CombateJugador.Arma.Punos;
    public int durabilidad;
    public bool clavos;
    [Tooltip("Objetos visuales del arma, hijos del punto de agarre de la mano.")]
    public GameObject visualPalo;
    public GameObject visualFierro;
    public GameObject visualMachete;
    public GameObject visualHonda;
    public GameObject visualRevolver;
    public GameObject visualClavos;

    [Header("Equipo")]
    public bool tieneLinterna;
    public Ranura ranura = Ranura.CuerpoACuerpo;

    private readonly List<string> llaves = new List<string>();
    private readonly Dictionary<string, string> nombresLlaves = new Dictionary<string, string>();
    public readonly List<string> archivos = new List<string>();

    public IEnumerable<string> NombresObjetosClave
    {
        get
        {
            foreach (string id in llaves)
            {
                string n;
                yield return nombresLlaves.TryGetValue(id, out n) ? n : id;
            }
        }
    }

    public int CantidadObjetosClave { get { return llaves.Count; } }

    private void Start()
    {
        ActualizarVisual();
    }

    public int DurabilidadMaxima { get { return CombateJugador.Datos(arma).durabilidad + (clavos ? 4 : 0); } }

    public void Equipar(CombateJugador.Arma nueva)
    {
        arma = nueva;
        clavos = false;
        durabilidad = CombateJugador.Datos(nueva).durabilidad;
        ranura = Ranura.CuerpoACuerpo;
        ActualizarVisual();
    }

    public void PonerClavos()
    {
        if (arma == CombateJugador.Arma.Punos) { return; }
        clavos = true;
        durabilidad = Mathf.Min(DurabilidadMaxima, durabilidad + 4);
        ActualizarVisual();
    }

    /// <summary>Resta un uso al arma. Devuelve true si se rompio.</summary>
    public bool GastarDurabilidad()
    {
        if (arma == CombateJugador.Arma.Punos)
        {
            return false;
        }
        durabilidad--;
        if (durabilidad <= 0)
        {
            Equipar(CombateJugador.Arma.Punos);
            return true;
        }
        return false;
    }

    public void ActualizarVisual()
    {
        bool melee = ranura == Ranura.CuerpoACuerpo;
        if (visualPalo != null) { visualPalo.SetActive(melee && arma == CombateJugador.Arma.Palo); }
        if (visualFierro != null) { visualFierro.SetActive(melee && arma == CombateJugador.Arma.Fierro); }
        if (visualMachete != null) { visualMachete.SetActive(melee && arma == CombateJugador.Arma.Machete); }
        if (visualClavos != null) { visualClavos.SetActive(melee && clavos && arma != CombateJugador.Arma.Punos); }
        if (visualHonda != null) { visualHonda.SetActive(ranura == Ranura.Honda); }
        if (visualRevolver != null) { visualRevolver.SetActive(ranura == Ranura.Revolver); }
    }

    public bool AgregarConsumible(ref int actual, int maximo, int cantidad)
    {
        if (actual >= maximo)
        {
            return false;
        }
        actual = Mathf.Min(maximo, actual + cantidad);
        return true;
    }

    // ---------- Acceso por nombre (trueque, recetas, recompensas) ----------

    public int Cantidad(string recurso)
    {
        switch (recurso)
        {
            case "vendas": return vendas;
            case "botellas": return botellas;
            case "pilas": return pilas;
            case "alcohol": return alcohol;
            case "trapo": return trapo;
            case "cinta": return cinta;
            case "cuchilla": return cuchilla;
            case "molotovs": return molotovs;
            case "puntas": return puntas;
            case "piedras": return piedras;
            case "balas": return balas + balasCargadas;
        }
        return Tiene(recurso) ? 1 : 0;
    }

    public int Maximo(string recurso)
    {
        switch (recurso)
        {
            case "vendas": return maxVendas;
            case "botellas": return maxBotellas;
            case "pilas": return maxPilas;
            case "alcohol":
            case "trapo":
            case "cinta":
            case "cuchilla": return maxMaterial;
            case "molotovs": return maxMolotovs;
            case "puntas": return maxPuntas;
            case "piedras": return maxPiedras;
            case "balas": return maxBalas;
        }
        return 1;
    }

    /// <summary>Suma (o resta) un recurso respetando el tope. Devuelve cuanto cambio realmente.</summary>
    public int Sumar(string recurso, int n)
    {
        int antes = Cantidad(recurso);
        int max = Maximo(recurso);
        int nuevo = Mathf.Clamp(antes + n, 0, recurso == "balas" ? max + capacidadTambor : max);
        int delta = nuevo - antes;
        switch (recurso)
        {
            case "vendas": vendas += delta; break;
            case "botellas": botellas += delta; break;
            case "pilas": pilas += delta; break;
            case "alcohol": alcohol += delta; break;
            case "trapo": trapo += delta; break;
            case "cinta": cinta += delta; break;
            case "cuchilla": cuchilla += delta; break;
            case "molotovs": molotovs += delta; break;
            case "puntas": puntas += delta; break;
            case "piedras": piedras += delta; break;
            case "balas":
                if (delta >= 0) { balas += delta; }
                else
                {
                    int quitar = -delta;
                    int deReserva = Mathf.Min(balas, quitar);
                    balas -= deReserva;
                    balasCargadas -= quitar - deReserva;
                }
                break;
        }
        return delta;
    }

    public static string NombreRecurso(string recurso, int n)
    {
        bool uno = n == 1;
        switch (recurso)
        {
            case "vendas": return uno ? "venda" : "vendas";
            case "botellas": return uno ? "botella" : "botellas";
            case "pilas": return "pilas";
            case "alcohol": return "alcohol";
            case "trapo": return uno ? "trapo" : "trapos";
            case "cinta": return "cinta";
            case "cuchilla": return uno ? "cuchilla" : "cuchillas";
            case "molotovs": return uno ? "molotov" : "molotovs";
            case "puntas": return uno ? "punta" : "puntas";
            case "piedras": return uno ? "piedra" : "piedras";
            case "balas": return uno ? "bala" : "balas";
        }
        return recurso;
    }

    // ---------- Objetos clave ----------

    public void AgregarObjetoClave(string id, string nombre)
    {
        if (!llaves.Contains(id)) { llaves.Add(id); }
        nombresLlaves[id] = nombre;
    }

    public bool Tiene(string id)
    {
        return !string.IsNullOrEmpty(id) && llaves.Contains(id);
    }

    public void Quitar(string id)
    {
        llaves.Remove(id);
    }

    // ---------- Guardado ----------

    public DatosInventario Exportar()
    {
        var d = new DatosInventario
        {
            vendas = vendas, botellas = botellas, pilas = pilas, alcohol = alcohol, trapo = trapo, cinta = cinta, cuchilla = cuchilla,
            molotovs = molotovs, puntas = puntas, piedras = piedras, balas = balas, balasCargadas = balasCargadas,
            arma = (int)arma, durabilidad = durabilidad, clavos = clavos, tieneHonda = tieneHonda, tieneRevolver = tieneRevolver,
            tieneLinterna = tieneLinterna, ranura = (int)ranura
        };
        Linterna l = GetComponent<Linterna>();
        d.bateria = l != null ? l.bateria : 100f;
        foreach (string id in llaves)
        {
            d.llaves.Add(id);
            string n;
            d.nombresLlaves.Add(nombresLlaves.TryGetValue(id, out n) ? n : id);
        }
        d.archivos.AddRange(archivos);
        return d;
    }

    public void Importar(DatosInventario d)
    {
        if (d == null) { return; }
        vendas = d.vendas; botellas = d.botellas; pilas = d.pilas; alcohol = d.alcohol; trapo = d.trapo; cinta = d.cinta; cuchilla = d.cuchilla;
        molotovs = d.molotovs; puntas = d.puntas; piedras = d.piedras; balas = d.balas; balasCargadas = d.balasCargadas;
        arma = (CombateJugador.Arma)d.arma; durabilidad = d.durabilidad; clavos = d.clavos;
        tieneHonda = d.tieneHonda; tieneRevolver = d.tieneRevolver; tieneLinterna = d.tieneLinterna;
        ranura = (Ranura)d.ranura;
        Linterna l = GetComponent<Linterna>();
        if (l != null) { l.bateria = d.bateria; }
        llaves.Clear();
        nombresLlaves.Clear();
        for (int i = 0; i < d.llaves.Count; i++)
        {
            llaves.Add(d.llaves[i]);
            nombresLlaves[d.llaves[i]] = i < d.nombresLlaves.Count ? d.nombresLlaves[i] : d.llaves[i];
        }
        archivos.Clear();
        archivos.AddRange(d.archivos);
        ActualizarVisual();
    }
}
