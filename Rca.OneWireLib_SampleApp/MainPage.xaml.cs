using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace Rca.OneWireLib_SampleApp
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Sample1 m_Sandbox;

        public MainPage()
        {
            this.InitializeComponent();

            m_Sandbox = new Sample1();

            m_Sandbox.Init();

            //Testloop
            Debug.WriteLine("Start test loop");
            var noError = true;
            while (noError)
            {
                try
                {
                    m_Sandbox.TestThermometer();
                    SpinWait.SpinUntil(() => false, 500);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    noError = false;
                }
            }
        }
    }
}
