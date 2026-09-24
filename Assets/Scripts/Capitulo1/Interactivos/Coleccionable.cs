using UnityEngine;

/// <summary>
/// Illa de Alasitas: miniatura coleccionable escondida en el nivel (la feria de Alasitas se
/// celebra en La Paz cada 24 de enero; se compran miniaturas de lo que uno desea y el Ekeko
/// las carga). Cada una tiene su pequena historia. Cuenta para el rango y un logro.
/// </summary>
public class Coleccionable : Interactivo
{
    public string titulo = "Illa";
    [TextArea(3, 10)] public string descripcion;

    public bool Recogido { get; private set; }

    private Vector3 basePos;
    private float fase;

    private void Reset()
    {
        destello = true;
        radio = 1.6f;
        altura = 0.25f;
    }

    private void Start()
    {
        basePos = transform.position;
        fase = Random.value * 6f;
    }

    private void Update()
    {
        fase += Time.deltaTime;
        transform.position = basePos + Vector3.up * (Mathf.Sin(fase * 2.2f) * 0.025f);
        transform.Rotate(0f, 40f * Time.deltaTime, 0f);
    }

    public override string Prompt { get { return "Recoger illa: " + titulo; } }

    public override void Usar(Jugador j)
    {
        if (Recogido) { return; }
        Recogido = true;
        Juego g = Juego.I;
        g.RegistrarColeccionable();
        int n = Stats.D.coleccionables;
        AudioCap1.Play2D("sfx_logro", 0.5f);
        g.Mensaje("Illa de Alasitas " + n + " / " + g.ColeccionablesTotales);
        Diario.Registrar("Illa: " + titulo, descripcion, false);
        g.Leer("Illa de Alasitas: " + titulo, descripcion + "\n\n<i>Illas encontradas: " + n + " de " + g.ColeccionablesTotales + "</i>", false, null, transform.position);
        gameObject.SetActive(false);
    }

    public void MarcarRecogido()
    {
        Recogido = true;
        Diario.Registrar("Illa: " + titulo, descripcion, false);
        gameObject.SetActive(false);
    }
}
