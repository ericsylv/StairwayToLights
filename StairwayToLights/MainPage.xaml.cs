using StairwayToLights.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace StairwayToLights
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    private const int PIR_TOP_PIN_NUMBER = 23;
    private const int PIR_BOTTOM_PIN_NUMBER = 24;
    private readonly List<int> STAIRS_PIN_NUMBERS = new List<int>() { 4, 5, 6, 17, 13, 19, 26, 22, 16, 20, 21, 18, 25 };

    public MainPage()
    {
      ViewModel = new StairwayViewModel(PIR_TOP_PIN_NUMBER, PIR_BOTTOM_PIN_NUMBER, STAIRS_PIN_NUMBERS);
      ViewModel.CreateStairs(13);

      Unloaded += MainPage_Unloaded; ;
      this.InitializeComponent();
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null)
      {
        ViewModel.Dispose();
      }
    }

    public StairwayViewModel ViewModel { get; set; }

    private void btnPirTop_Click(object sender, RoutedEventArgs e)
    {
      if (!ViewModel.IsSomeoneInStairway)
      {
        ViewModel.GoDown();
      }
    }

    private void btnPirBottom_Click(object sender, RoutedEventArgs e)
    {
      if (!ViewModel.IsSomeoneInStairway)
      {
        ViewModel.GoUp();
      }
    }
  }
}
