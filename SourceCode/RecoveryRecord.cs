using System;

namespace CST2550
{
    public class RecoveryRecord
    {
        public string NumberPlate { get; set; }
        public string CarModel { get; set; }
        public DateTime BreakdownTime { get; set; }
        public string Status { get; set; }

        // simple constructor to get started
        public RecoveryRecord(string plate, string model)
        {
            NumberPlate = plate;
            CarModel = model;
            BreakdownTime = DateTime.Now;
            Status = "Pending";
        }

        public override string ToString()
        {
            return $"Plate: {NumberPlate}, Model: {CarModel}, Time: {BreakdownTime}, Status: {Status}";
        }
    }
}
