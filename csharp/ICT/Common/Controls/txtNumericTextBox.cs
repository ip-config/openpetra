//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christiank
//
// Copyright 2004-2010 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;

namespace Ict.Common.Controls
{
    /// <summary>
    /// Extends a normal textbox and restricts text to numbers.
    /// Can be used as a normal textbox or as a textbox that restricts text to certain type of numbers
    /// by setting the ControlMode property.
    ///
    /// There are three ways this control can operate, determined by the ControlMode property
    /// ControlMode.NormalTextBox  - Behave as a completely normal textbox
    /// ControlMode.Integer        - accepts only numbers without digits
    /// ControlMode.Decimal        - accepts only numbers including digits and formats the number
    ///                              according to the DecimalPlaces Property.
    /// ControlMode.Currency       - accepts only numbers including digits and formats the number
    ///                              according to the DecimalPlaces Property, plus adds the Currency Symbol
    ///                              as specified with the CurrencySymbol Property.
    /// </summary>
    public class TTxtNumericTextBox : System.Windows.Forms.TextBox
    {
        private const Int32 CONTROL_CHARS_BACKSPACE = 8;
        private const int WM_PASTE = 0x0302;
        private const int WM_CUT = 0x0300;
        private const int WM_CLEAR = 0x0303;

        /// <summary>
        /// todoComment
        /// </summary>
        public enum TNumberPrecision
        {
            /// <summary>
            /// todoComment
            /// </summary>
            Decimal,
            /// <summary>
            /// todoComment
            /// </summary>
            Double
        }

        private TNumericTextBoxMode FControlMode = TNumericTextBoxMode.Decimal;
        private TNumberPrecision FNumberPrecision = TNumberPrecision.Decimal;
        private int FDecimalPlaces = 2;
        private string FCurrencySymbol = "###";
        private bool FCurrencySybolRightAligned = true;
        private bool FNullValueAllowed = false;

        private string FNumberDecimalSeparator = ".";
        private string FCurrencyDecimalSeparator = ".";

        private string FNumberPositiveSign = "+";
        private string FNumberNegativeSign = "-";

        private bool FDecimalPointEntered = true;

        /// <summary>
        /// todoComment
        /// </summary>
        public enum TNumericTextBoxMode
        {
            /// <summary>
            /// todoComment
            /// </summary>
            NormalTextBox,
            /// <summary>
            /// todoComment
            /// </summary>
            Integer,
            /// <summary>
            /// todoComment
            /// </summary>
            Decimal,
            /// <summary>
            /// todoComment
            /// </summary>
            Currency
        }

        #region Properties

        /// <summary>
        /// This Property is ignored (!) unless ControlMode is 'NormalTextMode'! For all other cases, the value to be displayed needs to be set programmatically through the 'NumberValueDecimal' or 'NumberValueInt' Properties.
        /// </summary>
        [Description(
             "This Property is ignored (!) unless ControlMode is 'NormalTextMode'! For all other cases, the value to be displayed needs to be set programmatically through the 'NumberValueDecimal' or 'NumberValueInt' Properties.")
        ]
        public override string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                if (FControlMode == TNumericTextBoxMode.NormalTextBox)
                {
                    base.Text = value;
                }
            }
        }

        /// <summary>
        /// Determines what input the Control accepts and how it formats it.
        /// </summary>
        [Category("NumericTextBox"),
         RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.All),
         Browsable(true),
         Description("Determines what input the Control accepts and how it formats it.")]
        public TNumericTextBoxMode ControlMode
        {
            get
            {
                return FControlMode;
            }

            set
            {
                FControlMode = value;

                if ((value == TNumericTextBoxMode.NormalTextBox)
                    || (value == TNumericTextBoxMode.Integer))
                {
                    FDecimalPlaces = 0;
                }

                if (DesignMode)
                {
                    if (value != TNumericTextBoxMode.NormalTextBox)
                    {
                        base.Text = "1234";
                    }
                    else
                    {
                        base.Text = "NormalTextBox Mode";
                    }
                }

                FormatValue(RemoveNonNumeralChars());
            }
        }

        /// <summary>
        /// Determines the number of decimal places (valid only for Decimal and Currency ControlModes).
        /// </summary>
        [Category("NumericTextBox"),
         RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.All),
         DefaultValue(2),
         Browsable(true),
         Description("Determines the number of decimal places (valid only for Decimal and Currency ControlModes).")]
        public int DecimalPlaces
        {
            get
            {
                return FDecimalPlaces;
            }

            set
            {
                FDecimalPlaces = value;

                if (DesignMode)
                {
                    if (FControlMode != TNumericTextBoxMode.NormalTextBox)
                    {
                        base.Text = "1234";
                    }

                    if ((FControlMode == TNumericTextBoxMode.Integer)
                        || (FControlMode == TNumericTextBoxMode.NormalTextBox))
                    {
                        if (value != 0)
                        {
                            FDecimalPlaces = 0;
                        }
                    }
                }

                FormatValue(RemoveNonNumeralChars());
            }
        }

        /// <summary>
        /// Determines the currency symbol. Will only be shown if ControlMode is 'Currency'.
        /// </summary>
        [Category("NumericTextBox"),
         RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.All),
         DefaultValue("###"),
         Browsable(true),
         Description("Determines the currency symbol. Will only be shown if ControlMode is 'Currency'.")]
        public string CurrencySymbol
        {
            get
            {
                return FCurrencySymbol;
            }

            set
            {
                FCurrencySymbol = value;

                if (DesignMode)
                {
                    if (FControlMode != TNumericTextBoxMode.NormalTextBox)
                    {
                        base.Text = "1234";
                    }
                }

                FormatValue(RemoveNonNumeralChars());
            }
        }

        /// <summary>
        /// Determines where the currency symbol is shown in relation to the value of the control. Only has an effect if ControlMode is 'Currency'.
        /// </summary>
        [Category("NumericTextBox"),
         RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.All),
         DefaultValue(true),
         Browsable(true),
         Description(
             "Determines where the currency symbol is shown in relation to the value of the control. Only has an effect if ControlMode is 'Currency'.")
        ]
        public bool CurrencySybolRightAligned
        {
            get
            {
                return FCurrencySybolRightAligned;
            }

            set
            {
                FCurrencySybolRightAligned = value;

                if (DesignMode)
                {
                    if (FControlMode != TNumericTextBoxMode.NormalTextBox)
                    {
                        base.Text = "1234";
                    }
                }

                FormatValue(RemoveNonNumeralChars());
            }
        }

        /// <summary>
        /// Determines whether the control allows a null value, or not.
        /// </summary>
        [Category("NumericTextBox"),
         RefreshPropertiesAttribute(System.ComponentModel.RefreshProperties.All),
         DefaultValue(false),
         Browsable(true),
         Description("Determines whether the control allows a null value, or not.")]
        public bool NullValueAllowed
        {
            get
            {
                return FNullValueAllowed;
            }

            set
            {
                FNullValueAllowed = value;
            }
        }


        /// This property gets hidden because it doesn't make sense in the Designer!
        [Browsable(false),
         DefaultValue(0.00)]
        public decimal ? NumberValueDecimal
        {
            get
            {
                if (!DesignMode)
                {
                    if (this.Text != String.Empty)
                    {
                        return Convert.ToDecimal(RemoveNonNumeralChars());
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if ((FControlMode == TNumericTextBoxMode.Decimal)
                    || (FControlMode == TNumericTextBoxMode.Currency))
                {
                    FNumberPrecision = TNumberPrecision.Decimal;

                    if (value != null)
                    {
                        base.Text = value.ToString();
                    }
                    else
                    {
                        if (FNullValueAllowed)
                        {
                            base.Text = String.Empty;
                            return;
                        }
                        else
                        {
                            throw new ArgumentNullException(
                                "The 'NumberValueDecimal' Property must not be set to if the 'NullValueAllowed' Property is false.");
                        }
                    }

                    FormatValue(RemoveNonNumeralChars());
                }
                else
                {
                    if (!DesignMode)
                    {
                        throw new ApplicationException(
                            "The 'NumberValueDecimal' Property can only be set if the 'ControlMode' Property is 'Decimal' or 'Currency'!");
                    }
                }
            }
        }

        /// This property gets hidden because it doesn't make sense in the Designer!
        [Browsable(false),
         DefaultValue(0.00)]
        public double ? NumberValueDouble
        {
            get
            {
                if (!DesignMode)
                {
                    if (this.Text != String.Empty)
                    {
                        return Convert.ToDouble(RemoveNonNumeralChars());
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if ((FControlMode == TNumericTextBoxMode.Decimal)
                    || (FControlMode == TNumericTextBoxMode.Currency))
                {
                    FNumberPrecision = TNumberPrecision.Double;

                    if (value != null)
                    {
                        base.Text = value.ToString();
                    }
                    else
                    {
                        if (FNullValueAllowed)
                        {
                            base.Text = String.Empty;
                            return;
                        }
                        else
                        {
                            throw new ArgumentNullException(
                                "The 'NumberValueDouble' Property must not be set to if the 'NullValueAllowed' Property is false.");
                        }
                    }

                    FormatValue(RemoveNonNumeralChars());
                }
                else
                {
                    if (!DesignMode)
                    {
                        throw new ApplicationException(
                            "The 'NumberValueDouble' Property can only be set if the 'ControlMode' Property is 'Decimal' or 'Currency'!");
                    }
                }
            }
        }

        /// This property gets hidden because it doesn't make sense in the Designer!
        [Browsable(false),
         DefaultValue(0)]
        public int ? NumberValueInt
        {
            get
            {
                if (!DesignMode)
                {
                    if (this.Text != String.Empty)
                    {
                        return Convert.ToInt32(this.Text);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (FControlMode == TNumericTextBoxMode.Integer)
                {
                    if (value != null)
                    {
                        base.Text = value.ToString();
                    }
                    else
                    {
                        if (FNullValueAllowed)
                        {
                            base.Text = String.Empty;
                            return;
                        }
                        else
                        {
                            throw new ArgumentNullException(
                                "The 'NumberValueInt' Property must not be set to null if the 'NullValueAllowed' Property is false.");
                        }
                    }

                    FormatValue(RemoveNonNumeralChars());
                }
                else
                {
                    if (!DesignMode)
                    {
                        throw new ApplicationException(
                            "The 'NumberValueInt' Property can only be set if the 'ControlMode' Property is 'Integer'!");
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public TTxtNumericTextBox()
        {
            NumberFormatInfo NfiCurrenThread = System.Globalization.NumberFormatInfo.CurrentInfo;

            FNumberDecimalSeparator = NfiCurrenThread.NumberDecimalSeparator;     // TODO: make this customisable in Client .config file
            FCurrencyDecimalSeparator = NfiCurrenThread.CurrencyDecimalSeparator; // TODO: make this customisable in Client .config file

            // Hook up Events
            this.KeyPress += new KeyPressEventHandler(OnKeyPress);
            this.Leave += new EventHandler(OnLeave);
            this.Enter += new EventHandler(OnEntering);

//            this.MouseDown += new MouseEventHandler(OnMouseDown);
//            this.LostFocus +=new EventHandler(OnLostFocus);

            this.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Font = new System.Drawing.Font("Verdana", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (Byte)0);

            FormatValue("0");
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (this.ControlMode == TNumericTextBoxMode.NormalTextBox)
            {
                // just be a textbox!
                e.Handled = false;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                this.SelectNextControl(this, true, true, true, true);
                e.Handled = true;
            }

            // handle COPY
            if ((e.KeyCode == Keys.C) && (e.Modifiers == Keys.Control))
            {
                try
                {
                    Clipboard.SetDataObject(this.SelectedText);
                    e.Handled = true;
                }
                catch (Exception)
                {
//                  MessageBox.Show("Exception in OnKeyDown: " + exp.ToString());

                    // never mind
                }
            }

            // handle CUT
            if ((e.KeyCode == Keys.X) && (e.Modifiers == Keys.Control))
            {
                HandleCut();
                e.Handled = true;
            }

            // handle PASTE
            if ((e.KeyCode == Keys.V) && (e.Modifiers == Keys.Control))
            {
                HandlePaste();
                e.Handled = true;
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            Char chrKeyPressed;
            Int32 intSelStart;
            Int32 intDelTo;
            String strText;
            bool bolDelete;
            bool bolDecimalplaceValid = false;
            int intActDecPlace = -1;

            if (this.ControlMode == TNumericTextBoxMode.NormalTextBox)
            {
                // just be a textbox!
                e.Handled = false;
                return;
            }

            if (this.ReadOnly == true)
            {
                // no further action
                e.Handled = true;
                return;
            }

            try
            {
                chrKeyPressed = e.KeyChar;

                // Original cursor position
                intSelStart = this.SelectionStart;

                if (this.SelectionLength == this.Text.Length)
                {
                    if (!(Control.ModifierKeys == Keys.Control))
                    {
                        //ClearBox();
                        base.Text = "";
//                        intSelStart = this.Text.Length;
                        intSelStart = 0;
                    }
                }

                // In case of a selection, delete text to this position
                intDelTo = intSelStart + this.SelectionLength - 1;
                strText = this.Text;

                // Used to avoid deletion of the selection when an invalid key is pressed
                bolDelete = false;

                e.Handled = true;

                if ((chrKeyPressed == (char)(CONTROL_CHARS_BACKSPACE))
                    && (this.SelectionStart != 0))
                {
                    bolDelete = true;

                    if ((intSelStart > 0) && (intDelTo < intSelStart))
                    {
                        intSelStart = intSelStart - 1;
                    }
                }

                switch (FControlMode)
                {
                    case TNumericTextBoxMode.Integer :
                    case TNumericTextBoxMode.Decimal :
                    case TNumericTextBoxMode.Currency :
                        {
                            #region Numeric Validation Rule

                            if (FControlMode == TNumericTextBoxMode.Decimal)
                            {
                                intActDecPlace = this.Text.IndexOf(FNumberDecimalSeparator);
                            }
                            else if (FControlMode == TNumericTextBoxMode.Currency)
                            {
                                intActDecPlace = this.Text.IndexOf(FCurrencyDecimalSeparator);
                            }

                            // Check & Reset boolean if the decimal place does not exist
                            if (intActDecPlace == -1)
                            {
                                FDecimalPointEntered = false;
                            }

                            // If Keypressed is of type numeric or the decimal separator (usually ".")
                            if (Char.IsDigit(chrKeyPressed))
                            {
                                #region Decimal place check

                                if (FDecimalPlaces > 0)
                                {
                                    if ((this.SelectionLength == 0)
                                        && (intSelStart > intActDecPlace))
                                    {
                                        // Decimalplace validation
                                        if (ControlMode == TNumericTextBoxMode.Decimal)
                                        {
                                            bolDecimalplaceValid = (this.Text.Length - intActDecPlace) > FDecimalPlaces;
                                        }
                                        else if (ControlMode == TNumericTextBoxMode.Currency)
                                        {
                                            if (FCurrencySybolRightAligned)
                                            {
                                                bolDecimalplaceValid = ((this.Text.Length - (FCurrencySymbol.Length + 1)) - intActDecPlace) >
                                                                       FDecimalPlaces;
                                            }
                                            else
                                            {
                                                bolDecimalplaceValid = (this.Text.Length - intActDecPlace) > FDecimalPlaces;
                                            }
                                        }

                                        if ((intActDecPlace > 0)
                                            && (bolDecimalplaceValid))
                                        {
                                            e.Handled = true;
                                            return;
                                        }
                                    }
                                    else if (this.SelectionLength == this.Text.Length)
                                    {
                                        e.Handled = false;
                                    }
                                }

//                            else
//                            {
//                                // Don't allow decimal separator to be entered if ControlMode is Integer or there are no decimal places
//                                e.Handled = true;
//                            }

                                #endregion

                                e.Handled = false;
                            }
                            else if (IsDecimalSeparator(chrKeyPressed))
                            {
                                #region Decimal validation

                                if (FControlMode != TNumericTextBoxMode.Integer)
                                {
                                    if (FDecimalPointEntered != true)
                                    {
                                        FDecimalPointEntered = true;
                                        e.Handled = false;
                                    }
                                    else
                                    {
                                        e.Handled = true;
                                    }
                                }
                                else
                                {
                                    // no decimal point allowed with Integers
                                    e.Handled = true;
                                }

                                #endregion
                            }
                            else if (chrKeyPressed == FNumberNegativeSign[0])
                            {
                                // allow negative sign only in front of all digits, and only if it isn't there yet
                                if ((intSelStart == 0)
                                    && (!this.Text.Contains(FNumberNegativeSign)))
                                {
                                    e.Handled = false;
                                }
                                else
                                {
                                    e.Handled = true;
                                }
                            }
                            else if (chrKeyPressed == FNumberPositiveSign[0])
                            {
                                // allow positive sign to be entered in front of all digits, and only if there is a negative sign
                                if ((intSelStart == 0)
                                    || (intSelStart == 1))
                                {
                                    if (this.Text.Substring(0, FNumberNegativeSign.Length) == FNumberNegativeSign)
                                    {
                                        base.Text = this.Text.Substring(1);
                                        e.Handled = false;
                                    }
                                    else
                                    {
                                        e.Handled = true;
                                    }
                                }
                                else
                                {
                                    e.Handled = true;
                                }
                            }
                            else
                            {
                                e.Handled = true;
                            }

                            #endregion

                            break;
                        }

                    case TNumericTextBoxMode.NormalTextBox:
                    {
                        //Nothing here..
                        break;
                    }
                }

                if (bolDelete == true)
                {
                    base.Text = strText.Substring(0, intSelStart) + strText.Substring(intSelStart + 1);
                    this.SelectionStart = intSelStart;
                    this.SelectionLength = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "OnKeyPress Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void ClearBox()
        {
            if (FNullValueAllowed)
            {
                base.Text = String.Empty;
            }
            else
            {
                switch (FControlMode)
                {
                    case TNumericTextBoxMode.NormalTextBox:
                        base.Text = String.Empty;
                        break;

                    case TNumericTextBoxMode.Integer:
                        base.Text = "0";
                        break;

                    case TNumericTextBoxMode.Decimal:
                        base.Text = "0" + FNumberDecimalSeparator + "00";
                        break;

                    case TNumericTextBoxMode.Currency:
                        FormatValue("0" + FCurrencyDecimalSeparator + "00");
                        break;
                }
            }
        }

        private bool IsDecimalSeparator(char AKeyChar)
        {
            char[] DecimalSeparatorChars;

            if (FControlMode == TNumericTextBoxMode.Decimal)
            {
                DecimalSeparatorChars = FNumberDecimalSeparator.ToCharArray();
            }
            else
            {
                DecimalSeparatorChars = FCurrencyDecimalSeparator.ToCharArray();
            }

            List <char>DecimalSeparatorCharsList = new List <char>(DecimalSeparatorChars);

            if (DecimalSeparatorCharsList.Contains(AKeyChar))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void HandlePaste()
        {
            String str;
            IDataObject clip;

            if (!this.ReadOnly)
            {
//          MessageBox.Show("HandlePaste");

                clip = Clipboard.GetDataObject();

                if (clip != null)
                {
                    // try and paste the contents
                    try
                    {
                        str = (String)(clip.GetData(DataFormats.Text));

                        if (this.SelectionLength > 0)
                        {
                            this.SelectedText = str;
//                              ProcessChangedText(this.Text);
                        }
                        else if (this.SelectionStart > 0)
                        {
                            base.Text = this.Text.Substring(0, this.SelectionStart) + str + this.Text.Substring(this.SelectionStart);
                        }
                        else
                        {
                            base.Text = str;
                        }
                    }
                    catch (Exception Exp)
                    {
                        MessageBox.Show("Exception in TTxtNumericTextBox.HandlePaste: " + Exp.ToString());

                        // never mind
                    }
                }
            }
        }

        private void HandleCut()
        {
            try
            {
                if (this.SelectedText.Length > 0)
                {
                    Clipboard.SetDataObject(this.SelectedText);

                    if (!this.ReadOnly)
                    {
                        if (this.SelectionLength == this.Text.Length)
                        {
                            this.ClearBox();
                        }
                        else
                        {
                            this.SelectedText = new String('0', this.SelectedText.Length);
//                            ProcessChangedText(this.Text);
                        }
                    }
                }
            }
            catch (Exception Exp)
            {
                MessageBox.Show("Exception in Exception in TTxtNumericTextBox.HandleCut: " + Exp.ToString());

                // never mind
            }
        }

        private void FormatValue(string AValue)
        {
            double NumberValueDouble;
            decimal NumberValueDecimal;
            string strnumformat = "";

            if (AValue != String.Empty)
            {
//MessageBox.Show("FormatValue input string AValue: " + AValue);
                switch (FControlMode)
                {
                    case TNumericTextBoxMode.Decimal:

                        if (FNumberPrecision == TNumberPrecision.Double)
                        {
                            NumberValueDouble = Convert.ToDouble(AValue);
                            //                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                            //                NumberFormatInfo ni = ci.NumberFormat;

                            base.Text = NumberValueDouble.ToString("N" + FDecimalPlaces);
                            //                string strnumformat = d.ToString("c", ni);
                            //                this.Text = strnumformat.Remove(0, 1);
                        }
                        else if (FNumberPrecision == TNumberPrecision.Decimal)
                        {
                            NumberValueDecimal = Convert.ToDecimal(AValue);
                            //                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                            //                NumberFormatInfo ni = ci.NumberFormat;

                            base.Text = NumberValueDecimal.ToString("N" + FDecimalPlaces);
                            //                string strnumformat = d.ToString("c", ni);
                            //                this.Text = strnumformat.Remove(0, 1);
                        }

                        break;

                    case TNumericTextBoxMode.Currency:
                        CultureInfo ci = CultureInfo.CreateSpecificCulture("en-GB");
                        NumberFormatInfo ni = ci.NumberFormat;
                        ni.CurrencyDecimalDigits = FDecimalPlaces;

                        if (FNumberPrecision == TNumberPrecision.Double)
                        {
                            NumberValueDouble = Convert.ToDouble(AValue);
                            strnumformat = NumberValueDouble.ToString("c", ni);

                            // Remove currency symbol (we know it's '�' because we selected en-GB culture)
                            if (NumberValueDouble >= 0)
                            {
                                strnumformat = strnumformat.Remove(0, 1);
                            }
                            else
                            {
                                strnumformat = strnumformat.Remove(1, 1);
                            }
                        }
                        else if (FNumberPrecision == TNumberPrecision.Decimal)
                        {
                            NumberValueDecimal = Convert.ToDecimal(AValue);
                            strnumformat = NumberValueDecimal.ToString("c", ni);

                            // Remove currency symbol (we know it's '�' because we selected en-GB culture)
                            if (NumberValueDecimal >= 0)
                            {
                                strnumformat = strnumformat.Remove(0, 1);
                            }
                            else
                            {
                                strnumformat = strnumformat.Remove(1, 1);
                            }
                        }

                        if (FCurrencySybolRightAligned)
                        {
                            base.Text = strnumformat + " " + FCurrencySymbol;
                        }
                        else
                        {
                            base.Text = FCurrencySymbol + " " + strnumformat;
                        }

                        break;

                    case TNumericTextBoxMode.Integer:
                        // No formatting is done
                        base.Text = AValue;

                        break;

                    default:
                        // No formatting is done
                        base.Text = AValue;

                        break;
                }
            }
            else
            {
                ClearBox();
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLeave(object sender, EventArgs e)
        {
            if (!(FNullValueAllowed && (this.Text == String.Empty)))
            {
                try
                {
                    FormatValue(RemoveNonNumeralChars());
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("The entered data has an invalid format!", "Invalid format");
                }
                catch (Exception Exp)
                {
                    MessageBox.Show("Exception in TTxtNumericTextBox.OnLeave: " + Exp.ToString());
                }
            }
        }

        private string RemoveNonNumeralChars()
        {
            string ReturnValue = String.Empty;
            char ExaminedChar;
            bool GoodChar;

            if ((FControlMode == TNumericTextBoxMode.Currency)
                && (this.Text != String.Empty))
            {
//MessageBox.Show("RemoveNonNumeralChars: Text as it is: '" + this.Text + "'");

//                    if (this.Text.Contains(FCurrencySymbol))
//                    {
//                        ReturnValue = this.Text.Remove(this.Text.IndexOf(FCurrencySymbol), FCurrencySymbol.Length);
//                    }

                for (int CharCounter = 0; CharCounter < this.Text.Length; CharCounter++)
                {
                    ExaminedChar = this.Text[CharCounter];
                    GoodChar = false;

                    if (Char.IsDigit(ExaminedChar))
                    {
                        GoodChar = true;
                    }
                    else if (IsDecimalSeparator(ExaminedChar))
                    {
                        GoodChar = true;
                    }
                    else if ((ExaminedChar == FNumberNegativeSign[0])
                             || (ExaminedChar == FNumberPositiveSign[0]))
                    {
                        GoodChar = true;
                    }

                    if (GoodChar)
                    {
                        ReturnValue = ReturnValue + ExaminedChar;
                    }
                }

//                    ReturnValue = ReturnValue.Trim();
//                }
//MessageBox.Show("RemoveNonNumeralChars: Text without non-numeral characters: '" + ReturnValue + "'");
            }
            else
            {
                ReturnValue = this.Text;
            }

            return ReturnValue;
        }

//        protected override void OnPaint(PaintEventArgs e)
//        {
//            base.OnPaint(e);
//
//            OnLeave(this, null);
//        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnEntering(object sender, EventArgs e)
        {
            this.SelectAll();

//            this.BackColor = Color.Blue ;
//            this.ForeColor = Color.White;
        }

//        protected void OnMouseDown(object sender, MouseEventArgs e)
//        {
//            this.SelectAll();
//        }
    }
}