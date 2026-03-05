import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { ThemeToggle } from '../../shared/components/theme-toggle/theme-toggle';

@Component({
  selector: 'app-home',
  imports: [RouterLink, DecimalPipe, ThemeToggle],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {
  stats = [
    { value: 0, target: 50000, label: 'Policies Issued', suffix: '+' },
    { value: 0, target: 98, label: 'Claims Settled', suffix: '%' },
    { value: 0, target: 25000, label: 'Happy Customers', suffix: '+' },
    { value: 0, target: 15, label: 'Years of Trust', suffix: '+' }
  ];

  features = [
    { icon: '🛡️', title: 'Trusted Security', desc: 'Your details are safe with high-level security.' },
    { icon: '⚡', title: 'Fast Claims', desc: 'Claims handled within 48 hours with clear status updates.' },
    { icon: '📱', title: 'Digital First', desc: 'Handle your plans, payments, and claims all from one app.' },
    { icon: '🤝', title: 'Expert Help', desc: 'Helpful experts to guide you through every step of your plan.' },
    { icon: '💰', title: 'Best Prices', desc: 'Great prices with custom costs based on your details.' },
    { icon: '📊', title: 'Full Clearness', desc: 'Live screens with full control over all your plans.' }
  ];

  ngOnInit(): void {
    this.animateCounters();
  }

  animateCounters(): void {
    this.stats.forEach((stat, i) => {
      const duration = 2000;
      const steps = 60;
      const increment = stat.target / steps;
      let current = 0;
      const timer = setInterval(() => {
        current += increment;
        if (current >= stat.target) {
          current = stat.target;
          clearInterval(timer);
        }
        this.stats[i] = { ...stat, value: Math.round(current) };
      }, duration / steps);
    });
  }
}
