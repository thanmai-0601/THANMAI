import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AdminDashboardDto } from '../../../core/models/dashboard.model';
import { StatCard } from '../../../shared/components/stat-card/stat-card';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatCard, LoadingSpinner],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  data: AdminDashboardDto | null = null;
  loading = true;

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    this.api.get<AdminDashboardDto>('dashboard/summary').subscribe({
      next: (res) => {
        this.data = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
