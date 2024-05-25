using System;
using System.Collections.Generic;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;

namespace CFSyncFolders.Services
{
    public class PlaceholderService : IPlaceholderService
    {
        public List<Placeholder> GetAll()
        {
            return new List<Placeholder>()
            {
                new Placeholder()
                {
                    Name = "{date:MM-yyyy}",
                    Description = "Date",
                    GetValue = (parameters) => 
                    {
                        DateTime dateTime = (DateTime)parameters["date"];
                        return string.Format("{0:00}-{1:0000}", dateTime.Month, dateTime.Year);
                    }
                },
                new Placeholder()
                {
                    Name = "{date:yyyy-MM-dd}",
                    Description = "Date",
                    GetValue = (parameters) =>
                    {
                        DateTime dateTime = (DateTime)parameters["date"];
                        return dateTime.ToString("yyyy-MM-dd");
                    },
                },
                new Placeholder()
                {
                    Name = "{day-number}",
                    Description = "Day number",
                    GetValue = (parameters) =>
                    {
                        DateTime dateTime = (DateTime)parameters["date"];
                        return dateTime.Day.ToString("00");
                    },
                },
                new Placeholder()
                {
                    Name = "{machine}",
                    Description = "Local machine name",
                    GetValue = (parameters) => Environment.MachineName
                },
                new Placeholder()
                {
                    Name = "{month-number}",
                    Description = "Month number",
                    GetValue = (parameters) =>
                    {
                        DateTime dateTime = (DateTime)parameters["date"];
                        return dateTime.Month.ToString("00");
                    },
                },
                new Placeholder()
                {
                    Name = "{user}",
                    Description = "Local user",
                    GetValue = (parameters) => Environment.UserName
                },
                new Placeholder()
                {
                    Name = "{year-number}",
                    Description = "Year number",
                     GetValue = (parameters) =>
                    {
                        DateTime dateTime = (DateTime)parameters["date"];
                        return dateTime.Year.ToString();
                    },
                }
            };
        }      

        public string GetWithPlaceholdersReplaced(string input, Dictionary<string, object> parameters)
        {
            string output = input;
            foreach(var placeholder in GetAll())
            {
                // Only call Placeholder.GetValue if necessary as it might be expensive for some placeholders
                if (output.Contains(placeholder.Name))
                {
                    var newValue = placeholder.GetValue(parameters);
                    output = output.Replace(placeholder.Name, newValue);
                }
            }

            return output;
        }
    }
}
