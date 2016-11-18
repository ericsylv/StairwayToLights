using StairwayToLights.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;

namespace StairwayToLights.ViewModels
{
  public class StairwayViewModel : BindableBase, IDisposable
  {
    private bool _isPirTopOn;
    private bool _isPirBottomOn;
    private string _status;
    private bool _isSomeoneInStairway;
    private double _delayBetweenEachStair;
    private double _delayBeforeTurningOffLights;

    private static Func<DispatchedHandler, Task> _callOnUiThread = async (handler)
            => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);

    //IoT
    private int _pirTopPinNumber;
    private int _pirBottomPinNumber;
    private GpioPin _pirTopPin;
    private GpioPin _pirBottomPin;
    private List<int> _orderedPinsForStairs;

    public StairwayViewModel(int pirTopPinNumber, int pirBottomPinNumber, List<int> orderedPinsForStairs)
    {
      // Initialization      
      IsPirTopOn = false;
      IsPirBottomOn = false;
      IsSomeoneInStairway = false;
      Status = "Waiting for input...";
      DelayBetweenEachStair = 1000;
      DelayBeforeTurningOffLights = 3000;

      Logs = new ObservableCollection<string>()
      {
        string.Format("[{0}]: Waiting for input", DateTime.Now.ToString()),
        string.Format("[{0}]: Ready to operate", DateTime.Now.ToString()),
      };
      Stairs = new ObservableCollection<StairViewModel>();

      //Iot
      _pirTopPinNumber = pirTopPinNumber;
      _pirBottomPinNumber = pirBottomPinNumber;
      _orderedPinsForStairs = orderedPinsForStairs;
      //InitGpio();
    }

    public double DelayBetweenEachStair
    {
      get { return _delayBetweenEachStair; }
      set
      {
        SetProperty(ref _delayBetweenEachStair, value);
      }
    }

    public double DelayBeforeTurningOffLights
    {
      get { return _delayBeforeTurningOffLights; }
      set
      {
        SetProperty(ref _delayBeforeTurningOffLights, value);
      }
    }

    public bool IsPirTopOn
    {
      get { return _isPirTopOn; }
      set
      {
        SetProperty(ref _isPirTopOn, value);
      }
    }

    public bool IsPirBottomOn
    {
      get { return _isPirBottomOn; }
      set
      {
        SetProperty(ref _isPirBottomOn, value);
      }
    }
    public string Status
    {
      get { return _status; }
      set
      {
        SetProperty(ref _status, value);
      }
    }


    public bool IsSomeoneInStairway
    {
      get { return _isSomeoneInStairway; }
      set
      {
        SetProperty(ref _isSomeoneInStairway, value);
      }
    }

    public ObservableCollection<string> Logs { get; set; }
    public ObservableCollection<StairViewModel> Stairs { get; set; }

    public void CreateStairs(int numberOfStairs)
    {
      if (numberOfStairs > _orderedPinsForStairs.Count)
      {
        throw new Exception("Impossible to create stairs, not enough available pins");
      }

      for (int i = 0; i < numberOfStairs; i++)
      {
        Stairs.Add(new StairViewModel(i + 1, _orderedPinsForStairs.First()));
        _orderedPinsForStairs.RemoveAt(0);
        Logs.Insert(0, string.Format("[{0}]: {1}", DateTime.Now.ToString(), string.Concat("Just added stair #", Stairs.Last().ID.ToString())));
      }
    }
    public void GoDown()
    {
      if (Stairs.Any() && !IsSomeoneInStairway)
      {
        IsPirTopOn = true;
        IsPirBottomOn = false;
        IsSomeoneInStairway = true;
        Status = "Motion detected on TOP of stairway.";
        Logs.Insert(0, string.Format("[{0}]: {1}", DateTime.Now.ToString(), "Motion detected on TOP of stairway."));

        Task.Run(() =>
        {
          foreach (StairViewModel stair in Stairs)
          {
            _callOnUiThread(() => stair.TurnOnLight());
            Task.Delay((int)DelayBetweenEachStair).Wait();
          }

          Task.Delay((int)DelayBeforeTurningOffLights).Wait();

          foreach (StairViewModel stair in Stairs)
          {
            _callOnUiThread(() => stair.TurnOffLight());
            Task.Delay((int)DelayBetweenEachStair).Wait();
          }

          _callOnUiThread(() => IsPirTopOn = false);
          _callOnUiThread(() => IsSomeoneInStairway = false);
          _callOnUiThread(() => Status = "Waiting for input...");
        });
      }
    }

    public void GoUp()
    {
      if (Stairs.Any() && !IsSomeoneInStairway)
      {
        IsPirTopOn = false;
        IsPirBottomOn = true;
        IsSomeoneInStairway = true;
        Status = "Motion detected on BOTTOM of stairway.";

        Task.Run(() =>
        {
          foreach (StairViewModel stair in Stairs.Reverse())
          {
            _callOnUiThread(() => stair.TurnOnLight());
            Task.Delay((int)DelayBetweenEachStair).Wait();
          }

          Task.Delay((int)DelayBeforeTurningOffLights).Wait();

          foreach (StairViewModel stair in Stairs.Reverse())
          {
            _callOnUiThread(() => stair.TurnOffLight());
            Task.Delay((int)DelayBetweenEachStair).Wait();
          }

          _callOnUiThread(() => IsPirBottomOn = false);
          _callOnUiThread(() => IsSomeoneInStairway = false);
          _callOnUiThread(() => Status = "Waiting for input...");
        });
      }
    }

    public void Dispose()
    {
      if (Stairs.Any())
      {
        Stairs.ToList().ForEach(x => x.Dispose());
      }

      if (_pirTopPin != null)
      {
        _pirTopPin.ValueChanged -= SomeoneDetectedOnTop;
      }

      if (_pirBottomPin != null)
      {
        _pirBottomPin.ValueChanged -= SomeoneDetectedAtBottom;
      }
    }

    private void InitGpio()
    {
      var controller = GpioController.GetDefault();

      _pirTopPin = controller.OpenPin(_pirTopPinNumber);
      _pirTopPin.SetDriveMode(GpioPinDriveMode.Input);
      _pirTopPin.ValueChanged += SomeoneDetectedOnTop;

      _pirBottomPin = controller.OpenPin(_pirBottomPinNumber);
      _pirBottomPin.SetDriveMode(GpioPinDriveMode.Input);
      _pirBottomPin.ValueChanged += SomeoneDetectedAtBottom;
    }

    private void SomeoneDetectedOnTop(GpioPin sender, GpioPinValueChangedEventArgs args)
    {
      GoDown();
    }
    private void SomeoneDetectedAtBottom(GpioPin sender, GpioPinValueChangedEventArgs args)
    {
      GoUp();
    }
  }
}
