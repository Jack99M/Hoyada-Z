using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Flujo global del juego: pantalla de titulo (menu), estado "jugando" y
/// pantallas de cierre (victoria / game over) con Reintentar y Menu.
/// Centraliza cuando el jugador tiene control (EnJuego).
/// </summary>
public class FlujoJuego : MonoBehaviour
{
    public static FlujoJuego Instancia { get; private set; }

    public enum EstadoJuego { Menu, Jugando }
    public EstadoJuego Estado { get; private set; } = EstadoJuego.Menu;

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

        estiloSubtitulo = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        estiloSubtitulo.normal.textColor = new Color(0.72f, 0.74f, 0.82f);

        estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        estiloTexto.normal.textColor = new Color(0.8f, 0.8f, 0.85f);

        estiloBoton = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
    }

    private void OnGUI()
    {
        InitEstilos();

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
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        GUI.Label(new Rect(0, Screen.height * 0.2f, Screen.width, 70), "HOYADA Z", estiloTitulo);
        GUI.Label(new Rect(0, Screen.height * 0.2f + 66, Screen.width, 36), "Ecos del Altiplano", estiloSubtitulo);
        GUI.Label(new Rect(cx - 320, Screen.height * 0.42f, 640, 100),
            "Episodio I  -  La Paz, 07:42\n\nLas primeras horas del brote. Reune provisiones, decide a quien salvar y lleva a tu grupo al refugio... si puedes.",
            estiloTexto);

        if (GUI.Button(new Rect(cx - 110, Screen.height * 0.63f, 220, 56), "JUGAR", estiloBoton))
        {
            Estado = EstadoJuego.Jugando;
        }

        GUI.Label(new Rect(0, Screen.height - 42, Screen.width, 26),
            "WASD moverse   ·   Espacio saltar   ·   F linterna", estiloSubtitulo);
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

        float cx = Screen.width / 2f;
        float y = Screen.height * 0.64f;

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
