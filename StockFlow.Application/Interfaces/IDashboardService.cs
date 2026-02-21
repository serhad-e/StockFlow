using StockFlow.Application.DTOs;

namespace StockFlow.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}