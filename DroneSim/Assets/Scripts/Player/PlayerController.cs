using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour, IPunObservable
{
	public enum FlightModes {Acro, Angle }
	public float angleModeRate = 5f;
	public FlightModes flightModes = FlightModes.Acro;
    private int droneType=-1; //gets set by spawner, so we know which model to create
	private string[] dronePrefabNames = new string[1] { "tinywhoop65mm" };
	public Drone drone;
	private PlayerSettings playerSettings;
	public PhotonView view;
    [HideInInspector] public Camera playerCamera;
	private Rigidbody rb;
	private AudioSource aS;
	private float zeroDistance = 0;
	private Vector3 positionLastFrame = Vector3.zero;
	public LayerMask groundEffectLayerMask;
    #region Post FX
    [HideInInspector] public PostProcessLayer postProcessLayer;
    [HideInInspector]public PostProcessVolume postProcessVolume;
	#endregion
	#region Inputs
	private Vector4 scaledInputs =Vector4.zero;//w = throttle
	[SerializeField]private Vector3 idealAngle = Vector3.zero;
    #endregion
    #region UI References
    public GameObject allUI;
	public RectTransform horizonLinesParent;
	public RectTransform osdParent;
    public TMP_Text nametag;
    public TMP_Text MAINUI_speedText;
	public TMP_Text MAINUI_nameText;
	public TMP_Text MAINUI_altitudeText;
	public TMP_Text MAINUI_fpsText;
	public TMP_Text MAINUI_timerText;
	public Image MAINUI_cameraNoise;
	#endregion 
	#region Trails
	private TrailRenderer trailRenderer;
	private Color[] colors = new Color[9]{
		Color.red, Color.green, Color.blue, Color.magenta, Color.yellow, Color.blue+Color.white/2, Color.red+Color.white/2,Color.green+Color.white/2,Color.magenta+Color.white/2
    };
	private int curColor
	{
		get { return _curTrailColor; }
		set {
			//Clamp and set
			if (value >= colors.Length) { _curTrailColor = 0; } 
			else if (value < 0) { _curTrailColor = colors.Length - 1; } 
			else { _curTrailColor = value; } 
		}
	}
	private int _curTrailColor = 0;
    #endregion
    #region Ground Effect
    public GameObject groundEffectParticles;
	private float lastGroundEffectParticlesSpawnTime = 0f;
	public bool groundEffect = false;
    #endregion
    #region Impact Audio
    public List<AudioClip> impactAudioClips = new List<AudioClip>();
	private AudioClip getImpactAudioClip => impactAudioClips[Random.Range(0, impactAudioClips.Count)];
	#endregion
	private float droneAudioPitch = 1f;
    [SerializeField]private float droneAudioPitchChangeSpeed = 15f;
    [SerializeField] private float droneAudioPitchThrottleMod = 1.5f;
    [SerializeField] private float baseDroneAudioPitch = 0.9f;
	public float timer = 0f;
	public bool timerActive = false;
	public List<float> timerLaps = new List<float>();
	private string timerLapsText="";
	public Hoop lastHoopHit;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			//stream.SendNext(droneType);
			stream.SendNext((Vector3)transform.position);
			stream.SendNext((Quaternion)transform.rotation);
			//stream.SendNext((float)aS.pitch);
			//stream.SendNext(curTrailColor);
			//stream.SendNext(tR.enabled);
		}
		else if (stream.IsReading)
		{
			//droneType = (int)stream.ReceiveNext();
			transform.position = (Vector3)stream.ReceiveNext();
			transform.rotation = (Quaternion)stream.ReceiveNext();
			//aS.pitch = (float)stream.ReceiveNext();
		}
	}
	#region Unity Callbacks
	private void Awake()
	{
		trailRenderer = GetComponentInChildren<TrailRenderer>();
		aS = GetComponent<AudioSource>();
		view = GetComponent<PhotonView>();
		postProcessVolume=GetComponentInChildren<PostProcessVolume>();
		postProcessLayer= GetComponentInChildren<PostProcessLayer>();
		playerCamera = GetComponentInChildren<Camera>();
		trailRenderer.enabled = true;
		playerCamera.enabled = false;
		allUI.SetActive(false);
		if (view.IsMine)
		{
            playerSettings = (PlayerSettings)Resources.Load("DefaultPlayerSettings");

            rb = transform.AddComponent<Rigidbody>();
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Destroy(nametag.gameObject);
			//view.RPC("SetDroneType", RpcTarget.AllBufferedViaServer, 0);
		}
		else
		{
			Destroy(postProcessVolume);
			Destroy(postProcessLayer);
			Destroy(playerCamera.gameObject);
			Destroy(allUI);
		}

	}

	private void Start()
	{        
		if (view.IsMine)
		{
			drone = null;

            //Setup camera    
            playerCamera.transform.parent = transform; 
			playerCamera.transform.localEulerAngles = new Vector3(playerSettings.cameraAngle, 0, 0);
			//Setup trail
			curColor = Random.Range(0, colors.Length);
			trailRenderer.startColor = colors[curColor];
			trailRenderer.endColor = colors[curColor];
			//Set UI stuff
			view.RPC("SetName", RpcTarget.AllBufferedViaServer, "Player" + $"{view.ViewID}");
			//Move to spawn
			transform.position = GameManager.instance.levelRules.spawn;


			SetOsdElementsToOsdElementDatas();
		}
		else
		{
			Destroy(osdParent.gameObject);
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(rb);
		}
	}
	private void FixedUpdate()
	{
		if (view.IsMine && drone!=null) {
			HandleGroundEffect();
            HandleHudUI();
			positionLastFrame = transform.position;
            ApplyForces();

        }
    }
	private void Update()
	{
		if (view.IsMine && drone != null) {
            CalculateInputs();
			SpinPropellors();
			HandleSounds();
			HandleOtherInputs();
			MAINUI_fpsText.text = $"{(int)(1f / Time.deltaTime)} FPS";
			if (timerActive) { timer += Time.deltaTime; }
		}
	}
	#endregion
    #region RPCs
    [PunRPC]
    public void SetDroneType(int newDroneType)
    {
        //Destroy old drone
        if (drone != null) { Destroy(drone.gameObject); }
        //Set new drone type
        newDroneType = Mathf.Clamp(newDroneType, 0, dronePrefabNames.Length);
        droneType = newDroneType;
        //Spawn new drone
        drone = Instantiate((GameObject)Resources.Load($"Drones/{dronePrefabNames[droneType]}"), Vector3.zero, Quaternion.identity).GetComponent<Drone>();
        //Set positions
        drone.gameObject.transform.parent = transform;
		drone.gameObject.transform.localPosition = Vector3.zero;
		drone.gameObject.transform.localEulerAngles = Vector3.zero;
        //Setup physics values can camera position if owner
        if (view.IsMine)
        {
            playerCamera.enabled = true;
            rb.mass = drone.droneStats.weight / 40f / 1000f;//Kg to g, divided by a constant of 40 so a normal drone weight of 400g is a good weight for the rb
            rb.drag = drone.droneStats.drag;
            rb.angularDrag = drone.droneStats.angularDrag;
            rb.useGravity = true;
            playerCamera.transform.position = drone.cameraMount.position;
            playerCamera.fieldOfView = drone.droneStats.fieldOfView;
			aS.Play();
			allUI.SetActive(true);
			SettingsManager.instance.SetDefaultValues();
        }
    }
	[PunRPC]
	void SetName(string newName)
	{
		if (view.IsMine)
		{
			MAINUI_nameText.text = name;
		}
		else
		{
            nametag.text = newName;
        }
        name = newName;
	}
    [PunRPC]
	void SetColor(int color)
	{
		curColor = color;
		nametag.color = colors[curColor];
		trailRenderer.startColor = colors[curColor];
		trailRenderer.endColor = colors[curColor];
	}
	[PunRPC]
	void ToggleTrail(bool state)
	{
		trailRenderer.enabled = state;
	}
	#endregion
	public void HandleGroundEffect()
	{
        groundEffect = Physics.Raycast(transform.position, -Vector3.up, 0.75f, groundEffectLayerMask);
		if (groundEffect && GameManager.instance.time > lastGroundEffectParticlesSpawnTime + 0.5f) {
			lastGroundEffectParticlesSpawnTime = GameManager.instance.time;
            Instantiate(groundEffectParticles, transform.position, Quaternion.identity, null);
		}
    }
	Vector3 GetHorizonPoint()
	{
		Vector3 v= transform.forward * 999999f;
		return new Vector3(v.x,0,v.z);
	}
	void HandleSounds() {
		float desiredPitch = baseDroneAudioPitch + (InputManager.instance.throttleInput * droneAudioPitchThrottleMod);

        droneAudioPitch = Mathf.Lerp(droneAudioPitch, desiredPitch, Time.deltaTime * droneAudioPitchChangeSpeed);
		aS.pitch = droneAudioPitch;
		aS.volume = SettingsManager.instance.playerSettings.masterVolume * SettingsManager.instance.playerSettings.soundFxVolume;
	}
	void HandleOtherInputs()
	{
		zeroDistance = Mathf.Abs(Vector2.Distance(Vector2.zero, new Vector2(transform.position.x, transform.position.z)));
		if (InputManager.instance.respawn || (zeroDistance > GameManager.instance.levelRules.worldSize || transform.position.y < -1)){ Respawn(); }//This also handles 0 distance respawn
		if (InputManager.instance.toggleSkycam) { SkyCamManager.instance.skyCam.enabled = !SkyCamManager.instance.skyCam.enabled; }
		if (InputManager.instance.toggleTrail) { view.RPC("ToggleTrail", RpcTarget.AllBufferedViaServer, !trailRenderer.enabled); }
		if (InputManager.instance.openMenu) { SettingsManager.instance.ToggleUI(); }
		if (InputManager.instance.setSpawn && zeroDistance < GameManager.instance.levelRules.worldSize - 10 && transform.position.y >= 0) { GameManager.instance.SetSpawn(transform.position, transform.rotation); }
		if (InputManager.instance.flip) { transform.rotation = Quaternion.identity; }
		if (InputManager.instance.toggleChat) { ChatManager.instance.OpenChat(); }
		if (InputManager.instance.color) {
			view.RPC("SetColor", RpcTarget.AllBufferedViaServer, curColor++);
		}    
	}
	private void CalculateInputs()
	{
		//Get rotational inputs
		Vector4 rawInputs = new Vector4(InputManager.instance.directionalInputs.x, InputManager.instance.directionalInputs.y, InputManager.instance.directionalInputs.z, InputManager.instance.throttleInput);

        //Evaluate on curve
        scaledInputs = new Vector4(
			playerSettings.pitchRollCurve.Evaluate(Mathf.Abs(rawInputs.x)) * drone.droneStats.pitchRollModifier, 			
			playerSettings.yawCurve.Evaluate(Mathf.Abs(rawInputs.y)) * drone.droneStats.yawSpeedModifier, 
			playerSettings.pitchRollCurve.Evaluate(Mathf.Abs(rawInputs.z)) * drone.droneStats.pitchRollModifier,
            playerSettings.throttleCurve.Evaluate(rawInputs.w) * drone.droneStats.throttleModifier * GameManager.instance.levelRules.globalSpeedModifier);


        idealAngle = new Vector3(
			Mathf.Lerp(0f, playerSettings.angleModeMaxAngle, scaledInputs.x), 
			0f, 
			Mathf.Lerp(0f, playerSettings.angleModeMaxAngle, scaledInputs.z));//Calculate before regaining singage for lerp calls

        //Because we used abs prior to evaluate on curve we must regain signage. Ignore throttle because it must be positive
        if (rawInputs.x != Mathf.Abs(rawInputs.x)) { scaledInputs.x *= -1f; idealAngle.x *= -1f; }
		if (rawInputs.y != Mathf.Abs(rawInputs.y)) { scaledInputs.y *= -1f; idealAngle.y *= -1f; }
        if (rawInputs.z != Mathf.Abs(rawInputs.z)) { scaledInputs.z *= -1f; idealAngle.z *= -1f; }

		//Debug.Log($"RAW : X {rawInputs.x:F2}, Y {rawInputs.y:F2}, Z {rawInputs.z:F2}, T {rawInputs.w:F2}\nSCALED : X {scaledInputs.x:F2}, Y {scaledInputs.y:F2}, Z {scaledInputs.z:F2}, T {scaledInputs.w:F2}");
	}
	private void ApplyForces()
	{
		//Apply rotations
		switch (flightModes)
		{
			case FlightModes.Acro:
                transform.Rotate((Vector3)scaledInputs * Time.fixedDeltaTime * 300f);
				break;
			case FlightModes.Angle:
				transform.Rotate(Vector3.up * scaledInputs.y * Time.fixedDeltaTime * 300f);//Apply yaw normally

				transform.rotation = Quaternion.Euler(new Vector3(
                    Mathf.LerpAngle(transform.eulerAngles.x, idealAngle.x, Time.fixedDeltaTime * angleModeRate),
                    transform.eulerAngles.y,
                    Mathf.LerpAngle(transform.eulerAngles.z, idealAngle.z, Time.fixedDeltaTime * angleModeRate)));
                break;
			default:
				break;
		}
        //Apply throttle
        rb.AddForce(transform.up * scaledInputs.w * (groundEffect ? GameManager.instance.levelRules.groundEffectMultiplier : 1f) * Time.fixedDeltaTime);
        //fake more gravity
        rb.AddForce(Vector3.down * GameManager.instance.levelRules.additionalGravity * (drone.droneStats.weight/550f) * Time.fixedDeltaTime);

	}
	private void SpinPropellors()
	{
		for (int i = 0; i < drone.propellors.Length; i++) {
			bool even = i%2 == 0;//Flip every other prop
			float amount = 6000 * Time.deltaTime * (InputManager.instance.throttleInput + 3) * (even?1f:-1f);

            drone.propellors[i].transform.localEulerAngles = drone.propellors[i].transform.localEulerAngles+drone.propAxis*amount;
		}
	}


	public void Respawn()
	{
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		transform.position = GameManager.instance.levelRules.spawn;
		transform.rotation = GameManager.instance.levelRules.spawnRotation;
		trailRenderer.Clear();
		SetTimer(false);
	}
    #region Race Timer
    public void SetTimer(bool timerOn)
	{
		bool prior = timerActive;
		timerActive=timerOn;
		if (prior && timerActive || prior && !timerActive) { timerLaps.Add(timer);timer = 0; UpdateTimerLapsText(); }//If turned on while already on, count that as a lap complete,, or the end gate
		else if (!prior && timerActive) { timer = 0f; timerLaps.Clear(); UpdateTimerLapsText(); }//Starting for the first time
    }
	private void UpdateTimerLapsText()
	{
		timerLapsText = "";
		for (int i = timerLaps.Count-1; i >= 0; i--)
		{
			timerLapsText += $"l{i+1} {timerLaps[i]:F3}\n";
		}
	}
	public void SetLastHitHoop(Hoop hoop)
	{
		lastHoopHit = hoop;
	}
    #endregion
    #region UI
	//Set OSD UI elements to the values we have saved for them in playersettings
    public void SetOsdElementsToOsdElementDatas()
    {
        OSD_Element[] OSD_Elements = osdParent.GetComponentsInChildren<OSD_Element>();
        Debug.Log($"OSD {OSD_Elements.Length},{playerSettings.allOsdElemDatas.Length}");

        for (int i = 0; i < OSD_Elements.Length; i++)
        {
            for (int j = 0; j < playerSettings.allOsdElemDatas.Length; j++)
            {

                if (playerSettings.allOsdElemDatas[j].elementName == OSD_Elements[i].elementName)
                {
                    Debug.Log($"Moving OSD element {playerSettings.allOsdElemDatas[j].elementName},{OSD_Elements[i].elementName}");
					//Set positions
                    ((RectTransform)OSD_Elements[i].transform).anchoredPosition= playerSettings.allOsdElemDatas[j].position;
                    OSD_Elements[i].transform.localScale = playerSettings.allOsdElemDatas[j].scale;

					//OSD_Elements[i].gameObject.SetActive(playerSettings.allOsdElemDatas[j].elementEnabled); Dont do this because GetComponentsInChildren<OSD_Element>() wont return objects that are set to inactive
					bool showElement = playerSettings.allOsdElemDatas[j].elementEnabled;
					//Enable/disable the OSD elements text or image component
                    if (OSD_Elements[i].transform.TryGetComponent(out Image image)) { image.enabled = showElement; }
                    else if (OSD_Elements[i].transform.TryGetComponent(out TMP_Text text)) { text.enabled = showElement; }
                }
            }
        }
    }
    void HandleHudUI()
    {
        MAINUI_speedText.text = $"m/s {((transform.position - positionLastFrame).magnitude / Time.fixedDeltaTime):F1}";
        MAINUI_altitudeText.text = $"alt {transform.position.y:F1}m";
        MAINUI_timerText.text = $"t {timer:F3}\n"+timerLapsText;
        horizonLinesParent.position = playerCamera.WorldToScreenPoint(GetHorizonPoint());
        horizonLinesParent.position = new Vector3(horizonLinesParent.position.x, horizonLinesParent.position.y, 0f);
		MAINUI_cameraNoise.transform.position -= new Vector3(0, Time.fixedDeltaTime * Random.Range(-25f,25f), 0);
		if (MAINUI_cameraNoise.transform.position.y < -500f) { MAINUI_cameraNoise.transform.position += new Vector3(0, 900f, 0); }
		if (MAINUI_cameraNoise.transform.position.y > 500f) { MAINUI_cameraNoise.transform.position -= new Vector3(0, 900f, 0); }
    }

	#endregion
	

    private void OnCollisionEnter(Collision collision)
    {
		Debug.Log($"rb vel mag {rb.velocity.magnitude}");
		if (rb.velocity.magnitude > 5f)
		{
            Debug.Log($"thingy");

            AudioManager.instance.PlaySound(getImpactAudioClip, transform.position);
		}
    }
}
