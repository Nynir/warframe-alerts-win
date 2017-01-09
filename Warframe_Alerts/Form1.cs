using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Warframe_Alerts
{
    public partial class Main_Window : Form
    {
        List<string> ID_List = new List<string>();

        int U_Interval = 1 * 60 * 1000;
        bool Start_Minimized = false;

        bool F_Mods = true;
        bool F_Resources = true;
        bool F_Credits = true;
        bool F_Blueprints = true;

        System.Windows.Forms.Timer Update_Timer = new System.Windows.Forms.Timer();
        bool Phase_Shift = false;

        public Main_Window()
        {
            InitializeComponent();
            Apply_Settings();
            WF_Update();

            Update_Timer.Interval = U_Interval;
            Update_Timer.Tick += new EventHandler(Update_Click);
            Update_Timer.Start();
        }

        public void Apply_Settings()
        {
            if (!File.Exists("Config.xml"))
            {
                XDocument NewDoc = new XDocument(
                    new XElement("body",

                        new XElement("LoadMinimized", "0")


                    )
                );

                NewDoc.Save("Config.xml");
                return;
            }

            XDocument Doc = XDocument.Load("Config.xml");

            if (Doc.Element("body").Element("LoadMinimized").Value == "1")
            {
                Start_Minimized = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                buttonSM.Text = "Disable Start Minimized";
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            //Notify_Icon.BalloonTipText = "TEST MESSAGE";
            //Notify_Icon.BalloonTipTitle = "TEST TITLE";
            //Notify_Icon.ShowBalloonTip(1000);
            WF_Update();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Setting_Click(object sender, EventArgs e)
        {
            Secondary_Form SF = new Secondary_Form(this,F_Resources,F_Mods,F_Credits,F_Blueprints);
            SF.ShowDialog();
        }

        public void WF_Update()
        {
            Warframe_Handler WF = new Warframe_Handler();

            List<Alert> Alerts = new List<Alert>();
            List<Invasion> Invasions = new List<Invasion>();
            List<Outbreak> Outbreaks = new List<Outbreak>();

            string Status = "";
            string Response = WF.GetXML(ref Status);

            if (Status != "OK")
            {
                string message = "Network not responding" + '\n';
                message = message + Response;

                Notify_Icon.BalloonTipText = message;
                Notify_Icon.BalloonTipTitle = "Update Failed";
                Notify_Icon.ShowBalloonTip(2000);
                return;
            }

            WF.GetObjects(Response, ref Alerts, ref Invasions, ref Outbreaks);

            Notify_Alerts_And_Invasions(ref Alerts, ref Invasions, ref Outbreaks);

            AlertData.Rows.Clear();
            InvasionData.Rows.Clear();
            ID_List.Clear();

            for (int i = 0; i < Alerts.Count; i++)
            {
                DateTime S_Time = Convert.ToDateTime(Alerts[i].Start_Date);
                DateTime E_Time = Convert.ToDateTime(Alerts[i].Expiry_Date);

                string Title = Alerts[i].Title;
                string Description = Alerts[i].Description;
                string Faction = Alerts[i].Faction;
                string A_ID = Alerts[i].ID;

                TimeSpan A_Span = E_Time.Subtract(DateTime.Now);
                string A_Left = "";
                
                if (A_Span.Days != 0)
                {
                    A_Left = A_Left + A_Span.Days.ToString() + " Days ";
                }

                if (A_Span.Hours != 0)
                {
                    A_Left = A_Left + A_Span.Hours.ToString() + " Hours ";
                }

                if (A_Span.Minutes != 0)
                {
                    A_Left = A_Left + A_Span.Minutes.ToString() + " Minutes ";
                }

                A_Left = A_Left + A_Span.Seconds.ToString() + " Seconds Left";

                ID_List.Add(A_ID);
                AlertData.Rows.Add(Description, Title, Faction, A_Left);
                //AlertData.Rows.Add(Description, Title, Faction, Time_Left + " min left");
            }

            for (int i = 0; i < Invasions.Count; i++)
            {
                string Title = Invasions[i].Title;
                string I_ID = Invasions[i].ID;

                DateTime S_Time = Convert.ToDateTime(Invasions[i].Start_Date);
                DateTime T_Now = DateTime.Now;
                TimeSpan I_Span = T_Now.Subtract(S_Time);

                string I_Time = "";

                if (I_Span.Hours != 0)
                {
                    I_Time = I_Time + I_Span.Hours.ToString() + " Hours ";
                }

                I_Time = I_Time + I_Span.Minutes.ToString() + " Minutes Ago";

                ID_List.Add(I_ID);
                InvasionData.Rows.Add(Title, "Invasion", I_Time);
                //InvasionData.Rows.Add(Title, "Invasion", S_Time.ToString());
            }

            for (int i = 0; i < Outbreaks.Count; i++)
            {
                string Title = Outbreaks[i].Title;
                string O_ID = Outbreaks[i].ID;

                DateTime S_Time = Convert.ToDateTime(Outbreaks[i].Start_Date);
                DateTime T_Now = DateTime.Now;
                TimeSpan O_Span = T_Now.Subtract(S_Time);

                string O_Time = "";

                if (O_Span.Hours != 0)
                {
                    O_Time = O_Time + O_Span.Hours.ToString() + " Hours ";
                }

                O_Time = O_Time + O_Span.Minutes.ToString() + " Minutes Ago";

                ID_List.Add(O_ID);
                InvasionData.Rows.Add(Title, "Outbreak", O_Time);
            }
        }

        public void Notify_Alerts_And_Invasions(ref List<Alert> A, ref List<Invasion> I, ref List<Outbreak> O)
        {
            string Notification_Message = "";

            for (int i = 0; i < A.Count; i++)
            {
                bool Found = false;

                for (int j = 0; j < ID_List.Count && !Found; j++)
                {
                    if (A[i].ID == ID_List[j])
                    {
                        Found = true;
                    }
                }

                if (!Found)
                {
                    Notification_Message = Notification_Message + A[i].Title + '\n';
                }
            }

            for (int i = 0; i < I.Count; i++)
            {
                bool Found = false;

                for (int j = 0; j < ID_List.Count && !Found; j++)
                {
                    if (I[i].ID == ID_List[j])
                    {
                        Found = true;
                    }
                }

                if (!Found)
                {
                    Notification_Message = Notification_Message + I[i].Title + '\n';
                }
            }

            for (int i = 0; i < O.Count; i++)
            {
                bool Found = false;

                for (int j = 0; j < ID_List.Count && !Found; j++)
                {
                    if (O[i].ID == ID_List[j])
                    {
                        Found = true;
                    }
                }

                if (!Found)
                {
                    Notification_Message = Notification_Message + O[i].Title + '\n';
                }
            }

            if (Notification_Message != "")
            {
                Notify_Icon.BalloonTipText = Notification_Message;
                Notify_Icon.BalloonTipTitle = "Update";
                Notify_Icon.ShowBalloonTip(2000);
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Resize_Action(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && !Phase_Shift)
            {
                this.Hide();
                Notify_Icon.BalloonTipText = "Warframe_Alerts is running in background";
                Notify_Icon.BalloonTipTitle = "Update";
                Notify_Icon.ShowBalloonTip(2000);
            }
        }

        private void Notification_Icon_Double_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Phase_Shift = true;
                this.ShowInTaskbar = true;
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                Phase_Shift = false;
            }
        }

        private void buttonSM_Click(object sender, EventArgs e)
        {
            if (Start_Minimized)
            {
                string message = "Start minimized has been disabled";
                string caption = "Success";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show(message, caption, buttons);

                Start_Minimized = false;

                XDocument Doc = XDocument.Load("Config.xml");
                Doc.Element("body").Element("LoadMinimized").Value = "0";
                Doc.Save("Config.xml");
            }
            else
            {
                string message = "Start minimized has been enabled";
                string caption = "Success";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show(message, caption, buttons);

                Start_Minimized = true;

                XDocument Doc = XDocument.Load("Config.xml");
                Doc.Element("body").Element("LoadMinimized").Value = "1";
                Doc.Save("Config.xml");
            }
        }

        public int Update_Interval
        {
            get
            {
                return U_Interval;
            }
            set
            {
                U_Interval = value;
                Update_Timer.Interval = U_Interval;
            }
        }

        public bool Resource_Filter
        {
            get
            {
                return F_Resources;
            }
            set
            {
                F_Resources = value;
            }
        }

        public bool Credit_Filter
        {
            get
            {
                return F_Credits;
            }
            set
            {
                F_Credits = value;
            }
        }

        public bool Mod_Filter
        {
            get
            {
                return F_Mods;
            }
            set
            {
                F_Mods = value;
            }
        }

        public bool Blueprint_Filter
        {
            get
            {
                return F_Blueprints;
            }
            set
            {
                F_Blueprints = value;
            }
        }
    }
}
