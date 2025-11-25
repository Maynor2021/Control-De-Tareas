using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Control_De_Tareas.Services
{
    public interface IFileStorageService
    {
        string GetUploadPath(int courseOfferingId, int taskId);
        void EnsureDirectoryExists(string path);
        bool ValidateFile(IFormFile file, out string errorMessage);
        Task<string> SaveFileAsync(IFormFile file, int courseOfferingId, int taskId, string uniqueFileName);
    }
}