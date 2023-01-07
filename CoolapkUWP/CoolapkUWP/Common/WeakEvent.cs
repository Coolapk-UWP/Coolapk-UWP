using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoolapkUWP.Common
{
    public class WeakEvent<TEventArgs>
    {
        private class Method
        {
            private readonly bool _isStatic;
            private readonly WeakReference _reference;
            private readonly MethodInfo _method;

            public bool IsDead => !(_isStatic || _reference.IsAlive);

            public Method(Action<TEventArgs> callback)
            {
                _isStatic = callback.Target == null;
                _reference = new WeakReference(callback.Target);
                _method = callback.GetMethodInfo();
            }

            public bool Equals(Action<TEventArgs> callback)
            {
                return _reference.Target == callback.Target && _method == callback.GetMethodInfo();
            }

            public void Invoke(object arg)
            {
                _method.Invoke(_reference.Target, new object[] { arg });
            }
        }

        private readonly List<Method> _list = new List<Method>();

        public int Count => _list.Count;

        public void Add(Action<TEventArgs> callback)
        {
            _list.Add(new Method(callback));
        }

        public void Remove(Action<TEventArgs> callback)
        {
            for (int i = _list.Count - 1; i > -1; i--)
            {
                if (_list[i].Equals(callback))
                {
                    _list.RemoveAt(i);
                }
            }
        }

        public void Invoke(TEventArgs arg)
        {
            for (int i = _list.Count - 1; i > -1; i--)
            {
                if (_list[i].IsDead)
                {
                    _list.RemoveAt(i);
                }
                else
                {
                    _list[i].Invoke(arg);
                }
            }
        }

        public void Clear()
        {
            _list.Clear();
        }
    }
}
