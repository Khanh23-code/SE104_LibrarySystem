using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace THUVIENZ.Converters
{
    /// <summary>
    /// Chuyển đổi chuỗi trạng thái mượn sách thành màu nền badge tương ứng.
    /// </summary>
    public class StatusToBadgeBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "Quá hạn"    => new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)), // đỏ nhạt
                "Đang mượn"  => new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE)), // xanh nhạt
                "Đã trả"     => new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)), // xanh lá nhạt
                _            => new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Chuyển đổi chuỗi trạng thái mượn sách thành màu chữ badge tương ứng.
    /// </summary>
    public class StatusToBadgeForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "Quá hạn"    => new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)), // đỏ
                "Đang mượn"  => new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8)), // xanh dương
                "Đã trả"     => new SolidColorBrush(Color.FromRgb(0x05, 0x96, 0x69)), // xanh lá
                _            => new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
