using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace Application.Services;

public class ClaimsOfficerAssignmentService : IClaimsOfficerAssignmentService
{
    private readonly IUserRepository _userRepo;
    private readonly IClaimRepository _claimRepo;

    public ClaimsOfficerAssignmentService(
        IUserRepository userRepo,
        IClaimRepository claimRepo)
    {
        _userRepo = userRepo;
        _claimRepo = claimRepo;
    }

    public async Task<int> AssignOfficerAsync()
    {
        var officers = await _userRepo.GetActiveClaimsOfficersAsync();

        if (!officers.Any())
            throw new InvalidOperationException(
                "No active claims officers available.");

        var workloads = new List<(int OfficerId, int ActiveCount, DateTime? LastAssigned)>();

        foreach (var officer in officers)
        {
            var activeCount =
                await _claimRepo.GetActiveCountByOfficerAsync(officer.Id);

            var lastAssigned =
                await _claimRepo.GetLastAssignmentDateAsync(officer.Id);

            workloads.Add((officer.Id, activeCount, lastAssigned));
        }

        var selected = workloads
            .OrderBy(o => o.ActiveCount)
            .ThenBy(o => o.LastAssigned ?? DateTime.MinValue)
            .First();

        return selected.OfficerId;
    }





}