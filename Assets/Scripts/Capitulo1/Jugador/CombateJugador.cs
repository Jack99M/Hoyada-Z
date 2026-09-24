using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Combate de Mateo:
/// - Cuerpo a cuerpo (punos, palo, fierro, machete) con durabilidad y clavos.
/// - Honda andina (warak'a) con piedras: silenciosa, aturde y mata de un tiro a la cabeza.
/// - Revolver .38: fuerte pero MUY ruidoso (atrae a todo el barrio). Recarga con [R].
/// - Apuntar con clic derecho (armas a distancia), disparar con clic izquierdo.
/// - Empujon (clic derecho con arma cuerpo a cuerpo), ataque sigiloso [E] agachado por la espalda
///   (a un fungico solo con una punta), botellas [Q], molotovs [G] y curarse [H].
/// Ranuras: [1] cuerpo a cuerpo  [2] honda  [3] revolver.
/// </summary>
[RequireComponent(typeof(Jugador))]
public class CombateJugador : MonoBehaviour
{
    public enum Arma { Punos, Palo, Fierro, Machete }

    public struct DatosArma
    {
        public string nombre;
        public int dano;
        public float alcance;
        public float cadencia;
        public float estamina;
        public int durabilidad;
        public float ruido;
        public float aturdir;
    }

    public static DatosArma Datos(Arma a)
    {
        switch (a)
        {
            case Arma.Palo:
                return new DatosArma { nombre = "Palo", dano = 34, alcance = 1.9f, cadencia = 0.75f, estamina = 12f, durabilidad = 10, ruido = 6f, aturdir = 0.7f };
            case Arma.Fierro:
                return new DatosArma { nombre = "Fierro de construcción", dano = 58, alcance = 2.0f, cadencia = 0.9f, estamina = 16f, durabilidad = 16, ruido = 7f, aturdir = 0.9f };
            case Arma.Machete:
                return new DatosArma { nombre = "Machete", dano = 74, alcance = 2.1f, cadencia = 0.8f, estamina = 14f, durabilidad = 20, ruido = 6f, aturdir = 0.8f };
            default:
                return new DatosArma { nombre = "Puños", dano = 14, alcance = 1.35f, cadencia = 0.55f, estamina = 7f, durabilidad = 0, ruido = 4f, aturdir = 0.45f };
        }
    }

    [Header("Curacion")]
    public int curaPorVenda = 45;
    public float tiempoCurar = 2.2f;

    [Header("Arrojadizos")]
    public float fuerzaLanzamiento = 15f;
    public GameObject prefabBotella;

    [Header("Honda")]
    public int danoHonda = 34;
    public float velHonda = 34f;
    public float cadenciaHonda = 0.85f;

    [Header("Revolver")]
    public int danoRevolver = 95;
    public float cadenciaRevolver = 0.55f;
    public float tiempoRecarga = 1.9f;
    public float ruidoDisparo = 34f;

    public Infectado ObjetivoSigilo { get; private set; }
    public bool Curando { get; private set; }
    public float ProgresoCura { get; private set; }
    public bool Apuntando { get; private set; }
    public bool Recargando { get; private set; }
    public float ProgresoRecarga { get; private set; }
    public bool Golpeando { get { return golpePendiente; } }
    public float UltimoImpacto { get; private set; } = -10f;
    public bool UltimoFueCabeza { get; private set; }
    public float Dispersion { get; private set; }
    public bool EnGuardia { get { return Time.time - ultimoCombate < 2.5f; } }
    public Ranura RanuraActiva { get { return inv != null ? inv.ranura : Ranura.CuerpoACuerpo; } }

    /// <summary>La tecla R la usa el revolver (si no, cambia pilas de la linterna).</summary>
    public bool UsaTeclaR
    {
        get { return inv != null && inv.ranura == Ranura.Revolver && inv.tieneRevolver && inv.balasCargadas < inv.capacidadTambor && inv.balas > 0; }
    }

    private Jugador j;
    private Inventario inv;
    private AnimProcedural anim;
    private float siguienteAtaque;
    private float siguienteEmpujon;
    private float siguienteDisparo;
    private bool golpePendiente;
    private float golpeEn;
    private float golpeMultiplicador = 1f;
    private bool empujonPendiente;
    private float empujonEn;
    private float curaInicio;
    private float recargaInicio;
    private float ultimoCombate = -10f;
    private float dispersionExtra;
    private Light fogonazo;
    private LineRenderer trazador;
    private float efectoHasta;

    private void Awake()
    {
        j = GetComponent<Jugador>();
        inv = GetComponent<Inventario>();
    }

    private void Start()
    {
        anim = j.anim;
        var fg = new GameObject("Fogonazo");
        fg.transform.SetParent(transform, false);
        fogonazo = fg.AddComponent<Light>();
        fogonazo.type = LightType.Point;
        fogonazo.color = new Color(1f, 0.75f, 0.4f);
        fogonazo.range = 9f;
        fogonazo.intensity = 30f;
        fogonazo.enabled = false;
        trazador = fg.AddComponent<LineRenderer>();
        trazador.positionCount = 2;
        trazador.startWidth = 0.025f;
        trazador.endWidth = 0.01f;
        trazador.useWorldSpace = true;
        Shader s = Shader.Find("Sprites/Default");
        if (s != null) { trazador.sharedMaterial = new Material(s); }
        trazador.startColor = new Color(1f, 0.9f, 0.6f, 0.9f);
        trazador.endColor = new Color(1f, 0.9f, 0.6f, 0f);
        trazador.enabled = false;
    }

    private void Update()
    {
        if (Time.time > efectoHasta)
        {
            if (fogonazo != null) { fogonazo.enabled = false; }
            if (trazador != null) { trazador.enabled = false; }
        }

        if (j.Muerto)
        {
            ObjetivoSigilo = null;
            Apuntando = false;
            return;
        }

        bool control = Juego.Control && !j.Bloqueado && !j.Agarrado && !j.Esquivando;
        bool fabricando = j.Crafteo != null && (j.Crafteo.Abierto || j.Crafteo.Fabricando);
        ObjetivoSigilo = control && !fabricando ? BuscarSigilo() : null;

        if (golpePendiente && Time.time >= golpeEn) { ResolverGolpe(); }
        if (empujonPendiente && Time.time >= empujonEn) { ResolverEmpujon(); }

        if (Curando)
        {
            ProgresoCura = Mathf.Clamp01((Time.time - curaInicio) / tiempoCurar);
            if (ProgresoCura >= 1f) { TerminarCura(); }
        }
        if (Recargando)
        {
            ProgresoRecarga = Mathf.Clamp01((Time.time - recargaInicio) / tiempoRecarga);
            if (ProgresoRecarga >= 1f) { TerminarRecarga(); }
        }

        // Dispersion del apuntado
        float baseDisp = inv.ranura == Ranura.Honda ? 1.3f : 0.8f;
        if (j.VelocidadActual > 0.5f) { baseDisp += 1.4f; }
        if (j.Agachado) { baseDisp *= 0.7f; }
        dispersionExtra = Mathf.MoveTowards(dispersionExtra, 0f, Time.deltaTime * 5f);
        Dispersion = baseDisp + dispersionExtra;

        if (!control)
        {
            Apuntando = false;
            return;
        }

        Mouse m = Mouse.current;
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;

        if (!fabricando && kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) { CambiarRanura(Ranura.CuerpoACuerpo); }
            else if (kb.digit2Key.wasPressedThisFrame) { CambiarRanura(Ranura.Honda); }
            else if (kb.digit3Key.wasPressedThisFrame) { CambiarRanura(Ranura.Revolver); }
        }
        if (pad != null && pad.dpad.right.wasPressedThisFrame) { CambiarRanura((Ranura)(((int)inv.ranura + 1) % 3)); }

        bool distancia = inv.ranura != Ranura.CuerpoACuerpo;
        bool botonApuntar = (m != null && m.rightButton.isPressed) || (pad != null && pad.leftTrigger.isPressed);
        Apuntando = distancia && botonApuntar && !Curando && !fabricando;

        bool atacar = (m != null && m.leftButton.wasPressedThisFrame) || (pad != null && pad.rightTrigger.wasPressedThisFrame);
        bool empujar = !distancia && ((m != null && m.rightButton.wasPressedThisFrame) || (pad != null && pad.leftTrigger.wasPressedThisFrame));
        bool lanzar = kb != null && kb.qKey.wasPressedThisFrame;
        bool molotov = (kb != null && kb.gKey.wasPressedThisFrame) || (pad != null && pad.dpad.left.wasPressedThisFrame);
        bool curar = (kb != null && kb.hKey.wasPressedThisFrame) || (pad != null && pad.dpad.up.wasPressedThisFrame);
        bool recargar = kb != null && kb.rKey.wasPressedThisFrame && UsaTeclaR;

        if (fabricando)
        {
            return;
        }

        if (atacar && Apuntando && !Recargando) { Disparar(); }
        else if (atacar && !Curando && !Apuntando && Time.time >= siguienteAtaque) { Atacar(); }
        else if (empujar && !Curando && Time.time >= siguienteEmpujon) { Empujar(); }
        else if (lanzar && !Curando) { Lanzar(false); }
        else if (molotov && !Curando) { Lanzar(true); }
        else if (curar && !Curando) { EmpezarCura(); }
        else if (recargar && !Recargando && !Curando) { EmpezarRecarga(); }
    }

    public void CambiarRanura(Ranura r)
    {
        if (r == inv.ranura) { return; }
        if (r == Ranura.Honda && !inv.tieneHonda) { Juego.I.Mensaje("No tengo una honda"); return; }
        if (r == Ranura.Revolver && !inv.tieneRevolver) { Juego.I.Mensaje("No tengo un arma de fuego"); return; }
        inv.ranura = r;
        Recargando = false;
        inv.ActualizarVisual();
        AudioCap1.Play3D("sfx_arma", transform.position, 0.35f, 1.2f);
    }

    // ---------- Golpe ----------

    private void Atacar()
    {
        DatosArma d = Datos(inv.arma);
        golpeMultiplicador = j.Estamina >= d.estamina * 0.5f ? 1f : 0.55f;
        j.GastarEstamina(d.estamina);

        Infectado objetivo = BuscarObjetivo(d.alcance + 1.2f, 80f);
        if (objetivo != null)
        {
            Vector3 hacia = objetivo.transform.position - transform.position; hacia.y = 0f;
            if (hacia.sqrMagnitude > 0.001f) { transform.rotation = Quaternion.LookRotation(hacia); }
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(j.AdelanteCamara);
        }

        if (anim != null) { anim.Golpear(d.cadencia); }
        golpePendiente = true;
        golpeEn = Time.time + d.cadencia * 0.5f;
        siguienteAtaque = Time.time + d.cadencia;
        ultimoCombate = Time.time;
        AudioCap1.Play3D("sfx_swing", transform.position, 0.5f, Random.Range(0.9f, 1.15f));
    }

    private void ResolverGolpe()
    {
        golpePendiente = false;
        DatosArma d = Datos(inv.arma);

        Infectado golpeado = BuscarObjetivo(d.alcance + (inv.arma == Arma.Punos ? 0f : 0.2f), 65f);
        if (golpeado == null)
        {
            return;
        }

        Vector3 dir = golpeado.transform.position - transform.position; dir.y = 0f;
        float mult = golpeMultiplicador * Random.Range(0.9f, 1.1f);
        if (inv.clavos && inv.arma != Arma.Punos) { mult *= 1.4f; }
        if (golpeado.EstadoActual == Infectado.Estado.Aturdido) { mult *= 1.35f; }
        int dano = Mathf.RoundToInt(d.dano * mult);
        golpeado.RecibirImpacto(dano, dir.normalized, d.aturdir, FuenteDano.Melee, false, transform.position);
        SistemaRuido.Emitir(transform.position, d.ruido, true);
        CamaraTPS.Sacudir(0.18f, 0.15f);
        AudioCap1.Play3D(inv.arma != Arma.Punos ? "sfx_golpe_arma" : "sfx_golpe_carne", golpeado.transform.position + Vector3.up, 0.9f, Random.Range(0.9f, 1.1f));
        ultimoCombate = Time.time;
        UltimoImpacto = Time.time;
        UltimoFueCabeza = false;

        if (inv.arma != Arma.Punos)
        {
            if (inv.GastarDurabilidad())
            {
                AudioCap1.Play3D("sfx_rompe_palo", transform.position, 0.9f);
                if (Juego.I != null) { Juego.I.Mensaje("¡Tu arma se rompió!"); }
            }
        }
    }

    /// <summary>Infectado vivo mas conveniente delante de Mateo.</summary>
    private Infectado BuscarObjetivo(float alcance, float anguloMax)
    {
        Infectado mejor = null;
        float mejorPuntaje = float.MaxValue;
        Vector3 pos = transform.position;
        Vector3 adelante = golpePendiente ? transform.forward : j.AdelanteCamara;

        foreach (Infectado inf in Infectado.Todos)
        {
            if (inf == null || inf.Muerto) { continue; }
            Vector3 hacia = inf.transform.position - pos;
            if (Mathf.Abs(hacia.y) > 1.6f) { continue; }
            hacia.y = 0f;
            float dist = hacia.magnitude - inf.RadioExtra;
            if (dist > alcance) { continue; }
            float ang = dist > 0.01f ? Vector3.Angle(adelante, hacia) : 0f;
            if (ang > anguloMax && dist > 0.9f) { continue; }
            float puntaje = dist + ang * 0.02f;
            if (puntaje < mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = inf;
            }
        }
        return mejor;
    }

    // ---------- Empujon ----------

    private void Empujar()
    {
        j.GastarEstamina(12f);
        siguienteEmpujon = Time.time + 1f;
        if (anim != null) { anim.Empujar(); }
        empujonPendiente = true;
        empujonEn = Time.time + 0.18f;
        ultimoCombate = Time.time;
        AudioCap1.Play3D("sfx_swing", transform.position, 0.35f, 0.8f);
    }

    private void ResolverEmpujon()
    {
        empujonPendiente = false;
        foreach (Infectado inf in Infectado.Todos.ToArray())
        {
            if (inf == null || inf.Muerto) { continue; }
            Vector3 hacia = inf.transform.position - transform.position;
            if (Mathf.Abs(hacia.y) > 1.5f) { continue; }
            hacia.y = 0f;
            if (hacia.magnitude - inf.RadioExtra > 1.9f || Vector3.Angle(transform.forward, hacia) > 75f) { continue; }
            inf.RecibirEmpujon(hacia.normalized);
            AudioCap1.Play3D("sfx_golpe_carne", inf.transform.position + Vector3.up, 0.5f, 0.8f);
        }
        SistemaRuido.Emitir(transform.position, 3f, true);
    }

    // ---------- Sigilo ----------

    private Infectado BuscarSigilo()
    {
        if (!j.Agachado)
        {
            return null;
        }
        bool punta = inv.puntas > 0;
        Infectado mejor = null;
        float mejorDist = 1.9f;
        foreach (Infectado inf in Infectado.Todos)
        {
            if (inf == null || !inf.PuedeSerSigiloso(punta)) { continue; }
            Vector3 hacia = transform.position - inf.transform.position;
            if (Mathf.Abs(hacia.y) > 1f) { continue; }
            hacia.y = 0f;
            float dist = hacia.magnitude;
            if (dist > mejorDist) { continue; }
            if (Vector3.Dot(inf.transform.forward, hacia.normalized) > -0.25f && inf.tipo != Infectado.Tipo.Fungico) { continue; }
            mejorDist = dist;
            mejor = inf;
        }
        return mejor;
    }

    /// <summary>Texto del prompt de sigilo.</summary>
    public string TextoSigilo
    {
        get
        {
            if (ObjetivoSigilo == null) { return null; }
            return ObjetivoSigilo.tipo == Infectado.Tipo.Fungico ? "[E]  Apuñalar con la punta" : "[E]  Ataque sigiloso";
        }
    }

    public void AtaqueSigiloso()
    {
        Infectado inf = ObjetivoSigilo;
        if (inf == null)
        {
            return;
        }
        ObjetivoSigilo = null;
        bool conPunta = inf.tipo == Infectado.Tipo.Fungico;
        if (conPunta)
        {
            if (inv.puntas <= 0) { return; }
            inv.puntas--;
        }

        Vector3 detras = inf.transform.position - inf.transform.forward * 0.75f;
        detras.y = transform.position.y;
        j.Teletransportar(detras, Quaternion.LookRotation(inf.transform.forward));
        j.Bloquear(conPunta ? 1f : 1.35f);
        if (anim != null) { anim.Sigilo(); }
        inf.RecibirSigilo(conPunta ? 0.8f : 1.1f);
        SistemaRuido.Emitir(transform.position, 2f, true);
        AudioCap1.Play3D("sfx_estrangular", inf.transform.position + Vector3.up, 0.8f);
        ultimoCombate = Time.time;
        if (conPunta && Juego.I != null) { Juego.I.Mensaje("Usaste una punta"); }
    }

    // ---------- Armas a distancia ----------

    private Ray RayoMira(float dispersionGrados)
    {
        Camera c = Camera.main;
        Ray r = c != null ? c.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)) : new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
        // Ayuda al apuntar
        float ayuda = Dificultad.AyudaApuntado;
        if (ayuda > 0f)
        {
            float mejor = 2.5f + 4f * ayuda;
            Vector3 dirMejor = r.direction;
            foreach (Infectado inf in Infectado.Todos)
            {
                if (inf == null || inf.Muerto) { continue; }
                Vector3 p = inf.transform.position + Vector3.up * inf.AlturaPecho;
                Vector3 d = p - r.origin;
                if (d.magnitude > 35f) { continue; }
                float a = Vector3.Angle(r.direction, d);
                if (a < mejor)
                {
                    mejor = a;
                    dirMejor = Vector3.Slerp(r.direction, d.normalized, 0.35f + 0.5f * ayuda);
                }
            }
            r.direction = dirMejor;
        }
        if (dispersionGrados > 0.01f)
        {
            Vector2 azar = Random.insideUnitCircle * dispersionGrados;
            r.direction = Quaternion.AngleAxis(azar.x, c != null ? c.transform.up : Vector3.up) * Quaternion.AngleAxis(azar.y, c != null ? c.transform.right : Vector3.right) * r.direction;
        }
        return r;
    }

    private void Disparar()
    {
        if (Time.time < siguienteDisparo)
        {
            return;
        }
        if (inv.ranura == Ranura.Honda) { DispararHonda(); }
        else if (inv.ranura == Ranura.Revolver) { DispararRevolver(); }
    }

    private void DispararHonda()
    {
        if (inv.piedras <= 0)
        {
            Juego.I.Mensaje("No tengo piedras para la honda");
            AudioCap1.Play2D("sfx_vacio", 0.5f);
            siguienteDisparo = Time.time + 0.4f;
            return;
        }
        inv.piedras--;
        siguienteDisparo = Time.time + cadenciaHonda;
        Stats.D.disparosHonda++;
        ultimoCombate = Time.time;

        Ray r = RayoMira(Dispersion * 0.5f);
        Vector3 objetivo;
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 60f, ~((1 << 8) | (1 << 2)), QueryTriggerInteraction.Ignore)) { objetivo = hit.point; }
        else { objetivo = r.origin + r.direction * 45f; }

        Vector3 origen = transform.position + Vector3.up * 1.6f + transform.right * 0.3f + transform.forward * 0.3f;
        Proyectiles.Piedra(origen, objetivo, velHonda, danoHonda, this);
        if (anim != null) { anim.Golpear(0.45f); }
        AudioCap1.Play3D("sfx_honda", transform.position + Vector3.up, 0.8f, Random.Range(0.95f, 1.1f));
        SistemaRuido.Emitir(transform.position, 2.5f, true);
        dispersionExtra += 1.2f;
    }

    private void DispararRevolver()
    {
        if (inv.balasCargadas <= 0)
        {
            AudioCap1.Play2D("sfx_vacio", 0.7f);
            siguienteDisparo = Time.time + 0.35f;
            if (inv.balas > 0) { EmpezarRecarga(); }
            else { Juego.I.Mensaje("No me quedan balas"); }
            return;
        }
        inv.balasCargadas--;
        siguienteDisparo = Time.time + cadenciaRevolver;
        Stats.D.disparosRevolver++;
        ultimoCombate = Time.time;

        Ray r = RayoMira(Dispersion * 0.5f);
        Vector3 boca = transform.position + Vector3.up * 1.45f + transform.right * 0.28f + transform.forward * 0.55f;
        Vector3 fin = r.origin + r.direction * 80f;
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 80f, ~((1 << 8) | (1 << 2)), QueryTriggerInteraction.Ignore))
        {
            fin = hit.point;
            Infectado inf = hit.collider.GetComponentInParent<Infectado>();
            if (inf != null && !inf.Muerto)
            {
                bool cabeza = hit.point.y >= inf.transform.position.y + inf.AlturaCabeza - 0.18f;
                inf.RecibirImpacto(cabeza ? danoRevolver * 3 : danoRevolver, r.direction, 1.2f, FuenteDano.Revolver, cabeza, transform.position);
                Stats.D.aciertosRevolver++;
                if (cabeza) { Stats.D.tirosCabeza++; Logros.Revisar(); }
                UltimoImpacto = Time.time;
                UltimoFueCabeza = cabeza;
            }
            else
            {
                EfectosFX.Polvo(hit.point);
            }
        }

        fogonazo.transform.position = boca;
        fogonazo.enabled = true;
        trazador.SetPosition(0, boca);
        trazador.SetPosition(1, fin);
        trazador.enabled = true;
        efectoHasta = Time.time + 0.05f;

        AudioCap1.Play3D("sfx_disparo", boca, 1f, Random.Range(0.95f, 1.05f));
        SistemaRuido.Emitir(transform.position, ruidoDisparo, true);
        CamaraTPS.Sacudir(0.25f, 0.12f);
        CamaraTPS.Retroceso(2.2f);
        dispersionExtra += 2.5f;
        if (anim != null) { anim.Empujar(); }
    }

    /// <summary>Llamado por la piedra al impactar.</summary>
    public void ImpactoPiedra(bool acerto, bool cabeza)
    {
        if (!acerto) { return; }
        Stats.D.aciertosHonda++;
        if (cabeza) { Stats.D.tirosCabeza++; Logros.Revisar(); }
        UltimoImpacto = Time.time;
        UltimoFueCabeza = cabeza;
    }

    private void EmpezarRecarga()
    {
        if (inv.balas <= 0 || inv.balasCargadas >= inv.capacidadTambor)
        {
            return;
        }
        Recargando = true;
        recargaInicio = Time.time;
        ProgresoRecarga = 0f;
        AudioCap1.Play3D("sfx_recarga", transform.position + Vector3.up, 0.7f);
    }

    private void TerminarRecarga()
    {
        Recargando = false;
        int n = Mathf.Min(inv.capacidadTambor - inv.balasCargadas, inv.balas);
        inv.balas -= n;
        inv.balasCargadas += n;
    }

    // ---------- Arrojadizos ----------

    private void Lanzar(bool esMolotov)
    {
        if (esMolotov ? inv.molotovs <= 0 : inv.botellas <= 0)
        {
            if (Juego.I != null) { Juego.I.Mensaje(esMolotov ? "No tengo molotovs. [Tab] para fabricar" : "No tengo botellas para lanzar"); }
            return;
        }
        if (esMolotov) { inv.molotovs--; Stats.D.molotovsLanzados++; }
        else { inv.botellas--; Stats.D.botellasLanzadas++; }

        Camera c = Camera.main;
        Vector3 origen = transform.position + Vector3.up * 1.55f + transform.right * 0.25f;
        Vector3 objetivo = origen + j.AdelanteCamara * 12f;
        if (c != null)
        {
            Ray r = c.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, 40f, ~((1 << 8) | (1 << 2)), QueryTriggerInteraction.Ignore))
            {
                objetivo = hit.point;
            }
            else
            {
                objetivo = r.origin + r.direction * 25f;
            }
        }

        Vector3 hacia = objetivo - origen;
        transform.rotation = Quaternion.LookRotation(new Vector3(hacia.x, 0f, hacia.z).normalized);
        Vector3 vel = Balistica(origen, objetivo, fuerzaLanzamiento);

        GameObject b;
        if (esMolotov) { b = Proyectiles.Molotov(origen); }
        else { b = prefabBotella != null ? Instantiate(prefabBotella, origen, Random.rotation) : Botella.CrearSimple(origen); }
        Collider colBotella = b.GetComponent<Collider>();
        if (colBotella != null && j.Controlador != null) { Physics.IgnoreCollision(colBotella, j.Controlador); }
        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = vel;
            rb.angularVelocity = Random.insideUnitSphere * 12f;
        }
        if (anim != null) { anim.Golpear(0.5f); }
        AudioCap1.Play3D("sfx_swing", transform.position, 0.4f, 1.2f);
    }

    /// <summary>Velocidad inicial para llegar al objetivo con una rapidez aproximada.</summary>
    public static Vector3 Balistica(Vector3 desde, Vector3 hasta, float rapidez)
    {
        Vector3 d = hasta - desde;
        Vector3 plano = new Vector3(d.x, 0f, d.z);
        float dist = plano.magnitude;
        float g = -Physics.gravity.y;
        float t = Mathf.Clamp(dist / rapidez, 0.08f, 2.2f);
        Vector3 v = plano / t;
        v.y = (d.y + 0.5f * g * t * t) / t;
        return v;
    }

    // ---------- Curar ----------

    private void EmpezarCura()
    {
        if (inv.vendas <= 0)
        {
            if (Juego.I != null) { Juego.I.Mensaje(inv.alcohol > 0 && inv.trapo > 0 ? "No tengo vendas. [Tab] para fabricar una" : "No tengo vendas"); }
            return;
        }
        if (j.Salud >= j.saludMax)
        {
            if (Juego.I != null) { Juego.I.Mensaje("No necesito curarme ahora"); }
            return;
        }
        Curando = true;
        curaInicio = Time.time;
        ProgresoCura = 0f;
        if (anim != null) { anim.Curar(tiempoCurar); }
        AudioCap1.Play3D("sfx_vendar", transform.position, 0.7f);
    }

    private void TerminarCura()
    {
        Curando = false;
        inv.vendas--;
        Stats.D.vendasUsadas++;
        j.Curar(curaPorVenda);
        if (Juego.I != null) { Juego.I.Mensaje("+" + curaPorVenda + " salud"); }
    }

    /// <summary>Cancela golpes, recarga y curacion (al recibir dano, esquivar o morir).</summary>
    public void Interrumpir()
    {
        if (Curando)
        {
            Curando = false;
            if (anim != null) { anim.CancelarAcciones(); }
        }
        Recargando = false;
        golpePendiente = false;
        empujonPendiente = false;
        Apuntando = false;
    }
}
