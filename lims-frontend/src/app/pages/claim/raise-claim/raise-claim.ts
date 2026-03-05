import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { RaiseClaimDto, ClaimResponse, ClaimDocumentDto } from '../../../core/models/claim.model';
import { PolicyResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-raise-claim',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinner],
  templateUrl: './raise-claim.html',
  styleUrl: './raise-claim.css'
})
export class RaiseClaim implements OnInit {
  // All active policies
  policies: PolicyResponse[] = [];
  loadingPolicies = true;

  // The policy user selected for the claim
  selectedPolicy: PolicyResponse | null = null;

  // Track policies that already have claims
  claimedPolicyNumbers = new Set<string>();

  private getLocalYMD(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  // Min date for dateOfDeath: policy start date
  // Max date: today
  today = this.getLocalYMD(new Date());
  minDeathDate = '';
  deathCertificate: ClaimDocumentDto | null = null;

  claimForm: FormGroup;
  submitting = false;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.claimForm = this.fb.group({
      policyNumber: ['', Validators.required],
      causeOfDeath: ['', Validators.required],
      dateOfDeath: ['', Validators.required],
      nomineeName: ['', Validators.required],
      nomineeRelationship: ['', Validators.required],
      bankAccountName: ['', Validators.required],
      bankAccountNumber: ['', [Validators.required, Validators.pattern('^[0-9]{9,20}$')]],
      bankIfscCode: ['', [Validators.required, Validators.minLength(11), Validators.maxLength(11)]]
    });
  }

  ngOnInit(): void {
    let policiesLoaded = false;
    let claimsLoaded = false;

    const checkLoading = () => {
      if (policiesLoaded && claimsLoaded) {
        this.loadingPolicies = false;
      }
    };

    this.api.get<PolicyResponse[]>('policy').subscribe({
      next: (res: PolicyResponse[]) => {
        this.policies = res.filter(p => p.status === 'Active');
        policiesLoaded = true;
        checkLoading();
      },
      error: () => {
        policiesLoaded = true;
        checkLoading();
      }
    });

    this.api.get<ClaimResponse[]>('claim').subscribe({
      next: (claims: ClaimResponse[]) => {
        claims.forEach(c => {
          if (c.status !== 'Rejected') {
            this.claimedPolicyNumbers.add(c.policyNumber);
          }
        });
        claimsLoaded = true;
        checkLoading();
      },
      error: () => {
        claimsLoaded = true;
        checkLoading();
      }
    });
  }

  hasClaim(policyNumber: string): boolean {
    return this.claimedPolicyNumbers.has(policyNumber);
  }

  onPolicyChange(): void {
    const policyNumber = this.claimForm.get('policyNumber')?.value;
    this.selectedPolicy = this.policies.find(p => p.policyNumber === policyNumber) ?? null;

    if (this.selectedPolicy && this.selectedPolicy.activeFrom) {
      // Clean ISO date extraction to ensure YYYY-MM-DD format
      const dateStr = this.selectedPolicy.activeFrom.toString();
      this.minDeathDate = dateStr.includes('T') ? dateStr.split('T')[0] : dateStr;
    } else {
      this.minDeathDate = '';
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.toast.show('File is too large. Max size is 5MB.', 'error');
        event.target.value = '';
        return;
      }
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.deathCertificate = {
          documentType: 'DeathCertificate',
          fileName: file.name,
          fileBase64: e.target.result
        };
      };
      reader.readAsDataURL(file);
    }
  }

  submit(): void {
    if (this.claimForm.invalid || !this.deathCertificate) {
      const msg = !this.deathCertificate ? 'Please upload the Death Certificate.' : 'Please complete all required fields correctly.';
      this.toast.show(msg, 'warning');
      this.claimForm.markAllAsTouched();
      return;
    }

    const payload: RaiseClaimDto = {
      ...this.claimForm.value,
      deathCertificate: this.deathCertificate
    };

    if (this.minDeathDate && payload.dateOfDeath < this.minDeathDate) {
      this.toast.show('Date of death cannot be before the policy start date.', 'warning');
      return;
    }

    if (!payload.causeOfDeath.trim()) {
      this.toast.show('Please state the cause of death.', 'warning');
      return;
    }
    if (!payload.nomineeName.trim() || !payload.nomineeRelationship.trim()) {
      this.toast.show('Please provide nominee details exactly as registered.', 'warning');
      return;
    }
    if (!payload.bankAccountName.trim() || !payload.bankAccountNumber.trim() || !payload.bankIfscCode.trim()) {
      this.toast.show('Please provide complete bank account details for settlement.', 'warning');
      return;
    }

    this.submitting = true;
    this.api.post<{ claimId: number }>('claim/raise', payload).subscribe({
      next: (res) => {
        this.toast.show('Claim request submitted successfully! A claims officer has been assigned.', 'success');
        this.router.navigate(['/app/claim', res.claimId]);
      },
      error: () => this.submitting = false
    });
  }
}
