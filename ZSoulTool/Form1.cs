using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Msgfile;

namespace ZSoulTool
{
    public partial class Form1 : Form
    {
        #region Data_types
        string[] ListNames = {"Health","Ki","Ki Recovery","Stamina",
            "Stamina Recovery","Enemy Stamina Eraser", "Unknown 1","Ground Speed",
        "Air Speed", "Dash Speed", "Unknown 2", "Normal Attack Damage",
            "Normal Ki Blast Damage","Super Attack Damage","Super Ki Blasts", "Physical Damage Received",
        "Ki Damage Received", "Physical Recharge Damage Received", "Ki Recharge Damage Received","Transform Duration",
        "Reinforcement Skills Duration","Unknown 3","Revival HP Amount","Unknown 4",
        "Ally Revival Speed"};
        string FileName;
        EffectList eList;
        ActivationList aList;
        idbItem[] Items;
        string FileNameMsgN;
        msg Names;
        string FileNameMsgD;
        msg Descs;
        bool NamesLoaded = false;
        bool DescsLoaded = false;
        bool lockMod = false;
        int copy;

        #endregion

        public Form1()
        {
            InitializeComponent();
            foreach (string str in ListNames) {
                var Item = new ListViewItem(new[] { str, "1.0" });
                var Item1 = new ListViewItem(new[] { str, "1.0" });
                var Item2 = new ListViewItem(new[] { str, "1.0" });
                lstvBasic.Items.Add(Item);
                lstvEffect1.Items.Add(Item1);
                lstvEffect2.Items.Add(Item2);
            }
        }

        #region ListBox Methods
        
        private void lstvBasic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvBasic.SelectedItems.Count != 0 && !lockMod)
            {
                txtEditNameb.Text = lstvBasic.SelectedItems[0].SubItems[0].Text;
                txtEditValueb.Text = lstvBasic.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void txtEditValueb_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                lstvBasic.SelectedItems[0].SubItems[1].Text = txtEditValueb.Text;
                float n;
                if (float.TryParse(txtEditValueb.Text, out n))
                    Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, 32 + (lstvBasic.SelectedItems[0].Index * 4), 4);
            }
        }

        private void lstvEffect1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvEffect1.SelectedItems.Count != 0 && !lockMod)
            {
                txtEditName1.Text = lstvEffect1.SelectedItems[0].SubItems[0].Text;
                txtEditValue1.Text = lstvEffect1.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void txtEditValue1_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                lstvEffect1.SelectedItems[0].SubItems[1].Text = txtEditValue1.Text;
                float n;
                if (float.TryParse(txtEditValue1.Text, out n))
                    Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, 256 + (lstvEffect1.SelectedItems[0].Index * 4), 4);
            }
        }

        private void lstvEffect2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvEffect2.SelectedItems.Count != 0 && !lockMod)
            {
                txtEditName2.Text = lstvEffect2.SelectedItems[0].SubItems[0].Text;
                txtEditValue2.Text = lstvEffect2.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void txtEditValue2_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                lstvEffect2.SelectedItems[0].SubItems[1].Text = txtEditValue2.Text;
                float n;
                if (float.TryParse(txtEditValue2.Text, out n))
                    Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, 480 + (lstvEffect2.SelectedItems[0].Index * 4), 4);
            }
        }

        public static void Applybyte(ref byte[] file, byte[] data, int pos, int count)
        {
            for (int i = 0; i < count; i++)
                file[pos + i] = data[i];
        }
        #endregion

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] idbfile = new byte[1];
            eList = new EffectList();
            aList = new ActivationList();
            
            
            //640
            //load talisman
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Filter = "Xenoverse idb (*.idb)|*.idb";
            browseFile.Title = "Browse for Talisman idb File";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;

            int count = 0;
            if (browseFile.FileName.Contains("talisman"))
            {
                FileName = browseFile.FileName;
                //MessageBox.Show(FileName);
                idbfile = File.ReadAllBytes(FileName);
                count = BitConverter.ToInt32(idbfile, 8);
            }

            if (chkMsgName.Checked)
            {
                //load msgfile for names
                browseFile = new OpenFileDialog();
                browseFile.Filter = "Xenoverse msg (*.msg)|*.msg";
                browseFile.Title = "Browse for talisman msg name File";
                if (!(browseFile.ShowDialog() == DialogResult.Cancel))
                    NamesLoaded = true;


                if (browseFile.FileName.Contains("talisman_name") && NamesLoaded)
                {
                    FileNameMsgN = browseFile.FileName;
                    Names = msgStream.Load(FileNameMsgN);
                }
            }

            if (chkMsgDesc.Checked)
            {
                //load msgfile for names
                browseFile = new OpenFileDialog();
                browseFile.Filter = "Xenoverse msg (*.msg)|*.msg";
                browseFile.Title = "Browse for talisman msg info File";
                if (!(browseFile.ShowDialog() == DialogResult.Cancel))
                    DescsLoaded = true;


                if (browseFile.FileName.Contains("talisman_info") && DescsLoaded)
                {
                    FileNameMsgD = browseFile.FileName;
                    Descs = msgStream.Load(FileNameMsgD);
                }
            }

            //idbItems set
            Items = new idbItem[count];
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i].Data = new byte[640];
                Array.Copy(idbfile, 16 + (i * 640), Items[i].Data, 0, 640);
                if (NamesLoaded)
                    Items[i].msgIndexName = FindmsgIndex(ref Names, BitConverter.ToInt16(Items[i].Data, 4));
                if (DescsLoaded)
                    Items[i].msgIndexDesc = FindmsgIndex(ref Descs, BitConverter.ToInt16(Items[i].Data, 6));
            }

            

            itemList.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                if (NamesLoaded)
                    itemList.Items.Add(BitConverter.ToInt16(Items[i].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[i].Data, 0)) + "-" + Names.data[Items[i].msgIndexName].Lines[0]);
                else
                    itemList.Items.Add(BitConverter.ToInt16(Items[i].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[i].Data, 0)));
            }
            EffectData();
            itemList.SelectedIndex = 0;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<byte> Finalize = new List<byte>();
            Finalize.AddRange(new byte[] { 0x23, 0x49, 0x44, 0x42, 0xFE, 0xFF, 0x07, 0x00 });
            Finalize.AddRange(BitConverter.GetBytes(Items.Length));
            Finalize.AddRange(new byte[] { 0x10, 0x00, 0x00, 0x00 });

            for (int i = 0; i < Items.Length; i++)
                Finalize.AddRange(Items[i].Data);

            FileStream fs = new FileStream(FileName, FileMode.Create);
            fs.Write(Finalize.ToArray(), 0, Finalize.Count);
            fs.Close();

            if (NamesLoaded)
                msgStream.Save(Names, FileNameMsgN);

            if (DescsLoaded)
                msgStream.Save(Descs, FileNameMsgD);

        }

        public void EffectData()
        {

            if (File.Exists("EffectData.xml"))
            {
                XmlDocument xd = new XmlDocument();
                xd.Load("EffectData.xml");
                eList.ConstructList(xd.SelectSingleNode("EffectData/Effects").ChildNodes);
                aList.ConstructList(xd.SelectSingleNode("EffectData/Activations").ChildNodes);
            }
            else
            {
                eList.ConstructFromUnknown(ref Items);
                aList.ConstructFromUnknown(ref Items);

                //build EFfectData
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "\t",

                };

                using (XmlWriter xw = XmlWriter.Create("EffectData.xml", xmlWriterSettings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("EffectData");
                    xw.WriteStartElement("Effects");
                    for (int i = 0; i < eList.effects.Length; i++)
                    {
                        xw.WriteStartElement("Item");
                        xw.WriteStartAttribute("id");
                        xw.WriteValue(eList.effects[i].ID);
                        xw.WriteEndAttribute();
                        xw.WriteStartAttribute("hex");
                        xw.WriteValue(String.Format("{0:X}",eList.effects[i].ID));
                        xw.WriteEndAttribute();
                        xw.WriteValue(eList.effects[i].Description);
                        xw.WriteEndElement();

                    }
                    xw.WriteEndElement();

                    xw.WriteStartElement("Activations");
                    for (int i = 0; i < aList.activations.Length; i++)
                    {
                        xw.WriteStartElement("Item");
                        xw.WriteStartAttribute("id");
                        xw.WriteValue(aList.activations[i].ID);
                        xw.WriteEndAttribute();
                        xw.WriteStartAttribute("hex");
                        xw.WriteValue(String.Format("{0:X}", aList.activations[i].ID));
                        xw.WriteEndAttribute();
                        xw.WriteValue(aList.activations[i].Description);
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Close();
                }

            }

            cbEffect1.Items.Clear();
            cbEffect2.Items.Clear();
            cbActive1.Items.Clear();
            cbActive2.Items.Clear();

            for (int i = 0; i < eList.effects.Length; i++)
            {
                cbEffect1.Items.Add(eList.effects[i].ID.ToString() + "/" + String.Format("{0:X}", eList.effects[i].ID) + " " + eList.effects[i].Description);
                cbEffect2.Items.Add(eList.effects[i].ID.ToString() + "/" + String.Format("{0:X}", eList.effects[i].ID) + " " + eList.effects[i].Description);
            }

            for (int i = 0; i < aList.activations.Length; i++)
            {
                cbActive1.Items.Add(aList.activations[i].ID.ToString() + "/" + String.Format("{0:X}", aList.activations[i].ID) + " " + aList.activations[i].Description);
                cbActive2.Items.Add(aList.activations[i].ID.ToString() + "/" + String.Format("{0:X}", aList.activations[i].ID) + " " + aList.activations[i].Description);
            }

            
        }

        public int FindmsgIndex(ref msg msgdata,int id)
        {
            for (int i = 0; i < msgdata.data.Length; i++)
            {
                if (msgdata.data[i].ID == id)
                    return i;
            }
            return 0;
        }

        public byte[] int16byte(string text)
        {
            Int16 value;
            value = Int16.Parse(text);
            return BitConverter.GetBytes(value);
        }

        public byte[] int32byte(string text)
        {
            Int32 value;
            value = Int32.Parse(text);
            return BitConverter.GetBytes(value);
        }

        public byte[] floatbyte(string text)
        {
            float value;
            value = float.Parse(text);
            return BitConverter.GetBytes(value);
        }

        private void itemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            
                UpdateData();
            
        }

        #region edit item
        private void txtMsgName_TextChanged(object sender, EventArgs e)
        {
            if (NamesLoaded && !lockMod)
            {
                Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0] = txtMsgName.Text;
                itemList.Items[itemList.SelectedIndex] = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 0)) + "-" + Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
            }
        }

        private void txtMsgDesc_TextChanged(object sender, EventArgs e)
        {
            if (DescsLoaded && !lockMod)
                Descs.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0] = txtMsgDesc.Text;
        }

        private void cbStar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!lockMod)
                Array.Copy(BitConverter.GetBytes((short)(cbStar.SelectedIndex + 1)), 0, Items[itemList.SelectedIndex].Data, 2, 2);
        }

        private void txtNameID_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                short ID;
                if (short.TryParse(txtNameID.Text, out ID))
                    Array.Copy(BitConverter.GetBytes(ID), 0, Items[itemList.SelectedIndex].Data, 4, 2);

                if (NamesLoaded)
                {
                    Items[itemList.SelectedIndex].msgIndexName = FindmsgIndex(ref Names, BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 4));
                    txtMsgName.Text = Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
                }

                if (NamesLoaded)
                    itemList.Items[itemList.SelectedIndex] = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 0)) + "-" + Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
            }
        }

        private void txtDescID_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                short ID;
                if (short.TryParse(txtDescID.Text, out ID))
                    Array.Copy(BitConverter.GetBytes(ID), 0, Items[itemList.SelectedIndex].Data, 6, 2);

                if (DescsLoaded)
                {
                    Items[itemList.SelectedIndex].msgIndexDesc = FindmsgIndex(ref Descs, BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 6));
                    txtMsgDesc.Text = Descs.data[Items[itemList.SelectedIndex].msgIndexDesc].Lines[0];
                }
            }
        }

        private void txtBuy_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int n;
                if (int.TryParse(txtBuy.Text, out n))
                    Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, 16, 4);
            }
        }

        private void txtSell_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int n;
                if (int.TryParse(txtSell.Text, out n))
                    Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, 20, 4);
            }
        }

        private void cbEffect1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID = eList.effects[cbEffect1.SelectedIndex].ID;
                byte[] pass;
                if (ID == -1)
                    pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                else
                    pass = BitConverter.GetBytes(ID);

                Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 160, 4);
            }
        }

        private void cbActive1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID = aList.activations[cbActive1.SelectedIndex].ID;
                byte[] pass;
                if (ID == -1)
                    pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                else
                    pass = BitConverter.GetBytes(ID);

                Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 164, 4);
            }
        }

        private void txtTimes1_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod) { 
                int ID;
                if (int.TryParse(txtTimes1.Text, out ID))
                {
                    byte[] pass;
                    if (ID == -1)
                        pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                    else
                        pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 168, 4);
                }
            }
        }

        private void txtADelay1_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                float ID;
                if (float.TryParse(txtADelay1.Text, out ID))
                {
                    byte[] pass;

                    pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 172, 4);
                }
            }
        }

        private void txtAVal1_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                float ID;
                if (float.TryParse(txtAVal1.Text, out ID))
                {
                    byte[] pass;

                    pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 176, 4);
                }
            }
        }

        private void txtChance1_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID;
                if (int.TryParse(txtChance1.Text, out ID))
                {
                    byte[] pass;
                    if (ID == -1)
                        pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                    else
                        pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 200, 4);
                }
            }
        }

        private void cbEffect2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID = eList.effects[cbEffect2.SelectedIndex].ID;
                byte[] pass;
                if (ID == -1)
                    pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                else
                    pass = BitConverter.GetBytes(ID);

                Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 384, 4);
            }
        }

        private void cbActive2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID = aList.activations[cbActive2.SelectedIndex].ID;
                byte[] pass;
                if (ID == -1)
                    pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                else
                    pass = BitConverter.GetBytes(ID);

                Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 388, 4);
            }
        }

        private void txtTimes2_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID;
                if (int.TryParse(txtTimes2.Text, out ID))
                {
                    byte[] pass;
                    if (ID == -1)
                        pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                    else
                        pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 392, 4);
                }
            }
        }

        private void txtADelay2_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                float ID;
                if (float.TryParse(txtADelay2.Text, out ID))
                {
                    byte[] pass;

                    pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 396, 4);
                }
            }
        }

        private void txtAVal2_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                float ID;
                if (float.TryParse(txtAVal2.Text, out ID))
                {
                    byte[] pass;

                    pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 400, 4);
                }
            }
        }

        private void txtChance2_TextChanged(object sender, EventArgs e)
        {
            if (!lockMod)
            {
                int ID;
                if (int.TryParse(txtChance2.Text, out ID))
                {
                    byte[] pass;
                    if (ID == -1)
                        pass = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                    else
                        pass = BitConverter.GetBytes(ID);

                    Array.Copy(pass, 0, Items[itemList.SelectedIndex].Data, 424, 4);
                }
            }
        }
        #endregion

        private void UpdateData()
        {
            lockMod = true;
            // msg data
            if (NamesLoaded)
                txtMsgName.Text = Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
            if (DescsLoaded)
                txtMsgDesc.Text = Descs.data[Items[itemList.SelectedIndex].msgIndexDesc].Lines[0];

            // basic data
            txtID.Text = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 0).ToString();
            cbStar.SelectedIndex = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 2) - 1;
            txtNameID.Text = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 4).ToString();
            txtDescID.Text = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 6).ToString();
            txtBuy.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 16).ToString();
            txtSell.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 20).ToString();

            for (int i = 0; i < lstvBasic.Items.Count; i++)
            {
                lstvBasic.Items[i].SubItems[1].Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 32 + (i * 4)).ToString();
            }

            cbEffect1.SelectedIndex = eList.FindIndex(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 160));
            cbActive1.SelectedIndex = aList.FindIndex(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 164));
            txtTimes1.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 168).ToString();
            txtADelay1.Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 172).ToString();
            txtAVal1.Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 176).ToString();
            txtChance1.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 200).ToString();
            for (int i = 0; i < lstvEffect1.Items.Count; i++)
            {
                lstvEffect1.Items[i].SubItems[1].Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 256 + (i * 4)).ToString();
            }

            cbEffect2.SelectedIndex = eList.FindIndex(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 384));
            cbActive2.SelectedIndex = aList.FindIndex(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 388));
            txtTimes2.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 392).ToString();
            txtADelay2.Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 396).ToString();
            txtAVal2.Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 400).ToString();
            txtChance2.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, 424).ToString();

            for (int i = 0; i < lstvEffect2.Items.Count; i++)
            {
                lstvEffect2.Items[i].SubItems[1].Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, 480 + (i * 4)).ToString();
            }

            if (lstvBasic.SelectedItems.Count != 0)
            {
                txtEditNameb.Text = lstvBasic.SelectedItems[0].SubItems[0].Text;
                txtEditValueb.Text = lstvBasic.SelectedItems[0].SubItems[1].Text;
            }

            if (lstvEffect1.SelectedItems.Count != 0)
            {
                txtEditName1.Text = lstvEffect1.SelectedItems[0].SubItems[0].Text;
                txtEditValue1.Text = lstvEffect1.SelectedItems[0].SubItems[1].Text;
            }

            if (lstvEffect2.SelectedItems.Count != 0)
            {
                txtEditName2.Text = lstvEffect2.SelectedItems[0].SubItems[0].Text;
                txtEditValue2.Text = lstvEffect2.SelectedItems[0].SubItems[1].Text;
            }
            lockMod = false;
        }
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //add/import Z -soul
            //load zss file
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Filter = "Z Soul Share File (*.zss)|*.zss";
            browseFile.Title = "Select the Z-Soul you want to use as a template for the new Z-soul";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;

            byte[] zssfile = File.ReadAllBytes(browseFile.FileName);
            int nameCount = BitConverter.ToInt32(zssfile, 4);
            int DescCount = BitConverter.ToInt32(zssfile, 8);

            //expand the item array
            
            idbItem[] Expand = new idbItem[Items.Length + 1];
            Array.Copy(Items, Expand, Items.Length);
            Expand[Expand.Length - 1].Data = new byte[640];
            Items = Expand;
            short ID = BitConverter.ToInt16(Items[Items.Length - 2].Data, 0);
            ID++;
            Array.Copy(BitConverter.GetBytes(ID),Items[Items.Length - 1].Data,2);

            //apply Zss data to added z-soul
            Array.Copy(zssfile, 12 + (nameCount * 2) + (DescCount * 2), Items[Items.Length - 1].Data, 2, 638);

            //expand Names msg
            byte[] pass;
            if (NamesLoaded)
            {
                msgData[] Expand2 = new msgData[Names.data.Length + 1];
                Array.Copy(Names.data, Expand2, Names.data.Length);
                Expand2[Expand2.Length - 1].NameID = "talisman_" + Names.data.Length.ToString("000");
                Expand2[Expand2.Length - 1].ID = Names.data.Length;
                if (nameCount > 0)
                {
                    pass = new byte[nameCount * 2];
                    Array.Copy(zssfile, 12, pass, 0, nameCount * 2);
                    Expand2[Expand2.Length - 1].Lines = new string[] { BytetoString(pass) };
                }
                else
                    Expand2[Expand2.Length - 1].Lines = new string[] { "New Name Entry" };

                Array.Copy(BitConverter.GetBytes((short)Expand2[Expand2.Length - 1].ID), 0, Items[Items.Length - 1].Data, 4, 2);
                Names.data = Expand2;
                Items[Items.Length - 1].msgIndexName = FindmsgIndex(ref Names, Names.data[Names.data.Length - 1].ID);

            }

            //expand description msg
            if (DescsLoaded)
            {

                msgData[] Expand3 = new msgData[Descs.data.Length + 1];
                Array.Copy(Descs.data, Expand3, Descs.data.Length);
                Expand3[Expand3.Length - 1].NameID = "talisman_eff_" + Descs.data.Length.ToString("000");
                Expand3[Expand3.Length - 1].ID = Descs.data.Length;
                if (DescCount > 0)
                {
                    pass = new byte[DescCount * 2];
                    Array.Copy(zssfile, 12 + (nameCount * 2), pass, 0, DescCount * 2);
                    Expand3[Expand3.Length - 1].Lines = new string[] { BytetoString(pass) };
                }
                else
                    Expand3[Expand3.Length - 1].Lines = new string[] { "New Description Entry" };

                Array.Copy(BitConverter.GetBytes((short)Expand3[Expand3.Length - 1].ID), 0, Items[Items.Length - 1].Data, 6, 2);
                Descs.data = Expand3;
                Items[Items.Length - 1].msgIndexDesc = FindmsgIndex(ref Descs, Descs.data[Descs.data.Length - 1].ID);

            }

            //loadzss(browseFile.FileName, Items.Length - 1);
            //itemList.SelectedIndex = itemList.Items.Count - 1;
            itemList.Items.Clear();
            for (int i = 0; i < Items.Length; i++)
            {
                if (NamesLoaded)
                    itemList.Items.Add(BitConverter.ToInt16(Items[i].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[i].Data, 0)) + "-" + Names.data[Items[i].msgIndexName].Lines[0]);
                else
                    itemList.Items.Add(BitConverter.ToInt16(Items[i].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[i].Data, 0)));
            }
            
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //remove Z-soul  
            if (Items.Length > 211)
            {
                itemList.SelectedIndex = 0;
                idbItem[] Reduce = new idbItem[Items.Length - 1];
                Array.Copy(Items, Reduce, Items.Length - 1);
                
                Items = Reduce;
                
                itemList.Items.Clear();
                for (int i = 0; i < Items.Length; i++)
                {
                    if (NamesLoaded)
                        itemList.Items.Add(BitConverter.ToInt16(Items[i].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[i].Data, 0)) + "-" + Names.data[Items[i].msgIndexName].Lines[0]);
                    else
                        itemList.Items.Add(BitConverter.ToInt16(Items[i].Data, 0).ToString() + " / " + String.Format("{0:X}", BitConverter.ToInt16(Items[i].Data, 0)));
                }


            }
        }

        private void nameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //add msg name
            if (NamesLoaded)
            {
               
            msgData[] Expand = new msgData[Names.data.Length + 1];
            Array.Copy(Names.data, Expand, Names.data.Length);
            Expand[Expand.Length - 1].NameID = "talisman_" + Names.data.Length.ToString("000");
            Expand[Expand.Length - 1].ID = Names.data.Length;
            Expand[Expand.Length - 1].Lines = new string[] { "New Name Entry" };
            Names.data = Expand;

            DialogResult msgbox = MessageBox.Show("Do you want to set current Z-soul to use this Name", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (msgbox == DialogResult.Yes)
                txtNameID.Text = Names.data[Names.data.Length - 1].ID.ToString();   
            }
        }

        private void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //remove msg name
            if (Items.Length > 211 && NamesLoaded)
            {
                msgData[] reduce = new msgData[Names.data.Length - 1];
                Array.Copy(Names.data, reduce, Names.data.Length - 1);
                Names.data = reduce;
            }
        }

        private void nameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
            //add msg desc
            if (DescsLoaded)
            {
                
                msgData[] Expand = new msgData[Descs.data.Length + 1];
                Array.Copy(Descs.data, Expand, Descs.data.Length);
                Expand[Expand.Length - 1].NameID = "talisman_eff_" + Descs.data.Length.ToString("000");
                Expand[Expand.Length - 1].ID = Descs.data.Length;
                Expand[Expand.Length - 1].Lines = new string[] { "New Description Entry" };
                Descs.data = Expand;

                DialogResult msgbox = MessageBox.Show("Do you want to set current Z-soul to use this Description", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (msgbox == DialogResult.Yes)
                    txtDescID.Text = Descs.data[Descs.data.Length - 1].ID.ToString();
                
            }
        }

        private void descriptionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //remove msg desc
            if (Items.Length > 211 && DescsLoaded)
            {
                msgData[] reduce = new msgData[Descs.data.Length - 1];
                Array.Copy(Descs.data, reduce, Descs.data.Length - 1);
                Descs.data = reduce;
            }
        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {

        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //export ZSS
            List<byte> zssfile = new List<byte>();
            zssfile.AddRange(new byte[] {0x23,0x5A,0x53,0x53});
            if (NamesLoaded)
                zssfile.AddRange(BitConverter.GetBytes(txtMsgName.TextLength));
            else
                zssfile.AddRange(BitConverter.GetBytes(0));

            if (DescsLoaded)
                zssfile.AddRange(BitConverter.GetBytes(txtMsgDesc.TextLength));
            else
                zssfile.AddRange(BitConverter.GetBytes(0));

            if (NamesLoaded)
                zssfile.AddRange(CharByteArray(txtMsgName.Text));

            if (DescsLoaded)
                zssfile.AddRange(CharByteArray(txtMsgDesc.Text));

            byte[] itempass = new byte[638];
            Array.Copy(Items[itemList.SelectedIndex].Data, 2, itempass, 0, 638);
            zssfile.AddRange(itempass);

            FileStream fs = new FileStream(txtMsgName.Text + ".zss", FileMode.Create);
            fs.Write(zssfile.ToArray(), 0, zssfile.Count);
            fs.Close();
        }

        private byte[] CharByteArray(string text)
        {
            char[] chrArray = text.ToCharArray();
            List<byte> bytelist = new List<byte>();
            for (int i = 0;i < chrArray.Length; i++)
            {
                bytelist.AddRange(BitConverter.GetBytes(chrArray[i]));
            }
            return bytelist.ToArray();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //import
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Filter = "Z Soul Share File (*.zss)|*.zss";
            browseFile.Title = "Browse for Z Soul Share File";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;

            loadzss(browseFile.FileName, itemList.SelectedIndex, 0 , 0, false);

            UpdateData();

        }

        private void loadzss(string pFileName, int oItem, short nID, short dID, bool useID)
        {
            byte[] zssfile = File.ReadAllBytes(pFileName);
            int nameCount = BitConverter.ToInt32(zssfile, 4);
            int DescCount = BitConverter.ToInt32(zssfile, 8);

            Array.Copy(zssfile, 12 + (nameCount * 2) + (DescCount * 2), Items[oItem].Data, 2, 638);




            byte[] pass;
            if (nameCount > 0)
            {
                pass = new byte[nameCount * 2];
                Array.Copy(zssfile, 12, pass, 0, nameCount * 2);
                txtMsgName.Text = BytetoString(pass);
            }

            if (DescCount > 0)
            {
                pass = new byte[DescCount * 2];
                Array.Copy(zssfile, 12 + (nameCount * 2), pass, 0, DescCount * 2);
                txtMsgDesc.Text = BytetoString(pass);
            }

            

            //UpdateData();
        }

        private string BytetoString(byte[] bytes)
        {
            char[] chrArray = new char[bytes.Length / 2];
            for (int i = 0; i < bytes.Length / 2; i++)
                chrArray[i] = BitConverter.ToChar(bytes, i * 2);

            return new string(chrArray);
        }

        private void replaceImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //import/replace
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Filter = "Z Soul Share File (*.zss)|*.zss";
            browseFile.Title = "Browse for Z Soul Share File";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;

            byte[] zssfile = File.ReadAllBytes(browseFile.FileName);
            int nameCount = BitConverter.ToInt32(zssfile, 4);
            int DescCount = BitConverter.ToInt32(zssfile, 8);

            short nameID = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 4);
            short DescID = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 6);
            Array.Copy(zssfile, 12 + (nameCount * 2) + (DescCount * 2), Items[itemList.SelectedIndex].Data, 2, 638);

            Array.Copy(BitConverter.GetBytes(nameID), 0, Items[itemList.SelectedIndex].Data, 4, 2);
            Array.Copy(BitConverter.GetBytes(DescID), 0, Items[itemList.SelectedIndex].Data, 6, 2);

            byte[] pass;
            if (nameCount > 0)
            {
                pass = new byte[nameCount * 2];
                Array.Copy(zssfile, 12, pass, 0, nameCount * 2);
                txtMsgName.Text = BytetoString(pass);
            }

            if (DescCount > 0)
            {
                pass = new byte[DescCount * 2];
                Array.Copy(zssfile, 12 + (nameCount * 2), pass, 0, DescCount * 2);
                txtMsgDesc.Text = BytetoString(pass);
            }

            UpdateData();
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //export ZSS
            List<byte> zssfile = new List<byte>();
            zssfile.AddRange(new byte[] { 0x23, 0x5A, 0x53, 0x53 });
            if (NamesLoaded)
                zssfile.AddRange(BitConverter.GetBytes(txtMsgName.TextLength));
            else
                zssfile.AddRange(BitConverter.GetBytes(0));

            if (DescsLoaded)
                zssfile.AddRange(BitConverter.GetBytes(txtMsgDesc.TextLength));
            else
                zssfile.AddRange(BitConverter.GetBytes(0));

            if (NamesLoaded)
                zssfile.AddRange(CharByteArray(txtMsgName.Text));

            if (DescsLoaded)
                zssfile.AddRange(CharByteArray(txtMsgDesc.Text));

            byte[] itempass = new byte[638];
            Array.Copy(Items[itemList.SelectedIndex].Data, 2, itempass, 0, 638);
            zssfile.AddRange(itempass);

            FileStream fs = new FileStream(txtMsgName.Text + ".zss", FileMode.Create);
            fs.Write(zssfile.ToArray(), 0, zssfile.Count);
            fs.Close();
        }

        private void patchesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void patchAllForStoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //patch for store
            for (int i = 0; i < Items.Length; i++)
            {
                int sellValue = BitConverter.ToInt32(Items[i].Data, 20);
                Array.Copy(BitConverter.GetBytes(sellValue * 2), 0, Items[i].Data, 16, 4);
            }
        }
    }
    
}
