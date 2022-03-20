namespace Stepflow {
namespace Midi
{
    partial class StringControl
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
        if (disposing && (components != null))
        {
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
            this.dbglbl = new System.Windows.Forms.Label();
            this.dbglbl2 = new System.Windows.Forms.Label();
            this.steg = new System.Windows.Forms.SplitContainer();
            this.dbglbl4 = new System.Windows.Forms.Label();
            this.dbglbl3 = new System.Windows.Forms.Label();
            this.InstrumentContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)(this.steg)).BeginInit();
            this.steg.Panel1.SuspendLayout();
            this.steg.Panel2.SuspendLayout();
            this.steg.SuspendLayout();
            this.InstrumentContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dbglbl
            // 
            this.dbglbl.AutoSize = true;
            this.dbglbl.Location = new System.Drawing.Point(9, 6);
            this.dbglbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.dbglbl.Name = "dbglbl";
            this.dbglbl.Size = new System.Drawing.Size(35, 13);
            this.dbglbl.TabIndex = 3;
            this.dbglbl.Text = "label1";
            // 
            // dbglbl2
            // 
            this.dbglbl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dbglbl2.AutoSize = true;
            this.dbglbl2.Location = new System.Drawing.Point(9, 236);
            this.dbglbl2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.dbglbl2.Name = "dbglbl2";
            this.dbglbl2.Size = new System.Drawing.Size(35, 13);
            this.dbglbl2.TabIndex = 4;
            this.dbglbl2.Text = "label1";
            // 
            // steg
            // 
            this.steg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.steg.Location = new System.Drawing.Point(0, 0);
            this.steg.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.steg.Name = "steg";
            // 
            // steg.Panel1
            // 
            this.steg.Panel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.steg.Panel1.Controls.Add(this.dbglbl2);
            this.steg.Panel1.Controls.Add(this.dbglbl);
            // 
            // steg.Panel2
            // 
            this.steg.Panel2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.steg.Panel2.Controls.Add(this.dbglbl4);
            this.steg.Panel2.Controls.Add(this.dbglbl3);
            this.steg.Size = new System.Drawing.Size(1067, 260);
            this.steg.SplitterDistance = 266;
            this.steg.SplitterWidth = 3;
            this.steg.TabIndex = 5;
            // 
            // dbglbl4
            // 
            this.dbglbl4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dbglbl4.AutoSize = true;
            this.dbglbl4.Location = new System.Drawing.Point(713, 6);
            this.dbglbl4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.dbglbl4.Name = "dbglbl4";
            this.dbglbl4.Size = new System.Drawing.Size(35, 13);
            this.dbglbl4.TabIndex = 1;
            this.dbglbl4.Text = "label1";
            // 
            // dbglbl3
            // 
            this.dbglbl3.AutoSize = true;
            this.dbglbl3.Location = new System.Drawing.Point(11, 6);
            this.dbglbl3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.dbglbl3.Name = "dbglbl3";
            this.dbglbl3.Size = new System.Drawing.Size(35, 13);
            this.dbglbl3.TabIndex = 0;
            this.dbglbl3.Text = "label1";
            // 
            // InstrumentContextMenu
            // 
            this.InstrumentContextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.InstrumentContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripSeparator1});
            this.InstrumentContextMenu.Name = "InstrumentContextMenu";
            this.InstrumentContextMenu.Size = new System.Drawing.Size(150, 32);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 22);
            this.toolStripMenuItem1.Text = "ChannelMode";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItem3.Text = "Per String Mono-Channels";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItem4.Text = "All Strings in Poly-Channel";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(146, 6);
            // 
            // StringControl
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CausesValidation = false;
            this.ContextMenuStrip = this.InstrumentContextMenu;
            this.Controls.Add(this.steg);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "StringControl";
            this.Size = new System.Drawing.Size(1067, 260);
            this.steg.Panel1.ResumeLayout(false);
            this.steg.Panel1.PerformLayout();
            this.steg.Panel2.ResumeLayout(false);
            this.steg.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.steg)).EndInit();
            this.steg.ResumeLayout(false);
            this.InstrumentContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.Label dbglbl;
    private System.Windows.Forms.Label dbglbl2;
    private System.Windows.Forms.SplitContainer steg;
    private System.Windows.Forms.Label dbglbl3;
    private System.Windows.Forms.Label dbglbl4;
        private System.Windows.Forms.ContextMenuStrip InstrumentContextMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}}
