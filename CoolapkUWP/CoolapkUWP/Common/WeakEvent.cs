using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoolapkUWP.Common
{
    public sealed class WeakEvent<TEventArgs> : IList<Action<TEventArgs>>
    {
        private sealed class Method : IEquatable<Method>, IEquatable<Action<TEventArgs>>
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

            public void Invoke(TEventArgs arg)
            {
                if (!IsDead)
                {
                    _method.Invoke(_reference.Target, new object[] { arg });
                }
            }

            public bool Equals(Method other) =>
                other != null
                    && _reference.Target == other._reference.Target
                    && _method == other._method;

            public bool Equals(Action<TEventArgs> callback) =>
                callback != null
                    && _reference.Target == callback.Target
                    && _method == callback.GetMethodInfo();

            public override bool Equals(object obj)
            {
                switch (obj)
                {
                    case Method other:
                        return Equals(other);
                    case Action<TEventArgs> callback:
                        return Equals(callback);
                    default:
                        return false;
                }
            }

            public override int GetHashCode() => (_reference, _method).GetHashCode();

            public static implicit operator Method(Action<TEventArgs> callback) => new Method(callback);

            public static explicit operator Action<TEventArgs>(Method method) => method.IsDead ? null : method._method.CreateDelegate(typeof(Action<TEventArgs>), method._reference.Target) as Action<TEventArgs>;
        }

        private readonly List<Method> _list;

        public WeakEvent() => _list = new List<Method>();

        public WeakEvent(IEnumerable<Action<TEventArgs>> collection) => _list = new List<Method>(collection.Select<Action<TEventArgs>, Method>(x => x));

        public WeakEvent(int capacity) => _list = new List<Method>(capacity);

        public int Count => _list.Count;

        bool ICollection<Action<TEventArgs>>.IsReadOnly => ((ICollection<Method>)_list).IsReadOnly;

        public Action<TEventArgs> this[int index]
        {
            get => (Action<TEventArgs>)_list[index];
            set => _list[index] = value;
        }

        public void Invoke(TEventArgs arg)
        {
            for (int i = _list.Count; --i >= 0;)
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

        public void Add(Action<TEventArgs> callback) => _list.Add(callback);

        public void AddRange(IEnumerable<Action<TEventArgs>> collection) => _list.AddRange(collection.Select<Action<TEventArgs>, Method>(x => x));

        public void Insert(int index, Action<TEventArgs> item) => _list.Insert(index, item);

        public void CopyTo(Action<TEventArgs>[] array, int arrayIndex) => Array.Copy(_list.Select(x => (Action<TEventArgs>)x).ToArray(), 0, array, arrayIndex, _list.Count);

        public bool Remove(Action<TEventArgs> callback)
        {
            for (int i = _list.Count; --i >= 0;)
            {
                if (_list[i].IsDead)
                {
                    _list.RemoveAt(i);
                }
                else if (_list[i].Equals(callback))
                {
                    _list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAt(int index) => _list.RemoveAt(index);

        public int RemoveAll(Predicate<Action<TEventArgs>> predicate) => _list.RemoveAll(x => predicate((Action<TEventArgs>)x));

        public void Clear() => _list.Clear();

        public bool Contains(Action<TEventArgs> callback)
        {
            for (int i = _list.Count; --i >= 0;)
            {
                if (_list[i].IsDead)
                {
                    _list.RemoveAt(i);
                }
                else if (_list[i].Equals(callback))
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(Action<TEventArgs> callback)
        {
            for (int i = _list.Count; --i >= 0;)
            {
                if (_list[i].IsDead)
                {
                    _list.RemoveAt(i);
                }
                else if (_list[i].Equals(callback))
                {
                    return i;
                }
            }
            return -1;
        }

        public IEnumerator<Action<TEventArgs>> GetEnumerator() => _list.Select(x => (Action<TEventArgs>)x).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static WeakEvent<TEventArgs> operator +(WeakEvent<TEventArgs> weakEvent, Action<TEventArgs> callback)
        {
            weakEvent.Add(callback);
            return weakEvent;
        }

        public static WeakEvent<TEventArgs> operator -(WeakEvent<TEventArgs> weakEvent, Action<TEventArgs> callback)
        {
            _ = weakEvent.Remove(callback);
            return weakEvent;
        }
    }
}
