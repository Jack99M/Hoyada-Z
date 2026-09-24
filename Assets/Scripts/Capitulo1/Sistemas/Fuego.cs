using UnityEngine;

/// <summary>
/// Fuego con llamas, humo y luz parpadeante, creado por codigo (autos quemandose,
/// barricadas, basureros en llamas).
/// </summary>
public class Fuego : MonoBehaviour
{
    public float tamano = 1f;
    public bool conHumo = true;
    public bool conLuz = true;

    private void Start()
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        CrearSistema("Llamas", sh, new Color(1f, 0.55f, 0.15f, 0.9f), new Color(1f, 0.2f, 0.05f, 0f), 0.6f, 1.2f, 1.5f, 0.35f * tamano, 0.9f * tamano, 40f * tamano, true, -0.2f);
        if (conHumo)
        {
            CrearSistema("Humo", sh, new Color(0.12f, 0.11f, 0.1f, 0.55f), new Color(0.2f, 0.2f, 0.2f, 0f), 2.5f, 4.5f, 1.2f, 0.8f * tamano, 2.5f * tamano, 8f * tamano, false, -0.05f);
        }
        if (conLuz)
        {
            var lg = new GameObject("LuzFuego");
            lg.transform.SetParent(transform, false);
            lg.transform.localPosition = Vector3.up * 1.2f * tamano;
            var l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.55f, 0.2f);
            l.intensity = 6f * tamano;
            l.range = 10f * tamano;
            l.shadows = LightShadows.None;
            lg.AddComponent<LuzParpadeante>().modo = LuzParpadeante.Modo.Fuego;
        }
    }

    private void CrearSistema(string nombre, Shader sh, Color ini, Color fin, float vidaMin, float vidaMax, float vel, float tamMin, float tamMax, float tasa, bool aditivo, float gravedad)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(vidaMin, vidaMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(vel * 0.5f, vel);
        main.startSize = new ParticleSystem.MinMaxCurve(tamMin, tamMax);
        main.startColor = ini;
        main.gravityModifier = gravedad;
        main.maxParticles = 400;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = tasa;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f * tamano;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(ini, 0f), new GradientColorKey(fin, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(ini.a, 0.15f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, aditivo ? AnimationCurve.Linear(0f, 1f, 1f, 0.2f) : AnimationCurve.Linear(0f, 0.5f, 1f, 1.6f));
        var r = go.GetComponent<ParticleSystemRenderer>();
        var m = new Material(sh != null ? sh : Shader.Find("Sprites/Default"));
        m.SetColor("_BaseColor", Color.white);
        m.SetTexture("_BaseMap", EfectosFX.CirculoSuave);
        m.mainTexture = EfectosFX.CirculoSuave;
        if (aditivo && sh != null)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 2f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3100;
        }
        else if (sh != null)
        {
            m.SetFloat("_Surface", 1f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3050;
        }
        r.sharedMaterial = m;
        ps.Play();
    }
}
