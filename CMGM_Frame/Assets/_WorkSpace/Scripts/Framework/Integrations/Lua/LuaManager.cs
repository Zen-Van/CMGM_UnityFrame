using Cysharp.Threading.Tasks;
using CMGM.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XLua;
using XLua.TemplateEngine;

/// <summary>
/// Lua管理器，Boot 型单例：框架 Boot 中 await InitAsync()。
/// </summary>
public class LuaManager : BootSingleton<LuaManager>, ILuaService
{
    /// <summary> 是否在editor中调试时热加载lua（可以不重启游戏进行编辑）</summary>
    private bool IS_HOT_LUA = Application.isEditor && CmgmFrameSettings.Instance.IS_HOT_LUA;

    #region 关于热重载和非热重载的路径说明
    /*
    ┌─────────────────────────────────────────────────────────┐
    │  热重载（Editor + IS_HOT_LUA）                           │
    │  InitAsync → 跳过 LoadLuaMapper                          │
    │  require / ExecuteLua → Loader 1 / GetLuaContent 读磁盘  │
    └─────────────────────────────────────────────────────────┘

    ┌─────────────────────────────────────────────────────────┐
    │  正式包体（非热重载）                                    │
    │  InitAsync → LoadLuaMapper 预加载全部 Lua                │
    │  require → Loader 3 查 luaMapper                        │
    │  ExecuteLua → GetLuaContent 查 luaMapper                │
    └─────────────────────────────────────────────────────────┘
    */
    #endregion
    
    private string ROOT_FILE_URI = CmgmFrameSettings.Instance.ROOT_LUA_URI;

    private LuaManager() { }
    public LuaEnv LuaEnv { get; private set; }

    public bool IsInited => IsReady;

    /// <summary>
    /// 创建luaEnv，设置luaEnv路径重定向，将所有lua脚本载入内存，执行lua根脚本
    /// </summary>
    protected override async UniTask OnInitAsync()
    {
        LuaEnv = new LuaEnv();

        //重定向指在lua脚本中require时加载的相对路径
        LuaEnv.AddLoader((ref string uri) =>
        {
            //编辑器下查询逻辑，每次require都读磁盘
            string path = Consts.Paths.Lua_Path + "/" + uri;
            if (File.Exists(path))
                return File.ReadAllBytes(path);

            return null;
        });
        LuaEnv.AddLoader((ref string uri) =>
        {
            //Resource下查询逻辑，备用
            TextAsset lua = Resources.Load<TextAsset>(uri);
            if (lua != null)
                return lua.bytes;
            return null;
        });
        LuaEnv.AddLoader((ref string uri) =>
        {
            // Loader 1 失败时的兜底：正式包体下 require 主要走这里（luaMapper 预加载）；
            // 热重载下 require 通常已由 Loader 1 直读磁盘解决
            string key = uri.ToLower();
            if (luaMapper != null && luaMapper.TryGetValue(key, out byte[] content))
                return content;

            return null;
        });

        //Lua管理器初始化的时候，就把所有lua脚本载入内存，并且把main执行了
        // 正式包体：预加载全部 Lua 到 luaMapper；热重载：跳过，运行时直读磁盘
        // 热重载真正读脚本还是靠 Loader 1 / GetLuaContent 读磁盘，不靠luaMapper
        if (!IS_HOT_LUA)
            await LoadLuaMapper();
        //执行lua根文件
        await ExecuteLua(ROOT_FILE_URI);
    }

    /// <summary>
    /// 清空Lua数据和缓存，清空c#和lua互转数据，删除luaEnv
    /// </summary>
    public void Clear()
    {
        ResetBootState();

        //释放lua解释器
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }

        //清空lua mapper
        ClearLuaMapper();
    }
    /// <summary>
    /// 释放lua垃圾，可以定期执行
    /// </summary>
    public void Tick()
    {
        if (LuaEnv == null)
        {
            Debug.Log("解析器未初始化");
            return;
        }
        LuaEnv.Tick();
    }


    #region 所有Lua资产的映射与管理
    private Dictionary<string, byte[]> luaMapper = null;

    // 正式包体专用：从 Addressables 预加载全部 Lua 到 luaMapper
    public async UniTask LoadLuaMapper()
    {
        string[] files = (await AddressablesResMgr.Instance.LoadResourceLocationsAsync("Lua", typeof(TextAsset)))
            .Select(loc => loc.PrimaryKey)
            .Where(key => key.EndsWith(".lua.txt"))
            .ToArray();

        if (files.Length <= 0)
        {
            CmgmLog.fPositive("没有找到任何Lua脚本");
            return;
        }

        luaMapper = new Dictionary<string, byte[]>();
        for (int i = 0; i < files.Length; i++)
        {
            string fileUrl = files[i].ToLower();
            string fileUri = fileUrl.Substring(Consts.Paths.Lua_Path.Length + 1);//此处的+1删除了斜杠
            var asset = await AddressablesResMgr.Instance.LoadAssetAsync<TextAsset>($"Lua/{fileUri}");

            luaMapper[fileUri] = Encoding.UTF8.GetBytes(asset.text);
            CmgmLog.fPositive($"载入了 Lua 脚本【{fileUri}】");
        }
    }
    /// <summary>
    /// 清空LuaMapper
    /// </summary>
    public void ClearLuaMapper()
    {
        if (luaMapper == null) return;

        luaMapper.Clear();
        luaMapper = null;
    }
    /// <summary>
    /// 得到一个lua事件的所有代码
    /// </summary>
    /// <param name="uri">Consts.Paths.Lua_Path路径下的相对路径</param>
    public byte[] GetLuaContent(string uri)
    {
        uri = uri.ToLower();

        //如果启用编辑器加载，方便调试，不reload所有lua，直接读文件
        if (IS_HOT_LUA)
        {
            return File.ReadAllBytes($"{Consts.Paths.Lua_Path}/{uri}");
        }

        //非编辑器热重载模式下，从内存中读
        if (luaMapper.ContainsKey(uri))
            return luaMapper[uri];

        //没找到
        CmgmLog.fError($"找不到Lua脚本 [{Consts.Paths.Lua_Path}/{uri}]");
        return null;
    }

    #endregion

    #region 执行LUA的外部接口：Executor
    //Lua文件执行
    /// <summary>Lua语句完全执行完成的标记栈，每次开始执行lua语句push一个新标记，每次执行完毕从lua中设置结果</summary>
    public readonly Stack<UniTaskCompletionSource<string>> CurrentEventSourceStack = new Stack<UniTaskCompletionSource<string>>();
    /// <summary>
    /// 执行一个lua文件
    /// </summary>
    /// <param name="uri">Consts.Paths.Lua_Path路径下的相对路径,需要带完整后缀</param>
    public async UniTask ExecuteLua(string uri)
    {
        bool isRootFile = uri == ROOT_FILE_URI;
        //如果不是要执行根文件，等初始化完成了再执行
        if (!isRootFile)
            await UniTask.WaitUntil(() => IsInited);

        //lua脚本的内容
        string luaContent = Encoding.UTF8.GetString(GetLuaContent(uri)).Trim('\n').Trim('\r');
        string template = isRootFile ? luaContent :
            $"local function temp_lua_func()\r\n " +
            $"{luaContent}\r\n " +
            $"end\r\n " +
            $"util.coroutine_call(combine(temp_lua_func, LuaExecuteFinished))();\r\n";
        //上面的combine函数，将第一个函数的返回值当作了第二个函数的参数，会被设置为CompletionSource的Result
        // 根文件在 OnInitAsync 内执行，此时 IsReady 仍为 false，不可经 Lua 回调 LuaExecuteFinished（会走 Instance）

        CmgmLog.fPositive($"开始执行lua语句：\r\n{template}");

        //新增一条完成标记
        //TODO:lua是按顺序执行的吗??? 在异步方法中调用会不会同时执行??? Stack顺序会不会混乱???
        var cs = new UniTaskCompletionSource<string>();
        CurrentEventSourceStack.Push(cs);

        try
        {
            LuaEnv.DoString(template.Trim());
            if (isRootFile)
                CompleteCurrentExecution(string.Empty);
            await cs.Task;
        }
        catch (Exception e)
        {
            if (IsExecuting())
            {
                //异常情况有时候不会执行到Lua代码末尾，要手动结束掉对应的UniTaskCompletionSource
                if (CurrentEventSourceStack.Peek() == cs)
                    CompleteCurrentExecution(string.Empty);
            }
            Debug.LogError("lua执行错误：" + e.ToString());
        }
    }

    /// <summary>一段 Lua 执行完毕时弹出 CompletionSource 并标记完成；根脚本 Init 阶段由 Manager 内部直接调用。</summary>
    internal void CompleteCurrentExecution(string ret)
    {
        var s = CurrentEventSourceStack.Pop();
        s.TrySetResult(ret);
        CmgmLog.fPositive("结束了该lua脚本的调用");
    }
    public bool IsExecuting()
    {
        return CurrentEventSourceStack.Count > 0;
    }
    public void ClearAllExecute()
    {
        if (CurrentEventSourceStack.Count > 0)
            CurrentEventSourceStack.Clear();
    }


    //TODO:Lua函数执行
    #endregion
}
