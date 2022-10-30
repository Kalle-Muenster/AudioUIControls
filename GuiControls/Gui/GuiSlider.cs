//#define IMPLEMENT_AS_MIDI_CONTROL

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Stepflow;
using Stepflow.Gui;
using Stepflow.TaskAssist;
using Stepflow.Gui.Geometry;
using Stepflow.Gui.Helpers;
using Stepflow.Gui.Automation;
using Stepflow.Controller;

using Resources = GuiControls.Properties.Resources;
using Orientation = Stepflow.Gui.Orientation;
using Style = Stepflow.Gui.Style;
#if DEBUG
using Std = Consola.StdStream;
#endif
using Rectangle = System.Drawing.Rectangle;
using R = Stepflow.Gui.Geometry.RECT;
using P = Stepflow.Gui.Geometry.Point64;
using p = Stepflow.Gui.Geometry.Point32;
using Win32Imports.Touch;


namespace Stepflow.Gui
{
    public partial class GuiSlider
        : UserControl
        , IInterValuable<Controlled.Float32>
        , ITouchGesturedElement<GuiSlider>
        , ITaskAsistableVehicle<Action,Action>
    {
        public enum InteractionMode { Linear, XFactor, Directional, YpsFactor }
        public enum DefaultMarkers { Minimum = 0, Center = 1, Maximum = 2 }
        public enum LEDSource { OwnValue, SideChain }

        public delegate void MarkerPassedDelegate( object sender, MarkerPassedEventArgs marker );

        public event ValueChangeDelegate<float> MovementFast;
        public event ValueChangeDelegate<float> ReleasedKnob;
        public event MarkerPassedDelegate       MarkerPassed;
        public event ValueChangeDelegate<float> ValueChanged;

        private static Bitmap[]                               images;
        private static IRectangle[][][][]                     source;

        private SpriteSheet                                   bgimg;
        private SpriteSheet                                   nippl;                               
        private Matrix                                        rotate;
        private ValenceField<Controlled.Float32,ValenceField> joints;
        private Controlled.Float32                            value;
        private List<MarkerPassedEvent>                       marker;
        private float                                         fastmove;
        private int                                           lastMouse;
        private LedGlimmer                                    glimmer;
        protected TaskAssist<SteadyAction,Action,Action>      taskassist; 

        private ValenceBondMenu<Controlled.Float32>           mnu_valene;
        ToolStripItemCollection IInterValuable.getMenuHook() { return mnu_context.Items; }
        public IControllerValenceField<Controlled.Float32> valence() { return joints.field(); }
        public IControllerValenceField<Controlled.Float32> valence(Enum which){return joints.field(which);}
        IControllerValenceField IInterValuable.valence<cT>( Enum which ) { return joints.field( which ); }
        IControllerValenceField IInterValuable.valence<cT>(){ return joints.field(); }
        Action IInterValuable.getInvalidationTrigger() { return valueUpdate; }
        void IInterValuable.callOnInvalidated( InvalidateEventArgs e ) { OnInvalidated(e); }
        private void valueUpdate() { TriggerEvents(); Invalidate(); }

        internal protected ITouchGesturedElement<GuiSlider> getInTouch() { return this; }

        public interface IControlMarker
        {
            float  Value { get; }
            float  Speed { get; }
            int    Index { get; }
            Enum   Endex { get; }
            string Named { get; }
            MarkerPassedEventArgs eArgs { get; }
        }

        public abstract class MarkerPassedEventArgs
            : EventArgs
            , IControlMarker
        {
            public static readonly new IControlMarker Empty = new MarkerPassedEventArgs<string>( float.NaN, string.Empty, null );

            internal protected GuiSlider slide;
            internal protected float     value;
            internal protected float     speed;

            public MarkerPassedEventArgs( GuiSlider markerElement, float markerPosition )
                : base() {
                value = markerPosition;
                slide = markerElement;
                speed = 0;
            }
            public float Value { get { return value; } }
            public float Speed { get { return speed; } }
            public MarkerPassedEventArgs eArgs { get { return this; } }
            public int   Index {
                get { int ordinal = 0;
                    IControlMarker find = slide.NextMarkerNearest( slide.Minimum );
                    while( find.Value != value ) { ++ordinal;
                        find = slide.NextMarkerAbove( find.Value );
                    } return ordinal;
                }
            }
            abstract public Enum   Endex { get; }
            abstract public string Named { get; }
        }

        public abstract class MarkerPassedEvent
            : MarkerPassedEventArgs
        {
            public event MarkerPassedDelegate Passed;
            public MarkerPassedEvent( GuiSlider inst, float mark )
                : base(inst,mark) {
                Passed = null;
            }
            internal void pass( float speedOfSliderWhenPassingTheMark ) {
                if( speed != speedOfSliderWhenPassingTheMark ) {
                    speed = speedOfSliderWhenPassingTheMark;
                    Passed?.Invoke( slide, this );
                }
            }
        }

        public class MarkerPassedEventArgs<IdxTy> 
            : MarkerPassedEvent, IComparable
            where IdxTy : IConvertible
        {
            internal protected IdxTy index;

            public MarkerPassedEventArgs( float markerPosition, IdxTy markerIndex, GuiSlider markedElement )
                : base( markedElement, markerPosition ) {
                index = markerIndex;
            }

            override public string Named {
                get { return index.ToString(); }
            }
            override public Enum Endex {
                get{ if( index is Enum )
                    return (Enum)Enum.ToObject( index.GetType(), index.ToInt32( Resources.Culture ) );
                else if( value == slide.Minimum )
                    return DefaultMarkers.Minimum;
                else if( value == slide.Maximum )
                    return DefaultMarkers.Maximum;
                else if( value == (slide.ValueRange * 0.5f - slide.Minimum) )
                     return DefaultMarkers.Center;
                else return (Enum)Enum.ToObject(typeof(DefaultMarkers),Index); }
            }
            int IComparable.CompareTo( object obj ) {
                float other = (obj as MarkerPassedEvent).Value;
                return other < value ? 1 : other > value ? -1 : 0;
            }

            public class Event
                : MarkerPassedEventArgs<IdxTy>
            {
                internal Event( IdxTy name, float mark, GuiSlider inst )
                    : base( mark, name, inst ) {
                }
            }
        }

        internal MarkerPassedEvent CreateMarkerEvent( float mark, string name )
        {
            return new MarkerPassedEventArgs<string>.Event( name, mark, this );
        }

        internal MarkerPassedEvent CreateMarkerEvent( float mark, Enum name )
        {
            return new MarkerPassedEventArgs<Enum>.Event( name, mark, this );
        }

        protected MarkerPassedEventArgs CreateDefaultMarker( DefaultMarkers which )
        {
            float mark = 0;
            switch ( which ) {
                case DefaultMarkers.Minimum: mark = value.MIN; break;
                case DefaultMarkers.Center: mark = Center; break;
                case DefaultMarkers.Maximum: mark = value.MAX; break;
            } return new MarkerPassedEventArgs<DefaultMarkers>(
                mark, which, this
            );
        } 

        protected virtual void OnValueChanged( float changed )
        {
            ValueChanged?.Invoke( this, new ValueChangeArgs<float>( changed ) );
        } 

        protected virtual void OnMarkerPassed( MarkerPassedEvent marke, float bySpeed )
        {
            marke.pass( bySpeed );
            MarkerPassed?.Invoke( this, marke.eArgs );
        }

        private void TriggerEvents()
        {
            float l = LastValue;
            bool st = value.Check();
            float v = value.VAL;

            if ( l != v ) {
                OnValueChanged( v );
            }
        
            float speedOfMove = value.MOV;
            if ( MovementFast != null && fastmove != 0 )
                if ( Math.Abs( speedOfMove ) >= fastmove ) {
                    MovementFast( this, speedOfMove );
                }    
            for ( int i = 0; i < marker.Count; ++i ) {
                MarkerPassedEvent trigger = marker[i];
                if ( speedOfMove != trigger.speed ) {
                    if( (l < trigger.value && v >= trigger.value) || (l > trigger.value && v <= trigger.value) ) {
                        OnMarkerPassed( trigger, speedOfMove );
                    }
                }
            }
            value.Active = st;
        }
        
        private void NuppsiUpdate()
        {
            switch( Orientation ) {
                case Orientation.Horizontal: {
                    Nuppsi.Left = (int)( (Inverted? 1.0-Proportion : Proportion) * PixelRange );
                } break;
                case Orientation.Vertical: {
                    Nuppsi.Top = (int)( (Inverted? 1.0-Proportion : Proportion) * PixelRange );
                } break;
            } Invalidate();
        }

        private void NuppsiResize()
        {
            Nuppsi.Anchor = AnchorStyles.None;
            switch( Orientation ) {
                case Orientation.Rondeal: {
                    Nuppsi.Location = Point.Empty;
                    setNupsiMipmap( Nuppsi.Width = this.Width-mipmap );
                    Nuppsi.Height = Nuppsi.Width;
                    Nuppsi.Anchor = AnchorStyles.Left | AnchorStyles.Right
                                  | AnchorStyles.Top  | AnchorStyles.Bottom;
                    Invalidate();
                } break;
                case Orientation.Horizontal: {
                    setNupsiMipmap( this.Height );
                    Nuppsi.Height = this.Height - ((Nuppsi.Top = (mipmap)) + mipmap);
                    Nuppsi.Width  = (int)( this.Height / 1.6f );
                    Nuppsi.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                    NuppsiUpdate();
                } break;
                case Orientation.Vertical: {
                    setNupsiMipmap( this.Width );
                    Nuppsi.Width  = this.Width - ((Nuppsi.Left = (mipmap)) + mipmap);
                    Nuppsi.Height = (int)( this.Width / 1.6f );
                    Nuppsi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                    NuppsiUpdate();
                } break;      
            } 
        }

        private int mipmap;
        private void setNupsiMipmap( int newsize ) {
            mipmap = newsize >= 20
                   ? newsize < 25 
                   ? 1 : 2 : 0;
        }
        
        //////////////////////////////////////////////////////////////////////////
        // event handlers

        private void OnMovementFast( object sender, ValueChangeArgs<float> value )
        {
            style = (sbyte)(style|0x02);
        }

        protected void OnResize(  object sender,  EventArgs e )
        {
            NuppsiResize();
        }

        private void OnToolStripMenuItemClick( object sender, EventArgs e )
        {
            value.Active = false;
            switch ( int.Parse((sender as ToolStripMenuItem).Tag.ToString()) ) {
                case 0: Value = NextMarkerBelow(value).Value; break;
                case 1: Proportion = 0.5f; break;
                case 2: Value = NextMarkerAbove(value).Value; break;
            } value.Active = true;
            NuppsiUpdate();
        }

        /////////////////////////////////////////////////////////////////
       // Mouse interactions:

        private float directionalMode(int mausX, int mausY)
        {
            float cX = Nuppsi.Location.X + Nuppsi.Width/2;
            float cY = Nuppsi.Location.Y + Nuppsi.Height/2;
            cX = mausX - cX; cY = mausY - cY; double angle;
            if( cX < 0 ) {
                cY = cY == 0 ? float.Epsilon : cY;
                angle = (270 - (Math.Atan(cX / cY) * 180) / Math.PI) 
                      + ((cY > 0)? -90 : 90);
            } else {
                cX = cX == 0 ? float.Epsilon : cX;
                angle = 90+(Math.Atan(cY/cX)*180)/Math.PI;
            } angle = ((angle + (Cycled ? 0 : 135)) % 360) 
                    / PixelRange;
            if( Inverted ) angle = 1.0 - angle;
            return (float)angle;
        }

        private void Nuppsi_Move( object sender, EventArgs evarg )
        {
            Point e = (evarg is MouseEventArgs)
                    ? (evarg as MouseEventArgs).Location
                    : (Point)(evarg as FingerTip).Position;

            if( Interaction == InteractionMode.Directional ) {
                Proportion = directionalMode( e.X, e.Y );
            return; }
            float fast = ( orientation == 1 ? e.X : e.Y );
            float move = ( (fast-lastMouse) / PixelRange )
                         * ValueRange * invertor;
            if( orientation == 0 ) {
                lastMouse = (int)fast;
                if( interaction == InteractionMode.YpsFactor )
                fast = Math.Abs(1.0f /
                      (0.05f * ( (float)e.Y
                               - ((float)Nuppsi.Height / 2.0f
                               + (float)Nuppsi.Location.Y) ) )
                ); else
                fast = Math.Abs(1.0f /
                      (0.05f * ( (float)e.X
                               - ((float)Nuppsi.Width / 2.0f
                               + (float)Nuppsi.Location.X) ) )
                ); move *= -fast; 
            } fast = (Value + move);
            if( !( float.IsNaN(fast)
                || float.IsInfinity(fast)) ) {
                Value = fast;
            }
        }

        private void Nuppsi_MouseDown( object sender, EventArgs evarg )
        {
            if (!(evarg is FingerTip)) {
                MouseEventArgs e = evarg as MouseEventArgs;
                Nuppsi.MouseMove += Nuppsi_Move;
                lastMouse = orientation == 0 ? e.X : e.Y;
            } else {
                FingerTip e = evarg as FingerTip;
                TouchMove += Nuppsi_Move;
                lastMouse = orientation == 0 ? e.X : e.Y;
            }
        }

        private void Nuppsi_MouseUp( object sender, EventArgs evarg )
        {
            if( !(evarg is FingerTip) ) {
                MouseEventArgs e = evarg as MouseEventArgs;
                if ( e.Button == MouseButtons.Left ) {
                    Nuppsi.MouseMove -= Nuppsi_Move;
#if DEBUG
                    Consola.StdStream.Out.WriteLine("FaderRelease");
#endif
                    if( ReleasedKnob != null )
                        ReleasedKnob(this,new ValueChangeArgs<float>(value));
                }
            } else {
                FingerTip e = evarg as FingerTip;
                if( touch.hasFinger( e.Id ) ) {
                    this.TouchMove -= Nuppsi_Move;
#if DEBUG
                    Consola.StdStream.Out.WriteLine("FaderRelease");
#endif
                    if( ReleasedKnob != null )
                        ReleasedKnob(this,new ValueChangeArgs<float>(value));
                }
            }
        }


        private P l1,l2;
        private void GuiSlider_TouchDown( object sender, FingerTip tip )
        {
            if( tip.HasFlags( IsTouching.Prime ) ) {
                
                if( Orientation == Orientation.Rondeal ) {
                    l1 = tip.Position;
                    l2 = tip.NextHere.Position;
                }
            } TouchMove += GuiSlider_TouchMove;
        }

        private void GuiSlider_TouchLift( object sender, FingerTip tip )
        {
            if( !tip.HasFlags( IsTouching.TrackKept ) )
                this.TouchMove -= GuiSlider_TouchMove;
        }

        private void GuiSlider_TouchMove( object sender, FingerTip tip )
        {
            float range = 0;
            switch( Orientation ) {
                case Orientation.Rondeal:
                    if( tip.Count == 2 ) {
                        P p1 = tip.Position;
                        P p2 = tip.NextHere.Position;
                        P pC = p1+((p2-p1)*0.5f);
                        p mv = new p(p1-l1);
                        float mC = 0;
                        P dC = pC - p1;
                        float vDy = (float)dC.x / (dC.x+dC.y);
                        float vDx = (float)dC.y / (dC.x+dC.y);
                        if( p1.x < pC.x ) {
                            mC -= (mv.y*vDy);
                        } else {
                            mC += (mv.y*vDy);
                        }
                        if( p1.y < pC.y ) {
                            mC += (mv.x*vDx); 
                        } else {
                            mC -= (mv.x*vDx);
                        } mv = new p(p2-l2);
                        if( p1.x < pC.x ) {
                            mC -= (mv.y*vDy);
                        } else {
                            mC += (mv.y*vDy);
                        }
                        if( p1.y < pC.y ) {
                            mC += (mv.x*vDx); 
                        } else {
                            mC -= (mv.x*vDx);
                        }
                        range = Proportion + (( ((mC/(dC.x+dC.y))*90) / PixelRange) * invertor);
                        l1 = p1;
                        l2 = p2;
                    }
                    break;
                case Orientation.Horizontal:
                    range = Inverted
                          ? 1.0f-((float)tip.Position.x / PixelRange)
                          : (float)tip.Position.x / PixelRange;
                    break;
                case Orientation.Vertical:
                    range = Inverted
                          ? 1.0f-((float)tip.Position.y / PixelRange)
                          : (float)tip.Position.y / PixelRange;
                    break;
            } Proportion = range;
        }


        //////////////////////////////////////////////////////////////
        // Drawing:

        private IRectangle leDrect() {
            CenterAndScale ledrect = CenterAndScale.FromRectangle( Nuppsi.Bounds );
            ledrect.Scale -= new p( mipmap + mipmap, mipmap + mipmap );
            if ( Style == Style.Flat ) {
                ledrect.Scale.x = ledrect.Scale.y = (short)(
                    ( ledrect.Scale.x < ledrect.Scale.y
                    ? ledrect.Scale.x : ledrect.Scale.y ) / 2 + 1 );
            } return ledrect; 
        }

        private void paintBackground( Graphics g )
        {
            IRectangle draw;
            switch( Orientation ) {
                case Orientation.Rondeal: {
                    draw = Rectangle<CenterAndScale>.Create(StorageLayout.CornerAndSizes,0,0,Width,Height);
                    draw.Scale -= new p( Width / 16, Width / 16 );
                    bgimg.GetSprite( 0 ).Draw( g, draw );
                } break;
                case Orientation.Horizontal: {
                    draw = new CornerAndSize(0, 0, Width, Height);
                    short offset = (short)( Height/2 );
                    draw.W = offset;
                    bgimg.GetSprite( 1 ).Draw( g, draw );
                    draw.X += (Width - offset);
                    bgimg.GetSprite( 3 ).Draw( g, draw );
                    draw.W = Width - (offset + offset); draw.X = offset;
                    bgimg.GetSprite( 2 ).Draw( g, draw );
                } break;
                case Orientation.Vertical: {
                    draw = new CornerAndSize(0, 0, Width, Height);
                    short offset = (short)(Width/2);
                    draw.H = offset;
                    bgimg.GetSprite( 1 ).Draw( g, draw );
                    draw.Y += (Height - offset);
                    bgimg.GetSprite( 3 ).Draw( g, draw );
                    draw.H = Height - (offset + offset); draw.Y = offset;
                    bgimg.GetSprite( 2 ).Draw( g, draw );
                } break;
            }
        }

        private void paintRondael( Graphics g )
        {
            Rectangle a = Nuppsi.Bounds;
            float  angle = (Inverted ? (1.0f-Proportion) : Proportion) * PixelRange
                         - (Cycled ? 0 : 135);

            GraphicsState pushed = g.Save();

                rotate.Reset();
                rotate.RotateAt( angle, new PointF( (float)Width/2.0f, (float)Height/2.0f ) );
                rotate.Multiply( g.Transform );
                g.Transform = rotate;

                nippl.GetSprite( mipmap ).Draw( g, a );

                if( !glimmer.Off ) {
                    a.Width /= 3; a.X += a.Width; a.Height /= 2;
                    glimmer.DrawSprite( g, a );
                }

            g.Restore( pushed );
        }

        private void paintLinear( Graphics g )
        {
            IRectangle ledRect = leDrect();
            glimmer.DrawBrush( g, ledRect );

            nippl.GetSprite( mipmap ).Draw( g, Nuppsi.Bounds );

            if( !glimmer.Off ) {
                glimmer.DrawSprite( g, ledRect );
            }
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            base.OnPaint( e );
            paintBackground( e.Graphics );
            glimmer.Lum = m_ledSource == LEDSource.OwnValue
                        ? Proportion : SideChain;

            if( Orientation == Orientation.Rondeal ) {
                paintRondael( e.Graphics );
            } else {
                paintLinear( e.Graphics );
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        // public properties:

        private int orientation;
        public Orientation Orientation {
            get { return (Orientation)orientation; }
            set { if ( (int)value != orientation ) {
                    if ( value != Orientation.Rondeal ) {
                        Nuppsi.Cursor = value == Orientation.Horizontal
                                      ? Cursors.VSplit
                                      : Cursors.HSplit;
                        if ( orientation != (int)Orientation.Rondeal ) {
                            Inverted = !Inverted;
                            int tempsize = this.Height;
                            this.Height = this.Width;
                            this.Width = tempsize;
                        } else {
                            if( value == Orientation.Horizontal ) {
                                Width *= 2; Height /= 2;
                            } else {
                                Width /= 2; Height *= 2;
                            }
                        }
                    } else if ( orientation==(int)Orientation.Horizontal ) {
                        Width = Height = (Height*2);
                    } else {
                        Width = Height = (Width*2);
                    } orientation = (int)value;
                    glimmer.SetSheet( orientation / 2 );
                    bgimg.SelectGroup( value );
                    nippl.SelectGroup( value );
                    NuppsiResize();
                    Invalidate( true );
                }
            }
        }

        private sbyte style;
        public Style Style {
            get { return (Style)(sbyte)style; }
            set { if ( style != (sbyte)value ) {
                    style = (sbyte)value;
                    glimmer.SetStyle( value );
                    bgimg.SelectLoop( value );
                    nippl.SelectLoop( value );
                    Invalidate();  
                }
            }
        }

        public LED LedColor {
            get { return glimmer.Led; }
            set { glimmer.Led = value;
                  Invalidate(); }
        }



        private LEDSource m_ledSource = LEDSource.OwnValue;
        public LEDSource LedSource {
            get { return m_ledSource; }
            set { if ( m_ledSource != value ) {
                    switch (value) {
                        case LEDSource.OwnValue:
                            task().StoptAssist();
                            glimmer.Lum = Proportion;
                            Invalidate();
                        break;
                        case LEDSource.SideChain:
                            glimmer.Lum = SideChain;
                            task().StartAssist();
                        break;
                    } m_ledSource = value;
                }
            }
        }

        private IntPtr cycleMode;
        public bool Cycled {
            get { unsafe { return *(bool*)cycleMode.ToPointer(); } }
            set { unsafe { *(bool*)cycleMode.ToPointer() = value; } }
        }

        private IntPtr unSigning;
        public bool Unsigned {
            get { unsafe { return *(bool*)unSigning.ToPointer(); } }
            set { unsafe { *(bool*)unSigning.ToPointer() = value; } }
        }

        private IntPtr autoClamp;
        public bool Clamped {
            get { unsafe { return *(bool*)autoClamp.ToPointer(); } }
            set { unsafe { *(bool*)autoClamp.ToPointer() = value; } }
        }

        private IntPtr peakValue;
        public float PeakValue {
            get { unsafe { return *(float*)peakValue.ToPointer(); } }
        }

        private IntPtr lastValue;
        private float LastValue {
            get { unsafe { return *(float*)lastValue.ToPointer(); } }
        }

        private MixAndFeel mixingBehavior;
        public MixAndFeel Behavior {
            get { return mixingBehavior; }
            set { if (value != mixingBehavior) {
                    if( ( mixingBehavior = value ) == MixAndFeel.Realistic ) {
                        MovementFast += OnMovementFast;
                        if( ThresholdForFastMovement == 0 )
                            ThresholdForFastMovement = ValueRange / 10; 
                    } else {
                        MovementFast -= OnMovementFast;
                    }
                }
            }
        }

        private InteractionMode interaction; 
        public InteractionMode Interaction {
            get { return Orientation == Orientation.Rondeal ? interaction : InteractionMode.Linear; }
            set { if( value != interaction && value != InteractionMode.Linear ) 
                      interaction = value; }
        }

        private int invertor;
        public bool Inverted {
            get { return invertor < 0; }
            set { if ( value != (invertor < 0) ) {
                    this.value.VAL = (ValueRange - (this.value.VAL - this.value.MIN)) + this.value.MIN;
                    invertor = value ? -1 : 1;
                    NuppsiUpdate();
                } 
            }
        }

        /// <summary>Readonly Property 'PixelRange'
        /// Returns a Sliders actual pixel range as count on pixels<parameter>
        /// its Orientation axis measures on the display, or when Orientation is
        /// Rondeal, the rotation angle (in degree) between min and max position
        /// </parameter> and <parameter>Maximum</parameter></summary>
        private int PixelRange {
            get { switch( orientation ) {
                  case 0: return Cycled ? 360 : 270;
                  case 1: return Width - Nuppsi.Width;
                  case 2: return Height - Nuppsi.Height;
                  } return 0;
            }
        }

        /// <summary>Readonly Property 'ValueRange'
        /// Returns a Sliders actual value range as distace between <parameter>
        /// Minimum</parameter> and <parameter>Maximum</parameter></summary>
        public float ValueRange {
            get { return (value.MAX - value.MIN); }
        }

        /// <summary>Property 'Proportion' (float between 0.0 and 1.0)
        /// Get (or set) a Slider's value in proportion to it's actual ValueRange
        /// </summary>
        public float Proportion {
            get { return (value.VAL-value.MIN) / ValueRange; }
            set { Value = ( ValueRange * value ) + this.value.MIN; }
        }

        private IntPtr side = IntPtr.Zero;
        private float  mult = 1.0f;
        public float SideChain {
            get { if( side == IntPtr.Zero ) return 0; unsafe { return mult * *(float*)side.ToPointer(); } }
            set { if( side != IntPtr.Zero ) unsafe { *(float*)side.ToPointer() = value / mult; } }
        }
        public void AttachSideChain( IntPtr ptr )
        {
            side = ptr;
            if ( ptr == IntPtr.Zero ) {
                LedSource = LEDSource.OwnValue;
            } else {
                mult = 1.0f;
                glimmer.Pre = 0.5f;
                while( SideChain > 2.0f ) mult *= 0.9f;
                LedSource = LEDSource.SideChain;
                glimmer.Lum = SideChain;
                task().StartAssist();
            }
        }
        public void AttachSideChain( ref float var )
        {
            unsafe { fixed(float* ptr = &var) {
                AttachSideChain( new IntPtr(ptr) );
            } }
        }

        private bool Symetrical {
            get { return this.value.MIN < 0 ? this.value.MAX == -this.value.MIN : this.value.MIN == 0; }
        }

        public float Value {
            get { return value; }
            set { this.value.VAL = value;
                  valence().SetDirty( ValenceFieldState.Flags.VAL
                                    | ValenceFieldState.Flags.MOV );
                  TriggerEvents();
                  NuppsiUpdate();
            }
        }

        public float Maximum {
            get { return value.MAX; }
            set { this.value.MAX = value;
                  valence().SetDirty( ValenceFieldState.Flags.MAX );
                  Invalidate( true ); }
        }

        public float Minimum {
            get { return value.MIN; }
            set { this.value.MIN = value;
                  valence().SetDirty( ValenceFieldState.Flags.MIN );
                  Invalidate( true ); }
        }
        public float Center {
            get { float central = value.MIN;
                 return central + ((value.MAX - central) / 2.0f);
            }
        }
        public float Movement {
            get { return value.MOV; }
        }

        public float ThresholdForFastMovement {
            get { return fastmove; }
            set { fastmove = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Construction:

        static GuiSlider()
        {
#if DEBUG
            Std.Init( Consola.CreationFlags.TryConsole );
            Win32Imports.RETURN_CODE.SetLogOutWriter(Std.Out.WriteLine);
#endif
            Valence.RegisterIntervaluableType<Controlled.Float32>();
            images = new Bitmap[4];

            source = new IRectangle[3][][][];
            SpriteSheet.Loader loader = new SpriteSheet.Loader( Resources.slider_complete_xml
                                                                );
            int elmcount = loader.elmCount();
            source[0] = new IRectangle[elmcount][][];
            source[1] = new IRectangle[elmcount][][];
            source[2] = new IRectangle[elmcount][][];
            for( Orientation o = Orientation.Rondeal; o <= Orientation.Vertical; ++o ) {
                int e = o.ToInt32();
                images[e] = loader.getImage( loader.makeXPath( o.ToString(),
                            SpriteSheet.Loader.ElmType.element ).ToString() );
                source[0][e] = new IRectangle[3][];
                source[1][e] = new IRectangle[3][];
                source[2][e] = new IRectangle[1][] { loader.getArray( o, "Led" ) };
                for( Style s = Style.Flat; s <= Style.Dark; ++s ) {
                    source[0][e][(int)s] = new IRectangle[o == Orientation.Rondeal ? 1 : 4];
                    source[0][e][(int)s][0] = loader.getSprite( o, s+".Blet.All" );
                    if( o > Orientation.Rondeal ) {
                    source[0][e][(int)s][1] = loader.getSprite( o, s+".Blet.Min" );
                    source[0][e][(int)s][2] = loader.getSprite( o, s+".Blet.Mid" );
                    source[0][e][(int)s][3] = loader.getSprite( o, s+".Blet.Max" );
                    } source[1][e][(int)s] = new IRectangle[3];
                    for( MipMap m = MipMap.Small; m <= MipMap.Large; ++m )
                    source[1][e][(int)s][(int)m] = loader.getSprite( o, s+".Nipple."+m );
                }
            } images[3] = Resources.slider_leds_png;
            
            TaskAssist<SteadyAction,Action,Action>.Init(60);
            if(!PointerInput.isInitialized() ) {
                PointerInput.AutoRegistration = AutoRegistration.Enabled;
            }
        }

        protected IContainer getConnector()
        {
            return components;
        }

        public GuiSlider()
        {       
            rotate = new Matrix();
            glimmer = new LedGlimmer( images[3], 0.75f );
            glimmer.SetSheet(0);
            for( int i = 0; i < 8; ++i  ) {
                glimmer.SetSource( (LED)i, source[2][1][0][i] );
            }
            glimmer.SetSheet(1);
            for( int i = 0; i < 8; ++i  ) {
                glimmer.SetSource( (LED)i, source[2][2][0][i] );
            }

            bgimg = new SpriteSheet( images, source[0] );
            nippl = new SpriteSheet( images, source[1] );
            bgimg.SetColor( Color.White );
            nippl.SetColor( Color.White );

            fastmove = 0;
            lastMouse = 0;
            mipmap = 2;
            invertor = 1;
            style = 1;
            glimmer.Pre = 0.75f;
            glimmer.Led = LED.off;
            orientation = (int)Orientation.Horizontal;
            marker = new List<MarkerPassedEvent>();
            value = new Controlled.Float32();
            value.SetUp(-1, 1, 0, 0, ControlMode.Element);
            lastValue = value.GetPin(ElementValue.LAST);
            peakValue = value.GetPin(ElementValue.PEAK);
            cycleMode = value.GetPin(ElementValue.CYCLED);
            unSigning = value.GetPin(ElementValue.UNSIGNED);
            autoClamp = value.GetPin(ElementValue.CLAMPED);
            value.Active = true;
            Clamped = true;
            joints = new ValenceField<Controlled.Float32,ValenceField>(
                                 this, new Controlled.Float32[]{value} );
            interaction = InteractionMode.XFactor;
            getInTouch().handler = new TouchGesturesHandler<GuiSlider>(this);
            InitializeComponent();
            Resize += OnResize;
            BorderStyle = BorderStyle.None;
            glimmer.SetStyle( Style );
            glimmer.SetModus( LedGlimmer.Mode.AutoBrushSelect );
            bgimg.SelectLoop( Orientation, Style );
            nippl.SelectLoop( Orientation, Style );
            mnu_valene = new ValenceBondMenu<Controlled.Float32>( this, getConnector() );

            if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                if( PointerInput.Dispatcher == null ) {
                    PointerInput.Initialized += (this as ITouchableElement).TouchInputReady; 
                } else
                    PointerInput.Dispatcher.RegisterTouchableElement( this );
            }
            Nuppsi.MouseDown += Nuppsi_MouseDown;
            Nuppsi.MouseUp += Nuppsi_MouseUp;
            taskassist = new TaskAssist<SteadyAction,Action,Action>( this, flashPoint, 60 );

            TouchDown += GuiSlider_TouchDown;
            TouchLift += GuiSlider_TouchLift;

            Load += AdjustSpriteColor;

            Disposed += GuiSlider_Disposed;
        }

        private void GuiSlider_Disposed( object sender, EventArgs e )
        {
            Valence.UnRegisterIntervaluableElement( this );
            PointerInput.Dispatcher?.UnRegisterTouchableElement( this );
        }

        private void AdjustSpriteColor( object sender, EventArgs e )
        {
            Color bg = Parent.FindForm().BackColor;
            base.BackColor = bg;
            base.ForeColor = bg;
            Load -= AdjustSpriteColor;
            Invalidate( true );
        }

        public Color DrawColor {
            get { return bgimg.Color; }
            set { bgimg.Color = value; }
        }

        public Color KnobColor {
            get { return nippl.Color; }
            set { nippl.Color = value; }
        }
        public void flashPoint()
        {
            if( glimmer.Off ) {
                task().StoptAssist();
            } Invalidate();
        }

        void ITouchableElement.TouchInputReady( PointerInput inst )
        {
            PointerInput.Initialized -= touch.element().TouchInputReady;
            inst.RegisterTouchableElement( this );
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        // ControlMarkers:

        public void AddEventMarker( float position, string named ) 
        {
            marker.Add( CreateMarkerEvent( position, named ) );
            marker.Sort();
        }
        public void AddEventMarker( float position, string named, MarkerPassedDelegate handler )
        {
            MarkerPassedEvent markedPosition = CreateMarkerEvent( position, named );
            markedPosition.Passed += handler;
            marker.Add( markedPosition );
            marker.Sort();
        }

        public void AddEventMarker( float position, Enum name ) 
        {
            marker.Add( CreateMarkerEvent( position, name ) );
            marker.Sort();
        }
        public void AddEventMarker( float position, Enum named, MarkerPassedDelegate handler )
        {
            MarkerPassedEvent markedPosition = CreateMarkerEvent( position, named );
            markedPosition.Passed += handler;
            marker.Add( markedPosition );
            marker.Sort();
        }

        public IControlMarker GetEventMarker( int byIndex )
        {
            return byIndex < 0 || byIndex >= marker.Count
                 ? MarkerPassedEventArgs.Empty
                 : marker[byIndex];
        }
        public IControlMarker GetEventMarker( float atPosition )
        {
            int index = marker.FindIndex( mrk => mrk.Value == atPosition );
            return index >= 0 ? marker[index] : MarkerPassedEventArgs.Empty;
        }
        public IControlMarker GetEventMarker( Enum byEnum )
        {
            int index = marker.FindIndex( mrk => mrk.Endex == byEnum );
            return index >= 0 ? marker[index] : MarkerPassedEventArgs.Empty;
        }
        public IControlMarker GetEventMarker( string byName )
        {
            int index = marker.FindIndex( mrk => mrk.Named == byName );
            return index >= 0 ? marker[index] : MarkerPassedEventArgs.Empty;
        }
        public MarkerPassedEvent[] EventMarkers 
        {
            get { return (marker as IReadOnlyList<MarkerPassedEvent>).ToArray(); }
        }
        public void RemoveMarker( int atIndex ) 
        {
            marker.RemoveAt( atIndex );
        }
        public void RemoveMarker( float atPosition ) 
        {
            marker.RemoveAt( marker.FindIndex(m => m.Value == atPosition) );
        }
        public void RemoveMarker( string byName )
        {
            marker.RemoveAt( marker.FindIndex(m => m.Named == byName) );
        }
        public void RemoveMarker( Enum byEnum )
        {
            int found = marker.FindIndex( m => m.Endex == byEnum );
            if( found >= 0 ) marker.RemoveAt( found );
            else marker.Remove( marker.ElementAt( byEnum.ToInt32() ) );
        }
        public void ClearEventMarkers()
        {
            marker.Clear();
        }


        public IControlMarker NextMarkerBelow( float position )
        {
            for( int i = marker.Count-1; i >= 0; --i ) {
                if ( marker[i].Value < position )
                    return marker[i];
            } return CreateDefaultMarker( position > Center
                   ? DefaultMarkers.Center
                   : DefaultMarkers.Minimum ); 
        }

        public IControlMarker NextMarkerAbove( float position )
        {
            for ( int i = 0; i < marker.Count; ++i ) {
                if ( marker[i].Value > position )
                    return marker[i];
            } return CreateDefaultMarker( position < Center
                   ? DefaultMarkers.Center
                   : DefaultMarkers.Maximum );
        }

        public IControlMarker NextMarkerNearest( float position )
        {
            IControlMarker above = NextMarkerAbove( position );
            IControlMarker below = NextMarkerBelow( position );
            return (position - below.Value) < (above.Value - position)
                           ?   below        :  above;
        }


        //////////////////////////////////////////////////////////
        #region // ITouchableElement

        private IGestureTouchTrigger touchEvents() { return getInTouch().handler.events(); }
        TouchGesturesHandler<GuiSlider> ITouchGesturedElement<GuiSlider>.handler { get; set; }

        // basic touch interface events, directly triggered on touch down/move/lift for each finger (similar to mouse down/up/move events)

        public event FingerTip.TouchDelegate TouchDown {
            add{ touchEvents().TouchDown += value; }
            remove{ touchEvents().TouchDown -= value; }
        }
        public event FingerTip.TouchDelegate TouchLift {
            add{ touchEvents().TouchLift += value; }
            remove{ touchEvents().TouchLift -= value; }
        }
        public event FingerTip.TouchDelegate TouchMove {
            add{ touchEvents().TouchMove += value; }
            remove{ touchEvents().TouchMove -= value; }
        }

        // higher level events, abstracted from interpreting several Downs/Moves/Lifts (of maybe several fingers) over time axis...  (similar to mouse click/double/drag events )

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

        public event MultiFinger.TouchDelegate TouchDraged { // almost same like 'dragndrop finished' or 'dropped'
            add { touchEvents().TouchDraged += value; }
            remove { touchEvents().TouchDraged -= value; }
        }
        public event MultiFinger.TouchDelegate TouchRotate { // apears when more then one fingers (at least two involved) gesturing a rotation on the screen
            add { touchEvents().TouchRotate += value; }
            remove { touchEvents().TouchRotate -= value; }
        }
        public event MultiFinger.TouchDelegate TouchResize { // apears when more then one fingers (at least two involved) gesturing a resize on the screen 
            add { touchEvents().TouchResize += value; }
            remove { touchEvents().TouchResize -= value; }
        }


        public virtual void OnTouchDown( FingerTip tip )
        {}
        public virtual void OnTouchMove( FingerTip tip )
        {}
        public virtual void OnTouchLift( FingerTip tip )
        {}

        virtual public void OnTouchDupple( FingerTip tipple )
        {
            mnu_context.Show( this, tipple.Position );
        }
        virtual public void OnTouchTapped( FingerTip tapple )
        {}
        virtual public void OnTouchTrippl( FingerTip triple )
        {}

        virtual public void OnTouchDraged( MultiFinger tip )
        {}
        virtual public void OnTouchResize( MultiFinger tip )
        {}
        virtual public void OnTouchRotate( MultiFinger tip )
        {}

        public P ScreenLocation()
        {
            return PointToScreen( Point.Empty );
        }

        public IRectangle ScreenRectangle()
        {
            return AbsoluteEdges.FromRectangle( RectangleToScreen( new Rectangle( 0, 0, Width, Height ) ) );
        }

        public ITouchEventTrigger touch
        {
            get { return (this as ITouchGesturedElement<GuiSlider>).handler; }
        }

        public ITouchDispatchTrigger screen()
        {
            return touch.screen();
        }

        public ITouchableElement element()
        {
            return this;
        }

        private bool touchToMouse = false;
        public bool RouteTouchesToMouseEvents {
            get { return touchToMouse; }
            set { if ( value != touchToMouse ) {
                    touchToMouse = value;
                }
            }
        }

        public Control Element { get { return this; } }

        public bool IsTouched { get { return touch.IsTouched; } }
                #endregion


        public ITaskAsistableVehicle<Action,Action> task()
        {
            return this;
        }

        int IAsistableVehicle<IActionDriver<Action, ILapFinish<Action>, Action>, ILapFinish<Action>>.StartAssist()
        {
            return task().assist.GetAssistence( taskassist.action );
        }

        int IAsistableVehicle<IActionDriver<Action, ILapFinish<Action>, Action>, ILapFinish<Action>>.StoptAssist()
        {
            return task().assist.ReleaseAssist( taskassist.action );
        }

        private void mnu_context_Opened( object sender, EventArgs e )
        {
            Nuppsi.MouseMove -= Nuppsi_Move;
              this.TouchMove -= Nuppsi_Move;
        }

        ITaskAssistor<Action,Action> ITaskAsistableVehicle<Action,Action>.assist
        {
            get { return taskassist; }
            set { taskassist = value as TaskAssist<SteadyAction,Action,Action>; }
        }
    }
}

