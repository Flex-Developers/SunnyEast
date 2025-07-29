namespace Application.Contract.Order.Queries.Hub;

/// <summary>
///  Возвращает список групп SignalR, в которые должен быть
///  добавлен пользователь при подключении к OrderHub.
/// </summary>
public sealed record GetOrderHubGroupsQuery(string UserName, string Role) : IRequest<List<string>>;