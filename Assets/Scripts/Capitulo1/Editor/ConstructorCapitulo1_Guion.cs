/// <summary>
/// Guion del Capitulo 1 "Resaca": tarjetas, mensajes del celular, archivos y final.
/// Separado del constructor para que el equipo de narrativa lo pueda editar facil.
/// </summary>
public static partial class ConstructorCapitulo1
{
    private static void GuionIntro(Juego j)
    {
        j.tituloCapitulo = "Capítulo 1: Resaca";
        j.tarjetasIntro = new[]
        {
            "La Paz, Bolivia",
            "Sopocachi. Sábado, 05:40 de la madrugada.",
            "Hace tres días, el SEDES reportó una \"fiebre rara\" en los barrios junto al río Choqueyapu.\nNadie le dio importancia."
        };
        j.alEmpezar = new Acciones
        {
            subtitulos = new[]
            {
                "Mateo|Ugh... mi cabeza. ¿Qué hora es?",
                "Mateo|¿Diego? ¿Chicos? ...¿Dónde se fue todo el mundo?",
                "Mateo|Mi celular... lo dejé en la barra."
            },
            objetivo = "Busca tu celular en la barra de la discoteca",
            mensaje = "Mouse: mirar  ·  WASD: moverte  ·  E: interactuar"
        };
        j.finalHonesto = new[]
        {
            "Bety no dice nada. Te lava la herida con agua hervida y hierbas que huelen a muña y eucalipto.",
            "\"Mi abuelo era kallawaya\", susurra. \"Él decía que hay males que no se curan... pero que se pueden detener.\"",
            "Afuera, el sol sale sobre el Illimani como cualquier otro día.",
            "A las 07:48, Mateo empieza a toser."
        };
        j.finalMentira = new[]
        {
            "Bety te mira un largo rato. Luego asiente, sin creerte.",
            "Te deja agua, pan y una frazada en el cuarto del fondo. Y cierra la puerta con llave. Desde afuera.",
            "Afuera, el sol sale sobre el Illimani como cualquier otro día.",
            "A las 07:48, Mateo empieza a toser. Nadie lo escucha."
        };
    }

    private const string TextoCelular =
        "<b>MAMÁ BETY (vecina)</b>  01:12\nMateo, hijito, ¿dónde estás? Dicen en la radio que no salgamos. Hay gente enferma que ataca.\n\n" +
        "<b>DIEGO</b>  01:47\nbro me voy, la gente se está volviendo loca en la pista. uno mordió al guardia. SAL DE AHÍ\n\n" +
        "<b>DIEGO</b>  01:52\nte dejé dormido en el vip, perdón bro. no podía cargarte\n\n" +
        "<b>MAMÁ BETY</b>  03:30\nCerré el portón con cadena. Solo abro si eres tú. Ven por las gradas de atrás del mercado, la avenida está tomada.\n\n" +
        "<b>MAMÁ BETY</b>  04:58\nMateo contesta por favor\n\n" +
        "<color=#ff7766>Batería: 3%</color>";

    private const string TextoNotaDJ =
        "Rami:\n\nSi lees esto, cerré la cortina por fuera como dijo el jefe. La policía dice que no dejemos salir a nadie hasta que llegue la ambulancia.\n\n" +
        "Los que se pusieron mal estaban tosiendo toda la noche. Tenían los ojos negros, como si se les hubiera reventado todo por dentro.\n\n" +
        "El Diego se llevó a los que pudo por la puerta de atrás. Tu llavero lo tienes tú, así que el depósito queda cerrado.\n\n" +
        "No le abras a nadie.\n\n— Checo";

    private const string TextoSEDES =
        "SERVICIO DEPARTAMENTAL DE SALUD — LA PAZ\nALERTA SANITARIA N° 14\n\n" +
        "Se comunica a la población que se registraron casos de un cuadro febril agudo en zonas cercanas al río Choqueyapu (Sopocachi, San Pedro, Obrajes).\n\n" +
        "SÍNTOMAS: fiebre alta, temblores, pupilas dilatadas, venas oscuras, conducta agresiva.\n\n" +
        "RECOMENDACIONES:\n• No consuma agua de la red sin hervir.\n• Evite el contacto con personas con síntomas.\n• Ante mordeduras o rasguños, acuda INMEDIATAMENTE al centro de salud.\n• Permanezca en su domicilio.\n\n" +
        "La situación está bajo control.";

    private const string TextoDoctora =
        "Para quien lea esto:\n\nAtendí a once personas esta noche. Todas con lo mismo: fiebre, temblores, las venas negras.\n" +
        "Todas habían tomado agua de la pila del mercado.\n\nNo es rabia. No es gripe. Los antibióticos no hacen nada.\n" +
        "Un paciente murió a las 2:10. A las 2:25 se levantó.\n\nMe mordió en la mano. Ya siento la fiebre.\n\n" +
        "Dejé la llave del mercado en el depósito, al fondo. Adentro hay agua embotellada: que la saquen para los vecinos.\n\n" +
        "Que Dios nos perdone.\n— Dra. Julia Mamani";

    private const string TextoCuaderno =
        "Cuaderno de ventas — Rosa Quispe, puesto 23\n\n" +
        "Lunes: 40 Bs de papa, 25 de chuño. Poca gente.\n" +
        "Martes: la pila del mercado sale con olor raro. Don Efraín dice que es el río.\n" +
        "Miércoles: la vecina del 12 no vino. Dicen que tiene fiebre.\n" +
        "Jueves: vinieron camionetas del hospital. Se llevaron a tres caseras.\n" +
        "Viernes: cerramos temprano. Don Efraín ya no es él. Tiene como costras amarillas en la cara y hace un ruido con la garganta, clic, clic, como si llamara a alguien. " +
        "No ve nada, pero te encuentra igual. Si haces ruido, te encuentra.\n\n" +
        "Si me encuentras, no me abras.";

    private const string TextoPolicia =
        "RADIO PATRULLA 214 — Transcripción\n\n" +
        "04:10 — Central, cerramos la 20 de Octubre a la altura del mercado. Hay más de cuarenta sujetos agresivos.\n" +
        "04:22 — El sargento Choque fue mordido. Dice que está bien.\n" +
        "04:31 — Central, el sargento... ya no responde. Solicitamos apoyo. Solicitamos...\n" +
        "04:33 — [estática]";
}
