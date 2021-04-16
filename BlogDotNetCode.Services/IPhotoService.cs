using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BlogDotNetCode.Services
{
    public interface IPhotoService
    {
        public Task<ImageUploadResult> AddPhotoAsync(IFormFile fromfile);

        public Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}
