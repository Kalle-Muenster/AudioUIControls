using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Consola;
using Consola.Test;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Midi;
using Stepflow.Gui.Geometry;
using System.Windows.Forms;
using System.Drawing;
using MidiGUI.Test.Container;

namespace MidiGUI.Test
{
    public class MidiGUIControls
        : Suite<Container.Form1>
    {
        public MidiGUIControls( Container.Form1 mainwindow, TestResults args )
            : base( mainwindow
                  , args.HasFlag(TestResults.Verbose)
                  , args.HasFlag(TestResults.XmlOutput)
                  ) {
            AddTestCase( "LedButton", Test_LedButton );
            AddTestCase( "GuiMeter", Test_GuiMeter );
            AddTestCase( "GuiSlider", Test_GuiSlider );
        }

        protected override ConTrol.Point GetMenuPoint( string menupath )
        {
            return Aut.GetMenuPosition( menupath ).ConTrolPoint();
        }

        protected override Area GetScreenArea( object descriptor )
        {
            IRectangle rect;
            if( descriptor is Rectangle ) {
                rect = new SystemDefault((Rectangle)descriptor);
            } else if( descriptor is IRectangle ) {
                rect = descriptor as IRectangle;
            } else if( descriptor is string ) {
                string name = (string)descriptor;
                if( name == "Testling" ) return Aut.GetTestlingArea().ConTrolArea();
                else rect = CenterAndScale.FromRectangle(Aut.Controls.Find((string)descriptor, false)[0].Bounds);
            } else if( descriptor is Control ) {
                rect = new SystemDefault(( descriptor as Control ).Bounds);
            } else rect = CenterAndScale.Empty;
            Area area = new Area( rect.W, rect.H );
            return area.At( new ConTrol.Point( Win.Point.X + rect.X, Win.Point.Y + rect.Y ) );
        }

        protected override Area GetWindowArea()
        {
            return Aut.location.ConTrolArea();
        }

        private void SelectControlType( Type controltype )
        {
            ConTrol.Click( ConTrol.Button.L, GetMenuPoint("Controlls") );
            Thread.Sleep( 2000 );
            ConTrol.Point point = GetMenuPoint( "Controlls." + controltype.Name );
            ConTrol.Click( ConTrol.Button.L, point );
            Thread.Sleep( 2000 );
        }


        private void Test_GuiMeter()
        {
            SelectControlType( typeof(GuiMeter) );
            Thread.Sleep( 1000 );
            GuiMeter testling = Aut.GetStagedControl() as GuiMeter;
            CheckStep( testling != null, "{0} instanciated", CurrentCase );
            testling.Damped = false;
            while( testling.Style != Style.Flat ) {
                ConTrol.Click( ConTrol.Button.L, GetScreenArea("btn_set_Style").Center );
                Thread.Sleep( 200 );
            }
            testling.Range = 100.0f;
            Aut.SetControlValue( 0.0f );
            Thread.Sleep( 2000 );
            float level = testling.Level;
            Aut.SetControlValue( 120.0f );
            Thread.Sleep( 2000 );
            level = testling.ClipValue;
            CheckStep( level == 120.0f, "meter level expected ({0}) clip value is: {1}", 120, level );
            level = testling.ClipFactor;
            CheckStep( level == 0.2f, "meter level expected ({0}) clip factor is: {1}", 0.2f, level );
            level = testling.Level;
            CheckStep( level == 100.0f, "meter level ({0}) clamped to maximum: {1}", level, testling.valence().controller().MAX );
            Aut.SetControlValue( -120.0f );
            Thread.Sleep(2000);
            level = testling.Level;
            CheckStep( level == -100.0f, "meter level ({0}) clamped to minimum: {1}", level, testling.valence().controller().MIN );
            Thread.Sleep(1000);

            testling.Unsigned = true;
            Aut.SetControlValue( 0.0f );
            Thread.Sleep( 2000 );
            level = testling.Level;
            Aut.SetControlValue( 120.0f );
            Thread.Sleep( 2000 );
            level = testling.ClipValue;
            CheckStep(level == 120.0f, "meter level expected ({0}) clip value is: {1}", 120, level);
            level = testling.ClipFactor;
            CheckStep(level == 0.2f, "meter level expected ({0}) clip factor is: {1}", 0.2f, level);
            level = testling.Level;
            CheckStep(level == 100.0f, "meter level ({0}) clamped to maximum: {1}", level, testling.valence().controller().MAX);
            Aut.SetControlValue( 0.0f );
            Thread.Sleep( 2000 );
            Aut.SetControlValue( -10.0f );
            Thread.Sleep( 500 );
            level = testling.Level;
            CheckStep( level == 10.0f, "set level to (-10) treated as: {0}", level );
            Thread.Sleep( 1000 );
        }

        private void Test_LedButton()
        {
            SelectControlType( typeof(LedButton) );

            LedButton testling = Aut.GetStagedControl() as LedButton;
            CheckStep( testling != null, "{0} instanciated", CurrentCase );
            Thread.Sleep( 1000 );

            Aut.SetControlValue( LedButton.Default.ON );
            Thread.Sleep( 1000 );
            MatchStep( testling.State.ToString(), "ON", "LedButton.State", "...shows lable as expected" );
            Thread.Sleep( 1000 );

            Point32 point = Aut.GetTestlingArea().Center;
            InfoStep( "Clicking {0} at x:{1},y:{2}", CurrentCase, point.X, point.Y );
            ConTrol.Click( ConTrol.Button.L, point.X, point.Y );
            Thread.Sleep( 1000 );

            MatchStep( testling.Text, "OFF", "LedButton.State", "...state changed as expected" );
            Thread.Sleep( 1000 );
        }

        private int counter = 0;
        private void SliderMarkerPassed( object sender, GuiSlider.MarkerPassedEventArgs e )
        {
            GuiSlider slider = sender as GuiSlider;
            PassStep("Slider {0} (now at value {1}) has passed '{2}' value {3} at a speed of {4} units per move", slider.Name, slider.Value, e.Named, e.Value, e.Speed );
            ++counter;
        }

        private void Test_GuiSlider()
        {
            SelectControlType( typeof(GuiSlider) );
            GuiSlider testling = Aut.GetStagedControl() as GuiSlider;
            CheckStep(testling != null, "{0} instanciated", CurrentCase );
            Thread.Sleep( 1000 );

            while( testling.Orientation != Stepflow.Gui.Orientation.Vertical ) {
                ConTrol.Click( ConTrol.Button.L, GetScreenArea("btn_set_Orientation").Center );
                Thread.Sleep( 200 );
            }

            InfoStep("Set {0} range: 0 to 100", CurrentCase);
            Controlled.Float32 testvalue = testling.valence().controller();
            testvalue.MIN = 0;
            testvalue.MAX = 100;

            Aut.SetControlWidth( 48 );
            Aut.SetControlHeight( 512 );
            InfoStep( "Assigning value 50 to {0}", CurrentCase );
            Aut.SetControlValue( 50.0f );
            Thread.Sleep ( 1000 );
            ConTrol.Point point = GetScreenArea( testling ).Center;
            InfoStep( "Sliding to topmost position" );
            ConTrol.Mouse( ConTrol.Move.Absolute, point );
            ConTrol.Click( ConTrol.Button.L|ConTrol.Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            } ConTrol.Click( ConTrol.Button.L|ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MAX, CurrentCase+".Value", "...value changed to "+testvalue.MAX.ToString() );
            InfoStep( "Sliding down to bottom position" );
            ConTrol.Click( ConTrol.Button.L|ConTrol.Button.DOWN );
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            } ConTrol.Click( ConTrol.Button.L|ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MIN, CurrentCase+".Value", "...value changed to "+ testvalue.MIN.ToString() );

            Thread.Sleep( 100 );
            Aut.SetControlValue( 50.0f );
            InfoStep( "Reset Slider to value 50" );
            InfoStep( "Inverting Slider direction" );
            while( testling.Inverted ) {
                ConTrol.Click(ConTrol.Button.L, GetScreenArea("btn_Invert").Center);
                Thread.Sleep(200);
            }

            point = GetScreenArea( testling ).Center;
            InfoStep( "Sliding to topmost position" );
            ConTrol.Mouse( ConTrol.Move.Absolute, point );
            ConTrol.Click( ConTrol.Button.L | ConTrol.Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click( ConTrol.Button.L | ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MIN, CurrentCase + ".Value", "...value changed to "+ testvalue.MIN.ToString() );
            InfoStep( "Sliding down to bottom position" );
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.DOWN);
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.UP);
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MAX, CurrentCase + ".Value", "...value changed to " + testvalue.MAX.ToString() );

            Aut.SetControlValue( 50.0f );
            InfoStep( "Reset Slider to value 50" );

            ConTrol.Click( ConTrol.Button.L, GetScreenArea("btn_Invert").Center );
            Thread.Sleep( 200 );
            InfoStep( "Reset Slider to (not inverted) regular direction" );

            testling.AddEventMarker(75, "Upper Mark", SliderMarkerPassed );
            testling.AddEventMarker(25, "Lower Mark", SliderMarkerPassed );

            counter = 0;
            InfoStep( "Add two event markers: at value 25 and at value 75");
            InfoStep( "Sliding to topmost position" );
            point = GetScreenArea(testling).Center;
            ConTrol.Mouse( ConTrol.Move.Absolute, point );
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.DOWN);
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.UP);
            Thread.Sleep( 100 );
            InfoStep("Sliding down to bottom position");
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.DOWN);
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep(100);
                ConTrol.Mouse(ConTrol.Move.Absolute, point);
            }
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.UP);
            Thread.Sleep(100);

            MatchStep( counter, 3, "times a marker was passed", "times" );
        }
    }
}
