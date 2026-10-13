using UnityEngine;
using _Project.Scripts.Ui;
using UnityEngine.InputSystem;

public class TimeGaugeTester : MonoBehaviour
{
    [SerializeField] private TimeGaugeUI timeGauge;

    private void Start()
    {   
        timeGauge.SetupTargetWindow(50f, 60f);
        timeGauge.UpdateGauge(0f);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) timeGauge.UpdateGauge(15f);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) timeGauge.UpdateGauge(30f);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) timeGauge.UpdateGauge(54f);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) timeGauge.UpdateGauge(60f);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) timeGauge.UpdateGauge(61f);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) timeGauge.UpdateGauge(72f);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) timeGauge.UpdateGauge(90f);
    }
}