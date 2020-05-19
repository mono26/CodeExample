using BugProject.Core;
using BugProject.Core.Input;
using BugProject.Core.Views;
using BugProject.Input;
using System.Collections.Generic;
using UnityEngine;

namespace BugProject.Player
{
    public class PlayerView : BugView
    {
        public override void OnInput(IPlatformInput _input)
        {
            Dictionary<BugInputCommands, float> commandValueMap = new Dictionary<BugInputCommands, float>();
#if UNITY_EDITOR || UNITY_STANDALONE
            // Temporal check for joysticks.
            if (UnityEngine.Input.GetJoystickNames().Length.Equals(0))
            {
                PCInput pc = (PCInput)_input;
                commandValueMap = InterpretePCInput(pc);
            }
            else
            {
                ControllerInput controller = (ControllerInput)_input;
                commandValueMap = InterpreteControllerInput(controller);
            }

#endif
            GetApp.GetEventSystem.PushEvent(BugViewEvents.INPUT_INTERPRETED, new BugViewInputInterpretedEventArgs()
            {
                actorId = GetOwnerId,
                viewId = GetUniqueId,
                interpretedInput = commandValueMap
            });
        }

        private Dictionary<BugInputCommands, float> InterpretePCInput(PCInput _input)
        {
            Dictionary<BugInputCommands, float> commandValueMap = new Dictionary<BugInputCommands, float>();
            // Vertical movement.
            if (_input.inputMap.ContainsKey(KeyCode.W))
            {
                // TODO change the values to get it from the key.
                commandValueMap[BugInputCommands.MoveY] = 1.0f;
            }
            else if (_input.inputMap.ContainsKey(KeyCode.S))
            {
                commandValueMap[BugInputCommands.MoveY] = -1.0f;
            }

            // Horizontal movement.
            if (_input.inputMap.ContainsKey(KeyCode.D))
            {
                commandValueMap[BugInputCommands.MoveX] = 1.0f;
            }
            else if (_input.inputMap.ContainsKey(KeyCode.A))
            {
                commandValueMap[BugInputCommands.MoveX] = -1.0f;
            }

            // Jump input
            if (_input.inputMap.ContainsKey(KeyCode.Space))
            {
                commandValueMap[BugInputCommands.Jump] = 1.0f;
            }

            // Melee or fire input.
            if (_input.inputMap.ContainsKey(KeyCode.Mouse0))
            {
                commandValueMap[BugInputCommands.Fire] = 1.0f;
            }
            else if (_input.inputMap.ContainsKey(KeyCode.F))
            {
                commandValueMap[BugInputCommands.Melee] = 1.0f;
            }

            commandValueMap[BugInputCommands.AimX] = _input.mousePosition.x;
            commandValueMap[BugInputCommands.AimY] = _input.mousePosition.y;

            return commandValueMap;
        }

        private Dictionary<BugInputCommands, float> InterpreteControllerInput(ControllerInput _input)
        {
            Dictionary<BugInputCommands, float> commandValueMap = new Dictionary<BugInputCommands, float>();
            // Vertical movement.
            commandValueMap[BugInputCommands.MoveY] = _input.inputMap[BugApp.INPUT_VERTICAL];

            // Horizontal movement.
            commandValueMap[BugInputCommands.MoveX] = _input.inputMap[BugApp.INPUT_HORIZONTAL];

            // Jump input
            if (_input.inputMap.ContainsKey(BugApp.INPUT_JUMP))
            {
                // TODO change the values to get it from the key.
                commandValueMap[BugInputCommands.Jump] = 1.0f;
            }

            // Melee or fire input.
            if (_input.inputMap.ContainsKey(BugApp.INPUT_FIRE))
            {
                commandValueMap[BugInputCommands.Fire] = 1.0f;
            }
            else if (_input.inputMap.ContainsKey(BugApp.INPUT_MELEE))
            {
                commandValueMap[BugInputCommands.Melee] = 1.0f;
            }

            commandValueMap[BugInputCommands.AimX] = _input.inputMap[BugApp.INPUT_MOUSE_X];
            commandValueMap[BugInputCommands.AimY] = _input.inputMap[BugApp.INPUT_MOUSE_Y];

            return commandValueMap;
        }

        public override void OnUpdate(float _deltaMS)
        {
            // Render player ui.
        }
    }
}
