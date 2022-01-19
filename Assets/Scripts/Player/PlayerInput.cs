using UnityEngine;

namespace Scripts.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public PlayerCore Core { get; set; }

        [SerializeField] private float _coyoteTimeThreshold = 0.1f;
        [SerializeField] private float _jumpBuffer = 0.1f;

        private float _lastJumpPressed;

        public UserInput GatherInput()
        {
            bool jump = Input.GetButtonDown("Jump");
            if (jump) {
                _lastJumpPressed = Time.time;
            }
            if (Core.Collider.Grounded) {
                // Player grounded. Check buffer
                if (_lastJumpPressed + _jumpBuffer > Time.time) {
                    jump = true;
                }
            } else if (jump) {
                // Player not grounded. Check coyote time
                jump = Core.Collider.TimeLastGrounded + _coyoteTimeThreshold > Time.time;
            }
            var input = new UserInput {
                Jump = jump,
                X = UnityEngine.Input.GetAxisRaw("Horizontal"),
                Y = UnityEngine.Input.GetAxisRaw("Vertical")
            };
            return input;
        }
    }
}