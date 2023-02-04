using Outsiders.Messages;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;

namespace HellsingerVR.Components
{
	public class VRRig : MonoBehaviour
	{
		public Transform head;
		public SteamVR_Camera vrCamera;
		public Camera camera;

		public Transform PlayerTransform;
		public Transform CameraTransform;
		private float LastLocalHeadYaw;

		// Printing the character extents to log yields: (0.60, 0.80, 0.60)
		private const float Height = 1.6f;
		private Vector3 InitialPosition;
		private Quaternion InitialRotation;
		private bool InCutscene = false;
		private bool _InLevel = false;
		private int CameraMask = 0;
		private int uiMask = LayerMask.GetMask("UI");

		public UI.RhythmIndicator rhythmIndicator;
		public UI.Health health;

		public VRViewModelManager viewModelManager;

		private FirstPersonController fpController;

		private Vector3 RoomscaleOffset = Vector3.zero;

		public bool InLevel
		{
			get { return _InLevel; }
		}

		public Vector3 GetHeadXZ()
		{
			return new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
		}

		public void Start()
		{
			CameraMask = vrCamera.TrueMask;
			Invoke("EnterTitleScreen", 0.1f);
		}

		public void OnBeat(OnBeatThisFrameMessage message)
		{

		}

		public void EnterTitleScreen()
		{
			rhythmIndicator = null;
			health = null;

			viewModelManager.enabled = false;

			_InLevel = false;

			transform.position = HellsingerVR.TitleScreenPosition - (Vector3.up * Height);

			transform.rotation = Quaternion.Euler(0.0f, 180.0f - head.transform.localRotation.eulerAngles.y, 0.0f);

			PlayerTransform = null;
			CameraTransform = null;

			HellsingerVR.MoveOverlayToWorld(true);
		}

		public void EnterLevel()
		{
			_InLevel = true;
			InCutscene = false;

			viewModelManager.enabled = true;

			HellsingerVR._instance.Log.LogInfo("Entered Level");
		}

		public void EnterCutscene()
		{
			InitialPosition = -head.localPosition;
			InitialRotation = Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);

			InCutscene = true;

			fpController = FindObjectOfType<FirstPersonController>();
			if (fpController)
			{
				PlayerTransform = fpController.m_player.PlayerTransform;
				CameraTransform = fpController.m_cameraTransform;
			}

			HellsingerVR.DisableUIDepth();

			HellsingerVR._instance.Log.LogInfo("Entered cutscene");
		}

		public void ExitCutscene()
		{
			HellsingerVR._instance.Log.LogInfo("Left cutscene");
			InCutscene = false;

			LastLocalHeadYaw = head.localRotation.eulerAngles.y;

			RoomscaleOffset = head.localPosition;
			RoomscaleOffset.y = 0;

			transform.position = PlayerTransform.position - RoomscaleOffset;
			transform.rotation = PlayerTransform.rotation * Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);

			OnLevelBegin();
		}

		public void OnLevelBegin()
		{
			if (!PlayerTransform)
			{
				fpController = FindObjectOfType<FirstPersonController>();
				if (fpController)
				{
					PlayerTransform = fpController.m_player.PlayerTransform;
					CameraTransform = fpController.m_cameraTransform;
				}
			}

			if (rhythmIndicator == null)
			{
				rhythmIndicator = new UI.RhythmIndicator();
			}

			if (health == null)
			{
				health = new UI.Health();
			}

			rhythmIndicator.Init();
			health.Init();

			viewModelManager.HideArms();

			HellsingerVR.MoveOverlayToWorld();
		}

		public void Update()
		{	
			// Only need to hide the world while in a level
			// Title screen, level select, etc are supposed to render their geo
			if ((HellsingerVR.IsPaused || HellsingerVR.IsLoading) && InLevel)
			{
				vrCamera.TrueMask = uiMask;
			}
			else
			{
				vrCamera.TrueMask = CameraMask;
			}

			if (InLevel)
			{
				UpdateLevel();
			}
		}

		public void LateUpdate()
		{
			// Do UI in LateUpdate so game state is most up to date
			if (InLevel && !InCutscene)
			{
				UpdateTransform();

				if (!HellsingerVR.IsPaused && !HellsingerVR.IsLoading)
				{
					HellsingerVR.MoveOverlayToWorld();
				}

				if (rhythmIndicator != null) rhythmIndicator.Update();
				if (health != null) health.Update();

			}
			else if (InCutscene && CameraTransform != null)
			{
				HellsingerVR.MoveOverlayToWorld();
			}
		}

		public void UpdateLevel()
		{
			if (InCutscene && CameraTransform != null)
			{
				transform.position = CameraTransform.position + InitialPosition;
				transform.rotation = Quaternion.Euler(0.0f, CameraTransform.rotation.eulerAngles.y, 0.0f) * InitialRotation;
			}

			if (HellsingerVR._instance.BeatVibrationStrength.Value > 0)
			{
				if (fpController?.m_player?.m_audioGameplayController != null && fpController.m_player.m_audioGameplayController.IsOnBeatThisFrame())
				{
					Debug.Log("Beat");

					float frequency = HellsingerVR._instance.BeatVibrationFrequency.Value;

					SteamVR_Input.GetVibrationAction("game", "Vibration", false).Execute(0, HellsingerVR._instance.BeatVibrationStrength.Value / 1000.0f, frequency, HellsingerVR._instance.BeatVibrationStrength.Value, SteamVR_Input_Sources.Any);
				}
			}
		}

		public void UpdateTransform()
		{
			if (!PlayerTransform)
			{
				return;
			}

			float DeltaRotation = head.localRotation.eulerAngles.y - LastLocalHeadYaw;
			LastLocalHeadYaw = head.localRotation.eulerAngles.y;

			Vector3 euler = CameraTransform.eulerAngles;

			float NewYaw = euler.y + DeltaRotation;

			if (VRInputManager.HasPendingSnapMove)
			{
				NewYaw += VRInputManager.PendingSnapDirection * HellsingerVR._instance.SnapTurningAngle.Value;
			}

			CameraTransform.rotation = Quaternion.Euler(head.rotation.eulerAngles.x, NewYaw, 0.0f);


			bool IsDoingAnimatedMove = fpController.GetCurrentMovementStateType() == MovementStateType.Overkill;

			if (!IsDoingAnimatedMove)
			{
				Vector3 RoomscaleMovement = head.localPosition - RoomscaleOffset;
				RoomscaleMovement.y = 0;

				fpController.m_player.CharacterController.Move(RoomscaleMovement);

				RoomscaleOffset = head.localPosition;
				RoomscaleOffset.y = 0;

				transform.position = PlayerTransform.position - RoomscaleOffset;
			}
			else
			{
				transform.position = PlayerTransform.position;
			}
			transform.rotation = Quaternion.Euler(0.0f, NewYaw - LastLocalHeadYaw, 0.0f);
		}
	}
}
