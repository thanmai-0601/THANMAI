import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, AuthService } from '../../../core/services';
import { ClaimResponse } from '../../../core/models/claim.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-officer-claims',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './officer-claims.html',
  styleUrl: './officer-claims.css'
})
export class OfficerClaims implements OnInit {
  claims: ClaimResponse[] = [];
  loading = true;

  constructor(private api: ApiService, private auth: AuthService) { }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
  }

  ngOnInit(): void {
    this.api.get<ClaimResponse[]>('claim').subscribe({
      next: (res: ClaimResponse[]) => {
        this.claims = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
