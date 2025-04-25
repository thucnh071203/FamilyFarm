using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ReportDAO
    {
        private readonly IMongoCollection<Report> _Reports;
        public ReportDAO(IMongoDatabase database)
        {
            _Reports = database.GetCollection<Report>("Report");
        }

        /// <summary>
        /// Retrieves all non-deleted reports from the database.
        /// </summary>
        /// <returns>A list of non-deleted reports.</returns>
        public async Task<List<Report>> GetAllAsync()
        {
            return await _Reports.Find(r => !r.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// Retrieves a report by its ID, ensuring it is not deleted.
        /// </summary>
        /// <param name="id">The ID of the report to retrieve.</param>
        /// <returns>The report if found, or null if not found or deleted.</returns>
        public async Task<Report?> GetByIdAsync(string id)
        {
            return await _Reports.Find(r => r.ReportId == id && !r.IsDeleted).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a report based on the provided post ID and reporter ID, ensuring it is not deleted.
        /// </summary>
        /// <param name="postId">The ID of the post associated with the report.</param>
        /// <param name="reporterId">The ID of the reporter who filed the report.</param>
        /// <returns>The report if found, or null if not found or deleted.</returns>
        public async Task<Report?> GetByPostAndReporterAsync(string postId, string reporterId)
        {
            if (!ObjectId.TryParse(reporterId, out _)
                || !ObjectId.TryParse(postId, out _))
                return null;
            return await _Reports.Find(r => r.PostId == postId && r.ReporterId == reporterId && !r.IsDeleted).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new report in the database with the provided report object.
        /// The report ID is generated, and the status is set to "pending".
        /// </summary>
        /// <param name="report">The report object to be created.</param>
        /// <returns>The created report if successful, or null if invalid data is provided.</returns>
        public async Task<Report> CreateAsync(Report report)
        {
            if (!ObjectId.TryParse(report.ReporterId, out _) 
                || !ObjectId.TryParse(report.PostId, out _))
                return null;
            report.ReportId = ObjectId.GenerateNewId().ToString();
            report.Status = "pending";
            report.HandledById = null;
            report.HandlerNote = null;
            report.HandledAt = null;
            await _Reports.InsertOneAsync(report);
            return report;
        }

        /// <summary>
        /// Updates an existing report with the provided report data, ensuring the report exists and is not deleted.
        /// </summary>
        /// <param name="id">The ID of the report to update.</param>
        /// <param name="report">The updated report data.</param>
        /// <returns>The updated report if successful, or null if the report does not exist or is invalid.</returns>
        public async Task<Report> UpdateAsync(string id, Report report)
        {
            //if (!ObjectId.TryParse(report.ReporterId, out _) 
            //    || !ObjectId.TryParse(report.PostId, out _) 
            //    || !ObjectId.TryParse(report.HandledById, out _))
            //    return null;
            var existing = await _Reports.Find(r => r.ReportId == id && r.IsDeleted != true).FirstOrDefaultAsync();
            if (existing == null) return null;

            report.ReportId = id;
            report.HandledAt = DateTime.UtcNow;
            await _Reports.ReplaceOneAsync(r => r.ReportId == id && r.IsDeleted != true, report);
            return report;
        }

        /// <summary>
        /// Soft deletes a report by setting its 'IsDeleted' field to true.
        /// </summary>
        /// <param name="id">The ID of the report to delete.</param>
        /// <returns>An asynchronous task that represents the operation.</returns>
        public async Task DeleteAsync(string id)
        {
            var update = Builders<Report>.Update.Set(r => r.IsDeleted, true);
            await _Reports.UpdateOneAsync(r => r.ReportId == id, update);
        }
    }
}
