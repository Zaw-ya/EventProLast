using Microsoft.AspNetCore.Mvc;
using EventPro.Services.UnitOFWorkService.Interface;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System.IO;
using System.Threading.Tasks;


namespace EventPro.Web.Controllers
{
    public class BackupController : Controller
    {
        private readonly IUnitOFWorkService _unitOFWorkService;

        public BackupController(IUnitOFWorkService unitOFWorkService)
        {
            _unitOFWorkService = unitOFWorkService;
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> Create()
        {
            try
            {
                await _unitOFWorkService.Backup.GetDatabaseBackup();
                await _unitOFWorkService.Backup.GetEventsDataBackup();
            }
            catch { }

            return Redirect("~/Admin/");
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DownloadDataBase()
        {
            try
            {
                await _unitOFWorkService.Backup.GetDatabaseBackup();
            }
            catch { }

            string DatabasePath = _unitOFWorkService.Backup.FindLastCreatedDatabaseBackup();
            var stream = new FileStream(DatabasePath, FileMode.Open);

            return File(stream, "application/octet-stream", "DataBase_Backup.bak");
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DownloadEventsData()
        {
            try
            {
                await _unitOFWorkService.Backup.GetEventsDataBackup();
            }
            catch { }

            string EventsDataPath = _unitOFWorkService.Backup.FindLastCreatedEventsDataBackup();
            var stream = new FileStream(EventsDataPath, FileMode.Open);

            return File(stream, "application/zip", "Event_Data_Backup.zip");
        }

    }
}
