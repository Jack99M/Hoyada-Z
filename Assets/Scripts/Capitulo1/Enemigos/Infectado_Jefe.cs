using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Jefe "El Carnicero" (don Cirilo, puesto 7 del Mercado Sopocachi): un infectado enorme.
/// - Barrido: golpe de brazo a corta distancia.
/// - Embestida: ruge (aviso) y carga en linea recta. Si choca contra un puesto o una pared
///   queda aturdido: es el momento de pegarle (dano x2).
/// - Pisoton: si te quedas pegado a el, golpea el suelo y te lanza.
/// - A la mitad de la vida ruge y llama a otros infectados del mercado.
/// Punto debil: el saco fungico de la espalda (dano x1.5 por detras). El fuego lo hace
/// vulnerable (dano x1.8 mientras arde).
/// </summary>
public partial class Infectado
{
    public enum FaseJefe { Dormido, Rugido, Persecucion, Barrido, PreparaCarga, Carga, Aturdido, Pisoton, Invocar, Muerto }

    [Header("Jefe (solo Carnicero)")]
    public string nombreJefe = "EL CARNICERO";
    public string subtituloJefe = "Don Cirilo, puesto 7";
    public Infectado[] refuerzos;
    public Acciones alIniciarJefe = new Acciones();
    public Acciones alFase2 = new Acciones();

    public static Infectado JefeEnCombate { get; private set; }
    public FaseJefe Fase { get; private set; } = FaseJefe.Dormido;
    public bool JefeActivo { get { return tipo == Tipo.Carnicero && Fase != FaseJefe.Dormido && Fase != FaseJefe.Muerto; } }
    public bool JefeFase2 { get { return fase2; } }
    public float JefeUltimoGolpe { get; private set; } = -10f;

    private float tFase;
    private Vector3 dirCarga;
    private float recorridoCarga;
    private int framesBloqueado;
    private bool fase2;
    private bool golpeHecho;
    private float jefeQuemadoHasta;
    private float siguienteCarga;
    private float siguientePisoton;
    private float tiempoPegado;
    private float inicioCombate;
    private float siguienteTicFuego;
    private float siguienteRugidoDolor;

    private void ConfigurarJefe()
    {
        switch (Dificultad.Nivel)
        {
            case NivelDificultad.Facil: VidaMax = 650f; break;
            case NivelDificultad.Superviviente: VidaMax = 1250f; break;
            default: VidaMax = 900f; break;
        }
        velDeambular = 1.2f; velInvestigar = 2f; velPerseguir = 2.4f;
        rangoVision = 30f; anguloVision = 360f; oido = 2f; dano = 30; cadencia = 1.3f;
    }

    /// <summary>Empieza la pelea (lo llama el evento del mercado).</summary>
    public void IniciarJefe()
    {
        if (tipo != Tipo.Carnicero || Muerto || JefeActivo)
        {
            return;
        }
        if (!gameObject.activeSelf) { gameObject.SetActive(true); }
        if (jugador == null) { jugador = Jugador.I; }
        // La dificultad se elige en el menu despues de cargar la escena: se recalcula aqui.
        ConfigurarJefe();
        Vida = VidaMax;
        JefeEnCombate = this;
        inicioCombate = Time.time;
        siguienteCarga = Time.time + 4f;
        siguientePisoton = Time.time + 8f;
        CambiarFase(FaseJefe.Rugido);
        AudioCap1.Musica("mus_jefe", 1f);
        if (alIniciarJefe != null) { alIniciarJefe.Ejecutar(transform.position); }
    }

    private void CambiarFase(FaseJefe f)
    {
        Fase = f;
        tFase = 0f;
        golpeHecho = false;
        EstadoActual = f == FaseJefe.Persecucion ? Estado.Perseguir : (f == FaseJefe.Aturdido ? Estado.Aturdido : (f == FaseJefe.Dormido ? Estado.Quieto : Estado.Atacar));
        if (agente.enabled && agente.isOnNavMesh)
        {
            bool mover = f == FaseJefe.Persecucion;
            agente.isStopped = !mover;
            if (!mover) { agente.ResetPath(); }
            agente.speed = fase2 ? velPerseguir * 1.25f : velPerseguir;
            agente.stoppingDistance = 1.8f;
        }
        switch (f)
        {
            case FaseJefe.Rugido:
            case FaseJefe.Invocar:
                AudioCap1.Play3D("sfx_rugido", transform.position + Vector3.up * 2f, 1f, f == FaseJefe.Invocar ? 0.85f : 1f);
                CamaraTPS.Sacudir(0.35f, 1.2f);
                SistemaRuido.Emitir(transform.position, 20f, false);
                if (anim != null) { anim.Aturdir(); }
                break;
            case FaseJefe.PreparaCarga:
                AudioCap1.Play3D("sfx_rugido", transform.position + Vector3.up * 2f, 0.8f, 1.25f);
                break;
            case FaseJefe.Barrido:
                if (anim != null) { anim.AtacarInfectado(); }
                AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up * 2f, 1f, 0.6f);
                break;
            case FaseJefe.Pisoton:
                if (anim != null) { anim.Aturdir(); }
                AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up * 2f, 1f, 0.5f);
                break;
            case FaseJefe.Aturdido:
                if (anim != null) { anim.Aturdir(); }
                break;
        }
    }

    private void ActualizarJefe(float dt)
    {
        if (Fase == FaseJefe.Dormido)
        {
            if (anim != null) { anim.velocidad = 0f; anim.alerta = false; anim.agachado = false; }
            return;
        }
        if (jugador == null) { jugador = Jugador.I; }
        tFase += dt;

        // Fuego
        if (Time.time < jefeQuemadoHasta)
        {
            MostrarFuego(true);
            if (Time.time >= siguienteTicFuego)
            {
                siguienteTicFuego = Time.time + 0.25f;
                Vida -= 5f;
                Stats.D.danoCausado += 5;
                if (Vida <= 0f) { MorirJefe(FuenteDano.Fuego); return; }
            }
        }
        else if (fuegoVisual != null && fuegoVisual.activeSelf)
        {
            MostrarFuego(false);
        }

        Vector3 aJugador = jugador != null ? jugador.transform.position - transform.position : transform.forward;
        aJugador.y = 0f;
        float dist = aJugador.magnitude;
        bool jugadorVivo = jugador != null && !jugador.Muerto;

        switch (Fase)
        {
            case FaseJefe.Rugido:
                GirarHacia(transform.position + aJugador, 4f);
                if (tFase > 1.8f) { CambiarFase(FaseJefe.Persecucion); }
                break;

            case FaseJefe.Invocar:
                if (tFase > 2.2f) { CambiarFase(FaseJefe.Persecucion); }
                break;

            case FaseJefe.Persecucion:
                if (!jugadorVivo) { if (agente.isOnNavMesh) { agente.isStopped = true; } break; }
                if (agente.enabled && agente.isOnNavMesh && Time.time >= siguienteRepath)
                {
                    siguienteRepath = Time.time + 0.2f;
                    agente.SetDestination(jugador.transform.position);
                }
                if (dist < 3.5f) { GirarHacia(jugador.transform.position, 6f); }
                tiempoPegado = dist < 3.2f ? tiempoPegado + dt : Mathf.Max(0f, tiempoPegado - dt);

                if (dist < 2.7f && Time.time >= siguienteAtaque)
                {
                    if (tiempoPegado > 2.5f && Time.time >= siguientePisoton) { CambiarFase(FaseJefe.Pisoton); }
                    else { CambiarFase(FaseJefe.Barrido); }
                }
                else if (dist > 5f && dist < 17f && Time.time >= siguienteCarga && VeJugadorDirecto())
                {
                    CambiarFase(FaseJefe.PreparaCarga);
                }
                break;

            case FaseJefe.Barrido:
                GirarHacia(transform.position + aJugador, 5f);
                if (!golpeHecho && tFase >= 0.55f)
                {
                    golpeHecho = true;
                    if (jugadorVivo && dist < 3f + RadioExtra && Vector3.Angle(transform.forward, aJugador) < 70f && !jugador.Esquivando)
                    {
                        jugador.RecibirDano(fase2 ? 34 : 28, transform.position, "El Carnicero te partió en dos. Esquiva con [Alt] cuando levante el brazo.");
                        jugador.Impulsar(aJugador.normalized * 5f);
                    }
                    AudioCap1.Play3D("sfx_swing", transform.position + Vector3.up * 1.5f, 1f, 0.6f);
                }
                if (tFase >= 1.2f)
                {
                    siguienteAtaque = Time.time + (fase2 ? 0.8f : 1.2f);
                    CambiarFase(FaseJefe.Persecucion);
                }
                break;

            case FaseJefe.PreparaCarga:
                GirarHacia(transform.position + aJugador, 7f);
                if (tFase >= (fase2 ? 0.7f : 0.95f))
                {
                    dirCarga = aJugador.sqrMagnitude > 0.01f ? aJugador.normalized : transform.forward;
                    recorridoCarga = 0f;
                    framesBloqueado = 0;
                    CambiarFase(FaseJefe.Carga);
                    AudioCap1.Play3D("sfx_grito", transform.position + Vector3.up * 2f, 1f, 0.55f);
                }
                break;

            case FaseJefe.Carga:
                ActualizarCarga(dt, jugadorVivo);
                break;

            case FaseJefe.Aturdido:
                if (tFase >= (fase2 ? 2.4f : 3.2f))
                {
                    siguienteCarga = Time.time + (fase2 ? 3.5f : 5.5f);
                    CambiarFase(FaseJefe.Persecucion);
                }
                break;

            case FaseJefe.Pisoton:
                if (!golpeHecho && tFase >= 0.85f)
                {
                    golpeHecho = true;
                    AudioCap1.Play3D("sfx_golpe_suelo", transform.position, 1f);
                    EfectosFX.Polvo(transform.position + Vector3.up * 0.2f);
                    EfectosFX.Polvo(transform.position + transform.forward + Vector3.up * 0.2f);
                    CamaraTPS.Sacudir(0.6f, 0.5f);
                    SistemaRuido.Emitir(transform.position, 14f, false);
                    if (jugadorVivo && dist < 4f && !jugador.Esquivando)
                    {
                        jugador.RecibirDano(fase2 ? 26 : 20, transform.position, "El pisotón del Carnicero te aplastó. No te quedes pegado a él.");
                        jugador.Impulsar(aJugador.normalized * 9f + Vector3.up * 3f);
                    }
                    tiempoPegado = 0f;
                    siguientePisoton = Time.time + 9f;
                }
                if (tFase >= 1.6f)
                {
                    siguienteAtaque = Time.time + 1f;
                    CambiarFase(FaseJefe.Persecucion);
                }
                break;
        }

        if (anim != null)
        {
            anim.velocidad = Fase == FaseJefe.Carga ? 6f : (agente.enabled ? agente.velocity.magnitude : 0f);
            anim.alerta = Fase != FaseJefe.Aturdido;
            anim.agachado = Fase == FaseJefe.PreparaCarga || Fase == FaseJefe.Aturdido;
        }
        if (Fase == FaseJefe.Persecucion && Time.time >= siguienteSonido)
        {
            siguienteSonido = Time.time + Random.Range(2f, 4f);
            AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up * 2f, 1f, Random.Range(0.5f, 0.65f));
        }
    }

    private bool VeJugadorDirecto()
    {
        if (jugador == null) { return false; }
        Vector3 ojos = transform.position + Vector3.up * 1.5f;
        Vector3 hacia = jugador.Pecho - ojos;
        return !Physics.SphereCast(ojos, 0.4f, hacia.normalized, out _, hacia.magnitude - 0.5f, capaObstaculos, QueryTriggerInteraction.Ignore);
    }

    private void ActualizarCarga(float dt, bool jugadorVivo)
    {
        float vel = fase2 ? 10.5f : 9f;
        Vector3 antes = transform.position;
        if (agente.enabled && agente.isOnNavMesh) { agente.Move(dirCarga * vel * dt); }
        transform.rotation = Quaternion.LookRotation(dirCarga);
        float movido = (transform.position - antes).magnitude;
        recorridoCarga += movido;

        // Choque contra obstaculo: aturdido
        bool obstaculo = Physics.SphereCast(transform.position + Vector3.up * 1f, 0.5f, dirCarga, out _, 0.9f, capaObstaculos, QueryTriggerInteraction.Ignore);
        framesBloqueado = movido < vel * dt * 0.35f ? framesBloqueado + 1 : 0;
        if ((obstaculo && tFase > 0.15f) || framesBloqueado >= 3)
        {
            AudioCap1.Play3D("sfx_golpe_suelo", transform.position + dirCarga, 1f, 0.8f);
            AudioCap1.Play3D("sfx_rompe_palo", transform.position + dirCarga, 0.9f, 0.6f);
            EfectosFX.Polvo(transform.position + dirCarga + Vector3.up);
            CamaraTPS.Sacudir(0.5f, 0.4f);
            SistemaRuido.Emitir(transform.position, 16f, false);
            if (Juego.I != null && Stats.D.tiempoJefe <= 0f && Time.time - inicioCombate < 40f) { Juego.I.Mensaje("¡Chocó! Está aturdido: ¡ahora!"); }
            CambiarFase(FaseJefe.Aturdido);
            return;
        }

        // Golpe al jugador
        if (jugadorVivo)
        {
            Vector3 d = jugador.transform.position - transform.position; d.y = 0f;
            if (d.magnitude < 1.5f + RadioExtra && Vector3.Dot(d.normalized, dirCarga) > 0.2f && !jugador.Esquivando)
            {
                jugador.RecibirDano(fase2 ? 45 : 38, transform.position, "La embestida del Carnicero te aplastó. Esquiva a un lado con [Alt] y hazlo chocar contra los puestos.");
                jugador.Impulsar(dirCarga * 10f + Vector3.up * 3.5f);
                siguienteCarga = Time.time + (fase2 ? 4f : 6f);
                CambiarFase(FaseJefe.Persecucion);
                return;
            }
        }

        if (recorridoCarga > 18f || tFase > 3f)
        {
            siguienteCarga = Time.time + (fase2 ? 4f : 6f);
            CambiarFase(FaseJefe.Persecucion);
        }
    }

    private void JefeOirRuido(Vector3 pos)
    {
        if (JefeActivo) { ultimaPosVista = pos; ultimoVisto = Time.time; }
    }

    private void JefeQuemar(float segundos)
    {
        bool nuevo = Time.time >= jefeQuemadoHasta;
        jefeQuemadoHasta = Mathf.Max(jefeQuemadoHasta, Time.time + segundos);
        UltimaFuente = FuenteDano.Fuego;
        if (nuevo && Time.time >= siguienteRugidoDolor)
        {
            siguienteRugidoDolor = Time.time + 3f;
            AudioCap1.Play3D("sfx_rugido", transform.position + Vector3.up * 2f, 0.9f, 1.15f);
            if (Juego.I != null) { Juego.I.Mensaje("¡Está ardiendo! Le haces mucho más daño"); }
        }
    }

    private void JefeRecibirDano(int cantidad, Vector3 direccion, FuenteDano fuente, Vector3 origenAtaque)
    {
        if (!JefeActivo)
        {
            return;
        }
        float mult = 1f;
        Vector3 haciaAtacante = origenAtaque - transform.position; haciaAtacante.y = 0f;
        bool espalda = haciaAtacante.sqrMagnitude > 0.01f && Vector3.Dot(transform.forward, haciaAtacante.normalized) < -0.3f;
        if (espalda) { mult *= 1.5f; }
        if (Fase == FaseJefe.Aturdido) { mult *= 2f; }
        if (Time.time < jefeQuemadoHasta) { mult *= 1.8f; }
        if (fuente == FuenteDano.Botella) { mult *= 0.3f; }

        int real = Mathf.RoundToInt(cantidad * mult);
        Vida -= real;
        Stats.D.danoCausado += real;
        UltimaFuente = fuente;
        JefeUltimoGolpe = Time.time;
        EfectosFX.Sangre(transform.position + Vector3.up * 1.8f, direccion);
        if (espalda) { EfectosFX.Sangre(transform.position + Vector3.up * 2f - transform.forward * 0.4f, -transform.forward); }
        AudioCap1.Play3D("sfx_grunido", transform.position + Vector3.up * 2f, 0.7f, Random.Range(0.45f, 0.6f));

        if (Vida <= 0f)
        {
            MorirJefe(fuente);
            return;
        }
        if (!fase2 && Vida < VidaMax * 0.5f)
        {
            fase2 = true;
            CambiarFase(FaseJefe.Invocar);
            if (refuerzos != null)
            {
                foreach (Infectado r in refuerzos)
                {
                    if (r != null && !r.Muerto) { r.gameObject.SetActive(true); r.Alertar(); }
                }
            }
            if (alFase2 != null) { alFase2.Ejecutar(transform.position); }
        }
    }

    private void MorirJefe(FuenteDano fuente)
    {
        if (Muerto)
        {
            return;
        }
        Fase = FaseJefe.Muerto;
        EstadoActual = Estado.Muerto;
        MostrarFuego(false);
        if (agente.enabled)
        {
            if (agente.isOnNavMesh) { agente.isStopped = true; }
            agente.enabled = false;
        }
        foreach (Collider c in colisiones) { if (c != null) { c.enabled = false; } }
        if (anim != null) { anim.velocidad = 0f; anim.alerta = false; anim.agachado = false; anim.Morir(); }
        AudioCap1.Play3D("sfx_rugido", transform.position + Vector3.up * 2f, 1f, 0.7f);
        AudioCap1.Play3D("sfx_golpe_suelo", transform.position, 1f, 0.7f);
        EfectosFX.Sangre(transform.position + Vector3.up * 1.5f, Vector3.up);
        CamaraTPS.Sacudir(0.5f, 0.8f);
        JefeEnCombate = null;

        float duracion = Time.time - inicioCombate;
        Stats.D.jefeDerrotado = true;
        Stats.D.tiempoJefe = duracion;
        Stats.Kill(tipo, fuente);
        Logros.Desbloquear("jefe");
        if (duracion < 90f) { Logros.Desbloquear("jefe_rapido"); }
        AudioCap1.Musica(null, 2f);
        if (Juego.I != null) { Juego.I.RegistrarMuerteInfectado(this); }
        if (soltarAlMorir != null)
        {
            soltarAlMorir.transform.position = transform.position + transform.right * 1f + Vector3.up * 0.05f;
            soltarAlMorir.SetActive(true);
        }
        if (alMorir != null) { alMorir.Ejecutar(transform.position); }
    }

    private void JefeReiniciar()
    {
        if (Fase == FaseJefe.Dormido)
        {
            return;
        }
        ConfigurarJefe();
        Vida = VidaMax;
        fase2 = false;
        jefeQuemadoHasta = 0f;
        MostrarFuego(false);
        if (agente.enabled) { agente.Warp(origen); }
        transform.rotation = rotOrigen;
        inicioCombate = Time.time;
        siguienteCarga = Time.time + 5f;
        siguienteAtaque = Time.time + 2f;
        JefeEnCombate = this;
        CambiarFase(FaseJefe.Rugido);
        AudioCap1.Musica("mus_jefe", 1f);
    }

    /// <summary>Al cargar una partida con el jefe ya derrotado.</summary>
    public void MarcarJefeDerrotado()
    {
        Fase = FaseJefe.Muerto;
        MarcarMuerto();
    }
}
