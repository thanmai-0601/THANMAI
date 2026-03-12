import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { NotificationService, NotificationDto } from '../../core/services/notification';
import { ThemeToggle } from '../../shared/components/theme-toggle/theme-toggle';
import { AppIcon } from '../../shared/components/app-icon/app-icon';

interface MenuItem { label: string; icon: string; link: string; }

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ThemeToggle, AppIcon],
  templateUrl: './layout.html',
  styleUrl: './layout.css'
})
export class Layout implements OnInit {
  isSidebarOpen = true;
  isMobile = false;

  userName = '';
  userEmail = '';
  userRole = '';
  menuItems: MenuItem[] = [];

  notifications: NotificationDto[] = [];
  unreadCount = 0;
  isNotificationMenuOpen = false;

  constructor(
    private auth: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.checkScreenSize();
    window.addEventListener('resize', () => this.checkScreenSize());
  }

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.userName = this.auth.getUserName() || '';
      this.userEmail = this.auth.getUserEmail() || '';
      this.userRole = this.auth.getUserRole() || '';
      this.setupMenu(this.userRole);
      this.loadNotifications();
    }
  }

  loadNotifications(): void {
    this.notificationService.getMyNotifications().subscribe({
      next: (data) => {
        this.notifications = data;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      },
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  toggleNotificationMenu(): void {
    this.isNotificationMenuOpen = !this.isNotificationMenuOpen;
    if (this.isNotificationMenuOpen) {
      this.isProfileMenuOpen = false;
    }
  }

  closeNotificationMenu(): void {
    this.isNotificationMenuOpen = false;
  }

  markAsRead(id: number, event: Event): void {
    event.stopPropagation();
    const notification = this.notifications.find(n => n.notificationId === id);
    if (notification && !notification.isRead) {
      this.notificationService.markAsRead(id).subscribe({
        next: () => {
          notification.isRead = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      });
    }
  }

  checkScreenSize(): void {
    this.isMobile = window.innerWidth <= 768;
    if (this.isMobile) this.isSidebarOpen = false;
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebarMobile(): void {
    if (this.isMobile) this.isSidebarOpen = false;
  }

  isProfileMenuOpen = false;

  toggleProfileMenu(): void {
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
    if (this.isProfileMenuOpen) {
      this.isNotificationMenuOpen = false;
    }
  }

  closeProfileMenu(): void {
    this.isProfileMenuOpen = false;
  }

  setupMenu(role: string): void {
    switch (role) {
      case 'Admin':
        this.menuItems = [
          { label: 'Dashboard', icon: 'dashboard', link: '/app/dashboard/admin' },
          { label: 'All Policies', icon: 'policies', link: '/app/all-policies' },
          { label: 'All Claims', icon: 'claims', link: '/app/all-claims' },
          { label: 'Endorsements', icon: 'endorsements', link: '/app/admin/endorsements' },
          { label: 'User Management', icon: 'users', link: '/app/admin/users' },
          { label: 'Plan Management', icon: 'settings', link: '/app/admin/plans' }
        ];
        break;
      case 'Agent':
        this.menuItems = [
          { label: 'Dashboard', icon: 'dashboard', link: '/app/dashboard/agent' },
          { label: 'Assigned Policies', icon: 'policies', link: '/app/agent-policies' },
          { label: 'Endorsements', icon: 'endorsements', link: '/app/endorsement/pending' }
        ];
        break;
      case 'ClaimsOfficer':
        this.menuItems = [
          { label: 'Dashboard', icon: 'dashboard', link: '/app/dashboard/claims-officer' },
          { label: 'Assigned Claims', icon: 'claims', link: '/app/officer-claims' }
        ];
        break;
      case 'Customer':
        this.menuItems = [
          { label: 'Dashboard', icon: 'dashboard', link: '/app/dashboard/customer' },
          { label: 'Browse Plans', icon: 'search', link: '/app/plans' },
          { label: 'My Policies', icon: 'policies', link: '/app/my-policies' },
          { label: 'My Payments', icon: 'payments', link: '/app/my-payments' },
          { label: 'My Claims', icon: 'claims', link: '/app/my-claims' },
          { label: 'My Endorsements', icon: 'endorsements', link: '/app/endorsement/my-endorsements' }
        ];
        break;
    }
  }

  get isNotDashboard(): boolean {
    return !this.router.url.includes('/dashboard/');
  }

  get dashboardLink(): string {
    const roleMap: any = {
      'Admin': '/app/dashboard/admin',
      'Agent': '/app/dashboard/agent',
      'ClaimsOfficer': '/app/dashboard/claims-officer',
      'Customer': '/app/dashboard/customer'
    };
    return roleMap[this.userRole] || '/app';
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
