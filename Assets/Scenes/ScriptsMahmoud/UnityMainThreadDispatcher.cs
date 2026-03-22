
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Permet d'exécuter des actions sur le thread principal Unity
/// depuis des callbacks Android (threads Java).
/// Placez ce script sur un GameObject persistant (ex: GameManager).
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static UnityMainThreadDispatcher _instance;

    public static void Enqueue(Action action)
    {
        if (action == null) return;
        lock (_queue)
            _queue.Enqueue(action);
    }

    void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }
}
