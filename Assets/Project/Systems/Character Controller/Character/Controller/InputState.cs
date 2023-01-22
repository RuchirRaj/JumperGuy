using System;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [Serializable]
    public class InputState
    {
        //Properties
        [field: SerializeField] public Vector2 Look { get; private set; }
        [field: SerializeField] public bool JumpPressed { get; private set; }
        [field: SerializeField] public bool CrouchPressed { get; private set; }
        [field: SerializeField] public bool RunPressed { get; private set; }
        [field: SerializeField] public bool DashPressed { get; private set; }
        [field: SerializeField] public Vector2 Move { get; private set; }
        [field: SerializeField] public Quaternion ForwardRotation { get; private set; }
        
        //Events
        public Action<bool> onCrouchEvent;
        public Action<bool> onRunEvent;
        public Action<bool> onJumpEvent;
        public Action<bool> onDashEvent;
        
        //Input Events
        public (bool valid, bool value) onJumpTuple;
        public (bool valid, bool value) onRunTuple;
        public (bool valid, bool value) onDashTuple;
        public (bool valid, bool value) onCrouchTuple;

        //Update Functions
        public void INPUT_Move(Vector2 value) => Move = value;
        public void INPUT_Look(Vector2 value) => Look = value;

        public void CopyValue(InputState source)
        {
            Look = source.Look;
            JumpPressed = source.JumpPressed;
            CrouchPressed = source.CrouchPressed;
            RunPressed = source.RunPressed;
            DashPressed = source.DashPressed;
            Move = source.Move;
            ForwardRotation = source.ForwardRotation;
            
            onJumpTuple = source.onJumpTuple;
            onRunTuple = source.onRunTuple;
            onDashTuple = source.onDashTuple;
            onCrouchTuple = source.onCrouchTuple;
        }
        
        //TODO Add events for start and cancel
        public void INPUT_Jump(bool value)
        {
            if(value != JumpPressed)
            {
                onJumpTuple = (true, value);
                onJumpEvent?.Invoke(value);
            }
            JumpPressed = value;
        }

        public void INPUT_Dash(bool value)
        {
            if(value != DashPressed)
            {
                onDashTuple = (true, value);
                onDashEvent?.Invoke(value);
            }
            DashPressed = value;
        }

        public void INPUT_Run(bool value)
        {
            if(value != RunPressed)
            {
                onRunTuple = (true, value);
                onRunEvent?.Invoke(value);
            }

            RunPressed = value;
        }

        public void INPUT_Crouch(bool value)
        {
            if(value != CrouchPressed)
            {
                onCrouchTuple = (true, value);
                onCrouchEvent?.Invoke(value);
            }

            CrouchPressed = value;
        }

        public void INPUT_ForwardRotation(Quaternion value)
        {
            ForwardRotation = value;
        }
        
        //Reset 
        public void ResetState()
        {
            onJumpTuple = (false, false);
            onRunTuple = (false, false);
            onDashTuple = (false, false);
            onCrouchTuple = (false, false);
        }
    }
}