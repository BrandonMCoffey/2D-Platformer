using UnityEngine;

namespace Scripts.Player
{
    public class PlayerPhysics : MonoBehaviour
    {
        public PlayerCore Core { get; set; }

        [Header("Walking")]
        [SerializeField] private float _acceleration = 90f;
        [SerializeField] private float _moveClamp = 13f;
        [SerializeField] private float _deceleration = 60f;
        [SerializeField] private float _apexBonus = 2f;

        [Header("Jumping")]
        [SerializeField] private float _jumpHeight = 30f;
        [SerializeField] private float _jumpApexThreshold = 10f;
        [SerializeField] private float _jumpEndEarlyGravityModifier = 3f;

        [Header("Gravity")]
        [SerializeField] private float _fallClamp = -40f;
        [SerializeField] private float _minFallSpeed = 80f;
        [SerializeField] private float _maxFallSpeed = 120f;

        [Header("Move")]
        [SerializeField, Tooltip("Increased accuracy, reduced performance.")]
        private int _freeColliderIterations = 10;

        public Vector3 Velocity { get; private set; }
        public Vector3 RawMovement { get; private set; }

        private Vector3 _lastPosition;
        private float _currentHorzSpeed;
        private float _currentVertSpeed;
        private float _apexPoint;
        private float _fallSpeed;

        public void CheckVelocity() {
            Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;
        }

        public void ProcessInput(UserInput input, CollisionSet collisions) {
            CalculateWalk(input.X, collisions);
            CalculateJumpApex();
            CalculateGravity(input.HoldJump);
            CalculateJump(input.Jump, collisions);
            CalculateDash(input.Dash);
        }

        private void CalculateWalk(float horz, CollisionSet collisions) {
            if (horz != 0) {
                _currentHorzSpeed += horz * _acceleration * Time.deltaTime;
                _currentHorzSpeed = Mathf.Clamp(_currentHorzSpeed, -_moveClamp, _moveClamp);

                var apexBonus = Mathf.Sign(horz) * _apexBonus * _apexPoint;
                _currentHorzSpeed += apexBonus * Time.deltaTime;
            }
            else {
                // No input. Let's slow the character down
                _currentHorzSpeed = Mathf.MoveTowards(_currentHorzSpeed, 0, _deceleration * Time.deltaTime);
            }

            if (_currentHorzSpeed > 0 && collisions.Right || _currentHorzSpeed < 0 && collisions.Left) {
                // Don't walk through walls
                _currentHorzSpeed = 0;
            }
        }

        private void CalculateJumpApex() {
            if (!Core.Collider.Grounded) {
                // Gets stronger the closer to the top of the jump
                _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
                _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
            }
            else {
                _apexPoint = 0;
            }
        }

        private void CalculateGravity(bool holdJump) {
            if (Core.Collider.Grounded) {
                // Move out of the ground
                if (_currentVertSpeed < 0) _currentVertSpeed = 0;
            }
            else {
                // Add downward force while ascending if we ended the jump early
                var fallSpeed = !holdJump && _currentVertSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

                // Fall
                _currentVertSpeed -= fallSpeed * Time.deltaTime;

                // Clamp
                if (_currentVertSpeed < _fallClamp) _currentVertSpeed = _fallClamp;
            }
        }

        private void CalculateJump(bool jump, CollisionSet collisions) {
            if (jump) {
                _currentVertSpeed = _jumpHeight;
            }

            if (collisions.Up && _currentVertSpeed > 0) {
                _currentVertSpeed = 0;
            }
        }

        private void CalculateDash(bool dash) {
        }

        public void Move() {
            var pos = transform.position;
            RawMovement = new Vector3(_currentHorzSpeed, _currentVertSpeed);
            var move = RawMovement * Time.deltaTime;
            var furthestPoint = (Vector2)(pos + move);

            var offset = Core.Collider.Offset;
            var size = Core.Collider.Size;
            var ground = Core.Collider.Ground;

            // check furthest movement. If nothing hit, move and don't do extra checks
            var hit = Physics2D.OverlapBox(furthestPoint + offset, size, 0, ground);
            if (!hit) {
                transform.position += move;
                return;
            }

            // otherwise increment away from current pos; see what closest position we can move to
            var positionToMoveTo = transform.position;
            for (int i = 1; i < _freeColliderIterations; i++) {
                // increment to check all but furthestPoint - we did that already
                var t = (float)i / _freeColliderIterations;
                var posToTry = Vector2.Lerp(pos, furthestPoint, t);

                if (Physics2D.OverlapBox(posToTry + offset, size, 0, ground)) {
                    transform.position = positionToMoveTo;

                    // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                    if (i == 1) {
                        if (_currentVertSpeed < 0) {
                            _currentVertSpeed = 0;
                        }
                        var dir = transform.position - hit.transform.position;
                        transform.position += dir.normalized * move.magnitude;
                    }

                    return;
                }

                positionToMoveTo = posToTry;
            }
        }
    }
}