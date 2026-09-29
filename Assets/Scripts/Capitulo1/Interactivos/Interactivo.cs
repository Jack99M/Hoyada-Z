using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de todo lo que el jugador puede usar con [E]: objetos, puertas, documentos,
/// sobrevivientes. El jugador elige el mas cercano que tenga enfrente.
/// </summary>
public abstract class Interactivo : MonoBehaviour
{
    public static readonly List<Interactivo> Todos = new List<Interactivo>();

    [Tooltip("Texto que aparece junto a [E].")]
    public string prompt = "Usar";

    [Tooltip("Distancia maxima para usarlo.")]
    public float radio = 1.8f;

    [Tooltip("Altura del punto de interaccion sobre el pivote.")]
    public float altura = 0.8f;

    [Tooltip("Mostrar un destello sobre el objeto cuando esta cerca (objetos recogibles).")]
    public bool destello;

    public virtual bool Disponible { get { return isActiveAndEnabled; } }

    public virtual Vector3 Punto { get { return transform.position + Vector3.up * altura; } }

    public virtual string Prompt { get { return prompt; } }

    public abstract void Usar(Jugador jugador);

    protected virtual void OnEnable()
    {
        if (!Todos.Contains(this)) { Todos.Add(this); }
    }

    protected virtual void OnDisable()
    {
        Todos.Remove(this);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.6f);
        Gizmos.DrawWireSphere(Punto, radio);
    }
}
