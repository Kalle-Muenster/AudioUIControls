using System;
using Stepflow;
using System.Collections;
using System.Collections.Generic;
using Win32Imports;
using System.Runtime.InteropServices;
#if   USE_WITH_WF
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rect  = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rect  = System.Windows.Int32Rect;
using RectF = System.Windows.Rect;
#endif

namespace Stepflow {
    namespace Gui {

        public enum ValenceField {
            Default = 0
        };

        [ClassInterface(ClassInterfaceType.AutoDispatch)]
        public class ValenceField<ControllerType,ValenceFieldsEnum>
            : IInterValuableElementComponent<ControllerType>
            , IControllerValenceField<ControllerType>
        where ControllerType : ControllerBase
        where ValenceFieldsEnum : struct
        {
            private readonly uint elmId; 
            private readonly ValenceFieldsEnum nullum;
            private ControllerBase[] controllers = null;
            private List<KeyValuePair<Action,ValenceFieldState.Flags>>[] dirtThrowers = null;
            private int current = -1;

            //-----------access--------------
            IControllerValenceField<cT> IControllerValenceField.field<cT>() {
                current = 0;
                return this as IControllerValenceField<cT>;
            }
            IControllerValenceField<cT> IControllerValenceField.field<cT>(Enum idx) {
                current = Field.Indices.IndexOf( idx );
                return this as IControllerValenceField<cT>;
            }
            /// <summary>field(opt. enum value)
            /// In fact it's implemented just 'explicitely', it's functinallity
            /// can only be accessed via the 'valence()' methods then. (e.g. 
            /// it simply should return 'this', but casted to an IInterValuable
            /// object then) a 'field' parameter can be given for selecting a
            /// distinct 'valence field' (a Controlled.Value instance which
            /// is declared via the IInterValuable interface for being able
            /// to shre controller variables with other 'valence field' variable
            /// on other control elements). The parameterless overload always
            /// selects the elements primary, by default declared controller.   
            /// </summary><returns>this as IInterValuable</returns>           
            public IControllerValenceField<ControllerType> field() {
                current = 0;
                return this;
            }
            /// <summary>valence(opt. enum value)
            /// In fact it's implemented just 'explicitely', it's functinallity
            /// can only be accessed via the 'valence()' methods then. (e.g. 
            /// it simply should return 'this', but casted to an IInterValuable
            /// object then) a 'field' parameter can be given for selecting a
            /// distinct 'valence field' (a Controlled.Value instance which
            /// is declared via the IInterValuable interface for being able
            /// to share controller variables with other 'valence field' variable
            /// on other control elements). The parameterless overload always
            /// selects the elements primary, by default declared controller.   
            /// </summary><param name="field">If an element declares more then
            /// one 'valence field' controller (via bypassing an Enum value to
            /// its internal ControllElement(Enum fields) constructor and the 
            /// passed enum type declares more then one enum constant) this
            /// parameter can be given then for selecting desired controllers
            /// on control elements then.</param>
            /// <returns>this as IInterValuable</returns>
            public IControllerValenceField<ControllerType> field( Enum fieldIndex ) {
                current = Field.Indices.IndexOf( fieldIndex );
                return this;
            }
            ControllerType IControllerValenceField<ControllerType>.controller() {
                return controllers[ current ] as ControllerType;
            }
            cT IControllerValenceField.controller<cT>() {
                return controllers[ current ] as cT;
            }
            //------------public------------

            public readonly int ControllerCount;
            public IInterValuable<ControllerType> element {
                get; set;
            }

            IInterValuable IStepflowControlElementComponent.getElement() {
                return element;
            }

            public void FreeAllFields() {
                foreach( ValenceFieldsEnum fieldIdx in Enum.GetValues( typeof(ValenceFieldsEnum) ) ) {
                    field( fieldIdx as Enum ).Free();
                }
            }

            public Valence.FieldIndex Field {
                get { return new Valence.FieldIndex( nullum as Enum ); }
            }

            //------------private------------

            private static readonly ValenceFieldState.Flags[] flagsmap = new ValenceFieldState.Flags[] {
                ValenceFieldState.Flags.VAL, ValenceFieldState.Flags.MIN,
                ValenceFieldState.Flags.MAX, ValenceFieldState.Flags.MOV,
                ValenceFieldState.Flags.VAL| ValenceFieldState.Flags.MIN|
                ValenceFieldState.Flags.MAX| ValenceFieldState.Flags.MOV
            };
            private void setDirty( int number, ValenceFieldState.Flags condition ) {
                if( controllers[ number ].ValenceField[ condition ] )
                    foreach( KeyValuePair<Action,ValenceFieldState.Flags> dirt in dirtThrowers[number] )
                        if( condition.HasFlag( dirt.Value ) )
                            dirt.Key();
            }
            private int findJoint( ControllerVariable variable, IInterValuable other ) {
                Action search = other.getInvalidationTrigger();
                for( int i=0;i<dirtThrowers[current].Count; ++i ) {
                    if( dirtThrowers[current][i].Key == search )
                        if( dirtThrowers[current][i].Value.HasFlag( flagsmap[(int)variable] ) )
                            return i;
                } return -1;
            }
            public void SetController( int fieldIndex, ControllerBase controller ) {
                controllers[fieldIndex] = controller;
            }
            public void SetControllerArray( ControllerBase[] controllerArray ) {
                if( controllerArray.Length < ControllerCount )
                    throw new Exception("parent element must define 'ControllerCount' on controllers");
                controllers = controllerArray;
            }
            protected virtual void ExternalInvalidated( object sender, InvalidateEventArgs e ) {
                (this as IInterValuableElementComponent<ControllerType>).element.callOnInvalidated( e );
            }

            //------------construction------------

            public ValenceField( IInterValuable<ControllerType> parent )
            {
                nullum = new ValenceFieldsEnum();
                ControllerCount = Enum.GetValues( nullum.GetType() ).Length;
                dirtThrowers = new List<KeyValuePair<Action,ValenceFieldState.Flags>>[ControllerCount];
                for( int i = 0; i < ControllerCount; ++i ) {
                    dirtThrowers[i] = new List<KeyValuePair<Action,ValenceFieldState.Flags>>();
                } current = 0;
                controllers = new ControllerType[ControllerCount];
                elmId = Valence.RegisterIntervaluableElement( parent );
                (this as IInterValuableElementComponent<ControllerType>).element = Valence.FindInterValuableElement(elmId) as IInterValuable<ControllerType>;
            }

            public ValenceField( IInterValuable<ControllerType> parent,
                                   ControllerBase[] definedControllers ) 
                : this( parent ) {
                SetControllerArray( definedControllers );
                for( int i=0; i < controllers.Length; ++i ) {
                    if( !(controllers[i] is ControllerType) ) {
                        Valence.RegisterValenceOfDifferentType( elmId,
                            (Enum)Enum.GetValues(typeof(ValenceFieldsEnum)).GetValue(i),
                                       controllers[i].GetType() );
                    }
                }
            }


            //----------internal helpers----------

            void IControllerValenceField.addInvalidator( Action dirtable, ControllerVariable forvar, int onController ) {
                dirtThrowers[onController].Add( 
                    new KeyValuePair<Action,ValenceFieldState.Flags>( dirtable, flagsmap[(int)forvar] )
                );
            }
            InvalidateEventHandler IControllerValenceField.getInvalidationHandler() {
                return ExternalInvalidated;
            }
            Action IControllerValenceField.getInvalidationTrigger() {
                return (this as IInterValuableElementComponent<ControllerType>).element.getInvalidationTrigger();
            }

            //-------------usage-------------------

            void IControllerValenceField.SetDirty( ValenceFieldState.Flags variables ) {
                setDirty( current, variables );
            }
            bool IControllerValenceField.IsJoint( ControllerVariable variable ) {
                return controllers[ current ].ValenceField[ flagsmap[(int)variable] ];
            }

            //----------connecting----------------

            void IControllerValenceField.Join( ControllerVariable variable,
                                                   IInterValuable other,
                                               ControllerVariable otherVar )
            {
                bool bound = false;
                if ( bound = !field().IsJoint(variable) ) {
                    controllers[current].LetPoint( variable,
                        other.valence<ControllerType>().controller<ControllerType>().GetPointer(otherVar)
                                                        ); } else
                if ( bound = !other.valence<ControllerType>().IsJoint(otherVar) ) {
                     other.valence<ControllerType>().controller<ControllerType>().LetPoint( otherVar,
                                                 controllers[current].GetPointer(variable) );
                                                                                     }
                else bound = ( controllers[current].GetPointer(variable) == other.valence<ControllerType>()
                                                                                .controller<ControllerType>()
                                                                               .GetPointer( otherVar ) );
                if ( bound ) {
                if ( !other.valence<ControllerType>().controller<ControllerType>().ValenceField[flagsmap[(int)variable]] )
                   ( this as IControllerValenceField<ControllerType>).addInvalidator(
                                   other.getInvalidationTrigger(), variable, current );
                }
            }

            void IControllerValenceField.Join( ControllerVariable thisVariable,
                                                   IInterValuable otherElement,
                                                             Enum otherValence,
                                               ControllerVariable otherVariable )
            {
                bool bound = false;
                if ( bound = !field().IsJoint(thisVariable) ) {
                     controllers[current].LetPoint(thisVariable,
                         otherElement.valence<ControllerType>(otherValence).controller<ControllerType>().GetPointer(otherVariable)
                                                      );
                } else
                if ( bound = !otherElement.valence<ControllerType>(otherValence).IsJoint(otherVariable) ) {
                     otherElement.valence<ControllerType>(otherValence).controller<ControllerType>().LetPoint(
                                                 otherVariable, controllers[current].GetPointer(thisVariable) ); }
                else bound = (controllers[current].GetPointer(thisVariable) == otherElement.valence<ControllerType>(otherValence)
                                                                                          .controller<ControllerType>()
                                                                                         .GetPointer(otherVariable) );
                if ( bound ) {
                if ( !otherElement.valence<ControllerType>(otherValence).controller<ControllerType>().ValenceField[flagsmap[(int)thisVariable]] )
                   ( this as IControllerValenceField<ControllerType>).addInvalidator(
                        otherElement.getInvalidationTrigger(), thisVariable, current );
                }
            }

            void IControllerValenceField.Join( ControllerVariable variable,
                                                   IInterValuable other )
            {
                (this as IControllerValenceField<ControllerType>).Join( variable, other, variable );
            }

            //-------------disconnecting-----------

            // free this controller from any other variables maybe joined with any of this controllers variables 
            public void Free() { unsafe {
                ControllerType ctrldVal = (ControllerType)controllers[ current ]; 
                ulong[] actualValues = new ulong[4] {
                    *(ulong*)ctrldVal.GetPointer(ControllerVariable.VAL).ToPointer(),
                    *(ulong*)ctrldVal.GetPointer(ControllerVariable.MIN).ToPointer(),
                    *(ulong*)ctrldVal.GetPointer(ControllerVariable.MAX).ToPointer(),
                    *(ulong*)ctrldVal.GetPointer(ControllerVariable.MOV).ToPointer()
                }; controllers[ current ].ValenceField.Clear();
                 dirtThrowers[ current ].Clear();
                for( int i = 0; i < 4; ++i ) {
                    *(ulong*)controllers[current].GetPointer((ControllerVariable)i).ToPointer()
                      = actualValues [i];
                } }
            }

            // free one distinct ControllerVariable of this controller from any other variables maybe joined with this distinct variable 
            public void Free( ControllerVariable variable ) {
                for( int i = dirtThrowers[current].Count-1; i >= 0; --i ) {
                    if( dirtThrowers[ current ][ i ].Value.HasFlag( flagsmap[(int)variable] ) )
                        dirtThrowers[ current ].RemoveAt( i );
                } unsafe { ulong actualValue =
                    *(ulong*)controllers[ current ].GetPointer( variable ).ToPointer();
                    controllers[ current ].LetPoint( variable, IntPtr.Zero );
                    *(ulong*)controllers[ current ].GetPointer( variable ).ToPointer()
                  = actualValue; }
            }
            // free one distinct ControllerVariable of this controller from just these other ControllerVariables which belong to the given distinct controller innstance 
            public void Free( ControllerVariable variable,
                              IInterValuable otherElement ) {
                for( int found = findJoint( variable, otherElement ); found != -1; 
                         found = findJoint( variable, otherElement ) ) {
                    dirtThrowers[current].RemoveAt( found );
                } unsafe { ulong actualValue =
                    *(ulong*)controllers[ current ].GetPointer( variable ).ToPointer();
                    controllers[ current ].LetPoint( variable, IntPtr.Zero );
                    *(ulong*)controllers[ current ].GetPointer( variable ).ToPointer()
                  = actualValue; }
            }
            // free one distinct ControllerVariable of this controller from exactly one distinct ControlllerVariable of the given distant controller instance
            public void Free( ControllerVariable thisVariable,
                              IInterValuable otherElement,
                              ControllerVariable otherVariable ) {
                if( otherElement.valence<ControllerType>().controller<ControllerType>().GetPointer( otherVariable ) 
                 == controllers[ current ].GetPointer( thisVariable ) ) {
                    (this as IControllerValenceField<ControllerType>).Free( thisVariable, otherElement );
                }
            }
            // free exactly one distinct ControllerVariable of this controller from a distinct ControllerVariable that resides on a distinct 'Control' element
            public void Free( ControllerVariable thisVariable,
                              IInterValuable otherElement,
                              Enum otherValence,
                              ControllerVariable otherVariable ) {
                if( otherElement.valence<ControllerType>( otherValence ).controller<ControllerType>().GetPointer( otherVariable ) 
                 == controllers[ current ].GetPointer( thisVariable ) ) {
                    (this as IControllerValenceField<ControllerType>).Free( thisVariable, otherElement );
                }
            }
        }

        public static class Valence
        {
            public struct FieldDescriptor {
                public readonly Enum  Index;
                public readonly Type  VarType;
                public readonly IInterValuable Element;
                public FieldDescriptor( Type ty, object el, Enum fi ) {
                    Index = fi;
                    VarType = ty;
                    Element = el as IInterValuable;
                }
                public override string ToString() {
                    return string.Format( "{0}->{1} field {2}",
                        (Element as Control).Name,
                        VarType.Name,
                        Index.ToString()    
                    );
                }
            };
            public class FieldIndex {
                public Enum Index;
                public static implicit operator int( FieldIndex cast ) {
                    return cast.Indices.IndexOf( cast.Index );                    
                } 
                internal FieldIndex( Enum fromEnum ) {
                    Index = fromEnum;
                }
                public FieldIndices Indices {
                    get { return Enum.GetValues( Index.GetType() ); }
                }
                public override string ToString() {
                    return Index.ToString();
                }
                public long Value {
                    get { return Index.ToValue(); }
                }
            };
            public class FieldIndices : IEnumerable {
                private Array collection;
                internal FieldIndices( Array fromArray ) {
                    collection = new Enum[fromArray.Length];
                    fromArray.CopyTo(collection,0);
                }
                public int Count {
                    get { return collection.Length; }
                }
                public override string ToString() {
                    return collection.GetValue(0).GetType().Name;
                }

                public IEnumerator GetEnumerator()
                {
                    return collection.GetEnumerator();
                }

                public static implicit operator FieldIndices( Array cast ) {
                    return new FieldIndices(cast); 
                }
                public Enum this[int index] {
                    get { return collection.GetValue(index) as Enum; }
                }
                public int IndexOf( Enum element )
                {
                    for( int i = 0; i < collection.Length; ++i )
                        if( (collection.GetValue(i) as Enum).CompareTo( element ) == 0 )
                            return i;
                    return -1;
                }
            };

            private static uint                    idGenerator;
            private static Dictionary<object,uint> elementlist;
            private static Hashtable               dictionarries;
            private static Hashtable               divergedTypes;
            private static uint                    NewId {
                get { return ++idGenerator; }
            }


            static Valence()
            {
                idGenerator = 0;
                elementlist = new Dictionary<object,uint>(2);
                dictionarries = new Hashtable(new Dictionary<Type,Dictionary<uint,object>>());
                divergedTypes = new Hashtable(new Dictionary<Type,Dictionary<uint,List<Enum>>>());
            }

            /// <summary>CompatibleElements&gtcT&lt()
            /// Returns a List of InterValuable control elements with are compatible
            /// for joining valence variables of compiletime known generic type parameter</summary>
            /// <typeparam name="cT">Type of Controller Variables the returned elements 
            /// are able joining there variables with other elements</typeparam>
            /// <returns>List&gtIInterValuable&gtcT&lt&lt</returns>
            public static IInterValuable<cT>[] CompatibleElements<cT>()
                where cT : ControllerBase {
                Dictionary<uint,object> dict = dictionarries[typeof(cT)] as Dictionary<uint,object>;
                IInterValuable<cT>[] list = new IInterValuable<cT>[dict.Count];
                dict.Values.CopyTo( list, 0 );
                return list;
            }

            /// <summary>CompatibleElements(Type compatible)
            /// Returns a List of InterValuable control elements wich are compatible
            /// for joining valence variables of the given, at runtime evaluated type</summary>
            /// <param name="cT">Type of Controller Variables the returned elements 
            /// are able joining there variables with</param><returns>
            /// IInterValuable[] or null if cT is no valid valence type</returns>
            public static IInterValuable[] CompatibleElements(Type cT) {
                if( cT.BaseType == typeof(ControllerBase) ) {
                    Dictionary<uint,object> dict = dictionarries[cT] as Dictionary<uint,object>;
                    IInterValuable[] list = new IInterValuable[dict.Count];
                    dict.Values.CopyTo( list, 0 );
                    return list;
                } return null;
            }

            /// <summary>CompatibleFields&gtcT&lt()
            /// Returns a List of ValenceFields (residing on regestered intervaluable
            /// control elements) which have variables of compatible data type for
            /// joining valence variables with these. (difference to CompatibleElements
            /// function is that CompatibleFields&gtcT&lt() returnes a list which 
            /// contains also valence fields of control elements of different 'main'
            /// data type, but which declare additional valence fields of different
            /// type then the 'main value' properties type)</summary>
            /// <typeparam name="cT">Type of Controller Variables the returned valence
            /// fields are able joining their variables with variables on other elements</typeparam>
            /// <returns>List&gtControllerField&lt - where ControllerField is container
            /// containing information about valence field and control elements where
            /// these field actually residing on (parent elements where belonging to)</returns>
            public static List<FieldDescriptor> CompatibleFields<cT>()
                where cT : ControllerBase
            {
                List<FieldDescriptor> list = new List<FieldDescriptor>();
                if( dictionarries.Contains( typeof(cT) ) ) {
                    Dictionary<uint,object> dict = dictionarries[typeof(cT)] as Dictionary<uint,object>;
                    foreach (KeyValuePair<uint,object> elm in dict) {
                        FieldIndices indices = (elm.Value as IInterValuable<cT>).valence().Field.Indices;
                        for(int i=0; i<indices.Count; ++i )
                            if( (elm.Value as IInterValuable
                                 ).valence<ControllerBase>(indices[i])
                               .controller<ControllerBase>() is cT )
                                list.Add( new FieldDescriptor( typeof(cT), elm.Value, indices[i] ) );  
                    }
                }
                if( divergedTypes.Contains( typeof(cT) ) ) {
                    Dictionary<uint,List<Enum>> enums = divergedTypes[typeof(cT)] as Dictionary<uint,List<Enum>>;
                    foreach( KeyValuePair<uint,List<Enum>> ent in enums ) {
                        object elm = FindInterValuableElement( ent.Key );
                        foreach( Enum idx in ent.Value )
                            list.Add( new FieldDescriptor( typeof(cT), elm, idx ) );
                    }
                } return list;
            }

            /// <summary>CompatibleFields(Type compatible)
            /// Returns a List of ValenceFields (residing on regestered intervaluable
            /// control elements) which have variables of compatible data type for
            /// joining valence variables with these. (difference to CompatibleElements
            /// function is that CompatibleFields() returnes a list which 
            /// contains also valence fields of control elements with differen 'main'
            /// data type, but which declare additional valence fields of types different
            /// then the 'main value' property type)</summary>
            /// <param name="cT">Type of Controller Variables the returned valence
            /// fields are able joining their variables with variables on other elements</param>
            /// <returns>List&gtControllerField&lt - where ControllerField is container
            /// containing information about valence field and control elements where
            /// these field actually residing on (parent elements where belonging to)</returns>
            public static List<FieldDescriptor> CompatibleFields( Type cT ) {
                if(cT.BaseType == typeof(ControllerBase)) 
            {
                List<FieldDescriptor> list = new List<FieldDescriptor>();
                if( dictionarries.Contains( cT ) ) {
                    Dictionary<uint,object> dict = dictionarries[cT] as Dictionary<uint,object>;
                    foreach( KeyValuePair<uint,object> elm in dict ) {
                        FieldIndices indices = (elm.Value as IInterValuable).valence<ControllerBase>().Field.Indices;
                        for( int i=0; i<indices.Count; ++i )
                            if( (elm.Value as IInterValuable
                                 ).valence<ControllerBase>( indices[i] )
                               .controller<ControllerBase>().GetType() == cT )
                                list.Add( new FieldDescriptor( cT, elm.Value, indices[i] ) );  
                    }
                }
                if( divergedTypes.Contains( cT ) ) {
                    Dictionary<uint,List<Enum>> enums = divergedTypes[cT] as Dictionary<uint,List<Enum>>;
                    foreach( KeyValuePair<uint,List<Enum>> ent in enums ) {
                        object elm = FindInterValuableElement( ent.Key );
                        foreach( Enum idx in ent.Value )
                            list.Add( new FieldDescriptor( cT, elm, idx ) );
                    }
                } return list;
            } return null; }

            public static void RegisterIntervaluableType<cT>() 
                where cT : ControllerBase {
                if( !dictionarries.ContainsKey( typeof(cT) ) ) {
                    dictionarries.Add( typeof(cT), new Dictionary<uint,object>() );
                    divergedTypes.Add( typeof(cT), new Dictionary<uint,List<Enum>>() );
                } 
            }

            public static uint RegisterIntervaluableElement<cT>( IInterValuable<cT> element )
                where cT : ControllerBase {
                if(!elementlist.ContainsKey( element )) {
                    elementlist.Add( element, NewId );
                    Dictionary<uint,object> dict = dictionarries[typeof(cT)] as Dictionary<uint,object>;
                    dict.Add( idGenerator, element );
                    return  idGenerator;
                } else return elementlist[element];
            }

            public static void RegisterValenceOfDifferentType( uint elementId, Enum fieldEnum, Type ctrltype )
            {
                if (!divergedTypes.ContainsKey(ctrltype)) {
                    divergedTypes.Add( ctrltype, new Dictionary<uint, List<Enum>>() );
                }
                Dictionary<uint,List<Enum>> additionals = divergedTypes[ctrltype] as Dictionary<uint,List<Enum>>;

                if( !additionals.ContainsKey( elementId ) ) {
                     additionals.Add( elementId, new List<Enum>(1) );
                } additionals[elementId].Add( fieldEnum );
            }
            
            public static object FindInterValuableElement( uint byElmId )
            {
                foreach( KeyValuePair<object,uint> elm in elementlist )
                    if( elm.Value == byElmId )
                        return elm.Key;
                return null;
            }
            
            /// <summary>UnRegisterIntervaluableElement<CtrlrType>(element)
            /// Unregisters an element which was previously registered via
            /// RegisterInterValuableElement<CtrlrType>() function. At best
            /// to be called from within control elements disposal function
            /// </summary><typeparam name="cT">The (ControlledValue) Type of
            /// the element's primary valenceable property</typeparam>
            /// <param name="element">The IIntervaluable Control Element
            /// which is going to to be unregistered </param>
            public static void UnRegisterIntervaluableElement<cT>( IInterValuable<cT> element )
                where cT : ControllerBase {
                if( elementlist.ContainsKey(element) ) {
                    uint foundID = elementlist[element];
                    if( divergedTypes.ContainsKey(foundID) )
                        divergedTypes.Remove(foundID);
                    elementlist.Remove(foundID);
                } element.valence().FreeAllFields();
            }
        }
    }
}
