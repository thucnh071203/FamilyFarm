using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ProcessStepDAO
    {
        private readonly IMongoCollection<ProcessStep> _ProcessSteps;
        private readonly IMongoCollection<ProcessStepResults> _ProcessStepResults;
        private readonly IMongoCollection<StepResultImages> _StepResultImages;

        public ProcessStepDAO(IMongoDatabase database)
        {
            _ProcessSteps = database.GetCollection<ProcessStep>("ProcessStep");
            _ProcessStepResults = database.GetCollection<ProcessStepResults>("ProcessStepResults");
            _StepResultImages = database.GetCollection<StepResultImages>("StepResultImages");
        }

        public async Task<List<ProcessStep>?> GetStepsByProcessId(string processId)
        {
            if (!ObjectId.TryParse(processId, out _)) return null;
            return await _ProcessSteps.Find(p => p.ProcessId == processId).ToListAsync();
        }

        public async Task<ProcessStep?> GetStepById(string? stepId)
        {
            if (string.IsNullOrEmpty(stepId))
                return null;

            var filter = Builders<ProcessStep>.Filter.Eq(s => s.StepId, stepId);
            return await _ProcessSteps.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ProcessStep?> CreateStep(ProcessStep? request)
        {
            if (request == null)
                return null;

            await _ProcessSteps.InsertOneAsync(request);
            return request;
        }

        public async Task<ProcessStep?> EditStep(string stepId, ProcessStep? request)
        {
            if (!ObjectId.TryParse(stepId, out _)) return null;

            var filter = Builders<ProcessStep>.Filter.Eq(p => p.StepId, stepId);

            if (filter == null) return null;

            var update = Builders<ProcessStep>.Update
                .Set(p => p.StepTitle, request.StepTitle)
                .Set(p => p.StepDesciption, request.StepDesciption);

            var result = await _ProcessSteps.UpdateOneAsync(filter, update);

            var updatedStep = await _ProcessSteps.Find(p => p.StepId == stepId).FirstOrDefaultAsync();

            return updatedStep;
        }

        public async Task DeleteStepById(string stepId)
        {
            var filter = Builders<ProcessStep>.Filter.Eq(p => p.StepId, stepId);
            await _ProcessSteps.DeleteOneAsync(filter);
        }

        public async Task<bool?> HardDeleteByProcess(string? processId)
        {
            if (string.IsNullOrEmpty(processId))
                return null;

            var filter = Builders<ProcessStep>.Filter.Eq(p => p.ProcessId, processId);
            var result = await _ProcessSteps.DeleteManyAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<List<ProcessStep>?> GetStepsBySubprocess(string? subprocessId)
        {
            if (!ObjectId.TryParse(subprocessId, out _)) return null;
            return await _ProcessSteps.Find(p => p.SubprocessId == subprocessId).ToListAsync();
        }

        // Thêm ProcessStepResult
        public async Task<ProcessStepResults> CreateProcessStepResultAsync(ProcessStepResults result)
        {
            await _ProcessStepResults.InsertOneAsync(result);
            return result;
        }

        // Lấy danh sách ProcessStepResults theo StepId, sắp xếp theo CreatedAt giảm dần
        public async Task<List<ProcessStepResults>> GetProcessStepResultsByStepIdAsync(string stepId)
        {
            return await _ProcessStepResults
                .Find(r => r.StepId == stepId)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // Thêm StepResultImage
        public async Task<StepResultImages> CreateStepResultImageAsync(StepResultImages image)
        {
            await _StepResultImages.InsertOneAsync(image);
            return image;
        }

        // Lấy danh sách hình ảnh theo StepResultId
        public async Task<List<StepResultImages>> GetStepResultImagesByStepResultIdAsync(string stepResultId)
        {
            return await _StepResultImages
                .Find(i => i.StepResultId == stepResultId)
                .ToListAsync();
        }
    }
}
