using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Stepflow.Gui.Geometry;

namespace MidiGUI.Test
{
    public static class Extensions
    {
        public static Consola.Test.ConTrol.Point ConTrolPoint( this Point32 point )
        {
            return new Consola.Test.ConTrol.Point(point.X, point.Y);
        }

        public static Consola.Test.ConTrol.Point ConTrolPoint( this Point64 point )
        {
            return new Consola.Test.ConTrol.Point(point.x, point.y);
        }

        public static Consola.Test.Area ConTrolArea( this IRectangle rectangle )
        {
            return new Consola.Test.Area( rectangle.Corner.ConTrolPoint(), rectangle.Sizes.ConTrolPoint() );
        }

        public static Consola.Test.ConTrol.Point ToPoint( this Point32 cast )
        {
            return new Consola.Test.ConTrol.Point(cast.X, cast.Y);
        }

        public static Point32 ToPoint32( this Consola.Test.ConTrol.Point cast )
        {
            return new Point32(cast.X, cast.Y);
        }
    }

    public static class Helper
    {
        public static float lerp( float pos, float bis, float val )
        {
            return val * ( pos / bis );
        }
    }

    public enum ControlFlags
    {
        Cycled, Inverted, Normal
    }
}
