namespace SerialPortTerminal
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ReceivedTextBox = new System.Windows.Forms.TextBox();
            this.CommandTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ResetButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PortConfiguration = new Utilities.SerialPortSettingsControl();
            this.CrcErrorLabel = new System.Windows.Forms.Label();
            this.NotReadyLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.LoggingButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.CrcOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.InitialValueTextBox = new System.Windows.Forms.TextBox();
            this.PolynomialTextBox = new System.Windows.Forms.TextBox();
            this.MsByteFirstCheckBox = new System.Windows.Forms.CheckBox();
            this.ResidueTextBox = new System.Windows.Forms.TextBox();
            this.MsBitFirstCheckBox = new System.Windows.Forms.CheckBox();
            this.TermCharTextBox = new System.Windows.Forms.TextBox();
            this.OmitTermCharCheckBox = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.PostInvertCheckbox = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.TermCharLabel = new System.Windows.Forms.Label();
            this.txCRCLabel = new System.Windows.Forms.Label();
            this.rxCRCLabel = new System.Windows.Forms.Label();
            this.bytesCheckBox = new System.Windows.Forms.CheckBox();
            this.bytePaceTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.messagePaceTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.CrcButton = new System.Windows.Forms.Button();
            this.txCrcValue = new System.Windows.Forms.Label();
            this.rxCrcValue = new System.Windows.Forms.Label();
            this.CommandCountValue = new System.Windows.Forms.Label();
            this.ResponseCountValue = new System.Windows.Forms.Label();
            this.BytesReadValue = new System.Windows.Forms.Label();
            this.LogCheckBox = new System.Windows.Forms.CheckBox();
            this.ClearButton = new System.Windows.Forms.Button();
            this.IgnoreCRCErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.CrcOptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReceivedTextBox
            // 
            this.ReceivedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReceivedTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ReceivedTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ReceivedTextBox.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReceivedTextBox.Location = new System.Drawing.Point(16, 343);
            this.ReceivedTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.ReceivedTextBox.Multiline = true;
            this.ReceivedTextBox.Name = "ReceivedTextBox";
            this.ReceivedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ReceivedTextBox.Size = new System.Drawing.Size(653, 281);
            this.ReceivedTextBox.TabIndex = 2;
            this.ReceivedTextBox.WordWrap = false;
            // 
            // CommandTextBox
            // 
            this.CommandTextBox.AcceptsReturn = true;
            this.CommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandTextBox.Location = new System.Drawing.Point(16, 311);
            this.CommandTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.CommandTextBox.Name = "CommandTextBox";
            this.CommandTextBox.Size = new System.Drawing.Size(653, 22);
            this.CommandTextBox.TabIndex = 1;
            this.CommandTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
            this.CommandTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TextBoxPreviewKeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 292);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Command:";
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.Location = new System.Drawing.Point(679, 633);
            this.ResetButton.Margin = new System.Windows.Forms.Padding(4);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 28);
            this.ResetButton.TabIndex = 5;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.PortConfiguration);
            this.groupBox1.Location = new System.Drawing.Point(16, 33);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(303, 240);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Controller Port Settings";
            // 
            // PortConfiguration
            // 
            this.PortConfiguration.AutoSize = true;
            this.PortConfiguration.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PortConfiguration.BaudRate = 115200;
            this.PortConfiguration.DataBits = 8;
            this.PortConfiguration.Handshake = System.IO.Ports.Handshake.None;
            this.PortConfiguration.Location = new System.Drawing.Point(8, 18);
            this.PortConfiguration.Margin = new System.Windows.Forms.Padding(5);
            this.PortConfiguration.Name = "PortConfiguration";
            this.PortConfiguration.Parity = System.IO.Ports.Parity.None;
            this.PortConfiguration.PortName = "COM5";
            this.PortConfiguration.Size = new System.Drawing.Size(286, 198);
            this.PortConfiguration.StopBits = System.IO.Ports.StopBits.One;
            this.PortConfiguration.TabIndex = 6;
            // 
            // CrcErrorLabel
            // 
            this.CrcErrorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CrcErrorLabel.AutoSize = true;
            this.CrcErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CrcErrorLabel.Location = new System.Drawing.Point(12, 639);
            this.CrcErrorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CrcErrorLabel.Name = "CrcErrorLabel";
            this.CrcErrorLabel.Size = new System.Drawing.Size(85, 17);
            this.CrcErrorLabel.TabIndex = 8;
            this.CrcErrorLabel.Text = "CRC Error!";
            this.CrcErrorLabel.Visible = false;
            // 
            // NotReadyLabel
            // 
            this.NotReadyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NotReadyLabel.AutoSize = true;
            this.NotReadyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NotReadyLabel.Location = new System.Drawing.Point(109, 639);
            this.NotReadyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NotReadyLabel.Name = "NotReadyLabel";
            this.NotReadyLabel.Size = new System.Drawing.Size(84, 17);
            this.NotReadyLabel.TabIndex = 9;
            this.NotReadyLabel.Text = "Not Ready";
            this.NotReadyLabel.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(820, 28);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileOpenMenuItem,
            this.exitMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(46, 24);
            this.FileMenuItem.Text = "&File";
            // 
            // fileOpenMenuItem
            // 
            this.fileOpenMenuItem.Name = "fileOpenMenuItem";
            this.fileOpenMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.fileOpenMenuItem.Size = new System.Drawing.Size(190, 26);
            this.fileOpenMenuItem.Text = "&Open...";
            this.fileOpenMenuItem.Click += new System.EventHandler(this.fileOpenMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitMenuItem.Size = new System.Drawing.Size(190, 26);
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "txt";
            this.openFileDialog1.Filter = "Text files|*.txt;*.csv";
            this.openFileDialog1.Title = "Select file to graph";
            // 
            // LoggingButton
            // 
            this.LoggingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoggingButton.Location = new System.Drawing.Point(561, 633);
            this.LoggingButton.Margin = new System.Windows.Forms.Padding(4);
            this.LoggingButton.Name = "LoggingButton";
            this.LoggingButton.Size = new System.Drawing.Size(109, 28);
            this.LoggingButton.TabIndex = 11;
            this.LoggingButton.Text = "Start Logging";
            this.LoggingButton.UseVisualStyleBackColor = true;
            this.LoggingButton.Click += new System.EventHandler(this.LoggingButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(679, 537);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Responses";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(679, 489);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Commands";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(679, 585);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "BytesRead";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CrcOptionsGroupBox
            // 
            this.CrcOptionsGroupBox.Controls.Add(this.label6);
            this.CrcOptionsGroupBox.Controls.Add(this.InitialValueTextBox);
            this.CrcOptionsGroupBox.Controls.Add(this.PolynomialTextBox);
            this.CrcOptionsGroupBox.Controls.Add(this.MsByteFirstCheckBox);
            this.CrcOptionsGroupBox.Controls.Add(this.ResidueTextBox);
            this.CrcOptionsGroupBox.Controls.Add(this.MsBitFirstCheckBox);
            this.CrcOptionsGroupBox.Controls.Add(this.TermCharTextBox);
            this.CrcOptionsGroupBox.Controls.Add(this.OmitTermCharCheckBox);
            this.CrcOptionsGroupBox.Controls.Add(this.label7);
            this.CrcOptionsGroupBox.Controls.Add(this.PostInvertCheckbox);
            this.CrcOptionsGroupBox.Controls.Add(this.label8);
            this.CrcOptionsGroupBox.Controls.Add(this.TermCharLabel);
            this.CrcOptionsGroupBox.Location = new System.Drawing.Point(327, 154);
            this.CrcOptionsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.CrcOptionsGroupBox.Name = "CrcOptionsGroupBox";
            this.CrcOptionsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.CrcOptionsGroupBox.Size = new System.Drawing.Size(455, 121);
            this.CrcOptionsGroupBox.TabIndex = 13;
            this.CrcOptionsGroupBox.TabStop = false;
            this.CrcOptionsGroupBox.Text = "CRC Options";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 31);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 16);
            this.label6.TabIndex = 0;
            this.label6.Text = "Initial Value";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // InitialValueTextBox
            // 
            this.InitialValueTextBox.Location = new System.Drawing.Point(97, 27);
            this.InitialValueTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.InitialValueTextBox.Name = "InitialValueTextBox";
            this.InitialValueTextBox.Size = new System.Drawing.Size(57, 22);
            this.InitialValueTextBox.TabIndex = 2;
            this.InitialValueTextBox.Text = "FFFF";
            this.InitialValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialValueTextBox.Leave += new System.EventHandler(this.InitialValueTextBox_Leave);
            // 
            // PolynomialTextBox
            // 
            this.PolynomialTextBox.Location = new System.Drawing.Point(248, 27);
            this.PolynomialTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.PolynomialTextBox.Name = "PolynomialTextBox";
            this.PolynomialTextBox.Size = new System.Drawing.Size(57, 22);
            this.PolynomialTextBox.TabIndex = 2;
            this.PolynomialTextBox.Text = "BAAD";
            this.PolynomialTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PolynomialTextBox.Leave += new System.EventHandler(this.PolynomialTextBox_Leave);
            // 
            // MsByteFirstCheckBox
            // 
            this.MsByteFirstCheckBox.AutoSize = true;
            this.MsByteFirstCheckBox.Location = new System.Drawing.Point(215, 59);
            this.MsByteFirstCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.MsByteFirstCheckBox.Name = "MsByteFirstCheckBox";
            this.MsByteFirstCheckBox.Size = new System.Drawing.Size(82, 20);
            this.MsByteFirstCheckBox.TabIndex = 10;
            this.MsByteFirstCheckBox.Text = "MSB-first";
            this.MsByteFirstCheckBox.UseVisualStyleBackColor = true;
            // 
            // ResidueTextBox
            // 
            this.ResidueTextBox.Location = new System.Drawing.Point(384, 27);
            this.ResidueTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.ResidueTextBox.Name = "ResidueTextBox";
            this.ResidueTextBox.Size = new System.Drawing.Size(57, 22);
            this.ResidueTextBox.TabIndex = 2;
            this.ResidueTextBox.Text = "0820";
            this.ResidueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ResidueTextBox.Leave += new System.EventHandler(this.ResidueTextBox_Leave);
            // 
            // MsBitFirstCheckBox
            // 
            this.MsBitFirstCheckBox.AutoSize = true;
            this.MsBitFirstCheckBox.Location = new System.Drawing.Point(121, 59);
            this.MsBitFirstCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.MsBitFirstCheckBox.Name = "MsBitFirstCheckBox";
            this.MsBitFirstCheckBox.Size = new System.Drawing.Size(79, 20);
            this.MsBitFirstCheckBox.TabIndex = 10;
            this.MsBitFirstCheckBox.Text = "msb-first";
            this.MsBitFirstCheckBox.UseVisualStyleBackColor = true;
            // 
            // TermCharTextBox
            // 
            this.TermCharTextBox.Location = new System.Drawing.Point(227, 85);
            this.TermCharTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.TermCharTextBox.Name = "TermCharTextBox";
            this.TermCharTextBox.Size = new System.Drawing.Size(44, 22);
            this.TermCharTextBox.TabIndex = 2;
            this.TermCharTextBox.Text = "03";
            this.TermCharTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TermCharTextBox.Leave += new System.EventHandler(this.TermCharTextBox_Leave);
            // 
            // OmitTermCharCheckBox
            // 
            this.OmitTermCharCheckBox.AutoSize = true;
            this.OmitTermCharCheckBox.Location = new System.Drawing.Point(12, 87);
            this.OmitTermCharCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.OmitTermCharCheckBox.Name = "OmitTermCharCheckBox";
            this.OmitTermCharCheckBox.Size = new System.Drawing.Size(119, 20);
            this.OmitTermCharCheckBox.TabIndex = 10;
            this.OmitTermCharCheckBox.Text = "Omit TermChar";
            this.OmitTermCharCheckBox.UseVisualStyleBackColor = true;
            this.OmitTermCharCheckBox.CheckedChanged += new System.EventHandler(this.OmitTermCharCheckBox_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(164, 31);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 16);
            this.label7.TabIndex = 0;
            this.label7.Text = "Polynomial";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PostInvertCheckbox
            // 
            this.PostInvertCheckbox.AutoSize = true;
            this.PostInvertCheckbox.Checked = true;
            this.PostInvertCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PostInvertCheckbox.Location = new System.Drawing.Point(12, 59);
            this.PostInvertCheckbox.Margin = new System.Windows.Forms.Padding(4);
            this.PostInvertCheckbox.Name = "PostInvertCheckbox";
            this.PostInvertCheckbox.Size = new System.Drawing.Size(92, 20);
            this.PostInvertCheckbox.TabIndex = 10;
            this.PostInvertCheckbox.Text = "Post-invert";
            this.PostInvertCheckbox.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(315, 31);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 16);
            this.label8.TabIndex = 0;
            this.label8.Text = "Residue";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TermCharLabel
            // 
            this.TermCharLabel.AutoSize = true;
            this.TermCharLabel.Location = new System.Drawing.Point(148, 88);
            this.TermCharLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TermCharLabel.Name = "TermCharLabel";
            this.TermCharLabel.Size = new System.Drawing.Size(67, 16);
            this.TermCharLabel.TabIndex = 0;
            this.TermCharLabel.Text = "TermChar";
            this.TermCharLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txCRCLabel
            // 
            this.txCRCLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txCRCLabel.AutoSize = true;
            this.txCRCLabel.Location = new System.Drawing.Point(679, 315);
            this.txCRCLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.txCRCLabel.Name = "txCRCLabel";
            this.txCRCLabel.Size = new System.Drawing.Size(44, 16);
            this.txCRCLabel.TabIndex = 5;
            this.txCRCLabel.Text = "txCRC";
            this.txCRCLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rxCRCLabel
            // 
            this.rxCRCLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rxCRCLabel.AutoSize = true;
            this.rxCRCLabel.Location = new System.Drawing.Point(679, 348);
            this.rxCRCLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.rxCRCLabel.Name = "rxCRCLabel";
            this.rxCRCLabel.Size = new System.Drawing.Size(45, 16);
            this.rxCRCLabel.TabIndex = 5;
            this.rxCRCLabel.Text = "rxCRC";
            this.rxCRCLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bytesCheckBox
            // 
            this.bytesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bytesCheckBox.AutoSize = true;
            this.bytesCheckBox.Location = new System.Drawing.Point(687, 378);
            this.bytesCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.bytesCheckBox.Name = "bytesCheckBox";
            this.bytesCheckBox.Size = new System.Drawing.Size(96, 20);
            this.bytesCheckBox.TabIndex = 10;
            this.bytesCheckBox.Text = "show bytes";
            this.bytesCheckBox.UseVisualStyleBackColor = true;
            this.bytesCheckBox.CheckedChanged += new System.EventHandler(this.bytesCheckBox_CheckedChanged);
            // 
            // bytePaceTextBox
            // 
            this.bytePaceTextBox.Location = new System.Drawing.Point(431, 41);
            this.bytePaceTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.bytePaceTextBox.Name = "bytePaceTextBox";
            this.bytePaceTextBox.Size = new System.Drawing.Size(57, 22);
            this.bytePaceTextBox.TabIndex = 2;
            this.bytePaceTextBox.Text = "-1";
            this.bytePaceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.bytePaceTextBox.Leave += new System.EventHandler(this.bytePaceTextBox_Leave);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(351, 44);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 16);
            this.label10.TabIndex = 0;
            this.label10.Text = "byte pace";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // messagePaceTextBox
            // 
            this.messagePaceTextBox.Location = new System.Drawing.Point(431, 64);
            this.messagePaceTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.messagePaceTextBox.Name = "messagePaceTextBox";
            this.messagePaceTextBox.Size = new System.Drawing.Size(57, 22);
            this.messagePaceTextBox.TabIndex = 2;
            this.messagePaceTextBox.Text = "0";
            this.messagePaceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.messagePaceTextBox.Leave += new System.EventHandler(this.messagePaceTextBox_Leave);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(321, 68);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 16);
            this.label11.TabIndex = 0;
            this.label11.Text = "message pace";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(497, 44);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(112, 16);
            this.label12.TabIndex = 0;
            this.label12.Text = "(ms; -1 to disable)";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(497, 68);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(33, 16);
            this.label13.TabIndex = 0;
            this.label13.Text = "(ms)";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CrcButton
            // 
            this.CrcButton.Location = new System.Drawing.Point(327, 118);
            this.CrcButton.Margin = new System.Windows.Forms.Padding(4);
            this.CrcButton.Name = "CrcButton";
            this.CrcButton.Size = new System.Drawing.Size(127, 28);
            this.CrcButton.TabIndex = 5;
            this.CrcButton.Text = "Disable CRC";
            this.CrcButton.UseVisualStyleBackColor = true;
            this.CrcButton.Click += new System.EventHandler(this.CrcButton_Click);
            // 
            // txCrcValue
            // 
            this.txCrcValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txCrcValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txCrcValue.Location = new System.Drawing.Point(736, 311);
            this.txCrcValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.txCrcValue.Name = "txCrcValue";
            this.txCrcValue.Size = new System.Drawing.Size(59, 25);
            this.txCrcValue.TabIndex = 5;
            this.txCrcValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rxCrcValue
            // 
            this.rxCrcValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rxCrcValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.rxCrcValue.Location = new System.Drawing.Point(736, 346);
            this.rxCrcValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.rxCrcValue.Name = "rxCrcValue";
            this.rxCrcValue.Size = new System.Drawing.Size(59, 25);
            this.rxCrcValue.TabIndex = 5;
            this.rxCrcValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CommandCountValue
            // 
            this.CommandCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandCountValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CommandCountValue.Location = new System.Drawing.Point(679, 505);
            this.CommandCountValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CommandCountValue.Name = "CommandCountValue";
            this.CommandCountValue.Size = new System.Drawing.Size(123, 25);
            this.CommandCountValue.TabIndex = 5;
            this.CommandCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ResponseCountValue
            // 
            this.ResponseCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResponseCountValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ResponseCountValue.Location = new System.Drawing.Point(679, 553);
            this.ResponseCountValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ResponseCountValue.Name = "ResponseCountValue";
            this.ResponseCountValue.Size = new System.Drawing.Size(123, 25);
            this.ResponseCountValue.TabIndex = 5;
            this.ResponseCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BytesReadValue
            // 
            this.BytesReadValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BytesReadValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.BytesReadValue.Location = new System.Drawing.Point(679, 601);
            this.BytesReadValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BytesReadValue.Name = "BytesReadValue";
            this.BytesReadValue.Size = new System.Drawing.Size(123, 25);
            this.BytesReadValue.TabIndex = 5;
            this.BytesReadValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LogCheckBox
            // 
            this.LogCheckBox.AutoSize = true;
            this.LogCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LogCheckBox.Location = new System.Drawing.Point(680, 43);
            this.LogCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.LogCheckBox.Name = "LogCheckBox";
            this.LogCheckBox.Size = new System.Drawing.Size(112, 20);
            this.LogCheckBox.TabIndex = 15;
            this.LogCheckBox.Text = "Controller Log";
            this.LogCheckBox.UseVisualStyleBackColor = true;
            this.LogCheckBox.CheckedChanged += new System.EventHandler(this.LogCheckBox_CheckedChanged);
            // 
            // ClearButton
            // 
            this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearButton.Location = new System.Drawing.Point(444, 633);
            this.ClearButton.Margin = new System.Windows.Forms.Padding(4);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(109, 28);
            this.ClearButton.TabIndex = 11;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // IgnoreCRCErrorsCheckBox
            // 
            this.IgnoreCRCErrorsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.IgnoreCRCErrorsCheckBox.AutoSize = true;
            this.IgnoreCRCErrorsCheckBox.Location = new System.Drawing.Point(690, 406);
            this.IgnoreCRCErrorsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.IgnoreCRCErrorsCheckBox.Name = "IgnoreCRCErrorsCheckBox";
            this.IgnoreCRCErrorsCheckBox.Size = new System.Drawing.Size(105, 20);
            this.IgnoreCRCErrorsCheckBox.TabIndex = 10;
            this.IgnoreCRCErrorsCheckBox.Text = "Ignore errors";
            this.IgnoreCRCErrorsCheckBox.UseVisualStyleBackColor = true;
            this.IgnoreCRCErrorsCheckBox.CheckedChanged += new System.EventHandler(this.IgnoreCRCErrorsCheckBox_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 679);
            this.Controls.Add(this.LogCheckBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.messagePaceTextBox);
            this.Controls.Add(this.IgnoreCRCErrorsCheckBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.bytePaceTextBox);
            this.Controls.Add(this.CrcOptionsGroupBox);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.LoggingButton);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.NotReadyLabel);
            this.Controls.Add(this.CrcErrorLabel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CrcButton);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.rxCRCLabel);
            this.Controls.Add(this.ResponseCountValue);
            this.Controls.Add(this.BytesReadValue);
            this.Controls.Add(this.CommandCountValue);
            this.Controls.Add(this.rxCrcValue);
            this.Controls.Add(this.txCrcValue);
            this.Controls.Add(this.txCRCLabel);
            this.Controls.Add(this.bytesCheckBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CommandTextBox);
            this.Controls.Add(this.ReceivedTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Serial Port Terminal";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClosingHandler);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.CrcOptionsGroupBox.ResumeLayout(false);
            this.CrcOptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ReceivedTextBox;
        private System.Windows.Forms.TextBox CommandTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ResetButton;
        private Utilities.SerialPortSettingsControl PortConfiguration;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label CrcErrorLabel;
        private System.Windows.Forms.Label NotReadyLabel;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileOpenMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button LoggingButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.GroupBox CrcOptionsGroupBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox InitialValueTextBox;
		private System.Windows.Forms.TextBox PolynomialTextBox;
		private System.Windows.Forms.CheckBox MsByteFirstCheckBox;
		private System.Windows.Forms.TextBox ResidueTextBox;
		private System.Windows.Forms.CheckBox MsBitFirstCheckBox;
		private System.Windows.Forms.TextBox TermCharTextBox;
		private System.Windows.Forms.CheckBox OmitTermCharCheckBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox PostInvertCheckbox;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label TermCharLabel;
		private System.Windows.Forms.Label txCRCLabel;
		private System.Windows.Forms.Label rxCRCLabel;
		private System.Windows.Forms.CheckBox bytesCheckBox;
		private System.Windows.Forms.TextBox bytePaceTextBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox messagePaceTextBox;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button CrcButton;
        private System.Windows.Forms.Label txCrcValue;
        private System.Windows.Forms.Label rxCrcValue;
        private System.Windows.Forms.Label CommandCountValue;
        private System.Windows.Forms.Label ResponseCountValue;
        private System.Windows.Forms.Label BytesReadValue;
        private System.Windows.Forms.CheckBox LogCheckBox;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.CheckBox IgnoreCRCErrorsCheckBox;
    }
}

