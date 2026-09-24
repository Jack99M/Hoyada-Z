using UnityEngine;

/// <summary>
/// Zonas del Capitulo 1 (coordenadas en metros, +Z = norte):
/// Discoteca "Altura" -> Callejon -> Av. 20 de Octubre y Plaza Abaroa -> Farmacia ->
/// Mercado Sopocachi -> Gradas -> Calle de doña Bety (asedio final).
/// </summary>
public static partial class ConstructorCapitulo1
{
    // =====================================================================
    // ZONA 1: DISCOTECA "ALTURA"  (x -30..-8, z -18..6)
    // =====================================================================
    private static void Discoteca(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z1_Discoteca").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        // Piso, techo y paredes exteriores
        Bloque("Disco_Piso", new Vector3(-30f, -0.2f, -18f), new Vector3(-8f, 0f, 6f), "mat_concreto");
        Bloque("Disco_Pista", new Vector3(-20f, 0f, -15f), new Vector3(-11f, 0.015f, -4f), "mat_pista", false);
        Bloque("Disco_Techo", new Vector3(-30.15f, 4.5f, -18.15f), new Vector3(-7.85f, 4.8f, 6.15f), "mat_negro");
        Pared("Disco_ParedS", new Vector3(-30.15f, 0f, -18f), new Vector3(-7.85f, 0f, -18f), 4.5f, 0.3f, "mat_ladrillo");
        Pared("Disco_ParedN", new Vector3(-30.15f, 0f, 6f), new Vector3(-7.85f, 0f, 6f), 4.5f, 0.3f, "mat_ladrillo");
        Pared("Disco_ParedO", new Vector3(-30f, 0f, -18f), new Vector3(-30f, 0f, 6f), 4.5f, 0.3f, "mat_ladrillo", Hueco.Puerta(20f, 1.4f, 2.4f));
        Pared("Disco_ParedE", new Vector3(-8f, 0f, -18f), new Vector3(-8f, 0f, 6f), 4.5f, 0.3f, "mat_ladrillo", Hueco.Puerta(12f, 4f, 3.2f));
        // Tabiques internos: banos y deposito
        Pared("Disco_Tabique", new Vector3(-26f, 0f, -18f), new Vector3(-26f, 0f, 6f), 4.5f, 0.2f, "mat_revoque_gris", Hueco.Puerta(6f, 1.4f, 2.4f), Hueco.Puerta(18f, 1.2f, 2.3f));
        Pared("Disco_TabiqueBD", new Vector3(-30f, 0f, -8f), new Vector3(-26f, 0f, -8f), 4.5f, 0.2f, "mat_revoque_gris");
        // Segundo piso (exterior) y fachada hacia la avenida
        Edificio("Disco_PisoSuperior", new Vector3(-30.15f, 4.8f, -18.15f), new Vector3(-7.85f, 10f, 6.15f), "mat_ladrillo", false, false, true, true, false, true);
        Caja("Disco_Letrero", new Vector3(-7.7f, 4.1f, -6f), new Vector3(0.12f, 0.6f, 5f), "mat_neon_magenta", false);
        Luz("Disco_LuzLetrero", new Vector3(-6.8f, 4.1f, -6f), new Color(1f, 0.2f, 0.7f), 3f, 9f, luces, LuzParpadeante.Modo.Falla);

        // Barra
        Caja("Barra", new Vector3(-18f, 0.55f, 2f), new Vector3(8f, 1.1f, 0.8f), "mat_madera");
        Caja("Barra_Cubierta", new Vector3(-18f, 1.125f, 2f), new Vector3(8.2f, 0.05f, 0.95f), "mat_metal_oscuro", false);
        Caja("Barra_Estante", new Vector3(-18f, 1.2f, 5.55f), new Vector3(8f, 2.4f, 0.6f), "mat_madera");
        for (int i = 0; i < 26; i++)
        {
            float x = -21.8f + i * 0.3f;
            string m = i % 3 == 0 ? "mat_farmacia_verde" : (i % 3 == 1 ? "mat_vidrio" : "mat_toldo_rojo");
            Caja("BotellaEstante", new Vector3(x, 1.55f + (i % 2) * 0.7f, 5.4f), new Vector3(0.08f, 0.3f, 0.08f), m, false);
        }
        Caja("Neon_Barra", new Vector3(-18f, 3.4f, 5.8f), new Vector3(6f, 0.18f, 0.06f), "mat_neon_cian", false);
        Luz("Luz_Barra", new Vector3(-18f, 3.6f, 3.4f), new Color(0.3f, 0.8f, 1f), 3.5f, 9f, luces, LuzParpadeante.Modo.Falla);
        for (int i = 0; i < 5; i++)
        {
            bool caido = i == 1 || i == 3;
            Caja("Banquito", new Vector3(-21f + i * 1.5f, caido ? 0.2f : 0.38f, caido ? 0.7f : 1.15f), caido ? new Vector3(0.4f, 0.4f, 0.75f) : new Vector3(0.4f, 0.75f, 0.4f), "mat_metal_oscuro", true, i * 23f);
        }

        // DJ, parlantes y pista
        Caja("DJ_Tarima", new Vector3(-24f, 0.3f, -7.7f), new Vector3(4f, 0.6f, 5.4f), "mat_negro");
        Caja("DJ_Escalon", new Vector3(-21.7f, 0.15f, -6.2f), new Vector3(0.6f, 0.3f, 1.6f), "mat_metal_oscuro");
        Caja("DJ_Mesa", new Vector3(-23f, 1.1f, -8.4f), new Vector3(0.8f, 1f, 3f), "mat_metal_oscuro");
        Caja("DJ_Pantalla", new Vector3(-23.4f, 1.8f, -8.4f), new Vector3(0.05f, 0.5f, 0.8f), "mat_pantalla", false);
        Caja("Parlante_S", new Vector3(-25.2f, 1.3f, -13.8f), new Vector3(1f, 2.6f, 1f), "mat_negro");
        Caja("Parlante_N", new Vector3(-25.2f, 1.3f, -4.2f), new Vector3(1f, 2.6f, 1f), "mat_negro");
        Sonido("Parlante_Musica", new Vector3(-25.2f, 1.8f, -4.2f), "amb_disco", 0.45f, 30f, raiz);
        Luz("Estrobo", new Vector3(-15.5f, 4.2f, -9.5f), Color.white, 9f, 15f, luces, LuzParpadeante.Modo.Estrobo);
        var c1 = Luz("Luces_Pista_A", new Vector3(-18f, 4.1f, -12f), new Color(1f, 0.1f, 0.8f), 5f, 11f, luces, LuzParpadeante.Modo.Colores);
        c1.GetComponent<LuzParpadeante>().colores = new[] { new Color(1f, 0.1f, 0.8f), new Color(0.2f, 0.3f, 1f), new Color(0.1f, 1f, 0.9f) };
        var c2 = Luz("Luces_Pista_B", new Vector3(-13f, 4.1f, -6f), new Color(0.2f, 0.3f, 1f), 5f, 11f, luces, LuzParpadeante.Modo.Colores);
        c2.GetComponent<LuzParpadeante>().colores = new[] { new Color(0.1f, 1f, 0.9f), new Color(1f, 0.1f, 0.8f), new Color(0.6f, 0.1f, 1f) };
        Caja("Neon_Pista_1", new Vector3(-20.1f, 0.03f, -9.5f), new Vector3(0.08f, 0.04f, 11f), "mat_neon_magenta", false);
        Caja("Neon_Pista_2", new Vector3(-10.9f, 0.03f, -9.5f), new Vector3(0.08f, 0.04f, 11f), "mat_neon_cian", false);

        // Mesas (algunas volcadas)
        Vector3[] mesas = { new Vector3(-13f, 0f, -16f), new Vector3(-10f, 0f, -12f), new Vector3(-21f, 0f, -16.5f), new Vector3(-12.5f, 0f, -2f), new Vector3(-22f, 0f, -1f) };
        for (int i = 0; i < mesas.Length; i++)
        {
            bool volcada = i % 2 == 0;
            Caja("Mesa", mesas[i] + Vector3.up * (volcada ? 0.4f : 0.38f), volcada ? new Vector3(0.8f, 0.8f, 0.06f) : new Vector3(0.8f, 0.76f, 0.8f), "mat_madera", true, i * 37f);
        }

        // VIP donde despierta Mateo
        Caja("VIP_Sofa_E", new Vector3(-9.1f, 0.35f, 2f), new Vector3(1.2f, 0.7f, 6f), "mat_tela_sofa");
        Caja("VIP_Respaldo_E", new Vector3(-8.5f, 0.95f, 2f), new Vector3(0.3f, 0.9f, 6f), "mat_tela_sofa");
        Caja("VIP_Sofa_N", new Vector3(-12.5f, 0.35f, 5.2f), new Vector3(5f, 0.7f, 1.2f), "mat_tela_sofa");
        Caja("VIP_Respaldo_N", new Vector3(-12.5f, 0.95f, 5.75f), new Vector3(5f, 0.9f, 0.3f), "mat_tela_sofa");
        Caja("VIP_Mesita", new Vector3(-12.6f, 0.25f, 2.4f), new Vector3(1.2f, 0.5f, 0.8f), "mat_madera");
        Luz("VIP_Luz", new Vector3(-11f, 3.9f, 2.5f), new Color(1f, 0.3f, 0.35f), 2.2f, 7f, luces);
        Luz("Disco_Relleno_1", new Vector3(-17f, 4.2f, -2f), new Color(0.55f, 0.35f, 0.9f), 1.6f, 12f, luces);
        Luz("Disco_Relleno_2", new Vector3(-12f, 4.2f, -14f), new Color(0.9f, 0.3f, 0.5f), 1.4f, 11f, luces);
        Luz("Disco_Relleno_3", new Vector3(-21f, 4.2f, -15f), new Color(0.3f, 0.5f, 0.9f), 1.2f, 10f, luces);

        // Primer cadaver en la pista y sangre
        CrearNPC("Rig_Infectado", "Cadaver_Pista", new Vector3(-14.2f, 0.02f, -10.8f), 35f, "mat_char_civil", PoseNPC.Pose.Cadaver, false, pers, AnimProcedural.Estilo.Humano);
        Sangre(new Vector3(-14f, 0f, -11.5f), 1.6f, raiz);
        Sangre(new Vector3(-16.5f, 0f, -12.2f), 0.6f, raiz);
        Sangre(new Vector3(-24.5f, 0f, -12f), 0.9f, raiz);
        Sangre(new Vector3(-22.8f, 0f, -11.6f), 0.5f, raiz);

        // Celular, nota y botella en la barra
        var cel = Doc("Celular_Mateo", new Vector3(-19.6f, 1.16f, 2f), "Celular de Mateo — 7 mensajes nuevos", TextoCelular, raiz, true, true);
        cel.alLeer = new Acciones
        {
            objetivo = "Sal de la discoteca",
            subtitulos = new[] { "Mateo|...Esto tiene que ser una broma.", "Mateo|Tengo que llegar donde doña Bety." }
        };
        Doc("Nota_Checo", new Vector3(-15.2f, 1.16f, 1.95f), "Nota para Rami", TextoNotaDJ, raiz);
        Objeto("Botella_Barra", new Vector3(-16.4f, 1.15f, 2.1f), Recogible.TipoObjeto.Botella, 1, raiz);
        Objeto("Palo_DJ", new Vector3(-21.3f, 0.02f, -8.6f), Recogible.TipoObjeto.Palo, 1, raiz, null, "Pata de mesa rota");

        // Cortina metalica principal (trabada)
        var cortina = Puerta("Cortina_Principal", new Vector3(-8f, 0f, -6f), 90f, 4f, 3.2f, PuertaCap.Tipo.Cortina, "mat_cortina", raiz);
        cortina.trabada = true;
        cortina.mensajeTrabada = "La cortina está bajada y con candado por fuera. Nos encerraron... o encerraron algo.";
        Luz("Luz_Emergencia", new Vector3(-8.8f, 3.8f, -6f), new Color(1f, 0.1f, 0.05f), 2f, 7f, luces, LuzParpadeante.Modo.Falla);

        // Banos
        Bloque("Banos_Piso", new Vector3(-30f, 0f, -18f), new Vector3(-26f, 0.015f, -8f), "mat_baldosa", false);
        Caja("Lavamanos", new Vector3(-28f, 0.85f, -8.45f), new Vector3(3.4f, 0.15f, 0.6f), "mat_baldosa");
        Caja("Espejo", new Vector3(-28f, 1.8f, -8.13f), new Vector3(3f, 1f, 0.03f), "mat_vidrio", false);
        Caja("Cubiculo_1", new Vector3(-28.6f, 1.05f, -16.9f), new Vector3(0.05f, 2.1f, 2.2f), "mat_metal_pintado");
        Caja("Cubiculo_2", new Vector3(-27.2f, 1.05f, -16.9f), new Vector3(0.05f, 2.1f, 2.2f), "mat_metal_pintado");
        Luz("Banos_Fluorescente", new Vector3(-28f, 4.1f, -13f), new Color(0.8f, 0.95f, 1f), 2.6f, 8f, luces, LuzParpadeante.Modo.Falla);
        Sangre(new Vector3(-28.4f, 0f, -14.8f), 1.8f, raiz);
        Sangre(new Vector3(-27f, 0f, -12.3f), 0.7f, raiz);
        CrearNPC("Rig_Infectado", "Cadaver_Banos", new Vector3(-28.8f, 0.02f, -15.6f), 100f, "mat_char_civil", PoseNPC.Pose.Cadaver, false, pers, AnimProcedural.Estilo.Humano);

        var guardia = CrearInfectado("Infectado_Guardia", new Vector3(-28.4f, 0f, -14.4f), 200f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 3f);
        var llave = Objeto("Llave_Deposito", guardia.transform.position, Recogible.TipoObjeto.ObjetoClave, 1, raiz, "llave_deposito", "Llavero del guardia");
        llave.alRecoger = new Acciones
        {
            objetivo = "Abre el depósito y sal por la puerta de atrás",
            subtitulos = new[] { "Mateo|Perdón, don Ramiro... perdón." }
        };
        llave.gameObject.SetActive(false);
        guardia.soltarAlMorir = llave.gameObject;
        guardia.alMorir = new Acciones { subtitulos = new[] { "Mateo|¿Qué... qué le pasó? Tenía los ojos negros...", "Mateo|Tiene su llavero en el cinturón." } };

        // Deposito
        var puertaDep = Puerta("Puerta_Deposito", new Vector3(-26f, 0f, 0f), 90f, 1.2f, 2.3f, PuertaCap.Tipo.Bisagra, "mat_madera", raiz);
        puertaDep.llaveRequerida = "llave_deposito";
        puertaDep.nombreLlave = "Llavero del guardia";
        puertaDep.mensajeCerrada = "El depósito está cerrado con llave. Don Ramiro, el guardia, siempre carga el llavero.";
        puertaDep.angulo = -100f;
        Caja("Salida_Letrero", new Vector3(-26.12f, 2.65f, 0f), new Vector3(0.05f, 0.25f, 0.6f), "mat_neon_verde", false);
        Luz("Salida_Luz", new Vector3(-25.6f, 2.6f, 0f), new Color(0.1f, 1f, 0.3f), 1.2f, 4f, luces);
        // 4 estantes: el quinto tapaba la puerta trasera (salida al callejon)
        for (int i = 0; i < 4; i++)
        {
            Caja("Deposito_Estante_" + i, new Vector3(-29.55f, 1.2f, -6.5f + i * 2.2f), new Vector3(0.6f, 2.4f, 1.9f), "mat_metal_oscuro");
        }
        Caja("Deposito_Cajas_1", new Vector3(-27f, 0.45f, 1.3f), new Vector3(0.9f, 0.9f, 0.9f), "mat_carton");
        Caja("Deposito_Cajas_2", new Vector3(-27.6f, 0.4f, -4.6f), new Vector3(0.8f, 0.8f, 1.2f), "mat_carton");
        Caja("Deposito_Cajas_3", new Vector3(-27.4f, 1.2f, -4.6f), new Vector3(0.6f, 0.7f, 0.6f), "mat_carton");
        var linterna = Objeto("Linterna_DJ", new Vector3(-27f, 0.93f, 1.3f), Recogible.TipoObjeto.Linterna, 1, raiz);
        linterna.alRecoger = new Acciones
        {
            mensaje = "[F] encender / apagar la linterna   ·   [R] cambiar pilas",
            subtitulos = new[] { "Mateo|La linterna del DJ. Me va a servir." }
        };
        Objeto("Pilas_Deposito", new Vector3(-27.85f, 0.83f, -4.15f), Recogible.TipoObjeto.Pilas, 1, raiz);
        Luz("Deposito_LuzRoja", new Vector3(-28f, 4f, -2f), new Color(1f, 0.15f, 0.1f), 0.8f, 7f, luces);
        Caja("Salida_Letrero_2", new Vector3(-29.8f, 2.8f, 2f), new Vector3(0.05f, 0.25f, 0.6f), "mat_neon_verde", false);
        Luz("Salida_Luz_2", new Vector3(-29.3f, 2.7f, 2f), new Color(0.1f, 1f, 0.3f), 1.2f, 4f, luces);

        var trasera = Puerta("Puerta_Trasera", new Vector3(-30f, 0f, 2f), 90f, 1.4f, 2.4f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", raiz);
        trasera.angulo = 100f;
        trasera.alAbrir = new Acciones { progresoCielo = 0.08f };

        // Eventos
        var zDeposito = Zona("Z_DepositoCerrado", new Vector3(-24.8f, 1.5f, 0f), new Vector3(2f, 3f, 2.5f), ev, new Acciones
        {
            objetivo = "Consigue la llave del depósito (don Ramiro, el guardia, siempre la carga)"
        });
        zDeposito.requiereSinObjeto = "llave_deposito";
        Zona("Z_RuidoBanos", new Vector3(-22.5f, 1.5f, -11.5f), new Vector3(4f, 3f, 6f), ev, new Acciones
        {
            subtitulos = new[] { "Mateo|Ese ruido... viene de los baños.", "Mateo|Mejor agarro algo para defenderme." },
            mensaje = "Los objetos que puedes recoger brillan cuando estás cerca",
            sonido = "sfx_grunido"
        });
        Zona("Z_EntrarBanos", new Vector3(-27f, 1.5f, -12f), new Vector3(2.4f, 3f, 2f), ev, new Acciones
        {
            subtitulos = new[] { "Mateo|¿Don Ramiro? ...¿Está bien?" }
        });
        var zAtaque = Zona("Z_GuardiaAtaca", new Vector3(-27.6f, 1.5f, -12.5f), new Vector3(3.2f, 3f, 3f), ev, new Acciones
        {
            alertar = new[] { guardia },
            mensaje = "Clic izquierdo: golpear   ·   Clic derecho: empujar   ·   H: curarte"
        });
        zAtaque.retraso = 1.4f;
    }

    // =====================================================================
    // ZONA 2: CALLEJON (x -36..-30, z -18..28) + PASAJE (x -30..-8, z 22..28)
    // =====================================================================
    private static void Callejon(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z2_Callejon").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        Bloque("Callejon_Piso", new Vector3(-36f, -0.2f, -18.6f), new Vector3(-30f, 0f, 28f), "mat_asfalto");
        Bloque("Pasaje_Piso", new Vector3(-30f, -0.2f, 22f), new Vector3(-8f, 0f, 28f), "mat_asfalto");
        Edificio("Callejon_EdifOeste", new Vector3(-44f, 0f, -30f), new Vector3(-36f, 12f, 36f), "mat_revoque_gris", false, false, true, false, false);
        Edificio("Edif_NorteDisco", new Vector3(-30f, 0f, 6.3f), new Vector3(-8f, 11f, 22f), "mat_revoque_ocre", true, false, true, true, true);
        Edificio("Edif_PasajeNorte", new Vector3(-44f, 0f, 28f), new Vector3(-8f, 12f, 36f), "mat_ladrillo", false, true, true, false, true);
        Edificio("Edif_SurDisco", new Vector3(-44f, 0f, -30f), new Vector3(-8f, 10f, -18.3f), "mat_revoque_verde", false, false, true, false, true);

        Caja("Contenedor", new Vector3(-34.8f, 0.7f, -15.5f), new Vector3(1.6f, 1.4f, 2.6f), "mat_metal_pintado");
        Basura(new Vector3(-35f, 0f, -11f), raiz);
        Basura(new Vector3(-31f, 0f, 4.5f), raiz);
        Basura(new Vector3(-35.2f, 0f, 14f), raiz);
        Basura(new Vector3(-20f, 0f, 27.2f), raiz);
        Caja("Cartones", new Vector3(-31f, 0.3f, -6f), new Vector3(1f, 0.6f, 0.8f), "mat_carton", true, 20f);
        Caja("Cartones_2", new Vector3(-35.3f, 0.25f, 19f), new Vector3(0.8f, 0.5f, 1.1f), "mat_carton", true, -15f);
        Caja("Charco_1", new Vector3(-33f, 0.006f, -3f), new Vector3(2f, 0.01f, 1.4f), "mat_vidrio", false, 20f);
        Caja("Charco_2", new Vector3(-22f, 0.006f, 25f), new Vector3(1.6f, 0.01f, 2.2f), "mat_vidrio", false, -30f);
        for (int i = 0; i < 6; i++)
        {
            Caja("Cable_" + i, new Vector3(-33f, 5.2f + i * 0.25f, -12f + i * 7f), new Vector3(6.4f, 0.03f, 0.03f), "mat_negro", false, i * 6f - 12f);
        }

        Foco("Callejon_Foco_Puerta", new Vector3(-30.4f, 3.4f, 2f), new Vector3(-33f, 0f, 2f), new Color(1f, 0.7f, 0.4f), 6f, 10f, 90f, luces, LuzParpadeante.Modo.Falla);
        Foco("Callejon_Foco_Esquina", new Vector3(-33f, 4.6f, 24.5f), new Vector3(-30f, 0f, 24f), new Color(0.7f, 0.8f, 1f), 5f, 14f, 100f, luces, null);

        Objeto("Botella_Callejon_1", new Vector3(-34.6f, 0.02f, 7.5f), Recogible.TipoObjeto.Botella, 1, raiz);
        Objeto("Botella_Callejon_2", new Vector3(-31.2f, 0.02f, 12.5f), Recogible.TipoObjeto.Botella, 1, raiz);
        Objeto("Venda_Contenedor", new Vector3(-35.3f, 0.02f, -13.4f), Recogible.TipoObjeto.Venda, 1, raiz);

        CrearNPC("Rig_Infectado", "Cadaver_Callejon", new Vector3(-33f, 0.02f, 17.4f), 0f, "mat_char_civil", PoseNPC.Pose.Cadaver, false, pers, AnimProcedural.Estilo.Humano);
        Sangre(new Vector3(-33f, 0f, 17f), 1.8f, raiz);
        CrearInfectado("Infectado_Callejon_Comiendo", new Vector3(-33f, 0f, 16.4f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 3f);
        CrearInfectado("Infectado_Pasaje", new Vector3(-17f, 0f, 25f), 270f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 3f);

        Cartel("Cartel_SEDES", new Vector3(-12f, 1.6f, 22.07f), 0f, "Comunicado del SEDES", TextoSEDES, raiz);

        Zona("Z_Callejon_Inicio", new Vector3(-33f, 1.5f, 2f), new Vector3(6f, 3f, 4f), ev, new Acciones
        {
            puntoControl = true,
            progresoCielo = 0.1f,
            objetivo = "Sal del callejón hacia la avenida 20 de Octubre",
            subtitulos = new[] { "Mateo|Aire... qué frío hace." }
        });
        Zona("Z_Tutorial_Sigilo", new Vector3(-33f, 1.5f, 8.5f), new Vector3(6f, 3f, 3f), ev, new Acciones
        {
            subtitulos = new[] { "Mateo|Hay alguien ahí agachado... ¿se está comiendo a alguien?" },
            mensaje = "[C] agacharte  ·  Acércate por la espalda sin que te vea  ·  [E] ataque sigiloso"
        });
        Zona("Z_Tutorial_Botella", new Vector3(-33f, 1.5f, 21.5f), new Vector3(6f, 3f, 3f), ev, new Acciones
        {
            subtitulos = new[] { "Mateo|Otro más en el pasaje. Si tiro algo lejos, tal vez lo distraigo." },
            mensaje = "[Q] lanzar botella hacia donde apuntas: el ruido atrae a los infectados"
        });
        Zona("Z_Llegada_Avenida", new Vector3(-10f, 1.5f, 25f), new Vector3(4f, 3f, 6f), ev, new Acciones
        {
            puntoControl = true,
            progresoCielo = 0.25f,
            objetivo = "Llega al edificio de doña Bety: sube las gradas detrás del Mercado Sopocachi",
            subtitulos = new[] { "Mateo|No... no puede ser.", "Mateo|La 20 de Octubre está en llamas.", "Mateo|Doña Bety vive arriba de las gradas, atrás del mercado." },
            sonido = "sfx_susto"
        });
    }

    // =====================================================================
    // ZONA 3: AV. 20 DE OCTUBRE (x -8..8) Y PLAZA ABAROA (x 8..40, z 16..46)
    // =====================================================================
    private static void AvenidaYPlaza(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z3_Avenida_Plaza").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        // Suelos
        Bloque("Avenida_Asfalto", new Vector3(-8f, -0.2f, -30f), new Vector3(48f, -0.02f, 60f), "mat_asfalto");
        Bloque("Vereda_Oeste", new Vector3(-8f, 0f, -26f), new Vector3(-5f, 0.15f, 60f), "mat_adoquin");
        Bloque("Vereda_Este_S", new Vector3(5f, 0f, -26f), new Vector3(8f, 0.15f, 10f), "mat_adoquin");
        Bloque("Vereda_Este_N", new Vector3(5f, 0f, 52f), new Vector3(10f, 0.15f, 66f), "mat_adoquin");
        Bloque("Plaza_Suelo", new Vector3(8f, 0f, 16f), new Vector3(40f, 0.18f, 46f), "mat_adoquin");
        Bloque("Plaza_Jardin_1", new Vector3(12f, 0.18f, 19f), new Vector3(21f, 0.3f, 27f), "mat_verde_arbol");
        Bloque("Plaza_Jardin_2", new Vector3(31f, 0.18f, 19f), new Vector3(38f, 0.3f, 27f), "mat_verde_arbol");
        Bloque("Plaza_Jardin_3", new Vector3(12f, 0.18f, 37f), new Vector3(21f, 0.3f, 44f), "mat_verde_arbol");
        Bloque("Plaza_Jardin_4", new Vector3(31f, 0.18f, 37f), new Vector3(38f, 0.3f, 44f), "mat_verde_arbol");

        // Monumento a Eduardo Abaroa
        Bloque("Monumento_Base", new Vector3(24f, 0.18f, 29f), new Vector3(28f, 1f, 33f), "mat_concreto");
        Bloque("Monumento_Pedestal", new Vector3(24.8f, 1f, 29.8f), new Vector3(27.2f, 3.2f, 32.2f), "mat_concreto");
        Caja("Estatua_Cuerpo", new Vector3(26f, 4.2f, 31f), new Vector3(0.7f, 2f, 0.5f), "mat_fierro");
        Caja("Estatua_Brazo", new Vector3(26.4f, 5.3f, 31.1f), new Vector3(0.2f, 1.1f, 0.2f), "mat_fierro", false, 0f);
        var cabeza = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cabeza.name = "Estatua_Cabeza";
        cabeza.transform.SetParent(raiz, false);
        cabeza.transform.position = new Vector3(26f, 5.45f, 31f);
        cabeza.transform.localScale = Vector3.one * 0.42f;
        cabeza.GetComponent<Renderer>().sharedMaterial = Mat("mat_fierro");
        Object.DestroyImmediate(cabeza.GetComponent<Collider>());
        Caja("Monumento_Placa", new Vector3(26f, 2f, 29.75f), new Vector3(1.2f, 0.6f, 0.05f), "mat_metal_oscuro", false);

        // Arboles, bancas, kiosco
        Vector3[] arboles = { new Vector3(14f, 0.3f, 21f), new Vector3(19f, 0.3f, 25f), new Vector3(35f, 0.3f, 21f), new Vector3(14f, 0.3f, 42f),
                              new Vector3(36f, 0.3f, 42f), new Vector3(33f, 0.3f, 25f), new Vector3(18f, 0.3f, 39f), new Vector3(33f, 0.3f, 39f) };
        foreach (var a in arboles) { Arbol(a, raiz); }
        Banca(new Vector3(23f, 0.18f, 26f), 0f, raiz);
        Banca(new Vector3(29f, 0.18f, 26f), 0f, raiz);
        Banca(new Vector3(23f, 0.18f, 36f), 180f, raiz);
        Banca(new Vector3(29f, 0.18f, 36f), 180f, raiz);
        Banca(new Vector3(21f, 0.18f, 31f), 90f, raiz);
        Banca(new Vector3(31f, 0.18f, 31f), -90f, raiz);

        Caja("Kiosco", new Vector3(16f, 1.48f, 45f), new Vector3(3f, 2.6f, 2f), "mat_revoque_verde");
        Caja("Kiosco_Techo", new Vector3(16f, 2.9f, 44.6f), new Vector3(3.6f, 0.12f, 3f), "mat_calamina", false);
        Caja("Kiosco_Ventana", new Vector3(16f, 1.6f, 43.96f), new Vector3(2f, 0.9f, 0.05f), "mat_negro", false);
        Sonido("Radio_Kiosco", new Vector3(16f, 1.4f, 43.8f), "amb_radio", 0.5f, 12f, raiz);
        Zona("Z_Radio", new Vector3(16f, 1.5f, 42f), new Vector3(6f, 3f, 4f), ev, new Acciones
        {
            subtitulos = new[]
            {
                "Radio|...repetimos: las autoridades piden a la población no salir de sus domicilios...",
                "Radio|...el Ejército instaló un cerco en la autopista a El Alto. Nadie baja, nadie sube...",
                "Radio|...los contagiados no responden al... [estática]"
            }
        });

        // Vehiculos y fuego en la avenida
        Modelo("Props/prop_minibus.obj", new Vector3(-3.4f, 0f, 7.5f), 22f, "mat_prop_minibus_blanco", raiz);
        Incendio(new Vector3(-3.3f, 2.4f, 6.2f), 1.2f, raiz);
        var volcado = Modelo("Props/prop_auto_sedan.obj", new Vector3(2f, 1.52f, 30f), 0f, "mat_prop_sedan", raiz);
        volcado.transform.rotation = Quaternion.Euler(0f, 70f, 180f);
        Modelo("Props/prop_auto_sedan.obj", new Vector3(-2.2f, 0f, 42f), -15f, "mat_metal_pintado", raiz);
        Incendio(new Vector3(-2.2f, 1.45f, 42f), 1.2f, raiz);
        Modelo("Props/prop_minibus.obj", new Vector3(3.2f, 0f, 45.5f), 160f, "mat_prop_minibus_celeste", raiz);

        // Cierre sur de la avenida
        Modelo("Props/prop_minibus.obj", new Vector3(-2f, 0f, -21f), 88f, "mat_prop_minibus_blanco", raiz);
        Modelo("Props/prop_minibus.obj", new Vector3(3.5f, 0f, -23.5f), 75f, "mat_prop_minibus_celeste", raiz);
        Incendio(new Vector3(0.5f, 1f, -21.5f), 1.4f, raiz);
        Bloque("Barrera_Sur", new Vector3(-8f, 0f, -26.5f), new Vector3(8f, 1.1f, -25.6f), "mat_concreto");
        Edificio("Avenida_FondoSur", new Vector3(-8f, 0f, -34f), new Vector3(48f, 12f, -26.5f), "mat_ladrillo", true, false, false, false, true);

        // Edificios del lado oeste (al norte del pasaje)
        Edificio("Av_Oeste_3", new Vector3(-24f, 0f, 36f), new Vector3(-8f, 13f, 60f), "mat_revoque_verde", false, false, true, false, true);
        // Lado este (sur): farmacia y vecinos
        Edificio("Av_Este_S1", new Vector3(8f, 0f, -26.5f), new Vector3(48f, 11f, -6.3f), "mat_revoque_gris", false, false, false, true, true);
        Edificio("Av_Este_S2", new Vector3(8f, 0f, -6.3f), new Vector3(11.7f, 9f, 10f), "mat_ladrillo", true, false, false, true, true);
        Edificio("Av_Este_S3", new Vector3(26.3f, 0f, -6.3f), new Vector3(48f, 10f, 10f), "mat_revoque_ocre", true, false, false, false, true);
        Edificio("Plaza_Este", new Vector3(48f, 0f, -34f), new Vector3(60f, 12f, 80f), "mat_ladrillo", false, false, false, true, true);
        // Calle este bloqueada
        Modelo("Props/prop_minibus.obj", new Vector3(44f, 0f, 30f), 5f, "mat_negro", raiz);
        Incendio(new Vector3(44f, 2.6f, 30f), 1.6f, raiz);
        Bloque("Barrera_Este_S", new Vector3(40f, 0f, 12f), new Vector3(48f, 1.1f, 13f), "mat_concreto");
        Bloque("Barrera_Este_N", new Vector3(40f, 0f, 49f), new Vector3(48f, 1.1f, 50f), "mat_concreto");
        Objeto("Venda_BusQuemado", new Vector3(42f, 0.02f, 25f), Recogible.TipoObjeto.Venda, 1, raiz);

        // Barricada policial (norte) y fondo
        var patrulla = Modelo("Props/prop_auto_sedan.obj", new Vector3(-1f, 0f, 56.5f), 85f, "mat_policia", raiz);
        Caja("Patrulla_Luces", patrulla.transform.position + Vector3.up * 1.6f, new Vector3(0.9f, 0.12f, 0.3f), "mat_neon_cian", false, 85f);
        Luz("Patrulla_Baliza", patrulla.transform.position + Vector3.up * 2f, new Color(0.2f, 0.4f, 1f), 4f, 12f, luces, LuzParpadeante.Modo.Estrobo);
        for (int i = 0; i < 3; i++)
        {
            Caja("Barrera_Policial_" + i, new Vector3(-4f + i * 3.4f, 0.55f, 58.5f), new Vector3(3f, 1.1f, 0.4f), "mat_barrera", true, (i - 1) * 6f);
        }
        Bloque("Barrera_Policial_Cierre", new Vector3(-8f, 0f, 59f), new Vector3(5f, 1.2f, 60f), "mat_concreto");
        Edificio("Av_FondoNorte", new Vector3(-8f, 0f, 60f), new Vector3(5f, 14f, 72f), "mat_revoque_gris", false, true, false, false, true);
        Doc("Transcripcion_Policia", new Vector3(1.6f, 0.02f, 53.4f), "Radio patrulla 214", TextoPolicia, raiz);
        CrearInfectado("Policia_1", new Vector3(-1f, 0f, 52.5f), 180f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 3f, "mat_char_policia");
        CrearInfectado("Policia_2", new Vector3(3f, 0f, 54.5f), 250f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 2f, "mat_char_policia");
        Zona("Z_Barricada", new Vector3(0f, 1.5f, 49f), new Vector3(10f, 3f, 3f), ev, new Acciones
        {
            subtitulos = new[] { "Mateo|La policía cerró la avenida... y ahora son como ellos.", "Mateo|Por aquí no paso. Tengo que ir por el mercado." }
        });

        // Postes con luz
        Poste(new Vector3(-6.6f, 0.15f, -12f), 90f, raiz, luces, true);
        Poste(new Vector3(-6.6f, 0.15f, 3f), 90f, raiz, luces, false);
        Poste(new Vector3(-6.6f, 0.15f, 17f), 90f, raiz, luces, true);
        Poste(new Vector3(-6.6f, 0.15f, 38f), 90f, raiz, luces, true);
        Poste(new Vector3(6.6f, 0.15f, -8f), -90f, raiz, luces, true);
        Poste(new Vector3(9.4f, 0.18f, 30f), -90f, raiz, luces, true);
        Poste(new Vector3(24f, 0.18f, 16.6f), 0f, raiz, luces, true);
        Poste(new Vector3(24f, 0.18f, 45.4f), 180f, raiz, luces, true);
        Poste(new Vector3(6.6f, 0.15f, 55f), -90f, raiz, luces, false);
        Luz("Luna_Plaza", new Vector3(26f, 12f, 31f), new Color(0.5f, 0.6f, 0.9f), 2f, 30f, luces);

        // Infectados de la avenida y la plaza
        CrearNPC("Rig_Infectado", "Cadaver_Avenida", new Vector3(-1.6f, 0.02f, 16.4f), 200f, "mat_char_civil", PoseNPC.Pose.Cadaver, false, pers, AnimProcedural.Estilo.Humano);
        Sangre(new Vector3(-1.8f, 0f, 16.2f), 1.5f, raiz);
        CrearInfectado("Infectado_Avenida_Comiendo", new Vector3(-2.6f, 0f, 16f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 3f);
        CrearInfectado("Infectado_Avenida_1", new Vector3(0f, 0f, 36f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 5f);
        CrearInfectado("Infectado_Plaza_1", new Vector3(22f, 0.18f, 22.5f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 5f);
        CrearInfectado("Infectado_Plaza_2", new Vector3(33f, 0.18f, 35f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 5f);
        CrearInfectado("Corredor_Plaza", new Vector3(36.5f, 0.18f, 30f), 270f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 3f);
        CrearInfectado("Infectado_Plaza_Dormido", new Vector3(19.6f, 0.18f, 34.2f), 160f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 3f);
        CrearInfectado("Infectado_CalleSur", new Vector3(31f, 0f, 13f), 270f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 4f);

        // Sobreviviente: don Freddy (decision moral)
        var freddy = CrearNPC("Rig_Infectado", "Sobreviviente_Freddy", new Vector3(22.2f, 0f, 10.9f), 0f, "mat_char_civil", PoseNPC.Pose.Sentado, true, pers, AnimProcedural.Estilo.Humano);
        var em = freddy.AddComponent<EleccionMoral>();
        em.nombreNPC = "Don Freddy";
        em.radio = 2.3f;
        em.altura = 0.8f;
        em.texto = "Un hombre mayor, sentado contra la pared de la farmacia. Tiene la pierna vendada con su chompa y la tela está negra de sangre.\n\n\"Joven... ¿no tendrás una venda? Esa cosa me mordió en la pantorrilla.\"";
        em.opciones = new[]
        {
            new EleccionMoral.OpcionMoral
            {
                texto = "Darle una de tus vendas", costoVendas = 1, moral = 10, darPilas = 1,
                respuesta = "Gracias, hijo. Dios te lo pague.\nToma estas pilas, de algo te servirán.\nLa doctora de la farmacia tenía la llave del mercado... pero ya no es ella. Ten cuidado."
            },
            new EleccionMoral.OpcionMoral
            {
                texto = "\"Lo siento, no tengo nada para usted\"", moral = -5,
                respuesta = "Anda nomás... anda.\nSi vas al mercado, la llave la tenía la doctora, adentro de la farmacia."
            },
            new EleccionMoral.OpcionMoral
            {
                texto = "Quitarle su mochila (tiene provisiones)", moral = -15, darBotellas = 1, darVendas = 1,
                respuesta = "¡Ladrón! ¡Así nos vamos a morir todos, desgraciado!"
            }
        };
    }

    // =====================================================================
    // ZONA 4: FARMACIA (x 12..26, z -6..10)
    // =====================================================================
    private static void Farmacia(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z4_Farmacia").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        Cuarto("Farmacia", 12f, 26f, -6f, 10f, 0f, 3.6f, "mat_revoque_gris", "mat_baldosa", "mat_concreto",
            new[] { Hueco.Puerta(19f, 2.2f, 2.6f), new Hueco(14.8f, 3f, 0.8f, 2.6f), new Hueco(23.3f, 3f, 0.8f, 2.6f) },
            SinHuecos, SinHuecos, SinHuecos);
        Edificio("Farmacia_PisoSuperior", new Vector3(11.85f, 3.9f, -6.15f), new Vector3(26.15f, 8f, 10.15f), "mat_revoque_verde", true, false, false, false, false, true);
        Caja("Vitrina_O", new Vector3(14.8f, 1.7f, 10f), new Vector3(3f, 1.8f, 0.04f), "mat_vidrio", true);
        Caja("Letrero_Farmacia", new Vector3(19f, 3.25f, 10.22f), new Vector3(5f, 0.6f, 0.12f), "mat_farmacia_verde", false);
        Caja("Cruz_V", new Vector3(24.8f, 3.2f, 10.3f), new Vector3(0.25f, 0.8f, 0.08f), "mat_neon_verde", false);
        Caja("Cruz_H", new Vector3(24.8f, 3.2f, 10.3f), new Vector3(0.8f, 0.25f, 0.08f), "mat_neon_verde", false);
        Luz("Cruz_Luz", new Vector3(24.8f, 3.2f, 11f), new Color(0.1f, 1f, 0.35f), 2.5f, 8f, luces, LuzParpadeante.Modo.Falla);
        for (int i = 0; i < 6; i++)
        {
            Caja("Vidrio_Roto_" + i, new Vector3(18f + i * 0.35f, 0.01f, 10.6f + (i % 3) * 0.3f), new Vector3(0.25f, 0.01f, 0.15f), "mat_vidrio", false, i * 40f);
        }

        // Estantes y mostrador
        Caja("Estante_1", new Vector3(15.6f, 0.9f, 6f), new Vector3(0.6f, 1.8f, 4f), "mat_metal_pintado");
        Caja("Estante_2", new Vector3(22.4f, 0.9f, 6f), new Vector3(0.6f, 1.8f, 4f), "mat_metal_pintado");
        Caja("Mostrador", new Vector3(17.5f, 0.52f, 2.3f), new Vector3(9f, 1.05f, 0.7f), "mat_madera");
        Caja("Estante_Fondo", new Vector3(17f, 1.2f, 1.45f), new Vector3(9.4f, 2.4f, 0.5f), "mat_metal_pintado");
        for (int i = 0; i < 18; i++)
        {
            Caja("Caja_Medicina", new Vector3(12.6f + (i % 9) * 1f, 0.6f + (i / 9) * 0.8f, 1.2f), new Vector3(0.35f, 0.25f, 0.2f), i % 2 == 0 ? "mat_baldosa" : "mat_plastico_azul", false);
        }
        Pared("Farmacia_Tabique", new Vector3(12f, 0f, 1f), new Vector3(26f, 0f, 1f), 3.6f, 0.2f, "mat_revoque_gris", Hueco.Puerta(12.5f, 1.2f, 2.3f));
        Luz("Farmacia_Fluorescente", new Vector3(19f, 3.4f, 6f), new Color(0.85f, 0.95f, 1f), 3f, 10f, luces, LuzParpadeante.Modo.Falla);
        Luz("Farmacia_LuzFondo", new Vector3(19f, 3.3f, -2.5f), new Color(1f, 0.2f, 0.15f), 0.7f, 8f, luces);

        Objeto("Venda_Mostrador", new Vector3(16.6f, 1.07f, 2.3f), Recogible.TipoObjeto.Venda, 1, raiz);
        Objeto("Venda_Estante", new Vector3(22.4f, 1.82f, 5.2f), Recogible.TipoObjeto.Venda, 1, raiz);
        Objeto("Pilas_Estante", new Vector3(15.6f, 1.82f, 7f), Recogible.TipoObjeto.Pilas, 1, raiz);
        Objeto("Botella_Farmacia", new Vector3(24.6f, 0.02f, 6.2f), Recogible.TipoObjeto.Botella, 1, raiz);

        // Deposito del fondo
        for (int i = 0; i < 4; i++)
        {
            Caja("Deposito_Estante_F" + i, new Vector3(14f + i * 3.4f, 1.1f, -5.55f), new Vector3(2.6f, 2.2f, 0.7f), "mat_metal_oscuro");
        }
        Caja("Agua_Embotellada", new Vector3(23.6f, 0.5f, -3.6f), new Vector3(1.4f, 1f, 1.2f), "mat_plastico_azul");
        Caja("Escritorio", new Vector3(14.3f, 0.38f, -2.6f), new Vector3(1.6f, 0.76f, 0.8f), "mat_madera");
        Doc("Nota_Doctora", new Vector3(14.3f, 0.78f, -2.6f), "Nota de la Dra. Julia", TextoDoctora, raiz);
        var llave = Objeto("Llave_Mercado", new Vector3(23.6f, 1.03f, -3.6f), Recogible.TipoObjeto.ObjetoClave, 1, raiz, "llave_mercado", "Llave del Mercado Sopocachi");

        var doctora = CrearInfectado("Infectada_Doctora", new Vector3(20.5f, 0f, -2.6f), 200f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 2f, "mat_bata");
        doctora.despertarPorProximidad = 3f;
        var corredor = CrearInfectado("Corredor_Farmacia", new Vector3(19f, 0f, 14f), 180f, Infectado.Tipo.Corredor, Infectado.Estado.Deambular, pers, 2f, null, false);

        llave.alRecoger = new Acciones
        {
            alertar = new[] { corredor },
            progresoCielo = 0.5f,
            objetivo = "Entra al mercado por la puerta lateral (junto a la avenida)",
            subtitulos = new[] { "Mateo|La llave del mercado...", "Mateo|¡Algo viene corriendo!" }
        };

        Zona("Z_Farmacia_Entrada", new Vector3(19f, 1.5f, 8.2f), new Vector3(3f, 3f, 2f), ev, new Acciones
        {
            puntoControl = true,
            progresoCielo = 0.4f,
            subtitulos = new[] { "Mateo|¿Doña Julia? ...¿Está aquí?" }
        });
    }

    // =====================================================================
    // ZONA 5: MERCADO SOPOCACHI (x 10..40, z 52..74) + PATIO (z 74..80)
    // =====================================================================
    private static void Mercado(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z5_Mercado").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        Cuarto("Mercado", 10f, 40f, 52f, 74f, 0f, 6f, "mat_ladrillo", "mat_concreto", "mat_calamina",
            new[] { Hueco.Puerta(32f, 1.4f, 2.4f) },
            new[] { Hueco.Puerta(25f, 4f, 3.2f) },
            new[] { Hueco.Puerta(62f, 2.2f, 2.8f) },
            new[] { Hueco.Puerta(62f, 1.4f, 2.4f) });
        Caja("Mercado_Letrero", new Vector3(25f, 4.8f, 51.78f), new Vector3(9f, 1f, 0.12f), "mat_toldo_rojo", false);
        Caja("Mercado_Letrero_Texto", new Vector3(25f, 4.8f, 51.7f), new Vector3(7.5f, 0.35f, 0.04f), "mat_baldosa", false);

        // Barricada en la entrada principal (desde adentro)
        Modelo("Props/prop_puesto_mercado.obj", new Vector3(25f, 0f, 53.4f), 0f, "mat_prop_puesto", raiz);
        Caja("Barricada_Cajas_1", new Vector3(23.2f, 0.6f, 52.9f), new Vector3(1.4f, 1.2f, 1f), "mat_carton", true, 12f);
        Caja("Barricada_Cajas_2", new Vector3(26.9f, 0.9f, 52.9f), new Vector3(1.3f, 1.8f, 1f), "mat_madera", true, -8f);
        Caja("Barricada_Mesa", new Vector3(25f, 2.2f, 52.7f), new Vector3(4f, 0.9f, 0.3f), "mat_madera", true);

        // Puestos
        float[] filas = { 57f, 62.5f, 68f };
        float[] cols = { 14.5f, 20f, 25.5f, 31f, 36.5f };
        string[] toldos = { "mat_toldo_rojo", "mat_plastico_azul", "mat_barrera" };
        for (int f = 0; f < filas.Length; f++)
        {
            for (int c = 0; c < cols.Length; c++)
            {
                if (f == 1 && c == 0) { continue; }
                Modelo("Props/prop_puesto_mercado.obj", new Vector3(cols[c], 0f, filas[f]), f % 2 == 0 ? 0f : 180f, "mat_prop_puesto", raiz);
                Caja("Toldo", new Vector3(cols[c], 2.55f, filas[f]), new Vector3(2.8f, 0.05f, 2.2f), toldos[(f + c) % 3], false);
            }
        }
        Caja("Ferreteria_Mesa", new Vector3(37f, 0.5f, 71.6f), new Vector3(3.5f, 1f, 1.2f), "mat_madera");
        Caja("Ferreteria_Letrero", new Vector3(37f, 2.4f, 72.3f), new Vector3(3f, 0.5f, 0.05f), "mat_barrera", false);
        for (int i = 0; i < 6; i++) { Caja("Herramienta", new Vector3(35.6f + i * 0.5f, 1.05f, 71.8f), new Vector3(0.08f, 0.1f, 0.4f), "mat_metal_oscuro", false, i * 25f); }
        Caja("Cajas_Papa", new Vector3(12f, 0.45f, 71.8f), new Vector3(1.8f, 0.9f, 1.4f), "mat_carton");
        Caja("Cajas_Chuno", new Vector3(38.5f, 0.4f, 55f), new Vector3(1.2f, 0.8f, 2f), "mat_carton");

        Luz("Mercado_Foco_1", new Vector3(20f, 5.4f, 59.5f), new Color(1f, 0.75f, 0.45f), 3.5f, 12f, luces, LuzParpadeante.Modo.Falla);
        Luz("Mercado_Foco_2", new Vector3(36.5f, 5.4f, 70f), new Color(1f, 0.75f, 0.45f), 2.5f, 10f, luces, LuzParpadeante.Modo.Falla);
        Luz("Mercado_Claraboya", new Vector3(28f, 5.6f, 65f), new Color(0.45f, 0.55f, 0.85f), 1.8f, 14f, luces);

        // Objetos
        Objeto("Fierro_Mercado", new Vector3(12.6f, 0.03f, 70.4f), Recogible.TipoObjeto.Fierro, 1, raiz, null, "Fierro de construcción");
        var corta = Objeto("Cortafierro", new Vector3(37.6f, 1.03f, 71.5f), Recogible.TipoObjeto.ObjetoClave, 1, raiz, "cortafierro", "Cortafierro");
        Objeto("Botella_Mercado_1", new Vector3(20f, 0.02f, 59.3f), Recogible.TipoObjeto.Botella, 1, raiz);
        Objeto("Botella_Mercado_2", new Vector3(31f, 0.02f, 64.8f), Recogible.TipoObjeto.Botella, 1, raiz);
        Objeto("Venda_Mercado", new Vector3(25.5f, 0.02f, 70.2f), Recogible.TipoObjeto.Venda, 1, raiz);
        Objeto("Pilas_Mercado", new Vector3(14.2f, 0.02f, 59.3f), Recogible.TipoObjeto.Pilas, 1, raiz);
        Doc("Cuaderno_Rosa", new Vector3(26.8f, 0.02f, 59.4f), "Cuaderno de doña Rosa", TextoCuaderno, raiz);

        // Infectados
        CrearInfectado("Fungico_Mercado", new Vector3(25.5f, 0f, 65.2f), 0f, Infectado.Tipo.Fungico, Infectado.Estado.Deambular, pers, 9f);
        CrearInfectado("Infectado_Mercado_1", new Vector3(17.5f, 0f, 60f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 3f);
        CrearNPC("Rig_Infectado", "Cadaver_Mercado", new Vector3(34.4f, 0.02f, 59.8f), 70f, "mat_char_civil", PoseNPC.Pose.Cadaver, false, pers, AnimProcedural.Estilo.Humano);
        CrearInfectado("Infectado_Mercado_Comiendo", new Vector3(33.6f, 0f, 59.6f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 3f);
        CrearInfectado("Infectado_Mercado_Dormido", new Vector3(28.4f, 0f, 70.6f), 180f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 3f);
        var emb1 = CrearInfectado("Corredor_Mercado", new Vector3(23f, 0f, 54.6f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Deambular, pers, 3f, null, false);
        var emb2 = CrearInfectado("Infectado_Mercado_Emboscada", new Vector3(16f, 0f, 72.6f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 3f, null, false);
        corta.alRecoger = new Acciones
        {
            alertar = new[] { emb1, emb2 },
            objetivo = "Sal por la puerta trasera del mercado y corta la cadena de la reja",
            subtitulos = new[] { "Mateo|¡Me escucharon! ¡Tengo que salir de aquí!" }
        };

        // Puerta lateral (llave) y trasera
        var lateral = Puerta("Puerta_Lateral_Mercado", new Vector3(10f, 0f, 62f), 90f, 1.4f, 2.4f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", raiz);
        lateral.llaveRequerida = "llave_mercado";
        lateral.nombreLlave = "Llave del Mercado Sopocachi";
        lateral.consumirLlave = false;
        lateral.mensajeCerrada = "Cerrada con llave. Doña Julia, la de la farmacia, siempre abría el mercado temprano...";
        lateral.angulo = 100f;
        var trasera = Puerta("Puerta_Trasera_Mercado", new Vector3(32f, 0f, 74f), 0f, 1.4f, 2.4f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", raiz);
        trasera.angulo = -100f;

        Zona("Z_Mercado_Entrada", new Vector3(12.5f, 1.5f, 62f), new Vector3(3f, 3f, 3f), ev, new Acciones
        {
            puntoControl = true,
            progresoCielo = 0.6f,
            objetivo = "Cruza el mercado y sal por la puerta trasera, hacia las gradas",
            subtitulos = new[] { "Mateo|Huele a podrido...", "Mateo|Y ese ruido... clic, clic... ¿qué es eso?" },
            mensaje = "Los fúngicos son ciegos pero oyen todo: muévete agachado o distráelos con botellas"
        });
        var zFrente = Zona("Z_Mercado_Frente", new Vector3(25f, 1.5f, 49f), new Vector3(8f, 3f, 4f), ev, new Acciones
        {
            objetivo = "Busca la entrada lateral del mercado (del lado de la avenida)",
            subtitulos = new[] { "Mateo|La entrada está bloqueada. Alguien se atrincheró adentro.", "Mateo|Hay una puerta lateral, del lado de la avenida." }
        });
        zFrente.requiereSinObjeto = "llave_mercado";
        var zLateral = Zona("Z_Mercado_Lateral", new Vector3(7.8f, 1.5f, 62f), new Vector3(4f, 3f, 4f), ev, new Acciones
        {
            objetivo = "Consigue la llave del mercado en la farmacia de doña Julia (cruzando la plaza, al sur)"
        });
        zLateral.requiereSinObjeto = "llave_mercado";
        Bloque("Pasillo_Lateral_Cierre", new Vector3(5f, 0f, 66f), new Vector3(10f, 4f, 66.5f), "mat_ladrillo");

        // Patio trasero y reja con cadena
        Bloque("Patio_Piso", new Vector3(26f, -0.2f, 74f), new Vector3(40f, 0f, 80f), "mat_concreto");
        Bloque("Patio_ParedO", new Vector3(25.7f, 0f, 74f), new Vector3(26f, 4f, 80f), "mat_ladrillo");
        Bloque("Patio_ParedE", new Vector3(40f, 0f, 74f), new Vector3(40.3f, 4f, 80f), "mat_ladrillo");
        Pared("Patio_ParedN", new Vector3(26f, 0f, 80f), new Vector3(40f, 0f, 80f), 4f, 0.3f, "mat_ladrillo", Hueco.Puerta(6f, 4f, 2.6f));
        Basura(new Vector3(28f, 0f, 77f), raiz);
        Caja("Patio_Tanque", new Vector3(38.5f, 0.8f, 77.5f), new Vector3(1.2f, 1.6f, 1.2f), "mat_plastico_azul");
        Foco("Patio_Foco", new Vector3(32f, 3.8f, 75f), new Vector3(32f, 0f, 78f), new Color(1f, 0.8f, 0.6f), 5f, 10f, 100f, luces, LuzParpadeante.Modo.Falla);
        var reja = Puerta("Reja_Gradas", new Vector3(32f, 0f, 80f), 0f, 4f, 2.6f, PuertaCap.Tipo.Reja, "mat_metal_oscuro", raiz);
        reja.llaveRequerida = "cortafierro";
        reja.nombreLlave = "Cortafierro";
        reja.consumirLlave = false;
        reja.mensajeCerrada = "Una reja con cadena y candado. Necesito algo para cortar la cadena... En el mercado había una ferretería.";
        reja.alAbrir = new Acciones
        {
            puntoControl = true,
            progresoCielo = 0.75f,
            objetivo = "Sube las gradas hasta el edificio de doña Bety",
            subtitulos = new[] { "Mateo|Ya casi. El edificio de doña Bety está arriba." }
        };
        var zPatio = Zona("Z_Patio_SinCortafierro", new Vector3(32f, 1.5f, 77.5f), new Vector3(6f, 3f, 3f), ev, new Acciones
        {
            objetivo = "Busca un cortafierro en la ferretería del mercado (al fondo, a la derecha)"
        });
        zPatio.requiereSinObjeto = "cortafierro";
        Edificio("Tras_Mercado_O", new Vector3(8f, 0f, 74.3f), new Vector3(25.7f, 7f, 80f), "mat_revoque_gris", false, false, false, false, false);
        Edificio("Tras_Mercado_E", new Vector3(40.3f, 0f, 74.3f), new Vector3(50f, 8f, 80f), "mat_ladrillo", false, false, false, false, false);
        Edificio("Mercado_Techo_Vecinos", new Vector3(-8f, 0f, 72f), new Vector3(5f, 12f, 82f), "mat_ladrillo", false, false, false, false, false);
    }

    // =====================================================================
    // ZONA 6: GRADAS (x 30..34, z 80..100) + CALLE DE DOÑA BETY (y = 7.5)
    // =====================================================================
    private static void GradasYCalleSuperior(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var raiz = new GameObject("Z6_Gradas_Calle").transform;
        raiz.SetParent(nivel, false);
        padre = raiz;

        Tramo("Gradas_Tramo1", 32f, 4f, 80f, 89f, 0f, 3.4f, raiz);
        Bloque("Gradas_Descanso", new Vector3(30f, 0f, 89f), new Vector3(34f, 3.4f, 91f), "mat_concreto");
        Tramo("Gradas_Tramo2", 32f, 4f, 91f, 100f, 3.4f, 7.5f, raiz);
        Edificio("Gradas_CasaO", new Vector3(20f, 0f, 80f), new Vector3(30f, 11f, 100f), "mat_revoque_ocre", false, false, true, false, false);
        Edificio("Gradas_CasaE", new Vector3(34f, 0f, 80f), new Vector3(46f, 10f, 100f), "mat_ladrillo", false, false, false, true, false);
        Edificio("Ladera_O", new Vector3(8f, 0f, 80f), new Vector3(20f, 7.3f, 100f), "mat_ladrillo", false, false, false, false, false);
        Edificio("Ladera_E", new Vector3(46f, 0f, 80f), new Vector3(56f, 7.3f, 100f), "mat_revoque_gris", false, false, false, false, false);
        Caja("Baranda_Gradas_O", new Vector3(30.1f, 4.5f, 90f), new Vector3(0.05f, 0.05f, 20f), "mat_metal_oscuro", false);
        Luz("Gradas_Foco", new Vector3(32f, 6.4f, 90f), new Color(1f, 0.75f, 0.45f), 4f, 10f, luces, LuzParpadeante.Modo.Falla);

        // Calle de arriba
        Bloque("Calle_Plataforma", new Vector3(8f, 0f, 100f), new Vector3(56f, 7.5f, 112f), "mat_adoquin");
        Bloque("Calle_Base_N", new Vector3(0f, 0f, 112f), new Vector3(64f, 7.3f, 126f), "mat_concreto");
        Pared("Calle_Parapeto", new Vector3(8f, 7.5f, 100.15f), new Vector3(56f, 7.5f, 100.15f), 1.1f, 0.3f, "mat_concreto", Hueco.Puerta(24f, 4f, 2f));
        Edificio("Calle_FinO", new Vector3(0f, 7.5f, 98f), new Vector3(8f, 18f, 126f), "mat_ladrillo", false, false, true, false, false);
        Edificio("Calle_FinE", new Vector3(56f, 7.5f, 98f), new Vector3(64f, 18f, 126f), "mat_revoque_gris", false, false, false, true, false);
        Modelo("Props/prop_auto_sedan.obj", new Vector3(52f, 7.5f, 102.5f), 80f, "mat_prop_sedan", raiz);
        Modelo("Props/prop_minibus.obj", new Vector3(53f, 7.5f, 109f), 95f, "mat_prop_minibus_blanco", raiz);
        Incendio(new Vector3(53f, 9.2f, 106f), 1.1f, raiz);
        Edificio("Calle_N_O", new Vector3(8f, 7.5f, 112f), new Vector3(20f, 19f, 126f), "mat_ladrillo", false, true, false, false, true);
        Edificio("Calle_N_E", new Vector3(36f, 7.5f, 112f), new Vector3(56f, 20f, 126f), "mat_revoque_verde", false, true, false, false, true);

        // Edificio de doña Bety con zaguan
        Edificio("Casa_Bety_O", new Vector3(20f, 7.5f, 112f), new Vector3(27f, 22f, 126f), "mat_revoque_ocre", false, true, false, false, false);
        Edificio("Casa_Bety_E", new Vector3(29f, 7.5f, 112f), new Vector3(36f, 22f, 126f), "mat_revoque_ocre", false, true, false, false, false);
        Bloque("Casa_Bety_Dintel", new Vector3(27f, 10.7f, 112f), new Vector3(29f, 22f, 126f), "mat_revoque_ocre");
        Bloque("Zaguan_Piso", new Vector3(27f, 7.3f, 112f), new Vector3(29f, 7.5f, 117f), "mat_baldosa");
        Bloque("Zaguan_Fondo", new Vector3(27f, 7.5f, 117f), new Vector3(29f, 10.7f, 117.3f), "mat_madera");
        Luz("Zaguan_Luz", new Vector3(28f, 10.2f, 115f), new Color(1f, 0.8f, 0.55f), 2.5f, 6f, luces);
        var porton = Puerta("Porton_Bety", new Vector3(28f, 7.5f, 112f), 0f, 2f, 3.2f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", raiz);
        porton.trabada = true;
        porton.mensajeTrabada = "Está cerrado con cadena por dentro. ¡Doña Bety!";
        porton.angulo = -100f;
        porton.ruidoAlAbrir = 0f;
        Caja("Balcon_Bety", new Vector3(24f, 10.9f, 111.3f), new Vector3(2.6f, 0.15f, 1.4f), "mat_concreto");
        Caja("Balcon_Baranda", new Vector3(24f, 11.4f, 110.65f), new Vector3(2.6f, 0.9f, 0.05f), "mat_metal_oscuro", false);
        Caja("Ventana_Bety", new Vector3(24f, 12.2f, 111.95f), new Vector3(1.6f, 2f, 0.08f), "mat_luz_calida", false);
        Luz("Luz_Bety", new Vector3(24f, 12.6f, 110.8f), new Color(1f, 0.75f, 0.5f), 3f, 7f, luces);
        CrearNPC("Rig_Bety", "Bety_Balcon", new Vector3(24f, 10.98f, 111.3f), 180f, null, PoseNPC.Pose.DePie, true, pers, AnimProcedural.Estilo.Anciana);

        Poste(new Vector3(16f, 7.5f, 101.2f), 90f, raiz, luces, true);
        Poste(new Vector3(44f, 7.5f, 101.2f), 90f, raiz, luces, true);

        // Infectados de las gradas
        CrearInfectado("Infectado_Descanso", new Vector3(32f, 3.4f, 90f), 180f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 1.2f);
        CrearInfectado("Corredor_Gradas", new Vector3(33f, 7.5f, 103f), 180f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 2f);

        // Asedio final
        var asedioGo = new GameObject("Asedio_Final");
        asedioGo.transform.SetParent(ev, false);
        asedioGo.transform.position = new Vector3(32f, 7.5f, 106f);
        var asedio = asedioGo.AddComponent<EventoAsedio>();
        asedio.duracion = 50f;
        System.Func<string, Vector3, Infectado.Tipo, Infectado> ola = (n, p, t) => CrearInfectado(n, p, 180f, t, Infectado.Estado.Deambular, pers, 3f, null, false);
        asedio.oleadas = new[]
        {
            new EventoAsedio.Oleada { tiempo = 3f, infectados = new[] { ola("Asedio_1", new Vector3(32f, 0f, 82f), Infectado.Tipo.Comun), ola("Asedio_2", new Vector3(31f, 3.4f, 90.2f), Infectado.Tipo.Comun) } },
            new EventoAsedio.Oleada { tiempo = 12f, infectados = new[] { ola("Asedio_3", new Vector3(50f, 7.5f, 106f), Infectado.Tipo.Corredor) } },
            new EventoAsedio.Oleada { tiempo = 22f, infectados = new[] { ola("Asedio_4", new Vector3(50f, 7.5f, 108.5f), Infectado.Tipo.Comun), ola("Asedio_5", new Vector3(50.5f, 7.5f, 104f), Infectado.Tipo.Comun) } },
            new EventoAsedio.Oleada { tiempo = 32f, infectados = new[] { ola("Asedio_6", new Vector3(32f, 0f, 81f), Infectado.Tipo.Corredor), ola("Asedio_7", new Vector3(10f, 7.5f, 106f), Infectado.Tipo.Comun) } },
            new EventoAsedio.Oleada { tiempo = 42f, infectados = new[] { ola("Asedio_8", new Vector3(50f, 7.5f, 105f), Infectado.Tipo.Comun) } }
        };
        asedio.alTerminar = new Acciones
        {
            abrirPuerta = porton,
            progresoCielo = 1f,
            objetivo = "¡Entra al edificio!",
            subtitulos = new[] { "Bety|¡Mateo! ¡Ya está, entra, rápido!" }
        };

        Zona("Z_Tope_Gradas", new Vector3(32f, 9f, 101.8f), new Vector3(4f, 3f, 2.6f), ev, new Acciones
        {
            puntoControl = true,
            progresoCielo = 0.9f,
            iniciarAsedio = asedio,
            objetivo = "Resiste hasta que doña Bety abra el portón",
            subtitulos = new[]
            {
                "Bety|¡Mateo! ¡Hijito, gracias a Dios!",
                "Mateo|¡Doña Bety! ¡Ábrame!",
                "Bety|¡El portón está con cadena! ¡Espérame, ya bajo! ¡Aguanta!"
            }
        });
        Zona("Z_Final", new Vector3(28f, 9f, 114f), new Vector3(2f, 3f, 3f), ev, new Acciones { iniciarFinal = true });
    }

    /// <summary>Tramo de gradas: rampa invisible para caminar + escalones visibles.</summary>
    private static void Tramo(string n, float cx, float ancho, float z0, float z1, float y0, float y1, Transform p)
    {
        float largo = z1 - z0, alto = y1 - y0;
        float ang = Mathf.Atan2(alto, largo) * Mathf.Rad2Deg;
        float hip = Mathf.Sqrt(largo * largo + alto * alto);
        var rampa = new GameObject(n + "_Rampa");
        rampa.transform.SetParent(p, false);
        Vector3 medio = new Vector3(cx, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f);
        Quaternion rot = Quaternion.Euler(-ang, 0f, 0f);
        rampa.transform.SetPositionAndRotation(medio - rot * Vector3.up * 0.15f, rot);
        var bc = rampa.AddComponent<BoxCollider>();
        bc.size = new Vector3(ancho, 0.3f, hip + 0.1f);

        int escalones = Mathf.Max(4, Mathf.RoundToInt(alto / 0.17f));
        float huella = largo / escalones, contra = alto / escalones;
        for (int i = 0; i < escalones; i++)
        {
            float yTop = y0 + contra * (i + 1);
            float z = z0 + huella * (i + 0.5f);
            Caja(n + "_Escalon_" + i, new Vector3(cx, (y0 + yTop) * 0.5f - 0.02f, z), new Vector3(ancho, yTop - y0, huella), "mat_concreto", false);
        }
        Caja(n + "_Muro_O", new Vector3(cx - ancho * 0.5f - 0.15f, (y0 + y1) * 0.5f + 0.2f, (z0 + z1) * 0.5f), new Vector3(0.3f, alto + 1.4f, largo), "mat_ladrillo");
        Caja(n + "_Muro_E", new Vector3(cx + ancho * 0.5f + 0.15f, (y0 + y1) * 0.5f + 0.2f, (z0 + z1) * 0.5f), new Vector3(0.3f, alto + 1.4f, largo), "mat_ladrillo");
    }
}
