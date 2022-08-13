
namespace TestContainer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.controllsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.guiSliderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guiMeterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guiRangerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guiValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiButtonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiSliderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiMeterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jogDialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.laGuitarraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiStringSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sld_set_width = new Stepflow.Gui.GuiSlider();
            this.sld_set_height = new Stepflow.Gui.GuiSlider();
            this.val_set_width = new Stepflow.Gui.ValueDisplay();
            this.val_set_height = new Stepflow.Gui.ValueDisplay();
            this.btn_set_style = new Stepflow.Gui.LedButton();
            this.val_element_Val = new Stepflow.Gui.ValueDisplay();
            this.val_element_Min = new Stepflow.Gui.ValueDisplay();
            this.val_element_Max = new Stepflow.Gui.ValueDisplay();
            this.btn_set_Orientation = new Stepflow.Gui.LedButton();
            this.btn_set_Led = new Stepflow.Gui.LedButton();
            this.lbl_set_width = new System.Windows.Forms.Label();
            this.lbl_set_height = new System.Windows.Forms.Label();
            this.lbl_element_Val = new System.Windows.Forms.Label();
            this.lbl_element_Min = new System.Windows.Forms.Label();
            this.lbl_element_Max = new System.Windows.Forms.Label();
            this.lbl_set_Orientation = new System.Windows.Forms.Label();
            this.lbl_set_Led = new System.Windows.Forms.Label();
            this.midiComboBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiComboBoxToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.controllsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1198, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // controllsToolStripMenuItem
            // 
            this.controllsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.guiSliderToolStripMenuItem,
            this.guiMeterToolStripMenuItem,
            this.guiRangerToolStripMenuItem,
            this.guiValueToolStripMenuItem,
            this.midiButtonToolStripMenuItem,
            this.midiSliderToolStripMenuItem,
            this.midiMeterToolStripMenuItem,
            this.midiValueToolStripMenuItem,
            this.jogDialToolStripMenuItem,
            this.laGuitarraToolStripMenuItem,
            this.midiComboBoxToolStripMenuItem,
            this.midiStringSetToolStripMenuItem,
            this.midiComboBoxToolStripMenuItem1});
            this.controllsToolStripMenuItem.Name = "controllsToolStripMenuItem";
            this.controllsToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.controllsToolStripMenuItem.Text = "Controlls";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(158, 22);
            this.toolStripMenuItem1.Text = "GuiButton";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.mnu_LedButton_Click);
            // 
            // guiSliderToolStripMenuItem
            // 
            this.guiSliderToolStripMenuItem.Name = "guiSliderToolStripMenuItem";
            this.guiSliderToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.guiSliderToolStripMenuItem.Text = "GuiSlider";
            this.guiSliderToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiSlider_Click);
            // 
            // guiMeterToolStripMenuItem
            // 
            this.guiMeterToolStripMenuItem.Name = "guiMeterToolStripMenuItem";
            this.guiMeterToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.guiMeterToolStripMenuItem.Text = "GuiMeter";
            this.guiMeterToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiMeter_Click);
            // 
            // guiRangerToolStripMenuItem
            // 
            this.guiRangerToolStripMenuItem.Name = "guiRangerToolStripMenuItem";
            this.guiRangerToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.guiRangerToolStripMenuItem.Text = "GuiRanger";
            this.guiRangerToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiRangeControl_Click);
            // 
            // guiValueToolStripMenuItem
            // 
            this.guiValueToolStripMenuItem.Name = "guiValueToolStripMenuItem";
            this.guiValueToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.guiValueToolStripMenuItem.Text = "GuiValue";
            this.guiValueToolStripMenuItem.Click += new System.EventHandler(this.mnu_LedDisplay_Click);
            // 
            // midiButtonToolStripMenuItem
            // 
            this.midiButtonToolStripMenuItem.Name = "midiButtonToolStripMenuItem";
            this.midiButtonToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.midiButtonToolStripMenuItem.Text = "MidiButton";
            this.midiButtonToolStripMenuItem.Click += new System.EventHandler(this.mnu_LedButton_Click);
            // 
            // midiSliderToolStripMenuItem
            // 
            this.midiSliderToolStripMenuItem.Name = "midiSliderToolStripMenuItem";
            this.midiSliderToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.midiSliderToolStripMenuItem.Text = "MidiSlider";
            this.midiSliderToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiSlider_Click);
            // 
            // midiMeterToolStripMenuItem
            // 
            this.midiMeterToolStripMenuItem.Name = "midiMeterToolStripMenuItem";
            this.midiMeterToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.midiMeterToolStripMenuItem.Text = "MidiMeter";
            this.midiMeterToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiMeter_Click);
            // 
            // midiValueToolStripMenuItem
            // 
            this.midiValueToolStripMenuItem.Name = "midiValueToolStripMenuItem";
            this.midiValueToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.midiValueToolStripMenuItem.Text = "MidiValue";
            this.midiValueToolStripMenuItem.Click += new System.EventHandler(this.mnu_LedDisplay_Click);
            // 
            // jogDialToolStripMenuItem
            // 
            this.jogDialToolStripMenuItem.Name = "jogDialToolStripMenuItem";
            this.jogDialToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.jogDialToolStripMenuItem.Text = "JogDial";
            this.jogDialToolStripMenuItem.Click += new System.EventHandler(this.mnu_JogDial_Click);
            // 
            // laGuitarraToolStripMenuItem
            // 
            this.laGuitarraToolStripMenuItem.Name = "laGuitarraToolStripMenuItem";
            this.laGuitarraToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.laGuitarraToolStripMenuItem.Text = "MidiString";
            // 
            // midiStringSetToolStripMenuItem
            // 
            this.midiStringSetToolStripMenuItem.Name = "midiStringSetToolStripMenuItem";
            this.midiStringSetToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.midiStringSetToolStripMenuItem.Text = "StringControl";
            this.midiStringSetToolStripMenuItem.Click += new System.EventHandler(this.mnu_StringControl_Click);
            // 
            // sld_set_width
            // 
            this.sld_set_width.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
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
            this.sld_set_height.BackColor = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
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
            // 
            // lbl_set_width
            // 
            this.lbl_set_width.AutoSize = true;
            this.lbl_set_width.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_set_width.Location = new System.Drawing.Point(947, 60);
            this.lbl_set_width.Name = "lbl_set_width";
            this.lbl_set_width.Size = new System.Drawing.Size(39, 15);
            this.lbl_set_width.TabIndex = 5;
            this.lbl_set_width.Text = "Width";
            // 
            // lbl_set_height
            // 
            this.lbl_set_height.AutoSize = true;
            this.lbl_set_height.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_set_height.Location = new System.Drawing.Point(948, 123);
            this.lbl_set_height.Name = "lbl_set_height";
            this.lbl_set_height.Size = new System.Drawing.Size(43, 15);
            this.lbl_set_height.TabIndex = 6;
            this.lbl_set_height.Text = "Height";
            // 
            // btn_set_style
            // 
            this.btn_set_style.AutoText = false;
            this.btn_set_style.BackColor = System.Drawing.SystemColors.Control;
            this.btn_set_style.BackgroundImage = ( (System.Drawing.Image)( GuiControls.Properties.Resources.kein_bild ) );
            this.btn_set_style.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_set_style.CausesValidation = false;
            this.btn_set_style.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_set_style.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_set_style.LedLevel = 1F;
            this.btn_set_style.LedValue = System.Drawing.Color.FromArgb(( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 255 ) ) ) ));
            this.btn_set_style.Location = new System.Drawing.Point(11, 39);
            this.btn_set_style.Margin = new System.Windows.Forms.Padding(2);
            this.btn_set_style.Mode = Stepflow.Gui.LedButton.Transit.OnRelease;
            this.btn_set_style.Name = "btn_set_style";
            this.btn_set_style.NumberOfStates = ( (byte)( 2 ) );
            this.btn_set_style.SideChain = 0.95F;
            this.btn_set_style.Size = new System.Drawing.Size(76, 73);
            this.btn_set_style.State = Stepflow.Gui.LedButton.Default.OFF;
            this.btn_set_style.Style = Stepflow.Gui.Style.Dark;
            this.btn_set_style.TabIndex = 7;
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
            // lbl_element_Val
            // 
            this.lbl_element_Val.AutoSize = true;
            this.lbl_element_Val.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_element_Val.Location = new System.Drawing.Point(948, 286);
            this.lbl_element_Val.Name = "lbl_element_Val";
            this.lbl_element_Val.Size = new System.Drawing.Size(35, 15);
            this.lbl_element_Val.TabIndex = 9;
            this.lbl_element_Val.Text = "Value";
            // 
            // lbl_element_Min
            // 
            this.lbl_element_Min.AutoSize = true;
            this.lbl_element_Min.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_element_Min.Location = new System.Drawing.Point(948, 357);
            this.lbl_element_Min.Name = "lbl_element_Min";
            this.lbl_element_Min.Size = new System.Drawing.Size(60, 15);
            this.lbl_element_Min.TabIndex = 11;
            this.lbl_element_Min.Text = "Minimum";
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
            // lbl_element_Max
            // 
            this.lbl_element_Max.AutoSize = true;
            this.lbl_element_Max.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_element_Max.Location = new System.Drawing.Point(948, 217);
            this.lbl_element_Max.Name = "lbl_element_Max";
            this.lbl_element_Max.Size = new System.Drawing.Size(62, 15);
            this.lbl_element_Max.TabIndex = 13;
            this.lbl_element_Max.Text = "Maximum";
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
            this.btn_set_Orientation.BackColor = System.Drawing.SystemColors.Control;
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
            this.btn_set_Led.BackColor = System.Drawing.SystemColors.Control;
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
            // lbl_set_Orientation
            // 
            this.lbl_set_Orientation.AutoSize = true;
            this.lbl_set_Orientation.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_set_Orientation.Location = new System.Drawing.Point(948, 777);
            this.lbl_set_Orientation.Name = "lbl_set_Orientation";
            this.lbl_set_Orientation.Size = new System.Drawing.Size(67, 15);
            this.lbl_set_Orientation.TabIndex = 16;
            this.lbl_set_Orientation.Text = "Orientation";
            // 
            // lbl_set_Led
            // 
            this.lbl_set_Led.AutoSize = true;
            this.lbl_set_Led.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_set_Led.Location = new System.Drawing.Point(1059, 777);
            this.lbl_set_Led.Name = "lbl_set_Led";
            this.lbl_set_Led.Size = new System.Drawing.Size(55, 15);
            this.lbl_set_Led.TabIndex = 17;
            this.lbl_set_Led.Text = "LedColor";
            // 
            // midiComboBoxToolStripMenuItem
            // 
            this.midiComboBoxToolStripMenuItem.Name = "midiComboBoxToolStripMenuItem";
            this.midiComboBoxToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiComboBoxToolStripMenuItem.Text = "MidiComboBox";
            // 
            // midiComboBoxToolStripMenuItem1
            // 
            this.midiComboBoxToolStripMenuItem1.Name = "midiComboBoxToolStripMenuItem1";
            this.midiComboBoxToolStripMenuItem1.Size = new System.Drawing.Size(158, 22);
            this.midiComboBoxToolStripMenuItem1.Text = "MidiComboBox";
            this.midiComboBoxToolStripMenuItem1.Click += new System.EventHandler(this.mnu_MidiSelectBox_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(1198, 935);
            this.Controls.Add(this.lbl_set_Led);
            this.Controls.Add(this.lbl_set_Orientation);
            this.Controls.Add(this.btn_set_Led);
            this.Controls.Add(this.btn_set_Orientation);
            this.Controls.Add(this.lbl_element_Max);
            this.Controls.Add(this.val_element_Max);
            this.Controls.Add(this.lbl_element_Min);
            this.Controls.Add(this.val_element_Min);
            this.Controls.Add(this.lbl_element_Val);
            this.Controls.Add(this.val_element_Val);
            this.Controls.Add(this.btn_set_style);
            this.Controls.Add(this.lbl_set_height);
            this.Controls.Add(this.lbl_set_width);
            this.Controls.Add(this.val_set_height);
            this.Controls.Add(this.val_set_width);
            this.Controls.Add(this.sld_set_height);
            this.Controls.Add(this.sld_set_width);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem controllsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem guiSliderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem guiMeterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem guiRangerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem guiValueToolStripMenuItem;
        private Stepflow.Gui.GuiSlider sld_set_width;
        private Stepflow.Gui.GuiSlider sld_set_height;
        private Stepflow.Gui.ValueDisplay val_set_width;
        private Stepflow.Gui.ValueDisplay val_set_height;
        private System.Windows.Forms.Label lbl_set_width;
        private System.Windows.Forms.Label lbl_set_height;
        private Stepflow.Gui.LedButton btn_set_style;
        private System.Windows.Forms.Label lbl_element_Val;
        private Stepflow.Gui.ValueDisplay val_element_Val;
        private System.Windows.Forms.Label lbl_element_Min;
        private Stepflow.Gui.ValueDisplay val_element_Min;
        private System.Windows.Forms.Label lbl_element_Max;
        private Stepflow.Gui.ValueDisplay val_element_Max;
        private Stepflow.Gui.LedButton btn_set_Orientation;
        private Stepflow.Gui.LedButton btn_set_Led;
        private System.Windows.Forms.Label lbl_set_Orientation;
        private System.Windows.Forms.Label lbl_set_Led;
        private System.Windows.Forms.ToolStripMenuItem midiButtonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiSliderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiMeterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jogDialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem laGuitarraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiStringSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiComboBoxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiComboBoxToolStripMenuItem1;
    }
}

