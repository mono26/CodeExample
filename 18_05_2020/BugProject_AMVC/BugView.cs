using BugProject.Core.Actors;
using BugProject.Core.Input;
using System;
using UnityEngine;

namespace BugProject.Core.Views
{
    public abstract class BugView : BugScript
    {
        [SerializeField]
        private string uniqueId = null;
        [SerializeField]
        private string ownerId = null;

        public string GetOwnerId { get => ownerId; }
        public string GetUniqueId { get => uniqueId; }
        public string SetOwnerId { set => ownerId = value; }
        public string SetUniqueId { set => uniqueId = value; }

        #region Unity functions.
        private void Start()
        {
            OnInit();
        }
        #endregion

        /// <summary>
        /// Call to initialize the component.
        /// </summary>
        public virtual void OnInit()
        {
            SetUniqueId = Guid.NewGuid().ToString();

            SetOwnerId = GetComponent<BugActor>().GetUniqueId;
            GetApp.GetGameLogic.AddView(GetUniqueId, this);
        }

        /// <summary>
        /// Call to update the view by a give delta.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        public abstract void OnUpdate(float _deltaMS);

        /// <summary>
        /// Call when the app receives the platform input.
        /// </summary>
        /// <param name="_input">Captured platform input.</param>
        public abstract void OnInput(IPlatformInput _input);
    }
}
