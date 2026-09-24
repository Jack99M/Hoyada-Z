using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera un esqueleto humanoide (17 huesos) y pesos de piel para modelos OBJ estaticos
/// en T-pose o A-pose, y guarda un prefab animable con AnimProcedural.
/// Menu: Hoyada Z / Rig / Crear personajes animables.
/// Es una solucion temporal hasta tener los FBX con rig de Mixamo.
/// </summary>
public static class AutoRigHumanoide
{
    private const string CarpetaRig = "Assets/Arte/Modelos/Rig";
    private const string CarpetaPrefabs = "Assets/Arte/Prefabs";

    [MenuItem("Hoyada Z/Rig/Crear personajes animables")]
    public static void CrearTodos()
    {
        string log = CrearPersonajes();
        Debug.Log(log);
    }

    public static string CrearPersonajes()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Crear("Assets/Arte/Modelos/Personajes/char_mateo.obj", "Mateo", "Assets/Arte/Materiales/mat_char_mateo.mat", AnimProcedural.Estilo.Humano, false));
        sb.AppendLine(Crear("Assets/Arte/Modelos/Personajes/char_infectado.obj", "Infectado", "Assets/Arte/Materiales/mat_char_infectado.mat", AnimProcedural.Estilo.Infectado, false));
        sb.AppendLine(Crear("Assets/Arte/Modelos/Personajes/char_bety.obj", "Bety", "Assets/Arte/Materiales/mat_char_bety.mat", AnimProcedural.Estilo.Anciana, true));
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    private struct Seg
    {
        public Vector3 a, b;
    }

    public static string Crear(string rutaObj, string nombre, string rutaMaterial, AnimProcedural.Estilo estilo, bool falda)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(rutaObj);
        if (asset == null)
        {
            return "No existe " + rutaObj;
        }

        MeshFilter mf = asset.GetComponentInChildren<MeshFilter>();
        Mesh src = mf.sharedMesh;
        Matrix4x4 aRaiz = asset.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;

        Vector3[] v = src.vertices;
        Vector3[] n = src.normals;
        for (int i = 0; i < v.Length; i++)
        {
            v[i] = aRaiz.MultiplyPoint3x4(v[i]);
            if (n != null && n.Length == v.Length)
            {
                n[i] = aRaiz.MultiplyVector(n[i]).normalized;
            }
        }

        // ---------- Analisis de la malla ----------
        Bounds b = new Bounds(v[0], Vector3.zero);
        for (int i = 1; i < v.Length; i++) { b.Encapsulate(v[i]); }
        float H = b.size.y;
        float y0 = b.min.y;
        float cx = b.center.x;

        float torsoZ = PromedioZ(v, p => Mathf.Abs(p.x - cx) < 0.1f * H && p.y > y0 + 0.5f * H && p.y < y0 + 0.8f * H, b.center.z);
        float cabezaZ = PromedioZ(v, p => p.y > y0 + 0.9f * H, torsoZ);

        // Piernas
        float[] piernaX = new float[2];
        float[] piernaZ = new float[2];
        float[] rodillaZ = new float[2];
        float[] tobilloZ = new float[2];
        for (int lado = 0; lado < 2; lado++)
        {
            float signo = lado == 1 ? 1f : -1f;
            int cnt = 0; float sx = 0f, sz = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 p = v[i];
                if (p.y < y0 + 0.42f * H && (p.x - cx) * signo > 0.005f * H)
                {
                    sx += p.x; sz += p.z; cnt++;
                }
            }
            piernaX[lado] = cnt > 0 ? sx / cnt : cx + signo * 0.06f * H;
            piernaZ[lado] = cnt > 0 ? sz / cnt : torsoZ;
            float px = piernaX[lado];
            rodillaZ[lado] = PromedioZ(v, p => Mathf.Abs(p.y - (y0 + 0.28f * H)) < 0.03f * H && (p.x - cx) * signo > 0f, piernaZ[lado]);
            tobilloZ[lado] = PromedioZ(v, p => Mathf.Abs(p.y - (y0 + 0.06f * H)) < 0.025f * H && (p.x - cx) * signo > 0f, piernaZ[lado]);
        }

        // Brazos (PCA de los vertices alejados del torso)
        Vector3[] hombro = new Vector3[2];
        Vector3[] codo = new Vector3[2];
        Vector3[] muneca = new Vector3[2];
        Vector3[] punta = new Vector3[2];
        Vector3[] dirBrazo = new Vector3[2];
        for (int lado = 0; lado < 2; lado++)
        {
            float signo = lado == 1 ? 1f : -1f;
            var pts = new List<Vector3>();
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 p = v[i];
                if (p.y > y0 + 0.5f * H && (p.x - cx) * signo > 0.2f * H)
                {
                    pts.Add(p);
                }
            }
            if (pts.Count < 20)
            {
                return nombre + ": no se encontraron brazos (" + pts.Count + " vertices).";
            }

            Vector3 centro = Vector3.zero;
            foreach (var p in pts) { centro += p; }
            centro /= pts.Count;
            Vector3 dir = DireccionPrincipal(pts, centro);
            if (dir.x * signo < 0f) { dir = -dir; }

            float maxProj = float.MinValue;
            foreach (var p in pts) { maxProj = Mathf.Max(maxProj, Vector3.Dot(p - centro, dir)); }
            Vector3 tip = centro + dir * maxProj;

            float xHombro = cx + signo * 0.11f * H;
            float t = Mathf.Abs(dir.x) > 0.05f ? (xHombro - centro.x) / dir.x : -0.2f * H;
            Vector3 sh = centro + dir * t;
            sh.y = Mathf.Min(sh.y, y0 + 0.835f * H);
            sh.z = Mathf.Lerp(sh.z, torsoZ, 0.5f);

            dirBrazo[lado] = (tip - sh).normalized;
            float largo = Vector3.Distance(sh, tip);
            hombro[lado] = sh;
            muneca[lado] = tip - dirBrazo[lado] * Mathf.Min(0.11f * H, largo * 0.25f);
            codo[lado] = Vector3.Lerp(sh, muneca[lado], 0.5f);
            punta[lado] = tip;
        }

        // ---------- Posiciones de los huesos ----------
        var pos = new Vector3[EsqueletoHumanoide.Cantidad];
        var fin = new Vector3[EsqueletoHumanoide.Cantidad];
        pos[EsqueletoHumanoide.Hips] = new Vector3(cx, y0 + 0.53f * H, torsoZ);
        pos[EsqueletoHumanoide.Spine] = new Vector3(cx, y0 + 0.61f * H, torsoZ);
        pos[EsqueletoHumanoide.Chest] = new Vector3(cx, y0 + 0.71f * H, torsoZ);
        pos[EsqueletoHumanoide.Neck] = new Vector3(cx, y0 + 0.835f * H, Mathf.Lerp(torsoZ, cabezaZ, 0.5f));
        pos[EsqueletoHumanoide.Head] = new Vector3(cx, y0 + 0.875f * H, cabezaZ);
        fin[EsqueletoHumanoide.Head] = new Vector3(cx, y0 + H, cabezaZ);

        int[,] brazo = { { EsqueletoHumanoide.UpperArmL, EsqueletoHumanoide.LowerArmL, EsqueletoHumanoide.HandL },
                         { EsqueletoHumanoide.UpperArmR, EsqueletoHumanoide.LowerArmR, EsqueletoHumanoide.HandR } };
        int[,] pierna = { { EsqueletoHumanoide.UpperLegL, EsqueletoHumanoide.LowerLegL, EsqueletoHumanoide.FootL },
                          { EsqueletoHumanoide.UpperLegR, EsqueletoHumanoide.LowerLegR, EsqueletoHumanoide.FootR } };
        for (int lado = 0; lado < 2; lado++)
        {
            pos[brazo[lado, 0]] = hombro[lado];
            pos[brazo[lado, 1]] = codo[lado];
            pos[brazo[lado, 2]] = muneca[lado];
            fin[brazo[lado, 2]] = punta[lado];

            pos[pierna[lado, 0]] = new Vector3(piernaX[lado], y0 + 0.50f * H, piernaZ[lado]);
            pos[pierna[lado, 1]] = new Vector3(piernaX[lado], y0 + 0.28f * H, rodillaZ[lado]);
            pos[pierna[lado, 2]] = new Vector3(piernaX[lado], y0 + 0.055f * H, tobilloZ[lado]);
            fin[pierna[lado, 2]] = new Vector3(piernaX[lado], y0, tobilloZ[lado] + 0.1f * H);
        }

        // Extremo de cada hueso = posicion del primer hijo (o 'fin' para las hojas)
        for (int i = 0; i < EsqueletoHumanoide.Cantidad; i++)
        {
            if (fin[i] != Vector3.zero)
            {
                continue;
            }
            for (int j = 0; j < EsqueletoHumanoide.Cantidad; j++)
            {
                if (EsqueletoHumanoide.Padres[j] == i)
                {
                    fin[i] = pos[j];
                    break;
                }
            }
        }
        fin[EsqueletoHumanoide.Hips] = pos[EsqueletoHumanoide.Spine];
        fin[EsqueletoHumanoide.Chest] = pos[EsqueletoHumanoide.Neck];

        var seg = new Seg[EsqueletoHumanoide.Cantidad];
        for (int i = 0; i < seg.Length; i++) { seg[i] = new Seg { a = pos[i], b = fin[i] }; }

        // ---------- Pesos ----------
        var pesos = new BoneWeight[v.Length];
        int[] torso = { EsqueletoHumanoide.Hips, EsqueletoHumanoide.Spine, EsqueletoHumanoide.Chest, EsqueletoHumanoide.Neck, EsqueletoHumanoide.Head,
                        EsqueletoHumanoide.UpperArmL, EsqueletoHumanoide.UpperArmR, EsqueletoHumanoide.UpperLegL, EsqueletoHumanoide.UpperLegR };
        float yCadera = y0 + 0.50f * H;
        var conteo = new int[EsqueletoHumanoide.Cantidad];

        for (int i = 0; i < v.Length; i++)
        {
            Vector3 p = v[i];
            int lado = p.x > cx ? 1 : 0;
            List<int> cand = new List<int>(6);

            float proj = Vector3.Dot(p - hombro[lado], dirBrazo[lado]);
            Vector3 enLinea = hombro[lado] + dirBrazo[lado] * Mathf.Max(0f, proj);
            float lateral = Vector3.Distance(p, enLinea);
            bool esBrazo = proj > -0.02f * H && lateral < 0.1f * H && Mathf.Abs(p.x - cx) > 0.1f * H && p.y > y0 + 0.45f * H;

            if (esBrazo)
            {
                cand.Add(brazo[lado, 0]); cand.Add(brazo[lado, 1]); cand.Add(brazo[lado, 2]); cand.Add(EsqueletoHumanoide.Chest);
            }
            else if (p.y < yCadera - 0.02f * H)
            {
                if (falda && p.y > y0 + 0.14f * H)
                {
                    cand.Add(EsqueletoHumanoide.Hips);
                }
                else
                {
                    cand.Add(pierna[lado, 0]); cand.Add(pierna[lado, 1]); cand.Add(pierna[lado, 2]);
                    if (p.y > yCadera - 0.08f * H) { cand.Add(EsqueletoHumanoide.Hips); }
                }
            }
            else if (p.y > y0 + 0.83f * H && Mathf.Abs(p.x - cx) < 0.12f * H)
            {
                cand.Add(EsqueletoHumanoide.Neck); cand.Add(EsqueletoHumanoide.Head); cand.Add(EsqueletoHumanoide.Chest);
            }
            else
            {
                cand.AddRange(torso);
                if (falda) { cand.Remove(EsqueletoHumanoide.UpperLegL); cand.Remove(EsqueletoHumanoide.UpperLegR); }
            }

            int b1 = -1, b2 = -1; float d1 = float.MaxValue, d2 = float.MaxValue;
            foreach (int k in cand)
            {
                float d = DistanciaSegmento(p, seg[k].a, seg[k].b);
                if (d < d1) { d2 = d1; b2 = b1; d1 = d; b1 = k; }
                else if (d < d2) { d2 = d; b2 = k; }
            }

            float w1 = 1f / (Mathf.Pow(d1, 4f) + 1e-9f);
            float w2 = b2 >= 0 ? 1f / (Mathf.Pow(d2, 4f) + 1e-9f) : 0f;
            float suma = w1 + w2;
            w1 /= suma; w2 /= suma;
            if (w2 < 0.05f) { w1 = 1f; w2 = 0f; }

            pesos[i] = new BoneWeight { boneIndex0 = b1, weight0 = w1, boneIndex1 = Mathf.Max(0, b2), weight1 = w2 };
            conteo[b1]++;
        }

        // ---------- Jerarquia ----------
        var raiz = new GameObject(nombre);
        var visual = new GameObject("Visual");
        visual.transform.SetParent(raiz.transform, false);
        var mallaGo = new GameObject("Malla");
        mallaGo.transform.SetParent(visual.transform, false);

        var huesos = new Transform[EsqueletoHumanoide.Cantidad];
        for (int i = 0; i < huesos.Length; i++)
        {
            huesos[i] = new GameObject(EsqueletoHumanoide.Nombres[i]).transform;
        }
        for (int i = 0; i < huesos.Length; i++)
        {
            int padre = EsqueletoHumanoide.Padres[i];
            huesos[i].SetParent(padre < 0 ? visual.transform : huesos[padre], false);
        }
        for (int i = 0; i < huesos.Length; i++)
        {
            huesos[i].position = pos[i];
            huesos[i].rotation = Quaternion.identity;
        }

        var puntoArma = new GameObject("PuntoArma").transform;
        puntoArma.SetParent(huesos[EsqueletoHumanoide.HandR], false);
        puntoArma.position = Vector3.Lerp(muneca[1], punta[1], 0.45f);
        puntoArma.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        // ---------- Malla con pesos ----------
        var malla = new Mesh();
        malla.name = nombre + "_rig";
        malla.indexFormat = v.Length > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        malla.vertices = v;
        if (n != null && n.Length == v.Length) { malla.normals = n; }
        malla.uv = src.uv;
        malla.subMeshCount = src.subMeshCount;
        for (int s = 0; s < src.subMeshCount; s++) { malla.SetTriangles(src.GetTriangles(s), s); }
        malla.boneWeights = pesos;
        var bind = new Matrix4x4[huesos.Length];
        for (int i = 0; i < huesos.Length; i++) { bind[i] = huesos[i].worldToLocalMatrix * mallaGo.transform.localToWorldMatrix; }
        malla.bindposes = bind;
        malla.RecalculateBounds();
        if (n == null || n.Length != v.Length) { malla.RecalculateNormals(); }

        System.IO.Directory.CreateDirectory(CarpetaRig);
        System.IO.Directory.CreateDirectory(CarpetaPrefabs);
        string rutaMalla = CarpetaRig + "/" + nombre + "_rig.asset";
        AssetDatabase.DeleteAsset(rutaMalla);
        AssetDatabase.CreateAsset(malla, rutaMalla);

        var smr = mallaGo.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = malla;
        smr.bones = huesos;
        smr.rootBone = huesos[EsqueletoHumanoide.Hips];
        smr.quality = SkinQuality.Bone2;
        smr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(rutaMaterial);
        Bounds lb = malla.bounds;
        lb.center = huesos[EsqueletoHumanoide.Hips].InverseTransformPoint(lb.center);
        lb.Expand(0.8f);
        smr.localBounds = lb;

        var esq = raiz.AddComponent<EsqueletoHumanoide>();
        esq.huesos = huesos;
        esq.puntoArma = puntoArma;
        esq.visual = visual.transform;
        esq.altura = H;

        var anim = raiz.AddComponent<AnimProcedural>();
        anim.esqueleto = esq;
        anim.estilo = estilo;

        string rutaPrefab = CarpetaPrefabs + "/Rig_" + nombre + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(raiz, rutaPrefab);
        Object.DestroyImmediate(raiz);

        var sb = new StringBuilder();
        sb.Append(nombre + ": H=" + H.ToString("F2") + " verts=" + v.Length + " hombroR=" + hombro[1].ToString("F2") + " dirBrazoR=" + dirBrazo[1].ToString("F2") + " puntaR=" + punta[1].ToString("F2"));
        sb.Append(" | vertices por hueso: ");
        for (int i = 0; i < conteo.Length; i++) { sb.Append(EsqueletoHumanoide.Nombres[i] + "=" + conteo[i] + " "); }
        return sb.ToString();
    }

    private static float PromedioZ(Vector3[] v, System.Func<Vector3, bool> filtro, float porDefecto)
    {
        float s = 0f; int c = 0;
        for (int i = 0; i < v.Length; i++)
        {
            if (filtro(v[i])) { s += v[i].z; c++; }
        }
        return c > 0 ? s / c : porDefecto;
    }

    private static Vector3 DireccionPrincipal(List<Vector3> pts, Vector3 centro)
    {
        float xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;
        foreach (var q in pts)
        {
            Vector3 d = q - centro;
            xx += d.x * d.x; xy += d.x * d.y; xz += d.x * d.z;
            yy += d.y * d.y; yz += d.y * d.z; zz += d.z * d.z;
        }
        Vector3 vec = new Vector3(1f, 0.3f, 0.1f);
        for (int it = 0; it < 30; it++)
        {
            Vector3 nv = new Vector3(xx * vec.x + xy * vec.y + xz * vec.z,
                                     xy * vec.x + yy * vec.y + yz * vec.z,
                                     xz * vec.x + yz * vec.y + zz * vec.z);
            if (nv.sqrMagnitude < 1e-12f) { break; }
            vec = nv.normalized;
        }
        return vec;
    }

    private static float DistanciaSegmento(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float l2 = ab.sqrMagnitude;
        if (l2 < 1e-8f) { return Vector3.Distance(p, a); }
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / l2);
        return Vector3.Distance(p, a + ab * t);
    }
}
