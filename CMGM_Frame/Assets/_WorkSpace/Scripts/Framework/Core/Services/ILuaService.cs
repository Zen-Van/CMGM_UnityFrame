using Cysharp.Threading.Tasks;

namespace CMGM.Core
{
    /// <summary>
    /// Lua 服务契约。被 asmdef 封装的 Module 通过本接口调用 Lua，
    /// 不直接依赖 XLua 与 Assembly-CSharp 中的具体实现（依赖倒置，见 ARCHITECTURE §3.4 / §7.5）。
    /// <para>注意：Core 不引用 XLua，故接口不暴露 LuaEnv 等 XLua 类型。</para>
    /// <para>当前实现为 <c>LuaManager</c>（位于 Framework/Integrations/Lua，Assembly-CSharp）；
    /// 正式的注册 / 注入将在主线「启动组合根3.1」随 IGameModule 体系接入。</para>
    /// </summary>
    public interface ILuaService
    {
        /// <summary>Lua 环境是否已初始化完成。</summary>
        bool IsInited { get; }

        /// <summary>创建 LuaEnv、装载 Loader 链并执行根脚本。</summary>
        void Init();

        /// <summary>执行一个 Lua 文件（相对 Lua 根路径，需带完整后缀）。</summary>
        UniTask ExecuteLua(string uri);

        /// <summary>驱动 Lua GC，可定期调用。</summary>
        void Tick();

        /// <summary>释放 LuaEnv 与缓存。</summary>
        void Clear();
    }
}
