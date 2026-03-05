import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AgentDashboardDto } from '../../../core/models/dashboard.model';
import { StatCard } from '../../../shared/components/stat-card/stat-card';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-agent-dashboard', standalone: true,
  imports: [CommonModule, RouterLink, StatCard, LoadingSpinner],
  templateUrl: './agent-dashboard.html', styleUrl: './agent-dashboard.css'
})
export class AgentDashboard implements OnInit {
  data: AgentDashboardDto | null = null; loading = true;
  constructor(private api: ApiService) { }
  ngOnInit(): void { this.api.get<AgentDashboardDto>('dashboard/summary').subscribe({ next: r => { this.data = r; this.loading = false; }, error: () => this.loading = false }); }
}
