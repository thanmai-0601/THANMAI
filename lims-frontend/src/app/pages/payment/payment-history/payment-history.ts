import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PaymentResponse } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge],
  templateUrl: './payment-history.html',
  styleUrl: './payment-history.css'
})
export class PaymentHistory implements OnInit {
  payments: PaymentResponse[] = [];
  loading = true;
  policyId = 0;

  constructor(private api: ApiService, private route: ActivatedRoute) { }

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
