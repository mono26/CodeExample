using BugProject.Core.Debugging;
using System;
using System.Collections.Generic;

namespace BugProject.Core.EventSystem
{
    public class BugEventController : BugScript
    {
        private Dictionary<string, List<Action<IBugEventArgs>>> eventsMap = new Dictionary<string, List<Action<IBugEventArgs>>>();

        // Multiple queues are used to prevent recursive event triggering.
        private int activeQueue = 0;
        private Queue<KeyValuePair<string, IBugEventArgs>>[] queues = null;

        #region Unity functions
        private void Awake()
        {
            Init();
        }
        #endregion

        /// <summary>
        /// Initialized the event controller to a clean state.
        /// </summary>
        private void Init()
        {
            activeQueue = 0;
            queues = new Queue<KeyValuePair<string, IBugEventArgs>>[2] {
                new Queue<KeyValuePair<string, IBugEventArgs>>(0),
                new Queue<KeyValuePair<string, IBugEventArgs>>(0)
            };
        }     

        /// <summary>
        /// Triggers a specific event calling all the listeners in te listeners list.
        /// </summary>
        /// <param name="_eventId">Id of the event to trigger.</param>
        /// <param name="_args">Event data.</param>
        private void TriggerEvent(string _eventId, IBugEventArgs _args)
        {
            if (eventsMap.ContainsKey(_eventId))
            {
                foreach (Action<IBugEventArgs> action in eventsMap[_eventId])
                {
                    action?.Invoke(_args);
                }
            }
            else
            {
                // TODO throw trying to fire and event that doesn't exist.
            }
        }

        /// <summary>
        /// Clears the event map.
        /// </summary>
        private void ClearAllEvents()
        {
            eventsMap.Clear();
        }

        /// <summary>
        /// Sibscribes a listener to a specific event.
        /// </summary>
        /// <param name="_eventId">Id of the event to subscribe.</param>
        /// <param name="_action">Action to call when the event triggers.</param>
        public void SubscribeToEvent(string _eventId, Action<IBugEventArgs> _action)
        {
            if (eventsMap.ContainsKey(_eventId))
            {
                if (!eventsMap[_eventId].Contains(_action))
                {
                    eventsMap[_eventId].Add(_action);
                }
                else
                {
                    // TODO throw duplicate subscription for delegate.
                    BugDebugController.LogWarningMessage($"Subscribing action that is already subscribed { _action.GetHashCode() }");
                }
            }
            else
            {
                eventsMap[_eventId] = new List<Action<IBugEventArgs>>() { _action };
            }
        }

        /// <summary>
        /// Unsubscribes an action from a specific event.
        /// </summary>
        /// <param name="_eventId">Id of the event to unsubscribe from.</param>
        /// <param name="_action">Action to unsubscribe.</param>
        public void UnSubscribeFromEvent(string _eventId, Action<IBugEventArgs> _action)
        {
            if (eventsMap.ContainsKey(_eventId))
            {
                if (eventsMap[_eventId].Contains(_action))
                {
                    eventsMap[_eventId].Remove(_action);
                }
                else
                {
                    // TODO throw unsuscribing from event without subscription.
                    BugDebugController.LogWarningMessage($"Unsubscribing action that is not subscribed { _action.GetHashCode() }");
                }
            }
            else
            {
                // TODO throw unsubscribing action for event that doest exist.
                BugDebugController.LogWarningMessage($"Unsubscribing action from event that is not subscribed { _action.GetHashCode() }");
            }
        }

        /// <summary>
        /// Pushes a event into the event queue.
        /// </summary>
        /// <param name="_id">Id of the event to push.</param>
        /// <param name="_args">Event data.</param>
        public void PushEvent(string _id, IBugEventArgs _args)
        {
            int queueIndex = (activeQueue + 1) % 2;
            queues[queueIndex].Enqueue(new KeyValuePair<string, IBugEventArgs>(_id, _args));
        }

        /// <summary>
        /// Updates and executes the current event queue.
        /// </summary>
        public void OnUpdate()
        {
            int eventsCount = queues[activeQueue].Count;
            if (queues[activeQueue] != null && eventsCount > 0)
            {
                for (int i = 0; i < eventsCount; i++)
                {
                    KeyValuePair<string, IBugEventArgs> tempEvent = queues[activeQueue].Dequeue();
                    TriggerEvent(tempEvent.Key, tempEvent.Value);
                }
            }

            activeQueue++;
            activeQueue = activeQueue % 2;
        }
    }
}
