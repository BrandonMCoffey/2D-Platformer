using UnityEngine;

namespace Scripts.Player
{
    public class PlayerCore : MonoBehaviour
    {
        [SerializeField] private PlayerInput _input = null;
        [SerializeField] private PlayerCollider _collider = null;
        [SerializeField] private PlayerPhysics _physics = null;

        public PlayerCollider Collider => _collider;

        private bool _disabled;

        private void Awake() {
            if (_input == null || _collider == null || _physics == null) {
                _disabled = true;
            }
            else {
                _input.Core = this;
                _collider.Core = this;
                _physics.Core = this;

                // Delay Start
                _disabled = true;
                Invoke(nameof(Activate), 0.4f);
            }
        }

        private void Activate() {
            _disabled = false;
        }

        private void Update() {
            if (_disabled) return;
            _physics.CheckVelocity();
            var collisions = _collider.CheckCollider();
            var input = _input.GatherInput();
            _physics.ProcessInput(input, collisions);
            _physics.Move();
        }
    }
}