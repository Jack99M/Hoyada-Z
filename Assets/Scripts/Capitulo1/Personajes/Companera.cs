using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Wara, la cebra: una joven voluntaria de las "cebras" de La Paz (educadoras viales con
/// traje de cebra). Se une a Mateo en la Plaza Abaroa y lo acompaña hasta las gradas.
/// Como Ellie en The Last of Us: los infectados la ignoran, lo sigue agachandose cuando
/// Mateo se agacha, le avisa si alguien viene por detras, le tira piedras con su honda a
/// los infectados que lo atacan y, en la pelea con el Carnicero, se esconde y da consejos.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Companera : MonoBehaviour
{
    public enum EstadoC { Escondida = 0, Siguiendo = 1, Escondiendose = 2, Separada = 3 }

    public static Companera I { get; private set; }

    public string nombre = "Wara";
    public EstadoC estado = EstadoC.Escondida;
    [Tooltip("Donde se esconde durante la pelea con el jefe.")]
    public Vector3 escondite;
    [Tooltip("Hacia donde se va cuando se separa de Mateo.")]
    public Vector3 destinoSeparacion;
    public float intervaloAyuda = 8f;

    public bool Siguiendo { get { return estado == EstadoC.Siguiendo; } }
    public int EstadoGuardado { get { return (int)estado; } }

    private static readonly string[] Avisos = { "¡Mateo, atrás tuyo!", "¡Cuidado, detrás!", "¡Ahí viene uno por tu espalda!", "¡Mateo, date la vuelta!" };
    private static readonly string[] Tiros = { "¡Toma esto, desgraciado!", "¡Suéltalo!", "¡Ya, ya, te lo quité!", "¡Jallalla, le di!" };
    private static readonly string[] ConsejosJefe =
    {
        "¡Mateo! ¡Hazlo chocar contra los puestos, cuando carga no puede frenar!",
        "¡Cuando se aturde, dale con todo!",
        "¡En la espalda tiene como un saco de hongos! ¡Pégale ahí!",
        "¡El fuego! ¡Si tienes alcohol y un trapo, hazle un molotov!",
        "¡No te quedes pegado a él, va a pisotear!",
        "¡Esquiva a un lado cuando ruja! ¡Con espacio!"
    };

    private NavMeshAgent agente;
    private AnimProcedural anim;
    private float siguienteRepath;
    private float siguienteAyuda;
    private float siguienteAviso;
    private float siguienteConsejo;
    private float inicioSeparacion;
    private Infectado objetivoTiro;
    private float tiroEn = -1f;
    private int consejo;

    private void Awake()
    {
        I = this;
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<AnimProcedural>();
        agente.radius = 0.25f;
        agente.height = 1.6f;
        agente.speed = 3f;
        agente.acceleration = 14f;
        agente.angularSpeed = 540f;
        agente.stoppingDistance = 0.6f;
        agente.avoidancePriority = 80;
        if (anim != null)
        {
            anim.estilo = AnimProcedural.Estilo.Humano;
            anim.velocidadCaminar = 2.6f;
            anim.velocidadCorrer = 5.6f;
        }
    }

    private void OnDestroy()
    {
        if (I == this) { I = null; }
    }

    private void Update()
    {
        Jugador j = Jugador.I;
        if (j == null || Juego.I == null)
        {
            return;
        }
        if (Juego.I.estado == Juego.Estado.Menu || Juego.I.estado == Juego.Estado.Intro)
        {
            return;
        }

        switch (estado)
        {
            case EstadoC.Escondida:
                if (agente.enabled && agente.isOnNavMesh) { agente.isStopped = true; }
                if (anim != null) { anim.velocidad = 0f; anim.agachado = true; anim.alerta = false; }
                MirarA(j.transform.position, 3f);
                break;

            case EstadoC.Siguiendo:
                Seguir(j);
                Ayudar(j);
                Avisar(j);
                break;

            case EstadoC.Escondiendose:
                IrA(escondite, 5.5f);
                if (anim != null) { anim.agachado = agente.remainingDistance < 1f; anim.velocidad = agente.velocity.magnitude; anim.alerta = true; }
                if (Infectado.JefeEnCombate != null && Time.time >= siguienteConsejo && !Juego.I.HablandoAlguien)
                {
                    siguienteConsejo = Time.time + Random.Range(11f, 16f);
                    Juego.I.Decir(nombre, ConsejosJefe[consejo % ConsejosJefe.Length]);
                    consejo++;
                }
                break;

            case EstadoC.Separada:
                IrA(destinoSeparacion, 4.5f);
                if (anim != null) { anim.agachado = false; anim.velocidad = agente.velocity.magnitude; anim.alerta = false; }
                if (Time.time - inicioSeparacion > 25f || (agente.enabled && !agente.pathPending && agente.remainingDistance < 0.8f && Vector3.Distance(j.transform.position, transform.position) > 12f))
                {
                    gameObject.SetActive(false);
                }
                break;
        }

        if (tiroEn > 0f && Time.time >= tiroEn)
        {
            tiroEn = -1f;
            if (objetivoTiro != null && !objetivoTiro.Muerto)
            {
                Vector3 d = objetivoTiro.transform.position - transform.position; d.y = 0f;
                objetivoTiro.RecibirImpacto(8, d.normalized, 1.4f, FuenteDano.Otro, false, transform.position);
                AudioCap1.Play3D("sfx_piedra", objetivoTiro.transform.position + Vector3.up * 1.5f, 0.9f);
            }
            objetivoTiro = null;
        }
    }

    private void IrA(Vector3 destino, float velocidad)
    {
        if (!agente.enabled || !agente.isOnNavMesh) { return; }
        agente.isStopped = false;
        agente.speed = velocidad;
        if (Time.time >= siguienteRepath)
        {
            siguienteRepath = Time.time + 0.3f;
            agente.SetDestination(destino);
        }
    }

    private void Seguir(Jugador j)
    {
        if (!agente.enabled || !agente.isOnNavMesh)
        {
            TeletransportarCerca();
            return;
        }
        Vector3 pj = j.transform.position;
        float dist = Vector3.Distance(pj, transform.position);
        if (dist > 28f)
        {
            TeletransportarCerca();
            return;
        }
        Vector3 meta = pj - j.transform.forward * 1.7f + j.transform.right * 1.1f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(meta, out hit, 2f, NavMesh.AllAreas)) { meta = hit.position; }
        else { meta = pj; }

        if (dist > 2.6f)
        {
            agente.isStopped = false;
            agente.speed = dist > 7f ? 5.8f : (j.Agachado ? 1.7f : (j.Corriendo ? 5.8f : 3.1f));
            if (Time.time >= siguienteRepath)
            {
                siguienteRepath = Time.time + 0.25f;
                agente.SetDestination(meta);
            }
        }
        else if (agente.remainingDistance < 0.7f || dist < 1.6f)
        {
            agente.isStopped = true;
            MirarA(pj + j.transform.forward * 5f, 3f);
        }

        if (anim != null)
        {
            anim.velocidad = agente.velocity.magnitude;
            anim.agachado = j.Agachado;
            anim.alerta = Infectado.AlgunoPersiguiendo;
        }
    }

    private void Ayudar(Jugador j)
    {
        if (Time.time < siguienteAyuda || tiroEn > 0f)
        {
            return;
        }
        Infectado mejor = null;
        float mejorD = 14f;
        foreach (Infectado inf in Infectado.Todos)
        {
            if (inf == null || inf.Muerto || inf.tipo == Infectado.Tipo.Carnicero) { continue; }
            bool amenaza = (inf.Agarrando && j.Agarrador == inf) || (inf.Persiguiendo && Vector3.Distance(inf.transform.position, j.transform.position) < 3f);
            if (!amenaza) { continue; }
            float d = Vector3.Distance(inf.transform.position, transform.position);
            if (d > mejorD) { continue; }
            Vector3 ojos = transform.position + Vector3.up * 1.4f;
            Vector3 hacia = inf.transform.position + Vector3.up * 1.3f - ojos;
            if (Physics.Raycast(ojos, hacia.normalized, hacia.magnitude - 0.3f, ~((1 << 8) | (1 << 9) | (1 << 2)), QueryTriggerInteraction.Ignore)) { continue; }
            mejorD = d;
            mejor = inf;
            if (inf.Agarrando) { break; }
        }
        if (mejor == null)
        {
            return;
        }
        float factor = Dificultad.Nivel == NivelDificultad.Facil ? 0.6f : (Dificultad.Nivel == NivelDificultad.Superviviente ? 1.4f : 1f);
        siguienteAyuda = Time.time + intervaloAyuda * factor * Random.Range(0.85f, 1.2f);
        objetivoTiro = mejor;
        tiroEn = Time.time + 0.35f;
        MirarA(mejor.transform.position, 50f);
        if (anim != null) { anim.Golpear(0.5f); }
        AudioCap1.Play3D("sfx_honda", transform.position + Vector3.up, 0.8f, 1.1f);
        if (!Juego.I.HablandoAlguien) { Juego.I.Decir(nombre, Tiros[Random.Range(0, Tiros.Length)]); }
    }

    private void Avisar(Jugador j)
    {
        if (Time.time < siguienteAviso || Juego.I.HablandoAlguien)
        {
            return;
        }
        foreach (Infectado inf in Infectado.Todos)
        {
            if (inf == null || inf.Muerto || !inf.Persiguiendo) { continue; }
            Vector3 d = inf.transform.position - j.transform.position; d.y = 0f;
            if (d.magnitude > 9f || d.magnitude < 1.5f) { continue; }
            if (Vector3.Dot(j.AdelanteCamara, d.normalized) < -0.35f)
            {
                siguienteAviso = Time.time + 12f;
                Juego.I.Decir(nombre, Avisos[Random.Range(0, Avisos.Length)]);
                AudioCap1.Play3D("sfx_silbato", transform.position + Vector3.up * 1.5f, 0.6f);
                return;
            }
        }
    }

    private void MirarA(Vector3 p, float vel)
    {
        Vector3 d = p - transform.position; d.y = 0f;
        if (d.sqrMagnitude < 0.01f) { return; }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 1f - Mathf.Exp(-vel * Time.deltaTime));
    }

    // ---------- Ordenes desde eventos ----------

    public void Orden(int o)
    {
        switch (o)
        {
            case 1:
            case 4:
                estado = EstadoC.Siguiendo;
                if (!gameObject.activeSelf) { gameObject.SetActive(true); }
                if (agente.enabled && agente.isOnNavMesh) { agente.isStopped = false; }
                break;
            case 2:
                estado = EstadoC.Separada;
                inicioSeparacion = Time.time;
                if (Juego.I != null) { Juego.I.Flags.Add("wara_se_fue"); }
                break;
            case 3:
                estado = EstadoC.Escondiendose;
                siguienteConsejo = Time.time + 5f;
                break;
        }
    }

    public void TeletransportarCerca()
    {
        Jugador j = Jugador.I;
        if (j == null || estado != EstadoC.Siguiendo) { return; }
        Vector3 p = j.transform.position - j.transform.forward * 2f + j.transform.right * 0.8f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(p, out hit, 3f, NavMesh.AllAreas) || NavMesh.SamplePosition(j.transform.position, out hit, 3f, NavMesh.AllAreas))
        {
            if (agente.enabled) { agente.Warp(hit.position); }
            else { transform.position = hit.position; }
        }
    }

    public void RestaurarEstado(int s)
    {
        estado = (EstadoC)Mathf.Clamp(s, 0, 3);
        if (estado == EstadoC.Escondiendose) { estado = EstadoC.Siguiendo; }
        if (estado == EstadoC.Separada) { gameObject.SetActive(false); return; }
        if (estado == EstadoC.Siguiendo)
        {
            Conversacion c = GetComponent<Conversacion>();
            TeletransportarCerca();
        }
    }
}
