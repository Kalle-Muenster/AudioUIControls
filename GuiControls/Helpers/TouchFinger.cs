using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32Imports.Touch;
using TaskAssist.Geomety;
using Stepflow.Gui.Helpers;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Point = System.Drawing.Point;
using Rect  = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;

namespace Stepflow.Gui
{
    public class FingerTip : EventArgs, IEnumerable<FingerTip>
    {
        public delegate void TouchDelegate( object sender, FingerTip touch );
        public delegate void TouchMessages( FingerTip screen );

        public readonly ushort Id;
        internal IsTouching    info;
        private ushort         multiple;
        internal Point32       translat;
        internal Point32       location;
        internal DateTime      duration;
        private  Control       reporter;
        public FingerTip       NextHere;
        public TouchMessages   Move;
        public TouchMessages   Lift;
        

        public  int            X { get { return location.x - translat.x; } }
        public  int            Y { get { return location.y - translat.y; } }

        public Control Reporter {
            get { return reporter; }
        }

        internal FingerTip( PointerAction touchAction,
                            TouchMessages moveHandler,
                            TouchMessages liftHandler,
                            Control reportingElement )  {
            location = touchAction.pos;
            translat = new Point32( reportingElement.PointToScreen(reportingElement.Location) );
            reporter = reportingElement;
            Id = touchAction.pid;
            Move = moveHandler;
            Lift = liftHandler;
            NextHere = this;
            multiple = Id;
            duration = DateTime.Now;
        }

        internal void Update( Point32 position ) {
            location = position;
            Move?.Invoke( this );
        }
        internal void Remove( Point32 position ) {
            location = position;
            Lift?.Invoke( this );
        }
        internal void Return( ITouchDispatchTrigger toTheSource )
        {
            info |= IsTouching.TheScreen;
            if( info.HasFlag( IsTouching.TrackKept ) ) {
                Move += toTheSource.Move;
                Lift += toTheSource.Lift;
            } else {
                Move = toTheSource.Move;
                Lift = toTheSource.Lift;
            } info &= IsTouching.AnElement;
            toTheSource.Down( this );
        }
        internal void ReturnToTheSource()
        {
            Return( (reporter as ITouchable).screen() );
        }

        public FingerTip Prime { get {
               FingerTip find = this;
                 while ( find ) {
                     if( find.NextHere == find ) break;
                else if( find.info > IsTouching.SubPrime ) break;
                    else find = find.NextHere;
                } return find;
        } }

        public int Count {
            get { int count = 1;
                FingerTip next = NextHere;
                while ( next != this ) { ++count;
                    next = next.NextHere;
                } return count;
            }
        }

        public void BelongsThere()
        {
            info &= IsTouching.Here;
            info |= IsTouching.There;
        }

        public void BelongsHere()
        {
            info |= ( IsTouching.TrackKept
                    | IsTouching.Here );
        }

        public void KeepTrack( ITouchableElement trackKeeper )
        {
            TouchMessages touchfunc = new TouchMessages( trackKeeper.touch.Move );
            if( !(Move.GetInvocationList() as ICollection<Delegate>).Contains( touchfunc ) ) {
                Move += trackKeeper.touch.Move;
                Lift += trackKeeper.touch.Lift;
            } info |= IsTouching.TrackKept;
        }

        public bool TryPass( ITouchableElement element )
        {
            IRectangle bounds = element.ScreenRectangle();
            if ( bounds.Contains( Origin ) ) {
                if ( Interact( element ) ) {
                    SetOffset( bounds.Corner );
                    info |= IsTouching.AnElement;
                    return true;
                }
            } return false;
        }

        public bool Interact( ITouchableElement element )
        {
            //if( reporter == element.Element ) return true;
            reporter = element.Element;
            if( info.HasFlag( IsTouching.TrackKept ) ){
                Lift += element.touch.Lift;
                Move += element.touch.Move;
            } else {
                Lift = element.touch.Lift;
                Move = element.touch.Move;
            } translat += element.ScreenRectangle().Corner;
            element.touch.Down( this );
            return element.touch.hasFinger( this.Id );
        }

        public Point64 Position { get { return location - translat; } }
        public Point64 Origin { get { return location; } }
        public bool HasFlags( IsTouching allThese ) {
            return ( info & allThese) == allThese;
        }
        public bool AnyFlags( IsTouching anyOfThese ) {
            return (info & anyOfThese) != 0;
        }
        
        public bool HasFinger( ushort id ) {
            return (Prime.multiple & id) == id;
        }

        public TimeSpan TimeDown {
            get { return DateTime.Now - duration; }
        }

        public void SetHandler( TouchMessages onMove, TouchMessages onLift ) {
            Move = onMove;
            Lift = onLift;
        }
        public void AddHandler( TouchMessages onMove, TouchMessages onLift ) {
            Move += onMove;
            Lift += onLift;
        }
        public Point64 ReporterOffset {
            get { return translat; }
        } 
        public void SetOffset( Point32 position ) {
            translat = position;
        }

        public static implicit operator bool( FingerTip cast )
        { return ( cast == null ) ? false : ( cast.Id > 0 ); }
        public static implicit operator Point( FingerTip cast )
        { return cast.Position; }
        public static implicit operator IsTouching( FingerTip cast )
        { return cast.info; }
        

        public static FingerTip operator +( FingerTip self, FingerTip that )
        {
            int count = self.Count;
            self.multiple |= that.Id;
            that.multiple = self.multiple;
            while( self.NextHere != self.Prime ) self = self.NextHere;
            while( that.NextHere != that.Prime ) that = that.NextHere;
            FingerTip temp = that;
            temp.NextHere.info |= (IsTouching)(0x0001<<((++count)+3));
            while( that != temp.NextHere ) {
                that.info |=(IsTouching)(0x0001<<((++count)+3));
                that = that.NextHere;
            } temp.NextHere = self.NextHere;
            temp = self.NextHere;
            self.NextHere = that;
            return temp;
        }

        public static FingerTip operator -( FingerTip self, FingerTip that )
        {
            int count = self.Count;
            if( count == 1 )
                return self.Id == that.Id ? null : self;
            
            FingerTip prime = self.Prime;
            while( self.NextHere.Id != prime.Id ) self = self.NextHere;
            for( int i=0; i < count; ++i ) {
                if( self.NextHere.Id == that.Id ) {
                    prime.multiple &= that.Id;
                    that.multiple = that.Id;
                    self.NextHere = self.NextHere.NextHere;
                    break; }
            } return self;
        }

        public FingerTip[] GetFingerArray()
        {
            int count = this.Count;
            FingerTip[] touches = new FingerTip[count];
            touches[0] = this;
            for( int i = 1; i < count; ++i )
                touches[i] = touches[i-1].NextHere;
            return touches;
        }

        private class Enumerator : IEnumerator<FingerTip>
        {
            private bool      cycle;
            private FingerTip prime;
            internal Enumerator(FingerTip fingers)
            {
                cycle = false;
                prime = fingers.Prime;
            }

            public FingerTip Current {
                get; private set;
            }

            object IEnumerator.Current {
                get { return Current; }
            }

            public void Dispose()
            {
                //nothing to dispose actually....
            }

            public bool MoveNext()
            {
                if(!cycle ) {
                    Current = prime;
                    return cycle = true; 
                } else {
                    Current = Current.NextHere;
                    return cycle = (Current != prime);
                }
            }

            public void Reset()
            {
                cycle = false;
            }
        }

        public IEnumerator<FingerTip> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

    };

    public class MultiFinger
    {
        public delegate void TouchDelegate( object sender, MultiFinger gesture );

        private AbsoluteEdgesPointers  areal;
        private CenterAndScalePointers POINT;
        private CenterAndScalePointers PRIME;
        private CenterAndScalePointers RIGHT;
        private CenterAndScalePointers RANGE;

        private Controlled.Int16[]     point;
        private Controlled.Int16[]     prime;
        private Controlled.Int16[]     right;
        private Controlled.Int16[]     range;

        private FingerTip              touch;
        private bool[]                 dirts;
 
        private int Invalidator {
            set { if( dirts[value] ) {
                    CheckFingers(); 
                } else {
                    dirts[value] = true;
                }
            }
        }

        public static implicit operator bool( MultiFinger cast )
        {
            return cast?.touch != null;
        }

        public MultiFinger( FingerTip tips )
        {
            touch = tips;
            dirts = new bool[3] { true, true, true };
            areal = new AbsoluteEdgesPointers( ref tips.location, ref tips.NextHere.location );
            point = new Controlled.Int16[2] {
                new Controlled.Int16(ControlMode.None),
                new Controlled.Int16(ControlMode.None)
            };
            prime = new Controlled.Int16[2] {
                new Controlled.Int16(ControlMode.None),
                new Controlled.Int16(ControlMode.None)
            };
            right = new Controlled.Int16[2] {
                new Controlled.Int16(ControlMode.None),
                new Controlled.Int16(ControlMode.None)
            };
            range = new Controlled.Int16[2] {
                new Controlled.Int16(ControlMode.None),
                new Controlled.Int16(ControlMode.None)
            };

            point[0].SetUp((short)areal.L, (short)areal.R, 0, areal.Center.x, ControlMode.Element);
            point[1].SetUp((short)areal.T, (short)areal.B, 0, areal.Center.y, ControlMode.Element);
            point[0].SetCheckAtGet();
            point[1].SetCheckAtGet();
            POINT = new CenterAndScalePointers(
                point[0].GetTarget(), point[1].GetTarget(),
                point[0].GetPointer(ControllerVariable.MOV),
                point[1].GetPointer(ControllerVariable.MOV)
            );
            point[0].Active = true;
            point[1].Active = false;

            prime[0].SetUp(short.MinValue, short.MaxValue, 0, (short)areal.L, ControlMode.Element);
            prime[1].SetUp(short.MinValue, short.MaxValue, 0, (short)areal.T, ControlMode.Element);
            right[0].SetUp(short.MinValue, short.MaxValue, 0, (short)areal.R, ControlMode.Element);
            right[1].SetUp(short.MinValue, short.MaxValue, 0, (short)areal.B, ControlMode.Element);

            prime[0].SetCheckAtGet();
            prime[1].SetCheckAtGet();
            right[0].SetCheckAtGet();
            right[1].SetCheckAtGet();

            prime[0].LetPoint(ControllerVariable.VAL, point[0].GetPointer(ControllerVariable.MIN));
            prime[1].LetPoint(ControllerVariable.VAL, point[1].GetPointer(ControllerVariable.MIN));

            right[0].LetPoint(ControllerVariable.VAL, point[0].GetPointer(ControllerVariable.MAX));
            right[1].LetPoint(ControllerVariable.VAL, point[1].GetPointer(ControllerVariable.MAX));

            PRIME = new CenterAndScalePointers(
                prime[0].GetTarget(), prime[1].GetTarget(),
                prime[0].GetPointer(ControllerVariable.MOV),
                prime[1].GetPointer(ControllerVariable.MOV)
            );

            RIGHT = new CenterAndScalePointers(
                right[0].GetTarget(), right[1].GetTarget(),
                right[0].GetPointer(ControllerVariable.MOV),
                right[1].GetPointer(ControllerVariable.MOV)
            );

            
            range[0].SetUp(short.MinValue, short.MaxValue, 0, 0, ControlMode.Element);
            range[1].SetUp(short.MinValue, short.MaxValue, 0, 0, ControlMode.Element);

            RANGE = new CenterAndScalePointers(
                range[0].GetTarget(), range[1].GetTarget(),
                range[0].GetPointer(ControllerVariable.MOV),
                range[1].GetPointer(ControllerVariable.MOV)
            );
            RANGE.Center = areal.Sizes;
            right[0].SetCheckAtGet();
            right[1].SetCheckAtGet();
        }

        public void Discard()
        {
            touch = null;
        }

        // forget any maybe still referenced touch points and begin tracking new given ones
        public void FingerReset( FingerTip fingers )
        {
            Discard();
            touch = fingers;

            areal.RefereCompound( 1, ref touch.location );
            areal.RefereCompound( 2, ref touch.NextHere.location );

            point[0].MIN = (short)areal.L;
            point[0].MAX = (short)areal.R;
            point[1].MIN = (short)areal.T;
            point[1].MAX = (short)areal.B;

            prime[0].VAL = (short)areal.L;
            prime[1].VAL = (short)areal.T;
            right[0].VAL = (short)areal.R;
            right[1].VAL = (short)areal.B;

            RANGE.Center = areal.Sizes;
        } 

        // update the state to the actually referenced touch points latest movement and posaition values
        public void CheckFingers()
        {
            if( touch == null ) return;
            POINT.L = areal.L;
            POINT.T = areal.T;
            POINT.R = areal.R;
            POINT.B = areal.B;
            point[0].Check();
            point[1].Check();
            RANGE.Center = (RIGHT.Center - PRIME.Center);
            range[0].Check();
            range[1].Check();
            dirts[0] = dirts[1] = dirts[2] = false;
        }

        // returns delta rotation angle (in radian) of the diagonale between first and second finger 
        public float GetRotation()
        {
            Invalidator = 0;
            return ( (PRIME.Scale - RIGHT.Scale) / (PRIME.Center - RIGHT.Center) ).Summ();
        }

        // returns average of delta distances between first fingers actual x and first fingers last x  
        public Point GetMovement()
        {
            Invalidator = 1;
            return new Point( POINT.Scale );
        }

        // returns delta distance between Prime and SubPrime (between first finger and second finger)
        public float GetResizing()
        {
            Invalidator = 2;
            return (float)RANGE.Scale.Summ();
        }
    };
}
