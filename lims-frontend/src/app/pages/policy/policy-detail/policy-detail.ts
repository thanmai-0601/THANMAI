import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule, Location } from '@angular/common';
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
  imports: [CommonModule, FormsModule, RouterLink, StatusBadge, LoadingSpinner, AppIcon],
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

  newNominee: AddNomineeDto = { fullName: '', relationship: '', age: null, contactNumber: '', idNumber: '', email: '' };

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private auth: AuthService,
    private toast: ToastService,
    private location: Location
  ) { }

  goBack(): void {
    this.location.back();
  }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
  }

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
          idNumber: existing.idNumber,
          email: existing.email
        };
      } else {
        this.newNominee = { fullName: '', relationship: '', age: null, contactNumber: '', idNumber: '', email: '' };
      }
    }
    this.showNomineeForm = !this.showNomineeForm;
  }

  addNominee(): void {
    if (!this.newNominee.fullName || !this.newNominee.relationship || !this.newNominee.contactNumber || !this.newNominee.idNumber || !this.newNominee.email) {
      this.toast.show('Please fill all nominee fields (Full Name, Relationship, Age, Contact, Email and ID Number)', 'warning');
      return;
    }

    if (!/^\d{10}$/.test(this.newNominee.contactNumber)) {
      this.toast.show('Contact number must be exactly 10 digits', 'warning');
      return;
    }

    if (!/^\d{12}$/.test(this.newNominee.idNumber)) {
      this.toast.show('Nominee ID must be exactly 12 numeric digits (AADHAR)', 'warning');
      return;
    }

    this.submittingNominee = true;
    this.api.post(`policy/${this.policyId}/nominees`, { nominee: this.newNominee }).subscribe({
      next: () => {
        this.toast.show('Nominee added successfully', 'success');
        this.showNomineeForm = false;
        this.submittingNominee = false;
        this.loadPolicy();
      },
      error: (err: any) => {
        this.submittingNominee = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
      }
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

      if (!file.name.toLowerCase().endsWith('.pdf')) {
        this.toast.show('Only PDF files are allowed for upload.', 'error');
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
      error: (err: any) => {
        this.uploadingTypes[dto.documentType] = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
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
