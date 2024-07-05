using System.IO;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class GenTagEnum : MonoBehaviour
{
    [MenuItem("草木句萌/GenTagEnum")]
    public static void voidGenTagEnum()
    {
        var tags = InternalEditorUtility.tags;
        var arg = "";
        foreach (var tag in tags)
        {
            arg += "\t" + tag + ",\n";
        }
        var res = "public enum E_Tag\n{\n" + arg + "}\n";
        var path = Consts.Paths.ScriptsPath + "/Utils/TagManager/EnumTag.cs";
        File.WriteAllText(path, res, Encoding.UTF8);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

