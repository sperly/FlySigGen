using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace FlySigGenControl
{
    public class SerialReader
    {
        public event Action<SerialMessage> MessageReceived;

        public SerialReader(System.IO.Ports.SerialPort serialPort)
        {
            serialPort.DataReceived += serialPort_DataReceived;
        }

        #region constants

        private const int MaxPacketSize = 256;

        // Magic byte -> 1, 3, 3, 7
        private const byte FirstMagicByte = 031;
        private const byte SecondMagicByte = 0x33;
        private const byte ThirdMagicByte = 0x33;
        private const byte FourthMagicByte = 0x37;

        #endregion

        #region fields and properties


        private byte[] _buffer;
        private int _bufferIndex;
        private SerialMessageType _messageType;
        private ReadState _readState;
        private byte _dataLength;

        private bool MessageComplete
        {
            get
            {
                return _dataLength > 0 && _bufferIndex >= _dataLength;
            }
        }

        #endregion

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            int bytesToRead = serialPort.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            int bytesRead = serialPort.Read(buffer, 0, bytesToRead);

            for (int i = 0; i < bytesRead; ++i)
            {
                AddByte(buffer[i]);
            }

            // Can't use the SerialData.Eof flag since the binary data transmitted could easily contain the EOF byte.
        }

        private void AddByte(byte b)
        {
            switch (_readState)
            {
                case ReadState.WaitingForFirstMagicByte:
                    if (FirstMagicByte == b) _readState = ReadState.WaitingForSecondMagicByte;
                    break;
                case ReadState.WaitingForSecondMagicByte:
                    if (SecondMagicByte == b) _readState = ReadState.WaitingForThirdMagicByte;
                    else _readState = ReadState.WaitingForFirstMagicByte;
                    break;
                case ReadState.WaitingForThirdMagicByte:
                    if (ThirdMagicByte == b) _readState = ReadState.WaitingForFourthMagicByte;
                    else _readState = ReadState.WaitingForFirstMagicByte;
                    break;
                case ReadState.WaitingForFourthMagicByte:
                    if (FourthMagicByte == b) _readState = ReadState.WaitingForCommandTypeByte;
                    else _readState = ReadState.WaitingForFirstMagicByte;
                    break;
                case ReadState.WaitingForCommandTypeByte:
                    _messageType = (SerialMessageType)b;
                    _readState = ReadState.WaitingForDataLengthByte;
                    break;
                case ReadState.WaitingForDataLengthByte:
                    _dataLength = b;
                    //_dataLength -= 14;
                    _bufferIndex = 0;
                    _buffer = new byte[_dataLength];
                    _readState = ReadState.ReadingData;
                    break;
                case ReadState.ReadingData:
                    _buffer[_bufferIndex] = b;
                    ++_bufferIndex;
                    if (MessageComplete)
                    {
                        OnMessageComplete();
                        _readState = ReadState.WaitingForFirstMagicByte;
                    }
                    break;
            }
        }

        private void OnMessageComplete()
        {
            if (MessageReceived == null) return;

            var message = new SerialMessage
            {
                MessageType = _messageType,
                Payload = _buffer,
                PayloadLength = _dataLength
            };

            MessageReceived(message);
        }
    }

    public enum ReadState
    {
        WaitingForFirstMagicByte,
        WaitingForSecondMagicByte,
        WaitingForThirdMagicByte,
        WaitingForFourthMagicByte,
        WaitingForCommandTypeByte,
        WaitingForDataLengthByte,
        ReadingData
    }
}
