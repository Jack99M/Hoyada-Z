using UnityEngine;

/// <summary>
/// Efectos de particulas creados por codigo (sin assets): sangre, vidrio roto y polvo.
/// Usa un sistema de particulas compartido por tipo para no crear basura.
/// </summary>
public static class EfectosFX
{
    private static ParticleSystem sangre;
    private static ParticleSystem vidrio;
    private static ParticleSystem polvo;
    private static Texture2D circulo;

    /// <summary>Textura de circulo suave (para que las particulas no se vean cuadradas).</summary>
    public static Texture2D CirculoSuave
    {
        get
        {
            if (circulo != null) { return circulo; }
            const int n = 64;
            circulo = new Texture2D(n, n, TextureFormat.RGBA32, false);
            circulo.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    px[y * n + x] = new Color(1f, 1f, 1f, a * a);
                }
            }
            circulo.SetPixels(px);
            circulo.Apply();
            return circulo;
        }
    }

    public static void Sangre(Vector3 pos, Vector3 dir)
    {
        if (sangre == null) { sangre = Crear("FX_Sangre", new Color(0.35f, 0.02f, 0.02f), 0.06f, 0.14f, 3.5f, 0.9f); }
        Emitir(sangre, pos, dir, 22);
    }

    public static void Vidrio(Vector3 pos)
    {
        if (vidrio == null) { vidrio = Crear("FX_Vidrio", new Color(0.7f, 0.85f, 0.8f, 0.9f), 0.02f, 0.05f, 4f, 1.2f); }
        Emitir(vidrio, pos, Vector3.up, 30);
    }

    public static void Polvo(Vector3 pos)
    {
        if (polvo == null) { polvo = Crear("FX_Polvo", new Color(0.5f, 0.47f, 0.42f, 0.5f), 0.2f, 0.5f, 1.2f, 0.1f); }
        Emitir(polvo, pos, Vector3.up, 14);
    }

    private static void Emitir(ParticleSystem ps, Vector3 pos, Vector3 dir, int cantidad)
    {
        ps.transform.position = pos;
        ps.transform.rotation = dir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(dir) : Quaternion.identity;
        ps.Emit(cantidad);
    }

    private static ParticleSystem Crear(string nombre, Color color, float tamMin, float tamMax, float velocidad, float gravedad)
    {
        var go = new GameObject(nombre);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(velocidad * 0.3f, velocidad);
        main.startSize = new ParticleSystem.MinMaxCurve(tamMin, tamMax);
        main.startColor = color;
        main.gravityModifier = gravedad;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;

        var em = ps.emission;
        em.enabled = false;

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 35f;
        sh.radius = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                  new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var r = go.GetComponent<ParticleSystemRenderer>();
        Shader sh2 = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh2 == null) { sh2 = Shader.Find("Sprites/Default"); }
        var mat = new Material(sh2);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BaseMap", CirculoSuave);
        mat.mainTexture = CirculoSuave;
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        r.sharedMaterial = mat;
        return ps;
    }
}
