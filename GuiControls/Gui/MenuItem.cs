using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Geometry;

namespace Stepflow.Gui
{
    public class TouchStripMenuItem
        : ToolStripMenuItem
        , ITouchableObject<TouchStripMenuItem>
    {
        private Form mainform;

        public TouchStripMenuItem() : base()
        {
            ( this as ITouchableObject<TouchStripMenuItem> ).handler = new ItemTouchHandler<TouchStripMenuItem>( this );
            if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                if( PointerInput.Dispatcher == null ) {
                    PointerInput.Initialized += ( this as ITouchableElement ).TouchInputReady;
                } else
                    PointerInput.Dispatcher.RegisterTouchableElement(this);
            }
            Application.Idle += Application_Idle;
            
        }

        private void Application_Idle( object sender, EventArgs e )
        {
            mainform = Application.OpenForms[0];
            Application.Idle -= Application_Idle;
        }

        ItemTouchHandler<TouchStripMenuItem> ITouchableObject<TouchStripMenuItem>.handler { get; set; }

        public ITouchEventTrigger touch { get { return ( this as ITouchableObject<TouchStripMenuItem> ).handler; } }

        Rectangle ITouchable.Bounds { get { return this.Bounds; } set { } }

        public Control Element { get { return Parent; } }

        public bool IsTouched { get { return touch.IsTouched; } }

        private IBasicTouchTrigger touchEvents()
        {
            return ( this as ITouchableObject<TouchStripMenuItem> ).handler.events();
        }

        public event FingerTip.TouchDelegate TouchDown {
            add { touchEvents().TouchDown += value; }
            remove { touchEvents().TouchDown -= value; }
        }

        public event FingerTip.TouchDelegate TouchLift {
            add { touchEvents().TouchLift += value; }
            remove { touchEvents().TouchLift -= value; }
        }

        public event FingerTip.TouchDelegate TouchMove {
            add { touchEvents().TouchMove += value; }
            remove { touchEvents().TouchMove -= value; }
        }

        ITouchableElement ITouchable.element()
        {
            return this;
        }

        void IBasicTouchable.OnTouchDown( FingerTip tip )
        {
            OnMouseDown( new MouseEventArgs( MouseButtons.Left, 1, tip.X, tip.Y, 0) );   
        }

        void IBasicTouchable.OnTouchLift( FingerTip tip )
        {
            OnMouseUp( new MouseEventArgs( MouseButtons.Left, 1, tip.X, tip.Y, 0) );
        }

        void IBasicTouchable.OnTouchMove( FingerTip tip )
        {
            OnMouseMove( new MouseEventArgs( MouseButtons.None, 0, tip.X, tip.Y, 0) );
        }

        ITouchDispatchTrigger ITouchable.screen()
        {
            return PointerInput.Dispatcher.screen();
        }

        Point64 ITouchableElement.ScreenLocation()
        {
            if( Element != null )
            if( mainform != null ) {
                return new Point64( mainform.Location ) + Bounds.Location;
            } return Point64.EMPTY;
        }

        IRectangle ITouchableElement.ScreenRectangle()
        {
            if( Element != null ) 
            if( mainform != null ) { 
                SystemDefault bounds = new SystemDefault( Bounds );
                bounds.Corner += mainform.Location;
                return bounds;
            } return CenterAndScale.Empty;
        }

        void ITouchableElement.TouchInputReady( PointerInput touchdevice )
        {
            PointerInput.Initialized -= ( this as ITouchableElement ).TouchInputReady;
            touchdevice.RegisterTouchableElement( this );
        }
    }
}
