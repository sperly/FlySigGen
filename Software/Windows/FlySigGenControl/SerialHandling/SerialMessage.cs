using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlySigGenControl
{
    public class SerialMessage
    {
        public SerialMessageType MessageType;
        public byte[] Payload;
        public int PayloadLength;
    }

    public enum SerialMessageType : byte
    {
        Unknown = 0,
        Data
    }

    public class DataMessage
    {
        private readonly SerialMessage _msg;
        public DataMessage(SerialMessage msg)
        {
            _msg = msg;
        }

        public UInt64 Gen1Freq
        {
            get
            {
                byte[] bytes = { 0 };
                for (int i = 0; i < 8;i++ )
                {
                    bytes[i] = _msg.Payload[i];
                }
                return ToUint64(bytes);
            }
        }

        public UInt64 Gen2Freq
        {
            get
            {
                byte[] bytes = { 0 };
                for (int i = 0; i < 8; i++)
                {
                    bytes[i] = _msg.Payload[i+8];
                }
                return ToUint64(bytes);
            }
        }

        public UInt64 Gen3Freq
        {
            get
            {
                byte[] bytes = { 0 };
                for (int i = 0; i < 8; i++)
                {
                    bytes[i] = _msg.Payload[i + 16];
                }
                return ToUint64(bytes);
            }
        }

        //Takesan eightbyte array and turn it in to a uint64_t 
        private static UInt64 ToUint64(byte[] bytes)
        {
            UInt64 ret = 0;
            for(int i = 0; i < 8;i++)
            {
                ret += (UInt64)(bytes[i] << (i * 8));
            }
            return ret;
        }
    }
}
