import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ClaimsOfficerDashboardDto } from '../../../core/models/dashboard.model';
import { StatCard } from '../../../shared/components/stat-card/stat-card';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-claims-officer-dashboard', standalone: true,
  imports: [CommonModule, RouterLink, StatCard, LoadingSpinner, AppIcon],
  templateUrl: './claims-officer-dashboard.html', styleUrl: './claims-officer-dashboard.css'
})
export class ClaimsOfficerDashboard implements OnInit {
  data: ClaimsOfficerDashboardDto | null = null; loading = true;
  constructor(private api: ApiService) { }
  ngOnInit(): void { this.api.get<ClaimsOfficerDashboardDto>('dashboard/summary').subscribe({ next: r => { this.data = r; this.loading = false; }, error: () => this.loading = false }); }
}
