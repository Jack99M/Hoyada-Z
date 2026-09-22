using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquesta el flujo jugable del Episodio I: decisiones de acto, cierre del episodio
/// y GAME OVER cuando el grupo entra en crisis (moral o salud a cero).
/// Mientras hay una decision, el final o el game over en curso, el juego esta en pausa.
/// </summary>
public class DecisionController : MonoBehaviour
{
    public static DecisionController Instancia { get; private set; }

    // --- Decision activa ---
    public bool HayDecision { get; private set; }
    public string TituloDecision { get; private set; }
    public string DescDecision { get; private set; }
    public List<DecisionEpisodio> Opciones { get; private set; } = new List<DecisionEpisodio>();

    // --- Final del episodio ---
    public bool FinActivo { get; private set; }
    public float FinAlpha { get; private set; }
    private float finTimer;

    // --- Game over ---
    public bool GameOverActivo { get; private set; }
    public string MotivoGameOver { get; private set; }
    public float GameOverAlpha { get; private set; }
    private float gameOverTimer;

    public static bool PausaActiva
    {
        get { return Instancia != null && (Instancia.HayDecision || Instancia.FinActivo || Instancia.GameOverActivo); }
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    private void Update()
    {
        if (FinActivo)
        {
            finTimer += Time.deltaTime;
            FinAlpha = Mathf.Clamp01(finTimer / 2f);
        }

        if (GameOverActivo)
        {
            gameOverTimer += Time.deltaTime;
            GameOverAlpha = Mathf.Clamp01(gameOverTimer / 1.5f);
        }

        VigilarCrisis();
    }

    /// <summary>Si el grupo entra en crisis (y no hay ya un cierre en curso), dispara game over.</summary>
    private void VigilarCrisis()
    {
        if (GameOverActivo || FinActivo || HayDecision || DialogoController.DialogoPausa)
        {
            return;
        }

        GrupoEstado e = GrupoEstado.Instancia;
        if (e != null && e.EnCrisis())
        {
            string motivo = e.salud <= 20
                ? "Mateo no resistio. El grupo pierde a su guia y se dispersa."
                : "La moral se derrumbo. El grupo se rompe y cada quien huye por su lado.";
            DispararGameOver(motivo);
        }
    }

    // --- Acto II: provisiones ---
    public void PresentarProvisiones()
    {
        if (HayDecision || FinActivo || GameOverActivo)
        {
            return;
        }

        TituloDecision = "Acto II  -  Conseguir provisiones";
        DescDecision = "El grupo necesita agua, comida, medicina y abrigo. No hay opcion perfecta. Donde vamos?";
        Opciones = new List<DecisionEpisodio>
        {
            new DecisionEpisodio { titulo = "Mercado: mas provisiones, pero mucha gente", comida = 3, agua = 2, moral = -8, salud = -5 },
            new DecisionEpisodio { titulo = "Farmacia: medicinas, pero riesgo de heridos", medicina = 3, moral = -5, salud = -8 },
            new DecisionEpisodio { titulo = "Casa de un conocido: seguro, pero menos recursos", abrigo = 2, comida = 1, moral = 5 }
        };
        HayDecision = true;
    }

    // --- Acto IV: dilema del hombre atrapado ---
    public void PresentarHombreAtrapado()
    {
        if (HayDecision || FinActivo || GameOverActivo)
        {
            return;
        }

        TituloDecision = "Acto IV  -  Un hombre atrapado";
        DescDecision = "Un hombre quedo atrapado entre los autos volcados y grita. El grupo empuja para seguir. Que haces?";
        Opciones = new List<DecisionEpisodio>
        {
            new DecisionEpisodio { titulo = "Ayudarlo a salir (arriesgado, cuesta tiempo)", moral = 10, salud = -8 },
            new DecisionEpisodio { titulo = "Abandonarlo y seguir sin mirar atras", moral = -12 },
            new DecisionEpisodio { titulo = "Convencer al grupo de esperar un momento", moral = 3, salud = -3 },
            new DecisionEpisodio { titulo = "Buscar otra forma de sacarlo", moral = 6, salud = -4, medicina = -1 }
        };
        HayDecision = true;
    }

    /// <summary>Aplica la opcion elegida y cierra el panel de decision.</summary>
public void Elegir(int indice)
    {
        if (!HayDecision)
        {
            return;
        }

        if (indice >= 0 && indice < Opciones.Count)
        {
            if (GrupoEstado.Instancia != null)
            {
                GrupoEstado.Instancia.AplicarDecision(Opciones[indice]);
            }

            if (RegistroEpisodio.Instancia != null)
            {
                RegistroEpisodio.Instancia.RegistrarDecision(TituloDecision, Opciones[indice].titulo);
            }
        }

        HayDecision = false;
        Opciones = new List<DecisionEpisodio>();
    }

    /// <summary>Acto V: cierre del episodio con fundido a negro.</summary>
    public void FinEpisodio()
    {
        if (FinActivo || GameOverActivo)
        {
            return;
        }

        HayDecision = false;
        FinActivo = true;
        finTimer = 0f;
        Debug.Log("[Hoyada Z] Fin del Episodio I disparado.");
    }

    /// <summary>Dispara la pantalla de game over con un motivo.</summary>
    public void DispararGameOver(string motivo)
    {
        if (GameOverActivo || FinActivo)
        {
            return;
        }

        HayDecision = false;
        GameOverActivo = true;
        MotivoGameOver = motivo;
        gameOverTimer = 0f;
        Debug.Log("[Hoyada Z] GAME OVER: " + motivo);
    }
}
