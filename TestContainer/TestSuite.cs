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
using MidiGUI.Test.Container;
using static Consola.Test.ConTrol;
using static System.Threading.Thread;
using Button = Consola.Test.ConTrol.Button;
using Win32Imports.Midi;

namespace MidiGUI.Test
{
    public class MidiGUIControls
        : Suite<Form1>
        , IMidiControlElement<MidiInOut>
    {
        private MidiInOut midiInOut;

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
            midiInOut = new MidiInOut();
            midi().binding.AutomationEvent += midi().OnIncommingMidiControl;
            
            midipoller = new Task( midiQueueAction );
            midipoller.Start();
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
                else rect = CenterAndScale.FromRectangle( Aut.Controls.Find( (string)descriptor, false )[0].Bounds );
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
        private Win32Imports.Midi.Message ExpectedMidi = new Win32Imports.Midi.Message(0u);
        private ExpectedEvent NextExpected = 0;
        private enum ExpectedEvent : int
        {
            NoneExpected = 0,
            TurningStopt = 1,
            WheelReverse = 2,
            WheelRelease = 3,
            MovementFast = 4,
            WheelTouched = 5
        }

        float lerp( float pos, float bis, float val )
        {
            return val * (pos / bis);
        }

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
            Click( Button.L, GetScreenArea("btn_set_Led").Center );
            Sleep( 1000 );
            Click( Button.L, GetScreenArea("btn_set_Led").Center );
            Sleep( 1000 );

            IRectangle dialarea = Aut.GetTestlingArea();
            Point32 point = dialarea.Center;
            float r = ( dialarea.Scale.X - 20 );
            
            TimeSpan step = new TimeSpan( (long)(System.Diagnostics.Stopwatch.Frequency * (1.5/1000.0) ));
            float Hypothenuse = (float)Math.Pow(r,2);
            float Ankathete = Hypothenuse;
            float Gegenkathete = 0;

            Mouse( Move.Absolute, point.X - (int)r, point.Y );
            NextExpected = ExpectedEvent.WheelTouched;
            Click( Button.L | Button.DOWN );
            Sleep( 100 );
            
            // set touched flag to true - to ensure a later 'WheelRelease'
            // test can recognize state changing back to not-touched anymore
            // even if 'WheelTouched' maybe not was triggered with this teststep
            touched = true;

            // doing vorwärts 'anschups' gesture
            for( int i=0; i < 99; ++i ) {
                Gegenkathete = lerp( i, 100, Hypothenuse );
                Ankathete = Hypothenuse - Gegenkathete;
                Mouse( Move.Absolute, (int)(point.X - Math.Sqrt(Ankathete)), (int)(point.Y - Math.Sqrt(Gegenkathete)) );
                Sleep( step );
            } Ankathete = 0; Gegenkathete = Hypothenuse;
            NextExpected = ExpectedEvent.WheelRelease;
            point.Y = (int)( point.Y - Math.Sqrt(Gegenkathete) );
            Click( Button.L|Button.UP, point.X, point.Y );
            Sleep( step );
            for( int i = 0; i < 33; ++i ) {
                point.x += 5;
                Mouse( Move.Absolute, point.X, point.Y );
                Sleep( step );
            }
            NextExpected = ExpectedEvent.TurningStopt;
            Sleep( 3000 );
            touched = false;
            movement = 0;
            stops = 0;
            angel = 0;
            accelleration = 0;
            direction = 0;
            point = dialarea.Center;
            Mouse( Move.Absolute, point.x + (int)r, point.y );
            Sleep( 100 );
            NextExpected = ExpectedEvent.WheelTouched;
            Click( Button.L|Button.DOWN );
            Sleep( 100 );
            for( int i = 0; i < 99; ++i ) {
                Ankathete = Hypothenuse - ( Gegenkathete = lerp( i, 100, Hypothenuse ) );
                Mouse( Move.Absolute, (int)( point.X + Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ));
                Sleep( step );
            }
            Gegenkathete = Hypothenuse; Ankathete = 0;
            Mouse( Move.Absolute, point.X, (int)(point.Y-r) );
            Sleep( step );
            for( int i = 1; i < 100; ++i ) {
                Gegenkathete = Hypothenuse - ( Ankathete = lerp( i, 100, Hypothenuse ) );
                Mouse( Move.Absolute, (int)( point.X - Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ));
                Sleep( step );
            } 
            NextExpected = ExpectedEvent.WheelReverse;
            for( int i = 1; i < 99; ++i ) {
                Ankathete = Hypothenuse - ( Gegenkathete = lerp( i, 100, Hypothenuse ) );
                Mouse( Move.Absolute, (int)( point.X - Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ) );
                Sleep( step );
            } Ankathete = 0; Gegenkathete = Hypothenuse;
            NextExpected = ExpectedEvent.WheelRelease;
            point.y = (short)( point.y - r );
            Click( Button.L|Button.UP, point.X, point.Y );
            Sleep( step );
            for( int i = 0; i < 10; ++i ) {
                point.x += 10;
                Mouse( Move.Absolute, point.X, point.Y );
                Sleep( step );
            } 
            Sleep( 2000 );
        }

        private void Jogdial_MovementFast( object sender, ValueChangeArgs<float> value )
        {
            fast = true;
        }

        private void Jogdial_WheelRelease( object sender, ValueChangeArgs<float> value )
        {
            JogDial dial = sender as JogDial;
            touched = dial.IsTouched;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = value.Value;
            if( NextExpected == ExpectedEvent.WheelRelease ) {
                CheckStep( touched == false,
                    "wheel triggered 'WheelRelease' at {0}° from rotating '{1}'", angel,
                    direction);
                NextExpected = ExpectedEvent.TurningStopt;
            } else FailStep( "wheel unexpectedly triggered a 'WheelRelease' event" );
        }

        private void Jogdial_TurningStopt( object sender, ValueChangeArgs<JogDial.Direction> data )
        {
            ++stops;
            direction = data;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
            if( NextExpected == ExpectedEvent.TurningStopt ) {
                CheckStep(movement == 0.0f, "wheel triggered 'TurningStopt' from rotating '{0}'", direction);
                NextExpected = ExpectedEvent.NoneExpected;
            } else FailStep("wheel unexpectedly triggered a 'TurningStopt' event");
        }

        private void Jogdial_WheelReverse( object sender, ValueChangeArgs<JogDial.Direction> data )
        {
            reversed = true;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
            if( NextExpected == ExpectedEvent.WheelReverse ) {
                CheckStep(direction != data.Value,
                    "wheel triggered 'WheelReverse' event changing direction from '{0}' to '{1}'",
                           direction, data.Value);
                NextExpected = ExpectedEvent.NoneExpected;
            } else FailStep("wheel unexpectedly triggered a 'WheelReverse' event");
            direction = data;
        }

        private void Jogdial_WheelTouched( object sender, ValueChangeArgs<float> value )
        {
            touched = true;
            JogDial dial = sender as JogDial;
            movement = dial.Movement;
            accelleration = dial.Accellaration;
            angel = dial.Position;
            if( NextExpected == ExpectedEvent.WheelTouched ) {
                CheckStep( touched, "wheel triggered 'WheelTouched' at {0}° degree", value.Value );
                NextExpected = ExpectedEvent.NoneExpected;
            } else FailStep( "wheel unexpectedly triggered a 'WheelTouched' event" );
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


        private AutomationlayerAddressat[] midisettings = new AutomationlayerAddressat[1];
        public Value MidiValue { get; set; }
        public MidiInOut binding { get { return midiInOut; } }
        public MidiInputMenu<MidiInOut> inputMenu { get; set; }
        public MidiOutputMenu<MidiInOut> outputMenu { get; set; }

        public AutomationlayerAddressat[] channels { get { return midisettings; } }

        private bool midiloop = true;
        private Task midipoller;
        public void midiQueueAction()
        {
            EventArgs e = new EventArgs();
            while( midiloop ) {
                if( midi().binding.automation().messageAvailable() )
                    midi().binding.automation().ProcessMessageQueue( this, e );
                midipoller.Wait( 10 );
            }
        }

        public void BindMidiControl()
        {
            //AutomationlayerAddressat autput = new AutomationlayerAddressat();
            //(Aut.GetStagedControl() as IAutomationController<Win32Imports.Midi.Message>).ConfigureAsMessagingAutomat()
        }

        public void OnIncommingMidiControl( object sender, Win32Imports.Midi.Message value )
        {
            if( value.Value == ExpectedMidi.Value )
                PassStep( "Received expected Midi Message: {0}", value );
            else
                FailStep( "Received nonsense: {0}", value );
        }

        public IMidiControlElement<MidiInOut> midi()
        {
            return this;
        }
    }
}
