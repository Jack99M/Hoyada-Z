using UnityEngine;

/// <summary>
/// HUD del Episodio I (IMGUI): estado del grupo, panel de decision de acto,
/// panel de dialogo, fundido de cierre y pantalla de GAME OVER por crisis.
/// Solucion rapida para el vertical slice; la UI final iria en uGUI / UI Toolkit.
/// </summary>
public class HUDEstado : MonoBehaviour
{
    private GUIStyle estiloTexto;
    private GUIStyle estiloTitulo;
    private GUIStyle estiloBoton;
    private GUIStyle estiloFin;
    private GUIStyle estiloGameOver;
    private Texture2D negro;

    private void Start()
    {
        negro = new Texture2D(1, 1);
        negro.SetPixel(0, 0, Color.black);
        negro.Apply();
    }

    private void InitEstilos()
    {
        if (estiloTexto != null)
        {
            return;
        }

        estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
        estiloTexto.normal.textColor = Color.white;

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

        DibujarEstado();
        DibujarObjetivo();
        DibujarDecision();
        DibujarDialogo();
        DibujarConfirm();
        DibujarPrompt();
        DibujarFin();
        DibujarGameOver();
    }

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

        GUI.Box(new Rect(10, 10, 330, 150), string.Empty);
        GUILayout.BeginArea(new Rect(22, 16, 310, 142));
        GUILayout.Label("HOYADA Z  -  Episodio I", estiloTitulo);
        GUILayout.Label("Moral: " + e.moral + "      Salud: " + e.salud, estiloTexto);
        GUILayout.Label("Comida: " + e.comida + "   Agua: " + e.agua + "   Medicina: " + e.medicina, estiloTexto);
        GUILayout.Label("Abrigo: " + e.abrigo + "   Municion: " + e.municion, estiloTexto);
        GUILayout.Label("Confianza de Bety: " + confianza, estiloTexto);
        GUILayout.EndArea();
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
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f;

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

        float w = Mathf.Min(760f, Screen.width * 0.8f);
        float h = 210f;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height - h - 24f;

        GUI.Box(new Rect(x, y, w, h), string.Empty);
        GUILayout.BeginArea(new Rect(x + 22, y + 16, w - 44, h - 26));
        GUILayout.Label(dlg.Hablante, estiloTitulo);
        GUILayout.Label(dlg.LineaActual, estiloTexto);
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
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), negro);
        GUI.color = Color.white;

        if (d.FinAlpha <= 0.85f)
        {
            return;
        }

        GrupoEstado e = GrupoEstado.Instancia;
        int conf = DialogoController.Instancia != null ? DialogoController.Instancia.confianzaBety : 0;

        GUI.Label(new Rect(0, Screen.height * 0.16f, Screen.width, 60), "FIN DEL EPISODIO I", estiloFin);

        GUIStyle sub = new GUIStyle(estiloTexto) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        string veredicto = "El grupo se refugia en Sopocachi.";
        if (e != null)
        {
            if (e.moral >= 50 && e.salud >= 50) { veredicto = "El grupo llega entero. Todavia hay esperanza."; }
            else if (e.moral >= 25 || e.salud >= 25) { veredicto = "El grupo llega golpeado, pero vivo."; }
            else { veredicto = "El grupo llega diezmado. La cosa se ve fea."; }
        }
        GUI.Label(new Rect(0, Screen.height * 0.30f, Screen.width, 40), veredicto, sub);

        if (e != null)
        {
            string recap = "Moral " + e.moral + "     Salud " + e.salud + "     Confianza de Bety " + conf +
                "\nComida " + e.comida + "    Agua " + e.agua + "    Medicina " + e.medicina +
                "    Abrigo " + e.abrigo + "    Municion " + e.municion;
            GUI.Label(new Rect(0, Screen.height * 0.40f, Screen.width, 80), recap, sub);
        }

        GUIStyle small = new GUIStyle(estiloTexto) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        small.normal.textColor = new Color(0.72f, 0.72f, 0.78f);
        GUI.Label(new Rect(0, Screen.height * 0.54f, Screen.width, 40), "En un rincon, alguien tose. Silencio.", small);
    }

    private void DibujarGameOver()
    {
        DecisionController d = DecisionController.Instancia;
        if (d == null || !d.GameOverActivo || negro == null)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, d.GameOverAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), negro);
        GUI.color = Color.white;

        if (d.GameOverAlpha > 0.85f)
        {
            GUI.Label(new Rect(0, Screen.height / 2f - 90, Screen.width, 180),
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
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f;
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
        float x = (Screen.width - w) / 2f;
        float y = Screen.height - 100f;
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
        float x = (Screen.width - w) / 2f;
        GUI.Box(new Rect(x, 12f, w, 34f), string.Empty);
        GUIStyle s = new GUIStyle(estiloTexto) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        s.normal.textColor = new Color(1f, 0.88f, 0.5f);
        GUI.Label(new Rect(x, 12f, w, 34f), "OBJETIVO:   " + m.ObjetivoActual, s);
    }
}
