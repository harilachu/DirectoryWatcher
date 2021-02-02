using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract.ContractDefinition;

namespace MT.DirectoryWatcher.UI
{
    public class RowColorConverter : IValueConverter
    {

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //FetchTamperedDirectoriesOutputDTO directoryDto;
            FetchTamperedFilesOutputDTO fileDto;

            if (value is FetchTamperedFilesOutputDTO)
            {
                fileDto = (FetchTamperedFilesOutputDTO) value;
                if(!string.Equals(fileDto.NewHash , fileDto.OldHash))
                    return new SolidColorBrush(Color.FromArgb(20, 255, 20, 121));
            }

            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}
