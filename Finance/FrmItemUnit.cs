﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
//using Microsoft.VisualBasic;

namespace FinOrg
{

    
    public partial class FrmItemUnit : FinOrgForm
  {
        SqlConnection Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Con"].ConnectionString);
        DataTable dt = new DataTable();
        DataView dv = new DataView();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter ada = new SqlDataAdapter();
        DataSet ds = new DataSet();
        //string sql;
        
        private void save_form()
        {
            try
            {



                dgv1.EndEdit();
                DataGridViewCell ccell = dgv1.CurrentCell;
                dgv1.CurrentCell = dgv1.Rows[dgv1.RowCount - 1].Cells[1];
                dgv1.CurrentCell = ccell;
              
                ada.Update(ds,"UnitMaster");
                string sql = " update  UnitMaster set Unit_Aname=Unit_name where Unit_aname='' or Unit_Aname is null";



                cmd = new SqlCommand(sql, Conn);

                cmd.ExecuteNonQuery();
                
               // ada.Update(dt);
               
                //dt.AcceptChanges();
            }
            
            catch (System.Exception excep)
            {

                MessageBox.Show(excep.Message);

            }
        }

        private void load_leaders()
        {
            Conn.Close();
            Conn.Open();







            //sql = "select Empno,Employee_name,Tele_Home from Leaders";

            //SqlCommand cmd = new SqlCommand(sql, Conn);

            
            //SqlDataAdapter ada = new SqlDataAdapter(cmd);
            if (radioButton1.Checked)
            ada = new SqlDataAdapter("select * from UnitMaster ", Conn);
            else
                ada = new SqlDataAdapter("select * from UnitMaster", Conn);

            ///ada.TableMappings.Add("Table", "Leaders");
            ds = new DataSet();
            DataTable dt = new DataTable("UnitMaster");
            dt.AcceptChanges();
            ///ada.Fill(dt);

            this.ada.Fill(this.ds, "UnitMaster");
            dv.Table = dt;
            SqlCommandBuilder cmdBldr = new SqlCommandBuilder(ada);

     
            this.ds.AcceptChanges();
            //set the table as the datasource for the grid in order to show that data in the grid
           
            dgv1.Visible = true;
           

            
            
            dv.AllowEdit = true;
            dv.AllowNew = true;
            dv.AllowDelete = true;
            
           /// dv.Table = dt;
            
            dgv1.DataSource = ds;
            dgv1.DataMember = "UnitMaster";
            
            
           dgv1.Columns[1].Width = 300;
            dgv1.Visible = true;

            dgv1.Columns[2].ReadOnly=true;
            dgv1.Columns[2].Visible = false;




            //OdbcDataAdapter ada = new OdbcDataAdapter(cmd);

            
            //dt = new DataTable("Leaders");
            //ada.Fill(dt);


            //dataGrid1.DataSource=dt.DefaultView();
            //dataGrid1.DataSource=
            //    .DataContext = dt.DefaultView;
        }

        private void delete_leaders()
        {
            dgv1.Select();
        }

  
        public FrmItemUnit()
        {
            InitializeComponent(); txtpriv.Text = Gvar.frm_priv.ToString();
        }

        private void FrmLeader_Load(object sender, EventArgs e)
        {

            //dt = dataGrid1.DataContext;
            //dt.BeginInit();
            





        }

        private void SearchToolStripButton_Click(object sender, EventArgs e)
        {
            load_leaders();

        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            save_form();
        }

        private void toolclose_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void FrmLeader_Activated(object sender, EventArgs e)
        {
            load_leaders();
        }

        private void tooldelete_Click(object sender, EventArgs e)
        {
            delete_leaders();
        }

        private void dgv1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            dgv1.EndEdit();
            if (!string.IsNullOrEmpty(dgv1[1, e.RowIndex].Value.ToString()))
            {
                if (radioButton1.Checked)
                    dgv1[2, e.RowIndex].Value = "I";
                else
                    dgv1[2, e.RowIndex].Value = "S";

            }
            //else
                //dgv1[2, e.RowIndex].Value = null;


        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            load_leaders();
        }
    }
}
