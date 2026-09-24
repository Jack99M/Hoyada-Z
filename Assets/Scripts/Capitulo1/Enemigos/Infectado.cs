using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Infectado de la "Cepa del Valle". IA con NavMesh, vista y oido:
/// - Comun: lento, ve y oye. Se puede matar por la espalda. A veces te agarra.
/// - Corredor: recien infectado, rapido y con buena vista.
/// - Fungico: ciego, placas en la piel, oye muchisimo y hace "clics". Si te agarra, te mata
///   (salvo que tengas una punta). Solo se mata en sigilo con una punta.
/// - Griton: al verte grita y alerta a todos los infectados de alrededor. Matalo primero.
/// - Carnicero: jefe del Mercado Sopocachi (logica en Infectado_Jefe.cs).
/// Estados: quieto, deambular, comiendo, dormido, investigar, buscar, perseguir, atacar,
/// agarrando, gritando, quemandose, aturdido, sigilo, muerto.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public partial class Infectado : MonoBehaviour
{
    public enum Tipo { Comun, Corredor, Fungico, Griton, Carnicero }
    public enum Estado { Quieto, Deambular, Comiendo, Dormido, Investigar, Buscar, Perseguir, Atacar, Aturdido, Sigilo, Muerto, Agarrando, Gritando, Quemandose }

    public static readonly List<Infectado> Todos = new List<Infectado>();

    public static bool AlgunoPersiguiendo
    {
        get
        {
            foreach (Infectado i in Todos) { if (i != null && i.Persiguiendo) { return true; } }
            return false;
        }
    }

    [Header("Configuracion")]
    public Tipo tipo = Tipo.Comun;
    public Estado estadoInicial = Estado.Deambular;
    public float radioDeambular = 6f;
    [Tooltip("Comiendo / dormido: distancia a la que se levanta si Mateo se acerca de pie.")]
    public float despertarPorProximidad = 2.6f;
    [Tooltip("Objeto que aparece donde muere (por ejemplo, una llave).")]
    public GameObject soltarAlMorir;
    public Acciones alMorir = new Acciones();

    public float Vida { get; private set; }
    public float VidaMax { get; private set; }
    public Estado EstadoActual { get; private set; }
    public float Deteccion { get; private set; }
    public float UltimoAviso { get; private set; } = -100f;
    public float Escala { get; private set; } = 1f;
    public FuenteDano UltimaFuente { get; private set; } = FuenteDano.Otro;

    public bool Muerto { get { return EstadoActual == Estado.Muerto; } }
    public bool Persiguiendo { get { return EstadoActual == Estado.Perseguir || EstadoActual == Estado.Atacar || EstadoActual == Estado.Agarrando || EstadoActual == Estado.Gritando || (tipo == Tipo.Carnicero && JefeActivo); } }
    public bool Agarrando { get { return EstadoActual == Estado.Agarrando; } }
    public bool Quemandose { get { return Time.time < quemadoHasta && !Muerto; } }
    public float AlturaCabeza { get { return 1.58f * Escala; } }
    public float AlturaPecho { get { return 1.2f * Escala; } }
    public float RadioExtra { get { return tipo == Tipo.Carnicero ? 0.45f : 0f; } }
    public int DanoMordida { get { return tipo == Tipo.Corredor ? 24 : (tipo == Tipo.Griton ? 18 : 30); } }
    /// <summary>Se mueve o hace ruido (para el modo escucha).</summary>
    public bool HaceRuido
    {
        get
        {
            if (Muerto) { return false; }
            if (tipo == Tipo.Fungico || tipo == Tipo.Carnicero || Persiguiendo) { return true; }
            return agente != null && agente.enabled && agente.velocity.sqrMagnitude > 0.04f || EstadoActual == Estado.Comiendo;
        }
    }

    public bool PuedeSerSigiloso(bool conPunta)
    {
        if (tipo == Tipo.Carnicero || Muerto || !isActiveAndEnabled) { return false; }
        if (tipo == Tipo.Fungico && !conPunta) { return false; }
        return EstadoActual != Estado.Perseguir && EstadoActual != Estado.Atacar && EstadoActual != Estado.Sigilo
               && EstadoActual != Estado.Agarrando && EstadoActual != Estado.Gritando && EstadoActual != Estado.Quemandose;
    }

    private float velDeambular, velInvestigar, velPerseguir, rangoVision, anguloVision, oido, cadencia;
    private int dano;
    private const float Alcance = 1.6f;

    private NavMeshAgent agente;
    private AnimProcedural anim;
    private Jugador jugador;
    private Collider[] colisiones;
    private Vector3 origen;
    private Quaternion rotOrigen;
    private float tEstado;
    private float esperaHasta;
    private Vector3 destino;
    private Vector3 ultimaPosVista;
    private float ultimoVisto;
    private float siguienteAtaque;
    private bool ataqueResuelto;
    private float duracionAturdido;
    private float duracionSigilo;
    private float siguienteRepath;
    private float siguienteSonido;
    private int capaObstaculos;
    private bool yaGrito;
    private float quemadoHasta;
    private float siguienteQuemadura;
    private GameObject fuegoVisual;
    private bool configurado;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<AnimProcedural>();
        colisiones = GetComponentsInChildren<Collider>();
        origen = transform.position;
        rotOrigen = transform.rotation;
        capaObstaculos = ~((1 << 8) | (1 << 9) | (1 << 2));
        EsqueletoHumanoide e = GetComponentInChildren<EsqueletoHumanoide>();
        Escala = e != null ? Mathf.Max(0.5f, e.transform.localScale.y) : 1f;
        Configurar();
        Vida = VidaMax;
    }

    private void OnEnable()
    {
        if (!Todos.Contains(this)) { Todos.Add(this); }
        SistemaRuido.AlEmitir += OirRuido;
    }

    private void OnDisable()
    {
        Todos.Remove(this);
        SistemaRuido.AlEmitir -= OirRuido;
        if (JefeEnCombate == this) { JefeEnCombate = null; }
    }

    private void Start()
    {
        if (jugador == null) { jugador = Jugador.I != null ? Jugador.I : FindFirstObjectByType<Jugador>(); }
        if (!configurado) { CambiarEstado(tipo == Tipo.Carnicero ? Estado.Quieto : estadoInicial); }
        siguienteSonido = Time.time + Random.Range(1f, 5f);
    }

    private void Configurar()
    {
        switch (tipo)
        {
            case Tipo.Corredor:
                VidaMax = 50f; velDeambular = 1.2f; velInvestigar = 2.4f; velPerseguir = 5.1f;
                rangoVision = 16f; anguloVision = 130f; oido = 1.2f; dano = 10; cadencia = 1.25f;
                break;
            case Tipo.Fungico:
                VidaMax = 150f; velDeambular = 0.75f; velInvestigar = 1.3f; velPerseguir = 3.1f;
                rangoVision = 0f; anguloVision = 0f; oido = 1.8f; dano = 40; cadencia = 2.2f;
                break;
            case Tipo.Griton:
                VidaMax = 45f; velDeambular = 0.8f; velInvestigar = 1.4f; velPerseguir = 3.0f;
                rangoVision = 15f; anguloVision = 120f; oido = 1.1f; dano = 9; cadencia = 1.6f;
                break;
            case Tipo.Carnicero:
                ConfigurarJefe();
                break;
            default:
                VidaMax = 70f; velDeambular = 0.85f; velInvestigar = 1.5f; velPerseguir = 3.4f;
                rangoVision = 13f; anguloVision = 110f; oido = 1f; dano = 13; cadencia = 1.7f;
                break;
        }

        agente.angularSpeed = 420f;
        agente.acceleration = tipo == Tipo.Carnicero ? 18f : 12f;
        agente.radius = tipo == Tipo.Carnicero ? 0.55f : 0.32f;
        agente.height = 1.75f * Escala;
        agente.stoppingDistance = 0.1f;
        agente.autoBraking = true;

        if (anim != null)
        {
            anim.estilo = AnimProcedural.Estilo.Infectado;
            anim.velocidadCaminar = 0.9f;
            anim.velocidadCorrer = tipo == Tipo.Corredor ? 5f : 3.4f;
            anim.cojera = tipo == Tipo.Corredor ? 0.05f : (tipo == Tipo.Fungico ? 0.5f : (tipo == Tipo.Carnicero ? 0.2f : 0.35f));
        }
    }

    private void CambiarEstado(Estado e)
    {
        configurado = true;
        EstadoActual = e;
        tEstado = 0f;
        if (!agente.enabled || !agente.isOnNavMesh)
        {
            return;
        }

        switch (e)
        {
            case Estado.Quieto:
            case Estado.Comiendo:
            case Estado.Dormido:
            case Estado.Aturdido:
            case Estado.Sigilo:
            case Estado.Atacar:
            case Estado.Agarrando:
            case Estado.Gritando:
                agente.isStopped = true;
                agente.ResetPath();
                break;
            case Estado.Deambular:
                agente.isStopped = false;
                agente.stoppingDistance = 0.1f;
                agente.speed = velDeambular;
                esperaHasta = Time.time + Random.Range(0.5f, 2.5f);
                break;
            case Estado.Investigar:
                agente.isStopped = false;
                agente.stoppingDistance = 0.1f;
                agente.speed = velInvestigar;
                agente.SetDestination(destino);
                break;
            case Estado.Buscar:
                agente.isStopped = true;
                break;
            case Estado.Perseguir:
                agente.isStopped = false;
                // Se detiene a un paso del jugador para no empujarlo (alcance de ataque 1.6 m).
                agente.stoppingDistance = 1.05f;
                agente.speed = velPerseguir;
                siguienteRepath = 0f;
                break;
            case Estado.Quemandose:
                agente.isStopped = false;
                agente.stoppingDistance = 0.1f;
                agente.speed = velInvestigar * 1.3f;
                break;
        }
    }

    private void Update()
    {
        if (Muerto)
        {
            return;
        }
        if (jugador == null) { jugador = Jugador.I; }

        float dt = Time.deltaTime;
        tEstado += dt;

        if (tipo == Tipo.Carnicero)
        {
            ActualizarJefe(dt);
            return;
        }

        ActualizarFuego(dt);
        if (Muerto)
        {
            return;
        }

        if (EstadoActual == Estado.Sigilo)
        {
            if (tEstado >= duracionSigilo) { Morir(Vector3.zero, true, FuenteDano.Sigilo); }
            ActualizarAnim();
            return;
        }

        if (EstadoActual == Estado.Aturdido)
        {
            if (tEstado >= duracionAturdido)
            {
                CambiarEstado(Estado.Perseguir);
                MarcarJugadorVisto();
            }
            ActualizarAnim();
            return;
        }

        if (EstadoActual == Estado.Quemandose)
        {
            if (!Quemandose)
            {
                CambiarEstado(Estado.Perseguir);
                MarcarJugadorVisto();
            }
            else if (agente.enabled && agente.isOnNavMesh && (!agente.hasPath || agente.remainingDistance < 0.5f))
            {
                Vector2 r = Random.insideUnitCircle * 4f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position + new Vector3(r.x, 0f, r.y), out hit, 2f, NavMesh.AllAreas)) { agente.SetDestination(hit.position); }
            }
            ActualizarAnim();
            return;
        }

        if (EstadoActual == Estado.Gritando)
        {
            GirarHacia(jugador != null ? jugador.transform.position : transform.position + transform.forward, 6f);
            if (tEstado >= 1.6f)
            {
                CambiarEstado(Estado.Perseguir);
                MarcarJugadorVisto();
            }
            ActualizarAnim();
            return;
        }

        if (EstadoActual == Estado.Agarrando)
        {
            if (jugador != null) { GirarHacia(jugador.transform.position, 10f); }
            if (jugador == null || jugador.Agarrador != this && tEstado > 0.3f || tEstado > 4.5f)
            {
                siguienteAtaque = Time.time + 1.5f;
                CambiarEstado(Estado.Perseguir);
            }
            ActualizarAnim();
            return;
        }

        Percepcion(dt);

        switch (EstadoActual)
        {
            case Estado.Quieto:
                GirarHacia(transform.position + rotOrigen * Vector3.forward, 2f);
                break;
            case Estado.Comiendo:
            case Estado.Dormido:
                RevisarProximidad();
                break;
            case Estado.Deambular:
                Deambular();
                break;
            case Estado.Investigar:
                if (!agente.pathPending && agente.remainingDistance < 0.6f)
                {
                    CambiarEstado(Estado.Buscar);
                }
                else if (tEstado > 15f)
                {
                    VolverAOrigen();
                }
                break;
            case Estado.Buscar:
                transform.Rotate(0f, Mathf.Sin(tEstado * 1.3f) * 70f * dt, 0f);
                if (tEstado > 4f) { VolverAOrigen(); }
                break;
            case Estado.Perseguir:
                Perseguir();
                break;
            case Estado.Atacar:
                Atacar();
                break;
        }

        Sonidos();
        ActualizarAnim();
    }

    private void ActualizarAnim()
    {
        if (anim == null)
        {
            return;
        }
        anim.velocidad = agente.enabled ? agente.velocity.magnitude : 0f;
        anim.alerta = Persiguiendo || EstadoActual == Estado.Quemandose || EstadoActual == Estado.Investigar && tipo == Tipo.Corredor;
        anim.agachado = EstadoActual == Estado.Comiendo || EstadoActual == Estado.Dormido;
    }

    // ---------- Percepcion ----------

    private void Percepcion(float dt)
    {
        if (jugador == null || jugador.Muerto)
        {
            if (Persiguiendo) { VolverAOrigen(); }
            Deteccion = Mathf.MoveTowards(Deteccion, 0f, dt);
            return;
        }

        bool ve = PuedeVer();
        if (Persiguiendo)
        {
            if (ve) { MarcarJugadorVisto(); }
            return;
        }

        if (ve)
        {
            float dist = Vector3.Distance(transform.position, jugador.transform.position);
            float alcance = Mathf.Max(1f, rangoVision * jugador.Visibilidad);
            float cerca = 1f - Mathf.Clamp01(dist / alcance);
            float ritmo = Mathf.Lerp(0.6f, 4.5f, cerca) * Dificultad.Deteccion;
            if (EstadoActual == Estado.Investigar || EstadoActual == Estado.Buscar) { ritmo *= 1.8f; }
            if (EstadoActual == Estado.Comiendo) { ritmo *= 0.45f; }
            Deteccion += ritmo * dt;

            if (Deteccion >= 1f)
            {
                Stats.D.vecesVisto++;
                Alertar();
            }
            else if (Deteccion > 0.3f && (EstadoActual == Estado.Quieto || EstadoActual == Estado.Deambular || EstadoActual == Estado.Buscar))
            {
                // Sospecha: se detiene y mira hacia el jugador
                if (agente.enabled && agente.isOnNavMesh) { agente.isStopped = true; }
                GirarHacia(jugador.transform.position, 3f);
            }
        }
        else
        {
            Deteccion = Mathf.MoveTowards(Deteccion, 0f, dt * 0.3f);
            if (Deteccion < 0.2f && EstadoActual == Estado.Deambular && agente.enabled && agente.isOnNavMesh && agente.isStopped)
            {
                agente.isStopped = false;
            }
        }
    }

    private bool PuedeVer()
    {
        if (rangoVision <= 0f || EstadoActual == Estado.Dormido || jugador == null)
        {
            return false;
        }

        Vector3 ojos = transform.position + Vector3.up * 1.6f * Escala;
        Vector3 hacia = jugador.Pecho - ojos;
        float dist = hacia.magnitude;
        float alcance = rangoVision * jugador.Visibilidad * (EstadoActual == Estado.Comiendo ? 0.5f : 1f) * (Persiguiendo ? 1.7f : 1f);
        if (dist > alcance)
        {
            return false;
        }

        Vector3 plano = new Vector3(hacia.x, 0f, hacia.z);
        float sentidoCercano = jugador.Agachado || EstadoActual == Estado.Comiendo ? 0f : 1.3f;
        if (!Persiguiendo && dist > sentidoCercano && Vector3.Angle(transform.forward, plano) > anguloVision * 0.5f)
        {
            return false;
        }

        return !Physics.Raycast(ojos, hacia / dist, dist - 0.2f, capaObstaculos, QueryTriggerInteraction.Ignore);
    }

    private void MarcarJugadorVisto()
    {
        if (jugador == null) { jugador = Jugador.I; }
        if (jugador == null)
        {
            return;
        }
        ultimaPosVista = jugador.transform.position;
        ultimoVisto = Time.time;
    }

    private void OirRuido(Vector3 pos, float radio, bool delJugador)
    {
        if (Muerto || EstadoActual == Estado.Sigilo || !isActiveAndEnabled)
        {
            return;
        }
        if (tipo == Tipo.Carnicero)
        {
            JefeOirRuido(pos);
            return;
        }
        if (EstadoActual == Estado.Quemandose || EstadoActual == Estado.Agarrando || EstadoActual == Estado.Gritando)
        {
            return;
        }

        float alcance = radio * oido;
        if (EstadoActual == Estado.Comiendo) { alcance *= 0.6f; }
        if (EstadoActual == Estado.Dormido) { alcance *= 0.7f; }
        float dist = Vector3.Distance(pos, transform.position);
        if (dist > alcance)
        {
            return;
        }

        if (Persiguiendo)
        {
            if (delJugador && tipo == Tipo.Fungico)
            {
                ultimaPosVista = pos;
                ultimoVisto = Time.time;
            }
            return;
        }

        bool jugadorCerca = delJugador && jugador != null && Vector3.Distance(pos, jugador.transform.position) < 2.5f;
        if (jugadorCerca && (dist < alcance * 0.35f || (tipo == Tipo.Fungico && dist < alcance * 0.55f)))
        {
            Alertar();
        }
        else
        {
            Investigar(pos);
        }
    }

    // ---------- Comportamientos ----------

    private void RevisarProximidad()
    {
        if (jugador == null || jugador.Muerto)
        {
            return;
        }
        float dist = Vector3.Distance(transform.position, jugador.transform.position);
        float umbral = jugador.Agachado ? despertarPorProximidad * 0.45f : despertarPorProximidad;
        if (dist < umbral)
        {
            if (EstadoActual == Estado.Dormido)
            {
                AudioCap1.Play3D("sfx_grito", transform.position + Vector3.up, 1f);
                AudioCap1.Play2D("sfx_susto", 0.7f);
                CamaraTPS.Sacudir(0.3f, 0.35f);
            }
            Alertar();
        }
    }

    private void Deambular()
    {
        if (!agente.enabled || !agente.isOnNavMesh || agente.isStopped)
        {
            return;
        }
        if (!agente.pathPending && agente.remainingDistance < 0.4f && Time.time >= esperaHasta)
        {
            Vector2 r = Random.insideUnitCircle * radioDeambular;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(origen + new Vector3(r.x, 0f, r.y), out hit, 2f, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
            }
            esperaHasta = Time.time + Random.Range(3f, 7f);
        }
    }

    public void Investigar(Vector3 pos)
    {
        if (Muerto || Persiguiendo || EstadoActual == Estado.Sigilo || EstadoActual == Estado.Aturdido || EstadoActual == Estado.Quemandose || tipo == Tipo.Carnicero)
        {
            return;
        }
        NavMeshHit hit;
        destino = NavMesh.SamplePosition(pos, out hit, 3f, NavMesh.AllAreas) ? hit.position : pos;
        Deteccion = Mathf.Max(Deteccion, 0.3f);
        CambiarEstado(Estado.Investigar);
        if (Random.value < 0.6f) { AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up, 0.5f, Random.Range(0.85f, 1.1f)); }
    }

    /// <summary>Lo despierta (si estaba quieto/comiendo/dormido) y lo manda a investigar.</summary>
    public void Despertar(Vector3 pos)
    {
        if (Muerto || Persiguiendo || tipo == Tipo.Carnicero)
        {
            return;
        }
        if (!gameObject.activeSelf) { gameObject.SetActive(true); }
        Investigar(jugador != null ? jugador.transform.position : pos);
    }

    private void VolverAOrigen()
    {
        Deteccion = 0f;
        CambiarEstado(Estado.Deambular);
        if (agente.enabled && agente.isOnNavMesh) { agente.SetDestination(origen); }
        if (estadoInicial != Estado.Deambular && estadoInicial != Estado.Investigar) { esperaHasta = Time.time + 30f; }
    }

    public void Alertar()
    {
        if (Muerto)
        {
            return;
        }
        if (tipo == Tipo.Carnicero)
        {
            if (JefeActivo) { MarcarJugadorVisto(); }
            return;
        }
        if (!gameObject.activeSelf) { gameObject.SetActive(true); }
        if (EstadoActual == Estado.Quemandose || EstadoActual == Estado.Agarrando)
        {
            return;
        }
        bool nuevo = !Persiguiendo;
        Deteccion = 1f;
        MarcarJugadorVisto();
        if (nuevo)
        {
            UltimoAviso = Time.time;
            if (tipo == Tipo.Griton && !yaGrito)
            {
                Gritar();
                return;
            }
            AudioCap1.Play3D(tipo == Tipo.Fungico ? "sfx_chasquido" : "sfx_grito", transform.position + Vector3.up, 1f, Random.Range(0.9f, 1.1f));
            SistemaRuido.Emitir(transform.position, 8f, false);
        }
        if (EstadoActual != Estado.Atacar) { CambiarEstado(Estado.Perseguir); }
    }

    private void Gritar()
    {
        yaGrito = true;
        CambiarEstado(Estado.Gritando);
        if (anim != null) { anim.Aturdir(); }
        AudioCap1.Play3D("sfx_grito_griton", transform.position + Vector3.up * 1.6f, 1f);
        if (jugador != null && Vector3.Distance(jugador.transform.position, transform.position) < 20f) { CamaraTPS.Sacudir(0.25f, 0.8f); }
        if (Juego.I != null) { Juego.I.Mensaje("¡Un gritón! Todos los infectados de alrededor te escucharon"); }
        foreach (Infectado otro in Todos.ToArray())
        {
            if (otro == null || otro == this || otro.Muerto || otro.tipo == Tipo.Carnicero) { continue; }
            if (Vector3.Distance(otro.transform.position, transform.position) < 28f)
            {
                otro.Alertar();
            }
        }
    }

    private void Perseguir()
    {
        if (jugador == null || !agente.enabled || !agente.isOnNavMesh)
        {
            return;
        }

        float perder = tipo == Tipo.Fungico ? 4f : 7f;
        if (Time.time - ultimoVisto > perder)
        {
            Investigar(ultimaPosVista);
            return;
        }

        Vector3 meta = Time.time - ultimoVisto < 0.6f ? jugador.transform.position : ultimaPosVista;
        if (Time.time >= siguienteRepath)
        {
            siguienteRepath = Time.time + 0.15f;
            agente.SetDestination(meta);
        }

        float dist = Vector3.Distance(transform.position, jugador.transform.position);
        if (dist < 2.5f) { GirarHacia(jugador.transform.position, 10f); }
        if (dist < Alcance && Time.time >= siguienteAtaque && Mathf.Abs(jugador.transform.position.y - transform.position.y) < 1.2f && !jugador.Muerto)
        {
            ataqueResuelto = false;
            CambiarEstado(Estado.Atacar);
            if (anim != null) { anim.AtacarInfectado(); }
            AudioCap1.Play3D(tipo == Tipo.Fungico ? "sfx_chasquido" : "sfx_grunido", transform.position + Vector3.up, 0.8f, 1.15f);
        }
    }

    private void Atacar()
    {
        if (jugador == null)
        {
            CambiarEstado(Estado.Perseguir);
            return;
        }

        GirarHacia(jugador.transform.position, 8f);
        if (!ataqueResuelto && tEstado >= 0.38f)
        {
            ataqueResuelto = true;
            Vector3 hacia = jugador.transform.position - transform.position; hacia.y = 0f;
            if (hacia.magnitude < Alcance + 0.45f && Vector3.Angle(transform.forward, hacia) < 80f && !jugador.Muerto && !jugador.Esquivando)
            {
                if (tipo == Tipo.Fungico)
                {
                    FungicoAgarra();
                }
                else if (!jugador.Agarrado && Random.value < Dificultad.ProbAgarre && jugador.IniciarAgarre(this))
                {
                    CambiarEstado(Estado.Agarrando);
                    return;
                }
                else
                {
                    jugador.RecibirDano(dano, transform.position);
                }
            }
        }
        if (tEstado >= 0.9f)
        {
            siguienteAtaque = Time.time + cadencia;
            CambiarEstado(Estado.Perseguir);
        }
    }

    private void FungicoAgarra()
    {
        Inventario inv = jugador.Inventario;
        if (inv != null && inv.puntas > 0)
        {
            inv.puntas--;
            Stats.D.agarresEscapados++;
            Logros.Revisar();
            if (Juego.I != null) { Juego.I.Mensaje("¡Te zafaste clavándole una punta!"); }
            Vector3 d = transform.position - jugador.transform.position; d.y = 0f;
            AudioCap1.Play3D("sfx_estrangular", transform.position + Vector3.up, 0.9f);
            RecibirImpacto(70, d.normalized, 2.5f, FuenteDano.Melee, false, jugador.transform.position);
            CamaraTPS.Sacudir(0.35f, 0.3f);
            return;
        }
        if (Dificultad.FungicoMata)
        {
            AudioCap1.Play3D("sfx_agarre", jugador.transform.position + Vector3.up, 1f);
            jugador.Morir("Un fúngico te agarró y te desgarró el cuello.\nCon una punta te habrías podido zafar. Fabrícala con [Tab].");
        }
        else
        {
            jugador.RecibirDano(45, transform.position, "Un fúngico te despedazó. No los enfrentes de cerca.");
        }
    }

    /// <summary>Mateo termino el forcejeo.</summary>
    public void SoltarAgarre(bool zafado, bool conPunta)
    {
        if (Muerto)
        {
            return;
        }
        Vector3 d = transform.position - (jugador != null ? jugador.transform.position : transform.position - transform.forward); d.y = 0f;
        if (conPunta)
        {
            RecibirImpacto(75, d.normalized, 2.5f, FuenteDano.Melee, false, jugador != null ? jugador.transform.position : transform.position);
        }
        else if (zafado)
        {
            RecibirEmpujon(d.normalized);
        }
        else
        {
            siguienteAtaque = Time.time + 2f;
            CambiarEstado(Estado.Perseguir);
        }
    }

    private void GirarHacia(Vector3 punto, float velocidad)
    {
        Vector3 d = punto - transform.position; d.y = 0f;
        if (d.sqrMagnitude < 0.001f)
        {
            return;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 1f - Mathf.Exp(-velocidad * Time.deltaTime));
    }

    private void Sonidos()
    {
        if (Time.time < siguienteSonido || jugador == null)
        {
            return;
        }
        float dist = Vector3.Distance(transform.position, jugador.transform.position);
        if (tipo == Tipo.Fungico)
        {
            siguienteSonido = Time.time + Random.Range(0.9f, 1.8f);
            if (dist < 28f) { AudioCap1.Play3D("sfx_chasquido", transform.position + Vector3.up * 1.6f, 0.8f, Random.Range(0.9f, 1.1f)); }
        }
        else
        {
            siguienteSonido = Time.time + (Persiguiendo ? Random.Range(1.5f, 3f) : Random.Range(5f, 11f));
            if (dist < 22f && EstadoActual != Estado.Dormido)
            {
                AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up * 1.6f, Persiguiendo ? 0.8f : 0.45f, tipo == Tipo.Griton ? Random.Range(1.2f, 1.4f) : Random.Range(0.8f, 1.15f));
            }
        }
    }

    // ---------- Fuego ----------

    public void Quemar(float segundos)
    {
        if (Muerto)
        {
            return;
        }
        if (tipo == Tipo.Carnicero)
        {
            JefeQuemar(segundos);
            return;
        }
        bool empieza = !Quemandose;
        quemadoHasta = Mathf.Max(quemadoHasta, Time.time + segundos);
        if (empieza)
        {
            UltimaFuente = FuenteDano.Fuego;
            if (EstadoActual == Estado.Agarrando && jugador != null && jugador.Agarrador == this) { jugador.Liberar(true, false); }
            CambiarEstado(Estado.Quemandose);
            AudioCap1.Play3D("sfx_grito", transform.position + Vector3.up, 0.9f, 1.25f);
        }
        MostrarFuego(true);
    }

    private void MostrarFuego(bool si)
    {
        if (si && fuegoVisual == null)
        {
            fuegoVisual = new GameObject("FuegoCuerpo");
            fuegoVisual.transform.SetParent(transform, false);
            fuegoVisual.transform.localPosition = Vector3.up * 0.7f * Escala;
            var f = fuegoVisual.AddComponent<Fuego>();
            f.tamano = 0.6f * Escala;
            f.conHumo = false;
            f.conLuz = true;
        }
        if (fuegoVisual != null && fuegoVisual.activeSelf != si) { fuegoVisual.SetActive(si); }
    }

    private void ActualizarFuego(float dt)
    {
        if (!Quemandose)
        {
            if (fuegoVisual != null && fuegoVisual.activeSelf) { MostrarFuego(false); }
            return;
        }
        if (Time.time >= siguienteQuemadura)
        {
            siguienteQuemadura = Time.time + 0.25f;
            Vida -= 6f;
            Stats.D.danoCausado += 6;
            if (Vida <= 0f) { Morir(Vector3.zero, false, FuenteDano.Fuego); }
        }
    }

    // ---------- Recibir dano ----------

    /// <summary>Compatibilidad: golpe generico (botellas).</summary>
    public void RecibirGolpe(int cantidad, Vector3 direccion, float aturdir)
    {
        RecibirImpacto(cantidad, direccion, aturdir, FuenteDano.Botella, false, transform.position - direccion);
    }

    public void RecibirImpacto(int cantidad, Vector3 direccion, float aturdir, FuenteDano fuente, bool cabeza, Vector3 origenAtaque)
    {
        if (Muerto || EstadoActual == Estado.Sigilo)
        {
            return;
        }
        if (tipo == Tipo.Carnicero)
        {
            JefeRecibirDano(cantidad, direccion, fuente, origenAtaque);
            return;
        }

        Vida -= cantidad;
        Stats.D.danoCausado += cantidad;
        UltimaFuente = fuente;
        EfectosFX.Sangre(transform.position + Vector3.up * (cabeza ? AlturaCabeza : 1.4f * Escala), direccion);
        if (Vida <= 0f)
        {
            Morir(direccion, false, fuente);
            return;
        }

        if (EstadoActual == Estado.Quemandose)
        {
            return;
        }
        if (tipo == Tipo.Griton && !yaGrito && fuente != FuenteDano.Honda && fuente != FuenteDano.Revolver)
        {
            Gritar();
            return;
        }
        duracionAturdido = aturdir * (tipo == Tipo.Fungico ? 0.45f : 1f);
        CambiarEstado(Estado.Aturdido);
        if (agente.enabled && agente.isOnNavMesh) { agente.Move(direccion * 0.45f); }
        if (anim != null) { anim.Aturdir(); }
        if (!Persiguiendo) { UltimoAviso = Time.time; }
        Deteccion = 1f;
        MarcarJugadorVisto();
        AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up, 0.8f, 1.3f);
    }

    public void RecibirEmpujon(Vector3 direccion)
    {
        if (Muerto || EstadoActual == Estado.Sigilo)
        {
            return;
        }
        if (tipo == Tipo.Carnicero)
        {
            if (anim != null) { anim.Aturdir(); }
            return;
        }
        Vida -= 3f;
        duracionAturdido = tipo == Tipo.Fungico ? 0.7f : 1.4f;
        CambiarEstado(Estado.Aturdido);
        if (agente.enabled && agente.isOnNavMesh) { agente.Move(direccion * 1.3f); }
        if (anim != null) { anim.Aturdir(); }
        Deteccion = 1f;
        MarcarJugadorVisto();
    }

    public void RecibirSigilo(float duracion)
    {
        if (Muerto)
        {
            return;
        }
        duracionSigilo = duracion;
        CambiarEstado(Estado.Sigilo);
        if (anim != null) { anim.CancelarAcciones(); anim.Aturdir(); }
    }

    private void Morir(Vector3 direccion, bool silencioso, FuenteDano fuente)
    {
        if (Muerto)
        {
            return;
        }
        if (tipo == Tipo.Griton && !yaGrito) { Logros.Desbloquear("griton"); }
        EstadoActual = Estado.Muerto;
        MostrarFuego(false);
        if (agente.enabled)
        {
            if (agente.isOnNavMesh) { agente.isStopped = true; }
            agente.enabled = false;
        }
        foreach (Collider c in colisiones) { if (c != null) { c.enabled = false; } }
        if (anim != null) { anim.velocidad = 0f; anim.alerta = false; anim.agachado = false; anim.Morir(); }
        AudioCap1.Play3D(silencioso ? "sfx_golpe_carne" : "sfx_muerte_infectado", transform.position + Vector3.up, silencioso ? 0.5f : 0.9f);
        if (!silencioso) { EfectosFX.Sangre(transform.position + Vector3.up * 1.2f, direccion); }

        Stats.Kill(tipo, fuente);
        if (Juego.I != null) { Juego.I.RegistrarMuerteInfectado(this); }
        if (soltarAlMorir != null)
        {
            soltarAlMorir.transform.position = transform.position + transform.right * 0.5f + Vector3.up * 0.05f;
            soltarAlMorir.SetActive(true);
        }
        if (alMorir != null) { alMorir.Ejecutar(transform.position); }
    }

    /// <summary>Al cargar una partida: queda muerto sin efectos (desaparece el cuerpo).</summary>
    public void MarcarMuerto()
    {
        EstadoActual = Estado.Muerto;
        gameObject.SetActive(false);
    }

    /// <summary>Vuelve a su posicion inicial (al reintentar desde un punto de control).</summary>
    public void Reiniciar()
    {
        if (Muerto)
        {
            return;
        }
        if (tipo == Tipo.Carnicero)
        {
            JefeReiniciar();
            return;
        }
        Deteccion = 0f;
        quemadoHasta = 0f;
        MostrarFuego(false);
        if (anim != null) { anim.CancelarAcciones(); }
        if (agente.enabled)
        {
            agente.Warp(origen);
        }
        transform.rotation = rotOrigen;
        CambiarEstado(estadoInicial);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(Application.isPlaying ? origen : transform.position, radioDeambular);
    }
}
