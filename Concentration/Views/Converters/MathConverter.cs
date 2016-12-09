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

using NCalc;

using System;
using System.Globalization;
using System.Windows.Data;

namespace Concentration.Views.Converters
{
    public class MathConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// The 
        /// </summary>
        /// <param name="value">The value to insert into the expression.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The string expression to evaluate.  Use @VALUE as a placeholder in the expression for the value.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.IsNaN((double)value))
                return "NaN";

            // Parse value into equation and remove spaces
            string expressionString = parameter as string;

            if (!string.IsNullOrEmpty(expressionString))
            {
                expressionString = expressionString.Replace(" ", "");
                expressionString = expressionString.Replace("@VALUE", value.ToString());
            }

            return new Expression(expressionString).Evaluate();
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}


