using UnityEngine;
using UnityEngine.InputSystem;

namespace KartGame.KartSystems
{

    public class KeyboardInput : BaseInput
    {
        public bool accelerateState; //// CREATE VARIABLES
        public bool brakeState;      //// CREATE VARIABLES
        public float turnValue;      //// CREATE VARIABLES

        public void AccelerateInput(InputAction.CallbackContext context) //// CREATE UNITY EVENTS
        {
            if (isOwned)
            {
                accelerateState = context.action.triggered;
            }
        }

        public void BrakeInput(InputAction.CallbackContext context)      //// CREATE UNITY EVENTS
        {
            if (isOwned)
            {
                brakeState = context.action.triggered;
            }
        }
        public void TurnInput(InputAction.CallbackContext context)
        {
            if (isOwned)
            {
                turnValue = context.action.ReadValue<Vector2>().x;       //// CREATE UNITY EVENTS
                //Debug.Log(context.action.ReadValue<Vector2>().x);
            }
        }

        public override InputData GenerateInput()
        {
            return new InputData
            {
                Accelerate = accelerateState,
                Brake = brakeState,
                TurnInput = turnValue
            };
        }

    }
}
