using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    /// <summary>
    /// Maintains a collection of event handlers and associated filter. 
    /// Passes events on to handlers if the associated selector matches.
    /// </summary>
    public class FilteringEventDispatcher
    {
        readonly Dictionary<IHandleEvents, ISelectEvents> _eventHandlers;

        /// <summary>
        /// Initialize the dispatchers internal members
        /// </summary>
        public FilteringEventDispatcher()
        {
            _eventHandlers = new Dictionary<IHandleEvents, ISelectEvents>();
        }

        /// <summary>
        /// Register or reregister an event handler.
        /// </summary>
        public void Subscribe(IHandleEvents eventHandler, ISelectEvents eventFilter = null)
        {
            eventFilter = eventFilter ?? DelegateSelector.Any;
            _eventHandlers[eventHandler] = eventFilter;
        }

        /// <summary>
        /// Register or reregister a generic handler
        /// </summary>
        public void Subscribe(Action<IEvent> eventHandler, Func<IEvent, bool> selector = null)
        {
            _eventHandlers[new DelegateHandler(eventHandler)] = selector != null 
                    ? new DelegateSelector(selector) 
                    : DelegateSelector.Any;
        }

        /// <summary>
        /// Remove a previously subscribed generic handler
        /// </summary>
        /// <param name="eventHandler"></param>
        public void Unsubscribe(Action<IEvent> eventHandler)
        {
            _eventHandlers.Remove(new DelegateHandler(eventHandler));
        }


        /// <summary>
        /// Remove a previously subscribed event handler.
        /// </summary>
        /// <param name="eventHandler"></param>
        public void Unsubscribe(IHandleEvents eventHandler)
        {
            _eventHandlers.Remove(eventHandler);
        }

        /// <summary>
        /// Pass an event to each matching event handler ignoring any exceptions
        /// </summary>
        public void Dispatch(IEvent @event)
        {
            foreach (KeyValuePair<IHandleEvents, ISelectEvents> pair in _eventHandlers)
            {
                try
                {
                    var selector = pair.Value;
                    var handler = pair.Key;

                    if (selector.Matches(@event))
                    {
                        handler.Handle(@event);
                    }
                }
                catch
                {
                    //Swallow exceptions
                }
            }
        }
    }
}