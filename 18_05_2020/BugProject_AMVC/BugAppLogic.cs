using BugProject.Actors;
using BugProject.Core.Actors;
using BugProject.Core.Physics;
using BugProject.Core.Processes;
using BugProject.Core.Views;
using System.Collections.Generic;
using UnityEngine;

namespace BugProject.Core
{
    public class BugAppLogic : BugScript
    {
        [SerializeField]
        private BugProcessController processes = null;
        [SerializeField]
        private BugPhysicsController physics = null;

        private Dictionary<string, BugActor> actorMap = null;
        private Dictionary<string, BugView> viewsMaps = null;

        public Dictionary<string, BugView> GetViewsMaps { get => viewsMaps; }
        public BugPhysicsController GetPhysics { get => physics; }

        #region Unity functions
        private void Awake()
        {
            Init();
        }

        #endregion

        /// <summary>
        /// Call to initialize the logic layer.
        /// </summary>
        public void Init()
        {
            actorMap = new Dictionary<string, BugActor>();
            viewsMaps = new Dictionary<string, BugView>();

            CatchReferences();
        }

        /// <summary>
        /// Catches all the required references.
        /// </summary>
        private void CatchReferences()
        {
            processes = GetComponent<BugProcessController>();
            physics = GetComponent<BugPhysicsController>();
        }

        /// <summary>
        /// Adds and actor to the actors map and subsecuently adds it to the physics systems if there is a physics body attached.
        /// </summary>
        /// <param name="_id">Id of the actor to add.</param>
        /// <param name="_actorReference">Reference to the actor to add.</param>
        public void AddActor(string _id, BugActor _actorReference)
        {
            if (!actorMap.ContainsKey(_id))
            {
                actorMap[_id] = _actorReference;
                PhysicsBody actorBody = _actorReference.GetActorComponent<PhysicsBody>(PhysicsBody.GetUniqueId);
                if (actorBody)
                {
                    physics.AddBodyToPhysicsWorld(_id, actorBody);
                }
            }
            else
            {
                // Throw trying to re-add actor to map exception.
            }
        }

        /// <summary>
        /// Adds a view to the views map.
        /// </summary>
        /// <param name="_id">Id of the view to add.</param>
        /// <param name="_viewReference">Reference to the view to add.</param>
        public void AddView(string _id, BugView _viewReference)
        {
            if (!viewsMaps.ContainsKey(_id))
            {
                viewsMaps[_id] = _viewReference;
            }
            else
            {
                // Throw trying to re-add view to map exception.
            }
        }

        /// <summary>
        /// Gets an actor from the actors map.
        /// </summary>
        /// <param name="_id">Id of the actor to look for.</param>
        /// <returns></returns>
        public BugActor GetActor(string _id)
        {
            if (actorMap.ContainsKey(_id))
            {
                return actorMap[_id];
            }
            else
            {
                // Throw trying to get non existen actor exception.
                return null;
            }
        }

        /// <summary>
        /// Call when the logic layer should update by a time delta.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        public void OnUpdate(float _deltaMS)
        {
            // Update views
            UpdateViews(_deltaMS);

            UpdateActors(_deltaMS);
        }

        /// <summary>
        /// Updates all the views in the views map by a time delta.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        public void UpdateViews(float _detalMS)
        {
            foreach (BugView view in viewsMaps.Values)
            {
                view.OnUpdate(_detalMS);
            }
        }

        /// <summary>
        /// Updates all the actors in the actors map by a time delta.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        private void UpdateActors(float _detalMS)
        {
            foreach (BugActor actor in actorMap.Values)
            {
                actor.OnUpdate(_detalMS);
            }
        }

        public void OnFixedUpdate(float _deltaMS)
        {
            FixUpdateActors(_deltaMS);

            physics.OnFixedUpdate(_deltaMS);
        }

        private void FixUpdateActors(float _deltaMS)
        {
            foreach (BugActor actor in actorMap.Values)
            {
                actor.OnFixedUpdate(_deltaMS);
            }
        }
    }
}
