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
  public class StairViewModel : BindableBase, IDisposable
  {
    private bool _isIotConnected;
    private bool _isLightOn;
    private GpioPin _pin;

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

    public int ID { get; private set; }
    public int PinNumber { get; private set; }

    public bool IsLightOn
    {
      get { return _isLightOn; }
      set
      {
        SetProperty(ref _isLightOn, value);
      }
    }

    public void TurnOnLight()
    {
      IsLightOn = true;
      if (_isIotConnected)
      {
        _pin.Write(GpioPinValue.High);
      }

      Debug.WriteLine(string.Format("Stair #{0} light is ON ({1})", ID, DateTime.Now.ToString()));
    }

    public void TurnOffLight()
    {
      IsLightOn = false;
      if (_isIotConnected)
      {
        _pin.Write(GpioPinValue.Low);
      }

      Debug.WriteLine(string.Format("Stair #{0} light is OFF ({1})", ID, DateTime.Now.ToString()));
    }

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
