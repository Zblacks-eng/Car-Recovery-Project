using MobileCarRecoverySystem.Models;
using MobileCarRecoverySystem.DataStructures;

namespace MobileCarRecoverySystem.Services
{
    public class RecoveryService
    {
        private readonly RecoveryHashTable table;

        public RecoveryService()
        {
            table = new RecoveryHashTable(20);
            SeedData();
        }

        private void SeedData()
        {
            table.Add(new RecoveryCase
            {
                CaseId = 1,
                CustomerName = "Brent Li",
                PhoneNumber = "07111111111",
                VehicleRegistration = "ABC123D",
                VehicleModel = "Ford",
                BreakdownLocation = "London",
                RecoveryStatus = "Pending"
            });

            table.Add(new RecoveryCase
            {
                CaseId = 2,
                CustomerName = "Josh Carl",
                PhoneNumber = "07222222222",
                VehicleRegistration = "EFG456H",
                VehicleModel = "Tesla",
                BreakdownLocation = "Manchester",
                RecoveryStatus = "Completed"
            });
        }

        public void AddCase(RecoveryCase item)
        {
            table.Add(item);
        }

        public RecoveryCase Search(string registration)
        {
            return table.Search(registration);
        }

        public bool Remove(string registration)
        {
            return table.Remove(registration);
        }

        public List<RecoveryCase> GetAll()
        {
            return table.GetAll();
        }
    }
}