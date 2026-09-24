using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Paquete de acciones que disparan los eventos del nivel (recoger un objeto, entrar a una
/// zona, abrir una puerta, leer un documento...). Se configura desde el constructor del nivel.
/// </summary>
[System.Serializable]
public class Acciones
{
    [Tooltip("Nuevo objetivo (vacio = no cambia).")]
    public string objetivo;
    [Tooltip("Posicion del objetivo en el mapa / brujula.")]
    public bool usarMarcador;
    public Vector3 marcador;

    [Tooltip("Lineas de subtitulo con formato 'Quien|Texto'.")]
    public string[] subtitulos;

    [Tooltip("Aviso breve en la esquina.")]
    public string mensaje;

    public GameObject[] activar;
    public GameObject[] desactivar;

    [Tooltip("Infectados que empiezan a perseguir al jugador.")]
    public Infectado[] alertar;

    [Tooltip("Infectados que se despiertan y deambulan / investigan.")]
    public Infectado[] despertar;

    public bool puntoControl;
    public string sonido;
    public int moral;

    [Tooltip("Avance del amanecer (0..1). -1 = no cambia.")]
    public float progresoCielo = -1f;

    public PuertaCap abrirPuerta;
    public PuertaCap[] trabarPuertas;
    public PuertaCap[] destrabarPuertas;
    public EventoAsedio iniciarAsedio;
    public bool iniciarFinal;

    [Header("Extras")]
    public CinematicaCamara cinematica;
    public Infectado iniciarJefe;
    [Tooltip("0 nada, 1 se une y sigue, 2 se separa, 3 se esconde (jefe), 4 vuelve a seguir")]
    public int companera;
    public string logro;
    public string flag;
    public string musica;
    [Tooltip("Abre una conversacion en el nodo indicado.")]
    public Conversacion conversacion;
    public string nodoConversacion;

    public void Ejecutar(Vector3 posicion)
    {
        if (Juego.I != null)
        {
            Juego.I.Ejecutar(this, posicion);
        }
    }
}

/// <summary>
/// Director del capitulo: menu, intro, modos (Historia / Supervivencia), dificultad, estados
/// de juego, objetivos con marcador, subtitulos, lecturas, decisiones, puntos de control,
/// guardado, muerte, metricas, logros y final con rango.
/// </summary>
[DefaultExecutionOrder(-50)]
public class Juego : MonoBehaviour
{
    public enum Estado { Menu, Intro, Jugando, Pausa, Lectura, Eleccion, Cinematica, Muerto, Fin, Mapa }
    public enum Modo { Historia, Supervivencia }

    public static Juego I { get; private set; }
    public static bool Control { get { return I != null && I.estado == Estado.Jugando; } }

    // Lo que se pide al recargar la escena
    private static bool empezarDirecto;
    private static bool cargarPartida;
    private static Modo modoPendiente = Modo.Historia;

    [Header("Referencias")]
    public Jugador jugador;
    public CamaraTPS camara;
    public Light sol;
    public ModoSupervivencia supervivencia;

    [Header("Guion")]
    public string tituloCapitulo = "Capitulo 1: Resaca";
    [TextArea] public string[] tarjetasIntro;
    public Acciones alEmpezar = new Acciones();
    [TextArea] public string[] finalHonesto;
    [TextArea] public string[] finalMentira;

    [Header("Cielo: madrugada (0) a amanecer (1)")]
    public Color solInicio = new Color(0.42f, 0.5f, 0.78f);
    public Color solFinal = new Color(1f, 0.76f, 0.58f);
    public float intensidadInicio = 0.18f;
    public float intensidadFinal = 1.2f;
    public Color nieblaInicio = new Color(0.05f, 0.06f, 0.09f);
    public Color nieblaFinal = new Color(0.42f, 0.4f, 0.44f);
    public Color ambienteInicio = new Color(0.07f, 0.08f, 0.12f);
    public Color ambienteFinal = new Color(0.32f, 0.3f, 0.34f);
    public float densidadInicio = 0.05f;
    public float densidadFinal = 0.022f;

    [Header("Estado")]
    public Estado estado = Estado.Menu;
    public Modo modo = Modo.Historia;
    public int moral = 50;
    [Tooltip("Hora del reloj en minutos (05:40 = 340).")]
    public float minutoReloj = 340f;
    [Tooltip("Segundos reales por minuto de reloj.")]
    public float segundosPorMinuto = 12f;

    public string Objetivo { get; private set; } = "";

    /// <summary>
    /// Orden de los objetivos de la historia. Un objetivo de una etapa anterior ya no puede
    /// reemplazar al actual (por ejemplo, leer el celular despues de tener la llave del deposito).
    /// Los textos que no estan en la lista siempre se aplican.
    /// </summary>
    private static readonly string[][] EtapasObjetivo =
    {
        new[] { "Busca tu celular en la barra de la discoteca" },
        new[] { "Sal de la discoteca" },
        new[] { "Consigue la llave del depósito (don Ramiro, el guardia, siempre la carga)" },
        new[] { "Abre el depósito y sal por la puerta de atrás" },
        new[] { "Sal del callejón hacia la avenida 20 de Octubre" },
        new[] { "Llega al edificio de doña Bety: sube las gradas detrás del Mercado Sopocachi" },
        new[] { "Busca la entrada lateral del mercado (del lado de la avenida)" },
        new[] { "Consigue la llave del mercado en la farmacia (cruzando la plaza, al sur)", "Consigue la llave del mercado en la farmacia de doña Julia (cruzando la plaza, al sur)" },
        new[] { "Entra al mercado por la puerta lateral (junto a la avenida)" },
        new[] { "Cruza el mercado y sal por la puerta trasera, hacia las gradas" },
        new[] { "Busca un cortafierro en la ferretería del mercado (al fondo, a la derecha)" },
        new[] { "¡Sobrevive a El Carnicero!" },
        new[] { "Sal por la puerta trasera del mercado y corta la cadena de la reja" },
        new[] { "Sube las gradas hasta el edificio de doña Bety" },
        new[] { "Resiste hasta que doña Bety abra el portón" },
        new[] { "¡Entra al edificio!" }
    };
    private int etapaObjetivo = -1;

    /// <summary>
    /// Pista por etapa: si el jugador pasa mas de 75 s sin avanzar, aparece una pista
    /// (solo con las ayudas en pantalla activadas). Indices = EtapasObjetivo.
    /// </summary>
    private static readonly string[] Pistas =
    {
        "El celular está sobre la barra, junto a las botellas. Acércate y presiona [E].",
        "La cortina principal tiene candado por fuera. Busca otra salida: la puerta del depósito, al fondo junto a la barra.",
        "Don Ramiro estaba en los baños, pasando la tarima del DJ. Agarra algo para defenderte: hay una pata de mesa junto a la tarima.",
        "Usa el llavero en la puerta del depósito. La salida de atrás es la puerta con el foco verde.",
        "Sigue el callejón hacia adelante y dobla por el pasaje hasta la avenida. Revisa el mapa con [M].",
        "Cruza la Plaza Abaroa: el Mercado Sopocachi está detrás de la plaza. Revisa el mapa con [M].",
        "La entrada principal del mercado está bloqueada. La puerta lateral da a la avenida.",
        "La Farmacia Chuquiago está del otro lado de la plaza. La llave está en el fondo de la farmacia.",
        "La puerta lateral del mercado está sobre la avenida. Usa la llave con [E].",
        "La puerta trasera está al fondo del mercado, del lado de las gradas.",
        "La ferretería está en la esquina del fondo del mercado, junto a la puerta trasera. El cortafierro está sobre su mesa.",
        "Esquiva con [Alt] cuando embista: si choca contra un puesto queda aturdido. Pégale por la espalda o quémalo con una molotov.",
        "Sal al patio por la puerta trasera y usa el cortafierro en la reja con cadena.",
        "Sube todas las gradas. El edificio de doña Bety está en la calle de arriba.",
        "Quédate cerca del portón y aguanta: usa molotovs, la honda o el revólver. Cúrate con [H].",
        "El portón de doña Bety ya está abierto. ¡Entra!"
    };
    private float tiempoObjetivo;
    private float siguientePista;

    public static int EtapaDe(string texto)
    {
        if (string.IsNullOrEmpty(texto)) { return -1; }
        for (int i = 0; i < EtapasObjetivo.Length; i++)
        {
            foreach (string t in EtapasObjetivo[i]) { if (t == texto) { return i; } }
        }
        return -1;
    }
    public float ObjetivoCambio { get; private set; } = -100f;
    public bool HayMarcador { get; private set; }
    public Vector3 Marcador { get; private set; }

    private struct Linea { public string quien; public string texto; public float dur; }
    private readonly Queue<Linea> cola = new Queue<Linea>();
    public string SubQuien { get; private set; }
    public string SubTexto { get; private set; }
    private float subFin;

    public readonly List<KeyValuePair<string, float>> Mensajes = new List<KeyValuePair<string, float>>();
    public readonly HashSet<string> Flags = new HashSet<string>();
    public readonly HashSet<string> ZonasVisitadas = new HashSet<string>();

    public string LecturaTitulo { get; private set; }
    public string LecturaTexto { get; private set; }
    public bool LecturaCelular { get; private set; }
    private Acciones alCerrarLectura;
    private Vector3 posLectura;
    private Estado estadoAntesLectura = Estado.Jugando;

    public class Opcion
    {
        public string texto;
        public System.Action accion;
        public bool deshabilitada;
        public Opcion(string t, System.Action a, bool des = false) { texto = t; accion = a; deshabilitada = des; }
    }
    public string EleccionTitulo { get; private set; }
    public string EleccionTexto { get; private set; }
    public string EleccionRetrato { get; private set; }
    public List<Opcion> Opciones { get; private set; } = new List<Opcion>();

    private Vector3 cpPos;
    private Quaternion cpRot;
    private int cpSalud = 100;
    private DatosInventario cpInventario;
    private bool hayCp;

    public int InfectadosEliminados { get; private set; }
    public int DocumentosLeidos { get; private set; }
    public int DocumentosTotales { get; set; }
    public int ColeccionablesTotales { get; set; }
    public float TiempoJugado { get; private set; }
    public float ProgresoCielo { get; private set; }
    private float cieloObjetivo;

    public float Negro { get; private set; } = 1f;
    private float negroObjetivo = 1f;
    private float negroVelocidad = 1f;
    public float TiempoEstado { get; private set; }
    public string Tarjeta { get; private set; }
    public float TarjetaAlpha { get; private set; }
    public float Parpadeo { get; private set; }
    public float Franjas { get; private set; }
    public string MotivoMuerte { get; private set; }
    public string[] TextoFinal { get; private set; }
    public bool FueHonesto { get; private set; }
    public int PuntajeFinal { get; private set; }
    public string RangoFinal { get; private set; }
    public float UltimoGuardado { get; private set; } = -100f;

    private float siguienteZona;

    private void Awake()
    {
        I = this;
        Time.timeScale = 1f;
        global::Opciones.Cargar();
    }

    private void OnDestroy()
    {
        if (I == this)
        {
            I = null;
        }
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (jugador == null) { jugador = FindFirstObjectByType<Jugador>(); }
        if (camara == null) { camara = FindFirstObjectByType<CamaraTPS>(); }
        if (supervivencia == null) { supervivencia = FindFirstObjectByType<ModoSupervivencia>(FindObjectsInactive.Include); }
        DocumentosTotales = FindObjectsByType<Documento>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        ColeccionablesTotales = FindObjectsByType<Coleccionable>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        AplicarCielo(0f);
        global::Opciones.Aplicar();

        if (jugador != null)
        {
            GuardarPunto(false);
        }

        modo = modoPendiente;
        if (cargarPartida)
        {
            cargarPartida = false;
            empezarDirecto = false;
            StartCoroutine(CargarPartidaGuardada());
        }
        else if (empezarDirecto)
        {
            empezarDirecto = false;
            if (modo == Modo.Supervivencia) { StartCoroutine(EmpezarSupervivencia()); }
            else { StartCoroutine(Intro()); }
        }
        else
        {
            modo = Modo.Historia;
            CambiarEstado(Estado.Menu);
            Negro = 0f;
            negroObjetivo = 0f;
        }
    }

    private void CambiarEstado(Estado e)
    {
        estado = e;
        TiempoEstado = 0f;
        Time.timeScale = (e == Estado.Pausa || e == Estado.Lectura || e == Estado.Eleccion || e == Estado.Mapa) ? 0f : 1f;
    }

    private void Update()
    {
        float udt = Time.unscaledDeltaTime;
        TiempoEstado += udt;
        Negro = Mathf.MoveTowards(Negro, negroObjetivo, udt * negroVelocidad);
        Franjas = Mathf.MoveTowards(Franjas, estado == Estado.Cinematica && CinematicaCamara.Activa != null ? 1f : 0f, udt * 2.5f);
        Logros.Actualizar();

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        Gamepad pad = Gamepad.current;
        bool esc = (kb != null && kb.escapeKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame);
        bool teclaMapa = kb != null && kb.mKey.wasPressedThisFrame;

        switch (estado)
        {
            case Estado.Jugando:
                if (esc) { CambiarEstado(Estado.Pausa); }
                else if (teclaMapa && modo == Modo.Historia) { CambiarEstado(Estado.Mapa); AudioCap1.Play2D("sfx_papel", 0.5f); }
                TiempoJugado += Time.deltaTime;
                Stats.D.tiempo = TiempoJugado;
                if (tiempoObjetivo > TiempoJugado) { tiempoObjetivo = TiempoJugado; }
                if (modo == Modo.Historia && global::Opciones.AyudasEnPantalla && etapaObjetivo >= 0 && etapaObjetivo < Pistas.Length
                    && TiempoJugado - tiempoObjetivo > 75f && TiempoJugado >= siguientePista)
                {
                    siguientePista = TiempoJugado + 60f;
                    Mensaje("PISTA: " + Pistas[etapaObjetivo], 9f);
                }
                minutoReloj += Time.deltaTime / segundosPorMinuto;
                if (Time.time >= siguienteZona) { siguienteZona = Time.time + 0.5f; RegistrarZona(); }
                break;
            case Estado.Pausa:
                if (esc) { CambiarEstado(Estado.Jugando); }
                break;
            case Estado.Mapa:
                if (esc || teclaMapa) { CambiarEstado(Estado.Jugando); }
                break;
            case Estado.Lectura:
                bool cerrar = esc || (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    || (mouse != null && mouse.leftButton.wasPressedThisFrame);
                if (cerrar && TiempoEstado > 0.3f) { CerrarLectura(); }
                break;
        }

        // Subtitulos
        if (Time.unscaledTime >= subFin)
        {
            if (cola.Count > 0)
            {
                Linea l = cola.Dequeue();
                SubQuien = l.quien;
                SubTexto = l.texto;
                subFin = Time.unscaledTime + l.dur;
            }
            else
            {
                SubQuien = null;
                SubTexto = null;
            }
        }

        // Mensajes
        for (int i = Mensajes.Count - 1; i >= 0; i--)
        {
            if (Time.unscaledTime > Mensajes[i].Value) { Mensajes.RemoveAt(i); }
        }

        // Amanecer
        if (!Mathf.Approximately(ProgresoCielo, cieloObjetivo))
        {
            ProgresoCielo = Mathf.MoveTowards(ProgresoCielo, cieloObjetivo, Time.deltaTime * 0.03f);
            AplicarCielo(ProgresoCielo);
        }
    }

    private void RegistrarZona()
    {
        if (jugador == null) { return; }
        string z = MapaZonas.ZonaEn(jugador.transform.position);
        if (!string.IsNullOrEmpty(z)) { ZonasVisitadas.Add(z); }
    }

    private void AplicarCielo(float t)
    {
        RenderSettings.fogColor = Color.Lerp(nieblaInicio, nieblaFinal, t);
        RenderSettings.fogDensity = Mathf.Lerp(densidadInicio, densidadFinal, t);
        RenderSettings.ambientLight = Color.Lerp(ambienteInicio, ambienteFinal, t);
        if (sol != null)
        {
            sol.color = Color.Lerp(solInicio, solFinal, t);
            sol.intensity = Mathf.Lerp(intensidadInicio, intensidadFinal, t);
            sol.transform.rotation = Quaternion.Euler(Mathf.Lerp(8f, 24f, t), Mathf.Lerp(-60f, -35f, t), 0f);
        }
        if (camara != null)
        {
            Camera c = camara.GetComponent<Camera>();
            if (c != null) { c.backgroundColor = RenderSettings.fogColor; }
        }
    }

    public void FijarCielo(float t)
    {
        cieloObjetivo = t;
        ProgresoCielo = t;
        AplicarCielo(t);
    }

    // ---------- Menu, intro y modos ----------

    /// <summary>Nueva partida del modo historia con la dificultad elegida.</summary>
    public void Jugar(NivelDificultad dificultad)
    {
        if (estado != Estado.Menu) { return; }
        Dificultad.Nivel = dificultad;
        Stats.Reiniciar();
        modo = Modo.Historia;
        StartCoroutine(Intro());
    }

    public void Jugar()
    {
        Jugar(Dificultad.Nivel);
    }

    public void JugarSupervivencia(NivelDificultad dificultad)
    {
        if (estado != Estado.Menu) { return; }
        Dificultad.Nivel = dificultad;
        Stats.Reiniciar();
        modo = Modo.Supervivencia;
        StartCoroutine(EmpezarSupervivencia());
    }

    /// <summary>Carga la ultima partida guardada (recarga la escena).</summary>
    public void ContinuarPartida()
    {
        if (!Guardado.Existe) { return; }
        Time.timeScale = 1f;
        cargarPartida = true;
        modoPendiente = Modo.Historia;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator CargarPartidaGuardada()
    {
        modo = Modo.Historia;
        CambiarEstado(Estado.Intro);
        Negro = 1f;
        negroObjetivo = 1f;
        yield return null;
        yield return null;
        DatosPartida d = Guardado.Leer();
        if (d == null)
        {
            CambiarEstado(Estado.Menu);
            Negro = 0f; negroObjetivo = 0f;
            yield break;
        }
        Guardado.Aplicar(d, this);
        TiempoJugado = Stats.D.tiempo;
        AudioCap1.Musica(null, 1f);
        if (camara != null) { camara.ColocarDetras(); }
        GuardarPunto(false);
        CambiarEstado(Estado.Jugando);
        negroVelocidad = 0.8f;
        negroObjetivo = 0f;
        Mensaje("Partida cargada  ·  " + Dificultad.NombreActual);
    }

    public void AplicarGuardado(int moralGuardada, float minuto, float cielo, string objetivo, bool marcador, Vector3 posMarcador, IEnumerable<string> flags, IEnumerable<string> zonas)
    {
        moral = moralGuardada;
        minutoReloj = minuto;
        FijarCielo(cielo);
        Objetivo = objetivo ?? "";
        etapaObjetivo = EtapaDe(Objetivo);
        ObjetivoCambio = Time.unscaledTime;
        HayMarcador = marcador;
        Marcador = posMarcador;
        Flags.Clear();
        foreach (string f in flags) { Flags.Add(f); }
        ZonasVisitadas.Clear();
        foreach (string z in zonas) { ZonasVisitadas.Add(z); }
        DocumentosLeidos = Stats.D.archivos;
    }

    private IEnumerator Intro()
    {
        CambiarEstado(Estado.Intro);
        Negro = 1f;
        negroObjetivo = 1f;
        if (jugador != null) { jugador.Agachado = true; }
        AudioCap1.Musica("mus_menu", 0f);
        RetirarRecursosPorDificultad();

        if (tarjetasIntro != null)
        {
            foreach (string t in tarjetasIntro)
            {
                Tarjeta = t;
                for (float k = 0f; k < 3.4f; k += Time.unscaledDeltaTime)
                {
                    TarjetaAlpha = Mathf.Clamp01(Mathf.Min(k / 0.8f, (3.4f - k) / 0.8f));
                    if (SaltarPulsado() && k > 0.5f) { break; }
                    yield return null;
                }
            }
        }
        Tarjeta = null;
        TarjetaAlpha = 0f;

        // Despertar: parpadeos
        AudioCap1.Play2D("sfx_latido", 0.6f);
        AudioCap1.Musica(null, 2f);
        negroVelocidad = 0.35f;
        negroObjetivo = 0f;
        float[] ojos = { 1f, 0.2f, 0.9f, 0.1f, 0.7f, 0f };
        for (int i = 0; i < ojos.Length - 1; i++)
        {
            for (float k = 0f; k < 0.6f; k += Time.unscaledDeltaTime)
            {
                Parpadeo = Mathf.Lerp(ojos[i], ojos[i + 1], k / 0.6f);
                yield return null;
            }
        }
        Parpadeo = 0f;
        negroVelocidad = 1f;
        if (jugador != null) { jugador.Agachado = false; }

        CambiarEstado(Estado.Jugando);
        GuardarPunto(false);
        Ejecutar(alEmpezar, jugador != null ? jugador.transform.position : Vector3.zero);
    }

    private static bool SaltarPulsado()
    {
        Keyboard kb = Keyboard.current;
        return kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame);
    }

    /// <summary>En Superviviente se retiran algunos consumibles del mapa (siempre los mismos).</summary>
    private void RetirarRecursosPorDificultad()
    {
        float f = Dificultad.RecursosRetirados;
        if (f <= 0f) { return; }
        foreach (Recogible r in FindObjectsByType<Recogible>(FindObjectsSortMode.None))
        {
            if (!r.EsConsumible) { continue; }
            int h = Mathf.Abs(r.name.GetHashCode()) % 100;
            if (h < f * 100f) { r.gameObject.SetActive(false); }
        }
    }

    private IEnumerator EmpezarSupervivencia()
    {
        modo = Modo.Supervivencia;
        CambiarEstado(Estado.Intro);
        Negro = 1f;
        negroObjetivo = 1f;
        yield return null;
        if (supervivencia != null) { supervivencia.Preparar(this); }
        FijarCielo(0.05f);
        Tarjeta = "MODO SUPERVIVENCIA\nPlaza Abaroa, 03:00\n\nResiste todas las oleadas que puedas.";
        for (float k = 0f; k < 3.2f; k += Time.unscaledDeltaTime)
        {
            TarjetaAlpha = Mathf.Clamp01(Mathf.Min(k / 0.6f, (3.2f - k) / 0.6f));
            if (SaltarPulsado() && k > 0.5f) { break; }
            yield return null;
        }
        Tarjeta = null;
        TarjetaAlpha = 0f;
        negroVelocidad = 1f;
        negroObjetivo = 0f;
        if (camara != null) { camara.ColocarDetras(); }
        CambiarEstado(Estado.Jugando);
        GuardarPunto(false);
        if (supervivencia != null) { supervivencia.Iniciar(); }
    }

    // ---------- Acciones de eventos ----------

    public void Ejecutar(Acciones a, Vector3 pos)
    {
        if (a == null)
        {
            return;
        }

        if (a.subtitulos != null)
        {
            foreach (string s in a.subtitulos)
            {
                if (string.IsNullOrEmpty(s)) { continue; }
                string[] p = s.Split('|');
                if (p.Length >= 2)
                {
                    float dur = p.Length >= 3 ? float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture) : 0f;
                    Decir(p[0], p[1], dur);
                }
                else
                {
                    Decir("", s, 0f);
                }
            }
        }
        if (!string.IsNullOrEmpty(a.objetivo)) { SetObjetivo(a.objetivo, a.usarMarcador, a.marcador); }
        if (!string.IsNullOrEmpty(a.mensaje)) { Mensaje(a.mensaje); }
        if (a.activar != null) { foreach (var g in a.activar) { if (g != null) { g.SetActive(true); } } }
        if (a.desactivar != null) { foreach (var g in a.desactivar) { if (g != null) { g.SetActive(false); } } }
        if (a.despertar != null) { foreach (var inf in a.despertar) { if (inf != null) { inf.Despertar(pos); } } }
        if (a.alertar != null) { foreach (var inf in a.alertar) { if (inf != null) { inf.Alertar(); } } }
        if (!string.IsNullOrEmpty(a.sonido)) { AudioCap1.Play3D(a.sonido, pos, 1f); }
        if (a.moral != 0) { CambiarMoral(a.moral); }
        if (a.progresoCielo >= 0f) { cieloObjetivo = Mathf.Max(cieloObjetivo, a.progresoCielo); }
        if (a.abrirPuerta != null) { a.abrirPuerta.Abrir(); }
        if (a.trabarPuertas != null) { foreach (var p in a.trabarPuertas) { if (p != null) { p.Cerrar(); p.trabada = true; } } }
        if (a.destrabarPuertas != null) { foreach (var p in a.destrabarPuertas) { if (p != null) { p.trabada = false; } } }
        if (a.iniciarAsedio != null) { a.iniciarAsedio.Iniciar(); }
        if (!string.IsNullOrEmpty(a.flag))
        {
            Flags.Add(a.flag);
            if (Flags.Contains("ayuda_freddy") && Flags.Contains("ayuda_chino")) { Logros.Desbloquear("ayni"); }
        }
        if (!string.IsNullOrEmpty(a.logro)) { Logros.Desbloquear(a.logro); }
        if (!string.IsNullOrEmpty(a.musica)) { AudioCap1.Musica(a.musica == "-" ? null : a.musica, 2f); }
        if (a.companera != 0 && Companera.I != null) { Companera.I.Orden(a.companera); }
        if (a.puntoControl) { GuardarPunto(true); }
        if (a.cinematica != null) { a.cinematica.Reproducir(a.iniciarJefe); }
        else if (a.iniciarJefe != null) { a.iniciarJefe.IniciarJefe(); }
        if (a.iniciarFinal) { StartCoroutine(Final()); }
        if (a.conversacion != null) { a.conversacion.Mostrar(string.IsNullOrEmpty(a.nodoConversacion) ? a.conversacion.nodoInicio : a.nodoConversacion); }
    }

    public void SetObjetivo(string texto, bool marcador = false, Vector3 pos = default(Vector3))
    {
        int etapa = EtapaDe(texto);
        if (etapa >= 0)
        {
            if (etapa < etapaObjetivo) { return; }
            etapaObjetivo = etapa;
        }
        tiempoObjetivo = TiempoJugado;
        siguientePista = 0f;
        Objetivo = texto;
        ObjetivoCambio = Time.unscaledTime;
        HayMarcador = marcador;
        Marcador = pos;
        AudioCap1.Play2D("sfx_objetivo", 0.5f);
    }

    public void Decir(string quien, string texto, float duracion = 0f)
    {
        if (duracion <= 0f)
        {
            duracion = Mathf.Clamp(1.2f + texto.Length * 0.055f, 2.4f, 7f);
        }
        cola.Enqueue(new Linea { quien = quien, texto = texto, dur = duracion });
    }

    public bool HablandoAlguien { get { return SubTexto != null || cola.Count > 0; } }

    public void Mensaje(string texto)
    {
        Mensaje(texto, 4.5f);
    }

    public void Mensaje(string texto, float duracion)
    {
        if (string.IsNullOrEmpty(texto)) { return; }
        for (int i = 0; i < Mensajes.Count; i++)
        {
            if (Mensajes[i].Key == texto) { Mensajes.RemoveAt(i); break; }
        }
        Mensajes.Add(new KeyValuePair<string, float>(texto, Time.unscaledTime + duracion));
        while (Mensajes.Count > 5) { Mensajes.RemoveAt(0); }
    }

    public void CambiarMoral(int delta)
    {
        moral = Mathf.Clamp(moral + delta, 0, 100);
        Mensaje(delta > 0 ? "Tu conciencia está tranquila (+" + delta + " moral)" : "Algo se rompe por dentro (" + delta + " moral)");
    }

    // ---------- Lectura ----------

    public void Leer(string titulo, string texto, bool celular, Acciones despues, Vector3 pos)
    {
        LecturaTitulo = titulo;
        LecturaTexto = texto;
        LecturaCelular = celular;
        alCerrarLectura = despues;
        posLectura = pos;
        estadoAntesLectura = estado == Estado.Pausa ? Estado.Pausa : Estado.Jugando;
        AudioCap1.Play2D(celular ? "sfx_celular" : "sfx_papel", 0.7f);
        CambiarEstado(Estado.Lectura);
    }

    public void RegistrarDocumento()
    {
        DocumentosLeidos++;
        Stats.D.archivos = DocumentosLeidos;
        if (DocumentosTotales > 0 && DocumentosLeidos >= DocumentosTotales) { Logros.Desbloquear("archivos"); }
    }

    public void RegistrarColeccionable()
    {
        Stats.D.coleccionables++;
        if (ColeccionablesTotales > 0 && Stats.D.coleccionables >= ColeccionablesTotales) { Logros.Desbloquear("illas"); }
    }

    private void CerrarLectura()
    {
        CambiarEstado(estadoAntesLectura);
        Acciones a = alCerrarLectura;
        alCerrarLectura = null;
        if (a != null) { Ejecutar(a, posLectura); }
    }

    // ---------- Decisiones ----------

    public void MostrarEleccion(string titulo, string texto, string retrato, List<Opcion> opciones)
    {
        EleccionTitulo = titulo;
        EleccionTexto = texto;
        EleccionRetrato = retrato;
        Opciones = opciones;
        CambiarEstado(Estado.Eleccion);
    }

    public void Elegir(int i)
    {
        if (estado != Estado.Eleccion || i < 0 || i >= Opciones.Count)
        {
            return;
        }
        Opcion op = Opciones[i];
        if (op.deshabilitada)
        {
            AudioCap1.Play2D("sfx_vacio", 0.5f);
            return;
        }
        Opciones = new List<Opcion>();
        CambiarEstado(Estado.Jugando);
        AudioCap1.Play2D("sfx_objetivo", 0.4f);
        if (op.accion != null) { op.accion(); }
    }

    // ---------- Cinematicas ----------

    public void EmpezarCinematica()
    {
        CambiarEstado(Estado.Cinematica);
    }

    public void TerminarCinematica()
    {
        if (estado == Estado.Cinematica) { CambiarEstado(Estado.Jugando); }
    }

    // ---------- Puntos de control, guardado, muerte, pausa ----------

    public void GuardarPunto(bool avisar)
    {
        if (jugador == null || jugador.Muerto)
        {
            return;
        }
        cpPos = jugador.transform.position;
        cpRot = jugador.transform.rotation;
        cpSalud = jugador.Salud;
        cpInventario = jugador.Inventario != null ? jugador.Inventario.Exportar() : null;
        hayCp = true;
        if (avisar) { Mensaje("Punto de control"); }
    }

    /// <summary>Guardado permanente (altares con velas).</summary>
    public bool GuardarPartida(string lugar)
    {
        if (modo != Modo.Historia || jugador == null || jugador.Muerto) { return false; }
        if (Infectado.AlgunoPersiguiendo)
        {
            Mensaje("No puedo guardar con infectados persiguiéndome");
            return false;
        }
        Stats.D.guardados++;
        Stats.D.tiempo = TiempoJugado;
        Guardado.Escribir(this, lugar);
        GuardarPunto(false);
        UltimoGuardado = Time.unscaledTime;
        Mensaje("Partida guardada  ·  " + lugar);
        return true;
    }

    public void Continuar()
    {
        if (estado == Estado.Pausa || estado == Estado.Mapa) { CambiarEstado(Estado.Jugando); }
    }

    public void AbrirMapa()
    {
        if (estado == Estado.Pausa) { CambiarEstado(Estado.Mapa); }
    }

    public void JugadorMuerto(string motivo)
    {
        if (estado == Estado.Muerto)
        {
            return;
        }
        MotivoMuerte = motivo;
        StartCoroutine(Muerte());
    }

    private IEnumerator Muerte()
    {
        CambiarEstado(Estado.Muerto);
        AudioCap1.Play2D("sfx_muerte", 0.9f);
        if (modo == Modo.Supervivencia && supervivencia != null) { supervivencia.Terminar(); }
        yield return new WaitForSeconds(1.6f);
        negroVelocidad = 0.8f;
        negroObjetivo = 0.85f;
    }

    public void Reintentar()
    {
        if (modo == Modo.Supervivencia)
        {
            Time.timeScale = 1f;
            empezarDirecto = true;
            modoPendiente = Modo.Supervivencia;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        if (!hayCp || jugador == null)
        {
            JugarDeNuevo();
            return;
        }
        Time.timeScale = 1f;
        jugador.Revivir(cpPos, cpRot, Mathf.Max(cpSalud, 60));
        if (cpInventario != null && jugador.Inventario != null) { jugador.Inventario.Importar(cpInventario); }
        foreach (Infectado inf in Infectado.Todos.ToArray()) { inf.Reiniciar(); }
        foreach (EventoAsedio ev in FindObjectsByType<EventoAsedio>(FindObjectsSortMode.None)) { ev.Reiniciar(); }
        if (Companera.I != null) { Companera.I.TeletransportarCerca(); }
        if (camara != null) { camara.ColocarDetras(); }
        Negro = 1f;
        negroVelocidad = 1.2f;
        negroObjetivo = 0f;
        CambiarEstado(Estado.Jugando);
        Mensaje("Reintentando desde el último punto de control");
    }

    public void JugarDeNuevo()
    {
        Time.timeScale = 1f;
        empezarDirecto = true;
        modoPendiente = modo;
        Stats.Reiniciar();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        empezarDirecto = false;
        modoPendiente = Modo.Historia;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RegistrarMuerteInfectado(Infectado inf)
    {
        InfectadosEliminados++;
        if (modo == Modo.Supervivencia && supervivencia != null) { supervivencia.RegistrarBaja(inf); }
    }

    // ---------- Final ----------

    private IEnumerator Final()
    {
        CambiarEstado(Estado.Cinematica);
        AudioCap1.Musica(null, 2f);
        negroVelocidad = 0.6f;
        negroObjetivo = 1f;
        yield return new WaitForSeconds(2f);
        foreach (Infectado inf in Infectado.Todos.ToArray()) { if (inf != null) { inf.gameObject.SetActive(false); } }

        bool elegido = false;
        var ops = new List<Opcion>
        {
            new Opcion("\"Estoy bien, doña Bety. No me hicieron nada.\"", () => { FueHonesto = false; elegido = true; }),
            new Opcion("\"Me... me rasguñó uno. En el brazo.\"", () => { FueHonesto = true; elegido = true; })
        };
        string extra = Flags.Contains("wara_se_fue") ? "\n\n\"¿Y la chica cebra que te acompañaba? ...Ya. Dios la cuide.\"" : "";
        MostrarEleccion("Doña Beatriz", "Bety cierra el portón con tres candados. Le tiemblan las manos. Te mira el brazo, la sangre en la manga." + extra + "\n\n\"Hijito... dime la verdad. ¿Te mordieron?\"", "bety", ops);
        while (!elegido) { yield return null; }

        CambiarEstado(Estado.Cinematica);
        TextoFinal = FueHonesto ? finalHonesto : finalMentira;
        if (FueHonesto) { moral = Mathf.Clamp(moral + 10, 0, 100); Logros.Desbloquear("verdad"); }
        if (TextoFinal != null)
        {
            foreach (string t in TextoFinal)
            {
                Tarjeta = t;
                float dur = Mathf.Clamp(2.5f + t.Length * 0.045f, 3.5f, 8f);
                for (float k = 0f; k < dur; k += Time.unscaledDeltaTime)
                {
                    TarjetaAlpha = Mathf.Clamp01(Mathf.Min(k / 0.8f, (dur - k) / 0.8f));
                    if (SaltarPulsado() && k > 0.8f) { break; }
                    yield return null;
                }
            }
        }
        Tarjeta = null;

        // Metricas y rango
        Stats.D.tiempo = TiempoJugado;
        Stats.D.moralFinal = moral;
        Stats.D.dijoVerdad = FueHonesto;
        Stats.D.archivos = DocumentosLeidos;
        PuntajeFinal = Stats.Puntaje(DocumentosTotales, ColeccionablesTotales);
        RangoFinal = Stats.Rango(PuntajeFinal);
        Records.GuardarHistoria(RangoFinal, PuntajeFinal, TiempoJugado);
        if (Stats.D.muertes == 0) { Logros.Desbloquear("sin_morir"); }
        if (Dificultad.Nivel == NivelDificultad.Superviviente) { Logros.Desbloquear("superviviente"); }
        if (RangoFinal == "S") { Logros.Desbloquear("rango_s"); }
        if (Stats.D.TotalKills <= 6) { Logros.Desbloquear("pacifista"); }
        Guardado.Borrar();

        AudioCap1.Musica("mus_menu", 3f);
        CambiarEstado(Estado.Fin);
    }
}
