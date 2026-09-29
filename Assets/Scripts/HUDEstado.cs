using UnityEngine;

/// <summary>
/// HUD del Episodio I (IMGUI): estado del grupo con iconos y retrato de Mateo, panel de
/// decision de acto, panel de dialogo con retratos, reloj del episodio, fundido de cierre
/// y pantalla de GAME OVER por crisis.
/// Solucion rapida para el vertical slice; la UI final iria en uGUI / UI Toolkit.
/// </summary>
public class HUDEstado : MonoBehaviour
{
    // Escala la interfaz IMGUI para que se vea igual en 1080p, 1440p o 4K.
    private static float EscalaUI { get { return Mathf.Max(1f, Screen.height / 1080f); } }
    private static float AnchoUI { get { return Screen.width / EscalaUI; } }
    private static float AltoUI { get { return Screen.height / EscalaUI; } }

    [Header("Iconos de recursos")]
    public Texture2D iconoMoral;
    public Texture2D iconoAgua;
    public Texture2D iconoMedicina;
    public Texture2D iconoBateria;
    public Texture2D iconoTiempo;

    [Header("Retratos de Mateo (neutro / alerta / tension)")]
    public Texture2D retratoMateoNeutro;
    public Texture2D retratoMateoAlerta;
    public Texture2D retratoMateoTension;

    [Header("Retratos de Mama Bety (neutro / alerta / tension)")]
    public Texture2D retratoBetyNeutro;
    public Texture2D retratoBetyAlerta;
    public Texture2D retratoBetyTension;

    [Header("Retrato del infectado")]
    public Texture2D retratoInfectado;

    [Header("Reloj del episodio")]
    [Tooltip("Hora de inicio del episodio en minutos (07:42 = 462).")]
    public float minutoInicio = 462f;

    [Tooltip("Segundos reales que dura un minuto del juego.")]
    [Min(0.1f)] public float segundosPorMinuto = 3f;

    // Recorte de los retratos: quita el borde inferior que trae un artefacto.
    private static readonly Rect RecorteRetrato = new Rect(0.04f, 0.08f, 0.92f, 0.92f);

    private GUIStyle estiloTexto;
    private GUIStyle estiloTitulo;
    private GUIStyle estiloBoton;
    private GUIStyle estiloFin;
    private GUIStyle estiloGameOver;
    private GUIStyle estiloDato;
    private Texture2D negro;
    private float minutoActual;
    private MateoController mateo;

    private void Start()
    {
        negro = new Texture2D(1, 1);
        negro.SetPixel(0, 0, Color.black);
        negro.Apply();

        minutoActual = minutoInicio;
        mateo = Object.FindFirstObjectByType<MateoController>();
    }

    private void Update()
    {
        if (FlujoJuego.EnJuego)
        {
            minutoActual += Time.deltaTime / segundosPorMinuto;
        }
    }

    /// <summary>Hora del episodio en formato HH:MM.</summary>
    public string HoraTexto
    {
        get
        {
            int total = Mathf.FloorToInt(minutoActual);
            return ((total / 60) % 24).ToString("00") + ":" + (total % 60).ToString("00");
        }
    }

    private void InitEstilos()
    {
        if (estiloTexto != null)
        {
            return;
        }

        estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
        estiloTexto.normal.textColor = Color.white;

        estiloDato = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
        estiloDato.normal.textColor = Color.white;

        estiloTitulo = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        estiloTitulo.normal.textColor = new Color(0.9f, 0.92f, 1f);

        estiloBoton = new GUIStyle(GUI.skin.button) { fontSize = 15, wordWrap = true };

        estiloFin = new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        estiloFin.normal.textColor = Color.white;

        estiloGameOver = new GUIStyle(GUI.skin.label)
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        estiloGameOver.normal.textColor = new Color(0.85f, 0.2f, 0.2f);
    }

    private void OnGUI()
    {
        InitEstilos();
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(EscalaUI, EscalaUI, 1f));

        DibujarEstado();
        DibujarObjetivo();
        DibujarDecision();
        DibujarDialogo();
        DibujarConfirm();
        DibujarPrompt();
        DibujarFin();
        DibujarGameOver();
    }

    // ---------- Utilidades de dibujo ----------

    private static void DibujarRetrato(Rect r, Texture2D tex)
    {
        if (tex == null)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.DrawTextureWithTexCoords(r, tex, RecorteRetrato);
    }

    private void Dato(float x, float y, Texture2D icono, string texto)
    {
        float tam = 26f;
        if (icono != null)
        {
            GUI.DrawTexture(new Rect(x, y, tam, tam), icono, ScaleMode.ScaleToFit);
            x += tam + 6f;
        }
        GUI.Label(new Rect(x, y, 200f, tam), texto, estiloDato);
    }

    private Texture2D RetratoMateo()
    {
        GrupoEstado e = GrupoEstado.Instancia;
        int nivel = e != null ? Mathf.Min(e.moral, e.salud) : 100;
        if (nivel >= 55) { return retratoMateoNeutro; }
        if (nivel >= 30) { return retratoMateoAlerta != null ? retratoMateoAlerta : retratoMateoNeutro; }
        return retratoMateoTension != null ? retratoMateoTension : retratoMateoNeutro;
    }

    private Texture2D RetratoBety()
    {
        int conf = DialogoController.Instancia != null ? DialogoController.Instancia.confianzaBety : 55;
        if (conf >= 60) { return retratoBetyNeutro; }
        if (conf >= 35) { return retratoBetyAlerta != null ? retratoBetyAlerta : retratoBetyNeutro; }
        return retratoBetyTension != null ? retratoBetyTension : retratoBetyNeutro;
    }

    private Texture2D RetratoHablante(DialogoController dlg)
    {
        if (dlg.FaseActual == DialogoController.Fase.Respuestas)
        {
            return RetratoMateo();
        }

        string h = dlg.Hablante ?? string.Empty;
        if (h.Contains("Beatriz") || h.Contains("Bety"))
        {
            return RetratoBety();
        }
        if (h.Contains("Mateo"))
        {
            return RetratoMateo();
        }
        if (h == "..." || h.Contains("nfectad"))
        {
            return retratoInfectado;
        }
        return null;
    }

    // ---------- Paneles ----------

    private void DibujarEstado()
    {
        GrupoEstado e = GrupoEstado.Instancia;
        if (e == null)
        {
            return;
        }

        if (FlujoJuego.Instancia != null && FlujoJuego.Instancia.Estado == FlujoJuego.EstadoJuego.Menu)
        {
            return;
        }

        int confianza = DialogoController.Instancia != null ? DialogoController.Instancia.confianzaBety : 0;

        Rect caja = new Rect(10, 10, 420, 200);
        GUI.Box(caja, string.Empty);
        GUI.Box(caja, string.Empty);

        DibujarRetrato(new Rect(22, 50, 100, 100), RetratoMateo());
        GUI.Label(new Rect(22, 16, 400, 30), "HOYADA Z  -  Episodio I", estiloTitulo);

        float x = 136f;
        float x2 = 290f;
        Dato(x, 52, iconoMoral, "Moral " + e.moral);
        GUI.Label(new Rect(x2, 52, 130, 26), "Salud " + e.salud, estiloDato);
        Dato(x, 82, iconoAgua, "Agua " + e.agua);
        Dato(x2, 82, iconoMedicina, "Medicina " + e.medicina);
        GUI.Label(new Rect(x, 112, 290, 26), "Comida " + e.comida + "   Abrigo " + e.abrigo + "   Municion " + e.municion, estiloDato);

        bool luz = mateo != null && mateo.LinternaEncendida;
        Dato(x, 142, iconoBateria, luz ? "Linterna ON" : "Linterna OFF");
        Dato(x2, 142, iconoTiempo, HoraTexto);

        GUI.Label(new Rect(22, 172, 400, 26), "Confianza de Bety: " + confianza, estiloDato);
    }

    private void DibujarDecision()
    {
        DecisionController d = DecisionController.Instancia;
        if (d == null || !d.HayDecision)
        {
            return;
        }

        float w = 600f;
        float h = 290f;
        float x = (AnchoUI - w) / 2f;
        float y = (AltoUI - h) / 2f;

        GUI.Box(new Rect(x, y, w, h), string.Empty);
        GUILayout.BeginArea(new Rect(x + 22, y + 18, w - 44, h - 30));
        GUILayout.Label(d.TituloDecision, estiloTitulo);
        GUILayout.Label(d.DescDecision, estiloTexto);
        GUILayout.Space(10);
        for (int i = 0; i < d.Opciones.Count; i++)
        {
            if (GUILayout.Button(d.Opciones[i].titulo, estiloBoton, GUILayout.Height(36)))
            {
                d.Elegir(i);
            }
        }
        GUILayout.EndArea();
    }

    private void DibujarDialogo()
    {
        DialogoController dlg = DialogoController.Instancia;
        if (dlg == null || !dlg.HayDialogo)
        {
            return;
        }

        Texture2D retrato = RetratoHablante(dlg);
        float anchoRetrato = retrato != null ? 170f : 0f;

        float w = Mathf.Min(760f + anchoRetrato, AnchoUI * 0.9f);
        float h = 220f;
        float x = (AnchoUI - w) / 2f;
        float y = AltoUI - h - 24f;

        GUI.Box(new Rect(x, y, w, h), string.Empty);
        GUI.Box(new Rect(x, y, w, h), string.Empty);

        if (retrato != null)
        {
            DibujarRetrato(new Rect(x + 18, y + (h - 150f) / 2f, 150f, 150f), retrato);
        }

        GUILayout.BeginArea(new Rect(x + 22 + anchoRetrato, y + 16, w - 44 - anchoRetrato, h - 26));
        string nombre = dlg.FaseActual == DialogoController.Fase.Respuestas ? "Mateo" : dlg.Hablante;
        GUILayout.Label(nombre, estiloTitulo);
        if (dlg.FaseActual != DialogoController.Fase.Respuestas)
        {
            GUILayout.Label(dlg.LineaActual, estiloTexto);
        }
        GUILayout.Space(8);

        if (dlg.FaseActual == DialogoController.Fase.Respuestas)
        {
            for (int i = 0; i < dlg.Respuestas.Count; i++)
            {
                if (GUILayout.Button(dlg.Respuestas[i].texto, estiloBoton, GUILayout.Height(36)))
                {
                    dlg.Responder(i);
                }
            }
        }
        else
        {
            if (GUILayout.Button("Continuar", estiloBoton, GUILayout.Height(34)))
            {
                dlg.Continuar();
            }
        }

        GUILayout.EndArea();
    }

    private void DibujarFin()
    {
        DecisionController d = DecisionController.Instancia;
        if (d == null || !d.FinActivo || negro == null)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, d.FinAlpha);
        GUI.DrawTexture(new Rect(0, 0, AnchoUI, AltoUI), negro);
        GUI.color = Color.white;

        if (d.FinAlpha <= 0.85f)
        {
            return;
        }

        GrupoEstado e = GrupoEstado.Instancia;
        int conf = DialogoController.Instancia != null ? DialogoController.Instancia.confianzaBety : 0;

        GUI.Label(new Rect(0, AltoUI * 0.16f, AnchoUI, 60), "FIN DEL EPISODIO I", estiloFin);

        GUIStyle sub = new GUIStyle(estiloTexto) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        string veredicto = "El grupo se refugia en Sopocachi.";
        if (e != null)
        {
            if (e.moral >= 50 && e.salud >= 50) { veredicto = "El grupo llega entero. Todavia hay esperanza."; }
            else if (e.moral >= 25 || e.salud >= 25) { veredicto = "El grupo llega golpeado, pero vivo."; }
            else { veredicto = "El grupo llega diezmado. La cosa se ve fea."; }
        }
        GUI.Label(new Rect(0, AltoUI * 0.30f, AnchoUI, 40), veredicto, sub);

        if (e != null)
        {
            string recap = "Moral " + e.moral + "     Salud " + e.salud + "     Confianza de Bety " + conf +
                "\nComida " + e.comida + "    Agua " + e.agua + "    Medicina " + e.medicina +
                "    Abrigo " + e.abrigo + "    Municion " + e.municion + "     Hora " + HoraTexto;
            GUI.Label(new Rect(0, AltoUI * 0.40f, AnchoUI, 80), recap, sub);
        }

        GUIStyle small = new GUIStyle(estiloTexto) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        small.normal.textColor = new Color(0.72f, 0.72f, 0.78f);
        GUI.Label(new Rect(0, AltoUI * 0.54f, AnchoUI, 40), "En un rincon, alguien tose. Silencio.", small);
    }

    private void DibujarGameOver()
    {
        DecisionController d = DecisionController.Instancia;
        if (d == null || !d.GameOverActivo || negro == null)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, d.GameOverAlpha);
        GUI.DrawTexture(new Rect(0, 0, AnchoUI, AltoUI), negro);
        GUI.color = Color.white;

        if (d.GameOverAlpha > 0.85f)
        {
            if (retratoInfectado != null)
            {
                float tam = Mathf.Min(220f, AltoUI * 0.25f);
                GUI.DrawTextureWithTexCoords(new Rect((AnchoUI - tam) / 2f, AltoUI * 0.08f, tam, tam), retratoInfectado, RecorteRetrato);
            }

            GUI.Label(new Rect(0, AltoUI / 2f - 70, AnchoUI, 180),
                "GAME OVER\n\n" + d.MotivoGameOver, estiloGameOver);
        }
    }

    private void DibujarConfirm()
    {
        ConfirmController c = ConfirmController.Instancia;
        if (c == null || !c.Activo)
        {
            return;
        }

        float w = 460f;
        float h = 160f;
        float x = (AnchoUI - w) / 2f;
        float y = (AltoUI - h) / 2f;
        GUI.Box(new Rect(x, y, w, h), string.Empty);
        GUILayout.BeginArea(new Rect(x + 22, y + 20, w - 44, h - 30));
        GUILayout.Label(c.Pregunta, estiloTitulo);
        GUILayout.Space(14);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Si", estiloBoton, GUILayout.Height(42))) { c.Responder(true); }
        GUILayout.Space(18);
        if (GUILayout.Button("No", estiloBoton, GUILayout.Height(42))) { c.Responder(false); }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DibujarPrompt()
    {
        InteraccionManager im = InteraccionManager.Instancia;
        if (im == null || im.Actual == null || !FlujoJuego.EnJuego)
        {
            return;
        }

        float w = 380f;
        float x = (AnchoUI - w) / 2f;
        float y = AltoUI - 100f;
        GUI.Box(new Rect(x, y, w, 40f), string.Empty);
        GUIStyle s = new GUIStyle(estiloTitulo) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        GUI.Label(new Rect(x, y, w, 40f), "[E]   " + im.Actual.prompt, s);
    }

    private void DibujarObjetivo()
    {
        Misiones m = Misiones.Instancia;
        if (m == null || string.IsNullOrEmpty(m.ObjetivoActual))
        {
            return;
        }

        if (FlujoJuego.Instancia == null || FlujoJuego.Instancia.Estado == FlujoJuego.EstadoJuego.Menu)
        {
            return;
        }

        DecisionController d = DecisionController.Instancia;
        if (d != null && (d.FinActivo || d.GameOverActivo))
        {
            return;
        }

        float w = 640f;
        float x = Mathf.Max(440f, (AnchoUI - w) / 2f);
        GUI.Box(new Rect(x, 12f, w, 34f), string.Empty);
        GUIStyle s = new GUIStyle(estiloTexto) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        s.normal.textColor = new Color(1f, 0.88f, 0.5f);
        GUI.Label(new Rect(x, 12f, w, 34f), "OBJETIVO:   " + m.ObjetivoActual, s);
    }
}
