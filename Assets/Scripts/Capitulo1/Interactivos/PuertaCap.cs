using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Puerta del capitulo: de bisagra, cortina metalica (sube) o reja (se desliza).
/// Puede pedir un objeto clave, estar trabada, abrirse por un evento o tener un candado
/// que se fuerza con una punta (llaveRequerida = "*punta"), como en The Last of Us.
/// </summary>
public class PuertaCap : Interactivo
{
    public const string CandadoPunta = "*punta";

    public enum Tipo { Bisagra, Cortina, Reja }

    public Tipo tipo = Tipo.Bisagra;
    [Tooltip("Parte que se mueve (para bisagra, el pivote en el borde de la puerta).")]
    public Transform hoja;
    public float angulo = 100f;
    public float desplazamiento = 2.6f;
    public float duracion = 0.9f;

    [Header("Cerradura")]
    public string llaveRequerida;
    public string nombreLlave;
    public bool consumirLlave;
    [TextArea] public string mensajeCerrada = "Esta cerrada.";
    public bool trabada;
    [TextArea] public string mensajeTrabada = "No se abre desde este lado.";

    [Header("Efectos")]
    public float ruidoAlAbrir = 4f;
    public string sonido = "sfx_puerta";
    public Acciones alAbrir = new Acciones();

    public bool abierta;

    private Quaternion rotCerrada;
    private Vector3 posCerrada;
    private NavMeshObstacle obstaculo;
    private bool iniciado;

    private void Awake()
    {
        Iniciar();
    }

    private void Iniciar()
    {
        if (iniciado) { return; }
        iniciado = true;
        if (hoja == null) { hoja = transform; }
        rotCerrada = hoja.localRotation;
        posCerrada = hoja.localPosition;
        obstaculo = hoja.GetComponentInChildren<NavMeshObstacle>();
        if (abierta) { AplicarPose(1f); if (obstaculo != null) { obstaculo.enabled = false; } }
    }

    public override bool Disponible { get { return base.Disponible && !abierta; } }

    public bool EsCandadoPunta { get { return llaveRequerida == CandadoPunta; } }

    public override string Prompt
    {
        get
        {
            if (trabada) { return "Revisar"; }
            Jugador j = Jugador.I;
            if (EsCandadoPunta)
            {
                return j != null && j.Inventario != null && j.Inventario.puntas > 0 ? "Forzar el candado (usa 1 punta)" : "Revisar candado";
            }
            if (!string.IsNullOrEmpty(llaveRequerida))
            {
                if (j != null && j.Inventario != null && j.Inventario.Tiene(llaveRequerida))
                {
                    return "Usar " + nombreLlave;
                }
                return "Revisar";
            }
            return tipo == Tipo.Cortina ? "Levantar cortina" : "Abrir";
        }
    }

    public override void Usar(Jugador j)
    {
        if (abierta)
        {
            return;
        }
        if (trabada)
        {
            Juego.I.Decir("Mateo", mensajeTrabada);
            AudioCap1.Play3D("sfx_puerta_trabada", transform.position, 0.8f);
            return;
        }
        if (EsCandadoPunta)
        {
            if (j.Inventario.puntas <= 0)
            {
                Juego.I.Decir("Mateo", mensajeCerrada);
                Juego.I.Mensaje("Necesitas una punta para forzar el candado. [Tab] cinta + cuchilla");
                AudioCap1.Play3D("sfx_puerta_trabada", transform.position, 0.8f);
                return;
            }
            j.Inventario.puntas--;
            Juego.I.Mensaje("Forzaste el candado con una punta");
            AudioCap1.Play3D("sfx_cadena", transform.position + Vector3.up, 0.8f, 1.3f);
        }
        else if (!string.IsNullOrEmpty(llaveRequerida))
        {
            if (!j.Inventario.Tiene(llaveRequerida))
            {
                Juego.I.Decir("Mateo", mensajeCerrada);
                AudioCap1.Play3D("sfx_puerta_trabada", transform.position, 0.8f);
                return;
            }
            Juego.I.Mensaje("Usaste: " + nombreLlave);
            if (consumirLlave) { j.Inventario.Quitar(llaveRequerida); }
        }
        Abrir();
    }

    public void Abrir()
    {
        if (abierta)
        {
            return;
        }
        Iniciar();
        abierta = true;
        trabada = false;
        if (obstaculo != null) { obstaculo.enabled = false; }
        AudioCap1.Play3D(sonido, transform.position + Vector3.up, 0.9f);
        if (ruidoAlAbrir > 0f) { SistemaRuido.Emitir(transform.position, ruidoAlAbrir, true); }
        StartCoroutine(Animar());
        alAbrir.Ejecutar(transform.position);
    }

    /// <summary>Cierra la puerta (por ejemplo, al empezar la pelea con el jefe).</summary>
    public void Cerrar()
    {
        Iniciar();
        if (!abierta)
        {
            return;
        }
        abierta = false;
        if (obstaculo != null) { obstaculo.enabled = true; }
        AudioCap1.Play3D(sonido, transform.position + Vector3.up, 0.9f, 0.8f);
        StopAllCoroutines();
        StartCoroutine(AnimarCierre());
    }

    private IEnumerator AnimarCierre()
    {
        for (float t = 1f; t > 0f; t -= Time.deltaTime / duracion)
        {
            AplicarPose(Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        AplicarPose(0f);
    }

    /// <summary>Al cargar una partida: queda abierta sin sonido ni eventos.</summary>
    public void AbrirSinEfectos()
    {
        Iniciar();
        abierta = true;
        if (obstaculo != null) { obstaculo.enabled = false; }
        AplicarPose(1f);
    }

    private IEnumerator Animar()
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / duracion)
        {
            AplicarPose(Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        AplicarPose(1f);
    }

    private void AplicarPose(float t)
    {
        switch (tipo)
        {
            case Tipo.Bisagra:
                hoja.localRotation = rotCerrada * Quaternion.Euler(0f, angulo * t, 0f);
                break;
            case Tipo.Cortina:
                hoja.localPosition = posCerrada + Vector3.up * desplazamiento * t;
                break;
            case Tipo.Reja:
                hoja.localPosition = posCerrada + Vector3.right * desplazamiento * t;
                break;
        }
    }
}
