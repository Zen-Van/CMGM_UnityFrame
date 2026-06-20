using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace CMGM.Core
{
    /// <summary>
    /// 1.可以提供给外部添加帧更新事件的方法
    /// 2.可以提供给外部添加协程的方法
    /// 3.提供自动添加tag的方法
    /// </summary>
    public class MonoMgr : LazySingleton<MonoMgr>
    {
        private MonoController controller; 

        private MonoMgr() { }

        protected override void OnLazyInitialize()
        {
            GameObject obj = new GameObject("MonoController");
            controller = obj.AddComponent<MonoController>();
        }

        #region 帧更新封装
        /// <summary>
        /// 给外部提供的 添加帧更新事件的函数
        /// </summary>
        /// <param name="fun"></param>
        public void AddUpdateListener(UnityAction fun)
        {
            controller.AddUpdateListener(fun);
        }
        public void AddLateUpdateListener(UnityAction fun)
        {
            controller.AddLateUpdateListener(fun);
        }

        /// <summary>
        /// 给外部提供的 移除帧更新事件函数
        /// </summary>
        /// <param name="fun"></param>
        public void RemoveUpdateListener(UnityAction fun)
        {
            controller.RemoveUpdateListener(fun);
        }
        public void RemoveLateUpdateListener(UnityAction fun)
        {
            controller.RemoveLateUpdateListener(fun);
        }
        #endregion
    }
}
