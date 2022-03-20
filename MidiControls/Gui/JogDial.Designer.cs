namespace Stepflow
{ namespace Midi {
    partial class JogDial
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.Wheel = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.mnu_config = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.behaviorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slopeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.abstractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulateTurnTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.Wheel)).BeginInit();
            this.mnu_config.SuspendLayout();
            this.SuspendLayout();
            // 
            // Wheel
            // 
            this.Wheel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Wheel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Wheel.Location = new System.Drawing.Point(0, 0);
            this.Wheel.Margin = new System.Windows.Forms.Padding(0);
            this.Wheel.Name = "Wheel";
            this.Wheel.Size = new System.Drawing.Size(512, 512);
            this.Wheel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Wheel.TabIndex = 0;
            this.Wheel.TabStop = false;
            this.Wheel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Wheel_MouseDown);
            this.Wheel.MouseLeave += new System.EventHandler(this.Wheel_MouseLeave);
            this.Wheel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Wheel_MouseUp);
            // 
            // timer1
            // 
            this.timer1.Interval = 20;
            // 
            // mnu_config
            // 
            this.mnu_config.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mnu_config.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.behaviorToolStripMenuItem});
            this.mnu_config.Name = "mnu_config";
            this.mnu_config.Size = new System.Drawing.Size(121, 26);
            this.mnu_config.Text = "Cofiguration";
            // 
            // behaviorToolStripMenuItem
            // 
            this.behaviorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slopeToolStripMenuItem,
            this.timingToolStripMenuItem,
            this.modesToolStripMenuItem});
            this.behaviorToolStripMenuItem.Name = "behaviorToolStripMenuItem";
            this.behaviorToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.behaviorToolStripMenuItem.Text = "behavior";
            // 
            // slopeToolStripMenuItem
            // 
            this.slopeToolStripMenuItem.Name = "slopeToolStripMenuItem";
            this.slopeToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.slopeToolStripMenuItem.Text = "sloping";
            // 
            // timingToolStripMenuItem
            // 
            this.timingToolStripMenuItem.Name = "timingToolStripMenuItem";
            this.timingToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.timingToolStripMenuItem.Text = "timing";
            // 
            // modesToolStripMenuItem
            // 
            this.modesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.abstractToolStripMenuItem,
            this.emulateTurnTableToolStripMenuItem});
            this.modesToolStripMenuItem.Name = "modesToolStripMenuItem";
            this.modesToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.modesToolStripMenuItem.Text = "modes";
            // 
            // abstractToolStripMenuItem
            // 
            this.abstractToolStripMenuItem.Name = "abstractToolStripMenuItem";
            this.abstractToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.abstractToolStripMenuItem.Text = "abstract control dial";
            // 
            // emulateTurnTableToolStripMenuItem
            // 
            this.emulateTurnTableToolStripMenuItem.Name = "emulateTurnTableToolStripMenuItem";
            this.emulateTurnTableToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.emulateTurnTableToolStripMenuItem.Text = "emulate turn table";
            // 
            // JogDial
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ContextMenuStrip = this.mnu_config;
            this.Controls.Add(this.Wheel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "JogDial";
            this.Size = new System.Drawing.Size(512, 512);
            ((System.ComponentModel.ISupportInitialize)(this.Wheel)).EndInit();
            this.mnu_config.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.PictureBox Wheel;
        private System.Windows.Forms.ContextMenuStrip mnu_config;
        private System.Windows.Forms.ToolStripMenuItem behaviorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem slopeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem abstractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emulateTurnTableToolStripMenuItem;
    }
} }
