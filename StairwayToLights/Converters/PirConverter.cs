using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace StairwayToLights.Converters
{
  public class PirConverter : IValueConverter
  {
    private readonly Brush LIGHT_ON_COLOR = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
    private readonly Brush LIGHT_OFF_COLOR = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

    public PirConverter()
    {

    }
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      bool isLightOn = (bool)value;

      if (isLightOn)
      {
        return LIGHT_ON_COLOR;
      }
      else
      {
        return LIGHT_OFF_COLOR;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}