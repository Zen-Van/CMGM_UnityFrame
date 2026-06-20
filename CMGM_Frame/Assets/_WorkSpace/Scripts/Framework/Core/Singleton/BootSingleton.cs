using System;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace CMGM.Core
{
    /// <summary>
    /// Boot 型单例：须 <see cref="InitAsync"/> 完成后才允许 <see cref="Instance"/>。
    /// </summary>
    public abstract class BootSingleton<T> where T : BootSingleton<T>
    {
        protected static object lockObj = new object();
        private static T instance;
        private static bool isReady;
        private static readonly object initLock = new object();
        private static UniTask initTask;

        public static bool IsReady => isReady;

        /// <summary>Boot 组合根唯一推荐入口。</summary>
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
                initTask = InitInternalAsync();
                return initTask;
            }
        }

        private static async UniTask InitInternalAsync()
        {
            var inst = GetOrCreateInstance();
            await inst.OnInitAsync();
            isReady = true;
        }

        public static T Instance
        {
            get
            {
                if (!isReady)
                {
                    CmgmLog.fError(
                        $"[BootSingleton] {typeof(T).Name} 尚未 InitAsync，请先在 Boot 中 await {typeof(T).Name}.InitAsync()");
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
            initTask = default;
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
