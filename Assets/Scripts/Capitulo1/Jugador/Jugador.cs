using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mateo: movimiento relativo a la camara (caminar, correr con estamina, agacharse, saltar, esquivar),
/// salud con dificultad, pasos que hacen ruido, modo escucha, forcejeo cuando un infectado
/// lo agarra y seleccion del objeto interactivo con [E].
/// El combate esta en CombateJugador, la fabricacion en Crafteo, los objetos en Inventario
/// y la luz en Linterna.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(-10)]
public class Jugador : MonoBehaviour
{
    public static Jugador I { get; private set; }

    /// <summary>Solo para pruebas: Mateo no recibe dano.</summary>
    public static bool ModoDios;

    [Header("Movimiento (m/s)")]
    public float velCaminar = 2.9f;
    public float velCorrer = 5.8f;
    public float velAgachado = 1.5f;
    public float aceleracion = 14f;
    public float velocidadGiro = 12f;
    public float gravedad = -20f;

    [Header("Esquiva")]
    public float velEsquiva = 8.5f;
    public float duracionEsquiva = 0.32f;
    public float costoEsquiva = 18f;

    [Header("Salto")]
    [Tooltip("Altura maxima del salto en metros (sube a tarimas, mostradores y cajas bajas).")]
    public float alturaSalto = 0.95f;
    public float costoSalto = 8f;

    /// <summary>Solo para pruebas automaticas: direccion de movimiento (mundo) y salto.</summary>
    [System.NonSerialized] public Vector3 moverBot;
    [System.NonSerialized] public bool saltarBot;

    [Header("Salud y estamina")]
    public int saludMax = 100;
    public float estaminaMax = 100f;
    public float gastoCorrer = 14f;
    public float recuperacionEstamina = 24f;

    [Header("Ruido de pasos (radio en metros)")]
    public float ruidoCaminar = 5f;
    public float ruidoCorrer = 13f;
    public float ruidoAgachado = 0.6f;

    [Header("Referencias")]
    public AnimProcedural anim;

    public int Salud { get; private set; }
    public float Estamina { get; private set; }
    public bool Agachado { get; set; }
    public bool Corriendo { get; private set; }
    public bool Muerto { get; private set; }
    public bool Agotado { get { return Time.time < agotadoHasta; } }
    public float VelocidadActual { get; private set; }
    public float UltimoDano { get; private set; } = -100f;
    public Vector3 OrigenUltimoDano { get; private set; }
    public float NivelRuido { get; private set; }
    public Interactivo Actual { get; private set; }
    public bool Bloqueado { get { return Time.time < bloqueoHasta; } }
    public bool Esquivando { get { return Time.time < esquivaHasta; } }
    public bool Escuchando { get; private set; }
    public bool EnAire { get { return enAire; } }
    public float TiempoEscuchando { get; private set; }

    // Agarre
    public Infectado Agarrador { get; private set; }
    public bool Agarrado { get { return Agarrador != null; } }
    public float ProgresoZafarse { get; private set; }
    public float TiempoAgarre { get; private set; }
    public const float LimiteAgarre = 2.7f;

    /// <summary>Multiplicador externo de velocidad (eventos). El resto se calcula por estado.</summary>
    public float multVelocidad = 1f;

    public CombateJugador Combate { get; private set; }
    public Inventario Inventario { get; private set; }
    public Linterna Linterna { get; private set; }
    public Crafteo Crafteo { get; private set; }
    public CharacterController Controlador { get { return cc; } }

    private CharacterController cc;
    private Transform cam;
    private Vector3 velPlano;
    private float velY;
    private float agotadoHasta;
    private float ultimoGasto;
    private float bloqueoHasta;
    private float invulnerableHasta;
    private float esquivaHasta;
    private float inmuneAgarreHasta;
    private float distanciaPaso;
    private float ultimoEnSuelo;
    private float saltoPedido = -10f;
    private bool enAire;
    private bool pieIzq;

    private void Awake()
    {
        I = this;
        cc = GetComponent<CharacterController>();
        Combate = GetComponent<CombateJugador>();
        Inventario = GetComponent<Inventario>();
        Linterna = GetComponent<Linterna>();
        Crafteo = GetComponent<Crafteo>();
        if (Crafteo == null) { Crafteo = gameObject.AddComponent<Crafteo>(); }
        if (anim == null) { anim = GetComponentInChildren<AnimProcedural>(); }
        Salud = saludMax;
        Estamina = estaminaMax;
    }

    private void Start()
    {
        if (Camera.main != null) { cam = Camera.main.transform; }
    }

    /// <summary>Punto del pecho (para la vision de los infectados).</summary>
    public Vector3 Pecho { get { return transform.position + Vector3.up * (Agachado ? 0.85f : 1.35f); } }

    /// <summary>Que tan facil es ver a Mateo (1 = normal).</summary>
    public float Visibilidad
    {
        get
        {
            float v = Agachado ? 0.55f : 1f;
            if (Linterna != null && Linterna.Encendida) { v *= 1.35f; }
            if (Corriendo) { v *= 1.15f; }
            return v;
        }
    }

    public Vector3 AdelanteCamara
    {
        get
        {
            if (cam == null) { return transform.forward; }
            Vector3 f = cam.forward; f.y = 0f;
            return f.sqrMagnitude > 0.001f ? f.normalized : transform.forward;
        }
    }

    private float MultiplicadorVelocidad()
    {
        float m = multVelocidad;
        if (Combate != null)
        {
            if (Combate.Curando || Combate.Golpeando) { m *= 0.35f; }
            else if (Combate.Apuntando) { m *= 0.55f; }
            if (Combate.Recargando) { m *= 0.7f; }
        }
        if (Crafteo != null && Crafteo.Fabricando) { m *= 0.4f; }
        if (Escuchando) { m *= 0.45f; }
        return m;
    }

    private void Update()
    {
        if (cam == null && Camera.main != null) { cam = Camera.main.transform; }
        float dt = Time.deltaTime;

        if (Muerto)
        {
            if (anim != null) { anim.velocidad = 0f; }
            Actual = null;
            Escuchando = false;
            return;
        }

        if (Agarrado)
        {
            ActualizarAgarre(dt);
            return;
        }

        bool control = Juego.Control && !Bloqueado;
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        Gamepad pad = Gamepad.current;

        Vector2 entrada = Vector2.zero;
        if (control && kb != null)
        {
            if (kb.wKey.isPressed) { entrada.y += 1f; }
            if (kb.sKey.isPressed) { entrada.y -= 1f; }
            if (kb.dKey.isPressed) { entrada.x += 1f; }
            if (kb.aKey.isPressed) { entrada.x -= 1f; }
        }
        if (control && pad != null) { entrada += pad.leftStick.ReadValue(); }
        entrada = Vector2.ClampMagnitude(entrada, 1f);

        if (control && kb != null && (kb.cKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame))
        {
            Agachado = !Agachado;
        }

        bool apuntando = Combate != null && Combate.Apuntando;
        bool quiereCorrer = control && kb != null && kb.leftShiftKey.isPressed && entrada.sqrMagnitude > 0.05f && !Agotado
                            && !apuntando && (Combate == null || !Combate.Curando) && (Crafteo == null || !Crafteo.Fabricando);
        if (quiereCorrer && Agachado) { Agachado = false; }
        Corriendo = quiereCorrer && Estamina > 0f && !Esquivando;

        // Modo escucha (mantener Z)
        bool escuchar = control && !Corriendo && !apuntando && ((kb != null && kb.zKey.isPressed) || (pad != null && pad.rightShoulder.isPressed && pad.leftShoulder.isPressed));
        Escuchando = escuchar;
        TiempoEscuchando = escuchar ? TiempoEscuchando + dt : 0f;

        // Estamina
        if (Corriendo)
        {
            GastarEstamina(gastoCorrer * dt);
        }
        else if (Time.time - ultimoGasto > 0.9f)
        {
            Estamina = Mathf.Min(estaminaMax, Estamina + recuperacionEstamina * dt);
        }

        // Movimiento relativo a la camara
        Vector3 adelante = AdelanteCamara;
        Vector3 derecha = new Vector3(adelante.z, 0f, -adelante.x);
        Vector3 dir = adelante * entrada.y + derecha * entrada.x;
        if (moverBot.sqrMagnitude > 0.0001f) { dir = Vector3.ClampMagnitude(new Vector3(moverBot.x, 0f, moverBot.z), 1f); }

        // Esquiva
        bool esquivar = control && ((kb != null && (kb.leftAltKey.wasPressedThisFrame || kb.vKey.wasPressedThisFrame)) || (pad != null && pad.buttonEast.wasPressedThisFrame));
        if (esquivar && !Esquivando && cc.isGrounded && Estamina >= costoEsquiva * 0.6f && (Combate == null || !Combate.Curando))
        {
            Vector3 d = dir.sqrMagnitude > 0.01f ? dir.normalized : -transform.forward;
            Esquivar(d);
        }

        if (Esquivando)
        {
            velPlano = Vector3.MoveTowards(velPlano, velPlano.normalized * velCaminar, aceleracion * 0.6f * dt);
        }
        else
        {
            float vObjetivo = Agachado ? velAgachado : (Corriendo ? velCorrer : velCaminar);
            vObjetivo *= MultiplicadorVelocidad();
            Vector3 deseada = dir * vObjetivo;
            velPlano = Vector3.MoveTowards(velPlano, deseada, aceleracion * dt);
        }

        // Salto (Espacio / A del mando). Con un pequeno margen al salir de un borde y al
        // presionar justo antes de aterrizar, para que se sienta preciso.
        if (cc.isGrounded) { ultimoEnSuelo = Time.time; }
        bool saltar = control && ((kb != null && kb.spaceKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame) || saltarBot);
        saltarBot = false;
        if (saltar) { saltoPedido = Time.time; }
        bool puedeSaltar = Time.time - ultimoEnSuelo < 0.15f && velY <= 0.5f && !Esquivando && Estamina >= costoSalto * 0.5f
                           && (Combate == null || (!Combate.Curando && !Combate.Golpeando)) && (Crafteo == null || !Crafteo.Fabricando);
        bool salto = false;
        if (Time.time - saltoPedido < 0.15f && puedeSaltar)
        {
            saltoPedido = -10f;
            Saltar();
            salto = true;
        }

        // dt acotado: si un cuadro tarda mucho (tirón), el salto y la caida no se descontrolan
        float dtMov = Mathf.Min(dt, 0.05f);
        if (cc.isGrounded && velY < 0f) { velY = -2f; }
        if (!salto) { velY += gravedad * dtMov; }
        CollisionFlags choque = cc.Move((velPlano + Vector3.up * velY) * dtMov);
        if ((choque & CollisionFlags.Above) != 0 && velY > 0f) { velY = 0f; }
        if (!cc.isGrounded && !enAire && velY < -5f) { enAire = true; }
        if (enAire && cc.isGrounded && velY <= 0f) { Aterrizar(velY); }
        RevisarCaida();

        Vector3 vReal = cc.velocity; vReal.y = 0f;
        VelocidadActual = vReal.magnitude;
        if (Juego.Control) { Stats.D.distancia += VelocidadActual * dt; }

        bool ocupado = anim != null && anim.Ocupado;
        if (apuntando)
        {
            Quaternion obj = Quaternion.LookRotation(adelante);
            transform.rotation = Quaternion.Slerp(transform.rotation, obj, 1f - Mathf.Exp(-20f * dt));
        }
        else if (dir.sqrMagnitude > 0.01f && !ocupado && !Esquivando)
        {
            Quaternion obj = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, obj, 1f - Mathf.Exp(-velocidadGiro * dt));
        }

        // Capsula al agacharse
        bool bajo = Agachado || Esquivando;
        float alto = Mathf.Lerp(cc.height, bajo ? 1.2f : 1.8f, 1f - Mathf.Exp(-12f * dt));
        cc.height = alto;
        cc.center = new Vector3(0f, alto * 0.5f, 0f);

        if (anim != null)
        {
            anim.velocidad = VelocidadActual;
            anim.agachado = bajo || Escuchando || (enAire && velY > -9f);
            anim.alerta = Combate != null && (Combate.EnGuardia || Combate.Apuntando) && !Agachado;
        }

        Pasos(dt);
        NivelRuido = Mathf.MoveTowards(NivelRuido, 0f, dt * 12f);

        // Interaccion
        bool fabricando = Crafteo != null && (Crafteo.Abierto || Crafteo.Fabricando);
        Actual = control && !fabricando ? BuscarInteractivo() : null;
        bool usar = control && !fabricando && ((kb != null && kb.eKey.wasPressedThisFrame) || (pad != null && pad.buttonWest.wasPressedThisFrame));
        if (usar)
        {
            if (Combate != null && Combate.ObjetivoSigilo != null)
            {
                Combate.AtaqueSigiloso();
            }
            else if (Actual != null)
            {
                Actual.Usar(this);
            }
        }
    }

    private void Saltar()
    {
        Agachado = false;
        velY = Mathf.Sqrt(2f * alturaSalto * -gravedad);
        ultimoEnSuelo = -10f;
        enAire = true;
        GastarEstamina(costoSalto);
        if (Crafteo != null) { Crafteo.Interrumpir(); }
        AudioCap1.Play3D("sfx_esquiva", transform.position + Vector3.up, 0.35f, 1.3f);
    }

    /// <summary>Al caer al suelo hace ruido: saltar cerca de un infectado lo puede alertar.</summary>
    private void Aterrizar(float velocidadCaida)
    {
        enAire = false;
        if (velocidadCaida < -8f)
        {
            CamaraTPS.Sacudir(0.12f, 0.22f);
            EfectosFX.Polvo(transform.position + Vector3.up * 0.1f);
        }
        AudioCap1.Play3D("sfx_paso_1", transform.position, 0.55f, 0.8f);
        SistemaRuido.Emitir(transform.position, 4f, true);
        NivelRuido = Mathf.Max(NivelRuido, 4f);
    }

    private void Esquivar(Vector3 d)
    {
        GastarEstamina(costoEsquiva);
        esquivaHasta = Time.time + duracionEsquiva;
        invulnerableHasta = Mathf.Max(invulnerableHasta, Time.time + duracionEsquiva + 0.05f);
        velPlano = d * velEsquiva;
        Agachado = false;
        if (Combate != null) { Combate.Interrumpir(); }
        if (Crafteo != null) { Crafteo.Interrumpir(); }
        transform.rotation = Quaternion.LookRotation(Vector3.Dot(d, transform.forward) < -0.3f ? transform.forward : d);
        AudioCap1.Play3D("sfx_esquiva", transform.position, 0.6f, Random.Range(0.9f, 1.1f));
        SistemaRuido.Emitir(transform.position, 4f, true);
        Stats.D.esquivas++;
    }

    private void Pasos(float dt)
    {
        if (VelocidadActual < 0.4f || !cc.isGrounded)
        {
            return;
        }

        float largoPaso = Agachado ? 0.6f : (Corriendo ? 1.25f : 0.8f);
        distanciaPaso += VelocidadActual * dt;
        if (distanciaPaso < largoPaso)
        {
            return;
        }
        distanciaPaso = 0f;
        pieIzq = !pieIzq;

        float radio = Agachado ? ruidoAgachado : (Corriendo ? ruidoCorrer : ruidoCaminar);
        if (Escuchando) { radio *= 0.5f; }
        SistemaRuido.Emitir(transform.position, radio, true);
        NivelRuido = Mathf.Max(NivelRuido, radio);
        float vol = Agachado ? 0.12f : (Corriendo ? 0.55f : 0.3f);
        AudioCap1.Play3D(pieIzq ? "sfx_paso_1" : "sfx_paso_2", transform.position, vol, Random.Range(0.9f, 1.1f));
    }

    public void GastarEstamina(float cantidad)
    {
        Estamina -= cantidad;
        ultimoGasto = Time.time;
        if (Estamina <= 0f)
        {
            Estamina = 0f;
            agotadoHasta = Time.time + 1.8f;
            AudioCap1.Play3D("sfx_jadeo", transform.position, 0.6f);
        }
    }

    private Interactivo BuscarInteractivo()
    {
        Interactivo mejor = null;
        float mejorPuntaje = float.MaxValue;
        Vector3 adelante = AdelanteCamara;
        Vector3 pos = transform.position;

        foreach (Interactivo it in Interactivo.Todos)
        {
            if (it == null || !it.Disponible) { continue; }
            Vector3 p = it.Punto;
            if (Mathf.Abs(p.y - (pos.y + 1f)) > 2.2f) { continue; }
            Vector3 hacia = p - pos; hacia.y = 0f;
            float d = hacia.magnitude;
            if (d > it.radio) { continue; }
            float alineado = d > 0.01f ? Vector3.Dot(adelante, hacia / d) : 1f;
            if (d > 0.9f && alineado < 0.15f) { continue; }
            float puntaje = d - alineado * 0.9f;
            if (puntaje < mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = it;
            }
        }
        return mejor;
    }

    // ---------- Agarre ----------

    /// <summary>Un infectado intenta agarrar a Mateo. Devuelve true si el agarre empieza.</summary>
    public bool IniciarAgarre(Infectado inf)
    {
        if (Agarrado || Muerto || Esquivando || ModoDios || Time.time < inmuneAgarreHasta || inf == null)
        {
            return false;
        }
        if (Juego.I != null && Juego.I.estado != Juego.Estado.Jugando)
        {
            return false;
        }
        Agarrador = inf;
        ProgresoZafarse = 0.15f;
        TiempoAgarre = 0f;
        Agachado = false;
        Escuchando = false;
        velPlano = Vector3.zero;
        if (Combate != null) { Combate.Interrumpir(); }
        if (Crafteo != null) { Crafteo.Interrumpir(); }
        Vector3 d = inf.transform.position - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.01f) { transform.rotation = Quaternion.LookRotation(d); }
        Stats.D.agarresSufridos++;
        CamaraTPS.Sacudir(0.4f, 0.4f);
        AudioCap1.Play3D("sfx_agarre", transform.position + Vector3.up, 1f);
        AudioCap1.Play2D("sfx_susto", 0.35f);
        if (anim != null) { anim.alerta = true; anim.Empujar(); }
        return true;
    }

    private void ActualizarAgarre(float dt)
    {
        Infectado inf = Agarrador;
        if (inf == null || inf.Muerto || !inf.Agarrando)
        {
            Agarrador = null;
            inmuneAgarreHasta = Time.time + 1.5f;
            return;
        }

        TiempoAgarre += dt;
        Vector3 d = inf.transform.position - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.01f) { transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 1f - Mathf.Exp(-12f * dt)); }
        if (cc.isGrounded && velY < 0f) { velY = -2f; }
        velY += gravedad * dt;
        cc.Move(Vector3.up * velY * dt);
        if (anim != null) { anim.velocidad = 0f; anim.agachado = false; anim.alerta = true; }
        VelocidadActual = 0f;
        Actual = null;

        Keyboard kb = Keyboard.current;
        Mouse m = Mouse.current;
        Gamepad pad = Gamepad.current;
        bool machacar = (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) || (pad != null && pad.buttonWest.wasPressedThisFrame);
        bool punta = (m != null && m.leftButton.wasPressedThisFrame) || (pad != null && pad.rightTrigger.wasPressedThisFrame);

        if (Juego.I != null && Juego.I.estado != Juego.Estado.Jugando)
        {
            machacar = false;
            punta = false;
        }

        if (machacar)
        {
            ProgresoZafarse += Dificultad.FuerzaZafarse;
            CamaraTPS.Sacudir(0.12f, 0.08f);
            AudioCap1.Play3D("sfx_golpe_carne", transform.position + Vector3.up, 0.25f, Random.Range(0.8f, 1.2f));
        }
        ProgresoZafarse = Mathf.Max(0f, ProgresoZafarse - dt * 0.12f);

        if (punta && Inventario != null && Inventario.puntas > 0)
        {
            Inventario.puntas--;
            if (Juego.I != null) { Juego.I.Mensaje("¡Le clavaste la punta!"); }
            AudioCap1.Play3D("sfx_estrangular", transform.position + Vector3.up, 0.9f);
            Liberar(true, true);
            return;
        }
        if (ProgresoZafarse >= 1f)
        {
            Liberar(true, false);
            return;
        }
        if (TiempoAgarre >= LimiteAgarre)
        {
            Liberar(false, false);
        }
    }

    /// <summary>Termina el agarre. exito = Mateo se zafo; si no, recibe una mordida.</summary>
    public void Liberar(bool exito, bool conPunta)
    {
        Infectado inf = Agarrador;
        Agarrador = null;
        inmuneAgarreHasta = Time.time + 2.5f;
        if (inf == null)
        {
            return;
        }
        if (exito)
        {
            Stats.D.agarresEscapados++;
            Logros.Revisar();
            Vector3 atras = transform.position - inf.transform.position; atras.y = 0f;
            velPlano = atras.normalized * 3f;
            if (anim != null) { anim.Empujar(); }
            inf.SoltarAgarre(true, conPunta);
        }
        else
        {
            inf.SoltarAgarre(false, false);
            invulnerableHasta = 0f;
            RecibirDano(inf.DanoMordida, inf.transform.position, "Te mordieron. Machaca [E] para zafarte, o usa una punta con clic izquierdo.");
        }
    }

    // ---------- Salud ----------

    public void RecibirDano(int dano, Vector3 origen, string motivo = null)
    {
        if (Muerto || Time.time < invulnerableHasta || dano <= 0 || ModoDios)
        {
            return;
        }
        if (Juego.I != null && Juego.I.estado != Juego.Estado.Jugando && Juego.I.estado != Juego.Estado.Pausa)
        {
            return;
        }

        int real = Mathf.Max(1, Mathf.RoundToInt(dano * Dificultad.DanoRecibido));
        Salud = Mathf.Max(0, Salud - real);
        Stats.D.danoRecibido += real;
        UltimoDano = Time.time;
        OrigenUltimoDano = origen;
        invulnerableHasta = Time.time + 0.45f;

        Vector3 empuje = transform.position - origen; empuje.y = 0f;
        if (empuje.sqrMagnitude > 0.001f && !Agarrado) { velPlano += empuje.normalized * 3.5f; }

        if (Combate != null) { Combate.Interrumpir(); }
        if (Crafteo != null) { Crafteo.Interrumpir(); }
        if (anim != null) { anim.Aturdir(); }
        CamaraTPS.Sacudir(0.35f, 0.3f);
        AudioCap1.Play3D("sfx_golpe_recibido", transform.position, 0.9f);

        if (Salud <= 0)
        {
            Morir(string.IsNullOrEmpty(motivo) ? "Los infectados te alcanzaron." : motivo);
        }
    }

    /// <summary>Empujon fuerte (embestida del jefe).</summary>
    public void Impulsar(Vector3 v)
    {
        velPlano += new Vector3(v.x, 0f, v.z);
        velY = Mathf.Max(velY, v.y);
    }

    public void Curar(int cantidad)
    {
        Salud = Mathf.Min(saludMax, Salud + cantidad);
    }

    public void FijarSalud(int s)
    {
        Salud = Mathf.Clamp(s, 1, saludMax);
    }

    public void Morir(string motivo)
    {
        if (Muerto || ModoDios)
        {
            return;
        }
        Muerto = true;
        Agachado = false;
        Escuchando = false;
        if (Agarrado)
        {
            Infectado inf = Agarrador;
            Agarrador = null;
            inf.SoltarAgarre(false, false);
        }
        if (Combate != null) { Combate.Interrumpir(); }
        if (Crafteo != null) { Crafteo.Interrumpir(); }
        if (anim != null) { anim.Morir(); }
        Stats.D.muertes++;
        if (Juego.I != null) { Juego.I.JugadorMuerto(motivo); }
    }

    public void Revivir(Vector3 pos, Quaternion rot, int salud)
    {
        Muerto = false;
        Agarrador = null;
        Salud = Mathf.Clamp(salud, 1, saludMax);
        Estamina = estaminaMax;
        agotadoHasta = 0f;
        Agachado = false;
        multVelocidad = 1f;
        if (anim != null) { anim.Revivir(); }
        Teletransportar(pos, rot);
        invulnerableHasta = Time.time + 2f;
        inmuneAgarreHasta = Time.time + 3f;
    }

    private Vector3 posSegura;
    private bool haySegura;
    private float siguienteSegura;

    /// <summary>
    /// Red de seguridad: recuerda el ultimo punto firme del navmesh y, si Mateo atraviesa el
    /// piso (por ejemplo, empujado por una horda contra una pared), lo devuelve ahi.
    /// </summary>
    private void RevisarCaida()
    {
        if (cc.isGrounded && Time.time >= siguienteSegura)
        {
            siguienteSegura = Time.time + 0.5f;
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 0.6f, UnityEngine.AI.NavMesh.AllAreas))
            {
                posSegura = hit.position;
                haySegura = true;
            }
        }
        if (haySegura && transform.position.y < posSegura.y - 12f)
        {
            Teletransportar(posSegura + Vector3.up * 0.1f, transform.rotation);
            if (Juego.I != null) { Juego.I.Mensaje("Volviste a un lugar seguro"); }
        }
    }

    public void Teletransportar(Vector3 pos, Quaternion rot)
    {
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z) || float.IsInfinity(pos.x) || float.IsInfinity(pos.y) || float.IsInfinity(pos.z) || pos.sqrMagnitude > 1e8f)
        {
            Debug.LogWarning("Teletransportar: posicion invalida " + pos);
            return;
        }
        cc.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        cc.enabled = true;
        velPlano = Vector3.zero;
        velY = 0f;
    }

    public void Bloquear(float segundos)
    {
        bloqueoHasta = Mathf.Max(bloqueoHasta, Time.time + segundos);
        velPlano = Vector3.zero;
    }
}
