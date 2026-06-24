using System.Collections.Generic;
using CMGM.Core;

namespace CMGM.GameFlow
{
    /// <summary>
    /// 栈式宏观流程机：<see cref="SwitchTo"/> 清空栈并切根态；<see cref="Push"/> / <see cref="Pop"/> 叠层（如暂停）。
    /// </summary>
    public sealed class GameFlowMachine : LazySingleton<GameFlowMachine>
    {
        private readonly Stack<IGameFlowState> _stack = new Stack<IGameFlowState>();

        private GameFlowMachine() { }

        public IGameFlowState Current => _stack.Count > 0 ? _stack.Peek() : null;

        /// <summary>自底向顶的栈快照（只读，用于日志 / 调试）。</summary>
        public IReadOnlyList<IGameFlowState> StackSnapshot
        {
            get
            {
                if (_stack.Count == 0)
                    return System.Array.Empty<IGameFlowState>();

                var array = _stack.ToArray();
                System.Array.Reverse(array);
                return array;
            }
        }

        /// <summary>替换整条流程链：自顶向下 <see cref="IGameFlowState.Exit"/>，再 Enter 新根态。</summary>
        public void SwitchTo(IGameFlowState next)
        {
            if (next == null)
                return;

            if (_stack.Count == 1 && ReferenceEquals(Current, next))
                return;

            while (_stack.Count > 0)
            {
                var top = _stack.Pop();
                top.Exit();
            }

            CmgmLog.fNormal($"[GameFlow] SwitchTo → {next.StateName}");
            _stack.Push(next);
            next.Enter();
            LogStack();
        }

        /// <summary>叠层：保留栈下态，新态 Enter（下态不 Exit）。</summary>
        public void Push(IGameFlowState state)
        {
            if (state == null)
                return;

            if (ReferenceEquals(Current, state))
                return;

            CmgmLog.fNormal($"[GameFlow] Push → {state.StateName}");
            _stack.Push(state);
            state.Enter();
            LogStack();
        }

        /// <summary>弹出栈顶 Exit；若栈非空，下态保持 Enter 过的活跃状态（不重复 Enter）。</summary>
        public bool Pop()
        {
            if (_stack.Count == 0)
                return false;

            var top = _stack.Peek();
            CmgmLog.fNormal($"[GameFlow] Pop ← {top.StateName}");
            top.Exit();
            _stack.Pop();
            LogStack();
            return true;
        }

        public void Tick(float deltaTime)
        {
            Current?.Update(deltaTime);
        }

        private void LogStack()
        {
            if (_stack.Count == 0)
            {
                CmgmLog.fNormal("[GameFlow] stack: (empty)");
                return;
            }

            var names = new List<string>(_stack.Count);
            foreach (var state in StackSnapshot)
                names.Add(state.StateName);

            CmgmLog.fNormal($"[GameFlow] stack: [{string.Join(" > ", names)}]");
        }
    }
}
