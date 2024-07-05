using Mud.Core.Models;

namespace Mud.Core;

public interface IStockManager
{
    /// <summary>
    /// 搜索
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    Task<List<StockInfo>> SearchAsync(string keyword);

    /// <summary>
    /// 获取关注列表
    /// </summary>
    /// <returns></returns>
    Task<List<StockInfo>> GetFocusOnStockListAsync();

    /// <summary>
    /// 添加关注
    /// </summary>
    /// <param name="stockInfo"></param>
    /// <returns></returns>
    Task AddFocusOnStockAsync(StockInfo stockInfo);

    /// <summary>
    /// 一出关注
    /// </summary>
    /// <param name="stockInfo"></param>
    /// <returns></returns>
    Task RemoveFocusOnStockAsync(StockInfo stockInfo);

    /// <summary>
    /// 日线信息
    /// </summary>
    /// <param name="fullSymbol"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<List<DailyInfo>> GetDailyInfoAsync(string fullSymbol,DateTime start,DateTime? end = null);
}
