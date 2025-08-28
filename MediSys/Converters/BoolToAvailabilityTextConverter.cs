using System.Globalization;

namespace MediSys.Converters
{
	public class BoolToAvailabilityTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool disponible)
			{
				return disponible ? "DISPONIBLE" : "OCUPADO";
			}
			return "N/A";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}