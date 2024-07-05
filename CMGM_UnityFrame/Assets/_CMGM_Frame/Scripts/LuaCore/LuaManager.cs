using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;
using XLua.TemplateEngine;

public class LuaManager : Singleton<LuaManager>
{
    /// <summary> 是否在editor中调试时热加载lua（可以不重启游戏进行编辑）</summary>
    private bool LOAD_FROM_EDITOR = Application.isEditor && !CmgmFrameSettings.Instance.IS_LUA_LOAD_FROM_AB_IN_EDITOR;

    private string ROOT_FILE_URI = CmgmFrameSettings.Instance.ROOT_FILE_URI;

    private LuaManager() { }
    public LuaEnv LuaEnv { get; private set; }


    public bool IsInited { get; private set; } = false;

    /// <summary>
    /// 创建luaEnv，设置luaEnv路径重定向，将所有lua脚本载入内存，执行lua根脚本
    /// </summary>
    public override void Init()
    {
        if (IsInited) return;

        LuaEnv = new LuaEnv();

        //重定向指在lua脚本中require时加载的相对路径
        LuaEnv.AddLoader((ref string uri) =>
        {
            //编辑器下查询逻辑
            string path = Consts.Paths.Lua_Path + "/" + uri;
            if (File.Exists(path))
                return File.ReadAllBytes(path);

            return null;
        });
        LuaEnv.AddLoader((ref string uri) =>
        {
            //Resource下查询逻辑
            TextAsset lua = Resources.Load<TextAsset>(uri);
            if (lua != null)
                return lua.bytes;
            return null;
        });
        LuaEnv.AddLoader((ref string uri) =>
        {
            //AB包下查询逻辑
            TextAsset lua = AbResLoader.Instance.LoadRes<TextAsset>(uri);
            if (lua != null)
                return lua.bytes;
            else
                Debug.Log("lua文件重定向失败，试图重定向的lua文件：" + uri);

            return null;
        });

        //Lua管理器初始化的时候，就把所有lua脚本载入内存，并且把main执行了
        UniTask.Void(async () =>
        {
            //读取所有lua映射表
            await LoadLuaMapper();
            //执行lua根文件
            await ExecuteLua(ROOT_FILE_URI);
            //都完成了才初始化完成
            IsInited = true;
        });

    }
    /// <summary>
    /// 清空Lua数据和缓存，清空c#和lua互转数据，删除luaEnv
    /// </summary>
    public void Clear()
    {
        IsInited = false;

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

    //从所有lua加载路径加载lua文件，并存入luaMapper,映射关系为 【lua脚本名，带.lua后缀】->【luaBytes】
    public async UniTask LoadLuaMapper()
    {
        string[] files;
        if (LOAD_FROM_EDITOR)
        {
            //依次加载Consts.Paths.Lua_Path路径下的每一个文本文件，存进LuaMapper
            files = Directory.GetFiles(Consts.Paths.Lua_Path, "*.lua.txt", SearchOption.AllDirectories);
        }
        else
        {
            //依次加载lua包中的每一个文本文件，存进luaMapper
            await AbResLoader.Instance.LoadABAsync("lua");
            AssetBundle ab = AbResLoader.Instance.GetLoadedAB("lua");
            files = ab.GetAllAssetNames();
        }

        if (files.Length <= 0)
        {
            CmgmLog.FrameLogPositive("没有找到任何Lua脚本");
            return;
        }

        luaMapper = new Dictionary<string, byte[]>();
        for (int i = 0; i < files.Length; i++)
        {
            string fileUrl = files[i].ToLower();
            string fileUri = fileUrl.Substring(Consts.Paths.Lua_Path.Length + 1);//此处的+1删除了斜杠
            //Editor和AB包都通过Assets下完整路径加载
            var asset = await ResManager.Instance.LoadAsync<TextAsset>(fileUrl);

            //用lua文件夹下的uri映射内容
            luaMapper[fileUri] = Encoding.UTF8.GetBytes(asset.text);
            CmgmLog.FrameLogPositive($"载入了Lua脚本【{fileUri}】，其内容为：\n{asset.text}");
        }
    }
    /// <summary>
    /// 清空LuaMapper
    /// </summary>
    public void ClearLuaMapper()
    {
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
        if (LOAD_FROM_EDITOR)
        {
            return File.ReadAllBytes($"{Consts.Paths.Lua_Path}/{uri}");
        }

        //非编辑器热重载模式下，从内存中读
        if (luaMapper.ContainsKey(uri))
            return luaMapper[uri];

        //没找到
        CmgmLog.FrameLogError($"找不到Lua脚本 [{Consts.Paths.Lua_Path}/{uri}]");
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
        //如果不是要执行根文件，等初始化完成了再执行
        if (uri != ROOT_FILE_URI)
            await UniTask.WaitUntil(() => IsInited);

        //lua脚本的内容
        string luaContent = Encoding.UTF8.GetString(GetLuaContent(uri)).Trim('\n').Trim('\r');

        string template =
            $"local function temp_lua_func()\r\n " +
            $"{luaContent}\r\n " +
            $"end\r\n " +
            $"util.coroutine_call(combine(temp_lua_func, LuaExecuteFinished))();\r\n";
        //上面的combine函数，将第一个函数的返回值当作了第二个函数的参数，会被设置为CompletionSource的Result

        //如果是根文件
        if (uri == ROOT_FILE_URI) template = luaContent + "\r\n " +
            $"LuaExecuteFinished('');";


        CmgmLog.FrameLogPositive($"开始执行lua语句：\r\n{template}");

        //新增一条完成标记
        //TODO:lua是按顺序执行的吗??? 在异步方法中调用会不会同时执行??? Stack顺序会不会混乱???
        var cs = new UniTaskCompletionSource<string>();
        CurrentEventSourceStack.Push(cs);

        try
        {
            LuaEnv.DoString(template.Trim());
            await cs.Task;
        }
        catch (Exception e)
        {
            if (IsExecuting())
            {
                //异常情况有时候不会执行到Lua代码末尾，要手动结束掉对应的UniTaskCompletionSource
                if (CurrentEventSourceStack.Peek() == cs)
                    LuaBridge.LuaExecuteFinished(template);
            }
            Debug.LogError("lua执行错误：" + e.ToString());
        }
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
