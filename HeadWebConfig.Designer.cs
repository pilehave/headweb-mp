namespace HeadWeb
{
  partial class HeadWebConfig
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btSave = new System.Windows.Forms.Button();
      this.btCancel = new System.Windows.Forms.Button();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.checkboxLogin = new System.Windows.Forms.CheckBox();
      this.inputPassword = new System.Windows.Forms.TextBox();
      this.inputUsername = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.checkboxPin = new System.Windows.Forms.CheckBox();
      this.inputPincode = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.videoQuality = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.checkboxOrginalName = new System.Windows.Forms.CheckBox();
      this.checkboxAdult = new System.Windows.Forms.CheckBox();
      this.statusStrip1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // btSave
      // 
      this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btSave.Location = new System.Drawing.Point(151, 384);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(75, 22);
      this.btSave.TabIndex = 19;
      this.btSave.Text = "Save";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new System.EventHandler(this.btSave_Click);
      // 
      // btCancel
      // 
      this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btCancel.Location = new System.Drawing.Point(232, 384);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new System.Drawing.Size(75, 22);
      this.btCancel.TabIndex = 20;
      this.btCancel.Text = "Cancel";
      this.btCancel.UseVisualStyleBackColor = true;
      this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
      this.statusStrip1.Location = new System.Drawing.Point(0, 419);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(319, 22);
      this.statusStrip1.TabIndex = 27;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
      this.toolStripStatusLabel1.Text = "Ready";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkboxLogin);
      this.groupBox1.Controls.Add(this.inputPassword);
      this.groupBox1.Controls.Add(this.inputUsername);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(296, 105);
      this.groupBox1.TabIndex = 28;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Login settings";
      // 
      // checkboxLogin
      // 
      this.checkboxLogin.AutoSize = true;
      this.checkboxLogin.Location = new System.Drawing.Point(10, 79);
      this.checkboxLogin.Name = "checkboxLogin";
      this.checkboxLogin.Size = new System.Drawing.Size(214, 17);
      this.checkboxLogin.TabIndex = 4;
      this.checkboxLogin.Text = "Automatic login when MediaPortal starts";
      this.checkboxLogin.UseVisualStyleBackColor = true;
      // 
      // inputPassword
      // 
      this.inputPassword.Location = new System.Drawing.Point(115, 48);
      this.inputPassword.Name = "inputPassword";
      this.inputPassword.PasswordChar = '*';
      this.inputPassword.Size = new System.Drawing.Size(140, 20);
      this.inputPassword.TabIndex = 3;
      // 
      // inputUsername
      // 
      this.inputUsername.Location = new System.Drawing.Point(115, 19);
      this.inputUsername.Name = "inputUsername";
      this.inputUsername.Size = new System.Drawing.Size(140, 20);
      this.inputUsername.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(7, 51);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(56, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Password:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(7, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(58, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Username:";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.checkboxPin);
      this.groupBox2.Controls.Add(this.inputPincode);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Location = new System.Drawing.Point(12, 130);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(296, 76);
      this.groupBox2.TabIndex = 29;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "PIN Code";
      // 
      // checkboxPin
      // 
      this.checkboxPin.AutoSize = true;
      this.checkboxPin.Location = new System.Drawing.Point(10, 50);
      this.checkboxPin.Name = "checkboxPin";
      this.checkboxPin.Size = new System.Drawing.Size(258, 17);
      this.checkboxPin.TabIndex = 4;
      this.checkboxPin.Text = "PIN Code must be entered before each purchase";
      this.checkboxPin.UseVisualStyleBackColor = true;
      // 
      // inputPincode
      // 
      this.inputPincode.Location = new System.Drawing.Point(115, 19);
      this.inputPincode.Name = "inputPincode";
      this.inputPincode.PasswordChar = '*';
      this.inputPincode.Size = new System.Drawing.Size(70, 20);
      this.inputPincode.TabIndex = 2;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(7, 23);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(98, 13);
      this.label4.TabIndex = 0;
      this.label4.Text = "PIN Code (4 digits):";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.videoQuality);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Location = new System.Drawing.Point(12, 220);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(296, 61);
      this.groupBox3.TabIndex = 30;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Video quality";
      // 
      // videoQuality
      // 
      this.videoQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoQuality.FormattingEnabled = true;
      this.videoQuality.Items.AddRange(new object[] {
            "Let me choose",
            "Low",
            "Medium",
            "High"});
      this.videoQuality.Location = new System.Drawing.Point(115, 23);
      this.videoQuality.MaxDropDownItems = 3;
      this.videoQuality.Name = "videoQuality";
      this.videoQuality.Size = new System.Drawing.Size(121, 21);
      this.videoQuality.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(7, 23);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(73, 13);
      this.label3.TabIndex = 0;
      this.label3.Text = "Select quality:";
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.checkboxAdult);
      this.groupBox4.Controls.Add(this.checkboxOrginalName);
      this.groupBox4.Location = new System.Drawing.Point(12, 296);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(296, 76);
      this.groupBox4.TabIndex = 31;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Movie content";
      // 
      // checkboxOrginalName
      // 
      this.checkboxOrginalName.AutoSize = true;
      this.checkboxOrginalName.Location = new System.Drawing.Point(10, 50);
      this.checkboxOrginalName.Name = "checkboxOrginalName";
      this.checkboxOrginalName.Size = new System.Drawing.Size(241, 17);
      this.checkboxOrginalName.TabIndex = 4;
      this.checkboxOrginalName.Text = "View original name instead of translated name";
      this.checkboxOrginalName.UseVisualStyleBackColor = true;
      // 
      // checkboxAdult
      // 
      this.checkboxAdult.AutoSize = true;
      this.checkboxAdult.Location = new System.Drawing.Point(10, 27);
      this.checkboxAdult.Name = "checkboxAdult";
      this.checkboxAdult.Size = new System.Drawing.Size(158, 17);
      this.checkboxAdult.TabIndex = 5;
      this.checkboxAdult.Text = "Disable adult (erotic) movies";
      this.checkboxAdult.UseVisualStyleBackColor = true;
      // 
      // HeadWebConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(319, 441);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.btCancel);
      this.Name = "HeadWebConfig";
      this.Text = "HeadWeb Configuration";
      this.Load += new System.EventHandler(this.frmHeadWebLoadSettings);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btSave;
    private System.Windows.Forms.Button btCancel;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox checkboxLogin;
    private System.Windows.Forms.TextBox inputPassword;
    private System.Windows.Forms.TextBox inputUsername;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox checkboxPin;
    private System.Windows.Forms.TextBox inputPincode;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox videoQuality;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.CheckBox checkboxAdult;
    private System.Windows.Forms.CheckBox checkboxOrginalName;
  }
}