import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { EndorsementResponse } from '../../../core/models/endorsement.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-my-endorsements',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge],
  templateUrl: './my-endorsements.html',
  styleUrl: './my-endorsements.css'
})
export class MyEndorsements implements OnInit {
  endorsements: EndorsementResponse[] = [];
  loading = true;

  constructor(private api: ApiService) { }

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
