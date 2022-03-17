namespace Stepflow.Gui
{
    partial class ValueDisplay
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
            this.txt_value = new System.Windows.Forms.TextBox();
            this.mnu_value = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnu_value_reference = new System.Windows.Forms.ToolStripMenuItem();
            this.mnu_value_refer_Sellector = new System.Windows.Forms.ToolStripComboBox();
            this.txt_units = new System.Windows.Forms.TextBox();
            this.mnu_units = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnu_units_up = new System.Windows.Forms.ToolStripMenuItem();
            this.mnu_units_select = new System.Windows.Forms.ToolStripComboBox();
            this.mnu_units_down = new System.Windows.Forms.ToolStripMenuItem();
            this.mnu_value.SuspendLayout();
            this.mnu_units.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_value
            // 
            this.txt_value.AcceptsReturn = true;
            this.txt_value.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.txt_value.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_value.ContextMenuStrip = this.mnu_value;
            this.txt_value.Font = new System.Drawing.Font("Microsoft Sans Serif", 19F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txt_value.ForeColor = System.Drawing.Color.Lime;
            this.txt_value.Location = new System.Drawing.Point(5, 5);
            this.txt_value.Margin = new System.Windows.Forms.Padding(0);
            this.txt_value.MaxLength = 0;
            this.txt_value.Name = "txt_value";
            this.txt_value.Size = new System.Drawing.Size(187, 22);
            this.txt_value.TabIndex = 0;
            this.txt_value.Text = "0";
            this.txt_value.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_value.WordWrap = false;
            // 
            // mnu_value
            // 
            this.mnu_value.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator1,
            this.mnu_value_reference});
            this.mnu_value.Name = "mnu_value";
            this.mnu_value.Size = new System.Drawing.Size(146, 98);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cutToolStripMenuItem.Tag = "x";
            this.cutToolStripMenuItem.Text = "cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyToolStripMenuItem.Tag = "c";
            this.copyToolStripMenuItem.Text = "copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.pasteToolStripMenuItem.Tag = "v";
            this.pasteToolStripMenuItem.Text = "paste";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // mnu_value_reference
            // 
            this.mnu_value_reference.CheckOnClick = true;
            this.mnu_value_reference.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_value_refer_Sellector});
            this.mnu_value_reference.Name = "mnu_value_reference";
            this.mnu_value_reference.Size = new System.Drawing.Size(152, 22);
            this.mnu_value_reference.Text = "reference";
            this.mnu_value_reference.CheckStateChanged += new System.EventHandler(this.mnu_value_reference_CheckStateChanged);
            // 
            // mnu_value_refer_Sellector
            // 
            this.mnu_value_refer_Sellector.Items.AddRange(new object[] {
            "self"});
            this.mnu_value_refer_Sellector.Name = "mnu_value_refer_Sellector";
            this.mnu_value_refer_Sellector.Size = new System.Drawing.Size(300, 23);
            this.mnu_value_refer_Sellector.Text = "self";
            this.mnu_value_refer_Sellector.ToolTipText = "Select a control elemt which value to reference";
            this.mnu_value_refer_Sellector.SelectedIndexChanged += new System.EventHandler(this.mnu_value_refer_Sellector_SelectedIndexChanged);
            // 
            // txt_units
            // 
            this.txt_units.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_units.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.txt_units.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_units.ContextMenuStrip = this.mnu_units;
            this.txt_units.Font = new System.Drawing.Font("Microsoft Sans Serif", 19F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txt_units.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.txt_units.Location = new System.Drawing.Point(192, 5);
            this.txt_units.Margin = new System.Windows.Forms.Padding(0);
            this.txt_units.Name = "txt_units";
            this.txt_units.ReadOnly = true;
            this.txt_units.Size = new System.Drawing.Size(59, 22);
            this.txt_units.TabIndex = 1;
            this.txt_units.Text = " Hz";
            // 
            // mnu_units
            // 
            this.mnu_units.BackColor = System.Drawing.SystemColors.Control;
            this.mnu_units.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_units_up,
            this.mnu_units_select,
            this.mnu_units_down});
            this.mnu_units.Name = "mnu_units";
            this.mnu_units.Size = new System.Drawing.Size(182, 97);
            // 
            // mnu_units_up
            // 
            this.mnu_units_up.BackColor = System.Drawing.SystemColors.Control;
            this.mnu_units_up.Name = "mnu_units_up";
            this.mnu_units_up.Size = new System.Drawing.Size(181, 22);
            this.mnu_units_up.Tag = "+";
            this.mnu_units_up.Text = "Scale Up";
            this.mnu_units_up.Click += new System.EventHandler(this.ToolStripMenuItem_Click);
            // 
            // mnu_units_select
            // 
            this.mnu_units_select.Items.AddRange(new object[] {
            " _ ",
            "Hz",
            "Db",
            "V",
            "A",
            "sec",
            "Percent"});
            this.mnu_units_select.Name = "mnu_units_select";
            this.mnu_units_select.Size = new System.Drawing.Size(121, 23);
            this.mnu_units_select.Text = "Hz";
            this.mnu_units_select.TextChanged += new System.EventHandler(this.mnu_units_select_TextChanged);
            // 
            // mnu_units_down
            // 
            this.mnu_units_down.Name = "mnu_units_down";
            this.mnu_units_down.Size = new System.Drawing.Size(181, 22);
            this.mnu_units_down.Tag = "-";
            this.mnu_units_down.Text = "Scale Down";
            this.mnu_units_down.Click += new System.EventHandler(this.ToolStripMenuItem_Click);
            // 
            // ValueDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.txt_units);
            this.Controls.Add(this.txt_value);
            this.ForeColor = System.Drawing.Color.Lime;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ValueDisplay";
            this.Size = new System.Drawing.Size(256, 32);
            this.mnu_value.ResumeLayout(false);
            this.mnu_units.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_value;
        private System.Windows.Forms.TextBox txt_units;
        private System.Windows.Forms.ToolStripMenuItem mnu_units_up;
        private System.Windows.Forms.ToolStripMenuItem mnu_units_down;
        private System.Windows.Forms.ToolStripComboBox mnu_units_select;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnu_value_reference;
        private System.Windows.Forms.ToolStripComboBox mnu_value_refer_Sellector;
        protected System.Windows.Forms.ContextMenuStrip mnu_value;
        protected System.Windows.Forms.ContextMenuStrip mnu_units;
    }
}
