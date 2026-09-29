using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Prefabs reutilizables del juego.
/// Convierte los objetos que se repiten en las escenas (infectados, recogibles, documentos y coleccionables)
/// en instancias de Prefabs guardados en Assets/Prefabs. Si se modifica un Prefab, se actualizan todas sus copias;
/// lo propio de cada copia (posición, textos, referencias a la escena) queda guardado como override.
/// Menú: Hoyada Z/Prefabs
/// </summary>
public static class PrefabsHoyada
{
    private const string Raiz = "Assets/Prefabs";

    [MenuItem("Hoyada Z/Prefabs/Convertir escena abierta")]
    private static void MenuEscena() { Debug.Log(ConvertirEscena()); }

    [MenuItem("Hoyada Z/Prefabs/Convertir Capítulo 1 y 2")]
    private static void MenuEscenas() { Debug.Log(ConvertirEscenas()); }

    /// <summary>Convierte los objetos repetidos de las escenas abiertas en instancias de Prefab.</summary>
    public static string ConvertirEscena()
    {
        int nuevos = 0, vinculados = 0, omitidos = 0, fallos = 0;
        void Contar(int r)
        {
            if (r == 1) { nuevos++; }
            else if (r == 2) { vinculados++; }
            else if (r == 0) { omitidos++; }
            else { fallos++; }
        }

        foreach (var inf in Todos<Infectado>())
        {
            var smr = inf.GetComponentInChildren<SkinnedMeshRenderer>(true);
            string mat = smr != null && smr.sharedMaterial != null ? smr.sharedMaterial.name : "";
            mat = mat.Replace("mat_cuerpo_", "").Replace("mat_char_", "").Replace("mat_", "");
            string tipo = inf.tipo.ToString();
            bool mismo = mat.Length == 0 || mat == "infectado" || string.Equals(mat, tipo, System.StringComparison.OrdinalIgnoreCase);
            Contar(Vincular(inf.gameObject, "Infectados", "Infectado_" + tipo + (mismo ? "" : "_" + Titulo(mat))));
        }
        foreach (var r in Todos<Recogible>()) { Contar(Vincular(r.gameObject, "Recogibles", "Recogible_" + r.tipo)); }
        foreach (var d in Todos<Documento>()) { Contar(Vincular(d.gameObject, "Interactivos", "Documento")); }
        foreach (var c in Todos<Coleccionable>()) { Contar(Vincular(c.gameObject, "Interactivos", "Coleccionable")); }

        if (nuevos + vinculados > 0) { EditorSceneManager.MarkAllScenesDirty(); }
        AssetDatabase.SaveAssets();
        return "Prefabs: " + nuevos + " creados, " + vinculados + " copias vinculadas"
            + (omitidos > 0 ? ", " + omitidos + " ya eran prefab" : "")
            + (fallos > 0 ? ", " + fallos + " sin convertir" : "");
    }

    /// <summary>Abre el Capítulo 1 y 2, convierte y guarda.</summary>
    public static string ConvertirEscenas()
    {
        string previa = EditorSceneManager.GetActiveScene().path;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) { return "Cancelado."; }
        var sb = new StringBuilder();
        foreach (var ruta in new[] { "Assets/Scenes/Capitulo1_Resaca.unity", "Assets/Scenes/Capitulo2_CorteDePaso.unity" })
        {
            if (!File.Exists(ruta)) { continue; }
            var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            sb.AppendLine(Path.GetFileNameWithoutExtension(ruta) + ": " + ConvertirEscena());
            EditorSceneManager.SaveScene(escena);
        }
        if (!string.IsNullOrEmpty(previa)) { EditorSceneManager.OpenScene(previa, OpenSceneMode.Single); }
        return sb.ToString();
    }

    /// <summary>
    /// 1 = creó el Prefab con este objeto, 2 = lo vinculó a un Prefab existente, 0 = ya era prefab, -1 = no se pudo.
    /// Si un objeto tiene otra estructura que el Prefab existente, se crea una variante (_B, _C...).
    /// </summary>
    private static int Vincular(GameObject go, string carpeta, string nombreBase)
    {
        if (go == null || PrefabUtility.IsPartOfAnyPrefab(go)) { return 0; }
        NombresUnicos(go.transform);
        string firma = Firma(go.transform);
        for (int i = 0; i < 26; i++)
        {
            string nombre = nombreBase + (i == 0 ? "" : "_" + (char)('A' + i));
            string ruta = Raiz + "/" + carpeta + "/" + nombre + ".prefab";
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            if (pf == null)
            {
                Carpetas(carpeta);
                string nombreOriginal = go.name;
                var creado = PrefabUtility.SaveAsPrefabAssetAndConnect(go, ruta, InteractionMode.AutomatedAction);
                go.name = nombreOriginal;
                return creado != null ? 1 : -1;
            }
            if (Firma(pf.transform) != firma) { continue; }
            var ajustes = new ConvertToPrefabInstanceSettings
            {
                componentsNotMatchedBecomesOverride = true,
                gameObjectsNotMatchedBecomesOverride = true,
                recordPropertyOverridesOfMatches = true,
                changeRootNameToAssetName = false,
                logInfo = false
            };
            PrefabUtility.ConvertToPrefabInstance(go, pf, ajustes, InteractionMode.AutomatedAction);
            return 2;
        }
        return -1;
    }

    // Unity empareja los hijos por nombre: dos hermanos con el mismo nombre ("Visual", "Visual") no se pueden emparejar.
    // Se renombran a Visual, Visual_2... (ningún script los busca por nombre).
    private static void NombresUnicos(Transform raiz)
    {
        foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
        {
            var vistos = new Dictionary<string, int>();
            foreach (Transform h in t)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(h.gameObject) && !PrefabUtility.IsAddedGameObjectOverride(h.gameObject)) { continue; }
                if (vistos.TryGetValue(h.name, out int n)) { vistos[h.name] = n + 1; h.name = h.name + "_" + (n + 1); }
                else { vistos[h.name] = 1; }
            }
        }
    }

    // Estructura del objeto (rutas de todos sus hijos): solo se vinculan copias con la misma forma.
    private static string Firma(Transform raiz)
    {
        var rutas = new List<string>();
        foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (t == raiz) { continue; }
            string p = t.name;
            for (var x = t.parent; x != raiz; x = x.parent) { p = x.name + "/" + p; }
            rutas.Add(p);
        }
        rutas.Sort(System.StringComparer.Ordinal);
        return string.Join("|", rutas);
    }

    private static T[] Todos<T>() where T : Object
    {
        return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
    }

    private static void Carpetas(string carpeta)
    {
        if (!AssetDatabase.IsValidFolder(Raiz)) { AssetDatabase.CreateFolder("Assets", "Prefabs"); }
        if (!AssetDatabase.IsValidFolder(Raiz + "/" + carpeta)) { AssetDatabase.CreateFolder(Raiz, carpeta); }
    }

    private static string Titulo(string s)
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
