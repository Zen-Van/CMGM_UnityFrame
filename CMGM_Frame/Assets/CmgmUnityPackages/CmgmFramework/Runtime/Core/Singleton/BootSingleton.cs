using System;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace CMGM.Core
{
    /// <summary>
    /// Boot 型单例：推荐 <see cref="InitAsync"/>；未 Init 时 <see cref="Instance"/> 会 Error 并兜底阻塞 Init（防呆）。
    /// <para>禁止在 <see cref="OnInitAsync"/> 内访问<strong>自身</strong> <see cref="Instance"/>（重入时不阻塞，避免死锁）。</para>
    /// </summary>
    public abstract class BootSingleton<T> where T : BootSingleton<T>
    {
        protected static object lockObj = new object();
        private static T instance;
        private static bool isReady;
        private static bool isInitializing;
        private static bool fallbackInitWarned;
        private static bool reentrantAccessWarned;
        private static readonly object initLock = new object();
        private static UniTask initTask;

        public static bool IsReady => isReady;

        /// <summary>Boot 组合根 / Loading 任务唯一推荐入口。</summary>
        public static UniTask InitAsync()
        {
            if (isReady)
                return UniTask.CompletedTask;

            lock (initLock)
            {
                if (isReady)
                    return UniTask.CompletedTask;
                if (initTask.Status == UniTaskStatus.Pending)
                    return initTask;

                isInitializing = true;
                initTask = InitInternalAsync();
                return initTask;
            }
        }

        private static async UniTask InitInternalAsync()
        {
            try
            {
                var inst = GetOrCreateInstance();
                await inst.OnInitAsync();
                isReady = true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        public static T Instance
        {
            get
            {
                if (!isReady)
                {
                    if (isInitializing)
                    {
                        if (!reentrantAccessWarned)
                        {
                            reentrantAccessWarned = true;
                            CmgmLog.fError(
                                $"[BootSingleton] {typeof(T).Name} OnInitAsync 执行中又访问 Instance（重入），" +
                                "返回未 Ready 实例；请避免在 Init 内访问自身 Instance。");
                        }

                        return GetOrCreateInstance();
                    }

                    if (!fallbackInitWarned)
                    {
                        fallbackInitWarned = true;
                        CmgmLog.fError(
                            $"[BootSingleton] {typeof(T).Name} 尚未 InitAsync，兜底阻塞 Init（正式路径请显式 await InitAsync）。");
                    }

                    InitAsync().GetAwaiter().GetResult();
                }

                return GetOrCreateInstance();
            }
        }

        /// <summary>Boot 初始化逻辑（同步或 async）。</summary>
        protected virtual UniTask OnInitAsync() => UniTask.CompletedTask;

        /// <summary>如 <see cref="LuaManager.Clear"/> 后需再次 InitAsync。</summary>
        protected static void ResetBootState()
        {
            isReady = false;
            isInitializing = false;
            initTask = default;
            fallbackInitWarned = false;
            reentrantAccessWarned = false;
        }

        private static T GetOrCreateInstance()
        {
            if (instance != null)
                return instance;

            lock (lockObj)
            {
                if (instance == null)
                    instance = CreateInstance();
            }

            return instance;
        }

        private static T CreateInstance()
        {
            ConstructorInfo info = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            if (info == null)
                CmgmLog.fError(
                    "单例类 <color=yellow>[ " + typeof(T).Name + " ] </color>未声明<color=#C67171>私有</color>无参构造函数");

            return info.Invoke(null) as T;
        }
    }
}
