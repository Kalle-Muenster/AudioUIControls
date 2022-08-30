using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Midi;
using Stepflow.Gui.Geometry;

namespace MidiGUI.Test.Container
{

    public static class Extensions
    {
        public static Consola.Test.ConTrol.Point ConTrolPoint( this Point32 point )
        {
            return new Consola.Test.ConTrol.Point( point.X, point.Y );
        }

        public static Consola.Test.ConTrol.Point ConTrolPoint( this Point64 point )
        {
            return new Consola.Test.ConTrol.Point( point.x, point.y );
        }

        public static Consola.Test.Area ConTrolArea( this IRectangle rectangle )
        {
            return new Consola.Test.Area( rectangle.Corner.ConTrolPoint(), rectangle.Sizes.ConTrolPoint() );
        }
    }

    public enum ControlFlags
    {
        Cycled, Inverted, Normal
    }

    public partial class Form1 : Form
    {
        public delegate void SetInt32Value(int setValue);
        public delegate void SetEnumValue(Enum setValue);
        public delegate void SetColorValue(Color setValue);
        public delegate void SetMainValue(object setValue);

        private event SetInt32Value SetWidth;
        private event SetInt32Value SetHeight;
        private event SetEnumValue  SetStylo;
        private event SetEnumValue  SetOrientation;
        private event SetEnumValue  SetLed;
        private event SetEnumValue  SetCycled;
        private event SetEnumValue  SetInvert;

        private SetMainValue         SetValue;

        private string               Staged;
        public  Dictionary<string,IInterValuable> Testling;
        private Action               destruct;
        public IRectangle            location;
        private int                  setHeight;
        private int                  setWidth;
        private object               setValue;

        private LedButton            btn_Invert;
        private LedButton            btn_Cycled;

        private LedButton AddButton( Point32 position, string name, string text )
        {
            LedButton button = new LedButton();
            button.AutoText = false;
            button.BackColor = System.Drawing.Color.FromArgb(255,32,32,32);
            button.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            button.CausesValidation = false;
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            button.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            button.LedLevel = 1F;
            button.LedValue = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            button.Location = new System.Drawing.Point(position.X,position.Y);
            button.Margin = new System.Windows.Forms.Padding(2);
            button.Mode = Stepflow.Gui.LedButton.Transit.OnRelease;
            button.Name = name;
            button.NumberOfStates = ( (byte)( 2 ) );
            button.SideChain = 0.95F;
            button.Size = new System.Drawing.Size(96,96);
            button.State = Stepflow.Gui.LedButton.Default.OFF;
            button.Style = Stepflow.Gui.Style.Dark;

            Controls.Add( button );
            button.SetUp( LED.Red, LED.off, LED.Gelb );
            button.Text = text;

            return button;
        } 

        private void Connect( Control staged )
        {
            if ( Controls.ContainsKey( Staged ) ) {
                SetWidth = null;
                SetHeight = null;
                SetStylo = null;
                SetInvert = null;
                SetCycled = null;
                IDisposable contrl = Controls.Find( Staged, false )[0];
                contrl.Dispose();
                Controls.Remove( contrl as Control );
            } Staged = staged.Name;

            if ( Testling.ContainsKey( Staged ) ) {
                Testling.Remove( Staged );
            }

            Testling.Add( Staged, (IInterValuable)staged );
            Controls.Add( staged );

            val_element_Val.valence().Free();
            val_element_Min.valence().Free();
            val_element_Max.valence().Free();

            SetOrientation = null;
            SetLed = null;
            
            if (staged is GuiSlider) {
                GuiSlider slider = staged as GuiSlider;
                SetOrientation += (Enum set) => {
                    slider.Orientation = (Stepflow.Gui.Orientation)set;
                    if( slider.Orientation == Stepflow.Gui.Orientation.Vertical )
                        slider.Inverted = true;
                    btn_Invert.State = slider.Inverted 
                                     ? LedButton.Default.ON
                                     : LedButton.Default.OFF;
                };
                SetLed += (Enum set) => { slider.LedColor = (LED)set; };
                SetStylo += (Enum set) => { slider.Style = (Style)set; };
                SetCycled += ( Enum set ) => { slider.Cycled = ( (LedButton.Default)set == LedButton.Default.ON ); };
                SetInvert += ( Enum set ) => { slider.Inverted = ( (LedButton.Default)set == LedButton.Default.ON ); };
                val_element_Val.Wrap( slider );
            } else if (staged is LedButton) {
                LedButton button = staged as LedButton;
                SetLed += (Enum set) => { button.DefineState( button.Index, button.State, (LED)set ); };
                SetStylo += (Enum set) => { button.Style = (Style)set; };
                val_element_Val.Wrap( button );
            } else if ( staged is GuiMeter ) {
                GuiMeter meter = staged as GuiMeter;
                SetOrientation += (Enum set) => {
                    meter.Orientation = (Stepflow.Gui.Orientation)set;
                    if( meter.Orientation == Stepflow.Gui.Orientation.Vertical )
                        meter.Direction = DirectionalOrientation.Up;
                    btn_Invert.State = meter.Inverted 
                                     ? LedButton.Default.ON
                                     : LedButton.Default.OFF;
                };
                SetStylo += (Enum set) => { meter.Style = (Style)set; };
                SetLed += (Enum set) => { meter.ForeColor = Color.FromArgb( (int)Stepflow.Gui.Helpers.LedGlower.ledCol[set.ToInt32()] ); };
                SetCycled += ( Enum set ) => { meter.Cycled = ( (LedButton.Default)set == LedButton.Default.ON ); };
                SetInvert += ( Enum set ) => { meter.Inverted = ( (LedButton.Default)set == LedButton.Default.ON ); };
                val_element_Val.Wrap( meter );
            } else if( staged is JogDial ) {
                JogDial dial = staged as JogDial;
                SetStylo += (Enum set) => { dial.Style = (Style)set; };
                SetLed += (Enum set) => { dial.LedColor = (LED)set; };
                val_element_Val.Wrap( dial );
            } else if ( staged is StringControl ) {
                StringControl guitarra = staged as StringControl;
                SetOrientation += (Enum set) => { guitarra.Orientation = (Stepflow.Gui.Orientation)set; };
            }

            val_element_Min.valence().Join( ControllerVariable.VAL, val_element_Val, ControllerVariable.MIN );
            val_element_Max.valence().Join( ControllerVariable.VAL, val_element_Val, ControllerVariable.MAX );

            val_element_Val.Invalidate();
            val_element_Min.Invalidate();
            val_element_Max.Invalidate();

            SetWidth += (int set) => { staged.Width = set; };
            SetHeight += (int set) => { staged.Height = set; };

            UpdateScreenLocation();
        }

        public Form1()
        {
            setHeight = -1;
            setWidth = -1;
            setValue = null;

            Staged = "";
            Testling = new Dictionary<string,IInterValuable>();

            InitializeComponent();

            val_set_width.Wrap( sld_set_width );
            val_set_height.Wrap( sld_set_height );

            sld_set_width.Minimum = 16;
            sld_set_width.Maximum = (Width * 2) / 3;

            sld_set_height.Minimum = 16;
            sld_set_height.Maximum = (Height * 2) / 3;
            sld_set_height.Inverted = false;

            sld_set_width.ValueChanged += Sld_set_width_ReleasedKnob;
            sld_set_height.ValueChanged += Sld_set_height_ReleasedKnob;

            btn_set_style.SetUp(LED.Blue, LED.Pink, LED.Mint);
            btn_set_style.DefineState(1, Style.Flat, LED.Blue);
            btn_set_style.DefineState(2, Style.Lite, LED.Pink);
            btn_set_style.DefineState(3, Style.Dark, LED.Green);

            btn_set_style.AutoText = true;
            btn_set_style.Changed += Btn_set_style_Changed;

            btn_set_Orientation.SetUp( LED.Blue, LED.Orange, LED.Cyan );
            btn_set_Orientation.DefineState(1, Stepflow.Gui.Orientation.Rondeal, LED.Blue);
            btn_set_Orientation.DefineState(2, Stepflow.Gui.Orientation.Horizontal, LED.Orange);
            btn_set_Orientation.DefineState(3, Stepflow.Gui.Orientation.Vertical, LED.Mint);
            btn_set_Orientation.AutoText = true;
            btn_set_Orientation.Changed += Btn_set_Orientation_Changed;

            btn_set_Led.SetUp( LED.Green, LED.Red, LED.Gelb );
            btn_set_Led.DefineState( 1, LED.Green, LED.Green );
            btn_set_Led.DefineState( 2, LED.Red, LED.Red );
            btn_set_Led.SetUpState( LED.Blue, LED.Blue );
            btn_set_Led.SetUpState( LED.Cyan, LED.Cyan );
            btn_set_Led.SetUpState( LED.Pink, LED.Pink );
            btn_set_Led.SetUpState( LED.Mint, LED.Mint );
            btn_set_Led.SetUpState( LED.Orange, LED.Orange );
            btn_set_Led.AutoText = true;
            btn_set_Led.Changed += Btn_set_Led_Changed;

            val_element_Min.Maximum = float.MaxValue;
            val_element_Min.Minimum = float.MinValue;

            val_element_Max.Maximum = float.MaxValue;
            val_element_Max.Minimum = float.MinValue;

            btn_Cycled = AddButton( new Point32(1059, 678),"btn_Cycled", "Cycled" );
            btn_Invert = AddButton( new Point32(948, 678), "btn_Invert", "Invert" );

            btn_Cycled.Changed += Btn_Cycled_Changed;
            btn_Invert.Changed += Btn_Invert_Changed;

            Load += Form1_Load;
        }



        private void UpdateScreenLocation()
        {
            location = CornerAndSize.FromRectangle( RectangleToScreen( new Rectangle(0,0, Width, Height) ) );
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            Load -= Form1_Load;
            UpdateScreenLocation();
        }

        public IInterValuable GetStagedControl() 
        {
            if( Testling.ContainsKey( Staged ) ) {
                return Testling[Staged];
            } return null;
        }

        public void SetControlHeight( int height )
        {
            setHeight = height;
            Invalidate();
        }
        
        public void SetControlWidth( int width )
        {
            setWidth = width;
            Invalidate();
        }

        public void SetControlValue( object value )
        {
            setValue = value;
            Invalidate();
        }

        public Point64 GetMenuPosition( string menupath )
        {
            return FindMenuPosition( menupath, null, location.Corner );
        }
        private Point64 FindMenuPosition( string itempath, ToolStripItemCollection menuitems, Point32 offset )
        {
            if( menuitems == null ) {
                menuitems = menuStrip1.Items;
            }

            string[] path = itempath.Split('.',2);
            foreach( ToolStripMenuItem item in menuitems ) {
                if( item.Text == path[0] ) {
                    if( path.Length == 1 ) {
                        Rectangle rect = item.Bounds;
                        return CenterAndScale.FromRectangle( rect ).Center + offset;
                    } else {
                        offset.y += (short)( item.Bounds.Height );
                        return FindMenuPosition(path[1], item.DropDownItems, offset);
                    }
                }
            }
            return Point32.EMPTY;
        }

        public IRectangle GetTestlingArea()
        {
            CenterAndScale area = CenterAndScale.FromRectangle( (Testling[Staged] as Control).Bounds );
            area.Center += location.Corner;
            return area;
        }

        private void Btn_Invert_Changed( object sender, ValueChangeArgs<Enum> value )
        {
            SetInvert?.Invoke( value );
        }

        private void Btn_Cycled_Changed( object sender, ValueChangeArgs<Enum> value )
        {
            SetCycled?.Invoke( value );
        }

        private void Btn_set_Led_Changed(object sender, ValueChangeArgs<Enum> value)
        {
            SetLed?.Invoke( value.Value );
        }

        private void Btn_set_Orientation_Changed(object sender, ValueChangeArgs<Enum> value)
        {
            SetOrientation?.Invoke( value.Value );
        }

        private void Btn_set_style_Changed(object sender, ValueChangeArgs<Enum> value)
        {
            SetStylo?.Invoke( value.Value );
        }

        private void Sld_set_height_ReleasedKnob(object sender, ValueChangeArgs<float> value)
        {
            SetHeight?.Invoke( (int)value.Value );
        }

        private void Sld_set_width_ReleasedKnob(object sender, ValueChangeArgs<float> value)
        {
            SetWidth?.Invoke( (int)value.Value );
        }

        private void mnu_LedDisplay_Click( object sender, EventArgs e )
        {
            destruct?.Invoke();

            if( ( sender as ToolStripItem ).Text.Contains("Midi") ) {
                MidiValueDisplay dings = new MidiValueDisplay();
                dings.Location = new Point(200,200);
                dings.Size = new Size(256,64);
                dings.CanChangeUnits = true;
                dings.CanScaleUnits = true;
                dings.Units = UnitsType.Db;
                dings.Style = Style.Dark;
                dings.Tag = 1;
                Connect( dings );
                destruct = () => { dings.Dispose(); };
            } else {
                ValueDisplay dings = new ValueDisplay();
                dings.Location = new Point(200,200);
                dings.Size = new Size(256,64);
                dings.CanChangeUnits = true;
                dings.CanScaleUnits = true;
                dings.Units = UnitsType.Db;
                dings.Style = Style.Dark;
                dings.Tag = 1;
                Connect( dings );
                destruct = () => { dings.Dispose(); };
            }
        }

        private void mnu_MidiComboBox_Click( object sender, EventArgs e )
        {
            destruct?.Invoke();
            Stepflow.Midi.Gui.MidiComboBox dings = new Stepflow.Midi.Gui.MidiComboBox();
            dings.Location = new Point(200, 200);
            dings.Size = new Size(256,64);
            for( Win32Imports.Midi.Note note = Win32Imports.Midi.Note.C2; note < Win32Imports.Midi.Note.C4; ++note )
                dings.Items.Add( note );
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_MidiTrackBar_Click( object sender, EventArgs e )
        {
            destruct?.Invoke();
            Stepflow.Midi.Gui.MidiTrackBar dings = new Stepflow.Midi.Gui.MidiTrackBar();
            dings.Location = new Point(200, 200);
            dings.Size = new Size(256, 64);
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_LedButton_Click( object sender, EventArgs e )
        {
            destruct?.Invoke();
            LedButton button = (sender as ToolStripItem).Text.Contains("Midi") ? new MidiButton() : new LedButton();
            button.BackColor = BackColor;
            button.Location = new Point(200, 200);
            button.Size = new Size(64, 64);
            button.SetUp(LED.Green, LED.Red, LED.Gelb);
            button.AutoText = true;
            button.Style = Style.Dark;
            button.Tag = 1;
            Connect( button );
            destruct = () => { button.Dispose(); };
        }

        private void mnu_GuiSlider_Click(object sender, EventArgs e)
        {
            destruct?.Invoke();
            GuiSlider dings = (sender as ToolStripItem).Text.Contains("Midi") ? new MidiSlider() : new GuiSlider();
            dings.Location = new Point(200, 200);
            dings.Size = new Size(64, 64);
            dings.Orientation = Stepflow.Gui.Orientation.Rondeal;
            dings.Style = Style.Dark;
            dings.Interaction = GuiSlider.InteractionMode.Directional;
            dings.LedColor = LED.Pink;
            dings.BackColor = BackColor;
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_GuiMeter_Click(object sender, EventArgs e)
        {
            destruct?.Invoke();
            GuiMeter dings = (sender as ToolStripItem).Text.Contains("Midi") ? new MidiMeter() : new GuiMeter();
            dings.Location = new Point(200, 200);
            dings.Size = new Size(64,256);
            dings.Orientation = Stepflow.Gui.Orientation.Horizontal;
            dings.Style = Style.Dark;
            dings.Damped = true;
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_JogDial_Click(object sender, EventArgs e)
        {
            destruct?.Invoke();
            JogDial dings = new JogDial();
            dings.Location = new Point(120,120);
            dings.Size = new Size(384,384);
            dings.Style = Style.Dark;
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_StringControl_Click(object sender, EventArgs e)
        {
            destruct?.Invoke();
            StringControl dings = new StringControl();
            dings.Location = new Point(150,150);
            dings.Size = new Size(768,256);
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_GuiRangeControl_Click(object sender, EventArgs e)
        {
            destruct?.Invoke();
            GuiRanger dings = new GuiRanger();
            dings.BackColor = Color.FromArgb(255, 32, 32, 32);
            dings.Location = new Point(100, 300);
            dings.Size = new Size(512, 128);
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            base.OnPaint( e );
            if( setWidth > 0 ) {
                sld_set_width.Value = setWidth;
                setWidth = -1;
            }
            if (setHeight > 0 ) {
                sld_set_height.Value = setHeight;
                setHeight = -1;
            }
            if ( setValue != null ) {
                if( Testling[Staged] is LedButton ) ( Testling[Staged] as LedButton ).State = setValue as Enum;
                else if( Testling[Staged] is GuiSlider ) ( Testling[Staged] as GuiSlider ).Value = (float)setValue;
                else if( Testling[Staged] is GuiMeter ) ( Testling[Staged] as GuiMeter ).Level = (float)setValue;
                setValue = null;
            }
        }
    }
}
