using System.Collections.Generic;
using QuizRush.Services;
using UnityEngine;

namespace QuizRush.Analytics
{
    /// <summary>
    /// Lightweight analytics layer writing to the Unity console and persisting basic counters.
    /// </summary>
    public class AnalyticsHook : MonoBehaviour
    {
        private const string SessionCountKey = "QuizRush.Analytics.Sessions";

        private readonly Queue<string> recentEvents = new();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Records a new analytics event and stores a small rolling log for debugging.
        /// </summary>
        public void Track(string eventName, IDictionary<string, object> data = null)
        {
            var payload = data == null ? "" : JsonUtility.ToJson(new SerializableDictionary(data));
            Debug.Log($"[Analytics] {eventName} {payload}");
            recentEvents.Enqueue(eventName);
            while (recentEvents.Count > 32)
            {
                recentEvents.Dequeue();
            }
        }

        /// <summary>
        /// Called at the start of a new session.
        /// </summary>
        public void TrackSessionStart()
        {
            var count = PlayerPrefs.GetInt(SessionCountKey, 0) + 1;
            PlayerPrefs.SetInt(SessionCountKey, count);
            PlayerPrefs.Save();
            Track("session_start", new Dictionary<string, object> { { "count", count } });
        }

        private class SerializableDictionary
        {
            public List<string> keys = new();
            public List<string> values = new();

            public SerializableDictionary(IDictionary<string, object> data)
            {
                foreach (var pair in data)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value?.ToString() ?? string.Empty);
                }
            }
        }
    }
}
