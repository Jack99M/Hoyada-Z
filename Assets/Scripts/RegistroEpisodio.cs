using UnityEngine;

/// <summary>
/// Registra las decisiones clave del Episodio I y arma el epilogo/resumen final.
/// Se alimenta desde los puntos de decision y se lee al cerrar el episodio.
/// </summary>
public class RegistroEpisodio : MonoBehaviour
{
    public static RegistroEpisodio Instancia { get; private set; }

    public string bety = "no se cruzo con Mama Bety";
    public string provisiones = "no consiguio provisiones";
    public string hombreAtrapado = "no llego al hombre atrapado";
    public bool infectadoVisto;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    /// <summary>Registra una decision de acto segun el titulo del panel.</summary>
    public void RegistrarDecision(string tituloDecision, string opcionElegida)
    {
        if (string.IsNullOrEmpty(tituloDecision))
        {
            return;
        }

        string t = tituloDecision.ToLower();
        if (t.Contains("provision"))
        {
            provisiones = opcionElegida;
        }
        else if (t.Contains("atrapado"))
        {
            hombreAtrapado = opcionElegida;
        }
    }

    public void RegistrarBety(string respuesta)
    {
        bety = respuesta;
    }

    /// <summary>Arma el texto del epilogo leyendo las decisiones y el estado final.</summary>
    public string Resumen()
    {
        int confianza = DialogoController.Instancia != null ? DialogoController.Instancia.confianzaBety : 0;
        GrupoEstado e = GrupoEstado.Instancia;

        string s = "- RESUMEN DEL EPISODIO I -\n\n";
        s += "Con Mama Bety: " + bety + "\n";
        s += "Provisiones: " + provisiones + "\n";
        s += "El hombre atrapado: " + hombreAtrapado + "\n";
        if (infectadoVisto)
        {
            s += "Sobreviviste al primer infectado.\n";
        }

        if (e != null)
        {
            int recursos = e.comida + e.agua + e.medicina + e.abrigo;
            string estadoRecursos = recursos >= 14 ? "buenos" : (recursos >= 8 ? "justos" : "escasos");
            s += "\nMoral del grupo: " + Calificar(e.moral) + " (" + e.moral + ")\n";
            s += "Salud de Mateo: " + Calificar(e.salud) + " (" + e.salud + ")\n";
            s += "Recursos: " + estadoRecursos + "\n";
        }

        string relacion = confianza >= 65 ? "te ganaste su respeto" : (confianza >= 40 ? "tibia" : "desconfia de ti");
        s += "Confianza de Bety: " + relacion + " (" + confianza + ")";
        return s;
    }

    private string Calificar(int valor)
    {
        if (valor >= 70)
        {
            return "firme";
        }

        return valor >= 40 ? "tambaleante" : "quebrada";
    }
}
