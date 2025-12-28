using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface IAgpPeriodRepository : IGenericRepository<AgpPeriod>
{
    /// <summary>
    /// AGP'ye ait tüm dönemleri getir
    /// </summary>
    Task<IEnumerable<AgpPeriod>> GetByAgpIdAsync(int agpId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dönem detaylarını (milestone ve aktivitelerle birlikte) getir
    /// </summary>
    Task<AgpPeriod?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// AGP'ye ait tüm dönemleri detaylarıyla getir
    /// </summary>
    Task<IEnumerable<AgpPeriod>> GetByAgpIdWithDetailsAsync(int agpId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrencinin tüm dönemlerini getir (tüm AGP'ler dahil)
    /// </summary>
    Task<IEnumerable<AgpPeriod>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Çakışan dönemleri kontrol et
    /// </summary>
    Task<IEnumerable<AgpPeriod>> FindOverlappingPeriodsAsync(
        int agpId,
        DateTime startDate,
        DateTime endDate,
        int? excludePeriodId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AGP'deki en erken dönem başlangıç tarihini getir
    /// </summary>
    Task<DateTime?> GetEarliestStartDateAsync(int agpId, CancellationToken cancellationToken = default);

    /// <summary>
    /// AGP'deki en geç dönem bitiş tarihini getir
    /// </summary>
    Task<DateTime?> GetLatestEndDateAsync(int agpId, CancellationToken cancellationToken = default);
}
