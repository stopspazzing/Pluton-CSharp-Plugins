using Pluton;
using System;
using System.Collections.Generic;

namespace PlutonEssentials
{
    [Serializable]
    public class StructureComponent : CountedInstance
    {
        public StructureComponent(string str, SerializedVector3 v3, SerializedQuaternion q, int i)
        {
            throw new NotImplementedException();
        }

        public float Health;
        public string Prefab;
        public bool HasKeyLock;
        public bool HasCodeLock;
        public string LockCode;
        public List<ulong> LockWList;
        public BuildingGrade.Enum Grade;
        public SerializedVector3 LocalPosition;
        public SerializedQuaternion LocalRotation;

        public StructureComponent(BuildingPart bp, SerializedVector3 v3, SerializedQuaternion q)
        {
            Grade = bp.buildingBlock.grade;
            Prefab = bp.buildingBlock.LookupPrefabName();
            LocalPosition = v3;
            LocalRotation = q;
            Health = (float)((int)Math.Floor((double)(bp.Health / 85)) * 85);
            if (bp.buildingBlock.HasSlot(BaseEntity.Slot.Lock))
            {
                var baseLock = bp.buildingBlock.GetSlot(BaseEntity.Slot.Lock) as BaseLock;
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

        public override string ToString()
        {
            return String.Format("{0} [pos:{1}, rot:{2}]", Prefab, LocalPosition, LocalRotation);
        }
    }
}
