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
using System.Net;
using System.Net.Cache;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Script.Serialization;
using DBCOMMLib;


namespace LinGangProject
{
  
    public partial class Form1 : Form
    {
        public HttpWebRequest request;  //定义HTTP请求
        public JavaScriptSerializer jss;//定义Json数据序列化器
        //HTTP协议头部
        string baseStr2 = "http://192.168.0.10//api/v2.0.0/"; //agv 零件MIR_R672
        string baseStr = "http://192.168.0.20//api/v2.0.0/";//agv 装配MIR_R680
        string contentType = "application/json";  //HTTP协议头部字段
        string code = "Basic ZGlzdHJpYnV0b3I6NjJmMmYwZjFlZmYxMGQzMTUyYzk1ZjZmMDU5NjU3NmU0ODJiYjhlNDQ4MDY0MzNmNGNmOTI5NzkyODM0YjAxNA=="; //权限码
        string move0_id = "f3281854-6a62-11e8-bebc-94c6911adb1e";
        string move1_id = "1161a677-6a63-11e8-bebc-94c6911adb1e";
        string move2_id = "2c0a93a1-6a63-11e8-bebc-94c6911adb1e";
        string mission2_id = "02586b51-689b-11e8-b72c-94c6911adb1e"; //任务2guid号 go home
        string mission1_id = "1de5858f-663d-11e8-b296-94c6911adb1e"; //任务1guid号 go to workstation
        //mir672对齐4块VL板
        //string moveVL2_id = "42b8e9a8-8645-11e8-8efb-94c6911adb1e";
        //string moveVL4_id = "0b1a6731-8646-11e8-8efb-94c6911adb1e";
        string moveVL1_id = "be4f9c4e-8646-11e8-8efb-94c6911adb1e";
        string moveVL3_id = "f7cbc24c-8646-11e8-8efb-94c6911adb1e";
        //mir680对齐VL板
        string moveToVL2_id = "61bd786d-865d-11e8-873d-94c6911adbe1";
        string moveToVL4_id = "87ce9bf5-865d-11e8-873d-94c6911adbe1";
        string moveToWait_id = "1f32b149-8662-11e8-873d-94c6911adbe1";

        //string ip1 = "172.16.13.30";
        //string ip2 = "172.16.13.31";
        string ip2 = "192.168.0.10";
        string ip1 = "192.168.0.20";
        public static string position_x = "0.0";//存储X
        public static string position_y = "0.0";//存储Y
        public static string orientation = "0.0";//存储转角
        public static string linear_velocity = "0.0";//存储线速度
        public static string angular_velocity = "0.0";//存储角速度
        int ord_last = 0;//任务初始为0
        int ord2_last = 0;
        float reg1_last = 10;//寄存器1初始为0
        float reg2_last = 10;

        public Form1()
        {
            InitializeComponent();
            jss = new JavaScriptSerializer();//初始化Json数据序列化器

         
        }
        #region 定义HTTP协议GET请求函数
        private string HttpGet(string url, int timeout)
        {
            string retString=string.Empty;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType =contentType;
                request.Headers.Add(HttpRequestHeader.Authorization, code);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                
            }
            catch (Exception ex)
            { 
            }
            return retString;

        }
        #endregion
        #region 定义HTTP协议POST请求函数
        private string HttpPost(string url,string param)
        {
            string retString = string.Empty;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = contentType;
                request.Headers.Add(HttpRequestHeader.Authorization, code);
                byte[] data = Encoding.UTF8.GetBytes(param);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
            }
            catch(Exception ex) 
            {
 
            }
            return retString;
        }
        #endregion
        #region 定义HTTP协议PUT请求函数
        private string HttpPUT(string url, string param)
        {
            string retString = string.Empty;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PUT";
                request.ContentType = contentType;
                request.Headers.Add(HttpRequestHeader.Authorization, code);
                byte[] data = Encoding.UTF8.GetBytes(param);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
            }
            catch (Exception ex)
            {

            }
            return retString;
        }
        #endregion
        #region 寄存器操作
        //获取寄存器的值
        private float getRegisterValue(string register_id)
        {
            float value = 0;
            string url = baseStr + "registers/"+register_id;
            string result = HttpGet(url, 2000);
            RegisterJsonRecieve rjr=jss.Deserialize<RegisterJsonRecieve>(result);
            //value = int.Parse(rjr.value);
            value = rjr.value;
            return value;
        
        }

        private float getRegisterValue2(string register_id)
        {
            float value = 0;
            string url = baseStr2 + "registers/" + register_id;
            string result = HttpGet(url, 2000);
            RegisterJsonRecieve rjr = jss.Deserialize<RegisterJsonRecieve>(result);
            value = rjr.value;
            return value;

        }
        //为寄存器赋值
        private void setRegisterValue(string register_id,int value)
        {
            RegisterJsonSend rjs = new RegisterJsonSend();
            rjs.value = value;
            string strJson = jss.Serialize(rjs);
            string url = baseStr + "registers/"+register_id;
            string result = HttpPost(url, strJson);
       
        }
        #endregion
        #region  获取状态status信息
        private void getStatusInfo(string baseString)
        {
            string url = baseString + "status";
            string result = HttpGet(url, 2000);
            RootObject robj = jss.Deserialize<RootObject>(result);
            position_x = robj.position.x;//获取位置X
            position_y = robj.position.y;//获取位置Y
            orientation = robj.position.orientation;//获取转角
            linear_velocity = robj.velocity.linear;//获取线速度
            angular_velocity = robj.velocity.angular;//获取角速度
          
        }
        #endregion

        #region  设置为自动运行模式，要执行mission必须设置为该模式
        private void setAutoMode(string baseString)
        {
            AutoMotion am = new AutoMotion();
            //am.state_id = "3";//自动运行码
            am.state_id = 3;
            string strJson = jss.Serialize(am);
            string url = baseString + "status";
            string result = HttpPUT(url, strJson);
          
        }
        #endregion
        #region 下达任务Mission
        private void setMessionQueue(string baseString, string mission_id)
        {
            Mission ms = new Mission();
            ms.mission_id = mission_id;
            string strJson = jss.Serialize(ms);
            string url = baseString + "mission_queue";
            string result = HttpPost(url, strJson);
        }
        #endregion

        private void getButton_Click(object sender, EventArgs e)
        {
            //string url = baseStr + "status";
            //string result = HttpGet(url, 2000);
            //RootObject robj = jss.Deserialize<RootObject>(result);
            //textBox2.AppendText("x:"+robj.position.x+"\n");
            //textBox2.AppendText("y:"+robj.position.y+"\n");
            //textBox2.AppendText("linear-velocity:" + robj.velocity.linear + "\n");
            //textBox2.AppendText("angular-velocity:" + robj.velocity.angular + "\n");
            getStatusInfo(baseStr);
            textBox2.AppendText("The status of " + baseStr + "\n");
            textBox2.AppendText("x:" + position_x + "\n");
            textBox2.AppendText("y:" + position_y + "\n");
            textBox2.AppendText("alpha:" + orientation + "\n");
            textBox2.AppendText("linear-velocity:" + linear_velocity + "\n");
            textBox2.AppendText("angular-velocity:" + angular_velocity + "\n");
            float posx = Convert.ToSingle(position_x);
            float posy = Convert.ToSingle(position_y);
            axDBComm1.tagWriteFloat("posx1", posx);
            axDBComm1.tagWriteFloat("posy1", posy);
            float linear_v = Convert.ToSingle(linear_velocity);
            float angulat_v = Convert.ToSingle(angular_velocity);
            axDBComm1.tagWriteFloat("spe1", linear_v);
            axDBComm1.tagWriteFloat("wpe1", angulat_v);


            getStatusInfo(baseStr2);
            textBox2.AppendText("The status of " + baseStr2 + "\n");
            textBox2.AppendText("x:" + position_x + "\n");
            textBox2.AppendText("y:" + position_y + "\n");
            textBox2.AppendText("linear-velocity:" + linear_velocity + "\n");
            textBox2.AppendText("angular-velocity:" + angular_velocity + "\n");

            //float posx = Convert.ToSingle(robj.position.x);
            //float posy = Convert.ToSingle(robj.position.y);
            float posx2 = Convert.ToSingle(position_x);
            float posy2 = Convert.ToSingle(position_y);
            axDBComm1.tagWriteFloat("posx2", posx2);
            axDBComm1.tagWriteFloat("posy2", posy2);
            float linear_v2 = Convert.ToSingle(linear_velocity);
            float angulat_v2 = Convert.ToSingle(angular_velocity);
            axDBComm1.tagWriteFloat("spe2", linear_v2);
            axDBComm1.tagWriteFloat("wpe2", angulat_v2);
        }

        private void postButton_Click(object sender, EventArgs e)
        {
            RegisterJsonSend rjs = new RegisterJsonSend();
            rjs.value = 40;
            string strJson = jss.Serialize(rjs);
            string url = baseStr+"registers/1";
            string result = HttpPost(url, strJson);
            textBox2.Text = result;
        }

        private void putButton_Click(object sender, EventArgs e)
        {

        }

        private void setbutton_Click(object sender, EventArgs e)
        {
            float in1 = Convert.ToSingle(textBox1.Text);
            float in2 = Convert.ToSingle(textBox3.Text);
            float sum = in1 + in2;

            //textBox1.Text = "1";
            //textBox3.Text = "2";
            axDBComm1.tagWriteFloat("Input1", in1);
            axDBComm1.tagWriteFloat("Input2", in2);
            axDBComm1.tagWriteFloat("Output", sum);
              //m_DBCommCtrl.tagWriteFloat(_T("Output"),m_Output);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            axDBComm1.InitComm();
            textBox_ip.Text = ip1;
            textBox_ip2.Text = ip2;
        }

        private void axDBComm1_Enter(object sender, EventArgs e)
        {
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //string id1 = "1";
            //float value = getRegisterValue(id1);
            //textBox4.Text = value.ToString();
            //string strY = DateTime.Now.ToString();
            //axDBComm1.tagWriteMess("信息",strY);
            int ord_new = axDBComm1.tagReadInt("ord1");
            if (ord_new != ord_last)
            {
                if (ord_new == 2)
                {
                    setAutoMode(baseStr);
                    setMessionQueue(baseStr, moveToVL2_id);
                    ord_last = ord_new;
                }
                else if (ord_new == 4)
                {
                    setAutoMode(baseStr);
                    setMessionQueue(baseStr, moveToVL4_id);
                    ord_last = ord_new;
                }
                else if (ord_new == 5)
                {
                    setAutoMode(baseStr);
                    setMessionQueue(baseStr, moveToWait_id);
                    ord_last = ord_new;
                }
            }
            getStatusInfo(baseStr);
            float posx = Convert.ToSingle(position_x);
            float posy = Convert.ToSingle(position_y);
            axDBComm1.tagWriteFloat("posx1", posx);
            axDBComm1.tagWriteFloat("posy1", posy);
            float linear_v = Convert.ToSingle(linear_velocity);
            float angular_v = Convert.ToSingle(angular_velocity);
            axDBComm1.tagWriteFloat("spe1", linear_v);
            axDBComm1.tagWriteFloat("wpe1", angular_v);

            string id1 = "1";
            float reg1_new = getRegisterValue(id1);
            axDBComm1.tagWriteInt("signal1", Convert.ToInt16(reg1_new));
            //if (reg1_new != reg1_last)
            //{
            //    if (reg1_new == 0.0)
            //    {
            //        axDBComm1.tagWriteInt("signal1", 0);
            //    }
            //    else if (reg1_new == 1.0)
            //    {
            //        axDBComm1.tagWriteInt("signal1", 1);
            //    }
            //    else if (reg1_new == 2.0)
            //    {
            //        axDBComm1.tagWriteInt("signal1", 2);
            //    }
            //    else if (reg1_new == 3.0)
            //    {
            //        axDBComm1.tagWriteInt("signal1", 3);
            //    }
            //    else if (reg1_new == 4.0)
            //    {
            //        axDBComm1.tagWriteInt("signal1", 4);
            //    }
            //    else if (reg1_new == 5.0)
            //    {
            //        axDBComm1.tagWriteInt("signal1", 5);
            //    }
            //}
            textBox4.Text = reg1_new.ToString();
            //string strY = DateTime.Now.ToString();
            //axDBComm1.tagWriteMess("信息",strY);

            int ord2_new = axDBComm1.tagReadInt("ord2");
            if (ord2_new != ord2_last)
            {
                if (ord2_new == 1)
                {
                    setAutoMode(baseStr2);
                    setMessionQueue(baseStr2, moveVL1_id);
                    ord2_last = ord2_new;
                }
                else if (ord2_new == 3)
                {
                    setAutoMode(baseStr2);
                    setMessionQueue(baseStr2, moveVL3_id);
                    ord2_last = ord2_new;
                }
            }
            getStatusInfo(baseStr2);
            float posx2 = Convert.ToSingle(position_x);
            float posy2 = Convert.ToSingle(position_y);
            axDBComm1.tagWriteFloat("posx2", posx2);
            axDBComm1.tagWriteFloat("posy2", posy2);
            float linear_v2 = Convert.ToSingle(linear_velocity);
            float angular_v2 = Convert.ToSingle(angular_velocity);
            axDBComm1.tagWriteFloat("spe2", linear_v2);
            axDBComm1.tagWriteFloat("wpe2", angular_v2);

            string id2 = "1";
            float reg2_new = getRegisterValue2(id2);
            axDBComm1.tagWriteInt("signal2", Convert.ToInt16(reg2_new));

            //if (reg2_new != reg2_last)
            //{
            //    if (reg2_new == 0.0)
            //    {
            //        axDBComm1.tagWriteInt("signal2", 0);
            //    }
            //    else if (reg2_new == 1.0)
            //    {
            //        axDBComm1.tagWriteInt("signal2", 1);
            //    }
            //    else if (reg2_new == 2.0)
            //    {
            //        axDBComm1.tagWriteInt("signal2", 2);
            //    }
            //    else if (reg2_new == 3.0)
            //    {
            //        axDBComm1.tagWriteInt("signal2", 3);
            //    }
            //    else if (reg2_new == 4.0)
            //    {
            //        axDBComm1.tagWriteInt("signal2", 4);
            //    }
            //    else if (reg2_new == 5.0)
            //    {
            //        axDBComm1.tagWriteInt("signal2", 5);
            //    }
            //}
            textBox5.Text = reg2_new.ToString();
        }

        private void setMissionButton_Click(object sender, EventArgs e)
        {
            setAutoMode(baseStr);
            //setAutoMode(baseStr2);
            setMessionQueue(baseStr, mission1_id);

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                timer1.Enabled = true;
            else timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setAutoMode(baseStr);
            setMessionQueue(baseStr, mission2_id);
        }

        private void textBox_ip_TextChanged(object sender, EventArgs e)
        {

        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
    #region 任务mission类
    public class Mission
    {
        public string mission_id { get; set; }
    }
    #endregion
    #region  status对应的Json数据类
    public class AutoMotion 
    {
        //public string state_id { get; set; }
        public int state_id { get; set; }
    }
    public class Errors
    {
    }

    public class Position
    {
        public string orientation { get; set; }
        public string x { get; set; }
        public string y { get; set; }
    }

    public class Velocity
    {
        public string angular { get; set; }
        public string linear { get; set; }
    }

    public class RootObject
    {
        //public string allowed_methods { get; set; }
        public string battery_percentage { get; set; }
        public string battery_time_remaining { get; set; }
        public string distance_to_next_target { get; set; }
        public List<Errors> errors { get; set; }
        public string footprint { get; set; }
        public string map_id { get; set; }
        public string mission_queue_id { get; set; }
        public string mission_queue_url { get; set; }
        public string mission_text { get; set; }
        public string mode_id { get; set; }
        public string mode_text { get; set; }
        public string moved { get; set; }
        public Position position { get; set; }
        public string robot_model { get; set; }
        public string robot_name { get; set; }
        public string serial_number { get; set; }
        public string session_id { get; set; }
        public string state_id { get; set; }
        public string state_text { get; set; }
        public string unloaded_map_changes { get; set; }
        public string uptime { get; set; }
        public string user_prompt { get; set; }
        public Velocity velocity { get; set; }
    }
    #endregion
    #region 寄存器Json数据发送和接收类
    #region 发送类
    public class RegisterJsonSend
    {
        public int value { get; set; }
    }
    #endregion
    #region 接收类
    public class Allowed_methods
    {  
    }
    public class RegisterJsonRecieve
    {
        //public List<Allowed_methods> allowed_methods { get; set; }
        public string id { get; set; }
        public string label { get; set; }
        public string url { get; set; }
        public float value { get; set; }
    }
   #endregion
   #endregion
}