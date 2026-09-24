using UnityEngine;

/// <summary>
/// Canal global de ruidos. Los pasos, golpes, botellas y gritos emiten ruido con un radio;
/// los infectados escuchan este canal y van a investigar (o atacan si es muy cerca).
/// </summary>
public static class SistemaRuido
{
    /// <summary>posicion, radio, esDelJugador</summary>
    public static event System.Action<Vector3, float, bool> AlEmitir;

    public static Vector3 UltimaPosicion { get; private set; }
    public static float UltimoRadio { get; private set; }
    public static float UltimoTiempo { get; private set; } = -10f;

    public static void Emitir(Vector3 posicion, float radio, bool delJugador = true)
    {
        if (radio <= 0.05f)
        {
            return;
        }

        if (delJugador)
        {
            UltimaPosicion = posicion;
            UltimoRadio = radio;
            UltimoTiempo = Time.time;
        }

        if (AlEmitir != null)
        {
            AlEmitir(posicion, radio, delJugador);
        }
    }
}
