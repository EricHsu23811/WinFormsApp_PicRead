/*****************************************************************************
 **
 **  (c) All Rights Reserved.
 **
 ** 2025-09-15, 版本1.0.0.0： WinFormsApp_PicRead，讀取圖檔，使用 Tesseract OCR 引擎 進行文字辨識
 **
 **
******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;    //使用 Tesseract OCR 引擎 進行文字辨識
using TesseractOCR;

namespace WinFormsApp_PicRead
{
    public partial class Form1 : Form
    {
        private Bitmap _selectedImage;  // 用於存儲選擇的圖片
        
        public Form1()
        {
            InitializeComponent();
        }

        /*******************************************************************************
         * Constant 
         ******************************************************************************/

        /* These are constant variables */
        public static readonly string userDocFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /*******************************************************************************
         * Gobal Variable 
         ******************************************************************************/
        static string userAppFolder = userDocFolder + "\\pxToRGB";
        // 可選：獲取完整路徑
        string executablePath = Assembly.GetExecutingAssembly().Location;
        // 獲取目前執行檔的名稱
        static string fileName = Assembly.GetExecutingAssembly().GetName().Name;
        //獲取執行檔版本
        string fileVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string buildDateTime = System.IO.File.GetCreationTime(Assembly.GetExecutingAssembly().Location).ToString("yyyy-MM-dd HH:mm");    //ToString("yyyy-MM-dd HH:mm:ss")

        string strFilePath = "";    //userAppFolder + "\\" + "H4_IQC_LBS_BURN.csv";
        static string strDailyLogDate = DateTime.Now.ToString("yyyy-MM-dd");
        static string strFileDailyFolder = userAppFolder + "\\" + DateTime.Now.ToString("yyyy")
            + "\\" + DateTime.Now.ToString("MM") + "\\" + DateTime.Now.ToString("dd");
        static string strLogNameDaily = ""; //strFileDailyFolder + "\\" + "H4_IQC_LBS_BURN" + "_" + strDailyLogDate + ".csv";
        string strFileDailyPath = "";   //userAppFolder + "\\" + strLogNameDaily;    //@"\Burn1to10.csv";
        string strSwVer = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();

        Boolean bRemoveTabpge2 = true; //預設顯示tabPage1

        private void Form1_Load(object sender, EventArgs e)
        {
            txtMac.Text = GetMacAddress().ToString();
            lblSWver.Text = /*"SW : " +*/ fileName + " - ver: " + strSwVer + " ; 程式碼日期 : " + buildDateTime;

            if (bRemoveTabpge2) //預設不顯示tabPage2，2025-09-08
            {
                tabControl1.TabPages.Remove(tabPage2);
            }
            btnImgRead.Enabled = false;

            toolStripStatusLabel1.Text = "按 Select image 鈕選擇檔案";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            utilHelpAbout();
        }
        private void utilHelpAbout()
        {
            string company = "Test App " + fileName + " Ver:" + fileVersion + "\n";
            string copyright = "(C)Copyright Eric Hsu 2025-2035.\n";
            string buildDT = "Built on " + buildDateTime + "\n";
            string Author = "Author: Eric Hsu\n";
            MessageBox.Show(company + copyright + buildDT + Author, "About " + fileName);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public static string GetMacAddress()
        {
            string macAddresses = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)  //2023-03-21
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
                else if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            macAddresses = AddSpaceEveryNChar(macAddresses, 2); //2025-06-19
            return macAddresses;
        }
        private void utilCheckUserFile()
        {
            if (Directory.Exists(userAppFolder) == false)
            { Directory.CreateDirectory(userAppFolder); }
            //if (Directory.Exists(strFileDailyFolder) == false)
            //{ Directory.CreateDirectory(strFileDailyFolder); }
        }
        public bool IsFileLocked(string filename)
        {
            bool Locked = false;
            try
            {
                FileStream fs =
                    File.Open(filename, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                Locked = true;
            }
            return Locked;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Environment.Exit(Environment.ExitCode);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure to exit？", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            { }
            else { e.Cancel = true; }
        }

        static string AddSpaceEveryNChar(string str, int split)   //2023-08-11
        {
            for (int a = 2; a <= str.Length - 1; a = a + split + 1)
            {
                str = str.Insert(a, "-");
            }
            Console.WriteLine(str);
            return str;
        }

        private void btnImgSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select A File";
            openDialog.Filter = "Image Files (*.png;*.jpg;*.bmp;*.gif)|*.png;*.jpg;*.bmp;*.gif" + "|" + "All Files (*.*)|*.*";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openDialog.FileName;
                FilePath1.Text = file;
                toolStripStatusLabel1.Text = "InputFile= " + FilePath1.Text;
                try
                {
                    _selectedImage = new Bitmap(openDialog.FileName);
                    pictureBox1.Image = _selectedImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image: " + ex.Message);
                }
            }
            if (FilePath1.Text != "") { btnImgRead.Enabled = true; } 
            else { btnImgRead.Enabled = false; }

            toolStripStatusLabel1.Text = "可按下 Read image 按鈕以產出結果";
        }

        private void btnImgRead_Click(object sender, EventArgs e)
        {
            if (_selectedImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default)) // 確保 tessdata 資料夾存在，並包含 "eng.traineddata"
                {
                    // 建立一個 TesseractEngine 實例，指定 tessdata 資料夾的路徑和語言
                    // "eng" 代表英文。 你可以根據需要更改語言 (例如 "chi_tra" 代表繁體中文)
                    // 請確保你的 tessdata 資料夾中包含對應的語言檔 (.traineddata)
                    var page = engine.Process(_selectedImage);
                    string text = page.GetText();
                    lblOcrResult.Text = text;
                    lblOcrResult.Font = new Font("Arial", 16); // 設定字型和大小
                    lblOcrResult.ForeColor = Color.Blue; // 設定字體顏色
                    txtOcrResult.Text = text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error recognizing text: " + ex.Message);
            }
        }
    }
}
