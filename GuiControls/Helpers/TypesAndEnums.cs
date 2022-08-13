using System;
using Stepflow;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;
using Resources = GuiControls.Properties.Resources;
#if USE_WITH_WF
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Point = System.Drawing.Point;
using Rect = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point  = System.Windows.Point;
using Rect   = System.Windows.Int32Rect;
using RectF  = System.Windows.Rect;
using 
#endif


namespace System
{
    public static class Extensions
    {
        private static IFormatProvider fmtr = GuiControls.Properties.Resources.Culture;
        public static Int32 ToInt32( this Enum value )
        {
            return (value as IConvertible).ToInt32( fmtr );
        }
        public static long ToValue( this Enum value )
        {
            return (value as IConvertible).ToInt64( fmtr );
        }
        public static UInt64 ToUInt64( this Enum value )
        {
            return (value as IConvertible).ToUInt64( fmtr );
        }

        public static Stepflow.Gui.Geometry.IRectangle ToIRectangle( this Rectangle rectangle )
        {
            return new Stepflow.Gui.Geometry.SystemDefault( rectangle );
        }
    }
}

namespace Stepflow.Gui
{
    public class ValueChangeArgs<T> : System.EventArgs where T : IConvertible
    {
        public readonly T Value;
        public ValueChangeArgs(T value)
        {
            Value = value;
        }
        public static implicit operator T(ValueChangeArgs<T> cast)
        {
            return cast.Value;
        }
        public static implicit operator ValueChangeArgs<T>(T cast)
        {
            return new ValueChangeArgs<T>(cast);
        }
    }

    public delegate void ValueChangeDelegate<T>( object sender, ValueChangeArgs<T> value ) where T : IConvertible;
    public delegate void ControlledValueWrap<CT>( object sender, CT controller ) where CT : ControllerBase;

    public enum Style : int
    {
        Flat = 0,
        Lite = 1,
        Dark = 2
    };

    public enum LedButtonValence
    {
        State,
        Chain
    };

    public class RangeController : Controlled.Float32
    {
        private IntPtr Unsign;
        private IntPtr Cycled;

        private float CheckFunc( ref float val, ref float min, ref float max, ref float mov )
        {
            return -(mov = -(val = val > max ? max : val < min ? min : val));
        }
        public RangeController() : base()
        {
            Mode = ControlMode.Clamp;
            MIN = 0;
            MAX = 100;
            MOV = -1;
            VAL = 1;
            SetCheckAtSet();
        }
        public bool Join( ControllerBase that )
        {
            if( that.GetTypeCode() != TypeCode.Float32 ) return false;
            AttachedDelegate = CheckFunc;
            this.LetPoint( ControllerVariable.VAL, that.GetPointer( ControllerVariable.MAX ) );
            this.LetPoint( ControllerVariable.MOV, that.GetPointer( ControllerVariable.MIN ) );
            Unsign = IntPtr.Zero;
            Cycled = IntPtr.Zero;
            return true;
        }
    }

    public struct StyleSet
    {
        public static readonly  Color Dark = Color.FromArgb(32,32,32);
        public readonly Bitmap  image;
        public readonly Color   color;
        public readonly Color   value;
        public readonly Color   units;
        public StyleSet( Bitmap img, Color col,
                         Color  val, Color unt ) {
           image = img; color = col;
           value = val; units = unt;
        }
    };

    public enum ChannelMode
    {
        Mono = 1,
        Dual = 2
    };

    public enum LED : byte {
        Green, Red, Gelb, Blue,
        Orange, Mint, Pink,
        Cyan, off, EMPTY = 0xFF
    };

    public enum MipMap : byte {
        Small, Medium, Large
    }

    public enum UnitsType
    {
        CUSTOM, Hz, Db, V, Px, l, m, A, sec, Per
    };

    public enum UnitScale
    {
        p = -3, y = -2, m = -1, 
        Base = 0, K=1, M=2, G=3
    };

    public enum Orientation
    {
        Rondeal = 0,
    #if USE_WITH_WF
        Horizontal = System.Windows.Forms.Orientation.Horizontal + 1,
        Vertical = System.Windows.Forms.Orientation.Vertical + 1,
    #elif USE_WITH_WPF
        Horizontal = System.Windows.Controls.Orientation.Horizontal + 1,
        Vertical = System.Windows.Controls.Orientation.Vertical + 1,
    #endif
    };


    public enum DirectionalModification
    {
        None = 0,
        Proportional = 1,
    #if USE_WITH_WF
        Inverse = System.Windows.Forms.RightToLeft.Yes << 2,
    #elif USE_WITH_WPF
        Inverse = System.Windows.FlowDirection.RightToLeft << 2
    #endif
        AntiProportional = Inverse | Proportional,
    }

    public enum RondealDirection
    {
        ClockWise    = Orientation.Rondeal,
        CounterClock = DirectionalModification.Inverse | Orientation.Rondeal
    }

    public enum LinearDirection
    {
        Right = Orientation.Horizontal,
        Up    = Orientation.Vertical,
        Left  = DirectionalModification.Inverse | Orientation.Horizontal,
        Down  = DirectionalModification.Inverse | Orientation.Vertical
    }

    public enum DirectionalOrientation
    {
        ClockWise = RondealDirection.ClockWise,
        Right = LinearDirection.Right,
        Up = LinearDirection.Up,
        CounterClock = RondealDirection.CounterClock,
        Left =  LinearDirection.Left,
        Down = LinearDirection.Down
    };

    public enum MixAndFeel
    {
        Acurate, // means nothing
        Realistic // means controles behave like 'real' music instrumental hardware (fader nupsies may flup off when moved much too fast)
    };

    public enum RondealInteraction
    {
        Linear, XFactor, ByAngel, VierQuadranten
    }
}
