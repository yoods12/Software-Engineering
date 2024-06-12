﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using TP.control;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TP
{
    public partial class Return : Form
    {
        private string categori = null;
        private string label = "제품명";
        private bool saveSuccess = false; //저장 성공
        private bool selectsusses = false; // 검색 성공

        private InquiryInvenController stckcontroller;
        private OrderReturnController returnController;
        private LoginController loginController;
        private ProductInfoController productInfoController;

        private DataTable dt;

        public Return()
        {
            InitializeComponent();
            stckcontroller = new InquiryInvenController();
            returnController = new OrderReturnController();
            loginController = new LoginController();
            productInfoController = new ProductInfoController();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; //콤보 박스 읽기 전용
            comboBox1.Text = label;

            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);
            LoadCategories();

            dataview();
        }

        private void LoadCategories() // 카테고리 불러오기
        {
            try
            {
                // 기본 "전체" 추가
                comboBox2.Items.Add("전체");

                // 카테고리 데이터 불러오기
                DataTable categoryData = productInfoController.GetCategories();
                foreach (DataRow row in categoryData.Rows)
                {
                    comboBox2.Items.Add(row["카테고리"].ToString());
                }

                comboBox2.SelectedIndex = 0; // 기본적으로 "전체" 선택
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) //combobox2 선택된 인덱스 변경시 동작 
        {
            categori = comboBox2.SelectedItem.ToString() == "전체" ? null : comboBox2.SelectedItem.ToString();
            dataview();
        }

        private void dataview() //데이터 불러오는 부분 
        {
            try
            {

                dataGridView1.Columns.Clear();
                dt = stckcontroller.GetStock();
                if (!string.IsNullOrEmpty(categori)) // Check if categori is not empty or null
                {
                    dt.DefaultView.RowFilter = $"카테고리 ='{categori}'";
                }

                dataGridView1.AllowUserToAddRows = false; //빈레코드 표시x
                var chkCol = new DataGridViewCheckBoxColumn
                {
                    Name = "chk",
                    HeaderText = "선택"
                };
                dataGridView1.Columns.Add(chkCol);
                dataGridView1.DataSource = dt;   //데이터 추가 부분
                dataGridView1.Columns.Add("반품량", "반품량");
                dataGridView1.Columns.Add("비고", "비고");

                //크기 조절부분 
                dataGridView1.Columns[0].Width = 40;

                //dataGridView1.ReadOnly = true; //전부 읽기 전용           
                for (int i = 1; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].ReadOnly = true;
                }

                dataGridView1.Columns["반품량"].ReadOnly = false;
                dataGridView1.Columns["비고"].ReadOnly = false;
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void button2_Click(object sender, EventArgs e) 
        {
            //검색부분
            selectsusses = false;
            label = comboBox1.Text;
            find();
        }

        

        private void find() //검색 부분
        {
            string keyword = textBox1.Text;//Textbox에 입력된 메시지를 keyword 저장
                                           // 인덱스를 찾을 이름, 검색할 입력값

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {

                if (dataGridView1.Rows[i].Cells[$"{label}"].Value.ToString().Trim() == keyword.Trim())
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;  //색칠
                    selectsusses = true;
                }
                else
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }
            }
            if (selectsusses == false)
            {
                MessageBox.Show("검색 결과가 없습니다.");
            }

        }

        private void button1_Click(object sender, EventArgs e) //반품 버튼 클릭
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells["chk"].Value)) //체크된 데이터 선택 부분
                {
                    try
                    {
                        if (Properties.Settings.Default.Returnindex == 0)
                        {
                            returnController.SetReturn("DELETE 반품");
                        }

                        string user_address = loginController.GetUserDetail(Properties.Settings.Default.LoginIDSave.ToString(), "편의점주소");

                        string sqltxt = "MERGE \n into 반품 \n USING dual \n ON (반품제품 = :반품제품) " + "\n WHEN NOT MATCHED THEN \n" +
                            "insert (반품번호,반품고객,반품제품,반품수량,반품지,반품일자) values(:반품번호, :반품고객, :반품제품,:반품수량,:반품지,:반품일자)"
                            + "WHEN MATCHED THEN UPDATE SET 반품수량 = :반품수량 ";

                        OracleParameter[] returninfo =
                        {
                            new OracleParameter("반품번호", Properties.Settings.Default.Returnindex.ToString()),
                            new OracleParameter("반품고객", Properties.Settings.Default.LoginIDSave.ToString()),
                            new OracleParameter("반품제품", dataGridView1.Rows[i].Cells["제품번호"].Value.ToString()),
                            new OracleParameter("반품수량", Convert.ToInt32(dataGridView1.Rows[i].Cells["반품량"].Value)),
                            new OracleParameter("반품지", user_address),
                            new OracleParameter("반품일자", DateTime.Now.ToString("yyyy-MM-dd").ToString()),
                        };

                        returnController.SetReturn(sqltxt, returninfo);

                        if (DateTime.Now.ToString("yyyy-MM-dd").ToString() != Properties.Settings.Default.date)
                        {
                            sqltxt = "MERGE \n into 재고 \n USING dual \n ON (제품번호 = :제품번호) " +
                             "WHEN MATCHED THEN UPDATE SET 재고량 = 재고량 -:재고량 ";
                        }
                        else
                        {
                            sqltxt = "MERGE \n into 재고 \n USING dual \n ON (제품번호 = :제품번호) " +
                             "WHEN MATCHED THEN UPDATE SET 재고량 = 재고량 -:재고량 ";
                        }

                        OracleParameter[] stock =
                        {
                            new OracleParameter("제품번호", dataGridView1.Rows[i].Cells["제품번호"].Value.ToString()),
                            new OracleParameter("재고량", Convert.ToInt32(dataGridView1.Rows[i].Cells["반품량"].Value)),
                        };

                        stckcontroller.SetStock(sqltxt, stock);

                        Properties.Settings.Default.date = DateTime.Now.ToString("yyyy-MM-dd").ToString();

                        Properties.Settings.Default.Returnindex += 1; //반품번호 값증가시키기

                        saveSuccess = true;
                    }
                    catch (OracleException ex)
                    {
                        saveSuccess = false;
                        MessageBox.Show(ex.Message);
                    }
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                }
                else
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }

            }

            if (saveSuccess == true)
            {
                MessageBox.Show("저장되었습니다.");
                dataview();
            }
            //save 부분
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2.PerformClick();
            }
        }
        private void button3_Click(object sender, EventArgs e) //닫기 버튼 
        {
            this.Close();
        }

        private void Return_FormClosing(object sender, FormClosingEventArgs e)
        {
            //닫혔을때 save 하는지 물어보는 부분 
            if (saveSuccess == false)
            {
                DialogResult dialog = MessageBox.Show("저장하시겠습니까?", "경고", MessageBoxButtons.YesNoCancel);
                if (dialog == DialogResult.Yes)
                {
                    button1.PerformClick();
                }
                else if (dialog == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (dialog == DialogResult.No)
                {
                    e.Cancel = false;
                }
                //MessageBox.Show("저장하시겠습니까?"); //예,아니요,취소 부분 되게 
            }
        }
    }
}
