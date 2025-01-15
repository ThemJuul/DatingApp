using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Interfaces;

public interface ILikesRepository
{
    Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId);
    Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams);
    Task<IEnumerable<int>> GetCurrentUserLikeIds(int currenUserId);
    void DeleteLike(UserLike like);
    void AddLike(UserLike like);
}
