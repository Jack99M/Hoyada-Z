using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Capitulo 2, zonas 6 y 7: patio de la UTOP (jefe El Paco Antidisturbios) y el porton del
/// cerrojo oxidado hacia Villa Dolores (asedio de 45 s y final con el toque de queda).
/// </summary>
public static partial class ConstructorCapitulo1
{
    // =====================================================================
    // Z6 — PATIO DE LA UTOP: EL PACO ANTIDISTURBIOS
    // =====================================================================
    private static void C2_Patio(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z6_Patio_UTOP").transform;
        r.SetParent(nivel, false);
        padre = r;

        Bloque("Patio_Piso", new Vector3(-112f, -0.2f, 158f), new Vector3(-64f, 0f, 204.4f), "mat_asfalto");
        Bloque("Patio_Muro_S_O", new Vector3(-112f, 0f, 157.6f), new Vector3(-108f, 4f, 158f), "mat_ladrillo");
        Bloque("Patio_Muro_S_E", new Vector3(-104f, 0f, 157.6f), new Vector3(-82f, 4f, 158f), "mat_ladrillo");
        Bloque("Patio_Muro_O_S", new Vector3(-112.4f, 0f, 157.6f), new Vector3(-112f, 4f, 172f), "mat_ladrillo");
        Bloque("Patio_Muro_O_N", new Vector3(-112.4f, 0f, 176f), new Vector3(-112f, 4f, 204.4f), "mat_ladrillo");
        Bloque("Patio_Muro_N_O", new Vector3(-112.4f, 0f, 204f), new Vector3(-90f, 4f, 204.4f), "mat_ladrillo");
        Bloque("Patio_Muro_N_E", new Vector3(-84f, 0f, 204f), new Vector3(-63.5f, 4f, 204.4f), "mat_ladrillo");
        Bloque("Patio_Alambre_Muro", new Vector3(-112.4f, 4f, 204.1f), new Vector3(-63.5f, 4.1f, 204.3f), "mat_cable", false);
        Graf("cuarentena", new Vector3(-100f, 2f, 203.95f), Vector3.back, 4f, r);
        Graf("toque", new Vector3(-74f, 2.2f, 203.95f), Vector3.back, 4.5f, r);

        // Callejones por donde llegan los refuerzos
        Bloque("Callejon_O_Piso", new Vector3(-122f, -0.2f, 166f), new Vector3(-112f, 0f, 182f), "mat_asfalto");
        Bloque("Callejon_O_Muro_S", new Vector3(-122.4f, 0f, 165.6f), new Vector3(-112.4f, 4f, 166f), "mat_ladrillo");
        Bloque("Callejon_O_Muro_N", new Vector3(-122.4f, 0f, 182f), new Vector3(-112.4f, 4f, 182.4f), "mat_ladrillo");
        Bloque("Callejon_O_Muro_O", new Vector3(-122.8f, 0f, 165.6f), new Vector3(-122.4f, 4f, 182.4f), "mat_ladrillo");
        Bloque("Callejon_S_Piso", new Vector3(-110f, -0.2f, 146f), new Vector3(-102f, 0f, 158f), "mat_asfalto");
        Bloque("Callejon_S_Muro_O", new Vector3(-110.4f, 0f, 145.6f), new Vector3(-110f, 4f, 157.6f), "mat_ladrillo");
        Bloque("Callejon_S_Muro_E", new Vector3(-102f, 0f, 145.6f), new Vector3(-101.6f, 4f, 157.6f), "mat_ladrillo");
        Bloque("Callejon_S_Muro_S", new Vector3(-110.4f, 0f, 145.6f), new Vector3(-101.6f, 4f, 146f), "mat_ladrillo");
        Edificio("Patio_Edif_O", new Vector3(-150f, 0f, 150f), new Vector3(-122.8f, 9f, 215f), "mat_adobe", false, false, true, false);
        Edificio("Patio_Edif_S", new Vector3(-101.6f, 0f, 128f), new Vector3(-82f, 9f, 157.6f), "mat_revoque_gris", true, false, false, false);
        Edificio("Patio_Edif_SO", new Vector3(-150f, 0f, 120f), new Vector3(-110.4f, 8f, 150f), "mat_ladrillo", true, false, true, false);

        // Coberturas del lado de Wara
        Barricada("Cobertura_Tito", new Vector3(-71f, 0f, 175.8f), 6f, 0f, false, r);
        Barricada("Cobertura_1", new Vector3(-84f, 0f, 178f), 5f, 0f, true, r);
        Barricada("Cobertura_2", new Vector3(-98f, 0f, 180f), 5f, 10f, false, r);
        Barricada("Cobertura_3", new Vector3(-79f, 0f, 186.5f), 4f, -8f, true, r);
        Barricada("Cobertura_4", new Vector3(-94f, 0f, 188f), 4.5f, 5f, true, r);
        Barricada("Cobertura_5", new Vector3(-102f, 0f, 183.5f), 4f, 90f, false, r);
        Barricada("Cobertura_6", new Vector3(-67.5f, 0f, 188f), 4f, 90f, true, r);
        // Trinchera del Paco
        Barricada("Paco_Sacos_Centro", new Vector3(-88f, 0f, 195.3f), 6f, 0f, true, r);
        Barricada("Paco_Chapa_Oeste", new Vector3(-103f, 0f, 195.3f), 5f, 0f, false, r);
        Barricada("Paco_Sacos_Este", new Vector3(-73f, 0f, 195.3f), 5f, 0f, true, r);
        Patrullera("Patrullera_UTOP", new Vector3(-88f, 0f, 200.6f), 90f, r, luces);
        Caja("Patio_Tanque_Agua", new Vector3(-68f, 1f, 201f), new Vector3(2f, 2f, 2f), "mat_plastico_azul");
        for (int i = 0; i < 4; i++) { Caja("Patio_Llanta_" + i, new Vector3(-108f + i * 0.9f, 0.3f, 166f), new Vector3(0.8f, 0.6f, 0.8f), "mat_negro", true, i * 20f); }
        Cadaver("Patio_Cadaver_1", new Vector3(-80f, 0f, 181f), 20f, "mat_char_civil", r);
        Cadaver("Patio_Cadaver_2", new Vector3(-92f, 0f, 184f), 200f, "mat_char_policia", r);
        Cadaver("Patio_Cadaver_3", new Vector3(-99f, 0f, 173f), 120f, null, r);
        Luz("Patio_Reflector", new Vector3(-88f, 6f, 203f), new Color(1f, 0.9f, 0.75f), 1.6f, 22f, luces, LuzParpadeante.Modo.Falla);
        Poste(new Vector3(-75f, 0f, 166f), 90f, r, luces, false);
        Poste(new Vector3(-106f, 0f, 199f), 90f, r, luces, false);

        // Caseta de la UTOP
        Cuarto("Caseta_UTOP", -111f, -106f, 186f, 193f, 0f, 2.6f, "mat_revoque_gris", "mat_concreto", "mat_calamina",
            SinHuecos, SinHuecos, new[] { Hueco.Puerta(189.5f, 1.1f, 2.1f) }, SinHuecos, true, 0.2f);
        Caja("UTOP_Escritorio", new Vector3(-109.8f, 0.4f, 191.6f), new Vector3(1.2f, 0.8f, 0.7f), "mat_madera");
        Caja("UTOP_Radio", new Vector3(-109.9f, 0.9f, 191.7f), new Vector3(0.4f, 0.2f, 0.3f), "mat_negro", false);
        Letrero("utop", new Vector3(-105.85f, 2.3f, 189.5f), Vector3.right, 2.2f, r);
        Doc("Radio_UTOP", new Vector3(-109.5f, 0.82f, 191.4f), "Radio de la UTOP (transcripción)",
            "CENTRAL: Unidades de la Ceja, reporte.\n" +
            "MAMANI: Patio de la estación asegurado. Tengo fiebre, mi capitán, pero estoy firme.\n" +
            "CENTRAL: Nadie cruza hacia Villa Dolores. Candado maestro en el portón.\n" +
            "MAMANI: Los civiles no entienden, mi capitán. Los veo raros. Todos se ven raros.\n" +
            "CENTRAL: A partir de las 18:00 está autorizado el uso de fuerza letal. Repito: fuerza letal.\n" +
            "MAMANI: ...Me arde la cabeza. Voy a quedarme en el patio. Nadie pasa. Nadie.", r);
        Objeto("Venda_UTOP", new Vector3(-108.2f, 0f, 187f), Recogible.TipoObjeto.Venda, 1, r);
        Suministro("Molotov_UTOP", new Vector3(-110.3f, 0f, 187.4f), Recogible.TipoObjeto.Molotov, 1, r);
        Suministro("Piedras_Patio", new Vector3(-83f, 0f, 176.5f), Recogible.TipoObjeto.Piedras, 6, r);
        Objeto("Botella_Patio", new Vector3(-96f, 0f, 178.6f), Recogible.TipoObjeto.Botella, 2, r);

        // El Paco
        var paco = CrearInfectado("El_Paco", new Vector3(-88f, 0f, 197f), 180f, Infectado.Tipo.Paco, Infectado.Estado.Quieto, pers, 1f, "mat_verde_policia");
        var e = paco.GetComponentInChildren<EsqueletoHumanoide>();
        if (e != null)
        {
            Accesorio(e, EsqueletoHumanoide.Head, PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.28f, 0.32f), "mat_negro");
            Accesorio(e, EsqueletoHumanoide.Head, PrimitiveType.Cube, new Vector3(0f, 0.08f, 0.15f), new Vector3(0.24f, 0.13f, 0.03f), "mat_vidrio");
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0.02f), new Vector3(0.46f, 0.5f, 0.34f), "mat_chaleco");
            Accesorio(e, EsqueletoHumanoide.Chest, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.19f), new Vector3(0.3f, 0.1f, 0.02f), "mat_baldosa");
            if (e.puntoArma != null)
            {
                var escopeta = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(escopeta.GetComponent<Collider>());
                escopeta.name = "Escopeta_Paco";
                escopeta.transform.SetParent(e.puntoArma, false);
                escopeta.transform.localPosition = new Vector3(0f, 0f, 0.3f);
                escopeta.transform.localScale = new Vector3(0.05f, 0.07f, 0.8f);
                escopeta.GetComponent<Renderer>().sharedMaterial = Mat("mat_negro");
            }
        }
        paco.nombreJefe = "EL PACO ANTIDISTURBIOS";
        paco.subtituloJefe = "Subteniente Mamani, UTOP";
        paco.coberturas = new[] { new Vector3(-88f, 0f, 197f), new Vector3(-103f, 0f, 197f), new Vector3(-73f, 0f, 197f) };
        paco.materialLaser = Mat("mat_laser_rojo");
        paco.refuerzos = new[]
        {
            CrearInfectado("Refuerzo_UTOP_1", new Vector3(-109f, 0f, 188.5f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_policia", false),
            CrearInfectado("Refuerzo_Callejon_O_1", new Vector3(-118f, 0f, 172f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, null, false),
            CrearInfectado("Refuerzo_Callejon_S_1", new Vector3(-106f, 0f, 150f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false),
            CrearInfectado("Refuerzo_UTOP_2", new Vector3(-109.5f, 0f, 190.5f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_policia", false),
            CrearInfectado("Refuerzo_Callejon_O_2", new Vector3(-119f, 0f, 177f), 90f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, "mat_char_civil", false),
            CrearInfectado("Refuerzo_Callejon_S_2", new Vector3(-105f, 0f, 152f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_civil", false)
        };
        paco.alFase2 = new Acciones { subtitulos = new[] { "Tito|¡Está llamando a los otros! ¡Cuidado, Wara!" } };

        var botin = new GameObject("Botin_Paco");
        botin.transform.SetParent(r, false);
        botin.transform.position = paco.transform.position;
        var llave = Objeto("Llave_Maestra", botin.transform.position, Recogible.TipoObjeto.ObjetoClave, 1, botin.transform, "llave_maestra", "Llave del candado maestro");
        llave.alRecoger = new Acciones
        {
            objetivo = "Abre el portón que da hacia Villa Dolores",
            usarMarcador = true, marcador = new Vector3(-87f, 0f, 203f),
            puntoControl = true,
            subtitulos = new[] { "Wara|La llave del candado maestro...", "Tito|El portón, al fondo del patio. Yo me encargo del cerrojo." }
        };
        var revolver = Suministro("Revolver_Paco", botin.transform.position + new Vector3(0.7f, 0f, 0.4f), Recogible.TipoObjeto.Revolver, 6, botin.transform, "Revólver del Paco");
        revolver.alRecoger = new Acciones { mensaje = "Revólver: [3]. Pega fuerte, pero se escucha en toda la Ceja" };
        botin.SetActive(false);
        paco.soltarAlMorir = botin;
        paco.alMorir = new Acciones
        {
            objetivo = "Recoge la llave maestra del patio",
            usarMarcador = true, marcador = new Vector3(-88f, 0f, 196f),
            companera = 4,
            puntoControl = true,
            progresoCielo = 0.85f,
            subtitulos = new[] { "Wara|...Era policía. Seguro tenía familia.", "Tito|Ya no era él, Wara. Agarra la llave, rápido. Ya se viene la noche." }
        };

        var cine = new GameObject("Cine_Paco");
        cine.transform.SetParent(ev, false);
        var cc = cine.AddComponent<CinematicaCamara>();
        cc.tomas = new[]
        {
            new CinematicaCamara.Toma { desde = new Vector3(-72f, 3.2f, 168f), hasta = new Vector3(-74f, 3.4f, 171f), mirarDesde = new Vector3(-88f, 1.4f, 199f), mirarHasta = new Vector3(-88f, 1.6f, 197f), duracion = 2.6f, fov = 48f, subtitulo = "Tito|¡Es un paco! ¡Y tiene escopeta!", sonido = "sfx_recarga" },
            new CinematicaCamara.Toma { desde = new Vector3(-86f, 1.8f, 191f), hasta = new Vector3(-87f, 1.7f, 192.5f), mirarDesde = new Vector3(-88f, 1.7f, 197f), mirarHasta = new Vector3(-88f, 1.9f, 197f), duracion = 2.4f, fov = 40f, subtitulo = "Paco|¡ALTO AHÍ! ¡Toque de queda! ¡Al suelo o disparo!" }
        };
        Zona("Z_Paco_Inicio", new Vector3(-70f, 1.5f, 172.5f), new Vector3(6f, 3f, 5f), ev, new Acciones
        {
            puntoControl = true,
            companera = 3,
            cinematica = cc,
            iniciarJefe = paco,
            objetivo = "¡Cúbrete! Esquiva los disparos del policía",
            mensaje = "Agáchate [C] detrás de las barricadas: si te ve de pie, dispara  ·  [Alt] esquivar",
            progresoCielo = 0.75f
        });
        Zona("Z_Paco_Flanquear", new Vector3(-88f, 1.5f, 181f), new Vector3(48f, 3f, 40f), ev, new Acciones
        {
            objetivo = "Neutraliza al policía atrincherado",
            subtitulos = new[] { "Tito|¡Wara! ¡Yo le hago bulla desde aquí! ¡Tú rodéalo y dale por la espalda!" }
        }).retraso = 12f;

        // Z7: porton del candado maestro y el cerrojo oxidado
        var porton = Puerta("Porton_Villa_Dolores", new Vector3(-87f, 0f, 204.2f), 0f, 6f, 3.2f, PuertaCap.Tipo.Reja, "mat_fierro", r);
        porton.trabada = true;
        porton.radio = 0.01f;
        porton.mensajeTrabada = "Candado maestro y un cerrojo oxidado.";
        Caja("Porton_Candado", new Vector3(-87f, 1.2f, 204f), new Vector3(0.25f, 0.3f, 0.15f), "mat_metal_oscuro", false);
        var olas = new List<Infectado[]>
        {
            new[] { CrearInfectado("Porton_O_1", new Vector3(-117f, 0f, 170f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Porton_O_2", new Vector3(-119f, 0f, 174f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_civil", false) },
            new[] { CrearInfectado("Porton_S_1", new Vector3(-107f, 0f, 149f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Porton_S_2", new Vector3(-104f, 0f, 148f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false) },
            new[] { CrearInfectado("Porton_O_3", new Vector3(-120f, 0f, 178f), 90f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Porton_O_4", new Vector3(-116f, 0f, 179f), 90f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_policia", false),
                    CrearInfectado("Porton_S_3", new Vector3(-106f, 0f, 147f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, "mat_char_civil", false) },
            new[] { CrearInfectado("Porton_O_5", new Vector3(-118f, 0f, 168f), 90f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Porton_S_4", new Vector3(-103f, 0f, 150f), 0f, Infectado.Tipo.Corredor, Infectado.Estado.Quieto, pers, 1f, null, false),
                    CrearInfectado("Porton_S_5", new Vector3(-108f, 0f, 151f), 0f, Infectado.Tipo.Comun, Infectado.Estado.Quieto, pers, 1f, null, false) }
        };
        var asedio = Asedio("Asedio_Porton", new Vector3(-87f, 0f, 200f), 45f, ev);
        asedio.oleadas = new[]
        {
            new EventoAsedio.Oleada { tiempo = 3f, infectados = olas[0], aviso = "Tito|¡Por el callejón del oeste!" },
            new EventoAsedio.Oleada { tiempo = 13f, infectados = olas[1], aviso = "Tito|¡Por atrás, por la reja del sur!" },
            new EventoAsedio.Oleada { tiempo = 24f, infectados = olas[2], aviso = "Tito|¡Son muchos! ¡Aguanta, Wara!" },
            new EventoAsedio.Oleada { tiempo = 34f, infectados = olas[3], aviso = "Tito|¡Ya casi cede! ¡Un poquito más!" }
        };
        asedio.alTerminar = new Acciones
        {
            destrabarPuertas = new[] { porton },
            abrirPuerta = porton,
            companera = 4,
            objetivo = "¡Crucen el portón hacia Villa Dolores!",
            usarMarcador = true, marcador = new Vector3(-87f, 0f, 212f),
            subtitulos = new[] { "Tito|¡CEDIÓ! ¡Vamos, vamos, vamos!" },
            puntoControl = true,
            progresoCielo = 1f
        };
        var candado = Accion("Candado_Porton", new Vector3(-87f, 0f, 203.1f), 0f, "Abrir el candado (Tito fuerza el cerrojo)", r);
        candado.requiereObjeto = "llave_maestra";
        candado.consumirObjeto = true;
        candado.mensajeSinObjeto = "Candado maestro. La llave la debe tener ese policía...";
        candado.requiereCompanero = true;
        candado.mensajeSinCompanero = "El cerrojo está oxidado. Sola no puedo; necesito a Tito.";
        candado.duracion = 1.2f;
        candado.sonido = "sfx_cadena";
        candado.companeroTrabaja = true;
        candado.puntoTrabajo = new Vector3(-85.4f, 0f, 203.2f);
        candado.alUsar = new Acciones
        {
            iniciarAsedio = asedio,
            objetivo = "¡Resiste hasta que el cerrojo ceda!",
            subtitulos = new[] { "Tito|¡El candado sí, pero el cerrojo está oxidado! ¡Dame tiempo!", "Tito|¡Cúbreme la espalda, Wara!" },
            mensaje = "Protege a Tito mientras fuerza el cerrojo",
            puntoControl = true
        };
    }

    private static void Patrullera(string n, Vector3 pos, float rotY, Transform p, Transform luces)
    {
        var raiz = new GameObject(n).transform;
        raiz.SetParent(p, false);
        raiz.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        Caja(n + "_Caja", raiz.TransformPoint(-0.8f, 0.95f, 0f), new Vector3(2.2f, 1.1f, 3.2f), "mat_verde_policia", true, rotY, raiz);
        Caja(n + "_Carroceria", raiz.TransformPoint(0f, 0.7f, 0f), new Vector3(2.1f, 0.8f, 5.2f), "mat_verde_policia", true, rotY, raiz);
        Caja(n + "_Cabina", raiz.TransformPoint(0f, 1.5f, 1.2f), new Vector3(2f, 0.8f, 1.8f), "mat_verde_policia", true, rotY, raiz);
        Caja(n + "_Parabrisas", raiz.TransformPoint(0f, 1.55f, 2.12f), new Vector3(1.8f, 0.6f, 0.05f), "mat_vidrio", false, rotY, raiz);
        Caja(n + "_Franja", raiz.TransformPoint(0f, 0.85f, 0f), new Vector3(2.12f, 0.12f, 5.22f), "mat_baldosa", false, rotY, raiz);
        Caja(n + "_Sirena_R", raiz.TransformPoint(-0.4f, 2f, 1.2f), new Vector3(0.6f, 0.15f, 0.3f), "mat_toldo_rojo", false, rotY, raiz);
        Caja(n + "_Sirena_A", raiz.TransformPoint(0.4f, 2f, 1.2f), new Vector3(0.6f, 0.15f, 0.3f), "mat_neon_cian", false, rotY, raiz);
        foreach (float z in new[] { -1.7f, 1.7f })
        {
            foreach (float x in new[] { -1.05f, 1.05f })
            {
                var rueda = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rueda.name = n + "_Rueda";
                Object.DestroyImmediate(rueda.GetComponent<Collider>());
                rueda.transform.SetParent(raiz, false);
                rueda.transform.localPosition = new Vector3(x, 0.38f, z);
                rueda.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                rueda.transform.localScale = new Vector3(0.75f, 0.12f, 0.75f);
                rueda.GetComponent<Renderer>().sharedMaterial = Mat("mat_negro");
            }
        }
        Luz(n + "_Baliza", raiz.TransformPoint(0f, 2.3f, 1.2f), new Color(0.3f, 0.5f, 1f), 1.2f, 8f, luces, LuzParpadeante.Modo.Falla);
    }

    // =====================================================================
    // Z7 — VILLA DOLORES: FINAL DEL CAPITULO
    // =====================================================================
    private static void C2_VillaDolores(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z7_Villa_Dolores").transform;
        r.SetParent(nivel, false);
        padre = r;

        Bloque("Villa_Piso", new Vector3(-100f, -0.2f, 204.4f), new Vector3(-74f, 0f, 236f), "mat_pista");
        Edificio("Villa_Casas_O", new Vector3(-112f, 0f, 204.4f), new Vector3(-100f, 7f, 236f), "mat_adobe", false, false, true, false);
        Edificio("Villa_Casas_E", new Vector3(-74f, 0f, 204.4f), new Vector3(-62f, 6f, 236f), "mat_ladrillo", false, false, false, true);
        Edificio("Villa_Casas_N", new Vector3(-100f, 0f, 236f), new Vector3(-74f, 8f, 246f), "mat_revoque_ocre", false, true, false, false);
        Caja("Casa_Rosario_Porton", new Vector3(-87f, 1.3f, 235.9f), new Vector3(2.6f, 2.6f, 0.15f), "mat_puerta_azul", false);
        Caja("Casa_Rosario_Porton_Marco", new Vector3(-87f, 2.7f, 235.85f), new Vector3(3f, 0.25f, 0.2f), "mat_madera", false);
        Letrero("villa_dolores", new Vector3(-99.8f, 3f, 214f), Vector3.right, 3f, r);
        Bandera("mat_wiphala", new Vector3(-80f, 5.5f, 230f), Vector3.left, 1.6f, 1.1f, r);
        for (int i = 0; i < 3; i++) { Poste(new Vector3(-98.5f, 0f, 210f + i * 9f), 90f, r, luces, false); }
        Basura(new Vector3(-80f, 0f, 216f), r);
        Basura(new Vector3(-95f, 0f, 226f), r);

        // Helicoptero con reflector sobre la Ceja
        var heli = new GameObject("Helicoptero_Militar").transform;
        heli.SetParent(r, false);
        heli.position = new Vector3(-60f, 42f, 250f);
        Caja("Helicoptero_Cuerpo", heli.position, new Vector3(2.4f, 2f, 7f), "mat_verde_policia", false, 25f, heli);
        Caja("Helicoptero_Cola", heli.position + new Vector3(-1.5f, 0.4f, -4.5f), new Vector3(0.5f, 0.6f, 4f), "mat_verde_policia", false, 25f, heli);
        Caja("Helicoptero_Rotor", heli.position + Vector3.up * 1.3f, new Vector3(11f, 0.06f, 0.4f), "mat_negro", false, 70f, heli);
        var pivote = new GameObject("Reflector_Pivote").transform;
        pivote.SetParent(heli, false);
        pivote.position = heli.position + Vector3.down * 1.2f;
        var bal = pivote.gameObject.AddComponent<Balanceo>();
        bal.eje = Vector3.up;
        bal.amplitud = 35f;
        bal.velocidad = 0.5f;
        Sonido("Helicoptero_Rotor_Sonido", heli.position, "amb_helicoptero", 1f, 140f, heli);
        var reflector = Foco("Helicoptero_Reflector", pivote.position, new Vector3(-85f, 0f, 222f), new Color(0.9f, 0.95f, 1f), 30f, 70f, 16f, pivote, null, false);
        reflector.transform.SetParent(pivote, true);

        Zona("Z_Final_Altavoz", new Vector3(-87f, 1.5f, 211.5f), new Vector3(14f, 3f, 4f), ev, new Acciones
        {
            sonido = "sfx_megafono",
            musica = "-",
            subtitulos = new[]
            {
                "Wara|Esa es la casa de mi mamá... la del portón azul. Está todo apagado.",
                "Altavoz|Atención ciudadanos de El Alto: a partir de las 18:00 rige toque de queda absoluto y ley marcial.",
                "Altavoz|Todo civil en vía pública será neutralizado sin previo aviso.",
                "Tito|...Wara. Tenemos quince minutos."
            }
        });
        Zona("Z_Final", new Vector3(-87f, 1.5f, 211.5f), new Vector3(14f, 3f, 4f), ev, new Acciones { iniciarFinal = true }).retraso = 16f;
    }

    // =====================================================================
    // EXTRAS: illas del capitulo 2 y Tito
    // =====================================================================
    private static readonly string[,] IllasCap2 =
    {
        { "Minibús de la Ceja", "Un minibús de yeso con el letrero «CEJA – 16 DE JULIO – RÍO SECO». Alguien lo compró para tener su propia línea algún día. En la base: «Ruta 9, Wilmer y Néstor»." },
        { "Título profesional", "Un diploma en miniatura: «Licenciada en Enfermería». Las cebras que estudian de noche compran estos en Alasitas. Tiene el nombre borrado por la lluvia." },
        { "Chuspa de coca", "Una chuspa tejida del tamaño de un dedo, con tres hojitas de coca adentro. Para que nunca falte el akulliku en el trabajo." }
    };

    private static void Illa2(int i, Vector3 pos, Transform p, Transform luces)
    {
        var go = new GameObject("Illa_Cap2_" + (i + 1));
        go.transform.SetParent(p, false);
        go.transform.position = pos;
        var c = go.AddComponent<Coleccionable>();
        c.titulo = IllasCap2[i, 0];
        c.descripcion = IllasCap2[i, 1];
        c.destello = true;
        c.radio = 1.7f;
        c.altura = 0.15f;
        Visual(go, new Vector3(0.09f, 0.12f, 0.07f), "mat_illa", 0.06f);
        Visual(go, new Vector3(0.05f, 0.05f, 0.05f), "mat_yeso", 0.15f);
        var l = new GameObject("Brillo").AddComponent<Light>();
        l.transform.SetParent(go.transform, false);
        l.transform.localPosition = Vector3.up * 0.35f;
        l.type = LightType.Point;
        l.color = new Color(1f, 0.8f, 0.4f);
        l.intensity = 0.8f;
        l.range = 1.6f;
    }

    private static void C2_Extras(Transform nivel, Transform pers, Transform ev, Transform luces)
    {
        var r = new GameObject("Z8_Extras").transform;
        r.SetParent(nivel, false);
        padre = r;
        CrearTito(new Vector3(1.4f, 0f, -3.6f), 0f, pers);
        Illa2(2, new Vector3(-76.5f, 7.6f, 153.5f), r, luces);
        Graf("resiste", new Vector3(-22f, 3f, 123.95f), Vector3.back, 3f, r);
        Zona("Z_Tutorial_Tito", new Vector3(0f, 1.5f, 20f), new Vector3(8f, 3f, 3f), ev, new Acciones
        {
            mensaje = "Tito pelea cuerpo a cuerpo y te ayuda con las acciones dobles: trepar, palancas, cerrojos"
        });
    }
}
