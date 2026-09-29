using UnityEngine;

/// <summary>
/// Archivo legible (nota, comunicado, cuaderno, periodico) o el celular de Mateo.
/// Cuenta la historia del brote poco a poco, como los archivos de Resident Evil.
/// Se pueden volver a leer desde el Diario (pausa).
/// </summary>
public class Documento : Interactivo
{
    public string titulo = "Nota";
    [TextArea(6, 20)] public string texto;
    [Tooltip("Se muestra con estilo de pantalla de celular.")]
    public bool celular;
    [Tooltip("Desaparece al leerlo (Mateo se lo guarda).")]
    public bool guardarAlLeer;
    public Acciones alLeer = new Acciones();

    public bool Leido { get; private set; }

    private void Reset()
    {
        destello = true;
        radio = 1.6f;
        altura = 0.3f;
    }

    public override string Prompt { get { return celular ? "Revisar celular" : "Leer: " + titulo; } }

    public override void Usar(Jugador j)
    {
        Acciones despues = Leido ? null : alLeer;
        if (!Leido)
        {
            Leido = true;
            Juego.I.RegistrarDocumento();
            if (j.Inventario != null && !j.Inventario.archivos.Contains(titulo)) { j.Inventario.archivos.Add(titulo); }
            destello = false;
            Diario.Registrar(titulo, texto, celular);
        }
        Juego.I.Leer(titulo, texto, celular, despues, transform.position);
        if (guardarAlLeer) { gameObject.SetActive(false); }
    }

    /// <summary>Al cargar una partida.</summary>
    public void MarcarLeido()
    {
        Leido = true;
        destello = false;
        Diario.Registrar(titulo, texto, celular);
        if (guardarAlLeer) { gameObject.SetActive(false); }
    }
}

/// <summary>Archivos leidos en esta sesion para volver a leerlos desde la pausa.</summary>
public static class Diario
{
    public struct Entrada { public string titulo; public string texto; public bool celular; }
    public static readonly System.Collections.Generic.List<Entrada> Entradas = new System.Collections.Generic.List<Entrada>();

    public static void Registrar(string titulo, string texto, bool celular)
    {
        foreach (Entrada e in Entradas) { if (e.titulo == titulo) { return; } }
        Entradas.Add(new Entrada { titulo = titulo, texto = texto, celular = celular });
    }

    public static void Limpiar()
    {
        Entradas.Clear();
    }
}
