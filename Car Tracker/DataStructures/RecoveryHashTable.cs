using MobileCarRecoverySystem.Models;

namespace MobileCarRecoverySystem.DataStructures
{
    public class RecoveryHashTable
    {
        private List<RecoveryCase>[] buckets;
        private int size;

        public RecoveryHashTable(int size)
        {
            this.size = size;
            buckets = new List<RecoveryCase>[size];

            for (int i = 0; i < size; i++)
            {
                buckets[i] = new List<RecoveryCase>();
            }
        }

        private int Hash(string vehicleRegistration)
        {
            int hash = 0;

            foreach (char c in vehicleRegistration)
            {
                hash += c;
            }

            return hash % size;
        }

        public void Add(RecoveryCase recoveryCase)
        {
            int index = Hash(recoveryCase.VehicleRegistration);
            buckets[index].Add(recoveryCase);
        }

        public RecoveryCase Search(string vehicleRegistration)
        {
            int index = Hash(vehicleRegistration);

            foreach (var item in buckets[index])
            {
                if (item.VehicleRegistration == vehicleRegistration)
                {
                    return item;
                }
            }

            return null;
        }

        public bool Remove(string vehicleRegistration)
        {
            int index = Hash(vehicleRegistration);

            for (int i = 0; i < buckets[index].Count; i++)
            {
                if (buckets[index][i].VehicleRegistration == vehicleRegistration)
                {
                    buckets[index].RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public List<RecoveryCase> GetAll()
        {
            List<RecoveryCase> allCases = new List<RecoveryCase>();

            foreach (var bucket in buckets)
            {
                allCases.AddRange(bucket);
            }

            return allCases;
        }
    }
}