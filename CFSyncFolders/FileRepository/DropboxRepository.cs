using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace CFSyncFolders
{
    /// <summary>
    /// File repository for Dropbox (In progress)
    /// </summary>
    public class DropboxRepository : IFileRepository
    {
        private DropboxClient _client = null;
        private string _key = "";
        private string _accessToken = "";
        private string _uid = "";
        private bool _result = false;
        private string _oauth2State = "";      
        private const string _redirectUri = "https://localhost/authorize";
        private System.Windows.Forms.WebBrowser _webBrowser = null;

        public string Name
        {
            get { return "Dropbox"; }
        }

        private void Login()
        {
            _webBrowser = new System.Windows.Forms.WebBrowser();
            _webBrowser.Navigating += _webBrowser_Navigating;
        
            this.OAuth2State = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, _key, new Uri(_redirectUri), state: OAuth2State);
            _webBrowser.Navigate(_redirectUri);

            _client = new DropboxClient(_accessToken);
        }

        public void Initialise()
        {
            Login();
        }

        public bool IsFolderAvailable(string folder)
        {
            return false;
        }

        public bool IsFolderWritable(string folder)
        {
            return true;
        }

        private void _webBrowser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {          
            if (!e.Url.ToString().StartsWith(_redirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }

            try
            {
                OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(e.Url);
                if (result.State != this.OAuth2State)
                {
                    return;
                }

                _accessToken = result.AccessToken;
                _uid = result.Uid;
                _result = true;
            }
            catch (ArgumentException)
            {
                // There was an error in the URI passed to ParseTokenFragment
            }
            finally
            {
                e.Cancel = true;
                //this.Close();
            }
        }

        public string OAuth2State
        {
            get { return _oauth2State; }
            set { _oauth2State = value; }          
        }

        private static FileDetails GetFileDetails(FileMetadata fileMetadata)
        {
            FileDetails fileDetails = new FileDetails()
            {
                Name = fileMetadata.Name,
                TimeModified = fileMetadata.ClientModified,
                Length = Convert.ToInt64(fileMetadata.Size)
            };
            return fileDetails;
        }

        public FolderDetails GetFolderDetails(string folder)
        {
            throw new NotImplementedException();
        }

        private static FolderDetails GetFolderDetails(FolderMetadata folderMetadata)
        {
            FolderDetails folderDetails = new FolderDetails()
            {
                Name = folderMetadata.Name
            };
            return folderDetails;
        }

        public List<FileDetails> GetFileDetailsList(string folder)
        {
            List<FileDetails> fileDetailsList = new List<FileDetails>();

            ListFolderArg listFolderArg = new ListFolderArg(folder);
            Task<ListFolderResult> task = _client.Files.ListFolderAsync(listFolderArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error getting file details list");
            ListFolderResult listFolderResult = task.Result;

            // Process list entries
            foreach (Metadata metadata in listFolderResult.Entries.Where(x => x.IsFile))
            {              
                fileDetailsList.Add(GetFileDetails(metadata.AsFile));                
            }

            // If more results then request them          
            if (listFolderResult.HasMore)
            {
                ListFolderContinueArg listFolderContinueArg = new ListFolderContinueArg(listFolderResult.Cursor);
                task = _client.Files.ListFolderContinueAsync(listFolderContinueArg);
                Task.WaitAll(task);
                ThrowExceptionIfFaulted(task, "Error getting file details list");

                listFolderResult = task.Result;                
                foreach (Metadata metadata in listFolderResult.Entries.Where(x => x.IsFile))
                {               
                    fileDetailsList.Add(GetFileDetails(metadata.AsFile));                    
                }
            }
            return fileDetailsList;
        }

        public FileDetails GetFileDetails(string filePath)
        {
            GetMetadataArg metadataArg = new GetMetadataArg(filePath);
            Task<Metadata> task = _client.Files.GetMetadataAsync(metadataArg);            
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error getting file details");

            if (task.Result.IsFile)
            {                
                FileDetails fileDetails = GetFileDetails(task.Result.AsFile);                 
                return fileDetails;
            }
            return null;
        }

        private void ThrowExceptionIfFaulted(Task task, string message)
        {
            if (task.IsFaulted)
            {
                throw new Exception(message, task.Exception);
            }
        }

        public bool IsFileExists(string filePath)
        {
            GetMetadataArg getMetadataArg = new GetMetadataArg(filePath);
            Task<Metadata> task = _client.Files.GetMetadataAsync(getMetadataArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error getting file details");
            return task.Result.IsFile;
        }

        public bool IsFolderExists(string folder)
        {
            GetMetadataArg getMetadataArg = new GetMetadataArg(folder);
            Task<Metadata> task = _client.Files.GetMetadataAsync(getMetadataArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error getting folder details");
            return task.Result.IsFolder;              
        }

        public void DeleteFile(string filePath)
        {
            DeleteArg deleteArg = new DeleteArg(filePath);
            Task<Metadata> task = _client.Files.DeleteAsync(deleteArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error deleting file");
        }

        public void DeleteFolder(string folder)
        {
            DeleteArg deleteArg = new DeleteArg(folder);
            Task<Metadata> task = _client.Files.DeleteAsync(deleteArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error deleting folder");
        }

        public void WriteFile(string srcFilePath, string dstFilePath, bool copyProperties)
        {
           
        }

        public string PathCombine(string folder, string file)
        {
            return string.Format("{0}/{1}", folder, file);
        }

        public List<FolderDetails> GetFolderDetailsList(string folder)
        {
            List<FolderDetails> folderDetailsList = new List<FolderDetails>();

            ListFolderArg listFolderArg = new ListFolderArg(folder);
            Task<ListFolderResult> task = _client.Files.ListFolderAsync(listFolderArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error getting folder details list");
            ListFolderResult listFolderResult = task.Result;

            // Process list entries
            foreach(Metadata metadata in listFolderResult.Entries.Where(x => x.IsFolder))
            {          
                folderDetailsList.Add(GetFolderDetails(metadata.AsFolder));                
            }

            // If more results then request them            
            if (listFolderResult.HasMore)
            {
                ListFolderContinueArg listFolderContinueArg = new ListFolderContinueArg(listFolderResult.Cursor);
                task = _client.Files.ListFolderContinueAsync(listFolderContinueArg);
                Task.WaitAll(task);
                ThrowExceptionIfFaulted(task, "Error getting folder details list");

                listFolderResult = task.Result;
                foreach (Metadata metadata in listFolderResult.Entries.Where(x => x.IsFolder))
                {                    
                    folderDetailsList.Add(GetFolderDetails(metadata.AsFolder));                    
                }                
            }           
            return folderDetailsList;
        }

        public void CreateFolder(string folder)
        {
            CreateFolderArg folderArg = new CreateFolderArg(folder);
            Task<FolderMetadata> task = _client.Files.CreateFolderAsync(folderArg);
            Task.WaitAll(task);
            ThrowExceptionIfFaulted(task, "Error creating folder");
        }
    }
}
