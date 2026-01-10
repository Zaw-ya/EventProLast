using EventPro.Services.RemovingDataService.Interface;


namespace EventPro.Services.RemovingDataService.Implementation
{
    public class RemovingDataService : IRemovingDataService
    {
        public Task RemoveFromDirectoriesByPeriod(List<string> Directories, TimeSpan Period)
        {
            foreach (string folder in Directories)
            {
                if (Directory.Exists(folder))
                {
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        DateTime creationTime = File.GetCreationTime(file);
                        TimeSpan elapsed = DateTime.Now.Subtract(creationTime);

                        if (elapsed > Period)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch { }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task RemoveFromDirectoriesDBCheckByPeriod(List<string> Directories, TimeSpan Period)
        {
            foreach (string folder in Directories)
            {
                if (Directory.Exists(folder))
                {
                    var myFiles = Directory.GetFiles(folder);
                    var myFolders = Directory.GetDirectories(folder);
                    /*
                    if (myFiles.Length > 0)
                    {
                        foreach (string file in myFiles)
                        {
                            DateTime creationTime = File.GetCreationTime(file);
                            TimeSpan elapsed = DateTime.Now.Subtract(creationTime);

                            if (elapsed > Period)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch { }
                            }
                        }
                    }
                    */
                    if (myFolders.Length > 0)
                    {
                        foreach (string subFolder in myFolders)
                        {
                            var mySubFiles = Directory.GetFiles(subFolder);
                            if (mySubFiles.Length > 0)
                            {
                                foreach (string file in mySubFiles)
                                {
                                    DateTime creationTime = File.GetCreationTime(file);
                                    TimeSpan elapsed = DateTime.Now.Subtract(creationTime);

                                    if (elapsed > Period)
                                    {
                                        try
                                        {
                                            File.Delete(file);
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
