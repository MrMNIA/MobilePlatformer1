    using Unity.VisualScripting;
    using UnityEngine;

    public class CameraController : MonoBehaviour
    {
        [SerializeField] private MovementJoystick movementJoystick; //joystick referans�
        [SerializeField] private AttackJoystick attackJoystick;
        [SerializeField] private Transform cameraPointer;
        [SerializeField] private Transform playerPosition;
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private float movementDistance;
        [SerializeField] private float attackDistance;
        Vector3 control = Vector3.zero;
        public float smooothTime;

        private void LateUpdate()
        {

            Vector2 cameraTarget = new Vector2(playerPosition.position.x, playerPosition.position.y);
            
            float OffsetX = playerBody.linearVelocity.x * movementDistance + attackJoystick.Horizontal * attackDistance;
            float OffsetY = playerBody.linearVelocity.y * (movementDistance / 2) + attackJoystick.Vertical * attackDistance;

            cameraTarget.x += OffsetX;
            cameraTarget.y += OffsetY;

            cameraPointer.position = cameraTarget;

            Vector3 targetPosition = new Vector3(cameraPointer.position.x,cameraPointer.position.y, -10f);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref control, smooothTime);
        }
    }
