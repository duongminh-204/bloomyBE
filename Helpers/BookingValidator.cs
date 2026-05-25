using BloomyBE.Configuration;

namespace BloomyBE.Helpers
{
    public static class BookingValidator
    {
        public static bool IsValidServiceArea(string address, string district, BookingSettings settings, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(district))
            {
                errorMessage = "Vui lòng nhập đầy đủ địa chỉ và quận/huyện.";
                return false;
            }

            var addrLower = address.ToLowerInvariant();
            var hasHanoi = settings.HanoiKeywords.Any(kw => addrLower.Contains(kw));
            if (!hasHanoi)
            {
                errorMessage = "Địa chỉ phải thuộc thành phố Hà Nội (chứa từ khóa Hà Nội / Ha Noi / Hanoi).";
                return false;
            }

            if (!settings.AllowedDistricts.Contains(district))
            {
                errorMessage = "Bloomy chỉ phục vụ tại 3 khu vực: Sơn Tây, Thạch Thất và Ba Vì.";
                return false;
            }

            return true;
        }

        public static bool IsFutureOrToday(DateTime eventDate, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (eventDate.Date < DateTime.UtcNow.Date)
            {
                errorMessage = "Ngày tổ chức phải là hôm nay hoặc trong tương lai.";
                return false;
            }
            return true;
        }
    }
}
