using System.Collections;
using UnityEngine;

/// <summary>
/// Accion de un solo uso con [E]: trepar un muro con ayuda del compañero, forzar una caja,
/// bajar una palanca, abrir un cerrojo... Puede pedir un objeto clave, que el compañero este
/// cerca, tardar unos segundos (barra de progreso en el HUD), teletransportar al jugador (y al
/// compañero) al otro lado y mandar al compañero a trabajar en un punto.
/// </summary>
public class Interruptor : Interactivo
{
    /// <summary>La accion que se esta haciendo ahora (para la barra de progreso del HUD).</summary>
    public static Interruptor EnProgreso { get; private set; }

    [Tooltip("Texto de la accion junto a [E].")]
    public string accion = "Usar";
    public string requiereObjeto;
    public bool consumirObjeto;
    public bool requiereCompanero;
    [TextArea] public string mensajeSinObjeto = "Me falta algo para esto.";
    [TextArea] public string mensajeSinCompanero = "Sola no puedo. Necesito a Tito aquí.";
    [Tooltip("Segundos que tarda (el jugador no se mueve mientras tanto).")]
    public float duracion;
    public string sonido = "sfx_click";

    [Header("Teletransporte (trepar)")]
    public bool teletransportar;
    public Vector3 destino;
    public float destinoRotY;
    public Vector3 destinoCompanero;

    [Header("Compañero")]
    public bool companeroTrabaja;
    public Vector3 puntoTrabajo;

    [Header("Visuales")]
    public GameObject visualAntes;
    public GameObject visualDespues;

    public Acciones alUsar = new Acciones();

    public bool Usado { get; private set; }
    public bool EnCurso { get; private set; }
    public float Progreso { get; private set; }

    public override bool Disponible { get { return base.Disponible && !Usado && !EnCurso; } }
    public override string Prompt { get { return accion; } }

    public override void Usar(Jugador j)
    {
        if (Usado || EnCurso || Juego.I == null || j == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(requiereObjeto) && (j.Inventario == null || !j.Inventario.Tiene(requiereObjeto)))
        {
            Juego.I.Decir(Juego.Protagonista, mensajeSinObjeto);
            AudioCap1.Play2D("sfx_vacio", 0.5f);
            return;
        }
        if (requiereCompanero)
        {
            Companera c = Companera.I;
            if (c == null || !c.isActiveAndEnabled || !c.Siguiendo)
            {
                Juego.I.Decir(Juego.Protagonista, mensajeSinCompanero);
                AudioCap1.Play2D("sfx_vacio", 0.5f);
                return;
            }
            if (Vector3.Distance(c.transform.position, transform.position) > 6f) { c.Colocar(transform.position - transform.forward * 1.2f + transform.right * 0.8f); }
        }
        StartCoroutine(Ejecutar(j));
    }

    private IEnumerator Ejecutar(Jugador j)
    {
        EnCurso = true;
        EnProgreso = this;
        Progreso = 0f;
        if (!string.IsNullOrEmpty(sonido)) { AudioCap1.Play3D(sonido, transform.position + Vector3.up, 0.9f); }
        if (duracion > 0f)
        {
            for (float t = 0f; t < duracion; t += Time.deltaTime)
            {
                if (j.Muerto)
                {
                    EnCurso = false;
                    Progreso = 0f;
                    if (EnProgreso == this) { EnProgreso = null; }
                    yield break;
                }
                j.Bloquear(0.15f);
                Progreso = t / duracion;
                yield return null;
            }
        }
        Progreso = 1f;
        if (teletransportar)
        {
            j.Teletransportar(destino, Quaternion.Euler(0f, destinoRotY, 0f));
            Companera c = Companera.I;
            if (c != null && c.isActiveAndEnabled && c.Siguiendo)
            {
                c.Colocar(destinoCompanero != Vector3.zero ? destinoCompanero : destino);
            }
            if (Juego.I.camara != null) { Juego.I.camara.ColocarDetras(); }
        }
        if (companeroTrabaja && Companera.I != null) { Companera.I.Trabajar(puntoTrabajo); }
        if (consumirObjeto && !string.IsNullOrEmpty(requiereObjeto) && j.Inventario != null) { j.Inventario.Quitar(requiereObjeto); }
        Usado = true;
        EnCurso = false;
        if (EnProgreso == this) { EnProgreso = null; }
        AplicarVisual();
        if (alUsar != null) { alUsar.Ejecutar(transform.position); }
    }

    private void AplicarVisual()
    {
        if (visualAntes != null) { visualAntes.SetActive(!Usado); }
        if (visualDespues != null) { visualDespues.SetActive(Usado); }
    }

    /// <summary>Al cargar una partida en la que ya se uso.</summary>
    public void MarcarUsado()
    {
        Usado = true;
        AplicarVisual();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (EnProgreso == this) { EnProgreso = null; }
    }
}
