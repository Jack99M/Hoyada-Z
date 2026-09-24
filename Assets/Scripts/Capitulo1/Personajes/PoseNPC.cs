using UnityEngine;

/// <summary>
/// Pose fija para personajes de escenografia: cadaver tirado, sobreviviente sentado o
/// Bety en la ventana. Opcionalmente gira para mirar a Mateo cuando esta cerca.
/// </summary>
public class PoseNPC : MonoBehaviour
{
    public enum Pose { Cadaver, Sentado, DePie }

    public Pose pose = Pose.DePie;
    public bool mirarAlJugador;
    public float distanciaMirar = 12f;

    private AnimProcedural anim;
    private Quaternion rotInicial;

    private void Start()
    {
        anim = GetComponentInChildren<AnimProcedural>();
        rotInicial = transform.rotation;
        if (anim == null)
        {
            return;
        }
        anim.velocidad = 0f;
        switch (pose)
        {
            case Pose.Cadaver:
                anim.Morir();
                break;
            case Pose.Sentado:
                anim.agachado = true;
                break;
        }
    }

    private void Update()
    {
        if (!mirarAlJugador || pose == Pose.Cadaver || Jugador.I == null)
        {
            return;
        }
        Vector3 d = Jugador.I.transform.position - transform.position; d.y = 0f;
        Quaternion objetivo = d.magnitude < distanciaMirar && d.sqrMagnitude > 0.01f ? Quaternion.LookRotation(d) : rotInicial;
        transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, 1f - Mathf.Exp(-2.5f * Time.deltaTime));
    }
}
