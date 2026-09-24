using UnityEngine;

/// <summary>
/// Botella lanzada por Mateo. Al romperse hace un ruido fuerte que atrae a los infectados
/// cercanos; si le pega a uno, lo aturde.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Botella : MonoBehaviour
{
    public float radioRuido = 15f;
    private bool rota;
    private float nacimiento;

    private void Awake()
    {
        nacimiento = Time.time;
    }

    public static GameObject CrearSimple(Vector3 pos)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        b.name = "Botella_Lanzada";
        b.transform.position = pos;
        b.transform.localScale = new Vector3(0.09f, 0.14f, 0.09f);
        b.layer = 2;
        Renderer r = b.GetComponent<Renderer>();
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s != null)
        {
            var m = new Material(s);
            m.SetColor("_BaseColor", new Color(0.15f, 0.45f, 0.2f));
            m.SetFloat("_Smoothness", 0.9f);
            r.sharedMaterial = m;
        }
        var rb = b.AddComponent<Rigidbody>();
        rb.mass = 0.4f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        b.AddComponent<Botella>();
        return b;
    }

    private void OnCollisionEnter(Collision c)
    {
        if (rota || Time.time - nacimiento < 0.05f)
        {
            return;
        }
        if (c.collider.GetComponentInParent<Jugador>() != null)
        {
            return;
        }
        rota = true;

        Vector3 p = c.contacts.Length > 0 ? c.contacts[0].point : transform.position;
        SistemaRuido.Emitir(p, radioRuido, false);
        AudioCap1.Play3D("sfx_botella", p, 1f, Random.Range(0.9f, 1.1f));
        EfectosFX.Vidrio(p);

        Infectado inf = c.collider.GetComponentInParent<Infectado>();
        if (inf != null && !inf.Muerto)
        {
            Vector3 d = inf.transform.position - transform.position; d.y = 0f;
            inf.RecibirGolpe(10, d.normalized, 2f);
        }
        Destroy(gameObject);
    }
}
