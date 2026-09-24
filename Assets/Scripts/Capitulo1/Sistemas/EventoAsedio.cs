using UnityEngine;

/// <summary>
/// Asedio final: Mateo debe resistir un tiempo mientras llegan oleadas de infectados.
/// Al terminar ejecuta sus acciones (por ejemplo, Bety abre el porton).
/// </summary>
public class EventoAsedio : MonoBehaviour
{
    [System.Serializable]
    public class Oleada
    {
        public float tiempo;
        public Infectado[] infectados;
        [Tooltip("Frase al empezar la oleada (Quien|Texto).")]
        public string aviso;
    }

    public float duracion = 50f;
    public Oleada[] oleadas;
    public Acciones alTerminar = new Acciones();

    public bool Activo { get; private set; }
    public bool Terminado { get; private set; }
    public float Restante { get { return Activo ? Mathf.Max(0f, duracion - (Time.time - inicio)) : 0f; } }

    private float inicio;
    private int siguiente;

    public void Iniciar()
    {
        if (Activo || Terminado)
        {
            return;
        }
        Activo = true;
        inicio = Time.time;
        siguiente = 0;
        AudioCap1.Musica("mus_jefe", 1.5f);
    }

    public void MarcarTerminado()
    {
        Activo = false;
        Terminado = true;
    }

    private void Update()
    {
        if (!Activo)
        {
            return;
        }

        float t = Time.time - inicio;
        while (oleadas != null && siguiente < oleadas.Length && t >= oleadas[siguiente].tiempo)
        {
            Oleada o = oleadas[siguiente];
            foreach (Infectado inf in o.infectados)
            {
                if (inf != null && !inf.Muerto)
                {
                    inf.gameObject.SetActive(true);
                    inf.Alertar();
                }
            }
            if (!string.IsNullOrEmpty(o.aviso) && Juego.I != null)
            {
                string[] p = o.aviso.Split('|');
                if (p.Length >= 2) { Juego.I.Decir(p[0], p[1]); }
            }
            siguiente++;
        }

        if (t >= duracion)
        {
            Activo = false;
            Terminado = true;
            AudioCap1.Musica(null, 2f);
            alTerminar.Ejecutar(transform.position);
        }
    }

    /// <summary>Si Mateo muere durante el asedio, vuelve a empezar.</summary>
    public void Reiniciar()
    {
        if (!Activo)
        {
            return;
        }
        if (oleadas != null)
        {
            foreach (Oleada o in oleadas)
            {
                foreach (Infectado inf in o.infectados)
                {
                    if (inf != null && !inf.Muerto)
                    {
                        inf.Reiniciar();
                        inf.gameObject.SetActive(false);
                    }
                }
            }
        }
        inicio = Time.time;
        siguiente = 0;
    }
}
