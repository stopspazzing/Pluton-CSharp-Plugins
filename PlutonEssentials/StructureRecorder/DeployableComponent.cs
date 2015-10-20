using System;
using System.Collections.Generic;
using Pluton;

namespace PlutonEssentials
{
    [Serializable]
    public class DeployableComponent : CountedInstance
    {
        public string BagName;
        public string Prefab;
        public bool IsCupBoard;
        public bool HasOwner;
        public bool HasKeyLock;
        public bool HasCodeLock;
        public bool HasStorage;
        public bool HasPainting;
        public ulong DeployedBy;
        public string LockCode;
        public byte[] Painting;
        public bool PaintingLocked;
        public List<ulong> LockWList;
        public SerializedVector3 LocalPosition;
        public SerializedQuaternion LocalRotation;
        public List<ProtoBuf.PlayerNameID> AuthedPlayers;
        public List<Dictionary<string, object>> ItemList;

        public DeployableComponent(Deployable deployable, SerializedVector3 v3, SerializedQuaternion q)
        {
            Prefab = deployable.GetComponent<BaseNetworkable>().LookupPrefabName();
            LocalPosition = v3;
            LocalRotation = q;
            if (deployable.GetComponent<SleepingBag>())
            {
                SleepingBag sleepingBag = deployable.GetComponent<SleepingBag>();
                DeployedBy = sleepingBag.deployerUserID;
                BagName = sleepingBag.niceName;
                HasOwner = true;
                HasStorage = false;
                HasPainting = false;
                IsCupBoard = false;
            }
            else if (deployable.GetComponent<BuildingPrivlidge>())
            {
                IsCupBoard = true;
                BuildingPrivlidge buildingPrivlidge = deployable.GetComponent<BuildingPrivlidge>();
                AuthedPlayers = new List<ProtoBuf.PlayerNameID>();
                AuthedPlayers = buildingPrivlidge.authorizedPlayers;
            }
            else if (deployable.GetComponent<StorageContainer>())
            {
                HasOwner = false;
                HasStorage = true;
                HasPainting = false;
                IsCupBoard = false;
                StorageContainer storageContainer = deployable.GetComponent<StorageContainer>();
                ItemList = new List<Dictionary<string, object>>();
                foreach (Item item in storageContainer.inventory.itemList)
                {
                    var itemData = new Dictionary<string, object>();
                    itemData.Add("blueprint", item.IsBlueprint());
                    itemData.Add("id", item.info.itemid);
                    itemData.Add("amount", item.amount);
                    ItemList.Add(itemData);
                }
                if (storageContainer.HasSlot(BaseEntity.Slot.Lock))
                {
                    var baseLock = storageContainer.GetSlot(BaseEntity.Slot.Lock) as BaseLock;
                    if (baseLock == null)
                    {
                        HasCodeLock = false;
                        HasKeyLock = false;
                    }
                    else if (baseLock.GetComponent<CodeLock>())
                    {
                        HasCodeLock = true;
                        HasKeyLock = false;
                        CodeLock codeLock = baseLock.GetComponent<CodeLock>();
                        if (!string.IsNullOrEmpty((string)codeLock.GetFieldValue("code")))
                        {
                            LockCode = (string)codeLock.GetFieldValue("code");
                            LockWList = new List<ulong>();
                            LockWList = (List<ulong>)codeLock.GetFieldValue("whitelistPlayers");
                        }
                    }
                    else if (baseLock.GetComponent<KeyLock>())
                    {
                        HasCodeLock = false;
                        HasKeyLock = true;
                        KeyLock keyLock = baseLock.GetComponent<KeyLock>();
                        int keyCode = (int)keyLock.GetFieldValue("keyCode");
                        keyCode = (bool)keyLock.GetFieldValue("firstKeyCreated") ? keyCode |= 0x80 : (int)keyLock.GetFieldValue("keyCode");
                        LockCode = keyCode.ToString();
                    }
                }
            }
            else if (deployable.GetComponent<Signage>())
            {
                HasOwner = false;
                HasStorage = false;
                HasPainting = true;
                IsCupBoard = false;
                Signage signage = deployable.GetComponent<Signage>();
                byte[] tempImg = FileStorage.server.Get(signage.textureID, FileStorage.Type.png, signage.net.ID);
                if (signage.textureID > 0 && tempImg != null) Painting = tempImg;
                PaintingLocked = signage.IsLocked();
            }
            else
            {
                HasOwner = false;
                HasStorage = false;
                HasPainting = false;
                IsCupBoard = false;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} [pos:{1}, rot:{2}]", Prefab, LocalPosition, LocalRotation);
        }
    }
}

