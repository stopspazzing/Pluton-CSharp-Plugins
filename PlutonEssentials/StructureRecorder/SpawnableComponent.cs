using Pluton;
using System;

namespace PlutonEssentials
{
	[Serializable]
	public class SpawnableComponent : CountedInstance
	{
		public string Prefab;
		public SerializedVector3 LocalPosition;
		public SerializedQuaternion LocalRotation;

		public SpawnableComponent(Spawnable spawnable, SerializedVector3 v3, SerializedQuaternion q)
		{
			Prefab = spawnable.GetComponent<BaseNetworkable>().LookupPrefabName();
			LocalPosition = v3;
			LocalRotation = q;
		}

		public override string ToString()
		{
			return String.Format("{0} [pos:{1}, rot:{2}]", Prefab, LocalPosition, LocalRotation);
		}
	}
}

