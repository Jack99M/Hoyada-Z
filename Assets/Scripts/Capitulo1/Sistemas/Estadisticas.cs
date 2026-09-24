using System.Collections.Generic;
using UnityEngine;

public enum FuenteDano { Melee, Sigilo, Honda, Revolver, Fuego, Botella, Empujon, Otro }

/// <summary>
/// Metricas de la partida (se guardan con la partida y alimentan el rango final y los logros).
/// </summary>
[System.Serializable]
public class DatosStats
{
    public float tiempo;
    public int muertes;
    public int killsComun, killsCorredor, killsFungico, killsGriton, killsJefe;
    public int killsMelee, killsSigilo, killsHonda, killsRevolver, killsFuego;
    public int disparosHonda, aciertosHonda, disparosRevolver, aciertosRevolver, tirosCabeza;
    public int danoRecibido, danoCausado;
    public int vendasUsadas, crafteados, molotovsLanzados, botellasLanzadas;
    public int agarresEscapados, agarresSufridos, esquivas;
    public int coleccionables, archivos;
    public float distancia;
    public float tiempoJefe;
    public int vecesVisto;
    public int moralFinal;
    public bool jefeDerrotado;
    public bool dijoVerdad;
    public int guardados;

    public int TotalKills { get { return killsComun + killsCorredor + killsFungico + killsGriton + killsJefe; } }

    public float Precision
    {
        get
        {
            int d = disparosHonda + disparosRevolver;
            return d > 0 ? (aciertosHonda + aciertosRevolver) / (float)d : 0f;
        }
    }
}

public static class Stats
{
    public static DatosStats D = new DatosStats();

    public static void Reiniciar()
    {
        D = new DatosStats();
    }

    public static void Kill(Infectado.Tipo tipo, FuenteDano fuente)
    {
        switch (tipo)
        {
            case Infectado.Tipo.Corredor: D.killsCorredor++; break;
            case Infectado.Tipo.Fungico: D.killsFungico++; break;
            case Infectado.Tipo.Griton: D.killsGriton++; break;
            case Infectado.Tipo.Carnicero: D.killsJefe++; break;
            default: D.killsComun++; break;
        }
        switch (fuente)
        {
            case FuenteDano.Sigilo: D.killsSigilo++; break;
            case FuenteDano.Honda: D.killsHonda++; break;
            case FuenteDano.Revolver: D.killsRevolver++; break;
            case FuenteDano.Fuego: D.killsFuego++; break;
            default: D.killsMelee++; break;
        }
        Logros.Revisar();
    }

    /// <summary>Puntaje 0..100 para el rango final.</summary>
    public static int Puntaje(int documentosTotales, int coleccionablesTotales)
    {
        float p = 50f;
        float minutos = D.tiempo / 60f;
        p += Mathf.Clamp((40f - minutos) * 0.9f, -20f, 18f);          // rapidez
        p -= D.muertes * 6f;                                             // morir
        p += Mathf.Min(10f, D.killsSigilo * 1.2f);                       // sigilo
        p += (D.disparosHonda + D.disparosRevolver) > 4 ? (D.Precision - 0.5f) * 16f : 0f; // punteria
        if (documentosTotales > 0) { p += 6f * D.archivos / documentosTotales; }
        if (coleccionablesTotales > 0) { p += 6f * D.coleccionables / coleccionablesTotales; }
        p += (D.moralFinal - 50) * 0.12f;                                // conciencia
        p -= Mathf.Min(10f, D.danoRecibido / 120f);                      // salud
        if (Dificultad.Nivel == NivelDificultad.Superviviente) { p += 8f; }
        if (Dificultad.Nivel == NivelDificultad.Facil) { p -= 8f; }
        return Mathf.Clamp(Mathf.RoundToInt(p), 0, 100);
    }

    public static string Rango(int puntaje)
    {
        if (puntaje >= 88) { return "S"; }
        if (puntaje >= 74) { return "A"; }
        if (puntaje >= 58) { return "B"; }
        if (puntaje >= 40) { return "C"; }
        return "D";
    }

    /// <summary>Titulo segun el estilo de juego.</summary>
    public static string Titulo()
    {
        if (D.killsSigilo >= 8 && D.killsSigilo >= D.killsMelee) { return "Sombra de Sopocachi"; }
        if (D.killsHonda >= 6) { return "Honderito del Choqueyapu"; }
        if (D.killsFuego >= 5) { return "El Incendiario"; }
        if (D.killsRevolver >= 6) { return "Pistolero de la 20 de Octubre"; }
        if (D.TotalKills <= 6) { return "Fantasma: nadie te vio pasar"; }
        if (D.killsMelee >= 20) { return "Puño de hierro paceño"; }
        if (D.moralFinal >= 75) { return "Corazón de ayni"; }
        if (D.moralFinal <= 25) { return "Sálvese quien pueda"; }
        return "Sobreviviente de la hoyada";
    }

    public static string TiempoTexto(float s)
    {
        int seg = Mathf.FloorToInt(s);
        return (seg / 3600 > 0 ? (seg / 3600) + ":" : "") + ((seg / 60) % 60).ToString(seg >= 3600 ? "00" : "0") + ":" + (seg % 60).ToString("00");
    }
}

/// <summary>
/// Logros del juego: se desbloquean una vez (PlayerPrefs) y muestran un aviso en pantalla.
/// </summary>
public static class Logros
{
    public class Def
    {
        public string id, titulo, descripcion;
        public Def(string i, string t, string d) { id = i; titulo = t; descripcion = d; }
    }

    public static readonly Def[] Lista =
    {
        new Def("salida", "Resaca", "Sal de la discoteca Altura."),
        new Def("sigilo8", "Sombra de Sopocachi", "Elimina a 8 infectados con ataques sigilosos."),
        new Def("honda5", "Honderito", "Elimina a 5 infectados con la honda."),
        new Def("cabeza3", "Puntería de feria", "Acierta 3 tiros a la cabeza."),
        new Def("craft5", "Maestro chapuzas", "Fabrica 5 objetos."),
        new Def("fuego5", "Fuego purificador", "Quema a 5 infectados con molotovs."),
        new Def("agarre5", "Zafarrancho", "Escápate de 5 agarres."),
        new Def("griton", "Silencio, por favor", "Elimina a un gritón antes de que grite."),
        new Def("jefe", "Carne de carnicero", "Derrota a El Carnicero del Mercado Sopocachi."),
        new Def("jefe_rapido", "Plato del día", "Derrota a El Carnicero en menos de 90 segundos."),
        new Def("ayni", "Ayni", "Ayuda a don Freddy y cumple con el lustrabotas."),
        new Def("ladron", "Sálvese quien pueda", "Róbale a quien te pidió ayuda."),
        new Def("illas", "Coleccionista de Alasitas", "Encuentra todas las illas de Alasitas."),
        new Def("archivos", "Cronista paceño", "Lee todos los archivos del capítulo."),
        new Def("verdad", "La verdad duele", "Dile la verdad a doña Bety."),
        new Def("sin_morir", "Intocable", "Termina el capítulo sin morir."),
        new Def("superviviente", "Hijo de la hoyada", "Termina el capítulo en dificultad Superviviente."),
        new Def("rango_s", "Rango S", "Termina el capítulo con rango S."),
        new Def("pacifista", "Fantasma", "Termina el capítulo eliminando 6 infectados o menos."),
        new Def("oleada10", "La noche más larga", "Sobrevive 10 oleadas en el modo Supervivencia.")
    };

    public struct Aviso { public string titulo; public string descripcion; public float hasta; }
    public static readonly List<Aviso> Avisos = new List<Aviso>();

    public static bool Tiene(string id)
    {
        return PlayerPrefs.GetInt("hz_logro_" + id, 0) == 1;
    }

    public static int Cantidad
    {
        get
        {
            int n = 0;
            foreach (Def d in Lista) { if (Tiene(d.id)) { n++; } }
            return n;
        }
    }

    public static void Desbloquear(string id)
    {
        if (string.IsNullOrEmpty(id) || Tiene(id)) { return; }
        Def def = null;
        foreach (Def d in Lista) { if (d.id == id) { def = d; break; } }
        if (def == null) { return; }
        PlayerPrefs.SetInt("hz_logro_" + id, 1);
        PlayerPrefs.Save();
        Avisos.Add(new Aviso { titulo = def.titulo, descripcion = def.descripcion, hasta = Time.unscaledTime + 5.5f });
        AudioCap1.Play2D("sfx_logro", 0.8f);
    }

    /// <summary>Revisa los logros que dependen de contadores.</summary>
    public static void Revisar()
    {
        DatosStats d = Stats.D;
        if (d.killsSigilo >= 8) { Desbloquear("sigilo8"); }
        if (d.killsHonda >= 5) { Desbloquear("honda5"); }
        if (d.tirosCabeza >= 3) { Desbloquear("cabeza3"); }
        if (d.crafteados >= 5) { Desbloquear("craft5"); }
        if (d.killsFuego >= 5) { Desbloquear("fuego5"); }
        if (d.agarresEscapados >= 5) { Desbloquear("agarre5"); }
    }

    public static void Actualizar()
    {
        for (int i = Avisos.Count - 1; i >= 0; i--)
        {
            if (Time.unscaledTime > Avisos[i].hasta) { Avisos.RemoveAt(i); }
        }
    }
}

/// <summary>Records locales (mejor rango/tiempo por dificultad y puntajes de Supervivencia).</summary>
public static class Records
{
    public static void GuardarHistoria(string rango, int puntaje, float tiempo)
    {
        string k = "hz_rec_" + (int)Dificultad.Nivel;
        if (puntaje > PlayerPrefs.GetInt(k + "_pts", -1))
        {
            PlayerPrefs.SetInt(k + "_pts", puntaje);
            PlayerPrefs.SetString(k + "_rango", rango);
        }
        float mejor = PlayerPrefs.GetFloat(k + "_tiempo", 0f);
        if (mejor <= 0f || tiempo < mejor) { PlayerPrefs.SetFloat(k + "_tiempo", tiempo); }
        PlayerPrefs.SetInt("hz_completado", 1);
        PlayerPrefs.Save();
    }

    public static string TextoHistoria(NivelDificultad n)
    {
        string k = "hz_rec_" + (int)n;
        int pts = PlayerPrefs.GetInt(k + "_pts", -1);
        if (pts < 0) { return "—"; }
        return "Rango " + PlayerPrefs.GetString(k + "_rango", "?") + "   ·   " + pts + " pts   ·   mejor tiempo " + Stats.TiempoTexto(PlayerPrefs.GetFloat(k + "_tiempo", 0f));
    }

    public static bool GuardarSupervivencia(int puntaje, int oleada)
    {
        var lista = TablaSupervivencia();
        lista.Add(new KeyValuePair<int, int>(puntaje, oleada));
        lista.Sort((a, b) => b.Key.CompareTo(a.Key));
        bool top = lista.IndexOf(new KeyValuePair<int, int>(puntaje, oleada)) == 0;
        for (int i = 0; i < 5; i++)
        {
            if (i < lista.Count)
            {
                PlayerPrefs.SetInt("hz_sup_pts_" + i, lista[i].Key);
                PlayerPrefs.SetInt("hz_sup_ola_" + i, lista[i].Value);
            }
        }
        PlayerPrefs.Save();
        return top;
    }

    public static List<KeyValuePair<int, int>> TablaSupervivencia()
    {
        var l = new List<KeyValuePair<int, int>>();
        for (int i = 0; i < 5; i++)
        {
            int p = PlayerPrefs.GetInt("hz_sup_pts_" + i, -1);
            if (p >= 0) { l.Add(new KeyValuePair<int, int>(p, PlayerPrefs.GetInt("hz_sup_ola_" + i, 0))); }
        }
        return l;
    }
}
