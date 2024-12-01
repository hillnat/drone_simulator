using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour, IPunObservable
{
    private int droneType=-1; //gets set by spawner, so we know which model to create
	bool initialSetupComplete => droneType != -1;
	private string[] dronePrefabNames = new string[3] { "basicDrone", "raceDrone", "tinyWhoop" };
	public Drone drone;
	public PlayerSettings playerSettings;
	public PhotonView view;
	private Camera playerCamera;
	private Rigidbody rb;
	private AudioSource aS;
	private BoxCollider mainCollider;
	private float zeroDistance = 0;
	private SkyCam skyCam;
	private Vector3 positionLastFrame = Vector3.zero;
	public LayerMask groundEffectLayerMask;
	#region Post FX
	PostProcessLayer postProcessLayer;
	PostProcessVolume postProcessVolume;
	#endregion
	#region Inputs
	private Vector3 scaledInputs =Vector3.zero;
	private float throttle = 0;
    #endregion
    #region UI References
    public GameObject allUI;
	private bool angleIconsEnabled
	{
		get { return _angleIconsEnabled; }
		set { _angleIconsEnabled = value; for (int i = 0; i < angleIcons.Length; i++) { angleIcons[i].gameObject.SetActive(_angleIconsEnabled); } }
	}
	private bool _angleIconsEnabled = true;
	public RectTransform[] angleIcons = new RectTransform[5];
	public Canvas mainUICanvas;
	public Canvas settingsCanvas;    //Store all settings fields for settings default values
    public TMP_Text nametag;
    public TMP_Text MAINUI_speedText;
	public TMP_Text MAINUI_nameText;
	public TMP_Text MAINUI_altitudeText;
	public TMP_Text MAINUI_fpsText;
	public TMP_InputField SETTINGSUI_camOffsetYInputField;
	public TMP_InputField SETTINGSUI_camOffsetZInputField;
	public TMP_InputField SETTINGSUI_camAngleInputField;
	public TMP_InputField SETTINGSUI_nameInputField;
	public Slider SETTINGSUI_soundFXSlider;
	public Slider SETTINGSUI_qualitySlider;
	public Toggle SETTINGSUI_PostFXToggle;
	public Toggle SETTINGSUI_AngleIconsToggle;
	#endregion 
	#region Trails
	private TrailRenderer trailRenderer;
	private Color[] trailColors = new Color[5]{
		Color.red, Color.green, Color.blue, Color.magenta, Color.yellow
	};
	private int curTrailColor
	{
		get { return _curTrailColor; }
		set {
			//Clamp and set
			if (value >= trailColors.Length) { _curTrailColor = 0; } 
			else if (value < 0) { _curTrailColor = trailColors.Length - 1; } 
			else { _curTrailColor = value; } 
		}
	}
	private int _curTrailColor = 0;
	#endregion
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
		mainCollider=GetComponent<BoxCollider>();
		mainCollider.size = Vector3.zero;
		mainCollider.center = Vector3.zero;
		trailRenderer.enabled = true;
		playerCamera.enabled = false;
		allUI.SetActive(false);
		if (view.IsMine)
		{
			rb = transform.AddComponent<Rigidbody>();
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Destroy(nametag.gameObject);
			//view.RPC("SetDroneType", RpcTarget.AllBufferedViaServer, 0);
		}
		else
		{
			Destroy(mainCollider);
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
			//Set quality settings
			QualitySettings.SetQualityLevel(2, true);
            //Setup camera    
            playerCamera.transform.parent = transform; 
			playerCamera.transform.localEulerAngles = new Vector3(playerSettings.cameraAngle, 0, 0);
			//Setup skycam
			skyCam = GameObject.FindWithTag("SkyCam").GetComponent<SkyCam>();    
			skyCam.target = this;
			//Setup trail
			curTrailColor = Random.Range(0, trailColors.Length);
			trailRenderer.startColor = trailColors[curTrailColor];
			trailRenderer.endColor = trailColors[curTrailColor];
			//Set UI stuff
			view.RPC("SetName", RpcTarget.AllBufferedViaServer, "Player" + $"{view.ViewID}");
			angleIconsEnabled = true;
			settingsCanvas.gameObject.SetActive(false);
			SetSettingsUIDefaultValues();
			//Setup chat
            ChatManager.instance.localPlayerID = view.ViewID;

			//Move to spawn
			transform.position = GameManager.spawn;
		}
		else
		{
			Destroy(mainUICanvas.gameObject);
			Destroy(settingsCanvas.gameObject);
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(rb);
			Destroy(mainCollider);
		}
	}
	private void FixedUpdate()
	{
		if (view.IsMine && initialSetupComplete) {            
			ApplyForces();
			HandleHudUI();
			positionLastFrame = transform.position;
		}
	}
	private void Update()
	{
		if (view.IsMine && initialSetupComplete) {
			CalculateInputs();
			SpinPropellors();
			HandleSounds();
			HandleOtherInputs();
			MAINUI_fpsText.text = $"{(int)(1f / Time.deltaTime)} FPS";
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
        drone.gameObject.transform.localPosition = drone.droneStats.droneModelPositionOffset;
        drone.gameObject.transform.localEulerAngles = drone.droneStats.droneModelRotationOffset;
        //Setup physics values can camera position if owner
        if (view.IsMine)
        {
            playerCamera.enabled = true;
            rb.mass = drone.droneStats.weight / 40f / 1000f;//Kg to g, divided by a constant of 40 so a normal drone weight of 400g is a good weight for the rb
            rb.drag = drone.droneStats.drag;
            rb.angularDrag = drone.droneStats.angularDrag;
            rb.useGravity = true;
            playerCamera.transform.localPosition = drone.droneStats.cameraOffset;
            playerCamera.fieldOfView = drone.droneStats.fieldOfView;
            mainCollider.size = drone.droneStats.colliderSize;
            mainCollider.center = drone.droneStats.colliderCenter;
			allUI.SetActive(true);
			SetSettingsUIDefaultValues();
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
	void SetTrailColor(int color)
	{
		curTrailColor = color;
		trailRenderer.startColor = trailColors[curTrailColor];
		trailRenderer.endColor = trailColors[curTrailColor];
	}
	[PunRPC]
	void ToggleTrail(bool state)
	{
		trailRenderer.enabled = state;
	}
	#endregion
	void HandleSounds() {
		aS.pitch = 1.2f+(throttle/200);
	}
	void HandleOtherInputs()
	{
		zeroDistance = Mathf.Abs(Vector2.Distance(Vector2.zero, new Vector2(transform.position.x, transform.position.z)));
		if (InputManager.instance.respawn || (zeroDistance > GameManager.worldSize || transform.position.y < -1)){ Respawn(); }//This also handles 0 distance respawn
		if (InputManager.instance.toggleSkycam) { skyCam.enabled = !skyCam.enabled; }
		if (InputManager.instance.toggleTrail) { view.RPC("ToggleTrail", RpcTarget.AllBufferedViaServer, !trailRenderer.enabled); }
		if (InputManager.instance.openMenu) { settingsCanvas.gameObject.SetActive(!settingsCanvas.gameObject.activeInHierarchy); }
		if (InputManager.instance.setSpawn && zeroDistance < GameManager.worldSize - 10 && transform.position.y >= 0) { GameManager.spawn = transform.position; GameManager.spawnRotation = transform.rotation; }
		if (InputManager.instance.flip) { transform.rotation = Quaternion.identity; }
		if (InputManager.instance.toggleChat) { ChatManager.instance.OpenChat(); }
		if (InputManager.instance.trailColor) {
			view.RPC("SetTrailColor", RpcTarget.AllBufferedViaServer, curTrailColor++);
		}    
	}
	private void CalculateInputs()
	{
		//Get rotational inputs
		Vector3 rawInputs = InputManager.instance.directionalInputs;
		
		//Evaluate on curve
		scaledInputs = new Vector3(playerSettings.pitchRollCurve.Evaluate(Mathf.Abs(rawInputs.x)), playerSettings.yawCurve.Evaluate(Mathf.Abs(rawInputs.y)), playerSettings.pitchRollCurve.Evaluate(Mathf.Abs(rawInputs.z)));
		//add y rotation
		scaledInputs = new Vector3(scaledInputs.x * drone.droneStats.pitchRollModifier, scaledInputs.y * drone.droneStats.yawSpeedModifier, scaledInputs.z * drone.droneStats.pitchRollModifier);
		//Because we used abs prior we must regain signage. Ignore throttle because it must be positive
		if (rawInputs.x != Mathf.Abs(rawInputs.x)) { scaledInputs.x *= -1; }
		if (rawInputs.y != Mathf.Abs(rawInputs.y)) { scaledInputs.y *= -1; }
		if (rawInputs.z != Mathf.Abs(rawInputs.z)) { scaledInputs.z *= -1; }
		rawInputs.x *= -1;
		//Throttle cant be negative, so clamp01
		throttle = drone.droneStats.throttleModifier * GameManager.globalSpeedModifier  * playerSettings.throttleCurve.Evaluate(Mathf.Clamp01(InputManager.instance.throttleInput));
	}
	private void ApplyForces()
	{
		//Apply rotations
		rb.AddTorque(transform.rotation * scaledInputs * Time.fixedDeltaTime);
		//Check ground effect
		bool groundEffect = Physics.Raycast(transform.position, -Vector3.up, 0.75f, groundEffectLayerMask);
		//Apply throttle
		Vector3 throttleVector = transform.up * throttle * (groundEffect ? GameManager.groundEffectMultiplier : 1f) * Time.fixedDeltaTime;
		rb.AddForce(throttleVector);
		rb.AddForce(Vector3.down * GameManager.additionalGravity * (drone.droneStats.weight/800) * Time.fixedDeltaTime);//fake more gravity
	}
	private void SpinPropellors()
	{
		for (int i = 0; i < drone.propellors.Length; i++) {
			drone.propellors[i].transform.localEulerAngles = new Vector3(0, 0, drone.propellors[i].transform.localEulerAngles.z + 6000 * Time.deltaTime * (InputManager.instance.throttleInput + 3));
		}
	}
	public void Respawn()
	{
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		transform.position = GameManager.spawn;
		transform.rotation = GameManager.spawnRotation;
	}
    #region UI
    void HandleHudUI()
    {
        MAINUI_speedText.text = $"m/s {((transform.position - positionLastFrame).magnitude / Time.fixedDeltaTime):F1}";
        MAINUI_altitudeText.text = $"alt {transform.position.y:F1}m";
        //Set the position of the UI angle icon bars
        if (angleIconsEnabled)
        {
            float angleIconRiseAmount = transform.rotation.eulerAngles.z;
            if (angleIconRiseAmount > 180) { angleIconRiseAmount -= 360; }//Account for flipping
            angleIconRiseAmount /= 2;
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) { continue; }//Skip middle bar, stays put. Although keep it in the array for other math
                angleIcons[i].anchoredPosition = new Vector2(angleIcons[i].anchoredPosition.x, (angleIconRiseAmount * (i > 2 ? -1 : 1)) * Mathf.Abs(i - 2));
            }
        }
        //(amount * mod) : get initial height, with proper sign
        //* Mathf.Abs(i-2) : multiply by i's distance from the middle point of the array. ie becasue out of 0-4, 2 is the center number, 0 and 4 would output a multiplier of 2, and 1 and 3 output a multiplier of 1
        //(i>2?-1:1) flip if over halfway through iterations
    }
    private void SetSettingsUIDefaultValues()
	{
		if (!view.IsMine) { return; }
		if (drone != null && drone.droneStats !=null)
		{
			//Note setting input field value like this still invokes any callbacks on the input field
            SETTINGSUI_camOffsetYInputField.text = $"{drone.droneStats.cameraOffset.y}";
            SETTINGSUI_camOffsetZInputField.text = $"{drone.droneStats.cameraOffset.z}";
        }
		
		SETTINGSUI_camAngleInputField.text = $"{playerSettings.cameraAngle}";
		SETTINGSUI_nameInputField.text = $"{name}";
		SETTINGSUI_soundFXSlider.value = aS.volume;
		SETTINGSUI_PostFXToggle.isOn = postProcessVolume.enabled;
		SETTINGSUI_AngleIconsToggle.isOn = angleIconsEnabled;
		SETTINGSUI_qualitySlider.value = QualitySettings.GetQualityLevel();
	}
    #endregion
    #region UI Callbacks
    public void UICALLBACK_TogglePostFX()
    {
        postProcessVolume.enabled = !postProcessVolume.enabled;
        postProcessLayer.enabled = !postProcessLayer.enabled;
    }
    public void UICALLBACK_SoundFXVolume(float v)
    {
        aS.volume = v;
    }
    public void UICALLBACK_ToggleAngleIcons()
    {
        angleIconsEnabled = !angleIconsEnabled;
    }
    public void UICALLBACK_ChangeCamAngle(string c)
    {
        float v;
        if (float.TryParse(c, out v))
        {
            playerSettings.cameraAngle = v;
            playerCamera.transform.localEulerAngles = new Vector3(playerSettings.cameraAngle, 0, 0);
        }
    }
    public void UICALLBACK_ChangeCamOffsetY(string c)
    {
        float v;
        if (float.TryParse(c, out v))
        {
            drone.droneStats.cameraOffset.y = v;
            playerCamera.transform.localPosition = drone.droneStats.cameraOffset;
        }
    }
    public void UICALLBACK_ChangeCamOffsetZ(string c)
    {
        float v;
        if (float.TryParse(c, out v))
        {
            drone.droneStats.cameraOffset.z = v;
            playerCamera.transform.localPosition = drone.droneStats.cameraOffset;
        }
    }
    public void UICALLBACK_ChangeName(string v)
    {
        name = v;
        MAINUI_nameText.text = name;
    }
    public void UICALLBACK_ChangeQuality(float v)
    {
        QualitySettings.SetQualityLevel(Mathf.Clamp((int)v, 0, 3), true);
    }
    #endregion
}
