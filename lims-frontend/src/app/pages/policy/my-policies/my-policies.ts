import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PolicyResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-my-policies',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge],
  templateUrl: './my-policies.html',
  styleUrl: './my-policies.css'
})
export class MyPolicies implements OnInit {
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
