using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Motor de dialogos del Episodio I. Presenta lineas narrativas una a una y luego
/// un set de respuestas que afectan la moral del grupo y la confianza del personaje.
/// Incluye la primera conversacion con Dona Beatriz "Mama Bety".
/// </summary>
public class DialogoController : MonoBehaviour
{
    public static DialogoController Instancia { get; private set; }

    public enum Fase { Inactivo, Lineas, Respuestas, Replica }
    public Fase FaseActual { get; private set; } = Fase.Inactivo;

    public string Hablante { get; private set; }
    public string LineaActual { get; private set; }
    public List<RespuestaDialogo> Respuestas { get; private set; } = new List<RespuestaDialogo>();

    [Header("Relacion con Mama Bety (0-100)")]
    [Range(0, 100)] public int confianzaBety = 55;

    private Queue<string> cola = new Queue<string>();
    private List<RespuestaDialogo> respuestasPendientes = new List<RespuestaDialogo>();

    public bool HayDialogo { get { return FaseActual != Fase.Inactivo; } }
    public static bool DialogoPausa { get { return Instancia != null && Instancia.HayDialogo; } }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    /// <summary>Inicia un dialogo generico: primero las lineas, luego las respuestas.</summary>
    public void Iniciar(string hablante, string[] lineas, List<RespuestaDialogo> respuestas)
    {
        if (HayDialogo)
        {
            return;
        }

        Hablante = hablante;
        cola = new Queue<string>(lineas);
        respuestasPendientes = respuestas ?? new List<RespuestaDialogo>();
        Respuestas = new List<RespuestaDialogo>();
        FaseActual = Fase.Lineas;
        SiguienteLinea();
    }

    private void SiguienteLinea()
    {
        if (cola.Count > 0)
        {
            LineaActual = cola.Dequeue();
            FaseActual = Fase.Lineas;
        }
        else if (respuestasPendientes.Count > 0)
        {
            Respuestas = respuestasPendientes;
            FaseActual = Fase.Respuestas;
        }
        else
        {
            Cerrar();
        }
    }

    /// <summary>Boton "Continuar": avanza lineas o cierra tras la replica.</summary>
    public void Continuar()
    {
        if (FaseActual == Fase.Lineas)
        {
            SiguienteLinea();
        }
        else if (FaseActual == Fase.Replica)
        {
            Cerrar();
        }
    }

    /// <summary>El jugador elige una respuesta: aplica efectos y muestra la replica.</summary>
public void Responder(int indice)
    {
        if (FaseActual != Fase.Respuestas)
        {
            return;
        }

        if (indice < 0 || indice >= Respuestas.Count)
        {
            return;
        }

        RespuestaDialogo r = Respuestas[indice];
        if (GrupoEstado.Instancia != null)
        {
            GrupoEstado.Instancia.moral = Mathf.Clamp(GrupoEstado.Instancia.moral + r.moral, 0, 100);
        }
        confianzaBety = Mathf.Clamp(confianzaBety + r.confianza, 0, 100);

        if (RegistroEpisodio.Instancia != null)
        {
            RegistroEpisodio.Instancia.RegistrarBety(r.texto);
        }

        LineaActual = r.replica;
        Respuestas = new List<RespuestaDialogo>();
        FaseActual = Fase.Replica;

        Debug.Log("[Hoyada Z] Respuesta elegida -> Moral y confianza actualizadas. ConfianzaBety=" + confianzaBety);
    }

    private void Cerrar()
    {
        FaseActual = Fase.Inactivo;
        LineaActual = string.Empty;
        Respuestas = new List<RespuestaDialogo>();
        cola.Clear();
    }

    /// <summary>Primera conversacion con Dona Beatriz en el Acto I.</summary>
    public void IniciarConversacionBety()
    {
        string[] lineas =
        {
            "Joven, usted tambien escucho lo de la gente que ataca? En el mercado ya no se habla de otra cosa.",
            "Yo no me quedo aqui a esperar. Si va a salir de la zona, mejor vamos juntos. Se de hierbas, se curar heridas."
        };

        List<RespuestaDialogo> resp = new List<RespuestaDialogo>
        {
            new RespuestaDialogo { texto = "Venga con nosotros, Dona Beatriz. La necesitamos.", replica = "Entonces no perdamos tiempo. Cuidense y cuiden al grupo.", moral = 6, confianza = 12 },
            new RespuestaDialogo { texto = "No se si es buena idea cargar con mas gente.", replica = "Hmm. Ya veo que clase de lider es usted. Igual le sigo, no me queda otra.", moral = -4, confianza = -10 },
            new RespuestaDialogo { texto = "Que sabe usted de lo que esta pasando?", replica = "Se lo que veo: la gente cambia despues de morder o ser mordida. No es gripe, joven.", moral = 1, confianza = 3 }
        };

        Iniciar("Dona Beatriz", lineas, resp);
    }
}
