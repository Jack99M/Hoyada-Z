using UnityEngine;

/// <summary>
/// Altar con velas y hojas de coca (punto de guardado, como las maquinas de escribir de
/// Resident Evil). Encender una vela guarda la partida. No se puede guardar con infectados
/// persiguiendo a Mateo.
/// </summary>
public class PuntoGuardado : Interactivo
{
    public string lugar = "Altar";
    public Light llama;

    private float siguiente;

    private void Reset()
    {
        radio = 2f;
        altura = 0.9f;
        destello = true;
    }

    public override string Prompt { get { return "Encender una vela  (guardar partida)"; } }

    public override bool Disponible { get { return base.Disponible && Juego.I != null && Juego.I.modo == Juego.Modo.Historia; } }

    public override void Usar(Jugador j)
    {
        if (Time.unscaledTime < siguiente) { return; }
        siguiente = Time.unscaledTime + 1.5f;
        if (Juego.I.GuardarPartida(lugar))
        {
            AudioCap1.Play3D("sfx_guardar", transform.position + Vector3.up, 0.9f);
            if (llama != null)
            {
                llama.enabled = true;
                llama.intensity = Mathf.Max(llama.intensity, 4f);
            }
            if (j.Salud < 30) { j.Curar(10); }
        }
    }
}
