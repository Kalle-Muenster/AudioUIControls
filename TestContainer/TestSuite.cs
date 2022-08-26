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
    public class Suite
        : Suite<Container.Form1>
    {
        public Suite( Container.Form1 mainwindow, TestResults args )
            : base( mainwindow
                  , args.HasFlag(TestResults.Verbose)
                  , args.HasFlag(TestResults.XmlOutput)
                  ) {
            AddTestCase( "LedButton", Test_LedButton );
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
            InfoStep("Assigning value 50 to {0}", CurrentCase);
            Aut.SetControlValue( 50.0f );
            Thread.Sleep ( 1000 );
            ConTrol.Point point = GetScreenArea( testling ).Center;
            InfoStep("Sliding till maximum");
            ConTrol.Mouse( ConTrol.Move.Absolute, point );
            ConTrol.Click( ConTrol.Button.L|ConTrol.Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            } ConTrol.Click( ConTrol.Button.L|ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MAX, CurrentCase+".Value", "...value changed to 100" );
            InfoStep( "Sliding down to minimum" );
            ConTrol.Click( ConTrol.Button.L|ConTrol.Button.DOWN );
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep( 100 );
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            } ConTrol.Click( ConTrol.Button.L|ConTrol.Button.UP );
            Thread.Sleep( 100 );
            MatchStep( testvalue.VAL, testvalue.MIN, CurrentCase+".Value", "...value changed to 0" );

            Thread.Sleep( 100 );
            Aut.SetControlValue( 50.0f );
            ConTrol.Click( ConTrol.Button.L, GetScreenArea("btn_Invert").Center );
            Thread.Sleep(200);
            ConTrol.Click( ConTrol.Button.L, GetScreenArea("btn_Invert").Center );
            Thread.Sleep(200);
            ConTrol.Click( ConTrol.Button.L, GetScreenArea("btn_Invert").Center );
            Thread.Sleep(200);

            point = GetScreenArea( testling ).Center;
            InfoStep( "Sliding till maximum" );
            ConTrol.Mouse( ConTrol.Move.Absolute, point );
            ConTrol.Click( ConTrol.Button.L | ConTrol.Button.DOWN );
            for( int i = 0; i < 25; ++i ) {
                point.Y -= 10;
                Thread.Sleep(100);
                ConTrol.Mouse( ConTrol.Move.Absolute, point );
            }
            ConTrol.Click( ConTrol.Button.L | ConTrol.Button.UP );
            Thread.Sleep(100);
            MatchStep( testvalue.VAL, testvalue.MIN, CurrentCase + ".Value", "...value changed to "+ testvalue.MIN.ToString() );
            InfoStep( "Sliding down to minimum" );
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.DOWN);
            for( int i = 0; i < 50; ++i ) {
                point.Y += 10;
                Thread.Sleep(100);
                ConTrol.Mouse(ConTrol.Move.Absolute, point);
            }
            ConTrol.Click(ConTrol.Button.L | ConTrol.Button.UP);
            Thread.Sleep(100);
            MatchStep( testvalue.VAL, testvalue.MAX, CurrentCase + ".Value", "...value changed to " + testvalue.MAX.ToString() );

        }
    }
}
