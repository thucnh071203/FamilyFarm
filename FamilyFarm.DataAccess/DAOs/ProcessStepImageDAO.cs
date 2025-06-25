using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ProcessStepImageDAO
    {
        private readonly IMongoCollection<ProcessStepImage> _ProcessStepImags;

        public ProcessStepImageDAO(IMongoDatabase database)
        {
            _ProcessStepImags = database.GetCollection<ProcessStepImage>("ProcessStepImage");
        }

        public async Task CreateStepImage(ProcessStepImage? request)
        {
            if (request == null)
                return;

            await _ProcessStepImags.InsertOneAsync(request);
        }
    }
}
