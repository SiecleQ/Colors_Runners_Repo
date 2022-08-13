using UnityEngine;
using Data.UnityObjects;
using Data.ValueObjects;
using Keys;
using Signals;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        [Header("Data")] public InputData Data;

        #endregion

        #region Serialized Variables

        [SerializeField] private bool isReadyForTouch, isFirstTimeTouchTaken;

        #endregion

        #region Private Variables

        private bool _isTouching; //ref type
        private float _currentVelocity; //ref type
        private Vector2? _mousePosition; //ref type
        private Vector3 _moveVector; //ref type

        #endregion

        #endregion

        private void Awake()
        {
            Data = GetInputData();
        }

        private InputData GetInputData() => Resources.Load<CD_Input>("Data/CD_Input").InputData;

        #region Event Subscriptions

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            InputSignals.Instance.onEnableInput += OnEnableInput;
            InputSignals.Instance.onDisableInput += OnDisableInput;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onReset += OnReset;
        }

        private void UnSubscribeEvents()
        {
            InputSignals.Instance.onEnableInput -= OnEnableInput;
            InputSignals.Instance.onDisableInput -= OnDisableInput;
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onReset -= OnReset;
        }

        private void OnDisable()
        {
            UnSubscribeEvents();
        }

        #endregion

        private void Update()
        {
            if (!isReadyForTouch) return;

            if (Input.GetMouseButtonUp(0) && !IsPointerOverUIElement())
            {
                EndMouseDrag();
            }


            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
            {
                StartMouseDrag();
            }

            if (Input.GetMouseButton(0) && !IsPointerOverUIElement())
            {
                if (_isTouching)
                {
                    if (_mousePosition != null)
                    {
                        MouseDrag();
                    }
                }
            }
        }

        #region Mouse Drag Methods
        private void EndMouseDrag()
        {
            _isTouching = false;
            InputSignals.Instance.onInputReleased?.Invoke();
        }

        private void StartMouseDrag()
        {
            _isTouching = true;
            InputSignals.Instance.onInputTaken?.Invoke();
            if (!isFirstTimeTouchTaken)
            {
                isFirstTimeTouchTaken = true;
                InputSignals.Instance.onFirstTimeTouchTaken?.Invoke();
            }

            _mousePosition = Input.mousePosition;
        }

        private void MouseDrag()
        {
            Vector2 mouseDeltaPos = (Vector2)Input.mousePosition - _mousePosition.Value;


            if (mouseDeltaPos.x > Data.HorizontalInputSpeed)
                _moveVector.x = Data.HorizontalInputSpeed / 10f * mouseDeltaPos.x;
            else if (mouseDeltaPos.x < -Data.HorizontalInputSpeed)
                _moveVector.x = -Data.HorizontalInputSpeed / 10f * -mouseDeltaPos.x;
            else
                _moveVector.x = Mathf.SmoothDamp(_moveVector.x, 0f, ref _currentVelocity,
                    Data.ClampSpeed);

            _mousePosition = Input.mousePosition;

            InputSignals.Instance.onInputDragged?.Invoke(new HorizontalInputParams()
            {
                XValue = _moveVector.x,
                ClampValues = new Vector2(Data.ClampSides.x, Data.ClampSides.y)
            });
        }

        private bool IsPointerOverUIElement()
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
        #endregion


        #region Subscribed Methods

        private void OnEnableInput()
        {
            isReadyForTouch = true;
        }

        private void OnDisableInput()
        {
            isReadyForTouch = false;
        }

        private void OnPlay()
        {
            isReadyForTouch = true;
        }

        private void OnReset()
        {
            _isTouching = false;
            isReadyForTouch = false;
            isFirstTimeTouchTaken = false;
        }
        #endregion
    }
}