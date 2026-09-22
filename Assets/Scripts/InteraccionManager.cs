using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta el interactuable mas cercano a Mateo y ejecuta su accion con la tecla E.
/// El HUD lee 'Actual' para mostrar el prompt.
/// </summary>
public class InteraccionManager : MonoBehaviour
{
    public static InteraccionManager Instancia { get; private set; }

    private static readonly List<Interactuable> registro = new List<Interactuable>();

    public Interactuable Actual { get; private set; }

    private Transform jugador;

    public static void Registrar(Interactuable i)
    {
        if (i != null && !registro.Contains(i))
        {
            registro.Add(i);
        }
    }

    public static void Quitar(Interactuable i)
    {
        registro.Remove(i);
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    private void Start()
    {
        MateoController mc = Object.FindFirstObjectByType<MateoController>();
        if (mc != null)
        {
            jugador = mc.transform;
        }
    }

    private void Update()
    {
        Actual = null;

        if (jugador == null || !FlujoJuego.EnJuego)
        {
            return;
        }

        float mejor = float.MaxValue;
        for (int k = 0; k < registro.Count; k++)
        {
            Interactuable i = registro[k];
            if (i == null || !i.Disponible)
            {
                continue;
            }

            float d = Vector3.Distance(jugador.position, i.transform.position);
            if (d <= i.radio && d < mejor)
            {
                mejor = d;
                Actual = i;
            }
        }

        if (Actual != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Actual.Interactuar();
        }
    }
}
