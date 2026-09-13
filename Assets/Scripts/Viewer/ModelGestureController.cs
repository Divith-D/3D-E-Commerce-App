using System.Collections;
using UnityEngine;

public class ModelGestureController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField]
    private float rotationSensitivity = 0.25f;

    [Header("Scale")]
    [SerializeField]
    private float pinchSensitivity = 0.005f;

    [SerializeField]
    private float minScaleMultiplier = 0.5f;

    [SerializeField]
    private float maxScaleMultiplier = 2f;

    [Header("Double Tap Reset")]
    [SerializeField]
    private float doubleTapThreshold = 0.3f;

    [SerializeField]
    private float resetDuration = 0.35f;


    private Transform target;

    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    private float currentScaleMultiplier = 1f;

    private Vector3 previousMousePosition;

    private float lastMouseClickTime;

    private Coroutine resetCoroutine;
    [SerializeField]
    private float mouseScrollScaleSensitivity = 0.1f;


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target == null)
            return;

        defaultRotation =
            target.localRotation;

        defaultScale =
            target.localScale;

        currentScaleMultiplier = 1f;
    }


    private void Update()
    {
        if (target == null)
            return;


#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#endif

        HandleTouchInput();
    }


    // ------------------------------------
    // MOUSE / EDITOR
    // ------------------------------------

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float currentTime =
                Time.unscaledTime;

            if (currentTime - lastMouseClickTime
                <= doubleTapThreshold)
            {
                StartSmoothReset();
            }

            lastMouseClickTime =
                currentTime;

            previousMousePosition =
                Input.mousePosition;
        }


        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition =
                Input.mousePosition;

            Vector3 delta =
                currentMousePosition -
                previousMousePosition;

            if (delta.sqrMagnitude > 0f)
            {
                CancelReset();

                RotateModel(delta.x);
            }

            previousMousePosition =
                currentMousePosition;
        }

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            CancelReset();

            currentScaleMultiplier +=
                scroll * mouseScrollScaleSensitivity;

            currentScaleMultiplier =
                Mathf.Clamp(
                    currentScaleMultiplier,
                    minScaleMultiplier,
                    maxScaleMultiplier
                );

            target.localScale =
                defaultScale * currentScaleMultiplier;
        }
    }


    // ------------------------------------
    // TOUCH
    // ------------------------------------

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch =
                Input.GetTouch(0);


            // Double tap
            if (touch.phase == TouchPhase.Began &&
                touch.tapCount >= 2)
            {
                StartSmoothReset();

                return;
            }


            // Single finger drag
            if (touch.phase == TouchPhase.Moved)
            {
                CancelReset();

                RotateModel(
                    touch.deltaPosition.x
                );
            }
        }


        else if (Input.touchCount == 2)
        {
            CancelReset();

            Touch touch0 =
                Input.GetTouch(0);

            Touch touch1 =
                Input.GetTouch(1);


            Vector2 previous0 =
                touch0.position -
                touch0.deltaPosition;

            Vector2 previous1 =
                touch1.position -
                touch1.deltaPosition;


            float previousDistance =
                Vector2.Distance(
                    previous0,
                    previous1
                );

            float currentDistance =
                Vector2.Distance(
                    touch0.position,
                    touch1.position
                );


            float pinchDelta =
                currentDistance -
                previousDistance;


            ScaleModel(pinchDelta);
        }
    }


    // ------------------------------------
    // ROTATION
    // ------------------------------------

    private void RotateModel(float deltaX)
    {
        float rotationAmount =
            -deltaX *
            rotationSensitivity;


        target.Rotate(
            Vector3.up,
            rotationAmount,
            Space.World
        );
    }


    // ------------------------------------
    // SCALE
    // ------------------------------------

    private void ScaleModel(float pinchDelta)
    {
        currentScaleMultiplier +=
            pinchDelta *
            pinchSensitivity;


        currentScaleMultiplier =
            Mathf.Clamp(
                currentScaleMultiplier,
                minScaleMultiplier,
                maxScaleMultiplier
            );


        target.localScale =
            defaultScale *
            currentScaleMultiplier;
    }


    // ------------------------------------
    // RESET
    // ------------------------------------

    private void StartSmoothReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }


        resetCoroutine =
            StartCoroutine(
                SmoothReset()
            );
    }


    private IEnumerator SmoothReset()
    {
        Quaternion startRotation =
            target.localRotation;

        Vector3 startScale =
            target.localScale;


        float elapsed = 0f;


        while (elapsed < resetDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed / resetDuration
                );


            // Smooth interpolation
            t =
                t * t *
                (3f - 2f * t);


            target.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    defaultRotation,
                    t
                );


            target.localScale =
                Vector3.Lerp(
                    startScale,
                    defaultScale,
                    t
                );


            yield return null;
        }


        target.localRotation =
            defaultRotation;

        target.localScale =
            defaultScale;

        currentScaleMultiplier = 1f;

        resetCoroutine = null;
    }


    private void CancelReset()
    {
        if (resetCoroutine == null)
            return;

        StopCoroutine(resetCoroutine);

        resetCoroutine = null;
    }
}