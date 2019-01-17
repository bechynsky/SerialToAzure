using Microsoft.Azure.Devices.Client;
using System;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Configuration;

namespace SerialToAzure
{
    class Program
    {
        private static SerialPort _serial = new SerialPort();
        // Object for device to cloud communication D2C
        private static DeviceClient _deviceClient = null;
        // D2C message object
        private static Message _eventMessage = null;

        static void Main(string[] args)
        {
            // We use it because of decimal point separator
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            // Get connection string from config file
            String deviceConnectionString = ConfigurationManager.ConnectionStrings["iothub"].ToString();
            // New instance of DeviceClient from connection string
            _deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Http1);

            // Serial port configuration
            String port = ConfigurationManager.AppSettings["port"].ToString();
            int baudrate = int.Parse(ConfigurationManager.AppSettings["baudrate"].ToString());

            _serial.PortName = port;
            _serial.BaudRate = baudrate;
            _serial.DataReceived += _serial_DataReceived;

            _serial.Open();

            while (true) { };
        }

        private static async void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Proces data from device
            String[] message = _serial.ReadLine().Trim().Split(new char[] {';'});
            String payload = "{'t':" + message[0] + ",'h':" + message[1] + "}";

            // Prepare message
            _eventMessage = new Message(Encoding.UTF8.GetBytes(payload));
            // Send message
            await _deviceClient.SendEventAsync(_eventMessage);

            // Control output
            Console.WriteLine(payload);
        }
    }
}
