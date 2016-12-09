using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Concentration.Utilities
{
    public class PropertyChangedNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            // TODO: C# 6 way to do this.
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// Verifies the name of the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="System.Exception"></exception>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Used to tell viewmodels whether they should create design time data now or not.
        /// </summary>
        /// <returns></returns>
        protected bool IsInDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
    }
}

