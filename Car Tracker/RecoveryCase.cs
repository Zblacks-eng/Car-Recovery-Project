namespace MobileCarRecoverySystem.Models
{
    public class RecoveryCase
    {
        public int CaseId { get; set; }

        public string CustomerName { get; set; }

        public string PhoneNumber { get; set; }

        public string VehicleRegistration { get; set; }

        public string VehicleModel { get; set; }

        public string BreakdownLocation { get; set; }

        public string RecoveryStatus { get; set; }
    }
}