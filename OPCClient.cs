using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPCAutomation;
using System.Net;
using System.Threading;

namespace LinGangProject
{
    public class OPCClient
    {
        public OPCServer kepSever;
        public OPCGroups kepGroups;
        public OPCGroup kepGroup;
        public OPCItems kepItems;
        public OPCItem kepItem;
        /// <summary>
        /// 客户端句柄
        /// </summary>
        int itmHandleClient = 0;
        /// <summary>
        /// 服务端句柄
        /// </summary>
        int itmHandleServer = 0;
        public object readValue;
        public List<string> serverNames = new List<string>();
        public List<string> tags = new List<string>();

        /// <summary>
        /// 自动取得本机IP
        /// </summary>
        private string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostByName(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }
        /// <summary>
        /// 枚举本地OPC SERVER
        /// </summary>
        public void getOPCServers()
        {

            //string hostname = Dns.GetHostName();
            //IPHostEntry localhost = Dns.GetHostByName(hostname);
            string hostName = Dns.GetHostName();
            string strHostIP="";
            IPHostEntry ipHostEntry = Dns.GetHostEntry(hostName);
            for (int i = 0; i < ipHostEntry.AddressList.Length; i++)  
            {
                   if (ipHostEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)            
                   {
                        strHostIP = ipHostEntry.AddressList[i].ToString();  //获取IP地址
                        break;
                   }
            }

            try
            {

                kepSever = new OPCServer();
                object serverList = kepSever.GetOPCServers(hostName);
                foreach (string serverName in (Array)serverList)
                {
                    serverNames.Add(serverName);
                }
            }
            catch (Exception ex){ 
            }

        }
        /// <summary>
        /// 连接OPC SERVER
        /// </summary>
        /// <param name="serverName">OPC SERVER名字</param>
        public void connectServer(string serverName)
        {
            try
            {
                kepSever.Connect(serverName, "");
                if (kepSever.ServerState != (int)OPCServerState.OPCRunning)//判断是否连接成功
                {
                    kepSever.Disconnect();
                    return;
                }
                createGroup("group1");
                createItems();

            }
            catch (Exception ex)
            {
                kepSever.Disconnect();
            }
        }
        /// <summary>
        /// 创建组,组名无所谓
        /// </summary>
        private void createGroup(string groupName)
        {
            try
            {
                kepGroups = kepSever.OPCGroups;
                kepGroup = kepGroups.Add(groupName);
                //设置属性
                kepSever.OPCGroups.DefaultGroupIsActive = true;//激活组
                kepSever.OPCGroups.DefaultGroupDeadband = 0; //死区值，设为0，服务端组内任何数据变化都通知组
                kepGroup.UpdateRate = 250;   //默认组群的刷新频率为250ms
                kepGroup.IsActive = true;
                kepGroup.IsSubscribed = true; //使用订阅功能，即可以同步，默认false，如果没有订阅，回调事件DataChange不会发生
            }
            catch (Exception ex)
            { }
        }
        /// <summary>
        /// 添加组内的Items
        /// </summary>
        private void createItems()
        {
            kepItems=kepGroup.OPCItems;
            kepGroup.DataChange+=new DIOPCGroupEvent_DataChangeEventHandler(KepGroup_DataChange);
            AddOpcItem();
        }
        /// <summary>
        /// 把需要采集数据的点加进去
        /// </summary>
        private void AddOpcItem()
        {
            kepItems.AddItem("a1.22.1", 1);
            kepItems.AddItem("a2.22.2", 2);
            kepItems.AddItem("a3.22.3", 3);
            OPCItem BI = kepItems.Item(1);
        }
        /// <summary>  
        /// 每当项数据有变化时执行的事件  
        /// </summary>  
        /// <param name="TransactionID">处理ID</param>  
        /// <param name="NumItems">项个数</param>  
        /// <param name="ClientHandles">项客户端句柄</param>  
        /// <param name="ItemValues">TAG值</param>  
        /// <param name="Qualities">品质</param>  
        /// <param name="TimeStamps">时间戳</param>  
         private void KepGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            for (int i = 1; i <= NumItems; i++)
            {
                readValue = ItemValues.GetValue(i).ToString();
            }
        }

         private void GetTagValue(string tagName)
         {
             try
             {
                 readValue = "";
                 if (itmHandleClient != 0)
                 {
                     Array Errors;
                     OPCItem bItem = kepItems.GetOPCItem(itmHandleServer);
                     //注：OPC中以1为数组的基数
                     int[] temp = new int[2] { 0, bItem.ServerHandle };
                     Array serverHandle = (Array)temp;
                     //移除上一次选择的项
                     kepItems.Remove(kepItems.Count, ref serverHandle, out Errors);
                 }
                 itmHandleClient = 12345;
                 kepItem = kepItems.AddItem(tagName, itmHandleClient);
                 itmHandleServer = kepItem.ServerHandle;
             }
             catch (Exception err)
             {
                 //没有任何权限的项，都是OPC服务器保留的系统项，此处可不做处理。
                 itmHandleClient = 0;
                 Console.WriteLine("Read value error:" + err.Message);
             }
         }

         public void WriteValue(string tagName, object _value)
         {
             GetTagValue(tagName);
             OPCItem bItem = kepItems.GetOPCItem(itmHandleServer);
             int[] temp = new int[2] { 0, bItem.ServerHandle };
             Array serverHandles = (Array)temp;
             object[] valueTemp = new object[2] { "", _value };
             Array values = (Array)valueTemp;
             Array Errors;
             int cancelID;
             kepGroup.AsyncWrite(1, ref serverHandles, ref values, out Errors, 2009, out cancelID);
             //KepItem.Write(txtWriteTagValue.Text);//这句也可以写入，但并不触发写入事件
             GC.Collect();
         }

         public object ReadValue(string tagName)
         {
             GetTagValue(tagName);
             Thread.Sleep(500);
             try
             {
                 return kepItem.Value;
             }
             catch
             {
                 return null;
             }
         }

         public void ReadValue(string tagName, bool wtf)
         {
             GetTagValue(tagName);
             OPCItem bItem = kepItems.GetOPCItem(itmHandleServer);
             int[] temp = new int[2] { 0, bItem.ServerHandle };
             Array serverHandles = (Array)temp;
             Array Errors;
             int cancel;
             kepGroup.AsyncRead(1, ref serverHandles, out Errors, 2009, out cancel);
             GC.Collect();
         }
         public void closeConnect()
         {
             kepSever.OPCGroups.RemoveAll();
             kepSever.Disconnect();
         }
    }
}
