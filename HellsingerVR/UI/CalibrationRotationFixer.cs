using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HellsingerVR.UI
{
	public class CalibrationRotationFixer : MonoBehaviour
	{
		public Transform UIElement;
		public Quaternion DesiredRot = Quaternion.identity;
		
		public void Update()
		{
			if (UIElement)
			{
				UIElement.localRotation = DesiredRot;
			}
		}
	}
}
