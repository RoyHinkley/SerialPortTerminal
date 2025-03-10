using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO.Ports;

namespace Utilities
{
	public partial class SerialPortSettingsControl : UserControl
	{
		public SerialPortSettings PortSettings;

		#region Properties
		[Category("Port Settings"), TypeConverter(typeof(PortNameTypeConverter))]
		public string PortName
		{ 
			get { return PortSettings.PortName; }
			set { PortSettings.PortName = value; portName.SelectedItem = PortSettings.PortName; } 
		}

		[Category("Port Settings"), TypeConverter(typeof(BaudRateTypeConverter))]
		public int BaudRate 
		{ 
			get { return PortSettings.BaudRate; }
			set { PortSettings.BaudRate = value; baudRate.SelectedItem = PortSettings.BaudRate; } 
		}

		[Category("Port Settings")]
		public Parity Parity
		{
			get { return PortSettings.Parity; }
			set { PortSettings.Parity = value; parity.SelectedItem = PortSettings.Parity; } 
		}

		[Category("Port Settings"), TypeConverter(typeof(DataBitsTypeConverter))]
		public int DataBits 
		{
			get { return PortSettings.DataBits; }
			set { PortSettings.DataBits = value; dataBits.SelectedItem = PortSettings.DataBits; } 
	}

		[Category("Port Settings")]
		public StopBits StopBits
		{
			get { return PortSettings.StopBits; }
			set { PortSettings.StopBits = value; stopBits.SelectedItem = PortSettings.StopBits; } 
		}

		[Category("Port Settings")]
		public Handshake Handshake
		{ 
			get { return PortSettings.Handshake; }
			set { PortSettings.Handshake = value; handshake.SelectedItem = PortSettings.Handshake; } 
		}
		#endregion

		public SerialPortSettingsControl()
		{
			InitializeComponent();

			portName.DataSource = SerialPortSettings.PortNameValues;
			baudRate.DataSource = SerialPortSettings.BaudRateValues;
			parity.DataSource = SerialPortSettings.ParityValues;
			dataBits.DataSource = SerialPortSettings.DataBitsValues;
			stopBits.DataSource = SerialPortSettings.StopBitValues;
			handshake.DataSource = SerialPortSettings.HandShakeValues;

			PortSettings = new SerialPortSettings();
			PortName = PortSettings.PortName;
			BaudRate = PortSettings.BaudRate;
			Parity = PortSettings.Parity;
			DataBits = PortSettings.DataBits;
			StopBits = PortSettings.StopBits;
			Handshake = PortSettings.Handshake;
		}

		private void portNameChanged(object sender, EventArgs e)
		{ if (PortSettings != null) PortSettings.PortName = (string) portName.SelectedItem; }

		private void baudRateChanged(object sender, EventArgs e)
		{ if (PortSettings != null) PortSettings.BaudRate = (int) baudRate.SelectedItem; }

		private void parityChanged(object sender, EventArgs e)
		{ if (PortSettings != null) PortSettings.Parity = (Parity) parity.SelectedItem; }

		private void dataBitsChanged(object sender, EventArgs e)
		{ if (PortSettings != null) PortSettings.DataBits = (int) dataBits.SelectedItem; }

		private void stopBitsChanged(object sender, EventArgs e)
		{ if (PortSettings != null) PortSettings.StopBits = (StopBits) stopBits.SelectedItem; }

		private void handshakeChanged(object sender, EventArgs e)
		{ if (PortSettings != null) PortSettings.Handshake = (Handshake) handshake.SelectedItem; }

        private void portName_Enter(object sender, EventArgs e)
        {
            portName.DataSource = SerialPortSettings.PortNameValues;
        }
    }


    #region Type Converters

    public class PortNameTypeConverter : TypeConverter
	{
		public PortNameTypeConverter() { }

		// Indicates that this converter provides a list of standard values.
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{ return true; }

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
		{ return new StandardValuesCollection(SerialPortSettings.PortNameValues); }

		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
		{ return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType); }

		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string) return value;
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string)) return value.ToString();
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}


	public class BaudRateTypeConverter : TypeConverter
	{
		public BaudRateTypeConverter() { }

		// Indicates that this converter provides a list of standard values.
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{ return true; }

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
		{ return new StandardValuesCollection(SerialPortSettings.BaudRateValues); }

		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
		{ return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType); }

		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string) return int.Parse((string)value);
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string)) return ((int)value).ToString();
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	public class DataBitsTypeConverter : TypeConverter
	{
		public DataBitsTypeConverter() { }

		// Indicates that this converter provides a list of standard values.
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{ return true; }

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
		{ return new StandardValuesCollection(SerialPortSettings.DataBitsValues); }

		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
		{ return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType); }

		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string) return int.Parse((string)value);
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string)) return ((int)value).ToString();
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	#endregion
}
