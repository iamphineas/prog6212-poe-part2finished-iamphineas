using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClaimPro.Data;
using ClaimPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClaimPro.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ClaimsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Claims
        [Authorize(Roles = "Lecturer")] // Only Lecturers can access their claims
        public async Task<IActionResult> Index()
        {
            return View(await _context.Claims
                .Where(user => user.User.Equals(_userManager.GetUserAsync(this.User).Result.Email))
                .ToListAsync());
        }

        // GET: Claims/PendingClaims
        [Authorize(Roles = "Manager,Coordinator")] // Only Managers and Coordinators can access PendingClaims
        public async Task<IActionResult> PendingClaims()
        {
            var pendingClaims = await _context.Claims
                .Where(c => c.Status == ClaimStatus.Pending)
                .ToListAsync();

            return View(pendingClaims);
        }

        // GET: Claims/ClaimHistory
        [Authorize(Roles = "Manager,Coordinator")] // Only Managers and Coordinators can access ClaimHistory
        public async Task<IActionResult> ClaimHistory()
        {
            var claimsHistory = await _context.Claims
                .Where(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Rejected)
                .ToListAsync();

            return View(claimsHistory);
        }

        // GET: Claims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims.FirstOrDefaultAsync(m => m.ClaimId == id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // GET: Claims/Create
        [Authorize(Roles = "Lecturer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Create([Bind("ClaimId,LecturerId,HoursWorked,HourlyRate,TotalAmount,Status,SubmittedDate,ImageUrl,ImageFile,DocumentType,ApprovalBy,ApprovalDate,ApprovalStatus,Notes,Comments,OriginalFileName")] Claim claim)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (claim.ImageFile != null && claim.ImageFile.Length > 0)
                {
                    // Check file size limit (5MB)
                    if (claim.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "File size cannot exceed 5MB.");
                        return View(claim);
                    }

                    // Handle file type validation
                    var validExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpeg", ".jpg" };
                    var extension = Path.GetExtension(claim.ImageFile.FileName).ToLower();
                    if (!validExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Invalid file type. Only PDF, DOCX, XLSX, PNG, JPEG, JPG files are allowed.");
                        return View(claim);
                    }

                    string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string imagesFolder = Path.Combine(wwwRootPath, "images");

                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    claim.OriginalFileName = claim.ImageFile.FileName;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(claim.ImageFile.FileName);
                    string filePath = Path.Combine(imagesFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await claim.ImageFile.CopyToAsync(fileStream);
                    }

                    claim.ImageUrl = "/images/" + fileName;
                }

                _context.Add(new Claim
                {
                    HoursWorked = claim.HoursWorked,
                    HourlyRate = claim.HourlyRate,
                    TotalAmount = claim.TotalAmount,
                    SubmittedDate = claim.SubmittedDate,
                    DocumentType = claim.DocumentType,
                    ImageFile = claim.ImageFile,
                    ImageUrl = claim.ImageUrl,
                    OriginalFileName = claim.OriginalFileName,
                    Notes = claim.Notes,
                    User = _userManager.GetUserAsync(this.User).Result.Email
                });
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(claim);
        }

        // GET: Claims/Edit/5
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClaimId,LecturerId,HoursWorked,HourlyRate,TotalAmount,Status,SubmittedDate,ImageUrl,ImageFile,DocumentType,ApprovalBy,ApprovalDate,ApprovalStatus,Notes,Comments,OriginalFileName")] Claim claim)
        {
            if (id != claim.ClaimId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (claim.ImageFile != null && claim.ImageFile.Length > 0)
                    {
                        string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string imagesFolder = Path.Combine(wwwRootPath, "images");

                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(claim.ImageFile.FileName);
                        string filePath = Path.Combine(imagesFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await claim.ImageFile.CopyToAsync(fileStream);
                        }

                        claim.ImageUrl = "/images/" + fileName;
                    }

                    _context.Update(claim);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClaimExists(claim.ClaimId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        // GET: Claims/Delete/5
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims.FirstOrDefaultAsync(m => m.ClaimId == id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: Claims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                _context.Claims.Remove(claim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Claims/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Coordinator")]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            claim.Status = ClaimStatus.Rejected;
            claim.Comments = comment;

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingClaims));
        }

        // POST: Claims/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Coordinator")]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            claim.Status = ClaimStatus.Approved;
            claim.ApprovalDate = DateTime.Now;

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingClaims));
        }

        // HR: View Approved Claims for Invoice Generation
        // GET: Claims/ApprovedClaims
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ApprovedClaims()
        {
            var approvedClaims = await _context.Claims
                .Where(c => c.Status == ClaimStatus.Approved && c.InvoiceGenerated == null)
                .ToListAsync();

            return View(approvedClaims);
        }

        // POST: Claims/GenerateInvoice/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR")]
        public IActionResult GenerateInvoice(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null)
            {
                return NotFound();
            }

            claim.InvoiceGenerated = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction("ApprovedClaims");
        }

        // GET: Claims/GeneratedInvoices
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GeneratedInvoices()
        {
            var generatedInvoices = await _context.Claims
                .Where(c => c.InvoiceGenerated != null)
                .ToListAsync();

            return View(generatedInvoices);
        }
        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.ClaimId == id);
        }
    }
}
