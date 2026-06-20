using System;
using System.Reflection;

namespace CMGM.Core
{
    /// <summary>
    /// 懒加载单例：首次 <see cref="Instance"/> 时创建；Boot 中默认不调用。
    /// </summary>
    public abstract class LazySingleton<T> where T : LazySingleton<T>
    {
        protected static object lockObj = new object();
        private static T instance;
        private bool _lazyInitialized;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = CreateInstance();
                    }
                }

                if (!instance._lazyInitialized)
                {
                    lock (lockObj)
                    {
                        if (!instance._lazyInitialized)
                        {
                            instance.OnLazyInitialize();
                            instance._lazyInitialized = true;
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>首次 <see cref="Instance"/> 时调用一次；子类可放轻量 setup。</summary>
        protected virtual void OnLazyInitialize() { }

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
