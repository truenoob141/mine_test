using System;
using System.Collections.Generic;
using System.Linq;

namespace Mine
{
    public class EventManager
    {
        private Dictionary<Type, List<EventHandler>> events = new Dictionary<Type, List<EventHandler>>();

        public void Subscribe<T>(Action handler)
        {
            Subscribe(typeof(T), new ActionHandler(handler));
        }

        public void Subscribe<T>(Action<T> handler)
        {
            Subscribe(typeof(T), new ActionHandler<T>(handler));
        }

        public void Unsubscribe<T>(Action handler)
        {
            Unsubscribe(typeof(T), handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            Unsubscribe(typeof(T), handler);
        }

        public void Trigger<T>()
        {
            Trigger(typeof(T), Activator.CreateInstance<T>());
        }

        public void Trigger<T>(T arg)
        {
            Trigger(typeof(T), arg);
        }

        private void Trigger(Type type, object arg)
        {
            List<EventHandler> handlers;
            if (!events.TryGetValue(type, out handlers))
                return;

            foreach (var handler in handlers.ToArray())
            {
                try
                {
                    handler.Invoke(arg);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }

        private void Subscribe(Type type, EventHandler handler)
        {
            List<EventHandler> handlers;
            if (!events.TryGetValue(type, out handlers))
            {
                handlers = new List<EventHandler>(1);
                events.Add(type, handlers);
            }

            if (handlers.Any(h => h.Handler.Equals(handler.Handler)))
                throw new ArgumentException("Already subscribe " + handler.Handler);

            handlers.Add(handler);
        }

        private void Unsubscribe(Type type, object handler)
        {
            List<EventHandler> handlers;
            if (!events.TryGetValue(type, out handlers))
                return;

            if (handlers.RemoveAll(h => h.Handler.Equals(handler)) > 0)
            {
                if (handlers.Count == 0)
                    events.Remove(type);
            }
        }

        private abstract class EventHandler
        {
            public abstract object Handler { get; }

            public abstract void Invoke(object arg);
        }

        private class ActionHandler : EventHandler
        {
            public override object Handler => handler;

            private Action handler;

            public ActionHandler(Action handler)
            {
                this.handler = handler;
            }

            public override void Invoke(object arg)
            {
                handler();
            }
        }

        private class ActionHandler<T> : EventHandler
        {
            public override object Handler => handler;

            private Action<T> handler;

            public ActionHandler(Action<T> handler)
            {
                this.handler = handler;
            }

            public override void Invoke(object arg)
            {
                handler((T)arg);
            }
        }
    }
}
