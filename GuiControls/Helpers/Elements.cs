using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepflow.Helpers
{
    public abstract class Element
    {
        internal static void ErrorReport( string what, string why ) {
            MessageLogger.logErrorSchlimm( "# Assigning {0} is disabled on {1}", what, why );
        }
        internal static void InfoReport( string what, string why ) {
            MessageLogger.logInfoWichtig( "# log: {0} info: {1}", what, why );
        }
        public const uint ElementCode = 0;

        protected bool phasys;
        protected Element attached;
        protected Hashtable elements;
        public Element pinoekel {
            get { Element find = this;
                while (find != find.attached) {
                    find = find.attached;
                } return find;
            }
        }

        protected virtual void update() {}

        public void Update( bool currentPhasys ) {
            if (currentPhasys == phasys) {
                update();
                phasys = !phasys;
            }
        }

        public Element()
        {
            phasys = false;
            elements = new Hashtable();
        }
        public virtual Element Init( Element attach )
        {
            attached = attach;
            phasys = attached == null ? false : attached.phasys;
            return this;
        }
        public virtual Element Init( Element attach, params object[] initializations )
        {
            return Init(attach);
        }

        public E Add<B,E>( params object[] parameter ) where E : B, new() where B : Element, new()
        {
            Type b = typeof(B);
            if( elements.ContainsKey(b) ) { 
                List<B> list = elements[b] as List<B>;
                list.Add( new E().Init( this, parameter ) as E );
                return list[list.Count-1] as E;
            } else if (elements.ContainsKey( typeof(E) ) ) {
                List<B> neu = new List<B>();
                neu.AddRange( elements[typeof(E)] as List<E> );
                neu.Add( new E().Init( this, parameter ) as E );
                elements.Remove( typeof(E) );
                elements.Add( b, neu );
                return neu[neu.Count-1] as E;
            } else { E neu = new E().Init(this, parameter) as E;
                elements.Add(b, new List<B>(1) {
                    neu } );
                return neu;
            }
        }
        public int Idx<B,E>(int number) where E : B where B : Element
        {
            if( Has<B>() ) {
                List<B> list = elements[typeof(B)] as List<B>;
                for (int i = 0; i < list.Count; ++i)
                    if (list[i] is E) if(--number < 0) return i;
            } return -1;
        }
        public int Idx<B,E>() where E : B where B : Element
        {
            return Idx<B,E>(0);
        }
        public int Idx<B>( Element E, int number ) where B : Element
        {
            if (number == 0) { 
                if (elements.ContainsKey(E.GetType()))
                    return 0;
            }
            if( elements.ContainsKey(typeof(B)) ) {
                List<B> list = elements[typeof(B)] as List<B>;
                for (int i = 0; i < list.Count; ++i)
                    if (list[i] == E) if (--number < 0) return i;
            } return -1;
        }
        public int Idx<B>(Element E) where B : Element
        {
            return Idx<B>(E,0);
        }

        public E Get<B,E>( int idx ) where E : B, new() where B : Element, new()
        {
            if (!Has<B>()) {
                List<B> list = new List<B>( idx + 1 );
                for (int i = 0; i < idx; ++i) list.Add( new B() );
                list.Add( new E() );
                elements.Add( typeof(B), list );
            } return (elements[typeof(B)] as List<B>)[idx] as E;
        }

        public E Add<E>( params object[] parameter ) where E : Element, new()
        {
            if ( !elements.ContainsKey( typeof(E) ) ) {
                return Get<E>().Init( this, parameter ) as E;
            } else {
                E neu = new E();
                neu.Init( this, parameter );
                (elements[typeof(E)] as List<E>).Add(neu);
                return neu;
            }
        }

        public E Add<E>( E instance ) where E : Element
        {
            Type e = typeof(E);
            if ( elements.ContainsKey(e) ) {
                List<E> list = elements[e] as List<E>;
                if ( !list.Contains(instance) )
                    list.Add( instance );
            } else elements.Add( e, new List<E> { instance } );
            return instance;
        }

        internal E Add<E>( E instance, Type B ) where E : Element
        {
            Type lt = typeof(List<>).MakeGenericType( new Type[]{B} );
            if ( (!elements.ContainsKey(B)) ) {
                object bList = lt.GetConstructor( Type.EmptyTypes ).Invoke( new object[0] );
                if( elements.ContainsKey( typeof(E) ) ) {
                    List<E> eList = elements[typeof(E)] as List<E>;
                    object[] objar = new object[eList.Count];
                    for (int i = 0; i < objar.Length; ++i)
                    { objar[i] = eList[i]; }
                    lt.GetMethod( "Add" ).Invoke( bList, objar );
                    elements.Remove( typeof(E) );
                } elements.Add( B, bList );
            } lt.GetMethod( "Add" ).Invoke( elements[B], new object[]{instance} );
            return instance;
        }

        public E Get<E>() where E : Element, new()
        { Type t = typeof(E);
            if ( !elements.ContainsKey(t) ) {
                elements.Add( t, new List<E>(){new E()} );
            } return (elements[t] as List<E>)[0];
        }

        public E Get<E>( int idx ) where E : Element, new()
        {
            Type e = typeof(E); ++idx;
            if ( !elements.ContainsKey(e) ) {
                List<E> neu = new List<E>(idx);
                for (int i = 0; i < idx; ++i) {
                    neu.Add( new E() );
                } elements.Add( e, neu );
            } return (elements[e] as List<E>)[idx-1];
        }

        public E Set<E>( int idx, E elm ) where E : Element
        {
            Type e = typeof(E);
            if ( !elements.ContainsKey(e) ) {
                if (idx == 0) {
                    elements.Add( e, new List<E>(){elm} );
                    return elm; } 
            } else {
                List<E> list = elements[e] as List<E>;
                if( list.Count > idx ) {
                    return ( list[idx] = elm );
                } else if( list.Count == idx ) {
                    list.Add( elm );
                    return elm;
                }
            } ErrorReport(
                "instance to element '" + idx + "'",
                "no element '" + idx + "'exists at all"
                             );
            return elm;
        }

        public bool HasElementar<T>()
        {
            return Has<Elementar<T>>() ? true
                 : elements.ContainsKey(typeof(Elementar<T>));
        }

        public T GetElementar<T>()
        {
            return Get<Elementar<T>>().entity;
        }

        public T GetElementar<T>(int idx)
        {
            return Get<Elementar<T>>(idx).entity;
        }

        public bool Has( Type T )
        {
            if ( T.IsSubclassOf( typeof(Element) ) ) {
                return elements.ContainsKey(T);
            } else if ( !T.IsGenericType ) {
                return Has( typeof(Elementar<>).MakeGenericType(new Type[]{T}) );
            } else return elements.ContainsKey( T.GetElementType() );
        }
        public bool Has<E>() where E : Element
        {
            return elements.ContainsKey( typeof(E) );
        }

        public bool Has<E>( int idx ) where E : Element
        {
            if( elements.ContainsKey( typeof(E) ) ) {
                return (elements[typeof(E)] as List<E>).Count > idx;
            } return false;
        }

        public int Num<E>() where E : Element
        {
            if( elements.ContainsKey( typeof(E) ) ) {
                return (elements[typeof(E)] as List<E>).Count;
            } return 0;
        }

        public IEnumerator<E> All<E>() where E : Element
        {
            if( elements.ContainsKey( typeof(E) ) ) {
                return (elements[typeof(E)] as List<E>).GetEnumerator();
            } return new List<E>(0).GetEnumerator();
        }

        public void ForAll<E>( Action<E> perform ) where E : Element
        {
            IEnumerator<E> it = All<E>();
            while ( it.MoveNext() ) {
                perform( it.Current );
            }
        }

        public void ForAllIn<E,F>( F ins, Action<E,F> perform ) where E : Element
        {
            IEnumerator<E> it = All<E>();
            while ( it.MoveNext() ) {
                perform( it.Current, ins );
            }
        }

        public void UpdateAll<E>( bool currentPhasys ) where E : Element
        {
            IEnumerator<E> it = All<E>();
            while ( it.MoveNext() ) {
                it.Current.Update( currentPhasys );
            }
        }

        public void Rem<E>( int idx ) where E : Element
        { 
            int count = Num<E>();
            if( count > 0 ) {
                if (count == 1 || idx < 0)
                    elements.Remove( typeof(E) );
                else ( elements[typeof(E)] as List<E>
                      ).RemoveAt( idx );
            }
        }

        public E Find<E>() where E : Element, new()
        {
            if( Has<E>() ) {
                return Get<E>();
            } else if (attached != this) {
                return attached.Find<E>();
            } return null;
        }
    }

    public class Elementar<T> : Element
    {
        public T entity;
    }
}
