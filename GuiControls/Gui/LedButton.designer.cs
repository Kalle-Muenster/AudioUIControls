//using Stepflow.Midi.ControlHelpers;
namespace Stepflow.Gui
{
    partial class LedButton
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
            this.Label = new System.Windows.Forms.Label();
            this.mnu_config = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // Label
            // 
            this.Label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label.BackColor = System.Drawing.Color.Transparent;
            this.Label.CausesValidation = false;
            this.Label.ContextMenuStrip = this.mnu_config;
            this.Label.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.Label.Location = new System.Drawing.Point(0, 23);
            this.Label.Margin = new System.Windows.Forms.Padding(0);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(96, 57);
            this.Label.TabIndex = 0;
            this.Label.Text = "GUI";
            this.Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Label_MouseDown);
            this.Label.MouseEnter += new System.EventHandler(this.Label_MouseEnter);
            this.Label.MouseLeave += new System.EventHandler(this.Label_MouseLeave);
            this.Label.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Label_MouseUp);
            // 
            // mnu_config
            // 
            this.mnu_config.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mnu_config.Name = "mnu_config";
            this.mnu_config.Size = new System.Drawing.Size(61, 4);
            // 
            // LedButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 33F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::Stepflow.Properties.Resources.LedButton_Lite;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CausesValidation = false;
            this.ContextMenuStrip = this.mnu_config;
            this.Controls.Add(this.Label);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LedButton";
            this.Size = new System.Drawing.Size(96, 96);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip mnu_config;
        public System.Windows.Forms.Label Label;
    }
}
