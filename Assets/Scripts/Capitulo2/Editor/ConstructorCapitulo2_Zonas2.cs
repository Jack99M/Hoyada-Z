using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Capitulo 2, zonas 4 y 5: estacion del teleferico (vestibulo, pasillo de mantenimiento y sala de
/// transformadores con el generador), anden superior, pasarela del cable con las dos cabinas rojas
/// varadas sobre la Av. 6 de Marzo, anden de descarga oeste y escaleras de emergencia.
/// </summary>
public static partial class ConstructorCapitulo1
{
    private static GameObject lucesEmergencia;

    // =====================================================================
    // Z4 — ESTACION: VESTIBULO, PASILLO Y SALA DE TRANSFORMADORES
    // =====================================================================
    private static void C2_Estacion(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z4_Estacion").transform;
        r.SetParent(nivel, false);
        padre = r;

        lucesEmergencia = new GameObject("Luces_Emergencia");
        lucesEmergencia.transform.SetParent(luces, false);
        Transform le = lucesEmergencia.transform;

        // Envolvente
        Bloque("Estacion_Piso", new Vector3(-20f, -0.2f, 124f), new Vector3(31f, 0f, 156f), "mat_baldosa");
        Bloque("Estacion_Anexo_O_S", new Vector3(-24f, -6f, 124f), new Vector3(-20f, 16f, 146.3f), "mat_concreto");
        Bloque("Estacion_Anexo_O_N", new Vector3(-24f, -6f, 148.7f), new Vector3(-20f, 16f, 156f), "mat_concreto");
        Bloque("Estacion_Anexo_O_Bajo", new Vector3(-24f, -6f, 146.3f), new Vector3(-20f, 7.7f, 148.7f), "mat_concreto");
        Bloque("Estacion_Anexo_O_Alto", new Vector3(-24f, 10.6f, 146.3f), new Vector3(-20f, 16f, 148.7f), "mat_concreto");
        Pared("Estacion_Muro_E", new Vector3(31f, 0f, 124f), new Vector3(31f, 0f, 156f), 16f, 0.4f, "mat_revoque_gris", Hueco.Puerta(7f, 1.4f, 2.4f));
        Pared("Estacion_Muro_N", new Vector3(-20f, 0f, 156f), new Vector3(31f, 0f, 156f), 16f, 0.4f, "mat_revoque_gris");
        Pared("Estacion_Muro_O", new Vector3(-20f, 0f, 124f), new Vector3(-20f, 0f, 156f), 16f, 0.4f, "mat_revoque_gris", new Hueco(23.5f, 2.4f, 8f, 10.6f));
        Bloque("Estacion_Muro_S_Oficinas", new Vector3(12f, 0f, 123.7f), new Vector3(31f, 16f, 124.3f), "mat_revoque_gris");
        Bloque("Estacion_Techo", new Vector3(-20.3f, 16f, 123.6f), new Vector3(31.3f, 16.4f, 156.3f), "mat_calamina");
        Caja("Estacion_Franja_Roja", new Vector3(5.5f, 14.8f, 123.5f), new Vector3(51f, 1.2f, 0.1f), "mat_teleferico_rojo", false);

        // Fachada de vidrio (doble altura) con la entrada principal trancada
        for (int i = 0; i <= 10; i++)
        {
            float x = -20f + i * 3.2f;
            Caja("Fachada_Parante_" + i, new Vector3(x, 8f, 124f), new Vector3(0.25f, 16f, 0.3f), "mat_metal_oscuro");
            if (i == 10) { break; }
            float cx = x + 1.6f;
            bool puerta = cx > 1f && cx < 9f;
            if (!puerta) { Caja("Fachada_Vidrio_" + i, new Vector3(cx, 8f, 124f), new Vector3(2.95f, 16f, 0.05f), "mat_vidrio"); }
            else { Caja("Fachada_Vidrio_Alto_" + i, new Vector3(cx, 9.4f, 124f), new Vector3(2.95f, 13.2f, 0.05f), "mat_vidrio"); }
        }
        for (int k = 1; k <= 3; k++) { Caja("Fachada_Travesano_" + k, new Vector3(-4f, k * 4f, 124f), new Vector3(32f, 0.2f, 0.32f), "mat_metal_oscuro", false); }
        Caja("Entrada_Barricada_Sofa", new Vector3(3f, 0.5f, 125f), new Vector3(2.2f, 1f, 0.9f), "mat_tela_sofa", true, 5f);
        Caja("Entrada_Barricada_Mesa", new Vector3(5.2f, 1.4f, 125.1f), new Vector3(1.8f, 0.9f, 1f), "mat_madera", true, -12f);
        Caja("Entrada_Barricada_Lockers", new Vector3(7.3f, 1.1f, 125.2f), new Vector3(1.6f, 2.2f, 0.6f), "mat_metal_pintado", true, 8f);
        Caja("Entrada_Barricada_Bloque", new Vector3(5f, 1.4f, 124.6f), new Vector3(8f, 2.8f, 0.3f), "mat_carton", true);
        for (int k = 0; k < 5; k++) { Caja("Entrada_Alambre_" + k, new Vector3(5f, 0.5f + k * 0.5f, 123.6f), new Vector3(8f, 0.02f, 0.02f), "mat_cable", false, (k % 2 == 0 ? 2f : -2f)); }
        Letrero("linea_roja", new Vector3(5f, 3.6f, 123.5f), Vector3.back, 7f, r);

        // Losa del anden superior (y = 8) con el hueco de la escalera
        Bloque("Anden_Losa_A", new Vector3(-14.4f, 7.7f, 136f), new Vector3(31f, 8f, 156f), "mat_baldosa");
        Bloque("Anden_Losa_B", new Vector3(-20f, 7.7f, 140f), new Vector3(-14.4f, 8f, 156f), "mat_baldosa");
        Bloque("Anden_Losa_C", new Vector3(12f, 7.7f, 124.3f), new Vector3(31f, 8f, 136f), "mat_baldosa");
        Baranda("Anden_Baranda_Vacio", new Vector3(-14.4f, 8f, 136f), new Vector3(12f, 8f, 136f), r);
        Baranda("Anden_Baranda_Vacio_E", new Vector3(12f, 8f, 124.3f), new Vector3(12f, 8f, 136f), r);
        Baranda("Anden_Baranda_Hueco", new Vector3(-14.4f, 8f, 136f), new Vector3(-14.4f, 8f, 140f), r);

        // Escalera al anden y su reja electrica
        Escalera("Estacion_Escalera", new Vector3(-17f, 0f, 128f), new Vector3(-17f, 8f, 140f), 4f, true, r);
        var rejaEsc = Puerta("Reja_Escaleras", new Vector3(-17f, 0f, 127.6f), 0f, 4.2f, 3f, PuertaCap.Tipo.Reja, "mat_fierro", r);
        rejaEsc.trabada = true;
        rejaEsc.mensajeTrabada = "La reja eléctrica de las escaleras. Sin energía no se abre.";

        // Oficinas (macizo), pasillo de mantenimiento y sala de transformadores
        Bloque("Estacion_Oficinas", new Vector3(12f, 0f, 124.3f), new Vector3(30.8f, 7.7f, 129f), "mat_revoque_gris");
        Pared("Pasillo_Muro_N", new Vector3(12f, 0f, 133f), new Vector3(30.8f, 0f, 133f), 7.7f, 0.3f, "mat_concreto", Hueco.Puerta(3f, 1.6f, 2.4f));
        Pared("Pasillo_Muro_O", new Vector3(12f, 0f, 129f), new Vector3(12f, 0f, 133f), 7.7f, 0.3f, "mat_concreto", Hueco.Puerta(2f, 1.4f, 2.4f));
        Pared("Sala_Muro_O", new Vector3(12f, 0f, 133f), new Vector3(12f, 0f, 156f), 7.7f, 0.3f, "mat_concreto");
        var servicio = Puerta("Puerta_Servicio", new Vector3(12f, 0f, 131f), 90f, 1.4f, 2.4f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", r);
        servicio.trabada = true;
        servicio.mensajeTrabada = "Cerradura eléctrica. Sin energía no abre.";
        var mant = Puerta("Puerta_Mantenimiento", new Vector3(31f, 0f, 131f), 90f, 1.4f, 2.4f, PuertaCap.Tipo.Bisagra, "mat_metal_pintado", r);
        mant.llaveRequerida = "tarjeta_mantenimiento";
        mant.nombreLlave = "Tarjeta de mantenimiento";
        mant.mensajeCerrada = "Lector de tarjeta. «Solo personal de Mi Teleférico».";
        mant.alAbrir = new Acciones
        {
            objetivo = "Restablece la energía en la sala de transformadores",
            usarMarcador = true, marcador = new Vector3(22.3f, 0f, 152.8f),
            subtitulos = new[] { "Tito|Está oscuro como boca de lobo. Prende tu linterna, Wara.", "Tito|La sala de transformadores está al fondo. Si arranco el generador de emergencia, vuelve la luz." },
            mensaje = "[F] linterna  ·  [Z] escuchar",
            puntoControl = true,
            progresoCielo = 0.35f
        };
        Luz("Pasillo_Emergencia_Roja", new Vector3(28f, 2.6f, 131f), new Color(1f, 0.15f, 0.1f), 0.5f, 5f, luces, LuzParpadeante.Modo.Falla);
        Luz("Sala_Emergencia_Roja", new Vector3(20f, 3.5f, 150f), new Color(1f, 0.15f, 0.1f), 0.45f, 7f, luces, LuzParpadeante.Modo.Falla);
        CrearInfectado("Pasillo_Dormido", new Vector3(20f, 0f, 130.3f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 1f, "mat_char_tito");
        CrearInfectado("Fungico_Sala", new Vector3(18f, 0f, 144f), 0f, Infectado.Tipo.Fungico, Infectado.Estado.Deambular, pers, 4f);
        for (int i = 0; i < 3; i++)
        {
            Caja("Transformador_" + i, new Vector3(28.6f, 1.2f, 137f + i * 4f), new Vector3(2.2f, 2.4f, 2.6f), "mat_metal_pintado");
            Caja("Transformador_Aletas_" + i, new Vector3(27.4f, 1.2f, 137f + i * 4f), new Vector3(0.2f, 2f, 2.2f), "mat_metal_oscuro", false);
        }
        Caja("Generador", new Vector3(25.5f, 0.9f, 153.6f), new Vector3(3.2f, 1.8f, 2.2f), "mat_metal_oscuro");
        Caja("Generador_Escape", new Vector3(26.5f, 3.5f, 154.4f), new Vector3(0.3f, 3.6f, 0.3f), "mat_fierro", false);
        Caja("Tablero_Palanca", new Vector3(22.3f, 1.2f, 154.6f), new Vector3(1f, 1.4f, 0.3f), "mat_metal_pintado");
        var palancaArriba = Caja("Palanca_Arriba", new Vector3(22.3f, 1.55f, 154.35f), new Vector3(0.08f, 0.5f, 0.08f), "mat_toldo_rojo", false);
        var palancaAbajo = Caja("Palanca_Abajo", new Vector3(22.3f, 1.05f, 154.3f), new Vector3(0.08f, 0.5f, 0.08f), "mat_neon_verde", false);
        palancaAbajo.SetActive(false);
        Caja("Sala_Banco", new Vector3(15.2f, 0.45f, 150f), new Vector3(0.9f, 0.9f, 3f), "mat_madera");
        Caja("Sala_Lockers", new Vector3(13f, 1f, 140f), new Vector3(0.6f, 2f, 3f), "mat_metal_pintado");
        for (int i = 0; i < 4; i++) { Caja("Sala_Bandeja_Cable_" + i, new Vector3(21f, 6.5f, 135f + i * 5.5f), new Vector3(17f, 0.15f, 0.5f), "mat_cable", false); }
        Cadaver("Tecnico_Muerto", new Vector3(16.5f, 0f, 152f), 200f, "mat_char_tito", r);
        Suministro("Cinta_Sala", new Vector3(15.2f, 0.92f, 149f), Recogible.TipoObjeto.Cinta, 1, r);
        Suministro("Cuchilla_Sala", new Vector3(15.2f, 0.92f, 151f), Recogible.TipoObjeto.Cuchilla, 1, r);
        Objeto("Pilas_Sala", new Vector3(13.4f, 0f, 146f), Recogible.TipoObjeto.Pilas, 1, r);
        var ruidoGenerador = Sonido("Generador_Ruido", new Vector3(25.5f, 1.2f, 153.6f), "amb_generador", 1f, 32f, r);
        ruidoGenerador.gameObject.SetActive(false);

        // Asedio: el motor atrae a los corredores del exterior
        var olas = new List<Infectado[]>
        {
            new[] { CrearInfectado("Gen_Corredor_1", new Vector3(33.5f, 0f, 127f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Gen_Corredor_2", new Vector3(34.5f, 0f, 128.5f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false) },
            new[] { CrearInfectado("Gen_Comun_1", new Vector3(33.5f, 0f, 126f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_civil", false),
                    CrearInfectado("Gen_Corredor_3", new Vector3(35f, 0f, 127f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false) },
            new[] { CrearInfectado("Gen_Comun_2", new Vector3(34f, 0f, 125.5f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Gen_Comun_3", new Vector3(35f, 0f, 129f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_policia", false) }
        };
        var asedio = Asedio("Asedio_Generador", new Vector3(22f, 0f, 145f), 40f, ev);
        asedio.oleadas = new[]
        {
            new EventoAsedio.Oleada { tiempo = 4f, infectados = olas[0], aviso = "Tito|¡Vienen por el pasillo! ¡Son rápidos!" },
            new EventoAsedio.Oleada { tiempo = 16f, infectados = olas[1], aviso = "Tito|¡Más! ¡No los dejes pasar la puerta!" },
            new EventoAsedio.Oleada { tiempo = 28f, infectados = olas[2], aviso = "Tito|¡Ya casi carga, aguanta!" }
        };
        asedio.alTerminar = new Acciones
        {
            activar = new[] { lucesEmergencia },
            destrabarPuertas = new[] { servicio },
            abrirPuerta = rejaEsc,
            companera = 4,
            objetivo = "Sube las escaleras hacia el andén superior",
            usarMarcador = true, marcador = new Vector3(-17f, 0f, 127f),
            subtitulos = new[] { "Tito|¡Hay luz! ¡Se abrió la reja de las escaleras!", "Tito|Por el vestíbulo, pasando los torniquetes. ¡Vamos arriba!" },
            sonido = "sfx_click",
            puntoControl = true,
            progresoCielo = 0.45f
        };
        var palanca = Accion("Palanca_Generador", new Vector3(22.3f, 0f, 153.4f), 0f, "Arrancar el generador (Tito sostiene la palanca)", r);
        palanca.requiereCompanero = true;
        palanca.mensajeSinCompanero = "La palanca vuelve sola si la suelto. Necesito que Tito la sostenga.";
        palanca.duracion = 1.2f;
        palanca.sonido = "sfx_golpe_arma";
        palanca.companeroTrabaja = true;
        palanca.visualAntes = palancaArriba;
        palanca.visualDespues = palancaAbajo;
        palanca.puntoTrabajo = new Vector3(23.3f, 0f, 153.4f);
        palanca.alUsar = new Acciones
        {
            activar = new[] { ruidoGenerador.gameObject },
            iniciarAsedio = asedio,
            objetivo = "Cubre la puerta mientras Tito sostiene la palanca",
            usarMarcador = true, marcador = new Vector3(15f, 0f, 133.5f),
            subtitulos = new[] { "Tito|¡Arrancó! ¡Pero hace un escándalo!", "Tito|¡Wara, cuida la puerta! ¡Si suelto la palanca se apaga todo!" },
            mensaje = "Protege a Tito: la puerta de la sala da al pasillo",
            puntoControl = true
        };

        // Vestibulo (se enciende con el generador)
        for (int i = 0; i < 9; i++) { Caja("Torniquete_" + i, new Vector3(-10f + i * 2.4f, 0.5f, 138f), new Vector3(0.35f, 1f, 1f), "mat_metal_pintado"); }
        Caja("Boleteria", new Vector3(6f, 1.2f, 146f), new Vector3(3f, 2.4f, 2f), "mat_teleferico_rojo");
        Caja("Boleteria_Vidrio", new Vector3(6f, 1.7f, 144.95f), new Vector3(2.6f, 1f, 0.05f), "mat_vidrio", false);
        for (int i = 0; i < 3; i++) { Banca(new Vector3(-8f + i * 6f, 0f, 129f), 0f, r); }
        Cartel("Aviso_Teleferico", new Vector3(-6f, 1.7f, 155.75f), 180f, "Aviso de Mi Teleférico",
            "LÍNEA ROJA — SERVICIO SUSPENDIDO\n\nPor un corte general de energía, la línea se detuvo a las 06:12.\nEl personal técnico evacuó a los pasajeros de las cabinas.\nDos cabinas permanecen detenidas junto a la pasarela de mantenimiento del cable (lado oeste).\n\nSe prohíbe el ingreso a la pasarela de mantenimiento.\n\n— Mi Teleférico, Empresa Estatal de Transporte por Cable", r);
        CrearInfectado("Vestibulo_Dormido_1", new Vector3(-5f, 0f, 146f), 30f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 2f);
        CrearInfectado("Vestibulo_Dormido_2", new Vector3(4f, 0f, 151f), 250f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 2f, "mat_char_civil");
        CrearInfectado("Vestibulo_Griton", new Vector3(-11f, 0f, 152f), 120f, Infectado.Tipo.Griton, Infectado.Estado.Quieto, pers, 2f);
        Cadaver("Vestibulo_Cadaver", new Vector3(0f, 0f, 132f), 60f, null, r);
        Suministro("Molotov_Vestibulo", new Vector3(6f, 0f, 143.6f), Recogible.TipoObjeto.Molotov, 1, r);
        Objeto("Venda_Vestibulo", new Vector3(-13f, 0f, 150f), Recogible.TipoObjeto.Venda, 1, r);
        Objeto("Botella_Vestibulo", new Vector3(9f, 0f, 130f), Recogible.TipoObjeto.Botella, 1, r);
        Altar("Altar_Vestibulo", new Vector3(-12f, 0f, 125.2f), 0f, "Vestíbulo del teleférico", r, luces);
        Luz("Emergencia_Vestibulo_1", new Vector3(-8f, 7f, 132f), new Color(1f, 0.92f, 0.8f), 1.3f, 14f, le);
        Luz("Emergencia_Vestibulo_2", new Vector3(6f, 7f, 132f), new Color(1f, 0.92f, 0.8f), 1.3f, 14f, le);
        Luz("Emergencia_Vestibulo_3", new Vector3(-6f, 7f, 148f), new Color(1f, 0.92f, 0.8f), 1.2f, 12f, le);
        Luz("Emergencia_Vestibulo_4", new Vector3(6f, 7f, 150f), new Color(1f, 0.92f, 0.8f), 1.2f, 12f, le);
        Luz("Emergencia_Pasillo", new Vector3(21f, 2.8f, 131f), new Color(1f, 0.92f, 0.8f), 0.9f, 10f, le);
        Luz("Emergencia_Sala", new Vector3(21f, 6f, 145f), new Color(1f, 0.92f, 0.8f), 1.2f, 14f, le);
        Luz("Emergencia_Escalera", new Vector3(-17f, 6f, 134f), new Color(1f, 0.92f, 0.8f), 1f, 10f, le);
    }

    // =====================================================================
    // Z5 — ANDEN SUPERIOR, CABINAS VARADAS Y ANDEN OESTE
    // =====================================================================
    private static void C2_Cabinas(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z5_Cabinas").transform;
        r.SetParent(nivel, false);
        padre = r;
        Transform le = lucesEmergencia.transform;

        // Anden superior
        var rueda = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rueda.name = "Anden_Rueda_Motriz";
        Object.DestroyImmediate(rueda.GetComponent<Collider>());
        rueda.transform.SetParent(r, false);
        rueda.transform.position = new Vector3(4f, 11.5f, 147.5f);
        rueda.transform.localScale = new Vector3(7f, 0.15f, 7f);
        rueda.GetComponent<Renderer>().sharedMaterial = Mat("mat_metal_oscuro");
        Caja("Anden_Eje", new Vector3(4f, 13.7f, 147.5f), new Vector3(0.5f, 4.6f, 0.5f), "mat_fierro", false);
        Caja("Anden_Borde_Amarillo", new Vector3(4f, 8.01f, 143f), new Vector3(20f, 0.02f, 0.3f), "mat_barrera", false);
        CabinaVisual("Cabina_Estacionada_1", new Vector3(10f, 8f, 151.5f), r);
        CabinaVisual("Cabina_Estacionada_2", new Vector3(15f, 8f, 151.5f), r);
        Caja("Anden_Mostrador", new Vector3(22f, 8.5f, 139f), new Vector3(2.4f, 1f, 0.8f), "mat_madera");
        Doc("Registro_Operacion", new Vector3(22f, 9.02f, 139f), "Registro de operación — Línea Roja",
            "06:12  Corte general. La línea se detiene. 38 cabinas en ruta.\n" +
            "06:15  Freno de emergencia OK. Sin energía para el motor auxiliar.\n" +
            "06:50  Rescate con cuerdas de las cabinas sobre la 6 de Marzo. Algunos pasajeros con fiebre alta, agresivos.\n" +
            "08:20  Las cabinas 17 y 18 quedan pegadas a la pasarela de mantenimiento. No se pueden mover.\n" +
            "08:40  Orden de evacuar la estación. El generador de emergencia queda en la sala de transformadores.", r);
        CrearInfectado("Anden_Comun_1", new Vector3(0f, 8f, 141.5f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 4f);
        CrearInfectado("Anden_Comun_2", new Vector3(18f, 8f, 146f), 200f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 4f, "mat_char_civil");
        CrearInfectado("Anden_Corredor", new Vector3(-8f, 8f, 153f), 180f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 2f);
        Luz("Emergencia_Anden_1", new Vector3(-8f, 14f, 147f), new Color(1f, 0.92f, 0.8f), 1.4f, 16f, le);
        Luz("Emergencia_Anden_2", new Vector3(12f, 14f, 147f), new Color(1f, 0.92f, 0.8f), 1.4f, 16f, le);
        Luz("Emergencia_Anden_3", new Vector3(24f, 12f, 132f), new Color(1f, 0.92f, 0.8f), 1f, 12f, le);
        Zona("Z_Anden_Superior", new Vector3(-17f, 9.5f, 141f), new Vector3(5f, 3f, 3f), ev, new Acciones
        {
            objetivo = "Atraviesa las cabinas detenidas sobre la avenida",
            usarMarcador = true, marcador = new Vector3(-40f, 8f, 147.5f),
            subtitulos = new[] { "Tito|Mira: las dos cabinas quedaron colgadas junto a la pasarela del cable.", "Wara|¿Vamos a caminar por ahí? ...Ya. No mires abajo, no mires abajo." },
            musica = "mus_ceja",
            progresoCielo = 0.55f
        });

        // Pasarela de mantenimiento del cable sobre la Av. 6 de Marzo
        Bloque("Cable_Pasarela", new Vector3(-62.5f, 7.7f, 146.3f), new Vector3(-20f, 8f, 148.7f), "mat_metal_oscuro");
        float[,] tramos = { { -29.5f, -24f }, { -47.5f, -33f }, { -62.5f, -51f } };
        for (int i = 0; i < 3; i++)
        {
            Baranda("Cable_Baranda_S_" + i, new Vector3(tramos[i, 0], 8f, 146.3f), new Vector3(tramos[i, 1], 8f, 146.3f), r);
            Baranda("Cable_Baranda_N_" + i, new Vector3(tramos[i, 0], 8f, 148.7f), new Vector3(tramos[i, 1], 8f, 148.7f), r);
        }
        Invisible("Cable_S", new Vector3(-62.5f, 8f, 145.95f), new Vector3(-24f, 12f, 146.3f), r);
        Invisible("Cable_N", new Vector3(-62.5f, 8f, 148.7f), new Vector3(-24f, 12f, 149.05f), r);
        CabinaVarada("Cabina_Varada_17", -31.25f, r);
        CabinaVarada("Cabina_Varada_18", -49.25f, r);
        Cable("Cable_Principal_A", new Vector3(-20f, 12.4f, 147.1f), new Vector3(-80f, 12.4f, 147.1f), 0.08f, r);
        Cable("Cable_Principal_B", new Vector3(-20f, 12.4f, 147.9f), new Vector3(-80f, 12.4f, 147.9f), 0.08f, r);
        Caja("Torre_Cable", new Vector3(-42f, 3.2f, 152f), new Vector3(1.2f, 18.4f, 1.2f), "mat_teleferico_rojo");
        Caja("Torre_Cable_Brazo", new Vector3(-42f, 12.5f, 149.8f), new Vector3(0.6f, 0.5f, 5.2f), "mat_teleferico_rojo", false);
        Sonido("Viento_Cable", new Vector3(-40f, 10f, 147.5f), "amb_viento", 0.7f, 40f, r);

        // Fachada oeste de la estacion (lo que se ve desde la pasarela y el anden oeste)
        Caja("Estacion_Oeste_Franja", new Vector3(-24.06f, 14.6f, 140f), new Vector3(0.1f, 1.2f, 32f), "mat_teleferico_rojo", false);
        Caja("Estacion_Oeste_Ventanal_S", new Vector3(-24.06f, 9.4f, 135f), new Vector3(0.1f, 1.8f, 20f), "mat_vidrio", false);
        Caja("Estacion_Oeste_Ventanal_N", new Vector3(-24.06f, 9.4f, 152.4f), new Vector3(0.1f, 1.8f, 7f), "mat_vidrio", false);
        Caja("Estacion_Oeste_Marco_Sup", new Vector3(-24.08f, 10.75f, 147.5f), new Vector3(0.15f, 0.3f, 3f), "mat_teleferico_rojo", false);
        Caja("Estacion_Oeste_Marco_S", new Vector3(-24.08f, 9.2f, 146.15f), new Vector3(0.15f, 3f, 0.3f), "mat_teleferico_rojo", false);
        Caja("Estacion_Oeste_Marco_N", new Vector3(-24.08f, 9.2f, 148.85f), new Vector3(0.15f, 3f, 0.3f), "mat_teleferico_rojo", false);
        Letrero("linea_roja", new Vector3(-24.12f, 12.4f, 138f), Vector3.left, 9f, r);
        CrearInfectado("Pasajero_Cabina_18", new Vector3(-49.9f, 8f, 147.4f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 0.5f, "mat_char_civil");
        Doc("Dibujo_Cabina", new Vector3(-48.4f, 8.47f, 146.25f), "Dibujo con crayones",
            "Un dibujo de una niña: el teleférico rojo, las montañas, un sol con lentes.\nAbajo, con letra grande: «MI MAMÁ Y YO VAMOS A LA FERIA». Está firmado: «Abigail, 7 años».", r);
        Suministro("Piedras_Cabina", new Vector3(-30.5f, 8.47f, 148.75f), Recogible.TipoObjeto.Piedras, 5, r);
        Zona("Z_Antes_Salto", new Vector3(-57f, 9.5f, 147.5f), new Vector3(4f, 3f, 2.4f), ev, new Acciones
        {
            objetivo = "Salta al andén de descarga del lado oeste",
            usarMarcador = true, marcador = new Vector3(-67f, 7.6f, 147.5f),
            subtitulos = new[] { "Tito|Se acabó la pasarela... ¡Hay que saltar, Wara! ¡Con impulso!" },
            mensaje = "Corre con [Shift] y salta con [Espacio]"
        });

        // Anden de descarga oeste (y = 7.6) sobre un edificio macizo
        Bloque("Anden_Oeste_Macizo", new Vector3(-82f, -6f, 136f), new Vector3(-64f, 7.6f, 158f), "mat_concreto");
        Caja("Anden_Oeste_Chapa", new Vector3(-65.2f, 7.62f, 147.5f), new Vector3(2.2f, 0.03f, 3f), "mat_calamina", false);
        Baranda("Anden_Oeste_Baranda_E1", new Vector3(-64f, 7.6f, 136f), new Vector3(-64f, 7.6f, 145.6f), r);
        Baranda("Anden_Oeste_Baranda_E2", new Vector3(-64f, 7.6f, 149.4f), new Vector3(-64f, 7.6f, 158f), r);
        Baranda("Anden_Oeste_Baranda_S", new Vector3(-82f, 7.6f, 136f), new Vector3(-64f, 7.6f, 136f), r);
        Baranda("Anden_Oeste_Baranda_N1", new Vector3(-82f, 7.6f, 158f), new Vector3(-71.3f, 7.6f, 158f), r);
        Baranda("Anden_Oeste_Baranda_N2", new Vector3(-68.7f, 7.6f, 158f), new Vector3(-64f, 7.6f, 158f), r);
        Bloque("Anden_Oeste_Muro_O", new Vector3(-82.4f, 7.6f, 136f), new Vector3(-82f, 12.5f, 158f), "mat_ladrillo");
        Bloque("Anden_Oeste_Techo", new Vector3(-82.4f, 12.5f, 136f), new Vector3(-66f, 12.8f, 158f), "mat_calamina");
        for (int i = 0; i < 4; i++) { Caja("Anden_Oeste_Pilar_" + i, new Vector3(-66.5f, 10.05f, 138f + i * 6.4f), new Vector3(0.35f, 4.9f, 0.35f), "mat_teleferico_rojo"); }
        CabinaVisual("Cabina_Oeste_Estacionada", new Vector3(-76f, 7.6f, 142f), r);
        var griton = CrearInfectado("Anden_Oeste_Griton", new Vector3(-74f, 7.6f, 151f), 90f, Infectado.Tipo.Griton, Infectado.Estado.Dormido, pers, 2f);
        Zona("Z_Chapa_Cruje", new Vector3(-66f, 8.6f, 147.5f), new Vector3(4f, 2.5f, 4f), ev, new Acciones
        {
            sonido = "sfx_chapa",
            despertar = new[] { griton },
            mensaje = "¡La chapa del andén crujió! Un gritón se despertó: elimínalo antes de que grite",
            subtitulos = new[] { "Tito|¡Uf! ...Aquí estoy, aquí estoy." },
            objetivo = "Baja por las escaleras de emergencia al patio trasero",
            usarMarcador = true, marcador = new Vector3(-70f, 7.6f, 157f),
            progresoCielo = 0.65f
        });
        Altar("Altar_Anden_Oeste", new Vector3(-80f, 7.6f, 139f), 90f, "Andén de descarga oeste", r, luces);
        Objeto("Venda_Anden_Oeste", new Vector3(-80.5f, 7.6f, 150f), Recogible.TipoObjeto.Venda, 1, r);
        Objeto("Botella_Anden_Oeste", new Vector3(-72f, 7.6f, 155f), Recogible.TipoObjeto.Botella, 1, r);
        Zona("Z_Balazos", new Vector3(-70f, 9f, 155.8f), new Vector3(5f, 3f, 3f), ev, new Acciones
        {
            sonido = "sfx_disparo",
            puntoControl = true,
            objetivo = "Baja por las escaleras de emergencia al patio trasero",
            usarMarcador = true, marcador = new Vector3(-70f, 0f, 171f),
            subtitulos = new[] { "Tito|¿Escuchaste? ...Balazos. Alguien sigue vivo allá abajo.", "Wara|O algo que antes estaba vivo." }
        });

        // Tito cruza el hueco con un enlace de navegacion
        var enlace = new GameObject("Enlace_Salto_Cable");
        enlace.transform.SetParent(r, false);
        enlace.transform.position = new Vector3(-63.25f, 8f, 147.5f);
        var link = enlace.AddComponent<NavMeshLink>();
        link.startPoint = new Vector3(1.6f, 0f, 0f);
        link.endPoint = new Vector3(-2.2f, -0.4f, 0f);
        link.width = 1.2f;
        link.bidirectional = true;

        // Escaleras de emergencia al patio
        Escalera("Escalera_Emergencia", new Vector3(-70f, 0f, 170f), new Vector3(-70f, 7.6f, 158f), 2.4f, true, r);

        // Av. 6 de Marzo (hundida, infestada): solo se ve desde arriba
        Bloque("Av6Marzo_Piso", new Vector3(-64f, -6.2f, 95f), new Vector3(-24f, -6f, 205f), "mat_asfalto");
        Bloque("Av6Marzo_Muro_E", new Vector3(-24.4f, -6f, 95f), new Vector3(-24f, 0f, 124f), "mat_concreto");
        Edificio("Av6Marzo_Edif_O", new Vector3(-92f, -6f, 95f), new Vector3(-64f, 14f, 136f), "mat_ladrillo", false, false, true, false);
        Edificio("Av6Marzo_Edif_N", new Vector3(-64f, -6f, 205f), new Vector3(-24f, 14f, 215f), "mat_revoque_ocre", false, true, false, false);
        Edificio("Tras_Estacion_Edif", new Vector3(-24f, -6f, 156f), new Vector3(12f, 16f, 205f), "mat_ladrillo", false, false, false, true);
        Bloque("Patio_Parapeto_Avenida", new Vector3(-64.5f, -6f, 158f), new Vector3(-63.5f, 1.2f, 204.4f), "mat_concreto");
        string[] autos = { "Props/prop_minibus.obj", "Props/prop_auto_sedan.obj", "Props/prop_auto_sedan.obj", "Props/prop_minibus.obj", "Props/prop_auto_sedan.obj", "Props/prop_minibus.obj", "Props/prop_auto_sedan.obj" };
        string[] matsAuto = { "mat_prop_minibus_blanco", "mat_prop_sedan", "mat_metal_pintado", "mat_prop_minibus_celeste", "mat_prop_sedan", "mat_prop_minibus_blanco", "mat_metal_pintado" };
        for (int i = 0; i < autos.Length; i++)
        {
            Modelo(autos[i], new Vector3(-54f + (i % 3) * 13f, -6f, 104f + i * 13f), (i * 53f) % 360f, matsAuto[i], r);
        }
        Incendio(new Vector3(-40f, -6f, 118f), 1.5f, r);
        Incendio(new Vector3(-50f, -6f, 165f), 1.8f, r);
        Incendio(new Vector3(-33f, -6f, 190f), 1.3f, r);
        Luz("Av6_Fuego_1", new Vector3(-40f, -4.5f, 118f), new Color(1f, 0.55f, 0.2f), 2.4f, 14f, luces, LuzParpadeante.Modo.Fuego);
        Luz("Av6_Fuego_2", new Vector3(-50f, -4.5f, 165f), new Color(1f, 0.55f, 0.2f), 2.6f, 14f, luces, LuzParpadeante.Modo.Fuego);
        Luz("Av6_Fuego_3", new Vector3(-33f, -4.5f, 190f), new Color(1f, 0.55f, 0.2f), 2.2f, 14f, luces, LuzParpadeante.Modo.Fuego);
        for (int i = 0; i < 9; i++)
        {
            CrearInfectado("Av6_Infectado_" + i, new Vector3(-58f + (i % 4) * 10f, -6f, 110f + i * 10f), i * 40f, i % 4 == 1 ? Infectado.Tipo.Corredor : Infectado.Tipo.Comun, i % 3 == 0 ? Infectado.Estado.Comiendo : Infectado.Estado.Deambular, pers, 6f, i % 2 == 0 ? "mat_char_civil" : null);
        }
        for (int i = 0; i < 6; i++) { Cadaver("Av6_Cadaver_" + i, new Vector3(-55f + (i % 3) * 12f, -6f, 112f + i * 14f), i * 50f, i % 2 == 0 ? "mat_char_civil" : null, r); }
        Sonido("Av6_Sirena", new Vector3(-44f, -2f, 150f), "amb_sirena", 0.4f, 90f, r);

        // La estacion no tiene luz hasta que Tito arranca el generador
        lucesEmergencia.SetActive(false);
    }

    /// <summary>Cabina roja varada: se camina por adentro (puertas abiertas en los extremos).</summary>
    private static void CabinaVarada(string n, float cx, Transform p)
    {
        var raiz = new GameObject(n).transform;
        raiz.SetParent(p, false);
        foreach (float z in new[] { 145.95f, 149.05f })
        {
            Caja(n + "_Lado_Bajo", new Vector3(cx, 8.5f, z), new Vector3(3.5f, 1f, 0.1f), "mat_teleferico_rojo", true, 0f, raiz);
            Caja(n + "_Lado_Ventana", new Vector3(cx, 9.4f, z), new Vector3(3.5f, 0.8f, 0.06f), "mat_vidrio", true, 0f, raiz);
            Caja(n + "_Lado_Alto", new Vector3(cx, 10.1f, z), new Vector3(3.5f, 0.6f, 0.1f), "mat_teleferico_rojo", true, 0f, raiz);
            Caja(n + "_Asiento", new Vector3(cx, 8.22f, z + (z < 147.5f ? 0.3f : -0.3f)), new Vector3(3f, 0.45f, 0.4f), "mat_tela_sofa", false, 0f, raiz);
        }
        foreach (float x in new[] { cx - 1.75f, cx + 1.75f })
        {
            Caja(n + "_Frente_S", new Vector3(x, 9.35f, 146.3f), new Vector3(0.1f, 2.7f, 0.8f), "mat_teleferico_rojo", true, 0f, raiz);
            Caja(n + "_Frente_N", new Vector3(x, 9.35f, 148.7f), new Vector3(0.1f, 2.7f, 0.8f), "mat_teleferico_rojo", true, 0f, raiz);
            Caja(n + "_Dintel", new Vector3(x, 10.4f, 147.5f), new Vector3(0.1f, 0.6f, 1.6f), "mat_teleferico_rojo", true, 0f, raiz);
        }
        Caja(n + "_Techo", new Vector3(cx, 10.75f, 147.5f), new Vector3(3.7f, 0.12f, 3.3f), "mat_teleferico_rojo", true, 0f, raiz);
        Caja(n + "_Colgante", new Vector3(cx, 11.6f, 147.5f), new Vector3(0.14f, 1.6f, 0.14f), "mat_fierro", false, 0f, raiz);
        Caja(n + "_Pinza", new Vector3(cx, 12.4f, 147.5f), new Vector3(0.9f, 0.3f, 1.2f), "mat_metal_oscuro", false, 0f, raiz);
    }

    /// <summary>Cabina roja cerrada (decorado en los andenes).</summary>
    private static void CabinaVisual(string n, Vector3 pos, Transform p)
    {
        Caja(n, pos + Vector3.up * 1.35f, new Vector3(3.2f, 2.7f, 3f), "mat_teleferico_rojo");
        Caja(n + "_Ventanas", pos + Vector3.up * 1.7f, new Vector3(3.25f, 0.8f, 2.6f), "mat_vidrio", false);
        Caja(n + "_Colgante", pos + Vector3.up * 3.4f, new Vector3(0.14f, 1.4f, 0.14f), "mat_fierro", false);
    }
}
