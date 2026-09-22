using UnityEngine;

/// <summary>
/// Base de todo objeto con el que Mateo puede interactuar (puertas, items, pistas).
/// Se registra solo en el InteraccionManager mientras esta activo.
/// </summary>
public abstract class Interactuable : MonoBehaviour
{
    [Tooltip("Texto que se muestra en el prompt, ej: 'Abrir puerta'.")]
    public string prompt = "Interactuar";

    [Tooltip("Distancia a la que se puede interactuar.")]
    [Min(0.5f)] public float radio = 3f;

    public bool Disponible { get; protected set; } = true;

    protected virtual void OnEnable()
    {
        InteraccionManager.Registrar(this);
    }

    protected virtual void OnDisable()
    {
        InteraccionManager.Quitar(this);
    }

    public abstract void Interactuar();
}
