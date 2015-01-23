using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace FlySigGenControl
{
    public class SerialHandler : IDisposable
    {
        private readonly SerialPort _serialPort;
        private SerialReader _messageReader;

        public Action<SerialMessage> MessageReceived;

        public SerialHandler(string portName, int baudRate)
        {
            if (!SerialPort.GetPortNames().Contains(portName))
            {
                throw new InvalidOperationException("Attempted to open an unknown COM port: " + portName);
            }

            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.XOnXOff,
                DtrEnable = true,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _messageReader = new SerialReader(_serialPort);
            _messageReader.MessageReceived += _messageReader_MessageReceived;
        }

        /// <summary>
        /// Opens the serial port and starts pumping DataReceived events
        /// </summary>
        public void StartListening()
        {
            if (!_serialPort.IsOpen) _serialPort.Open();
        }

        public void StopListening()
        {
            _serialPort.Close();
        }

        public void Dispose()
        {
            _serialPort.Close();
        }

        #region Event listeners

        private void _messageReader_MessageReceived(SerialMessage message)
        {
            if (MessageReceived != null) MessageReceived(message);
        }

        #endregion

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        internal void Send(byte[] message)
        {
            _serialPort.Write(message, 0, message.Length);
        }
    }
}
