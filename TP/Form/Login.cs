﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace TP
{
    public partial class Login : Form
    {
        Main main;
        TextBox[] txtList;
        private const string IdPlaceholder = "아이디";
        private const string PwPlaceholder = "비밀번호";
        private string id;
        private string pw;
        LoginController loginController;

        public Login(Main main)
        {
            InitializeComponent();
            this.main = main;
            txtList = new TextBox[] { textBox1, textBox2 };
            foreach (var txt in txtList)
            {
                txt.ForeColor = Color.DarkGray;
                if (txt == textBox1)
                    txt.Text = IdPlaceholder;
                else if (txt == textBox2)
                    txt.Text = PwPlaceholder;
                txt.GotFocus += RemovePlaceholder;
                txt.LostFocus += SetPlaceholder;
            }
            loginController = new LoginController();
        }


        private void RemovePlaceholder(object sender, EventArgs e) //비밀번호 입력시 정보보호를 위해 ●으로 표시
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text == IdPlaceholder | txt.Text == PwPlaceholder)
            {
                txt.ForeColor = Color.Black;
                txt.Text = String.Empty;
                if (txt == textBox2)
                    textBox2.PasswordChar = '●';
            }
        }

        private void SetPlaceholder(object sender, EventArgs e) //입력이 없을때 아이디와 비밀번호를 회색으로 텍스트박스에 표시 
        {
            TextBox txt = (TextBox)sender;
            if (string.IsNullOrEmpty(txt.Text))
            {
                txt.ForeColor = Color.DarkGray;
                if (txt == textBox1)
                    txt.Text = IdPlaceholder;
                else if (txt == textBox2)
                {
                    txt.Text = PwPlaceholder;
                    textBox2.PasswordChar = default;
                }
            }
        }

        private void Login_Click(object sender, EventArgs e) //로그인 클릭
        {

            id = textBox1.Text;
            pw = textBox2.Text;

            if (id == IdPlaceholder || pw == PwPlaceholder)
            {
                MessageBox.Show("ID 또는 Password를 입력하세요.");
            }
            else
            {
                if (loginController.checkUser(id, pw))
                {
                    if (checkBox1.Checked == true) //아이디 저장할지 여부 
                    {
                        Properties.Settings.Default.LoginIDSave = id; //저장시 세팅값에 저장됨
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Properties.Settings.Default.LoginIDSave = IdPlaceholder;
                        Properties.Settings.Default.Save();
                    }
                    MessageBox.Show("로그인에 성공했습니다.", "로그인 성공");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("잘못된 아이디 또는 비밀번호 입니다.", "로그인 실패");
                }
            }
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            id = textBox1.Text;
            pw = textBox2.Text;
            if (!loginController.checkUser(id, pw)) //로그인 성공하면 그냥 메인문도 닫히기 때문에 로그인 성공하지 못한상태에서 닫을시 메인문도 닫힘
                main.Close();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.LoginIDSave != IdPlaceholder)
            {
                checkBox1.Checked = true;
                textBox1.Text = Properties.Settings.Default.LoginIDSave;
                textBox1.ForeColor = Color.Black;
            }
        }
    }
}
