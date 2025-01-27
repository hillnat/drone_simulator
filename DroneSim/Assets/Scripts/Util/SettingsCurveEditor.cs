using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public enum PairableCurves
{
    Pitch,Roll,Yaw,Throttle
}
public class SettingsCurveEditor : MonoBehaviour
{
    private GraphicRaycaster gR;
    private EventSystem eR;
    public RectTransform[] keys = new RectTransform[10];
    public PairableCurves pairedCurve;
    private AnimationCurve curve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    private const int curveEditorSize = 300;
    private int selectedKey = -1;
    public RectTransform zero;
    private Vector2 mouseDownAtPosition = Vector2.zero;
    private Vector2 selectedKeyPreMovePosition;
    public TMP_Text label;
    private void Start()
    {
        gR = GetComponentInParent<GraphicRaycaster>();
        eR = InputManager.instance.GetComponent<EventSystem>();
        UpdatePairedCurve();
        UpdateVisualKeys();//align keys to what the anim curve has
        CalculateCurve();//calculate for line renderer
    }
    private void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            if (selectedKey == -1)//Hasnt yet selected a key to move
            {
                // Create a pointer event data object
                PointerEventData pointerEventData = new PointerEventData(eR);
                pointerEventData.position = InputManager.instance.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                // Raycast from mouse position onto UI
                gR.Raycast(pointerEventData, results);

                if (results.Count > 0)
                {
                    foreach (RaycastResult result in results)
                    {
                        if (result.gameObject.tag=="CurveEditorKey")
                        {
                            selectedKey = int.Parse(result.gameObject.name);
                            selectedKeyPreMovePosition = keys[selectedKey].anchoredPosition;
                            mouseDownAtPosition = InputManager.instance.mousePosition;
                            break;
                        }
                    }
                }
            }
            else//Has selected a key to mvoe
            {
                float mouseDeltaY = InputManager.instance.mousePosition.y - mouseDownAtPosition.y;//Get mouse y delta
                keys[selectedKey].anchoredPosition = new Vector2(
                    keys[selectedKey].anchoredPosition.x,//save x pos
                    Mathf.Clamp(selectedKeyPreMovePosition.y + mouseDeltaY, zero.anchoredPosition.y, zero.anchoredPosition.y + curveEditorSize));//move key by y delta clamped between editor size
                CalculateCurve();
            }
        }
        else
        {
            selectedKey = -1; mouseDownAtPosition = Vector2.zero;
        }
    }
    private void UpdateVisualKeys()//Set position of UI keypoint buttons
    {
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].anchoredPosition = new Vector2(//Map that range to 0-1
                Mathf.Lerp(zero.anchoredPosition.x, zero.anchoredPosition.x + curveEditorSize, (1f / keys.Length) * (i + 1)),
                Mathf.Lerp(zero.anchoredPosition.y, zero.anchoredPosition.y + curveEditorSize, curve.Evaluate((1f / keys.Length) * (i+1))));
        }
    }
    private void CalculateCurve()
    {
        curve.ClearKeys();
        curve.AddKey(0, 0);

        for (int i=0; i<keys.Length; i++)
        {
            Vector2 delta = keys[i].anchoredPosition - zero.anchoredPosition;//keys postiion relative to 0
            Vector2 keyValue = new Vector2(
                (1f / keys.Length) * (i + 1),
                Mathf.InverseLerp(zero.anchoredPosition.y, zero.anchoredPosition.y+curveEditorSize, delta.y));//Map y delta between 0 and editor size
            curve.AddKey(keyValue.x, keyValue.y);
        }
    }
    private void UpdatePairedCurve()
    {
        switch (pairedCurve)
        {
            case PairableCurves.Pitch:
                curve = SettingsManager.instance.playerSettings.pitchCurve;
                label.text = "Pitch";
                break;
            case PairableCurves.Roll:
                curve = SettingsManager.instance.playerSettings.rollCurve;
                label.text = "Roll";
                break;
            case PairableCurves.Yaw:
                curve = SettingsManager.instance.playerSettings.yawCurve;
                label.text = "Yaw";
                break;
            case PairableCurves.Throttle:
                curve = SettingsManager.instance.playerSettings.throttleCurve;
                label.text = "Throttle";
                break;
            default:
                break;
        }
    }
    public void UICALLBACK_ChangePairedCurve(int c)
    {
        pairedCurve++;
        if ((int)pairedCurve >= 4) { pairedCurve = (PairableCurves)0; }
        else if ((int)pairedCurve < 0) { pairedCurve = (PairableCurves)3; }
        UpdatePairedCurve();
        UpdateVisualKeys();//align keys to what the anim curve has
    }
}
