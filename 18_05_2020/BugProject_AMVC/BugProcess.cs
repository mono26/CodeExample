using UnityEngine;

namespace BugProject.Core.Processes
{
    public class BugProcess : MonoBehaviour
    {
        private string uniqueId;
        private BugProcessState currentState;
        private BugProcess childProcess;

        #region Getters and Setters
        public BugProcessState GetCurrentState { get => currentState; }
        public string GetUniqueId { get => uniqueId; }
        private BugProcessState SetCurrentState { set => currentState = value; }
        #endregion

        public BugProcess()
        {
            uniqueId = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Call to initialize the process.
        /// </summary>
        public void OnInit()
        {
            SetCurrentState = BugProcessState.Running;
        }

        #region Process manager calls.
        /// <summary>
        /// Call to update the process by time delta.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        public virtual void OnUpdate(float _deltaMS) { }

        /// <summary>
        /// Call when the process is in aborted state.
        /// </summary>
        public virtual void OnAbort() { }

        /// <summary>
        /// Call when the process is in success state.
        /// </summary>
        public virtual void OnSucces() { }

        /// <summary>
        /// Call when the process is in failed state.
        /// </summary>
        public virtual void OnFail() { }
        #endregion

        /// <summary>
        /// Call to set the current state to aborted.
        /// </summary>
        public void Abort() { SetCurrentState = BugProcessState.Aborted; }

        /// <summary>
        /// Call to set the current state to failed.
        /// </summary>
        public void Fail() { SetCurrentState = BugProcessState.Failed; }

        /// <summary>
        /// Call to set the current state to paused.
        /// </summary>
        public void Pause() { SetCurrentState = BugProcessState.Paused; }

        /// <summary>
        /// Call to set the current state to running.
        /// </summary>
        public void UnPause() { SetCurrentState = BugProcessState.Running; }

        /// <summary>
        /// Call to set the current state to succeeded.
        /// </summary>
        public void Success() { SetCurrentState = BugProcessState.Succeeded; }

        /// <summary>
        /// Call to set the current state to removed.
        /// </summary>
        public void Remove() { SetCurrentState = BugProcessState.Removed; }

        #region Conditonals
        /// <summary>
        /// Checks if the process is a state that tells this process is not done.
        /// </summary>
        /// <returns>Returns true if is still running or paused.</returns>
        public bool IsAlive()
        {
            return (currentState.Equals(BugProcessState.Running) || currentState.Equals(BugProcessState.Paused));
        }

        /// <summary>
        /// Checks if the process is in a state that tells this proccess is done.
        /// </summary>
        /// <returns>Return true if the proccess succeeded, failed or aborted.</returns>
        public bool IsDead()
        {
            return (currentState.Equals(BugProcessState.Succeeded) || currentState.Equals(BugProcessState.Failed) || currentState.Equals(BugProcessState.Aborted));
        }

        /// <summary>
        /// Checks if the process got removed from the queue.
        /// </summary>
        /// <returns>Return true if the proccess was removed.</returns>
        public bool IsRemoved()
        {
            return (currentState.Equals(BugProcessState.Removed));
        }
        #endregion

        #region Child management
        /// <summary>
        /// Attaches a child process. A child process will run after this one.
        /// </summary>
        /// <param name="_childProcess">Process to attach as child.</param>
        public void AttachChild(BugProcess _childProcess)
        {
            childProcess = _childProcess;
        }

        /// <summary>
        /// Removes the child process.
        /// </summary>
        /// <returns>Returns a reference to the child. The child is removed.</returns>
        public BugProcess RemoveChild()
        {
            childProcess = null;
            return childProcess;
        }

        /// <summary>
        /// Take a look at the child.
        /// </summary>
        /// <returns>Returns a reference to the child. The child is not removed.</returns>
        public BugProcess PeekChild()
        {
            return childProcess;
        }
        #endregion
    }
}

