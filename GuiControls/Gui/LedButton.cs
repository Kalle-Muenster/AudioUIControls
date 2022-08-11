using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Stepflow.Gui.Helpers;
using Stepflow.Gui.Automation;
using Stepflow.TaskAssist;
using Stepflow.Controller;
using Orientation = Stepflow.Gui.Orientation;
using Rectangle = System.Drawing.Rectangle;
using Resources = GuiControls.Properties.Resources;
using ButtonValenceField = Stepflow.Gui.ValenceField< 
        Stepflow.Controlled.Int8,
        Stepflow.Gui.LedButtonValence
    >;
using Stepflow.Gui.Geometry;

namespace Stepflow.Gui
{
    public partial class LedButton
        : UserControl
        , IInterValuable<Controlled.Int8>
        , ITaskAsistableVehicle<Action,Action>
        , IBasicTouchableElement<LedButton>
    {
        public const LedButtonValence StateField = LedButtonValence.State;
        public const LedButtonValence ChainField = LedButtonValence.Chain;

        public enum Transit {
            // OnTrigger means: button waits for some distinct condition 
            //                  to become true until it changes state.  
            OnRelease, OnPress, OnTrigger
        };

        /// <summary>
        /// Default enum value flags used by LedButtons when not configured individually with explict state values
        /// </summary>
        public enum Default : sbyte {
            NoState=0, ON=0x1, OFF=0x2, 
            Tic=0x4,  Tric=0x8, Trac=0x10,
            Flip=0x20, Flop=0x40, Flup=-128,
        CLICK=-1 };

        public Enum[] States = new Enum[MaxNumState] {
            Default.NoState,
            Default.ON,
            Default.OFF,
            Default.Tic,
            Default.Tric,
            Default.Trac,
            Default.Flip,
            Default.Flop,
            Default.Flup,
            Default.CLICK
        };

        private static Bitmap[]  background;
        private static Bitmap    buttonLeds;

//        private static SpriteSheet[] images;

        public const   sbyte    MaxNumState = 10;

        public event ValueChangeDelegate<Enum> Changed;
        public event ValueChangeDelegate<Enum> Pressed;
        public event ValueChangeDelegate<Enum> Release;
        public event ValueChangeDelegate<Enum> Waiting;

        private int                hover;
        private bool               autoText;
        private LedGlimmer         glimmer;
        private ButtonValenceField joints;

        private byte               highest;
        private System.Drawing.Rectangle          leDrect;
        private LED[]              led = new LED[MaxNumState];
        private bool               glimme = false;
        private int                tackte = 0;

        #region Impl: ITaskAssistable

        public ITaskAsistableVehicle<Action,Action> task() { return this; }
        ITaskAssistor<Action, Action> ITaskAsistableVehicle<Action, Action>.assist { get; set; }
        int IAsistableVehicle<IActionDriver<Action,ILapFinish<Action>,Action>,ILapFinish<Action>>.StartAssist()
        {
           #if DEBUG
            int placement = task().assist.GetAssistence( task().assist.action );  
            Consola.StdStream.Out.WriteLine( "DampfDruck now at {0} cylinders", placement );
            return placement;
#else
            return task().assist.GetAssistence( task().assist.action );   
#endif
        }
        int IAsistableVehicle<IActionDriver<Action,ILapFinish<Action>,Action>,ILapFinish<Action>>.StoptAssist()
        {
           #if DEBUG
            int placement = task().assist.ReleaseAssist( task().assist.action );
            Consola.StdStream.Out.WriteLine( "DampfStopt! no cylinders anymore", placement );
            return placement;
#else
            return task().assist.ReleaseAssist( task().assist.action );
#endif
        }
        #endregion

        #region Impl: IInterValuable
        private ValenceBondMenu<Controlled.Int8> mnu_bonds;
        private void stateUpdate() { State = States[state.VAL]; }
        protected IContainer getConnector() { return components; }
        ToolStripItemCollection IInterValuable.getMenuHook() { return mnu_config.Items; }
        Action IInterValuable.getInvalidationTrigger() { return stateUpdate; }
        void IInterValuable.callOnInvalidated(InvalidateEventArgs e) { OnInvalidated(e); }
        public IControllerValenceField<Controlled.Int8> valence() { return joints.field(); }
        public IControllerValenceField valence<cT>(Enum which) where cT : ControllerBase { return joints.field(which); }
        IControllerValenceField IInterValuable.valence<cT>() { return joints.field(); }
        IControllerValenceField<Controlled.Int8> IInterValuable<Controlled.Int8>.valence(Enum which) { return joints.field(which); }
#endregion

        private sbyte style;
        public Style Style {
            get { return (Style)style; }
            set { if( style != (sbyte)value ) {
                    style = (sbyte)value;
                    BackgroundImage = background[style+hover];
                //    BorderFrame.Image = hoverframe[style];
                    Label.ForeColor = value <= Style.Flat
                                             ? Color.FromKnownColor(KnownColor.ControlDarkDark)
                                             : Color.FromKnownColor(KnownColor.ControlLight);
                Invalidate(); }
            }
        }

        public override string Text {
            get { return Label.Text; }
            set { Label.Text = value;
                  AutoText = false;
                  Invalidate(false);
            }
        }

        public bool AutoText {
            get { return autoText; }
            set { if( autoText = value ) {
                    Label.Text = State.ToString();
                } else if( Tag != null ) {
                    Label.Text = Tag.ToString();
                }
            }
        }

        private Transit mode;
        public  Transit Mode {
            get { return mode; }
            set { if( value != mode ) {
                    mode = value;
                    Invalidate(false);
                } 
            }
        }

        public float LedLevel {
            get { return glimmer.Lum; }
            set { glimmer.Pre = value;
                  Invalidate(false);
            }
        }

        public Color LedValue {
            get { return glimmer.Rgb; }
            set { glimmer.Rgb = value; }
        }

        public LED LedColor {
            get { return led[state]; }
            private set {
                if( glimmer.Led != value ) {
                    glimmer.Pre = ( 0.5f + SideChain * 0.5f ) *0.75f;
                    glimmer.Led = value;
                    glimme = true;
                    ImTackt();
                }
            }
        }

        public override Color ForeColor {
            get { return Label.ForeColor; }
            set { Label.ForeColor = value; }
        }

        static LedButton()
        {
#if DEBUG
            Consola.StdStream.Init( Consola.CreationFlags.TryConsole );
            Win32Imports.RETURN_CODE.SetLogOutWriter(Consola.StdStream.Out.WriteLine);
#endif
            Valence.RegisterIntervaluableType<Controlled.Int8>();
            Valence.RegisterIntervaluableType<Controlled.Float32>();

            //images = SpriteSheet.loadSheetFromXml( Stepflow.Properties.Resources.button_leds_xml );
            buttonLeds = Resources.LedButton_LEDs;
            background = new Bitmap[6] { Resources.LedButton_Flat,
                                         Resources.button_hover_Lite,
                                         Resources.button_hover_Dark,
                                         Resources.button_hover_Flat,
                                         Resources.LedButton_Lite,
                                         Resources.LedButton_Dark };
            TaskAssist<SteadyAction,Action,Action>.Init( 60 );

            if( !PointerInput.isInitialized() ) {
                PointerInput.AutoRegistration = AutoRegistration.Enabled;
            }
        }

        public LedButton()
        {
            hover = 0;
            mode = Transit.OnRelease;
            state = new Controlled.Int8(0,9,0,ControlMode.None);
            state.VAL = 2;
            state.Mode = ControlMode.Element;
            unsafe {
                *(bool*)state.GetPin((int)ElementValue.UNSIGNED).ToPointer() = true;
                *(bool*)state.GetPin((int)ElementValue.CYCLED).ToPointer() = true;
            }
            state.SetCheckAtSet();
            state.Active = true;

            glimmer = new LedGlimmer( buttonLeds, 1 );
            glimmer.SetSheet(0);
            glimmer.Pre = 1.0f;
            glimmer.Lum = 1.0f;
            for( int i=0; i<8; ++i ) {
                glimmer.SetSource( (LED)i, 0, i * 32, 32, 32 );
            }

            side = new RangeController();
            side.MIN = 0;
            side.MAX = 100;
            side.MOV = -1;
            side.VAL = 1;
            side.SetCheckAtSet();
            side.Active = true;
            joints = new ButtonValenceField( this,
                     new ControllerBase[2] { state, side } );

            led[0] = LED.off;
            led[1] = LED.Green;
            led[2] = LED.Red;

            for ( int i = 3; i < MaxNumState; ++i )
                led[i] = LED.EMPTY;
            
            led[9] = LED.Orange;
            highest = 3;

            style = 1;
            Enabled = true;
            Visible = true;
            clicked = false;
            task().assist = new TaskAssist<SteadyAction,Action,Action>( this, ImTackt, 60 );
            touche_inter_patsche = new BasicTouchHandler<LedButton>( this );


            InitializeComponent();

            leDrect.X = Width / 4;
            leDrect.Width = Width / 2;
            leDrect.Height = Height / 2;

            SideChain = 0.95f;
            mnu_bonds = new ValenceBondMenu<Controlled.Int8>( this, getConnector() );

            if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                if( PointerInput.Dispatcher == null ) {
                    PointerInput.Initialized += (this as ITouchableElement).TouchInputReady;
                } else
                    PointerInput.Dispatcher.RegisterTouchableElement(this);
            }

        }



        public new void Dispose()
        {
            Valence.UnRegisterIntervaluableElement( this );
            PointerInput.Dispatcher.UnRegisterTouchableElement( this );
            base.Dispose();
        }

        private bool clicked;
        private bool intermediate {
            get { return clicked; }
            set { if( value != clicked ) {
                    if ( value && (Waiting != null) )
                        Waiting( this, new ValueChangeArgs<Enum>(States[state.VAL]) );
                    clicked = value;
                }
            }
        }

        private RangeController side;
        public float SideChain {
            get { return side; }
            set { side.VAL = value;
                valence<RangeController>( ChainField ).SetDirty( ValenceFieldState.Flags.VAL|ValenceFieldState.Flags.MOV );
            }
        }

        public bool ConnectSideChain( ControllerBase othervalue )
        {
            return side.Join( othervalue );
        }

        protected Controlled.Int8 state;
        public Enum State {
            get { return States[state.VAL]; }

            set { sbyte newValue = state.VAL; 
                Type valTyp = value.GetType();
                bool change = (valTyp != States[newValue].GetType());
                if( !change ) change = value.CompareTo( States[newValue] ) != 0;
                if ( change ) {
                    for(int i=0;i<MaxNumState;++i ) {
                        if ( valTyp == States[i].GetType() )
                            if( value.CompareTo( States[i] ) == 0 ) {
                                newValue = (sbyte)i;
                                change = false;
                     break; }
                    } if( change )
                        newValue = 0;
                    if( autoText ) Label.Text = States[newValue].ToString();
                    state.VAL = newValue;
                    valence<Controlled.Int8>( StateField ).SetDirty( ValenceFieldState.Flags.VAL );
                    Changed?.Invoke( this, new ValueChangeArgs<Enum>( States[newValue] ) );
                    LedColor = led[newValue];
                }
            }    
        }

        public bool Hovered {
            get { return hover == 3; }
            private set {
                if ( value != Hovered ) {
                    hover = value ? 3 : 0;
                    BackgroundImage = background[style+hover];
                    Invalidate();
                } hovered = value;
            }
        }

        public long Value {
            get { return State.ToValue(); }
        }

        public int Index {
            get { return state; }
        }

        public byte NumberOfStates {
            get { return (byte)(highest - 1); }
            set { highest = ++value < MaxNumState 
                          ?   value : highest; }
        }

        public static implicit operator bool( LedButton cast ) {
            return ( cast.state.VAL % 2 ) > 0;
        }

        public void SetUp( LED ON, LED OFF, LED CLICK )
        {
            highest = 1;
            if ( (int)ON < 8 )
                highest += 1;
            if ( (int)OFF < 8 )
                highest += 1;

            led[(int)Default.NoState] = LED.off;
            led[(int)Default.ON] = ON;
            led[(int)Default.OFF] = OFF;
            led[9] = CLICK;
        }

        public void DefineState( int index, Enum value, LED color )
        {
            if( led[9] != LED.off && led[9] == color ) return; 
            if( index > 0 && index < 9 ) {
                States[index] = value;
                led[index] = color;
                if (NumberOfStates < index) {
                    NumberOfStates = (byte)index;
                }
            }
        }

        public bool SetUpState( LED color, Enum value )
        {
            if (led[9] == color) return false;
            int index = NumberOfStates;
            if ((index <= 0 || index >= 8)) return false;
            Type et = value.GetType();
            bool accept = true;
            for ( int i = 1; i<index; ++i ) {
                if (led[i] == color) { index = i-1; break; }
                if (States[i] == (Enum)LED.EMPTY) { index = i-1; break; }
                if (et == States[i].GetType()) {
                    if( !(accept = value.ToValue() != States[i].ToValue()) )
                        break;
                }
            } if ( accept ) {
                if ( accept = (++index < 9) ) {
                    States[index] = value;
                    led[index] = color;
                    if ( index >= NumberOfStates ) {
                        NumberOfStates = (byte)index;
                    }
                }
            } return accept;
        }

        public void DefineClick( LED color )
        {
            led[9] = color;
        }

        public void AddState( Enum newOrExistingState, LED newOrExistingColor )
        {
            bool found = false;
            byte brandNewState = 0;
            for( int i = 0; i < MaxNumState; ++i ) {
                if( newOrExistingState == States[i] ) {
                    led[i] = newOrExistingColor;
                    found = true;
                    brandNewState = (byte)i;
                    break;
                }
            } if( !found ) {
                for( int i=0;i< MaxNumState; ++i ) {
                    if( led[i] == newOrExistingColor ) {
                        States[i] = newOrExistingState;
                        found = true;
                        brandNewState = (byte)i;
                        break;
                    }
                }
            } if(!found) {
                if( highest < MaxNumState ) {
                    brandNewState = ++highest;
                } 
            } highest = brandNewState > highest
                      ? brandNewState : highest;
            if( brandNewState > 0 ) {
                States[brandNewState] = newOrExistingState;
                led[brandNewState] = newOrExistingColor;
            }
        }

        public void ResetButtonStates()
        {
            led[0] = LED.off;
            States[0] = Default.NoState;
            for (int i = 0; i < 8; ++i) {
                States[i + 1] = (Default)(1 << i);
                led[i + 1] = LED.EMPTY;
            } highest = 2;
            led[9] = LED.off;
            States[9] = Default.CLICK;
        }

        private Enum transition( Enum to )
        {
            switch( mode ) {
                default: return to;
            }
        }

        private void Transition( Transit byTrigger )
        {
            sbyte currentstate = state;
            Enum actualState = States[currentstate];
            if ( mode == byTrigger ) {
                while ( led[++currentstate] == LED.EMPTY );
                actualState = currentstate <= highest 
                          ? States[currentstate]
                          : States[currentstate=1];
            } else if ( byTrigger == Transit.OnPress ) {
                glimmer.Pre = ( 0.75f + SideChain * 0.25f ) * 0.75f;
                glimme = true;
                tackte = 4;
                ImTackt();
            }
            switch( byTrigger ) {
                case Transit.OnPress:
                    Pressed?.Invoke( this, actualState );
                    break;
                case Transit.OnRelease:
                    Release?.Invoke( this, actualState );
                    break;
            } State = actualState;
        }

        private void ImTackt()
        {
            if( glimme ) {
                task().StartAssist();
                glimme = false;
            } else { 
                if( glimmer.Pre >= SideChain ) {
                    glimmer.Pre = SideChain;
                    if( !clicked )
                        task().StoptAssist();
                } if( mode == Transit.OnRelease ) {
                    if( clicked ) {
                        if( tackte > 0 ) {
                            if( --tackte == 0 ) {
                                glimmer.Led = led[9];
                            }
                        }
                    }
                } Invalidate( false );
            }
        }

        private void processButtonPressed()
        {
            if ( !clicked ) {
                intermediate = true;
                Transition( Transit.OnPress );
            }
        } 

        private void processButtonReleased() 
        {
            if ( clicked ) {
                Transition( Transit.OnRelease );
                intermediate = false;
            }
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
                processButtonPressed();
        }

        protected override void OnMouseUp( MouseEventArgs e )
        {
            if (e.Button == MouseButtons.Left)
                processButtonReleased();
        }

        protected override void OnResize( EventArgs e )
        {
            base.OnResize(e);
            leDrect.X = Width / 4;
            leDrect.Width = Width / 2;
            leDrect.Height = Height / 2;
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            if( hovered != Hovered )
                Hovered = hovered;
            base.OnPaint(e);
            if( glimmer.Pre < SideChain )
                if( ( mode == Transit.OnRelease ) && (!clicked) )
                    glimmer.Pre += 0.033f;
                else if( mode == Transit.OnPress )
                    glimmer.Pre += 0.025f;
            glimmer.Lum = glimmer.Pre;
            glimmer.DrawSprite( e.Graphics, leDrect );
        }

        private void Label_MouseDown( object sender, MouseEventArgs e )
        {
            OnMouseDown( e );
        }

        private void Label_MouseUp( object sender, MouseEventArgs e )
        {
            OnMouseUp( e );
        }

        protected override void OnMouseEnter( EventArgs e )
        {
            Hovered = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        private bool hovered = false; 
        protected override void OnMouseLeave( EventArgs e )
        {
            hovered = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        private void Label_MouseEnter( object sender, EventArgs e )
        {
            Hovered = true;
        }

        private void Label_MouseLeave( object sender, EventArgs e )
        {
            hovered = false;
            Invalidate();
        }


        #region ITouchable

        private BasicTouchHandler<LedButton> touche_inter_patsche;

        public ITouchEventTrigger touch {
            get { return touche_inter_patsche; }
        }

        Control ITouchable.Element {
            get { return this; }
        }

        bool ITouchable.IsTouched {
            get { return touche_inter_patsche.IsTouched; }
        }

        void ITouchableElement.TouchInputReady( PointerInput touchdevice )
        {
            PointerInput.Initialized -= touch.element().TouchInputReady;
            touchdevice.RegisterTouchableElement(this);
        }

        event FingerTip.TouchDelegate IBasicTouchable.TouchDown {
            add { touche_inter_patsche.events().TouchDown += value; }
            remove { touche_inter_patsche.events().TouchDown -= value; }
        }

        event FingerTip.TouchDelegate IBasicTouchable.TouchLift {
            add { touche_inter_patsche.events().TouchLift += value; }
            remove { touche_inter_patsche.events().TouchLift -= value; }
        }

        event FingerTip.TouchDelegate IBasicTouchable.TouchMove {
            add { touche_inter_patsche.events().TouchMove += value; }
            remove { touche_inter_patsche.events().TouchMove -= value; }
        }


        BasicTouchHandler<LedButton> IBasicTouchableElement<LedButton>.handler()
        {
            return touche_inter_patsche;
        }

        void IBasicTouchable.OnTouchDown( FingerTip tip ) {
            processButtonPressed();
        }

        void IBasicTouchable.OnTouchMove( FingerTip tip )
        {}

        void IBasicTouchable.OnTouchLift( FingerTip tip ) {
            processButtonReleased();
        }

        Point64 ITouchableElement.ScreenLocation()
        {
            return PointToScreen( Point.Empty );
        }

        IRectangle ITouchableElement.ScreenRectangle()
        {
            return AbsoluteEdges.FromRectangle( RectangleToScreen(new Rectangle(0, 0, Width, Height)) );
        }

        ITouchDispatchTrigger ITouchable.screen()
        {
            return touche_inter_patsche.screen();
        }

        ITouchableElement ITouchable.element()
        {
            return this;
        }

        #endregion

    }
}
