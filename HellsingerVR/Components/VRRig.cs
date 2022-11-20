using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace HellsingerVR.Components
{
    public class VRRig : MonoBehaviour
    {
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        public Transform PlayerTransform;

        float lastheadRot;

        // Printing the character extents to log yields: (0.60, 0.80, 0.60)
        const float Height = 1.6f;

        Vector3 InitialPosition;
        Quaternion InitialRotation;

        bool InCutscene = false;

        bool _InLevel = false;

        public UI.RhythmIndicator rhythmIndicator;
        public UI.Health health;

        public VRViewModelManager viewModelManager;

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
            EnterTitleScreen();
        }

        public void EnterTitleScreen()
        {
            rhythmIndicator = null;
            health = null;

			viewModelManager.enabled = false;

			_InLevel = false;

            transform.position = HellsingerVR.TitleScreenPosition - Vector3.up * Height;

            transform.rotation = Quaternion.Euler(0.0f, -90.0f - head.transform.localRotation.eulerAngles.y, 0.0f);
        }

        public void EnterLevel()
        {
            _InLevel = true;
            InCutscene = false;

			viewModelManager.enabled = true;

			Debug.Log("Entered Level");
        }

        public void EnterCutscene()
        {
            InitialPosition = -head.localPosition;
            InitialRotation = Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);

            InCutscene = true;
            transform.parent = null;

			if (Camera.main)
			{
                Camera.main.enabled = true;
                Camera.main.cullingMask = 0;
			}

			Debug.Log("Entered cutscene");
        }

        public void ExitCutscene()
        {
            Debug.Log("Left cutscene");
            InCutscene = false;
            FirstPersonController fpController = FindObjectOfType<FirstPersonController>();
            
            lastheadRot = head.localRotation.eulerAngles.y;

            if (fpController)
            {
                /*
				transform.parent = fpController.m_player.PlayerTransform;
				transform.localPosition = -new Vector3(head.localPosition.x, 0.0f, head.localPosition.z);
				transform.localRotation = Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);
				*/

                PlayerTransform = fpController.m_player.PlayerTransform;

                transform.position = PlayerTransform.position;
                transform.rotation = PlayerTransform.rotation * Quaternion.Euler(0.0f, -head.localRotation.eulerAngles.y, 0.0f);

                //Debug.Log($"Parented to {transform.parent.gameObject.name}");
            }

            HellsingerVR.MoveOverlayToWorld();
            //PrintGameObject(HellsingerVR.overlay.gameObject, "");

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

			if (Camera.main)
			{
                Camera.main.cullingMask = 0;
				Camera.main.enabled = false;
			}

			HellsingerVR.RemoveDOF();
		}

        public void PrintGameObject(GameObject go, string indent)
        {
            Debug.Log(indent + go.name + " - " + go.transform.position);
            var allComps = go.GetComponents(Il2CppType.Of<MonoBehaviour>());
            foreach (Component mono in allComps)
            {
                Debug.Log(indent + "|-" + mono.name);
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                PrintGameObject(go.transform.GetChild(i).gameObject, indent + "|");
            }
        }

        /*
		
		Base
		|-Camera
		|-Transform
		|ChildObject
		||-Canvas
		||-RigidBody


		 */

        // Is this the right time?
        public void Update()
        {
            DebugUpdate();

            if (InLevel)
            {
                UpdateLevel();
            }
        }

        public void UpdateLevel()
        {
            Transform camTransform = null;

            if (Camera.main)
            {
                camTransform = Camera.main.transform;
            }

            if (InCutscene && camTransform != null)
            {
                // Pesky DOF
                HellsingerVR.RemoveDOF();
                transform.position = camTransform.position + InitialPosition;
                transform.rotation = Quaternion.Euler(0.0f, camTransform.rotation.eulerAngles.y, 0.0f) * InitialRotation;
                HellsingerVR.MoveOverlayToWorld();
            }

            if (!InCutscene)
            {
                UpdateTransform();

                if (rhythmIndicator != null) rhythmIndicator.Update();
                if (health != null) health.Update();
            }
        }

        public void UpdateTransform()
        {
            if (!PlayerTransform)
            {
                return;
            }

            float rot = head.localRotation.eulerAngles.y - lastheadRot;
            lastheadRot = head.localRotation.eulerAngles.y;

            PlayerTransform.rotation *= Quaternion.Euler(0.0f, rot, 0.0f);

            transform.position = PlayerTransform.position;
            transform.rotation = PlayerTransform.rotation * Quaternion.Euler(0.0f, -rot, 0.0f);
        }

        public void DebugUpdate()
        {

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (PlayerTransform)
                {
                    Transform t = PlayerTransform;
                    while (t.parent != null)
                    {
                        t = t.parent;
                    }
                    PrintGameObject(t.gameObject, "");
                }
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                foreach (Scene scene in SceneManager.GetAllScenes())
                {
                    if (!scene.isLoaded)
                    {
                        continue;
                    }
                    var allGOs = scene.GetRootGameObjects();
                    foreach (GameObject go in allGOs)
                    {
                        PrintGameObject(go, "");
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("" + Camera.main.transform.position);
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas c in canvases)
                {
                    if (c.renderMode == RenderMode.WorldSpace)
                    {
                        continue;
                    }
                    Debug.Log(c.name);
                    Debug.Log("" + c.renderMode);
                    Debug.Log("" + c.transform.position);
                    c.renderMode = RenderMode.WorldSpace;
                    c.transform.position = transform.position + Vector3.back * 2.0f + Vector3.up * Height;
                    c.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                    RectTransform rect = c.GetComponent<RectTransform>();
                    c.transform.localScale = Vector3.one * (2.0f / rect.rect.height);
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                transform.position += Vector3.forward * 0.5f;
                Debug.Log($"{transform.position}");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                transform.position += Vector3.back * 0.5f;
                Debug.Log($"{transform.position}");
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                transform.position += Vector3.left * 0.5f;
                Debug.Log($"{transform.position}");
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                transform.position += Vector3.right * 0.5f;
                Debug.Log($"{transform.position}");
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                transform.position += Vector3.down * 0.5f;
                Debug.Log($"{transform.position}");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                transform.position += Vector3.up * 0.5f;
                Debug.Log($"{transform.position}");
            }
        }
    }
}
