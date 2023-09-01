using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace machineFilesInfo
{
    public partial class Service1 : ServiceBase
    {
        private readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly List<FileInformation> ProvenMachineProgramList = new List<FileInformation>();
        private readonly List<FileInformation> StandardSoftwareProgramList = new List<FileInformation>();
        private List<TimeSpan> shiftDetails = new List<TimeSpan>();
        private readonly List<FileInformation> dblist = new List<FileInformation>();
        private readonly string synctype = ConfigurationManager.AppSettings["syncType"];
        private readonly Thread StartFunctionThread = null;
        private DateTime Target = DateTime.Now.AddHours(-1);
        private bool running;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            running = true;
            if (!Directory.Exists(appPath + "\\Logs\\"))
            {
                _ = Directory.CreateDirectory(appPath + "\\Logs\\");
            }

            Thread.CurrentThread.Name = "Main";
            Thread StartFunctionThread = new Thread(new ThreadStart(StartFunction))
            {
                Name = "FileDataBaseInfo"
            };
            StartFunctionThread.Start();
        }

        private void StartFunction()
        {
            try
            {
                if (synctype.Equals("shiftend", StringComparison.OrdinalIgnoreCase))
                {
                    shiftDetails = fileDataBaseAccess.GetShiftDetails();
                    shiftDetails.Sort();
                }
                else
                {
                    TimeSpan startTime = DateTime.Parse(ConfigurationManager.AppSettings["startTime"]).TimeOfDay;
                    shiftDetails.Add(startTime);
                }

                while (running)
                {
                    try
                    {
                        if (DateTime.Now >= Target)
                        {
                            Logger.WriteDebugLog("Process Started at" + DateTime.Now);
                            setAndGetFileInfo();
                            Logger.WriteDebugLog("Process Ended at" + DateTime.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteErrorLog(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
        }

        private void GetProvenFiles(string path)
        {
            try
            {
                foreach (string subDirectory in Directory.GetDirectories(path, "Proven Machine Program", SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.GetFiles(subDirectory))
                    {
                        FileInformation localFile = new FileInformation();
                        FileInfo fileInfo = new FileInfo(file);
                        localFile.FileName = fileInfo.Name;
                        localFile.FileType = fileInfo.Extension;
                        localFile.FolderPath = fileInfo.DirectoryName;
                        localFile.FileSize = fileInfo.Length;
                        localFile.CreatedDate = fileInfo.CreationTime;
                        localFile.ModifiedDate = fileInfo.LastWriteTime;
                        localFile.Owner = "UnknownOwner";
                        localFile.ComputerName = Environment.MachineName;
                        ProvenMachineProgramList.Add(localFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
        }
        private void GetStandardFiles(string path)
        {
            try
            {
                foreach (string subDirectory in Directory.GetDirectories(path, "Standard Software Program", SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.GetFiles(subDirectory))
                    {
                        FileInformation localFile = new FileInformation();
                        FileInfo fileInfo = new FileInfo(file);
                        localFile.FileName = fileInfo.Name;
                        localFile.FileType = fileInfo.Extension;
                        localFile.FolderPath = fileInfo.DirectoryName;
                        localFile.FileSize = fileInfo.Length;
                        localFile.CreatedDate = fileInfo.CreationTime;
                        localFile.ModifiedDate = fileInfo.LastWriteTime;
                        localFile.Owner = "UnknownOwner";
                        localFile.ComputerName = Environment.MachineName;
                        StandardSoftwareProgramList.Add(localFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
        }

        public void setAndGetFileInfo()
        {
            SqlConnection conn = null;
            try
            {
                string LocalDirectory = ConfigurationManager.AppSettings["folderPath"].ToString();
                GetStandardFiles(LocalDirectory);
                conn = ConnectionManager.GetConnection();
                foreach (FileInformation sfile in StandardSoftwareProgramList)
                {
                    fileDataBaseAccess.InsertOrUpdateStandardIntoDatabase(sfile, conn);
                }
                conn?.Close();
                conn = ConnectionManager.GetConnection();
                GetProvenFiles(LocalDirectory);
                foreach (FileInformation pfile in ProvenMachineProgramList)
                {
                    string folder = pfile.FolderPath.Substring(0, pfile.FolderPath.LastIndexOf('\\'));

                    FileInformation sfile = StandardSoftwareProgramList.Find(x => x.FolderPath.StartsWith(folder));
                    fileDataBaseAccess.InsertOrUpdateProvenIntoDatabase(pfile, conn);
                    if (sfile == null)
                    {
                        fileDataBaseAccess.UpdateStatusStandardToNULL(pfile, conn);
                    }
                }

                conn?.Close();
                conn = ConnectionManager.GetConnection();
                fileDataBaseAccess.UpdateStatus(conn);
                conn?.Close();
                // target time should be one of the shift end time which is getter the current time
                //get index of shift end time which is getter the current
                int idx = shiftDetails.FindIndex(x => x > DateTime.Now.TimeOfDay);
                if (idx == -1)
                {
                    idx = 0;
                }
                //make target time as shift end time of today 
                Target = DateTime.Today.Add(shiftDetails[idx]);
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog($"An error occurred: {ex.Message}" + DateTime.Now);
            }
            finally
            {
                conn?.Close();
                //clear the list
                ProvenMachineProgramList.Clear();
                StandardSoftwareProgramList.Clear();
            }
        }

        protected override void OnStop()
        {
            running = false;
            StartFunctionThread.Join();
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                Thread.CurrentThread.Name = "Main";
            }
            Logger.WriteDebugLog($"Service Stop at: {DateTime.Now}");
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}