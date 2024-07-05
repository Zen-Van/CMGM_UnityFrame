using System;
using System.Reflection;

/// <summary>
/// 懒汉单例（子类必须具有私有的无参构造函数）
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> where T : Singleton<T>
{
    /// <summary>
    /// 用于枷锁，保护类型，子类可以用来加锁数据
    /// </summary>
    protected static object lockObj = new object();
    private static T instance;
    public static T Instance
    {
        get
        {
            //单例模式加锁中独特的：双重校验锁
            //必须锁内外各判空一次才能一定不会破坏单例
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        //反射绕过访问修饰符获得构造函数信息，破坏了C#底层的封装性但正是我们需要的
                        ConstructorInfo info = typeof(T).GetConstructor(
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null, Type.EmptyTypes, null);

                        if (info == null) CmgmLog.FrameLogError("单例类 <color=yellow>[ " + typeof(T).Name + " ] </color>未声明私有无参构造函数");
                        else instance = info.Invoke(null) as T;
                    }
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// 初始化，可以用于提前加载懒汉单例模式，也可以在子类中重写增加新的初始化逻辑
    /// <para>若其中有逻辑，必须手动调用</para>
    /// </summary>
    public virtual void Init() { } //为空即可
}