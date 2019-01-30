using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityScript.Events
{
    public class InternalEvent<T>
    {
        public event Action<object, T> Handlers
        {
            add => Add(value);
            remove => Remove(value);
        }

        protected readonly Func<Exception, bool> exceptionHandler;

        public InternalEvent(Func<Exception, bool> exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
        }

        protected List<Tuple<int, Action<object, T>>> handlers = new List<Tuple<int, Action<object, T>>>();

        public void Add(Action<object, T> handler, int priority = 0)
        {
            int index = 0;
            for(;index < handlers.Count; index++)
            {
                if (handlers[index].Item1 >= priority)
                    break;
            }

            handlers.Insert(index, new Tuple<int, Action<object, T>>(priority, handler));

        }

        public void Remove(Action<object, T> handler, int priority = 0)
        {
            handlers.RemoveAll(hand => hand.Item2 == handler);
        }

        protected internal void Run(object sender, T args)
        {
            if (args is EatableEventArgs eatable)
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler.Item2(sender, args);
                    }
                    catch (Exception ex)
                    {
                        if (exceptionHandler(ex))
                            return;
                    }

                    if (eatable.Eaten)
                        return;
                }
            }
            else
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler.Item2(sender, args);
                    }
                    catch (Exception ex)
                    {
                        if (exceptionHandler(ex))
                            return;
                    }
                }
            }
        }
    }

    public class InternalEvent : InternalEvent<EventArgs>
    {
        public InternalEvent(Func<Exception, bool> exceptionHandler) : base(exceptionHandler)
        {

        }
        protected internal void Run(object sender)
            => Run(sender, EventArgs.Empty);
    }

    public class Event<T> : InternalEvent<T>
    {
        public Event(Func<Exception, bool> exceptionHandler) : base(exceptionHandler)
        {

        }

        public new void Run(object sender, T args)
        {
            base.Run(sender, args);
        }
    }

    public class Event : Event<EventArgs>
    {
        public Event(Func<Exception, bool> exceptionHandler) : base(exceptionHandler)
        {

        }

        public void Run(object sender)
            => Run(sender, EventArgs.Empty);
    }

    public class EatableEventArgs : EventArgs
    {
        public bool Eaten { get; private set; }

        public void Eat()
        {
            Eaten = true;
        }
    }
}
