using UnityEngine;

/// <summary>
/// Pantallas: pausa (resumen, diario de archivos, mapa, logros, opciones), mapa [M],
/// lectura de archivos, decisiones, muerte (historia y supervivencia) y final con rango.
/// </summary>
public partial class HUDCap1
{
    private enum TabPausa { Resumen, Diario, Mapa, Logros, Opciones, Controles }
    private TabPausa tab = TabPausa.Resumen;
    private Vector2 scrollDiario;

    // ----- Pausa -----

    private void Pausa(Juego g)
    {
        Rect(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.78f));
        float cx = W / 2f;
        Sombra(new Rect(0, 26, W, 60), "PAUSA", new GUIStyle(sEnorme) { fontSize = 50 });

        string[] nombres = g.modo == Juego.Modo.Historia
            ? new[] { "RESUMEN", "DIARIO", "MAPA", "LOGROS", "OPCIONES", "CONTROLES" }
            : new[] { "RESUMEN", "", "", "LOGROS", "OPCIONES", "CONTROLES" };
        float tw = 170, th = 42;
        int visibles = 0;
        foreach (string n in nombres) { if (n.Length > 0) { visibles++; } }
        float tx = cx - (visibles * (tw + 8)) / 2f;
        for (int i = 0; i < nombres.Length; i++)
        {
            if (nombres[i].Length == 0) { continue; }
            Rect r = new Rect(tx, 96, tw, th);
            bool activo = (int)tab == i;
            GUIStyle st = new GUIStyle(sBoton) { fontSize = 18 };
            if (activo) { st.normal.background = fondoBotonHover; st.normal.textColor = Color.white; }
            if (GUI.Button(r, nombres[i], st))
            {
                tab = (TabPausa)i;
                AudioCap1.Play2D("sfx_click", 0.5f);
            }
            tx += tw + 8;
        }

        Rect area = new Rect(cx - 560, 150, 1120, 640);
        switch (tab)
        {
            case TabPausa.Resumen: PausaResumen(g, area); break;
            case TabPausa.Diario: PausaDiario(g, area); break;
            case TabPausa.Mapa: GUI.DrawTexture(area, fondoPanel); DibujarMapa(g, new Rect(area.x + 10, area.y + 10, area.width - 20, area.height - 20)); break;
            case TabPausa.Logros: PanelLogros(area); break;
            case TabPausa.Opciones: PanelOpciones(new Rect(cx - 420, area.y, 840, 470)); break;
            case TabPausa.Controles: Controles(area); break;
        }

        float by = H - 110;
        float bw = 300;
        if (Boton(new Rect(cx - bw * 1.5f - 16, by, bw, 54), "CONTINUAR")) { Opciones.Guardar(); g.Continuar(); }
        if (Boton(new Rect(cx - bw / 2f, by, bw, 54), g.modo == Juego.Modo.Historia ? "ÚLTIMO PUNTO DE CONTROL" : "REINICIAR")) { g.Reintentar(); }
        if (Boton(new Rect(cx + bw / 2f + 16, by, bw, 54), "MENÚ PRINCIPAL")) { Opciones.Guardar(); g.SalirAlMenu(); }
    }

    private void PausaResumen(Juego g, Rect r)
    {
        GUI.DrawTexture(r, fondoPanel);
        float x = r.x + 30, y = r.y + 22, w = r.width - 60;
        DatosStats d = Stats.D;
        string izq, der;
        if (g.modo == Juego.Modo.Supervivencia && g.supervivencia != null)
        {
            izq = "<b>Modo:</b> Supervivencia (" + Dificultad.NombreActual + ")\n<b>Oleada:</b> " + g.supervivencia.Oleada + "\n<b>Puntaje:</b> " + g.supervivencia.Puntaje + "\n<b>Bajas:</b> " + g.supervivencia.Bajas;
        }
        else
        {
            izq = "<b>Objetivo:</b> " + g.Objetivo + "\n\n" +
                  "<b>Dificultad:</b> " + Dificultad.NombreActual + "\n" +
                  "<b>Tiempo:</b> " + Stats.TiempoTexto(g.TiempoJugado) + "\n" +
                  "<b>Moral:</b> " + g.moral + " / 100\n" +
                  "<b>Archivos:</b> " + g.DocumentosLeidos + " / " + g.DocumentosTotales + "\n" +
                  "<b>Illas de Alasitas:</b> " + d.coleccionables + " / " + g.ColeccionablesTotales + "\n" +
                  "<b>Muertes:</b> " + d.muertes;
        }
        der = "<b>Infectados eliminados:</b> " + d.TotalKills + "\n" +
              "     sigilo " + d.killsSigilo + "  ·  cuerpo a cuerpo " + d.killsMelee + "  ·  honda " + d.killsHonda + "\n" +
              "     revólver " + d.killsRevolver + "  ·  fuego " + d.killsFuego + "\n" +
              "<b>Precisión:</b> " + Mathf.RoundToInt(d.Precision * 100f) + "%   ·   <b>Tiros a la cabeza:</b> " + d.tirosCabeza + "\n" +
              "<b>Daño recibido:</b> " + d.danoRecibido + "   ·   <b>Vendas usadas:</b> " + d.vendasUsadas + "\n" +
              "<b>Objetos fabricados:</b> " + d.crafteados + "\n" +
              "<b>Agarres escapados:</b> " + d.agarresEscapados + " / " + d.agarresSufridos + "   ·   <b>Esquivas:</b> " + d.esquivas + "\n" +
              "<b>Distancia recorrida:</b> " + Mathf.RoundToInt(d.distancia) + " m";
        GUI.Label(new Rect(x, y, w * 0.48f, r.height - 40), izq, new GUIStyle(sPequeno) { fontSize = 18 });
        GUI.Label(new Rect(x + w * 0.52f, y, w * 0.48f, r.height - 40), der, new GUIStyle(sPequeno) { fontSize = 18 });

        if (j != null && j.Inventario != null)
        {
            Inventario inv = j.Inventario;
            string mochila = "<b>MOCHILA</b>\n" +
                "Vendas " + inv.vendas + "/" + inv.maxVendas + "   ·   Botellas " + inv.botellas + "/" + inv.maxBotellas + "   ·   Molotovs " + inv.molotovs + "/" + inv.maxMolotovs + "   ·   Puntas " + inv.puntas + "/" + inv.maxPuntas + "   ·   Pilas " + inv.pilas + "/" + inv.maxPilas + "\n" +
                "Alcohol " + inv.alcohol + "   ·   Trapo " + inv.trapo + "   ·   Cinta " + inv.cinta + "   ·   Cuchilla " + inv.cuchilla +
                (inv.tieneHonda ? "   ·   Piedras " + inv.piedras : "") + (inv.tieneRevolver ? "   ·   Balas " + (inv.balas + inv.balasCargadas) : "") + "\n" +
                "<b>Objetos clave:</b> " + (inv.CantidadObjetosClave > 0 ? string.Join(", ", new System.Collections.Generic.List<string>(inv.NombresObjetosClave).ToArray()) : "ninguno");
            GUI.Label(new Rect(x, r.yMax - 150, w, 140), mochila, new GUIStyle(sPequeno) { fontSize = 17 });
        }
    }

    private void PausaDiario(Juego g, Rect r)
    {
        GUI.DrawTexture(r, fondoPanel);
        Sombra(new Rect(r.x + 24, r.y + 14, r.width, 30), "DIARIO — archivos y hallazgos (" + Diario.Entradas.Count + ")", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 20 }, new Color(0.95f, 0.72f, 0.35f));
        if (Diario.Entradas.Count == 0)
        {
            Sombra(new Rect(r.x + 24, r.y + 60, r.width - 48, 30), "Todavía no encontraste nada que leer.", sPequeno, new Color(0.6f, 0.6f, 0.62f));
            return;
        }
        Rect vista = new Rect(r.x + 20, r.y + 54, r.width - 40, r.height - 70);
        float alto = Diario.Entradas.Count * 52f;
        scrollDiario = GUI.BeginScrollView(vista, scrollDiario, new Rect(0, 0, vista.width - 20, alto));
        for (int i = 0; i < Diario.Entradas.Count; i++)
        {
            Diario.Entrada e = Diario.Entradas[i];
            if (GUI.Button(new Rect(0, i * 52f, vista.width - 24, 46), (e.celular ? "[Celular]  " : "") + e.titulo, new GUIStyle(sBoton) { fontSize = 18, alignment = TextAnchor.MiddleLeft }))
            {
                AudioCap1.Play2D("sfx_click", 0.5f);
                g.Leer(e.titulo, e.texto, e.celular, null, j != null ? j.transform.position : Vector3.zero);
            }
        }
        GUI.EndScrollView();
    }

    // ----- Mapa -----

    private void PantallaMapa(Juego g)
    {
        Rect(new Rect(0, 0, W, H), new Color(0.02f, 0.02f, 0.03f, 0.92f));
        Sombra(new Rect(0, 24, W, 50), "MAPA — SOPOCACHI, LA PAZ", new GUIStyle(sGrande) { fontSize = 32 });
        DibujarMapa(g, new Rect(W / 2f - 520, 90, 1040, H - 180));
        Sombra(new Rect(0, H - 60, W, 30), "[M] o [Esc] cerrar", new GUIStyle(sCentro) { fontSize = 18 }, new Color(0.7f, 0.7f, 0.72f));
    }

    private void DibujarMapa(Juego g, Rect area)
    {
        Rect lim = MapaZonas.Limites;
        float esc = Mathf.Min(area.width * 0.62f / lim.width, area.height / lim.height);
        float mw = lim.width * esc, mh = lim.height * esc;
        Rect mapa = new Rect(area.x + 20, area.y + (area.height - mh) / 2f, mw, mh);
        Rect(mapa, new Color(0.08f, 0.08f, 0.09f, 0.95f));
        Marco(mapa, new Color(0.5f, 0.45f, 0.35f, 0.6f), 2f);

        System.Func<Vector3, Vector2> A = p => new Vector2(mapa.x + (p.x - lim.xMin) * esc, mapa.y + (lim.yMax - p.z) * esc);

        foreach (MapaZonas.Zona z in MapaZonas.Zonas)
        {
            bool visto = g.ZonasVisitadas.Contains(z.nombre);
            Vector2 a = A(new Vector3(z.r.xMin, 0f, z.r.yMax));
            Vector2 b = A(new Vector3(z.r.xMax, 0f, z.r.yMin));
            Rect rz = UnityEngine.Rect.MinMaxRect(a.x, a.y, b.x, b.y);
            Rect(rz, visto ? new Color(z.color.r, z.color.g, z.color.b, 0.75f) : new Color(0.2f, 0.2f, 0.22f, 0.5f));
            Marco(rz, new Color(0f, 0f, 0f, 0.6f), 1f);
            if (rz.width > 40 && rz.height > 18)
            {
                GUIStyle st = new GUIStyle(sMini) { alignment = TextAnchor.MiddleCenter, wordWrap = true, fontSize = 12 };
                Sombra(rz, visto ? z.nombre : "???", st, visto ? Color.white : new Color(0.5f, 0.5f, 0.5f));
            }
        }

        foreach (PuntoGuardado pg in FindObjectsByType<PuntoGuardado>(FindObjectsSortMode.None))
        {
            Vector2 p = A(pg.transform.position);
            Circulo(p, 7f, new Color(1f, 0.85f, 0.35f, 0.95f));
        }

        if (g.HayMarcador)
        {
            Vector2 p = A(g.Marcador);
            float pulso = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 4f);
            Circulo(p, 14f * pulso + 6f, new Color(1f, 0.3f, 0.2f, 0.35f));
            Circulo(p, 6f, new Color(1f, 0.3f, 0.2f, 1f));
        }

        if (j != null)
        {
            Vector2 p = A(j.transform.position);
            Matrix4x4 prev = GUI.matrix;
            GUIUtility.RotateAroundPivot(j.transform.eulerAngles.y, new Vector2(p.x * escala, p.y * escala));
            Rect(new Rect(p.x - 6, p.y - 4, 12, 12), new Color(0.3f, 0.8f, 1f));
            Rect(new Rect(p.x - 3, p.y - 12, 6, 9), new Color(0.3f, 0.8f, 1f));
            GUI.matrix = prev;
        }
        if (Companera.I != null && Companera.I.isActiveAndEnabled && Companera.I.Siguiendo)
        {
            Circulo(A(Companera.I.transform.position), 5f, new Color(0.9f, 0.9f, 0.9f));
        }

        float lx = mapa.xMax + 30, ly = mapa.y + 10;
        float lw = area.xMax - lx - 10;
        GUIStyle ls = new GUIStyle(sPequeno) { fontSize = 16 };
        Sombra(new Rect(lx, ly, lw, 28), "LEYENDA", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold }, new Color(0.95f, 0.72f, 0.35f));
        ly += 36;
        Rect(new Rect(lx, ly + 6, 12, 12), new Color(0.3f, 0.8f, 1f)); Sombra(new Rect(lx + 22, ly, lw, 26), "Mateo", ls); ly += 30;
        Circulo(new Vector2(lx + 6, ly + 12), 6f, new Color(1f, 0.3f, 0.2f)); Sombra(new Rect(lx + 22, ly, lw, 26), "Objetivo", ls); ly += 30;
        Circulo(new Vector2(lx + 6, ly + 12), 6f, new Color(1f, 0.85f, 0.35f)); Sombra(new Rect(lx + 22, ly, lw, 26), "Altar (guardar)", ls); ly += 30;
        Circulo(new Vector2(lx + 6, ly + 12), 5f, new Color(0.9f, 0.9f, 0.9f)); Sombra(new Rect(lx + 22, ly, lw, 26), "Wara", ls); ly += 44;
        string zona = j != null ? MapaZonas.ZonaEn(j.transform.position) : null;
        Sombra(new Rect(lx, ly, lw, 28), "Estás en:", ls, new Color(0.7f, 0.7f, 0.72f)); ly += 26;
        Sombra(new Rect(lx, ly, lw, 30), zona ?? "—", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 20 }); ly += 44;
        if (!string.IsNullOrEmpty(g.Objetivo))
        {
            Sombra(new Rect(lx, ly, lw, 26), "Objetivo:", ls, new Color(0.7f, 0.7f, 0.72f)); ly += 26;
            GUI.Label(new Rect(lx, ly, lw, 120), g.Objetivo, new GUIStyle(sPequeno) { fontSize = 16 });
            ly += 120;
        }
        Sombra(new Rect(lx, ly, lw, 60), "Zonas descubiertas: " + g.ZonasVisitadas.Count + " / " + MapaZonas.Zonas.Length, ls, new Color(0.7f, 0.7f, 0.72f));
    }

    // ----- Lectura y decisiones -----

    private void Lectura(Juego g)
    {
        Rect(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.75f));
        if (g.LecturaCelular)
        {
            float w = 480, h = 820;
            Rect r = new Rect(W / 2f - w / 2f, H / 2f - h / 2f, w, h);
            Rect(r, new Color(0.02f, 0.02f, 0.03f, 1f));
            Rect(new Rect(r.x + 14, r.y + 60, w - 28, h - 120), new Color(0.07f, 0.09f, 0.12f, 1f));
            Sombra(new Rect(r.x, r.y + 16, w, 30), g.LecturaTitulo, new GUIStyle(sCentro) { fontSize = 20, fontStyle = FontStyle.Bold });
            GUI.Label(new Rect(r.x + 32, r.y + 76, w - 64, h - 150), g.LecturaTexto, new GUIStyle(sPequeno) { fontSize = 19 });
            Sombra(new Rect(r.x, r.y + h - 48, w, 30), "[E] guardar el celular", new GUIStyle(sCentro) { fontSize = 16 }, new Color(0.7f, 0.7f, 0.75f));
        }
        else
        {
            float w = 860, h = 800;
            Rect r = new Rect(W / 2f - w / 2f, H / 2f - h / 2f, w, h);
            GUI.DrawTexture(r, fondoPapel);
            GUI.DrawTexture(r, vineta, ScaleMode.StretchToFill, true, 0f, new Color(0.35f, 0.25f, 0.1f, 0.35f), 0f, 0f);
            GUI.Label(new Rect(r.x + 50, r.y + 36, w - 100, 50), g.LecturaTitulo, sPapelTitulo);
            GUI.Label(new Rect(r.x + 50, r.y + 96, w - 100, h - 150), g.LecturaTexto, sPapel);
            Sombra(new Rect(r.x, r.y + h + 10, w, 30), "[E] cerrar", new GUIStyle(sCentro) { fontSize = 18 });
        }
    }

    private void Eleccion(Juego g)
    {
        Rect(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.55f));
        Texture2D ret = RetratoPorNombre(g.EleccionRetrato);
        float w = 1060;
        GUIStyle st = new GUIStyle(sTexto);
        float tw0 = w - (ret != null ? 260 : 60);
        float altoTexto = Mathf.Max(170f, st.CalcHeight(new GUIContent(g.EleccionTexto), tw0) + 10f);
        float h = 90 + altoTexto + g.Opciones.Count * 62;
        Rect r = new Rect(W / 2f - w / 2f, Mathf.Max(20f, H / 2f - h / 2f), w, h);
        GUI.DrawTexture(r, fondoPanel);
        Marco(r, new Color(1f, 1f, 1f, 0.08f), 1f);
        float tx = r.x + 30;
        if (ret != null)
        {
            Retrato(new Rect(r.x + 30, r.y + 30, 180, 180), ret);
            tx = r.x + 240;
        }
        float tw = r.xMax - 30 - tx;
        GUI.Label(new Rect(tx, r.y + 22, tw, 40), g.EleccionTitulo, sTitulo);
        GUI.Label(new Rect(tx, r.y + 68, tw, altoTexto), g.EleccionTexto, st);

        float y = r.y + 80 + altoTexto;
        for (int i = 0; i < g.Opciones.Count; i++)
        {
            Juego.Opcion op = g.Opciones[i];
            if (Boton(new Rect(r.x + 30, y, w - 60, 54), (i + 1) + ".  " + op.texto, !op.deshabilitada))
            {
                g.Elegir(i);
                break;
            }
            y += 62;
        }
    }

    // ----- Muerte y final -----

    private void Muerte(Juego g)
    {
        float t = g.TiempoEstado;
        Rect(new Rect(0, 0, W, H), new Color(0.35f, 0f, 0f, Mathf.Clamp01(t * 0.6f) * 0.5f));
        if (t < 1.8f)
        {
            return;
        }
        Color prev = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01((t - 1.8f) * 1.5f));
        bool sup = g.modo == Juego.Modo.Supervivencia && g.supervivencia != null;
        Sombra(new Rect(0, H * 0.22f, W, 90), sup ? "LA PLAZA TE DEVORÓ" : "TE ALCANZARON", sEnorme, new Color(0.85f, 0.15f, 0.12f));
        Sombra(new Rect(W / 2f - 460, H * 0.32f, 920, 70), g.MotivoMuerte, new GUIStyle(sCentro) { fontSize = 22 });
        float by = H * 0.5f;
        if (sup)
        {
            ModoSupervivencia s = g.supervivencia;
            Sombra(new Rect(0, H * 0.4f, W, 40), "Oleada " + s.Oleada + "   ·   " + s.Puntaje + " puntos   ·   " + s.Bajas + " bajas", new GUIStyle(sGrande) { fontSize = 28 });
            if (s.NuevoRecord) { Sombra(new Rect(0, H * 0.45f, W, 30), "¡NUEVO RÉCORD!", new GUIStyle(sCentro) { fontSize = 24, fontStyle = FontStyle.Bold }, new Color(1f, 0.85f, 0.3f)); }
            by = H * 0.55f;
        }
        GUI.color = prev;
        if (t > 2.4f)
        {
            if (Boton(new Rect(W / 2f - 220, by, 440, 58), sup ? "OTRA NOCHE" : "REINTENTAR (PUNTO DE CONTROL)")) { g.Reintentar(); }
            float y2 = by + 72;
            if (!sup && Guardado.Existe)
            {
                if (Boton(new Rect(W / 2f - 220, y2, 440, 50), "CARGAR PARTIDA GUARDADA")) { g.ContinuarPartida(); }
                y2 += 62;
            }
            if (Boton(new Rect(W / 2f - 220, y2, 440, 50), "MENÚ PRINCIPAL")) { g.SalirAlMenu(); }
        }
    }

    private void Fin(Juego g)
    {
        Rect(new Rect(0, 0, W, H), Color.black);
        if (fondoMenu != null)
        {
            GUI.DrawTexture(new Rect(0, 0, W, H), fondoMenu, ScaleMode.ScaleAndCrop, false, 0f, new Color(1f, 1f, 1f, 0.22f), 0f, 0f);
        }
        float cx = W / 2f;
        Sombra(new Rect(0, H * 0.04f, W, 50), "FIN DEL " + g.tituloCapitulo.ToUpper(), new GUIStyle(sGrande) { fontSize = 36 });
        Sombra(new Rect(0, H * 0.09f, W, 36), "Continuará...", new GUIStyle(sCentro) { fontSize = 22, fontStyle = FontStyle.Italic }, new Color(0.8f, 0.8f, 0.85f));

        float t = g.TiempoEstado;
        string rango = string.IsNullOrEmpty(g.RangoFinal) ? "?" : g.RangoFinal;
        Color cr = rango == "S" ? new Color(1f, 0.82f, 0.3f) : (rango == "A" ? new Color(0.6f, 0.9f, 0.6f) : (rango == "B" ? new Color(0.6f, 0.8f, 1f) : new Color(0.85f, 0.6f, 0.55f)));
        float aparece = Mathf.Clamp01((t - 0.6f) * 2f);
        float tam = Mathf.Lerp(260f, 150f, Mathf.SmoothStep(0f, 1f, aparece));
        Color prev = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, aparece);
        Sombra(new Rect(cx - 520, H * 0.17f, 300, 200), rango, new GUIStyle(sEnorme) { fontSize = Mathf.RoundToInt(tam) }, cr);
        Sombra(new Rect(cx - 520, H * 0.17f + 190, 300, 30), "RANGO   ·   " + g.PuntajeFinal + " / 100", new GUIStyle(sCentro) { fontSize = 18, fontStyle = FontStyle.Bold }, cr);
        Sombra(new Rect(cx - 560, H * 0.17f + 222, 380, 30), "\"" + Stats.Titulo() + "\"", new GUIStyle(sCentro) { fontSize = 20, fontStyle = FontStyle.Italic });
        GUI.color = prev;

        DatosStats d = Stats.D;
        string stats =
            "<b>Dificultad:</b> " + Dificultad.NombreActual + "\n" +
            "<b>Tiempo:</b> " + Stats.TiempoTexto(g.TiempoJugado) + "     <b>Muertes:</b> " + d.muertes + "\n" +
            "<b>Infectados eliminados:</b> " + d.TotalKills + "  (sigilo " + d.killsSigilo + ", honda " + d.killsHonda + ", revólver " + d.killsRevolver + ", fuego " + d.killsFuego + ")\n" +
            "<b>Precisión:</b> " + Mathf.RoundToInt(d.Precision * 100f) + "%     <b>Tiros a la cabeza:</b> " + d.tirosCabeza + "\n" +
            "<b>El Carnicero:</b> " + (d.jefeDerrotado ? "derrotado en " + Stats.TiempoTexto(d.tiempoJefe) : "—") + "\n" +
            "<b>Archivos:</b> " + g.DocumentosLeidos + " / " + g.DocumentosTotales + "     <b>Illas:</b> " + d.coleccionables + " / " + g.ColeccionablesTotales + "\n" +
            "<b>Objetos fabricados:</b> " + d.crafteados + "     <b>Agarres escapados:</b> " + d.agarresEscapados + "\n" +
            "<b>Moral final:</b> " + g.moral + "     <b>Le dijiste la verdad a Bety:</b> " + (g.FueHonesto ? "sí" : "no");
        Rect r = new Rect(cx - 150, H * 0.17f, 680, 330);
        GUI.DrawTexture(r, fondoPanel);
        GUI.Label(new Rect(r.x + 26, r.y + 18, r.width - 52, r.height - 30), stats, new GUIStyle(sTexto) { fontSize = 19 });

        Sombra(new Rect(0, H * 0.52f, W, 30), "Logros: " + Logros.Cantidad + " de " + Logros.Lista.Length + "   ·   Mejor resultado en " + Dificultad.NombreActual + ": " + Records.TextoHistoria(Dificultad.Nivel), new GUIStyle(sCentro) { fontSize = 17 }, new Color(0.8f, 0.8f, 0.82f));

        if (Boton(new Rect(cx - 200, H * 0.62f, 400, 58), "JUGAR DE NUEVO")) { g.JugarDeNuevo(); }
        if (Boton(new Rect(cx - 200, H * 0.62f + 72, 400, 50), "MENÚ PRINCIPAL")) { g.SalirAlMenu(); }
        Sombra(new Rect(0, H - 60, W, 30), "Gracias por jugar Hoyada Z: Ecos del Altiplano", new GUIStyle(sCentro) { fontSize = 16 }, new Color(0.6f, 0.6f, 0.65f));
    }
}
