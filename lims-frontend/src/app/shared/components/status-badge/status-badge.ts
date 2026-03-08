import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  imports: [NgClass],
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.css'
})
export class StatusBadge {
  @Input() status = '';

  getDotClass(): string {
    const s = this.status.toLowerCase();
    if (['active', 'approved', 'settled', 'paid', 'verified'].includes(s))
      return 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]';
    if (['submitted', 'underreview', 'under review', 'pending', 'requested', 'grace', 'documentssubmitted', 'documents submitted'].includes(s))
      return 'bg-amber-500 shadow-[0_0_8px_rgba(245,158,11,0.5)]';
    if (['rejected', 'lapsed', 'cancelled', 'failed', 'suspended'].includes(s))
      return 'bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.5)]';
    if (['draft', 'uploaded'].includes(s))
      return 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.5)]';
    return 'bg-slate-400';
  }

  getStatusClass(): string {
    const s = this.status.toLowerCase();
    if (['active', 'approved', 'settled', 'paid', 'verified'].includes(s))
      return 'bg-emerald-50 text-emerald-700 border-emerald-100 dark:bg-emerald-900/20 dark:text-emerald-400 dark:border-emerald-800/50';
    if (['submitted', 'underreview', 'under review', 'pending', 'requested', 'grace', 'documentssubmitted', 'documents submitted'].includes(s))
      return 'bg-amber-50 text-amber-700 border-amber-100 dark:bg-amber-900/20 dark:text-amber-400 dark:border-amber-800/50';
    if (['rejected', 'lapsed', 'cancelled', 'failed', 'suspended'].includes(s))
      return 'bg-red-50 text-red-700 border-red-100 dark:bg-red-900/20 dark:text-red-400 dark:border-red-800/50';
    if (['draft', 'uploaded'].includes(s))
      return 'bg-blue-50 text-blue-700 border-blue-100 dark:bg-blue-900/20 dark:text-blue-400 dark:border-blue-800/50';
    return 'bg-slate-50 text-slate-600 border-slate-100 dark:bg-slate-800 dark:text-slate-400 dark:border-slate-700';
  }

  getDisplayName(): string {
    const s = this.status.toLowerCase();
    if (s === 'grace') return 'Pending';
    return this.status;
  }
}
