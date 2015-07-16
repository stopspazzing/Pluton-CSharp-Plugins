using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using PlutonEssentials;
using Pluton;

namespace PlutonEssentials
{
	[Serializable]
	public class Structure : CountedInstance
	{
		public string Name;
		public Origo Origo;
		public Dictionary<string, StructureComponent> StructureComponents;
		public Dictionary<string, DeployableComponent> DeployableComponents;
		public Dictionary<string, SpawnableComponent> SpawnableComponents;

		public Structure(string name)
		{
			Name = name;
			StructureComponents = new Dictionary<string, StructureComponent>();
			DeployableComponents = new Dictionary<string, DeployableComponent>();
			SpawnableComponents = new Dictionary<string, SpawnableComponent>();
		}

		public void AddComponent(BuildingPart bp)
		{
			if (Origo == null)
			{
				Origo = new Origo(new SerializedVector3(bp.Location), new SerializedQuaternion(bp.buildingBlock.transform.rotation));
			}
			var v3 = new SerializedVector3(bp.Location - Origo.Position.ToVector3());
			var q = new SerializedQuaternion(bp.buildingBlock.transform.rotation);
			var component = new StructureComponent(bp, v3, q);
			if (component == null)
			{
				Logger.LogDebug("[StructureRecorder] BuildingPart component is null!");
				return;
			}
			if (!StructureComponents.ContainsKey(component.ToString()))
			{
				StructureComponents.Add(component.ToString(), component);
			}
			else
			{
				StructureComponents[component.ToString()] = component;
			}
		}

		public void AddComponent(Deployable deployable)
		{
			if (Origo == null)
			{
				Origo = new Origo(new SerializedVector3(deployable.transform.position), new SerializedQuaternion(deployable.transform.rotation));
			}
			var v3 = new SerializedVector3(deployable.transform.position - Origo.Position.ToVector3());
			var q = new SerializedQuaternion(deployable.transform.rotation);
			var component = new DeployableComponent(deployable, v3, q);
			if (component == null)
			{
				Logger.LogDebug("[StructureRecorder] Deployable component is null!");
				return;
			}
			if (!DeployableComponents.ContainsKey(component.ToString()))
			{
				DeployableComponents.Add(component.ToString(), component);
			}
			else
			{
				DeployableComponents[component.ToString()] = component;
			}
		}

		public void AddComponent(Spawnable spawnable)
		{
			if (Origo == null)
			{
				Origo = new Origo(new SerializedVector3(spawnable.transform.position), new SerializedQuaternion(spawnable.transform.rotation));
			}
			var v3 = new SerializedVector3(spawnable.transform.position - Origo.Position.ToVector3());
			var q = new SerializedQuaternion(spawnable.transform.rotation);
			var component = new SpawnableComponent(spawnable, v3, q);
			if (component == null)
			{
				Logger.LogDebug("[StructureRecorder] Deployable component is null!");
				return;
			}
			if (!SpawnableComponents.ContainsKey(component.ToString()))
			{
				SpawnableComponents.Add(component.ToString(), component);
			}
			else
			{
				SpawnableComponents[component.ToString()] = component;
			}
		}

		public void Build(Vector3 spawnAt)
		{
			foreach (StructureComponent component in StructureComponents.Values)
			{
				Vector3 v3 = (component.LocalPosition.ToVector3() + spawnAt);
				BaseEntity ent = GameManager.server.CreateEntity(component.Prefab, v3, component.LocalRotation.ToQuaternion());
				ent.SpawnAsMapEntity();
				BuildingBlock bb = ent.GetComponent<BuildingBlock>();
				bb.blockDefinition = PrefabAttribute.server.Find<Construction>(bb.prefabID);
				bb.grade = component.Grade;
				bb.health = component.Health;
				if (bb.HasSlot(BaseEntity.Slot.Lock))
				{
					if (component.HasCodeLock)
					{
						BaseEntity baseEntity = GameManager.server.CreateEntity("build/locks/lock.code", Vector3.zero, new Quaternion());
						baseEntity.OnDeployed(bb);
						if (!string.IsNullOrEmpty(component.LockCode))
						{
							CodeLock codeLock = baseEntity.GetComponent<CodeLock>();
							codeLock.SetFlag(BaseEntity.Flags.Locked, true);
							codeLock.SetFieldValue("code", component.LockCode);
							codeLock.SetFieldValue("whitelistPlayers", component.LockWList);
						}
						baseEntity.gameObject.Identity();
						baseEntity.SetParent(bb, "lock");
						baseEntity.Spawn ();
						bb.SetSlot(BaseEntity.Slot.Lock, baseEntity);
					}
					else if (component.HasKeyLock)
					{
						BaseEntity baseEntity = GameManager.server.CreateEntity("build/locks/lock.key", Vector3.zero, new Quaternion());
						baseEntity.OnDeployed(bb);
						int code = component.LockCode.ToInt();
						if ((code & 0x80) != 0)
						{
							KeyLock keyLock = baseEntity.GetComponent<KeyLock>();
							keyLock.SetFieldValue("keycode", (code & 0x7F));
							keyLock.SetFieldValue("firstKeyCreated", true);
							keyLock.SetFlag(BaseEntity.Flags.Locked, true);
						}
						baseEntity.gameObject.Identity();
						baseEntity.SetParent(bb, "lock");
						baseEntity.Spawn ();
						bb.SetSlot(BaseEntity.Slot.Lock, baseEntity);
					}
				}
				bb.SendNetworkUpdateImmediate();
			}
			foreach (DeployableComponent component in DeployableComponents.Values)
			{
				Vector3 v3 = (component.LocalPosition.ToVector3() + spawnAt);
				GameObject gameObject = GameManager.server.FindPrefab(component.Prefab);
				BaseEntity ent = GameManager.server.CreateEntity(gameObject, v3, component.LocalRotation.ToQuaternion());
				ent.SpawnAsMapEntity();
				if (component.HasOwner)
				{
					SleepingBag sleepingBag = ent.GetComponent<SleepingBag>();
					sleepingBag.deployerUserID = component.DeployedBy;
					sleepingBag.niceName = component.BagName;
				}
				else if (component.IsCupBoard)
				{
					BuildingPrivlidge buildingPrivlidge = ent.GetComponent<BuildingPrivlidge>();
					buildingPrivlidge.authorizedPlayers = component.AuthedPlayers;
				}
				else if (component.HasStorage)
				{
					StorageContainer storageContainer = ent.GetComponent<StorageContainer>();
					var items = component.ItemList;
					foreach (var item in items)
					{
						Item newItem = ItemManager.CreateByItemID((int)item["id"], (int)item["amount"], (bool)item["blueprint"]);
						newItem.MoveToContainer(storageContainer.inventory);
					}
					if (ent.HasSlot(BaseEntity.Slot.Lock))
					{
						if (component.HasCodeLock)
						{
							BaseEntity baseEntity = GameManager.server.CreateEntity("build/locks/lock.code", Vector3.zero, new Quaternion());
							baseEntity.OnDeployed(ent);
							if (!string.IsNullOrEmpty(component.LockCode))
							{
								CodeLock codeLock = baseEntity.GetComponent<CodeLock>();
								codeLock.SetFlag(BaseEntity.Flags.Locked, true);
								codeLock.SetFieldValue("code", component.LockCode);
								codeLock.SetFieldValue("whitelistPlayers", component.LockWList);
							}
							baseEntity.gameObject.Identity();
							baseEntity.SetParent(ent, "lock");
							baseEntity.Spawn ();
							ent.SetSlot(BaseEntity.Slot.Lock, baseEntity);
						}
						else if (component.HasKeyLock)
						{
							BaseEntity baseEntity = GameManager.server.CreateEntity("build/locks/lock.key", Vector3.zero, new Quaternion());
							baseEntity.OnDeployed(ent);
							int code = component.LockCode.ToInt();
							if ((code & 0x80) != 0)
							{
								KeyLock keyLock = baseEntity.GetComponent<KeyLock>();
								keyLock.SetFieldValue("keycode", (code & 0x7F));
								keyLock.SetFieldValue("firstKeyCreated", true);
								keyLock.SetFlag(BaseEntity.Flags.Locked, true);
							}
							baseEntity.gameObject.Identity();
							baseEntity.SetParent(ent, "lock");
							baseEntity.Spawn ();
							ent.SetSlot(BaseEntity.Slot.Lock, baseEntity);
						}
					}
				}
				else if (component.HasPainting)
				{
					Signage signage = ent.GetComponent<Signage>();
					if (component.Painting != null)
					{
						byte[] painting = component.Painting;
						signage.textureID = FileStorage.server.Store(painting, FileStorage.Type.png);
					}
					if (component.PaintingLocked)
					{
						signage.SetFlag(BaseEntity.Flags.Locked, true);
					}
					signage.SendNetworkUpdate();
				}
				ent.SendNetworkUpdateImmediate();
			}
			foreach (SpawnableComponent component in SpawnableComponents.Values)
			{
				Vector3 v3 = (component.LocalPosition.ToVector3() + spawnAt);
				BaseEntity ent = GameManager.server.CreateEntity(component.Prefab, v3, component.LocalRotation.ToQuaternion());
				ent.SpawnAsMapEntity();
				ent.SendNetworkUpdateImmediate();
			}
		}

		public void Export()
		{
			string path = Path.Combine(Util.GetStructuresFolder(), Name + ".sps");
			using (var stream = new FileStream(path, FileMode.Create))
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, this);
			}
		}

		public void RemoveComponent(BuildingPart bp)
		{
			var v3 = new SerializedVector3(bp.Location - Origo.Position.ToVector3());
			var q = new SerializedQuaternion(bp.buildingBlock.transform.rotation);
			var component = new StructureComponent(bp, v3, q);
			if (StructureComponents.ContainsKey(component.ToString()))
			{
				StructureComponents.Remove(component.ToString());
			}
		}

		public void RemoveComponent(Deployable deployable)
		{
			var v3 = new SerializedVector3(deployable.transform.position - Origo.Position.ToVector3());
			var q = new SerializedQuaternion(deployable.transform.rotation);
			var component = new DeployableComponent(deployable, v3, q);
			if (DeployableComponents.ContainsKey(component.ToString()))
			{
				DeployableComponents.Remove(component.ToString());
			}
		}

		public void RemoveComponent(Spawnable spawnable)
		{
			var v3 = new SerializedVector3(spawnable.transform.position - Origo.Position.ToVector3());
			var q = new SerializedQuaternion(spawnable.transform.rotation);
			var component = new SpawnableComponent(spawnable, v3, q);
			if (SpawnableComponents.ContainsKey(component.ToString()))
			{
				SpawnableComponents.Remove(component.ToString());
			}
		}

		public override string ToString()
		{
			return String.Format("Structure ({0}, {1}) - Deployable ({0}, {2}) - Spawnable ({0}, {3})", Name, StructureComponents.Count, DeployableComponents.Count, SpawnableComponents.Count);
		}
	}
}