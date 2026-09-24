using UnityEngine;

/// <summary>
/// Referencias a los huesos de un personaje con esqueleto generado por AutoRigHumanoide.
/// El orden de los huesos es fijo (ver constantes). La pose de reposo es la del modelo
/// original (T-pose o A-pose), con todas las rotaciones locales en identidad.
/// </summary>
public class EsqueletoHumanoide : MonoBehaviour
{
    public const int Hips = 0, Spine = 1, Chest = 2, Neck = 3, Head = 4;
    public const int UpperArmL = 5, LowerArmL = 6, HandL = 7;
    public const int UpperArmR = 8, LowerArmR = 9, HandR = 10;
    public const int UpperLegL = 11, LowerLegL = 12, FootL = 13;
    public const int UpperLegR = 14, LowerLegR = 15, FootR = 16;
    public const int Cantidad = 17;

    public static readonly string[] Nombres =
    {
        "Hips", "Spine", "Chest", "Neck", "Head",
        "UpperArm.L", "LowerArm.L", "Hand.L",
        "UpperArm.R", "LowerArm.R", "Hand.R",
        "UpperLeg.L", "LowerLeg.L", "Foot.L",
        "UpperLeg.R", "LowerLeg.R", "Foot.R"
    };

    public static readonly int[] Padres = { -1, 0, 1, 2, 3, 2, 5, 6, 2, 8, 9, 0, 11, 12, 0, 14, 15 };

    [Tooltip("Huesos en el orden de las constantes de esta clase.")]
    public Transform[] huesos = new Transform[Cantidad];

    [Tooltip("Punto donde se sujeta el arma (hijo de la mano derecha).")]
    public Transform puntoArma;

    [Tooltip("Contenedor visual (malla + esqueleto). Se rota entero al morir.")]
    public Transform visual;

    [Tooltip("Altura total del modelo en metros.")]
    public float altura = 1.75f;

    public Transform this[int i] { get { return huesos[i]; } }
}
