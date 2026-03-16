import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { RaiseClaimDto, ClaimResponse, ClaimDocumentDto } from '../../../core/models/claim.model';
import { PolicyResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-raise-claim',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinner, AppIcon],
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
  nomineeIdProof: ClaimDocumentDto | null = null;

  claimForm: FormGroup;
  submitting = false;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder,
    private auth: AuthService
  ) {
    this.claimForm = this.fb.group({
      policyNumber: ['', Validators.required],
      causeOfDeath: ['', Validators.required],
      dateOfDeath: ['', Validators.required],
      nomineeName: ['', Validators.required],
      nomineeRelationship: ['', Validators.required],
      nomineeIdNumber: ['', [Validators.required, Validators.pattern('^[0-9]{12}$')]],
      bankAccountName: ['', Validators.required],
      bankAccountNumber: ['', [Validators.required, Validators.pattern('^[0-9]{9,20}$')]],
      bankIfscCode: ['', [Validators.required, Validators.pattern('^[A-Z]{4}0[A-Z0-9]{6}$')]]
    });
  }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
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
        // Policies must be Active AND not have a settled claim already.
        this.policies = res.filter(p => p.status === 'Active' && !p.hasSettledClaim);
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
          // A policy is considered to have a current claim if it's not Rejected AND not already Settled.
          if (c.status !== 'Rejected' && c.status !== 'Settled') {
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

    // Reset validations on policy change
    this.claimForm.get('nomineeIdNumber')?.updateValueAndValidity();
  }

  get nomineeAadharMatchError(): string | null {
    if (!this.selectedPolicy || !this.claimForm.get('nomineeIdNumber')?.value) return null;
    if (this.claimForm.get('nomineeIdNumber')?.value.length < 12) return null;
    
    const registeredNominee = this.selectedPolicy.nominees?.[0];
    if (registeredNominee && this.claimForm.get('nomineeIdNumber')?.value !== registeredNominee.idNumber) {
      return 'AADHAR number does not match our policy records.';
    }
    return null;
  }

  get nomineeFileMatchError(): string | null {
    if (!this.selectedPolicy || !this.nomineeIdProof) return null;
    
    const registeredIdProof = this.selectedPolicy.documents?.find(d => d.documentType === 'Nominee ID Proof');
    if (registeredIdProof && this.nomineeIdProof.fileName !== registeredIdProof.fileName) {
      return 'This file name does not match the Nominee ID Proof registered in your policy.';
    }
    return null;
  }

  onFileSelected(event: any, type: string): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.toast.show('File is too large. Max size is 5MB.', 'error');
        event.target.value = '';
        return;
      }

      if (!file.name.toLowerCase().endsWith('.pdf')) {
        this.toast.show('Only PDF files are allowed for submission.', 'error');
        event.target.value = '';
        return;
      }

      const reader = new FileReader();
      reader.onload = (e: any) => {
        const doc: ClaimDocumentDto = {
          documentType: type === 'death' ? 'DeathCertificate' : 'Nominee ID Proof',
          fileName: file.name,
          fileBase64: e.target.result
        };
        if (type === 'death') this.deathCertificate = doc;
        else this.nomineeIdProof = doc;
      };
      reader.readAsDataURL(file);
    }
  }

  submit(): void {
    if (this.claimForm.invalid || !this.deathCertificate || !this.nomineeIdProof) {
      const msg = !this.deathCertificate ? 'Please upload the Death Certificate.' : 
                  !this.nomineeIdProof ? 'Please upload the Nominee ID Proof.' :
                  'Please complete all required fields correctly.';
      this.toast.show(msg, 'warning');
      this.claimForm.markAllAsTouched();
      return;
    }

    const payload: RaiseClaimDto = {
      ...this.claimForm.value,
      deathCertificate: this.deathCertificate,
      nomineeIdProof: this.nomineeIdProof
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
      this.toast.show('Please provide nominee details.', 'warning');
      return;
    }

    // New logic: Only aadhar number and ID proof file must match the record
    if (this.selectedPolicy) {
      // 1. Check Aadhar Number
      if (this.selectedPolicy.nominees && this.selectedPolicy.nominees.length > 0) {
        const registeredNominee = this.selectedPolicy.nominees[0];
        if (payload.nomineeIdNumber !== registeredNominee.idNumber) {
          this.toast.show('Nominee Aadhar number does not match our records.', 'error');
          return;
        }
      }

      // 2. Check ID Proof file names (as a proxy for the same document)
      const registeredIdProof = this.selectedPolicy.documents?.find(d => d.documentType === 'Nominee ID Proof');
      if (registeredIdProof && payload.nomineeIdProof.fileName !== registeredIdProof.fileName) {
        this.toast.show('The uploaded Nominee ID Proof must be the same file as registered in your policy.', 'error');
        return;
      }
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
      error: (err: any) => {
        this.submitting = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
      }
    });
  }
}
