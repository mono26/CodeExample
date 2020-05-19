using UnityEngine;

namespace BugProject.Core.Actors
{
    /// <summary>
    /// Base class for all component.
    /// </summary>
    public abstract class BugActorComponent : BugScript
    {
        [SerializeField]
        private string ownerId = null;

        /// <summary>
        /// Gets the unique id for this actor component implementation.
        /// </summary>
        public static string GetUniqueId;

        /// <summary>
        /// Sets the id of the actor that owns this component.
        /// </summary>
        public string SetOwner { set => ownerId = value; }
        public string GetOwnerId { get => ownerId; }

        /// <summary>
        /// Catch the require refernces by the component.
        /// </summary>
        /// <param name="_actor">Actor to catch references from.</param>
        protected abstract void CatchReferences(BugActor _actor);

        /// <summary>
        /// Attaches this component to the actor.
        /// </summary>
        /// <param name="_actor">Actor to attach the component to.</param>
        protected abstract void AttachToActor(BugActor _actor);

        /// <summary>
        /// Call to initialize the component.
        /// </summary>
        /// <param name="_actor">Owner of the component.</param>
        public virtual void OnInit(BugActor _actor)
        {
            AttachToActor(_actor);
        }

        /// <summary>
        /// Call after initialization is done.
        /// </summary>
        /// <param name="_actor"></param>
        public virtual void OnPostInit(BugActor _actor)
        {
            CatchReferences(_actor);
        }

        /// <summary>
        /// Call to update the component.
        /// </summary>
        /// <param name="_deltaMS">Delta to apply.</param>
        public virtual void OnUpdate(float _deltaMS) { }

        /// <summary>
        /// Call to fix update the component. Usually done by a physics tick.
        /// </summary>
        /// <param name="_deltaMS">Delta to apply.</param>
        public virtual void OnFixedUpdate(float _deltaMS) { }
    }
}
