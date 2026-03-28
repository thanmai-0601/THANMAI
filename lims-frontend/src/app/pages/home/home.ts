import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { ThemeToggle } from '../../shared/components/theme-toggle/theme-toggle';
import { AppIcon } from '../../shared/components/app-icon/app-icon';

@Component({
  selector: 'app-home',
  imports: [RouterLink, DecimalPipe, ThemeToggle, AppIcon],
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
    { icon: 'shield', title: 'Solid Security', desc: 'We use high-level encryption to keep all your personal and policy details safe and private.' },
    { icon: 'bolt', title: 'Quick Claims', desc: 'Get your claims settled quickly. Our team works fast so you get the support you need when it matters.' },
    { icon: 'handshake', title: 'Easy for Agents', desc: 'Our portal helps agents manage policies easily, with clear tracking of their earnings and commissions.' },
    { icon: 'user', title: 'Nominee Support', desc: 'We make it simple for your loved ones to file claims and get the help they need if something happens.' },
    { icon: 'money', title: 'Smart Investments', desc: 'Beyond just insurance, explore plans that help you save money and grow your wealth over time.' },
    { icon: 'dashboard', title: 'Clear Dashboards', desc: 'See everything at a glance—from policy status to payment history—on your own easy-to-use dashboard.' },
    { icon: 'bell', title: 'Stay Updated', desc: 'Always know what is happening with instant notifications for policy approvals or reassignments.' },
    { icon: 'lock', title: 'Safe Payments', desc: 'Pay your premiums securely with our trusted payment systems, keeping your financial life stress-free.' }
  ];

  processSteps = [
    { id: '01', title: 'Quick Sign-Up', desc: 'Create your account in seconds. Just provide a few basic details to get started on your protection journey.' },
    { id: '02', title: 'Pick Your Plan', desc: 'Browse through our simple insurance options. Find the one that fits your life and family needs perfectly.' },
    { id: '03', title: 'Stay Protected', desc: 'Once active, you are covered. Pay easily online and manage everything from your personal dashboard anytime.' }
  ];

  ecosystemNodes = [
    { role: 'Customer', icon: 'user', desc: 'The center of our universe. Every feature we build aims to simplify your financial journey. Manage diverse policy portfolios, add multiple secure nominees, and track every rupee of your maturity value with 100% precision through our high-performance client portal.' },
    { role: 'Agent', icon: 'handshake', desc: 'The bridge of trust between Nexalife and the world. Empowering agents with automated commission tracking, electronic document submission, and a real-time policy lifecycle dashboard that maximizes productivity and minimizes overhead cost.' },
    { role: 'Claims Officer', icon: 'staff', desc: 'The guardians of fairness. Leveraging advanced logic engines to audit documents, verify medical/financial credentials, and fast-track claim approvals to ensure that families receive their promised support within record-breaking timelines.' },
    { role: 'Admin', icon: 'settings', desc: 'The architects of stability. Overseeing global system health, configuring dynamic premium structures, managing multi-tier user permissions, and ensuring that Nexalife continues to scale with absolute reliability 24 hours a day.' }
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
