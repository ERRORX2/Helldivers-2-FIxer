using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using System.Linq.Expressions;
using System.Net;
using System.ComponentModel;
using System.Drawing.Text;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Eventing.Reader;

namespace proj_03;
public class Result
{
    public static int iter = 0;
    public static int Value = 0;
    public string Msg {get; set;}
    public Color color {get; set;}  
    public string path { get; set;}
}

partial class HD2_Fixer
{
    //
    public static async void Downloader()
    {
       
        List<string> urls = new List<string>
            {
                "https://download.microsoft.com/download/1/6/B/16B06F60-3B20-4FF2-B699-5E9B7962F9AE/VSU_4/vcredist_x64.exe",
                "https://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x64.exe",
                "https://aka.ms/vs/17/release/vc_redist.x64.exe",
                // Add more URLs as needed
            };

        await Parallel.ForEachAsync(urls, async (url, token) =>
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, token))
                {
                    response.EnsureSuccessStatusCode();

                    string fileName = Path.GetFileName(url);
                    Random random = new Random();
                    string result = "";
                    for (int i = 0; i < 10; i++) {
                        result += random.Next(10).ToString();
                    }
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string uniqueFileName = $"{fileName}_{timestamp + (result)}";
                    

                    // Get the temporary folder path
                    string tempFolderPath = Path.GetTempPath();

                    // Combine the temp path and filename
                    string filePath = Path.Combine(tempFolderPath, uniqueFileName + ".exe");

                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (FileStream fileStream = File.Create(filePath))
                        {
                            await responseStream.CopyToAsync(fileStream);
                        }
                    }
                    
                    Result.iter += 1;
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = filePath;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    Process.Start(startInfo);
                    

                }
            }
        });
    }
   //Easiest way is to fake it
    public void Change()
    {
        updateTimer = new System.Windows.Forms.Timer();
        updateTimer.Interval = 200;
        updateTimer.Tick += UpdateProgressBar;
        updateTimer.Start();
       
    }
    private void UpdateProgressBar(object sender, EventArgs e) 
    {
        
       
        if (Result.Value >= 70) { 
        updateTimer.Stop();
            Cursor = Cursors.Default;
            label3.Text = "Completed. Please install or Repair";
            progressBar1.Value = 100;
            Result.Value = 0;
        }
        if (Result.iter < 3)
        {
            Result.Value = 33 * Result.iter;
            progressBar1.Value = Result.Value;  
        }

    }

    //
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
    
    private void ExecuteCommandAsAdmin(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.Verb = "runas";
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C " + command;
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    public static string location()
    {
        string GameID = "553850";
        string SteamPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null);
        if (string.IsNullOrEmpty(SteamPath))
        {
            MessageBox.Show("Steam path not found.");
            return null;
        }
        string HD2_LOCATION = ("");

        string libraryDataPath = Path.Combine(SteamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryDataPath))
        {
            MessageBox.Show("Library folders file not found.");
            return null;
        }

        string[] libraryDataLines = File.ReadAllLines(libraryDataPath);

        foreach (string line in libraryDataLines)
        {
            if (Regex.IsMatch(line, @"path"))
            {
                HD2_LOCATION = line.Split('"')[3].Replace("\\\\","\\");
            }
            

            if (Regex.IsMatch(line, $"\"{GameID}\""))
            {
                string gameDataPath = Path.Combine(HD2_LOCATION, "steamapps", $"appmanifest_{GameID}.acf");
                string[] gameDataLines = File.ReadAllLines(gameDataPath);
                string gameFolderName = gameDataLines[7].Split('"')[3];
                return Path.Combine(HD2_LOCATION, "steamapps", "common", gameFolderName);
                
            }
            
        }
        return null;
    }

    public Result RemoveAppdata() {
 
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Arrowhead\\Helldivers2\\";
        if (Directory.Exists(folderPath))
        {
            try
            {
                Directory.Delete(folderPath, true);
                return new Result { Msg = "Folder deleted successfully.", color = Color.Green};
                
               
            }
            catch (IOException e)
            {
                return new Result { Msg = ("Error deleting folder: " + e.Message), color = Color.Yellow};
                
            }
        }
        else
        {
            return new Result {Msg = "Folder not found or already deleted.",color= Color.Red };
            
        }
    }
    public Result FixAntiCheat()
    {
        string antiCheatLocation = Path.Combine(location() + "\\tools");
        if (Directory.Exists(antiCheatLocation))
        {
            try
            {
                string antiCheatUnins = Path.Combine(antiCheatLocation + "\\gguninst.exe");
                string antiCheatInst = Path.Combine(antiCheatLocation + "\\GGSetup.exe");
                if (!File.Exists(antiCheatUnins))
                {
                    return new Result {path = antiCheatUnins, Msg = "Uninstaller not Found", color = Color.Red };
                }
                    if (!File.Exists(antiCheatInst))
                    {
                        return new Result { Msg = "Installer not Found", color = Color.Red };
                    }

                    return new Result {path = antiCheatUnins ,Msg = antiCheatInst, color = Color.Green};
                
            }
            catch (IOException e)
            {
                return new Result { Msg = ("Error" + e.Message), color = Color.Yellow };

            }
        }
        else
        {
            return new Result {path = antiCheatLocation ,Msg = "Folder not found", color = Color.Red };
        }
    } 

    public static bool isAdmin()
    {
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new Container();
        ComponentResourceManager resources = new ComponentResourceManager(typeof(HD2_Fixer));
        button1 = new Button();
        button2 = new Button();
        button3 = new Button();
        button4 = new Button();
        button5 = new Button();
        button6 = new Button();
        toolTip1 = new ToolTip(components);
        label1 = new Label();
        label2 = new Label();
        progressBar1 = new ProgressBar();
        label3 = new Label();
        SuspendLayout();
        // 
        // button1
        // 
        button1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button1.Location = new Point(32, 221);
        button1.Name = "button1";
        button1.Size = new Size(107, 43);
        button1.TabIndex = 0;
        button1.Text = "Network Reset";
        toolTip1.SetToolTip(button1, "Resets Network by clearing DNS and some extras");
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // button2
        // 
        button2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button2.Font = new Font("Segoe UI", 9F);
        button2.Location = new Point(152, 221);
        button2.Name = "button2";
        button2.Size = new Size(107, 43);
        button2.TabIndex = 1;
        button2.Text = "Remove Appdata";
        toolTip1.SetToolTip(button2, "Removes User_Settings and Shader Cache");
        button2.UseVisualStyleBackColor = true;
        button2.Click += button2_Click;
        // 
        // button3
        // 
        button3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button3.Location = new Point(272, 221);
        button3.Name = "button3";
        button3.Size = new Size(107, 43);
        button3.TabIndex = 2;
        button3.Text = "TDR FIX";
        toolTip1.SetToolTip(button3, "Adds a TDR delay of 6sec to prevent driver crashes");
        button3.UseVisualStyleBackColor = true;
        button3.Click += button3_Click;
        // 
        // button4
        // 
        button4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button4.Location = new Point(392, 221);
        button4.Name = "button4";
        button4.Size = new Size(107, 43);
        button4.TabIndex = 3;
        button4.Text = "Reinstall GameGuard";
        toolTip1.SetToolTip(button4, "Reinstall the Anti-Cheat");
        button4.UseVisualStyleBackColor = true;
        button4.Click += button4_Click;
        // 
        // button5
        // 
        button5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button5.Location = new Point(512, 221);
        button5.Name = "button5";
        button5.Size = new Size(107, 43);
        button5.TabIndex = 4;
        button5.Text = "VC++ Download";
        toolTip1.SetToolTip(button5, "Downloads VC++ and will ask you how to proceed");
        button5.UseVisualStyleBackColor = true;
        button5.Click += button5_Click;
        // 
        // button6
        // 
        button6.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button6.Location = new Point(632, 221);
        button6.Name = "button6";
        button6.Size = new Size(107, 43);
        button6.TabIndex = 5;
        button6.Text = "Close";
        toolTip1.SetToolTip(button6, "Closes your room door");
        button6.UseVisualStyleBackColor = true;
        button6.Click += button6_Click;
        // 
        // toolTip1
        // 
        toolTip1.Tag = "123";
        toolTip1.ToolTipTitle = "What this does is: ";
        toolTip1.Popup += toolTip1_Popup;
        // 
        // label1
        // 
        label1.AccessibleRole = AccessibleRole.Alert;
        label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label1.AutoEllipsis = true;
        label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
        label1.ForeColor = Color.Red;
        label1.Location = new Point(120, 67);
        label1.MaximumSize = new Size(500, 200);
        label1.Name = "label1";
        label1.Size = new Size(482, 94);
        label1.TabIndex = 6;
        label1.Text = "label1";
        // 
        // label2
        // 
        label2.AccessibleRole = AccessibleRole.Alert;
        label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label2.ForeColor = Color.Green;
        label2.Location = new Point(32, 267);
        label2.Name = "label2";
        label2.Size = new Size(52, 21);
        label2.TabIndex = 7;
        label2.Text = "label2";
        // 
        // progressBar1
        // 
        progressBar1.ForeColor = Color.Red;
        progressBar1.Location = new Point(32, 12);
        progressBar1.Name = "progressBar1";
        progressBar1.RightToLeftLayout = true;
        progressBar1.Size = new Size(707, 23);
        progressBar1.TabIndex = 8;
        progressBar1.Visible = false;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.ForeColor = Color.ForestGreen;
        label3.Location = new Point(32, 38);
        label3.Name = "label3";
        label3.Size = new Size(38, 15);
        label3.TabIndex = 9;
        label3.Text = "label3";
        label3.Visible = false;
        // 
        // HD2_Fixer
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = SystemColors.Control;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(769, 293);
        Controls.Add(label3);
        Controls.Add(progressBar1);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(button6);
        Controls.Add(button5);
        Controls.Add(button4);
        Controls.Add(button3);
        Controls.Add(button2);
        Controls.Add(button1);
        DoubleBuffered = true;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MdiChildrenMinimizedAnchorBottom = false;
        MinimizeBox = false;
        Name = "HD2_Fixer";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "HD2 Fixer";
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion

    private Button button1;
    private Button button2;
    private Button button3;
    private Button button4;
    private Button button5;
    private Button button6;
    private ToolTip toolTip1;
    private Label label1;
    private Label label2;
    private ProgressBar progressBar1;
    private WebClient webClient = new WebClient();
    public Label label3;
    private System.Windows.Forms.Timer updateTimer;
}
