using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugProject.Core.Processes
{
    public class BugProcessController : MonoBehaviour
    {
        private List<BugProcess> processList;

        public int GetProcessCount { get => processList.Count; }

        /// <summary>
        /// Attaches process to the list of running processes.
        /// </summary>
        /// <param name="_processToAttach">Reference to the process to attach.</param>
        public void AttachProcess(BugProcess _processToAttach)
        {
            processList.Add(_processToAttach);
        }

        /// <summary>
        /// Aborts all the running processes.
        /// </summary>
        public void AbortAllProcesses()
        {
            foreach (BugProcess process in processList)
            {
                process.OnAbort();
            }
        }

        /// <summary>
        /// Updates all the running process by a give time delta.
        /// </summary>
        /// <param name="_deltaMS">Time delta in milliseconds.</param>
        private void UpdateProcesses(float _deltaMS)
        {
            List<BugProcess>.Enumerator iterator = processList.GetEnumerator();
            while (iterator.MoveNext())
            {
                BugProcess currentProcess = iterator.Current;

                if (currentProcess.GetCurrentState.Equals(BugProcessState.UnInitialized))
                {
                    currentProcess.OnInit();
                }

                if (currentProcess.GetCurrentState.Equals(BugProcessState.Running))
                {
                    currentProcess.OnUpdate(_deltaMS);
                }

                if (currentProcess.IsDead())
                {
                    switch (currentProcess.GetCurrentState)
                    {
                        case BugProcessState.Succeeded:
                            {
                                currentProcess.OnSucces();
                                BugProcess child = currentProcess.RemoveChild();
                                if (child != null)
                                {
                                    AttachProcess(child);
                                }
                                break;
                            }
                        case BugProcessState.Failed:
                            {
                                currentProcess.OnFail();
                                break;
                            }
                        case BugProcessState.Aborted:
                            {
                                currentProcess.OnSucces();
                                break;
                            }
                    }
                    // Return to process pool.
                    currentProcess.Remove();
                    processList.Remove(currentProcess);
                }
            }
        }

        /// <summary>
        /// Clears the list of running processes.
        /// </summary>
        private void ClearAllProcesses()
        {
            processList.Clear();
        }
    }
}
