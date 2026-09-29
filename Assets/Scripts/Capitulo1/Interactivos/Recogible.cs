using UnityEngine;

/// <summary>
/// Objeto que Mateo puede recoger: consumibles (vendas, botellas, pilas), materiales de
/// fabricacion (alcohol, trapo, cinta, cuchilla), municion (piedras, balas), armas (palo,
/// fierro, machete, honda, revolver), la linterna u objetos clave (llaves, cortafierro...).
/// </summary>
public class Recogible : Interactivo
{
    public enum TipoObjeto { Venda, Botella, Pilas, Palo, Fierro, Linterna, ObjetoClave, Alcohol, Trapo, Cinta, Cuchilla, Piedras, Balas, Honda, Revolver, Machete, Molotov, Punta }

    public TipoObjeto tipo = TipoObjeto.Venda;
    public int cantidad = 1;
    [Tooltip("Solo para objetos clave: identificador (p. ej. 'llave_deposito').")]
    public string idObjeto;
    [Tooltip("Nombre que se muestra al jugador.")]
    public string nombre;
    public Acciones alRecoger = new Acciones();

    public bool Recogido { get; private set; }

    private float fase;
    private Vector3 basePos;
    private bool girar;

    private void Reset()
    {
        destello = true;
        radio = 1.7f;
        altura = 0.3f;
    }

    private void Start()
    {
        basePos = transform.position;
        fase = Random.value * 10f;
        girar = tipo == TipoObjeto.ObjetoClave;
    }

    /// <summary>Consumible que la dificultad Superviviente puede retirar del mapa.</summary>
    public bool EsConsumible
    {
        get
        {
            switch (tipo)
            {
                case TipoObjeto.Venda:
                case TipoObjeto.Botella:
                case TipoObjeto.Pilas:
                case TipoObjeto.Alcohol:
                case TipoObjeto.Trapo:
                case TipoObjeto.Cinta:
                case TipoObjeto.Cuchilla:
                case TipoObjeto.Piedras:
                case TipoObjeto.Balas:
                    return alRecoger == null || string.IsNullOrEmpty(alRecoger.objetivo);
            }
            return false;
        }
    }

    public string NombreMostrado
    {
        get
        {
            if (!string.IsNullOrEmpty(nombre)) { return nombre; }
            switch (tipo)
            {
                case TipoObjeto.Venda: return "Venda";
                case TipoObjeto.Botella: return "Botella vacía";
                case TipoObjeto.Pilas: return "Pilas";
                case TipoObjeto.Palo: return "Palo";
                case TipoObjeto.Fierro: return "Fierro de construcción";
                case TipoObjeto.Machete: return "Machete";
                case TipoObjeto.Linterna: return "Linterna";
                case TipoObjeto.Alcohol: return "Alcohol";
                case TipoObjeto.Trapo: return "Trapo";
                case TipoObjeto.Cinta: return "Cinta adhesiva";
                case TipoObjeto.Cuchilla: return "Cuchilla";
                case TipoObjeto.Piedras: return "Piedras";
                case TipoObjeto.Balas: return "Balas .38";
                case TipoObjeto.Honda: return "Honda (warak'a)";
                case TipoObjeto.Revolver: return "Revólver .38";
                case TipoObjeto.Molotov: return "Molotov";
                case TipoObjeto.Punta: return "Punta";
                default: return idObjeto;
            }
        }
    }

    public override string Prompt { get { return "Recoger " + NombreMostrado + (cantidad > 1 ? " x" + cantidad : ""); } }

    private void Update()
    {
        if (girar)
        {
            fase += Time.deltaTime;
            transform.position = basePos + Vector3.up * (Mathf.Sin(fase * 2f) * 0.03f);
        }
    }

    private bool SumarRecurso(Inventario inv, string recurso, string lleno, ref string mensajeLleno)
    {
        int d = inv.Sumar(recurso, cantidad);
        if (d <= 0) { mensajeLleno = lleno; return false; }
        return true;
    }

    public override void Usar(Jugador j)
    {
        Inventario inv = j.Inventario;
        bool ok = true;
        string lleno = null;

        switch (tipo)
        {
            case TipoObjeto.Venda: ok = SumarRecurso(inv, "vendas", "No puedo cargar más vendas", ref lleno); break;
            case TipoObjeto.Botella: ok = SumarRecurso(inv, "botellas", "No puedo cargar más botellas", ref lleno); break;
            case TipoObjeto.Pilas: ok = SumarRecurso(inv, "pilas", "No puedo cargar más pilas", ref lleno); break;
            case TipoObjeto.Alcohol: ok = SumarRecurso(inv, "alcohol", "No puedo cargar más alcohol", ref lleno); break;
            case TipoObjeto.Trapo: ok = SumarRecurso(inv, "trapo", "No puedo cargar más trapos", ref lleno); break;
            case TipoObjeto.Cinta: ok = SumarRecurso(inv, "cinta", "No puedo cargar más cinta", ref lleno); break;
            case TipoObjeto.Cuchilla: ok = SumarRecurso(inv, "cuchilla", "No puedo cargar más cuchillas", ref lleno); break;
            case TipoObjeto.Piedras: ok = SumarRecurso(inv, "piedras", "No me caben más piedras", ref lleno); break;
            case TipoObjeto.Balas: ok = SumarRecurso(inv, "balas", "No puedo cargar más balas", ref lleno); break;
            case TipoObjeto.Molotov: ok = SumarRecurso(inv, "molotovs", "No puedo cargar más molotovs", ref lleno); break;
            case TipoObjeto.Punta: ok = SumarRecurso(inv, "puntas", "No puedo cargar más puntas", ref lleno); break;
            case TipoObjeto.Palo:
                if ((inv.arma == CombateJugador.Arma.Fierro || inv.arma == CombateJugador.Arma.Machete) && inv.durabilidad > 4)
                {
                    ok = false;
                    lleno = "Mejor me quedo con lo que tengo";
                }
                else if (inv.arma == CombateJugador.Arma.Palo && inv.durabilidad >= inv.DurabilidadMaxima)
                {
                    ok = false;
                    lleno = "Ya tengo un palo en buen estado";
                }
                else
                {
                    inv.Equipar(CombateJugador.Arma.Palo);
                }
                break;
            case TipoObjeto.Fierro:
                if (inv.arma == CombateJugador.Arma.Machete && inv.durabilidad > 4)
                {
                    ok = false;
                    lleno = "Mejor me quedo con el machete";
                }
                else if (inv.arma == CombateJugador.Arma.Fierro && inv.durabilidad >= inv.DurabilidadMaxima)
                {
                    ok = false;
                    lleno = "Ya tengo un fierro en buen estado";
                }
                else
                {
                    inv.Equipar(CombateJugador.Arma.Fierro);
                }
                break;
            case TipoObjeto.Machete:
                inv.Equipar(CombateJugador.Arma.Machete);
                break;
            case TipoObjeto.Honda:
                inv.tieneHonda = true;
                inv.Sumar("piedras", Mathf.Max(0, cantidad));
                inv.ranura = Ranura.Honda;
                inv.ActualizarVisual();
                break;
            case TipoObjeto.Revolver:
                inv.tieneRevolver = true;
                inv.balasCargadas = Mathf.Max(inv.balasCargadas, Mathf.Min(cantidad, inv.capacidadTambor));
                inv.ranura = Ranura.Revolver;
                inv.ActualizarVisual();
                break;
            case TipoObjeto.Linterna:
                inv.tieneLinterna = true;
                break;
            case TipoObjeto.ObjetoClave:
                inv.AgregarObjetoClave(idObjeto, NombreMostrado);
                break;
        }

        if (!ok)
        {
            Juego.I.Mensaje(lleno);
            return;
        }

        bool esArma = tipo == TipoObjeto.Palo || tipo == TipoObjeto.Fierro || tipo == TipoObjeto.Machete || tipo == TipoObjeto.Honda || tipo == TipoObjeto.Revolver;
        Juego.I.Mensaje("Recogiste: " + NombreMostrado + (cantidad > 1 && tipo != TipoObjeto.Honda && tipo != TipoObjeto.Revolver ? " x" + cantidad : ""));
        AudioCap1.Play3D(esArma ? "sfx_arma" : "sfx_recoger", transform.position, 0.8f);
        MarcarRecogido();
        alRecoger.Ejecutar(transform.position);
    }

    /// <summary>Desaparece del mundo (al recogerlo o al cargar una partida).</summary>
    public void MarcarRecogido()
    {
        Recogido = true;
        gameObject.SetActive(false);
    }
}
