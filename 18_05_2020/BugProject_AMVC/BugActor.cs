using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugProject.Core.Actors
{
    public class BugActor : BugScript
    {
        [SerializeField]
        private string uniqueId = null;
        private Dictionary<string, BugActorComponent> componentsMap = new Dictionary<string, BugActorComponent>();

        public string SetUniqueId { set => uniqueId = value; }
        public string GetUniqueId { get => uniqueId; }

        #region Unity functions.
        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            PostInit();
        }
        #endregion

        /// <summary>
        /// Initializes the actor.
        /// </summary>
        private void Init()
        {
            SetUniqueId = Guid.NewGuid().ToString();

            BugActorComponent[] components = GetComponents<BugActorComponent>();
            foreach (BugActorComponent component in components)
            {
                component.OnInit(this);
            }

            // TODO create an actor subscriber for scene actors.
            // TODO remove this.
            GetApp?.GetGameLogic?.AddActor(GetUniqueId, this);
        }

        /// <summary>
        /// Post initializes the actor. Should be called after Init().
        /// </summary>
        private void PostInit()
        {
            BugActorComponent[] components = GetComponents<BugActorComponent>();
            foreach (BugActorComponent component in components)
            {
                component.OnPostInit(this);
            }
        }

        /// <summary>
        /// Updates the actor by a delta of time passed.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        public void OnUpdate(float _deltaMS)
        {
            foreach (BugActorComponent component in componentsMap.Values)
            {
                component.OnUpdate(_deltaMS);
            }
        }

        public void OnFixedUpdate(float _deltaMS)
        {
            foreach (BugActorComponent component in componentsMap.Values)
            {
                component.OnFixedUpdate(_deltaMS);
            }
        }

        /// <summary>
        /// Gets a component attached to the actor.
        /// </summary>
        /// <typeparam name="T">Type of the component to get.</typeparam>
        /// <returns></returns>
        public T GetActorComponent<T>(string _id) where T : BugActorComponent
        {
            if (componentsMap.ContainsKey(_id))
            {
                return componentsMap[_id] as T;
            }

            return null;
        }

        /// <summary>
        /// Attaches a new component of the type and stores a referece in the components map with the specified id.
        /// </summary>
        /// <typeparam name="T">Type of the component to attach.</typeparam>
        /// <param name="_id">Unique id of the component.</param>
        public void AttachActorComponent<T>(string _id) where T : BugActorComponent
        {
            T temp = gameObject.AddComponent<T>();
            AttachActorComponent(_id, temp);
            // Catch duplicate component exception.
        }

        /// <summary>
        /// Attaches a the component and stores a referece in the components map with the specified id.
        /// </summary>
        /// <typeparam name="T">Type of the component to attach.</typeparam>
        /// <param name="_id">Unique id of the component.</param>
        /// <param name="_component">Reference to the component to attach.</param>
        public void AttachActorComponent<T>(string _id, T _component) where T : BugActorComponent
        {
            if (!SubscribeActorComponent(_id, _component))
            {
                Debugging.BugDebugController.LogErrorMessage($"Duplicate components: { _id }, destroying.");
                Destroy(_component);
                // Throw duplicate component exception.
            }
        }

        /// <summary>
        /// Tries to subscribes and actor component to the component map.
        /// </summary>
        /// <typeparam name="T">Specific type of the component to attach.</typeparam>
        /// <param name="_component">Component to attach.</param>
        /// <returns>True if the subscribe attempt succeded.</returns>
        private bool SubscribeActorComponent<T>(string _id, T _component) where T : BugActorComponent
        {
            if (!componentsMap.ContainsKey(_id))
            {
                componentsMap[_id] = _component;
                _component.SetOwner = uniqueId;
                return true;
            }

            // TODO throw duplicate component exception.
            return false;
        }
    }
}
