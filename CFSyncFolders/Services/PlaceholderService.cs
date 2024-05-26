using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;

namespace CFSyncFolders.Services
{
    /// <summary>
    /// Replaces placeholders in strings.
    /// 
    /// Placeholders are of the following formats:
    /// "{placeholder}" : E.g. "{machine}", "{user}"
    /// "{placeholder:parameter1:parameter2}" : E.g. "{date:yyyy-MM-dd}", "{special-folder:Desktop}"
    /// </summary>
    public class PlaceholderService : IPlaceholderService
    {        
        public List<Placeholder> GetAll()
        {            
            return new List<Placeholder>()
            {               
                new Placeholder()
                {
                    Name = "{process-folder}",                   
                    CanGetValue = (placeholderName) => placeholderName.Equals("{process-folder}"),
                    Description = "Process folder",
                    GetValue = (placeholderName, parameters) => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
                },
                new Placeholder()
                {
                    Name = "{process-name}",
                    CanGetValue = (placeholderName) => placeholderName.Equals("{process-name}"),
                    Description = "Process name",
                    GetValue = (placeholderName, parameters) => Process.GetCurrentProcess().ProcessName
                },
                new Placeholder()
                {
                    Name = "{process-id}",
                    CanGetValue = (placeholderName) => placeholderName.Equals("{process-id}"),
                    Description = "Process ID",
                    GetValue = (placeholderName, parameters) => Process.GetCurrentProcess().Id.ToString()
                },
                new Placeholder()
                {
                    Name = "{date:<format>}",   // Parameter=Format. E.g. MM-yyyy, yyyy-MM-dd
                    Description = "Date. E.g. {date:yyyy-MM-dd}",
                    CanGetValue = (placeholderName) => placeholderName.StartsWith("{date:"),
                    GetValue = (placeholderName, parameters) =>
                    {
                        var dateFormat = GetPlaceholderElements(placeholderName)[1];
                        DateTime dateTime = parameters.ContainsKey("date") ?
                                (DateTime)parameters["date"] : DateTime.Now;
                        return String.IsNullOrEmpty(dateFormat) ? dateTime.ToString() : dateTime.ToString(dateFormat);
                    }
                },                             
                new Placeholder()
                {                    
                    Name = "{machine}",
                    CanGetValue = (placeholderName) => placeholderName.Equals("{machine}"),
                    Description = "Machine name",
                    GetValue = (placeholderName, parameters) => Environment.MachineName
                },            
                new Placeholder()
                {
                    Name = "{user}",
                    Description = "User",
                    CanGetValue = (placeholderName) => placeholderName.Equals("{user}"),
                    GetValue = (placeholderName, parameters) => Environment.UserName
                },
                  new Placeholder()
                {
                    Name = "{user-domain}",
                    Description = "User domain",
                    CanGetValue = (placeholderName) => placeholderName.Equals("{user-domain}"),
                    GetValue = (placeholderName, parameters) => Environment.UserDomainName
                },
                new Placeholder()
                {
                    Name = "{special-folder:<folder enum>}",   // Parameter=System.Environment.SpecialFolder. E.g. "{special_folder:Desktop}"
                    Description = "Special folder. E.g. {special_folder:Desktop}",
                    CanGetValue = (placeholderName) => placeholderName.StartsWith("{special-folder:"),
                    GetValue = (placeholderName, parameters) =>
                    {
                          var folderType = (System.Environment.SpecialFolder)Enum.Parse(typeof(System.Environment.SpecialFolder),
                                  GetPlaceholderElements(placeholderName)[1], true);
                          return Environment.GetFolderPath(folderType);
                    }
                }
            };
        }      

        /// <summary>
        /// Gets all placeholders in string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<string> GetPlaceholderNames(string input, IReadOnlyList<Placeholder> placeholders)
        {
            var placeholderNames = new List<string>();
            int currentStartPos = 0;

            // Process input until all placeholders read
            do
            {
                // Find start position of next possible placeholder
                var startPos = input.IndexOf("{", currentStartPos);
                if (startPos > -1)  // Start position found
                {
                    int endPos = input.IndexOf("}", startPos + 1);
                    if (endPos > -1)   // End character found
                    {
                        // Check if this is actually a placeholder
                        var placeholderName = input.Substring(startPos, endPos - startPos + 1);
                        if (placeholders.Any(p => p.CanGetValue(placeholderName)))    // One of our placeholders
                        {
                            placeholderNames.Add(placeholderName);
                            currentStartPos = endPos + 1;   // Move position for next placeholder search
                        }                        
                        else    // Not one of our placeholders, move position to next character
                        {
                            currentStartPos++;
                        }
                    }
                    else   // No end character for this character, not a placeholder
                    {
                        currentStartPos = -1;
                    }
                }
                else   // No more placeholders
                {
                    currentStartPos = -1;
                }
            } while (currentStartPos != -1);

            return placeholderNames;
        }        

        public string GetWithPlaceholdersReplaced(string input, Dictionary<string, object> parameters)                                
        {
            // Get all placeholders
            var placeholders = GetAll();

            // Get all placeholder names in input
            var placeholderNames = GetPlaceholderNames(input, placeholders);            

            // Replace each placeholder
            string output = input;            
            foreach (var placeholderName in placeholderNames)
            {
                // Get placeholder that can handle this placeholder name
                var placeholder = placeholders.FirstOrDefault(p => p.CanGetValue(placeholderName));
                if (placeholder != null)
                {
                    // Replaceholder placeholder value
                    var placeholderValue = placeholder.GetValue(placeholderName, parameters);
                    if (output.Contains(placeholderName))
                    {
                        output = output.Replace(placeholderName, placeholderValue);
                    }
                }                        
            }

            return output;
        }     
      
        /// <summary>
        /// Gets placeholder elements. E.g. "{date:yyyy-MM-dd}" returns ["date","yyyy-MM-dd"]
        /// </summary>
        /// <param name="placeholderName"></param>
        /// <returns></returns>
        private static string[] GetPlaceholderElements(string placeholderName)
        {
            var elements = placeholderName.Replace("{", "")
                    .Replace("}", "")
                    .Split(':');
            return elements;
        }       
    }
}
