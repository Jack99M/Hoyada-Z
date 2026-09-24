using UnityEngine;

public enum NivelDificultad { Facil, Normal, Superviviente }

/// <summary>
/// Tres niveles de dificultad. Cambian el dano que recibe Mateo, que tan rapido lo detectan,
/// los agarres, la ayuda al apuntar y cuantos recursos hay en el mapa.
/// </summary>
public static class Dificultad
{
    private const string Clave = "hz_dificultad";
    private static bool cargada;
    private static NivelDificultad nivel = NivelDificultad.Normal;

    public static NivelDificultad Nivel
    {
        get
        {
            if (!cargada) { cargada = true; nivel = (NivelDificultad)Mathf.Clamp(PlayerPrefs.GetInt(Clave, 1), 0, 2); }
            return nivel;
        }
        set
        {
            nivel = value;
            cargada = true;
            PlayerPrefs.SetInt(Clave, (int)value);
        }
    }

    public static string Nombre(NivelDificultad n)
    {
        switch (n)
        {
            case NivelDificultad.Facil: return "Fácil";
            case NivelDificultad.Superviviente: return "Superviviente";
            default: return "Normal";
        }
    }

    public static string Descripcion(NivelDificultad n)
    {
        switch (n)
        {
            case NivelDificultad.Facil: return "Para disfrutar la historia. Los infectados pegan menos, hay más recursos y el apuntado ayuda.";
            case NivelDificultad.Superviviente: return "Como debe ser. Pegan fuerte, te ven rápido, hay pocos recursos y un fúngico te mata si te agarra.";
            default: return "La experiencia pensada. Cada bala y cada venda cuentan.";
        }
    }

    public static string NombreActual { get { return Nombre(Nivel); } }

    /// <summary>Multiplicador del dano que recibe Mateo.</summary>
    public static float DanoRecibido { get { return Nivel == NivelDificultad.Facil ? 0.6f : (Nivel == NivelDificultad.Superviviente ? 1.45f : 1f); } }

    /// <summary>Multiplicador de la velocidad a la que un infectado se da cuenta de Mateo.</summary>
    public static float Deteccion { get { return Nivel == NivelDificultad.Facil ? 0.7f : (Nivel == NivelDificultad.Superviviente ? 1.3f : 1f); } }

    /// <summary>Probabilidad de que un ataque de infectado comun se convierta en agarre.</summary>
    public static float ProbAgarre { get { return Nivel == NivelDificultad.Facil ? 0.15f : (Nivel == NivelDificultad.Superviviente ? 0.45f : 0.3f); } }

    /// <summary>Si un fungico que agarra a Mateo lo mata (salvo que tenga una punta).</summary>
    public static bool FungicoMata { get { return Nivel != NivelDificultad.Facil; } }

    /// <summary>0 = sin ayuda al apuntar, 1 = mucha ayuda.</summary>
    public static float AyudaApuntado { get { return Nivel == NivelDificultad.Facil ? 1f : (Nivel == NivelDificultad.Superviviente ? 0f : 0.45f); } }

    /// <summary>Fraccion de consumibles del mapa que se retiran al empezar.</summary>
    public static float RecursosRetirados { get { return Nivel == NivelDificultad.Superviviente ? 0.35f : 0f; } }

    /// <summary>Golpes por segundo necesarios para zafarse de un agarre.</summary>
    public static float FuerzaZafarse { get { return Nivel == NivelDificultad.Facil ? 0.2f : (Nivel == NivelDificultad.Superviviente ? 0.11f : 0.14f); } }

    /// <summary>Alcance del modo escucha en metros.</summary>
    public static float AlcanceEscucha { get { return Nivel == NivelDificultad.Superviviente ? 16f : 26f; } }
}

/// <summary>
/// Opciones del jugador guardadas en PlayerPrefs: sensibilidad, invertir Y, volumenes, subtitulos,
/// brillo, campo de vision, calidad grafica, pantalla completa y contador de FPS.
/// </summary>
public static class Opciones
{
    public static float Sensibilidad = 0.12f;
    public static bool InvertirY;
    public static float VolumenGeneral = 1f;
    public static float VolumenMusica = 0.45f;
    public static bool Subtitulos = true;
    public static bool AyudasEnPantalla = true;
    [Tooltip("Exposicion extra (EV) sobre la del post-proceso. La noche en La Paz es oscura.")]
    public static float Brillo = 0f;
    public static float CampoVision = 58f;
    public static bool PantallaCompleta = true;
    public static bool MostrarFPS;
    public static bool SubtitulosGrandes;
    private static bool cargadas;
    private static UnityEngine.Rendering.Volume volumen;
    private static UnityEngine.Rendering.Universal.ColorAdjustments ajustes;
    private static float exposicionBase;

    public static void Cargar()
    {
        if (cargadas) { return; }
        cargadas = true;
        Sensibilidad = PlayerPrefs.GetFloat("hz_sens", 0.12f);
        InvertirY = PlayerPrefs.GetInt("hz_invy", 0) == 1;
        VolumenGeneral = PlayerPrefs.GetFloat("hz_vol", 1f);
        VolumenMusica = PlayerPrefs.GetFloat("hz_volmus", 0.45f);
        Subtitulos = PlayerPrefs.GetInt("hz_subs", 1) == 1;
        AyudasEnPantalla = PlayerPrefs.GetInt("hz_ayudas", 1) == 1;
        Brillo = PlayerPrefs.GetFloat("hz_brillo", 0f);
        CampoVision = PlayerPrefs.GetFloat("hz_fov", 58f);
        PantallaCompleta = PlayerPrefs.GetInt("hz_completa", Screen.fullScreen ? 1 : 0) == 1;
        MostrarFPS = PlayerPrefs.GetInt("hz_fps", 0) == 1;
        SubtitulosGrandes = PlayerPrefs.GetInt("hz_subsg", 0) == 1;
        int calidad = PlayerPrefs.GetInt("hz_calidad", -1);
        if (calidad >= 0 && calidad < QualitySettings.names.Length && calidad != QualitySettings.GetQualityLevel()) { QualitySettings.SetQualityLevel(calidad, true); }
        Aplicar();
    }

    public static void Guardar()
    {
        PlayerPrefs.SetFloat("hz_sens", Sensibilidad);
        PlayerPrefs.SetInt("hz_invy", InvertirY ? 1 : 0);
        PlayerPrefs.SetFloat("hz_vol", VolumenGeneral);
        PlayerPrefs.SetFloat("hz_volmus", VolumenMusica);
        PlayerPrefs.SetInt("hz_subs", Subtitulos ? 1 : 0);
        PlayerPrefs.SetInt("hz_ayudas", AyudasEnPantalla ? 1 : 0);
        PlayerPrefs.SetFloat("hz_brillo", Brillo);
        PlayerPrefs.SetFloat("hz_fov", CampoVision);
        PlayerPrefs.SetInt("hz_completa", PantallaCompleta ? 1 : 0);
        PlayerPrefs.SetInt("hz_fps", MostrarFPS ? 1 : 0);
        PlayerPrefs.SetInt("hz_subsg", SubtitulosGrandes ? 1 : 0);
        PlayerPrefs.SetInt("hz_calidad", QualitySettings.GetQualityLevel());
        PlayerPrefs.Save();
        Aplicar();
    }

    public static void Aplicar()
    {
        AudioListener.volume = VolumenGeneral;
        if (AudioCap1.I != null) { AudioCap1.I.volumenMusica = VolumenMusica; }
        CamaraTPS c = Object.FindFirstObjectByType<CamaraTPS>();
        if (c != null)
        {
            c.sensibilidadMouse = Sensibilidad;
            c.invertirY = InvertirY;
            c.fovNormal = CampoVision;
            c.fovCorriendo = CampoVision + 8f;
        }
        AplicarBrillo();
        if (!Application.isEditor && Screen.fullScreen != PantallaCompleta) { Screen.fullScreen = PantallaCompleta; }
    }

    /// <summary>Sube o baja la exposicion del post-proceso (sin tocar el perfil guardado).</summary>
    private static void AplicarBrillo()
    {
        if (!Application.isPlaying) { return; }
        if (volumen == null || ajustes == null)
        {
            volumen = Object.FindFirstObjectByType<UnityEngine.Rendering.Volume>();
            ajustes = null;
            if (volumen == null) { return; }
            UnityEngine.Rendering.Universal.ColorAdjustments ca;
            if (volumen.profile.TryGet(out ca))
            {
                ajustes = ca;
                exposicionBase = ca.postExposure.value;
            }
        }
        if (ajustes != null)
        {
            ajustes.postExposure.overrideState = true;
            ajustes.postExposure.value = exposicionBase + Brillo;
        }
    }

    public static string NombreCalidad()
    {
        string n = QualitySettings.names[QualitySettings.GetQualityLevel()];
        if (n == "Mobile" || n == "Performant") { return "Baja (más FPS)"; }
        if (n == "Balanced") { return "Media"; }
        if (n == "PC" || n == "High Fidelity") { return "Alta"; }
        return n;
    }

    public static void SiguienteCalidad()
    {
        int n = (QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length;
        QualitySettings.SetQualityLevel(n, true);
    }
}
