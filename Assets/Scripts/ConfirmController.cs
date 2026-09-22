using System;
using UnityEngine;

/// <summary>
/// Confirmacion Si/No generica con callbacks. La usan puertas y otras acciones.
/// Mientras esta activa, el juego se pausa.
/// </summary>
public class ConfirmController : MonoBehaviour
{
    public static ConfirmController Instancia { get; private set; }

    public bool Activo { get; private set; }
    public string Pregunta { get; private set; }

    private Action onSi;
    private Action onNo;

    public static bool Pausa { get { return Instancia != null && Instancia.Activo; } }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    public void Pedir(string pregunta, Action si, Action no)
    {
        if (Activo)
        {
            return;
        }

        Pregunta = pregunta;
        onSi = si;
        onNo = no;
        Activo = true;
    }

    public void Responder(bool esSi)
    {
        if (!Activo)
        {
            return;
        }

        Activo = false;
        Action cb = esSi ? onSi : onNo;
        onSi = null;
        onNo = null;
        Pregunta = null;

        if (cb != null)
        {
            cb();
        }
    }
}
