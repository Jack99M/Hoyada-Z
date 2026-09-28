/// <summary>
/// Guion del Capitulo 2 "Corte de Paso": tarjetas de entrada, primer objetivo y final.
/// Separado del constructor para que el equipo de narrativa lo pueda editar facil.
/// Las lineas que dependen del Capitulo 1 (Wara se quedo con Mateo o se fue) estan en
/// las zonas Z_Inicio_Queda / Z_Inicio_Fue de ConstructorCapitulo2_Zonas.cs.
/// </summary>
public static partial class ConstructorCapitulo1
{
    private static void GuionIntroCap2(Juego j)
    {
        j.tituloCapitulo = "Capítulo 2: Corte de Paso";
        j.tarjetasIntro = new[]
        {
            "El Alto, Bolivia",
            "La Ceja. Sábado, 16:20.",
            "Desde el amanecer, la fiebre del Choqueyapu subió por las laderas.\nA media tarde, la Feria 16 de Julio ya no era una feria.",
            "Wara Condori subió sola a El Alto para buscar a su mamá en Villa Dolores.\nEn la feria, un técnico de celulares la sacó de entre la turba: Tito Huanca."
        };
        j.alEmpezar = new Acciones
        {
            objetivo = "Sal del callejón antes de que rompan la reja",
            usarMarcador = true,
            marcador = new UnityEngine.Vector3(0f, 0f, 31f),
            mensaje = "Juegas como Wara  ·  [2] honda: silenciosa, aturde  ·  [Shift] correr  ·  [E] interactuar"
        };
        j.finalHonesto = new[]
        {
            "Villa Dolores. 17:52.",
            "El portón azul de doña Rosario está entreabierto. Adentro no hay luz. Solo el olor a api frío y una radio encendida sin señal.",
            "Sobre la mesa, una nota escrita con lápiz labial:\n«Wara: me llevaron al coliseo de la Ceja. Dicen que ahí hay médicos. Te espero. Mamá.»",
            "Afuera, el helicóptero repite su anuncio hasta perderse detrás del Illimani.",
            "Tito cierra el portón y se sienta en el suelo, con la llave inglesa sobre las rodillas.\n\"El coliseo queda a doce cuadras. Con toque de queda... va a ser otra cosa, cebrita.\"",
            "Son las 17:58."
        };
        j.finalMentira = j.finalHonesto;
    }
}
