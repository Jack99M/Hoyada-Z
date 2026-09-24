using UnityEngine;

/// <summary>
/// Animacion procedural para personajes con EsqueletoHumanoide (sin clips de animacion).
/// Genera en tiempo real: respirar, caminar, correr, agacharse, golpear, empujar, curarse,
/// ataque de infectado, aturdimiento y muerte. Los controladores solo llenan las entradas
/// (velocidad, agachado, alerta) y llaman a las acciones (Golpear, Morir, etc.).
/// Cuando el equipo entregue personajes con rig de Mixamo, se reemplaza por un Animator.
/// </summary>
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public class AnimProcedural : MonoBehaviour
{
    public enum Estilo { Humano, Infectado, Anciana }

    public EsqueletoHumanoide esqueleto;
    public Estilo estilo = Estilo.Humano;

    [Header("Entradas (las llenan los controladores)")]
    public float velocidad;
    public bool agachado;
    [Tooltip("Humano: guardia de combate. Infectado: brazos estirados persiguiendo.")]
    public bool alerta;

    [Header("Parametros")]
    public float velocidadCaminar = 1.8f;
    public float velocidadCorrer = 6f;
    [Tooltip("Grados bajo la horizontal de los brazos en reposo.")]
    public float anguloBrazos = 76f;
    [Tooltip("Cojera del infectado (0 = ninguna).")]
    [Range(0f, 1f)] public float cojera = 0.35f;

    private const int L = 0, R = 1;

    private readonly Quaternion[] brazoBajo = new Quaternion[2];
    private readonly Vector3[] ejeCodo = new Vector3[2];
    private Vector3 ejeAdelante;   // positivo: extremidad que cuelga va hacia adelante
    private Vector3 ejeRodilla;    // positivo: pie va hacia atras
    private Vector3 ejeInclinar;   // positivo: torso se inclina hacia adelante
    private Vector3 ejeCaer;       // positivo: cuerpo cae de espaldas
    private Vector3 hipsRest;
    private Quaternion visualRest;
    private Vector3 visualPosRest;
    private float escala = 1f;
    private bool listo;

    private float fase;
    private float velSuave;
    private float agache;
    private float alertaSuave;
    private float semilla;

    private float golpeT = -1f, golpeDur = 0.45f;
    private float empujonT = -1f;
    private float curarT = -1f, curarDur = 1.4f;
    private float ataqueT = -1f;
    private float aturdidoT = -1f;
    private float sigiloT = -1f;
    private float muerteT = -1f;

    public bool Muerto { get { return muerteT >= 0f; } }
    public bool Ocupado { get { return golpeT >= 0f || empujonT >= 0f || sigiloT >= 0f || curarT >= 0f; } }

    private void Awake()
    {
        if (esqueleto == null)
        {
            esqueleto = GetComponentInChildren<EsqueletoHumanoide>();
        }
        Preparar();
    }

    /// <summary>Calcula ejes y rotaciones base a partir de la pose de reposo del rig.</summary>
    public void Preparar()
    {
        if (esqueleto == null || esqueleto.huesos == null || esqueleto.huesos.Length < EsqueletoHumanoide.Cantidad)
        {
            return;
        }

        escala = esqueleto.altura / 1.75f;
        ejeAdelante = Eje(Vector3.down, Vector3.forward);
        ejeRodilla = Eje(Vector3.down, Vector3.back);
        ejeInclinar = Eje(Vector3.up, Vector3.forward);
        ejeCaer = Eje(Vector3.up, Vector3.back);

        PrepararBrazo(L, EsqueletoHumanoide.UpperArmL, EsqueletoHumanoide.LowerArmL, EsqueletoHumanoide.HandL);
        PrepararBrazo(R, EsqueletoHumanoide.UpperArmR, EsqueletoHumanoide.LowerArmR, EsqueletoHumanoide.HandR);

        hipsRest = esqueleto[EsqueletoHumanoide.Hips].localPosition;
        if (esqueleto.visual != null)
        {
            visualRest = esqueleto.visual.localRotation;
            visualPosRest = esqueleto.visual.localPosition;
        }
        semilla = Random.value * 100f;
        listo = true;
    }

    private void PrepararBrazo(int lado, int hombro, int codo, int mano)
    {
        Vector3 dir = esqueleto[codo].localPosition.normalized;
        Vector3 hor = new Vector3(dir.x, 0f, dir.z);
        if (hor.sqrMagnitude < 0.0001f)
        {
            hor = lado == R ? Vector3.right : Vector3.left;
        }
        hor.Normalize();
        float a = anguloBrazos * Mathf.Deg2Rad;
        Vector3 objetivo = (hor * Mathf.Cos(a) + Vector3.down * Mathf.Sin(a) + Vector3.forward * 0.06f).normalized;
        brazoBajo[lado] = Quaternion.FromToRotation(dir, objetivo);

        Vector3 antebrazo = esqueleto[mano].localPosition.normalized;
        Vector3 adelanteLocal = Quaternion.Inverse(brazoBajo[lado]) * Vector3.forward;
        ejeCodo[lado] = Eje(antebrazo, adelanteLocal);
    }

    /// <summary>Eje tal que un angulo positivo gira 'desde' hacia 'hacia'.</summary>
    private static Vector3 Eje(Vector3 desde, Vector3 hacia)
    {
        Vector3 ax = Vector3.Cross(desde, hacia);
        if (ax.sqrMagnitude < 0.000001f)
        {
            ax = Vector3.right;
        }
        ax.Normalize();
        if (Vector3.Dot(Quaternion.AngleAxis(10f, ax) * desde, hacia) < Vector3.Dot(desde, hacia))
        {
            ax = -ax;
        }
        return ax;
    }

    // ---------- Acciones ----------

    public void Golpear(float duracion) { if (!Muerto) { golpeT = 0f; golpeDur = Mathf.Max(0.2f, duracion); } }
    public void Empujar() { if (!Muerto) { empujonT = 0f; } }
    public void Curar(float duracion) { if (!Muerto) { curarT = 0f; curarDur = duracion; } }
    public void AtacarInfectado() { if (!Muerto) { ataqueT = 0f; } }
    public void Aturdir() { if (!Muerto) { aturdidoT = 0f; } }
    public void Sigilo() { if (!Muerto) { sigiloT = 0f; } }
    public void CancelarAcciones() { golpeT = empujonT = curarT = ataqueT = sigiloT = -1f; }

    public void Morir()
    {
        if (!Muerto)
        {
            CancelarAcciones();
            muerteT = 0f;
        }
    }

    public void Revivir()
    {
        muerteT = -1f;
        aturdidoT = -1f;
        CancelarAcciones();
        if (esqueleto != null && esqueleto.visual != null)
        {
            esqueleto.visual.localRotation = visualRest;
            esqueleto.visual.localPosition = visualPosRest;
        }
    }

    private static float Avanzar(ref float t, float dur)
    {
        if (t < 0f)
        {
            return -1f;
        }
        t += Time.deltaTime / dur;
        if (t >= 1f)
        {
            t = -1f;
        }
        return t;
    }

    private static float Suave(float a, float b, float t)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(a, b, t));
    }

    // ---------- Pose ----------

    private void LateUpdate()
    {
        if (!listo || esqueleto == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        float tg = Avanzar(ref golpeT, golpeDur);
        float te = Avanzar(ref empujonT, 0.45f);
        float tc = Avanzar(ref curarT, curarDur);
        float ta = Avanzar(ref ataqueT, 0.9f);
        float tu = Avanzar(ref aturdidoT, 0.55f);
        float ts = Avanzar(ref sigiloT, 1.3f);
        if (muerteT >= 0f)
        {
            muerteT = Mathf.Min(1f, muerteT + dt / 1.1f);
        }

        bool inf = estilo == Estilo.Infectado;
        bool vieja = estilo == Estilo.Anciana;
        float tiempo = Time.time + semilla;

        velSuave = Mathf.Lerp(velSuave, Muerto ? 0f : velocidad, 1f - Mathf.Exp(-10f * dt));
        agache = Mathf.MoveTowards(agache, agachado && !Muerto ? 1f : 0f, dt * 4f);
        alertaSuave = Mathf.MoveTowards(alertaSuave, alerta ? 1f : 0f, dt * 3f);

        float v = velSuave;
        float andar = Mathf.Clamp01(v / velocidadCaminar);
        float correr = Mathf.Clamp01((v - velocidadCaminar * 1.2f) / Mathf.Max(0.1f, velocidadCorrer - velocidadCaminar * 1.2f));
        float zancada = Mathf.Lerp(1.15f, 2.5f, correr) * escala * (vieja ? 0.7f : 1f);
        if (v > 0.05f)
        {
            fase += 2f * Mathf.PI * v / zancada * dt;
        }

        float s = Mathf.Sin(fase);
        float c = Mathf.Cos(fase);
        float sR = inf ? Mathf.Sin(fase + Mathf.PI) * (1f - cojera) : Mathf.Sin(fase + Mathf.PI);

        float ampMuslo = andar * Mathf.Lerp(24f, 44f, correr) * Mathf.Lerp(1f, 0.55f, agache) * (vieja ? 0.6f : 1f);
        float ampRodilla = andar * Mathf.Lerp(35f, 90f, correr);
        float ampBrazo = andar * Mathf.Lerp(18f, 48f, correr) * (inf ? 0.4f : 1f);
        float codoBase = 10f + andar * Mathf.Lerp(10f, 80f, correr) + (vieja ? 55f : 0f);
        float inclinar = andar * Mathf.Lerp(3f, 14f, correr) + agache * 24f + (vieja ? 12f : 0f) + (inf ? 10f + alertaSuave * 12f : 0f);
        float rebote = andar * Mathf.Lerp(0.025f, 0.06f, correr) * Mathf.Abs(s) * escala;
        float respira = Mathf.Sin(tiempo * (inf ? 2.6f : 1.5f)) * 1.6f;

        // Espasmos del infectado
        float espX = 0f, espY = 0f, espZ = 0f;
        if (inf && !Muerto)
        {
            espX = (Mathf.PerlinNoise(tiempo * 2.3f, 0.3f) - 0.5f) * 30f;
            espY = (Mathf.PerlinNoise(0.7f, tiempo * 2.9f) - 0.5f) * 40f;
            espZ = (Mathf.PerlinNoise(tiempo * 1.7f, 5.1f) - 0.5f) * 30f;
            if (Mathf.PerlinNoise(tiempo * 0.9f, 9f) > 0.72f)
            {
                espY += Mathf.Sin(tiempo * 40f) * 12f;
            }
        }

        // Aturdido: retroceso del torso
        float aturdido = tu >= 0f ? Mathf.Sin(tu * Mathf.PI) : 0f;
        inclinar -= aturdido * 28f;

        // Ataque del infectado: embestida
        float embiste = 0f, brazosAtaque = 0f;
        if (ta >= 0f)
        {
            brazosAtaque = ta < 0.35f ? Suave(0f, 0.35f, ta) : 1f - Suave(0.6f, 1f, ta);
            embiste = ta < 0.35f ? 0f : (ta < 0.55f ? Suave(0.35f, 0.55f, ta) : 1f - Suave(0.55f, 1f, ta));
            inclinar += embiste * 30f;
        }

        // Empujon / curar / sigilo inclinan un poco
        if (te >= 0f) { inclinar += Mathf.Sin(te * Mathf.PI) * 12f; }
        if (ts >= 0f) { inclinar += Mathf.Sin(ts * Mathf.PI) * 18f; }

        // ----- Caderas -----
        Transform hips = esqueleto[EsqueletoHumanoide.Hips];
        float bajar = rebote + agache * 0.36f * escala + (vieja ? 0.03f : 0f);
        hips.localPosition = hipsRest + Vector3.down * bajar;
        hips.localRotation = Quaternion.Euler(0f, s * 6f * andar, s * 3f * andar);

        // ----- Torso -----
        float giroGolpe = 0f;
        if (tg >= 0f)
        {
            giroGolpe = tg < 0.4f ? Suave(0f, 0.4f, tg) * 28f : Mathf.Lerp(28f, -30f, Suave(0.4f, 0.6f, tg)) * (1f - Suave(0.7f, 1f, tg));
        }
        Quaternion inclinQ = Quaternion.AngleAxis(inclinar * 0.5f + respira * 0.3f, ejeInclinar);
        esqueleto[EsqueletoHumanoide.Spine].localRotation = inclinQ * Quaternion.Euler(0f, -s * 4f * andar + giroGolpe * 0.4f + espY * 0.2f, espZ * 0.15f);
        esqueleto[EsqueletoHumanoide.Chest].localRotation = Quaternion.AngleAxis(inclinar * 0.5f + respira * 0.5f, ejeInclinar) * Quaternion.Euler(0f, -s * 6f * andar + giroGolpe * 0.6f, espZ * 0.2f);

        float cabezaAbajo = -inclinar * 0.55f + (inf ? 18f - alertaSuave * 20f : 0f) + (tc >= 0f ? 20f : 0f) + aturdido * -15f;
        esqueleto[EsqueletoHumanoide.Neck].localRotation = Quaternion.AngleAxis(cabezaAbajo * 0.4f, ejeInclinar) * Quaternion.Euler(0f, espY * 0.3f, espZ * 0.3f);
        esqueleto[EsqueletoHumanoide.Head].localRotation = Quaternion.AngleAxis(cabezaAbajo * 0.6f + espX * 0.4f, ejeInclinar) * Quaternion.Euler(0f, espY * 0.5f, espZ * 0.6f + (inf ? 14f : 0f));

        // ----- Piernas -----
        float muslo = -s * ampMuslo; // positivo = adelante (usamos ejeAdelante)
        float musloL = s * ampMuslo;
        float musloR = sR * ampMuslo;
        float rodL = ampRodilla * Mathf.Max(0f, -c) + 6f;
        float rodR = ampRodilla * Mathf.Max(0f, c) * (inf ? 1f - cojera * 0.8f : 1f) + 6f;
        if (Muerto)
        {
            float colapso = Suave(0f, 0.35f, muerteT);
            rodL += colapso * 50f;
            rodR += colapso * 60f;
            musloL += colapso * 35f;
            musloR += colapso * 25f;
        }
        muslo = 0f;
        AplicarPierna(EsqueletoHumanoide.UpperLegL, EsqueletoHumanoide.LowerLegL, EsqueletoHumanoide.FootL, musloL + agache * 70f, rodL + agache * 105f);
        AplicarPierna(EsqueletoHumanoide.UpperLegR, EsqueletoHumanoide.LowerLegR, EsqueletoHumanoide.FootR, musloR + agache * 62f, rodR + agache * 100f);

        // ----- Brazos -----
        // Balanceo al caminar (positivo = adelante), opuesto a la pierna del mismo lado.
        float brazoL = -s * ampBrazo;
        float brazoR = -sR * ampBrazo;
        float codoL = codoBase;
        float codoR = codoBase;

        if (vieja)
        {
            brazoL += 28f; brazoR += 28f;
            codoL = 75f; codoR = 75f;
        }

        if (inf)
        {
            float estirar = alertaSuave * 78f;
            brazoL += estirar + espX * 0.5f;
            brazoR += estirar * 0.9f - espX * 0.4f;
            codoL += 18f + espZ * 0.3f;
            codoR += 25f - espZ * 0.3f;
            if (brazosAtaque > 0f)
            {
                brazoL = Mathf.Lerp(brazoL, 115f, brazosAtaque);
                brazoR = Mathf.Lerp(brazoR, 110f, brazosAtaque);
                codoL = Mathf.Lerp(codoL, 20f, brazosAtaque);
                codoR = Mathf.Lerp(codoR, 20f, brazosAtaque);
            }
        }
        else if (alertaSuave > 0f && !vieja)
        {
            // Guardia: brazos un poco al frente
            brazoL = Mathf.Lerp(brazoL, 35f, alertaSuave);
            codoL = Mathf.Lerp(codoL, 85f, alertaSuave);
            brazoR = Mathf.Lerp(brazoR, 25f, alertaSuave);
            codoR = Mathf.Lerp(codoR, 70f, alertaSuave);
        }

        if (tg >= 0f)
        {
            // Golpe con el arma (brazo derecho): carga arriba, golpe al frente, recupera.
            float arriba = tg < 0.4f ? Mathf.Lerp(brazoR, 165f, Suave(0f, 0.4f, tg))
                : (tg < 0.6f ? Mathf.Lerp(165f, 35f, Suave(0.4f, 0.6f, tg)) : Mathf.Lerp(35f, brazoR, Suave(0.65f, 1f, tg)));
            brazoR = arriba;
            codoR = tg < 0.4f ? Mathf.Lerp(codoR, 75f, Suave(0f, 0.4f, tg)) : Mathf.Lerp(75f, 8f, Suave(0.4f, 0.6f, tg));
            brazoL = Mathf.Lerp(brazoL, 40f, Mathf.Sin(tg * Mathf.PI));
            codoL = Mathf.Lerp(codoL, 80f, Mathf.Sin(tg * Mathf.PI));
        }

        if (te >= 0f)
        {
            float k = te < 0.3f ? Suave(0f, 0.3f, te) : 1f - Suave(0.55f, 1f, te);
            float ext = Suave(0.2f, 0.4f, te);
            brazoL = Mathf.Lerp(brazoL, 85f, k); brazoR = Mathf.Lerp(brazoR, 85f, k);
            codoL = Mathf.Lerp(codoL, Mathf.Lerp(80f, 5f, ext), k); codoR = Mathf.Lerp(codoR, Mathf.Lerp(80f, 5f, ext), k);
        }

        if (tc >= 0f)
        {
            float k = Mathf.Min(Suave(0f, 0.15f, tc), 1f - Suave(0.85f, 1f, tc));
            brazoL = Mathf.Lerp(brazoL, 45f, k); brazoR = Mathf.Lerp(brazoR, 50f, k);
            codoL = Mathf.Lerp(codoL, 110f, k); codoR = Mathf.Lerp(codoR, 100f, k);
        }

        if (ts >= 0f)
        {
            float agarre = Mathf.Min(Suave(0f, 0.15f, ts), 1f - Suave(0.85f, 1f, ts));
            float golpe = Mathf.Abs(Mathf.Sin(ts * Mathf.PI * 3f));
            brazoL = Mathf.Lerp(brazoL, 80f, agarre); codoL = Mathf.Lerp(codoL, 95f, agarre);
            brazoR = Mathf.Lerp(brazoR, Mathf.Lerp(150f, 40f, golpe), agarre); codoR = Mathf.Lerp(codoR, 30f, agarre);
        }

        if (Muerto)
        {
            float k = Suave(0.2f, 0.8f, muerteT);
            brazoL = Mathf.Lerp(brazoL, -20f, k); brazoR = Mathf.Lerp(brazoR, 30f, k);
            codoL = Mathf.Lerp(codoL, 15f, k); codoR = Mathf.Lerp(codoR, 40f, k);
        }

        AplicarBrazo(L, EsqueletoHumanoide.UpperArmL, EsqueletoHumanoide.LowerArmL, brazoL, codoL);
        AplicarBrazo(R, EsqueletoHumanoide.UpperArmR, EsqueletoHumanoide.LowerArmR, brazoR, codoR);

        // ----- Caida al morir -----
        if (esqueleto.visual != null)
        {
            if (Muerto)
            {
                float caida = Suave(0.25f, 0.85f, muerteT);
                float rebotin = muerteT > 0.85f ? Mathf.Sin((muerteT - 0.85f) / 0.15f * Mathf.PI) * 3f : 0f;
                esqueleto.visual.localRotation = visualRest * Quaternion.AngleAxis(caida * 84f - rebotin, ejeCaer);
                esqueleto.visual.localPosition = visualPosRest + Vector3.back * caida * 0.25f * escala;
            }
            else if (embiste > 0f)
            {
                esqueleto.visual.localPosition = visualPosRest + Vector3.forward * embiste * 0.35f;
            }
            else
            {
                esqueleto.visual.localPosition = visualPosRest;
            }
        }
    }

    private void AplicarPierna(int muslo, int rodilla, int pie, float anguloMuslo, float anguloRodilla)
    {
        esqueleto[muslo].localRotation = Quaternion.AngleAxis(anguloMuslo, ejeAdelante);
        esqueleto[rodilla].localRotation = Quaternion.AngleAxis(anguloRodilla, ejeRodilla);
        esqueleto[pie].localRotation = Quaternion.AngleAxis(-(anguloMuslo - anguloRodilla) * 0.5f, ejeAdelante);
    }

    private void AplicarBrazo(int lado, int hombro, int codo, float balanceo, float flexion)
    {
        esqueleto[hombro].localRotation = Quaternion.AngleAxis(balanceo, ejeAdelante) * brazoBajo[lado];
        esqueleto[codo].localRotation = Quaternion.AngleAxis(flexion, ejeCodo[lado]);
    }
}
