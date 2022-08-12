using System;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using Stepflow.Midi;
using Win32Imports.Midi;
using Message = Win32Imports.Midi.Message;

using System.Windows.Forms;

namespace Stepflow.Midi
{
    public class MidiInputMenu<MidiClass> where MidiClass : ImportWraper
    {
        public IMidiControlElement<MidiClass> element;
        public  ContextMenuStrip              midiIn_mnu;
        private ToolStripMenuItem             ContextMenuHook;
        public  ToolStripMenuItem             midiIn_mnu_binding_mnu;
        private ToolStripComboBox             midiIn_mnu_input_port;
        private ToolStripSeparator            midiIn_mnu_seperator1;
        private ToolStripMenuItem             midiIn_mnu_binding_learn;
        private ToolStripComboBox             midiIn_mnu_binding_channel;
        private ToolStripComboBox             midiIn_mnu_binding_control;
        private ToolStripMenuItem             midiIn_mnu_binding_accept;


        public static implicit operator ToolStripMenuItem( MidiInputMenu<MidiClass> cast ) {
            return cast.ContextMenuHook;
        }


        internal void OnPortChanged( object sender, ValueChangeArgs<int> newPortId )
        {
            if (sender != midiIn_mnu_input_port)
            {
                midiIn_mnu_input_port.SelectedIndexChanged -= midiIn_mnu_input_port_SelectedIndexChanged;
                midiIn_mnu_input_port.SelectedIndex = newPortId;
                midiIn_mnu_input_port.SelectedIndexChanged += midiIn_mnu_input_port_SelectedIndexChanged;
            }
        }

        private void OnLearnClick( object sender, EventArgs e )
        {
            if( midiIn_mnu_binding_learn.Checked ) {
                if ( element.binding is In ) {
                    (element.binding as In).RemoveAnyFilters();
                    (element.binding as MidiInput).setLearnung();
                } else if (element.binding is Thru ) {
                    (element.binding as Thru).RemoveAnyFilters();
                    (element.binding as MidiInOut).setLearnung();
                }
            }
        }

        private void OnSetClick( object sender, EventArgs e )
        {
            AutomationlayerAddressat layer = new AutomationlayerAddressat();
            layer.loShort = (short)(int)midiIn_mnu_binding_channel.Tag;
            layer.hiShort = (short)(int)midiIn_mnu_binding_control.Tag;
            if ( element.binding is MidiInput )
                (element.binding as MidiInput).RegisterAsMesssageListener( layer );
            else
            if ( element.binding is MidiInOut )
                (element.binding as MidiInOut).automation().RegisterAsMesssageListener( layer );
            midiIn_mnu.Close();
        }

        private void OnCancelClick( object sender, EventArgs e )
        {
            midiIn_mnu.Close();
        }

        private void midiIn_mnu_binding_Input_config_SelectedIndexChanged( object sender, EventArgs e )
        {
            ToolStripComboBox config = sender as ToolStripComboBox;
            config.Tag = config.SelectedIndex;
        }

        private void midiIn_mnu_input_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( element.binding is MidiInOut )
                (element.binding as MidiInOut).triggerPortChange( AutomationDirection.Input, sender, (element.binding as Thru).MidiInPortID = midiIn_mnu_input_port.SelectedIndex );
            else
            if ( element.binding is MidiInput )
                (element.binding as MidiInput).triggerPortChange( AutomationDirection.Input, sender, (element.binding as In).MidiInPortID = midiIn_mnu_input_port.SelectedIndex );
        }

        public MidiInputMenu( IMidiControlElement<MidiClass> parent,
                              System.ComponentModel.IContainer connector )
        {
            element = parent;
            element.inputMenu = this;


            this.midiIn_mnu = new System.Windows.Forms.ContextMenuStrip(connector);
            this.midiIn_mnu.SuspendLayout();

            this.midiIn_mnu_input_port = new System.Windows.Forms.ToolStripComboBox();
            this.midiIn_mnu_seperator1 = new System.Windows.Forms.ToolStripSeparator();
            this.midiIn_mnu_binding_mnu = new System.Windows.Forms.ToolStripMenuItem();
            this.midiIn_mnu_binding_learn = new System.Windows.Forms.ToolStripMenuItem();
            this.midiIn_mnu_binding_channel = new System.Windows.Forms.ToolStripComboBox();
            this.midiIn_mnu_binding_control = new System.Windows.Forms.ToolStripComboBox();
            this.midiIn_mnu_binding_accept = new System.Windows.Forms.ToolStripMenuItem();
            

            this.midiIn_mnu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.midiIn_mnu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.midiIn_mnu_seperator1,
            this.midiIn_mnu_binding_mnu});
            this.midiIn_mnu.Name = "midiIn_mnu";
            this.midiIn_mnu.Size = new System.Drawing.Size(182, 114);
            this.midiIn_mnu.Text = "Midi Input";

            this.midiIn_mnu_input_port.Name = "midiIn_mnu_input_port";
            this.midiIn_mnu_input_port.Size = new System.Drawing.Size(121, 33);

            if( element.binding is MidiInOut ) {
                int numPorts = (element.binding as MidiInOut).NumberOfMidiInPorts;
                for (int i = 0; i < numPorts; ++i) {
                    this.midiIn_mnu_input_port.Items.Add((element.binding as MidiInOut).MidiInPortName(i));
                }
            } else if( element.binding is MidiInput ) {
                int numPorts = (element.binding as MidiInput).NumberOfMidiInPorts;
                for (int i = 0; i < numPorts; ++i) {
                    this.midiIn_mnu_input_port.Items.Add((element.binding as MidiInput).MidiInPortName(i));
                }
            }

            this.midiIn_mnu_seperator1.Name = "midiIn_mnu_seperator1";
            this.midiIn_mnu_seperator1.Size = new System.Drawing.Size(195, 6);

            this.midiIn_mnu_binding_mnu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.midiIn_mnu_input_port,
            this.midiIn_mnu_binding_channel,
            this.midiIn_mnu_binding_control,
            this.midiIn_mnu_binding_learn,
            this.midiIn_mnu_binding_accept});
            this.midiIn_mnu_binding_mnu.Name = "midiIn_mnu_binding_mnu";
            this.midiIn_mnu_binding_mnu.Size = new System.Drawing.Size(198, 30);
            this.midiIn_mnu_binding_mnu.Text = "Input Binding";

            this.midiIn_mnu_binding_learn.Name = "midiIn_mnu_binding_learn";
            this.midiIn_mnu_binding_learn.Size = new System.Drawing.Size(193, 30);
            this.midiIn_mnu_binding_learn.Text = "Learn...";
            this.midiIn_mnu_binding_learn.CheckOnClick = true;

            this.midiIn_mnu_binding_channel.Items.AddRange(new object[] {
                 "ALL","1","2","3","4","5","6","7","8",
                 "9","10","11","12","13","14","15","16" });
            this.midiIn_mnu_binding_channel.Name = "midiIn_mnu_binding_channel";
            this.midiIn_mnu_binding_channel.Size = new System.Drawing.Size(121, 33);
            this.midiIn_mnu_binding_channel.Text = "1";
            this.midiIn_mnu_binding_channel.Tag = 1;
            this.midiIn_mnu_binding_channel.SelectedIndexChanged += midiIn_mnu_binding_Input_config_SelectedIndexChanged;

            object[] itemgenerator = new object[128];
            for (int i = 0; i < 128; ++i)
                itemgenerator[i] = i.ToString();
            this.midiIn_mnu_binding_control.Items.AddRange(itemgenerator);
            this.midiIn_mnu_binding_control.Items.Add("HiRes");

            this.midiIn_mnu_binding_control.Name = "midiIn_mnu_binding_control";
            this.midiIn_mnu_binding_control.Size = new System.Drawing.Size(121, 33);
            this.midiIn_mnu_binding_control.Text = "33";
            this.midiIn_mnu_binding_control.Tag = 33;
            this.midiIn_mnu_binding_control.SelectedIndexChanged += midiIn_mnu_binding_Input_config_SelectedIndexChanged;

            this.midiIn_mnu_binding_accept.Name = "midiIn_mnu_binding_accept";
            this.midiIn_mnu_binding_accept.Size = new System.Drawing.Size(193, 30);
            this.midiIn_mnu_binding_accept.Text = "Set";

            midiIn_mnu_input_port.SelectedIndexChanged += midiIn_mnu_input_port_SelectedIndexChanged;
            midiIn_mnu_binding_accept.Click += OnSetClick;
            midiIn_mnu_binding_learn.Click += OnLearnClick;
            midiIn_mnu.AutoClose = false;

            this.ContextMenuHook = new ToolStripMenuItem();
            this.ContextMenuHook.Name = "ContextMenuHook";
            this.ContextMenuHook.Text = "Midi Input";
            this.ContextMenuHook.CheckOnClick = true;
            this.ContextMenuHook.Checked = false;
            this.ContextMenuHook.DropDown = midiIn_mnu_binding_mnu.DropDown;

            midiIn_mnu.ResumeLayout();
            (element as IInterValuable).getMenuHook().Add( this );
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////
   /// MidiOutputMenu

    public class MidiOutputMenu<MidiClass> where MidiClass : Win32Imports.Midi.ImportWraper
    {
        public IMidiControlElement<MidiClass> element;
        private int  destination;

        public  ContextMenuStrip   midiOut_mnu;
        private ToolStripComboBox  midiOut_mnu_output_port;
        private ToolStripSeparator midiOut_mnu_seperator1;
        public  ToolStripMenuItem  midiOut_mnu_binding_mnu;
        private ToolStripComboBox  midiOut_mnu_binding_channel;
        private ToolStripComboBox  midiOut_mnu_binding_control;
        private ToolStripMenuItem  midiOut_mnu_binding_accept;
        private ToolStripMenuItem  ContextMenuHook;


        public static implicit operator ToolStripMenuItem( MidiOutputMenu<MidiClass> cast ) {
            return cast.ContextMenuHook;
        }


        
        internal void OnPortChanged( object sender, ValueChangeArgs<int> newPortId )
        {
            if (sender != midiOut_mnu_output_port) {
                midiOut_mnu_output_port.SelectedIndexChanged -= Midi_mnu_output_port_SelectedIndexChanged;
                midiOut_mnu_output_port.SelectedIndex = newPortId;
                midiOut_mnu_output_port.SelectedIndexChanged += Midi_mnu_output_port_SelectedIndexChanged;
            }
        }

        public int AddressableAutomationChannels {
            get { return element.channels.Length; }
        } 
        public AutomationlayerAddressat GetAutomationDestination( int addressedChannel ) {
            return element.channels[addressedChannel];
        }
        public void SetAutomationDestination( int addresseChannel, AutomationlayerAddressat value ) {
            element.channels[addresseChannel] = value;
        }

        private void OnSetClick( object sender, EventArgs e )
        {
            AutomationlayerAddressat automat = new AutomationlayerAddressat(
                (byte)(int)midiOut_mnu_binding_channel.Tag,  (byte)Message.TYPE.CTRL_CHANGE,
                (byte)(int)midiOut_mnu_binding_control.Tag, (byte)midiOut_mnu_output_port.SelectedIndex
            );

            if( element.binding is MidiOutput )
                (element.binding as MidiOutput).ConfigureAsMessagingAutomat(automat, destination);
            else
                (element.binding as MidiInOut).ConfigureAsMessagingAutomat(automat, destination);
            
            midiOut_mnu.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            midiOut_mnu.Close();
        }

        private void Midi_mnu_binding_output_config_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox config = sender as ToolStripComboBox;
            config.Tag = config.SelectedIndex;
        }

        private void Midi_mnu_output_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( element.binding is MidiInOut )
                (element.binding as MidiInOut).triggerPortChange(AutomationDirection.Output, sender, (element.binding as Thru).MidiOutPortID = midiOut_mnu_output_port.SelectedIndex );
            else
            if ( element.binding is MidiOutput )
                (element.binding as MidiOutput).triggerPortChange(AutomationDirection.Output, sender, (element.binding as Out).MidiOutPortID = midiOut_mnu_output_port.SelectedIndex );
        }

        public MidiOutputMenu( IMidiControlElement<MidiClass> parent,
                               System.ComponentModel.IContainer connector )
        {
            element = parent;
            if( typeof(MidiClass) == typeof(MidiInput) ) {
                element.outputMenu = null;
                MessageLogger.logInfoWichtig( "MidiOutputMenu not supported on input only elements" );
                return;
            } else          
            element.outputMenu = this;
            destination = 0;

            this.midiOut_mnu = new System.Windows.Forms.ContextMenuStrip(connector);
            this.midiOut_mnu_output_port = new System.Windows.Forms.ToolStripComboBox();
            this.midiOut_mnu_seperator1 = new System.Windows.Forms.ToolStripSeparator();
            this.midiOut_mnu_binding_mnu = new System.Windows.Forms.ToolStripMenuItem();
            this.midiOut_mnu_binding_channel = new System.Windows.Forms.ToolStripComboBox();
            this.midiOut_mnu_binding_control = new System.Windows.Forms.ToolStripComboBox();
            this.midiOut_mnu_binding_accept = new System.Windows.Forms.ToolStripMenuItem();
            this.midiOut_mnu.SuspendLayout();

            this.midiOut_mnu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.midiOut_mnu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.midiOut_mnu_seperator1,
            this.midiOut_mnu_binding_mnu});
            this.midiOut_mnu.Name = "midiOut_mnu";
            this.midiOut_mnu.Size = new System.Drawing.Size(182, 114);
            this.midiOut_mnu.Text = "Midi Output Settings";

            this.midiOut_mnu_output_port.Name = "midiOut_mnu_output_port";
            this.midiOut_mnu_output_port.Size = new System.Drawing.Size(121, 33);

            if( element.binding is MidiInOut ) {
                int numPorts = (element.binding as MidiInOut).NumberOfMidiOutPorts;
                for (int i = 0; i < numPorts; ++i) {
                    this.midiOut_mnu_output_port.Items.Add( (element.binding as MidiInOut).MidiOutPortName(i) );
                }
            } else if( element.binding is MidiOutput ) {
                int numPorts = (element.binding as MidiOutput).NumberOfMidiOutPorts;
                for (int i = 0; i < numPorts; ++i) {
                    this.midiOut_mnu_output_port.Items.Add( (element.binding as MidiOutput).MidiOutPortName(i) );
                }
            }

            this.midiOut_mnu_seperator1.Name = "midiOut_mnu_seperator1";
            this.midiOut_mnu_seperator1.Size = new System.Drawing.Size(195, 6);

            this.midiOut_mnu_binding_mnu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.midiOut_mnu_output_port,
            this.midiOut_mnu_binding_channel,
            this.midiOut_mnu_binding_control,
            this.midiOut_mnu_binding_accept});
            this.midiOut_mnu_binding_mnu.Name = "midiOut_mnu_binding_mnu";
            this.midiOut_mnu_binding_mnu.Size = new System.Drawing.Size(198, 30);
            this.midiOut_mnu_binding_mnu.Text = "Output Binding";

            this.midiOut_mnu_binding_channel.Items.AddRange(new object[] {
                 "ALL","1","2","3","4","5","6","7","8",
                 "9","10","11","12","13","14","15","16" });
            this.midiOut_mnu_binding_channel.Name = "midiOut_mnu_binding_channel";
            this.midiOut_mnu_binding_channel.Size = new System.Drawing.Size(121, 33);
            this.midiOut_mnu_binding_channel.Text = "1";
            this.midiOut_mnu_binding_channel.Tag = 1;
            this.midiOut_mnu_binding_channel.SelectedIndexChanged += Midi_mnu_binding_output_config_SelectedIndexChanged;

            object[] itemgenerator = new object[128];
            for (int i = 0; i < 128; ++i)
                itemgenerator[i] = i.ToString();
            this.midiOut_mnu_binding_control.Items.AddRange(itemgenerator);
            this.midiOut_mnu_binding_control.Items.Add("HiRes");

            this.midiOut_mnu_binding_control.Name = "midiOut_mnu_binding_control";
            this.midiOut_mnu_binding_control.Size = new System.Drawing.Size(121, 33);
            this.midiOut_mnu_binding_control.Text = "33";
            this.midiOut_mnu_binding_control.Tag = 33;
            this.midiOut_mnu_binding_control.SelectedIndexChanged += Midi_mnu_binding_output_config_SelectedIndexChanged;

            this.midiOut_mnu_binding_accept.Name = "midi_mnu_binding_accept";
            this.midiOut_mnu_binding_accept.Size = new System.Drawing.Size(193, 30);
            this.midiOut_mnu_binding_accept.Text = "Set";
 
            midiOut_mnu_output_port.SelectedIndexChanged += Midi_mnu_output_port_SelectedIndexChanged;
            midiOut_mnu_binding_accept.Click += OnSetClick;
            midiOut_mnu.AutoClose = false;

            ContextMenuHook = new ToolStripMenuItem();
            ContextMenuHook.Name = "mnu_MidiOutput";
            ContextMenuHook.Text = "Midi Output";
            ContextMenuHook.CheckOnClick = true;
            ContextMenuHook.Checked = false;
            ContextMenuHook.DropDown = midiOut_mnu_binding_mnu.DropDown;

            midiOut_mnu.ResumeLayout();
            (element as IInterValuable).getMenuHook().Add( this );
        }
    }

}
