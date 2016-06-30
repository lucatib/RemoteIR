using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Java.Interop;
using Android.Hardware;
using Java.Lang.Reflect;
using Java.Lang;
using PanasonicCKP;

namespace remoteir
{
    [Activity(Label = "remoteir", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        ConsumerIrManager mCIR;

#if false
        private Java.Lang.Object irService;
        private Method readIR;
        private Method sendIR;
#endif 
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mCIR = (ConsumerIrManager)GetSystemService(Context.ConsumerIrService);

#if false
            irService = GetSystemService("irda");
            readIR = irService.Class.GetMethod("read_irsend", new Class[0]);
            sendIR = irService.Class.GetMethod("write_irsend", Java.Lang.Class.FromType(typeof(Java.Lang.String)));
            sendIR.Invoke(irService, new Java.Lang.Object[] );
#endif 
        }



        private const int carrier_freq = 38000;

        [Export("HandleClick")]
        public void HandleClick(View v)
        {
            // ConsumerIrManager.CarrierFrequencyRange[] mrange =  mCIR.GetCarrierFrequencies();
            int[] marray=null;

            byte tag = Convert.ToByte(((Button)v).Tag.ToString());
            if (tag == 0)
            {
                //POWER ON
                new panasonicCKP().ShortCommand(0xF9, 0x02, ref marray);
            }
            else if (tag == 1)
            {
                //ION
                new panasonicCKP().ShortCommand(0x48, 0x33, ref marray);
            }
            else if (tag == 2)
            {
                //POWERFULL
                new panasonicCKP().ShortCommand(0x86, 0x35, ref marray);  
            }
            else if (tag == 3)
            {
                //QUIET
                new panasonicCKP().ShortCommand(0x81, 0x33, ref marray);
            }
            else if (tag == 4)
            {
                //VSWING Auto
                new panasonicCKP().ShortCommand(0x80, 0x30, ref marray);  
            }

            //Temperature ("TEMP   " + ((int)(bytes[0] & 0x0F) + 15)) 
            //new panasonicCKP().LongCommand(0x00, 0x06|0x08, 0x00 , 0x36, ref marray);   //Auto
            if (marray != null)
            {
                mCIR.Transmit(carrier_freq, marray);
            }
                
#if false
            //
            { //Power on
                Collection<int> mcoll = new Collection<int>();
                marray = new int[cmdpc_on.Length];
                foreach (int it in cmdpc_on)
                {
                    mcoll.Add(it * 1000000 / 38000);
                }
                mcoll.CopyTo(marray, 0);

                mCIR.Transmit(38000, marray);
            }
            
#endif 
        }
    }
}

