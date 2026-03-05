import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { EndorsementResponse, EndorsementDecisionDto } from '../../../core/models/endorsement.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-endorsement-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, FormsModule],
  templateUrl: './endorsement-detail.html',
  styleUrl: './endorsement-detail.css'
})
export class EndorsementDetail implements OnInit {
  end: EndorsementResponse | null = null;
  loading = true;
  role = '';
  endorsementId = 0;

  submitLoading = false;
  remarks = '';

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private auth: AuthService,
    private toast: ToastService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.role = this.auth.getUserRole() || '';
    const id = this.route.snapshot.paramMap.get('id');
    if (id !== null) {
      this.endorsementId = +id;
      this.loadEndorsement();
    } else {
      this.loading = false;
    }
  }

  loadEndorsement(): void {
    this.api.get<EndorsementResponse>(`endorsement/${this.endorsementId}`).subscribe({
      next: (res: EndorsementResponse) => {
        this.end = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  get parsedChanges(): any {
    if (!this.end?.changeRequested) return { isJson: false, data: [] };
    try {
      const parsed = JSON.parse(this.end.changeRequested);
      return { isJson: true, data: Array.isArray(parsed) ? parsed : [parsed] };
    } catch {
      return { isJson: false, data: this.end.changeRequested };
    }
  }

  objectKeys(obj: any): string[] {
    return Object.keys(obj);
  }

  formatKey(key: string): string {
    return key.replace(/([A-Z])/g, ' $1').trim().replace(/^[a-z]/, (val) => val.toUpperCase());
  }

  submitReview(decision: string): void {
    const isApproved = decision === 'Approve';
    if (!isApproved && !this.remarks) {
      this.toast.show('Remarks are required when rejecting an endorsement.', 'warning');
      return;
    }

    this.submitLoading = true;
    const payload: EndorsementDecisionDto = {
      isApproved,
      agentRemarks: isApproved ? this.remarks : undefined,
      rejectionReason: !isApproved ? this.remarks : undefined
    };

    this.api.post(`endorsement/${this.endorsementId}/decision`, payload).subscribe({
      next: () => {
        this.toast.show(`Endorsement ${decision} successfully!`, 'success');
        this.router.navigate(['/app/endorsement/pending']);
      },
      error: () => this.submitLoading = false
    });
  }
}
