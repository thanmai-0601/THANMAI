import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { ClaimResponse, ClaimDocumentDto, ClaimDecisionDto } from '../../../core/models/claim.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-claim-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinner, StatusBadge],
  templateUrl: './claim-detail.html',
  styleUrl: './claim-detail.css'
})
export class ClaimDetail implements OnInit {
  claim: ClaimResponse | null = null;
  loading = true;
  role = '';
  claimId = 0;
  fileBaseUrl = environment.apiUrl.replace('/api', '');

  showDocForm = false;
  submittingDoc = false;
  newDocument: ClaimDocumentDto = { documentType: 'DeathCertificate', fileName: '', fileBase64: '' };

  decisionForm: any = { settledAmount: null, remarks: '', rejectionReason: '' };
  submittingDecision = false;
  startingReview = false;

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
      this.claimId = +id;
      this.loadClaim();
    } else {
      this.loading = false;
    }
  }

  loadClaim(): void {
    // Officers use the claimsofficer endpoint; customers/admin use claim endpoint
    const endpoint = this.role === 'ClaimsOfficer'
      ? `claim/${this.claimId}`
      : `claim/${this.claimId}`;

    this.api.get<ClaimResponse>(endpoint).subscribe({
      next: (res: ClaimResponse) => {
        this.claim = res;
        // Pre-fill officer settled amount with full claim amount
        if (this.role === 'ClaimsOfficer' && res.claimAmount) {
          this.decisionForm.settledAmount = res.claimAmount;
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  get paymentCompliance(): number {
    if (!this.claim || this.claim.totalInvoices === 0) return 0;
    return Math.round((this.claim.paidInvoices / this.claim.totalInvoices) * 100);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.toast.show('File is too large. Max size is 5MB.', 'error');
        event.target.value = '';
        return;
      }
      this.newDocument.fileName = file.name;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.newDocument.fileBase64 = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  uploadDoc(): void {
    if (!this.newDocument.fileName) {
      this.toast.show('Please select a file first.', 'warning');
      return;
    }
    if (!this.newDocument.fileBase64) {
      this.toast.show('File is still loading, please wait a moment and try again.', 'warning');
      return;
    }

    this.submittingDoc = true;
    this.api.post(`claim/${this.claimId}/documents`, this.newDocument).subscribe({
      next: () => {
        this.toast.show('Document uploaded successfully!', 'success');
        this.showDocForm = false;
        this.submittingDoc = false;
        this.newDocument = { documentType: 'DeathCertificate', fileName: '', fileBase64: '' };
        this.loadClaim();
      },
      error: (err: any) => {
        this.submittingDoc = false;
        const msg = err?.error?.message || err?.error || 'Upload failed. Please try again.';
        this.toast.show(typeof msg === 'string' ? msg : 'Upload failed. Please try again.', 'error');
      }
    });
  }

  startReview(): void {
    this.startingReview = true;
    this.api.patch(`claim/${this.claimId}/start-review`, {}).subscribe({
      next: () => {
        this.toast.show('Claim is now under review.', 'success');
        this.startingReview = false;
        this.loadClaim();
      },
      error: () => this.startingReview = false
    });
  }

  submitDecision(status: string): void {
    const isApproved = status === 'Approve';

    if (isApproved) {
      if (!this.decisionForm.settledAmount || this.decisionForm.settledAmount <= 0) {
        this.toast.show('Please enter a valid settlement amount.', 'warning');
        return;
      }
      if (!this.decisionForm.remarks?.trim()) {
        this.toast.show('Officer remarks are required when approving.', 'warning');
        return;
      }
    } else {
      if (!this.decisionForm.rejectionReason?.trim()) {
        this.toast.show('Rejection reason is mandatory.', 'warning');
        return;
      }
    }

    this.submittingDecision = true;

    const payload: ClaimDecisionDto = {
      isApproved,
      settledAmount: isApproved ? this.decisionForm.settledAmount : null,
      officerRemarks: isApproved ? this.decisionForm.remarks : this.decisionForm.remarks || undefined,
      rejectionReason: !isApproved ? this.decisionForm.rejectionReason : undefined
    };

    this.api.post(`claim/${this.claimId}/decision`, payload).subscribe({
      next: () => {
        const msg = isApproved
          ? `Claim approved & settled! Transfer reference will be generated.`
          : `Claim rejected.`;
        this.toast.show(msg, isApproved ? 'success' : 'error');
        this.router.navigate(['/app/officer-claims']);
      },
      error: () => this.submittingDecision = false
    });
  }
}
