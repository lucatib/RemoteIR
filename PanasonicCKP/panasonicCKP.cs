using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using System.Diagnostics;
using System.Collections.ObjectModel;

namespace remoteir
{
    public class panasonicCKP
    {
        #region RX
        private const int PANASONIC_AIRCON1_HDR_MARK = 3400;
        private const int PANASONIC_AIRCON1_HDR_SPACE = 3500;
        private const int PANASONIC_AIRCON1_BIT_MARK = 850;
        private const int PANASONIC_AIRCON1_ONE_SPACE = 2700;
        private const int PANASONIC_AIRCON1_ZERO_SPACE = 950;
        private const int PANASONIC_AIRCON1_MSG_SPACE = 14000;

        private const int PANASONIC_AIRCON1_SHORT_MSG = 202;
        private const int PANASONIC_AIRCON1_LONG_MSG = 272;
        private const int PANASONIC_AIRCON1_TIMER_MSG = 420;   


        // Check for repeated patterns in the byte array
        static Boolean CheckRepeated(byte[] bytes, int startingPosition, int offset, int repeats)
        {
            for (int i = 0; i < repeats; i++)
            {
                for (int j = startingPosition; j < offset; j++)
                {
                    if (bytes[j] != bytes[j + i * offset])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Check for repeated pairs in the byte array
        static Boolean CheckRepeatedPairs(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i += 2)
            {
                if (bytes[i] != bytes[i + 1])
                {
                    return false;
                }
            }

            return true;
        }

        private static void PrintPanasonicCKP(byte[] bytes)
        {
            // Short message is about setting
            // * ION
            // * QUIET
            // * POWERFUL
            // * SWINGS

            if ((bytes.Length == 12) && 
                CheckRepeated(bytes, 0, 4, 3) &&
                CheckRepeatedPairs(bytes))
            {
                if ((bytes[0] == 0x48) && (bytes[2] == 0x33))
                {
                    System.Diagnostics.Debug.Print("ION ON/OFF");
                }
                else if ((bytes[0] == 0x81) && (bytes[2] == 0x33))
                {
                    System.Diagnostics.Debug.Print("QUIET ON/OFF");
                }
                else if ((bytes[0] == 0x86) && (bytes[2] == 0x35))
                {
                    System.Diagnostics.Debug.Print("POWERFUL ON/OFF");
                }
                else if ((bytes[0] == 0x80) && (bytes[2] == 0x30))
                {
                    System.Diagnostics.Debug.Print("VSWING AUTO");
                }
                else if (((bytes[0] & 0XF0) == 0xA0) && (bytes[2] == 0x32))
                {
                    switch (bytes[0] & 0X0F)
                    {
                        case 0x01:
                            System.Diagnostics.Debug.Print("VSWING FULL UP");
                            break;
                        case 0x02:
                            System.Diagnostics.Debug.Print("VSWING MIDDLE UP");
                            break;
                        case 0x03:
                            System.Diagnostics.Debug.Print("VSWING MIDDLE");
                            break;
                        case 0x04:
                            System.Diagnostics.Debug.Print("VSWING MIDDLE DOWN");
                            break;
                        case 0x05:
                            System.Diagnostics.Debug.Print("VSWING DOWN");
                            break;
                        case 0x08:
                            System.Diagnostics.Debug.Print("HSWING AUTO");
                            break;
                        case 0x09:
                            System.Diagnostics.Debug.Print("HSWING MIDDLE");
                            break;
                        case 0x0C:
                            System.Diagnostics.Debug.Print("HSWING LEFT");
                            break;
                        case 0x0D:
                            System.Diagnostics.Debug.Print("HSWING MIDDLE LEFT");
                            break;
                        case 0x0E:
                            System.Diagnostics.Debug.Print("HSWING MIDDLE RIGHT");
                            break;
                        case 0x0F:
                            System.Diagnostics.Debug.Print("HSWING RIGHT");
                            break;
                        default:
                            System.Diagnostics.Debug.Print("Unknown SWING");
                            break;
                    }
                }
            }

            // Timer message
            else if ((bytes.Length == 24) &&
                      CheckRepeatedPairs(bytes) &&
                      bytes[22] == 0x34)
            {
                if (bytes[0] == 0x7F && bytes[8] == 0x7F)
                {
                    System.Diagnostics.Debug.Print("Timer CANCEL message");
                }
                else
                {
                    System.Diagnostics.Debug.Print("Timer SET message");

                    if (bytes[8] == 0x7F)
                    {
                        System.Diagnostics.Debug.Print("ON: <not set>");
                    }
                    else
                    {
                        System.Diagnostics.Debug.Print("ON:  " + (bytes[12] - 0x80).ToString("X2") + ":" + bytes[8].ToString("X2"));
                    }

                    if (bytes[0] == 0x7F)
                    {
                        System.Diagnostics.Debug.Print("OFF: <not set>");
                    }
                    else
                    {
                        System.Diagnostics.Debug.Print("OFF: " + (bytes[4] - 0x80).ToString("X2") + ":" + bytes[0].ToString("X2"));
                    }
                }

                System.Diagnostics.Debug.Print("NOW: " + (bytes[20] - 0x80).ToString("X2") + ":" + bytes[16].ToString("X2"));
            }

            // Everything else is a long message
            else if ((bytes.Length == 16) &&
                    CheckRepeated(bytes, 0, 4, 2) &&
                    CheckRepeated(bytes, 8, 4, 2) &&
                    CheckRepeatedPairs(bytes) &&
                    (bytes[10] == 0x36))
            {
                String Mode = "Unknown";
                String Fan = "AUTO";
                String SwingV;
                String SwingH;

                // Operation mode, low bits of byte 3

                switch (bytes[2] & 0x07)
                {
                    case 0x06:
                        Mode = "AUTO";
                        break;
                    case 0x04:
                        Mode = "HEAT";
                        break;
                    case 0x02:
                        Mode = "COOL";
                        break;
                    case 0x03:
                        Mode = "DRY";
                        break;
                    case 0x01:
                        Mode = "FAN";
                        break;
                }

                if ((bytes[2] & 0x08) != 0x08)
                {
                    Mode += " ON/OFF";
                }

                // Fan speed, high bits of byte 1

                int fan = bytes[0] & 0xF0;
                if (fan != 0xF0)
                {
                    Fan = ((fan >> 4) - 1).ToString();
                }

                // Vertical swing, high bits of byte 9

                switch (bytes[8] & 0XF0)
                {
                    case 0xF0:
                        SwingV = "AUTO";
                        break;
                    case 0xD0:
                        SwingV = "FULL DOWN";
                        break;
                    case 0xC0:
                        SwingV = "MIDDLE DOWN";
                        break;
                    case 0xB0:
                        SwingV = "MIDDLE";
                        break;
                    case 0xA0:
                        SwingV = "MIDDLE UP";
                        break;
                    case 0x90:
                        SwingV = "FULL UP";
                        break;
                    default:
                        SwingV = "Unknown";
                        break;
                }

                // Horizontal swing, low bits of byte 9

                switch (bytes[8] & 0x0F)
                {
                    case 0x08:
                        SwingH = "AUTO";
                        break;
                    case 0x00:
                        SwingH = "MANUAL";
                        break;
                    default:
                        SwingH = "Unknown";
                        break;
                }

                // Print the state

                System.Diagnostics.Debug.Print("MODE   " + Mode);
                System.Diagnostics.Debug.Print("FAN    " + Fan);
                if (Mode != "FAN")
                {
                    System.Diagnostics.Debug.Print("TEMP   " + ((int)(bytes[0] & 0x0F) + 15));
                }
                System.Diagnostics.Debug.Print("SWINGV " + SwingV);
                System.Diagnostics.Debug.Print("SWINGH " + SwingH);
            }
            else
            {
                System.Diagnostics.Debug.Print("protocol error");
            }
        }

        #endregion

        #region TX
        //Power on dump (38k timing units)
        int[] cmdpc_on = new int[]{
            136,126,                                            //Head
            40,90,40,26,40,26,40,90,40,90,40,90,40,90,40,90,    //Byte1 lsb->msb 11111001  
            40,90,40,26,40,26,40,90,40,90,40,90,40,90,40,90,    //Byte1
            40,26,40,90,40,26,40,26,40,26,40,26,40,26,40,26,    //Byte2 lsb->msb 00000010
            40,26,40,90,40,26,40,26,40,26,40,26,40,26,40,26,    //Byte2
            136,126,
            40,90,40,26,40,26,40,90,40,90,40,90,40,90,40,90,
            40,90,40,26,40,26,40,90,40,90,40,90,40,90,40,90,
            40,26,40,90,40,26,40,26,40,26,40,26,40,26,40,26,
            40,26,40,90,40,26,40,26,40,26,40,26,40,26,40,26,
            136,126,
            40,90,40,26,40,26,40,90,40,90,40,90,40,90,40,90,
            40,90,40,26,40,26,40,90,40,90,40,90,40,90,40,90,
            40,26,40,90,40,26,40,26,40,26,40,26,40,26,40,26,
            40,26,40,90,40,26,40,26,40,26,40,26,40,26,40,26,
            136,4544                                            //Stop   
        };
        
        private const int carrier_freq = 38000;
        
        //usec units
        private const int usec_tonstart = 3400; //136 * (1000000 / carrier_freq); 
        private const int usec_toffstart = 3500; //126 * (1000000 / carrier_freq);

        private const int usec_tonend = 3400; //136 * (1000000 / carrier_freq);
        private const int usec_toffend = 14000; //4544 * (1000000 / carrier_freq);

        private const int usec_tonbit = 850;//40 * (1000000 / carrier_freq);
        private const int usec_toffzero = 950;//26 * (1000000 / carrier_freq);
        private const int usec_toffone = 2700;//90 * (1000000 / carrier_freq);

        private void AppendFoooter(ref Collection<int> mcommand)
        {
            mcommand.Add(usec_tonend);
            mcommand.Add(usec_toffend);
        }

        private void AppendHeader(ref Collection<int> mcommand)
        {
            mcommand.Add(usec_tonstart);
            mcommand.Add(usec_toffstart);
        }

        private void AppendByte(byte inbyte, ref Collection<int> mcommand)
        {
            byte mask = 0x80;
            for (int i = 0; i < 8; i++)
            {
                mask = (byte)(0x01 << i);  //lsb first
                mcommand.Add(usec_tonbit);
                if ((inbyte & mask) > 0)
                {
                    mcommand.Add(usec_toffone);
                }
                else
                {
                    mcommand.Add(usec_toffzero);
                }
                mask >>= 1;
            }
        }

        //b1b1b2b2 b1b1b2b2 b3b3b4b4 b3b3b4b4
        private void LongCommand(byte b1, byte b2, byte b3, byte b4, ref Collection<int> mcoll) {
            int i;

            for (i = 0; i < 2; i++)
            {
                AppendHeader(ref mcoll);

                AppendByte(b1, ref mcoll);
                AppendByte(b1, ref mcoll);
                AppendByte(b2, ref mcoll);
                AppendByte(b2, ref mcoll);
            }

            for (i = 0; i < 2; i++)
            {
                AppendHeader(ref mcoll);

                AppendByte(b3, ref mcoll);
                AppendByte(b3, ref mcoll);
                AppendByte(b4, ref mcoll);
                AppendByte(b4, ref mcoll);
            }

            AppendFoooter(ref mcoll);

        }

        //b1b1b2b2 b1b1b2b2
        private void ShortCommand(byte b1,byte b2, ref Collection<int> mcoll) {
            for (int i = 0; i < 3; i++)
            {
                AppendHeader(ref mcoll);

                AppendByte(b1, ref mcoll);
                AppendByte(b1, ref mcoll);
                AppendByte(b2, ref mcoll);
                AppendByte(b2, ref mcoll);

            }

            AppendFoooter(ref mcoll);
        }

        public bool LongCommand(byte b1, byte b2, byte b3, byte b4, ref int[] marray)
        {
            try
            {
                Collection<int> mcoll = new Collection<int>();
                LongCommand(b1, b2, b3, b4, ref mcoll);
                marray = mcoll.ToArray<int>();
                return true;
            }
            catch (Exception)
            {
                return false;                
            }
        }

        public bool ShortCommand(byte b1, byte b2, ref int[] marray)
        {
            try
            {
                Collection<int> mcoll = new Collection<int>();
                ShortCommand(b1, b2, ref mcoll);
                marray = mcoll.ToArray<int>();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}