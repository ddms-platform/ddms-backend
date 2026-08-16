using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IOwnerToursRepository _tourRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailSender _emailSender;
    private readonly IOwnerDocumentService _docService;

    public ScheduleService(
        IScheduleRepository scheduleRepository, 
        IOwnerToursRepository tourRepository,
        INotificationService notificationService,
        IEmailSender emailSender,
        IOwnerDocumentService docService)
    {
        _scheduleRepository = scheduleRepository;
        _tourRepository = tourRepository;
        _notificationService = notificationService;
        _emailSender = emailSender;
        _docService = docService;
    }

    public async Task<PagedResponse<ScheduleItemResponse>> GetSchedulesAsync(Guid userId, ScheduleListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            if (!TourConstants.ScheduleStatuses.Allowed.Contains(normalizedStatus))
            {
                throw new AppException(ErrorCode.ScheduleStatusInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
                {
                    ["status"] = [ErrorCode.Messages.ScheduleStatusInvalid]
                });
            }
        }

        var (items, total) = await _scheduleRepository.GetPagedAsync(userId, query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        return new PagedResponse<ScheduleItemResponse>
        {
            items = items.Select(MapSchedule).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<ScheduleItemResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var entity = await _scheduleRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapSchedule(entity);
    }

    public async Task<ScheduleItemResponse> CreateAsync(Guid userId, CreateScheduleRequest request)
    {
        var docOverview = await _docService.GetOverviewByUserIdAsync(userId);
        if (docOverview.IsLocked)
        {
            throw new AppException(
                ErrorCode.OwnerDocumentOverdueBlocked,
                "Tài khoản của bạn đang bị tạm khóa do chưa hoàn tất phê duyệt giấy tờ pháp lý. Không thể tạo lịch trình tour mới!");
        }

        ValidateTimeRange(request.startTime, request.endTime);

        var tourEntity = await _tourRepository.GetByIdAsync(request.tourId, userId);
        if (tourEntity is null)
        {
            throw new AppException(ErrorCode.ScheduleTourNotFound, ErrorCode.Messages.ScheduleTourNotFound);
        }

        await ValidateBoatDockReferences(request.boatId, request.dockId);
        await ValidateNoTimeOverlapAsync(request.boatId, request.dockId, request.startTime, request.endTime, null);

        var entity = new tour_schedule
        {
            id = Guid.NewGuid(),
            tour_id = request.tourId,
            boat_id = request.boatId,
            dock_id = request.dockId,
            start_time = request.startTime,
            end_time = request.endTime,
            status = TourConstants.ScheduleStatuses.Scheduled,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _scheduleRepository.AddAsync(entity);
        var created = await _scheduleRepository.GetByIdAsync(entity.id, userId);
        return MapSchedule(created!);
    }

    public async Task<ScheduleItemResponse> UpdateAsync(Guid id, Guid userId, UpdateScheduleRequest request)
    {
        var docOverview = await _docService.GetOverviewByUserIdAsync(userId);
        if (docOverview.IsLocked)
        {
            throw new AppException(
                ErrorCode.OwnerDocumentOverdueBlocked,
                "Tài khoản của bạn đang bị tạm khóa do chưa hoàn tất phê duyệt giấy tờ pháp lý. Không thể cập nhật lịch trình tour!");
        }

        ValidateTimeRange(request.startTime, request.endTime);

        var normalizedStatus = request.status.Trim().ToLowerInvariant();
        if (!TourConstants.ScheduleStatuses.Allowed.Contains(normalizedStatus))
        {
            throw new AppException(ErrorCode.ScheduleStatusInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
            {
                ["status"] = [ErrorCode.Messages.ScheduleStatusInvalid]
            });
        }

        var tourEntity = await _tourRepository.GetByIdAsync(request.tourId, userId);
        if (tourEntity is null)
        {
            throw new AppException(ErrorCode.ScheduleTourNotFound, ErrorCode.Messages.ScheduleTourNotFound);
        }

        await ValidateBoatDockReferences(request.boatId, request.dockId);
        await ValidateNoTimeOverlapAsync(request.boatId, request.dockId, request.startTime, request.endTime, id);

        var entity = await _scheduleRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        var oldStartTime = entity.start_time;

        entity.tour_id = request.tourId;
        entity.boat_id = request.boatId;
        entity.dock_id = request.dockId;
        entity.start_time = request.startTime;
        entity.end_time = request.endTime;
        entity.status = normalizedStatus;

        await _scheduleRepository.UpdateAsync(entity);

        if (oldStartTime != request.startTime)
        {
            try
            {
                var activeBookings = await _scheduleRepository.GetActiveBookingsForScheduleAsync(id, default);
                if (activeBookings != null && activeBookings.Any())
                {
                    var newTimeFormatted = request.startTime.ToString("HH:mm dd/MM/yyyy");
                    var tourName = entity.tour?.name ?? "N/A";

                    foreach (var booking in activeBookings)
                    {
                        var bookingCode = booking.id.ToString().Substring(0, 8).ToUpper();
                        var body = $"Lịch khởi hành tour {tourName} (Mã: {bookingCode}) đã thay đổi sang {newTimeFormatted} do điều kiện kỹ thuật/thời tiết. Rất mong bạn thông cảm.";

                        // 1. Gửi thông báo In-App
                        await _notificationService.CreateNotificationAsync(
                            senderId: null,
                            type: "system",
                            title: "Thay đổi lịch trình tour ⚠️",
                            body: body,
                            recipientIds: new List<Guid> { booking.user_id },
                            data: null,
                            ct: default
                        );

                        // 2. Gửi thông báo Email
                        if (booking.user != null && !string.IsNullOrEmpty(booking.user.email))
                        {
                            await _emailSender.SendScheduleChangeEmailAsync(
                                booking.user.email,
                                booking.user.full_name ?? "Khách hàng",
                                bookingCode,
                                tourName,
                                oldStartTime,
                                request.startTime
                            );
                        }
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi gửi thông báo để tránh làm hỏng tiến trình cập nhật lịch trình chính
            }
        }

        var updated = await _scheduleRepository.GetByIdAsync(id, userId);
        return MapSchedule(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var docOverview = await _docService.GetOverviewByUserIdAsync(userId);
        if (docOverview.IsLocked)
        {
            throw new AppException(
                ErrorCode.OwnerDocumentOverdueBlocked,
                "Tài khoản của bạn đang bị tạm khóa do chưa hoàn tất phê duyệt giấy tờ pháp lý. Không thể xóa lịch trình tour!");
        }

        var entity = await _scheduleRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.status = TourConstants.ScheduleStatuses.Cancelled;
        await _scheduleRepository.UpdateAsync(entity);
    }

    private async Task ValidateBoatDockReferences(Guid? boatId, Guid? dockId)
    {
        if (boatId.HasValue && !await _scheduleRepository.BoatExistsAsync(boatId.Value))
        {
            throw new AppException(ErrorCode.ScheduleBoatNotFound, ErrorCode.Messages.ScheduleBoatNotFound);
        }

        if (dockId.HasValue && !await _scheduleRepository.DockExistsAsync(dockId.Value))
        {
            throw new AppException(ErrorCode.ScheduleDockNotFound, ErrorCode.Messages.ScheduleDockNotFound);
        }
    }

    private async Task ValidateNoTimeOverlapAsync(
        Guid? boatId,
        Guid? dockId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeScheduleId)
    {
        if (boatId.HasValue &&
            await _scheduleRepository.HasBoatTimeOverlapAsync(boatId.Value, startTime, endTime, excludeScheduleId))
        {
            throw new AppException(ErrorCode.ScheduleBoatOverlap, ErrorCode.Messages.ScheduleBoatOverlap);
        }

        if (dockId.HasValue &&
            await _scheduleRepository.HasDockTimeOverlapAsync(dockId.Value, startTime, endTime, excludeScheduleId))
        {
            throw new AppException(ErrorCode.ScheduleDockOverlap, ErrorCode.Messages.ScheduleDockOverlap);
        }
    }

    private static void ValidateTimeRange(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new AppException(ErrorCode.ScheduleTimeInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
            {
                ["endTime"] = [ErrorCode.Messages.ScheduleTimeInvalid]
            });
        }
    }

    private static ScheduleItemResponse MapSchedule(tour_schedule entity)
    {
        return new ScheduleItemResponse
        {
            id = entity.id,
            tourId = entity.tour_id,
            tourName = entity.tour.name,
            boatId = entity.boat_id,
            boatName = entity.boat?.name,
            dockId = entity.dock_id,
            dockName = entity.dock?.name,
            startTime = entity.start_time,
            endTime = entity.end_time,
            status = entity.status,
            createdAt = entity.created_at,
            updatedAt = entity.updated_at
        };
    }
}
