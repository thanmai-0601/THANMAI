import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, AuthService } from '../../../core/services';
import { EndorsementResponse } from '../../../core/models/endorsement.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';

@Component({
  selector: 'app-all-endorsements',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './all-endorsements.html',
  styleUrl: './all-endorsements.css'
})
export class AllEndorsements implements OnInit {
  endorsements: EndorsementResponse[] = [];
  loading = true;

  constructor(private api: ApiService, private auth: AuthService) { }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
  }

  ngOnInit(): void {
    this.api.get<EndorsementResponse[]>('endorsement').subscribe({
      next: (res: EndorsementResponse[]) => {
        this.endorsements = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
