using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Jefe del Capitulo 2: "El Paco Antidisturbios" (subteniente Mamani), un policia en fase temprana
/// de la infeccion, atrincherado en el patio de la UTOP con casco antimotines y chaleco.
/// - Si ve a Wara de pie, apunta (laser rojo) y dispara: revolver de lejos, escopeta de cerca.
///   Agachada detras de las barricadas no la puede ver. Esquivar ([Alt]) justo al disparo evita el tiro.
/// - De frente el casco y el chaleco lo protegen (15% del dano); de costado 60%; por la espalda 130%.
/// - Los ruidos lejos de Wara (botellas, piedras, Tito golpeando chapas) lo distraen: se voltea y
///   dispara hacia alla. Es el momento de flanquearlo.
/// - Si le pegan por el costado o la espalda (o si Wara se le pega de frente), empuja, corre a
///   otra cobertura y su griterio atrae a 2 infectados.
/// - Cada 6 tiros recarga: ventana para atacar.
/// Reutiliza la estructura del jefe del Capitulo 1 (Infectado_Jefe.cs).
/// </summary>
public partial class Infectado
{
    public enum FasePaco { Cubierto, Apuntando, Distraido, Recargando, Empujon, Cambiando }

    [Header("Jefe (solo Paco)")]
    [Tooltip("Coberturas entre las que se mueve (la primera es la inicial).")]
    public Vector3[] coberturas;
    public Material materialLaser;

    public FasePaco EstadoPaco { get; private set; }
    public int TirosCargados { get { return tirosCargados; } }

    private static readonly string[] GritosPaco =
    {
        "¡Salgan con las manos arriba!",
        "¡Esta zona está bajo control policial!",
        "¡Nadie cruza! ¡Órdenes del comando!",
        "¡Los veo! ¡No se muevan!",
        "Me arde... la cabeza me arde...",
        "¡Toque de queda, carajo! ¡Toque de queda!"
    };

    private int coberturaActual;
    private float tPaco;
    private Vector3 puntoDistraccion;
    private float siguienteDisparo;
    private int tirosCargados = 6;
    private int refuerzosLlamados;
    private float siguienteAvisoCasco;
    private float tiempoCerca;
    private bool disparoDistraccionHecho;
    private LineRenderer laser;
    private Light fogonazo;
    private float fogonazoHasta;
    private Vector3 miraSuave;

    private float IntervaloPaco
    {
        get
        {
            float b = Dificultad.Nivel == NivelDificultad.Facil ? 3.4f : (Dificultad.Nivel == NivelDificultad.Superviviente ? 2.1f : 2.7f);
            return fase2 ? b * 0.8f : b;
        }
    }

    private float TiempoApuntar
    {
        get { return Dificultad.Nivel == NivelDificultad.Facil ? 1.25f : (Dificultad.Nivel == NivelDificultad.Superviviente ? 0.75f : 0.95f); }
    }

    private Vector3 CoberturaActual
    {
        get { return coberturas != null && coberturas.Length > 0 ? coberturas[coberturaActual % coberturas.Length] : origen; }
    }

    private void ConfigurarPaco()
    {
        switch (Dificultad.Nivel)
        {
            case NivelDificultad.Facil: VidaMax = 190f; break;
            case NivelDificultad.Superviviente: VidaMax = 360f; break;
            default: VidaMax = 260f; break;
        }
        velDeambular = 1.2f; velInvestigar = 2.4f; velPerseguir = 4.2f;
        rangoVision = 42f; anguloVision = 160f; oido = 1.5f; dano = 20; cadencia = 2f;
    }

    private void IniciarPaco(bool gritar = false)
    {
        CrearVisualesPaco();
        coberturaActual = 0;
        tirosCargados = 6;
        refuerzosLlamados = 0;
        tiempoCerca = 0f;
        siguienteDisparo = Time.time + 2.5f;
        Fase = FaseJefe.Persecucion;
        EstadoActual = Estado.Atacar;
        CambiarPaco(FasePaco.Cubierto);
        if (gritar && Juego.I != null) { Juego.I.Decir("Paco", "¡ALTO AHÍ! ¡Toque de queda! ¡Al suelo o disparo!"); }
    }

    private void CambiarPaco(FasePaco f)
    {
        EstadoPaco = f;
        tPaco = 0f;
        golpeHecho = false;
        disparoDistraccionHecho = false;
        if (laser != null) { laser.enabled = f == FasePaco.Apuntando || f == FasePaco.Distraido; }
        if (!agente.enabled || !agente.isOnNavMesh) { return; }
        if (f == FasePaco.Cambiando)
        {
            agente.isStopped = false;
            agente.speed = velPerseguir * (fase2 ? 1.15f : 1f);
            agente.stoppingDistance = 0.3f;
            agente.SetDestination(CoberturaActual);
            siguienteRepath = Time.time + 0.4f;
        }
        else
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
    }

    private void ActualizarPaco(float dt)
    {
        tPaco += dt;

        // Fuego: contra esto no hay casco
        if (Time.time < jefeQuemadoHasta)
        {
            MostrarFuego(true);
            if (Time.time >= siguienteTicFuego)
            {
                siguienteTicFuego = Time.time + 0.25f;
                Vida -= 4f;
                Stats.D.danoCausado += 4;
                if (Vida <= 0f) { MorirJefe(FuenteDano.Fuego); return; }
            }
        }
        else if (fuegoVisual != null && fuegoVisual.activeSelf)
        {
            MostrarFuego(false);
        }
        if (fogonazo != null && fogonazo.enabled && Time.time > fogonazoHasta) { fogonazo.enabled = false; }

        bool vivo = jugador != null && !jugador.Muerto;
        Vector3 aJ = vivo ? jugador.transform.position - transform.position : transform.forward;
        aJ.y = 0f;
        float dist = aJ.magnitude;
        bool ve = vivo && VePacoAlJugador();

        // Si Wara se le pega de frente, la empuja
        bool cercaDeFrente = vivo && dist < 2.2f && Vector3.Dot(transform.forward, aJ.normalized) > 0.2f;
        tiempoCerca = cercaDeFrente ? tiempoCerca + dt : 0f;
        if (tiempoCerca > 1.2f && EstadoPaco != FasePaco.Empujon && EstadoPaco != FasePaco.Cambiando)
        {
            tiempoCerca = 0f;
            CambiarPaco(FasePaco.Empujon);
        }

        switch (EstadoPaco)
        {
            case FasePaco.Cubierto:
                if (vivo) { GirarHacia(jugador.transform.position, 3f); }
                if (ve && Time.time >= siguienteDisparo)
                {
                    miraSuave = jugador.Pecho;
                    CambiarPaco(FasePaco.Apuntando);
                    AudioCap1.Play3D("sfx_recarga", transform.position + Vector3.up * 1.4f, 0.6f, 1.25f);
                }
                break;

            case FasePaco.Apuntando:
                if (vivo)
                {
                    GirarHacia(jugador.transform.position, 5f);
                    miraSuave = Vector3.Lerp(miraSuave, jugador.Pecho, 1f - Mathf.Exp(-6f * dt));
                }
                ActualizarLaser(miraSuave);
                if (tPaco >= TiempoApuntar)
                {
                    DispararPaco(vivo && VePacoAlJugador(), dist);
                    CambiarPaco(tirosCargados <= 0 ? FasePaco.Recargando : FasePaco.Cubierto);
                }
                break;

            case FasePaco.Distraido:
                GirarHacia(puntoDistraccion, 6f);
                ActualizarLaser(puntoDistraccion + Vector3.up * 0.8f);
                if (!disparoDistraccionHecho && tPaco >= 0.9f)
                {
                    disparoDistraccionHecho = true;
                    DispararHacia(puntoDistraccion + Vector3.up * 0.8f);
                }
                if (tPaco >= 3.6f)
                {
                    siguienteDisparo = Time.time + 0.8f;
                    CambiarPaco(tirosCargados <= 0 ? FasePaco.Recargando : FasePaco.Cubierto);
                }
                break;

            case FasePaco.Recargando:
                if (tPaco >= (fase2 ? 2.4f : 3.2f))
                {
                    tirosCargados = 6;
                    AudioCap1.Play3D("sfx_recarga", transform.position + Vector3.up * 1.2f, 0.8f, 0.9f);
                    siguienteDisparo = Time.time + 0.6f;
                    CambiarPaco(FasePaco.Cubierto);
                }
                break;

            case FasePaco.Empujon:
                if (vivo) { GirarHacia(jugador.transform.position, 12f); }
                if (!golpeHecho && tPaco >= 0.35f)
                {
                    golpeHecho = true;
                    if (anim != null) { anim.AtacarInfectado(); }
                    AudioCap1.Play3D("sfx_swing", transform.position + Vector3.up * 1.4f, 1f, 0.8f);
                    if (vivo && dist < 2.6f && !jugador.Esquivando)
                    {
                        jugador.RecibirDano(8, transform.position, "El Paco te derribó de un culatazo. Pégale por la espalda y aléjate.");
                        jugador.Impulsar(aJ.normalized * 8f + Vector3.up * 2f);
                    }
                }
                if (tPaco >= 0.9f) { SiguienteCobertura(); }
                break;

            case FasePaco.Cambiando:
                if (agente.enabled && agente.isOnNavMesh && Time.time >= siguienteRepath)
                {
                    siguienteRepath = Time.time + 0.4f;
                    agente.SetDestination(CoberturaActual);
                }
                Vector3 d = CoberturaActual - transform.position; d.y = 0f;
                if (d.magnitude < 0.8f || tPaco > 7f)
                {
                    siguienteDisparo = Time.time + 1.4f;
                    CambiarPaco(FasePaco.Cubierto);
                }
                break;
        }

        if (anim != null)
        {
            anim.velocidad = agente.enabled ? agente.velocity.magnitude : 0f;
            anim.alerta = true;
            anim.agachado = EstadoPaco == FasePaco.Recargando || (EstadoPaco == FasePaco.Cubierto && !ve);
        }
        if (EstadoPaco == FasePaco.Cubierto && Time.time >= siguienteSonido && Juego.I != null && !Juego.I.HablandoAlguien)
        {
            siguienteSonido = Time.time + Random.Range(7f, 11f);
            Juego.I.Decir("Paco", GritosPaco[Random.Range(0, GritosPaco.Length)]);
        }
    }

    private bool VePacoAlJugador()
    {
        if (jugador == null || jugador.Muerto) { return false; }
        Vector3 ojos = transform.position + Vector3.up * 1.6f;
        Vector3 hacia = jugador.Pecho - ojos;
        float dist = hacia.magnitude;
        if (dist > rangoVision || dist < 0.01f) { return false; }
        Vector3 plano = hacia; plano.y = 0f;
        if (EstadoPaco != FasePaco.Apuntando && Vector3.Angle(transform.forward, plano) > anguloVision * 0.5f) { return false; }
        return !Physics.Raycast(ojos, hacia / dist, dist - 0.3f, capaObstaculos, QueryTriggerInteraction.Ignore);
    }

    private Vector3 PuntoBoca()
    {
        return transform.position + Vector3.up * 1.35f + transform.forward * 0.6f + transform.right * 0.2f;
    }

    private void CrearVisualesPaco()
    {
        if (laser == null)
        {
            var go = new GameObject("LaserPaco");
            go.transform.SetParent(transform, false);
            laser = go.AddComponent<LineRenderer>();
            laser.positionCount = 2;
            laser.startWidth = 0.03f;
            laser.endWidth = 0.015f;
            laser.useWorldSpace = true;
            laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laser.receiveShadows = false;
            if (materialLaser != null) { laser.sharedMaterial = materialLaser; }
            laser.startColor = new Color(1f, 0.1f, 0.05f, 0.9f);
            laser.endColor = new Color(1f, 0.1f, 0.05f, 0.6f);
            laser.enabled = false;
        }
        if (fogonazo == null)
        {
            var gl = new GameObject("Fogonazo");
            gl.transform.SetParent(transform, false);
            gl.transform.localPosition = new Vector3(0.2f, 1.35f, 0.75f);
            fogonazo = gl.AddComponent<Light>();
            fogonazo.type = LightType.Point;
            fogonazo.range = 8f;
            fogonazo.intensity = 14f;
            fogonazo.color = new Color(1f, 0.75f, 0.4f);
            fogonazo.enabled = false;
        }
    }

    private void ActualizarLaser(Vector3 hacia)
    {
        if (laser == null) { return; }
        Vector3 boca = PuntoBoca();
        Vector3 dir = hacia - boca;
        if (dir.sqrMagnitude < 0.01f) { return; }
        RaycastHit hit;
        Vector3 fin = Physics.Raycast(boca, dir.normalized, out hit, 60f, capaObstaculos | (1 << 8), QueryTriggerInteraction.Ignore) ? hit.point : boca + dir.normalized * 60f;
        laser.SetPosition(0, boca);
        laser.SetPosition(1, fin);
    }

    private void EfectoDisparo(bool escopeta)
    {
        Vector3 boca = PuntoBoca();
        AudioCap1.Play3D(escopeta ? "sfx_escopeta" : "sfx_disparo", boca, 1f);
        if (fogonazo != null) { fogonazo.enabled = true; fogonazoHasta = Time.time + 0.07f; }
        if (jugador != null && Vector3.Distance(jugador.transform.position, transform.position) < 25f) { CamaraTPS.Sacudir(0.12f, 0.15f); }
        SistemaRuido.Emitir(transform.position, 22f, false);
    }

    private void DispararPaco(bool acierta, float dist)
    {
        tirosCargados--;
        bool escopeta = dist < 9f;
        EfectoDisparo(escopeta);
        siguienteDisparo = Time.time + IntervaloPaco;
        Vector3 boca = PuntoBoca();
        if (acierta && jugador != null && !jugador.Esquivando)
        {
            int d = escopeta ? 34 : 22;
            if (fase2) { d += 4; }
            jugador.RecibirDano(d, transform.position, escopeta
                ? "El Paco te voló con la escopeta. No te le acerques de frente."
                : "El Paco te disparó. Agáchate detrás de las barricadas: si te ve de pie, dispara.");
            EfectosFX.Sangre(jugador.Pecho, (jugador.Pecho - boca).normalized);
            return;
        }
        Vector3 objetivo = jugador != null ? jugador.Pecho : boca + transform.forward * 10f;
        RaycastHit hit;
        Vector3 dir = (objetivo - boca).normalized + Random.insideUnitSphere * 0.04f;
        if (Physics.Raycast(boca, dir, out hit, 60f, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            EfectosFX.Polvo(hit.point);
            AudioCap1.Play3D("sfx_golpe_arma", hit.point, 0.6f, 1.6f);
        }
    }

    private void DispararHacia(Vector3 punto)
    {
        if (tirosCargados <= 0)
        {
            AudioCap1.Play3D("sfx_vacio", transform.position + Vector3.up * 1.3f, 0.8f);
            return;
        }
        tirosCargados--;
        EfectoDisparo(false);
        Vector3 boca = PuntoBoca();
        RaycastHit hit;
        if (Physics.Raycast(boca, (punto - boca).normalized, out hit, 60f, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            EfectosFX.Polvo(hit.point);
            AudioCap1.Play3D("sfx_golpe_arma", hit.point, 0.7f, 1.5f);
        }
    }

    private void PacoOirRuido(Vector3 pos, float radio, bool delJugador)
    {
        if (!JefeActivo || delJugador) { return; }
        float d = Vector3.Distance(pos, transform.position);
        if (d < 3f || d > radio * 2.5f + 4f) { return; }
        if (jugador != null && Vector3.Distance(pos, jugador.transform.position) < 3.5f) { return; }
        if (EstadoPaco == FasePaco.Empujon || EstadoPaco == FasePaco.Cambiando || EstadoPaco == FasePaco.Distraido) { return; }
        if (EstadoPaco == FasePaco.Apuntando && tPaco > TiempoApuntar * 0.6f) { return; }
        puntoDistraccion = pos;
        CambiarPaco(FasePaco.Distraido);
        if (Juego.I != null && Time.time - inicioCombate < 90f) { Juego.I.Mensaje("¡Se volteó! Atácalo por la espalda"); }
    }

    private void PacoRecibirDano(int cantidad, Vector3 direccion, FuenteDano fuente, Vector3 origenAtaque)
    {
        Vector3 hacia = origenAtaque - transform.position; hacia.y = 0f;
        float dot = hacia.sqrMagnitude > 0.01f ? Vector3.Dot(transform.forward, hacia.normalized) : 1f;
        float mult = dot > 0.35f ? 0.15f : (dot > -0.25f ? 0.6f : 1.3f);
        if (Time.time < jefeQuemadoHasta) { mult = Mathf.Max(mult, 0.8f); }
        if (fuente == FuenteDano.Fuego) { mult = 1f; }
        if (fuente == FuenteDano.Botella) { mult *= 0.3f; }

        int real = Mathf.Max(1, Mathf.RoundToInt(cantidad * mult));
        Vida -= real;
        Stats.D.danoCausado += real;
        UltimaFuente = fuente;
        JefeUltimoGolpe = Time.time;
        if (mult <= 0.2f)
        {
            AudioCap1.Play3D("sfx_golpe_arma", transform.position + Vector3.up * 1.5f, 0.9f, 1.5f);
            EfectosFX.Polvo(transform.position + Vector3.up * 1.4f + transform.forward * 0.3f);
            if (Juego.I != null && Time.time >= siguienteAvisoCasco)
            {
                siguienteAvisoCasco = Time.time + 8f;
                Juego.I.Mensaje("¡Rebota en el casco y el chaleco! Atácalo por la espalda");
            }
        }
        else
        {
            EfectosFX.Sangre(transform.position + Vector3.up * 1.4f, direccion);
            AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up * 1.7f, 0.8f, 0.9f);
        }

        if (Vida <= 0f)
        {
            MorirJefe(fuente);
            return;
        }
        if (!fase2 && Vida < VidaMax * 0.5f)
        {
            fase2 = true;
            if (alFase2 != null) { alFase2.Ejecutar(transform.position); }
            if (Juego.I != null) { Juego.I.Decir("Paco", "¡¿Así que así va a ser?! ¡Refuerzos! ¡REFUERZOS!"); }
            LlamarRefuerzos();
        }
        if (mult >= 0.6f && fuente != FuenteDano.Fuego && EstadoPaco != FasePaco.Empujon && EstadoPaco != FasePaco.Cambiando)
        {
            CambiarPaco(FasePaco.Empujon);
        }
        else if (mult <= 0.2f && EstadoPaco == FasePaco.Cubierto)
        {
            siguienteDisparo = Mathf.Min(siguienteDisparo, Time.time + 0.4f);
        }
    }

    private void SiguienteCobertura()
    {
        if (coberturas != null && coberturas.Length > 1)
        {
            int actual = coberturaActual % coberturas.Length;
            int mejor = actual;
            float mejorD = -1f;
            for (int i = 0; i < coberturas.Length; i++)
            {
                if (i == actual) { continue; }
                float d = jugador != null ? Vector3.Distance(coberturas[i], jugador.transform.position) : i;
                if (d > mejorD) { mejorD = d; mejor = i; }
            }
            coberturaActual = mejor;
        }
        LlamarRefuerzos();
        CambiarPaco(FasePaco.Cambiando);
    }

    private void LlamarRefuerzos()
    {
        if (refuerzos == null) { return; }
        int n = 0;
        while (refuerzosLlamados < refuerzos.Length && n < 2)
        {
            Infectado r = refuerzos[refuerzosLlamados++];
            if (r != null && !r.Muerto)
            {
                r.gameObject.SetActive(true);
                r.Alertar();
                n++;
            }
        }
        if (n > 0 && Juego.I != null) { Juego.I.Mensaje("Los gritos del Paco atrajeron a más infectados"); }
    }

    private void ReiniciarPaco()
    {
        ConfigurarPaco();
        Vida = VidaMax;
        fase2 = false;
        jefeQuemadoHasta = 0f;
        MostrarFuego(false);
        if (agente.enabled) { agente.Warp(origen); }
        transform.rotation = rotOrigen;
        inicioCombate = Time.time;
        JefeEnCombate = this;
        AudioCap1.Musica("mus_jefe", 1f);
        IniciarPaco(true);
    }

    private void ApagarPaco()
    {
        if (laser != null) { laser.enabled = false; }
        if (fogonazo != null) { fogonazo.enabled = false; }
    }
}
