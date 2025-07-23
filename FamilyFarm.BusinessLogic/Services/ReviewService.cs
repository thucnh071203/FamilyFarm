using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Repositories;
using AutoMapper;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IBookingServiceRepository _bookingServiceRepository;
        public ReviewService(IReviewRepository reviewRepository, IAccountRepository accountRepository, IMapper mapper, IBookingServiceRepository bookingServiceRepository)
        {
            _reviewRepository = reviewRepository;
            _accountRepository = accountRepository;
            _mapper = mapper;
            _bookingServiceRepository = bookingServiceRepository;
        }

        public async Task<ListReviewResponseDTO> GetByServiceIdAsync(string serviceId)
        {
            // Validate service ID format
            if (!ObjectId.TryParse(serviceId, out _))
            {
                return new ListReviewResponseDTO
                {
                    Success = false,
                    Message = "Invalid Service ID format"
                };
            }

            var reviews = await _reviewRepository.GetByServiceIdAsync(serviceId);

            if (reviews == null)
            {
                return new ListReviewResponseDTO
                {
                    Success = false,
                    Message = "No reviews found for the specified service."
                };
            }

            // Map each review to ReviewDTO (with reviewer profile)
            var reviewDTOs = new List<ReviewDTO>();
            foreach (var review in reviews)
            {
                // Load reviewer profile
                var account = await _accountRepository.GetAccountByAccId(review.AccId);
                var reviewer = _mapper.Map<MyProfileDTO>(account);
                reviewDTOs.Add(new ReviewDTO
                {
                    Review = review,
                    Reviewer = reviewer
                });
            }

            return new ListReviewResponseDTO
            {
                Success = true,
                Message = "Get list review successfully!",
                Data = reviewDTOs
            };
        }

        public async Task<ReviewSummaryDTO> GetSummaryByServiceId(string serviceId)
        {
            // Validate service ID format
            if (!ObjectId.TryParse(serviceId, out _))
            {
                return new ReviewSummaryDTO
                {
                    Success = false,
                    Message = "Invalid Service ID format"
                };
            }

            var reviews = await _reviewRepository.GetByServiceIdAsync(serviceId);

            if (reviews == null || !reviews.Any())
            {
                return new ReviewSummaryDTO
                {
                    Success = false,
                    Message = "No reviews found for the specified service."
                };
            }

            var validReviews = reviews
                .Where(r => !r.IsDeleted)
                .ToList();

            // Ép kiểu Rating từ int? -> int
            var ratingCounts = validReviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            double avgRating = Math.Round(validReviews.Average(r => r.Rating), 1);

            // Bổ sung các mức sao còn thiếu
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingCounts.ContainsKey(i))
                    ratingCounts[i] = 0;
            }

            return new ReviewSummaryDTO
            {
                Success = true,
                Message = "Get review summary successfully!",
                AverageRating = avgRating,
                RatingCounts = ratingCounts
                    .OrderByDescending(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Value)
            };
        }


        //public async Task<Review> GetByIdAsync(string id)
        //{
        //    var review = await _reviewRepository.GetByIdAsync(id);
        //    if (review == null)
        //    {
        //        throw new Exception("Review not found.");
        //    }
        //    return review;
        //}

        public async Task<ReviewResponseDTO> CreateAsync(ReviewRequestDTO request, string accId)
        {
            // Validate request
            if (request == null || string.IsNullOrEmpty(request.ServiceId) || request.Rating == null)
                return new ReviewResponseDTO { Success = false, Message = "Invalid review data" };

            if (request.Rating < 1 || request.Rating > 5)
                return new ReviewResponseDTO { Success = false, Message = "Rating must be between 1 and 5 stars" };

            // Validate object ID formats
            if (!ObjectId.TryParse(request.ServiceId, out _) || !ObjectId.TryParse(accId, out _))
                return new ReviewResponseDTO { Success = false, Message = "Invalid Service ID or Account ID" };

            // Map DTO to entity
            var review = new Review
            {
                ReviewId = ObjectId.GenerateNewId().ToString(),
                ServiceId = request.ServiceId,
                Rating = request.Rating,
                Comment = request.Comment,
                AccId = accId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdReview = await _reviewRepository.CreateAsync(review);
            if (createdReview == null)
                return new ReviewResponseDTO { Success = false, Message = "Failed to create review" };

            // Load reviewer profile
            var account = await _accountRepository.GetAccountByAccId(accId);
            var reviewer = _mapper.Map<MyProfileDTO>(account);

            //SAU KHI REVIEW THÀNH CÔNG THÌ SET trường completed của boooking
            if(request.BookingServiceId != null)
            {
                var booking = await _bookingServiceRepository.GetById(request.BookingServiceId);

                booking.CompleteServiceAt = DateTime.UtcNow;
                booking.IsCompletedFinal = true;

                await _bookingServiceRepository.UpdateStatus(booking);
            }
            

            return new ReviewResponseDTO
            {
                Success = true,
                Message = "Review created successfully",
                Data = new ReviewDTO
                {
                    Review = createdReview,
                    Reviewer = reviewer
                }
            };
        }

        //public async Task<ReviewResponseDTO> UpdateAsync(string id, ReviewRequestDTO request, string accId)
        //{
        //    // Validate rating and request
        //    if (request == null || request.Rating == null || request.Rating < 1 || request.Rating > 5)
        //        return new ReviewResponseDTO { Success = false, Message = "Invalid rating data" };

        //    // Find the review and validate ownership
        //    var existingReview = await _reviewRepository.GetByIdAsync(id);
        //    if (existingReview == null || existingReview.AccId != accId)
        //        return new ReviewResponseDTO { Success = false, Message = "Review not found or permission denied" };

        //    // Update review fields
        //    existingReview.Rating = request.Rating.Value;
        //    existingReview.Comment = request.Comment;
        //    existingReview.UpdatedAt = DateTime.UtcNow;

        //    var updatedReview = await _reviewRepository.UpdateAsync(id, existingReview);
        //    if (updatedReview == null)
        //        return new ReviewResponseDTO { Success = false, Message = "Failed to update review" };

        //    // Load reviewer profile
        //    var account = await _accountRepository.GetAccountByAccId(accId);
        //    var reviewer = _mapper.Map<MyProfileDTO>(account);

        //    return new ReviewResponseDTO
        //    {
        //        Success = true,
        //        Message = "Review updated successfully",
        //        Data = new ReviewDTO
        //        {
        //            Review = updatedReview,
        //            Reviewer = reviewer
        //        }
        //    };
        //}

        public async Task<ReviewResponseDTO> DeleteAsync(string id, string accId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null || review.IsDeleted)
            {
                return new ReviewResponseDTO
                {
                    Success = false,
                    Message = "Review not found or already deleted."
                };
            }

            await _reviewRepository.DeleteAsync(id);

            return new ReviewResponseDTO
            {
                Success = true,
                Message = "Review deleted successfully"
            };
        }
    }
}
