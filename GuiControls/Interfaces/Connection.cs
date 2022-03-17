using System;
using Stepflow;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if   USE_WITH_WF
using System.Windows.Forms;
#elif USE_WITH_WPF
using System.Windows.Controls;
#endif

namespace Stepflow
{
    namespace Gui
    {
        public interface IStepflowControlElementComponent 
        {
            IInterValuable getElement();
        }

        public interface IInterValuableElementComponent<cT> : IStepflowControlElementComponent 
            where cT : ControllerBase
        {
            IInterValuable<cT> element { get; set; }
        }

        public interface IControllerValenceField
        {
            void addInvalidator( Action InvalidationTrigger,
                                 ControllerVariable forVariable,
                                 int onController );

            IControllerValenceField<cT> field<cT>() where cT : ControllerBase;
            IControllerValenceField<cT> field<cT>(Enum fieldIndex) where cT : ControllerBase;

            /// <summary>getInvalidationTrigger()
            /// should return a simple trigger which other controlls can call
            /// on recognizing/progressing/causing any variable changes value
            /// to let this control also get informed about ( returning Invalidate
            /// as delegate or function pointer would be apropirate )  
            /// </summary><returns>Action</returns>
            Action getInvalidationTrigger();

            /// <summary>getInvalidationHandler(sender,eventarg)
            /// same like getInvalidationTrigger() but returns an EventHandler
            /// function which is able being attached to System.Windows.Forms
            /// Control.Invalidated events on control element instances
            /// </summary><returns>InvalidateEventHandler</returns>
            InvalidateEventHandler getInvalidationHandler();

            /// <summary>controller(), MUST BE IMPLEMENTED
            /// should return the elements main value property's controller
            /// instance which's valence variables will be made 'joinable'
            /// </summary><returns>Stepflow.Controlled.Value</returns>
            cT controller<cT>() where cT : ControllerBase;

            Valence.FieldIndex Field { get; }

            /// <summary>JoinVariable(varnam,element)
            /// Join a controll elements 'main' value variable storage with
            /// that variable of another control element in a 'union' style like manner.
            /// when joined, both controlls will use the same memory location
            /// for storing that value which they're representing. (e.g. assume a
            /// slider element which internally used variable for storing 
            /// actual value position, would be same variable in memory which
            /// maybe a progress bar element uses internaly for storing it's
            /// actual shown up percentage value...) In conjunction to just 
            /// joining variable storage, the binding consists from a notification
            /// hook, which makes each other known about one actually changed
            /// a value which's simultanuosly used by more then one control element
            /// for ensuring any involved controlls recognizing these changes
            /// being able updating, redrawing, triggering events or whatever. 
            /// </summary><param name="variable">Variable descriptor describing
            /// which variable of the controller should be joined to be shared
            /// together with some other element's controller variables</param>
            /// <param name="element">The other controll element where to
            /// join variables of its main value's controller instance</param>             
            void Join( ControllerVariable variable, IInterValuable element );
            void Join( ControllerVariable variable, IInterValuable otherElement,
                       ControllerVariable otherVariable );
            void Join( ControllerVariable variable, IInterValuable otherElement,
                       Enum otherValenceField, ControllerVariable otherVariable );

            /// <summary>IsJoined(variable)
            /// Check if some distinct variable on a controller is joined with
            /// another variable on another controller on another element.</summary>
            /// <param name="variable">ControllerBase.Variable which discribes
            /// which variable on the currently selected controller should be checked
            /// if it's maybe joined with another variable on another element</param>
            /// <returns>true if variable IS joined, false if it's NOT joined</returns>
            bool IsJoint( ControllerVariable variable );

            /// <summary>FreeAllFields()
            /// Free all valence field bindings this control element could have
            /// </summary>
            void FreeAllFields();

            /// <summary>Free()
            /// Frees selected valence field's maybe bound variables being not bound anymore.
            /// </summary>
            void Free();

            /// <summary>Free(variable)
            /// Frees a currently selected valence field's distinct variable from any
            /// bindings it eventually could be involved within.</summary>
            /// <param name="variable">the ControllerBase.Variable which (if it's part
            /// of a binding) will be freed from the variable on the distant element</param>
            void Free( ControllerVariable variable );

            /// <summary>Free(variable,element)
            /// Frees a distinct maybe bound variable on the currently selected
            /// valence field, but only when it's really bound to a distinct external
            /// 'element' at least. Any bindings to variables on other elements then 
            /// 'element' will stay intact and won't be touched at all</summary>
            /// <param name="variable">the ControllerBase.Variable which (if it's part
            /// of a binding) will be freed from their bound variables on that distant
            /// element which is pointed by the 'element' parameter</param>
            /// <param name="element">The element which's bindings to the own variable
            /// which is pointed by the 'variable' parameter should be freed.</param>
            void Free( ControllerVariable variable, IInterValuable element );

            /// <summary>Free(variable,element,variable)
            /// Frees a distinct maybe bound variable on the currently selected
            /// valence field, but only when it's bound really to some distinct
            /// 'otherVariable' which is located on that distant element pointed by 
            /// the 'otherElement' parameter. Any bindings to variables on different
            /// elements then 'otherElement', as well as any bindings to different 
            /// variables then 'otherVariable' (even if located on 'otherElement') won't
            /// be touched and will stay intact.</summary><param name="variable">
            /// the ControllerBase.Variable which (if it's part
            /// of a binding) will be freed from it's bound 'otherVariables' on that
            /// distant element which is pointed by the 'otherElement' parameter</param>
            /// <param name="otherElement">The element where bindings of 'otherVariable'
            /// to this currently selected valence field's 'variable' should be freed.
            /// </param><param name="otherVariable">A distinct variable on 'otherElement'
            /// which (if bound to this 'variable') will be freed being unbound</param>
            void Free( ControllerVariable variable, IInterValuable otherElement,
                       ControllerVariable otherVariable );

            /// <summary>Free(variable,element,valence,variable)
            /// Frees a distinct maybe bound variable on the currently selected
            /// valence field controller from some distinct 'otherVariable' which
            /// is located within some 'otherElement's distinct valence field named 
            /// by the 'otherValenceField' parameter. Any bindings to variables on
            /// different elements then 'otherElement', as well as any bindings to
            /// different variables then 'otherVariable' (even if these are located
            /// on 'otherElement') as well as 'otherVariables' on 'otherElement' 
            /// which are contained in any valence fields different then 'otherValence'
            /// won't be touched and their bindings will stay intact.</summary>
            /// <param name="variable">the ControllerBase.Variable which (if it's part
            /// of a binding) will be freed from it's binding to the 'otherVariables'
            /// located in a distinct 'otherValenceField' on a distant 'otherElement'
            /// </param><param name="otherElement">The element where bindings of
            /// 'otherVariable' in 'otherValenceField' should be freed from this
            /// currently selected valence field's 'variable' to be unbound again.
            /// </param><param name="otherValenceField">A distinct valence field
            /// declared on 'otherElement' which contains the 'otherVariable'.</param>
            /// <param name="otherVariable">A distinct variable on 'otherElement'
            /// in 'otherValenceField' which (if bound to this 'variable') will be
            /// freed from it's binding for being not bound anymore.</param>
            void Free( ControllerVariable variable, IInterValuable otherElement,
                       Enum otherValenceField, ControllerVariable otherVariable );

            /// <summary>SetDirty(condition) 
            /// to be called on a controll element which shares it's variables 
            /// with other controlls. should be called each time such joinable
            /// 'valence' value changes due to an element is progressing it's 
            /// functionallity. To let other (maybe involved) controlls informed
            /// about such value changes, and to make possible these can invalidate
            /// themself for updating also as needed.</summary>
            /// <param name="variables">A flag, describing which of the controller's
            /// valence variables (eventually several) got changes applied to and
            /// any other (eventually several) joined controll elements so would
            /// become inflicted within doing updates maybe also.</param>
            void SetDirty( ValenceFieldState.Flags variables );
        }
        public interface IInterValuable //: IDisposable
        {
            // <summary>valence() 
            /// Access to the IControllerValenceField interface on this element
            /// </summary>
            IControllerValenceField valence<cT>() where cT : ControllerBase;
            IControllerValenceField valence<cT>( Enum field ) where cT : ControllerBase;

            /// <summary>callOnInvalidated(eventargs)
            /// Event Handler which must be implemented for triggering any kind
            /// of additional actions related to the actual Control Elemet</summary>
            /// <param name="e">InvalidateEventArgs which the Control passes
            /// with generated Invalidation events which cause calling this hanler</param>
            void callOnInvalidated( InvalidateEventArgs e );

            /// <summary>
            /// InterValuable elements must implement the 'Invalidate()' function
            /// </summary>
            void Invalidate();

            /// <summary>
            /// getInvalidationTrigger() should return a simple trigger which
            /// other controlls can call on recognizing/progressing/causing
            /// any variable changes value to let this control also get informed
            /// about ( returning Invalidate as delegate or function pointer 
            /// would be apropirate ) </summary><returns>Action</returns>
            Action getInvalidationTrigger();

            /// <summary>
            /// getMenuHook() should return the 'ToolStripItemCollection'
            /// where components can add ther menu entries to if they may
            /// implement functionality which makes menu usage nessessary
            /// </summary><returns>ToolStripItemCollection</returns>
            ToolStripItemCollection getMenuHook();
        }

        public interface IInterValuable<cT> : IInterValuable
            where cT : ControllerBase
        {
            /// <summary>valence() Access to the IControllerValenceField
            /// interface on this element </summary>
            IControllerValenceField<cT> valence();
            /// <summary>valence( field ) Access to the IControllerValenceField
            /// interface of given 'field'</summary><param name="field"> enum
            /// index of distinct field </param>
            IControllerValenceField<cT> valence( Enum field );
        }

        public interface IControllerValenceField<cT> : IControllerValenceField
            where cT : ControllerBase 
        {
            /// <summary>controller(), MUST BE IMPLEMENTED
            /// should return the elements main value property's controller
            /// instance which's valence variables will be made 'joinable'
            /// </summary><returns>Stepflow.Controlled.Value</returns>
            cT controller();
        }
    }
}
