using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vecino asustado del Acto I: al acercarse Mateo, advierte del brote y da una pista.
/// Reutiliza el motor de dialogos con respuestas que afectan la moral del grupo.
/// </summary>
public class DisparadorDialogoVecino : MonoBehaviour
{
    [Min(0.5f)] public float radio = 4f;

    private Transform jugador;
    private bool disparado;

    private void Start()
    {
        MateoController mc = Object.FindFirstObjectByType<MateoController>();
        if (mc != null)
        {
            jugador = mc.transform;
        }
    }

    private void Update()
    {
        if (disparado || jugador == null)
        {
            return;
        }

        if (Vector3.Distance(jugador.position, transform.position) <= radio)
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        disparado = true;

        if (DialogoController.Instancia == null)
        {
            return;
        }

        string[] lineas =
        {
            "Oiga! No vaya al centro, ahi estan atacando. Vi como uno se le fue encima a la casera del mercado.",
            "La gente enferma... muerde. Y no para. Esto no es normal, joven."
        };

        List<RespuestaDialogo> resp = new List<RespuestaDialogo>
        {
            new RespuestaDialogo { texto = "Calmese. Que fue exactamente lo que vio?", replica = "Mordio a la gente y seguia. Yo me largo a mi pueblo ahora mismo.", moral = 3 },
            new RespuestaDialogo { texto = "Venga con nosotros, en grupo es mas seguro.", replica = "No... no. Yo me voy solo. Cuidese usted, y suerte.", moral = 5 },
            new RespuestaDialogo { texto = "No tengo tiempo para esto.", replica = "Alla usted. Yo le avise, no diga que no.", moral = -3 }
        };

        DialogoController.Instancia.Iniciar("Vecino asustado", lineas, resp);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 0.9f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
