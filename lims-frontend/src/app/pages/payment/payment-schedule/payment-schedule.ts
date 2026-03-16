import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { PaymentScheduleDto, InvoiceResponse } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-payment-schedule',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './payment-schedule.html',
  styleUrl: './payment-schedule.css'
})
export class PaymentSchedule implements OnInit {
  schedule: PaymentScheduleDto | null = null;
  invoices: InvoiceResponse[] = [];
  loading = true;
  generating = false;
  policyId = 0;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private toast: ToastService,
    private location: Location
  ) { }

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('policyId');
    if (id !== null) {
      this.policyId = +id;
      this.api.get<PaymentScheduleDto>(`payment/policy/${this.policyId}?type=schedule`).subscribe({
        next: (res: PaymentScheduleDto) => {
          this.schedule = res;
          this.invoices = res.invoices || [];
          this.loading = false;
        },
        error: () => this.loading = false
      });
    } else {
      this.loading = false;
    }
  }

  loadSchedule(): void {
    this.loading = true;
    this.api.get<PaymentScheduleDto>(`payment/policy/${this.policyId}?type=schedule`).subscribe({
      next: (res: PaymentScheduleDto) => {
        this.schedule = res;
        this.invoices = res.invoices || [];
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  generateSchedule(): void {
    if (this.generating) return;
    this.generating = true;
    this.api.post(`payment/policy/${this.policyId}/generate-schedule`, {}).subscribe({
      next: () => {
        this.toast.show('Invoice schedule generated successfully!', 'success');
        this.generating = false;
        this.loadSchedule();
      },
      error: () => {
        this.toast.show('Failed to generate schedule.', 'error');
        this.generating = false;
      }
    });
  }

  get nextInvoiceId(): number | undefined {
    return this.invoices.find(i => i.status !== 'Paid')?.invoiceId;
  }

  trackByInvoiceId(index: number, invoice: InvoiceResponse): number {
    return invoice.invoiceId;
  }
}
