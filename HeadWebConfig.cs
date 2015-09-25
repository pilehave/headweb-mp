using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using System.Reflection;

namespace HeadWeb
{
  public partial class HeadWebConfig : Form
  {
    public HeadWebConfig()
    {
      InitializeComponent();
    }

    private void frmHeadWebLoadSettings(object sender, EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "HeadWeb.xml")))
      {
        inputUsername.Text = xmlreader.GetValueAsString("Settings", "Username", "");
        inputPassword.Text = xmlreader.GetValueAsString("Settings", "Password", "");
        inputPincode.Text = xmlreader.GetValueAsString("Settings", "PINCode", "");
        if (xmlreader.GetValueAsString("Settings", "VideoQuality", "").Length <= 0)
        {
          videoQuality.SelectedIndex = 0;
        }
        else
        {
          videoQuality.SelectedIndex = videoQuality.Items.IndexOf(xmlreader.GetValueAsString("Settings", "VideoQuality", ""));
        }
        if (xmlreader.GetValueAsInt("Settings", "AutoLogin", 0) != 0) checkboxLogin.Checked = true;
        if (xmlreader.GetValueAsInt("Settings", "PinApprovePurchase", 0) != 0) checkboxPin.Checked = true;
        if (xmlreader.GetValueAsInt("Settings", "DisableAdultMovies", 0) != 0) checkboxAdult.Checked = true;
        if (xmlreader.GetValueAsInt("Settings", "UseOrginalName", 0) != 0) checkboxOrginalName.Checked = true;
      }
    }

    // Save settings to file
    private void btSave_Click(object sender, EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "HeadWeb.xml")))
      {
        xmlwriter.SetValue("Settings", "Username", inputUsername.Text);
        xmlwriter.SetValue("Settings", "Password", inputPassword.Text);
        xmlwriter.SetValue("Settings", "PINCode", inputPincode.Text);
        xmlwriter.SetValue("Settings", "VideoQuality", videoQuality.Text);
        xmlwriter.SetValue("Settings", "AutoLogin", checkboxLogin.Checked ? 1 : 0);
        xmlwriter.SetValue("Settings", "PinApprovePurchase", checkboxPin.Checked ? 1 : 0);
        xmlwriter.SetValue("Settings", "DisableAdultMovies", checkboxAdult.Checked ? 1 : 0);
        xmlwriter.SetValue("Settings", "UseOrginalName", checkboxOrginalName.Checked ? 1 : 0);
        this.Close();
      }
    }

    private void btCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}
