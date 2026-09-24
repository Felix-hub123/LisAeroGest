using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisAeroGest.Mobile.Converters
{
    /// <summary>
    /// Converte o estado do bilhete (string) num bool — true se o bilhete
    /// estiver pago e ainda não tiver feito check-in, para mostrar o botão de check-in.
    /// </summary>
    public class StatusIsPaidConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is string status && status == "Paid";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
