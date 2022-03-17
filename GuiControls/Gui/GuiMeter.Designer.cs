namespace Stepflow.Gui
{
    partial class GuiMeter
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mnu_MeterOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnu_Invert = new System.Windows.Forms.ToolStripMenuItem();
            this.mnu_Unsigned = new System.Windows.Forms.ToolStripMenuItem();
            this.mnu_Input_Range = new System.Windows.Forms.ToolStripTextBox();
            this.mnu_Accept_Input = new System.Windows.Forms.ToolStripMenuItem();
            this.mnu_MeterOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnu_MeterOptions
            // 
            this.mnu_MeterOptions.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mnu_MeterOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_Invert,
            this.mnu_Unsigned});
            this.mnu_MeterOptions.Name = "mnu_MeterOptions";
            this.mnu_MeterOptions.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.mnu_MeterOptions.ShowImageMargin = false;
            this.mnu_MeterOptions.Size = new System.Drawing.Size(112, 48);
            // 
            // mnu_Invert
            // 
            this.mnu_Invert.Name = "mnu_Invert";
            this.mnu_Invert.Size = new System.Drawing.Size(111, 22);
            this.mnu_Invert.Text = "Set Inverted";
            this.mnu_Invert.Click += new System.EventHandler(this.mnu_Invert_Click);
            // 
            // mnu_Unsigned
            // 
            this.mnu_Unsigned.Name = "mnu_Unsigned";
            this.mnu_Unsigned.Size = new System.Drawing.Size(111, 22);
            this.mnu_Unsigned.Text = "Unsigned";
            this.mnu_Unsigned.Click += new System.EventHandler(this.mnu_Unsigned_Click);
            // 
            // mnu_Input_Range
            // 
            this.mnu_Input_Range.AutoToolTip = true;
            this.mnu_Input_Range.MaxLength = 16;
            this.mnu_Input_Range.Name = "mnu_Input_Range";
            this.mnu_Input_Range.Size = new System.Drawing.Size(100, 31);
            this.mnu_Input_Range.ToolTipText = "set range maximum";
            // 
            // mnu_Accept_Input
            // 
            this.mnu_Accept_Input.Name = "mnu_Accept_Input";
            this.mnu_Accept_Input.Size = new System.Drawing.Size(172, 30);
            this.mnu_Accept_Input.Text = "Accept";
            this.mnu_Accept_Input.Click += new System.EventHandler(this.mnu_Accept_Input_Click);
            // 
            // GuiMeter
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ContextMenuStrip = this.mnu_MeterOptions;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.Lime;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(16, 16);
            this.Name = "GuiMeter";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Size = new System.Drawing.Size(30, 256);
            this.mnu_MeterOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

#endregion

        private System.Windows.Forms.ContextMenuStrip mnu_MeterOptions;
        private System.Windows.Forms.ToolStripMenuItem mnu_Invert;
        private System.Windows.Forms.ToolStripTextBox mnu_Input_Range;
        private System.Windows.Forms.ToolStripMenuItem mnu_Accept_Input;
        private System.Windows.Forms.ToolStripMenuItem mnu_Unsigned;
    }
}
