using UnityEngine;

/// <summary>
/// Proyectiles de Mateo creados por codigo: piedra de honda, molotov y el area de fuego
/// que deja el molotov al romperse.
/// </summary>
public static class Proyectiles
{
    private static Material matPiedra, matMolotov, matMecha;

    private static Material MatLit(Color c, float suave)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        var m = new Material(s != null ? s : Shader.Find("Standard"));
        m.SetColor("_BaseColor", c);
        m.color = c;
        m.SetFloat("_Smoothness", suave);
        return m;
    }

    public static GameObject Piedra(Vector3 origen, Vector3 objetivo, float rapidez, int dano, CombateJugador dueno)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        p.name = "Piedra_Honda";
        p.layer = 2;
        p.transform.position = origen;
        p.transform.localScale = Vector3.one * 0.09f;
        if (matPiedra == null) { matPiedra = MatLit(new Color(0.35f, 0.33f, 0.3f), 0.1f); }
        p.GetComponent<Renderer>().sharedMaterial = matPiedra;
        var rb = p.AddComponent<Rigidbody>();
        rb.mass = 0.2f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = CombateJugador.Balistica(origen, objetivo, rapidez);
        var pi = p.AddComponent<PiedraHonda>();
        pi.dano = dano;
        pi.dueno = dueno;
        if (Jugador.I != null && Jugador.I.Controlador != null) { Physics.IgnoreCollision(p.GetComponent<Collider>(), Jugador.I.Controlador); }
        var tr = p.AddComponent<TrailRenderer>();
        tr.time = 0.12f;
        tr.startWidth = 0.05f;
        tr.endWidth = 0f;
        Shader sh = Shader.Find("Sprites/Default");
        if (sh != null) { tr.sharedMaterial = new Material(sh); }
        tr.startColor = new Color(0.8f, 0.8f, 0.75f, 0.5f);
        tr.endColor = new Color(0.8f, 0.8f, 0.75f, 0f);
        Object.Destroy(p, 6f);
        return p;
    }

    public static GameObject Molotov(Vector3 pos)
    {
        var b = new GameObject("Molotov_Lanzado");
        b.layer = 2;
        b.transform.position = pos;
        var cuerpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Object.Destroy(cuerpo.GetComponent<Collider>());
        cuerpo.transform.SetParent(b.transform, false);
        cuerpo.transform.localScale = new Vector3(0.09f, 0.14f, 0.09f);
        if (matMolotov == null) { matMolotov = MatLit(new Color(0.45f, 0.3f, 0.12f), 0.85f); }
        cuerpo.GetComponent<Renderer>().sharedMaterial = matMolotov;
        cuerpo.layer = 2;
        var mecha = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(mecha.GetComponent<Collider>());
        mecha.transform.SetParent(b.transform, false);
        mecha.transform.localPosition = new Vector3(0f, 0.17f, 0f);
        mecha.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f);
        if (matMecha == null)
        {
            matMecha = MatLit(new Color(1f, 0.5f, 0.1f), 0.1f);
            matMecha.EnableKeyword("_EMISSION");
            matMecha.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.1f) * 4f);
        }
        mecha.GetComponent<Renderer>().sharedMaterial = matMecha;
        mecha.layer = 2;
        var luz = new GameObject("Llama").AddComponent<Light>();
        luz.transform.SetParent(b.transform, false);
        luz.transform.localPosition = new Vector3(0f, 0.22f, 0f);
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.55f, 0.2f);
        luz.intensity = 6f;
        luz.range = 5f;
        var col = b.AddComponent<CapsuleCollider>();
        col.radius = 0.05f;
        col.height = 0.28f;
        var rb = b.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        b.AddComponent<MolotovProyectil>();
        return b;
    }
}

/// <summary>Piedra de honda: aturde, hace poco ruido y con tiro a la cabeza puede matar.</summary>
public class PiedraHonda : MonoBehaviour
{
    public int dano = 34;
    public CombateJugador dueno;
    private bool usada;

    private void OnCollisionEnter(Collision c)
    {
        if (usada || c.collider.GetComponentInParent<Jugador>() != null)
        {
            return;
        }
        usada = true;
        Vector3 p = c.contacts.Length > 0 ? c.contacts[0].point : transform.position;
        Infectado inf = c.collider.GetComponentInParent<Infectado>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 dir = rb != null && rb.linearVelocity.sqrMagnitude > 0.01f ? rb.linearVelocity.normalized : transform.forward;
        if (inf != null && !inf.Muerto)
        {
            bool cabeza = p.y >= inf.transform.position.y + inf.AlturaCabeza - 0.2f;
            int d = cabeza ? Mathf.RoundToInt(dano * 2.6f) : dano;
            inf.RecibirImpacto(d, new Vector3(dir.x, 0f, dir.z).normalized, cabeza ? 1.6f : 1f, FuenteDano.Honda, cabeza, Jugador.I != null ? Jugador.I.transform.position : p);
            AudioCap1.Play3D("sfx_piedra", p, 0.9f, cabeza ? 1.2f : 1f);
            if (dueno != null) { dueno.ImpactoPiedra(true, cabeza); }
        }
        else
        {
            AudioCap1.Play3D("sfx_piedra", p, 0.6f, Random.Range(0.8f, 1.1f));
            SistemaRuido.Emitir(p, 4f, false);
            EfectosFX.Polvo(p);
            if (dueno != null) { dueno.ImpactoPiedra(false, false); }
        }
        Destroy(gameObject, 0.05f);
    }
}

/// <summary>Molotov en vuelo: al chocar se rompe y crea un area de fuego.</summary>
public class MolotovProyectil : MonoBehaviour
{
    private bool roto;
    private float nacimiento;

    private void Awake()
    {
        nacimiento = Time.time;
    }

    private void OnCollisionEnter(Collision c)
    {
        if (roto || Time.time - nacimiento < 0.05f || c.collider.GetComponentInParent<Jugador>() != null)
        {
            return;
        }
        roto = true;
        Vector3 p = c.contacts.Length > 0 ? c.contacts[0].point : transform.position;
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 0.5f, Vector3.down, out hit, 3f, ~((1 << 8) | (1 << 9) | (1 << 2)), QueryTriggerInteraction.Ignore))
        {
            p = hit.point;
        }
        AreaFuego.Crear(p, 3f, 7f);
        AudioCap1.Play3D("sfx_botella", p, 1f);
        EfectosFX.Vidrio(p);
        Destroy(gameObject);
    }
}

/// <summary>
/// Charco de fuego: quema a los infectados que lo pisan (y a Mateo si se mete).
/// Hace ruido y los infectados en llamas entran en panico.
/// </summary>
public class AreaFuego : MonoBehaviour
{
    public float radio = 3f;
    public float duracion = 7f;
    private float inicio;
    private float siguienteTic;
    private Fuego fuego;
    private bool apagando;

    public static AreaFuego Crear(Vector3 pos, float radio, float duracion)
    {
        var go = new GameObject("AreaFuego");
        go.transform.position = pos;
        var a = go.AddComponent<AreaFuego>();
        a.radio = radio;
        a.duracion = duracion;
        return a;
    }

    private void Start()
    {
        inicio = Time.time;
        fuego = gameObject.AddComponent<Fuego>();
        fuego.tamano = radio * 0.55f;
        fuego.conHumo = true;
        fuego.conLuz = true;
        AudioCap1.Play3D("sfx_molotov", transform.position, 1f);
        SistemaRuido.Emitir(transform.position, 16f, false);
        CamaraTPS.Sacudir(0.15f, 0.2f);
    }

    private void Update()
    {
        float t = Time.time - inicio;
        if (t > duracion)
        {
            if (!apagando)
            {
                apagando = true;
                foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>()) { ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
                foreach (Light l in GetComponentsInChildren<Light>()) { l.enabled = false; }
                Destroy(gameObject, 3f);
            }
            return;
        }
        if (Time.time < siguienteTic)
        {
            return;
        }
        siguienteTic = Time.time + 0.25f;
        foreach (Infectado inf in Infectado.Todos.ToArray())
        {
            if (inf == null || inf.Muerto) { continue; }
            Vector3 d = inf.transform.position - transform.position; d.y = 0f;
            if (d.magnitude <= radio + inf.RadioExtra && Mathf.Abs(inf.transform.position.y - transform.position.y) < 2f)
            {
                inf.Quemar(4f);
            }
        }
        Jugador j = Jugador.I;
        if (j != null && !j.Muerto)
        {
            Vector3 d = j.transform.position - transform.position; d.y = 0f;
            if (d.magnitude < radio * 0.85f && Mathf.Abs(j.transform.position.y - transform.position.y) < 2f)
            {
                j.RecibirDano(7, transform.position, "Te quemaste con tu propio molotov.");
            }
        }
    }
}
