using System;
using System.Collections.Generic;
using Scripts.TarodevController;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerCollider : MonoBehaviour
    {
        public PlayerCore Core { get; set; }

        [Header("Character Bounds")]
        [SerializeField] private Vector2 _offset = new Vector2(0, 0);
        [SerializeField] private Vector2 _size = new Vector2(1f, 2f);
        [SerializeField] private float _jumpCornerBuffer = 0.1f;
        [SerializeField] private float _groundCornerBuffer = 0.1f;
        [SerializeField] private float _headCornerBuffer = 0.1f;
        [SerializeField] private float _stairsCornerBuffer = 0.1f;
        [SerializeField] private float _rayDistance = 0.1f;
        [SerializeField] private int _extraRays = 1;

        private Vector3 Center => transform.position + (Vector3) _offset;
        public bool Grounded { get; private set; }
        public float TimeLastGrounded { get; private set; }

        private List<Ray> _rays = new List<Ray>(12);

        private void OnValidate()
        {
            _jumpCornerBuffer = Mathf.Clamp(_jumpCornerBuffer, 0, _size.x - _groundCornerBuffer);
            _groundCornerBuffer = Mathf.Clamp(_groundCornerBuffer, 0, _size.x - _jumpCornerBuffer);
            _headCornerBuffer = Mathf.Clamp(_headCornerBuffer, 0, _size.y - _stairsCornerBuffer);
            _stairsCornerBuffer = Mathf.Clamp(_stairsCornerBuffer, 0, _size.y - _headCornerBuffer);
        }

        private void OnDrawGizmos()
        {
            // Bounds
            Gizmos.color = new Color(0.49f, 0.79f, 0.47f);
            Gizmos.DrawWireCube(Center, _size);

            // Rays
            Gizmos.color = Color.red;
            if (!Application.isPlaying) {
                CalculateRayRanged();
            }
            foreach (var ray in _rays) {
                Gizmos.DrawRay(ray.origin, ray.direction * _rayDistance);
            }
        }

        private void CalculateRayRanged()
        {
            _rays.Clear();
            var bounds = new Bounds(Center, _size);
            // Top Rays
            foreach (var value in GetStepValues(bounds.min.x + _jumpCornerBuffer, bounds.max.x - _jumpCornerBuffer, 1 + _extraRays)) {
                _rays.Add(new Ray(new Vector3(value, bounds.max.y), Vector3.up));
            }
            // Left Rays
            foreach (var value in GetStepValues(bounds.min.y + _stairsCornerBuffer, bounds.max.y - _headCornerBuffer, 1 + _extraRays)) {
                _rays.Add(new Ray(new Vector3(bounds.min.x, value), Vector3.left));
            }
            // Right Rays
            foreach (var value in GetStepValues(bounds.min.y + _stairsCornerBuffer, bounds.max.y - _headCornerBuffer, 1 + _extraRays)) {
                _rays.Add(new Ray(new Vector3(bounds.max.x, value), Vector3.right));
            }
            // Bottom Rays
            foreach (var value in GetStepValues(bounds.min.x + _groundCornerBuffer, bounds.max.x - _groundCornerBuffer, 1 + _extraRays)) {
                _rays.Add(new Ray(new Vector3(value, bounds.min.y), Vector3.down));
            }
        }

        private static IEnumerable<float> GetStepValues(float min, float max, int iterations)
        {
            var list = new List<float>(iterations);
            float offset = max - min;
            for (float value = 0; value <= offset; value += offset / iterations) {
                list.Add(min + value);
            }
            return list;
        }

        public void CheckCollider()
        {
            CalculateRayRanged();
            Grounded = true;
            if (Grounded) {
                TimeLastGrounded = Time.time;
            }
        }
    }
}