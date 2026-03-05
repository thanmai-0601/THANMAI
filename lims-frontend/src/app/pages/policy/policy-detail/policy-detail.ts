import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { PolicyResponse, AddNomineeDto, UploadDocumentDto } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-policy-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StatusBadge],
  templateUrl: './policy-detail.html',
  styleUrl: './policy-detail.css'
})
export class PolicyDetail implements OnInit {
  policy: PolicyResponse | null = null;
  loading = true;
  role = '';
  policyId = 0;

  // Forms
  showNomineeForm = false;
  submittingNominee = false;

  newNominee: AddNomineeDto = { fullName: '', relationship: '', age: 25, contactNumber: '', allocationPercentage: 100 };

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private auth: AuthService,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    this.role = this.auth.getUserRole() || '';
    const id = this.route.snapshot.paramMap.get('id');
    if (id !== null) {
      this.policyId = +id;
      this.loadPolicy();
    } else {
      this.loading = false;
    }
  }

  loadPolicy(): void {
    this.loading = true;
    this.api.get<PolicyResponse>(`policy/${this.policyId}`).subscribe({
      next: (r: PolicyResponse) => {
        this.policy = r;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  get canPay(): boolean {
    if (!this.policy) return false;
    const hasNominees = this.policy.nominees && this.policy.nominees.length > 0;

    const docs = this.policy.documents || [];
    const hasAddressProof = docs.some(d => d.documentType === 'Address Proof');
    const hasIncomeProof = docs.some(d => d.documentType === 'Income Proof');
    const hasNomineeId = docs.some(d => d.documentType === 'Nominee ID Proof');

    return hasNominees && hasAddressProof && hasIncomeProof && hasNomineeId;
  }

  toggleNomineeForm(): void {
    if (!this.showNomineeForm) {
      if (this.policy?.nominees && this.policy.nominees.length > 0) {
        const existing = this.policy.nominees[0];
        this.newNominee = {
          fullName: existing.fullName,
          relationship: existing.relationship,
          age: existing.age,
          contactNumber: existing.contactNumber,
          allocationPercentage: existing.allocationPercentage
        };
      } else {
        this.newNominee = { fullName: '', relationship: '', age: 25, contactNumber: '', allocationPercentage: 100 };
      }
    }
    this.showNomineeForm = !this.showNomineeForm;
  }

  addNominee(): void {
    if (!this.newNominee.fullName || !this.newNominee.relationship || !this.newNominee.contactNumber) {
      this.toast.show('Please fill all nominee fields', 'warning');
      return;
    }

    this.submittingNominee = true;
    this.api.post(`policy/${this.policyId}/nominees`, { nominees: [this.newNominee] }).subscribe({
      next: () => {
        this.toast.show('Nominee added successfully', 'success');
        this.showNomineeForm = false;
        this.submittingNominee = false;
        this.loadPolicy();
      },
      error: () => this.submittingNominee = false
    });
  }

  // Tracking upload state per type
  uploadingTypes: { [key: string]: boolean } = {};

  onFileSelected(event: any, type: string): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.toast.show('File is too large. Max size is 5MB.', 'error');
        event.target.value = '';
        return;
      }

      const reader = new FileReader();
      reader.onload = (e: any) => {
        const dto: UploadDocumentDto = {
          documentType: type,
          fileName: file.name,
          fileBase64: e.target.result
        };
        this.performUpload(dto);
      };
      reader.readAsDataURL(file);
      // Clear input so same file can be selected again if needed
      event.target.value = '';
    }
  }

  private performUpload(dto: UploadDocumentDto): void {
    this.uploadingTypes[dto.documentType] = true;

    this.api.post(`policy/${this.policyId}/documents`, dto).subscribe({
      next: () => {
        this.toast.show(`${dto.documentType} uploaded successfully`, 'success');
        this.uploadingTypes[dto.documentType] = false;
        this.loadPolicy();
      },
      error: () => {
        this.uploadingTypes[dto.documentType] = false;
      }
    });
  }

  hasDoc(type: string): boolean {
    return this.policy?.documents?.some(d => d.documentType === type) || false;
  }

  getDocStatus(type: string): string {
    const doc = this.policy?.documents?.find(d => d.documentType === type);
    return doc ? doc.status : '';
  }
}
