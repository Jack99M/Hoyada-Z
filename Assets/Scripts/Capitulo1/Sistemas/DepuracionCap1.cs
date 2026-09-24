using System.Reflection;
using UnityEngine;

/// <summary>
/// Atajos de prueba para el equipo (se llaman desde el editor o consola de depuracion):
/// saltar la intro, teletransportar a Mateo a una zona y fijar la camara.
/// No afecta al juego normal.
/// </summary>
public static class DepuracionCap1
{
    private const BindingFlags Priv = BindingFlags.NonPublic | BindingFlags.Instance;

    public static void SaltarIntro()
    {
        Juego g = Juego.I;
        if (g == null) { return; }
        typeof(Juego).GetMethod("CambiarEstado", Priv).Invoke(g, new object[] { Juego.Estado.Jugando });
        typeof(Juego).GetProperty("Negro").SetValue(g, 0f);
        typeof(Juego).GetField("negroObjetivo", Priv).SetValue(g, 0f);
        if (Jugador.I != null) { Jugador.I.Agachado = false; }
    }

    public static void Cielo(float t)
    {
        Juego g = Juego.I;
        if (g == null) { return; }
        typeof(Juego).GetField("cieloObjetivo", Priv).SetValue(g, t);
        typeof(Juego).GetProperty("ProgresoCielo").SetValue(g, t);
        typeof(Juego).GetMethod("AplicarCielo", Priv).Invoke(g, new object[] { t });
    }

    public static void Ir(Vector3 pos, float yaw, float pitch = 8f)
    {
        if (Jugador.I == null) { return; }
        if (float.IsNaN(pos.x) || float.IsInfinity(pos.x) || float.IsNaN(pos.y) || float.IsInfinity(pos.y) || float.IsNaN(pos.z) || float.IsInfinity(pos.z)) { return; }
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 3f, UnityEngine.AI.NavMesh.AllAreas)) { pos = hit.position; }
        Jugador.I.Teletransportar(pos, Quaternion.Euler(0f, yaw, 0f));
        CamaraTPS cam = Object.FindFirstObjectByType<CamaraTPS>();
        if (cam == null) { return; }
        cam.ColocarDetras();
        typeof(CamaraTPS).GetField("pitch", Priv).SetValue(cam, pitch);
    }

    public static void Dios(bool activo)
    {
        Jugador.ModoDios = activo;
    }

    public static void DarTodo()
    {
        if (Jugador.I == null) { return; }
        Inventario inv = Jugador.I.Inventario;
        inv.tieneLinterna = true;
        inv.vendas = 3; inv.botellas = 2; inv.pilas = 2;
        inv.Equipar(CombateJugador.Arma.Fierro);
        inv.alcohol = 2; inv.trapo = 2; inv.cinta = 2; inv.cuchilla = 2; inv.molotovs = 1; inv.puntas = 1;
        inv.tieneHonda = true; inv.piedras = 12; inv.tieneRevolver = true; inv.balasCargadas = 6; inv.balas = 6;
    }
}
