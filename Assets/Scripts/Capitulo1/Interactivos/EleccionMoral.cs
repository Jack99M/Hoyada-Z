using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sobreviviente con quien Mateo puede hablar. Presenta una decision moral que cambia
/// la moral, los recursos y a veces el camino (le da una pista, un objeto o te traiciona).
/// </summary>
public class EleccionMoral : Interactivo
{
    [System.Serializable]
    public class OpcionMoral
    {
        public string texto;
        public int moral;
        public int costoVendas;
        public int darVendas;
        public int darBotellas;
        public int darPilas;
        [Tooltip("Recursos extra que recibe Mateo: 'alcohol:1,trapo:1'")]
        public string dar;
        [TextArea] public string respuesta;
        public Acciones acciones = new Acciones();
    }

    public string nombreNPC = "Sobreviviente";
    [TextArea(3, 10)] public string texto;
    [Tooltip("Retrato del HUD: bety, mateo, infectado o vacio.")]
    public string retrato;
    public OpcionMoral[] opciones;

    public bool Usado { get; private set; }

    public override bool Disponible { get { return base.Disponible && !Usado; } }
    public override string Prompt { get { return "Hablar con " + nombreNPC; } }

    public void MarcarUsado()
    {
        Usado = true;
    }

    public override void Usar(Jugador j)
    {
        var lista = new List<Juego.Opcion>();
        foreach (OpcionMoral op in opciones)
        {
            OpcionMoral o = op;
            string etiqueta = o.texto;
            bool falta = o.costoVendas > 0 && j.Inventario.vendas < o.costoVendas;
            if (o.costoVendas > 0) { etiqueta += "   (-" + o.costoVendas + " venda" + (falta ? ", no tienes" : "") + ")"; }
            lista.Add(new Juego.Opcion(etiqueta, () => Resolver(j, o), falta));
        }
        Juego.I.MostrarEleccion(nombreNPC, texto, retrato, lista);
    }

    private void Resolver(Jugador j, OpcionMoral o)
    {
        Inventario inv = j.Inventario;
        if (o.costoVendas > 0 && inv.vendas < o.costoVendas)
        {
            Juego.I.Decir("Mateo", "No tengo vendas para darle...");
            return;
        }

        Usado = true;
        inv.vendas = Mathf.Max(0, inv.vendas - o.costoVendas);
        if (o.darVendas > 0) { inv.AgregarConsumible(ref inv.vendas, inv.maxVendas, o.darVendas); Juego.I.Mensaje("Recibiste: Venda x" + o.darVendas); }
        if (o.darBotellas > 0) { inv.AgregarConsumible(ref inv.botellas, inv.maxBotellas, o.darBotellas); Juego.I.Mensaje("Recibiste: Botella x" + o.darBotellas); }
        if (o.darPilas > 0) { inv.AgregarConsumible(ref inv.pilas, inv.maxPilas, o.darPilas); Juego.I.Mensaje("Recibiste: Pilas x" + o.darPilas); }
        if (!string.IsNullOrEmpty(o.dar)) { Conversacion.Entregar(inv, o.dar); }
        if (o.moral != 0) { Juego.I.CambiarMoral(o.moral); }
        if (!string.IsNullOrEmpty(o.respuesta))
        {
            foreach (string linea in o.respuesta.Split('\n'))
            {
                if (!string.IsNullOrEmpty(linea.Trim())) { Juego.I.Decir(nombreNPC, linea.Trim()); }
            }
        }
        if (o.acciones != null) { o.acciones.Ejecutar(transform.position); }
    }
}
