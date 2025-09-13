#if !UNITY_2021_1_OR_NEWER
using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
    private readonly Func<T> _createFunc;
    private readonly Action<T> _actionOnGet;
    private readonly Action<T> _actionOnRelease;
    private readonly Action<T> _actionOnDestroy;
    private readonly Stack<T> _stack;
    private readonly int _maxSize;

    public ObjectPool(
        Func<T> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        int defaultCapacity = 10,
        int maxSize = 1000)
    {
        _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        _actionOnGet = actionOnGet;
        _actionOnRelease = actionOnRelease;
        _actionOnDestroy = actionOnDestroy;
        _stack = new Stack<T>(defaultCapacity);
        _maxSize = maxSize;

        // 기본 용량만큼 미리 생성
        for (int i = 0; i < defaultCapacity; i++)
        {
            var item = _createFunc();
            _actionOnRelease?.Invoke(item);
            _stack.Push(item);
        }
    }

    public T Get()
    {
        T item = _stack.Count > 0 ? _stack.Pop() : _createFunc();
        _actionOnGet?.Invoke(item);
        return item;
    }

    public void ReturnToPool(T item)
    {
        if (_stack.Count < _maxSize)
        {
            _actionOnRelease?.Invoke(item);
            _stack.Push(item);
        }
        else
        {
            _actionOnDestroy?.Invoke(item);
        }
    }

    public void Clear()
    {
        while (_stack.Count > 0)
        {
            T item = _stack.Pop();
            _actionOnDestroy?.Invoke(item);
        }
    }
}
#endif