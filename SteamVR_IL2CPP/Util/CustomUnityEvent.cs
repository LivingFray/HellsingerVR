using System;
using System.Collections.Generic;

namespace SteamVR_IL2CPP.Util
{
	public class CustomUnityEvent<T1, T2, T3, T4>
	{
		private HashSet<Delegate> calls = new HashSet<Delegate>();

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			foreach (Delegate call in calls)
			{
				if (call != null)
				{
					call.DynamicInvoke(new object[4] { t1, t2, t3, t4 });
				}
			}
		}

		public void AddListener(Delegate d)
		{
			calls.Add(d);
		}

		public void RemoveListenser(Delegate d)
		{
			calls.Remove(d);
		}

	}
}
