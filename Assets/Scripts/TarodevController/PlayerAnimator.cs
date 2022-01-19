using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.TarodevController
{
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");

        [Header("Animations")]
        [SerializeField] private Animator _anim = null;
        [SerializeField] private float _maxTilt = .1f;
        [SerializeField] private float _tiltSpeed = 1;
        [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2;
        [SerializeField] private LayerMask _groundMask = 0;
        [Header("Audio")]
        [SerializeField] private AudioSource _source = null;
        [SerializeField] private List<AudioClip> _footsteps = new List<AudioClip>();
        [Header("Particles")]
        [SerializeField] private float _maxParticleFallSpeed = -40;
        [SerializeField] private ParticleSystem _jumpParticles = null;
        [SerializeField] private ParticleSystem _launchParticles = null;
        [SerializeField] private ParticleSystem _moveParticles = null;
        [SerializeField] private ParticleSystem _landParticles = null;

        private IPlayerController _player;
        private bool _playerGrounded;
        private ParticleSystem.MinMaxGradient _currentGradient;
        private Vector2 _movement;

        private void Awake()
        {
            _player = GetComponentInParent<IPlayerController>();
        }

        private void Update()
        {
            if (_player == null) return;

            // Flip the sprite
            if (_player.Input.X != 0) transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1);

            // Lean while running
            var targetRotVector = new Vector3(0, 0, Mathf.Lerp(-_maxTilt, _maxTilt, Mathf.InverseLerp(-1, 1, _player.Input.X)));
            _anim.transform.rotation = Quaternion.RotateTowards(_anim.transform.rotation, Quaternion.Euler(targetRotVector), _tiltSpeed * Time.deltaTime);

            // Speed up idle while running
            _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, Mathf.Abs(_player.Input.X)));

            // Splat
            if (_player.LandingThisFrame) {
                _anim.SetTrigger(GroundedKey);
                _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Count)]);
            }

            // Jump effects
            if (_player.JumpingThisFrame) {
                _anim.SetTrigger(JumpKey);
                _anim.ResetTrigger(GroundedKey);

                // Only play particles when grounded (avoid coyote)
                if (_player.Grounded) {
                    SetColor(_jumpParticles);
                    SetColor(_launchParticles);
                    _jumpParticles.Play();
                }
            }

            // Play landing effects and begin ground movement effects
            if (!_playerGrounded && _player.Grounded) {
                _playerGrounded = true;
                _moveParticles.Play();
                _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, _maxParticleFallSpeed, _movement.y);
                SetColor(_landParticles);
                _landParticles.Play();
            } else if (_playerGrounded && !_player.Grounded) {
                _playerGrounded = false;
                _moveParticles.Stop();
            }

            // Detect ground color
            var groundHit = Physics2D.Raycast(transform.position, Vector3.down, 2, _groundMask);
            if (groundHit && groundHit.transform.TryGetComponent(out SpriteRenderer r)) {
                _currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
                SetColor(_moveParticles);
            }

            _movement = _player.RawMovement; // Previous frame movement is more valuable
        }

        private void OnDisable()
        {
            _moveParticles.Stop();
        }

        private void OnEnable()
        {
            _moveParticles.Play();
        }

        private void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }
    }
}