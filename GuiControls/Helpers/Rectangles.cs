using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Win32Imports.Touch;
#if USE_WITH_WF
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
#else
using Point = System.Windows.Point;
#endif


namespace Stepflow.Gui
{

    public enum StorageLayout
    {
        CenterAndScale,
        CornerAndSizes,
        AbsoluteEdges,
        SystemDefault
    }
    
    /// <summary> PointPT (struct)
    /// A structure which defines a point in 2D space via two 
    /// individual pointer values, each pointing a distant variables.
    /// </summary>
    public unsafe struct PointPT
    {
        public IntPtr pX;
        public IntPtr pY;

        public PointPT( ref Point32 refereto ) {
            fixed( short* p= &refereto.x ) {
                pX = new IntPtr( p );
                pY = new IntPtr(p+1);
            }
        }

        public PointPT( ref short ixo, ref short ypso ) {
            fixed( short* p= &ixo ) {
                pX = new IntPtr( p ); }
            fixed( short* p= &ypso ) {
                pY = new IntPtr( p ); }
        }

        public PointPT( IntPtr xptr, IntPtr ypsp ) {
            pX = xptr;
            pY = ypsp;
        }

        public static implicit operator Point32( PointPT cast ) {
            return new Point32(
                *(short*)cast.pX.ToPointer(),
                *(short*)cast.pY.ToPointer()
            );
        }

        public int X {
            get{ return *(short*)pX.ToPointer(); }
            set { *(short*)pX.ToPointer() = (short)value; }
        }
        public int Y {
            get{ return *(short*)pY.ToPointer(); }
            set { *(short*)pY.ToPointer() = (short)value; }
        }

        public void Set( Point32 point )
        {
            X = point.x;
            Y = point.y;
        }

    }


    #region IRectangle Interface definition
    // Interface for objects representing geometrical 'Rectangle' shaped, two dimensional areas 
    // which makes able assigning and comparing instances which implement it against each other 
    // in a way which doesn't needs to care about implementation details, storage model layout

    public interface IRectangleCompounds
    {
        Point32 Corner { get; set; }
        Point32 Center { get; set; }
    
        Point32 Sizes { get; set; }
        Point32 Scale { get; set; }

        PointPT  CompoundA { get; }
        PointPT  CompoundB { get; }
    }
    
    public interface IRectangleValues
    {
        int X{ get; set; }
        int Y{ get; set; }
        int W{ get; set; }
        int H{ get; set; }
        int L{ get; set; }
        int R{ get; set; }
        int T{ get; set; }
        int B{ get; set; }
    }
    
    public interface IRectanglePointers
    {
        IntPtr pA1{ get; set; }
        IntPtr pA2{ get; set; }
        IntPtr pB1{ get; set; }
        IntPtr pB2{ get; set; }
    }

    // IRectangle (interface) - use this for handling objects which implement the interface
    public interface IRectangle : IRectangleValues, IRectangleCompounds
    {
        IRectangle converted<RectangleData>() where RectangleData : struct, IRectangle;
        IRectangle<RType> cast<RType>() where RType : struct, IRectangle;
        Rectangle ToRectangle();
        bool Contains( IRectangle other );
        bool Contains( Point64 point );
        bool Intersect( IRectangle other );
        StorageLayout StorageLayout { get; }
    }
    
    public interface IRectanglePtrs<RectangleData>
        : IRectangle
        , IRectanglePointers
    where RectangleData
        : struct
        , IRectangle
    {
        IRectanglePtrs<RectangleData> refereTo( ref RectangleData from );
        IRectanglePtrs<RectangleData> casted();
        IRectanglePtrs<RectangleData> copied();
        RectangleData                 resolve();
    }

    public interface IRectangle<RectangleData> : IRectangle where RectangleData : struct, IRectangle
    {
        IRectangle<RectangleData>         casted();
        RectangleData                     copied();
        RectangleReference<RectangleData> refere();
        RectangleData                     FromRectangle( Rectangle from );
    }

    #endregion

    #region ValueType based Rectangles
    // which using different kinds of data storage models and layout kinds

    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct RectangleData
    {
        public static readonly RectangleData None = new RectangleData();
        [FieldOffset(0)] public fixed byte data[8];
        [FieldOffset(0)] public Point32 compound1;
        [FieldOffset(4)] public Point32 compound2;
    }

    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct CenterAndScale
        : IRectangle<CenterAndScale> {
        public static readonly CenterAndScale None = new CenterAndScale();
        [FieldOffset(0)] internal RectangleData Abstract; 
        [FieldOffset(0)] public   Point32 Center;
        [FieldOffset(4)] public   Point32 Scale;
    
 
        public CenterAndScale( int centerX, int centerY, int scaleX, int scaleY ) : this() {
            Center.x = (short)centerX;
            Center.y = (short)centerY;
            Scale.x = (short)scaleX;
            Scale.y = (short)scaleY;
        }

        Point32 IRectangleCompounds.Center {
            get {return  Center; }
            set { Center = value; }
        }
        Point32 IRectangleCompounds.Scale {
            get { return Scale; }
            set { Scale = value; }
        }
    
        public Point32 Corner {
            get { return Center - Scale; }
            set { Center = value + Scale; }
        }
        public Point32 Sizes {
            get { return Scale * 2; }
            set { Center = (Center - Scale) + (Scale = (value / 2)); }
        }
        
        public int X { get { return (short)(Center.x - Scale.x); } set { value = (short)((value -= X) / 2); Center.X += value; Scale.X += value; } }
        public int Y { get { return (short)(Center.y - Scale.y); } set { value = (short)((value -= Y) / 2); Center.Y += value; Scale.Y += value; } }
        public int W { get { return (short)(Scale.x + Scale.x); } set { Center.x += (short)((value /= 2)-Scale.x); Scale.X = value; } }
        public int H { get { return (short)(Scale.y + Scale.y); } set { Center.y += (short)((value /= 2)-Scale.y); Scale.Y = value; } }
        public int L { get { return X; } set { X = value; } }
        public int T { get { return Y; } set { Y = value; } }
        public int R { get { return (short)(Center.x + Scale.x); } set { value = (short)((value -= R) / 2); Center.X += value; Scale.X += value; } }
        public int B { get { return (short)(Center.y + Scale.y); } set { value = (short)((value -= B) / 2); Center.Y += value; Scale.Y += value; } }

        public PointPT CompoundA { get{ return new PointPT(ref Center); } }
        public PointPT CompoundB { get{ return new PointPT(ref Scale); } }

        public StorageLayout StorageLayout {
            get { return StorageLayout.CenterAndScale; }
        }

        public IRectangle<CenterAndScale> casted() {
            return this;
        }
        IRectangle<RType> IRectangle.cast<RType>()
        {
            if( typeof(RType) != typeof(CenterAndScale) )
                return Rectangle<CenterAndScale>.Convert<RType>( this ).cast<RType>();
            else 
                return casted() as IRectangle<RType>;
        }
        IRectangle IRectangle.converted<RectangleData>() {
            return Rectangle<CenterAndScale>.Convert<RectangleData>(this);
        }

        RectangleReference<CenterAndScale> IRectangle<CenterAndScale>.refere()
        {
            return new CenterAndScalePointers( ref this );
        }

        CenterAndScale IRectangle<CenterAndScale>.copied()
        {
            CenterAndScale clone = None;
            clone.Center = Center;
            clone.Scale = Scale;
            return clone;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle( X, Y, W, H );
        }
        public static CenterAndScale FromRectangle( Rectangle from )
        {
            CenterAndScale to = CenterAndScale.None;
            to.Scale = new Point32( (Point)from.Size ) / 2;
            to.Center = to.Scale + from.Location;
            return to;
        }

        CenterAndScale IRectangle<CenterAndScale>.FromRectangle( Rectangle from )
        {
            return CenterAndScale.FromRectangle( from );
        }

        public bool Contains(IRectangle other)
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains(Point64 point)
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersect( IRectangle other )
        {
            return Contains(other.Corner)
                || Contains(other.Center + other.Scale)
                || Contains(other.Center + other.Scale.flypst())
                || Contains(other.Center + other.Scale.flixed());
        }

    }
    
    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct CornerAndSize
        : IRectangle<CornerAndSize> {
        public static readonly CornerAndSize None = new CornerAndSize();
        [FieldOffset(0)] internal RectangleData Abstract; 
        [FieldOffset(0)] public Point32 Corner;
        [FieldOffset(0)] public short   X;
        [FieldOffset(0)] public short   L;
        [FieldOffset(2)] public short   Y;
        [FieldOffset(2)] public short   T;
        [FieldOffset(4)] public Point32 Sizes;
        [FieldOffset(4)] public short   W;
        [FieldOffset(6)] public short   H;

        public CornerAndSize( int x, int y, int w, int h ) : this() {
            X = (short)x;
            Y = (short)y;
            W = (short)w;
            H = (short)h;
        }
    
        Point32 IRectangleCompounds.Corner { get { return Corner; } set { Corner = value; } }
        Point32 IRectangleCompounds.Sizes { get { return Sizes; } set { Sizes = value; } }

        int IRectangleValues.X { get {return X; } set { X = (short)value; } }
        int IRectangleValues.Y { get {return Y; } set { Y = (short)value; } }
        int IRectangleValues.W { get {return W; } set { W = (short)value; } }
        int IRectangleValues.H { get {return H; } set { H = (short)value; } }
        int IRectangleValues.L { get {return L; } set { L = (short)value; } }
        int IRectangleValues.T { get {return T; } set { T = (short)value; } }
    
    
        public Point32 Center { get{ return Corner + Scale; } set{ Scale = value - Corner; } }
        public Point32 Scale { get{ return Sizes / 2; } set{ Sizes = value * 2;  } }
        public int R { get{ return (short)(X + W); } set{ L = (short)(value - W); } }
        public int B { get{ return (short)(Y + H); } set{ T = (short)(value - H); } }

        public PointPT CompoundA { get{ return new PointPT(ref Corner); } }
        public PointPT CompoundB { get{ return new PointPT(ref Sizes); } }

        public StorageLayout StorageLayout {
            get { return StorageLayout.CornerAndSizes; }
        }

        IRectangle IRectangle.converted<RectangleData>() {
            return Rectangle<CornerAndSize>.Convert<RectangleData>(this);
        }

        CornerAndSize IRectangle<CornerAndSize>.copied() {
            CornerAndSize clone = None;
            clone.Abstract = Abstract;
            return clone;
        }

        RectangleReference<CornerAndSize> IRectangle<CornerAndSize>.refere() {
            return new CornerAndSizePointers( ref this );
        }

        public IRectangle<CornerAndSize> casted() {
            return this;
        }

        IRectangle<RType> IRectangle.cast<RType>() {
            if( typeof(RType) != typeof(CornerAndSize) )
                return Rectangle<CornerAndSize>.Convert<RType>(this).cast<RType>();
            else 
                return casted() as IRectangle<RType>;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle( X, Y, W, H );
        }

        public static CornerAndSize FromRectangle( Rectangle from )
        {
            return new CornerAndSize( from.X, from.Y, from.Width, from.Height );
        }

        CornerAndSize IRectangle<CornerAndSize>.FromRectangle( Rectangle from )
        {
            return CornerAndSize.FromRectangle( from );
        }

        public bool Contains(IRectangle other)
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains( Point64 point )
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersect( IRectangle other )
        {
            return Contains(other.Corner)
                || Contains(other.Center + other.Scale)
                || Contains(other.Center + other.Scale.flypst())
                || Contains(other.Center + other.Scale.flixed());
        }

    }

    [StructLayout(LayoutKind.Explicit,Size = 8)]
    public unsafe struct AbsoluteEdges
        : IRectangle<AbsoluteEdges> {
        public static readonly AbsoluteEdges None = new AbsoluteEdges();
        [FieldOffset(0)] internal RectangleData Abstract; 
        [FieldOffset(0)] public Point32 Corner;
        [FieldOffset(0)] public short X;
        [FieldOffset(0)] public short L;
        [FieldOffset(2)] public short Y;
        [FieldOffset(2)] public short T;
        [FieldOffset(4)] public short R;
        [FieldOffset(6)] public short B;
    
        public AbsoluteEdges( int l, int r, int t, int b ) : this() {
            L = (short)l;
            R = (short)r;
            T = (short)t;
            B = (short)b;
        }

        Point32 IRectangleCompounds.Corner { get { return  Corner; } set { Corner = value; } }

        int IRectangleValues.L { get { return  L; } set { L = (short)value; } }
        int IRectangleValues.T { get { return  T; } set { T = (short)value; } }
        int IRectangleValues.X { get { return  X; } set { X = (short)value; } }
        int IRectangleValues.Y { get { return  Y; } set { Y = (short)value; } }
        int IRectangleValues.R { get { return  R; } set { R = (short)value; } }
        int IRectangleValues.B { get { return  B; } set { B = (short)value; } }
    
        
        public Point32 Center { get{ return new Point32((L+R)/2,(T+B)/2); } set{ Point32 m = value - Center; Corner += m; R += m.x; B += m.y; } }
        public Point32 Sizes { get{ return new Point32(W,H); } set{ W = value.x; H = value.y; } } 
        public Point32 Scale { get{ return Center - Corner; } set{ Point32 s = Scale;  value -= s; Corner -= value; R += value.x; B += value.y; } }
    
        public int W { get { return (short)(R - L); } set { L = (short)((R = Center.x) - (value/=2)); R += (short)value; } }
        public int H { get { return (short)(B - T); } set { T = (short)((B = Center.y) - (value/=2)); B += (short)value; } }

        public PointPT CompoundA { get{ return new PointPT(ref Abstract.compound1); } } 
        public PointPT CompoundB { get{ return new PointPT(ref Abstract.compound2); } }

        public StorageLayout StorageLayout {
            get { return StorageLayout.AbsoluteEdges; }
        }

        IRectangle IRectangle.converted<RectangleData>()
        {
            return Rectangle<AbsoluteEdges>.Convert<RectangleData>(this);
        }

        public IRectangle<AbsoluteEdges> casted()
        {
            return this;
        }

        AbsoluteEdges IRectangle<AbsoluteEdges>.copied()
        {
            AbsoluteEdges clone = None;
            clone.Abstract = this.Abstract;
            return clone;
        }

        RectangleReference<AbsoluteEdges> IRectangle<AbsoluteEdges>.refere()
        {
            return new AbsoluteEdgesPointers( ref this );
        }


        IRectangle<RType> IRectangle.cast<RType>()
        {
            if( typeof(RType) != typeof(AbsoluteEdges) )
                return Rectangle<AbsoluteEdges>.Convert<RType>(this).cast<RType>();
            else 
                return casted() as IRectangle<RType>;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X,Y,W,H);
        }

        public static AbsoluteEdges FromRectangle( Rectangle from )
        {
            AbsoluteEdges to = AbsoluteEdges.None;
            to.Corner = new Point32(from.Location);
            to.R = (short)from.Right;
            to.B = (short)from.Bottom;
            return to;
        }

        AbsoluteEdges IRectangle<AbsoluteEdges>.FromRectangle( Rectangle from )
        {
            return AbsoluteEdges.FromRectangle( from );
        }

        public bool Contains(IRectangle other)
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains(Point64 point)
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersect( IRectangle other )
        {
            return Contains(other.Corner)
                || Contains(other.Center + other.Scale)
                || Contains(other.Center + other.Scale.flypst())
                || Contains(other.Center + other.Scale.flixed());
        }
    }

    [StructLayout(LayoutKind.Explicit,Size = 16)]
    public unsafe struct SystemDefault
        : IRectangle<SystemDefault>
    {
        [FieldOffset(0)]
        private Rectangle data;
        [FieldOffset(0)]
        private short x;
        [FieldOffset(0)]
        public int X;
        [FieldOffset(4)]
        private short y;
        [FieldOffset(4)]
        public int Y;
        [FieldOffset(8)]
        private short w;
        [FieldOffset(8)]
        public int W;
        [FieldOffset(12)]
        private short h;
        [FieldOffset(12)]
        public int H;

        public SystemDefault( Rectangle from ) : this()
        {
            data = from;
        }
 
        public static implicit operator Rectangle( SystemDefault cast )
        {
            return cast.data;
        }

        public Rectangle Assign( Rectangle rectangle )
        {
            return data = rectangle;
        }

        public int L {
            get { return data.Left; }
            set { int r = R; data.X = value; data.Width = (r - value);
                if( data.Width < 0 ) {
                    data.Width = -data.Width;
                    data.X = r;
                }
            }
        }
        
        public int R {
            get { return data.Right; }
            set { data.Width = value - data.X;
                if( data.Width < 0 ) {
                    int neuX = R;
                    data.Width = -data.Width;
                    data.X = neuX;
                }
            }
        }

        public int T {
            get { return data.Top; }
            set { int b = B; data.Y = value; data.Width = (b - value);
                if( data.Height < 0 ) {
                    data.Height = -data.Height;
                    data.Y = b;
                }
            }
        }

        public int B {
            get { return data.Bottom; }
            set { data.Height = value - data.Y;
                if( data.Height < 0 ) {
                    int neuY = B;
                    data.Width = -data.Width;
                    data.Y = neuY;
                }
            }
        }

        public Point32 Center {
            get { return new Point32(data.X + data.Width / 2, data.Y + data.Height / 2); }
            set { Point32 delta = value - Center;  data.X += delta.x;  data.Y += delta.y; }
        }

        public PointPT CompoundA {
            get { return new PointPT( ref x, ref y ); }
        }

        public PointPT CompoundB {
            get { return new PointPT( ref w, ref h ); }
        }

        public Point32 Corner {
            get { return new Point32(x, y); }
            set { x = value.x; y = value.y; }
        }

        int IRectangleValues.W {
            get { return data.Width; }
            set { data.Width = value; }
        }

        int IRectangleValues.H {
            get { return data.Height; }
            set { data.Height = value; }
        }

        public Point32 Scale {
            get { return new Point32(data.Width / 2, data.Height / 2); }
            set { Point32 center = Center; data.Size = value; Center = center; }
        }

        public Point32 Sizes {
            get { return new Point32(x,y); }
            set { data.Size = value; }
        }

        public StorageLayout StorageLayout {
            get { return StorageLayout.SystemDefault; }
        }

        int IRectangleValues.X {
            get { return data.X; }
            set { data.X = value; }
        }

        int IRectangleValues.Y {
            get { return data.Y; }
            set { data.Y = value; }
        }

        public IRectangle<RType> cast<RType>() where RType : struct, IRectangle
        {
            if( typeof(RType) != typeof(SystemDefault) )
                return Rectangle<SystemDefault>.Convert<RType>(this).cast<RType>();
            else 
                return casted() as IRectangle<RType>;
        }

        public IRectangle<SystemDefault> casted()
        {
            return this;
        }

        public bool Contains( Point64 point )
        {
            return data.Contains( point );
        }

        public bool Contains( IRectangle other )
        {
            return data.Contains( other.ToRectangle() );
        }

        public IRectangle converted<RectangleData>() where RectangleData : struct, IRectangle
        {
            return Rectangle<RectangleData>.Create( StorageLayout, data.X, data.Y, data.Width, data.Height );
        }

        public SystemDefault copied()
        {
            return new SystemDefault( data );
        }

        public static SystemDefault FromRectangle( Rectangle from )
        {
            return new SystemDefault( from );
        }

        SystemDefault IRectangle<SystemDefault>.FromRectangle( Rectangle from )
        {
            return SystemDefault.FromRectangle( from );
        }

        public bool Intersect( IRectangle other )
        {
            return data.IntersectsWith( other.ToRectangle() );
        }

        public RectangleReference<SystemDefault> refere()
        {
            throw new Exception("cannot create reference to SystemDefault rectangle instance");
        }

        public Rectangle ToRectangle()
        {
            return data;
        }
    }
    #endregion

    #region Reference based Rectangles
    // (consisting from 4 pointers, each independantly pointing a distant variable)

    public abstract class RectangleReference<RactanglePointerType>
        : IRectanglePtrs<RactanglePointerType>
    where RactanglePointerType
        : struct, IRectangle
    {
        public IntPtr pA1;
        public IntPtr pA2;
        public IntPtr pB1;
        public IntPtr pB2;

        public RectangleReference()
        {
            pA1 = IntPtr.Zero;
            pA2 = IntPtr.Zero;
            pB1 = IntPtr.Zero;
            pB2 = IntPtr.Zero;
        }

        public RectangleReference(ref Point32 a, ref Point32 b)
        { unsafe {
              fixed ( Point32* p = &a ) {
                pA1 = new IntPtr(&p->x);
                pA2 = new IntPtr(&p->y);
            } fixed ( Point32* p = &b ) {
                pB1 = new IntPtr(&p->x);
                pB2 = new IntPtr(&p->y);
            } }
        }

        public RectangleReference( ref short a1, ref short a2, ref short b1, ref short b2 )
        { unsafe {
            fixed ( short* p = &a1) {
                pA1 = new IntPtr(p); 
            } fixed( short* p = &a2) {
                pA2 = new IntPtr(p);
            } fixed( short* p = &b1) {
                pB1 = new IntPtr(p);
            } fixed( short* p = &b2) {
                pB2 = new IntPtr(p);
            }
          }
        }

        public RectangleReference( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
        {
            pA1 = a1;
            pA2 = a2;
            pB1 = b1;
            pB2 = b2;
        }

        public void RefereCompound( int cN, ref Point32 point )
        { unsafe {
            fixed (Point32* p = &point) {
                if( cN == 1 ) {
                         pA1 = new IntPtr(&p->x);
                         pA2 = new IntPtr(&p->y);
                } else { pB1 = new IntPtr(&p->x);
                         pB2 = new IntPtr(&p->y);
                }
            }
        } }
        public void RefereCompound( int cN, ref short x, ref short y )
        { unsafe {
              fixed (short* p = &x) {
                if( cN == 1 ) pA1 = new IntPtr(p);
                         else pB1 = new IntPtr(p);
            } fixed (short* p = &y) {
                if( cN == 1 ) pA2 = new IntPtr(p);
                         else pB2 = new IntPtr(p);
            }
        } }
        public void RefereValue( int idx, IntPtr val )
        {
            switch( idx ) {
                case 0: pA1 = val; break;
                case 1: pA2 = val; break;
                case 2: pB1 = val; break;
                case 3: pB2 = val; break;
            }
        }

        public abstract int X { get; set; }
        public abstract int Y { get; set; }
        public abstract int W { get; set; }
        public abstract int H { get; set; }
        public abstract int L { get; set; }
        public abstract int R { get; set; }
        public abstract int T { get; set; }
        public abstract int B { get; set; }
        public abstract Point32 Corner { get; set; }
        public abstract Point32 Center { get; set; }
        public abstract Point32 Sizes { get; set; }
        public abstract Point32 Scale { get; set; }
        public abstract PointPT CompoundA { get; }
        public abstract PointPT CompoundB { get; }

        public abstract StorageLayout StorageLayout { get; }

        IntPtr IRectanglePointers.pA1 {
            get { return pA1; }
            set { pA1 = value; }
        }

        IntPtr IRectanglePointers.pA2 {
            get { return pA2; }
            set { pA2 = value; }
        }

        IntPtr IRectanglePointers.pB1 {
            get { return pB1; }
            set { pB1 = value; }
        }

        IntPtr IRectanglePointers.pB2 {
            get { return pB2; }
            set { pB2 = value; }
        }

        public abstract IRectangle converted<RectangleData>() where RectangleData : struct, IRectangle;

        IRectangle<RType> IRectangle.cast<RType>()
        {
            if ( typeof(RType) != typeof(RactanglePointerType) ) {
                return Rectangle<RectangleReference<RactanglePointerType>>.Convert<RType>( this ).cast<RType>();
            } else {
                return converted<RType>().cast<RType>();
            }
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle( X, Y, W, H );
        }

        public bool Contains( IRectangle other )
        {
            return L <= other.L && T <= other.T
                && R >= other.R && B >= other.B;   
        }

        public bool Contains( Point64 point )
        {
            return point.x > L && point.x < R
                && point.y > T && point.y < B; 
        }

        public bool Intersect( IRectangle other )
        {
            return Contains( other.Center + other.Scale )
                || Contains( other.Center - other.Scale )
                || Contains( other.Center + other.Scale.flixed() )
                || Contains( other.Center + other.Scale.flypst() );
        }

        public RactanglePointerType resolve()
        {
            return (RactanglePointerType)Rect64.Create<RactanglePointerType>( CompoundA.X, CompoundA.Y, CompoundB.X, CompoundB.Y );
        }

        public IRectanglePtrs<RactanglePointerType> refereTo( ref RactanglePointerType that )
        {
            pA1 = that.CompoundA.pX;
            pA2 = that.CompoundA.pY;
            pB1 = that.CompoundB.pX;
            pB2 = that.CompoundB.pY;
            return this;
        }

        public IRectanglePtrs<RactanglePointerType> casted()
        {
            return this;
        }

        public IRectanglePtrs<RactanglePointerType> copied()
        {
            IRectanglePtrs<RactanglePointerType> clone = null;
            switch( this.StorageLayout ) {
                case StorageLayout.CornerAndSizes: clone = (IRectanglePtrs<RactanglePointerType>)Rectangle<CornerAndSizePointers>.Create(); break;
                case StorageLayout.CenterAndScale: clone = (IRectanglePtrs<RactanglePointerType>)Rectangle<CenterAndScalePointers>.Create(); break;
                case StorageLayout.AbsoluteEdges: clone = (IRectanglePtrs<RactanglePointerType>)Rectangle<AbsoluteEdgesPointers>.Create(); break;
            }
            clone.pA1 = this.pA1;
            clone.pA2 = this.pA2;
            clone.pB1 = this.pB1;
            clone.pB2 = this.pB2;
            return clone;
        }

    }

    public class CornerAndSizePointers : RectangleReference<CornerAndSize>
    {
        public static readonly CornerAndSizePointers Zero = new CornerAndSizePointers();

        public CornerAndSizePointers() : base() {}

        public CornerAndSizePointers(ref Point32 a, ref Point32 b)
            : base(ref a, ref b) {
        }

        public CornerAndSizePointers(ref short x, ref short y, ref short w, ref short h)
            : base(ref x, ref y, ref w, ref h) {
        }

        public CornerAndSizePointers( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
            : base(a1,a2,b1,b2) {
        }

        public CornerAndSizePointers( ref CornerAndSize strect )
            : base( ref strect.Abstract.compound1, ref strect.Abstract.compound2 ) {
        }

        override public Point32 Corner {
            get { unsafe { return new Point32( *(short*)pA1.ToPointer(), *(short*)pA2.ToPointer() ); } }
            set { unsafe { *(short*)pA1.ToPointer()=value.x; *(short*)pA2.ToPointer() = value.y; } }
        }
        override public Point32 Sizes {
            get { unsafe { return new Point32( *(short*)pB1.ToPointer(), *(short*)pB2.ToPointer() ); } }
            set { unsafe { *(short*)pB1.ToPointer()=value.x; *(short*)pB2.ToPointer() = value.y; } }
        }
        override public Point32 Center {
            get{ return Corner + Scale; }
            set{ Scale = value - Corner; }
        }
        override public Point32 Scale {
            get{ return Sizes / 2; }
            set{ Sizes = value * 2;  }
        }

        override public int X { get { unsafe{ return *(short*)pA1.ToPointer(); } } set{ unsafe{ *(short*)pA1.ToPointer() = (short)value; } } }
        override public int Y { get { unsafe{ return *(short*)pA2.ToPointer(); } } set{ unsafe{ *(short*)pA2.ToPointer() = (short)value; } } }
        override public int W { get { unsafe{ return *(short*)pB1.ToPointer(); } } set{ unsafe{ *(short*)pB1.ToPointer() = (short)value; } } }
        override public int H { get { unsafe{ return *(short*)pB2.ToPointer(); } } set{ unsafe{ *(short*)pB2.ToPointer() = (short)value; } } }
        override public int L { get { return  X; } set { X = value; } }
        override public int T { get { return  Y; } set { Y = value; } }
        override public int R { get{ return (short)(X + W); } set{ L = (short)(value - W); } }
        override public int B { get{ return (short)(Y + H); } set{ T = (short)(value - H); } }

        public override PointPT CompoundA { get { return new PointPT( pA1, pA2 ); } }

        public override PointPT CompoundB { get { return new PointPT( pB1, pB2 ); } }

        public override StorageLayout StorageLayout {
            get { return StorageLayout.CornerAndSizes; }
        }

        public override IRectangle converted<RectangleData>() {
            return Rectangle<CornerAndSizePointers>.Convert<RectangleData>( this );
        }
    }

    public class CenterAndScalePointers : RectangleReference<CenterAndScale>
    {
        public static readonly CenterAndScalePointers Zero = new CenterAndScalePointers();

        public CenterAndScalePointers() : base() {}

        public CenterAndScalePointers( ref CenterAndScale structangle )
            : base( ref structangle.Abstract.compound1,
                    ref structangle.Abstract.compound2 ) {
        }

        public CenterAndScalePointers( PointPT a, PointPT b )
            : base( a.pX, a.pY, b.pX, b.pY ) {
        }

        public CenterAndScalePointers( ref short a1, ref short a2, ref short b1, ref short b2 )
            : base(ref a1, ref a2, ref b1, ref b2) {
        }

        public CenterAndScalePointers( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
            : base( a1, a2, b1, b2 ) {
        }

        override public int X {
            get { unsafe { return (short)(*(short*)pA1.ToPointer() - *(short*)pB1.ToPointer()); } }
            set { value = (short)((value -= X) / 2); unsafe{ *(short*)pA1.ToPointer() += (short)value; *(short*)pB1.ToPointer() += (short)value; } }
        }
        override public int Y {
            get { unsafe { return (short)(*(short*)pA2.ToPointer() - *(short*)pB2.ToPointer()); } }
            set { value = (short)((value -= Y) / 2); unsafe{ *(short*)pA2.ToPointer() += (short)value; *(short*)pB2.ToPointer() += (short)value; } }
        }
        override public int W {
            get { unsafe { return (short)(*(short*)pB1.ToPointer() * 2); } }
            set { unsafe { *(short*)pA1.ToPointer() += (short)((value /= 2)-*(short*)pB1.ToPointer()); *(short*)pB1.ToPointer() = (short)value; } }
        }
        override public int H {
            get { unsafe { return (short)(*(short*)pB2.ToPointer() * 2); } }
            set { unsafe { *(short*)pA2.ToPointer() += (short)((value /= 2)-*(short*)pB2.ToPointer()); *(short*)pB2.ToPointer() = (short)value; } }
        }

        override public int L { get { return  X; } set { X = value; } }
        override public int T { get { return  Y; } set { Y = value; } }

        override public int R {
            get { unsafe { return (short)(*(short*)pA1.ToPointer() + *(short*)pB1.ToPointer()); } } 
            set { value = (short)((value -= R) / 2); unsafe{ *(short*)pA1.ToPointer() += (short)value; *(short*)pB1.ToPointer() += (short)value; } }
        }
        override public int B {
            get { unsafe { return (short)(*(short*)pA2.ToPointer() + *(short*)pB2.ToPointer()); } } 
            set { value = (short)((value -= B) / 2); unsafe{ *(short*)pA2.ToPointer() += (short)value; *(short*)pB2.ToPointer() += (short)value; } }
        }

        override public Point32 Corner {
            get { return Center - Scale; }
            set { Center = value + Scale; }
        }
        override public Point32 Sizes {
            get { return Scale * 2; }
            set { Center = (Center - Scale) + (Scale = (value / 2)); }
        }
        override public Point32 Center { 
            get{ unsafe {return new Point32(*(short*)pA1.ToPointer(),*(short*)pA2.ToPointer()); } }
            set{ unsafe { *(short*)pA1.ToPointer() = value.x; *(short*)pA2.ToPointer() = value.y; } }
        }
        override public Point32 Scale { 
            get{ unsafe {return new Point32(*(short*)pB1.ToPointer(),*(short*)pB2.ToPointer()); } }
            set{ unsafe { *(short*)pB1.ToPointer() = value.x; *(short*)pB2.ToPointer() = value.y; } }
        }

        public override PointPT CompoundA { get{ return new PointPT(pA1,pA2); } }

        public override PointPT CompoundB { get{ return new PointPT(pB1,pB2); } }

        public override StorageLayout StorageLayout {
            get { return StorageLayout.CenterAndScale; }
        }

        public override IRectangle converted<RectangleData>()
        {
            return Rectangle<CenterAndScalePointers>.Convert<RectangleData>( this );
        }
    }

    public class AbsoluteEdgesPointers : RectangleReference<AbsoluteEdges>
    {
        public static readonly AbsoluteEdgesPointers Zero = new AbsoluteEdgesPointers();

        public AbsoluteEdgesPointers() : base() {}

        public AbsoluteEdgesPointers(ref short a1, ref short a2, ref short b1, ref short b2)
            : base(ref a1, ref a2, ref b1, ref b2) {
        }

        public AbsoluteEdgesPointers(ref Point32 LT, ref Point32 RB)
            : base( ref LT, ref RB) {
        }

        public AbsoluteEdgesPointers( ref AbsoluteEdges rect )
            : base( ref rect.Abstract.compound1, ref rect.Abstract.compound2 ) {
        }

        override public int L { get { unsafe{ return *(short*)pA1.ToPointer(); } } set{ unsafe{ *(short*)pA1.ToPointer() = (short)value; } } }
        override public int T { get { unsafe{ return *(short*)pA2.ToPointer(); } } set{ unsafe{ *(short*)pA2.ToPointer() = (short)value; } } }
        override public int R { get { unsafe{ return *(short*)pB1.ToPointer(); } } set{ unsafe{ *(short*)pB1.ToPointer() = (short)value; } } }
        override public int B { get { unsafe{ return *(short*)pB2.ToPointer(); } } set{ unsafe{ *(short*)pB2.ToPointer() = (short)value; } } }
        override public int X { get { return  L; } set { L = value; } }
        override public int Y { get { return  T; } set { T = value; } }
        override public int W { get { return (short)(R - L); } set { L = (short)((R = Center.x) - (value/=2)); R += value; } }
        override public int H { get { return (short)(B - T); } set { T = (short)((B = Center.y) - (value/=2)); B += value; } }

        override public Point32 Corner { get { return new Point32(X, Y); } set { X = value.x; Y = value.y; } }
        override public Point32 Center { get{ return new Point32((L+R)/2,(T+B)/2); } set{ Point32 m = value - Center; Corner += m; R += m.x; B += m.y; } }
        override public Point32 Sizes { get{ return new Point32(W,H); } set{ W = value.x; H = value.y; } } 
        override public Point32 Scale { get{ return Center - Corner; } set{ Point32 s = Scale;  value -= s; Corner -= value; R += value.x; B += value.y; } }

        public override PointPT CompoundA { get{ return new PointPT(pA1,pA2); } }

        public override PointPT CompoundB { get{ return new PointPT(pB1,pB2); } }

        public override StorageLayout StorageLayout {
            get { return StorageLayout.AbsoluteEdges; }
        }

        public override IRectangle converted<RectangleData>() {
             return Rectangle<AbsoluteEdgesPointers>.Convert<RectangleData>( this );
        }
    }

    #endregion

    #region Static Helper classes
    // for helping with Construction and with Conversion between diferent storage models, etc...  

    public class Rect64
    {    
        public static IRectangle Create<Layout>( Point a, Point b )
            where Layout : IRectangle // struct, IRectangleValues, IRectangleCompounds
        {
            if( typeof(Layout) == typeof(CornerAndSize) ) {
                CornerAndSize create = CornerAndSize.None;
                create.Corner = new Point32(a);
                create.Sizes = new Point32(b);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(CenterAndScale) ) {
                CenterAndScale create = CenterAndScale.None;
                create.Corner = new Point32(a);
                create.Sizes = new Point32(b);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(AbsoluteEdges) ) {
                AbsoluteEdges create = AbsoluteEdges.None;
                create.Corner = new Point32(a);
                create.R = (short)b.X;
                create.B = (short)b.Y;
                return create as IRectangle;
            } return null;
        }
        /// <summary> RectangleType.Create.LayoutType() 
        /// Create a rectangle instance of type RectangleType from given values a1,a2,b1,b2
        /// where these values are interpreted being meand 'LayoutType'. 
        /// example:
        /// if 'LayoutType' parameter is 'AbsoluteEdges' then the a1 and a2 parameters are 
        /// assumed being 'Left' and 'Right' edges of that rectange which to create, and
        /// b1 and b2 parameters are assumed being positions for 'Top' and 'Bottom' edges
        ///  - regardless what kind of storage model the creted rectangle may use - 
        /// parameters will be converted so that instance to be created will have L/R edges
        /// posisioned at a1/a2 along X axis and T/B edges possitioned at b1/b2 along Y axis  
        /// </summary>   
        public static IRectangle Create<Layout>( int a1, int a2, int b1, int b2 )
            where Layout : IRectangle
        {
            if( typeof(Layout) == typeof(CornerAndSize) ) {
                CornerAndSize create = CornerAndSize.None;
                create.Corner = new Point32(a1,a2);
                create.Sizes = new Point32(b1,b2);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(CenterAndScale) ) {
                CenterAndScale create = CenterAndScale.None;
                create.Corner = new Point32(a1,a2);
                create.Sizes = new Point32(b1,b2);
                return create as IRectangle;
            } else if( typeof(Layout) == typeof(AbsoluteEdges) ) {
                AbsoluteEdges create = AbsoluteEdges.None;
                create.Corner = new Point32(a1,a2);
                create.R = (short)b1;
                create.B = (short)b2;
                return create as IRectangle;
            } return null;
        }

        public static RectangleReference<Layout> Refere<Layout>( ref IRectangle<Layout> rect )
            where Layout : struct, IRectangle {
            return rect.refere();
        }

        public static IRectangle Refere<Layout>( IntPtr rect )
        { unsafe{
            Type lt = typeof(Layout);
                   if( lt == typeof(CornerAndSize) ) {
                return new CornerAndSizePointers(ref *(CornerAndSize*)rect.ToPointer());
            } else if( lt == typeof(CenterAndScale) ) {
                 return new CenterAndScalePointers(ref *(CenterAndScale*)rect.ToPointer());
            } else if( lt == typeof(AbsoluteEdges) ) {
                 return new AbsoluteEdgesPointers(ref *(AbsoluteEdges*)rect.ToPointer());
            } return null;
        } }
        public static IRectangle Refere<Layout>( IntPtr pA, IntPtr pB )
        { unsafe {
            Type lt = typeof(Layout);
                   if( lt == typeof(CornerAndSize) ) {
                return new CornerAndSizePointers( ref *(Point32*)pA.ToPointer(),
                                                  ref *(Point32*)pB.ToPointer() );
            } else if( lt == typeof(CenterAndScale) ) {
                 return new CenterAndScalePointers( new PointPT(ref *(Point32*)pA.ToPointer() ),
                                                    new PointPT(ref *(Point32*)pB.ToPointer() ) );
            } else if( lt == typeof(AbsoluteEdges) ) {
                 return new AbsoluteEdgesPointers( ref *(Point32*)pA.ToPointer(),
                                                   ref *(Point32*)pB.ToPointer() );
            } return null;
        } }
        public static IRectangle Refere<Layout>( IntPtr a1, IntPtr a2, IntPtr b1, IntPtr b2 )
        { unsafe {
            Type lt = typeof(Layout);
                   if( lt == typeof(CornerAndSize) ) {
                return new CornerAndSizePointers(
                   ref *(short*)a1.ToPointer(),
                   ref *(short*)a2.ToPointer(),
                   ref *(short*)b1.ToPointer(),
                   ref *(short*)b2.ToPointer()
                );
            } else if( lt == typeof(CenterAndScale) ) {
                return new CenterAndScalePointers(
                   ref *(short*)a1.ToPointer(),
                   ref *(short*)a2.ToPointer(),
                   ref *(short*)b1.ToPointer(),
                   ref *(short*)b2.ToPointer()
                );
            } else if( lt == typeof(AbsoluteEdges) ) {
                return new AbsoluteEdgesPointers(
                   ref *(short*)a1.ToPointer(),
                   ref *(short*)a2.ToPointer(),
                   ref *(short*)b1.ToPointer(),
                   ref *(short*)b2.ToPointer()
                );
            } return null;
        } }
    }

    public class Rectangle<rType> : Rect64 where rType : IRectangle
    {
        public const uint LRTB = (uint)StorageLayout.AbsoluteEdges;
        public const uint XYWH = (uint)StorageLayout.CornerAndSizes;
        public const uint CPSZ = (uint)StorageLayout.CenterAndScale;

        public static IRectangle Convert<Layout>( rType from )
            where Layout : struct, IRectangleValues, IRectangleCompounds
        {
            Type layoutType = typeof(Layout);
                   if( layoutType == typeof( CornerAndSize ) ) {
                return Create( StorageLayout.CornerAndSizes, from.X, from.Y, from.W, from.H ); 
            } else if( layoutType == typeof( CenterAndScale ) ) {
                return Create( StorageLayout.CenterAndScale, from.Center.x, from.Center.y, from.Scale.x, from.Scale.y ); 
            } else if( layoutType == typeof( AbsoluteEdges ) ) {
                return Create( StorageLayout.AbsoluteEdges, from.L, from.R, from.T, from.B ); 
            } return null;
        }

        public static IRectangle Refere( ref rType databased )
        {
            switch( databased.StorageLayout ) {
                case StorageLayout.CornerAndSizes: {
                    return Refere<CornerAndSize>(
                        databased.CompoundA.pX, databased.CompoundA.pY,
                        databased.CompoundB.pX, databased.CompoundB.pY
                    );
                }
                case StorageLayout.CenterAndScale: {
                    return Refere<CenterAndScale>(
                        databased.CompoundA.pX, databased.CompoundA.pY,
                        databased.CompoundB.pX, databased.CompoundB.pY
                    );
                }
                case StorageLayout.AbsoluteEdges: {
                    return Refere<AbsoluteEdges>(
                        databased.CompoundA.pX, databased.CompoundA.pY,
                        databased.CompoundB.pX, databased.CompoundB.pY
                    );
                }
            default: return null; }
        }

        public static IRectangle FromRectangle( Rectangle from )
        {
            if( !typeof(rType).BaseType.IsAbstract ) {
                return Create( StorageLayout.CornerAndSizes, from.X, from.Y, from.Width, from.Height ); 
            } else {
                throw new InvalidOperationException("cannot create references to abstract rectangle types");
            }
        }

        public static IRectangle Create()
        {
                   Type rType =  typeof(rType);
                   if ( rType == typeof(CenterAndScale) ) {
                return CenterAndScale.None;
            } else if ( rType == typeof(CornerAndSize) ) {
                return CornerAndSize.None;
            } else if ( rType == typeof(AbsoluteEdges) ) {
                return AbsoluteEdges.None;
            } else if ( rType == typeof(CenterAndScalePointers) ) {
                return CenterAndScalePointers.Zero;
            } else if ( rType == typeof(CornerAndSizePointers) ) {
                return CornerAndSizePointers.Zero;
            } else if ( rType == typeof(AbsoluteEdgesPointers) ) {
                return AbsoluteEdgesPointers.Zero;
            } else {
                return new SystemDefault( Rectangle.Empty );
            }
        }

        public static IRectangle Create( int a1, int a2, int b1, int b2 )
        {
            return Create<rType>( a1, a2, b1, b2 );
        }

        public static IRectangle Create( Point p1, Point p2 )
        {
            return Create<rType>( p1, p2 );
        }

        public static IRectangle Create( Point32 p1, Point32 p2 )
        {
            return Create<rType>( p1.x, p1.y, p2.x, p2.y );
        }

        public static IRectangle Create( StorageLayout fromParameters, int val1, int val2, int val3, int val4 )
        {
            IRectangle create = Rectangle<rType>.Create();
            switch( fromParameters ) {
                case StorageLayout.CenterAndScale: {
                       create.Center = new Point32(val1,val2);
                       create.Scale  = new Point32(val3,val4);
                return create; }
                case StorageLayout.SystemDefault:
                case StorageLayout.CornerAndSizes: {
                       create.Center = new Point32( val1+val3/2, val2+val4/2 );
                       create.Scale = new Point32( val3/2, val4/2 );
                return create; }
                case StorageLayout.AbsoluteEdges: {
                       create.L = (short)val1;
                       create.R = (short)val2;
                       create.T = (short)val3;
                       create.B = (short)val4;
                return create; }
                default:
                return create;
            }
        }
    }

    #endregion
}
