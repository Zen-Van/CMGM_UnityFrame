using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CMGM.Core
{
    /// <summary>
    /// Mono的管理者
    /// </summary>
    public class MonoController : MonoBehaviour
    {
        public event UnityAction updateEvent;
        public event UnityAction lateUpdateEvent;
        
        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            updateEvent?.Invoke();
        }

        void LateUpdate()
        {
            lateUpdateEvent?.Invoke();
        }

        /// <summary>
        /// 给外部提供的 添加帧更新事件的函数
        /// </summary>
        /// <param name="fun"></param>
        public void AddUpdateListener(UnityAction fun)
        {
            updateEvent += fun;
        }
        public void AddLateUpdateListener(UnityAction fun)
        {
            lateUpdateEvent += fun;
        }

        /// <summary>
        /// 给外部提供的 移除帧更新事件函数
        /// </summary>
        /// <param name="fun"></param>
        public void RemoveUpdateListener(UnityAction fun)
        {
            updateEvent -= fun;
        }
        public void RemoveLateUpdateListener(UnityAction fun)
        {
            lateUpdateEvent -= fun;
        }
    }
}
