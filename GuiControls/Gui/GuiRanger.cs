using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Stepflow;
using Stepflow.Gui.Geometry;
using Stepflow.Gui.Automation;
using Stepflow.Gui.Helpers;
using Stepflow.Controller;
using Stepflow.Helpers;
using System.Xml;
using System.Xml.XPath;
using Point32 = Stepflow.Gui.Geometry.Point32;
using Point64 = Stepflow.Gui.Geometry.Point64;
using Win32Imports.Touch;
using System.Collections;
using Resources = GuiControls.Properties.Resources;

namespace Stepflow.Gui
{
    public interface IRange
    {
        float  From { get; }
        float  Till { get; }
        string Name { get; }
    }

    public struct Range : IRange
    {
        public float From;
        public float Till;
        float IRange.From {
            get { return From; }
        }
   
        public string Name {
            get { return "unnamed"; }
        }

        float IRange.Till {
            get { return Till; }
        }

        public Range(float from,float till)
        {
            From = from;
            Till = till;
        }
    }

    public partial class GuiRanger
        : UserControl, IRange
        , IInterValuable<Controlled.Float32>
        , ITouchGesturedElement<GuiRanger>
    {
        public enum DefaultRanges : int {
            FirstRange, SecondRange,
            ThirdRange, FourthRage,
            FirstLevel, SecondLevel,
            ThirdLevel, FourthLevel,
        }
        public enum RangeMaxima : int {
            First = 0,
            Last = Int32.MaxValue,
            WholeRange = Int32.MinValue,
            Invalid = -1,
        }

        public struct RangeData : IRange
        {
            public readonly float From;
            public readonly float Till;
            public readonly Enum  Name;
            public readonly float Level;

            float IRange.From {
                get { return From; }
            }

            float IRange.Till {
                get { return Till; }
            }

            string IRange.Name {
                get { return Name.ToString(); }
            }

            internal RangeData( Enum name, float from, float till, float level )
            {
                Name = name;
                From = from;
                Till = till;
                Level = level;
            }

            public override string ToString()
            {
                return string.Format("{0}: {1} to {2}",Name,From,Till);
            }
        }

        public class SplitRangeEventArgs : EventArgs
        {
            new public static readonly SplitRangeEventArgs Empty = new SplitRangeEventArgs( new RangeData( RangeMaxima.Invalid, 0, 0, 0) );
            public readonly IRange Range;
            internal SplitRangeEventArgs( IRange arg ) {
                Range = arg;
            }
        }

        public class SplitPointEventArgs : ValueChangeArgs<float>
        {
            public SplitPoint Point;
            public SplitPointEventArgs( SplitPoint point ) : base( point.value.VAL )
            {
                Point = point;
            }

            public static implicit operator SplitPointEventArgs( SplitPoint cast )
            {
                return new SplitPointEventArgs( cast );
            }
        }

        public class RangeLevelEventArgs : ValueChangeArgs<float>
        {
            public SplitRange Range;
            public RangeLevelEventArgs( SplitRange range ) : base( range.Level )
            {
                Range = range;
            }

            public static implicit operator RangeLevelEventArgs( SplitRange cast )
            {
                return new RangeLevelEventArgs( cast );
            }
        }

        public delegate void RangeChangedEvent( object sender, SplitRangeEventArgs changed );
        public delegate void SplitChangedEvent( object sender, SplitPointEventArgs changed );
        public delegate void RangeLevelChanged( object sender, RangeLevelEventArgs changed );

        private event RangeChangedEvent LastRangeChanged;
        public event RangeChangedEvent RangeChanged;
        public event SplitChangedEvent SplitChanged;
        public event RangeLevelChanged LevelChanged;

        private SplitRangeEventArgs rangeEvent( SplitRangeEventArgs args )
        {
            if( RangeChanged != null ) {
                RangeChanged( this, args );
            } return args;
        }

        private SplitPointEventArgs splitEvent( SplitPointEventArgs args )
        {
            if( SplitChanged != null ) {
                SplitChanged( this, args );
            } return args;
        }

        private RangeLevelEventArgs levelEvent( RangeLevelEventArgs args )
        {
            if( LevelChanged != null ) {
                LevelChanged( this, args );
            } return args;
        }

        public class SplitPoint
        {
            public GuiRanger          cntrl;
            public Controlled.Float32 value;
            public CenterAndScale     areal;
            public LED                color;
            private IntPtr            clamp;
            private Enum              below;
            private Enum              lower;
            private Enum              upper;
            private Action            dirty;

            public Enum Name {
                get { return lower; }
            }

            public int Index {
                get { return lower.ToInt32(); }
            }

            public event ValueChangeDelegate<float> SplitPointChanged;
            public event RangeChangedEvent          LowerRangeChanged;
            public event RangeChangedEvent          UpperRangeChanged {
                add { if( upper.ToInt32() < cntrl.SplitCount ) {
                        cntrl.ranges[ upper.ToInt32() ].LowerRangeChanged += value;
                    } else cntrl.LastRangeChanged += value;
                }
                remove { if( upper.ToInt32() < cntrl.SplitCount ) {
                        cntrl.ranges[ upper.ToInt32() ].LowerRangeChanged -= value;
                    } else cntrl.LastRangeChanged += value;
                }
            }

            public class RangeEnumerator : IEnumerator<RangeData>
            {
                private SplitPoint split;
                private bool       setup;
                private bool       upper;

                internal RangeEnumerator( SplitPoint first )
                {
                    setup = false;
                    split = first;
                    upper = false;
                }

                public RangeData Current {
                    get { return upper ? split.UpperRange : split.LowerRange; }
                }

                object IEnumerator.Current {
                    get { return upper ? split.UpperRange : split.LowerRange; }
                }

                public void Dispose()
                {}

                public bool MoveNext()
                {
                    if( setup == false ) {
                        split = split.cntrl.ranges[0];
                        return setup = true;
                    }
                    if( upper ) {
                        int next = split.upper.ToInt32() + 1;
                        if( next < split.cntrl.SplitCount ) {
                            upper = false;
                        } else if( --next == split.cntrl.SplitCount ) {
                            return false;
                        } split = split.cntrl.ranges[next];
                    } else {
                        upper = true;
                    } return true;
                }

                public void Reset()
                {
                    setup = upper = false;
                }

            }

            public SplitPoint( GuiRanger elm, float min, float max, float val, Enum low, Enum high, LED color ) {
                cntrl = elm;
                value = new Controlled.Float32();
                value.SetUp( min, max, 0, val, ControlMode.Element );
                int enumvalue = low.ToInt32();
                if( enumvalue > 0 )
                    below = Enum.GetValues( low.GetType() ).GetValue( enumvalue - 1 ) as Enum;
                else below = RangeMaxima.Invalid;
                lower = low;
                upper = high;
            }

            internal unsafe void LateInit() {
                *(byte*)value.GetPin( ElementValue.CYCLED ).ToPointer() = (byte)0x00;
                *(byte*)value.GetPin( ElementValue.UNSIGNED ).ToPointer() = (byte)0x00;
                clamp = value.GetPin( ElementValue.CLAMPED );
                value.SetCheckAtSet();
                value.Active = true;
                Clamped = true;
                if ( upper.ToInt32() == cntrl.SplitCount ) {
                    dirty = () => {
                        cntrl.valence(below).SetDirty( ValenceFieldState.Flags.MAX );
                        cntrl.valence(lower).SetDirty( ValenceFieldState.Flags.VAL );
                        SplitPointEventArgs point = cntrl.splitEvent( this );
                        SplitPointChanged?.Invoke( cntrl, point );
                        SplitRangeEventArgs args = cntrl.rangeEvent( new SplitRangeEventArgs( LowerRange ) );
                        LowerRangeChanged?.Invoke( cntrl, args );
                        args = cntrl.rangeEvent( new SplitRangeEventArgs( UpperRange ) );
                        cntrl.LastRangeChanged?.Invoke( cntrl, args );
                    };
                } else if ( below.ToInt32() == (-1) ) {
                    dirty = () => {
                        cntrl.valence(lower).SetDirty( ValenceFieldState.Flags.VAL );
                        cntrl.valence(upper).SetDirty( ValenceFieldState.Flags.MIN );
                        SplitPointEventArgs point = cntrl.splitEvent( this );
                        SplitPointChanged?.Invoke( cntrl, point );
                        SplitRangeEventArgs args = cntrl.rangeEvent( new SplitRangeEventArgs( LowerRange ) );
                        LowerRangeChanged?.Invoke( cntrl, args );
                        args = cntrl.rangeEvent( new SplitRangeEventArgs( UpperRange ) );
                        cntrl.ranges[upper.ToInt32()].LowerRangeChanged?.Invoke( cntrl, args );
                    };
                } else {
                    dirty = () => {
                        cntrl.valence(below).SetDirty( ValenceFieldState.Flags.MAX );
                        cntrl.valence(lower).SetDirty( ValenceFieldState.Flags.VAL );
                        cntrl.valence(upper).SetDirty( ValenceFieldState.Flags.MIN );
                        SplitPointEventArgs point = cntrl.splitEvent( this );
                        SplitPointChanged?.Invoke( cntrl, point );
                        SplitRangeEventArgs args = cntrl.rangeEvent( new SplitRangeEventArgs( LowerRange ) );
                        LowerRangeChanged?.Invoke( cntrl, args );
                        args = cntrl.rangeEvent( new SplitRangeEventArgs( UpperRange ) );
                        cntrl.ranges[upper.ToInt32()].LowerRangeChanged?.Invoke( cntrl, args );
                    };
                }
            }

            public bool Clamped {
                get { unsafe { return *(byte*)clamp.ToPointer() != 0; } }
                set { unsafe { *(byte*)clamp.ToPointer() = (byte)( value ? 0x01 : 0x00 ); } }
            }

            public float Proportion {
                get { return (value.VAL - cntrl.range.From) / (cntrl.range.Till - cntrl.range.From); }
                set { Position = cntrl.range.From + (value * (cntrl.range.Till - cntrl.range.From)); }
            }

            public float Position {
                get { return value; }
                set { if ( this.value != value ) {
                        this.value.VAL = value;
                        if (cntrl.Orientation == Orientation.Horizontal) {
                            areal.Center.X = cntrl.getPixlPos( Proportion );
                        } else {
                            areal.Center.Y = cntrl.getPixlPos( Proportion );
                        } dirty();
                    }
                }
            }

            public float Minimum {
                get { return value.MIN; }
                set { if ( lower.ToInt32() > 0 ) {
                        cntrl.ranges[below.ToInt32()].Position = value;
                    } else { this.value.MIN = value;
                        cntrl.valence(lower).SetDirty( ValenceFieldState.Flags.MIN );
                        SplitRangeEventArgs args = cntrl.rangeEvent( new SplitRangeEventArgs( LowerRange ) );
                        LowerRangeChanged?.Invoke( cntrl, args );
                    }
                }
            }

            public float Maximum {
                get { return value.MAX; }
                set { if ( upper.ToInt32() < cntrl.SplitCount ) {
                        cntrl.ranges[upper.ToInt32()].Position = value;
                    } else { this.value.MAX = value;
                        cntrl.valence(lower).SetDirty( ValenceFieldState.Flags.MAX );
                        SplitRangeEventArgs args = cntrl.rangeEvent( new SplitRangeEventArgs( UpperRange ) );
                        cntrl.LastRangeChanged?.Invoke( cntrl, args );
                    }
                }
            }

            public int Location {
                get { return cntrl.orientation == 1 ? areal.Center.x : areal.Center.y; }
                set { Proportion = cntrl.getPropVal( value ); }
            }

            public RangeData LowerRange {
                get { return new RangeData( lower, value.MIN, value, cntrl.values[lower.ToInt32()] );  }
            }

            public RangeData UpperRange {
                get { return new RangeData( upper, value, value.MAX, cntrl.values[upper.ToInt32()] );  }
            }
        }

        public class SplitRange : IRange
        {
            private Enum      level_index;
            private Enum      range_index;
            private bool      islast;
            private GuiRanger instance;
            private Controlled.Float32 level;
            private Action    wrapinst_Invalidate;
            private int       wrapinst_Invert;

            private void Invalidate()
            {
                instance.valence( level_index ).SetDirty( ValenceFieldState.Flags.VAL );
                RangeLevelEventArgs args = instance.levelEvent( this ); 
                LevelChanged?.Invoke( instance, args );
                instance.Invalidate();
                wrapinst_Invalidate?.Invoke();
            }
            public event RangeLevelChanged LevelChanged;

            internal SplitRange( Enum rngidx, Enum lvlidx, Controlled.Float32 lvl, GuiRanger elm )
            {
                islast = lvlidx.ToInt32() == Enum.GetValues(lvlidx.GetType()).Length - 1;
                wrapinst_Invalidate = null;
                level_index = lvlidx;
                range_index = rngidx;
                instance = elm;
                level = lvl;
            }

            public void Wrap( IControllerValenceField<Controlled.Float32> otherField )
            {
                IInterValuable otherElement = (otherField as IStepflowControlElementComponent).getElement();
                if ( otherElement != instance ) {
                    (otherElement as Control).Invalidated += OnWrapUpdate; // instance.valence(level_index).getInvalidationHandler();
                    wrapinst_Invalidate = otherElement.getInvalidationTrigger();
                    level = otherField.controller();
                    instance.setter( level_index, level );
                } 
            }

            public bool Inverted {
                get { unsafe { return wrapinst_Invert > 0; } }
                set { unsafe { wrapinst_Invert = value ? 1 : -1; } }
            }

            private void OnWrapUpdate( object sender, InvalidateEventArgs args )
            {
                Level = level;
            }

            public string Name {
                get { return range_index.ToString(); }
            }

            public float From {
                get { if (islast) {
                        return instance.ranges[range_index.ToInt32()-1].Position;
                    } else {
                        return instance.ranges[range_index.ToInt32()].Minimum;
                    }
                }
                set { if (islast) {
                        instance.ranges[range_index.ToInt32()-1].Position = value;
                    } else {
                        instance.ranges[range_index.ToInt32()].Minimum = value;
                    }
                }
            }

            public float Till {
                get { if (islast) {
                        return instance.ranges[range_index.ToInt32()-1].Maximum;
                    } else {
                        return instance.ranges[range_index.ToInt32()].Position;
                    }
                }
                set { if (islast) {
                        instance.ranges[range_index.ToInt32()-1].Maximum = value;
                    } else {
                        instance.ranges[range_index.ToInt32()].Position = value;
                    }
                }
            }

            public float Level {
                get { return level.VAL; }
                set { if ( level.VAL != value ) {
                        level.VAL = value;
                        Invalidate();
                    }
                }
            }

            public float MaxLevel {
                get { return level.MAX; }
                set { level.MAX = value;
                    instance.valence( level_index ).SetDirty( ValenceFieldState.Flags.MAX );
                }
            }

            public float MinLevel {
                get { return level.MIN; }
                set { level.MIN = value;
                    instance.valence( level_index ).SetDirty( ValenceFieldState.Flags.MIN );
                }
            }

            public float Proportion {
                get { return Inverted ? 1.0f - (level.VAL - level.MIN) / (level.MAX - level.MIN) : (level.VAL - level.MIN) / (level.MAX - level.MIN); }
                set { Level = Inverted ? (level.MIN + ((1.0f-value) * (level.MAX - level.MIN))) : (level.MIN + (value * (level.MAX - level.MIN))); }
            }

            public static implicit operator float( SplitRange cast )
            {
                return cast.level.VAL;
            }
        }

        private static Bitmap[]                               images;
        private static IRectangle[][][]                       source;
        private Type                                          enmtyp = null;
        private LedGlimmer                                    glimmer;
        private object                                        bindis;
        private Controlled.Int32                              active;
        private List<SplitPoint>                              ranges;
        private SplitRange[]                                  values;
        private Dictionary<ushort,int>                        finger;
        private short                                         offset;
        private Range                                         range;

        private Pen                                           curve;

        private delegate IControllerValenceField<Controlled.Float32> ValenceGetter();
        private delegate void ValenceSetter( Enum index, ControllerBase controller );

        private ValenceGetter                                 getter;
        private ValenceSetter                                 setter;
        private ValenceBondMenu<Controlled.Float32>           mnu_valence;



        static GuiRanger()
        {
            Valence.RegisterIntervaluableType<Controlled.Float32>();
            Valence.RegisterIntervaluableType<Controlled.Int32>();

            images = new Bitmap[] {
                Resources.slider_leds_png,
                Resources.slider_H_png,
                Resources.slider_V_png
            };

            SpriteSheet.Loader loader = new SpriteSheet.Loader( Resources.slider_complete_xml );

            source = new IRectangle[2][][] {
                new IRectangle[3][] {
                    new IRectangle[3], new IRectangle[3], null
                },
                new IRectangle[3][] {
                    new IRectangle[3], new IRectangle[3], null
                },
            };

            for (Orientation direction=Orientation.Horizontal; direction <= Orientation.Vertical; ++direction ) {
                source[direction-Orientation.Horizontal][0][0] = loader.getSprite( direction, "Flat.Blet.Min" );
                source[direction-Orientation.Horizontal][0][1] = loader.getSprite( direction, "Flat.Blet.Mid" );
                source[direction-Orientation.Horizontal][0][2] = loader.getSprite( direction, "Flat.Blet.Max" );
                source[direction-Orientation.Horizontal][1][0] = loader.getSprite( direction, "Flat.Nipple.Small" );
                source[direction-Orientation.Horizontal][1][1] = loader.getSprite( direction, "Flat.Nipple.Medium" );
                source[direction-Orientation.Horizontal][1][2] = loader.getSprite( direction, "Flat.Nipple.Large" );
                source[direction-Orientation.Horizontal][2]    =  loader.getArray( direction, "Led" );
            }

            if(!PointerInput.isInitialized() ) {
                PointerInput.AutoRegistration = AutoRegistration.Enabled;
            }
        }

        public void InitRanges<UsedEnumType>( float from, float till ) where UsedEnumType : struct
        {
            range = new Range( from, till );
            Type initType = typeof( UsedEnumType );
            if ( initType == enmtyp ) return;

            if( !Enum.IsDefined( initType, 0 ) ) {
                throw new Exception( "UsedEnumType must define a constante of value 0" );
            } enmtyp = initType;

            Array fieldindices = Enum.GetValues( enmtyp );
            int rangecount = fieldindices.Length/2;
            if( ranges == null )
                ranges = new List<SplitPoint>( rangecount-1 );
            if( ranges.Count > 0 )
                ranges.Clear();
            values = new SplitRange[rangecount];

            ControllerBase[] initarray = new ControllerBase[fieldindices.Length];
            for( int i = 0; i < rangecount-1; ++i ) {
                float I = (i+1)*3;
                SplitPoint r = new SplitPoint( this,  I-2.0f, I, I-1.0f,
                    fieldindices.GetValue(i) as Enum,
                    fieldindices.GetValue(i+1) as Enum,
                    (LED)((i%8)+1)
                );
                if ( i > 0 ) {
                    r.value.LetPoint( ControllerVariable.MIN, ranges[i-1].value.GetTarget() );
                    ranges[i-1].value.LetPoint( ControllerVariable.MAX, r.value.GetTarget() );
                } ranges.Add( r );
                initarray[i] = r.value;
            }
            
            active.Active = false;
            active.MIN = 0;
            active.MAX = rangecount - 2;
            active.MOV = 1;
            active.VAL = 0;
            active.Active = true;
            initarray[rangecount-1] = active;

            ranges[ranges.Count-1].value.MAX = range.Till;
            ranges[0].value.MIN = range.From;
            float part = (range.Till - range.From)/(SplitCount-1);
            for( int i = ranges.Count-1; i >= 0; --i ) {
                ranges[i].value.VAL = range.From + (part * i);
                ranges[i].LateInit();
            }
            for ( int i = 0; i < values.Length; ++i ) unsafe {
                Controlled.Float32 v = new Controlled.Float32();
                v.SetUp(-1, 1, 0, 0, ControlMode.Element);
                v.SetCheckAtSet();
                *(bool*)v.GetPin(ElementValue.CYCLED).ToPointer() = false;
                *(bool*)v.GetPin(ElementValue.CLAMPED).ToPointer() = true;
                v.Active = true;
                values[i] = new SplitRange( fieldindices.GetValue(i) as Enum,
                                            fieldindices.GetValue(i+rangecount) as Enum,
                                            v, this );
                initarray[i + rangecount] = v;
            }

            bindis = new ValenceField<Controlled.Float32,UsedEnumType>( this, initarray );
            getter = () => { return bindis as ValenceField<Controlled.Float32,UsedEnumType>; };
            setter = (Enum index, ControllerBase reset) => {
                (bindis as ValenceField<Controlled.Float32, UsedEnumType>).SetController( index.ToInt32(), reset );
            };
            Invalidate();
        }

        private void InitNippels( Orientation direction )
        {
            short range = 0;
            int count = ranges.Count;
            short position = 0;
            if ( direction == Orientation.Horizontal ) {
                offset = position = (short)(Height / 2);
                range = (short)((Width - Height) / count);
                SplitPoint rng;
                for( int i = 0; i < count; ++i ) {
                    rng = ranges[i];
                    rng.color = (LED)((i % 8) + 1);
                    rng.areal.Scale.y = rng.areal.Center.y = offset;
                    rng.areal.Scale.x = (short)((offset * 10) / 16);
                    rng.areal.Center.x = position;
                    position += range;
                }
            } else {
                offset = position = (short)(Width / 2);
                range = (short)((Height - Width) / count);
                SplitPoint rng;
                for( int i = 0; i < ranges.Count; ++i ) {
                    rng = ranges[i];
                    rng.color = (LED)((i % 8) + 1);
                    rng.areal.Scale.x = rng.areal.Center.x = offset;
                    rng.areal.Scale.y = (short)((offset * 10) / 16);
                    rng.areal.Center.y = position;
                    position += range;
                }
            }
        }

        public GuiRanger()
        {
            curve = new Pen(Color.CornflowerBlue, 5);
            finger = new Dictionary<ushort,int>(0);
            bindis = new ValenceField<Controlled.Float32,DefaultRanges>( this );
            active = new Controlled.Int32();
            active.SetUp( 1, 3, 1, 1, ControlMode.Element );
            glimmer = new LedGlimmer( images[0], 1.0f, source[0][2] );
            glimmer.AddSheet( source[1][2] );

            InitRanges<DefaultRanges>(0,1000);
            InitializeComponent();
            
            mnu_valence = new ValenceBondMenu<Controlled.Float32>( this, components );
            (this as IInterValuable).getMenuHook().Add( mnu_valence );

            touchhandler = new TouchGesturesHandler<GuiRanger>( this );
            if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                if( PointerInput.Dispatcher == null ) {
                    PointerInput.Initialized += TouchInputReady;
                } else
                    PointerInput.Dispatcher.RegisterTouchableElement( this );
            }

            glimmer.Pre = 0.75f;
            glimmer.Led = LED.off;
            glimmer.SetStyle( style = Style.Flat );
            glimmer.SetModus( LedGlimmer.Mode.AutoBrushSelect );
            glimmer.Lum = 1.0f;

            InitNippels( Orientation );
            Resize += Resized;
            mipmap = 2;
        }

        public new void Dispose()
        {
            Valence.UnRegisterIntervaluableElement( this );
            PointerInput.Dispatcher.UnRegisterTouchableElement( this );
            base.Dispose();
        }

        private int mipmap;
        private void setNupsiMipmap( int newsize ) {
            mipmap = newsize >= 20
                   ? newsize < 25 
                   ? 1 : 2 : 0;
        }

        private int orientation = 1;
        public Orientation Orientation {
            get { return (Orientation)orientation; }
            set { if (orientation != (int)value) {
                    orientation = (int)value;
                    Size change = new Size( Height, Width );
                    glimmer.SetSheet( orientation - 1 );
                    Size = change;
                }    
            }
        }

        private Style style;
        public Style Style {
            get { return style; }
            set { if( value != style ) {
                    style = value;
                    Invalidate();
                }
            }
        }

        
        public int PixelRange {
            get { return Orientation == Orientation.Horizontal ? Width-Height : Height-Width; }
        }

        private float getPropVal( int pixelposition )
        {
            return ( (float)pixelposition - offset ) / PixelRange;
        }

        private int getPixlPos( float propval )
        {
            return (int)( (propval * PixelRange) + offset ) ;
        }

        public IRange WholeRange {
            get { return this; }
            set { if ( range.From != value.From ) {
                     From = value.From;
                } if ( range.Till != value.Till ) {
                     Till = value.Till;
                }
            }
        }

        public int SplitCount {
            get { return ranges.Count; }
        }

        public int RangeCount {
            get { return ranges.Count + 1; }
        }

        public float From {
            get { return range.From; }
            set { ranges[0].value.MIN = range.From = value; }
        }

        public float Till {
            get { return range.Till; }
            set { ranges[ranges.Count-1].value.MAX = range.Till = value; }
        }

        public SplitRange GetRange( Enum index )
        {
            return GetRange( index.ToInt32() );
        }

        public SplitRange GetRange( int index )
        {
            return values[index];
        }

        public void SetRange( Enum index, float from, float till )
        {
            int idx = index.ToInt32();
            if( idx == ranges.Count ) {
                ranges[idx-1].value.MAX = range.Till = till;
                ranges[idx-1].Position = from;
            } else if ( idx == 0 ) {
                ranges[idx].value.MIN = range.From = from;
                ranges[idx].Position = till;
            } else {
                ranges[idx-1].Position = from;
                ranges[idx].Position = till;
            }
        }

        public SplitPoint GetSplit( Enum index )
        {
            return ranges[index.ToInt32()];
        }
        public SplitPoint GetSplit( int index )
        {
            return ranges[index];
        }

        public void SetSplit( int index, float value )
        {
            ranges[index].Position = value;
        }

        public float GetLevel( Enum index )
        {
            unsafe { return values[index.ToInt32()-RangeCount].Level; }
        }

        public void SetLevel( Enum index, float value )
        {
            unsafe { values[index.ToInt32()-RangeCount].Level = value; }
        }

        public SplitRange this[int idx] {
            get { return values[ idx ]; }
        }

        public IEnumerator<SplitPoint> SplitPoints {
            get { return ranges.GetEnumerator(); }
        }

        public IEnumerator<RangeData> SplitRanges {
            get { return new SplitPoint.RangeEnumerator( ranges[0] ); }
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
                StartInteract( e.Location, 0 );
        }

        private void StartInteract( Point location, ushort interaction )
        {
            int count = ranges.Count;
            bool nippl = false;
            for (int i = 0; i < count;  ++i ) {
                if ( ranges[i].areal.Contains( location ) ) {
                    if ( interaction > 0 ) {
                        int fingers = finger.Count;
                        finger.Add( interaction, i );
                        if (fingers==0) {
                            TouchMove += GuiRanger_TouchMove;
                            TouchLift += GuiRanger_TouchLift;
                        }
                    } else {
                        active.MAX = count - 1;
                        active.VAL = i;
                        MouseMove += MouseMoved;
                        MouseUp += MouseRelease;
                    } nippl = true;
                    break;
                }
            } if ( !nippl ) {
                AbsoluteEdges rect = AbsoluteEdges.FromRectangle( Bounds );
                for( int i=0; i<count; ++i ) {
                    if (orientation == 1)
                        rect.R = (short)ranges[i].Location;
                    else
                        rect.B = (short)ranges[i].Location;
                    if ( rect.Contains( location ) ) {
                        if ( interaction > 0 ) {
                            int fingers = finger.Count;
                            finger.Add( interaction, i );
                            if (fingers==0) {
                                TouchMove += TouchMove_ValueChange;
                                TouchLift += TouchLift_RangeRelease;
                            }
                        } else {
                            active.MAX = SplitCount;
                            active.VAL = i;
                            MouseMove += MouseMove_ValueChange;
                            MouseUp += MouseUp_RangeRelease;
                        } nippl = true;
                        break;
                    } else if (orientation == 1)
                        rect.L = rect.R;
                    else
                        rect.T = rect.B;
                } if (!nippl) {
                    if (orientation == 1)
                        rect.R = (short)(Left + Width);
                    else
                        rect.B = (short)(Top + Height);
                    if ( rect.Contains( location ) ) {
                        if ( interaction > 0 ) {
                            int fingers = finger.Count;
                            finger.Add( interaction, SplitCount );
                            if (fingers==0) {
                                TouchMove += TouchMove_ValueChange;
                                TouchLift += TouchLift_RangeRelease;
                            }
                        } else {
                            active.MAX = SplitCount;
                            active.VAL = SplitCount;
                            MouseMove += MouseMove_ValueChange;
                            MouseUp += MouseUp_RangeRelease;
                        }
                    }
                }
            }
        }

        private void TouchLift_RangeRelease(object sender, FingerTip touch)
        {
            if( finger.Remove( touch.Id ) ) {
                if ( finger.Count == 0 ) {
                    TouchMove -= TouchMove_ValueChange;
                    TouchLift -= TouchLift_RangeRelease;
                }
            }
        }

        private void TouchMove_ValueChange(object sender, FingerTip touch)
        {
            ValueFunction( touch.Position, finger[touch.Id] );
        }

        private void MouseUp_RangeRelease(object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Left ) {
                active.VAL = 0;
                active.MAX = SplitCount-1;
                MouseMove -= MouseMove_ValueChange;
                MouseUp -= MouseUp_RangeRelease;
            }
        }

        private void MouseMove_ValueChange( object sender, MouseEventArgs e )
        {
            ValueFunction( e.Location, active );
        }

        private void GuiRanger_TouchLift( object sender, FingerTip touch )
        {
            if ( finger.Remove( touch.Id ) ) {
                if ( finger.Count == 0 ) {
                    TouchMove -= GuiRanger_TouchMove;
                    TouchLift -= GuiRanger_TouchLift;
                }
            }
        }

        private void MoveFunction( Point position, int nippelindex )
        {
            if( Orientation == Orientation.Horizontal ) {
                ranges[nippelindex].Location = position.X;
            } else { 
                ranges[nippelindex].Location = position.Y;
            } Invalidate();
        }

        private void ValueFunction( Point position, int rangeindex )
        {
            if( Orientation == Orientation.Horizontal ) {
                values[rangeindex].Proportion = ((float)(-(offset - position.Y)) / (offset * 5));
            } else {
                values[rangeindex].Proportion = ((float)(offset - position.X) / (offset * 5));
            } Invalidate();
        }

        private void MouseMoved( object sender, MouseEventArgs e )
        {
            MoveFunction( e.Location, active );
        }

        private void GuiRanger_TouchMove( object sender, FingerTip tip )
        {
            MoveFunction( tip.Position, finger[tip.Id] );
        }

        private void MouseRelease( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left ) {
                MouseMove -= MouseMoved;
                MouseUp -= MouseRelease;
            }
        }

        private void Resized( object sender, EventArgs e )
        {
            InitNippels( Orientation );
        }

        private void PaintBackground( PaintEventArgs e )
        {
            CornerAndSize draw = new CornerAndSize( 0, 0, Width, Height );
            if( Orientation == Orientation.Horizontal ) {
                draw.W = offset;
                e.Graphics.DrawImage( images[orientation], draw.ToRectangle(), source[0][0][0].ToRectangle(), GraphicsUnit.Pixel);
                draw.Corner.X += (Width - offset);
                e.Graphics.DrawImage( images[orientation], draw.ToRectangle(), source[0][0][2].ToRectangle(), GraphicsUnit.Pixel);
                draw.Sizes.X = Width - (offset + offset); draw.X = offset;
                e.Graphics.DrawImage( images[orientation], draw.ToRectangle(), source[0][0][1].ToRectangle(), GraphicsUnit.Pixel);
            } else {
                draw.H = offset;
                e.Graphics.DrawImage( images[orientation], draw.ToRectangle(), source[1][0][0].ToRectangle(), GraphicsUnit.Pixel);
                draw.Corner.Y += (Height - offset);
                e.Graphics.DrawImage( images[orientation], draw.ToRectangle(), source[1][0][2].ToRectangle(), GraphicsUnit.Pixel);
                draw.Sizes.Y = Height - (offset + offset); draw.Y = offset;
                e.Graphics.DrawImage( images[orientation], draw.ToRectangle(), source[1][0][1].ToRectangle(), GraphicsUnit.Pixel);
            }
        }

        private void drawCurve( Graphics ctx, int idx, Point32 p1, Point32 p2 )
        {
            Point eins = p1;
            Point zwei = p2;
            int level = (int)(((values[idx].Proportion * 2.0f) - 1.0f) * offset);
            if (Orientation == Orientation.Horizontal) {
                eins.Y += level;
                zwei.Y = eins.Y;
            } else {
                eins.X += level;
                zwei.X = eins.X;
            } ctx.DrawBezier(curve, idx > 0 ? p1 : eins, eins, zwei,idx == ranges.Count ? zwei : p2 );
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            base.OnPaint( e );
            PaintBackground( e );
            CenterAndScale rect = new CenterAndScale(offset,offset,offset,offset);
            drawCurve( e.Graphics, 0, rect.Center, ranges[0].areal.Center );
            
            int count = ranges.Count;
            for( int i = 0; i < count; ++i ) {
                rect = ranges[i].areal;
                rect.Scale.x = rect.Scale.y = (short)(
                    (rect.Scale.y > rect.Scale.x
                   ? rect.Scale.x : rect.Scale.y) / 2);
                glimmer.Led = ranges[i].color;
                glimmer.Lum = Math.Abs( values[i].Level );
                glimmer.DrawBrush( e.Graphics, rect );
                rect.Scale *= 2;
                glimmer.DrawSprite( e.Graphics, rect.ToRectangle() );
                e.Graphics.DrawImage(
                    images[orientation], ranges[i].areal.ToRectangle(),
                    source[orientation-1][1][mipmap].ToRectangle(), GraphicsUnit.Pixel
                );
                if (i < count-1)
                drawCurve( e.Graphics, i+1, rect.Center, ranges[i+1].areal.Center );
            }

            if (Orientation == Orientation.Horizontal)
            drawCurve( e.Graphics, count, rect.Center, new Point32(Width-offset,offset) );
            else drawCurve( e.Graphics, count, rect.Center, new Point32(offset,Height-offset) );
        }

        #region IInterValuable

        void IInterValuable.callOnInvalidated( InvalidateEventArgs e )
        {
            OnInvalidated( e );
        }

        Action IInterValuable.getInvalidationTrigger()
        {
            return ValenceUpdate;  // Invalidate;
        }

        private void ValenceUpdate()
        {
            for (int i=0;i<values.Length;++i ) {
                values[i].Level = values[i].Level;
            } Invalidate();
        }

        ToolStripItemCollection IInterValuable.getMenuHook()
        {
            return mnu_config.Items;
        }

        void IInterValuable.Invalidate()
        {
            Invalidate();
        }

        IControllerValenceField<Controlled.Float32> IInterValuable<Controlled.Float32>.valence()
        {
            IControllerValenceField v = getter();
            return v.field<Controlled.Float32>( v.Field.Indices[0] );
        }

        public IControllerValenceField<Controlled.Float32> valence( Enum field )
        {
            if ( field.ToInt32() != SplitCount )
                return getter().field<Controlled.Float32>( field );
            else throw new Exception(string.Format("ValenceField '{0}' is not Float32",field));
        }

        IControllerValenceField IInterValuable.valence<cT>()
        {
            return getter().field<cT>();
        }

        IControllerValenceField IInterValuable.valence<cT>( Enum field )
        {
            if( field.ToValue() == SplitCount && typeof(cT) == typeof(Controlled.Int32) )
                return getter().field<Controlled.Int32>( field );
            else if( typeof(cT) == typeof(Controlled.Float32) )
                return getter().field<Controlled.Float32>(field);
            else throw new Exception(
                string.Format( "ValenceField '{0}' is not of type {1}",
                  field, typeof(cT) ) );
        }

#endregion
#region ITouchGesturedElement

        internal protected ITouchGesturedElement<GuiSlider> getInTouch() {
            return this as ITouchGesturedElement<GuiSlider>;
        }

        private TouchGesturesHandler<GuiRanger> touchhandler;
                TouchGesturesHandler<GuiRanger> ITouchGesturedElement<GuiRanger>.handler() {
            return touchhandler;
        }

        public bool IsTouched {
            get { return touchhandler.IsTouched; }
        }

        public event FingerTip.TouchDelegate TouchDown {
            add { touchhandler.events().TouchDown += value; }
            remove { touchhandler.events().TouchDown -= value; }
        }
        public event FingerTip.TouchDelegate TouchMove {
            add { touchhandler.events().TouchMove += value; }
            remove { touchhandler.events().TouchMove -= value; }
        }
        public event FingerTip.TouchDelegate TouchLift {
            add { touchhandler.events().TouchLift += value; }
            remove { touchhandler.events().TouchLift -= value; }
        }

        public event FingerTip.TouchDelegate TouchTapped {
            add { touchhandler.events().TouchTapped += value; }
            remove { touchhandler.events().TouchTapped -= value; }
        }
        public event FingerTip.TouchDelegate TouchDupple {
            add { touchhandler.events().TouchDupple += value; }
            remove { touchhandler.events().TouchDupple -= value; }
        }
        public event FingerTip.TouchDelegate TouchTrippl {
            add { touchhandler.events().TouchTrippl += value; }
            remove { touchhandler.events().TouchTrippl -= value; }
        }

        public event MultiFinger.TouchDelegate TouchDraged {
            add { touchhandler.events().TouchDraged += value; }
            remove { touchhandler.events().TouchDraged -= value; }
        }
        public event MultiFinger.TouchDelegate TouchResize {
            add { touchhandler.events().TouchResize += value; }
            remove { touchhandler.events().TouchResize -= value; }
        }
        public event MultiFinger.TouchDelegate TouchRotate {
            add { touchhandler.events().TouchRotate += value; }
            remove { touchhandler.events().TouchRotate -= value; }
        }

        public void OnTouchTapped(FingerTip tip )
        {}
        public void OnTouchDupple(FingerTip tip )
        {}

        public void OnTouchTrippl(FingerTip tip )
        {}

        public void OnTouchDraged(MultiFinger tip)
        {}

        public void OnTouchResize(MultiFinger tip)
        {}

        public void OnTouchRotate(MultiFinger tip)
        {}

        public void OnTouchDown( FingerTip tip )
        {
            StartInteract( tip.Position, tip.Id );
        }

        public void OnTouchMove(FingerTip tip)
        {}

        public void OnTouchLift(FingerTip tip)
        {}

        private void TouchInputReady( PointerInput inst )
        {
            PointerInput.Initialized -= TouchInputReady;
            inst.RegisterTouchableElement( this );
        }

        Point64 ITouchableElement.ScreenLocation() {
            return PointToScreen( Location );
        }

        IRectangle ITouchableElement.ScreenRectangle() {
            return AbsoluteEdges.FromRectangle( RectangleToScreen( touch.Bounds ) );
        }

        ITouchDispatchTrigger ITouchable.screen() {
            return touchhandler.screen();
        }

        ITouchableElement ITouchable.element() {
            return this;
        }

        Control ITouchable.Element {
            get { return this; }
        }

        public ITouchEventTrigger touch {
            get { return touchhandler; }
        }
        #endregion

    }
}
