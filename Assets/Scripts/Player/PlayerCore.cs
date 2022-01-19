using UnityEngine;

namespace Scripts.Player
{
    public class PlayerCore : MonoBehaviour
    {
        [SerializeField] private PlayerInput _input;
        [SerializeField] private PlayerCollider _collider;
        [SerializeField] private PlayerPhysics _physics;

        public PlayerCollider Collider => _collider;

        private bool _missingReferences;

        private void Awake()
        {
            if (_input == null || _collider == null || _physics == null) {
                _missingReferences = true;
            } else {
                _input.Core = this;
                _collider.Core = this;
                _physics.Core = this;
            }
        }

        private void Update()
        {
            if (_missingReferences) return;
            _collider.CheckCollider();
            var input = _input.GatherInput();
        }
    }
}