using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Stepflow.Gui.Geometry;

namespace MidiGUI.Test.Container
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

    public enum ControlFlags
    {
        Cycled, Inverted, Normal
    }

    public class EventLog : IList
    {
        public class Enumerator : IEnumerator<string>
        {
            private List<string>.Enumerator  data;
            private EventLog                 root;

            public string Current { get { return data.Current; } }

            object IEnumerator.Current { get { return data.Current; } }

            public void Dispose()
            {
                root.Unlock();
                data.Dispose();
            }

            public bool MoveNext()
            {
                return data.MoveNext();
            }

            public void Reset()
            {
                data = root.data.GetEnumerator();
            }
        }

        private List<string>  data;
        private int           size;
        private int           full;
        private volatile int  sync;

        public event EventHandler Changed;

        private bool Lock()
        {
            while( sync != 0 ) Thread.Sleep( 100 );
            sync = Thread.CurrentThread.ManagedThreadId;
            return true;
        }
        private bool Lock( int timeout )
        {
            while( sync != 0 ) {
                if( ( timeout -= 100 ) < 0 ) return false;
                Thread.Sleep( 100 );
            } sync = Thread.CurrentThread.ManagedThreadId;
            return true;
        }

        private void Unlock()
        {
            if( sync == Thread.CurrentThread.ManagedThreadId )
                sync = 0;
        }

        private void TriggerChangedEvent()
        {
            Changed?.Invoke( this, null );
        }

        public EventLog( int size )
        {
            full = size;
            data = new List<string>( size );
            this.size = 0;
        }

        public void Log( string message )
        {
            if( Lock() ) {
                if( size == full ) data.RemoveAt(0);
                else ++size;
                data.Add( message );
                Unlock();
                if ( Changed != null ) {
                    new Task(TriggerChangedEvent).Start( TaskScheduler.Default );
                }
            }
        }

        public void Log( string message, int timeout )
        {
            if( Lock( timeout ) ) {
                if( size == full ) data.RemoveAt(0);
                else ++size;
                data.Add( message );
                Unlock();
            }
        }

        public int Count { get { return size; } }

        public bool IsSynchronized { get { return true; } }

        public object SyncRoot { get { return sync; } }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public object this[int index] { get{ return data[index]; } set { data[index] = (string)value; } }

        public void CopyTo( Array array, int index )
        {
            if( Lock() ) {
                Array.Copy( data.ToArray(), 0, array, index, size );
                Unlock();
            }
        }

        public IEnumerator GetEnumerator()
        {
            if( Lock() ) return data.GetEnumerator();
            else return null;
        }

        public int Add( object value )
        {
            Log( value.ToString() );
            return size - 1;
        }

        public void Clear()
        {
            data.Clear();
            size = 0;
            if( Changed != null ) {
                new Task(TriggerChangedEvent).Start(TaskScheduler.Default);
            }
        }

        public bool Contains( object value )
        {
            string val = value.ToString();
            foreach( string s in data )
                if( s == val ) return true;
            return false;
        }

        public int IndexOf( object value )
        {
            string val = value.ToString();
            string[] s = data.ToArray();
            for( int i = 0; i < size; ++i )
                if(s[i] == val) return i;
            return -1;
        }

        public void Insert( int index, object value )
        {
            data.Insert( index, value.ToString() );
            ++size;
            if( Changed != null ) {
                new Task(TriggerChangedEvent).Start(TaskScheduler.Default);
            }
        }

        public void Remove( object value )
        {
            data.Remove( value.ToString() );
            --size;
            if( Changed != null ) {
                new Task(TriggerChangedEvent).Start(TaskScheduler.Default);
            }
        }

        public void RemoveAt( int index )
        {
            data.RemoveAt( index );
            --size;
            if( Changed != null ) {
                new Task(TriggerChangedEvent).Start(TaskScheduler.Default);
            }
        }
    }
}
