using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace proj_03;


public partial class HD2_Fixer : Form
{
    public HD2_Fixer()
    {
        InitializeComponent();

        //admin priv check
        if (isAdmin())
        {
            label1.Dispose();
        }
        else
        {
            label1.Text = "Please run the application as administrator. Some buttons have been disabled!";
            label1.ForeColor = Color.Red;
            BackColor = Color.Black;
            button1.Dispose();

            button3.Dispose();
            //button4.Dispose();
            button5.Dispose();
            //button6.Dispose();



        }
        label2.Text = "Game Located at:  " + location();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void toolTip1_Popup(object sender, PopupEventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        if (isAdmin())
        {
            ExecuteCommandAsAdmin("netsh winsock reset & netsh int ip reset all & netsh winhttp reset proxy & ipconfig /flushdns");
            MessageBox.Show("Completed ");
            button1.ForeColor = Color.Green;
        }
        else
        {
            MessageBox.Show("You can't run this command without admin rights");
            button1.ForeColor = Color.Red;
        }
        progressBar1.Visible = false;
        label3.Visible = false;
        richTextBox1.Visible = false;

    }

    private void button2_Click(object sender, EventArgs e)
    {
        Result result = RemoveAppdata();
        MessageBox.Show(result.Msg);
        button2.ForeColor = result.color;
        progressBar1.Visible = false;
        label3.Visible = false;
        richTextBox1.Visible = false;

    }

    private void button3_Click(object sender, EventArgs e)
    {
        
        if (isAdmin())
        {
            ExecuteCommandAsAdmin("reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\" /v \"TdrDelay\" /t REG_DWORD /d 6 & reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\" /v \"TdrDdiDelay\" /t REG_DWORD /d 6");
            MessageBox.Show("Completed ");
            button1.ForeColor = Color.Green;
        }
        else
        {
            MessageBox.Show("You can't run this command without admin rights");
            button1.ForeColor = Color.Red;
        }
        progressBar1.Visible = false;
        label3.Visible = false;
        richTextBox1.Visible = false;
    }
    private void button4_Click(object sender, EventArgs e)
    {
        Result paths = FixAntiCheat();
        ExecuteCommandAsAdmin('"'+paths.path+'"');
        ExecuteCommandAsAdmin('"' + paths.Msg + '"');
        MessageBox.Show("Done");
        progressBar1.Visible = false;
        label3.Visible = false;
        richTextBox1.Visible = false;
    }
    private void button5_Click(object sender, EventArgs e)
    {

        Downloader();
        Change();
        label3.Visible = true;
        label3.Text = "Please Wait.";
        progressBar1.Visible = true;
        Cursor = Cursors.WaitCursor;
        richTextBox1.Visible = false;

    }
    private void button6_Click(object sender, EventArgs e)
    {   
        richTextBox1.Visible = true;
        richTextBox1.Clear();
        progressBar1.Visible = false;
        label3.Visible = false;
        InstalledApp installedApp = new InstalledApp();
        getBadBoys(richTextBox1);
        label3.Visible = true;
        label3.Text = "Programs that can cause issues found:";
        


    }
}