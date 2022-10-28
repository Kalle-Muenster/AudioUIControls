using System;
using System.Threading;
using System.Threading.Tasks;
using Consola.Test;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Midi;
using Stepflow.Gui.Automation;
using Stepflow.Gui.Geometry;
using System.Windows.Forms;
using System.Drawing;
using static Consola.Test.ConTrol;
using static System.Threading.Thread;
using Button = Consola.Test.ConTrol.Button;
using Win32Imports.Midi;


namespace MidiGUI.Test.Container
{
    public class Meters
    {
        private SuiteGUIControls test;
        private Form1            Aut;
        private float            ExpectedValue;

        public Meters( SuiteGUIControls suite, Form1 aut )
        {
            test = suite;
            Aut = aut;
        }

        public void Test_GuiMeter()
        {
            test.SelectControlType(typeof(GuiMeter));
            Thread.Sleep(1000);
            GuiMeter testling = Aut.GetStagedControl() as GuiMeter;
            test.CheckStep(testling != null, "{0} instanciated", test.CurrentCase);
            testling.Damped = false;
            while( testling.Style != Style.Flat ) {
                Click( Button.L, test.StyleButton.Center );
                Thread.Sleep(200);
            }
            testling.Range = 100.0f;
            Aut.SetControlValue(0.0f);
            Thread.Sleep(2000);
            float level = testling.Level;
            Aut.SetControlValue(120.0f);
            Thread.Sleep(2000);
            level = testling.ClipValue;
            test.CheckStep(level == 120.0f, "meter level expected ({0}) clip value is: {1}", 120, level);
            level = testling.ClipFactor;
            test.CheckStep(level == 0.2f, "meter level expected ({0}) clip factor is: {1}", 0.2f, level);
            level = testling.Level;
            test.CheckStep(level == 100.0f, "meter level ({0}) clamped to maximum: {1}", level, testling.valence().controller().MAX);
            Aut.SetControlValue(-120.0f);
            Thread.Sleep(2000);
            level = testling.Level;
            test.CheckStep(level == -100.0f, "meter level ({0}) clamped to minimum: {1}", level, testling.valence().controller().MIN);
            Thread.Sleep(1000);

            testling.Unsigned = true;
            Aut.SetControlValue(0.0f);
            Thread.Sleep(2000);
            level = testling.Level;
            Aut.SetControlValue(120.0f);
            Thread.Sleep(2000);
            level = testling.ClipValue;
            test.CheckStep(level == 120.0f, "meter level expected ({0}) clip value is: {1}", 120, level);
            level = testling.ClipFactor;
            test.CheckStep(level == 0.2f, "meter level expected ({0}) clip factor is: {1}", 0.2f, level);
            level = testling.Level;
            test.CheckStep(level == 100.0f, "meter level ({0}) clamped to maximum: {1}", level, testling.valence().controller().MAX);
            Aut.SetControlValue(0.0f);
            Thread.Sleep(2000);
            Aut.SetControlValue(-10.0f);
            Thread.Sleep(500);
            level = testling.Level;
            test.CheckStep(level == 10.0f, "set level to (-10) treated as: {0}", level);
            Thread.Sleep(1000);
        }




        public void Test_MidiMeter()
        {
            test.SelectAndBindMidiReceivingControl<MidiMeter>(1,1);
            MidiMeter dings = Aut.GetStagedControl() as MidiMeter;
            dings.LevelChanged += Dings_LevelChanged;

            ExpectedValue = dings.Range / 2.0f;
            test.midi().SendControlChange(64);

            // TODO verify that the meter chart shows up center-value / half-range
            Sleep(200);

            test.midi().SendControlChange(0);

            Sleep(200);

            test.midi().SendControlChange(127);

            Sleep(200);

            /////////
            // TODO sending more different midi messages and veify the control receives them
        }

        private void Dings_LevelChanged( object sender, ValueChangeArgs<float> value )
        {
            test.CheckStep( value == ExpectedValue, "Meter level changed to {0}", value.Value );
        }
    }
}
