using CMGM.Core;

/// <summary>
/// CmgmFramework/Editor 内路径常量。
/// </summary>
public static class Edt_CmgmEditorPaths
{
    public static string Root => Consts.Paths.Framework.Editor;
    public static string ProjectSetup => Root + "/ProjectSetup";
    public static string ProjectLayerManifest => ProjectSetup + "/Manifests/project_layer.manifest";
    public static string FrameworkLayoutManifest => ProjectSetup + "/Manifests/framework_path_check.manifest";
    public static string FrameworkDependenciesManifest => ProjectSetup + "/Manifests/framework_dependencies.manifest";
    public static string ProjectLayerSeeds => ProjectSetup + "/Seeds";
    public static string AssetTemplates => Root + "/AssetTemplates/Templates";
}
