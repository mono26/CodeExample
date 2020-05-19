using BugProject.Core.EventSystem;
using BugProject.Core.Input;
using BugProject.Input;
using System.Collections.Generic;
using UnityEngine;

namespace BugProject.Core
{
    public class BugApp : MonoBehaviour
    {
        private static BugApp appInstance = null;
        public static BugApp GetAppInstance { get => appInstance; }

        [SerializeField]
        private BugEventController eventSystem = null;
        [SerializeField]
        private BugAppLogic gameLogic = null;

        public const string INPUT_HORIZONTAL = "Horizontal";
        public const string INPUT_VERTICAL = "Vertical";
        public const string INPUT_JUMP = "Jump";
        public const string INPUT_MELEE = "Fire1";
        public const string INPUT_FIRE = "Fire2";
        public const string INPUT_MOUSE_X = "Mouse X";
        public const string INPUT_MOUSE_Y = "Mouse Y";

        public BugEventController GetEventSystem { get => eventSystem; }
        public BugAppLogic GetGameLogic { get => gameLogic; }

        private void Awake()
        {
            if (appInstance == null)
            {
                appInstance = this;
            }
            else
            {
                Destroy(this);
            }

            CatchSystemReferences();
        }

        private void CatchSystemReferences()
        {
            eventSystem = GetComponent<BugEventController>();
            gameLogic = GetComponent<BugAppLogic>();
        }

        private void Update()
        {
            // Update event manager.

            if (GetGameLogic.GetViewsMaps != null && GetGameLogic.GetViewsMaps.Count > 0)
            {
                IPlatformInput currentInput = CatchInput();
                foreach (var view in GetGameLogic.GetViewsMaps.Values)
                {
                    view.OnInput(currentInput);
                }
            }

            eventSystem.OnUpdate();

            gameLogic.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            gameLogic.OnFixedUpdate(Time.fixedDeltaTime);
        }

        private IPlatformInput CatchInput()
        {
            IPlatformInput input;
#if UNITY_EDITOR
            if (UnityEngine.Input.GetJoystickNames().Length.Equals(0))
            {
                input = CatchPCInput();
            }
            else
            {
                input = CatchControllerInput();
            }
#endif
            return input;
        }

#if UNITY_EDITOR
        private PCInput CatchPCInput()
        {
            PCInput input = new PCInput();
            input.inputMap = new Dictionary<KeyCode, float>();
            // Vertical movement.
            if (UnityEngine.Input.GetKey(KeyCode.W))
            {
                // 1.0f equal this key is pressed.
                input.inputMap[KeyCode.W] = 1.0f;
            }
            else if (UnityEngine.Input.GetKey(KeyCode.S))
            {
                input.inputMap[KeyCode.S] = 1.0f;
            }

            // Horizontal movement.
            if (UnityEngine.Input.GetKey(KeyCode.D))
            {
                input.inputMap[KeyCode.D] = 1.0f;
            }
            else if (UnityEngine.Input.GetKey(KeyCode.A))
            {
                input.inputMap[KeyCode.A] = 1.0f;
            }

            // Jump input
            if (UnityEngine.Input.GetKey(KeyCode.Space))
            {
                input.inputMap[KeyCode.Space] = 1.0f;
            }

            //// Melee or gun input.
            //if (UnityEngine.Input.GetMouseButton(0))
            //{
            //    input.inputMap[KeyCode.Mouse0] = 1.0f;
            //}
            //else if (UnityEngine.Input.GetKeyDown(KeyCode.F))
            //{
            //    input.inputMap[KeyCode.F] = 1.0f;
            //}

            //// Mouse position info.
            //input.mousePosition = UnityEngine.Input.mousePosition;

            return input;
        }
        private ControllerInput CatchControllerInput()
        {
            ControllerInput input = new ControllerInput();
            input.inputMap = new Dictionary<string, float>();
            // Vertical movement.
            input.inputMap[INPUT_VERTICAL] = UnityEngine.Input.GetAxis(INPUT_VERTICAL);

            // Horizontal movement.
            input.inputMap[INPUT_HORIZONTAL] = UnityEngine.Input.GetAxis(INPUT_HORIZONTAL);

            // Jump input
            if (UnityEngine.Input.GetButton(INPUT_JUMP))
            {
                input.inputMap[INPUT_JUMP] = 1.0f;
            }

            // Melee or gun input.
            if (UnityEngine.Input.GetButton(INPUT_FIRE))
            {
                input.inputMap[INPUT_FIRE] = 1.0f;
            }
            else if (UnityEngine.Input.GetButtonDown(INPUT_MELEE))
            {
                input.inputMap[INPUT_MELEE] = 1.0f;
            }

            // Aim position info.
            input.inputMap[INPUT_MOUSE_X] = UnityEngine.Input.GetAxis(INPUT_MOUSE_X);
            input.inputMap[INPUT_MOUSE_Y] = UnityEngine.Input.GetAxis(INPUT_MOUSE_Y);

            return input;
        }
#endif
    }
}
