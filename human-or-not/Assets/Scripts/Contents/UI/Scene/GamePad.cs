using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamePad : SceneUI
{
    public bool Zoomed { get; private set; } = false;
    public float ZoomValue
    {
        get
        {
            Slider slider = GetSlider((int)Sliders.ZoomSlider).GetComponent<Slider>();
            if (slider == null)
            {
                CameraController cameraController = Camera.main.GetComponent<CameraController>();
                return (cameraController.MaxZoomRatio + cameraController.MinZoomRatio) / 2f;
            }

            return slider.value;
        }
    }
    public Vector2 Direction { get; private set; } = Vector2.zero;
    public JoystickDetect JoyStickDetected { get; private set; } = JoystickDetect.Center;
    public enum JoystickDetect
    {
        Center,
        CloseToCenter,
        FarFromCenter,
    }

    public Vector2 TouchPoint { get; private set; } = Vector2.zero;
    public RotatePanelTap RotatePanelTapped { get; private set; } = RotatePanelTap.None;
    public enum RotatePanelTap
    {
        None,
        Begin,
        On,
    }
    public ButtonClick ButtonClicked { get; set; } = ButtonClick.None;
    public enum ButtonClick
    {
        None,
        Attack,
        Jump,
    }

    enum Images
    {
        RotatePanel,
        Joystick,
        OuterCircle,
        InnerCircle,
        Zoom,
    }

    enum Sliders
    {
        ZoomSlider,
    }

    enum Buttons
    {
        Attack,
        Jump,
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        // Binding
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<Slider>(typeof(Sliders));
    }

    void Start()
    {
        InitJoystick();
        InitRotatePanel();
        InitTriggerButtons();
        InitZoomSlider();
    }

    void InitJoystick()
    {
        Image joystick = GetImage((int)Images.Joystick);
        Image outerCircle = GetImage((int)Images.OuterCircle);
        Image innerCircle = GetImage((int)Images.InnerCircle);

        float outerCircleRadius = outerCircle.rectTransform.rect.width * 0.5f;
        float boundary = outerCircleRadius * 0.85f;

        BindEvent(joystick.gameObject, (PointerEventData eventData) =>
        {
            outerCircle.gameObject.SetActive(true);
            innerCircle.rectTransform.position = outerCircle.rectTransform.position = eventData.position;
        }, Definition.UIEvent.PointerDown);

        BindEvent(joystick.gameObject, (PointerEventData eventData) =>
        {
            Vector2 dir = Vector2.ClampMagnitude(eventData.position - (Vector2)outerCircle.rectTransform.position, outerCircleRadius);
            if (dir.magnitude < boundary)
            {
                // 걷기
                if (JoyStickDetected != JoystickDetect.CloseToCenter)
                {
                    JoyStickDetected = JoystickDetect.CloseToCenter;
                    innerCircle.color = Color.gray;    // 무분별한 색깔 전환 방지
                }
            }
            else
            {
                // 뛰기
                if (JoyStickDetected != JoystickDetect.FarFromCenter)
                {
                    JoyStickDetected = JoystickDetect.FarFromCenter;
                    innerCircle.color = Color.black;   // 무분별한 색깔 전환 방지
                }
            }
            innerCircle.rectTransform.localPosition = dir;
            Direction = dir.normalized;
        }, Definition.UIEvent.OnDrag);

        BindEvent(joystick.gameObject, (PointerEventData eventData) =>
        {
            Direction = innerCircle.rectTransform.localPosition = Vector3.zero;
            innerCircle.color = Color.white;
            outerCircle.gameObject.SetActive(false);

            JoyStickDetected = JoystickDetect.Center;
        }, Definition.UIEvent.PointerUp);

    }

    void InitRotatePanel()
    {
        Image panel = GetImage((int)Images.RotatePanel);

        BindEvent(panel.gameObject, (PointerEventData eventData) =>
        {
            RotatePanelTapped = RotatePanelTap.Begin;

            TouchPoint = eventData.position;
        }, Definition.UIEvent.PointerDown);

        BindEvent(panel.gameObject, (PointerEventData eventData) =>
        {
            if (RotatePanelTapped == RotatePanelTap.Begin)
                RotatePanelTapped = RotatePanelTap.On;

            // 짧은 순간에 거리 제곱 차가 클 경우 무시
            if ((Camera.main.ScreenToViewportPoint(TouchPoint) - Camera.main.ScreenToViewportPoint(eventData.position)).magnitude > 0.1f)
                return;

            TouchPoint = eventData.position;
        }, Definition.UIEvent.OnDrag);

        BindEvent(panel.gameObject, (PointerEventData eventData) => { RotatePanelTapped = RotatePanelTap.None; }, Definition.UIEvent.PointerUp);
    }

    void InitZoomSlider()
    {
        Image zoom = GetImage((int)Images.Zoom);
        Slider zoomSlider = GetSlider((int)Sliders.ZoomSlider);

        // 1 ~ 10 단계
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        zoomSlider.minValue = cameraController.MinZoomRatio;
        zoomSlider.maxValue = cameraController.MaxZoomRatio;
        zoomSlider.value = (cameraController.MaxZoomRatio + cameraController.MinZoomRatio) / 2f;

        BindEvent(zoom.gameObject, (PointerEventData) => { }); // blocking
        BindEvent(zoomSlider.gameObject, (PointerEventData eventData) => { Zoomed = true; }, Definition.UIEvent.PointerDown);
        BindEvent(zoomSlider.gameObject, (PointerEventData eventData) => { Zoomed = false; }, Definition.UIEvent.PointerUp);
    }

    void InitTriggerButtons()
    {
        Button attackButton = GetButton((int)Buttons.Attack);
        Button jumpButton = GetButton((int)Buttons.Jump);

        BindEvent(attackButton.gameObject, (PointerEventData eventData) => { ButtonClicked = ButtonClick.Attack; }, Definition.UIEvent.PointerDown);
        BindEvent(attackButton.gameObject, (PointerEventData eventData) => { ButtonClicked = ButtonClick.None; }, Definition.UIEvent.PointerUp);

        BindEvent(jumpButton.gameObject, (PointerEventData eventData) => { ButtonClicked = ButtonClick.Jump; }, Definition.UIEvent.PointerDown);
        BindEvent(jumpButton.gameObject, (PointerEventData eventData) => { ButtonClicked = ButtonClick.None; }, Definition.UIEvent.PointerUp);
    }
}