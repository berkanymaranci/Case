using System.Collections.Generic;
using UnityEngine;

namespace BusJam.Pooling
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _stack;

        public ObjectPool(T prefab, Transform parent, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;
            _stack = new Stack<T>();
            for (int i = 0; i < prewarmCount; i++)
            {
                var instance = Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _stack.Push(instance);
            }
        }

        public T Get()
        {
            if (_stack.Count > 0)
            {
                var instance = _stack.Pop();
                instance.gameObject.SetActive(true);
                return instance;
            }
            return Object.Instantiate(_prefab, _parent);
        }

        public void Return(T instance)
        {
            instance.transform.SetParent(_parent, false);
            instance.gameObject.SetActive(false);
            _stack.Push(instance);
        }
    }
}