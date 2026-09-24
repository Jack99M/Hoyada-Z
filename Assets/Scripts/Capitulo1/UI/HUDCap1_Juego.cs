using UnityEngine;

/// <summary>
/// HUD durante el juego: estado de Mateo, armas y municion, fabricacion, modo escucha,
/// indicador de dano, marcadores de deteccion, objetivo, jefe, supervivencia, forcejeo,
/// mira dinamica, avisos, prompts y subtitulos.
/// </summary>
public partial class HUDCap1
{
    // ----- Efectos de pantalla -----

    private void Vinetas()
    {
        GUI.DrawTexture(new Rect(0, 0, W, H), vineta, ScaleMode.StretchToFill, true, 0f, new Color(0f, 0f, 0f, 0.55f), 0f, 0f);
        if (j == null)
        {
            return;
        }
        float golpe = Mathf.Clamp01(1f - (Time.time - j.UltimoDano) / 0.9f) * 0.75f;
        float baja = j.Salud < 40 ? (1f - j.Salud / 40f) * (0.35f + 0.2f * Mathf.Sin(Time.time * 5f)) : 0f;
        float a = Mathf.Max(golpe, baja);
        if (j.Agarrado) { a = Mathf.Max(a, 0.45f + 0.15f * Mathf.Sin(Time.time * 12f)); }
        if (a > 0.01f)
        {
            GUI.DrawTexture(new Rect(0, 0, W, H), vineta, ScaleMode.StretchToFill, true, 0f, new Color(0.55f, 0f, 0f, a), 0f, 0f);
        }
    }

    /// <summary>Flecha roja en el borde de la pantalla hacia donde vino el golpe.</summary>
    private void IndicadorDano()
    {
        if (j == null || Camera.main == null) { return; }
        float desde = Time.time - j.UltimoDano;
        if (desde > 1.2f) { return; }
        Vector3 d = j.OrigenUltimoDano - j.transform.position; d.y = 0f;
        if (d.sqrMagnitude < 0.01f) { return; }
        Vector3 f = Camera.main.transform.forward; f.y = 0f;
        float ang = Vector3.SignedAngle(f, d, Vector3.up);
        float a = Mathf.Clamp01(1.2f - desde);
        Vector2 c = new Vector2(W / 2f, H / 2f);
        Matrix4x4 prev = GUI.matrix;
        GUIUtility.RotateAroundPivot(ang, new Vector2(c.x * escala, c.y * escala));
        Rect(new Rect(c.x - 70, c.y - 250, 140, 10), new Color(0.9f, 0.1f, 0.08f, 0.85f * a));
        Rect(new Rect(c.x - 40, c.y - 262, 80, 8), new Color(0.9f, 0.1f, 0.08f, 0.6f * a));
        GUI.matrix = prev;
    }

    /// <summary>Modo escucha: la pantalla se apaga y se ven los infectados que hacen ruido.</summary>
    private void Escucha(Juego g)
    {
        if (j == null || !j.Escuchando) { return; }
        float t = Mathf.Clamp01(j.TiempoEscuchando * 2.5f);
        Rect(new Rect(0, 0, W, H), new Color(0.02f, 0.04f, 0.08f, 0.55f * t));
        GUI.DrawTexture(new Rect(0, 0, W, H), vineta, ScaleMode.StretchToFill, true, 0f, new Color(0f, 0f, 0f, 0.9f * t), 0f, 0f);
        if (j.TiempoEscuchando < 0.35f) { return; }
        float alcance = Dificultad.AlcanceEscucha;
        foreach (Infectado inf in Infectado.Todos)
        {
            if (inf == null || !inf.HaceRuido) { continue; }
            float dist = Vector3.Distance(inf.transform.position, j.transform.position);
            if (dist > alcance) { continue; }
            Vector2 pie, cabeza;
            if (!APantalla(inf.transform.position, out pie) || !APantalla(inf.transform.position + Vector3.up * inf.AlturaCabeza, out cabeza)) { continue; }
            float alto = Mathf.Max(12f, pie.y - cabeza.y);
            float ancho = alto * 0.32f;
            float alfa = Mathf.Clamp01(1.2f - dist / alcance) * t;
            Color c = inf.Persiguiendo ? new Color(1f, 0.35f, 0.3f, 0.8f * alfa) : new Color(0.9f, 0.95f, 1f, 0.65f * alfa);
            Rect(new Rect(cabeza.x - ancho / 2f, cabeza.y + alto * 0.14f, ancho, alto * 0.5f), c);
            Rect(new Rect(cabeza.x - ancho * 0.4f, cabeza.y + alto * 0.62f, ancho * 0.3f, alto * 0.38f), c);
            Rect(new Rect(cabeza.x + ancho * 0.1f, cabeza.y + alto * 0.62f, ancho * 0.3f, alto * 0.38f), c);
            Circulo(new Vector2(cabeza.x, cabeza.y + alto * 0.07f), alto * 0.1f, c);
            float pulso = (Time.time * 1.3f + inf.GetInstanceID() * 0.13f) % 1f;
            Circulo(new Vector2(cabeza.x, cabeza.y + alto * 0.4f), alto * (0.3f + pulso * 0.6f), new Color(c.r, c.g, c.b, c.a * 0.25f * (1f - pulso)));
            if (inf.tipo == Infectado.Tipo.Fungico || inf.tipo == Infectado.Tipo.Griton || inf.tipo == Infectado.Tipo.Carnicero)
            {
                string n = inf.tipo == Infectado.Tipo.Fungico ? "FÚNGICO" : (inf.tipo == Infectado.Tipo.Griton ? "GRITÓN" : "CARNICERO");
                Sombra(new Rect(cabeza.x - 60, cabeza.y - 26, 120, 20), n, new GUIStyle(sMini) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold }, new Color(1f, 0.8f, 0.4f, alfa));
            }
        }
        Sombra(new Rect(0, H - 120, W, 30), "ESCUCHANDO", new GUIStyle(sCentro) { fontSize = 18, fontStyle = FontStyle.Bold }, new Color(0.7f, 0.85f, 1f, t));
    }

    // ----- Paneles -----

    private void PanelEstado(Juego g)
    {
        if (j == null)
        {
            return;
        }
        float x = 28, y = 26;
        Retrato(new Rect(x, y, 92, 92), RetratoMateo());
        float bx = x + 108;
        Sombra(new Rect(bx, y - 4, 300, 28), "MATEO QUISPE", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold }, new Color(0.85f, 0.87f, 0.95f));
        if (g.modo == Juego.Modo.Historia)
        {
            Sombra(new Rect(bx + 150, y - 4, 150, 28), Dificultad.NombreActual, new GUIStyle(sMini) { alignment = TextAnchor.MiddleRight }, new Color(0.6f, 0.6f, 0.65f));
        }

        Color cSalud = j.Salud > 50 ? new Color(0.75f, 0.15f, 0.12f) : new Color(0.95f, 0.2f, 0.12f);
        Barra(new Rect(bx, y + 24, 260, 16), j.Salud / (float)j.saludMax, cSalud, new Color(0f, 0f, 0f, 0.6f));
        for (int i = 1; i < 4; i++) { Rect(new Rect(bx + 65 * i, y + 24, 1, 16), new Color(0f, 0f, 0f, 0.5f)); }
        Sombra(new Rect(bx + 266, y + 18, 60, 26), j.Salud.ToString(), sPequeno);

        Color cEst = j.Agotado ? new Color(0.9f, 0.4f, 0.1f) : new Color(0.9f, 0.85f, 0.55f);
        Barra(new Rect(bx, y + 46, 260, 7), j.Estamina / j.estaminaMax, cEst, new Color(0f, 0f, 0f, 0.6f));

        float ruido = Mathf.Clamp01(j.NivelRuido / 13f);
        Barra(new Rect(bx, y + 58, 260, 4), ruido, ruido > 0.7f ? new Color(1f, 0.5f, 0.35f, 0.95f) : new Color(0.6f, 0.75f, 0.95f, 0.9f), new Color(0f, 0f, 0f, 0.4f));
        Sombra(new Rect(bx + 266, y + 48, 90, 20), "ruido", new GUIStyle(sPequeno) { fontSize = 13 }, new Color(0.7f, 0.75f, 0.85f));

        float iy = y + 70;
        if (j.Inventario != null && j.Inventario.tieneLinterna && j.Linterna != null)
        {
            if (iconoBateria != null) { GUI.DrawTexture(new Rect(bx, iy, 26, 26), iconoBateria); }
            Color cb = j.Linterna.bateria < 15f ? new Color(1f, 0.35f, 0.3f) : new Color(0.9f, 0.9f, 0.9f);
            Sombra(new Rect(bx + 32, iy + 1, 120, 26), Mathf.CeilToInt(j.Linterna.bateria) + "%" + (j.Linterna.Encendida ? "  ON" : ""), sPequeno, cb);
        }
        if (g.modo == Juego.Modo.Historia)
        {
            if (iconoTiempo != null) { GUI.DrawTexture(new Rect(bx + 150, iy, 26, 26), iconoTiempo); }
            int m = Mathf.FloorToInt(g.minutoReloj);
            Sombra(new Rect(bx + 182, iy + 1, 100, 26), ((m / 60) % 24).ToString("00") + ":" + (m % 60).ToString("00"), sPequeno);
        }

        string estado = null;
        if (j.Escuchando) { estado = "ESCUCHANDO"; }
        else if (j.Agachado) { estado = "AGACHADO"; }
        if (estado != null)
        {
            Sombra(new Rect(x, y + 104, 200, 24), estado, new GUIStyle(sPequeno) { fontSize = 14, fontStyle = FontStyle.Bold }, new Color(0.6f, 0.85f, 0.6f));
        }
        if (Companera.I != null && Companera.I.isActiveAndEnabled && (Companera.I.Siguiendo || Companera.I.estado == Companera.EstadoC.Escondiendose))
        {
            Sombra(new Rect(x, y + 126, 300, 22), "Con Wara", new GUIStyle(sMini) { fontStyle = FontStyle.Bold }, new Color(0.85f, 0.85f, 0.85f, 0.8f));
        }
    }

    private void PanelArmas(Juego g)
    {
        if (j == null || j.Inventario == null)
        {
            return;
        }
        Inventario inv = j.Inventario;
        float w = 440;
        float x = W - w - 28;
        float y = H - 212;
        Rect(new Rect(x - 12, y - 10, w + 24, 200), new Color(0f, 0f, 0f, 0.45f));

        string nombre, detalle = null;
        float barra = -1f;
        switch (inv.ranura)
        {
            case Ranura.Honda:
                nombre = "Honda (warak'a)";
                detalle = "Piedras  " + inv.piedras + " / " + inv.maxPiedras;
                break;
            case Ranura.Revolver:
                nombre = "Revólver .38";
                detalle = inv.balasCargadas + "  |  " + inv.balas;
                break;
            default:
                CombateJugador.DatosArma d = CombateJugador.Datos(inv.arma);
                nombre = d.nombre + (inv.clavos && inv.arma != CombateJugador.Arma.Punos ? " con clavos" : "");
                if (inv.arma != CombateJugador.Arma.Punos) { barra = inv.durabilidad / (float)Mathf.Max(1, inv.DurabilidadMaxima); }
                break;
        }
        Sombra(new Rect(x, y, w, 30), nombre.ToUpper(), new GUIStyle(sTexto) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight });
        if (detalle != null)
        {
            bool vacio = (inv.ranura == Ranura.Honda && inv.piedras == 0) || (inv.ranura == Ranura.Revolver && inv.balasCargadas == 0);
            Sombra(new Rect(x, y + 28, w, 24), detalle, new GUIStyle(sPequeno) { alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Bold }, vacio ? new Color(1f, 0.4f, 0.3f) : new Color(0.95f, 0.9f, 0.75f));
        }
        if (barra >= 0f)
        {
            Barra(new Rect(x + w - 180, y + 34, 180, 6), barra, barra > 0.3f ? new Color(0.85f, 0.85f, 0.85f) : new Color(1f, 0.4f, 0.3f), new Color(0f, 0f, 0f, 0.6f));
        }

        float sx = x + w;
        sx = DibujarRanura(sx, y + 58, "3", ".38", inv.tieneRevolver, inv.ranura == Ranura.Revolver);
        sx = DibujarRanura(sx, y + 58, "2", "HONDA", inv.tieneHonda, inv.ranura == Ranura.Honda);
        DibujarRanura(sx, y + 58, "1", inv.arma == CombateJugador.Arma.Punos ? "PUÑOS" : CombateJugador.Datos(inv.arma).nombre.Split(' ')[0].ToUpper(), true, inv.ranura == Ranura.CuerpoACuerpo);

        float iy = y + 96;
        float ix = x + w;
        ix = Consumible(ix, iy, iconoBateria, null, "Pilas", inv.pilas, inv.maxPilas);
        ix = Consumible(ix, iy, null, "PT", "Puntas", inv.puntas, inv.maxPuntas);
        ix = Consumible(ix, iy, null, "MOL", "Molotov [G]", inv.molotovs, inv.maxMolotovs);
        ix = Consumible(ix, iy, null, "BOT", "Botellas [Q]", inv.botellas, inv.maxBotellas);
        Consumible(ix, iy, iconoMedicina, null, "Vendas [H]", inv.vendas, inv.maxVendas);

        string mat = "Alcohol " + inv.alcohol + "   ·   Trapo " + inv.trapo + "   ·   Cinta " + inv.cinta + "   ·   Cuchilla " + inv.cuchilla + "      [Tab] fabricar";
        Sombra(new Rect(x - 40, y + 156, w + 40, 22), mat, new GUIStyle(sMini) { alignment = TextAnchor.MiddleRight }, new Color(0.75f, 0.78f, 0.7f));

        if (inv.CantidadObjetosClave > 0)
        {
            string claves = string.Join("  ·  ", new System.Collections.Generic.List<string>(inv.NombresObjetosClave).ToArray());
            Sombra(new Rect(x - 400, y - 38, w + 400, 24), claves, new GUIStyle(sDerecha) { fontSize = 15 }, new Color(0.95f, 0.8f, 0.45f));
        }

        CombateJugador cb = j.Combate;
        if (cb != null && cb.Curando)
        {
            Barra(new Rect(W / 2f - 120, H / 2f + 60, 240, 8), cb.ProgresoCura, new Color(0.4f, 0.9f, 0.5f), new Color(0f, 0f, 0f, 0.6f));
            Sombra(new Rect(0, H / 2f + 72, W, 26), "Vendándote...", new GUIStyle(sCentro) { fontSize = 18 });
        }
        if (cb != null && cb.Recargando)
        {
            Barra(new Rect(W / 2f - 120, H / 2f + 60, 240, 8), cb.ProgresoRecarga, new Color(0.95f, 0.8f, 0.4f), new Color(0f, 0f, 0f, 0.6f));
            Sombra(new Rect(0, H / 2f + 72, W, 26), "Recargando...", new GUIStyle(sCentro) { fontSize = 18 });
        }
    }

    private float DibujarRanura(float xDerecha, float y, string tecla, string nombre, bool tiene, bool activa)
    {
        float w = 104;
        Rect r = new Rect(xDerecha - w, y, w, 30);
        Rect(r, activa ? new Color(0.55f, 0.12f, 0.1f, 0.85f) : new Color(0f, 0f, 0f, tiene ? 0.5f : 0.25f));
        if (activa) { Marco(r, new Color(1f, 0.6f, 0.5f, 0.8f), 1f); }
        Color c = tiene ? (activa ? Color.white : new Color(0.8f, 0.8f, 0.82f)) : new Color(0.4f, 0.4f, 0.42f);
        Sombra(new Rect(r.x + 6, r.y + 3, 20, 24), tecla, new GUIStyle(sMini) { fontStyle = FontStyle.Bold }, new Color(0.95f, 0.72f, 0.35f, tiene ? 1f : 0.4f));
        Sombra(new Rect(r.x + 20, r.y + 3, w - 24, 24), tiene ? nombre : "—", new GUIStyle(sMini) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold }, c);
        return xDerecha - w - 8;
    }

    private float Consumible(float xDerecha, float y, Texture2D icono, string sigla, string nombre, int cant, int max)
    {
        float ancho = 84;
        float x = xDerecha - ancho;
        if (icono != null) { GUI.DrawTexture(new Rect(x, y, 28, 28), icono); }
        else { Sombra(new Rect(x - 4, y + 2, 40, 26), sigla, new GUIStyle(sPequeno) { fontSize = 13, fontStyle = FontStyle.Bold }, new Color(0.5f, 0.85f, 0.55f)); }
        Sombra(new Rect(x + 32, y + 2, 60, 26), cant + "/" + max, sPequeno, cant > 0 ? Color.white : new Color(0.6f, 0.6f, 0.6f));
        Sombra(new Rect(x - 4, y + 27, 92, 20), nombre, new GUIStyle(sPequeno) { fontSize = 11 }, new Color(0.7f, 0.7f, 0.75f));
        return x - 4;
    }

    /// <summary>Mochila de fabricacion (mantener Tab).</summary>
    private void PanelCrafteo(Juego g)
    {
        if (j == null || j.Crafteo == null) { return; }
        Crafteo cf = j.Crafteo;
        if (!cf.Abierto && !cf.Fabricando) { return; }
        float w = 520, h = 60 + Crafteo.Recetas.Length * 76;
        Rect r = new Rect(W - w - 40, H / 2f - h / 2f - 60, w, h);
        GUI.DrawTexture(r, fondoPanel);
        Sombra(new Rect(r.x + 20, r.y + 12, w - 40, 30), "MOCHILA — FABRICAR", new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 20 }, new Color(0.95f, 0.72f, 0.35f));
        Sombra(new Rect(r.x + 20, r.y + 12, w - 40, 30), "el tiempo no se detiene", new GUIStyle(sMini) { alignment = TextAnchor.MiddleRight }, new Color(0.7f, 0.5f, 0.45f));
        for (int i = 0; i < Crafteo.Recetas.Length; i++)
        {
            Crafteo.Receta rc = Crafteo.Recetas[i];
            string motivo = cf.Motivo(i);
            bool ok = motivo == null;
            Rect f = new Rect(r.x + 16, r.y + 54 + i * 76, w - 32, 68);
            Rect(f, cf.RecetaActual == i ? new Color(0.35f, 0.2f, 0.05f, 0.8f) : new Color(1f, 1f, 1f, ok ? 0.07f : 0.03f));
            Sombra(new Rect(f.x + 10, f.y + 4, 40, 30), (i + 1).ToString(), new GUIStyle(sTitulo) { fontSize = 24 }, ok ? new Color(0.95f, 0.72f, 0.35f) : new Color(0.45f, 0.45f, 0.45f));
            Sombra(new Rect(f.x + 44, f.y + 4, 250, 26), rc.nombre, new GUIStyle(sPequeno) { fontStyle = FontStyle.Bold, fontSize = 18 }, ok ? Color.white : new Color(0.55f, 0.55f, 0.58f));
            Sombra(new Rect(f.x + 44, f.y + 28, f.width - 50, 20), rc.descripcion, new GUIStyle(sMini), new Color(0.72f, 0.72f, 0.75f));
            string costo = "";
            foreach (string c in rc.costo)
            {
                string[] p = c.Split(':');
                int tengo = j.Inventario.Cantidad(p[0]);
                int necesito = int.Parse(p[1]);
                string col = tengo >= necesito ? "#9FD89F" : "#E07060";
                costo += (costo.Length > 0 ? "  +  " : "") + "<color=" + col + ">" + Inventario.NombreRecurso(p[0], 1) + " " + tengo + "/" + necesito + "</color>";
            }
            GUI.Label(new Rect(f.x + 44, f.y + 46, f.width - 50, 20), costo, new GUIStyle(sMini) { richText = true });
            if (!ok) { Sombra(new Rect(f.x, f.y + 4, f.width - 10, 24), motivo, new GUIStyle(sMini) { alignment = TextAnchor.MiddleRight }, new Color(0.85f, 0.45f, 0.4f)); }
            if (cf.RecetaActual == i && cf.Fabricando)
            {
                Barra(new Rect(f.x, f.yMax - 4, f.width, 4), cf.Progreso, new Color(0.95f, 0.72f, 0.35f), new Color(0f, 0f, 0f, 0.5f));
            }
        }
    }

    private void ObjetivoYAsedio(Juego g)
    {
        if (!string.IsNullOrEmpty(g.Objetivo))
        {
            float desde = Time.unscaledTime - g.ObjetivoCambio;
            if (desde < 6f)
            {
                float a = Mathf.Clamp01(Mathf.Min(desde * 3f, (6f - desde) * 1.5f));
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, a);
                Rect(new Rect(W / 2f - 420, 26, 840, 86), new Color(0f, 0f, 0f, 0.55f));
                Sombra(new Rect(0, 30, W, 26), "NUEVO OBJETIVO", new GUIStyle(sCentro) { fontSize = 16, fontStyle = FontStyle.Bold }, new Color(0.9f, 0.35f, 0.3f));
                Sombra(new Rect(W / 2f - 400, 56, 800, 52), g.Objetivo, new GUIStyle(sCentro) { fontSize = 24 });
                GUI.color = prev;
            }
            else if (Infectado.JefeEnCombate == null && g.modo == Juego.Modo.Historia)
            {
                Sombra(new Rect(W / 2f - 400, 22, 800, 30), g.Objetivo, new GUIStyle(sCentro) { fontSize = 18 }, new Color(0.85f, 0.8f, 0.65f));
            }
        }

        foreach (EventoAsedio ev in FindObjectsByType<EventoAsedio>(FindObjectsSortMode.None))
        {
            if (!ev.Activo) { continue; }
            int s = Mathf.CeilToInt(ev.Restante);
            Sombra(new Rect(0, 120, W, 50), "RESISTE  " + (s / 60) + ":" + (s % 60).ToString("00"), new GUIStyle(sGrande) { fontSize = 40 }, new Color(1f, 0.35f, 0.3f));
        }
    }

    /// <summary>Diamante del objetivo sobre el mundo (con distancia) o en el borde de la pantalla.</summary>
    private void MarcadorObjetivo(Juego g)
    {
        if (!g.HayMarcador || !Opciones.AyudasEnPantalla || j == null || Camera.main == null || g.estado != Juego.Estado.Jugando) { return; }
        if (Time.unscaledTime - g.ObjetivoCambio < 1.5f) { return; }
        Vector3 m = g.Marcador;
        float dist = Vector3.Distance(m, j.transform.position);
        if (dist < 3f) { return; }
        Camera c = Camera.main;
        Vector3 s = c.WorldToScreenPoint(m + Vector3.up * 1.2f);
        Vector2 p = new Vector2(s.x / escala, (Screen.height - s.y) / escala);
        bool detras = s.z < 0f;
        if (detras) { p = new Vector2(W - p.x, H - p.y); }
        bool fuera = detras || p.x < 60 || p.x > W - 60 || p.y < 60 || p.y > H - 60;
        if (fuera)
        {
            Vector2 cc = new Vector2(W / 2f, H / 2f);
            Vector2 d = (p - cc);
            if (d.sqrMagnitude < 1f) { d = Vector2.up; }
            d.Normalize();
            float k = Mathf.Min((W / 2f - 70) / Mathf.Max(0.001f, Mathf.Abs(d.x)), (H / 2f - 70) / Mathf.Max(0.001f, Mathf.Abs(d.y)));
            p = cc + d * k;
        }
        float pulso = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 3f);
        Matrix4x4 prev = GUI.matrix;
        GUIUtility.RotateAroundPivot(45f, new Vector2(p.x * escala, p.y * escala));
        Rect(new Rect(p.x - 9, p.y - 9, 18, 18), new Color(0f, 0f, 0f, 0.5f));
        Marco(new Rect(p.x - 9, p.y - 9, 18, 18), new Color(1f, 0.82f, 0.4f, pulso), 2f);
        Rect(new Rect(p.x - 3, p.y - 3, 6, 6), new Color(1f, 0.82f, 0.4f, pulso));
        GUI.matrix = prev;
        Sombra(new Rect(p.x - 50, p.y + 14, 100, 20), Mathf.RoundToInt(dist) + " m", new GUIStyle(sMini) { alignment = TextAnchor.MiddleCenter }, new Color(1f, 0.9f, 0.7f, pulso));
    }

    private void BarraJefe()
    {
        Infectado b = Infectado.JefeEnCombate;
        if (b == null || b.Muerto) { return; }
        float w = Mathf.Min(900f, W * 0.6f);
        float x = W / 2f - w / 2f, y = 34f;
        Sombra(new Rect(x, y - 6, w, 34), b.nombreJefe, new GUIStyle(sTitulo) { fontSize = 26, alignment = TextAnchor.MiddleLeft }, new Color(0.95f, 0.35f, 0.25f));
        Sombra(new Rect(x, y - 6, w, 34), b.subtituloJefe + (b.JefeFase2 ? "   ·   ENFURECIDO" : ""), new GUIStyle(sMini) { alignment = TextAnchor.MiddleRight }, new Color(0.8f, 0.75f, 0.7f));
        float k = Mathf.Clamp01(b.Vida / Mathf.Max(1f, b.VidaMax));
        Rect(new Rect(x - 3, y + 30, w + 6, 20), new Color(0f, 0f, 0f, 0.8f));
        Barra(new Rect(x, y + 33, w, 14), k, b.JefeFase2 ? new Color(0.9f, 0.2f, 0.1f) : new Color(0.7f, 0.12f, 0.1f), new Color(0.15f, 0.05f, 0.05f, 1f));
        Rect(new Rect(x + w * 0.5f, y + 33, 2, 14), new Color(1f, 1f, 1f, 0.4f));
        float golpe = Mathf.Clamp01(1f - (Time.time - b.JefeUltimoGolpe) * 4f);
        if (golpe > 0f) { Rect(new Rect(x, y + 33, w * k, 14), new Color(1f, 1f, 1f, 0.35f * golpe)); }
        string estado = b.Fase == Infectado.FaseJefe.Aturdido ? "¡ATURDIDO!  ¡PÉGALE!" : (b.Fase == Infectado.FaseJefe.PreparaCarga ? "¡VA A EMBESTIR!  ¡ESQUIVA!" : "");
        if (!string.IsNullOrEmpty(estado))
        {
            Sombra(new Rect(x, y + 52, w, 28), estado, new GUIStyle(sCentro) { fontSize = 20, fontStyle = FontStyle.Bold }, new Color(1f, 0.85f, 0.3f));
        }
    }

    private void HUDSupervivencia(Juego g)
    {
        if (g.modo != Juego.Modo.Supervivencia || g.supervivencia == null) { return; }
        ModoSupervivencia s = g.supervivencia;
        Rect(new Rect(W / 2f - 260, 20, 520, 84), new Color(0f, 0f, 0f, 0.55f));
        Sombra(new Rect(W / 2f - 250, 24, 250, 40), "OLEADA " + s.Oleada, new GUIStyle(sGrande) { fontSize = 30, alignment = TextAnchor.MiddleLeft }, new Color(1f, 0.4f, 0.3f));
        Sombra(new Rect(W / 2f, 24, 250, 40), s.Puntaje + " pts", new GUIStyle(sGrande) { fontSize = 30, alignment = TextAnchor.MiddleRight });
        string linea = s.EnDescanso ? "Siguiente oleada en " + Mathf.CeilToInt(Mathf.Max(0f, s.FinDescanso - Time.time)) + " s  ·  ¡busca suministros!" : "Quedan " + s.Restantes + " infectados  ·  bajas " + s.Bajas;
        Sombra(new Rect(W / 2f - 250, 66, 500, 30), linea, new GUIStyle(sCentro) { fontSize = 17 }, new Color(0.85f, 0.85f, 0.88f));
        float desde = Time.unscaledTime - s.UltimosPuntosT;
        if (desde < 1.2f)
        {
            float a = 1f - desde / 1.2f;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, a);
            Sombra(new Rect(W / 2f - 150, 110 - desde * 20f, 300, 34), "+" + s.UltimosPuntos + (s.Combo > 1 ? "   COMBO x" + s.Combo : ""), new GUIStyle(sCentro) { fontSize = 24, fontStyle = FontStyle.Bold }, new Color(1f, 0.85f, 0.35f));
            GUI.color = prev;
        }
    }

    private void Avisos(Juego g)
    {
        float y = 170;
        foreach (var m in g.Mensajes)
        {
            float resta = m.Value - Time.unscaledTime;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(resta * 2f));
            // Los avisos largos (tutoriales, pistas) se parten en varias lineas en vez de cortarse
            bool pista = m.Key.StartsWith("PISTA:");
            GUIStyle st = new GUIStyle(sPequeno) { fontSize = 17, alignment = TextAnchor.MiddleRight, wordWrap = true };
            float alto = Mathf.Max(28f, st.CalcHeight(new GUIContent(m.Key), 540f));
            Rect(new Rect(W - 580, y, 560, alto + 6), pista ? new Color(0.25f, 0.17f, 0.02f, 0.72f) : new Color(0f, 0f, 0f, 0.5f));
            Sombra(new Rect(W - 570, y + 3, 540, alto), m.Key, st, pista ? new Color(1f, 0.85f, 0.45f) : Color.white);
            GUI.color = prev;
            y += alto + 12;
        }
    }

    private void Prompt(Juego g)
    {
        if (j == null || g.estado != Juego.Estado.Jugando || j.Agarrado)
        {
            return;
        }

        string texto = null;
        Color color = Color.white;
        if (j.Combate != null && j.Combate.ObjetivoSigilo != null)
        {
            texto = j.Combate.TextoSigilo;
            color = new Color(1f, 0.45f, 0.4f);
        }
        else if (j.Actual != null)
        {
            texto = "[E]  " + j.Actual.Prompt;
        }

        if (texto != null)
        {
            float w = 620;
            Rect(new Rect(W / 2f - w / 2f, H - 260, w, 44), new Color(0f, 0f, 0f, 0.6f));
            Sombra(new Rect(W / 2f - w / 2f, H - 258, w, 40), texto, new GUIStyle(sCentro) { fontSize = 22, fontStyle = FontStyle.Bold }, color);
        }
    }

    private void Mira()
    {
        if (!Juego.Control || j == null)
        {
            return;
        }
        CombateJugador cb = j.Combate;
        Vector2 c = new Vector2(W / 2f, H / 2f);
        if (cb != null && cb.Apuntando)
        {
            float sep = 8f + cb.Dispersion * 9f;
            Color col = new Color(1f, 1f, 1f, 0.85f);
            Rect(new Rect(c.x - sep - 12, c.y - 1, 12, 2), col);
            Rect(new Rect(c.x + sep, c.y - 1, 12, 2), col);
            Rect(new Rect(c.x - 1, c.y - sep - 12, 2, 12), col);
            Rect(new Rect(c.x - 1, c.y + sep, 2, 12), col);
            Rect(new Rect(c.x - 1.5f, c.y - 1.5f, 3, 3), col);
        }
        else
        {
            Rect(new Rect(c.x - 2, c.y - 2, 4, 4), new Color(1f, 1f, 1f, 0.5f));
        }
        if (cb != null)
        {
            float desde = Time.time - cb.UltimoImpacto;
            if (desde < 0.25f)
            {
                Color hc = cb.UltimoFueCabeza ? new Color(1f, 0.25f, 0.2f, 1f - desde * 4f) : new Color(1f, 1f, 1f, 1f - desde * 4f);
                Matrix4x4 prev = GUI.matrix;
                GUIUtility.RotateAroundPivot(45f, new Vector2(c.x * escala, c.y * escala));
                Rect(new Rect(c.x - 14, c.y - 1, 9, 2), hc);
                Rect(new Rect(c.x + 5, c.y - 1, 9, 2), hc);
                Rect(new Rect(c.x - 1, c.y - 14, 2, 9), hc);
                Rect(new Rect(c.x - 1, c.y + 5, 2, 9), hc);
                GUI.matrix = prev;
            }
        }
    }

    /// <summary>Forcejeo cuando un infectado agarra a Mateo.</summary>
    private void Agarre(Juego g)
    {
        if (j == null || !j.Agarrado) { return; }
        float w = 520;
        Rect r = new Rect(W / 2f - w / 2f, H * 0.62f, w, 120);
        Rect(r, new Color(0f, 0f, 0f, 0.6f));
        float temblor = Mathf.Sin(Time.unscaledTime * 30f) * 3f;
        Sombra(new Rect(r.x + temblor, r.y + 8, w, 40), "¡MACHACA  [E]!", new GUIStyle(sGrande) { fontSize = 32 }, new Color(1f, 0.85f, 0.3f));
        Barra(new Rect(r.x + 30, r.y + 56, w - 60, 16), j.ProgresoZafarse, new Color(0.95f, 0.8f, 0.3f), new Color(0.2f, 0.05f, 0.05f, 0.9f));
        Barra(new Rect(r.x + 30, r.y + 76, w - 60, 4), 1f - j.TiempoAgarre / Jugador.LimiteAgarre, new Color(0.9f, 0.25f, 0.2f), new Color(0f, 0f, 0f, 0.5f));
        if (j.Inventario != null && j.Inventario.puntas > 0)
        {
            Sombra(new Rect(r.x, r.y + 86, w, 28), "[Clic izquierdo]  Clavar una punta  (tienes " + j.Inventario.puntas + ")", new GUIStyle(sCentro) { fontSize = 18 }, new Color(1f, 0.55f, 0.45f));
        }
    }

    private void Subtitulos(Juego g)
    {
        if (string.IsNullOrEmpty(g.SubTexto))
        {
            return;
        }
        float w = Mathf.Min(Opciones.SubtitulosGrandes ? 1350f : 1100f, W * 0.86f);
        string quien = string.IsNullOrEmpty(g.SubQuien) ? "" : "<b><color=" + ColorHablante(g.SubQuien) + ">" + g.SubQuien + ":</color></b>  ";
        GUIStyle s = new GUIStyle(sCentro) { fontSize = Opciones.SubtitulosGrandes ? 31 : 24 };
        float h = s.CalcHeight(new GUIContent(quien + g.SubTexto), w - 40) + 18;
        float y = g.estado == Juego.Estado.Cinematica ? H - 150 : H - 190;
        Rect(new Rect(W / 2f - w / 2f, y - h / 2f, w, h), new Color(0f, 0f, 0f, 0.55f));
        Sombra(new Rect(W / 2f - w / 2f + 20, y - h / 2f + 9, w - 40, h - 18), quien + g.SubTexto, s);
    }

    private static string ColorHablante(string quien)
    {
        switch (quien)
        {
            case "Mateo": return "#E8B46A";
            case "Wara": return "#9FD0FF";
            case "Bety": return "#F0A0A0";
            case "Radio": return "#B0B0B0";
            case "El Chino": return "#C8E68A";
        }
        return "#D8C8A8";
    }

    private void Marcadores(Juego g)
    {
        if (j == null || g.estado != Juego.Estado.Jugando)
        {
            return;
        }

        foreach (Infectado inf in Infectado.Todos)
        {
            if (inf == null || inf.Muerto || inf.tipo == Infectado.Tipo.Carnicero) { continue; }
            float dist = Vector3.Distance(inf.transform.position, j.transform.position);
            if (dist > 35f) { continue; }
            Vector2 p;
            if (!APantalla(inf.transform.position + Vector3.up * (inf.AlturaCabeza + 0.55f), out p)) { continue; }

            if (inf.Persiguiendo)
            {
                float desde = Time.time - inf.UltimoAviso;
                float a = desde < 2f ? 1f : 0.35f;
                float tam = desde < 0.3f ? 44f + (0.3f - desde) * 80f : 40f;
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, a);
                Sombra(new Rect(p.x - 30, p.y - tam / 2f, 60, tam), "!", new GUIStyle(sMarca) { fontSize = Mathf.RoundToInt(tam) }, new Color(1f, 0.2f, 0.15f));
                GUI.color = prev;
            }
            else if (inf.Deteccion > 0.05f)
            {
                Color c = Color.Lerp(new Color(1f, 0.95f, 0.5f, 0.5f), new Color(1f, 0.6f, 0.1f, 1f), inf.Deteccion);
                Sombra(new Rect(p.x - 30, p.y - 20, 60, 40), "?", sMarca, c);
                Barra(new Rect(p.x - 18, p.y + 18, 36, 4), inf.Deteccion, c, new Color(0f, 0f, 0f, 0.5f));
            }
        }

        if (!Opciones.AyudasEnPantalla) { return; }

        foreach (Interactivo it in Interactivo.Todos)
        {
            if (it == null || !it.destello || !it.Disponible) { continue; }
            float dist = Vector3.Distance(it.Punto, j.transform.position);
            if (dist > 9f) { continue; }
            Vector2 p;
            if (!APantalla(it.Punto + Vector3.up * 0.25f, out p)) { continue; }
            float pulso = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f + it.GetInstanceID());
            float tam = (it == j.Actual ? 16f : 10f) * (1f + (1f - dist / 9f) * 0.4f);
            bool especial = it is Coleccionable;
            Color c = especial ? new Color(1f, 0.8f, 0.3f, pulso * Mathf.Clamp01(1.2f - dist / 9f)) : new Color(1f, 0.95f, 0.8f, pulso * Mathf.Clamp01(1.2f - dist / 9f));
            Matrix4x4 prev = GUI.matrix;
            GUIUtility.RotateAroundPivot(45f, new Vector2(p.x * escala, p.y * escala));
            Rect(new Rect(p.x - tam / 2f, p.y - tam / 2f, tam, tam), c);
            GUI.matrix = prev;
        }
    }
}
