using UnityEngine;

/// <summary>
/// Puerta interactuable: al usarla pregunta "Abrir la puerta? Si/No".
/// Si acepta, se abre girando y deja pasar; si no, sigue explorando.
/// </summary>
public class Puerta : Interactuable
{
    [Tooltip("Grados que gira la puerta al abrirse.")]
    public float anguloAbierto = 95f;

    [Tooltip("Linea al abrir.")]
    [TextArea(1, 2)] public string lineaAbrir = "La puerta cede con un chirrido. Adentro huele a humedad y desinfectante.";

    [Tooltip("Linea al negarse.")]
    [TextArea(1, 2)] public string lineaNo = "Mejor no todavia. Sigo revisando afuera.";

    private bool abierta;
    private Quaternion rotAbierta;
    private Collider col;

    private void Start()
    {
        rotAbierta = transform.localRotation * Quaternion.Euler(0f, anguloAbierto, 0f);
        col = GetComponent<Collider>();

        if (string.IsNullOrEmpty(prompt) || prompt == "Interactuar")
        {
            prompt = "Abrir puerta";
        }
    }

    public override void Interactuar()
    {
        if (abierta || ConfirmController.Instancia == null)
        {
            return;
        }

        ConfirmController.Instancia.Pedir("Abrir la puerta?", Abrir, Negar);
    }

    private void Abrir()
    {
        abierta = true;
        Disponible = false;

        if (col != null)
        {
            col.enabled = false;
        }

        if (DialogoController.Instancia != null)
        {
            DialogoController.Instancia.Iniciar("", new string[] { lineaAbrir }, null);
        }
    }

    private void Negar()
    {
        if (DialogoController.Instancia != null)
        {
            DialogoController.Instancia.Iniciar("", new string[] { lineaNo }, null);
        }
    }

    private void Update()
    {
        if (abierta)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotAbierta, Time.deltaTime * 4f);
        }
    }
}
