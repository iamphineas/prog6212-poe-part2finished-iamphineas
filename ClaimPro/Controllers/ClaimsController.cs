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
        [Authorize(Roles = "Lecturer")] // Only Lecturers can access Index
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
        [Authorize(Roles = "Lecturer")] // Only Lecturers can access Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")] // Ensure Create is only accessible by Lecturers
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

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Generate a unique file name and save the original file name
                    claim.OriginalFileName = claim.ImageFile.FileName; // Save original file name
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(claim.ImageFile.FileName);
                    string filePath = Path.Combine(imagesFolder, fileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await claim.ImageFile.CopyToAsync(fileStream);
                    }

                    // Save the relative path to the image URL
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

        // POST: Claims/Edit/5
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
                    // Handle file upload during edit
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

                        claim.ImageUrl = "/images/" + fileName;  // Save the relative path to the image
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
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            // Update the claim status and comments
            claim.Status = ClaimStatus.Rejected;
            claim.Comments = comment; // Save the comment

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingClaims)); // Redirect to pending claims
        }

        // POST: Claims/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            claim.Status = ClaimStatus.Approved;
            claim.ApprovalDate = DateTime.Now; // Set ApprovalDate here

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingClaims)); // Redirect to pending claims
        }

        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.ClaimId == id);
        }
    }
}

