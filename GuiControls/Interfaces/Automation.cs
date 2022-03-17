using System;
using Stepflow;
using System.Collections.Generic;
using Win32Imports;
using System.Runtime.InteropServices;
#if   USE_WITH_WF
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rect = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rect = System.Windows.Int32Rect;
using RectF = System.Windows.Rect;
#endif

namespace Stepflow
{
    namespace Gui
    {
        namespace Automation
        {

            /// <summary> IncommingAutomation 
            /// Delegate type used for handling Events which Control elements can receive via
            /// AutomationProtocol if an element implememts such interface </summary>
            /// <typeparam name="MsgType"> Type of EventData the Automation event delivers (for
            /// Automation via Midi protocol this type would be type MidiMessage )</typeparam>
            public delegate void IncommingAutomation<MsgType>( object sender, MsgType command );

            public enum AutomationDirection { Input, Output, Thruput };
            
            public struct MidiController
            {
                public int channel;
                public int control;
                public int resolut;
                public MidiController( int ch, int cc, int rs )
                {
                    channel = ch;
                    control = cc;
                    resolut = rs;
                }
            }

            public class InvokationByAutomationProtocol
            {
                public delegate void Invokation(object sender,EventArgs e);
                private object target;
                private Invokation call;

                public InvokationByAutomationProtocol(object targetObj)
                {
                    this.target = targetObj;
                    call = InvokationByAutomationProtocol.Invokation.CreateDelegate(typeof(Invokation), targetObj.GetType().GetMethod("ProcessMessageQueue")) as Invokation;
                }
            }
            
            /// <summary>
            /// AutomationlayerAddressat (struct)
            /// A structure which describes an abstract endpoint in a connected automation environment.
            /// It not complains to any distinct protocol which may be used, but matches anywhere
            /// instead. It is used for telling a control element that it should listen to some distinct
            /// message it may receive via its implemented AutomationProtocol(s) (maybe several), or to tell
            /// it shall forward it's own value change events to some other automatable, external endpoint    
            /// which is listening for distinct messages also a AutomationLayerAdressat structure defines.
            /// </summary>
            [StructLayout(LayoutKind.Explicit,Size = 4,Pack = 1)]
            public unsafe struct AutomationlayerAddressat
            {
                [FieldOffset(0)]
                private fixed byte bytes[4];
                [FieldOffset(0)]
                private System.UInt32 value;
                [FieldOffset(0)]
                public System.Int16 loShort;
                [FieldOffset(2)]
                public System.Int16 hiShort;

                public System.Byte loByte
                {
                    get { fixed(byte* p = bytes) return p[0]; }
                    set { fixed(byte* p = bytes) p[0] = value; }
                }

                public System.Byte hiByte
                {
                    get { fixed(byte* p = bytes) return p[2]; }
                    set { fixed(byte* p = bytes) p[2] = value; }
                }

                public System.UInt16 address
                {
                    get { fixed(byte* p=bytes) return (ushort)(p[0]+(p[2]<<8)); }
                    set { loShort = (short)(value % 8); hiShort = (short)(value >> 8); }
                }
                public System.Byte tyByte
                {
                    get { fixed ( byte* p = bytes ) return p[1]; }
                    set { fixed ( byte* p = bytes ) p[1] = value; }
                }
                public System.Byte dryByte
                {
                    get { fixed ( byte* p = bytes ) return p[3]; }
                    set { fixed ( byte* p = bytes ) p[3] = value; }
                }
                public static implicit operator bool(AutomationlayerAddressat cast)
                {
                    return ( (cast.bytes[0] != 0) && (cast.bytes[2] != 0) );
                } 
                public static implicit operator uint(AutomationlayerAddressat cast)
                {
                    return cast.value;
                } 
                public AutomationlayerAddressat(long val) : this()
                {
                    value = (uint)val;
                }
                public AutomationlayerAddressat(short low,short high) : this()
                {
                    loShort = low;
                    hiShort = high;
                }
                public AutomationlayerAddressat(ushort addr) : this()
                {
                    address = addr;
                }
                public AutomationlayerAddressat(byte lo, byte ty, byte hi, byte dry) : this()
                {
                    loByte = lo; tyByte = ty; hiByte = hi; dryByte = dry;
                }
            }


            /// <summary>IAutomationProtocol (interface)
            /// Base class where all supported automation protocol component classes derive from</summary>
            public interface IAutomationProtocol {
                AutomationDirection direction { get; }
            }

           
            /// <summary>IAutomationControlable[MessageType] (interface)
            /// Interface for components which can make control elements able receiving 
            /// automation messages of any kinds of supported protocol, so these can be
            /// controlled via control elements on external hardware devices</summary>
            /// <typeparam name="MessageType">data type for messages being received</typeparam>
            public interface IAutomationControlable<MessageType> : IAutomationProtocol 
                where MessageType : struct
            { 
                event IncommingAutomation<MessageType> AutomationEvent;
                void InitializeComponent( IAutomat element, System.ComponentModel.IContainer elementConnector, Action elementInvalidator );
                Action invalidation { get; set; }

                /// <summary>accessor for retreiving this controll element instance,
                /// casted to having this IAutomationControlabe interface accessible.
                /// (just needed if 'explicitely' implemented)</summary><returns>
                /// 'this' casted to an 'IAutomationControlabe object'</returns>
                IAutomationControlable<MessageType> automation();
                
                /// <summary>Must be overriden by implementation which retrieve the controll elements name.</summary>
                string getName();
                
                /// <summary>RegisterAsMesssageListener(bindingdescriptor):
                /// register as observer at the message loop which receives the automation messages from
                /// other device</summary><param name="bindingDescriptor">An 'AutomationlayerAddressat'
                /// struct containing information about on which kind of automation signals or messages
                /// the element should listen to. (for midi registration this would be 'portid + channel
                /// + controller' triade for identfying a distinct controller change channel within
                /// the midi environment we are actually connected to)</param>
                void RegisterAsMesssageListener( AutomationlayerAddressat bindingDescriptor );

                /// <summary>SignOutFromAutomationLoop():
                /// ...same like 'RegisterAsMesssageListener()' but unregisters
                /// the element from the automation message loop</summary>
                void SignOutFromAutomationLoop();

                /// <summary>Retreive a descriptor which describes 'where' or 'how'
                /// an element is integrated within it's automation environment</summary>
                /// <returns>AutomationlayerAddressat struct containing information about
                /// this controll elements bound automation targets or sources</returns>
                AutomationlayerAddressat GetAutomationBindingDescriptor( int channel );

                void incommingMessagQueue( MessageType message );

                /// <summary>ProcessMessageQueue(sender,eventargs):
                /// progress and empty the queue of received incomming messages where listening to ...
                /// needs to be called on a application owned thread (like the GUI thread for example) 
                /// for retrieving information if the queue need to be progressed actually, 'invalidation'
                /// callback should be connected to some invalidator on the parent object. (e.g. like 
                /// calling Invalidate(), repaint(), or similar or setting flags to tell the application
                /// about an element is in dirty state and needs update or cleansening actually.)</summary>
                /// <param name="sender">reference to this controll element as being message sender</param>
                /// <param name="e">EventArgs transporting any maybe needed data related to the event</param>
                void ProcessMessageQueue( object sender, EventArgs e );
            }

            /// <summary>IAutomationController[MessageType] (interface)
            /// Interface for components which can make control elements able 'sending' automation event messages
            /// of given 'MessageType' when interacted (e.g. for automating other controlls on external devices,
            /// for automating audio/music equipment as either external hardware or internal software components
            /// </summary><typeparam name="MessageType">Data type of messages the control element sends</typeparam>
            public interface IAutomationController<MessageType> : IAutomationProtocol where MessageType : struct
            { 
                bool MessageForwardingEnabled { get; }
                IAutomationController<MessageType> automate();

                void InitializeComponent( IAutomat element, System.ComponentModel.IContainer elementConnector );

                /// <summary>OwValueChange(sender,eventargs)
                /// should be 'implemented' within the AutomationProtocol type used in conjunction with this 
                /// IAutomationControllable interface for deriving automatable ui elements from. for to making the 
                /// element sending out messages into the connected automation environment, this should be 'called'
                /// on the ui element every time the elemts value propertie(s) may change.
                /// (for example when user acts with the mouse on the element..., or if any other application
                /// parts may change value properties by directly calling functions on the element or by directly
                /// assigning values to variables on the element)</summary><param name="sender">reference to this
                /// controll element as being message sender</param><param name="value">the value which is set to
                /// this controll elements 'main' value property by automation events</param>
                void OnValueChange(object sender, float value);

                /// <summary>OwValueChange(sender,eventargs)
                /// should be 'implemented' within the AutomationProtocol type used in conjunction with this 
                /// IAutomationControllable interface for deriving automatable ui elements from. for to making the 
                /// element sending out messages into the connected automation environment, this should be 'called'
                /// by the ui element every time the elemts value propertie(s) may change.
                /// (for example when user acts with the mouse on the element..., or if any other application
                /// parts may change value properties by directly calling functions on the element or by directly
                /// assigning values to variables on the element)</summary><param name="sender">reference to this
                /// controll element as being message sender</param><param name="value">the value which is set to
                /// this controll elements 'main' value property by automation events</param>
                void OnValueChange(object sender, MessageType value);

                /// <summary>
                /// ConfigureAsMessagingAutomat(bindingDescriptor)
                /// </summary>
                /// <param name="bindingDescriptor"></param>
                /// <param name="channelAutomatic"> optional channal parameter </param>
                void ConfigureAsMessagingAutomat( AutomationlayerAddressat bindingDescriptor, int channelAutomatic );
            }


            public interface IAutomat
            {
                AutomationlayerAddressat[] channels { get; }
            }

        }
    }
}
