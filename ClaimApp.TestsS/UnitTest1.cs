using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClaimPro.Controllers;
using ClaimPro.Data;
using ClaimPro.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClaimApp.Tests
{
    public class ClaimsControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public ClaimsControllerTests()
        {
            // Mock UserManager
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);

            // Set up a sample user
            var user = new IdentityUser { Email = "test@example.com", Id = "lecturer1" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        }

        [Fact]
        public async Task Create_ValidClaim_ReturnsRedirectToIndex()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ClaimsController(context, _mockUserManager.Object);
                var claim = new ClaimPro.Models.Claim
                {
                    ClaimId = 1,
                    LecturerId = "lecturer1",
                    HoursWorked = 10,
                    HourlyRate = 20,
                    TotalAmount = 200,
                    Status = ClaimStatus.Pending,
                    SubmittedDate = DateTime.Now,
                    Notes = "Test Claim"
                };

                // Act
                var result = await controller.Create(claim);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
            }
        }

        [Fact]
        public async Task Create_InvalidFileSize_ReturnsViewWithError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ClaimsController(context, _mockUserManager.Object);
                var claim = new ClaimPro.Models.Claim
                {
                    ClaimId = 1,
                    LecturerId = "lecturer1",
                    HoursWorked = 10,
                    HourlyRate = 20,
                    TotalAmount = 200,
                    Status = ClaimStatus.Pending,
                    SubmittedDate = DateTime.Now,
                    Notes = "Test Claim"
                };

                // Simulate a large file exceeding 5MB
                var largeFile = new Mock<IFormFile>();
                largeFile.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB
                claim.ImageFile = largeFile.Object;

                // Act
                var result = await controller.Create(claim);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.False(controller.ModelState.IsValid);
                Assert.Contains("File size cannot exceed 5MB.", controller.ModelState["ImageFile"].Errors[0].ErrorMessage);
            }
        }

        [Fact]
        public async Task Create_InvalidFileType_ReturnsViewWithError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ClaimsController(context, _mockUserManager.Object);
                var claim = new ClaimPro.Models.Claim
                {
                    ClaimId = 1,
                    LecturerId = "lecturer1",
                    HoursWorked = 10,
                    HourlyRate = 20,
                    TotalAmount = 200,
                    Status = ClaimStatus.Pending,
                    SubmittedDate = DateTime.Now,
                    Notes = "Test Claim"
                };

                // Simulate an invalid file type
                var invalidFile = new Mock<IFormFile>();
                invalidFile.Setup(f => f.FileName).Returns("invalid.exe");
                invalidFile.Setup(f => f.Length).Returns(500);
                claim.ImageFile = invalidFile.Object;

                // Act
                var result = await controller.Create(claim);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.False(controller.ModelState.IsValid);
                Assert.Contains("Invalid file type.", controller.ModelState["ImageFile"].Errors[0].ErrorMessage);
            }
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithClaims()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Claims.AddRange(new List<ClaimPro.Models.Claim>
                {
                    new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Status = ClaimStatus.Pending },
                    new ClaimPro.Models.Claim { ClaimId = 2, LecturerId = "lecturer2", Status = ClaimStatus.Approved }
                });
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.Index() as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<List<ClaimPro.Models.Claim>>(result.Model);
                Assert.Equal(2, model.Count);
            }
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithClaim()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var claim = new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Notes = "Test Claim" };
                context.Claims.Add(claim);
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.Details(1) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsType<ClaimPro.Models.Claim>(result.Model);
                Assert.Equal(claim.ClaimId, model.ClaimId);
            }
        }

        [Fact]
        public async Task Edit_ReturnsViewResult_WithClaim()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var claim = new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Notes = "Test Claim" };
                context.Claims.Add(claim);
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.Edit(1) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsType<ClaimPro.Models.Claim>(result.Model);
                Assert.Equal(claim.ClaimId, model.ClaimId);
            }
        }

        [Fact]
        public async Task Edit_ValidClaim_ReturnsRedirectToIndex()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var claim = new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Notes = "Test Claim" };
                context.Claims.Add(claim);
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);
                var updatedClaim = new ClaimPro.Models.Claim
                {
                    ClaimId = 1,
                    LecturerId = "lecturer1",
                    HoursWorked = 15,
                    HourlyRate = 25,
                    TotalAmount = 375,
                    Status = ClaimStatus.Approved,
                    SubmittedDate = DateTime.Now,
                    Notes = "Updated Claim"
                };

                // Act
                var result = await controller.Edit(1, updatedClaim);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                var claimInDb = await context.Claims.FindAsync(1);
                Assert.Equal(15, claimInDb.HoursWorked);
                Assert.Equal("Updated Claim", claimInDb.Notes);
            }
        }

        [Fact]
        public async Task Delete_ReturnsViewResult_WithClaim()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var claim = new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Notes = "Test Claim" };
                context.Claims.Add(claim);
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.Delete(1) as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsType<ClaimPro.Models.Claim>(result.Model);
                Assert.Equal(claim.ClaimId, model.ClaimId);
            }
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesClaimAndRedirectsToIndex()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var claim = new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Notes = "Test Claim" };
                context.Claims.Add(claim);
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.DeleteConfirmed(1);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Null(await context.Claims.FindAsync(1));
            }
        }

        [Fact]
        public async Task PendingClaims_ReturnsViewResult_WithPendingClaims()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var user = new IdentityUser { Id = "lecturer1", Email = "lecturer@example.com" };
                context.Users.Add(user);
                context.Claims.AddRange(new List<ClaimPro.Models.Claim>
                {
                    new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Status = ClaimStatus.Pending },
                    new ClaimPro.Models.Claim { ClaimId = 2, LecturerId = "lecturer1", Status = ClaimStatus.Approved }
                });
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.PendingClaims() as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<List<ClaimPro.Models.Claim>>(result.Model);
                Assert.Single(model);
                Assert.Equal(1, model.First().ClaimId);
            }
        }

        [Fact]
        public async Task ClaimHistory_ReturnsViewResult_WithClaimHistory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var user = new IdentityUser { Id = "lecturer1", Email = "lecturer@example.com" };
                context.Users.Add(user);
                context.Claims.AddRange(new List<ClaimPro.Models.Claim>
                {
                    new ClaimPro.Models.Claim { ClaimId = 1, LecturerId = "lecturer1", Status = ClaimStatus.Pending },
                    new ClaimPro.Models.Claim { ClaimId = 2, LecturerId = "lecturer1", Status = ClaimStatus.Approved }
                });
                await context.SaveChangesAsync();

                var controller = new ClaimsController(context, _mockUserManager.Object);

                // Act
                var result = await controller.ClaimHistory() as ViewResult;

                // Assert
                Assert.NotNull(result);
                var model = Assert.IsAssignableFrom<List<ClaimPro.Models.Claim>>(result.Model);
                Assert.Equal(2, model.Count);
            }
        }
    }
}
