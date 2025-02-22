using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.Detectors;

namespace CodeStage.AntiCheat.ObscuredTypes
{
    public class PlayerController2D : MonoBehaviour
    {

        public static readonly float playerDistanceRenderOrder = 0.25f;

        public static readonly float playerMaxTouchDistance = 0.18f;

        public static readonly float playerMaxTouchXAxelDistance = 0.12f;

        public static readonly float playerJumpDodgeCornerCorrectionFactor = 0.5f;

        public static readonly float playerJumpDodgeCornerMovementSpeed = 0.03f;

        private struct CharacterRaycastOrigins
        {
            public Vector3 topLeft;

            public Vector3 bottomRight;

            public Vector3 bottomLeft;
        }

        public class CharacterCollisionState2D
        {
            public ObscuredBool right;

            public ObscuredBool left;

            public ObscuredBool above;

            public ObscuredBool below;

            public ObscuredBool becameGroundedThisFrame;

            public ObscuredBool wasGroundedLastFrame;

            public ObscuredBool movingDownSlope;

            public ObscuredFloat slopeAngle;

            public bool hasCollision()
            {
                return (bool)below || (bool)right || (bool)left || (bool)above;
            }

            public void reset()
            {
                right = (left = (above = (below = (becameGroundedThisFrame = (movingDownSlope = false)))));
                slopeAngle = 0f;
            }

            public override string ToString()
            {
                return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}", right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame);
            }
        }

        public bool ignoreOneWayPlatformsThisFrame;

        [SerializeField]
        [Range(0.001f, 0.3f)]
        private float _skinWidth = 0.02f;

        public LayerMask platformMask = 0;

        public LayerMask triggerMask = 0;

        [SerializeField]
        public LayerMask oneWayPlatformMask = 0;

        [Range(0f, 90f)]
        public float slopeLimit = 30f;

        public float jumpingThreshold = 0.07f;

        public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

        [Range(2f, 20f)]
        public int totalHorizontalRays = 8;

        [Range(2f, 20f)]
        public int totalVerticalRays = 4;

        private float _slopeLimitTangent = Mathf.Tan(1.3089969f);

        [NonSerialized]
        [HideInInspector]
        public Transform myTransform;

        [NonSerialized]
        [HideInInspector]
        public BoxCollider2D boxCollider;

        [NonSerialized]
        [HideInInspector]
        public Rigidbody2D rigidBody2D;

        [NonSerialized]
        [HideInInspector]
        public CharacterCollisionState2D collisionState = new CharacterCollisionState2D();

        [NonSerialized]
        [HideInInspector]
        public Vector3 velocity;

        private const float kSkinWidthFloatFudgeFactor = 0.001f;

        private CharacterRaycastOrigins _raycastOrigins;

        private RaycastHit2D _raycastHit;

        private List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

        private float _verticalDistanceBetweenRays;

        private float _horizontalDistanceBetweenRays;

        private bool _isGoingUpSlope;

        private Vector3 tempDeltaMovement;

        private Bounds tempModifiedBounds;

        private bool tempIsGoingRight;

        private float tempRayDistance;

        private Vector2 tempRayDirection = Vector2.zero;

        private Vector3 tempInitialRayOrigin = Vector3.zero;

        private Vector2 tempRay = Vector2.zero;

        private Vector3 dodgeCornerMovement = Vector3.zero;

        private bool tempIsGoingUp;

        private LayerMask tempMask;

        private bool tempDidFirstRayHit;

        private bool tempDidLastRayHit;

        private int tempIndex;

        private bool tempDoCornerRayCheck;

        private float tempInsideCorrection;

        public float skinWidth
        {
            get
            {
                return _skinWidth;
            }
            set
            {
                _skinWidth = value;
                recalculateDistanceBetweenRays();
            }
        }

        public bool isGrounded
        {
            get
            {
                return collisionState.below;
            }
        }

        public bool isHeaded
        {
            get
            {
                return collisionState.above;
            }
        }

        public bool isRighted
        {
            get
            {
                return collisionState.right;
            }
        }

        public bool isLefted
        {
            get
            {
                return collisionState.left;
            }
        }

        public event Action<RaycastHit2D> onControllerCollidedEvent;

        public event Action<Collider2D> onTriggerEnterEvent;

        public event Action<Collider2D> onTriggerStayEvent;

        public event Action<Collider2D> onTriggerExitEvent;

        private void Awake()
        {
            platformMask = (int)platformMask | (int)oneWayPlatformMask;
            myTransform = GetComponent<Transform>();
            boxCollider = GetComponent<BoxCollider2D>();
            rigidBody2D = GetComponent<Rigidbody2D>();
            skinWidth = _skinWidth;
            for (int i = 0; i < 32; i++)
            {
                if ((triggerMask.value & (1 << i)) == 0)
                {
                    Physics2D.IgnoreLayerCollision(base.gameObject.layer, i);
                }
            }
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            if (this.onTriggerEnterEvent != null)
            {
                this.onTriggerEnterEvent(col);
            }
        }

        public void OnTriggerStay2D(Collider2D col)
        {
            if (this.onTriggerStayEvent != null)
            {
                this.onTriggerStayEvent(col);
            }
        }

        public void OnTriggerExit2D(Collider2D col)
        {
            if (this.onTriggerExitEvent != null)
            {
                this.onTriggerExitEvent(col);
            }
        }

        public void move(Vector3 deltaMovement, bool ignoreOneway = false)
        {
            collisionState.wasGroundedLastFrame = collisionState.below;
            collisionState.reset();
            _raycastHitsThisFrame.Clear();
            _isGoingUpSlope = false;
            primeRaycastOrigins();
            if (ignoreOneway)
            {
                ignoreOneWayPlatformsThisFrame = true;
            }
            if (deltaMovement.x != 0f)
            {
                moveHorizontally(ref deltaMovement);
            }
            else
            {
                tempDeltaMovement = deltaMovement;
                tempRayDistance = _skinWidth;
                tempRayDirection = Vector2.right;
                tempInitialRayOrigin = _raycastOrigins.bottomRight;
                for (int i = 0; i < totalHorizontalRays; i++)
                {
                    tempRay.x = tempInitialRayOrigin.x;
                    tempRay.y = tempInitialRayOrigin.y + (float)i * _verticalDistanceBetweenRays;
                    if (i == 0 && (bool)collisionState.wasGroundedLastFrame)
                    {
                        _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, platformMask);
                    }
                    else
                    {
                        _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, (int)platformMask & ~(int)oneWayPlatformMask);
                    }
                    if ((bool)_raycastHit)
                    {
                        tempDeltaMovement.x = _raycastHit.point.x - tempRay.x;
                        tempRayDistance = Mathf.Abs(tempDeltaMovement.x);
                        tempDeltaMovement.x -= _skinWidth;
                        collisionState.right = true;
                        _raycastHitsThisFrame.Add(_raycastHit);
                        if (tempRayDistance < _skinWidth + 0.001f)
                        {
                            break;
                        }
                    }
                }
                tempDeltaMovement = deltaMovement;
                tempRayDistance = _skinWidth;
                tempRayDirection = -Vector2.right;
                tempInitialRayOrigin = _raycastOrigins.bottomLeft;
                for (int j = 0; j < totalHorizontalRays; j++)
                {
                    tempRay.x = tempInitialRayOrigin.x;
                    tempRay.y = tempInitialRayOrigin.y + (float)j * _verticalDistanceBetweenRays;
                    if (j == 0 && (bool)collisionState.wasGroundedLastFrame)
                    {
                        _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, platformMask);
                    }
                    else
                    {
                        _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, (int)platformMask & ~(int)oneWayPlatformMask);
                    }
                    if ((bool)_raycastHit)
                    {
                        tempDeltaMovement.x = _raycastHit.point.x - tempRay.x;
                        tempRayDistance = Mathf.Abs(tempDeltaMovement.x);
                        tempDeltaMovement.x += _skinWidth;
                        collisionState.left = true;
                        _raycastHitsThisFrame.Add(_raycastHit);
                        if (tempRayDistance < _skinWidth + 0.001f)
                        {
                            break;
                        }
                    }
                }
            }
            if (deltaMovement.y != 0f)
            {
                moveVertically(ref deltaMovement);
            }
            else
            {
                dodgeCornerMovement.x = 0f;
            }
            myTransform.Translate(deltaMovement + dodgeCornerMovement, Space.World);
            if (Time.deltaTime > 0f)
            {
                velocity = deltaMovement / Time.deltaTime;
            }
            if (!collisionState.wasGroundedLastFrame && (bool)collisionState.below)
            {
                collisionState.becameGroundedThisFrame = true;
            }
            if (_isGoingUpSlope)
            {
                velocity.y = 0f;
            }
            if (this.onControllerCollidedEvent != null)
            {
                for (int k = 0; k < _raycastHitsThisFrame.Count; k++)
                {
                    this.onControllerCollidedEvent(_raycastHitsThisFrame[k]);
                }
            }
            ignoreOneWayPlatformsThisFrame = false;
        }

        public void warpToGrounded()
        {
            do
            {
                move(new Vector3(0f, -1f, 0f));
            }
            while (!isGrounded);
        }

        public void recalculateDistanceBetweenRays()
        {
            float num = boxCollider.size.y * Mathf.Abs(myTransform.localScale.y) - 2f * _skinWidth;
            _verticalDistanceBetweenRays = num / (float)(totalHorizontalRays - 1) * 0.5f;
            float num2 = boxCollider.size.x * Mathf.Abs(myTransform.localScale.x) - 2f * _skinWidth;
            _horizontalDistanceBetweenRays = num2 / (float)(totalVerticalRays - 1);
        }

        private void primeRaycastOrigins()
        {
            tempModifiedBounds = boxCollider.bounds;
            tempModifiedBounds.Expand(-2f * _skinWidth);
            _raycastOrigins.topLeft.x = tempModifiedBounds.min.x;
            _raycastOrigins.topLeft.y = tempModifiedBounds.max.y;
            _raycastOrigins.bottomRight.x = tempModifiedBounds.max.x;
            _raycastOrigins.bottomRight.y = tempModifiedBounds.min.y;
            _raycastOrigins.bottomLeft = tempModifiedBounds.min;
        }

        private void moveHorizontally(ref Vector3 deltaMovement)
        {
            tempIsGoingRight = deltaMovement.x > 0f;
            tempRayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
            tempRayDirection = ((!tempIsGoingRight) ? (-Vector2.right) : Vector2.right);
            tempInitialRayOrigin = ((!tempIsGoingRight) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight);
            for (int i = 0; i < totalHorizontalRays; i++)
            {
                tempRay.x = tempInitialRayOrigin.x;
                tempRay.y = tempInitialRayOrigin.y + (float)i * _verticalDistanceBetweenRays;
                if (i == 0 && (bool)collisionState.wasGroundedLastFrame)
                {
                    _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, platformMask);
                }
                else
                {
                    _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, (int)platformMask & ~(int)oneWayPlatformMask);
                }
                if ((bool)_raycastHit)
                {
                    if (i == 0 && handleHorizontalSlope(ref deltaMovement, Vector2.Angle(_raycastHit.normal, Vector2.up)))
                    {
                        _raycastHitsThisFrame.Add(_raycastHit);
                        break;
                    }
                    deltaMovement.x = _raycastHit.point.x - tempRay.x;
                    tempRayDistance = Mathf.Abs(deltaMovement.x);
                    if (tempIsGoingRight)
                    {
                        deltaMovement.x -= _skinWidth;
                        collisionState.right = true;
                    }
                    else
                    {
                        deltaMovement.x += _skinWidth;
                        collisionState.left = true;
                    }
                    _raycastHitsThisFrame.Add(_raycastHit);
                    if (tempRayDistance < _skinWidth + 0.001f)
                    {
                        break;
                    }
                }
            }
        }

        private bool handleHorizontalSlope(ref Vector3 deltaMovement, float angle)
        {
            if (Mathf.RoundToInt(angle) == 90)
            {
                return false;
            }
            if (angle < slopeLimit)
            {
                if (deltaMovement.y < jumpingThreshold)
                {
                    float num = slopeSpeedMultiplier.Evaluate(angle);
                    deltaMovement.x *= num;
                    deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * ((float)Math.PI / 180f)) * deltaMovement.x);
                    bool flag = deltaMovement.x > 0f;
                    Vector3 vector = ((!flag) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight);
                    RaycastHit2D raycastHit2D = ((!collisionState.wasGroundedLastFrame) ? Physics2D.Raycast(vector, deltaMovement.normalized, deltaMovement.magnitude, (int)platformMask & ~(int)oneWayPlatformMask) : Physics2D.Raycast(vector, deltaMovement.normalized, deltaMovement.magnitude, platformMask));
                    if ((bool)raycastHit2D)
                    {
                        deltaMovement = (Vector3)raycastHit2D.point - vector;
                        if (flag)
                        {
                            deltaMovement.x -= _skinWidth;
                        }
                        else
                        {
                            deltaMovement.x += _skinWidth;
                        }
                    }
                    _isGoingUpSlope = true;
                    collisionState.below = true;
                }
            }
            else
            {
                deltaMovement.x = 0f;
            }
            return true;
        }

        private void moveVertically(ref Vector3 deltaMovement)
        {
            tempIsGoingUp = deltaMovement.y > 0f;
            tempRayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
            tempRayDirection = ((!tempIsGoingUp) ? (-Vector2.up) : Vector2.up);
            tempInitialRayOrigin = ((!tempIsGoingUp) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft);
            tempInitialRayOrigin.x += deltaMovement.x;
            tempMask = platformMask;
            if (tempIsGoingUp || ignoreOneWayPlatformsThisFrame)
            {
                tempMask = (int)tempMask & ~(int)oneWayPlatformMask;
            }
            tempDidFirstRayHit = false;
            tempDidLastRayHit = false;
            float distance = 0f;
            for (int i = 0; i < totalVerticalRays; i++)
            {
                tempRay.x = tempInitialRayOrigin.x + (float)i * _horizontalDistanceBetweenRays;
                tempRay.y = tempInitialRayOrigin.y;
                _raycastHit = Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, tempMask);
                if ((bool)_raycastHit)
                {
                    if (i == 0)
                    {
                        tempDidFirstRayHit = true;
                        distance = tempRayDistance;
                    }
                    else if (i == totalVerticalRays - 1)
                    {
                        tempDidLastRayHit = true;
                    }
                    deltaMovement.y = _raycastHit.point.y - tempRay.y;
                    tempRayDistance = Mathf.Abs(deltaMovement.y);
                    if (tempIsGoingUp)
                    {
                        deltaMovement.y -= _skinWidth;
                        collisionState.above = true;
                    }
                    else
                    {
                        deltaMovement.y += _skinWidth;
                        collisionState.below = true;
                    }
                    _raycastHitsThisFrame.Add(_raycastHit);
                    if (!tempIsGoingUp && deltaMovement.y > 1E-05f)
                    {
                        _isGoingUpSlope = true;
                    }
                    if (tempRayDistance < _skinWidth + 0.001f)
                    {
                        break;
                    }
                }
            }
            if (tempIsGoingUp && deltaMovement.y >= 0f)
            {
                tempIndex = 0;
                tempDoCornerRayCheck = false;
                if (tempDidFirstRayHit && !tempDidLastRayHit)
                {
                    tempRay.x = tempInitialRayOrigin.x + (float)(totalVerticalRays - 1) * _horizontalDistanceBetweenRays;
                    tempRay.y = tempInitialRayOrigin.y;
                    if (!Physics2D.Raycast(tempRay, tempRayDirection, distance, tempMask))
                    {
                        tempIndex = 0;
                        tempDoCornerRayCheck = true;
                    }
                }
                else if (!tempDidFirstRayHit && tempDidLastRayHit)
                {
                    tempIndex = totalVerticalRays - 1;
                    tempDoCornerRayCheck = true;
                }
                else
                {
                    dodgeCornerMovement.x = 0f;
                }
            }
            if (tempDoCornerRayCheck)
            {
                tempInsideCorrection = _horizontalDistanceBetweenRays * playerJumpDodgeCornerCorrectionFactor;
                tempRay.x = tempInitialRayOrigin.x + (float)tempIndex * _horizontalDistanceBetweenRays + ((tempIndex != 0) ? (0f - tempInsideCorrection) : tempInsideCorrection);
                tempRay.y = tempInitialRayOrigin.y;
                if (!Physics2D.Raycast(tempRay, tempRayDirection, tempRayDistance, tempMask))
                {
                    if (tempIndex == 0)
                    {
                        dodgeCornerMovement.x = playerJumpDodgeCornerMovementSpeed;
                    }
                    else
                    {
                        dodgeCornerMovement.x = 0f - playerJumpDodgeCornerMovementSpeed;
                    }
                }
            }
            else
            {
                dodgeCornerMovement.x = 0f;
            }
        }

        private void handleVerticalSlope(ref Vector3 deltaMovement)
        {
            float num = (_raycastOrigins.bottomLeft.x + _raycastOrigins.bottomRight.x) * 0.5f;
            Vector2 direction = -Vector2.up;
            float distance = _slopeLimitTangent * (_raycastOrigins.bottomRight.x - num);
            Vector2 origin = new Vector2(num, _raycastOrigins.bottomLeft.y);
            _raycastHit = Physics2D.Raycast(origin, direction, distance, platformMask);
            if ((bool)_raycastHit)
            {
                float num2 = Vector2.Angle(_raycastHit.normal, Vector2.up);
                if (num2 != 0f && Mathf.Sign(_raycastHit.normal.x) == Mathf.Sign(deltaMovement.x))
                {
                    float num3 = slopeSpeedMultiplier.Evaluate(0f - num2);
                    deltaMovement.y += _raycastHit.point.y - origin.y - skinWidth;
                    deltaMovement.x *= num3;
                    collisionState.movingDownSlope = true;
                    collisionState.slopeAngle = num2;
                }
            }
        }
    }

}