using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using static System.Net.Mime.MediaTypeNames;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ProcessStepImageDAO
    {
        private readonly IMongoCollection<ProcessStepImage> _ProcessStepImags;

        public ProcessStepImageDAO(IMongoDatabase database)
        {
            _ProcessStepImags = database.GetCollection<ProcessStepImage>("ProcessStepImage");
        }

        public async Task<List<ProcessStepImage>> GetStepImagesByStepId(string stepId)
        {
            if (!ObjectId.TryParse(stepId, out _)) return null;
            return await _ProcessStepImags.Find(p => p.ProcessStepId == stepId).ToListAsync();
        }

        public async Task CreateStepImage(ProcessStepImage? request)
        {
            if (request == null)
                return;

            await _ProcessStepImags.InsertOneAsync(request);
        }

        public async Task<ProcessStepImage?> UpdateStepImage(string imageId, ProcessStepImage? request)
        {
            if (!ObjectId.TryParse(imageId, out _)) return null;

            var filter = Builders<ProcessStepImage>.Filter.Eq(p => p.ProcessStepImageId, imageId);

            var update = Builders<ProcessStepImage>.Update
                .Set(p => p.ImageUrl, request.ImageUrl);

            var result = await _ProcessStepImags.UpdateOneAsync(filter, update);

            // Lấy lại bản ghi sau khi cập nhật để trả về
            var updatedImage = await _ProcessStepImags.Find(p => p.ProcessStepImageId == imageId).FirstOrDefaultAsync();

            return updatedImage;
        }

        public async Task DeleteStepImageById(string imageId)
        {
            var filter = Builders<ProcessStepImage>.Filter.Eq(p => p.ProcessStepImageId, imageId);
            await _ProcessStepImags.DeleteOneAsync(filter);
        }

        public async Task DeleteImagesByStepId(string? stepId)
        {
            var filter = Builders<ProcessStepImage>.Filter.Eq(p => p.ProcessStepId, stepId);
            await _ProcessStepImags.DeleteManyAsync(filter);
        }
    }
}
