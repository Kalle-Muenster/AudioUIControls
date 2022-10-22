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

        private GuiSlider sld_set_width;
        private GuiSlider sld_set_height;
        private ValueDisplay val_set_width;
        private ValueDisplay val_set_height;
        private LedButton btn_set_Style;
        private ValueDisplay val_element_Val;
        private ValueDisplay val_element_Min;
        private ValueDisplay val_element_Max;
        private LedButton btn_set_Orientation;
        private LedButton btn_set_Led;

        private string               Staged;
        public  Dictionary<string,IInterValuable> Testling;
        private Action               destruct;
        public IRectangle            location;
        private int                  setHeight;
        private int                  setWidth;
        private object               setValue;
        private Rectangle            area;

        private LedButton            btn_Invert;
        private LedButton            btn_Cycled;


        private LedButton AddButton( Point32 position, string name, string text )
        {
            LedButton button = new LedButton();
            button.AutoText = false;
            button.BackColor = Color.FromArgb(255,32,32,32);
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.CausesValidation = false;
            button.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            button.ImeMode = ImeMode.NoControl;
            button.LedLevel = 1F;
            button.LedValue = Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            button.Location = new Point(position.X,position.Y);
            button.Margin = new Padding(2);
            button.Mode = LedButton.Transit.OnRelease;
            button.Name = name;
            button.NumberOfStates = ( (byte)( 2 ) );
            button.SideChain = 0.95F;
            button.Size = new Size(96,96);
            button.State = LedButton.Default.OFF;
            button.Style = Style.Dark;

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
                //slider.ValueChanged += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as GuiSlider ).Name} value changed: {e.Value}"); };
                //slider.ReleasedKnob += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as GuiSlider ).Name} fader release: {e.Value}"); };
                //slider.MarkerPassed += ( object sender, GuiSlider.MarkerPassedEventArgs e ) => { log_data.Log($"{( sender as GuiSlider ).Name} marker {e.Named} at {e.Value} passed at speed: {e.Speed}"); };
                val_element_Val.Wrap( slider );
            } else if (staged is LedButton) {
                LedButton button = staged as LedButton;
                SetLed += (Enum set) => { button.DefineState( button.Index, button.State, (LED)set ); };
                SetStylo += (Enum set) => { button.Style = (Style)set; };
              //  button.Changed += ( object sender, ValueChangeArgs<Enum> e ) => { log_data.Log($"{( sender as LedButton ).Name} changed value: {e.Value}"); };
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
            //    meter.LevelChanged += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as GuiMeter ).Name} changed value: {e.Value}"); };
            //    meter.LevelClipped += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as GuiMeter ).Name} level clipped: {e.Value}"); };
            //    meter.LevelRegular += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as GuiMeter ).Name} level regular: {e.Value}"); };
                val_element_Val.Wrap( meter );
            } else if( staged is Stepflow.Midi.JogDial ) {
                Stepflow.Midi.JogDial dial = staged as Stepflow.Midi.JogDial;
                SetStylo += (Enum set) => { dial.Style = (Style)set; };
                SetLed += (Enum set) => { dial.LedColor = (LED)set; };
                //dial.TurningStopt += ( object sender, ValueChangeArgs<JogDial.Direction> e ) => { log_data.Log($"{( sender as JogDial ).Name} stopped turning: {e.Value}"); };
                //dial.ValueChanged += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as JogDial ).Name} changed value: {e.Value}"); };
                //dial.WheelTouched += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as JogDial ).Name} wheel touched: {e.Value}"); };
                //dial.WheelReverse += ( object sender, ValueChangeArgs<JogDial.Direction> e ) => { log_data.Log($"{( sender as JogDial ).Name} changed direction: {e.Value}"); };
                //dial.WheelRelease += ( object sender, ValueChangeArgs<float> e ) => { log_data.Log($"{( sender as JogDial ).Name} wheel released: {e.Value}"); };
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

        protected void ConstructMidiControls()
        {
            this.sld_set_width = new Stepflow.Gui.GuiSlider();
            this.sld_set_height = new Stepflow.Gui.GuiSlider();
            this.val_set_width = new Stepflow.Gui.ValueDisplay();
            this.val_set_height = new Stepflow.Gui.ValueDisplay();
            this.btn_set_Style = new Stepflow.Gui.LedButton();
            this.val_element_Val = new Stepflow.Gui.ValueDisplay();
            this.val_element_Min = new Stepflow.Gui.ValueDisplay();
            this.val_element_Max = new Stepflow.Gui.ValueDisplay();
            this.btn_set_Orientation = new Stepflow.Gui.LedButton();
            this.btn_set_Led = new Stepflow.Gui.LedButton();
        }

        protected void InitializeMidiControls()
        {
            // 
            // val_element_Max
            // 
            this.val_element_Max.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ));
            this.val_element_Max.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.val_element_Max.CanChangeUnits = true;
            this.val_element_Max.CanScaleUnits = true;
            this.val_element_Max.ForeColor = System.Drawing.Color.Lime;
            this.val_element_Max.HasMidiMenu = true;
            this.val_element_Max.Location = new System.Drawing.Point(947, 232);
            this.val_element_Max.Margin = new System.Windows.Forms.Padding(0);
            this.val_element_Max.Maximum = 1F;
            this.val_element_Max.Minimum = -1F;
            this.val_element_Max.Movement = 0F;
            this.val_element_Max.Name = "val_element_Max";
            this.val_element_Max.Proportion = 0.5F;
            this.val_element_Max.Scale = Stepflow.Gui.UnitScale.Base;
            this.val_element_Max.Size = new System.Drawing.Size(223, 37);
            this.val_element_Max.Style = Stepflow.Gui.Style.Dark;
            this.val_element_Max.TabIndex = 12;
            this.val_element_Max.Units = Stepflow.Gui.UnitsType.Per;
            this.val_element_Max.Value = 0F;
            // 
            // btn_set_Orientation
            // 
            this.btn_set_Orientation.AutoText = false;
            this.btn_set_Orientation.BackColor = System.Drawing.Color.FromArgb(255, 32, 32, 32);
            this.btn_set_Orientation.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_set_Orientation.CausesValidation = false;
            this.btn_set_Orientation.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_set_Orientation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_set_Orientation.LedLevel = 1F;
            this.btn_set_Orientation.LedValue = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            this.btn_set_Orientation.Location = new System.Drawing.Point(948, 794);
            this.btn_set_Orientation.Margin = new System.Windows.Forms.Padding(2);
            this.btn_set_Orientation.Mode = Stepflow.Gui.LedButton.Transit.OnRelease;
            this.btn_set_Orientation.Name = "btn_set_Orientation";
            this.btn_set_Orientation.NumberOfStates = ( (byte)( 2 ) );
            this.btn_set_Orientation.SideChain = 0.95F;
            this.btn_set_Orientation.Size = new System.Drawing.Size(96, 96);
            this.btn_set_Orientation.State = Stepflow.Gui.LedButton.Default.OFF;
            this.btn_set_Orientation.Style = Stepflow.Gui.Style.Dark;
            this.btn_set_Orientation.TabIndex = 14;
            // 
            // btn_set_Led
            // 
            this.btn_set_Led.AutoText = false;
            this.btn_set_Led.BackColor = System.Drawing.Color.FromArgb(255, 32, 32, 32);
            this.btn_set_Led.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_set_Led.CausesValidation = false;
            this.btn_set_Led.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_set_Led.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_set_Led.LedLevel = 1F;
            this.btn_set_Led.LedValue = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            this.btn_set_Led.Location = new System.Drawing.Point(1059, 794);
            this.btn_set_Led.Margin = new System.Windows.Forms.Padding(2);
            this.btn_set_Led.Mode = Stepflow.Gui.LedButton.Transit.OnRelease;
            this.btn_set_Led.Name = "btn_set_Led";
            this.btn_set_Led.NumberOfStates = ( (byte)( 2 ) );
            this.btn_set_Led.SideChain = 0.95F;
            this.btn_set_Led.Size = new System.Drawing.Size(96, 96);
            this.btn_set_Led.State = Stepflow.Gui.LedButton.Default.OFF;
            this.btn_set_Led.Style = Stepflow.Gui.Style.Dark;
            this.btn_set_Led.TabIndex = 15;
            // 
            // btn_set_style
            // 
            this.btn_set_Style.AutoText = false;
            this.btn_set_Style.BackColor = System.Drawing.Color.FromArgb(255, 32, 32, 32);
            this.btn_set_Style.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_set_Style.CausesValidation = false;
            this.btn_set_Style.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_set_Style.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_set_Style.LedLevel = 1F;
            this.btn_set_Style.LedValue = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            this.btn_set_Style.Location = new System.Drawing.Point(11, 39);
            this.btn_set_Style.Margin = new System.Windows.Forms.Padding(2);
            this.btn_set_Style.Mode = Stepflow.Gui.LedButton.Transit.OnRelease;
            this.btn_set_Style.Name = "btn_set_style";
            this.btn_set_Style.NumberOfStates = ( (byte)( 2 ) );
            this.btn_set_Style.SideChain = 0.95F;
            this.btn_set_Style.Size = new System.Drawing.Size(76, 73);
            this.btn_set_Style.State = Stepflow.Gui.LedButton.Default.OFF;
            this.btn_set_Style.Style = Stepflow.Gui.Style.Dark;
            this.btn_set_Style.TabIndex = 7;
            // 
            // val_element_Val
            // 
            this.val_element_Val.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ));
            this.val_element_Val.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.val_element_Val.CanChangeUnits = true;
            this.val_element_Val.CanScaleUnits = true;
            this.val_element_Val.ForeColor = System.Drawing.Color.Lime;
            this.val_element_Val.HasMidiMenu = true;
            this.val_element_Val.Location = new System.Drawing.Point(947, 301);
            this.val_element_Val.Margin = new System.Windows.Forms.Padding(0);
            this.val_element_Val.Maximum = 1F;
            this.val_element_Val.Minimum = -1F;
            this.val_element_Val.Movement = 0F;
            this.val_element_Val.Name = "val_element_Val";
            this.val_element_Val.Proportion = 0.5F;
            this.val_element_Val.Scale = Stepflow.Gui.UnitScale.Base;
            this.val_element_Val.Size = new System.Drawing.Size(223, 37);
            this.val_element_Val.Style = Stepflow.Gui.Style.Dark;
            this.val_element_Val.TabIndex = 8;
            this.val_element_Val.Units = Stepflow.Gui.UnitsType.Per;
            this.val_element_Val.Value = 0F;
            // 
            // val_element_Min
            // 
            this.val_element_Min.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ));
            this.val_element_Min.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.val_element_Min.CanChangeUnits = true;
            this.val_element_Min.CanScaleUnits = true;
            this.val_element_Min.ForeColor = System.Drawing.Color.Lime;
            this.val_element_Min.HasMidiMenu = true;
            this.val_element_Min.Location = new System.Drawing.Point(947, 372);
            this.val_element_Min.Margin = new System.Windows.Forms.Padding(0);
            this.val_element_Min.Maximum = 1F;
            this.val_element_Min.Minimum = -1F;
            this.val_element_Min.Movement = 0F;
            this.val_element_Min.Name = "val_element_Min";
            this.val_element_Min.Proportion = 0.5F;
            this.val_element_Min.Scale = Stepflow.Gui.UnitScale.Base;
            this.val_element_Min.Size = new System.Drawing.Size(223, 37);
            this.val_element_Min.Style = Stepflow.Gui.Style.Dark;
            this.val_element_Min.TabIndex = 10;
            this.val_element_Min.Units = Stepflow.Gui.UnitsType.Per;
            this.val_element_Min.Value = 0F;
            // 
            // sld_set_width
            // 
            this.sld_set_width.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.sld_set_width.Behavior = Stepflow.Gui.MixAndFeel.Acurate;
            this.sld_set_width.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sld_set_width.Clamped = true;
            this.sld_set_width.Cursor = System.Windows.Forms.Cursors.Default;
            this.sld_set_width.Cycled = false;
            this.sld_set_width.ForeColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            this.sld_set_width.Interaction = Stepflow.Gui.GuiSlider.InteractionMode.Linear;
            this.sld_set_width.Inverted = false;
            this.sld_set_width.LedColor = Stepflow.Gui.LED.Mint;
            this.sld_set_width.LedSource = Stepflow.Gui.GuiSlider.LEDSource.OwnValue;
            this.sld_set_width.Location = new System.Drawing.Point(100, 39);
            this.sld_set_width.Margin = new System.Windows.Forms.Padding(0);
            this.sld_set_width.Maximum = 1F;
            this.sld_set_width.Minimum = -1F;
            this.sld_set_width.MinimumSize = new System.Drawing.Size(4, 3);
            this.sld_set_width.Name = "sld_set_width";
            this.sld_set_width.Orientation = Stepflow.Gui.Orientation.Horizontal;
            this.sld_set_width.Proportion = 0.5F;
            this.sld_set_width.RouteTouchesToMouseEvents = false;
            this.sld_set_width.SideChain = 0F;
            this.sld_set_width.Size = new System.Drawing.Size(803, 73);
            this.sld_set_width.Style = Stepflow.Gui.Style.Dark;
            this.sld_set_width.TabIndex = 1;
            this.sld_set_width.ThresholdForFastMovement = 0F;
            this.sld_set_width.Unsigned = false;
            this.sld_set_width.Value = 0F;
            // 
            // sld_set_height
            // 
            this.sld_set_height.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.sld_set_height.Behavior = Stepflow.Gui.MixAndFeel.Acurate;
            this.sld_set_height.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sld_set_height.Clamped = true;
            this.sld_set_height.Cursor = System.Windows.Forms.Cursors.Default;
            this.sld_set_height.Cycled = false;
            this.sld_set_height.ForeColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            this.sld_set_height.Interaction = Stepflow.Gui.GuiSlider.InteractionMode.Linear;
            this.sld_set_height.Inverted = true;
            this.sld_set_height.LedColor = Stepflow.Gui.LED.Mint;
            this.sld_set_height.LedSource = Stepflow.Gui.GuiSlider.LEDSource.OwnValue;
            this.sld_set_height.Location = new System.Drawing.Point(11, 123);
            this.sld_set_height.Margin = new System.Windows.Forms.Padding(0);
            this.sld_set_height.Maximum = 1000F;
            this.sld_set_height.Minimum = 16F;
            this.sld_set_height.MinimumSize = new System.Drawing.Size(4, 3);
            this.sld_set_height.Name = "sld_set_height";
            this.sld_set_height.Orientation = Stepflow.Gui.Orientation.Vertical;
            this.sld_set_height.Proportion = 0.5F;
            this.sld_set_height.RouteTouchesToMouseEvents = false;
            this.sld_set_height.SideChain = 0F;
            this.sld_set_height.Size = new System.Drawing.Size(76, 779);
            this.sld_set_height.Style = Stepflow.Gui.Style.Dark;
            this.sld_set_height.TabIndex = 2;
            this.sld_set_height.ThresholdForFastMovement = 0F;
            this.sld_set_height.Unsigned = false;
            this.sld_set_height.Value = 508F;
            // 
            // val_set_width
            // 
            this.val_set_width.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ));
            this.val_set_width.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.val_set_width.CanChangeUnits = true;
            this.val_set_width.CanScaleUnits = true;
            this.val_set_width.ForeColor = System.Drawing.Color.Lime;
            this.val_set_width.HasMidiMenu = true;
            this.val_set_width.Location = new System.Drawing.Point(947, 75);
            this.val_set_width.Margin = new System.Windows.Forms.Padding(0);
            this.val_set_width.Maximum = 1F;
            this.val_set_width.Minimum = -1F;
            this.val_set_width.Movement = 0F;
            this.val_set_width.Name = "val_set_width";
            this.val_set_width.Proportion = 0.5F;
            this.val_set_width.Scale = Stepflow.Gui.UnitScale.Base;
            this.val_set_width.Size = new System.Drawing.Size(223, 37);
            this.val_set_width.Style = Stepflow.Gui.Style.Dark;
            this.val_set_width.TabIndex = 3;
            this.val_set_width.Units = Stepflow.Gui.UnitsType.Per;
            this.val_set_width.Value = 0F;
            // 
            // val_set_height
            // 
            this.val_set_height.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ));
            this.val_set_height.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.val_set_height.CanChangeUnits = true;
            this.val_set_height.CanScaleUnits = true;
            this.val_set_height.ForeColor = System.Drawing.Color.Lime;
            this.val_set_height.HasMidiMenu = true;
            this.val_set_height.Location = new System.Drawing.Point(947, 138);
            this.val_set_height.Margin = new System.Windows.Forms.Padding(0);
            this.val_set_height.Maximum = 1F;
            this.val_set_height.Minimum = -1F;
            this.val_set_height.Movement = 0F;
            this.val_set_height.Name = "val_set_height";
            this.val_set_height.Proportion = 0.5F;
            this.val_set_height.Scale = Stepflow.Gui.UnitScale.Base;
            this.val_set_height.Size = new System.Drawing.Size(223, 37);
            this.val_set_height.Style = Stepflow.Gui.Style.Dark;
            this.val_set_height.TabIndex = 4;
            this.val_set_height.Units = Stepflow.Gui.UnitsType.Per;
            this.val_set_height.Value = 0F;
        }

        protected void AddMidiControlls()
        {
            this.Controls.Add(this.btn_set_Led);
            this.Controls.Add(this.btn_set_Orientation);
            this.Controls.Add(this.val_element_Max);
            this.Controls.Add(this.val_element_Min);
            this.Controls.Add(this.val_element_Val);
            this.Controls.Add(this.btn_set_Style);
            this.Controls.Add(this.val_set_height);
            this.Controls.Add(this.val_set_width);
            this.Controls.Add(this.sld_set_height);
            this.Controls.Add(this.sld_set_width);
        }

        public Form1()
        {
            
            setHeight = -1;
            setWidth = -1;
            setValue = null;

            Staged = "";
            Testling = new Dictionary<string,IInterValuable>();

            ConstructMidiControls();
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

            btn_set_Style.SetUp(LED.Blue, LED.Pink, LED.Mint);
            btn_set_Style.DefineState(1, Style.Flat, LED.Blue);
            btn_set_Style.DefineState(2, Style.Lite, LED.Pink);
            btn_set_Style.DefineState(3, Style.Dark, LED.Green);

            btn_set_Style.AutoText = true;
            btn_set_Style.Changed += Btn_set_style_Changed;

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
            area = new Rectangle(0, 0, Width, Height);

            Load += Form1_Load;
        }

        private void UpdateScreenLocation()
        {
            area.Size = this.Size;
            location = CornerAndSize.FromRectangle( RectangleToScreen( area ) );
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
                        return FindMenuPosition( path[1], item.DropDownItems, offset );
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
            dings.Orientation = Stepflow.Gui.Orientation.Vertical;
            dings.Style = Style.Lite;
            dings.Damped = false;
            dings.Cycled = false;
            dings.Clamped = true;
            dings.Unsigned = false;
            dings.Tag = 1;
            Connect( dings );
            destruct = () => { dings.Dispose(); };
        }

        private void mnu_JogDial_Click(object sender, EventArgs e)
        {
            destruct?.Invoke();
            Stepflow.Midi.JogDial dings = new Stepflow.Midi.JogDial();
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
