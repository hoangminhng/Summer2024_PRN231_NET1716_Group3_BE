﻿using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Service.Cloundinary;
using Service.Interface;

namespace Service.Implement
{
	public class CloudinaryService : ICloudinaryService
	{
		private readonly Cloudinary cloudinary;
		public CloudinaryService(IOptions<CloudinarySetting> config)
		{
			var acc = new Account(
				config.Value.CloudName,
				config.Value.ApiKey,
				config.Value.ApiSecret
			);

			cloudinary = new Cloudinary(acc);
		}
		public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
		{
			var uploadResult = new ImageUploadResult();
			if (file.Length > 0)
			{
				using var stream = file.OpenReadStream();
				var uploadParams = new ImageUploadParams
				{
					File = new FileDescription(file.FileName, stream),
					Folder = "PRN231_GroupProject"
				};
				uploadResult = await cloudinary.UploadAsync(uploadParams);
			}
			return uploadResult;
		}

		public async Task<DeletionResult> DeletePhotoAsync(string publicId)
		{
			var deleteParams = new DeletionParams(publicId);
			return await cloudinary.DestroyAsync(deleteParams);
		}
	}
}
