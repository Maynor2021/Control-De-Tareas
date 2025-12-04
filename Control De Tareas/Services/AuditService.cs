using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Control_De_Tareas.Services
{
    public class AuditService
    {
        private readonly ContextDB _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ContextDB context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Métodos principales 
        public async Task LogCreateAsync(string entity, Guid? entityId = null, string entityName = "")
        {
            await LogAsync("CREATE", entity, entityId,
                $"Creación de {entity}: {entityName}");
        }

        public async Task LogUpdateAsync(string entity, Guid? entityId = null, string entityName = "")
        {
            await LogAsync("UPDATE", entity, entityId,
                $"Actualización de {entity}: {entityName}");
        }

        public async Task LogDeleteAsync(string entity, Guid? entityId = null, string entityName = "")
        {
            await LogAsync("DELETE", entity, entityId,
                $"Eliminación de {entity}: {entityName}");
        }

        // Método genérico 
        public async Task LogAsync(string action, string entity, Guid? entityId = null, string details = "")
        {
            try
            {
                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();

                var auditLog = new AuditLogs
                {
                    UserId = userId,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Details = !string.IsNullOrEmpty(details) ? details : $"{action} de {entity} por {userName}",
                    CreatedAt = DateTime.Now,
                    IsSoftDeleted = false
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en auditoría: {ex.Message}");
            }
        }

        private Guid? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return userId;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string GetCurrentUserName()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema";
            }
            catch
            {
                return "Sistema";
            }
        }
    }
}