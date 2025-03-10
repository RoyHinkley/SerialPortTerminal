using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Utilities;


namespace SerialPortTerminal
{
    public partial class Form1 : Form
    {
        char[] Whitespace = new char[] { ' ', '\r', '\n', '\t' };

        SerialDevice SerialDevice;

        string LogFileName { get { return "SerialDevice"; } }
        LogFile openLog() { return new LogFile(LogFileName); }        
        public LogFile Log
        {
            get
            {
                if (_log == null)
                    Log = openLog();
                return _log;
            }
            set
            {
                _log = value;
            }
        }
        LogFile _log;

        public bool LogEverything
        {
            get { return _logEverything; }
            set
            {
                _logEverything = value;
                if (LogEverything && SerialDevice != null)
                    SerialDevice.Log = Log;
            }
        }
        bool _logEverything = false;

        string programName;
        string dataFileName = "";
        bool logging;
        LogFile dataLog;

        uint CommandCount = 0;
        uint ResponseCount = 0;

        // UI synchronization
        Queue<string> ResponseQ = new Queue<string>();
        Thread updateThread;
        ManualResetEvent updateSignal = new ManualResetEvent(false);

        CommandHistory CommandHistory = new CommandHistory();

        public Form1()
        {
            InitializeComponent();

            CrcOptions crcOptions = new CrcOptions();   // default values
            InitialValueTextBox.Text = crcOptions.InitialValue.ToString("X4");
            PolynomialTextBox.Text = crcOptions.Polynomial.ToString("X4");
            ResidueTextBox.Text = crcOptions.ExpectedResidue.ToString("X4");
            MsBitFirstCheckBox.Checked = crcOptions.MsBitFirst;
            MsByteFirstCheckBox.Checked = crcOptions.MsByteFirst;
            PostInvertCheckbox.Checked = crcOptions.PostInvert;
            OmitTermCharCheckBox.Checked = crcOptions.OmitTermChar;
            TermCharTextBox.Text = crcOptions.TermChar.ToString("X2");
            TermCharLabel.Visible = TermCharTextBox.Visible = !crcOptions.OmitTermChar;

            Thread.CurrentThread.Name = "UIThread";
            programName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            try
            {
                StreamReader fin = new StreamReader(programName + ".ini");
                dataFileName = fin.ReadLine();
                fin.Close();
            }
            catch { dataFileName = programName + ".txt"; }
            dataLog = new LogFile(dataFileName);
            this.Text = PortConfiguration.PortName + " : " + dataFileName;
            
            PortConfiguration.portName.SelectedIndex = 0;

            updateThread = new Thread(updateUI);
            updateThread.IsBackground = true;
            updateThread.Start();
        }


        private void FormClosingHandler(object sender, FormClosingEventArgs e)
        {
            if (SerialDevice != null) SerialDevice.Close();         // free serial port
            if (dataLog != null) dataLog.Close();        // terminate writer thread
        }


        void configureSerialDevice()
        {
            if (SerialDevice != null) SerialDevice.Close();
            SerialDevice = new SerialDevice(PortConfiguration.PortSettings.Clone());
            if (LogCheckBox.Checked)
                SerialDevice.Log = Log;
            SerialDevice.ResponseReceivedHandler = ProcessResponse;

            int i;
            if (int.TryParse(bytePaceTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                SerialDevice.MillisecondsBetweenBytes = i;

            if (int.TryParse(messagePaceTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                SerialDevice.MillisecondsBetweenMessages = i;

            SerialDevice.CrcConfig = GetCrcOptions();
            SerialDevice.IgnoreCRCErrors = IgnoreCRCErrorsCheckBox.Checked;

            SerialDevice.Connect();
            this.Text = SerialDevice.PortSettings.PortName + " : " + dataFileName;
        }


        CrcOptions GetCrcOptions()
        {
            if (!CrcOptionsGroupBox.Visible) return null;

            UInt16 n16; byte b;
            CrcOptions crcOptions = new CrcOptions();

            if (UInt16.TryParse(InitialValueTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out n16))
                crcOptions.InitialValue = n16;

            if (UInt16.TryParse(PolynomialTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out n16))
                crcOptions.Polynomial = n16;

            if (UInt16.TryParse(ResidueTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out n16))
                crcOptions.ExpectedResidue = n16;

            crcOptions.MsBitFirst = MsBitFirstCheckBox.Checked;
            crcOptions.MsByteFirst = MsByteFirstCheckBox.Checked;
            crcOptions.PostInvert = PostInvertCheckbox.Checked;
            crcOptions.OmitTermChar = OmitTermCharCheckBox.Checked;

            if (byte.TryParse(TermCharTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
                crcOptions.TermChar = b;

            return crcOptions;
        }


        bool CrcOptionsMatch(CrcOptions c1, CrcOptions c2)
        {
            if (c1 == null && c2 == null)
                return true;
            if (c1 == null || c2 == null)
                return false;
            return c1.InitialValue == c2.InitialValue &&
                c1.Polynomial == c2.Polynomial &&
                c1.ExpectedResidue == c2.ExpectedResidue &&
                c1.MsBitFirst == c2.MsBitFirst &&
                c1.MsByteFirst == c2.MsByteFirst &&
                c1.PostInvert == c2.PostInvert &&
                c1.PostInvert == c2.PostInvert &&
                c1.OmitTermChar == c2.OmitTermChar &&
                (c1.OmitTermChar || (c1.TermChar == c2.TermChar));
        }


        bool SerialDeviceConfigured()
        {
            return SerialDevice != null &&
                SerialDevice.PortSettings.PortName == PortConfiguration.PortSettings.PortName &&
                SerialDevice.PortSettings.BaudRate == PortConfiguration.PortSettings.BaudRate &&
                SerialDevice.PortSettings.Parity == PortConfiguration.PortSettings.Parity &&
                SerialDevice.PortSettings.DataBits == PortConfiguration.PortSettings.DataBits &&
                SerialDevice.PortSettings.StopBits == PortConfiguration.PortSettings.StopBits &&
                SerialDevice.PortSettings.Handshake == PortConfiguration.PortSettings.Handshake &&

                SerialDevice.MillisecondsBetweenBytes.ToString("0") == bytePaceTextBox.Text &&
                SerialDevice.MillisecondsBetweenMessages.ToString("0") == messagePaceTextBox.Text &&

                CrcOptionsMatch(GetCrcOptions(), SerialDevice.CrcConfig);
        }


        private void TextBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true;
                    break;
                default:
                    break;
            }
        }


        void PresetCommand(string s)
        {
            if (!string.IsNullOrEmpty(s))
                CommandTextBox.Text = s;
            CommandBeforeEditing = CommandTextBox.Text;
            CommandTextBox.SelectionStart = CommandTextBox.Text.Length;
            CommandTextBox.SelectionLength = 0;
        }

        string CommandBeforeEditing = "";
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            string s;
            if (e.KeyCode == Keys.Escape)
            {
                CommandTextBox.Clear();
                CommandBeforeEditing = CommandTextBox.Text;
                CommandHistory.Sync();

                txCrcValue.Text = "";
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (!SerialDeviceConfigured())
                    configureSerialDevice();

                var EnteredCommand = CommandTextBox.Text;

                s = System.Text.RegularExpressions.Regex.Unescape(EnteredCommand);
                if (LogEverything)
                {
                    Log.Record("send text: " + s);
                    Log.Record("send bytes: " + s.ToByteString());
                }
                NotReadyLabel.Visible = !SerialDevice.Command(s);
                if (!NotReadyLabel.Visible) CommandCount++;
                txCrcValue.Text = crcAsText(s);

                if (EnteredCommand != CommandBeforeEditing)
                {
                    CommandHistory.Add(EnteredCommand);
                    CommandTextBox.Clear();
                }
                else
                {
                    s = CommandHistory.Next();
                    if (string.IsNullOrEmpty(s))
                        CommandTextBox.Clear();
                    else
                        CommandTextBox.Text = s;
                }
                CommandBeforeEditing = CommandTextBox.Text;

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                PresetCommand(CommandHistory.Prior());
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                PresetCommand(CommandHistory.Next());
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        string crcAsText(string s)
        {
            if (SerialDevice == null || SerialDevice.CrcConfig == null)
                return "";
            Crc crc = new Crc(SerialDevice.CrcConfig);
            crc.Init();
            return crc.Update(s).ToString("X4");
        }

        private void CrcButton_Click(object sender, EventArgs e)
        {
            if (CrcOptionsGroupBox.Visible)
            {
                CrcOptionsGroupBox.Hide();
                txCrcValue.Hide();
                txCRCLabel.Hide();
                rxCrcValue.Hide();
                rxCRCLabel.Hide();
                CrcButton.Text = "Enable CRC";
            }
            else
            {
                CrcOptionsGroupBox.Show();
                txCrcValue.Show();
                txCRCLabel.Show();
                rxCrcValue.Show();
                rxCRCLabel.Show();
                CrcButton.Text = "Disable CRC";
            }
        }


        private void ResetButton_Click(object sender, EventArgs e)
        {
            configureSerialDevice();
            CommandCount = 0;
            ResponseCount = 0;

            ReceivedTextBox.Clear();
            rxCrcValue.Text = "";

            lock (ResponseQ)
            {
                response = "";
                responseBuffer.Clear();
            }

            CommandTextBox.Focus();
        }

        private void fileOpenMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(dataFileName);
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dataFileName = openFileDialog1.FileName;
                StreamWriter fout = new StreamWriter(programName + ".ini");
                fout.WriteLine(dataFileName);
                fout.Close();
                dataLog = new LogFile(dataFileName);
                this.Text = PortConfiguration.PortName + " : " + dataFileName;
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void LoggingButton_Click(object sender, EventArgs e)
        {
            logging = !logging;
            LoggingButton.Text = logging ? "Stop Logging" : "Start Logging";
            CommandTextBox.Focus();
        }

        void ProcessResponse(string s)
        {
            ResponseCount++;
            lock (ResponseQ) ResponseQ.Enqueue(s);
            if (LogEverything)
            {
                Log.Record("receive bytes: " + s.ToByteString());
                Log.Record("receive text: " + s);
            }
            updateSignal.Set();
        }

        // this runs in a background thread that's ok to be blocked by the UI
        string response = "";
        StringBuilder responseBuffer = new StringBuilder();
        void updateUI()
        {
            while (!ReceivedTextBox.IsHandleCreated)
                Thread.Sleep(20);

            while (true)
            {
                while (ResponseQ.Count > 0)
                {
                    lock (ResponseQ) response = ResponseQ.Dequeue();
                    if (LogEverything)
                    {
                        Log.Record("updateUI: " + response);
                    }
                    responseBuffer.Append(response);
                    var record = Regex.Replace(response.Trim(Whitespace), @"\s+", "\t");
                    if (logging) dataLog.Record(record);
                    

                    IAsyncResult ar = ReceivedTextBox.BeginInvoke((MethodInvoker)delegate
                    {   // this code is executed in UI thread
                        UpdateReceivedTextBox();
                    });
                    ReceivedTextBox.EndInvoke(ar);
                }
                Thread.Sleep(0);
                // wait here until awakened
                updateSignal.WaitOne();       // wait for signal to be set by ProcessResponse()
                updateSignal.Reset();         // reset the signal to the blocking state
            }
        }


        void UpdateReceivedTextBox()
        {
            ReceivedTextBox.AppendText(bytesCheckBox.Checked ? response.ToByteString() : response);
            rxCrcValue.Text = crcAsText(response);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SerialDevice != null)
            {
                CommandCountValue.Text = CommandCount.ToString();
                ResponseCountValue.Text = ResponseCount.ToString();
                BytesReadValue.Text = SerialDevice.TotalBytesRead.ToString();
            }
        }


        void bytePaceTextBox_Leave(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(bytePaceTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                bytePaceTextBox.Text = value.ToString("0");
            else if (SerialDevice != null)
                bytePaceTextBox.Text = SerialDevice.MillisecondsBetweenBytes.ToString("0");
            else
                bytePaceTextBox.Text = new SerialDevice().MillisecondsBetweenBytes.ToString("0");
        }


        void messagePaceTextBox_Leave(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(messagePaceTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                messagePaceTextBox.Text = value.ToString("0");
            else if (SerialDevice != null)
                messagePaceTextBox.Text = SerialDevice.MillisecondsBetweenMessages.ToString("0");
            else
                messagePaceTextBox.Text = new SerialDevice().MillisecondsBetweenMessages.ToString("0");
        }


        void InitialValueTextBox_Leave(object sender, EventArgs e)
        {
            UInt16 value;
            if (UInt16.TryParse(InitialValueTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                InitialValueTextBox.Text = value.ToString("X4");
            else if (SerialDevice != null)
                InitialValueTextBox.Text = SerialDevice.CrcConfig.InitialValue.ToString("X4");
            else
                InitialValueTextBox.Text = new CrcOptions().InitialValue.ToString("X4");
        }

        void PolynomialTextBox_Leave(object sender, EventArgs e)
        {
            UInt16 value;
            if (UInt16.TryParse(PolynomialTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                PolynomialTextBox.Text = value.ToString("X4");
            else if (SerialDevice != null)
                PolynomialTextBox.Text = SerialDevice.CrcConfig.Polynomial.ToString("X4");
            else
                PolynomialTextBox.Text = new CrcOptions().Polynomial.ToString("X4");
        }

        void ResidueTextBox_Leave(object sender, EventArgs e)
        {
            UInt16 value;
            if (UInt16.TryParse(ResidueTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                ResidueTextBox.Text = value.ToString("X4");
            else if (SerialDevice != null)
                ResidueTextBox.Text = SerialDevice.CrcConfig.ExpectedResidue.ToString("X4");
            else
                ResidueTextBox.Text = new CrcOptions().ExpectedResidue.ToString("X4");
        }

        void TermCharTextBox_Leave(object sender, EventArgs e)
        {
            byte value;
            if (byte.TryParse(TermCharTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                TermCharTextBox.Text = value.ToString("X2");
            else if (SerialDevice != null)
                TermCharTextBox.Text = SerialDevice.CrcConfig.TermChar.ToString("X2");
            else
                TermCharTextBox.Text = new CrcOptions().TermChar.ToString("X2");
        }

        private void bytesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ReceivedTextBox.Text = bytesCheckBox.Checked ? responseBuffer.ToString().ToByteString() : stringifyResponse();
            ReceivedTextBox.Select(ReceivedTextBox.SelectionStart, 0); //Is this what I want to do?
                                                                       //UpdateReceivedTextBox();
        }

        private string stringifyResponse()
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in responseBuffer.ToString())
                sb.Append(ASCII[b]);
            return sb.ToString();
        }

        string[] ASCII =
        {
            "«NUL»",
            "«SOH»",
            "«STX»",
            "«ETX»",
            "«EOT»",
            "«ENQ»",
            "«ACK»",
            "«BEL»",
            "«BS»",
            "«HT»",
            "«LF»",
            "«VT»",
            "«FF»",
            "«CR»",
            "«SO»",
            "«SI»",
            "«DLE»",
            "«DC1»",
            "«DC2»",
            "«DC3»",
            "«DC4»",
            "«NAK»",
            "«SYN»",
            "«ETB»",
            "«CAN»",
            "«EM»",
            "«SUB»",
            "«ESC»",
            "«FS»",
            "«GS»",
            "«RS»",
            "«US»",
            " ",
            "!",
            "\"",
            "#",
            "$",
            "%",
            "&",
            "'",
            "(",
            ")",
            "*",
            "+",
            ",",
            "-",
            ".",
            "/",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            ":",
            ";",
            "<",
            "=",
            ">",
            "?",
            "@",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "[",
            "\\",
            "]",
            "^",
            "_",
            "`",
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",
            "{",
            "|",
            "}",
            "~",
            "«DEL»",
            "«80»",
            "«81»",
            "«82»",
            "«83»",
            "«84»",
            "«85»",
            "«86»",
            "«87»",
            "«88»",
            "«89»",
            "«8A»",
            "«8B»",
            "«8C»",
            "«8D»",
            "«8E»",
            "«8F»",
            "«90»",
            "«91»",
            "«92»",
            "«93»",
            "«94»",
            "«95»",
            "«96»",
            "«97»",
            "«98»",
            "«99»",
            "«9A»",
            "«9B»",
            "«9C»",
            "«9D»",
            "«9E»",
            "«9F»",
            "«A0»",
            "«A1»",
            "«A2»",
            "«A3»",
            "«A4»",
            "«A5»",
            "«A6»",
            "«A7»",
            "«A8»",
            "«A9»",
            "«AA»",
            "«AB»",
            "«AC»",
            "«AD»",
            "«AE»",
            "«AF»",
            "«B0»",
            "«B1»",
            "«B2»",
            "«B3»",
            "«B4»",
            "«B5»",
            "«B6»",
            "«B7»",
            "«B8»",
            "«B9»",
            "«BA»",
            "«BB»",
            "«BC»",
            "«BD»",
            "«BE»",
            "«BF»",
            "«C0»",
            "«C1»",
            "«C2»",
            "«C3»",
            "«C4»",
            "«C5»",
            "«C6»",
            "«C7»",
            "«C8»",
            "«C9»",
            "«CA»",
            "«CB»",
            "«CC»",
            "«CD»",
            "«CE»",
            "«CF»",
            "«D0»",
            "«D1»",
            "«D2»",
            "«D3»",
            "«D4»",
            "«D5»",
            "«D6»",
            "«D7»",
            "«D8»",
            "«D9»",
            "«DA»",
            "«DB»",
            "«DC»",
            "«DD»",
            "«DE»",
            "«DF»",
            "«E0»",
            "«E1»",
            "«E2»",
            "«E3»",
            "«E4»",
            "«E5»",
            "«E6»",
            "«E7»",
            "«E8»",
            "«E9»",
            "«EA»",
            "«EB»",
            "«EC»",
            "«ED»",
            "«EE»",
            "«EF»",
            "«F0»",
            "«F1»",
            "«F2»",
            "«F3»",
            "«F4»",
            "«F5»",
            "«F6»",
            "«F7»",
            "«F8»",
            "«F9»",
            "«FA»",
            "«FB»",
            "«FC»",
            "«FD»",
            "«FE»",
            "«FF»"
        };

        private void IgnoreCRCErrorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SerialDevice != null)
                SerialDevice.IgnoreCRCErrors = IgnoreCRCErrorsCheckBox.Checked;
        }

        private void OmitTermCharCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TermCharLabel.Visible = TermCharTextBox.Visible = !OmitTermCharCheckBox.Checked;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            CommandTextBox.Focus();
        }

        private void LogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            LogEverything = LogCheckBox.Checked;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ReceivedTextBox.Clear();
            CommandHistory.Clear();
            CommandTextBox.Focus();
        }
    }


    class CommandHistory
    {
        string[] buf;
        int bufsize;
        int current = 0, peek = 0;

        public string Peek { get { return buf[peek]; } }

        public CommandHistory() : this(20) {}

        public CommandHistory(int size)
        {
            bufsize = size;
            buf = new string[bufsize];
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < bufsize; i++)
                buf[i] = "";
            current = 0;
            peek = 0;
        }

        public void Sync()
        {
            peek = current;
        }

        public void Add(string s)
        {
            if (buf[retreat(current)] != s)
            {
                buf[current] = s;
                current = advance(current);
            }
            Sync();
        }

        public int advance(int n)
        {
            n += 1;
            if (n >= buf.Length)
                n = 0;
            return n;
        }

        public int retreat(int n)
        {
            n -= 1;
            if (n < 0)
                n = buf.Length - 1;
            return n;
        }

        public String Next()
        {
            for (int i = 0; i < buf.Length; i++)
            {
                peek = advance(peek);
                if (!string.IsNullOrEmpty(Peek))
                    return Peek;
            }
            return "";
        }

        public string Prior()
        {
            for (int i = 0; i < buf.Length; i++)
            {
                peek = retreat(peek);
                if (!string.IsNullOrEmpty(Peek))
                    return Peek;
            }
            return "";
        }
    }
}
