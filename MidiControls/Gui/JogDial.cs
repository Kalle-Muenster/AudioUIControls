
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Stepflow;
using Stepflow.TaskAssist;
using Stepflow.Gui;
using Stepflow.Gui.Helpers;
using Stepflow.Gui.Geometry;
using Stepflow.Gui.Automation;
using Stepflow.Controller;
using MidiControls.Properties;
using Midi = Win32Imports.Midi;
using Stepflow.Midi.Helpers;
using Message = Win32Imports.Midi.Message;
using Rectangle = System.Drawing.Rectangle;


namespace Stepflow {
namespace Midi
{
    public enum JogDialValence {
        Absolute=1, Relative=0
    };

    public partial class JogDial 
        : UserControl
        , IInterValuable<Controlled.Float32>
        , IMidiControlElement<MidiInOut>
        , ITaskAsistableVehicle<Action,Action>
        , ITouchGesturedElement<JogDial>
    {
        public const JogDialValence Absolute = JogDialValence.Absolute;
        public const JogDialValence Relative = JogDialValence.Relative;
        private const int PropellorSpeed = 50;

        public enum Direction {
            Clockwise = 1,
            StandingStill = 0,
            CounterClock = -1
        }
        public enum InteractionMode {
            FlowByQuadrants = RondealInteraction.VierQuadranten,
            RotationByAngle = RondealInteraction.ByAngel
        }


        private static Bitmap[][]  image;
        private static Bitmap[]    leds;
        private static Rectangle[] ledBG;
         
        public delegate void DirectionHasChanged( object sender, ValueChangeArgs<Direction> data );
        public delegate void MouseEventDelegator( object sender, MouseEventArgs e );

        public event ValueChangeDelegate<float> MovementFast;
        public event ValueChangeDelegate<float> ValueChanged;
        public event ValueChangeDelegate<float> WheelTouched;
        public event ValueChangeDelegate<float> WheelRelease;
        public event DirectionHasChanged WheelReverse;
        public event DirectionHasChanged TurningStopt;

        private MouseEventDelegator SlopeDown;
        private Controlled.Float32 flow;
        private Controlled.Float32 pos;

        private float      scaleFactor = 1;
        private float      pixelScalar;
        private Matrix     rotator = new Matrix();
        private Point      lastMouse;
        private Point      halfSize;
        private LedGlimmer glimmer;
        private int        softrelease = 0;
        private float      angleOffset = 0;
        private bool?      quadrants;
        private bool       canStop;
        private Direction  currentDir;
        private MidiInOut  midiIO = null;
        private TaskAssist<SteadyAction,Action,Action> propellor;

        private static Rectangle rectFromXmlSheet(System.Xml.XPath.XPathNavigator xpath, string name)
        {
            string xpress = "//brush[@name='"+name+"']/{0}/text()";
            return new Rectangle(
                int.Parse(xpath.SelectSingleNode(string.Format(xpress,"X")).ToString()),
                int.Parse(xpath.SelectSingleNode(string.Format(xpress,"Y")).ToString()),
                int.Parse(xpath.SelectSingleNode(string.Format(xpress,"W")).ToString()),
                int.Parse(xpath.SelectSingleNode(string.Format(xpress,"H")).ToString())
            );
        }

        static JogDial() 
        {
            Valence.RegisterIntervaluableType<Controlled.Float32>();

            image = new Bitmap[3][] {
                new Bitmap[2] { Resources.DasFlacheRad,
                                Resources.DieFlachenLeds_png },
                new Bitmap[2] { Resources.DasNeueRad,
                                Resources.DieNeuenLeds },
                new Bitmap[2] { Resources.DasDunkleRad,
                                Resources.DieDunklenLeds }
            };

            leds = new Bitmap[8] {
                Resources.jogdial_leds_green,
                Resources.jogdial_leds_red,
                Resources.jogdial_leds_gelb,
                Resources.jogdial_leds_blue,
                Resources.jogdial_leds_orange,
                Resources.jogdial_leds_mint,
                Resources.jogdial_leds_pink,
                Resources.jogdial_leds_cyan
            };

            System.Xml.XPath.XPathNavigator xpath = new System.Xml.XPath.XPathDocument(
                   new System.IO.StringReader( Resources.DieFlachenLeds_xml ) 
                                                                         ).CreateNavigator();
            ledBG = new Rectangle[] {
                rectFromXmlSheet(xpath,"Top"), rectFromXmlSheet(xpath,"Left"),
                rectFromXmlSheet(xpath,"Right"), rectFromXmlSheet(xpath,"Bottom")
            };

            TaskAssist<SteadyAction,Action,Action>.Init( PropellorSpeed );

            if( !PointerInput.isInitialized() ) {
                PointerInput.AutoRegistration = AutoRegistration.Enabled;
            }
        }                                                    


        private void TriggerEvents( bool byFlow ) 
        {
            bool  state;
            float fLast;
            float pLast;
            float actual;

            if( byFlow ) { unsafe { 
                fLast = *(float*)flow.GetPin( ElementValue.LAST ).ToPointer();
                    pLast = pos.VAL; 
                } state = flow.Check();
                actual = flow.VAL;
                pos.VAL = ( pLast + (actual * scaleFactor) );
                pos.Check();
                valence( Absolute ).SetDirty( ValenceFieldState.Flags.VAL );
            } else { 
                // if byPosition
                unsafe { pLast = *(float*)pos.GetPin( ElementValue.LAST ).ToPointer(); }
                pos.Check();
                flow.VAL = (pos.MOV / scaleFactor);
                unsafe { fLast = *(float*)flow.GetPin( ElementValue.LAST ).ToPointer(); }
                state = flow.Check();
                actual = flow.VAL;
                valence( Relative ).SetDirty( ValenceFieldState.Flags.VAL );
            }
            float flowMov = flow.MOV;
            if( actual != fLast ) {
                midiIO.output().OnValueChange( this, MidiValue );
                if( canStop == false ) {
                    canStop = Math.Abs( actual ) > IsDownBelow;
                    if( canStop ) {
                        if( actual > 0 && flowMov > 0 )
                            currentDir = Direction.Clockwise;
                        else if ( actual < 0 && flowMov < 0 )
                            currentDir = Direction.CounterClock;
                    }
                } ValueChanged?.Invoke( this, new ValueChangeArgs<float>( actual ) );
            }
            if ( WheelReverse != null || (TurningStopt != null && canStop) ) {
                if ( (fLast <= 0 && actual > 0) || (actual < 0 && fLast >= 0) ) {
                        Direction newDir = actual > 0
                                         ? Direction.Clockwise
                                         : Direction.CounterClock;
                    if( newDir != currentDir ) {
                        WheelReverse?.Invoke( this, new ValueChangeArgs<Direction>( currentDir = newDir ) );
                        canStop = true;
                    } 
                } else if ( canStop && Math.Abs( actual ) < IsDownBelow && flowMov == 0 ) {
                    useracces = canStop = false;
                    TurningStopt?.Invoke( this, new ValueChangeArgs<Direction>( currentDir ) );
                }
            }
            if ( MovementFast != null ) unsafe {
                int m = ( 360 - (int)Math.Abs( pos.VAL - pLast ) ) % 360;
                if ( m > (flow.MAX * scaleFactor) ) {
                    MovementFast( this, new ValueChangeArgs<float>( actual > 0 ? m : -m ) );
                }
            }
            flow.Active = state;
            pos.Active = state;
        }


        private int  style;
        public Style Style {
            get { return (Style)style; }
            set { if( (int)value != style ) {
                    style = (int)value;
                    BackgroundImage = image[style][0];
                    if( value!=Style.Flat ) {
                        int col = (int)LedColor;
                        col = col < 8 ? col : 7;
                        glimmer.SetImage( leds[col] );
                    }
                    glimmer.SetStyle( value );
                    Invalidate(true);
                }
            }
        }

        public LED LedColor {
            get { return glimmer.Led; }
            set {
                glimmer.Led = value;
                if( Style!=Style.Flat )
                    if( value < LED.off )
                        glimmer.SetImage( leds[(int)value] );
            }
        }

        public float LedIntensity {
            get { return glimmer.Pre; }
            set { glimmer.Pre = value; }
        }

        private InteractionMode interaction;
        public InteractionMode Interaction {
            get { return interaction; }
            set { if( value != interaction ) {
                    if( quadrants != null ) {
                        if ( quadrants == false ) {
                            Wheel.MouseMove -= Wheel_DirectionalMouseMove;
                            Wheel.MouseMove += Wheel_VierQuadrantenMouseMove;
                            TouchMove -= Wheel_DirectionalTouchMove;
                            TouchMove += Wheel_VierQuadrantenTouchMove;
                            quadrants = true;
                        } else {
                            Wheel.MouseMove -= Wheel_VierQuadrantenMouseMove;
                            Wheel.MouseMove += Wheel_DirectionalMouseMove;
                            TouchMove -= Wheel_VierQuadrantenTouchMove;
                            TouchMove += Wheel_DirectionalTouchMove;
                            quadrants = false;
                        }
                    } interaction = value;
                }
            }
        }


        private volatile uint slopetime = 0;
        private volatile bool useracces = false;
        private volatile bool slopecase = false;
        private volatile bool midiacces = false;
        public bool SlopeCase {
            get { return slopecase; }
            private set {
                if ( value != slopecase ) {
                    if( slopecase = value ) {
                        task().StartAssist();
                    } else {
                        task().StoptAssist();
                        if( !useracces ) {
                            float last = flow.VAL; 
                            flow.VAL = 0;
                            valence( Relative ).SetDirty( ValenceFieldState.Flags.VAL );
                            if( ValueChanged != null ) {
                                ValueChanged( this, new ValueChangeArgs<float>( flow ) );
                            }
                            if( TurningStopt != null && canStop ) {
                                TurningStopt( this, new ValueChangeArgs<Direction>( last > 0
                                            ? Direction.Clockwise
                                            : Direction.CounterClock ) );
                                canStop = false;
                            }
                        }  
                    }
                }
            }
        }

        public int SlopePrecision {
            get { return 1000 / timer1.Interval; }
            set { timer1.Interval = 1000 / value; }
        }

        private float IsDownBelow = 0.075f;
        public float DeadZone {
            get { return IsDownBelow*scaleFactor; }
            set { IsDownBelow = value/scaleFactor; }
        }

        private float DampfActor = 0.925f;
        public float DampFactor {
            get { return DampfActor; }
            set { DampfActor = value; }
        }

        public float ValueRange {
            get { return flow.MAX; }
            set { flow.MIN = -value;
                  flow.MAX = value;
                valence( Relative ).SetDirty( ValenceFieldState.Flags.MIN
                                            | ValenceFieldState.Flags.MAX );
                scaleFactor = (10.0f / value);
            }
        }

        // e.g. position (absolute rotation angel)
        public float Position {
            get { return pos; }
            set { pos.VAL = value;
                valence( Absolute ).SetDirty( ValenceFieldState.Flags.VAL );
                TriggerEvents( false );
                if( ColorRotation ) glimmer.Hue = pos;
                Invalidate();
            }
        }

        // e.g. delta position (speed of rotation)
        public float Movement {
            get { return flow; }
            set { flow.VAL = value;
                valence( Relative ).SetDirty( ValenceFieldState.Flags.VAL );
                if ( !(slopecase || useracces) ) {
                    SlopeCase = true;
                } TriggerEvents( true );
                Invalidate();
            }
        }

        // e.g. delta Movement (speed of rotation speed increasing/decreasing)
        public float Accellaration {
            get { return flow.MOV; }
            set { flow.MOV = value;
                  valence( Relative ).SetDirty( ValenceFieldState.Flags.MOV );
                  Movement += value; }
        }


        public JogDial() { 

            (this as ITouchGesturedElement<JogDial>).handler = new TouchGesturesHandler<JogDial>( this );

            interaction = InteractionMode.FlowByQuadrants;
            InitializeComponent();
            InitMidi( components );

            // maybe this could be done by a timer thread better - DONE
            // Paint += midi().binding.automation().ProcessMessageQueue;

            style = (int)Style.Lite;
            BackgroundImage = image[style][0];
            scaleFactor = 1;

            pos = new Controlled.Float32();
            pos.SetUp(0, 360, 0, 0, ControlMode.Element);
            flow = new Controlled.Float32();
            flow.SetUp(-10, 10, IsDownBelow, 0, ControlMode.Element);
            unsafe {
                *(bool*)pos.GetPin(ElementValue.UNSIGNED).ToPointer() = false;
                *(bool*)pos.GetPin(ElementValue.CYCLED).ToPointer() = true;
                *(bool*)flow.GetPin(ElementValue.CYCLED).ToPointer() = false;
            }

            flow.Active = true;
            pos.Active = false;

            joints = new ValenceField<Controlled.Float32,JogDialValence>( this, new Controlled.Float32[2] { flow, pos } );

            int s = 256;
            Rectangle[] ledsources = new Rectangle[8] {
                new Rectangle(0,0,s,s), new Rectangle(0,0,s,s),
                new Rectangle(0,0,s,s), new Rectangle(0,0,s,s),
                new Rectangle(0,0,s,s), new Rectangle(0,0,s,s),
                new Rectangle(0,0,s,s), new Rectangle(0,0,s,s),
            };

            glimmer = new LedGlimmer( leds[0], 1.0f );
            glimmer.AddSheet( ledsources );
            glimmer.SetStyle( Style );
            glimmer.Pre = 0.075f;

            propellor     = new TaskAssist<SteadyAction,Action,Action>( this, WheelPropellor, PropellorSpeed );
            MausPropellor = true;
            MidiPropellor = false;

            if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                if( PointerInput.Dispatcher == null ) {
                    PointerInput.Initialized += ( this as ITouchableElement ).TouchInputReady;
                } else
                    PointerInput.Dispatcher.RegisterTouchableElement( this );
            }

            Disposed += JogDial_Disposed;

            Width = Height = 256;
            Invalidate( true );
        }

            private void JogDial_Disposed( object sender, EventArgs e )
            {
                Valence.UnRegisterIntervaluableElement( this );
                PointerInput.Dispatcher?.UnRegisterTouchableElement( this );
            }

            public event MultiFinger.TouchDelegate TouchDraged {
                add { touchEvents().TouchDraged += value; }
                remove { touchEvents().TouchDraged -=value; }
            }

            public event MultiFinger.TouchDelegate TouchResize {
                add { touchEvents().TouchResize += value; }
                remove { touchEvents().TouchResize -=value; }
            }

            public event MultiFinger.TouchDelegate TouchRotate {
                add { touchEvents().TouchRotate += value; }
                remove { touchEvents().TouchRotate -= value; }
            }

            public event FingerTip.TouchDelegate TouchTapped {
                add { touchEvents().TouchTapped += value; }
                remove { touchEvents().TouchTapped -= value; }
            }

            public event FingerTip.TouchDelegate TouchDupple {
                add { touchEvents().TouchDupple += value; }
                remove { touchEvents().TouchDupple -= value; }
            }

            public event FingerTip.TouchDelegate TouchTrippl {
                add { touchEvents().TouchTrippl += value; }
                remove { touchEvents().TouchTrippl -= value; }
            }

            public event FingerTip.TouchDelegate TouchDown {
                add { touchEvents().TouchDown += value; }
                remove { touchEvents().TouchDown -= value; }
            }

            public event FingerTip.TouchDelegate TouchLift {
                add { touchEvents().TouchLift += value; }
                remove { touchEvents().TouchLift -= value; }
            }

            public event FingerTip.TouchDelegate TouchMove {
                add { touchEvents().TouchMove += value; }
                remove { touchEvents().TouchMove -= value; }
            }

            public ITouchEventTrigger touch { get { return (this as ITouchGesturedElement<JogDial>).handler; } }
            private IGestureTouchTrigger touchEvents() { return (this as ITouchGesturedElement<JogDial>).handler.events(); }

            Control ITouchable.Element { get { return this; } }

            public bool IsTouched { get { return touch.IsTouched; } }

            TouchGesturesHandler<JogDial> ITouchGesturedElement<JogDial>.handler {
                get; set;
            }

            void ITouchGestutred.OnTouchDraged( MultiFinger tip )
            {}

            void ITouchGestutred.OnTouchResize( MultiFinger tip )
            {}

            void ITouchGestutred.OnTouchRotate( MultiFinger tip )
            {}

            void ITouchSelectable.OnTouchTapped( FingerTip tip )
            {}

            void ITouchSelectable.OnTouchDupple( FingerTip tip )
            {}

            void ITouchSelectable.OnTouchTrippl( FingerTip tip )
            {}

            void IBasicTouchable.OnTouchDown( FingerTip tip )
            {
                Wheel_StartInteract( tip.Position );
            }

            void IBasicTouchable.OnTouchMove( FingerTip tip )
            {}

            void IBasicTouchable.OnTouchLift( FingerTip tip )
            {
                tip.KeepTrack( this );
                softrelease = 5;
                WheelRelease?.Invoke( this, new ValueChangeArgs<float>(pos) );
                slopecase = false;
                SlopeCase = true;
            }

            Point64 ITouchableElement.ScreenLocation()
            {
                return PointToScreen( Point.Empty );
            }

            IRectangle ITouchableElement.ScreenRectangle()
            {
                return AbsoluteEdges.FromRectangle( RectangleToScreen(new Rectangle(0, 0, Width, Height)) );
            }

            void ITouchableElement.TouchInputReady( PointerInput touchdevice )
            {
                PointerInput.Initialized -= touch.element().TouchInputReady;
                touchdevice.RegisterTouchableElement( this );
            }

            ITouchDispatchTrigger ITouchable.screen()
            {
                return touch.screen();
            }

            ITouchableElement ITouchable.element()
            {
                return this;
            }

        private void MidiIn_PortChanged( object sender, int newPortId )
        {
            Message.Filter currentlyActive = midi().MessageFilter; 
            midi().RemoveAnyFilters();
            midi().UseMessageFilter( currentlyActive );
        }

        protected override void OnResize( EventArgs e )
        { base.OnResize(e);
            int s = Math.Max(Width,Height);
            Wheel.Width = Width = Wheel.Height = Height = s;
            halfSize.X = halfSize.Y = s/2;
            pixelScalar = ((float)Width/(float)image[style][0].Width) * 180;
            Invalidate();
        }

        private bool colorotation = false;
        public bool ColorRotation {
            get { return colorotation; }
            set { glimmer.SetSheet(value ? 1 : 0);
                  colorotation = value; }
        }

        protected override void OnPaint( PaintEventArgs e )
        { base.OnPaint(e);

            float angle = pos.VAL;
            GraphicsState pushed = e.Graphics.Save();
              if( Style == Style.Flat ) {      
                rotator.Reset();
                float scale = (float)Wheel.Width/image[style][0].Width;
                rotator.Scale( scale, scale );
                rotator.Multiply( e.Graphics.Transform );
                e.Graphics.Transform = rotator;
                rotator.Reset();
                rotator.RotateAt( angle , halfSize );
                rotator.Multiply( e.Graphics.Transform );
                e.Graphics.Transform = rotator;
                foreach( Rectangle led in ledBG )
                    glimmer.DrawBrush(e.Graphics, led);
                rotator.Reset();
                rotator.RotateAt( 45 , halfSize );
                rotator.Multiply( e.Graphics.Transform );
                e.Graphics.Transform = rotator;
                foreach( Rectangle led in ledBG )
                    glimmer.DrawBrush(e.Graphics, led);
                e.Graphics.Restore( pushed );
              }
                rotator.Reset();
                rotator.RotateAt( angle , halfSize );
                rotator.Multiply( e.Graphics.Transform );
                e.Graphics.Transform = rotator;
                glimmer.Lum = Math.Abs( Movement ) / ValueRange;
                e.Graphics.DrawImage( image[style][1], Wheel.Bounds );
                if( Style != Style.Flat )
                    glimmer.DrawSprite( e.Graphics, Wheel.Bounds );
                else
                    e.Graphics.DrawImage( image[style][1], Wheel.Bounds );
            e.Graphics.Restore( pushed );
        }

        
        private float touchPointAngle( PointF touchpoint )
        {
            lastMouse.X = (int)touchpoint.X;
            lastMouse.Y = (int)touchpoint.Y;
            touchpoint.X = Wheel.Location.X + Wheel.Width/2;
            touchpoint.Y = Wheel.Location.Y + Wheel.Height/2;
            touchpoint.X = lastMouse.X - touchpoint.X;
            touchpoint.Y = lastMouse.Y - touchpoint.Y;
            if( touchpoint.X < 0 ) {
                touchpoint.Y = touchpoint.Y == 0 ? float.Epsilon : touchpoint.Y;
                return (float)( ((270.0 - (Math.Atan( touchpoint.X / touchpoint.Y ) * 180.0) / Math.PI) 
                              + ((touchpoint.Y > 0)? -90.0 : 90.0) ) % 360.0);
            } else {
                touchpoint.X = touchpoint.X == 0 ? float.Epsilon : touchpoint.X;
                return (float)((90 + (Math.Atan( touchpoint.Y / touchpoint.X ) * 180.0) / Math.PI) % 360.0);
            }  
        }

        private float touchPointQuads( Point touchPoint )
        {
            float m = ( (float)((touchPoint.X) - lastMouse.X) * -((float)((touchPoint.Y+1)-halfSize.Y)*0.25f)
                      + (float)((touchPoint.Y) - lastMouse.Y) *  ((float)((touchPoint.X+1)-halfSize.X)*0.25f)
                       ) / pixelScalar;

            lastMouse = touchPoint;
            return m; 
        }


        private void Wheel_StartInteract( Point touchpoint )
        {
            if( ( quadrants = interaction == InteractionMode.FlowByQuadrants ).Value ) {
                Wheel.MouseMove += Wheel_VierQuadrantenMouseMove;
                TouchMove += Wheel_VierQuadrantenTouchMove;
                lastMouse = touchpoint;
            } else {
                angleOffset = touchPointAngle( touchpoint ) - Position;
                Wheel.MouseMove += Wheel_DirectionalMouseMove;
                TouchMove += Wheel_DirectionalTouchMove;
            }
            Wheel.MouseLeave += Wheel_MouseLeave;
            if( ( !useracces ) && slopecase ) {
                SlopeCase = true;
            } else {
                slopecase = false;
                useracces = true;
                slopetime = 0;
                task().StoptAssist();
            }
            WheelTouched?.Invoke( this, new ValueChangeArgs<float>(pos) );
            flow.VAL = 0;
            valence( Relative ).SetDirty( ValenceFieldState.Flags.VAL );
        }

        private void Wheel_MouseDown( object sender, MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Left ) {
                Wheel_StartInteract( e.Location );
            }
        }

        private void Wheel_MouseLeave( object sender, EventArgs e )
        {
            if( useracces ) {
                Wheel.Capture = false;
                softrelease = 5;
                useracces = false;
                SlopeCase = true;
                WheelRelease?.Invoke( this, new ValueChangeArgs<float>( pos ) );
            }
        }

        private void Wheel_VierQuadrantenMouseMove( object sender, MouseEventArgs e )
        {
            Wheel_VierQuadrantenInteraction( e.Location );
        }

        private void Wheel_VierQuadrantenTouchMove( object sender, FingerTip e )
        {
            Wheel_VierQuadrantenInteraction( e.Position );
        }

        private void Wheel_VierQuadrantenInteraction( Point touchpoint )
        {
            if( softrelease > 0 ) {
                if( --softrelease == 0 ) {
                    Wheel.MouseMove -= Wheel_VierQuadrantenMouseMove;
                    TouchMove -= Wheel_VierQuadrantenTouchMove;
                    useracces = false;
                    quadrants = null;
                }
                Movement = touchPointQuads(touchpoint) / scaleFactor;
            } else {
                Position += touchPointQuads(touchpoint);
            }
        }


        private void Wheel_DirectionalMouseMove( object sender, MouseEventArgs e )
        {
            Wheel_DirectionalInteraction( e.Location );
        }
        private void Wheel_DirectionalTouchMove( object sender, FingerTip e )
        {
            Wheel_DirectionalInteraction( e.Position );
        }

        private void Wheel_DirectionalInteraction( Point touchpoint )
        {
            if ( softrelease > 0 ) {
                if ( --softrelease > 0 ) {
                    Wheel.MouseMove -= Wheel_DirectionalMouseMove;
                    TouchMove -= Wheel_DirectionalTouchMove;   
                    useracces = false;
                    quadrants = null;
                }
                Movement = touchPointQuads( touchpoint ) / scaleFactor;
            } else {
                Position = touchPointAngle( touchpoint ) - angleOffset;
            }
        }

        public void SetMidiMoveMode()
        {
            useracces = midiacces = true;
            if ( (!propellor.driver.Drive( WheelPropellor )) || slopecase ) {
                SlopeCase = false;
                MausPropellor = false;
                MidiPropellor = true;
                SlopeCase = true;
            }
        }

        private void CheckMidiMovement( object sender, EventArgs e )
        {
            if ( midiacces ) {
                midiacces = false;
                softrelease = 4;
            } else if( --softrelease <= 0 ) {
                SlopeCase = useracces = false;
                MausPropellor = true;
                MidiPropellor = false;
            }
        }

        private bool slopefunction()
        {
            Position += ( (Movement * DampFactor) * scaleFactor );
            return Math.Abs(Movement) > IsDownBelow;
        }

        private bool MidiPropellor, MausPropellor;
        private void WheelPropellor()
        {
            if( MausPropellor ) {
                Slope_MouseMove( propellor, null );
            } else if ( MidiPropellor ) {
                CheckMidiMovement( propellor, null );
            }
        }
        
        private void Slope_MouseMove( object sender, EventArgs e )
        {
            if ( softrelease > 0 ) {
                if(--softrelease <= 0) {
                    if( interaction == InteractionMode.FlowByQuadrants ) {
                        Wheel.MouseMove -= Wheel_VierQuadrantenMouseMove;
                        TouchMove -= Wheel_VierQuadrantenTouchMove;
                    } else {
                        Wheel.MouseMove -= Wheel_DirectionalMouseMove;
                        TouchMove -= Wheel_DirectionalTouchMove;
                    }
                    Wheel.MouseLeave -= Wheel_MouseLeave;
                    useracces = false;
                    quadrants = null;
                }
            } else {
                if( SlopeCase = slopefunction() ) {
                    slopetime += (uint)propellor.driver.Speed;
                }
            }
        }

        
        private void Wheel_MouseUp( object sender, MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Left ) {
                softrelease = 3;
                WheelRelease?.Invoke( this, new ValueChangeArgs<float>( pos ) );
                SlopeCase = true;
            }
        }

#region Valence Interface
        private ValenceField<Controlled.Float32,JogDialValence> joints;
        Action IInterValuable.getInvalidationTrigger() { return valuesUpdate; }
        ToolStripItemCollection IInterValuable.getMenuHook() { return mnu_config.Items; }
        public IControllerValenceField<Controlled.Float32> valence() { return joints.field(); }
        public IControllerValenceField<Controlled.Float32> valence(Enum which) { return joints.field(which); }
        IControllerValenceField IInterValuable.valence<cT>() { return joints.field(); }
        IControllerValenceField IInterValuable.valence<cT>(Enum which) { return joints.field(which); }
        void IInterValuable.callOnInvalidated(InvalidateEventArgs e) { OnInvalidated(e); }
        private void valuesUpdate() { Movement = flow; }
#endregion

#region TaskAssistence Interface 
        public ITaskAsistableVehicle<Action,Action> task() { return this; }
        public ITaskAssistor<Action, Action> assist { get { return propellor; } set { propellor = value as TaskAssist<SteadyAction, Action, Action>; } }
        int IAsistableVehicle<IActionDriver<Action,ILapFinish<Action>,Action>,ILapFinish<Action>>.StartAssist() { return propellor.assist.GetAssistence( propellor.action ); }
        int IAsistableVehicle<IActionDriver<Action,ILapFinish<Action>,Action>,ILapFinish<Action>>.StoptAssist() { return propellor.assist.ReleaseAssist( propellor.action ); }
#endregion

#region Midi Interface
        private int    messageReadCount = 0;
        private Action messageLoopTrigger;
        AutomationlayerAddressat[] IAutomat<MidiInOut>.channels {
            get { Message.Filter filter = midi().MessageFilter;
                return new AutomationlayerAddressat[] {
                    new AutomationlayerAddressat( filter.from, (byte)filter.loTy,
                                                  filter.till, (byte)filter.Chan )
                };
            }
        }
        public MidiInOut midi() {
            return midiIO == null 
                 ? midiIO = new MidiInOut()
                 : midiIO;
        }

        public Win32Imports.Midi.Value MidiValue {
            get { return new Win32Imports.Midi.Value((int)((Position / 360.0f) * 127.0f)); }
            set { Position = value.getProportionalFloat() * 360; }
        }
        void IMidiControlElement<MidiInOut>.OnIncommingMidiControl( object sender, Message value ) {
            this.MidiValue = new Win32Imports.Midi.Value( (short)value.Value );
        }
        MidiInputMenu<MidiInOut> IMidiControlElement<MidiInOut>.inputMenu { get; set; }
        MidiOutputMenu<MidiInOut> IMidiControlElement<MidiInOut>.outputMenu { get; set; }
            private void readMessageQueue()
            {
                bool read = midi().input().messageAvailable();
                if( !read ) { if( --messageReadCount <= 0 ) task().assist.ReleaseAssist( messageLoopTrigger ); } else {
                    midi().input().ProcessMessageQueue( this, new EventArgs() );
                    messageReadCount = 10;
                }if( read ) Invalidate();
            }

            // called by some outer thread driving the massage pump on incomming midi message to let start
            // applications own thread polling from the queue which the pump actually is filling up
            private void emptyMessageQueue()
            {
                if( messageReadCount == 0 ) {
                    messageReadCount = 10;
                    task().assist.GetAssistence( messageLoopTrigger );
                } else messageReadCount = 10;
            }

            protected void InitMidi( IContainer connector )
            {
                messageLoopTrigger = readMessageQueue;
                midi().InitializeComponent( this, connector, emptyMessageQueue );
                midi().input().AutomationEvent += midi().automat().OnIncommingMidiControl;
                messageReadCount = 0;
            }

            #endregion
        }
    }
}