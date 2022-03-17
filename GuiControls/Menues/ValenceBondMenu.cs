using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Automation;


namespace Stepflow
{ namespace Gui {

    public class ValenceBondMenu<cT> where cT : ControllerBase
    {
        private IInterValuable<cT> parent;

        private ContextMenuStrip sub_Joined;
        private ContextMenuStrip sub_Valence;
                
        private ToolStripComboBox cbx_valence_FieldSelector;
        private ToolStripMenuItem mnu_valence_VAL;
        private ToolStripMenuItem mnu_valence_MIN;
        private ToolStripMenuItem mnu_valence_MAX;
        private ToolStripMenuItem mnu_valence_MOV;
                
        private ToolStripComboBox cbx_joined_FieldSelector;
        private ToolStripMenuItem mnu_joined_VAL;
        private ToolStripMenuItem mnu_joined_MIN;
        private ToolStripMenuItem mnu_joined_MAX;
        private ToolStripMenuItem mnu_joined_MOV;

        private ToolStripMenuItem ContextMenuHook;

        private List<Valence.FieldDescriptor> compatibles;
        private Enum                          innerField;
        private ControllerVariable?           innerValue;
        private Valence.FieldDescriptor?      outerField;


        public ValenceBondMenu(  IInterValuable<cT> parentControl, System.ComponentModel.IContainer connector )
        {
            innerField = null;
            outerField = null;
            innerValue = null;

            parent = parentControl;
            ContextMenuHook = new ToolStripMenuItem();

            sub_Joined = new ContextMenuStrip( connector );
            cbx_joined_FieldSelector = new ToolStripComboBox();
            mnu_joined_VAL = new ToolStripMenuItem();
            mnu_joined_MIN = new ToolStripMenuItem();
            mnu_joined_MAX = new ToolStripMenuItem();
            mnu_joined_MOV = new ToolStripMenuItem();

            sub_Valence = new ContextMenuStrip( connector );
            cbx_valence_FieldSelector = new ToolStripComboBox();
            mnu_valence_VAL = new ToolStripMenuItem();
            mnu_valence_MIN = new ToolStripMenuItem();
            mnu_valence_MAX = new ToolStripMenuItem();
            mnu_valence_MOV = new ToolStripMenuItem();

            InitializeComponent();

            cbx_valence_FieldSelector.Enabled = false;
            cbx_valence_FieldSelector.Items.Clear();
            cbx_joined_FieldSelector.Enabled = false;
            cbx_joined_FieldSelector.Items.Clear();
        }

        private void ValenceMenu_CheckedChanged( object sender, EventArgs e )
        {
            if( (sender as ToolStripMenuItem).Checked ) {
                int selfindex = -1;
                compatibles = Valence.CompatibleFields<cT>();
                for(int i=0; i<compatibles.Count; ++i ) {
                    string entry; Valence.FieldDescriptor field = compatibles[i];
                    if( field.Element == parent && selfindex < 0 ) { selfindex = i;
                        entry = string.Format(
                            "Self->{0}.{1}", field.Index.ToString(),
                                typeof(cT).Name );
                    } else {
                        entry = field.ToString();
                    } cbx_joined_FieldSelector.Items.Add( entry );
                } cbx_joined_FieldSelector.SelectedIndex = selfindex;
                Valence.FieldIndices indices = parent.valence().Field.Indices;
                for( int i = 0; i < indices.Count; ++i ) {
                    string entry = string.Format( "{0}.{1}", indices[i].ToString(),
                        compatibles[i+selfindex].VarType.Name );
                    cbx_valence_FieldSelector.Items.Add( entry );
                }
                cbx_valence_FieldSelector.Enabled = true;
                cbx_joined_FieldSelector.Enabled = true;
            } else {
                cbx_joined_FieldSelector.Enabled = false;
                cbx_joined_FieldSelector.Items.Clear();
                compatibles.Clear();
            }

        }

        private void Joined_FieldSelector_IndexChanged(object sender, EventArgs e)
        {
            outerField = compatibles[(sender as ToolStripComboBox).SelectedIndex];
        }

        private void Valence_FieldSelector_IndexChanged(object sender, EventArgs e)
        {
            innerField = parent.valence().Field.Indices[
                cbx_valence_FieldSelector.SelectedIndex ];
            foreach( var item in sub_Valence.Items ) {
                if( item is ToolStripMenuItem ) {
                    ToolStripMenuItem mnuItm = item as ToolStripMenuItem;
                    mnuItm.Enabled = false;
                    mnuItm.Checked = parent.valence( innerField ).IsJoint(
                                      (ControllerVariable)mnuItm.Tag );
                    mnuItm.Enabled = true;
                }
            }
        }

        private void ValenceVariable_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem var = sender as ToolStripMenuItem;
            if (!var.Checked) {
                parent.valence( innerField ).Free( (ControllerVariable)var.Tag );
            }
        }

        private void ValenceVariable_ExternalVariableClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem intern = sender as ToolStripMenuItem;
            if((e.ClickedItem as ToolStripMenuItem).Checked) {
                if( outerField != null ) {
                    parent.valence(innerField).Join(
                        (ControllerVariable)intern.Tag,
                        compatibles[cbx_joined_FieldSelector.SelectedIndex].Element,
                        (ControllerVariable)e.ClickedItem.Tag );
                    intern.Checked = true;
                }
            } else {
                parent.valence(innerField).Free(
                    (ControllerVariable)intern.Tag,
                    compatibles[cbx_joined_FieldSelector.SelectedIndex].Element,
                    (ControllerVariable)e.ClickedItem.Tag );
                if( parent.valence(innerField).IsJoint((ControllerVariable)intern.Tag) )
                    intern.Checked = false;
            } outerField = null;
        }

        private void InitializeComponent()
        {
            sub_Joined.SuspendLayout();
            sub_Valence.SuspendLayout();

            // 
            // sub_Joined
            // 
            sub_Joined.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            cbx_joined_FieldSelector,
            mnu_joined_VAL,
            mnu_joined_MIN,
            mnu_joined_MAX,
            mnu_joined_MOV});
            sub_Joined.Name = "mnu_value";
            sub_Joined.Size = new System.Drawing.Size(182, 119);
            // 
            // cbx_joined_FieldSelector
            // 
            cbx_joined_FieldSelector.Name = "cbx_joined_FieldSelector";
            cbx_joined_FieldSelector.Size = new System.Drawing.Size(121, 23);
            // 
            // mnu_joined_VAL
            // 
            mnu_joined_VAL.CheckOnClick = true;
            mnu_joined_VAL.Name = "mnu_joined_VAL";
            mnu_joined_VAL.Size = new System.Drawing.Size(181, 22);
            mnu_joined_VAL.Tag =  Stepflow.ControllerVariable.VAL;
            mnu_joined_VAL.Text = "VAL";
            // 
            // mnu_joined_MIN
            // 
            mnu_joined_MIN.CheckOnClick = true;
            mnu_joined_MIN.Name = "mnu_joined_MIN";
            mnu_joined_MIN.Size = new System.Drawing.Size(181, 22);
            mnu_joined_MIN.Tag = Stepflow.ControllerVariable.MIN;
            mnu_joined_MIN.Text = "MIN";
            // 
            // mnu_joined_MAX
            // 
            mnu_joined_MAX.CheckOnClick = true;
            mnu_joined_MAX.Name = "mnu_joined_MAX";
            mnu_joined_MAX.Size = new System.Drawing.Size(181, 22);
            mnu_joined_MAX.Tag = Stepflow.ControllerVariable.MAX;
            mnu_joined_MAX.Text = "MAX";
            // 
            // mnu_joined_MOV
            // 
            mnu_joined_MOV.CheckOnClick = true;
            mnu_joined_MOV.Name = "mnu_joined_MOV";
            mnu_joined_MOV.Size = new System.Drawing.Size(181, 22);
            mnu_joined_MOV.Tag = Stepflow.ControllerVariable.MOV;
            mnu_joined_MOV.Text = "MOV";
            // 
            // mnu_valence_MOV
            // 
            mnu_valence_MOV.CheckOnClick = true;
            mnu_valence_MOV.DropDown = sub_Joined;
            mnu_valence_MOV.Name = "mnu_valence_MOV";
            mnu_valence_MOV.Size = new System.Drawing.Size(181, 22);
            mnu_valence_MOV.Tag = Stepflow.ControllerVariable.MOV;
            mnu_valence_MOV.Text = "MOV";
            mnu_valence_MOV.DropDownItemClicked += ValenceVariable_ExternalVariableClicked;
            mnu_valence_MOV.CheckedChanged += ValenceVariable_CheckedChanged;

            // 
            // sub_Valence
            // 
            sub_Valence.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            cbx_valence_FieldSelector,
            mnu_valence_VAL,
            mnu_valence_MIN,
            mnu_valence_MAX,
            mnu_valence_MOV});
            sub_Valence.Name = "mnu_value";
            sub_Valence.Size = new System.Drawing.Size(182, 119);
            // 
            // cbx_valence_FieldSelector
            // 
            cbx_valence_FieldSelector.Name = "cbx_valence_FieldSelector";
            cbx_valence_FieldSelector.Size = new System.Drawing.Size(121, 23);
            // 
            // mnu_valence_VAL
            // 
            mnu_valence_VAL.CheckOnClick = true;
            mnu_valence_VAL.DropDown = sub_Joined;
            mnu_valence_VAL.Name = "mnu_valence_VAL";
            mnu_valence_VAL.Size = new System.Drawing.Size(181, 22);
            mnu_valence_VAL.Tag = Stepflow.ControllerVariable.VAL;
            mnu_valence_VAL.Text = "VAL";
            mnu_valence_VAL.DropDownItemClicked += ValenceVariable_ExternalVariableClicked;
            mnu_valence_VAL.CheckedChanged += ValenceVariable_CheckedChanged;

            // 
            // mnu_valence_MIN
            // 
            mnu_valence_MIN.CheckOnClick = true;
            mnu_valence_MIN.DropDown = sub_Joined;
            mnu_valence_MIN.Name = "mnu_valence_MIN";
            mnu_valence_MIN.Size = new System.Drawing.Size(181, 22);
            mnu_valence_MIN.Tag = Stepflow.ControllerVariable.MIN;
            mnu_valence_MIN.Text = "MIN";
            mnu_valence_MIN.DropDownItemClicked += ValenceVariable_ExternalVariableClicked;
            mnu_valence_MIN.CheckedChanged += ValenceVariable_CheckedChanged;

            // 
            // mnu_valence_MAX
            // 
            mnu_valence_MAX.CheckOnClick = true;
            mnu_valence_MAX.DropDown = sub_Joined;
            mnu_valence_MAX.Name = "mnu_valence_MAX";
            mnu_valence_MAX.Size = new System.Drawing.Size(181, 22);
            mnu_valence_MAX.Tag = Stepflow.ControllerVariable.MAX;
            mnu_valence_MAX.Text = "MAX";
            mnu_valence_MAX.DropDownItemClicked += ValenceVariable_ExternalVariableClicked;
            mnu_valence_MAX.CheckedChanged += ValenceVariable_CheckedChanged;

            ContextMenuHook.Name = "mnu_Valence";
            ContextMenuHook.Text = "Valence";
            ContextMenuHook.CheckOnClick = true;
            ContextMenuHook.Checked = false;
            ContextMenuHook.DropDown = sub_Valence;

            ContextMenuHook.CheckedChanged += ValenceMenu_CheckedChanged;
            cbx_valence_FieldSelector.SelectedIndexChanged += Valence_FieldSelector_IndexChanged;
            cbx_joined_FieldSelector.SelectedIndexChanged += Joined_FieldSelector_IndexChanged;

            sub_Joined.ResumeLayout(false);
            sub_Valence.ResumeLayout(false);

            parent.getMenuHook().Add(this);
        }

        public static implicit operator ToolStripMenuItem( ValenceBondMenu<cT> cast ) {
            return cast.ContextMenuHook;
        }
    }
}}
