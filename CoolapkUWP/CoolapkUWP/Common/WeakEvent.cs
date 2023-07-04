using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoolapkUWP.Common
{
    public class WeakEvent<TEventArgs> : IList<Action<TEventArgs>>
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

            public void Invoke(TEventArgs arg)
            {
                _method.Invoke(_reference.Target, new object[] { arg });
            }
        }

        private readonly List<Method> _list = new List<Method>();

        public int Count => _list.Count;

        public bool IsReadOnly => ((ICollection<Method>)_list).IsReadOnly;

        public Action<TEventArgs> this[int index]
        {
            get => _list[index].Invoke;
            set => _list[index] = new Method(value);
        }

        public void Add(Action<TEventArgs> callback) => _list.Add(new Method(callback));

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

        public void Clear() => _list.Clear();

        public int IndexOf(Action<TEventArgs> callback)
        {
            for (int i = _list.Count - 1; i > -1; i--)
            {
                if (_list[i].Equals(callback))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, Action<TEventArgs> item) => _list.Insert(index, new Method(item));

        public void RemoveAt(int index) => _list.RemoveAt(index);

        public bool Contains(Action<TEventArgs> callback)
        {
            for (int i = _list.Count - 1; i > -1; i--)
            {
                if (_list[i].Equals(callback))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(Action<TEventArgs>[] array, int arrayIndex) => Array.Copy(_list.Select<Method, Action<TEventArgs>>(x => x.Invoke).ToArray(), 0, array, arrayIndex, _list.Count);

        bool ICollection<Action<TEventArgs>>.Remove(Action<TEventArgs> callback)
        {
            for (int i = _list.Count - 1; i > -1; i--)
            {
                if (_list[i].Equals(callback))
                {
                    _list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<Action<TEventArgs>> GetEnumerator()
        {
            foreach (Method method in _list)
            {
                yield return method.Invoke;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    }
}
