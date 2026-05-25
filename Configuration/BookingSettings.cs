namespace BloomyBE.Configuration
{
    public class BookingSettings
    {
        public int MaxOrdersPerDay { get; set; } = 3;
        public int SetupDurationHours { get; set; } = 4;
        public decimal DepositPercentage { get; set; } = 0.5m;
        public string[] AllowedDistricts { get; set; } = ["Sơn Tây", "Thạch Thất", "Ba Vì"];
        public string[] HanoiKeywords { get; set; } = ["hà nội", "ha noi", "hanoi"];
    }
}
