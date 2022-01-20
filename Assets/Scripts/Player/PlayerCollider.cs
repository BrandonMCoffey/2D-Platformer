using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Scripts.Player
{
    public class PlayerCollider : MonoBehaviour
    {
        public PlayerCore Core { get; set; }

        [Header("Character Bounds")]
        [SerializeField] private Vector2 _offset = new Vector2(0, 0);
        [SerializeField] private Vector2 _size = new Vector2(1f, 1.3f);
        [SerializeField] private float _jumpCornerBuffer = 0.15f;
        [SerializeField] private float _groundCornerBuffer = 0.05f;
        [SerializeField] private float _headCornerBuffer;
        [SerializeField] private float _stairsCornerBuffer = 0.2f;
        [SerializeField] private float _rayDistance = 0.1f;
        [SerializeField] private int _extraRays = 1;

        [Header("Layers")]
        [SerializeField] private LayerMask _groundLayer = 1;

        public bool LandingThisFrame { get; private set; }
        public bool Grounded { get; private set; }
        public float TimeLastGrounded { get; private set; }
        public Vector2 Offset => _offset;
        public Vector2 Size => _size;
        public LayerMask Ground => _groundLayer;

        private RaySet _raysUp, _raysLeft, _raysRight, _raysDown;

        private Vector3 Center => transform.position + (Vector3)_offset;

#if UNITY_EDITOR
        private void OnValidate() {
            float halfX = _size.x * 0.5f - 0.01f;
            float halfY = _size.y * 0.5f - 0.01f;

            _jumpCornerBuffer = Mathf.Clamp(_jumpCornerBuffer, 0, halfX);
            _groundCornerBuffer = Mathf.Clamp(_groundCornerBuffer, 0, halfX);
            _headCornerBuffer = Mathf.Clamp(_headCornerBuffer, 0, halfY);
            _stairsCornerBuffer = Mathf.Clamp(_stairsCornerBuffer, 0, halfY);
            _rayDistance = Mathf.Clamp01(_rayDistance);
            _extraRays = Mathf.Clamp(_extraRays, 0, 50);
        }

        private void OnDrawGizmos() {
            // Bounds
            Gizmos.color = new Color(0.49f, 0.79f, 0.47f);
            Gizmos.DrawWireCube(Center, _size);

            // Rays
            Gizmos.color = Color.red;
            CalculateRays();
            DrawRays(_raysUp);
            DrawRays(_raysLeft);
            DrawRays(_raysRight);
            DrawRays(_raysDown);

            void DrawRays(RaySet set) {
                var origins = set.GetOrigins(_extraRays);
                foreach (var origin in origins) {
                    Gizmos.DrawRay(origin, set.Dir * _rayDistance);
                }
            }
        }
#endif

        public CollisionSet CheckCollider() {
            CalculateRays();

            LandingThisFrame = false;
            var groundedCheck = CheckCollisions(_raysDown);
            if (Grounded && !groundedCheck) {
                TimeLastGrounded = Time.time;
            }
            else if (!Grounded && groundedCheck) {
                LandingThisFrame = true;
            }
            Grounded = groundedCheck;

            return new CollisionSet {
                Up = CheckCollisions(_raysUp),
                Left = CheckCollisions(_raysLeft),
                Right = CheckCollisions(_raysRight),
                Down = groundedCheck
            };

            bool CheckCollisions(RaySet set) {
                var origins = set.GetOrigins(_extraRays);
                return origins.Any(origin => Physics2D.Raycast(origin, set.Dir, _rayDistance, _groundLayer));
            }
        }

        private void CalculateRays() {
            var bounds = new Bounds(Center, _size);
            // Top Rays
            _raysUp = new RaySet {
                X = true,
                Start = bounds.min.x + _jumpCornerBuffer,
                End = bounds.max.x - _jumpCornerBuffer,
                Other = bounds.max.y,
                Dir = Vector2.up
            };
            // Left Rays
            _raysLeft = new RaySet {
                X = false,
                Start = bounds.min.y + _stairsCornerBuffer,
                End = bounds.max.y - _headCornerBuffer,
                Other = bounds.min.x,
                Dir = Vector2.left
            };
            // Right Rays
            _raysRight = new RaySet {
                X = false,
                Start = bounds.min.y + _stairsCornerBuffer,
                End = bounds.max.y - _headCornerBuffer,
                Other = bounds.max.x,
                Dir = Vector2.right
            };
            // Bottom Rays
            _raysDown = new RaySet {
                X = true,
                Start = bounds.min.x + _groundCornerBuffer,
                End = bounds.max.x - _groundCornerBuffer,
                Other = bounds.min.y,
                Dir = Vector2.down
            };
        }
    }
}