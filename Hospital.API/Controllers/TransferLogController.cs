using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransferLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetTransferLogs(
    [FromQuery] string? searchTerm,
    [FromQuery] int? employeeId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 15)
        {
            // 1. نبدأ بالاستعلام الأساسي مع تضمين بيانات الموظف للبحث باسمه
            var query = _context.TransferLogs
                .Include(t => t.Employee)
                .AsNoTracking();

            // 2. الفلترة حسب الاسم (بحث سيرفر شامل لكل السجلات)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Employee.Name.Contains(searchTerm));
            }

            // 3. الفلترة حسب معرف الموظف (إذا وجدت)
            if (employeeId.HasValue)
            {
                query = query.Where(t => t.EmployeeId == employeeId);
            }

            // 4. حساب إجمالي السجلات بعد الفلترة وقبل التجزئة
            var totalRecords = await query.CountAsync();

            // حساب إجمالي الصفحات باستخدام المعادلة:
            // $$TotalPages = \lceil \frac{TotalRecords}{PageSize} \rceil$$
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // 5. جلب البيانات المخصصة للصفحة الحالية فقط مع الترتيب من الأحدث للأقدم
            var logs = await query
                .OrderByDescending(t => t.TransferDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransferLogDto
                {
                    Id = t.Id,
                    EmployeeId = t.EmployeeId,
                    EmployeeName = t.Employee.Name,
                    OldDepartmentId = t.OldDepartmentId,
                    NewDepartmentId = t.NewDepartmentId,
                    OldShiftType = t.OldShiftType,
                    NewShiftType = t.NewShiftType,
                    TransferDate = t.TransferDate,
                    AdOrderNumber = t.AdOrderNumber,
                    // جلب أسماء الأقسام
                    OldDepartmentName = _context.Departments.FirstOrDefault(d => d.Id == t.OldDepartmentId).Name,
                    NewDepartmentName = _context.Departments.FirstOrDefault(d => d.Id == t.NewDepartmentId).Name
                }).ToListAsync();

            // 6. إرجاع كائن يحتوي على البيانات ومعلومات الترقيم
            return Ok(new
            {
                Items = logs,
                TotalPages = totalPages,
                CurrentPage = page,
                TotalCount = totalRecords
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransferLogDto>> PostTransfer(CreateTransferLogDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) return NotFound(new { message = "لم يتم العثور على الموظف المحدد" });
            if (!await _context.Departments.AnyAsync(d => d.Id == dto.NewDepartmentId))
                return BadRequest(new { message = "لم يتم العثور على القسم المحدد" });
            var transferLog = new TransferLog
            {
                EmployeeId = dto.EmployeeId,
                OldDepartmentId = employee.DepartmentId, 
                NewDepartmentId = dto.NewDepartmentId,
                OldShiftType = employee.ShiftType,      
                NewShiftType = dto.NewShiftType,
                TransferDate = dto.TransferDate,
                AdOrderNumber = dto.AdOrderNumber
            };
            employee.DepartmentId = dto.NewDepartmentId;
            employee.ShiftType = (enShiftType)dto.NewShiftType;

            try
            {
                _context.TransferLogs.Add(transferLog);
                await _context.SaveChangesAsync();
                var resultDto = new TransferLogDto
                {
                    Id = transferLog.Id,
                    EmployeeId = transferLog.EmployeeId,
                    TransferDate = transferLog.TransferDate,
                    AdOrderNumber = transferLog.AdOrderNumber
                };

                return CreatedAtAction(nameof(GetTransferLogs), new { employeeId = transferLog.EmployeeId }, resultDto);
            }
            catch (Exception)
            {
                return StatusCode(500, new {message = "حدث خطأ أثناء معالجة البيانات"});
            }
        }
    }
}
