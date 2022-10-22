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
using Win32Imports.Midi;
using static Consola.Test.ConTrol;
using static System.Threading.Thread;
using Button = Consola.Test.ConTrol.Button;
using Point = Consola.Test.ConTrol.Point;

namespace MidiGUI.Test
{
    public class SuiteGUIControls
        : Suite<Container.Form1>
        , IMidiControlElement<MidiInOut>
    {
        private MidiInOut midiInOut;
        private Container.JogDial jogdial;
        private Container.Sliders sliders;
        private Container.Meters  meters;
        private Win32Imports.Midi.Message ExpectedMidi = new Win32Imports.Midi.Message(0u);
        internal Container.Form1 GetAut() { return Aut; }

        public SuiteGUIControls( Container.Form1 mainwindow, TestResults args )
            : this( mainwindow, args, string.Empty )
        {}

        public SuiteGUIControls( Container.Form1 mainwindow, TestResults args, string testcase )
            : base( mainwindow
                  , args.HasFlag(TestResults.Verbose)
                  , args.HasFlag(TestResults.XmlOutput)
                  ) {

            sliders = new Container.Sliders( this );
            meters = new Container.Meters( this );

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
                rect = new SystemDefault( (Rectangle)descriptor );
            } else if( descriptor is IRectangle ) {
                rect = descriptor as IRectangle;
            } else if( descriptor is string ) {
                string name = (string)descriptor;
                if( name == "Testling" ) return Aut.GetTestlingArea().ConTrolArea();
                else rect = CenterAndScale.FromRectangle( Aut.Controls.Find( (string)descriptor, false )[0].Bounds );
            } else if( descriptor is Control ) {
                rect = new SystemDefault( (descriptor as Control).Bounds );
            } else rect = CenterAndScale.Empty;
            Area area = new Area( rect.W, rect.H );
            return area.At( new ConTrol.Point( Win.Point.X + rect.X, Win.Point.Y + rect.Y ) );
        }

        public Area StyleButton {
            get { return GetScreenArea("btn_set_Style"); }
        }

        public Area OrientationButton {
            get { return GetScreenArea("btn_set_Orientation"); }
        }

        public Area InvertButton {
            get { return GetScreenArea("btn_Invert"); }
        }

        public Area ColorButton {
            get { return GetScreenArea("btn_set_Led"); }
        }

        protected override Area GetWindowArea()
        {
            return Aut.location.ConTrolArea();
        }

        private void SelectControlType( Type controltype )
        {
            Click( Button.L, GetMenuPoint( "Controlls" ) );
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


        private void Test_JogDial()
        {
            SelectControlType( typeof(JogDial) );
            jogdial = new Container.JogDial( this, Aut.GetStagedControl() as JogDial );
            CheckStep( jogdial.InstanceExists, "{0} instanciated", CurrentCase );
            Sleep( 1000 );
            jogdial.SwitchLEDColor();
            jogdial.SwitchLEDColor();
            jogdial.TestInteraction();
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
                Click( Button.L, OrientationButton.Center );
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
                ConTrol.Click( ConTrol.Button.L, InvertButton.Center );
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

            ConTrol.Click( ConTrol.Button.L, InvertButton.Center );
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
            Click( ConTrol.Button.L, InvertButton.Center );
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
