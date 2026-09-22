using System;
using UnityEngine;

/// <summary>
/// Una respuesta que el jugador puede dar en un dialogo, con la replica del personaje
/// y su impacto en la moral del grupo y en la relacion de confianza.
/// </summary>
[Serializable]
public class RespuestaDialogo
{
    [Tooltip("Lo que dice el jugador (texto del boton).")]
    public string texto;

    [Tooltip("Lo que responde el personaje tras esta eleccion.")]
    [TextArea(1, 3)] public string replica;

    [Tooltip("Cambio en la moral del grupo.")]
    public int moral;

    [Tooltip("Cambio en la confianza del personaje hacia Mateo.")]
    public int confianza;
}
