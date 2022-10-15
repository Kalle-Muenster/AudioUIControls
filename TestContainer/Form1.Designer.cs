
namespace MidiGUI.Test.Container
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
            this.midiStringSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.midiComboBoxToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.midiTrackBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbl_set_width = new System.Windows.Forms.Label();
            this.lbl_set_height = new System.Windows.Forms.Label();
            this.lbl_element_Val = new System.Windows.Forms.Label();
            this.lbl_element_Min = new System.Windows.Forms.Label();
            this.lbl_element_Max = new System.Windows.Forms.Label();
            this.lbl_set_Orientation = new System.Windows.Forms.Label();
            this.lbl_set_Led = new System.Windows.Forms.Label();
            this.lst_events_view = new System.Windows.Forms.ListBox();
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
            this.midiStringSetToolStripMenuItem,
            this.midiComboBoxToolStripMenuItem1,
            this.midiTrackBarToolStripMenuItem});
            this.controllsToolStripMenuItem.Name = "controllsToolStripMenuItem";
            this.controllsToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.controllsToolStripMenuItem.Text = "Controlls";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem1.Text = "LedButton";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.mnu_LedButton_Click);
            // 
            // guiSliderToolStripMenuItem
            // 
            this.guiSliderToolStripMenuItem.Name = "guiSliderToolStripMenuItem";
            this.guiSliderToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.guiSliderToolStripMenuItem.Text = "GuiSlider";
            this.guiSliderToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiSlider_Click);
            // 
            // guiMeterToolStripMenuItem
            // 
            this.guiMeterToolStripMenuItem.Name = "guiMeterToolStripMenuItem";
            this.guiMeterToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.guiMeterToolStripMenuItem.Text = "GuiMeter";
            this.guiMeterToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiMeter_Click);
            // 
            // guiRangerToolStripMenuItem
            // 
            this.guiRangerToolStripMenuItem.Name = "guiRangerToolStripMenuItem";
            this.guiRangerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.guiRangerToolStripMenuItem.Text = "GuiRanger";
            this.guiRangerToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiRangeControl_Click);
            // 
            // guiValueToolStripMenuItem
            // 
            this.guiValueToolStripMenuItem.Name = "guiValueToolStripMenuItem";
            this.guiValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.guiValueToolStripMenuItem.Text = "GuiValue";
            this.guiValueToolStripMenuItem.Click += new System.EventHandler(this.mnu_LedDisplay_Click);
            // 
            // midiButtonToolStripMenuItem
            // 
            this.midiButtonToolStripMenuItem.Name = "midiButtonToolStripMenuItem";
            this.midiButtonToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiButtonToolStripMenuItem.Text = "MidiButton";
            this.midiButtonToolStripMenuItem.Click += new System.EventHandler(this.mnu_LedButton_Click);
            // 
            // midiSliderToolStripMenuItem
            // 
            this.midiSliderToolStripMenuItem.Name = "midiSliderToolStripMenuItem";
            this.midiSliderToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiSliderToolStripMenuItem.Text = "MidiSlider";
            this.midiSliderToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiSlider_Click);
            // 
            // midiMeterToolStripMenuItem
            // 
            this.midiMeterToolStripMenuItem.Name = "midiMeterToolStripMenuItem";
            this.midiMeterToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiMeterToolStripMenuItem.Text = "MidiMeter";
            this.midiMeterToolStripMenuItem.Click += new System.EventHandler(this.mnu_GuiMeter_Click);
            // 
            // midiValueToolStripMenuItem
            // 
            this.midiValueToolStripMenuItem.Name = "midiValueToolStripMenuItem";
            this.midiValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiValueToolStripMenuItem.Text = "MidiValue";
            this.midiValueToolStripMenuItem.Click += new System.EventHandler(this.mnu_LedDisplay_Click);
            // 
            // jogDialToolStripMenuItem
            // 
            this.jogDialToolStripMenuItem.Name = "jogDialToolStripMenuItem";
            this.jogDialToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.jogDialToolStripMenuItem.Text = "JogDial";
            this.jogDialToolStripMenuItem.Click += new System.EventHandler(this.mnu_JogDial_Click);
            // 
            // midiStringSetToolStripMenuItem
            // 
            this.midiStringSetToolStripMenuItem.Name = "midiStringSetToolStripMenuItem";
            this.midiStringSetToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiStringSetToolStripMenuItem.Text = "StringControl";
            this.midiStringSetToolStripMenuItem.Click += new System.EventHandler(this.mnu_StringControl_Click);
            // 
            // midiComboBoxToolStripMenuItem1
            // 
            this.midiComboBoxToolStripMenuItem1.Name = "midiComboBoxToolStripMenuItem1";
            this.midiComboBoxToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.midiComboBoxToolStripMenuItem1.Text = "MidiComboBox";
            this.midiComboBoxToolStripMenuItem1.Click += new System.EventHandler(this.mnu_MidiComboBox_Click);
            // 
            // midiTrackBarToolStripMenuItem
            // 
            this.midiTrackBarToolStripMenuItem.Name = "midiTrackBarToolStripMenuItem";
            this.midiTrackBarToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.midiTrackBarToolStripMenuItem.Text = "MidiTrackBar";
            this.midiTrackBarToolStripMenuItem.Click += new System.EventHandler(this.mnu_MidiTrackBar_Click);
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
            // lbl_set_Orientation
            // 
            this.lbl_set_Orientation.AutoSize = true;
            this.lbl_set_Orientation.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_set_Orientation.Location = new System.Drawing.Point(948, 888);
            this.lbl_set_Orientation.Name = "lbl_set_Orientation";
            this.lbl_set_Orientation.Size = new System.Drawing.Size(67, 15);
            this.lbl_set_Orientation.TabIndex = 16;
            this.lbl_set_Orientation.Text = "Orientation";
            // 
            // lbl_set_Led
            // 
            this.lbl_set_Led.AutoSize = true;
            this.lbl_set_Led.ForeColor = System.Drawing.Color.Wheat;
            this.lbl_set_Led.Location = new System.Drawing.Point(1059, 888);
            this.lbl_set_Led.Name = "lbl_set_Led";
            this.lbl_set_Led.Size = new System.Drawing.Size(55, 15);
            this.lbl_set_Led.TabIndex = 17;
            this.lbl_set_Led.Text = "LedColor";
            // 
            // lst_events_view
            // 
            this.lst_events_view.FormattingEnabled = true;
            this.lst_events_view.ItemHeight = 15;
            this.lst_events_view.Location = new System.Drawing.Point(133, 581);
            this.lst_events_view.Name = "lst_events_view";
            this.lst_events_view.Size = new System.Drawing.Size(645, 154);
            this.lst_events_view.TabIndex = 18;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(1198, 935);
            this.Controls.Add(this.lst_events_view);
            this.Controls.Add(this.lbl_set_Led);
            this.Controls.Add(this.lbl_set_Orientation);
            this.Controls.Add(this.lbl_element_Max);
            this.Controls.Add(this.lbl_element_Min);
            this.Controls.Add(this.lbl_element_Val);
            this.Controls.Add(this.lbl_set_height);
            this.Controls.Add(this.lbl_set_width);
            this.Controls.Add(this.menuStrip1);

            this.InitializeMidiControls();
            this.AddMidiControlls();

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
        private System.Windows.Forms.Label lbl_set_width;
        private System.Windows.Forms.Label lbl_set_height;
        private System.Windows.Forms.Label lbl_element_Max;
        private System.Windows.Forms.Label lbl_element_Val;
        private System.Windows.Forms.Label lbl_element_Min;
        private System.Windows.Forms.Label lbl_set_Orientation;
        private System.Windows.Forms.Label lbl_set_Led;
        private System.Windows.Forms.ToolStripMenuItem midiButtonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiSliderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiMeterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jogDialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiStringSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem midiComboBoxToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem midiTrackBarToolStripMenuItem;
        private System.Windows.Forms.ListBox lst_events_view;
    }
}

