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
    public class JogDial
    {
        public enum ExpectedEvent : int
        {
            NoneExpected = 0,
            TurningStopt = 1,
            WheelReverse = 2,
            WheelRelease = 3,
            MovementFast = 4,
            WheelTouched = 5
        }

        private SuiteGUIControls      test;
        private Stepflow.Midi.JogDial dial;
        private bool touched = false;
        private bool reversed = false;
        private float movement = 0.0f;
        private Stepflow.Midi.JogDial.Direction direction = Stepflow.Midi.JogDial.Direction.StandingStill;
        private float angel = 0.0f;
        private float accelleration = 0.0f;
        private bool fast = false;
        private int stops = 0;
        
        private ExpectedEvent NextExpected = 0;

        


        public JogDial( Consola.Test.Test suite, Stepflow.Midi.JogDial testling )
        {
            test = suite as SuiteGUIControls;
            dial = testling;
            if( testling != null ) {
                dial.WheelTouched += WheelTouched_Event;
                dial.WheelReverse += this.WheelReverse_Event;
                dial.TurningStopt += TurningStopt_Event;
                dial.WheelRelease += WheelRelease_Event;
                dial.MovementFast += MovementFast_Event;
            }
        }

        public bool InstanceExists {
            get { return dial != null; }
        }

        public LED SwitchLEDColor()
        {
            Click( Button.L, test.ColorButton.Center );
            Sleep( 100 );
            return dial.LedColor;
        }

        public void TestInteraction()
        {
            IRectangle dialarea = test.GetAut().GetTestlingArea();
            Point32 point = dialarea.Center;
            float r = ( dialarea.Scale.X - 20 );

            TimeSpan step = new TimeSpan( (long)(System.Diagnostics.Stopwatch.Frequency * (1.5/1000.0) ));
            float Hypothenuse = (float)Math.Pow(r,2);
            float Ankathete = Hypothenuse;
            float Gegenkathete = 0;

            Mouse(Move.Absolute, point.X - (int)r, point.Y);
            NextExpected = ExpectedEvent.WheelTouched;
            Click(Button.L | Button.DOWN);
            Sleep(100);

            // set touched flag to true - to ensure a later 'WheelRelease'
            // test can recognize state changing back to not-touched anymore
            // even if 'WheelTouched' maybe not was triggered with this teststep
            touched = true;

            // doing vorwärts 'anschups' gesture
            for( int i = 0; i < 99; ++i ) {
                Gegenkathete = Helper.lerp(i, 100, Hypothenuse);
                Ankathete = Hypothenuse - Gegenkathete;
                Mouse(Move.Absolute, (int)( point.X - Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ));
                Sleep(step);
            }
            Ankathete = 0; Gegenkathete = Hypothenuse;
            NextExpected = ExpectedEvent.WheelRelease;
            point.Y = (int)( point.Y - Math.Sqrt(Gegenkathete) );
            Click(Button.L | Button.UP, point.X, point.Y);
            Sleep(step);
            for( int i = 0; i < 33; ++i ) {
                point.x += 5;
                Mouse(Move.Absolute, point.X, point.Y);
                Sleep(step);
            }
            NextExpected = ExpectedEvent.TurningStopt;
            Sleep(3000);
            touched = false;
            movement = 0;
            stops = 0;
            angel = 0;
            accelleration = 0;
            direction = 0;
            point = dialarea.Center;
            Mouse(Move.Absolute, point.x + (int)r, point.y);
            Sleep(100);
            NextExpected = ExpectedEvent.WheelTouched;
            Click(Button.L | Button.DOWN);
            Sleep(100);
            for( int i = 0; i < 99; ++i ) {
                Ankathete = Hypothenuse - ( Gegenkathete = Helper.lerp(i, 100, Hypothenuse) );
                Mouse(Move.Absolute, (int)( point.X + Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ));
                Sleep(step);
            }
            movement = dial.Movement;
            direction = movement > 0
                      ? Stepflow.Midi.JogDial.Direction.Clockwise
                      : movement < 0
                      ? Stepflow.Midi.JogDial.Direction.CounterClock
                      : Stepflow.Midi.JogDial.Direction.StandingStill;
            Gegenkathete = Hypothenuse; Ankathete = 0;
            Mouse( Move.Absolute, point.X, (int)( point.Y - r ) );
            Sleep( step );
            for( int i = 1; i < 100; ++i ) {
                Gegenkathete = Hypothenuse - ( Ankathete = Helper.lerp(i, 100, Hypothenuse) );
                Mouse(Move.Absolute, (int)( point.X - Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ));
                Sleep(step);
            }
            NextExpected = ExpectedEvent.WheelReverse;
            for( int i = 1; i < 99; ++i ) {
                Ankathete = Hypothenuse - ( Gegenkathete = Helper.lerp(i, 100, Hypothenuse) );
                Mouse(Move.Absolute, (int)( point.X - Math.Sqrt(Ankathete) ), (int)( point.Y - Math.Sqrt(Gegenkathete) ));
                Sleep(step);
            }
            Ankathete = 0; Gegenkathete = Hypothenuse;
            NextExpected = ExpectedEvent.WheelRelease;
            point.y = (short)( point.y - r );
            Click(Button.L | Button.UP, point.X, point.Y);
            Sleep(step);
            for( int i = 0; i < 10; ++i ) {
                point.x += 10;
                Mouse(Move.Absolute, point.X, point.Y);
                Sleep(step);
            }
            Sleep(2000);
        }

        public void MovementFast_Event( object sender, ValueChangeArgs<float> value )
        {
            fast = true;
        }

        public void WheelRelease_Event( object sender, ValueChangeArgs<float> value )
        {
            Stepflow.Midi.JogDial jogdial = sender as Stepflow.Midi.JogDial;
            touched = jogdial.IsTouched;
            movement = jogdial.Movement;
            accelleration = jogdial.Accellaration;
            angel = value.Value;
            if( NextExpected == ExpectedEvent.WheelRelease ) {
                test.CheckStep( touched == false,
                    "wheel triggered 'WheelRelease' at {0}° while rotating '{1}'", angel,
                    direction );
                NextExpected = ExpectedEvent.TurningStopt;
            } else test.FailStep( "wheel unexpectedly triggered a 'WheelRelease' event" );
        }

        public void TurningStopt_Event( object sender, ValueChangeArgs<Stepflow.Midi.JogDial.Direction> data )
        {
            ++stops;
            direction = data;
            Stepflow.Midi.JogDial jogdial = sender as Stepflow.Midi.JogDial;
            movement = jogdial.Movement;
            accelleration = jogdial.Accellaration;
            angel = jogdial.Position;
            if( NextExpected == ExpectedEvent.TurningStopt ) {
                test.CheckStep(movement == 0.0f, "wheel triggered 'TurningStopt' from rotating '{0}'", direction);
                NextExpected = ExpectedEvent.NoneExpected;
            } else test.FailStep("wheel unexpectedly triggered a 'TurningStopt' event");
        }

        public void WheelReverse_Event( object sender, ValueChangeArgs<Stepflow.Midi.JogDial.Direction> data )
        {
            reversed = true;
            Stepflow.Midi.JogDial jogdial = sender as Stepflow.Midi.JogDial;
            movement = jogdial.Movement;
            accelleration = jogdial.Accellaration;
            angel = jogdial.Position;
            if( NextExpected == ExpectedEvent.WheelReverse ) {
                test.CheckStep( direction != data.Value,
                    "wheel triggered 'WheelReverse' event changing direction from '{0}' to '{1}'",
                                direction, data.Value );
                NextExpected = ExpectedEvent.NoneExpected;
            } else test.FailStep("wheel unexpectedly triggered a 'WheelReverse' event");
            direction = data;
        }

        public void WheelTouched_Event( object sender, ValueChangeArgs<float> value )
        {
            touched = true;
            Stepflow.Midi.JogDial jogdial = sender as Stepflow.Midi.JogDial;
            movement = jogdial.Movement;
            accelleration = jogdial.Accellaration;
            angel = jogdial.Position;
            if( NextExpected == ExpectedEvent.WheelTouched ) {
                test.CheckStep(touched, "wheel triggered 'WheelTouched' at {0}° degree", value.Value);
                NextExpected = ExpectedEvent.NoneExpected;
            } else test.FailStep("wheel unexpectedly triggered a 'WheelTouched' event");
        }
    }
}
