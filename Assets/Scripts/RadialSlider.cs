using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HoloToolkit.Examples.InteractiveElements;

public class RadialSlider : GestureInteractiveControl
{
    //bool isPointerDown=false;
    //public KGFOrbitCam itsKGFOrbitCam;	//reference to the orbitcam	


    [Tooltip("Sync Value across the clients")]
    public bool SyncMessage = true;

    [Tooltip("The message manager")] public MessageManager SyncManager;

    private RectTransform throttle_rect;

    private RectTransform thisRect;

    //  private GestureInteractiveControl control;
    // public GameObject radialSlider_handControll;
    //private Vector3 HandPosition;
    private Vector2 HandPosition2D;
    private GestureInteractiveData vertData = new GestureInteractiveData(new Vector3(0, 1, 0), 0.1f, false);

    Vector2 throttle_ori;
    //private GestureInteractiveControl gestureInteractiveControl;

    void Start()
    {
        GameObject throttle_img = GameObject.FindWithTag("GameController");
        throttle_rect = throttle_img.GetComponent<RectTransform>();
        throttle_ori = throttle_rect.anchoredPosition;
        thisRect = gameObject.GetComponent<RectTransform>();

        /*  if (radialSlider = null)
          {
               radialSlider =this.gameObject;

          }*/

        //control = radialSlider_handControll.GetComponent<GestureInteractiveControl>();
        //HandPosition = control.CurrentGesturePosition;
        //Debug.Log("StartHandPosition="+HandPosition);
    }

    // Called when the pointer enters our GUI component.
    // Start tracking the mouse
    /*
	public void OnPointerEnter( PointerEventData eventData )
	{
		StartCoroutine( "TrackPointer" );            
	}
	
	// Called when the pointer exits our GUI component.
	// Stop tracking the mouse
	public void OnPointerExit( PointerEventData eventData )
	{
		StopCoroutine( "TrackPointer" );
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isPointerDown= true;
		//Debug.Log("mousedown");
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isPointerDown= false;
		//Debug.Log("mousedown");
	}*/

    public float ang = 0;
    public float rad = 0;

    public void ResetTheThrottle()
    {
        throttle_rect.anchoredPosition = throttle_ori;
    }

    public void SetAngRad(float outAng, float outRad)
    {
        this.ang = outAng;
        this.rad = outRad;
        SyncValue(this.ang, this.rad);
    }

    public void SetAngRad(float outAng, float outRad, Vector2 pos)
    {
        this.ang = outAng;
        this.rad = outRad;
        throttle_rect.anchoredPosition = pos;
    }

    private void SyncValue(float sAng, float sRad)
    {
        if (!SyncMessage)
            return;
        var throtPos = throttle_rect.anchoredPosition;
        SyncManager.SyncMessage(MessageManager.HoloMessageType.ChangeSlider, transform.name, new List<float>()
        {
            sAng,
            sRad,
            throtPos.x,
            throtPos.y
        });
    }

    public override void ManipulationUpdate(Vector3 startGesturePosition, Vector3 currentGesturePosition,
        Vector3 startHeadOrigin, Vector3 startHeadRay, GestureInteractive.GestureManipulationState gestureState)
    {
        GestureInteractiveData gestureData =
            GetGestureData(new Vector3(1, 0, 0), MaxGestureDistance, FlipDirectionOnCameraForward);
        vertData.Direction = gestureData.Direction;
        HandPosition2D.x = vertData.Direction.x * 30;
        HandPosition2D.y = vertData.Direction.y * 30;
        base.ManipulationUpdate(startGesturePosition, currentGesturePosition, startHeadOrigin, startHeadRay,
            gestureState);
    }

    public void StartCoroutineForCyclic()
    {
        StartCoroutine("TrackPointer");
    }

    public void StopCoroutineForCyclic()
    {
        StopCoroutine("TrackPointer");
    }


    protected override void Update()
    {
        base.Update();
        //HandPosition = control.CurrentGesturePosition;
        if (GestureStarted)
        {
            StartCoroutineForCyclic();
        }
        else
        {
            StopCoroutineForCyclic();
        }
    }

    // mainloop
    IEnumerator TrackPointer()
    {
        var ray = GetComponentInParent<GraphicRaycaster>();
        var input = FindObjectOfType<StandaloneInputModule>();
        var input_holo = FindObjectOfType<HoloLensInputModule>();

        var text = GetComponentInChildren<Text>();

        if (ray != null && input != null)
        {
            while (Application.isPlaying)
            {
                // TODO: if mousebutton down
                if (GestureStarted)

                {
                    Vector2 CurrentLocalPosition; // Mouse position  

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, HandPosition2D,
                        null, out CurrentLocalPosition);

                    // local pos is the mouse position.
                    //Debug.Log("HandPosition2D.x=" + HandPosition2D.x);
                    // Debug.Log("HandPosition2D.y=" + HandPosition2D.y);

                    CurrentLocalPosition.x = CurrentLocalPosition.x / 15;
                    CurrentLocalPosition.y = CurrentLocalPosition.y / 15;
                    float angle =
                        (Mathf.Atan2(-CurrentLocalPosition.y, CurrentLocalPosition.x) * 180f / Mathf.PI + 180f) / 360f;

                    GetComponent<Image>().fillAmount = angle;

                    var temAng = ((angle) * 360f);


                    if (CurrentLocalPosition.magnitude < (0.45f * thisRect.rect.width))
                    {
                        throttle_rect.anchoredPosition = CurrentLocalPosition;
                    }
                    else
                    {
                        throttle_rect.anchoredPosition = CurrentLocalPosition.normalized * (0.5f * thisRect.rect.width);
                    }

                    ;

                    float tmpRad = (CurrentLocalPosition.magnitude) / (0.5f * thisRect.rect.width);

                    this.SetAngRad(temAng, tmpRad);
                }

                //itsKGFOrbitCam.SetPanningEnable( !isPointerDown );
                yield return 0;
            }
        }
        else
            UnityEngine.Debug.LogWarning("Could not find GraphicRaycaster and/or StandaloneInputModule");
    }
}