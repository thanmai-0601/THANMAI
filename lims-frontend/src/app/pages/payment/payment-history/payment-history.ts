import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PaymentResponse } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CommonModule, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './payment-history.html',
  styleUrl: './payment-history.css'
})
export class PaymentHistory implements OnInit {
  payments: PaymentResponse[] = [];
  loading = true;
  policyId = 0;

  constructor(private api: ApiService, private route: ActivatedRoute, private location: Location) { }

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('policyId');
    if (id !== null) {
      this.policyId = +id;
      this.api.get<PaymentResponse[]>(`payment/policy/${this.policyId}?type=history`).subscribe({
        next: (res: PaymentResponse[]) => {
          this.payments = res;
          this.loading = false;
        },
        error: () => this.loading = false
      });
    } else {
      this.loading = false;
    }
  }

  trackByPaymentId(index: number, payment: PaymentResponse): number {
    return payment.paymentId;
  }
}
