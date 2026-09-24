using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Flujo global del juego: pantalla de titulo (menu con fondo y logo), estado "jugando"
/// y pantallas de cierre (victoria / game over) con Reintentar y Menu.
/// Centraliza cuando el jugador tiene control (EnJuego).
/// </summary>
public class FlujoJuego : MonoBehaviour
{
    // Escala la interfaz IMGUI para que se vea igual en 1080p, 1440p o 4K.
    private static float EscalaUI { get { return Mathf.Max(1f, Screen.height / 1080f); } }
    private static float AnchoUI { get { return Screen.width / EscalaUI; } }
    private static float AltoUI { get { return Screen.height / EscalaUI; } }

    public static FlujoJuego Instancia { get; private set; }

    public enum EstadoJuego { Menu, Jugando }
    public EstadoJuego Estado { get; private set; } = EstadoJuego.Menu;

    [Header("Arte del menu")]
    [Tooltip("Ilustracion de fondo del menu principal.")]
    public Texture2D fondoMenu;

    [Tooltip("Logotipo del juego (PNG con transparencia).")]
    public Texture2D logo;

    [Tooltip("Zona util del logo dentro de la imagen (UV). Recorta el espacio transparente sobrante.")]
    public Rect recorteLogo = new Rect(0.279f, 0.100f, 0.442f, 0.824f);

    private static bool iniciarEnJuego;

    private GUIStyle estiloTitulo;
    private GUIStyle estiloSubtitulo;
    private GUIStyle estiloTexto;
    private GUIStyle estiloBoton;

    /// <summary>El jugador solo controla a Mateo cuando esta jugando y no hay panel activo.</summary>
    public static bool EnJuego
    {
        get
        {
            return Instancia != null
                && Instancia.Estado == EstadoJuego.Jugando
                && !DecisionController.PausaActiva
                && !DialogoController.DialogoPausa
                && !ConfirmController.Pausa;
        }
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
        EleccionProvisiones.ReiniciarEleccion();
    }

    private void Start()
    {
        Estado = iniciarEnJuego ? EstadoJuego.Jugando : EstadoJuego.Menu;
        iniciarEnJuego = false;
    }

    private void InitEstilos()
    {
        if (estiloTitulo != null)
        {
            return;
        }

        estiloTitulo = new GUIStyle(GUI.skin.label) { fontSize = 48, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        estiloTitulo.normal.textColor = new Color(0.86f, 0.88f, 0.96f);

        estiloSubtitulo = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        estiloSubtitulo.normal.textColor = new Color(0.85f, 0.87f, 0.92f);

        estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        estiloTexto.normal.textColor = new Color(0.95f, 0.95f, 0.97f);

        estiloBoton = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };
    }

    private void OnGUI()
    {
        InitEstilos();
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(EscalaUI, EscalaUI, 1f));

        if (Estado == EstadoJuego.Menu)
        {
            DibujarMenu();
        }
        else
        {
            DibujarBotonesFinales();
        }
    }

    private void DibujarMenu()
    {
        Rect pantalla = new Rect(0, 0, AnchoUI, AltoUI);

        if (fondoMenu != null)
        {
            GUI.DrawTexture(pantalla, fondoMenu, ScaleMode.ScaleAndCrop);
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
        }
        else
        {
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
        }
        GUI.DrawTexture(pantalla, Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = AnchoUI / 2f;

        if (logo != null)
        {
            float alto = AltoUI * 0.36f;
            float aspecto = (recorteLogo.width * logo.width) / Mathf.Max(1f, recorteLogo.height * logo.height);
            float ancho = alto * aspecto;
            GUI.DrawTextureWithTexCoords(new Rect(cx - ancho / 2f, AltoUI * 0.06f, ancho, alto), logo, recorteLogo);
        }
        else
        {
            GUI.Label(new Rect(0, AltoUI * 0.2f, AnchoUI, 70), "HOYADA Z", estiloTitulo);
            GUI.Label(new Rect(0, AltoUI * 0.2f + 66, AnchoUI, 36), "Ecos del Altiplano", estiloSubtitulo);
        }

        Rect caja = new Rect(cx - 340, AltoUI * 0.45f, 680, 110);
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(caja, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(caja.x + 16, caja.y + 6, caja.width - 32, caja.height - 12),
            "Episodio I  -  La Paz, 07:42\n\nLas primeras horas del brote. Reune provisiones, decide a quien salvar y lleva a tu grupo al refugio... si puedes.",
            estiloTexto);

        if (GUI.Button(new Rect(cx - 120, AltoUI * 0.66f, 240, 60), "JUGAR", estiloBoton))
        {
            Estado = EstadoJuego.Jugando;
        }

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0, AltoUI - 48, AnchoUI, 48), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, AltoUI - 42, AnchoUI, 34),
            "WASD moverse   ·   Mouse mirar   ·   Espacio saltar   ·   F linterna   ·   E interactuar   ·   Esc soltar mouse",
            estiloSubtitulo);
    }

    private void DibujarBotonesFinales()
    {
        DecisionController d = DecisionController.Instancia;
        if (d == null)
        {
            return;
        }

        bool victoria = d.FinActivo && d.FinAlpha > 0.9f;
        bool derrota = d.GameOverActivo && d.GameOverAlpha > 0.9f;
        if (!victoria && !derrota)
        {
            return;
        }

        float cx = AnchoUI / 2f;
        float y = AltoUI * 0.64f;

        if (GUI.Button(new Rect(cx - 230, y, 210, 50), "REINTENTAR", estiloBoton))
        {
            iniciarEnJuego = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (GUI.Button(new Rect(cx + 20, y, 210, 50), "MENU", estiloBoton))
        {
            iniciarEnJuego = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
