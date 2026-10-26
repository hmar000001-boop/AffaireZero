using UnityEngine;
using UnityEngine.InputSystem;
using AffaireZero.UI;

namespace AffaireZero.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public GameUI UI;
        public Transform CameraPivot;
        public float MoveSpeed = 3.25f;
        public float LookSensitivity = 0.13f;
        private CharacterController motor;
        private float pitch;
        private float verticalVelocity;
        private WorldInteractable nearby;
        private WorldInteractable[] interactables = new WorldInteractable[0];
        private float scanTimer;

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            RefreshInteractables();
        }

        public void RefreshInteractables()
        {
            interactables = FindObjectsOfType<WorldInteractable>();
        }

        private void Update()
        {
            if (UI == null || !UI.IsPlaying) return;
            float h = 0f;
            float v = 0f;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                h = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
                v = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            }
            Vector2 joystick = UI.MoveInput;
            if (joystick.sqrMagnitude > 0.02f) { h = joystick.x; v = joystick.y; }
            Vector3 direction = transform.right * h + transform.forward * v;
            if (direction.sqrMagnitude > 1f) direction.Normalize();
            if (motor.isGrounded && verticalVelocity < 0f) verticalVelocity = -1.2f;
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            direction.y = verticalVelocity;
            motor.Move(direction * MoveSpeed * Time.deltaTime);

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                ApplyLook(delta * 2.4f);
            }
            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f) { scanTimer = 0.15f; ScanNearby(); }
        }

        public void ApplyLook(Vector2 delta)
        {
            if (UI == null || !UI.IsPlaying) return;
            transform.Rotate(0f, delta.x * LookSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - delta.y * LookSensitivity, -70f, 70f);
            if (CameraPivot != null) CameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void ScanNearby()
        {
            WorldInteractable best = null;
            float bestScore = float.MaxValue;
            Vector3 origin = CameraPivot != null ? CameraPivot.position : transform.position + Vector3.up * 1.5f;
            Vector3 forward = CameraPivot != null ? CameraPivot.forward : transform.forward;
            foreach (var candidate in interactables)
            {
                float distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance > 3.35f) continue;
                Vector3 toTarget = (candidate.transform.position + Vector3.up * 0.15f - origin).normalized;
                float angle = Vector3.Angle(forward, toTarget);
                if (angle > 78f) continue;
                float score = distance + angle * 0.018f;
                if (score < bestScore) { bestScore = score; best = candidate; }
            }
            if (best != nearby)
            {
                nearby = best;
                if (UI != null) UI.SetNearby(nearby);
            }
        }

        public void InteractNearby()
        {
            if (nearby != null && UI != null && UI.IsPlaying) nearby.Activate();
        }

        public void ClearNearby() { nearby = null; if (UI != null) UI.SetNearby(null); }

        public void ResetToStart()
        {
            if (motor == null) motor = GetComponent<CharacterController>();
            motor.enabled = false;
            transform.position = new Vector3(0f, 0.06f, -1.7f);
            transform.rotation = Quaternion.identity;
            pitch = 0f;
            verticalVelocity = 0f;
            if (CameraPivot != null) { CameraPivot.localPosition = new Vector3(0f, 1.58f, 0f); CameraPivot.localRotation = Quaternion.identity; }
            motor.enabled = true;
            ClearNearby();
        }
    }
}
