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
using static Consola.Test.ConTrol;
using static System.Threading.Thread;
using Button = Consola.Test.ConTrol.Button;

namespace MidiGUI.Test
{
    public static class Extensions
    {
        public static ConTrol.Point ToPoint( this Point32 cast )
        {
            return new ConTrol.Point( cast.X, cast.Y );
        }

        public static Point32 ToPoint32( this ConTrol.Point cast )
        {
            return new Point32( cast.X, cast.Y );
        }

    }


    public class MidiGUIControls
        : Suite<Form1>
    {
        public MidiGUIControls( Form1 mainwindow, TestResults args )
            : this( mainwindow, args, string.Empty )
        {}

        public MidiGUIControls( Form1 mainwindow, TestResults args, string testcase )
            : base( mainwindow
                  , args.HasFlag(TestResults.Verbose)
                  , args.HasFlag(TestResults.XmlOutput)
                  ) {
            switch( testcase ) {
                case "LedButton": AddTestCase( testcase, Test_LedButton ); break;
                case "GuiMeter":  AddTestCase( testcase, Test_GuiMeter ); break;
                case "GuiSlider": AddTestCase( testcase, Test_GuiSlider ); break;
                case "JogDial":   AddTestCase( testcase, Test_JogDial ); break;
                default:
                    AddTestCase( "LedButton", Test_LedButton );
                    AddTestCase( "GuiMeter", Test_GuiMeter );
                    AddTestCase( "GuiSlider", Test_GuiSlider );
                    AddTestCase( "JogDial", Test_JogDial );
                break;
            }
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
            Click( Button.L, GetMenuPoint("Controlls") );
            Sleep( 2000 );
            ConTrol.Point point = GetMenuPoint( "Controlls." + controltype.Name );
            Click( Button.L, point );
            Sleep( 2000 );
        }


        private void Test_GuiMeter()
        {
            SelectControlType( typeof(GuiMeter) );
            Thread.Sleep( 1000 );
            GuiMeter testling = Aut.GetStagedControl() as GuiMeter;
            CheckStep( testling != null, "{0} instanciated", CurrentCase );
            testling.Damped = false;
            while( testling.Style != Style.Flat ) {
                Click( Button.L, GetScreenArea("btn_set_Style").Center );
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
            CheckStep(level == 120.0f, "meter level expected ({0}) clip value is: {1}", 120, level );
            level = testling.ClipFactor;
            CheckStep(level == 0.2f, "meter level expected ({0}) clip factor is: {1}", 0.2f, level );
            level = testling.Level;
            CheckStep(level == 100.0f, "meter level ({0}) clamped to maximum: {1}", level, testling.valence().controller().MAX );
            Aut.SetControlValue( 0.0f );
            Thread.Sleep( 2000 );
            Aut.SetControlValue( -10.0f );
            Thread.Sleep( 500 );
            level = testling.Level;
            CheckStep( level == 10.0f, "set level to (-10) treated as: {0}", level );
            Thread.Sleep( 1000 );
        }


        private bool touched = false;
        private bool reversed = false;
        private float movement = 0.0f;
        private JogDial.Direction direction = (JogDial.Direction)0;
        private float angel = 0.0f;
        private float accelleration = 0.0f;
        private bool fast = false;
        private int stops = 0;

        private void Test_JogDial()
        {
            SelectControlType( typeof(JogDial) );

            JogDial jogdial = Aut.GetStagedControl() as JogDial;
            CheckStep( jogdial != null, "{0} instanciated", CurrentCase );
            Sleep( 1000 );

            jogdial.WheelTouched += Jogdial_WheelTouched;
            jogdial.WheelReverse += Jogdial_WheelReverse;
            jogdial.TurningStopt += Jogdial_TurningStopt;
            jogdial.WheelRelease += Jogdial_WheelRelease;
            jogdial.MovementFast += Jogdial_MovementFast;

            Click( Button.L, GetScreenArea("btn_set_Led").Center );
            Sleep( 1000 );
            Click( Button.L, GetScreenArea("btn_set_Led").Center );
            Sleep( 1000 );
            Click( Button.L, GetScreenArea("btn_set_Led").Center );
            Sleep( 1000 );
            Click( Button.L, GetScreenArea("btn_set_Led").Center );
            Sleep( 1000 );

            IRectangle globs = Aut.GetTestlingArea();
            Point32 point = globs.Center;
            float h = ( globs.Scale.X - 20 );
            Mouse( Move.Absolute, point.X-(int)h, point.Y );
            float H = (float)Math.Pow(h,2);
            float stueck = H/200.0f;
            float A = H;
            float B = 0;
            Click( Button.L | Button.DOWN );
            Sleep( 100 );
            CheckStep( touched, "wheel interacted" );
            touched = true;
            TimeSpan step = new TimeSpan((long)(System.Diagnostics.Stopwatch.Frequency * (1.5/1000.0) ));
            for( int i=0; i<98; ++i ) {
                stueck *= 1.01f;
                A -= stueck; B += stueck;
                Mouse( Move.Absolute, (int)(point.X - Math.Sqrt(A)), (int)(point.Y - Math.Sqrt(B)) );
                Sleep( step );
            }
            A -= stueck; B += stueck;
            Click( Button.L|Button.UP, (int)( point.X - Math.Sqrt(A) ), (int)( point.Y - Math.Sqrt(B) ));
            point.Y = (int)( point.Y - Math.Sqrt(B) );
            stueck = (float)Math.Sqrt(stueck);
            for( int i = 0; i < 98; ++i ) {
                point.x += 3;
                Mouse( Move.Absolute, point.X, point.Y );
                Sleep( step );
            }
            CheckStep( movement > 0, "wheel movement (expected greater 0) is: {0}", movement );
            Sleep( 100 );
            CheckStep( touched == false, "wheel released" );
            CheckStep( stops == 0, "wheel triggered {0} 'stoped' events during move", stops );
            Sleep( 3000 );
            CheckStep( stops > 0 && reversed == false && direction == JogDial.Direction.Clockwise && movement == 0,
                "wheel triggered '{0}' event (expected {1}) from turning '{2}' (expected {3})", 
                reversed ? "reverse" : "stoped", "stoped", direction, JogDial.Direction.Clockwise );
            Sleep( 2000 );
        }

        private void Jogdial_MovementFast( object sender, ValueChangeArgs<float> value )
        {
            fast = true;
        }

        private void Jogdial_WheelRelease( object sender, ValueChangeArgs<float> value )
        {
            touched = false;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
        }

        private void Jogdial_TurningStopt( object sender, ValueChangeArgs<JogDial.Direction> data )
        {
            ++stops;
            direction = data;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
        }

        private void Jogdial_WheelReverse( object sender, ValueChangeArgs<JogDial.Direction> data )
        {
            reversed = true;
            direction = data;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
        }

        private void Jogdial_WheelTouched( object sender, ValueChangeArgs<float> value )
        {
            touched = true;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
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
            Sleep( 1000 );

            while( testling.Orientation != Stepflow.Gui.Orientation.Vertical ) {
                Click( Button.L, GetScreenArea("btn_set_Orientation").Center );
                Thread.Sleep( 200 );
            }

            InfoStep( "Set {0} range: 0 to 100", CurrentCase );
            Controlled.Float32 testvalue = testling.valence().controller();
            testvalue.MIN = 0;
            testvalue.MAX = 100;

            Aut.SetControlWidth( 48 );
            Aut.SetControlHeight( 512 );
            InfoStep( "Assigning value 50 to {0}", CurrentCase );
            Aut.SetControlValue( 50.0f );
            Sleep ( 1000 );
            ConTrol.Point point = GetScreenArea( testling ).Center;
            InfoStep( "Sliding to topmost position" );
            Mouse( Move.Absolute, point );
            Click( Button.L|Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Thread.Sleep( 25 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            } ConTrol.Click( ConTrol.Button.L|ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MAX, CurrentCase+".Value", "...value changed to "+testvalue.MAX.ToString() );
            InfoStep( "Sliding down to bottom position" );
            ConTrol.Click( ConTrol.Button.L|ConTrol.Button.DOWN );
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep( 25 );
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
                Thread.Sleep( 25 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click( ConTrol.Button.L | ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MIN, CurrentCase + ".Value", "...value changed to "+ testvalue.MIN.ToString() );
            InfoStep( "Sliding down to bottom position" );
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.DOWN);
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep( 25 );
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
                Thread.Sleep( 25 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.UP);
            Thread.Sleep( 100 );
            InfoStep("Sliding down to bottom position");
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.DOWN);
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep( 25 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.UP);
            Thread.Sleep(100);

            MatchStep( counter, 3, "times a marker was passed", "times" );
            Click( ConTrol.Button.L, GetScreenArea("btn_Invert").Center );
        }
    }
}
