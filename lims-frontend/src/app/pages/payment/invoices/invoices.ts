import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { InvoiceResponse } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge],
  templateUrl: './invoices.html',
  styleUrl: './invoices.css'
})
export class Invoices implements OnInit {
  invoices: InvoiceResponse[] = [];
  loading = true;
  policyId = 0;

  constructor(private api: ApiService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('policyId');
    if (id !== null) {
      this.policyId = +id;
      this.api.get<InvoiceResponse[]>(`payment/policy/${this.policyId}?type=invoices`).subscribe({
        next: (res: InvoiceResponse[]) => {
          this.invoices = res;
          this.loading = false;
        },
        error: () => this.loading = false
      });
    } else {
      this.loading = false;
    }
  }

  trackByInvoiceId(index: number, invoice: InvoiceResponse): number {
    return invoice.invoiceId;
  }
}
