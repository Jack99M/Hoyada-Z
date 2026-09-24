using UnityEngine;

/// <summary>
/// Menu principal: continuar, nueva partida (con dificultad), modo Supervivencia, logros,
/// records, opciones y controles.
/// </summary>
public partial class HUDCap1
{
    private enum PantallaMenu { Principal, DificultadHistoria, DificultadSupervivencia, Controles, Opciones, Logros, Records }
    private PantallaMenu pantalla = PantallaMenu.Principal;

    private void Menu(Juego g)
    {
        if (fondoMenu != null) { GUI.DrawTexture(new Rect(0, 0, W, H), fondoMenu, ScaleMode.ScaleAndCrop); }
        Rect(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, fondoMenu != null ? 0.5f : 0.9f));
        GUI.DrawTexture(new Rect(0, 0, W, H), vineta, ScaleMode.StretchToFill, true, 0f, new Color(0f, 0f, 0f, 0.8f), 0f, 0f);

        float cx = W / 2f;
        bool principal = pantalla == PantallaMenu.Principal;
        float altoLogo = principal ? H * 0.3f : H * 0.2f;
        if (logo != null)
        {
            float ancho = altoLogo * (recorteLogo.width * logo.width) / Mathf.Max(1f, recorteLogo.height * logo.height);
            GUI.DrawTextureWithTexCoords(new Rect(cx - ancho / 2f, H * 0.03f, ancho, altoLogo), logo, recorteLogo);
        }
        else
        {
            Sombra(new Rect(0, H * 0.08f, W, 90), "HOYADA Z", sEnorme);
        }

        switch (pantalla)
        {
            case PantallaMenu.Principal: MenuPrincipal(g, cx); break;
            case PantallaMenu.DificultadHistoria: MenuDificultad(g, cx, false); break;
            case PantallaMenu.DificultadSupervivencia: MenuDificultad(g, cx, true); break;
            case PantallaMenu.Controles:
                Controles(new Rect(cx - 460, H * 0.26f, 920, 600));
                if (Boton(new Rect(cx - 130, H * 0.26f + 620, 260, 50), "VOLVER")) { pantalla = PantallaMenu.Principal; }
                break;
            case PantallaMenu.Opciones:
                PanelOpciones(new Rect(cx - 420, H * 0.26f, 840, 470));
                if (Boton(new Rect(cx - 130, H * 0.26f + 490, 260, 50), "VOLVER")) { Opciones.Guardar(); pantalla = PantallaMenu.Principal; }
                break;
            case PantallaMenu.Logros:
                PanelLogros(new Rect(cx - 520, H * 0.24f, 1040, 640));
                if (Boton(new Rect(cx - 130, H * 0.24f + 660, 260, 50), "VOLVER")) { pantalla = PantallaMenu.Principal; }
                break;
            case PantallaMenu.Records:
                PanelRecords(new Rect(cx - 440, H * 0.26f, 880, 560));
                if (Boton(new Rect(cx - 130, H * 0.26f + 580, 260, 50), "VOLVER")) { pantalla = PantallaMenu.Principal; }
                break;
        }
    }

    private void MenuPrincipal(Juego g, float cx)
    {
        Sombra(new Rect(0, H * 0.34f, W, 40), g.tituloCapitulo.ToUpper(), new GUIStyle(sGrande) { fontSize = 30 }, new Color(0.85f, 0.3f, 0.25f));
        Sombra(new Rect(cx - 460, H * 0.38f, 920, 40), "La Paz, Bolivia. La noche que la Cepa del Valle salió del río Choqueyapu.", new GUIStyle(sCentro) { fontSize = 20 });

        float y = H * 0.44f;
        float w = 420, h = 48, paso = 56;
        if (Guardado.Existe)
        {
            if (Boton(new Rect(cx - w / 2f, y, w, h + 6), "CONTINUAR")) { g.ContinuarPartida(); }
            Sombra(new Rect(cx - 400, y + h + 6, 800, 22), Guardado.Resumen, new GUIStyle(sMini) { alignment = TextAnchor.MiddleCenter }, new Color(0.7f, 0.7f, 0.72f));
            y += paso + 24;
        }
        if (Boton(new Rect(cx - w / 2f, y, w, h), "MODO HISTORIA")) { pantalla = PantallaMenu.DificultadHistoria; }
        y += paso;
        if (Boton(new Rect(cx - w / 2f, y, w, h), "MODO SUPERVIVENCIA")) { pantalla = PantallaMenu.DificultadSupervivencia; }
        y += paso;
        if (Boton(new Rect(cx - w / 2f, y, w / 2f - 4, h), "LOGROS  " + Logros.Cantidad + "/" + Logros.Lista.Length)) { pantalla = PantallaMenu.Logros; }
        if (Boton(new Rect(cx + 4, y, w / 2f - 4, h), "RÉCORDS")) { pantalla = PantallaMenu.Records; }
        y += paso;
        if (Boton(new Rect(cx - w / 2f, y, w / 2f - 4, h), "OPCIONES")) { pantalla = PantallaMenu.Opciones; }
        if (Boton(new Rect(cx + 4, y, w / 2f - 4, h), "CONTROLES")) { pantalla = PantallaMenu.Controles; }
        y += paso;
        if (Boton(new Rect(cx - w / 2f, y, w, h), "SALIR")) { g.SalirDelJuego(); }

        Sombra(new Rect(0, H - 44, W, 30), "Usa audífonos. Los infectados te escuchan.   ·   Proyecto Hoyada Z — Programación Gráfica y Multimedia II", new GUIStyle(sCentro) { fontSize = 16 }, new Color(0.65f, 0.65f, 0.7f));
    }

    private void MenuDificultad(Juego g, float cx, bool supervivencia)
    {
        Sombra(new Rect(0, H * 0.24f, W, 50), supervivencia ? "MODO SUPERVIVENCIA — ELIGE LA DIFICULTAD" : "MODO HISTORIA — ELIGE LA DIFICULTAD", new GUIStyle(sGrande) { fontSize = 30 });
        if (supervivencia)
        {
            Sombra(new Rect(cx - 480, H * 0.29f, 960, 60), "Noche en la Plaza Abaroa: oleadas infinitas de infectados. Suministros entre oleadas. ¿Cuánto aguantas?", new GUIStyle(sCentro) { fontSize = 19 }, new Color(0.8f, 0.8f, 0.82f));
        }
        float w = 340, h = 300, sep = 24;
        float x0 = cx - (w * 3 + sep * 2) / 2f;
        float y = H * 0.36f;
        for (int i = 0; i < 3; i++)
        {
            NivelDificultad n = (NivelDificultad)i;
            Rect r = new Rect(x0 + i * (w + sep), y, w, h);
            bool actual = Dificultad.Nivel == n;
            GUI.DrawTexture(r, fondoPanel);
            Marco(r, actual ? new Color(0.85f, 0.3f, 0.25f, 0.9f) : new Color(1f, 1f, 1f, 0.12f), actual ? 3f : 1f);
            Color c = i == 0 ? new Color(0.5f, 0.85f, 0.55f) : (i == 1 ? new Color(0.95f, 0.8f, 0.45f) : new Color(1f, 0.4f, 0.35f));
            Sombra(new Rect(r.x, r.y + 20, w, 40), Dificultad.Nombre(n).ToUpper(), new GUIStyle(sGrande) { fontSize = 28 }, c);
            GUI.Label(new Rect(r.x + 24, r.y + 76, w - 48, 140), Dificultad.Descripcion(n), new GUIStyle(sPequeno) { alignment = TextAnchor.UpperCenter });
            if (Boton(new Rect(r.x + 40, r.yMax - 70, w - 80, 50), "JUGAR"))
            {
                if (supervivencia) { g.JugarSupervivencia(n); }
                else { g.Jugar(n); }
                pantalla = PantallaMenu.Principal;
            }
        }
        if (!supervivencia && Guardado.Existe)
        {
            Sombra(new Rect(cx - 450, y + h + 16, 900, 26), "Empezar una partida nueva no borra tu partida guardada hasta que guardes en un altar.", new GUIStyle(sMini) { alignment = TextAnchor.MiddleCenter }, new Color(0.7f, 0.7f, 0.72f));
        }
        if (Boton(new Rect(cx - 130, y + h + 56, 260, 50), "VOLVER")) { pantalla = PantallaMenu.Principal; }
    }

    private void Controles(Rect r)
    {
        GUI.DrawTexture(r, fondoPanel);
        string t =
            "<b><color=#E8B46A>MOVIMIENTO</color></b>\n" +
            "<b>WASD</b> moverse   ·   <b>Mouse</b> mirar   ·   <b>Shift</b> correr   ·   <b>C / Ctrl</b> agacharse\n<b>Espacio</b> saltar   ·   <b>Alt / V</b> esquivar (te hace invulnerable un instante)\n" +
            "<b>Z</b> (mantener) modo escucha: oyes a los infectados a través de las paredes\n\n" +
            "<b><color=#E8B46A>COMBATE</color></b>\n" +
            "<b>1</b> cuerpo a cuerpo   ·   <b>2</b> honda   ·   <b>3</b> revólver\n" +
            "<b>Clic izquierdo</b> golpear / disparar   ·   <b>Clic derecho</b> empujar (cuerpo a cuerpo) o apuntar (honda y revólver)\n" +
            "<b>E</b> agachado y por la espalda: ataque sigiloso (a un fúngico solo con una punta)\n" +
            "<b>Q</b> lanzar botella (distrae)   ·   <b>G</b> lanzar molotov   ·   <b>R</b> recargar revólver / cambiar pilas\n" +
            "Si te agarran: machaca <b>E</b> para zafarte, o <b>clic izquierdo</b> para clavar una punta\n\n" +
            "<b><color=#E8B46A>SUPERVIVENCIA</color></b>\n" +
            "<b>Tab</b> (mantener) mochila de fabricación + <b>1-4</b> fabricar: vendas, molotovs, puntas, clavos\n" +
            "<b>H</b> curarte   ·   <b>F</b> linterna   ·   <b>E</b> recoger / abrir / leer / hablar\n" +
            "<b>M</b> mapa   ·   <b>Esc</b> pausa (diario, mapa, logros)   ·   Los <b>altares con velas</b> guardan la partida\n\n" +
            "<i>El revólver hace muchísimo ruido. Los gritones alertan a todos. Los fúngicos son ciegos pero oyen todo.</i>";
        GUI.Label(new Rect(r.x + 28, r.y + 20, r.width - 56, r.height - 30), t, new GUIStyle(sPequeno) { fontSize = 17 });
    }

    private void PanelOpciones(Rect r)
    {
        GUI.DrawTexture(r, fondoPanel);
        GUIStyle et = new GUIStyle(sPequeno) { fontSize = 18 };
        // Columna izquierda: deslizadores
        float x = r.x + 32, y = r.y + 22, w = r.width * 0.52f - 44;
        Opciones.Sensibilidad = Deslizador(x, ref y, w, "Sensibilidad del mouse:  " + Mathf.RoundToInt(Opciones.Sensibilidad * 100f), Opciones.Sensibilidad, 0.04f, 0.3f, et);
        Opciones.VolumenGeneral = Deslizador(x, ref y, w, "Volumen general:  " + Mathf.RoundToInt(Opciones.VolumenGeneral * 100f) + "%", Opciones.VolumenGeneral, 0f, 1f, et);
        Opciones.VolumenMusica = Deslizador(x, ref y, w, "Volumen de la música:  " + Mathf.RoundToInt(Opciones.VolumenMusica * 100f) + "%", Opciones.VolumenMusica, 0f, 1f, et);
        Opciones.Brillo = Deslizador(x, ref y, w, "Brillo:  " + (Opciones.Brillo >= 0f ? "+" : "") + Opciones.Brillo.ToString("0.0"), Opciones.Brillo, -1f, 2f, et);
        Opciones.CampoVision = Mathf.Round(Deslizador(x, ref y, w, "Campo de visión:  " + Mathf.RoundToInt(Opciones.CampoVision) + "°", Opciones.CampoVision, 50f, 80f, et));

        // Columna derecha: interruptores
        float x2 = r.x + r.width * 0.52f + 4, w2 = r.width * 0.48f - 36, y2 = r.y + 26, h = 46, paso = 55;
        if (Boton(new Rect(x2, y2, w2, h), "Invertir eje Y:  " + (Opciones.InvertirY ? "SÍ" : "NO"))) { Opciones.InvertirY = !Opciones.InvertirY; }
        y2 += paso;
        if (Boton(new Rect(x2, y2, w2, h), "Subtítulos:  " + (Opciones.SubtitulosGrandes ? "GRANDES" : "NORMALES"))) { Opciones.SubtitulosGrandes = !Opciones.SubtitulosGrandes; }
        y2 += paso;
        if (Boton(new Rect(x2, y2, w2, h), "Ayudas en pantalla:  " + (Opciones.AyudasEnPantalla ? "SÍ" : "NO"))) { Opciones.AyudasEnPantalla = !Opciones.AyudasEnPantalla; }
        y2 += paso;
        if (Boton(new Rect(x2, y2, w2, h), "Calidad gráfica:  " + Opciones.NombreCalidad())) { Opciones.SiguienteCalidad(); }
        y2 += paso;
        if (Boton(new Rect(x2, y2, w2, h), "Pantalla completa:  " + (Opciones.PantallaCompleta ? "SÍ" : "NO"))) { Opciones.PantallaCompleta = !Opciones.PantallaCompleta; }
        y2 += paso;
        if (Boton(new Rect(x2, y2, w2, h), "Mostrar FPS:  " + (Opciones.MostrarFPS ? "SÍ" : "NO"))) { Opciones.MostrarFPS = !Opciones.MostrarFPS; }
        y2 += paso + 4;
        GUI.Label(new Rect(x2, y2, w2, 60), "<i>Si ves muy oscuro, sube el brillo. Las ayudas muestran el marcador del objetivo y el brillo de los objetos.</i>", new GUIStyle(sMini) { wordWrap = true, fontSize = 13 });
        Opciones.Aplicar();
    }

    private float Deslizador(float x, ref float y, float w, string etiqueta, float valor, float min, float max, GUIStyle et)
    {
        Sombra(new Rect(x, y, w, 28), etiqueta, et);
        valor = GUI.HorizontalSlider(new Rect(x, y + 34, w, 20), valor, min, max);
        y += 84;
        return valor;
    }

    private void PanelLogros(Rect r)
    {
        GUI.DrawTexture(r, fondoPanel);
        Sombra(new Rect(r.x, r.y + 12, r.width, 34), "LOGROS  ·  " + Logros.Cantidad + " de " + Logros.Lista.Length, new GUIStyle(sGrande) { fontSize = 26 });
        float colW = (r.width - 60) / 2f;
        float y0 = r.y + 60;
        for (int i = 0; i < Logros.Lista.Length; i++)
        {
            Logros.Def d = Logros.Lista[i];
            bool tiene = Logros.Tiene(d.id);
            int col = i % 2, fila = i / 2;
            Rect c = new Rect(r.x + 20 + col * (colW + 20), y0 + fila * 57, colW, 50);
            Rect(c, tiene ? new Color(0.25f, 0.18f, 0.06f, 0.7f) : new Color(1f, 1f, 1f, 0.04f));
            Rect(new Rect(c.x, c.y, 5, c.height), tiene ? new Color(0.95f, 0.72f, 0.25f) : new Color(0.3f, 0.3f, 0.3f));
            Sombra(new Rect(c.x + 16, c.y + 3, colW - 20, 24), d.titulo + (tiene ? "" : "   (bloqueado)"), new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 17 }, tiene ? new Color(1f, 0.85f, 0.5f) : new Color(0.55f, 0.55f, 0.58f));
            Sombra(new Rect(c.x + 16, c.y + 25, colW - 20, 22), d.descripcion, new GUIStyle(sMini), tiene ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.45f, 0.45f, 0.48f));
        }
    }

    private void PanelRecords(Rect r)
    {
        GUI.DrawTexture(r, fondoPanel);
        Sombra(new Rect(r.x, r.y + 14, r.width, 34), "RÉCORDS", new GUIStyle(sGrande) { fontSize = 28 });
        float x = r.x + 40, y = r.y + 70, w = r.width - 80;
        Sombra(new Rect(x, y, w, 28), "MODO HISTORIA — Capítulo 1: Resaca", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 19 }, new Color(0.9f, 0.4f, 0.32f));
        y += 34;
        for (int i = 0; i < 3; i++)
        {
            NivelDificultad n = (NivelDificultad)i;
            Sombra(new Rect(x + 10, y, 220, 26), Dificultad.Nombre(n), sPequeno, new Color(0.8f, 0.8f, 0.82f));
            Sombra(new Rect(x + 230, y, w - 230, 26), Records.TextoHistoria(n), sPequeno);
            y += 30;
        }
        y += 24;
        Sombra(new Rect(x, y, w, 28), "MODO SUPERVIVENCIA — Noche en la Plaza Abaroa", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 19 }, new Color(0.9f, 0.4f, 0.32f));
        y += 34;
        var tabla = Records.TablaSupervivencia();
        if (tabla.Count == 0) { Sombra(new Rect(x + 10, y, w, 26), "Todavía no hay récords. ¡Entra a la plaza!", sPequeno, new Color(0.6f, 0.6f, 0.62f)); }
        for (int i = 0; i < tabla.Count; i++)
        {
            Sombra(new Rect(x + 10, y, 60, 26), (i + 1) + ".", sPequeno);
            Sombra(new Rect(x + 60, y, 300, 26), tabla[i].Key + " pts", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold });
            Sombra(new Rect(x + 300, y, 300, 26), "oleada " + tabla[i].Value, sPequeno, new Color(0.75f, 0.75f, 0.78f));
            y += 30;
        }
    }
}
