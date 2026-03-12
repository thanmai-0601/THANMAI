import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PolicyResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-all-policies',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './all-policies.html',
  styleUrl: './all-policies.css'
})
export class AllPolicies implements OnInit {
  policies: PolicyResponse[] = [];
  loading = true;

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    this.api.get<PolicyResponse[]>('policy').subscribe({
      next: (r: PolicyResponse[]) => {
        this.policies = r;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
