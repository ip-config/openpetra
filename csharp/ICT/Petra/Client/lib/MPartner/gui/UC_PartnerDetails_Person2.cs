/* auto generated with nant generateWinforms from UC_PartnerDetails_Person2.yaml and template controlMaintainTable
 *
 * DO NOT edit manually, DO NOT edit with the designer
 *
 */
/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       auto generated
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
using System.Windows.Forms;
using System.Data;
using Ict.Petra.Shared;
using System.Resources;
using System.Collections.Specialized;
using Mono.Unix;
using Ict.Common;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Common.Controls;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Shared.MPartner.Partner.Data;

namespace Ict.Petra.Client.MPartner.Gui
{

  /// auto generated user control
  public partial class TUC_PartnerDetails_Person2: System.Windows.Forms.UserControl, Ict.Petra.Client.CommonForms.IFrmPetra
  {
    private TFrmPetraEditUtils FPetraUtilsObject;

    private Ict.Petra.Shared.MPartner.Partner.Data.PartnerEditTDS FMainDS;

    /// constructor
    public TUC_PartnerDetails_Person2() : base()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      #region CATALOGI18N

      // this code has been inserted by GenerateI18N, all changes in this region will be overwritten by GenerateI18N
      this.lblPreferredName.Text = Catalog.GetString("Preferred Name:");
      this.lblPreviousName.Text = Catalog.GetString("Previous Name:");
      this.lblLocalName.Text = Catalog.GetString("Local Name:");
      this.grpNames.Text = Catalog.GetString("Names");
      this.lblDateOfBirth.Text = Catalog.GetString("Date Of Birth:");
      this.lblDecorations.Text = Catalog.GetString("Decorations:");
      this.lblMaritalStatus.Text = Catalog.GetString("Marital Status:");
      this.lblAcademicTitle.Text = Catalog.GetString("Academic Title:");
      this.lblMaritalStatusSince.Text = Catalog.GetString("Marital Status Since:");
      this.lblMaritalStatusComment.Text = Catalog.GetString("Marital Status Comment:");
      this.lblLanguageCode.Text = Catalog.GetString("Language Code:");
      this.lblAcquisitionCode.Text = Catalog.GetString("Acquisition Code:");
      this.txtOccupationCode.ButtonText = Catalog.GetString("Find");
      this.lblOccupationCode.Text = Catalog.GetString("Occupation:");
      this.grpMisc.Text = Catalog.GetString("Miscellaneous");
      #endregion

    }

    /// helper object for the whole screen
    public TFrmPetraEditUtils PetraUtilsObject
    {
        set
        {
            FPetraUtilsObject = value;
        }
    }

    /// dataset for the whole screen
    public Ict.Petra.Shared.MPartner.Partner.Data.PartnerEditTDS MainDS
    {
        set
        {
            FMainDS = value;
        }
    }

    /// needs to be called after FMainDS and FPetraUtilsObject have been set
    public void InitUserControl()
    {
        FPetraUtilsObject.SetStatusBarText(txtPreferredName, Catalog.GetString("Enter the name this person is commonly known by"));
        FPetraUtilsObject.SetStatusBarText(txtPreviousName, Catalog.GetString("Enter the previously used Surname (eg before marriage)"));
        FPetraUtilsObject.SetStatusBarText(txtLocalName, Catalog.GetString("Enter a short name for this partner in your local language"));
        FPetraUtilsObject.SetStatusBarText(txtDateOfBirth, Catalog.GetString("Enter the date the person was born"));
        FPetraUtilsObject.SetStatusBarText(txtDecorations, Catalog.GetString("ie. Nobel Peace Prize, Olympic Gold Medal, ?, etc."));
        FPetraUtilsObject.SetStatusBarText(cmbMaritalStatus, Catalog.GetString("Select marital status"));
        cmbMaritalStatus.InitialiseUserControl();
        FPetraUtilsObject.SetStatusBarText(txtAcademicTitle, Catalog.GetString("Enter the academic title for the person"));
        FPetraUtilsObject.SetStatusBarText(txtMaritalStatusSince, Catalog.GetString("Date from which the marital status is valid"));
        FPetraUtilsObject.SetStatusBarText(txtMaritalStatusComment, Catalog.GetString("Enter a comment for the marital status"));
        FPetraUtilsObject.SetStatusBarText(cmbLanguageCode, Catalog.GetString("Select the partner's preferred language"));
        cmbLanguageCode.InitialiseUserControl();
        FPetraUtilsObject.SetStatusBarText(cmbAcquisitionCode, Catalog.GetString("Select a method-of-acquisition code"));
        cmbAcquisitionCode.InitialiseUserControl();
        FPetraUtilsObject.SetStatusBarText(txtOccupationCode, Catalog.GetString("Enter an occupation code"));

        if(FMainDS != null)
        {
            ShowData(FMainDS.PPerson[0]);
        }
    }

    private void ShowData(PPersonRow ARow)
    {
        FPetraUtilsObject.DisableDataChangedEvent();
        if (ARow.IsPreferedNameNull())
        {
            txtPreferredName.Text = String.Empty;
        }
        else
        {
            txtPreferredName.Text = ARow.PreferedName;
        }
        if (FMainDS.PPartner[0].IsPreviousNameNull())
        {
            txtPreviousName.Text = String.Empty;
        }
        else
        {
            txtPreviousName.Text = FMainDS.PPartner[0].PreviousName;
        }
        if (FMainDS.PPartner[0].IsPartnerShortNameLocNull())
        {
            txtLocalName.Text = String.Empty;
        }
        else
        {
            txtLocalName.Text = FMainDS.PPartner[0].PartnerShortNameLoc;
        }
        if (ARow.IsDateOfBirthNull())
        {
            txtDateOfBirth.Text = String.Empty;
        }
        else
        {
            txtDateOfBirth.Text = ARow.DateOfBirth.ToString();
        }
        if (ARow.IsDecorationsNull())
        {
            txtDecorations.Text = String.Empty;
        }
        else
        {
            txtDecorations.Text = ARow.Decorations;
        }
        if (ARow.IsMaritalStatusNull())
        {
            cmbMaritalStatus.SelectedIndex = -1;
        }
        else
        {
            cmbMaritalStatus.SetSelectedString(ARow.MaritalStatus);
        }
        if (ARow.IsAcademicTitleNull())
        {
            txtAcademicTitle.Text = String.Empty;
        }
        else
        {
            txtAcademicTitle.Text = ARow.AcademicTitle;
        }
        if (ARow.IsMaritalStatusSinceNull())
        {
            txtMaritalStatusSince.Text = String.Empty;
        }
        else
        {
            txtMaritalStatusSince.Text = ARow.MaritalStatusSince.ToString();
        }
        if (ARow.IsMaritalStatusCommentNull())
        {
            txtMaritalStatusComment.Text = String.Empty;
        }
        else
        {
            txtMaritalStatusComment.Text = ARow.MaritalStatusComment;
        }
        if (FMainDS.PPartner[0].IsLanguageCodeNull())
        {
            cmbLanguageCode.SelectedIndex = -1;
        }
        else
        {
            cmbLanguageCode.SetSelectedString(FMainDS.PPartner[0].LanguageCode);
        }
        if (FMainDS.PPartner[0].IsAcquisitionCodeNull())
        {
            cmbAcquisitionCode.SelectedIndex = -1;
        }
        else
        {
            cmbAcquisitionCode.SetSelectedString(FMainDS.PPartner[0].AcquisitionCode);
        }
        if (ARow.IsOccupationCodeNull())
        {
            txtOccupationCode.Text = String.Empty;
        }
        else
        {
            txtOccupationCode.Text = String.Format("{0:0000000000}", ARow.OccupationCode);
        }
        FPetraUtilsObject.EnableDataChangedEvent();
    }

#region Implement interface functions
    /// auto generated
    public void RunOnceOnActivation()
    {
    }

    /// <summary>
    /// Adds event handlers for the appropiate onChange event to call a central procedure
    /// </summary>
    public void HookupAllControls()
    {
    }

    /// auto generated
    public void HookupAllInContainer(Control container)
    {
        FPetraUtilsObject.HookupAllInContainer(container);
    }

    /// auto generated
    public bool CanClose()
    {
        return FPetraUtilsObject.CanClose();
    }

    /// auto generated
    public TFrmPetraUtils GetPetraUtilsObject()
    {
        return (TFrmPetraUtils)FPetraUtilsObject;
    }
#endregion
  }
}