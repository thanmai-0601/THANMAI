import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { PolicyResponse } from '../../../core/models/policy.model';

@Component({
  selector: 'app-request-endorsement',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppIcon],
  templateUrl: './request-endorsement.html',
  styleUrl: './request-endorsement.css'
})
export class RequestEndorsement implements OnInit {
  policies: PolicyResponse[] = [];
  fetchingPolicies = true;
  loading = false;

  step = 1;

  endorsementForm: FormGroup;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder,
    private route: ActivatedRoute
  ) {
    this.endorsementForm = this.fb.group({
      policyId: ['', Validators.required],
      endorsementType: ['', Validators.required],
      newAddress: [''],
      nomineeName: ['', Validators.required],
      nomineeRelationship: ['', Validators.required],
      nomineeAge: [null, Validators.required],
      nomineeContact: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const preselectedId = this.route.snapshot.queryParamMap.get('policyId');
    if (preselectedId) {
      this.endorsementForm.patchValue({ policyId: preselectedId });
    }

    this.api.get<PolicyResponse[]>('policy').subscribe({
      next: (res: PolicyResponse[]) => {
        this.policies = res.filter(p => p.status !== 'Rejected' && p.status !== 'Cancelled' && p.status !== 'Settled' && !p.hasSettledClaim && !p.hasGlobalSettledDeathClaim);
        this.fetchingPolicies = false;
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
      if (!formValue.nomineeName || !formValue.nomineeRelationship || !formValue.nomineeContact) {
        this.toast.show('Please complete all nominee fields.', 'warning');
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
        age: formValue.nomineeAge,
        contactNumber: formValue.nomineeContact
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

    this.api.post(endpoint, payload).subscribe({
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
