using BlogDotNetCode.Models.Photo;
using BlogDotNetCode.Repository;
using BlogDotNetCode.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDotNetCode.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        // Create the dependencies:
        private readonly IPhotoRepository _photoRepository;
        private readonly IBlogRepository _blogRepository;
        private readonly IPhotoService _photoService;

        public PhotoController(
            IPhotoRepository photoRepository,
            IBlogRepository blogRepository,
            IPhotoService photoService)
        {
            // Startup/ConfigureServices will resolve:
            // - PhotoRepository, i.e. 'services.AddScoped<IPhotoRepository, PhotoRepository>();'
            // - BlogRepository, i.e. 'services.AddScoped<IBlogRepository, BlogRepository>();'
            // - PhotoService, i.e. 'services.AddScoped<IPhotoService, PhotoService>();'
            _photoRepository = photoRepository;
            _blogRepository = blogRepository;
            _photoService = photoService;
        }

        [Authorize] // i.e. This Endpoint can only be hit if this User has been authenticated and has now been authorized properly (& User/Login Token includes legit claims)
        [HttpPost]
        public async Task<ActionResult<Photo>> UploadPhoto(IFormFile file)
        {
            // 'Claims' have previously been assigned in our Token (see BlogDotNetCode.Services/TokenService.cs), i.e.
            // JwtRegisteredClaimNames.NameId = user.ApplicationUserId, JwtRegisteredClaimNames.UniqueName = user.Username
            // This means that the ApplicationUserId can be retrieved from the Token in order to identify the User uploading Photo:
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

            // Save reference of Photo to DB - i.e. get PublicId from Cloudinairy/uploadResult and save to DB:
            var photoCreate = new PhotoCreate
            {
                PublicId = uploadResult.PublicId,
                ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                Description = file.FileName
            };

            var photo = await _photoRepository.InsertAsync(photoCreate, applicationUserId);

            return Ok(photo);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Photo>>> GetByApplicationUserId()
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var photos = await _photoRepository.GetAllByUserIdAsync(applicationUserId);

            return Ok(photos);
        }

        [HttpGet("{photoId}")]  // e.g. http://localhost:5000/api/Photo/3 (Get Photo having photoId of 3)
        public async Task<ActionResult<Photo>> Get(int photoId)
        {
            var photo = await _photoRepository.GetAsync(photoId);

            return Ok(photo);
        }

        [Authorize]
        [HttpDelete("{photoId}")]
        public async Task<ActionResult<int>> Delete(int photoId)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var foundPhoto = await _photoRepository.GetAsync(photoId);

            if (foundPhoto != null)
            {
                // Check that the photoId of the Photo for deletion actually belongs to the logged-in User:
                if (foundPhoto.ApplicationUserId == applicationUserId)
                {
                    // Load ALL of this current User's PUBLISHED blogs, and see of any of these blogs have the photo - if so, photo CANNOT be deleted > return Bad Request:
                    var blogs = await _blogRepository.GetAllByUserIdAsync(applicationUserId);

                    var usedInBlog = blogs.Any(b => b.PhotoId == photoId);

                    if (usedInBlog) return BadRequest("Cannot remove photo as it is being used in published blog(s).");

                    var deleteResult = await _photoService.DeletePhotoAsync(foundPhoto.PublicId);

                    if (deleteResult.Error != null) return BadRequest(deleteResult.Error.Message);

                    var affectedRows = await _photoRepository.DeleteAsync(foundPhoto.PhotoId);

                    return Ok(affectedRows);
                }
                else
                {
                    return BadRequest("Photo was not uploaded by the current user.");
                }
            }

            return BadRequest("Photo does not exist.");
        }
    }
}
