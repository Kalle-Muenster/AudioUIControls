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
    public class Sliders
    {
        private SuiteGUIControls test;
        private int              counter;
        private Form1            Aut;


        public Sliders( SuiteGUIControls suite, Form1 aut )
        {
            counter = 0;
            test = suite;
            Aut = aut;
        }

        private void SliderMarkerPassed( object sender, GuiSlider.MarkerPassedEventArgs e )
        {
            GuiSlider slider = sender as GuiSlider;
            test.PassStep("Slider {0} (now at value {1}) has passed '{2}' value {3} at a speed of {4} units per move", slider.Name, slider.Value, e.Named, e.Value, e.Speed);
            ++counter;
        }

        public void Test_MidiSlider()
        {
            // TODO
        }

        public void Test_MidiTrackBar()
        {
            // TODO
        }

        public void Test_GuiSlider()
        {
            test.SelectControlType( typeof(GuiSlider) );
            GuiSlider testling = Aut.GetStagedControl() as GuiSlider;
            test.CheckStep( testling != null, "{0} instanciated", test.CurrentCase );
            Sleep( 1000 );

            while( testling.Orientation != Stepflow.Gui.Orientation.Vertical ) {
                Click( Button.L, test.OrientationButton.Center );
                Thread.Sleep( 200 );
            }

            test.InfoStep( "Set {0} range: 0 to 100", test.CurrentCase );
            Controlled.Float32 testvalue = testling.valence().controller();
            testvalue.MIN = 0;
            testvalue.MAX = 100;

            Aut.SetControlWidth(48);
            Aut.SetControlHeight(512);
            test.InfoStep( "Assigning value 50 to {0}", test.CurrentCase );
            Aut.SetControlValue(50.0f);
            Sleep(1000);

            IRectangle area = Aut.GetTestlingArea();
            ConTrol.Point point = area.Center.ToPoint();
            test.InfoStep( "Sliding to topmost position" );
            Mouse( Move.Absolute, point );
            Click( Button.L|Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Sleep(25);
                Mouse( ConTrol.Move.Absolute, point );
            }
            Click( Button.L|Button.UP );
            Sleep(100);
            test.MatchStep(testvalue.VAL, testvalue.MAX, test.CurrentCase + ".Value", "...value changed to " + testvalue.MAX.ToString());
            test.InfoStep("Sliding down to bottom position");
            Click( Button.L|Button.DOWN );
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Sleep(25);
                Mouse( Move.Absolute, point );
            }
            Click( Button.L|Button.UP );
            Sleep( 100 );
            test.MatchStep( testvalue.VAL, testvalue.MIN, test.CurrentCase + ".Value", "...value changed to " + testvalue.MIN.ToString());

            Sleep( 100 );
            Aut.SetControlValue( 50.0f );
            test.InfoStep("Reset Slider to value 50");
            test.InfoStep("Inverting Slider direction");
            while( testling.Inverted ) {
                Click( Button.L, test.InvertButton.Center );
                Sleep( 200 );
            }

            point = area.Center.ToPoint();
            test.InfoStep("Sliding to topmost position");
            Mouse( Move.Absolute, point );
            Click( Button.L | Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Sleep( 25 );
                Mouse( Move.Absolute, point );
            }
            Click( Button.L|Button.UP );
            Sleep( 100 );
            test.MatchStep( testvalue.VAL, testvalue.MIN, test.CurrentCase + ".Value", "...value changed to " + testvalue.MIN.ToString());
            test.InfoStep("Sliding down to bottom position");
            Click( Button.L|Button.DOWN );
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Sleep( 25 );
                Mouse( Move.Absolute, point );
            }
            Click( Button.L|Button.UP );
            Sleep( 100 );
            test.MatchStep( testvalue.VAL, testvalue.MAX, test.CurrentCase + ".Value", "...value changed to " + testvalue.MAX.ToString());

            Aut.SetControlValue(50.0f);
            test.InfoStep("Reset Slider to value 50");

            Click( Button.L, test.InvertButton.Center );
            Sleep( 200 );
            test.InfoStep( "Reset Slider to (not inverted) regular direction" );

            testling.AddEventMarker(75, "Upper Mark", SliderMarkerPassed);
            testling.AddEventMarker(25, "Lower Mark", SliderMarkerPassed);

            counter = 0;
            test.InfoStep("Add two event markers: at value 25 and at value 75");
            test.InfoStep("Sliding to topmost position");
            point = area.Center.ToPoint();
            Mouse( Move.Absolute, point );
            Click( Button.L|Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Sleep( 25 );
                Mouse( Move.Absolute, point );
            }
            Click( Button.L|Button.UP );
            Sleep( 100 );
            test.InfoStep("Sliding down to bottom position");
            Click( Button.L|Button.DOWN );
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Sleep(25);
                Mouse( Move.Absolute, point );
            }
            Click( Button.L|Button.UP );
            Sleep( 100 );

            test.MatchStep( counter, 3, "times a marker was passed", "times" );
            Click( Button.L, test.InvertButton.Center );
        }
    }
}
