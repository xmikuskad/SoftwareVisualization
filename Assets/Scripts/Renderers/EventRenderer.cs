using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Renderers
{
    public class EventRenderer : MonoBehaviour
    {
        public Queue<EventData> queue = new ();
        private EventData current;
        [Header("References")] public DataRenderer dataRenderer;

        public void Init(List<EventData> vertices)
        {
            foreach (var eventData in vertices)
            {
                queue.Enqueue(eventData);
            }
        }

        public void NextQueue()
        {
            if (SingletonManager.Instance.pauseManager.IsSomethingPaused() || queue.Count == 0)
            {
                return;
            }
            current = queue.Dequeue();
            dataRenderer.ProcessEvent(current);
        }
    }
}