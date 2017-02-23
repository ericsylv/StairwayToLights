using StairwayToLights.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace StairwayToLights.ViewModels
{
  /// <summary>
  /// ViewModel representing one stair
  /// </summary>
  /// <seealso cref="StairwayToLights.Common.BindableBase" />
  /// <seealso cref="System.IDisposable" />
  public class StairViewModel : BindableBase, IDisposable
  {
    private bool _isIotConnected;
    private bool _isLightOn;
    private GpioPin _pin;

    /// <summary>
    /// Initializes a new instance of the <see cref="StairViewModel"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="pinNumber">The pin number of the stair.</param>
    /// <param name="isIotConnected">if set to <c>true</c> IoT (Raspberry PI) is connected.</param>
    public StairViewModel(int id, int pinNumber, bool isIotConnected)
    {
      _isIotConnected = isIotConnected;
      ID = id;
      PinNumber = pinNumber;
      IsLightOn = false;

      if (_isIotConnected)
      {
        InitLedsGpio();
      }
    }

    /// <summary>
    /// Gets the identifier of the stair.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public int ID { get; private set; }

    /// <summary>
    /// Gets the pin number of the stair.
    /// </summary>
    /// <value>
    /// The pin number.
    /// </value>
    public int PinNumber { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is light on.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is light on; otherwise, <c>false</c>.
    /// </value>
    public bool IsLightOn
    {
      get { return _isLightOn; }
      set
      {
        SetProperty(ref _isLightOn, value);
      }
    }

    /// <summary>
    /// Turns the light on.
    /// </summary>
    public void TurnOnLight()
    {
      IsLightOn = true;
      if (_isIotConnected)
      {
        _pin.Write(GpioPinValue.High);
      }
    }

    /// <summary>
    /// Turns the off light.
    /// </summary>
    public void TurnOffLight()
    {
      IsLightOn = false;
      if (_isIotConnected)
      {
        _pin.Write(GpioPinValue.Low);
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (_isIotConnected)
      {
        _pin.Dispose();
      }
    }
    private void InitLedsGpio()
    {
      var controller = GpioController.GetDefault();
      _pin = controller.OpenPin(PinNumber);
      _pin.SetDriveMode(GpioPinDriveMode.Output);
      _pin.Write(GpioPinValue.Low);
    }
  }
}
