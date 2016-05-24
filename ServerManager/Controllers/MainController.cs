using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ServerManager.Controllers
{
    public class MainController: Controller
    {
        List<UserInfo> AllUsers;
        
        /*
        = {
            new UserInfo {UserName = "eric", Password="ostrich555", Directory=@"C:\Users\Buttercraft\Documents\Minecraft\Minecraft Servers\Eric\ElectroCraft\bungee\bungee" },
            new UserInfo {UserName = "peter", Password="foo", Directory=@"C:\Users\Peter\Documents\temp" },
        };
        */
        public MainController()
        {
            ReadUsers();
        }

        private void ReadUsers()
        {
            string filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/users.txt");
            AllUsers = new List<UserInfo>();
            string[] lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (string line in lines) {
                string[] split = line.Split('|');
                if (split.Length == 3) {
                    UserInfo userInfo = new UserInfo() { UserName = split[0], Password = split[1], Directory = split[2] };
                    AllUsers.Add(userInfo);
                }
            }
        }

        bool IsUser(string username, string password)
        {
            foreach (UserInfo ui in AllUsers) {
                if (ui.UserName == username && ui.Password == password)
                    return true;
            }

            return false;
        }

        string Directory(string username, string password)
        {
            foreach (UserInfo ui in AllUsers) {
                if (ui.UserName == username && ui.Password == password) {
                    return ui.Directory;
                }
            }

            throw new UnauthorizedAccessException("Bad username or password");
        }

        string ServerDirectory(string username, string password, string server)
        {
            string dir = Directory(username, password);
            string root = Path.Combine(dir, server);
            if (!System.IO.Directory.Exists(root))
                throw new UnauthorizedAccessException("Bad server name");
            if (!System.IO.File.Exists(System.IO.Path.Combine(root, "server.properties")))
                throw new UnauthorizedAccessException("Bad server name");

            return root;
        }

        public ActionResult Authenticate(string user, string pw)
        {
            if (IsUser(user, pw)) {
                return Content("true");
            }
            else {
                return Content("false");
            }
        }

        public ActionResult ListServers(string user, string pw)
        {
            try {
                StringBuilder result = new StringBuilder();

                string root = Directory(user, pw);

                foreach (string dir in System.IO.Directory.GetDirectories(root)) {
                    if (System.IO.Directory.Exists(dir)) {
                        if (System.IO.File.Exists(System.IO.Path.Combine(dir, "server.properties"))) {
                            string subDir = dir.Substring(root.Length + 1);
                            result.AppendLine(subDir);
                        }
                    }
                }

                return Content(result.ToString());
            }
            catch (UnauthorizedAccessException e) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
            }
        }

        public ActionResult ListFiles(string server, string user, string pw)
        {
            try {
                StringBuilder result = new StringBuilder();

                string root = ServerDirectory(user, pw, server);

                foreach (string fileName in System.IO.Directory.GetFiles(root, "*", SearchOption.AllDirectories)) {
                    if (fileName.StartsWith(root + "\\", StringComparison.InvariantCultureIgnoreCase)) {
                        string subFile = fileName.Substring(root.Length + 1);
                        result.AppendLine(subFile);
                    }
                }

                return Content(result.ToString());
            }
            catch (UnauthorizedAccessException e) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
            }
        }

        public ActionResult DownloadFile(string server, string filename, string user, string pw)
        {
            try {
                string root = ServerDirectory(user, pw, server);
                string pathName = root + "//" + filename;

                Stream fileStream = new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return File(fileStream, "application/octet-stream");
            }
            catch (UnauthorizedAccessException e) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(string server, string filename, string user, string pw)
        {
            try {
                string root = ServerDirectory(user, pw, server);
                string pathName = root + "//" + filename;

                System.IO.File.Delete(pathName);
                return Content("OK");
            }
            catch (UnauthorizedAccessException e) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpPost]
        public ActionResult UploadFile(string server, string filename, string user, string pw)
        {
            string root = ServerDirectory(user, pw, server);
            string pathName = root + "//" + filename;

            if (Request.Files.Count > 0) {
                Request.Files[0].SaveAs(pathName);
                return Content("OK");
            }
            else {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No file provided.");
            }
        }
    }

    class UserInfo
    {
        public string UserName;
        public string Password;
        public string Directory;
    }
}