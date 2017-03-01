using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {
        public class cellclass
        {
            public int actual = 0;
            public int col = 0;
            public int row = 0;
            public List<int> possible = new List<int>();
            public List<int> impossible = new List<int>();
            public List<string> groups = new List<string>();

            public bool setactual(int actualnumber)
            {
                if (actual != 0)
                    return false;

                foreach (string gr in groups)
                {
                    foreach (string cc in groupdict[gr].cells)
                    {
                        if (celldict[cc].actual == actualnumber)
                            return false;
                    }
                }

                actual = actualnumber;

                possible.Clear();

                foreach (string gr in groups)
                {
                    foreach (string cc in groupdict[gr].cells)
                    {
                        if (celldict[cc].actual == 0)
                        {
                            if (celldict[cc].possible.Contains(actualnumber))
                                celldict[cc].possible.Remove(actualnumber);
                            if (!celldict[cc].impossible.Contains(actualnumber))
                                celldict[cc].impossible.Add(actualnumber);
                        }
                            
                    }
                }


                return true;

            }
        }

        public class groupclass
        {
            public List<string> cells = new List<string>();

        }
        public static Dictionary<string,cellclass> celldict= new Dictionary<string,cellclass>();
        public static Dictionary<string, groupclass> groupdict = new Dictionary<string, groupclass>(); 


        public Form1()
        {
            InitializeComponent();
            dataGridView1.ColumnCount = 9;
            dataGridView1.RowCount = 9;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.Width = 36;
                col.ValueType = typeof(int);
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
                row.Height = 36;
            dataGridView1.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            //Single fontsize = 28.0F;
            //dataGridView1.DefaultCellStyle.Font = new Font(DataGridView.DefaultFont, fontsize,FontStyle.Regular);

            for (int icol = 0; icol < 9; icol++)
            {
                string colname = "c" + icol.ToString();
                groupclass colgroup = new groupclass();
                for (int irow = 0; irow < 9; irow++)
                {
                    string cellname = icol.ToString() + irow.ToString();
                    colgroup.cells.Add(cellname);
                }
                groupdict.Add(colname,colgroup);
                
                for (int irow = 0; irow < 9; irow++)
                {
                    string rowname = "r" + irow.ToString();
                    if (!groupdict.ContainsKey(rowname))
                    {
                        groupclass rowgroup = new groupclass();
                        for (int icol2 = 0; icol2 < 9; icol2++)
                        {
                            string cellname2 = icol2.ToString() + irow.ToString();
                            rowgroup.cells.Add(cellname2);
                        }
                        groupdict.Add(rowname, rowgroup);
                    }

                    string quadname = findquad(icol, irow);
                    if (!groupdict.ContainsKey(quadname))
                    {
                        groupclass quadgroup = new groupclass();
                        groupdict.Add(quadname,quadgroup);
                    }


                    cellclass cell = new cellclass();
                    cell.col = icol;
                    cell.row = irow;
                    for (int i = 1; i < 10; i++)
                        cell.possible.Add(i);
                    string cellname = icol.ToString() + irow.ToString();
                    groupdict[quadname].cells.Add(cellname);

                    cell.groups.Add(rowname);
                    cell.groups.Add(colname);
                    cell.groups.Add(quadname);

                    celldict.Add(cellname, cell);

                    int quadparity = ((icol / 3) + (irow / 3)) % 2;
                    if (quadparity == 0)
                        setgridcolor(icol, irow, Color.LightPink);
                    else
                        setgridcolor(icol, irow, Color.LightBlue);

                }
            }
        }

        public void loadfromgrid()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        string cellvalue = cell.Value.ToString();
                        int i = 0;
                        try
                        {
                            i = Convert.ToInt32(cellvalue);
                        }
                        catch (FormatException)
                        {
                            i = -1;
                        }

                        string cellname = cell.ColumnIndex.ToString() + cell.RowIndex.ToString();
                        if ((i > 0) && (i < 10))
                        {
                            celldict[cellname].setactual(i);
                        }
                    }

                }

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("button1click start\n");

            loadfromgrid();


            string line = "";
            foreach (string cc in celldict.Keys)
            {
                
                if (celldict[cc].actual > 0)
                    line += celldict[cc].actual.ToString();
                else
                    line += "_";
                if (celldict[cc].row == 8)
                {
                    line += "\n";
                    textBox1.AppendText(line);
                    line = "";
                }
            }

            //bool changesomething = false;
            int loopsnochange = 0;

            do
            {
                //changesomething = false;
                loopsnochange++;
                foreach (string cc in celldict.Keys)
                {
                    if (celldict[cc].actual == 0)
                        if (celldict[cc].possible.Count == 1)
                        {
                            int i = celldict[cc].possible[0];
                            {
                                celldict[cc].setactual(i);
                                setgridvalue(celldict[cc].col, celldict[cc].row, i,"Only one possible");
                                loopsnochange = 0;
                            }
                        }

                }

                foreach (string gr in groupdict.Keys)
                {
                    for (int i = 1; i < 10; i++)
                    {
                        int nposs = 0;
                        string foundposs = "";
                        foreach (string cc in groupdict[gr].cells)
                            if (celldict[cc].possible.Contains(i))
                            {
                                nposs++;
                                foundposs = cc;
                            }
                        if (nposs == 1)
                        {
                            celldict[foundposs].setactual(i);
                            setgridvalue(celldict[foundposs].col, celldict[foundposs].row, i,"Only place in group");
                            loopsnochange = 0;
                        }
                    }

                    if (checkBox1.Checked)
                    {
                        for (int i = 1; i < 10; i++)
                        {
                            List<string> ingroups = new List<string>();
                            List<string> toremove = new List<string>();
                            foreach (string cc in groupdict[gr].cells)
                                if (celldict[cc].possible.Contains(i))
                                {
                                    foreach (string gr2 in celldict[cc].groups)
                                        if (!ingroups.Contains(gr2))
                                            ingroups.Add(gr2);
                                }
                            foreach (string cc in groupdict[gr].cells)
                                if (celldict[cc].possible.Contains(i))
                                {
                                    toremove.Clear();
                                    foreach (string gr3 in ingroups)
                                        if (!celldict[cc].groups.Contains(gr3))
                                            toremove.Add(gr3);
                                    foreach (string gr4 in toremove)
                                        ingroups.Remove(gr4);
                                }
                            if (ingroups.Count > 1)
                            {
                                foreach (string gr5 in ingroups)
                                    if (gr5 != gr)
                                    {
                                        foreach (string cc2 in groupdict[gr5].cells)
                                            if (!celldict[cc2].groups.Contains(gr))
                                                if (celldict[cc2].possible.Contains(i))
                                                    celldict[cc2].possible.Remove(i);
                                    }
                            }
                        }
                    }

                }

                if (checkBox2.Checked)
                {
                    textBox1.AppendText("Subset search\n");
                    List<string> binaries = new List<string>();
                    for (int i = 1; i < 512; i++)
                    {
                        string bin = Convert.ToString(i, 2);
                        while (bin.Length < 9)
                            bin = "0" + bin;
                        int n1 = 0;
                        foreach (char c in bin.ToCharArray())
                            if (c == '1')
                                n1++;
                        if ((n1 > 1) && (n1 < 9))
                            binaries.Add(bin);
                    }
                    //textBox1.AppendText("# binaries = "+binaries.Count.ToString()+"\n");

                    foreach (string gr in groupdict.Keys)
                    {
                        foreach (string bin in binaries)
                        {
                            List<string> cellset = new List<string>();
                            char[] cbin =  bin.ToCharArray();
                            int ibin = 0;
                            foreach (string cc in groupdict[gr].cells)
                            {
                                if (cbin[ibin] == '1')
                                {
                                    if (celldict[cc].actual > 0)
                                    {
                                        cellset.Clear();
                                        break;
                                    }
                                    cellset.Add(cc);
                                }
                                ibin++;
                            }
                            if (cellset.Count > 1)
                            {
                                //textBox1.AppendText("# cellset = " + cellset.Count.ToString()+"\n");
                                List<int> possibles = new List<int>();
                                foreach (string cc2 in cellset)
                                    foreach (int j in celldict[cc2].possible)
                                        if (!possibles.Contains(j))
                                            possibles.Add(j);
                                //textBox1.AppendText("# possibles = " + possibles.Count.ToString()+"\n");
                                if (possibles.Count == cellset.Count)
                                {
                                    textBox1.AppendText("subset found\n");
                                    foreach (string cc3 in groupdict[gr].cells)
                                        if (!cellset.Contains(cc3))
                                            foreach (int k in possibles)
                                                if (celldict[cc3].possible.Contains(k))
                                                    celldict[cc3].possible.Remove(k);
                                }
                            }
                        }

                        foreach (string bin in binaries)
                        {
                            List<string> cellset = new List<string>();
                            char[] cbin = bin.ToCharArray();
                            int ibin = 0;
                            foreach (string cc in groupdict[gr].cells)
                            {
                                if (cbin[ibin] == '1')
                                {
                                    if (celldict[cc].actual > 0)
                                    {
                                        cellset.Clear();
                                        break;
                                    }
                                    cellset.Add(cc);
                                }
                                ibin++;
                            }
                            if (cellset.Count > 1)
                            {
                                List<int> onlyhere = new List<int>();
                                foreach (string cc2 in cellset)
                                    foreach (int j in celldict[cc2].possible)
                                    {
                                        bool foundelsewhere = false;
                                        foreach (string cc in groupdict[gr].cells)
                                            if (!cellset.Contains(cc))
                                                if (celldict[cc].actual == 0)
                                                    if (celldict[cc].possible.Contains(j))
                                                        foundelsewhere = true;
                                        if (!foundelsewhere)
                                            if (!onlyhere.Contains(j))
                                                onlyhere.Add(j);
                                    }
                                if (onlyhere.Count == cellset.Count)
                                {
                                    textBox1.AppendText("Only here\n");
                                    foreach (string cc3 in cellset)
                                    {
                                        for (int j = 1; j < 10; j++)
                                            if (!onlyhere.Contains(j))
                                                if (celldict[cc3].possible.Contains(j))
                                                    celldict[cc3].possible.Remove(j);
                                    }
                                        
                                }
                            }
                        }
                    }
                }
            }
            while (loopsnochange < 10);

            printpossible();



            textBox1.AppendText("button1click end\n");
            
        }

        public void printpossible()
        {
            string line = "";
            foreach (string cc in celldict.Keys)
            {

                if (celldict[cc].actual == 0)
                {
                    line = cc + ": ";
                    foreach (int ip in celldict[cc].possible)
                        line += ip.ToString();
                    line += "\n";
                    textBox1.AppendText(line);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        public string findquad(int icol, int irow)
        {
            int rr = irow / 3;
            int cc = icol / 3;

            string quadname = "q" + rr.ToString() + cc.ToString();

            return quadname;
        }

        public void setgridvalue(int col, int row, int val, string msg)
        {
            dataGridView1.Rows[row].Cells[col].Value = val.ToString();
            textBox1.AppendText("Setting " + row.ToString() + col.ToString() + " to " + val.ToString() + " " + msg + "\n");
            
        }

        public void setgridcolor(int col, int row, Color newcol)
        {
            dataGridView1.Rows[row].Cells[col].Style.BackColor = newcol;
            //textBox1.AppendText("Setting " + row.ToString() + col.ToString() + " to " + val.ToString() + " " + msg + "\n");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "c:\\Sudoku\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                    {
                        string line;
                        int irow = 0;
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            if (line.Length < 9)
                                continue;
                            Char[] chars = line.ToCharArray();
                            for (int icol = 0; icol < 9; icol++)
                            {
                                if ((chars[icol] >= '1') && (chars[icol] <= '9'))
                                    setgridvalue(icol, irow, Convert.ToInt32(chars[icol])-48, "Read file");
                                

                            }
                            irow++;
                            if (irow > 8)
                                break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadfromgrid();
            printpossible();
        }
    }


}
