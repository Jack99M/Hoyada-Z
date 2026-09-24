using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Utilidad interna de desarrollo: mueve archivos escritos en _Tmp (protegidos con
/// #if HOYADA_TMP) a su ubicacion final dentro de Scripts/Capitulo1, quitando la guarda.
/// </summary>
public static class HerramientaTmp
{
    private static string Base { get { return Path.Combine(Application.dataPath, "Scripts/Capitulo1/"); } }

    /// <summary>tmp = nombre dentro de _Tmp sin extension; destino = ruta relativa a Scripts/Capitulo1.</summary>
    public static string Promover(string tmp, string destino)
    {
        string origen = Base + "_Tmp/" + tmp + ".cs";
        if (!File.Exists(origen)) { return "No existe " + origen; }
        string[] lineas = File.ReadAllLines(origen);
        var sb = new System.Text.StringBuilder();
        int n = 0;
        foreach (string l in lineas)
        {
            string t = l.Trim();
            if (t == "#if HOYADA_TMP" || t == "#endif //HOYADA_TMP") { continue; }
            sb.Append(l).Append('\n');
            n++;
        }
        string dest = Base + destino;
        Directory.CreateDirectory(Path.GetDirectoryName(dest));
        File.WriteAllText(dest, sb.ToString());
        File.Delete(origen);
        if (File.Exists(origen + ".meta")) { File.Delete(origen + ".meta"); }
        return "OK " + destino + " (" + n + " lineas)";
    }
}
