import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ClaimResponse } from '../../../core/models/claim.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-my-claims',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './my-claims.html',
  styleUrl: './my-claims.css'
})
export class MyClaims implements OnInit {
  claims: ClaimResponse[] = [];
  loading = true;

  constructor(private api: ApiService) { }

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
