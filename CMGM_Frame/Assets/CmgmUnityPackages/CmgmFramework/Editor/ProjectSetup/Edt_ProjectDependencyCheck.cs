/// <summary>
/// 项目初始化：检查并安装 framework_dependencies.manifest 中的框架依赖。
/// 实现见 <see cref="Edt_GettingStartedProbe"/>（零依赖程序集，框架未编过时可独立运行）。
/// </summary>
public static class Edt_ProjectDependencyCheck
{
    public static Edt_GettingStartedProbe.Report EnsureDependencies(bool offerInstall) =>
        Edt_GettingStartedProbe.EnsureDependencies(offerInstall);
}
