using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace StairwayToLights.Converters
{
  /// <summary>
  /// Converter to change color of the PIR motion sensor
  /// </summary>
  /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
  public class PirConverter : IValueConverter
  {
    private readonly Brush LIGHT_ON_COLOR = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
    private readonly Brush LIGHT_OFF_COLOR = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

    /// <summary>
    /// Initializes a new instance of the <see cref="PirConverter"/> class.
    /// </summary>
    public PirConverter()
    {

    }
    /// <summary>
    /// Converts the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="language">The language.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Converts the back.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="language">The language.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}