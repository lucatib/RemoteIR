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
using System.Globalization;
using Android.Preferences;

namespace remoteir
{
    [Activity(Label = "remoteir", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const int carrier_freq = 38000;

        ConsumerIrManager mCIR;
        ISharedPreferences prefs;

        int glbl_temperature;
        byte glbl_mode;

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
            prefs = PreferenceManager.GetDefaultSharedPreferences(this.ApplicationContext);
            
            glbl_temperature = prefs.GetInt("TEMPERATURE", 20);
            glbl_mode = (byte)prefs.GetInt("MODE", 0x06);

            FindViewById<TextView>(Resource.Id.editTemp).SetText(FormatTemp(glbl_temperature), TextView.BufferType.Normal);
            //....
#if false
            irService = GetSystemService("irda");
            readIR = irService.Class.GetMethod("read_irsend", new Class[0]);
            sendIR = irService.Class.GetMethod("write_irsend", Java.Lang.Class.FromType(typeof(Java.Lang.String)));
            sendIR.Invoke(irService, new Java.Lang.Object[] );
#endif
        }

        protected override void OnDestroy()
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutInt("TEMPERATURE", glbl_temperature);
            editor.PutInt("MODE", glbl_mode);
            //....
            editor.Apply();
            base.OnDestroy();
        }

        [Export("HandleClick")]
        public void HandleClick(View v)
        {
            // ConsumerIrManager.CarrierFrequencyRange[] mrange =  mCIR.GetCarrierFrequencies();
            int[] marray = null;

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

            if (marray != null)
            {
                mCIR.Transmit(carrier_freq, marray);
            }
        }

        #region TEMP
        private string FormatTemp(int mvalue) {
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}°C", mvalue) ;
        }

        [Export("TUp")]
        public void TUp(View v)
        {
            int[] marray = null;
            if (glbl_temperature < 30)
            {
                glbl_temperature++;
                FindViewById<TextView>(Resource.Id.editTemp).SetText(FormatTemp(glbl_temperature), TextView.BufferType.Normal);
                new panasonicCKP().LongCommand((byte)(15 - glbl_temperature), (byte)(glbl_mode | 0x08), 0x00, 0x36, ref marray);   //Auto
                mCIR.Transmit(carrier_freq, marray);
            }
        }
        [Export("TDown")]
        public void TDown(View v)
        {
            int[] marray = null;
            if (glbl_temperature > 15)
            {
                glbl_temperature--; 
                FindViewById<TextView>(Resource.Id.editTemp).SetText(FormatTemp(glbl_temperature), TextView.BufferType.Normal);
                new panasonicCKP().LongCommand((byte)(15 - glbl_temperature), (byte)(glbl_mode | 0x08), 0x00, 0x36, ref marray);
                mCIR.Transmit(carrier_freq, marray);
            }
        }
        #endregion
    }
}
