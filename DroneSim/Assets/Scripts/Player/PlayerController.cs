using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour, IPunObservable
{
    private int droneType=-1; //gets set by spawner, so we know which model to create
	private string[] dronePrefabNames = new string[3] { "basicDrone", "raceDrone", "tinyWhoop" };
	public Drone drone;
	public PlayerSettings playerSettings;
	public PhotonView view;
    [HideInInspector] public Camera playerCamera;
	private Rigidbody rb;
	private AudioSource aS;
	private BoxCollider mainCollider;
	private float zeroDistance = 0;
	private Vector3 positionLastFrame = Vector3.zero;
	public LayerMask groundEffectLayerMask;
	private RenderTexture vrRendTexture;
	public bool vrEnabled = false;
    public bool vrEditMode = false;
    #region Post FX
    [HideInInspector] public PostProcessLayer postProcessLayer;
    [HideInInspector]public PostProcessVolume postProcessVolume;
	#endregion
	#region Inputs
	private Vector3 scaledInputs =Vector3.zero;
	private float throttle = 0;
    #endregion
    #region UI References
    public GameObject allUI;
	public bool angleIconsEnabled
	{
		get { return _angleIconsEnabled; }
		set { _angleIconsEnabled = value; for (int i = 0; i < angleIcons.Length; i++) { angleIcons[i].gameObject.SetActive(_angleIconsEnabled); } }
	}
	private bool _angleIconsEnabled = true;
	public RectTransform[] angleIcons = new RectTransform[5];
	public Canvas mainUICanvas;
	public Canvas vrCanvas;
    public TMP_Text nametag;
    public TMP_Text MAINUI_speedText;
	public TMP_Text MAINUI_nameText;
	public TMP_Text MAINUI_altitudeText;
	public TMP_Text MAINUI_fpsText;
	public RawImage VR_leftEye;
	public RawImage VR_rightEye;
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
	public GameObject groundEffectParticles;
	private float lastGroundEffectParticlesSpawnTime = 0f;
	public bool groundEffect = false;
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
			vrRendTexture = (RenderTexture)Resources.Load("VR_RenderTexture");
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

            //Setup camera    
            playerCamera.transform.parent = transform; 
			playerCamera.transform.localEulerAngles = new Vector3(playerSettings.cameraAngle, 0, 0);
			//Setup trail
			curColor = Random.Range(0, colors.Length);
			trailRenderer.startColor = colors[curColor];
			trailRenderer.endColor = colors[curColor];
			//Set UI stuff
			view.RPC("SetName", RpcTarget.AllBufferedViaServer, "Player" + $"{view.ViewID}");
			angleIconsEnabled = true;
			//Move to spawn
			transform.position = GameManager.instance.levelRules.spawn;
			//Setup VR
			if (vrEnabled) { EnableVr(); }
			else { DisableVr(); }
		}
		else
		{
			Destroy(mainUICanvas.gameObject);
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(rb);
			Destroy(mainCollider);
		}
	}
	private void FixedUpdate()
	{
		if (view.IsMine && drone!=null) {
			HandleGroundEffect();
            HandleHudUI();
			positionLastFrame = transform.position;
			if (vrEnabled && vrEditMode)
			{
				HandleVrEditMode();
			}
			else
			{
                ApplyForces();
            }
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
	void HandleSounds() {
		aS.pitch = 1.2f+(throttle/200);
		aS.volume = SettingsManager.instance.playerSettings.masterVolume * SettingsManager.instance.playerSettings.soundFxVolume;
	}
	void HandleOtherInputs()
	{
		zeroDistance = Mathf.Abs(Vector2.Distance(Vector2.zero, new Vector2(transform.position.x, transform.position.z)));
		if (InputManager.instance.respawn || (zeroDistance > GameManager.instance.levelRules.worldSize || transform.position.y < -1)){ Respawn(); }//This also handles 0 distance respawn
		if (InputManager.instance.toggleSkycam) { SkyCamManager.instance.skyCam.enabled = !SkyCamManager.instance.skyCam.enabled; }
		if (InputManager.instance.toggleTrail) { view.RPC("ToggleTrail", RpcTarget.AllBufferedViaServer, !trailRenderer.enabled); }
		if (InputManager.instance.openMenu) { SettingsManager.instance.ToggleUI(); }
		if (InputManager.instance.setSpawn && zeroDistance < GameManager.instance.levelRules.worldSize - 10 && transform.position.y >= 0) { GameManager.instance.levelRules.spawn = transform.position; GameManager.instance.levelRules.spawnRotation = transform.rotation; }
		if (InputManager.instance.flip) { transform.rotation = Quaternion.identity; }
		if (InputManager.instance.toggleChat) { ChatManager.instance.OpenChat(); }
		if (InputManager.instance.color) {
			view.RPC("SetColor", RpcTarget.AllBufferedViaServer, curColor++);
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
		throttle = drone.droneStats.throttleModifier * GameManager.instance.levelRules.globalSpeedModifier  * playerSettings.throttleCurve.Evaluate(Mathf.Clamp01(InputManager.instance.throttleInput));
	}
	private void ApplyForces()
	{
		//Apply rotations
		rb.AddTorque(transform.rotation * scaledInputs * Time.fixedDeltaTime);

		//Apply throttle
		rb.AddForce(transform.up * throttle * (groundEffect ? GameManager.instance.levelRules.groundEffectMultiplier : 1f) * Time.fixedDeltaTime);
        //fake more gravity
        rb.AddForce(Vector3.down * GameManager.instance.levelRules.additionalGravity * (drone.droneStats.weight/800) * Time.fixedDeltaTime);

	}
	private void SpinPropellors()
	{
		for (int i = 0; i < drone.propellors.Length; i++) {
			drone.propellors[i].transform.localEulerAngles = new Vector3(0, 0, drone.propellors[i].transform.localEulerAngles.z + 6000 * Time.deltaTime * (InputManager.instance.throttleInput + 3));
		}
	}
	public void HandleVrEditMode() {
		if (!vrEditMode || !vrEnabled) { return; }
        if (InputManager.instance.directionalInputs.x != 0f || InputManager.instance.directionalInputs.z != 0) { ChangeVrEyePos(new Vector2(-InputManager.instance.directionalInputs.z, InputManager.instance.directionalInputs.x)); }
        if (InputManager.instance.directionalInputs.y != 0f) { ChangeVrEyeSize(InputManager.instance.directionalInputs.y / 300f); }
    }

	public void Respawn()
	{
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		transform.position = GameManager.instance.levelRules.spawn;
		transform.rotation = GameManager.instance.levelRules.spawnRotation;
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
				angleIcons[i].anchoredPosition = new Vector2(angleIcons[i].anchoredPosition.x, (angleIconRiseAmount * (i > 2 ? 1 : -1)) * Mathf.Abs(i - 2));
			}
		}
        //(amount * mod) : get initial height, with proper sign
        //* Mathf.Abs(i-2) : multiply by i's distance from the middle point of the array. ie becasue out of 0-4, 2 is the center number, 0 and 4 would output a multiplier of 2, and 1 and 3 output a multiplier of 1
        //(i>2?-1:1) flip if over halfway through iterations
    }

	#endregion
	#region VR
	public void ChangeVrEyePos(Vector2 amount)
	{
		VR_rightEye.transform.Translate(VR_rightEye.transform.right * amount.x);
		VR_rightEye.transform.Translate(VR_rightEye.transform.up * amount.y);
		VR_leftEye.transform.Translate(-VR_leftEye.transform.right * amount.x);
		VR_leftEye.transform.Translate(VR_leftEye.transform.up * amount.y);

        SettingsManager.instance.playerSettings.eyePosition = VR_rightEye.transform.localPosition;

    }
    public void ChangeVrEyeSize(float amount)
    {
		VR_rightEye.transform.localScale += Vector3.one * amount;
        VR_leftEye.transform.localScale += Vector3.one * amount;

		SettingsManager.instance.playerSettings.eyeSize = VR_rightEye.transform.localScale;
    }
	public void SetVrDefaults()
	{
		Vector3 pos = SettingsManager.instance.playerSettings.eyePosition;
        VR_rightEye.transform.localPosition = pos;
        VR_leftEye.transform.localPosition = new Vector3(-pos.x, pos.y, pos.z);//Flip offset for left side
        VR_rightEye.transform.localScale = SettingsManager.instance.playerSettings.eyeSize;
        VR_leftEye.transform.localScale = SettingsManager.instance.playerSettings.eyeSize;
    }
	public void EnableVr()
	{
		vrEnabled = true;
        vrCanvas.gameObject.SetActive(true);
        vrCanvas.enabled = true;
		playerCamera.targetTexture = vrRendTexture;
        mainUICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mainUICanvas.worldCamera = playerCamera;
        mainUICanvas.planeDistance = 0.5f;
		SetVrDefaults();
		vrEditMode = true;
    }
    public void DisableVr()
    {
        vrEnabled = false;
		vrEditMode = false;
        vrCanvas.gameObject.SetActive(false);
		vrCanvas.enabled = false;
        playerCamera.targetTexture = null;
		mainUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
    #endregion
}
