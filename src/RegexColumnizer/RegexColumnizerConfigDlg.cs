using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace LogExpert
{
  public partial class RegexColumnizerConfigDlg : Form
  {
    RegexColumnizerConfig config;
    string timestampField;

    public RegexColumnizerConfigDlg(RegexColumnizerConfig config)
    {
      this.config = config;
      InitializeComponent();
      this.regexText.Text = this.config.Regex.ToString();
      this.localTimeCheckBox.Checked = this.config.LocalTimestamps;
      this.timestampField = this.config.TimestampField;
      this.formatComboBox.Text = this.config.TimestampFormat;

    }
    internal void Apply(RegexColumnizerConfig config)
    {
        config.Regex = new Regex(regexText.Text, RegexOptions.IgnoreCase);
        config.LocalTimestamps = localTimeCheckBox.Checked;
        config.TimestampField=timestampField;
        config.TimestampFormat = formatComboBox.Text;

        string[] selectedFields= new string[listView1.CheckedItems.Count];
        for(int i=0;i<selectedFields.Length;i++)
        {
            selectedFields[i]=listView1.CheckedItems[i].Text;
        }
        config.SelectedFields = selectedFields;
  
    }


    private void okButton_Click(object sender, EventArgs e)
    {
        Apply(config);
    }

    Dictionary<string, ListViewItem> oldFields = new Dictionary<string, ListViewItem>();

    private void regexText_Validating(object sender, CancelEventArgs e)
    {
        try
        {
            Regex regex = new Regex(regexText.Text, RegexOptions.IgnoreCase);
            foreach (ListViewItem item in listView1.Items)
            {
                oldFields[item.Text] = item;
            }
            fieldComboBox.Items.Clear();
            fieldComboBox.Items.Add("<Select Timestamp>");

            Dictionary<string, bool> fields = new Dictionary<string, bool>();

            int i = 0;
            foreach (string name in regex.GetGroupNames())
            {
                if (int.TryParse(name, out i)) continue;
                if (oldFields.ContainsKey(name))
                {
                    ListViewItem item = oldFields[name];
                    if (!listView1.Items.Contains(item))
                        listView1.Items.Add(item);
                }
                else
                {
                    System.Windows.Forms.ListViewItem item =
                        new System.Windows.Forms.ListViewItem(name);
                    item.Checked = true;
                    item.Name = name;
                    listView1.Items.Add(item);
                    
                }
                fieldComboBox.Items.Add(name);
                if (name.Equals(timestampField)) 
                    fieldComboBox.SelectedIndex = fieldComboBox.Items.Count-1;

                
            }

            if (fieldComboBox.SelectedIndex < 0) fieldComboBox.SelectedIndex = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                int groupNumber = regex.GroupNumberFromName(item.Text);
                if (groupNumber < 0) listView1.Items.Remove(item);
            }
        }
        catch(Exception ex)
        {
            MessageBox.Show("Invalid Regular Expression");
            e.Cancel = true;
        }
    }

    private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
        //Begins a drag-and-drop operation in the ListView control.
        listView1.DoDragDrop(listView1.SelectedItems, DragDropEffects.Move);
    }

    private void listView1_DragEnter(object sender, DragEventArgs e)
    {
        int len = e.Data.GetFormats().Length - 1;
        int i;
        for (i = 0; i <= len; i++)
        {
            if (e.Data.GetFormats()[i].Equals("System.Windows.Forms.ListView+SelectedListViewItemCollection"))
            {
                //The data from the drag source is moved to the target.	
                e.Effect = DragDropEffects.Move;
            }
        }
    }

    private void listView1_DragDrop(object sender, DragEventArgs e)
    {
        //Return if the items are not selected in the ListView control.
        if (listView1.SelectedItems.Count == 0)
        {
            return;
        }
        //Returns the location of the mouse pointer in the ListView control.
        Point cp = listView1.PointToClient(new Point(e.X, e.Y));
        //Obtain the item that is located at the specified location of the mouse pointer.
        ListViewItem dragToItem = listView1.GetItemAt(cp.X, cp.Y);
        if (dragToItem == null)
        {
            return;
        }
        //Obtain the index of the item at the mouse pointer.
        int dragIndex = dragToItem.Index;
        ListViewItem[] sel = new ListViewItem[listView1.SelectedItems.Count];
        for (int i = 0; i <= listView1.SelectedItems.Count - 1; i++)
        {
            sel[i] = listView1.SelectedItems[i];
        }
        for (int i = 0; i < sel.GetLength(0); i++)
        {
            //Obtain the ListViewItem to be dragged to the target location.
            ListViewItem dragItem = sel[i];
            int itemIndex = dragIndex;
            if (itemIndex == dragItem.Index)
            {
                return;
            }
            if (dragItem.Index < itemIndex)
                itemIndex++;
            else
                itemIndex = dragIndex + i;
            //Insert the item at the mouse pointer.
            ListViewItem insertItem = (ListViewItem)dragItem.Clone();
            listView1.Items.Insert(itemIndex, insertItem);
            //Removes the item from the initial location while 
            //the item is moved to the new location.
            listView1.Items.Remove(dragItem);
        }

    }

    private void RegexColumnizerConfigDlg_Load(object sender, EventArgs e)
    {
        ValidateChildren();
    }

    private void fieldComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        formatComboBox.Enabled = localTimeCheckBox.Enabled = fieldComboBox.SelectedIndex > 0;
    }

    private void fieldComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        timestampField = fieldComboBox.SelectedIndex==0?"":fieldComboBox.SelectedItem.ToString();
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
        Apply(config);
    }
  }
}
