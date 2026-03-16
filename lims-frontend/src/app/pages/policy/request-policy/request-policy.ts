import { Component, OnInit, OnDestroy } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { RequestPolicyDto, PlanResponse } from '../../../core/models/policy.model';

@Component({
  selector: 'app-request-policy',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AppIcon],
  templateUrl: './request-policy.html',
  styleUrl: './request-policy.css'
})
export class RequestPolicy implements OnInit, OnDestroy {
  planId: number = 0;
  plan: PlanResponse | null = null;
  loading = true;
  submitting = false;

  form: RequestPolicyDto = {
    insurancePlanId: 0,
    sumAssured: 500000,
    tenureYears: 10,
    annualIncome: 500000,
    occupation: '',
    address: '',
    nominee: { fullName: '', relationship: '', age: null, contactNumber: '', idNumber: '', email: '' },
    documents: []
  };

  docTypes = ['Address Proof', 'Income Proof', 'Nominee ID Proof'];
  selectedDocs: { [key: string]: File | null } = {
    'Address Proof': null,
    'Income Proof': null,
    'Nominee ID Proof': null
  };
  private objectUrls: string[] = [];

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('planId');
    if (id) {
      this.planId = +id;
      this.form.insurancePlanId = this.planId;

      this.api.get<PlanResponse>(`policy/plans/${this.planId}`).subscribe({
        next: (p: PlanResponse) => {
          if (!p.isActive) {
            this.toast.show('This insurance plan is currently inactive and not accepting new policy applications.', 'error');
            this.router.navigate(['/app/plans']);
            return;
          }
          this.plan = p;
          // Set defaults based on plan constraints
          this.form.sumAssured = p.minSumAssured;
          this.form.tenureYears = p.tenureOptions.length > 0 ? p.tenureOptions[0] : 10;
          this.form.annualIncome = Math.max(500000, p.minAnnualIncome);

          this.loading = false;
        },
        error: () => this.loading = false
      });
    } else {
      this.loading = false;
    }
  }

  submit(reqForm?: any): void {
    if (reqForm && reqForm.invalid) {
      this.toast.show('Please properly fill all required details and ensure they meet the rules.', 'warning');
      return;
    }

    if (!this.form.occupation || this.form.occupation.trim() === '') {
      this.toast.show('Please enter your occupation.', 'warning');
      return;
    }

    if (this.plan) {
      if (this.form.sumAssured === null || this.form.sumAssured < this.plan.minSumAssured || this.form.sumAssured > this.plan.maxSumAssured) {
        this.toast.show(`Sum assured must be between ₹${this.plan.minSumAssured} and ₹${this.plan.maxSumAssured}.`, 'error');
        return;
      }
      if (this.form.tenureYears === null || !this.plan.tenureOptions.includes(this.form.tenureYears)) {
        this.toast.show(`Tenure must be one of: ${this.plan.tenureOptions.join(', ')} years.`, 'error');
        return;
      }
    }

    // Validate Nominee
    if (!this.form.nominee?.fullName || !this.form.nominee?.relationship || !this.form.nominee?.contactNumber || !this.form.nominee?.idNumber || !this.form.nominee?.email) {
      this.toast.show('Please fill all nominee details, including email and the 12-digit Aadhar number.', 'warning');
      return;
    }

    if (!/^\d{10}$/.test(this.form.nominee.contactNumber)) {
      this.toast.show('Nominee contact must be exactly 10 digits.', 'warning');
      return;
    }

    if (!/^\d{12}$/.test(this.form.nominee.idNumber)) {
      this.toast.show('Nominee Aadhar number must be exactly 12 numeric digits.', 'warning');
      return;
    }

    // Validate Documents
    if (!this.selectedDocs['Address Proof'] || !this.selectedDocs['Income Proof'] || !this.selectedDocs['Nominee ID Proof']) {
      this.toast.show('Please select all required documents (Address, Income, and Nominee ID Proof).', 'warning');
      return;
    }

    this.submitting = true;
    this.prepareFilesAndSubmit();
  }



  onFileSelected(event: any, type: string) {
    const file = event.target.files[0];
    if (file) {
      if (!file.name.toLowerCase().endsWith('.pdf')) {
        this.toast.show('Only PDF files are allowed for submission.', 'error');
        event.target.value = '';
        return;
      }
      this.selectedDocs[type] = file;
    }
  }

  viewDocument(type: string): void {
    const file = this.selectedDocs[type];
    if (file) {
      const url = URL.createObjectURL(file);
      this.objectUrls.push(url);
      window.open(url, '_blank');
    }
  }

  ngOnDestroy(): void {
    this.objectUrls.forEach(url => URL.revokeObjectURL(url));
  }

  private async prepareFilesAndSubmit() {
    try {
      this.form.documents = [];
      for (const type of this.docTypes) {
        const file = this.selectedDocs[type];
        if (file) {
          const base64 = await this.toBase64(file);
          this.form.documents.push({
            documentType: type,
            fileName: file.name,
            fileBase64: base64
          });
        }
      }

      this.api.post<{ message: string; policyId: number }>('policy/request', this.form).subscribe({
        next: (res: { message: string; policyId: number }) => {
          this.toast.show('Policy requested successfully!', 'success');
          this.router.navigate(['/app/policy', res.policyId]);
        },
        error: (err: any) => {
          this.submitting = false;
          this.toast.show(ApiService.getErrorMessage(err), 'error');
        }
      });
    } catch (err) {
      this.submitting = false;
      this.toast.show('Document processing failed.', 'error');
    }
  }

  private toBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = error => reject(error);
    });
  }
}
