using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Win32Imports;
using Win32Imports.Midi;
using Stepflow.Midi.ControlHelpers;
using Message = Win32Imports.Midi.Message;

namespace Stepflow.Gui.Automation
{
    public partial class MidiInput 
        : In
        , IAutomationControlable<Message>
    {
        public event IncommingAutomation<Message> AutomationEvent;
        public event ValueChangeDelegate<int>     InpPortChanged;
        
        private MidiController     controll;
        private Queue<Message>     incoming;
        private volatile bool      learning;
        private volatile string    learnedInput;
        private Action             invalidator;
        private IMidiControlElement<MidiInput> element;


        internal void triggerPortChange(AutomationDirection direction,object sender, int newPort)
        {
            if( direction == AutomationDirection.Input )
                InpPortChanged?.Invoke(sender, newPort);
        }

        public void InitializeComponent( IAutomat parent, IContainer parentConnector, Action parentInvalidator )
        {
            element = parent as IMidiControlElement<MidiInput>;
            IContainer components = parentConnector;
            automation().invalidation = parentInvalidator;
            element.inputMenu = new Midi.MidiInputMenu<MidiInput>(element, components);
            InpPortChanged += element.inputMenu.OnPortChanged;
        }

        public MidiInput()
            : base()
        {  
            incoming = new Queue<Message>(0);
            controll = new MidiController(-1,-1,-1);
            IncomingMidiMessage += automation().incommingMessagQueue;
            return;   
        }

        public int MidiInDeviceID {
            get { return base.MidiInPortID; }
            set { if ( base.MidiInPortID != value ) {
                    InpPortChanged( this, base.MidiInPortID = value );
                }
            }
        }

        public AutomationDirection direction {
            get { return AutomationDirection.Input; }
        }

        private void Invalidate() {
            invalidator();
        }

        Action IAutomationControlable<Message>.invalidation {
            get { return invalidator; }
            set { invalidator = value; }
        }

        public IAutomationControlable<Message> automation() {
            return this;
        }

        public void RegisterAsMesssageListener( AutomationlayerAddressat bindingDescriptor )
        {
            if( (controll.channel == bindingDescriptor.loShort)
             && (controll.control == bindingDescriptor.hiShort) ) return;

            controll = new MidiController( bindingDescriptor.loShort,
                                           bindingDescriptor.hiShort > 127 ? 0 
                                         : bindingDescriptor.hiShort,
                                           bindingDescriptor.hiShort > 127
                                         ? bindingDescriptor.hiShort 
                                         : 127 );
            Message.Filter filt;
            if( controll.resolut < 128 ) {
                filt = new Message.Filter( (controll.channel - 1), controll.control );
            } else {
                filt = new Message.Filter( Message.TYPE.PITCH, (controll.channel-1) );
            } base.UseMessageFilter( filt );
        }

        public void SignOutFromAutomationLoop()
        {
            base.RemoveAnyFilters();
        }

        void IAutomationControlable<Message>.incommingMessagQueue( Message midiData )
        {
            if( !learning ) {
                incoming.Enqueue( midiData );
                automation().invalidation();
            } else { Message.TYPE type = midiData.Type;
                if ( type < Message.TYPE.SYSEX && type != Message.TYPE.PROG_CHANGE ) {
                    learnedInput = string.Format("{0}~{1}", (midiData.Channel+1).ToString(), midiData.Number);
                    learning = false;
                }
            }  
        }

        void IAutomationControlable<Message>.ProcessMessageQueue( object sender, EventArgs e )
        {
            if ( AutomationEvent != null ) {
                while ( incoming.Count > 0 )
                    AutomationEvent( sender, incoming.Dequeue() );
            } else {
                incoming.Clear();
            }
            //if ( learnedInput != null ) {
            //    if ( !midi_mnu.Visible )
            //        midi_mnu.Show();
            //    string[] chanNum = learnedInput.Split('~');
            //    midi_mnu_binding_channel.Text = chanNum[0];
            //    midi_mnu_binding_control.Text = chanNum[1];
            //    midi_mnu_binding_learn.Checked = false;
            //    learnedInput = null;
            //}
        }

        public string getName()
        {
            return (element as Control).Name;
        }

        public AutomationlayerAddressat GetAutomationBindingDescriptor( int descriptiionChannal )
        {
            return new AutomationlayerAddressat( (byte)MessageFilter.Chan,(byte)MessageFilter.loTy,
                                                 (byte)MessageFilter.from,(byte)MidiInPortID );
        }
    }
}
