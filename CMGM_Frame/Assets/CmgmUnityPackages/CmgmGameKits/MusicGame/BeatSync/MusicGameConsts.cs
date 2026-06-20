using CMGM.Core;

/// <summary>
/// MusicGame 音游工具包的路径常量。
/// 原为 <c>CMGM.Core.Consts.Paths.RhythmMap_Path</c>，编译边界2.7b 迁出 Core（框架 Core 不再持音游路径）。
/// 仍基于 Core 的工作区根派生（<c>Consts.Paths.HotRes</c>）。
/// </summary>
public static class MusicGameConsts
{
    /// <summary>节拍映射数据目录（HotRes/RhythmMap）。</summary>
    public static string RhythmMap_Path => Consts.Paths.HotRes + "/RhythmMap";
}
