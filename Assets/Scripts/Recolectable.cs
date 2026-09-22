using UnityEngine;

/// <summary>
/// Objeto recogible: al interactuar suma un recurso (o entrega una pista) y desaparece.
/// Permite "buscar suministros" y "encontrar documentos" explorando interiores.
/// </summary>
public class Recolectable : Interactuable
{
    public enum Recurso { Comida, Agua, Medicina, Abrigo, Municion, Pista }

    [Tooltip("Que otorga al recogerlo.")]
    public Recurso recurso = Recurso.Medicina;

    [Tooltip("Cantidad que suma (ignorado para Pista).")]
    public int cantidad = 1;

    [Tooltip("Texto que se muestra al recogerlo.")]
    [TextArea(1, 3)] public string texto = "Encuentras algo util.";

public override void Interactuar()
    {
        GrupoEstado e = GrupoEstado.Instancia;
        if (e != null)
        {
            switch (recurso)
            {
                case Recurso.Comida: e.comida += cantidad; break;
                case Recurso.Agua: e.agua += cantidad; break;
                case Recurso.Medicina: e.medicina += cantidad; break;
                case Recurso.Abrigo: e.abrigo += cantidad; break;
                case Recurso.Municion: e.municion += cantidad; break;
                case Recurso.Pista: break;
            }
        }

        if (recurso != Recurso.Pista && Misiones.Instancia != null)
        {
            Misiones.Instancia.ProvisionConseguida();
        }

        if (DialogoController.Instancia != null)
        {
            DialogoController.Instancia.Iniciar("", new string[] { texto }, null);
        }

        Disponible = false;
        gameObject.SetActive(false);
    }
}
