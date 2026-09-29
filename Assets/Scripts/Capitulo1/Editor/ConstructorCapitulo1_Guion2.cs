/// <summary>
/// Guion ampliado del Capitulo 1: archivos nuevos, illas de Alasitas, y los textos de
/// Wara (la cebra) y el Chino (lustrabotas). Separado para que narrativa lo edite facil.
/// </summary>
public static partial class ConstructorCapitulo1
{
    // ---------- Archivos nuevos ----------

    private const string TextoChatVecinos =
        "<b>SOPOCACHI UNIDOS (grupo)</b>\n\n" +
        "<b>Doña Marta:</b> vecinos alguien sabe por qué hay tanta ambulancia en la Ecuador??\n" +
        "<b>Jhonny:</b> dicen que es rabia, un perro mordió a varios en el mercado\n" +
        "<b>Lic. Zenteno:</b> No difundan rumores. El SEDES ya sacó comunicado. Es una fiebre.\n" +
        "<b>Doña Marta:</b> mi sobrino es enfermero en el Hospital de Clínicas dice que los muertos NO SE QUEDAN MUERTOS\n" +
        "<b>Jhonny:</b> jajaja ya pues doña Marta\n" +
        "<b>Kevin:</b> no es joda. acabo de ver a mi vecino morder a un policía en la Belisario Salinas\n" +
        "<b>Lic. Zenteno:</b> Unos dicen que es culpa del gobierno, otros de la oposición. Ahorita eso no importa. Cierren sus puertas.\n" +
        "<b>Doña Marta:</b> ALGUIEN ESTÁ GOLPEANDO MI PUERTA\n" +
        "<b>Doña Marta:</b> ayuda\n" +
        "<color=#888888>Doña Marta salió del grupo</color>";

    private const string TextoPeriodico =
        "EL CHASQUI PACEÑO — Viernes\n\n" +
        "<b>FIEBRE DEL CHOQUEYAPU: AUTORIDADES DESCARTAN EPIDEMIA</b>\n\n" +
        "Luego de que se reportaran 23 casos de un cuadro febril con conducta agresiva en barrios ribereños, el SEDES aseguró que \"no hay motivo de alarma\" y que se trataría de una intoxicación por aguas servidas.\n\n" +
        "Vecinos de Obrajes denuncian que desde hace semanas el río \"huele a hongo\" y que aparecieron costras amarillas en las piedras.\n\n" +
        "En la página 4: Alasitas, la feria de los deseos, abrirá igual en enero. \"El Ekeko no se suspende\", dicen los artesanos.";

    private const string TextoOrdenMilitar =
        "COMANDO DEPARTAMENTAL — ORDEN URGENTE\n\n" +
        "1. Cerrar el paso entre la hoyada y la ciudad de El Alto en la autopista y en las estaciones del teleférico.\n" +
        "2. Suspender el servicio de Mi Teleférico y de minibuses hacia la Ceja.\n" +
        "3. Todo civil con mordeduras o rasguños debe ser trasladado al estadio Hernando Siles para evaluación.\n" +
        "4. Ante agresión de sujetos que no respondan a órdenes verbales, se autoriza el uso de armas.\n\n" +
        "Nota a mano, al reverso:\n<i>\"Del estadio nadie ha vuelto. Sargento Choque.\"</i>";

    private const string TextoRecetaBety =
        "Hoja de cuaderno, letra redonda y cuidadosa:\n\n" +
        "<i>Para Rosita, para la fiebre de tu Efraín:</i>\n\n" +
        "Muña, wira wira y eucalipto, hervido en agua de vertiente, NO del grifo.\n" +
        "Frotarle el pecho con grasa de llama y cubrirlo con aguayo.\n" +
        "Si tiene los ojos negros y hace ruido con la garganta, ya no es tu Efraín. Enciérralo y reza.\n\n" +
        "Mi abuelo, que era kallawaya, decía que hay males que bajan del río. Que solo se detienen en la altura, donde el frío no los deja crecer.\n\n" +
        "— Beatriz Mamani";

    private const string TextoNotaCirilo =
        "Papel manchado, pegado a la puerta de la cámara frigorífica:\n\n" +
        "\"Me mordió el perro del mercado el martes. Desde ahí tengo hambre todo el tiempo. Hambre de carne cruda.\n\n" +
        "Anoche casi muerdo a mi hija.\n\n" +
        "Me voy a encerrar en la cámara. Hace frío adentro, dicen que el frío la frena.\n\n" +
        "SI ESCUCHAN GOLPES, NO ABRAN. POR NADA DEL MUNDO.\n\n" +
        "— Cirilo, puesto 7\"";

    private const string TextoNotaEstacion =
        "Hoja de cuaderno pegada con cinta a un poste, cerca de las gradas:\n\n" +
        "<b>WARA:</b>\n" +
        "Hijita, nos vamos a la casa de tu tía Juana en la Ceja, la del portón verde, detrás de la Plaza de la Cruz.\n" +
        "El teleférico no funciona. Vamos a pie, siguiendo los cables de la Línea Amarilla.\n" +
        "No tomes agua del grifo. Te esperamos.\n\n" +
        "— Mamá (Rosario)";

    // ---------- Illas de Alasitas ----------

    private static readonly string[,] Illas =
    {
        { "Ekeko de yeso", "El Ekeko, dios de la abundancia, bigotudo y cargado de todo lo que uno desea: casas, comida, billetes. Alguien lo dejó en el VIP de la discoteca, como pidiendo una noche con suerte." },
        { "Billetitos de Alasitas", "Un fajo de billetes miniatura, de esos que se hacen bendecir a mediodía del 24 de enero. En uno está escrito a mano: \"para el anticrético, 2027\"." },
        { "Casita en miniatura", "Una casita de yeso con techo de calamina. Una pareja joven soñaba con salir del cuarto alquilado. En la base dice: \"Pame y Luis\"." },
        { "Título profesional", "Un título de licenciatura en miniatura, con un sello dorado. Alguien de la farmacia soñaba con recibirse. Tal vez la misma doctora, hace muchos años." },
        { "Minibús de juguete", "Un minibús diminuto con su letrero: \"Sopocachi - Ceja\". Los choferes lo compran para que nunca les falte pasajero." },
        { "Pasaporte miniatura", "Un pasaporte de cartón con la foto recortada de una señora. Soñaba con viajar a ver a sus hijos en España. En Alasitas todo cabe en la palma de la mano." },
        { "Maletita de viaje", "Una maletita de plástico, llena de papelitos con deseos. Uno dice: \"que mi mamá se cure\". Otro: \"que llegue sano a la casa\"." },
        { "Camioncito de carga", "Un camioncito de madera cargado de bolsitas de azúcar, arroz y fideo miniatura. Para que en la casa nunca falte la comida." }
    };
}
