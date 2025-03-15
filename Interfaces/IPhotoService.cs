using CloudinaryDotNet.Actions;

namespace iFood.Interfaces;
public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId); 
}
