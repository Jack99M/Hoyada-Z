using System;
using UnityEngine;

/// <summary>
/// Una opcion de decision del jugador y su impacto en el estado del grupo.
/// Los valores son deltas (positivos o negativos) que se suman al estado actual.
/// Base del arbol de decisiones del Episodio I de Hoyada Z.
/// </summary>
[Serializable]
public class DecisionEpisodio
{
    public string titulo = "Nueva decision";

    [TextArea(2, 4)]
    public string descripcion;

    [Header("Impacto en el grupo (deltas)")]
    public int moral;
    public int salud;
    public int comida;
    public int agua;
    public int medicina;
    public int abrigo;
    public int municion;
}
