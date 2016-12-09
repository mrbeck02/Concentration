// * MOBILEMEDTEK CONFIDENTIAL                                                 |
// * -------------------------                                                 |
// * Copyright 2016 MobileMedTek. All rights reserved.                         |
// *                                                                           |
// * NOTICE:  All information contained herein is, and remains                 |
// * the property of MobileMedTek. The intellectual and                        |
// * technical concepts contained herein are proprietary to MobileMedTek       |
// * and its suppliers and may be covered by U.S. and Foreign Patents,         |
// * patents in process, and are protected by trade secret or copyright law.   |
// * Dissemination of this information or reproduction of this material        |
// * is strictly forbidden unless prior written permission is obtained         |
// * from MobileMedTek.                                                        |

using Concentration.Resources;
using Concentration.Views;
using Microsoft.Shell;

using System;
using System.Windows;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Concentration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ISingleInstanceApp
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string Unique = "My_Unique_Application_Stringy";

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            // Setup Logging
            log4net.Config.XmlConfigurator.Configure();

            // Handle un-handled exceptions
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception == null)
            {
                Current.Shutdown();
                return;
            }

            var exception = e.Exception.InnerException ?? e.Exception;

            Log.Debug(string.Format("{0}\r\n{1}\r\n", exception.Message, exception.StackTrace));

            if (MessageBox.Show(CautionStrings.App_UserInterfaceErrorMessage, CautionStrings.App_UserInterfaceErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                if (Current != null)
                {
                    Current.Shutdown();
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Catch unhandled exceptions not thrown by the main UI thread.
        /// The above AppUI_DispatcherUnhandledException method for DispatcherUnhandledException will only handle exceptions thrown by the main UI thread. 
        /// Unhandled exceptions caught by this method typically terminate the runtime.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception.InnerException ?? e.Exception;

            Dispatcher.UnhandledException -= OnDispatcherUnhandledException;

            Log.Debug(string.Format("{0}\r\n{1}\r\n", exception.Message, exception.StackTrace));

            if (MessageBox.Show(CautionStrings.App_UnhandledExceptionErrorMessage, CautionStrings.App_UnhandledExceptionErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                Current.Shutdown();
            }

            e.Handled = true;
        }

        #region ISingleInstanceApp

        /// <summary>
        /// Signals the external command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool SignalExternalCommandLineArgs(System.Collections.Generic.IList<string> args)
        {
            // Handle command line arguments of second instance of app being opened...

            // restore if minimized
            var window = Current.MainWindow as MainWindow;

            if (window != null && window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            return true;
        }

        #endregion
    }
}
