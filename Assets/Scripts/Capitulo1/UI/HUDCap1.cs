using UnityEngine;

/// <summary>
/// Interfaz completa del juego (IMGUI escalada a 1080p). Este archivo tiene la base
/// (estilos, utilidades y el despachador); el resto esta repartido en:
/// HUDCap1_Menu (menu, dificultad, opciones, logros, records), HUDCap1_Juego (HUD en juego,
/// armas, fabricacion, escucha, agarre, jefe, supervivencia) y HUDCap1_Pantallas (pausa con
/// diario y mapa, lectura, decisiones, muerte y final con rango).
/// </summary>
public partial class HUDCap1 : MonoBehaviour
{
    [Header("Iconos")]
    public Texture2D iconoMoral;
    public Texture2D iconoAgua;
    public Texture2D iconoMedicina;
    public Texture2D iconoBateria;
    public Texture2D iconoTiempo;

    [Header("Retratos")]
    public Texture2D mateoNeutro;
    public Texture2D mateoAlerta;
    public Texture2D mateoTension;
    public Texture2D betyNeutro;
    public Texture2D betyAlerta;
    public Texture2D betyTension;
    public Texture2D infectado;
    public Texture2D waraRetrato;
    public Texture2D lustraRetrato;

    [Header("Menu")]
    public Texture2D fondoMenu;
    public Texture2D logo;
    public Rect recorteLogo = new Rect(0.279f, 0.100f, 0.442f, 0.824f);

    private static readonly Rect RecorteRetrato = new Rect(0.04f, 0.08f, 0.92f, 0.92f);
    private const float AltoBase = 1080f;

    private float escala = 1f;
    private float W, H;
    private Texture2D vineta;
    private Texture2D blanco;
    private Texture2D circulo;
    private Texture2D fondoBoton, fondoBotonHover, fondoPanel, fondoPapel, fondoBotonOff;
    private GUIStyle sTexto, sTitulo, sGrande, sEnorme, sBoton, sBotonOff, sPequeno, sCentro, sPapel, sPapelTitulo, sDerecha, sMarca, sMini;
    private Jugador j;

    private void Start()
    {
        blanco = Texture2D.whiteTexture;
        vineta = CrearVineta(256);
        circulo = EfectosFX.CirculoSuave;
        fondoBoton = Solido(new Color(0.08f, 0.08f, 0.1f, 0.85f));
        fondoBotonHover = Solido(new Color(0.55f, 0.12f, 0.1f, 0.9f));
        fondoBotonOff = Solido(new Color(0.12f, 0.12f, 0.13f, 0.6f));
        fondoPanel = Solido(new Color(0.03f, 0.03f, 0.04f, 0.82f));
        fondoPapel = Solido(new Color(0.86f, 0.82f, 0.72f, 1f));
    }

    private static Texture2D Solido(Color c)
    {
        var t = new Texture2D(2, 2);
        t.SetPixels(new[] { c, c, c, c });
        t.Apply();
        return t;
    }

    private static Texture2D CrearVineta(int n)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2f - 1f;
                float dy = (y + 0.5f) / n * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx * 0.8f + dy * dy);
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1.35f, d));
                px[y * n + x] = new Color(1f, 1f, 1f, a);
            }
        }
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    private void Estilos()
    {
        if (sTexto != null)
        {
            return;
        }
        sTexto = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, richText = true };
        sTexto.normal.textColor = new Color(0.92f, 0.92f, 0.94f);
        sPequeno = new GUIStyle(sTexto) { fontSize = 16 };
        sMini = new GUIStyle(sTexto) { fontSize = 13, wordWrap = false };
        sTitulo = new GUIStyle(sTexto) { fontSize = 26, fontStyle = FontStyle.Bold };
        sGrande = new GUIStyle(sTexto) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        sEnorme = new GUIStyle(sTexto) { fontSize = 64, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        sCentro = new GUIStyle(sTexto) { alignment = TextAnchor.MiddleCenter };
        sDerecha = new GUIStyle(sPequeno) { alignment = TextAnchor.MiddleRight };
        sMarca = new GUIStyle(sTexto) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        sBoton = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true, richText = true };
        sBoton.normal.background = fondoBoton;
        sBoton.hover.background = fondoBotonHover;
        sBoton.active.background = fondoBotonHover;
        sBoton.focused.background = fondoBoton;
        sBoton.normal.textColor = new Color(0.9f, 0.9f, 0.92f);
        sBoton.hover.textColor = Color.white;
        sBoton.active.textColor = Color.white;
        sBoton.padding = new RectOffset(16, 16, 8, 8);

        sBotonOff = new GUIStyle(sBoton);
        sBotonOff.normal.background = fondoBotonOff;
        sBotonOff.hover.background = fondoBotonOff;
        sBotonOff.active.background = fondoBotonOff;
        sBotonOff.normal.textColor = new Color(0.5f, 0.5f, 0.52f);
        sBotonOff.hover.textColor = new Color(0.55f, 0.55f, 0.57f);

        sPapel = new GUIStyle(sTexto) { fontSize = 21 };
        sPapel.normal.textColor = new Color(0.14f, 0.12f, 0.1f);
        sPapelTitulo = new GUIStyle(sPapel) { fontSize = 28, fontStyle = FontStyle.Bold };
    }

    // ---------- Utilidades ----------

    private void Rect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = new Color(c.r, c.g, c.b, c.a * prev.a);
        GUI.DrawTexture(r, blanco);
        GUI.color = prev;
    }

    private void Marco(Rect r, Color c, float g = 2f)
    {
        Rect(new Rect(r.x, r.y, r.width, g), c);
        Rect(new Rect(r.x, r.yMax - g, r.width, g), c);
        Rect(new Rect(r.x, r.y, g, r.height), c);
        Rect(new Rect(r.xMax - g, r.y, g, r.height), c);
    }

    private void Circulo(Vector2 centro, float radio, Color c)
    {
        Color prev = GUI.color;
        GUI.color = new Color(c.r, c.g, c.b, c.a * prev.a);
        GUI.DrawTexture(new Rect(centro.x - radio, centro.y - radio, radio * 2f, radio * 2f), circulo);
        GUI.color = prev;
    }

    private void Sombra(Rect r, string t, GUIStyle s, Color? color = null)
    {
        Color prev = s.normal.textColor;
        s.normal.textColor = new Color(0f, 0f, 0f, 0.85f * GUI.color.a);
        GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), t, s);
        s.normal.textColor = color ?? prev;
        GUI.Label(r, t, s);
        s.normal.textColor = prev;
    }

    private void Barra(Rect r, float valor, Color c, Color fondo)
    {
        Rect(r, fondo);
        Rect(new Rect(r.x, r.y, r.width * Mathf.Clamp01(valor), r.height), c);
    }

    private void Retrato(Rect r, Texture2D t)
    {
        if (t == null)
        {
            return;
        }
        Rect(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), new Color(0f, 0f, 0f, 0.7f));
        GUI.DrawTextureWithTexCoords(r, t, RecorteRetrato);
    }

    private Texture2D RetratoPorNombre(string n)
    {
        if (string.IsNullOrEmpty(n)) { return null; }
        switch (n)
        {
            case "bety": return betyNeutro;
            case "bety_alerta": return betyAlerta;
            case "bety_tension": return betyTension;
            case "mateo": return RetratoMateo();
            case "infectado": return infectado;
            case "wara": return waraRetrato;
            case "lustra": return lustraRetrato;
        }
        return null;
    }

    private Texture2D RetratoMateo()
    {
        if (j == null) { return mateoNeutro; }
        if (j.Salud >= 60) { return mateoNeutro; }
        if (j.Salud >= 30) { return mateoAlerta != null ? mateoAlerta : mateoNeutro; }
        return mateoTension != null ? mateoTension : mateoNeutro;
    }

    private bool Boton(Rect r, string t, bool habilitado = true)
    {
        bool b = GUI.Button(r, t, habilitado ? sBoton : sBotonOff);
        if (b)
        {
            AudioCap1.Play2D(habilitado ? "sfx_click" : "sfx_vacio", 0.6f);
        }
        return b && habilitado;
    }

    private bool APantalla(Vector3 mundo, out Vector2 p)
    {
        p = Vector2.zero;
        Camera c = Camera.main;
        if (c == null) { return false; }
        Vector3 s = c.WorldToScreenPoint(mundo);
        if (s.z < 0.2f) { return false; }
        p = new Vector2(s.x / escala, (Screen.height - s.y) / escala);
        return p.x > -50 && p.x < W + 50 && p.y > -50 && p.y < H + 50;
    }

    // ---------- Dibujo ----------

    private void OnGUI()
    {
        Estilos();
        escala = Screen.height / AltoBase;
        W = Screen.width / escala;
        H = AltoBase;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(escala, escala, 1f));
        if (j == null) { j = Jugador.I; }
        ContadorFPS();

        Juego g = Juego.I;
        if (g == null)
        {
            return;
        }

        switch (g.estado)
        {
            case Juego.Estado.Menu:
                Menu(g);
                AvisosLogros();
                return;
            case Juego.Estado.Intro:
                Rect(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, g.Negro));
                Parpadeo(g);
                TarjetaCentral(g);
                Subtitulos(g);
                return;
        }

        bool enJuego = g.estado == Juego.Estado.Jugando || g.estado == Juego.Estado.Pausa || g.estado == Juego.Estado.Lectura
                       || g.estado == Juego.Estado.Eleccion || g.estado == Juego.Estado.Mapa;
        if (enJuego)
        {
            Escucha(g);
            Vinetas();
            IndicadorDano();
            Marcadores(g);
            MarcadorObjetivo(g);
            PanelEstado(g);
            PanelArmas(g);
            ObjetivoYAsedio(g);
            BarraJefe();
            HUDSupervivencia(g);
            Avisos(g);
            Prompt(g);
            Mira();
            Agarre(g);
            PanelCrafteo(g);
            Subtitulos(g);
        }
        else if (g.estado == Juego.Estado.Cinematica)
        {
            Franjas(g);
            Subtitulos(g);
        }

        if (g.Negro > 0.001f && g.estado != Juego.Estado.Fin)
        {
            Rect(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, g.Negro));
        }

        switch (g.estado)
        {
            case Juego.Estado.Pausa: Pausa(g); break;
            case Juego.Estado.Mapa: PantallaMapa(g); break;
            case Juego.Estado.Lectura: Lectura(g); break;
            case Juego.Estado.Eleccion: Eleccion(g); break;
            case Juego.Estado.Muerto: Muerte(g); break;
            case Juego.Estado.Cinematica: TarjetaCentral(g); if (g.Opciones != null && g.Opciones.Count > 0) { Eleccion(g); } break;
            case Juego.Estado.Fin: Fin(g); break;
        }
        AvisosLogros();
    }

    private float fpsSuave = 60f;

    private void ContadorFPS()
    {
        if (!Opciones.MostrarFPS) { return; }
        if (Event.current.type == EventType.Repaint && Time.unscaledDeltaTime > 0.0001f)
        {
            fpsSuave = Mathf.Lerp(fpsSuave, 1f / Time.unscaledDeltaTime, 0.05f);
        }
        Color c = fpsSuave >= 50f ? new Color(0.5f, 0.95f, 0.55f) : (fpsSuave >= 30f ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.4f, 0.35f));
        Sombra(new Rect(W - 130, 4, 120, 24), Mathf.RoundToInt(fpsSuave) + " FPS", new GUIStyle(sMini) { alignment = TextAnchor.MiddleRight, fontSize = 15 }, c);
    }

    private void Parpadeo(Juego g)
    {
        if (g.Parpadeo <= 0.001f)
        {
            return;
        }
        float h = g.Parpadeo * H * 0.5f;
        Rect(new Rect(0, 0, W, h), Color.black);
        Rect(new Rect(0, H - h, W, h), Color.black);
    }

    private void Franjas(Juego g)
    {
        float h = g.Franjas * H * 0.11f;
        if (h < 0.5f) { return; }
        Rect(new Rect(0, 0, W, h), Color.black);
        Rect(new Rect(0, H - h, W, h), Color.black);
        if (CinematicaCamara.Activa != null)
        {
            Sombra(new Rect(W - 260, H - 40, 240, 30), "[Espacio] omitir", new GUIStyle(sPequeno) { alignment = TextAnchor.MiddleRight }, new Color(0.6f, 0.6f, 0.65f));
        }
    }

    private void TarjetaCentral(Juego g)
    {
        if (string.IsNullOrEmpty(g.Tarjeta))
        {
            return;
        }
        Color prev = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, g.TarjetaAlpha);
        GUIStyle s = new GUIStyle(sGrande) { fontSize = 34, wordWrap = true, fontStyle = FontStyle.Normal };
        Sombra(new Rect(W * 0.15f, H * 0.3f, W * 0.7f, H * 0.4f), g.Tarjeta, s);
        GUI.color = prev;
    }

    private void AvisosLogros()
    {
        float y = 24f;
        foreach (Logros.Aviso a in Logros.Avisos)
        {
            float resta = a.hasta - Time.unscaledTime;
            float entrada = Mathf.Clamp01((5.5f - resta) * 4f);
            float salida = Mathf.Clamp01(resta * 2f);
            float x = W - 470f * Mathf.SmoothStep(0f, 1f, entrada);
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, salida);
            Rect r = new Rect(x, y, 450, 78);
            Rect(r, new Color(0.05f, 0.04f, 0.03f, 0.92f));
            Rect(new Rect(r.x, r.y, 6, r.height), new Color(0.95f, 0.72f, 0.25f));
            Sombra(new Rect(r.x + 20, r.y + 6, 420, 24), "LOGRO DESBLOQUEADO", new GUIStyle(sMini) { fontStyle = FontStyle.Bold }, new Color(0.95f, 0.72f, 0.25f));
            Sombra(new Rect(r.x + 20, r.y + 24, 420, 28), a.titulo, new GUIStyle(sPequeno) { fontSize = 20, fontStyle = FontStyle.Bold });
            Sombra(new Rect(r.x + 20, r.y + 50, 420, 24), a.descripcion, new GUIStyle(sMini), new Color(0.75f, 0.75f, 0.78f));
            GUI.color = prev;
            y += 86f;
        }
    }
}
