using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using Stepflow;
using Stepflow.Controller;
using Stepflow.TaskAssist;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using Stepflow.Gui.Helpers;
using Orientation = Stepflow.Gui.Orientation;
using Style       = Stepflow.Gui.Style;
using Point32 = Win32Imports.Touch.Point32;

#if   USE_WITH_WF
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Layout;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Rect  = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rect  = System.Windows.Int32Rect;
using RectF = System.Windows.Rect;
#endif
using Win32Imports;



namespace Stepflow.Gui
{

    public enum MeterValence
    {
        Level, Dampf
    }

    public partial class GuiMeter 
        : UserControl
        , IInterValuable<Controlled.Float32>
        , ITaskAsistableVehicle<SteadyAction>
    {
        private const  uint LEVEL = 0;
        private const  uint CLIPP = 0x00000004;
        private static Bitmap[]      images;
        private static Rectangle[][] sources;
        private static Point         scale;
        private Pen    line = new Pen(Color.FromArgb(255,64,64,64),5);
         
        protected TaskAssist<SteadyAction,Action> dampfTackter;
        public event ValueChangeDelegate<float>   LevelChanged;
        public event ValueChangeDelegate<float>   LevelClipped;
        public event ValueChangeDelegate<float>   LevelRegular;

        
        protected Controlled.Float32                  dampf;
        protected object                              joint;
        protected Controlled.Float32                  value;
        private   Matrix                              rotat;
        private   ValenceBondMenu<Controlled.Float32> menue;


        protected ValenceField<Controlled.Float32,vEt> getJoints<vEt>() where vEt : struct {
            if( joint == null ) { joint = new ValenceField<Controlled.Float32,vEt>(this); }
            return (joint as ValenceField<Controlled.Float32,vEt>);
        }
        
        Action IInterValuable.getInvalidationTrigger() { return levelUpdate; }
        virtual public IControllerValenceField<Controlled.Float32> valence() { return getJoints<MeterValence>().field(); }
        virtual public IControllerValenceField<Controlled.Float32> valence( Enum which ) { return getJoints<MeterValence>().field( which ); }
        IControllerValenceField IInterValuable.valence<cT>() { return getJoints<MeterValence>().field(); }
        IControllerValenceField IInterValuable.valence<cT>( Enum which ) { return getJoints<MeterValence>().field(which); }
        void IInterValuable.callOnInvalidated(InvalidateEventArgs e) { OnInvalidated(e); }

        public ITaskAsistableVehicle<SteadyAction> task()
        {
            return this;
        }

        public ITaskAssistor<SteadyAction> assist()
        {
            return dampfTackter;
        }

        int ITaskAsistableVehicle<SteadyAction>.StartAssist()
        {
#if DEBUG
            int dampfi = dampfTackter.GetAssistence( dampfTackter.action );
            Consola.StdStream.Out.WriteLine( "TaskAssist: {0} dampfTackters registered", dampfi );
            return dampfi;
#else
            return dampfTackter.GetAssistence( dampfTackter.action );
#endif
        }

        int ITaskAsistableVehicle<SteadyAction>.StoptAssist()
        {
#if DEBUG
            int dampfi = dampfTackter.ReleaseAssist( dampfTackter.action );
            Consola.StdStream.Out.WriteLine( "TaskAssist: dampfTackter {0} released assistence", dampfi );
            return dampfi;
#else
            return dampfTackter.ReleaseAssist( dampfTackter.action );
#endif
        }

        static GuiMeter()
        {
#if DEBUG
            Consola.StdStream.Init( Consola.CreationFlags.TryConsole );
            Win32Imports.RETURN_CODE.SetLogOutWriter(Consola.StdStream.Out.WriteLine);
#endif
            Valence.RegisterIntervaluableType<Controlled.Float32>();
            scale = SystemMetrics.SCREENSCALE;
            images = new Bitmap[3];
            images[0] = Stepflow.Properties.Resources.meter_R;
            images[1] = Stepflow.Properties.Resources.meter_H;
            images[2] = Stepflow.Properties.Resources.meter_V;

            TaskAssist<SteadyAction,Action>.Init( 60 );
            sources = new Rectangle[3][] {
                      new Rectangle[9],
                      new Rectangle[5],
                      new Rectangle[5]
            };
            for ( int x = 0, i = 0; x < images[0].Width; x = ++i * 64 ) {
                sources[0][i] = new Rectangle(x, 0, 64, 64);
                sources[0][i + 3] = new Rectangle(x, 64, 64, 64);
                sources[0][i + 6] = new Rectangle(x, 128, 64, 64);
            }
            for ( int y = 0, i = 0; y < images[1].Height; y = ++i * 32 )
                sources[1][i] = new Rectangle(0, y, images[1].Width, 30);
            for ( int x = 0, i = 0; x < images[2].Width; x = ++i * 32 )
                sources[2][i] = new Rectangle(x, 0, 30, images[2].Height);
        }

        private int             deckelGroup;
        private Bitmap          deckelImage;
        private SolidBrush       alertColor;
        private SolidBrush       levelColor;
        private SolidBrush       background;
        private SolidBrush       clipground;
        private RondealDirection accentMode;
        public  RondealDirection AccentMode {
            get { return accentMode; } set { accentMode = value; }
        }
        public  Color           AlertColor {
            get { return alertColor.Color; }
            private set {
                levelColor.Color = value;
                if( accentMode == RondealDirection.CounterClock ) {
                    alertColor.Color = Color.FromArgb( 
                    value.A, value.G, value.B, value.R );
                    clipground.Color = Color.FromArgb(
                    value.A, value.G/4, value.B/4, value.R/4);
                } else {
                    alertColor.Color = Color.FromArgb( 
                    value.A, value.B, value.R, value.G );
                    clipground.Color = Color.FromArgb(
                    value.A, value.B/4, value.R/4, value.G/4);
                }
            }
        }

        private float lastClip;
        private bool waitSafe;
        protected virtual void TriggerEvents()
        {
            bool state = value.Active;
            value.Active = false;
            float actual = value;
            LevelChanged?.Invoke( this, new ValueChangeArgs<float>(actual) );
            actual = ClipValue;
            if ( actual > value.MAX ) {
                if ( actual > lastClip ) {
                    lastClip = actual;
                    LevelClipped?.Invoke( this,  new ValueChangeArgs<float>(actual) );
                }
            } else if( waitSafe ) {
                lastClip = 0;
                waitSafe = false;
                LevelRegular?.Invoke( this,  new ValueChangeArgs<float>(actual) );
            } value.Active = state;
        }

        protected int directionalOrientation;
        public override RightToLeft RightToLeft {
            get { return (RightToLeft)(directionalOrientation >> 2); }
            set { if ( base.RightToLeft != value ) {
                    directionalOrientation = ( ((int)value << 2)
                                             | directionalOrientation & 0x03 );
                    base.RightToLeft = value;
                    Invalidate();
                }
            }
        }

        public Orientation Orientation {
            get { return (Orientation)(directionalOrientation & 0x03); }
            set { int newval = (directionalOrientation & 0x04) | (int)value;
                if ( directionalOrientation != newval ) {
                    Orientation actual = (Orientation)(directionalOrientation & 0x03);
                    if ( value == Orientation.Rondeal ) {
                        Width = Height = Math.Max(Width, Height) / 2;
                    } else if ( actual == Orientation.Rondeal ) {
                        Width = (int)(Width * (actual == Orientation.Horizontal ? 2 : 0.5));
                        Height = (int)(Height * (actual == Orientation.Vertical ? 2 : 0.5));
                    } else {
                        int t = Height; Height = Width; Width = t;
                    }
                    deckelGroup = newval & 0x03;
                    deckelImage = images[deckelGroup];
                    directionalOrientation = newval;
                    Invalidate();
                }
            }
        }

        public DirectionalOrientation Direction {
            get { return (DirectionalOrientation)directionalOrientation; }
            set { int newval = (int)value;
                if( newval != directionalOrientation ) {
                    Orientation = (Orientation)(newval&0x03);
                    RightToLeft = (RightToLeft)(newval>>2);
                }
            }
        }

        private int style;
        public Style Style {
            get { return (Style)style; }
            set { if ((int)value != style) {
                    style = (int)value;
                    Invalidate();
                }
            }
        }

        private Point32 border = Point32.ZERO;
        public int InnerBorder {
            get { return border.x; }
            set { if ( border.x != value ) {
                    line.Width = ( border.x = border.y = (short)value ) * 3;
                    Invalidate();
                }
            }
        }

        private const int channels = 1;
        public ChannelMode Channels {
            get { return (ChannelMode)channels; }
        }

        private IntPtr clip;
        public float ClipValue {
            get { unsafe { return *(float*)clip.ToPointer(); } }
        }

        private IntPtr cycle;
        public bool Cycled {
            get { unsafe { return *(bool*)cycle.ToPointer(); } }
            set { unsafe { *(bool*)cycle.ToPointer() = value; } }
        }

        private IntPtr sign;
        public bool Unsigned {
            get { unsafe { return *(bool*)sign.ToPointer(); } }
            set { unsafe { bool* actual = (bool*)sign.ToPointer();
                    if( *actual != value ) {
                        *actual = value;
                        Range = this.value.MAX;
                    }
                }
            }
        }

        public float Range {
            get { return value.MAX; }
            set { float dampfActor = DampFactor;
                  this.value.MIN = Unsigned ? 0 : -value;
                  this.value.MAX = value;
                  DampFactor = dampfActor;
                  valence().SetDirty( ValenceFieldState.Flags.MIN
                                    | ValenceFieldState.Flags.MAX );
                  Invalidate();
            }
        }

        public int PixelRange {
            get { switch(Orientation) {
                    case Orientation.Horizontal: return ( Unsigned ? Bounds.Width : Bounds.Width / 2 ) - border.Summ();
                    case Orientation.Vertical: return ( Unsigned ? Bounds.Height : Bounds.Height / 2 ) - border.Summ();
                } return 270;
            }
        }

        protected float clipo;
        private float triggerLevelSafe( float actual ) {
            if( actual == 0 ) {
                if( clipo > 0 ) 
                    waitSafe = true;
            } else {
                waitSafe = false;
            } return actual;
        }
        public virtual float ClipFactor {
            get { return clipo = triggerLevelSafe( value.VAL == value.MAX
                               ? ( ( ClipValue - value.MAX ) - value.MIN ) / ( value.MAX - value.MIN )
                               : 0                  );
            }
        }

        private IntPtr clamp;
        public bool Clamped {
            get { unsafe { return *(bool*)clamp.ToPointer(); } }
            set { unsafe { *(bool*)clamp.ToPointer() = value; } }
        }

        private bool damp = false;
        public bool Damped {
            get { return damp; }
            set { if (value != damp) {
                    if( value ) task().StartAssist();
                    else task().StoptAssist();
                    damp = value;
                } 
            }
        }

        public float DampFactor {
            get { return 1.0f-(dampf.MOV/Range); }
            set { dampf.MOV = (1.0f-value)*Range; }
        }

        public bool Inverted {
            get { return (base.RightToLeft == ( Orientation == Orientation.Vertical 
                                              ? RightToLeft.No : RightToLeft.Yes ));
                }
            set { if ( value != Inverted ) {
                    RightToLeft = base.RightToLeft == RightToLeft.Yes
                                ? RightToLeft.No : RightToLeft.Yes;
                    Invalidate();
                } 
            }
        }

        public float Level {
            get { return damp ? dampf : value; }
            set { this.value.VAL = value;
                  valence().SetDirty( ValenceFieldState.Flags.VAL );
                  TriggerEvents();
                if( damp ) task().StartAssist();
                else Invalidate();
            }
        }

        private void levelUpdate() {
            Level = value;
        }

        public float Proportion {
            get { return Level / value.MAX; } 
            set { Level = this.value.MAX * value; }
        }

        private void mnu_Invert_Click( object sender, EventArgs e ) {
            Inverted = !Inverted;
        }
        private void mnu_Accept_Input_Click( object sender, EventArgs e ) {
          float getter;
            if ( float.TryParse( mnu_Input_Range.Text, out getter ) )
                Range = getter;
        }
        private void mnu_Unsigned_Click( object sender, EventArgs e ) {
            Unsigned = !Unsigned;
        }

        public ValueChangeDelegate<float> levelDelegate() {
            return ChangeLevel;
        }
        public ValueChangeDelegate<float> proportionalDelegate() {
            return ProportionalChange;
        }
        void ChangeLevel(object sender,ValueChangeArgs<float> newLevel) {
            Level = newLevel;
        }
        void ProportionalChange(object sender,ValueChangeArgs<float> newProportion) {
            Proportion = newProportion;
        }
        private void WrapController( Controlled.Float32 control )
        {
            this.value = control;
            dampf.SetPin( (int)ValueFollow.TARGET, value.GetTarget() );
            dampf.LetPoint(ControllerVariable.MIN, value.GetPointer(ControllerVariable.MIN));
            dampf.LetPoint(ControllerVariable.MAX, value.GetPointer(ControllerVariable.MAX));
        }

        protected void InitValue()
        {
            value = new Controlled.Float32();
            value.SetUp( 0, 1000, 0, 0, ControlMode.Element );
            clip = value.GetPin(ElementValue.PEAK);
            clamp = value.GetPin(ElementValue.CLAMPED);
            sign = value.GetPin(ElementValue.UNSIGNED);
            cycle = value.GetPin(ElementValue.CYCLED);
            unsafe { *(bool*)cycle.ToPointer() = false;
                     *(bool*)clamp.ToPointer() = true;
                     *(bool*)sign.ToPointer() = true; }
            value.SetCheckAtSet();
            value.Active = true;

            dampf = new Controlled.Float32();
            dampf.SetUp(0, 1000, 100, 0, ControlMode.Follow);
            dampf.SetPin( (int)ValueFollow.TARGET, value.GetTarget() );
            dampf.LetPoint(ControllerVariable.MIN, value.GetPointer(ControllerVariable.MIN));
            dampf.LetPoint(ControllerVariable.MAX, value.GetPointer(ControllerVariable.MAX));
            unsafe { *(float*)dampf.GetPin( ValueFollow.DIRECT ).ToPointer() = 750; }
            dampf.SetCheckAtGet();
            dampf.Active = false;
        }

        private void OnDampfTackt()
        {
            dampf.Check();
            if( dampf.VAL == value.VAL ) {
                task().StoptAssist();
            } Invalidate();
        }

        protected GuiMeter( bool executeConstructor )
        { 
            damp = false;
            accentMode = RondealDirection.CounterClock; 
            if(!executeConstructor)
               return;
        }
        public GuiMeter()
        {
            InitValue();
            (valence() as ValenceField<Controlled.Float32,MeterValence>).SetControllerArray(
                new Controlled.Float32[] { value, dampf }
            );
            InitMeter();
            InitializeComponent();

            base.RightToLeft = RightToLeft.Yes;
            directionalOrientation = (int)DirectionalOrientation.Up;
            Inverted = true;
            BorderStyle = BorderStyle.None;
            menue = new ValenceBondMenu<Controlled.Float32>( this, components );
        }

        public new void Dispose()
        {
            Valence.UnRegisterIntervaluableElement( this );
            base.Dispose();
        }

        protected IContainer InitConnector()
        {
            InitializeComponent();
            return components;
        }
        protected void InitMeter()
        {
            damp = false;
            accentMode = RondealDirection.CounterClock; 
            rotat      = new Matrix();
            levelColor = new SolidBrush(Color.Lime);
            alertColor = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
            background = new SolidBrush(Color.DarkSlateGray);
            clipground = new SolidBrush(Color.FromArgb(255, 64, 0, 0));

            border.x = 2;
            border.y = 2;
            style = 0;
            deckelGroup = 2;
            deckelImage = images[deckelGroup];
            dampfTackter = new TaskAssist<SteadyAction,Action>( this, OnDampfTackt, 60 );
            waitSafe = false;
            line.SetLineCap( LineCap.Round,LineCap.Round, DashCap.Round );
            line.LineJoin = LineJoin.Round;
        }

        ToolStripItemCollection IInterValuable.getMenuHook()
        {
            return mnu_MeterOptions.Items;
        }

        private IRectangle scetchArea( CenterAndScale area, uint mode, float value )
        {
            mode = (uint)directionalOrientation ^ mode;
            short pos = (short)(value*0.5f);
            switch ( (LinearDirection)mode ) {
                case LinearDirection.Left:
                    area.Scale.x -= pos;
                    area.Center.x += pos;
                break;
                case LinearDirection.Right:
                    area.Scale.x -= pos;
                    area.Center.x -= pos;
                break;
                case LinearDirection.Up:
                    area.Scale.y -= pos;
                    area.Center.y += pos;
                break;
                case LinearDirection.Down:
                    area.Scale.y -= pos;
                    area.Center.y -= pos;
                break;       
            } return area;
        }

        public static float rasterize( int rastertiles, float precisevalue )
        {
            return rastertiles == 0 ? precisevalue 
                 : ( (float)( (int)( precisevalue * rastertiles ) ) ) 
                   / rastertiles;
        }

        private Brush foreGround( uint areatype )
        {
            return areatype == LEVEL
                 ? levelColor
                 : alertColor;
        }

        private Brush backGround()
        {
            return ClipValue > value.MAX
                 ? clipground
                 : background;
        }

        private Rectangle levelChart( uint mode, float prop )
        {
            uint chartSelect = (uint)(prop * 3)+3;
            chartSelect = chartSelect >= 6 ? 5 : chartSelect;
            return sources[0][ 3 * (mode / CLIPP) + chartSelect ]; 
        }

        private Rectangle deckelFrame()
        {
            return sources[ deckelGroup ][ channels+(style-1) ];
        }

        protected override void OnForeColorChanged( EventArgs e )
        {
            base.OnForeColorChanged(e);
            AlertColor = ForeColor;
        }

        private void paintBar( Graphics g, CenterAndScale area, float val )
        {
            val = 1.0f - val;
            int rng = PixelRange;
            val = ( (Style)style != Style.Flat
                  ? rasterize( 25, val )
                  : val ) * rng;

            g.FillRectangle( foreGround( LEVEL ), scetchArea( area, LEVEL, val ).ToRectangle() );

            if ( ClipFactor != 0 ) {
                clipo = (clipo > value.MAX) ? value.MAX : (clipo < value.MIN) ? value.MIN : clipo;
                val = Inverted ? 1 : -1;
                clipo = ( ( ( (Style)style != Style.Flat
                            ? rasterize( 25, clipo ) 
                            : clipo ) * rng ) * val ) 
                      + ((val - 1) / -2) * rng;
                g.FillRectangle( foreGround( CLIPP ), scetchArea( area, CLIPP, clipo ).ToRectangle() );
            }
        }

        private void paintRondael( Graphics g, Rectangle area, float propval )
        {
            int degrees = PixelRange;
            PointF pivot = new PointF(
                (float)area.Width/2.0f + area.X,
                (float)area.Height/2.0f + area.Y
                                          );

            GraphicsState pushed = g.Save();
                rotat.Reset();
                rotat.RotateAt( (propval * degrees), pivot );
                rotat.Multiply( g.Transform );
                g.Transform = rotat;
                g.DrawImage( deckelImage, area,
                             levelChart( LEVEL, propval ),
                             GraphicsUnit.Pixel );
            g.Restore( pushed );

            if( ClipFactor != 0 ) { pushed = g.Save();
                clipo = (clipo > value.MAX) ? value.MAX 
                      : (clipo < value.MIN) ? value.MIN : clipo;
                rotat.Reset();
                rotat.RotateAt( 360 - ( clipo * degrees ), pivot );
                rotat.Multiply( g.Transform );
                g.Transform = rotat;
                g.DrawImage( deckelImage, area,
                             levelChart( CLIPP, clipo ),
                             GraphicsUnit.Pixel );
            g.Restore( pushed ); }
        }

        protected unsafe override void OnPaint( PaintEventArgs g )
        {
            Rectangle rect = Bounds;
            rect.Location = Point.Empty;
            CenterAndScale area = CenterAndScale.FromRectangle( rect );
            bool drawBorder = Style == Style.Flat;
            area.Scale -= border;
            base.OnPaint( g );

            g.Graphics.FillRectangle( backGround(), area.ToRectangle() );

            if ( Orientation == Orientation.Rondeal ) {
                paintRondael( g.Graphics, rect, Proportion );
                g.Graphics.DrawImage( deckelImage, rect, deckelFrame(),
                                      GraphicsUnit.Pixel );
            } else {
                paintBar( g.Graphics, area, Proportion );
                if ( !drawBorder ) g.Graphics.DrawImage( deckelImage,
                                     rect, deckelFrame(),
                                     GraphicsUnit.Pixel );
            } if ( drawBorder ) {
                g.Graphics.DrawRectangle( line, rect );
            }
        }
    }
}
