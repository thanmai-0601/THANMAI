import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { PolicyResponse } from '../../../core/models/policy.model';

@Component({
  selector: 'app-request-endorsement',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppIcon, RouterLink],
  templateUrl: './request-endorsement.html',
  styleUrl: './request-endorsement.css'
})
export class RequestEndorsement implements OnInit {
  policies: PolicyResponse[] = [];
  fetchingPolicies = true;
  loading = false;
  hasSettledClaimGlobal = false;

  step = 1;

  endorsementForm: FormGroup;

  constructor(
    private api: ApiService,
    private auth: AuthService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder,
    private route: ActivatedRoute
  ) {
    this.endorsementForm = this.fb.group({
      policyId: ['', Validators.required],
      endorsementType: ['', Validators.required],
      newAddress: ['', [Validators.minLength(10)]],
      nomineeName: ['', [Validators.required]],
      nomineeRelationship: ['', [Validators.required]],
      nomineeAge: [null, [Validators.required, Validators.min(1), Validators.max(120)]],
      nomineeContact: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      nomineeIdNumber: ['', [Validators.required, Validators.pattern('^[0-9]{12}$')]],
      nomineeEmail: ['', [Validators.required, Validators.email]]
    });
  }

  getControlError(controlName: string): string | null {
    const control = this.endorsementForm.get(controlName);
    if (!control || !control.errors || !control.touched) return null;

    if (control.errors['required']) return `${this.getLabel(controlName)} is required`;
    if (control.errors['pattern']) {
      if (controlName === 'nomineeContact') return 'Contact must be 10 digits';
      if (controlName === 'nomineeIdNumber') return 'Aadhar must be 12 digits';
      return 'Invalid format';
    }
    if (control.errors['email']) return 'Invalid email address';
    if (control.errors['minlength']) return 'Minimum length not met';
    if (control.errors['min'] || control.errors['max']) return 'Value out of range';
    
    return 'Invalid field';
  }

  private getLabel(name: string): string {
    const labels: any = {
      policyId: 'Policy',
      endorsementType: 'Type',
      newAddress: 'Address',
      nomineeName: 'Nominee Name',
      nomineeRelationship: 'Relationship',
      nomineeAge: 'Age',
      nomineeContact: 'Contact',
      nomineeIdNumber: 'Aadhar',
      nomineeEmail: 'Email'
    };
    return labels[name] || name;
  }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
  }

  ngOnInit(): void {
    const preselectedId = this.route.snapshot.queryParamMap.get('policyId');
    if (preselectedId) {
      this.endorsementForm.patchValue({ policyId: +preselectedId });
    }

    this.api.get<PolicyResponse[]>('policy').subscribe({
      next: (res: PolicyResponse[]) => {
        this.hasSettledClaimGlobal = res.some(p => p.hasGlobalSettledDeathClaim || p.status === 'Settled' || p.hasSettledClaim);
        this.policies = res.filter(p => p.status !== 'Rejected' && p.status !== 'Cancelled' && p.status !== 'Settled' && !p.hasSettledClaim && !p.hasGlobalSettledDeathClaim);
        this.fetchingPolicies = false;

        if (this.hasSettledClaimGlobal) {
          this.endorsementForm.disable();
        }
      },
      error: () => this.fetchingPolicies = false
    });
  }

  nextStep(): void {
    const policyId = this.endorsementForm.get('policyId')?.value;
    const type = this.endorsementForm.get('endorsementType')?.value;

    if (!policyId || !type) {
      this.toast.show('Please select a policy and the type of change.', 'warning');
      this.endorsementForm.get('policyId')?.markAsTouched();
      this.endorsementForm.get('endorsementType')?.markAsTouched();
      return;
    }

    const isValidPolicy = this.policies.some(p => p.policyId === +policyId);
    if (!isValidPolicy) {
      this.toast.show('Selected policy is not eligible for endorsements (it may be settled or inactive).', 'error');
      return;
    }

    this.step = 2;
  }

  previousStep(): void {
    this.step = 1;
  }

  submit(): void {
    const formValue = this.endorsementForm.value;
    let endpoint = '';
    let payload: any = { policyId: +formValue.policyId };

    if (formValue.endorsementType === 'NomineeUpdate') {
      if (!formValue.nomineeName || !formValue.nomineeRelationship || !formValue.nomineeAge || !formValue.nomineeContact || !formValue.nomineeIdNumber || !formValue.nomineeEmail) {
        this.toast.show('Please complete all nominee fields, including Aadhar and Email.', 'warning');
        return;
      }
      if (!/^\d{10}$/.test(formValue.nomineeContact)) {
        this.toast.show('Nominee contact number must be exactly 10 digits.', 'warning');
        return;
      }
      endpoint = 'endorsement/request/nominee';
      payload.newNominee = {
        fullName: formValue.nomineeName,
        relationship: formValue.nomineeRelationship,
        age: +formValue.nomineeAge,
        contactNumber: formValue.nomineeContact,
        idNumber: formValue.nomineeIdNumber,
        email: formValue.nomineeEmail
      };
    } else if (formValue.endorsementType === 'AddressChange') {
      if (!formValue.newAddress || formValue.newAddress.length < 10) {
        this.toast.show('Please provide a complete new address (min 10 characters).', 'warning');
        return;
      }
      endpoint = 'endorsement/request/address';
      payload.newAddress = formValue.newAddress;
    }

    this.loading = true;

    this.api.post<any>(endpoint, payload).subscribe({
      next: () => {
        this.toast.show('Endorsement requested successfully!', 'success');
        this.router.navigate(['/app/endorsement/my-endorsements']);
      },
      error: (err: any) => {
        this.loading = false;
        this.toast.show(err.error?.message || 'Endorsement request failed', 'error');
      }
    });
  }
}
