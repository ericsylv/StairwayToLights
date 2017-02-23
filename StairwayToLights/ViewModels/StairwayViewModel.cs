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
  /// <summary>
  /// ViewModel representing the stairway
  /// </summary>
  /// <seealso cref="StairwayToLights.Common.BindableBase" />
  /// <seealso cref="System.IDisposable" />
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

    //IoT fields
    private bool _isIotConnected;
    private int _pirTopPinNumber;
    private int _pirBottomPinNumber;
    private GpioPin _pirTopPin;
    private GpioPin _pirBottomPin;
    private List<int> _orderedPinsForStairs;

    /// <summary>
    /// Initializes a new instance of the <see cref="StairwayViewModel"/> class.
    /// </summary>
    /// <param name="pirTopPinNumber">The pir top pin number.</param>
    /// <param name="pirBottomPinNumber">The pir bottom pin number.</param>
    /// <param name="orderedPinsForStairs">The ordered pins for stairs.</param>
    public StairwayViewModel(int pirTopPinNumber, int pirBottomPinNumber, List<int> orderedPinsForStairs)
    {
      // Initialization      
      IsPirTopOn = false;
      IsPirBottomOn = false;
      IsSomeoneInStairway = false;
      Status = "Waiting for input...";
      DelayBetweenEachStair = 300;
      DelayBeforeTurningOffLights = 5000;

      Logs = new ObservableCollection<string>()
      {
        string.Format("[{0}]: Waiting for input", DateTime.Now.ToString()),
        string.Format("[{0}]: Ready to operate", DateTime.Now.ToString()),
      };
      Stairs = new ObservableCollection<StairViewModel>();

      //Iot related
      _isIotConnected = false;
      _pirTopPinNumber = pirTopPinNumber;
      _pirBottomPinNumber = pirBottomPinNumber;
      _orderedPinsForStairs = orderedPinsForStairs;

      if (_isIotConnected)
      {
        InitGpio();
      }
    }

    /// <summary>
    /// Gets or sets the delay between each stair.
    /// </summary>
    /// <value>
    /// The delay between each stair. Default value is set in constructor.
    /// </value>
    public double DelayBetweenEachStair
    {
      get { return _delayBetweenEachStair; }
      set
      {
        SetProperty(ref _delayBetweenEachStair, value);
      }
    }

    /// <summary>
    /// Gets or sets the delay before turning off lights.
    /// </summary>
    /// <value>
    /// The delay between each stair. Default value is set in constructor.
    /// </value>
    public double DelayBeforeTurningOffLights
    {
      get { return _delayBeforeTurningOffLights; }
      set
      {
        SetProperty(ref _delayBeforeTurningOffLights, value);
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the top PIR is on.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is pir top on; otherwise, <c>false</c>.
    /// </value>
    public bool IsPirTopOn
    {
      get { return _isPirTopOn; }
      set
      {
        SetProperty(ref _isPirTopOn, value);
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the bottom PIR is on.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is pir bottom on; otherwise, <c>false</c>.
    /// </value>
    public bool IsPirBottomOn
    {
      get { return _isPirBottomOn; }
      set
      {
        SetProperty(ref _isPirBottomOn, value);
      }
    }
    /// <summary>
    /// Gets or sets the status of what's going on.
    /// </summary>
    /// <value>
    /// The status.
    /// </value>
    public string Status
    {
      get { return _status; }
      set
      {
        SetProperty(ref _status, value);
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether there is someone in stairway.
    /// </summary>
    /// <value>
    /// <c>true</c> if there is someone in stairway; otherwise, <c>false</c>.
    /// </value>
    public bool IsSomeoneInStairway
    {
      get { return _isSomeoneInStairway; }
      set
      {
        SetProperty(ref _isSomeoneInStairway, value);
      }
    }

    /// <summary>
    /// Gets or sets the logs of the app.
    /// </summary>
    /// <value>
    /// The logs.
    /// </value>
    public ObservableCollection<string> Logs { get; set; }

    /// <summary>
    /// Gets or sets the stairs.
    /// </summary>
    /// <value>
    /// The stairs.
    /// </value>
    public ObservableCollection<StairViewModel> Stairs { get; set; }

    /// <summary>
    /// Creates a number of stairs in the stairway.
    /// </summary>
    /// <param name="numberOfStairs">The number of stairs.</param>
    /// <exception cref="Exception">Impossible to create stairs, not enough available pins</exception>
    public void CreateStairs(int numberOfStairs)
    {
      if (numberOfStairs > _orderedPinsForStairs.Count)
      {
        throw new Exception("Impossible to create stairs, not enough available pins");
      }

      for (int i = 0; i < numberOfStairs; i++)
      {
        Stairs.Add(new StairViewModel(i + 1, _orderedPinsForStairs.First(), _isIotConnected));
        _orderedPinsForStairs.RemoveAt(0);
        Logs.Insert(0, string.Format("[{0}]: {1}", DateTime.Now.ToString(), string.Concat("Just added stair #", Stairs.Last().ID.ToString())));
      }
    }
    /// <summary>
    /// Someone is going down.
    /// </summary>
    public void GoDown()
    {
      if (Stairs.Any() && !IsSomeoneInStairway)
      {
        IsPirTopOn = true;
        IsPirBottomOn = false;
        IsSomeoneInStairway = true;
        Status = "Motion detected on TOP of stairway.";
        Logs.Insert(0, string.Format("[{0}]: {1}", DateTime.Now.ToString(), "Motion detected on TOP of stairway."));

        // Start to turn on stairs
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

    /// <summary>
    /// Someone is going up.
    /// </summary>
    public void GoUp()
    {
      if (Stairs.Any() && !IsSomeoneInStairway)
      {
        IsPirTopOn = false;
        IsPirBottomOn = true;
        IsSomeoneInStairway = true;
        Status = "Motion detected on BOTTOM of stairway.";
        Logs.Insert(0, string.Format("[{0}]: {1}", DateTime.Now.ToString(), "Motion detected at BOTTOM of stairway."));

        // Start to turn on stairs
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

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (_isIotConnected)
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
