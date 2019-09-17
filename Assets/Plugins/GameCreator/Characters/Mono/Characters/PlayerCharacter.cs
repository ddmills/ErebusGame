﻿namespace GameCreator.Characters
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.AI;
    using UnityEngine.SceneManagement;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("Game Creator/Characters/Player Character", 100)]
    public class PlayerCharacter : Character
    {
        public enum INPUT_TYPE
        {
            PointAndClick,
            Directional,
            FollowPointer,
            SideScrollX,
            SideScrollZ
        }

        public enum MOUSE_BUTTON
        {
            LeftClick = 0,
            RightClick = 1,
            MiddleClick = 2
        }

        private const string AXIS_H = "Horizontal";
        private const string AXIS_V = "Vertical";

        private static readonly Vector3 PLANE = new Vector3(1, 0, 1);

        private const string PLAYER_ID = "player";
        public static OnLoadSceneData ON_LOAD_SCENE_DATA = null;

        // PROPERTIES: ----------------------------------------------------------------------------

        public INPUT_TYPE inputType = INPUT_TYPE.Directional;
        public MOUSE_BUTTON mouseButtonMove = MOUSE_BUTTON.LeftClick;
        public LayerMask mouseLayerMask = ~0;
        public bool invertAxis = false;

        public KeyCode jumpKey = KeyCode.Space;
        public float jumpMomentumInitial = 15f;
        public float jumpMomentumPost = 1f;
        public float jumpMomentumPostDurationSeconds = 5;
        private float currentJumpDurationStartTime = 0;

        private bool uiConstrained = false;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void Awake()
        {
            if (!Application.isPlaying) return;
            this.CharacterAwake();

            this.initSaveData = new SaveData()
            {
                position = transform.position,
                rotation = transform.rotation,
            };

            if (this.save)
            {
                SaveLoadManager.Instance.Initialize(
                    this, (int)SaveLoadManager.Priority.Normal, true
                );
            }

            HookPlayer hookPlayer = gameObject.GetComponent<HookPlayer>();
            if (hookPlayer == null) gameObject.AddComponent<HookPlayer>();

            if (ON_LOAD_SCENE_DATA != null && ON_LOAD_SCENE_DATA.active)
            {
                transform.position = ON_LOAD_SCENE_DATA.position;
                transform.rotation = ON_LOAD_SCENE_DATA.rotation;
                ON_LOAD_SCENE_DATA.Consume();
            }
        }

        // UPDATE: --------------------------------------------------------------------------------
        private void Update()
        {
            if (Input.GetKeyDown(this.jumpKey) && this.IsControllable())
            {
                if (this.IsGrounded())
                {
                    this.AddMomentum(Vector3.up * jumpMomentumInitial);
                    this.currentJumpDurationStartTime = Time.fixedTime;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying) return;

            switch (this.inputType)
            {
                case INPUT_TYPE.Directional: this.UpdateInputDirectional(); break;
                case INPUT_TYPE.PointAndClick: this.UpdateInputPointClick(); break;
                case INPUT_TYPE.FollowPointer: this.UpdateInputFollowPointer(); break;
                case INPUT_TYPE.SideScrollX: this.UpdateInputSideScroll(Vector3.right); break;
                case INPUT_TYPE.SideScrollZ: this.UpdateInputSideScroll(Vector3.forward); break;
            }

            if (this.IsControllable())
            {
                if (Input.GetKey(this.jumpKey))
                {
                    float currentJumpDurationSeconds = Time.fixedTime - this.currentJumpDurationStartTime;

                    if (currentJumpDurationSeconds <= this.jumpMomentumPostDurationSeconds)
                    {
                        this.AddMomentum(Vector3.up * jumpMomentumPost);
                    }
                }
            }

            this.CharacterUpdate();
        }

        private void UpdateInputDirectional()
        {
            Vector3 direction = Vector3.zero;
            if (!this.IsControllable()) return;

            if (Application.isMobilePlatform || TouchStickManager.FORCE_USAGE)
            {
                Vector2 touchDirection = TouchStickManager.Instance.GetDirection(this);
                direction = new Vector3(touchDirection.x, 0.0f, touchDirection.y);
            }
            else
            {
                direction = new Vector3(
                    Input.GetAxis(AXIS_H),
                    0.0f,
                    Input.GetAxis(AXIS_V)
                );
            }

            Camera maincam = this.GetMainCamera();
            if (maincam == null) return;

            Vector3 moveDirection = maincam.transform.TransformDirection(direction);
            moveDirection.Scale(PLANE);
            moveDirection.Normalize();
            this.characterLocomotion.SetDirectionalDirection(moveDirection);
        }

        private void UpdateInputPointClick()
        {
            if (!this.IsControllable()) return;
            this.UpdateUIConstraints();

            if (Input.GetMouseButtonDown((int)this.mouseButtonMove) && !this.uiConstrained)
            {
                Camera maincam = this.GetMainCamera();
                if (maincam == null) return;

                Ray cameraRay = maincam.ScreenPointToRay(Input.mousePosition);
                this.characterLocomotion.SetTarget(cameraRay, this.mouseLayerMask, null, 0f, null);
            }
        }

        private void UpdateInputFollowPointer()
        {
            if (!this.IsControllable()) return;
            this.UpdateUIConstraints();

            if (Input.GetMouseButton((int)this.mouseButtonMove) && !this.uiConstrained)
            {
                if (HookPlayer.Instance == null) return;

                Camera maincam = this.GetMainCamera();
                if (maincam == null) return;

                Ray cameraRay = maincam.ScreenPointToRay(Input.mousePosition);

                Transform player = HookPlayer.Instance.transform;
                Plane groundPlane = new Plane(Vector3.up, player.position);

                float rayDistance = 0f;
                if (groundPlane.Raycast(cameraRay, out rayDistance))
                {
                    Vector3 cursor = cameraRay.GetPoint(rayDistance);
                    if (Vector3.Distance(player.position, cursor) >= 0.05f)
                    {
                        Vector3 target = Vector3.MoveTowards(player.position, cursor, 1f);
                        this.characterLocomotion.SetTarget(target, null, 0f, null);
                    }
                }
            }
        }

        private void UpdateInputSideScroll(Vector3 axis)
        {
            Vector3 direction = Vector3.zero;
            if (!this.IsControllable()) return;

            if (Application.isMobilePlatform || TouchStickManager.FORCE_USAGE)
            {
                Vector2 touchDirection = TouchStickManager.Instance.GetDirection(this);
                direction = axis * touchDirection.x;
            }
            else
            {
                direction = axis * Input.GetAxis(AXIS_H);
            }

            Camera maincam = this.GetMainCamera();
            if (maincam == null) return;

            float invertValue = (this.invertAxis ? -1 : 1);
            direction.Scale(axis * invertValue);
            direction.Normalize();
            this.characterLocomotion.SetDirectionalDirection(direction);
        }

        private Camera GetMainCamera()
		{
			if (HookCamera.Instance != null) return HookCamera.Instance.Get<Camera>();
			if (Camera.main != null) return Camera.main;

            Debug.LogError(ERR_NOCAM, gameObject);
			return null;
		}

        private void UpdateUIConstraints()
        {
            EventSystemManager.Instance.Wakeup();
            this.uiConstrained = EventSystemManager.Instance.IsPointerOverUI();

            #if UNITY_IOS || UNITY_ANDROID
            for (int i = 0; i < Input.touches.Length; ++i)
            {
                if (Input.GetTouch(i).phase != TouchPhase.Began) continue;

            int fingerID = Input.GetTouch(i).fingerId;
                bool pointerOverUI = EventSystemManager.Instance.IsPointerOverUI(fingerID);
                if (pointerOverUI) this.uiConstrained = true;
            }
            #endif
        }

        // GAME SAVE: -----------------------------------------------------------------------------

        protected override string GetUniqueCharacterID()
        {
            return PLAYER_ID;
        }
    }
}
