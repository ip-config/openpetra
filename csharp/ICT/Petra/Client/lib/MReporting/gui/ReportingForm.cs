﻿/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       timop
 *
 * Copyright 2004-2009 by OM International
 *
 * This file is part of OpenPetra.org.
 *
 * OpenPetra.org is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenPetra.org is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
 *
 ************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Data;
using System.Resources;
using System.Threading;
using Mono.Unix;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.MReporting.Logic;
using Ict.Petra.Shared.MReporting;
using Ict.Common.Verification;
using SourceGrid;
using SourceGrid.Selection;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using Ict.Common;
using Ict.Petra.Client.CommonForms;
using Ict.Common.Controls;

namespace Ict.Petra.Client.MReporting.Gui
{
    /// <summary>
    /// useful functions for report screens
    /// </summary>
    public class TFrmPetraReportingUtils : TFrmPetraUtils
    {
        /// <summary>number of columns that can be sorted</summary>
        public const Int32 NUMBER_SORTBY = 3;

        /// <summary>helper variable to unselect the column in the grid after cancel or apply</summary>
        private bool FDuringApplyOrCancel;

        /// <summary>we will not need resizing of form for forms that are generated by scaffolding</summary>
        protected Boolean FDontResizeForm;

        /// <summary>the name of the report; used to identify the directory where the settings are stored</summary>
        public string FReportName;

        /// <summary>name that should be in the menu; used for dynamically loading NRR reports</summary>
        protected string FMenuItemCaption;

        /// <summary>the name of the current settings, if they have been loaded or already saved under this name</summary>
        protected string FCurrentSettingsName;

        /// <summary>the CSV list of file names of xml files needed for this report</summary>
        public string FXMLFiles;

        /// <summary>the name of the report, as it is used in the xml file</summary>
        public string FCurrentReport;

        /// <summary>to be able to add the currently loaded settings name to the caption of the window.</summary>
        protected string FWindowCaption;

        /// <summary>this System.Object manages the stored settings of the current user and current report</summary>
        protected TStoredSettings FStoredSettings;

        /// <summary>the System.Object that is able to deal with all the parameters, and can calculate a report</summary>
        protected TRptCalculator FCalculator;

        /// <summary>this holds the currently configured columns</summary>
        protected TParameterList FColumnParameters;

        /// <summary>this shows which column is currently selected; it is 1 if no column is selected</summary>
        protected int FSelectedColumn;

        /// <summary>List of functions between columns, that are available for this report; is set by SetAvailableFunctions</summary>
        protected ArrayList FAvailableFunctions;

        /// <summary>list of verification results; ReadControls should add all errors to this list; ReadControlsWithErrorHandling will tell the user</summary>
        protected TVerificationResultCollection FVerificationResults;

        /// <summary>This is the thread used to generate the report; that way, the status bar is always updated, and the window does never turn blank</summary>
        protected Thread FGenerateReportThread;

        /// <summary>the path where the application is started from.</summary>
        public static string FApplicationDirectory;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ACallerWindowHandle">the int handle of the form that has opened this window; needed for focusing when this window is closed later</param>
        /// <param name="ATheForm"></param>
        /// <param name="AStatusBar"></param>
        public TFrmPetraReportingUtils(IntPtr ACallerWindowHandle, IFrmPetra ATheForm, TExtStatusBarHelp AStatusBar) : base(ACallerWindowHandle,
                                                                                                                           (IFrmPetra)ATheForm,
                                                                                                                           AStatusBar)
        {
            FCurrentSettingsName = "";
            FSelectedColumn = -1;
            FAvailableFunctions = null;
            FGenerateReportThread = null;
            FDuringApplyOrCancel = false;
            FDontResizeForm = false;
        }

        /// <summary>
        /// returns the string that is to be displayed in the menuitem
        /// that is mainly used for dynamically loaded nrr reports
        ///
        /// </summary>
        /// <returns>void</returns>
        public string GetMenuItemCaption()
        {
            return FMenuItemCaption;
        }

        /// <summary>
        /// This function is supposed to test if the current user has access to all the needed modules
        /// This method describes, which permissions are needed for this specific report.
        /// </summary>
        /// <returns>true if the current user is allowed to use this report
        /// </returns>
        public virtual bool HasSufficientPermissions()
        {
            return true;
        }

#if TODO
        private void BtnCSVDestination_Click(System.Object sender, System.EventArgs e)
        {
            if (SaveFileDialogCSV.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtCSVDestination.Text = SaveFileDialogCSV.FileName;
            }
        }

        private void TFrmReporting_KeyUp(System.Object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Alt == true)
            {
                switch (e.KeyCode)
                {
                    case Keys.G:
                        MI_GenerateReport_Click(sender, e);
                        break;

                    case Keys.S:
                        MI_SaveSettings_Click(sender, e);
                        break;

                    case Keys.A:
                        MI_SaveSettingsAs_Click(sender, e);
                        break;

                    case Keys.L:
                        MI_LoadSettingsDialog_Click(sender, e);
                        break;

                    case Keys.C:
                        MniFile_Click(mniFileClose, e);
                        break;
                }
            }
        }
#endif



        #region Screen Initialisation

        /// <summary>
        /// setup the form
        ///
        /// </summary>
        /// <returns>false if there are not enough permissions
        /// </returns>
        public virtual bool InitialiseData(String AReportParameter)
        {
            bool ReturnValue = true;

            if (!HasSufficientPermissions())
            {
                MessageBox.Show("You don't have enough permissions for this report");
                return false;
            }

            this.FCalculator = new TRptCalculator();
            FColumnParameters = new TParameterList();
            FColumnParameters.Add("MaxDisplayColumns", 0);

            FWindowCaption = FWinForm.Text;
            string SettingsDirectory = TClientSettings.ReportingPathReportSettings;
            this.FStoredSettings = new TStoredSettings(FReportName, SettingsDirectory);
            UpdateLoadingMenu(this.FStoredSettings.GetRecentlyUsedSettings());
            FSelectedColumn = -1;

            return ReturnValue;
        }

        #endregion


        /// <summary>
        /// This function makes sure whether the window can be closed.
        /// It can be used e.g. if something is still edited.
        /// </summary>
        /// <returns>true if window can be closed
        /// </returns>
        public new bool CanClose()
        {
            bool ReturnValue = true;

#if TODO
            System.Windows.Forms.DialogResult answer;
            ReturnValue = base.CanClose();

            if ((FGenerateReportThread != null) && FGenerateReportThread.IsAlive)
            {
                ReturnValue = false;
                answer = MessageBox.Show("A report is being calculated at the moment. " + Environment.NewLine + "Do you want to cancel the report?",
                    "Cancel Report?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);

                if (answer == System.Windows.Forms.DialogResult.Yes)
                {
                    FCalculator.CancelReportCalculation();
                    ReturnValue = true;
                }
            }

            // has anything changed in the currently selected column?
            if (ColumnChanged(FSelectedColumn))
            {
                MessageBox.Show(
                    "Window cannot be closed." + Environment.NewLine + "Please first apply the changes to the current column, " +
                    Environment.NewLine +
                    "or cancel the changes.",
                    "Column changed");
                ReturnValue = false;
            }

            if (ReturnValue == true)
            {
                if (this.Owner != null)
                {
                    this.Owner.Focus();
                }
            }
#endif
            return ReturnValue;
        }

        /// <summary>
        /// generate a report, called by menu item or toolbar button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MI_GenerateReport_Click(System.Object sender, System.EventArgs e)
        {
#if TODO
            if (!mniFileClose.Enabled)
            {
                // cancel the report
                FCalculator.CancelReportCalculation();
                return;
            }

            // has anything changed in the currently selected column?
            if (ColumnChanged(FSelectedColumn))
            {
                MessageBox.Show(
                    "Report cannot be generated." + Environment.NewLine + "Please first apply the changes to the current column, " +
                    Environment.NewLine +
                    "or cancel the changes.",
                    "Column changed");
                return;
            }
            else
            {
                SelectColumn(-1);
            }
#endif

            if ((FGenerateReportThread == null) || (!FGenerateReportThread.IsAlive))
            {
// TODO                EnableDisableToolbar(false);
                FGenerateReportThread = new Thread(GenerateReport);
                FGenerateReportThread.IsBackground = true;
                FGenerateReportThread.Start();
            }
        }

        /// <summary>
        /// Reads path of default browser from registry
        /// </summary>
        /// <returns>void</returns>
        public static string GetDefaultBrowserPath()
        {
            const String key = "htmlfile\\shell\\open\\command";
            RegistryKey regKey;
            string s;
            string delim;

            regKey = Registry.ClassesRoot.OpenSubKey(key, false);

            // get default browser path
            // see http:www.novell.com/coolsolutions/tip/11537.html; it seems FireFox does not change that registry setting
            s = regKey.GetValue(null, null).ToString();
            delim = "\"";
            return s.Split(delim.ToCharArray())[1];
        }

        /// <summary>
        /// This procedure does the calculation of the report, including fetching the parameters from the GUI, verifying them, and providing error messages This should be called in a different thread, by MI_GenerateReport_Click
        /// </summary>
        /// <returns>void</returns>
        private void GenerateReport()
        {
            TMyUpdateDelegate myDelegate;

            try
            {
                // read the settings and parameters from the controls
                if (!ReadControlsWithErrorHandling())
                {
// TODO                    EnableDisableToolbar(true);
                    return;
                }

#if DEBUGMODE
                FCalculator.GetParameters().Save("debugParameter.xml", true);
#endif
                this.FWinForm.Cursor = Cursors.WaitCursor;
                TLogging.SetStatusBarProcedure(this.WriteToStatusBar);

                // calculate the report
                // TODO : should the server know the user name and password? what about user permissions? does not know about the database
                if (FCalculator.GenerateResultRemoteClient())
                {
#if DEBUGMODE
                    FCalculator.GetParameters().Save("debugParameterReturn.xml", true);
                    FCalculator.GetResults().WriteCSV(FCalculator.GetParameters(), "debugResultReturn.csv");
#endif
                    this.FWinForm.Cursor = Cursors.Default;

                    if (FCalculator.GetParameters().Exists("SaveCSVFilename")
                        && (FCalculator.GetParameters().Get("SaveCSVFilename").ToString().Length > 0))
                    {
                        FCalculator.GetResults().WriteCSV(FCalculator.GetParameters(), FCalculator.GetParameters().Get("SaveCSVFilename").ToString());
                    }

                    if (FCalculator.GetParameters().GetOrDefault("OnlySaveCSV", -1, new TVariant(false)).ToBool() == true)
                    {
                        // TODO EnableDisableToolbar(true);
                    }
                    else
                    {
                        if ((this.FWinForm.Owner != null) && (this.FWinForm.Owner.GetType().ToString() == "TMainWinForm"))
                        {
                            // this is PetraClient_Experimenting
                            // using Delegate causes SEHException in PetraClient_Experimenting
                            PreviewReport();
                        }
                        else
                        {
                            myDelegate = @PreviewReport;
                            object[] Args = new Object[0];
                            FWinForm.Invoke((System.Delegate) new TMyUpdateDelegate(myDelegate));
                        }
                    }
                }
                else
                {
                    // if generateResult failed or was cancelled
                    this.FWinForm.Cursor = Cursors.Default;

                    // TODO EnableDisableToolbar(true);
                }
            }
            catch (Exception e)
            {
#if DEBUGMODE
                MessageBox.Show(e.ToString());
                MessageBox.Show(e.Message);
#endif

                // TODO EnableDisableToolbar(true);
            }
        }

        /// <summary>
        /// to be called by the thread, after the calculation of the report has been finished
        /// </summary>
        /// <returns>void</returns>
        protected void PreviewReport()
        {
            // show a print window with all kinds of output options
            TFrmPrintPreview printWindow = new TFrmPrintPreview(FWinForm.Handle, FReportName, FCalculator.GetDuration(),
                FCalculator.GetResults(), FCalculator.GetParameters());

            this.FWinForm.AddOwnedForm(printWindow);
            printWindow.Owner = FWinForm;

// TODO            printWindow.SetPrintChartProcedure(GenerateChart);
            printWindow.ShowDialog();

// TODO            EnableDisableToolbar(true);
        }

        #region Manage Settings

        /// <summary>
        /// This procedure loads the available saved settings into the Load menu
        ///
        /// </summary>
        protected void UpdateLoadingMenu(StringCollection ARecentlyUsedSettings)
        {
            for (System.Int32 Counter = 0; Counter <= ARecentlyUsedSettings.Count - 1; Counter++)
            {
                ToolStripItem mniItem, tbbItem;

                if (((IFrmReporting)FTheForm).GetRecentSettingsItems(Counter, out mniItem, out tbbItem))
                {
                    mniItem.Text = ARecentlyUsedSettings[Counter];

                    // TODO tbbItem.Text = ARecentlyUsedSettings[Counter];
                    mniItem.Visible = true;

                    // TODO tbbItem.Visible = true;
                }
            }

            for (System.Int32 Counter = ARecentlyUsedSettings.Count; true; Counter++)
            {
                ToolStripItem mniItem, tbbItem;

                if (((IFrmReporting)FTheForm).GetRecentSettingsItems(Counter, out mniItem, out tbbItem))
                {
                    mniItem.Visible = false;

                    // TODO tbbItem.Visible = false;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// This procedure loads the parameters of the given settings
        /// </summary>
        protected void LoadSettings(String ASettingsName)
        {
            TParameterList Parameters;
            StringCollection RecentlyUsedSettings;

            FCurrentSettingsName = ASettingsName;
            Parameters = new TParameterList();
            RecentlyUsedSettings = FStoredSettings.LoadSettings(ref FCurrentSettingsName, ref Parameters);

            // set the title of the window
            if (FCurrentSettingsName.Length > 0)
            {
                FWinForm.Text = FWindowCaption + ": " + FCurrentSettingsName;
            }
            else
            {
                FWinForm.Text = FWindowCaption;
            }

            SetControls(Parameters);
            UpdateLoadingMenu(RecentlyUsedSettings);
        }

        /// <summary>
        /// This procedure loads the parameters of the default settings;
        /// at the moment this is implemented to use the last used settings
        ///
        /// </summary>
        public void LoadDefaultSettings()
        {
            StringCollection RecentlyUsedSettings;

            RecentlyUsedSettings = FStoredSettings.GetRecentlyUsedSettings();

            if (RecentlyUsedSettings.Count > 0)
            {
                LoadSettings(RecentlyUsedSettings[0]);
            }
        }

        /// <summary>
        /// has anything changed in the currently selected column?
        /// if yes, show error message; telling the user to save changes first
        /// </summary>
        /// <returns>true if column has changed and error message was displayed</returns>
        private bool ColumnChangedWithErrorMessage(string AFailedAction)
        {
#if TODO
            if (ColumnChanged(FSelectedColumn))
            {
                MessageBox.Show(
                    AFailedAction + "." + Environment.NewLine + "Please first apply the changes to the current column, " +
                    Environment.NewLine +
                    "or cancel the changes.",
                    "Column changed");
                return true;
            }
            else
            {
                SelectColumn(-1);
            }
#endif
            return false;
        }

        /// <summary>
        /// show a dialog with all available stored settings for this report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MI_LoadSettingsDialog_Click(System.Object sender, System.EventArgs e)
        {
            if (ColumnChangedWithErrorMessage(Catalog.GetString("Settings cannot be loaded")))
            {
                return;
            }

            TFrmSettingsLoad SettingsDialog = new TFrmSettingsLoad(FStoredSettings);

            if (SettingsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadSettings(SettingsDialog.GetNewName());
            }
        }

        /// <summary>
        /// load settings from menu, recently used settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MI_LoadSettings_Click(System.Object sender, System.EventArgs e)
        {
            if (ColumnChangedWithErrorMessage(Catalog.GetString("Settings cannot be loaded")))
            {
                return;
            }

            ToolStripItem ctrl = (ToolStripItem)sender;

            if (ctrl.Name.Substring(3).StartsWith("LoadSettings")
                && !ctrl.Name.Contains("LoadSettingsDialog")
                && !ctrl.Name.EndsWith("LoadSettings"))
            {
                LoadSettings(ctrl.Text);
            }
        }

        /// <summary>
        /// save report settings with a new name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MI_SaveSettingsAs_Click(System.Object sender, System.EventArgs e)
        {
            if (ColumnChangedWithErrorMessage(Catalog.GetString("Settings cannot be saved")))
            {
                return;
            }

            // read the settings and parameters from the controls
            if (!ReadControlsWithErrorHandling())
            {
                return;
            }

            if (FCurrentSettingsName == "")
            {
                FCurrentSettingsName = FCurrentReport;
            }

            TFrmSettingsSave SettingsDialog = new TFrmSettingsSave(FStoredSettings, FCurrentSettingsName);

            if (SettingsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringCollection RecentlyUsedSettings = null;

                FCurrentSettingsName = SettingsDialog.GetNewName();

                // set the title of the window
                FWinForm.Text = FWindowCaption + ": " + FCurrentSettingsName;
                try
                {
                    RecentlyUsedSettings = this.FStoredSettings.SaveSettings(FCurrentSettingsName, FCalculator.GetParameters());
                }
                catch (Exception)
                {
                    MessageBox.Show("Not a valid name. Please use letters numbers and underscores etc, values not saved");
                    FWinForm.Text = FWindowCaption + ": Not a valid name, values not saved!";
                }

                if (RecentlyUsedSettings != null)
                {
                    UpdateLoadingMenu(RecentlyUsedSettings);
                }
            }
        }

        /// <summary>
        /// save report settings; if those are the system default settings, the user still has to enter a new name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MI_SaveSettings_Click(System.Object sender, System.EventArgs e)
        {
            if (ColumnChangedWithErrorMessage(Catalog.GetString("Settings cannot be saved")))
            {
                return;
            }

            if ((FCurrentSettingsName.Length == 0) || (FStoredSettings.IsSystemSettings(FCurrentSettingsName)))
            {
                MI_SaveSettingsAs_Click(sender, e);
            }
            else
            {
                // read the settings and parameters from the controls
                if (!ReadControlsWithErrorHandling())
                {
                    return;
                }

                StringCollection RecentlyUsedSettings = this.FStoredSettings.SaveSettings(FCurrentSettingsName, FCalculator.GetParameters());

                if (RecentlyUsedSettings != null)
                {
                    UpdateLoadingMenu(RecentlyUsedSettings);
                }
            }
        }

        /// <summary>
        /// maintain existing report settings (rename, delete)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MI_MaintainSettings_Click(System.Object sender, System.EventArgs e)
        {
            TFrmSettingsMaintain SettingsDialog;

            SettingsDialog = new TFrmSettingsMaintain(FStoredSettings);
            SettingsDialog.ShowDialog();
            UpdateLoadingMenu(this.FStoredSettings.GetRecentlyUsedSettings());
        }

        #endregion

        #region Parameter/Settings Handling

        /// <summary>
        /// to be called from outside
        /// </summary>
        /// <returns>true if successful
        /// </returns>
        protected virtual bool ReadControlsWithErrorHandling()
        {
            bool ReturnValue;
            TVerificationResult VerificationResultEntry;

            System.Collections.IEnumerator VerificationResultEnum;
            String UserMessage;
            ReturnValue = false;
            try
            {
                FVerificationResults = new TVerificationResultCollection();
                ReadControls();

                if (FVerificationResults.Count != 0)
                {
                    UserMessage = "Report could not be generated." + Environment.NewLine + Environment.NewLine + "Reasons:" + Environment.NewLine +
                                  Environment.NewLine;
                    VerificationResultEnum = FVerificationResults.GetEnumerator();

                    while (VerificationResultEnum.MoveNext())
                    {
                        VerificationResultEntry = ((TVerificationResult)VerificationResultEnum.Current);

                        if (VerificationResultEntry.FResultContext.Length > 0)
                        {
                            UserMessage = UserMessage + "  * [" + VerificationResultEntry.FResultContext + "] ";
                        }
                        else
                        {
                            UserMessage = UserMessage + "  * ";
                        }

                        UserMessage = UserMessage + VerificationResultEntry.FResultText + Environment.NewLine + Environment.NewLine;
                    }

                    MessageBox.Show(UserMessage, "Error");
                }
                else
                {
                    ReturnValue = true;
                }
            }
            catch (Exception e)
            {
#if DEBUGMODE
                MessageBox.Show(e.ToString(), "DEBUGMODE: Invalid Selection");

                // todo: use the verification tools from Christian
                MessageBox.Show(e.Message, "Invalid Selection");
#endif
            }
            return ReturnValue;
        }

        /// <summary>
        /// Reads the selected values from the controls,
        /// and stores them into the parameter system of FCalculator
        ///
        /// </summary>
        /// <returns>void</returns>
        public virtual void ReadControls()
        {
            // TODO
            FCalculator.ResetParameters();
            FCalculator.AddParameter("xmlfiles", FXMLFiles);
            FCalculator.AddParameter("currentReport", FCurrentReport);

            ((IFrmReporting) this.FTheForm).ReadControls(FCalculator);
        }

        /// <summary>
        /// Sets the selected values in the controls, using the parameters loaded from a file
        ///
        /// </summary>
        /// <returns>void</returns>
        public virtual void SetControls(TParameterList AParameters)
        {
            // TODO
            ((IFrmReporting) this.FTheForm).SetControls(AParameters);
        }

        #endregion

        #region Column Functions and Calculations

        /// <summary>
        /// This will return a string list of available functions
        ///
        /// </summary>
        /// <returns>void</returns>
        protected StringCollection GetAvailableFunctionsStringList()
        {
            StringCollection ReturnValue;

            ReturnValue = new StringCollection();

            foreach (TColumnFunction colfunc in FAvailableFunctions)
            {
                ReturnValue.Add(colfunc.GetDisplayValue());
            }

            return ReturnValue;
        }

        /// <summary>
        /// Remove an advertised function;
        /// that is necessary for some of the derived reports;
        /// e.g. on the monthly reports you don't want to see a "Actual End of Year"
        ///
        /// </summary>
        /// <returns>void</returns>
        protected void RemoveAvailableFunction(String AName)
        {
            foreach (TColumnFunction colfunc in FAvailableFunctions)
            {
                if (colfunc.GetDisplayValue() == AName)
                {
                    FAvailableFunctions.Remove(colfunc);
                    return;
                }
            }
        }

        /// <summary>
        /// This will add functions to the list of available functions
        ///
        /// </summary>
        /// <returns>void</returns>
        protected virtual void SetAvailableFunctions()
        {
            FAvailableFunctions = new ArrayList();
        }

        /// <summary>
        /// get the function System.Object of the given calculation string
        /// </summary>
        /// <returns>nil if the function cannot be found
        /// </returns>
        protected TColumnFunction GetFunction(String calculation)
        {
            if (FAvailableFunctions != null)
            {
                foreach (TColumnFunction Func in FAvailableFunctions)
                {
                    if (Func.GetDisplayValue() == calculation)
                    {
                        return Func;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AParameterList"></param>
        /// <param name="ACalculationName"></param>
        /// <param name="AColumnNr"></param>
        /// <returns></returns>
        protected TColumnFunction GetFunction(TParameterList AParameterList, String ACalculationName, System.Int32 AColumnNr)
        {
            TColumnFunction ReturnValue;

            ReturnValue = GetFunction(ACalculationName);

            if (ReturnValue == null)
            {
                // this might be a general function that has a parameter, that is displayed
                if (FAvailableFunctions != null)
                {
                    foreach (TColumnFunction Func in FAvailableFunctions)
                    {
                        if (Func.FDescription == ACalculationName)
                        {
                            // found an entry with e.g. DataLabelColumn
                            // now need to check if this columns FCalculationParameterValue is used
                            if (AParameterList.Get(Func.FCalculationParameterName, AColumnNr).ToString() == Func.FCalculationParameterValue)
                            {
                                return Func;
                            }
                        }
                    }
                }
            }

            return ReturnValue;
        }

        #endregion
    }

    /// <summary>
    /// a delegate for running the report preview window
    /// </summary>
    public delegate void TMyUpdateDelegate();

    /// for accessing the reporting form from the TFrmPetraReportingUtils object
    public interface IFrmReporting : IFrmPetra
    {
        /// <summary>
        /// read the values from the controls on the form
        /// </summary>
        /// <param name="ACalc"></param>
        void ReadControls(TRptCalculator ACalc);

        /// <summary>
        /// set the values of the controls on the form
        /// </summary>
        /// <param name="AParameters"></param>
        void SetControls(TParameterList AParameters);

        /// <summary>
        /// this is used for writing the captions of the menu items and toolbar buttons for recently used report settings
        /// </summary>
        /// <returns>false if an item with that index does not exist</returns>
        bool GetRecentSettingsItems(int AIndex, out ToolStripItem mniItem, out ToolStripItem tbbItem);
    }
}