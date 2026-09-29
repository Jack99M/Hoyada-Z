using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Capitulo 2, zonas 1 a 3: callejon de la Feria 16 de Julio (turba y muro), pasarela peatonal
/// sobre la autopista y explanada de la estacion (caseta municipal, chofer y reja perimetral).
/// </summary>
public static partial class ConstructorCapitulo1
{
    private static Infectado[] turbaFeria;
    private static PuertaCap rejaPerimetral;

    // =====================================================================
    // Z1 — CALLEJON DE LA FERIA 16 DE JULIO
    // =====================================================================
    private static void C2_Feria(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z1_Feria").transform;
        r.SetParent(nivel, false);
        padre = r;

        Bloque("Feria_Piso", new Vector3(-8f, -0.2f, -26f), new Vector3(8f, 0f, 51.5f), "mat_asfalto");
        Edificio("Feria_Edif_O", new Vector3(-18f, 0f, -26f), new Vector3(-4f, 9f, 32.3f), "mat_ladrillo", false, false, true, false);
        Edificio("Feria_Edif_E", new Vector3(4f, 0f, -26f), new Vector3(18f, 11f, 32.3f), "mat_revoque_ocre", false, false, false, true);
        Edificio("Feria_Edif_S", new Vector3(-18f, 0f, -34f), new Vector3(18f, 10f, -26f), "mat_revoque_gris", true, false, false, false);
        Edificio("Calle_Edif_O", new Vector3(-18f, 0f, 32.3f), new Vector3(-4f, 8f, 51.5f), "mat_revoque_verde", false, false, true, false);
        Edificio("Calle_Edif_E", new Vector3(4f, 0f, 32.3f), new Vector3(18f, 7f, 51.5f), "mat_ladrillo", false, false, false, true);

        // Muro del fondo: demasiado alto para saltarlo sola
        Bloque("Feria_Muro", new Vector3(-4f, 0f, 31.7f), new Vector3(4f, 2.4f, 32.3f), "mat_ladrillo");
        Caja("Feria_Muro_Vidrios", new Vector3(0f, 2.46f, 32f), new Vector3(8f, 0.1f, 0.25f), "mat_vidrio", false);
        Graf("alto", new Vector3(0f, 1.3f, 31.69f), Vector3.back, 3.4f, r);
        Graf("de_pie", new Vector3(-3.99f, 4.2f, 22f), Vector3.right, 5f, r);
        Sonido("Ambiente_Ceja", new Vector3(0f, 2f, 0f), "amb_ceja", 0.3f, 500f, r, 0f);

        // Puestos de la feria a los dos lados
        string[] toldos = { "mat_toldo_rojo", "mat_plastico_azul", "mat_aguayo", "mat_toldo_rojo", "mat_plastico_azul" };
        for (int i = 0; i < 6; i++)
        {
            float z = -7f + i * 5f;
            Puesto("Feria_Puesto_O_" + i, new Vector3(-2.9f, 0f, z), 90f, toldos[i % toldos.Length], r);
            if (i != 3) { Puesto("Feria_Puesto_E_" + i, new Vector3(2.9f, 0f, z + 2.4f), -90f, toldos[(i + 2) % toldos.Length], r); }
        }
        for (int i = 0; i < 8; i++)
        {
            Caja("Feria_Caja_" + i, new Vector3(i % 2 == 0 ? -3.3f : 3.3f, 0.25f, -8f + i * 4.7f), new Vector3(0.6f, 0.5f, 0.5f), "mat_carton", true, i * 17f);
            Basura(new Vector3((float)(azar.NextDouble() * 4 - 2), 0f, -6f + i * 4.5f), r);
        }
        Cable("Feria_Cable_1", new Vector3(-4f, 4.2f, -2f), new Vector3(4f, 4.4f, 6f), 0.03f, r);
        Cable("Feria_Cable_2", new Vector3(-4f, 4.6f, 10f), new Vector3(4f, 4.1f, 17f), 0.03f, r);
        Cable("Feria_Cable_3", new Vector3(-4f, 4.3f, 22f), new Vector3(4f, 4.5f, 27f), 0.03f, r);
        Luz("Feria_Foco_1", new Vector3(-1.5f, 2.5f, 2f), new Color(1f, 0.75f, 0.45f), 0.6f, 6f, luces, LuzParpadeante.Modo.Falla);
        Luz("Feria_Foco_2", new Vector3(1.5f, 2.5f, 18f), new Color(1f, 0.75f, 0.45f), 0.6f, 6f, luces, LuzParpadeante.Modo.Falla);

        // Reja del sur: la turba de la feria empuja desde atras
        var reja = new GameObject("Feria_Reja_Sur");
        reja.transform.SetParent(r, false);
        for (int i = 0; i < 27; i++) { Caja("Barrote_" + i, new Vector3(-3.9f + i * 0.3f, 1.35f, -10f), new Vector3(0.05f, 2.7f, 0.05f), "mat_fierro", false, 0f, reja.transform); }
        Caja("Travesano_A", new Vector3(0f, 0.3f, -10f), new Vector3(8f, 0.08f, 0.06f), "mat_fierro", false, 0f, reja.transform);
        Caja("Travesano_B", new Vector3(0f, 2.6f, -10f), new Vector3(8f, 0.08f, 0.06f), "mat_fierro", false, 0f, reja.transform);
        var colReja = new GameObject("Reja_Colision");
        colReja.transform.SetParent(reja.transform, false);
        colReja.transform.position = new Vector3(0f, 1.4f, -10f);
        colReja.AddComponent<BoxCollider>().size = new Vector3(8f, 2.8f, 0.3f);
        var obs = colReja.AddComponent<NavMeshObstacle>();
        obs.shape = NavMeshObstacleShape.Box;
        obs.size = new Vector3(8f, 2.8f, 0.6f);
        obs.carving = true;
        CambiarCapa(colReja, CapaPuertas);
        var rejaCaida = new GameObject("Feria_Reja_Caida");
        rejaCaida.transform.SetParent(r, false);
        for (int i = 0; i < 9; i++) { Caja("Barrote_Caido_" + i, new Vector3(-3.4f + i * 0.85f, 0.05f, -8.6f + (i % 3) * 0.3f), new Vector3(0.06f, 0.06f, 2.6f), "mat_fierro", false, (i % 2 == 0 ? 12f : -9f), rejaCaida.transform); }
        rejaCaida.SetActive(false);

        var turba = new List<Infectado>();
        for (int i = 0; i < 7; i++)
        {
            Vector3 pos = new Vector3(-3f + (i % 3) * 3f + (float)(azar.NextDouble() - 0.5), 0f, -12f - (i / 3) * 2.6f - (float)azar.NextDouble());
            turba.Add(CrearInfectado("Feria_Turba_" + i, pos, (float)(azar.NextDouble() * 30 - 15), i == 4 ? Infectado.Tipo.Corredor : Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1.5f, i % 2 == 0 ? "mat_char_civil" : null));
        }
        turbaFeria = turba.ToArray();

        var zRejaCede = Zona("Z_Reja_Cede", new Vector3(0f, 1.5f, 9f), new Vector3(8f, 3f, 4f), ev, new Acciones
        {
            desactivar = new[] { reja }, activar = new[] { rejaCaida }, alertar = turbaFeria, sonido = "sfx_cadena",
            mensaje = "¡La reja cedió! Corre al muro del fondo",
            subtitulos = new[] { "Tito|¡Rompieron la reja! ¡CORRE, WARA!" }
        });
        zRejaCede.retraso = 2.5f;
        var zRejaTarde = Zona("Z_Reja_Cede_Tarde", new Vector3(0f, 1.5f, -3f), new Vector3(8f, 3f, 7f), ev, new Acciones
        {
            desactivar = new[] { reja }, activar = new[] { rejaCaida }, alertar = turbaFeria, sonido = "sfx_cadena"
        });
        zRejaTarde.retraso = 40f;

        // Empieza: Tito, la reja y lo que paso con Mateo (depende del Capitulo 1)
        var zq = Zona("Z_Inicio_Queda", new Vector3(0f, 1.5f, -4.5f), new Vector3(8f, 3f, 8f), ev, new Acciones
        {
            subtitulos = new[]
            {
                "Wara|Dejé a Mateo con fiebre donde doña Bety... Le prometí volver con remedios.",
                "Tito|Primero salgamos vivos de esta, cebrita.",
                "Tito|¡Esa reja no va a aguantar! ¡Al fondo del callejón hay un muro, vamos!"
            }
        });
        zq.requiereFlag = "wara_se_queda";
        var zf = Zona("Z_Inicio_Fue", new Vector3(0f, 1.5f, -4.5f), new Vector3(8f, 3f, 8f), ev, new Acciones
        {
            subtitulos = new[]
            {
                "Wara|Ese chico de Sopocachi, Mateo... Ojalá la doña de las gradas lo haya podido curar.",
                "Tito|Si llegó arriba, está mejor que nosotros.",
                "Tito|¡Esa reja no va a aguantar! ¡Al fondo del callejón hay un muro, vamos!"
            }
        });
        zf.requiereSinFlag = "wara_se_queda";

        // Primeros recursos
        Objeto("Fierro_Feria", new Vector3(-1.7f, 0f, 3.5f), Recogible.TipoObjeto.Fierro, 1, r);
        Zona("Z_Tutorial_Fierro", new Vector3(0f, 1.5f, 3f), new Vector3(8f, 3f, 3f), ev, new Acciones { mensaje = "Recoge el fierro con [E]  ·  [1] cuerpo a cuerpo  ·  [2] honda" });
        Suministro("Piedras_Feria", new Vector3(2f, 0f, 13.5f), Recogible.TipoObjeto.Piedras, 6, r);
        Objeto("Venda_Feria", new Vector3(-2.4f, 0.95f, 18.2f), Recogible.TipoObjeto.Venda, 1, r);
        Objeto("Botella_Feria", new Vector3(1.9f, 0f, 22.5f), Recogible.TipoObjeto.Botella, 1, r);
        Doc("Chat_Tecnicos_Feria", new Vector3(1.9f, 0.02f, 9.6f), "Grupo «Técnicos Feria 16»",
            "11:02  Brayan: El refresco de la casera de la esquina sabía raro, ¿no?\n" +
            "11:05  Rosmery: Yo me tomé dos jaja. Hace un calor...\n" +
            "13:15  Brayan: Tengo fiebre. Me voy a echar un rato atrás del puesto.\n" +
            "14:40  Don Tito: ¿Alguien sabe algo de Rosmery? No contesta.\n" +
            "15:12  Freddy T.: BRAYAN MORDIÓ A LA ROSMERY. NO ES JODA.\n" +
            "15:58  Don Tito: No vengan a la feria. Repito: NO VENGAN.", r, true);
        var comiendo = CrearInfectado("Feria_Comiendo", new Vector3(1.4f, 0f, 16f), 200f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 2f, "mat_char_civil");
        Cadaver("Feria_Cadaver", new Vector3(1.2f, 0f, 16.9f), 30f, "mat_char_civil", r);
        Cadaver("Feria_Cadaver_2", new Vector3(-2.1f, 0f, 27f), 120f, null, r);
        Sangre(new Vector3(-0.5f, 0.01f, 24f), 1.6f, r);

        // Trepar el muro con ayuda de Tito
        var trepar = Accion("Trepar_Muro", new Vector3(0f, 0f, 31.1f), 0f, "Trepar el muro con ayuda de Tito", r);
        trepar.requiereCompanero = true;
        trepar.mensajeSinCompanero = "Es muy alto. Sola no llego... ¿Dónde está Tito?";
        trepar.duracion = 1.4f;
        trepar.sonido = "sfx_esquiva";
        trepar.teletransportar = true;
        trepar.destino = new Vector3(0f, 0.05f, 33.8f);
        trepar.destinoRotY = 0f;
        trepar.destinoCompanero = new Vector3(1.3f, 0f, 33.4f);
        var desact = new List<GameObject>();
        foreach (Infectado t in turbaFeria) { desact.Add(t.gameObject); }
        desact.Add(comiendo.gameObject);
        desact.Add(zRejaCede.gameObject);
        desact.Add(zRejaTarde.gameObject);
        trepar.alUsar = new Acciones
        {
            subtitulos = new[] { "Tito|¡Pisa mi mano! ¡Arriba!", "Wara|¡Dame la mano, Tito! ...¡Ya!", "Tito|Uf... Del otro lado de la pasarela está la estación del teleférico." },
            desactivar = desact.ToArray(),
            objetivo = "Cruza la pasarela peatonal hacia la estación",
            usarMarcador = true, marcador = new Vector3(0f, 5.5f, 66f),
            puntoControl = true,
            progresoCielo = 0.12f,
            musica = "-"
        };
        Illa2(0, new Vector3(-3f, 0.95f, 12.4f), r, luces);
    }

    // =====================================================================
    // Z2 — PASARELA PEATONAL SOBRE LA AUTOPISTA
    // =====================================================================
    private static void C2_Pasarela(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z2_Pasarela").transform;
        r.SetParent(nivel, false);
        padre = r;

        Escalera("Pasarela_Rampa_Sur", new Vector3(0f, 0f, 38f), new Vector3(0f, 5.5f, 50f), 3.2f, true, r);
        Bloque("Pasarela_Tablero", new Vector3(-2f, 5.2f, 50f), new Vector3(2f, 5.5f, 82f), "mat_concreto");
        Baranda("Pasarela_Baranda_O", new Vector3(-2f, 5.5f, 50f), new Vector3(-2f, 5.5f, 82f), r);
        Baranda("Pasarela_Baranda_E", new Vector3(2f, 5.5f, 50f), new Vector3(2f, 5.5f, 82f), r);
        Invisible("Pasarela_O", new Vector3(-2.35f, 5.5f, 50f), new Vector3(-2.05f, 10f, 82f), r);
        Invisible("Pasarela_E", new Vector3(2.05f, 5.5f, 50f), new Vector3(2.35f, 10f, 82f), r);
        for (int i = 0; i <= 8; i++)
        {
            float z = 50f + i * 4f;
            Caja("Pasarela_Poste_O_" + i, new Vector3(-2.1f, 7f, z), new Vector3(0.12f, 3f, 0.12f), "mat_metal_pintado", false);
            Caja("Pasarela_Poste_E_" + i, new Vector3(2.1f, 7f, z), new Vector3(0.12f, 3f, 0.12f), "mat_metal_pintado", false);
        }
        Caja("Pasarela_Techo", new Vector3(0f, 8.55f, 66f), new Vector3(4.6f, 0.08f, 32.4f), "mat_calamina", false);
        Caja("Pasarela_Pilar_S", new Vector3(0f, -0.75f, 52.5f), new Vector3(1.2f, 12f, 1.2f), "mat_concreto", true);
        Caja("Pasarela_Pilar_N", new Vector3(0f, -0.75f, 79.5f), new Vector3(1.2f, 12f, 1.2f), "mat_concreto", true);
        Escalera("Pasarela_Rampa_Norte", new Vector3(0f, 0f, 94f), new Vector3(0f, 5.5f, 82f), 3.2f, true, r);

        // Autopista hundida: autos chocados, fuego e infectados (no se puede bajar)
        Bloque("Autopista_Piso", new Vector3(-60f, -7.2f, 51.5f), new Vector3(60f, -7f, 80.5f), "mat_asfalto");
        Bloque("Autopista_Muro_S", new Vector3(-60f, -7f, 51.5f), new Vector3(60f, 0f, 52f), "mat_concreto");
        Bloque("Autopista_Muro_N", new Vector3(-60f, -7f, 80f), new Vector3(60f, 0f, 80.5f), "mat_concreto");
        Bloque("Autopista_Fin_O", new Vector3(-61f, -7f, 51.5f), new Vector3(-60f, 8f, 80.5f), "mat_ladrillo");
        Bloque("Autopista_Fin_E", new Vector3(60f, -7f, 51.5f), new Vector3(61f, 8f, 80.5f), "mat_ladrillo");
        Bloque("Parapeto_Autopista_S", new Vector3(-4f, 0f, 51.5f), new Vector3(4f, 1.2f, 51.9f), "mat_concreto");
        Bloque("Autopista_Mediana", new Vector3(-60f, -7f, 65.7f), new Vector3(60f, -6.2f, 66.3f), "mat_barrera");
        Modelo("Props/prop_minibus.obj", new Vector3(-14f, -7f, 60f), 75f, "mat_prop_minibus_blanco", r);
        Modelo("Props/prop_auto_sedan.obj", new Vector3(-6f, -7f, 71f), 200f, "mat_prop_sedan", r);
        Modelo("Props/prop_minibus.obj", new Vector3(9f, -7f, 69f), 100f, "mat_prop_minibus_celeste", r);
        Modelo("Props/prop_auto_sedan.obj", new Vector3(22f, -7f, 58f), 30f, "mat_metal_pintado", r);
        Modelo("Props/prop_auto_sedan.obj", new Vector3(-28f, -7f, 74f), 250f, "mat_prop_sedan", r);
        Modelo("Props/prop_minibus.obj", new Vector3(35f, -7f, 62f), 10f, "mat_prop_minibus_blanco", r);
        Incendio(new Vector3(9f, -7f, 64.5f), 1.3f, r);
        Incendio(new Vector3(-22f, -7f, 68f), 1.6f, r);
        Incendio(new Vector3(34f, -7f, 59f), 1.1f, r);
        Luz("Autopista_Fuego_1", new Vector3(9f, -5.5f, 64.5f), new Color(1f, 0.55f, 0.2f), 2f, 12f, luces, LuzParpadeante.Modo.Fuego);
        Luz("Autopista_Fuego_2", new Vector3(-22f, -5.5f, 68f), new Color(1f, 0.55f, 0.2f), 2.2f, 12f, luces, LuzParpadeante.Modo.Fuego);
        for (int i = 0; i < 6; i++)
        {
            float x = (i % 2 == 0 ? -1f : 1f) * (16f + i * 5f);
            CrearInfectado("Autopista_Infectado_" + i, new Vector3(x, -7f, 56f + (i * 4.3f) % 20f), i * 60f, i == 3 ? Infectado.Tipo.Corredor : Infectado.Tipo.Comun, i % 3 == 0 ? Infectado.Estado.Comiendo : Infectado.Estado.Deambular, pers, 5f);
        }
        for (int i = 0; i < 5; i++) { Cadaver("Autopista_Cadaver_" + i, new Vector3(-30f + i * 13f, -7f, 55f + (i * 7f) % 22f), i * 70f, i % 2 == 0 ? "mat_char_civil" : null, r); }
        Sonido("Autopista_Sirena", new Vector3(-40f, 0f, 66f), "amb_sirena", 0.35f, 90f, r);

        // Dos comiendo en la pasarela: sigilo
        CrearInfectado("Pasarela_Comiendo_A", new Vector3(0.4f, 5.5f, 64.6f), 190f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 1f, "mat_char_civil");
        CrearInfectado("Pasarela_Comiendo_B", new Vector3(-0.6f, 5.5f, 67.4f), 20f, Infectado.Tipo.Comun, Infectado.Estado.Comiendo, pers, 1f);
        Cadaver("Pasarela_Cadaver", new Vector3(-0.2f, 5.52f, 66f), 90f, "mat_char_civil", r);
        Zona("Z_Pasarela_Sigilo", new Vector3(0f, 7f, 55f), new Vector3(4f, 3f, 3f), ev, new Acciones
        {
            subtitulos = new[] { "Tito|Shh... Dos comiendo. Despacito, Wara." },
            mensaje = "Agáchate con [C] y acércate por la espalda: [E] los elimina en silencio"
        });
        Suministro("Piedras_Pasarela", new Vector3(1.5f, 5.5f, 74f), Recogible.TipoObjeto.Piedras, 5, r);

        // Final de la pasarela: se ve la estacion y la 6 de Marzo
        var cine = new GameObject("Cine_Estacion");
        cine.transform.SetParent(ev, false);
        var cc = cine.AddComponent<CinematicaCamara>();
        cc.tomas = new[]
        {
            new CinematicaCamara.Toma { desde = new Vector3(0f, 8f, 80f), hasta = new Vector3(2f, 9f, 84f), mirarDesde = new Vector3(5f, 6f, 130f), mirarHasta = new Vector3(5f, 9f, 130f), duracion = 3.4f, fov = 52f, subtitulo = "Wara|La estación de la Línea Roja... está parada." },
            new CinematicaCamara.Toma { desde = new Vector3(-20f, 6f, 100f), hasta = new Vector3(-22f, 7f, 108f), mirarDesde = new Vector3(-44f, -5f, 130f), mirarHasta = new Vector3(-44f, 4f, 148f), duracion = 3.6f, fov = 55f, subtitulo = "Tito|Abajo, la 6 de Marzo es muerte segura. Hay que cruzar por arriba, por los cables." }
        };
        Zona("Z_Fin_Pasarela", new Vector3(0f, 6.5f, 79f), new Vector3(4f, 3f, 3f), ev, new Acciones
        {
            cinematica = cc,
            musica = "mus_ceja",
            objetivo = "Busca una entrada a la estación del teleférico",
            usarMarcador = true, marcador = new Vector3(5f, 0f, 121f),
            progresoCielo = 0.25f
        });
    }

    // =====================================================================
    // Z3 — EXPLANADA DE LA ESTACION
    // =====================================================================
    private static void C2_Explanada(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z3_Explanada").transform;
        r.SetParent(nivel, false);
        padre = r;

        Bloque("Explanada_Piso", new Vector3(-24f, -0.2f, 80.5f), new Vector3(36f, 0f, 124f), "mat_adoquin");
        Bloque("Parapeto_Autopista_N", new Vector3(-24f, 0f, 80.5f), new Vector3(36f, 1.2f, 80.9f), "mat_concreto");
        Bloque("Parapeto_Avenida_Explanada", new Vector3(-24.4f, 0f, 95f), new Vector3(-24f, 1.2f, 124f), "mat_concreto");
        Edificio("Explanada_Edif_O", new Vector3(-64f, 0f, 80.9f), new Vector3(-24f, 12f, 95f), "mat_revoque_verde", true, false, true, false);
        Bloque("Explanada_Edif_O_Base", new Vector3(-64f, -7f, 80.9f), new Vector3(-24f, 0f, 95f), "mat_concreto");

        // Reja perimetral del este (la del chofer) y calle de atras
        for (int i = 0; i < 18; i++)
        {
            float z = 81.5f + i * 2.4f;
            if (z > 103.6f && z < 108.4f) { continue; }
            Caja("Reja_Perimetral_Poste_" + i, new Vector3(36f, 1.3f, z), new Vector3(0.1f, 2.6f, 0.1f), "mat_fierro", false);
        }
        Caja("Reja_Perimetral_Riel_S", new Vector3(36f, 2.5f, 92.2f), new Vector3(0.06f, 0.06f, 23.4f), "mat_fierro", false);
        Caja("Reja_Perimetral_Riel_N", new Vector3(36f, 2.5f, 116.2f), new Vector3(0.06f, 0.06f, 15.6f), "mat_fierro", false);
        for (int i = 0; i < 90; i++)
        {
            float z = 81f + i * 0.47f;
            if (z > 103.8f && z < 108.2f) { continue; }
            if (z > 123.8f) { break; }
            Caja("Reja_Perimetral_Barrote_" + i, new Vector3(36f, 1.25f, z), new Vector3(0.03f, 2.5f, 0.03f), "mat_fierro", false);
        }
        var colS = new GameObject("Reja_Perimetral_Colision_S");
        colS.transform.SetParent(r, false);
        colS.transform.position = new Vector3(36f, 1.4f, 92.35f);
        colS.AddComponent<BoxCollider>().size = new Vector3(0.2f, 2.8f, 23.1f);
        var colN = new GameObject("Reja_Perimetral_Colision_N");
        colN.transform.SetParent(r, false);
        colN.transform.position = new Vector3(36f, 1.4f, 116f);
        colN.AddComponent<BoxCollider>().size = new Vector3(0.2f, 2.8f, 16f);
        rejaPerimetral = Puerta("Reja_Perimetral", new Vector3(36f, 0f, 106f), 90f, 4f, 2.6f, PuertaCap.Tipo.Reja, "mat_fierro", r);
        rejaPerimetral.abierta = true;
        rejaPerimetral.mensajeTrabada = "La trancamos con cadena. Ya no se abre.";
        Bloque("Calle_Chofer_Piso", new Vector3(36f, -0.2f, 80.5f), new Vector3(54f, 0f, 124f), "mat_asfalto");
        Bloque("Parapeto_Calle_Chofer", new Vector3(36f, 0f, 80.5f), new Vector3(54f, 1.2f, 80.9f), "mat_concreto");
        Edificio("Calle_Chofer_Edif", new Vector3(54f, 0f, 80.5f), new Vector3(66f, 10f, 136f), "mat_ladrillo", false, false, false, true);
        Edificio("Calle_Chofer_Edif_N", new Vector3(36.4f, 0f, 124f), new Vector3(54f, 9f, 136f), "mat_revoque_ocre", false, true, false, false);
        Bloque("Pasaje_Lateral_Piso", new Vector3(31f, -0.2f, 124f), new Vector3(36.4f, 0f, 136.4f), "mat_adoquin");
        Bloque("Pasaje_Lateral_Cierre", new Vector3(31f, 0f, 136f), new Vector3(36.4f, 5f, 136.4f), "mat_ladrillo");
        Bloque("Pasaje_Lateral_Muro", new Vector3(36f, 0f, 124f), new Vector3(36.4f, 5f, 136f), "mat_ladrillo");
        var corredores = new[]
        {
            CrearInfectado("Corredor_Reja_A", new Vector3(46f, 0f, 97f), 270f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false),
            CrearInfectado("Corredor_Reja_B", new Vector3(47f, 0f, 112f), 270f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false)
        };

        // Chofer mordido junto a su minibus: la decision del capitulo
        Modelo("Props/prop_minibus.obj", new Vector3(27f, 0f, 111f), 5f, "mat_prop_minibus_celeste", r);
        var chofer = CrearNPC("Rig_Infectado", "Chofer_Minibus", new Vector3(30.2f, 0f, 107f), 250f, "mat_char_chofer", PoseNPC.Pose.Sentado, true, pers, AnimProcedural.Estilo.Humano);
        var eCh = chofer.GetComponentInChildren<EsqueletoHumanoide>();
        Rostro(eCh, false);
        Sangre(new Vector3(30.4f, 0.01f, 107.2f), 1.4f, r);
        var hermano = CrearNPC("Rig_Infectado", "Wilmer_Hermano", new Vector3(31.2f, 0f, 108.3f), 230f, "mat_verde_arbol", PoseNPC.Pose.Sentado, true, pers, AnimProcedural.Estilo.Humano);
        var eHe = hermano.GetComponentInChildren<EsqueletoHumanoide>();
        Rostro(eHe, true);
        hermano.SetActive(false);
        var em = chofer.AddComponent<EleccionMoral>();
        em.nombreNPC = "Chofer";
        em.retrato = "chofer";
        em.radio = 2.5f;
        em.altura = 0.8f;
        em.texto = "Un chofer de minibús está sentado contra la llanta, con la mano apretada en el cuello. La sangre le corre entre los dedos.\n\n\"No... no tranquen la reja, por favor. Mi hermano Wilmer viene corriendo desde la Juan Pablo. Yo ya estoy mordido... pero él no. Déjenla abierta.\"\n\nTito se rasca la nuca: \"Si la dejamos abierta, entra cualquier cosa, Wara.\"";
        em.opciones = new[]
        {
            new EleccionMoral.OpcionMoral
            {
                texto = "\"Vamos a dejar la reja abierta. Que su hermano pueda entrar.\"", moral = 8,
                respuesta = "\"Dios le pague, hijita... Wilmer tiene chompa verde. Díganle que lo esperé.\"",
                acciones = new Acciones { flag = "reja_abierta", subtitulos = new[] { "Tito|Ta' bien... Pero si entra algo, entra por nosotros." } }
            },
            new EleccionMoral.OpcionMoral
            {
                texto = "\"Lo siento. Tenemos que trancar la reja.\"", moral = -6,
                respuesta = "\"No... por favor...\" El chofer cierra los ojos. Tito pasa la cadena por los barrotes sin mirarlo.",
                acciones = new Acciones { flag = "reja_cerrada", trabarPuertas = new[] { rejaPerimetral }, sonido = "sfx_cadena" }
            }
        };
        Zona("Z_Chofer_Llama", new Vector3(27f, 1.5f, 104f), new Vector3(12f, 3f, 12f), ev, new Acciones { subtitulos = new[] { "Chofer|¡Chicos! ¡Oigan, chicos, por favor!" } });
        var zr = Zona("Z_Reja_Abierta_Corredores", new Vector3(-12f, 1.5f, 99.5f), new Vector3(5f, 3f, 8f), ev, new Acciones
        {
            activar = new[] { hermano },
            alertar = corredores,
            subtitulos = new[] { "Wilmer|¡Hermano! ¡HERMANO! ¡Aquí estoy!", "Tito|¡Wara! ¡Entraron por la reja detrás de él! ¡Cuidado!" }
        });
        zr.requiereFlag = "reja_abierta";
        zr.requiereObjeto = "tarjeta_mantenimiento";

        // Caseta municipal: la tarjeta de mantenimiento y un guardia dormido
        Cuarto("Caseta_Municipal", -22f, -15f, 96f, 103f, 0f, 2.8f, "mat_revoque_verde", "mat_baldosa", "mat_calamina",
            SinHuecos, SinHuecos, new[] { Hueco.Puerta(99.5f, 1.2f, 2.1f), Hueco.Ventana(101.8f, 1.2f) }, new[] { Hueco.Ventana(99.5f, 1.4f) }, true, 0.2f);
        Puerta("Caseta_Municipal_Puerta", new Vector3(-15f, 0f, 99.5f), 90f, 1.2f, 2.1f, PuertaCap.Tipo.Bisagra, "mat_metal_oscuro", r).ruidoAlAbrir = 2.5f;
        Caja("Caseta_Mesa", new Vector3(-20.4f, 0.4f, 101.8f), new Vector3(1.4f, 0.8f, 0.7f), "mat_madera");
        Caja("Caseta_Archivador", new Vector3(-21.4f, 0.6f, 97.1f), new Vector3(0.6f, 1.2f, 0.9f), "mat_metal_pintado");
        Caja("Caseta_Silla", new Vector3(-19.6f, 0.25f, 101f), new Vector3(0.45f, 0.5f, 0.45f), "mat_madera", true, 20f);
        Luz("Caseta_Fluorescente", new Vector3(-18.5f, 2.6f, 99.5f), new Color(0.8f, 0.95f, 1f), 0.5f, 5f, luces, LuzParpadeante.Modo.Falla);
        CrearInfectado("Guardia_Municipal_Dormido", new Vector3(-19.2f, 0f, 97.8f), 60f, Infectado.Tipo.Comun, Infectado.Estado.Dormido, pers, 1f, "mat_char_policia");
        var tarjeta = Objeto("Tarjeta_Mantenimiento", new Vector3(-20.4f, 0.82f, 101.9f), Recogible.TipoObjeto.ObjetoClave, 1, r, "tarjeta_mantenimiento", "Tarjeta de mantenimiento");
        tarjeta.alRecoger = new Acciones
        {
            objetivo = "Abre la puerta lateral de la estación",
            usarMarcador = true, marcador = new Vector3(32f, 0f, 131f),
            puntoControl = true,
            subtitulos = new[] { "Wara|«Mi Teleférico — Mantenimiento». ¡La tengo!", "Tito|La puerta lateral está al costado este de la estación. ¡Vamos!" }
        };
        Doc("Bitacora_Guardia", new Vector3(-21.4f, 1.22f, 97.1f), "Bitácora del guardia municipal",
            "06:05  Corte de luz en toda la Ceja. El teleférico se para con gente adentro.\n" +
            "06:40  Los técnicos bajan a la gente de las cabinas con cuerdas. Dos cabinas quedan colgadas junto al andén.\n" +
            "09:30  Llegan los de la UTOP. El subteniente Mamani cierra el patio de atrás de la estación.\n" +
            "12:10  Mamani tiene fiebre, dice que no es nada. Grita solo.\n" +
            "14:55  Se escuchan tiros en el patio. Cierro la caseta.\n" +
            "15:30  Me mordieron en la mano. Dejo la tarjeta de mantenimiento en la mesa. Si alguien lee esto: la puerta del costado este.", r);

        // Entrada principal bloqueada
        Zona("Z_Entrada_Bloqueada", new Vector3(5f, 1.5f, 120.5f), new Vector3(18f, 3f, 5f), ev, new Acciones
        {
            objetivo = "Encuentra la tarjeta de mantenimiento en la caseta",
            usarMarcador = true, marcador = new Vector3(-15f, 0f, 99.5f),
            subtitulos = new[] { "Wara|La entrada está trancada con muebles y alambre de púas.", "Tito|Hay una puerta de mantenimiento al costado, con lector de tarjeta. Los del municipio tenían una en su caseta." }
        });
        var zm = Zona("Z_Puerta_Sin_Tarjeta", new Vector3(33.5f, 1.5f, 131f), new Vector3(4f, 3f, 6f), ev, new Acciones
        {
            objetivo = "Encuentra la tarjeta de mantenimiento en la caseta",
            usarMarcador = true, marcador = new Vector3(-15f, 0f, 99.5f),
            subtitulos = new[] { "Wara|Lector de tarjeta... Sin tarjeta no abre.", "Tito|La caseta municipal, al otro lado de la explanada. Ahí guardaban la tarjeta." }
        });
        zm.requiereSinObjeto = "tarjeta_mantenimiento";

        // Mobiliario de la explanada
        Modelo("Props/prop_auto_sedan.obj", new Vector3(12f, 0f, 96f), 70f, "mat_prop_sedan", r);
        Modelo("Props/prop_auto_sedan.obj", new Vector3(-6f, 0f, 90f), 160f, "mat_metal_pintado", r);
        Modelo("Props/prop_minibus.obj", new Vector3(18f, 0f, 88f), 95f, "mat_prop_minibus_blanco", r);
        for (int i = 0; i < 5; i++)
        {
            Poste(new Vector3(-16f + i * 10f, 0f, 116f), 0f, r, luces, false);
            Banca(new Vector3(-14f + i * 10f, 0f, 112.5f), 180f, r);
        }
        Caja("Kiosco_Explanada", new Vector3(-4f, 1.2f, 104f), new Vector3(2.4f, 2.4f, 2f), "mat_metal_pintado");
        Caja("Kiosco_Explanada_Toldo", new Vector3(-4f, 2.5f, 103.2f), new Vector3(2.8f, 0.05f, 1.2f), "mat_toldo_rojo", false);
        Barricada("Barrera_Policial_Explanada", new Vector3(8f, 0f, 114f), 5f, 10f, false, r);
        for (int i = 0; i < 6; i++) { Basura(new Vector3(-18f + i * 8f, 0f, 92f + (i * 5f) % 20f), r); }
        Cadaver("Explanada_Cadaver_1", new Vector3(2f, 0f, 108f), 40f, "mat_char_civil", r);
        Cadaver("Explanada_Cadaver_2", new Vector3(20f, 0f, 118f), 150f, null, r);
        Cadaver("Explanada_Cadaver_3", new Vector3(-10f, 0f, 118f), 280f, "mat_char_policia", r);
        Incendio(new Vector3(18f, 0f, 90f), 0.9f, r);
        Luz("Explanada_Fuego", new Vector3(18f, 1.5f, 90f), new Color(1f, 0.55f, 0.2f), 1.6f, 10f, luces, LuzParpadeante.Modo.Fuego);
        CrearInfectado("Explanada_Infectado_1", new Vector3(8f, 0f, 106f), 30f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 5f);
        CrearInfectado("Explanada_Infectado_2", new Vector3(-8f, 0f, 112f), 200f, Infectado.Tipo.Comun, Infectado.Estado.Deambular, pers, 4f, "mat_char_civil");
        CrearInfectado("Explanada_Corredor_Dormido", new Vector3(15f, 0f, 119f), 90f, Infectado.Tipo.Corredor, Infectado.Estado.Dormido, pers, 2f);
        Objeto("Botella_Explanada", new Vector3(-3f, 0f, 102.4f), Recogible.TipoObjeto.Botella, 1, r);
        Suministro("Trapo_Explanada", new Vector3(-5.9f, 0f, 103.3f), Recogible.TipoObjeto.Trapo, 1, r);
        Suministro("Alcohol_Caseta", new Vector3(-20.95f, 0.82f, 101.65f), Recogible.TipoObjeto.Alcohol, 1, r);
        Objeto("Venda_Explanada", new Vector3(24f, 0f, 110.5f), Recogible.TipoObjeto.Venda, 1, r);
        Altar("Altar_Explanada", new Vector3(-12f, 0f, 107f), 180f, "Explanada de la estación", r, luces);
        Illa2(1, new Vector3(-15.9f, 0f, 102.3f), r, luces);
    }
}
